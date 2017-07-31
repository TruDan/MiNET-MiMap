L.Control.ChunkInfo = L.Control.extend({
    _hoverOverlay: null,
    _hoverBlockOverlay: null,

    _e: {},
    _sectionGroups: {},
    _sections: {},
    _element: null,
    _map: null,

    _selectedBlockPos: null,
    _selectedOverlay: null,
    _selectedBlockOverlay: null,

    _regionInfo: {},

    onAdd: function (map) {
        this._map = map;

        this._element = L.DomUtil.create('div', 'leaflet-control-chunkinfo leaflet-control-group');
        this._element.style.position = 'relative';

        this._addSectionGroup("hover", "Highlighted Block");
        this._addSection("hover", "region_pos", "Region");
        this._addSection("hover", "chunk_pos", "Chunk");
        this._addSection("hover", "block_pos", "Block");

        this._addSectionGroup("selected", "Selected Block");
        this._addSection("selected", "region_pos", "Region");
        this._addSection("selected", "chunk_pos", "Chunk");
        this._addSection("selected", "block_pos", "Block");

        this._addSection("selected", "block_id", "Block ID");
        this._addSection("selected", "block_biome", "Biome");
        this._addSection("selected", "block_light", "Light Level");

        this._bindEvents(map);

        return this._element;
    },

    onRemove: function (map) {
        this._unbindEvents(map);
    },

    _addSectionGroup: function (groupId, groupLabel) {
        var dom = L.DomUtil.create('table', 'leaflet-control leaflet-control-chunkinfo-group');
        this._sectionGroups[groupId] = dom;
        this._element.appendChild(dom);

        var label = L.DomUtil.create('caption', 'leaflet-control-chunkinfo-group-label');
        label.innerHTML = groupLabel;
        dom.appendChild(label);
    },

    _addSection: function (groupId, sectionId, sectionLabel) {
        if (!(groupId in this._sectionGroups)) {
            return;
        }

        var groupDom = this._sectionGroups[groupId];
        var dom = L.DomUtil.create('tr');

        if (!(groupId in this._sections)) {
            this._sections[groupId] = {};
        }
        groupDom.appendChild(dom);

        var labelDom = L.DomUtil.create('th');
        dom.appendChild(labelDom);

        labelDom.innerHTML = sectionLabel;

        var dataDom = L.DomUtil.create('td');
        dom.appendChild(dataDom);

        this._sections[groupId][sectionId] = [groupDom, labelDom, dataDom];
    },

    _updateSection: function (groupId, sectionId, newData) {
        var section = this._sections[groupId][sectionId];

        section[2].innerHTML = newData;
    },

    _bindEvents: function (map) {
        map.on("click", this.onMapMouseClick.bind(this));
        map.on("mousemove", this.onMapMouseMove.bind(this));
        map.on("mouseout", this.onMapMouseOut.bind(this));
    },

    _unbindEvents: function (map) {
        map.off("click", this.onMapMouseClick.bind(this));
        map.off("mousemove", this.onMapMouseMove.bind(this));
        map.off("mouseout", this.onMapMouseOut.bind(this));
    },

    __latlngToBlock: function (latlng) {
        var zoom = this._map.getZoom();
        var s = (256 / 512) * (1 << zoom); // Size of chunk at zoom 0

        var p = this._map.project(latlng, zoom);

        var x = Math.floor(p.x / s);
        var y = Math.floor(-p.y / s);

        return L.point(x, y);
    },

    __blockToLatlng: function (block) {
        var zoom = this._map.getZoom();
        var s = (256 / 512) * (1 << zoom); // Size of chunk at zoom 0

        return this._map.unproject(L.point(block.x * s, -(block.y * s)), zoom);
    },

    getRegionInfo: function (regionX, regionZ, callback) {
        var key = regionX + "_" + regionZ;
        var $this = this;

        if (!(key in this._regionInfo)) {
            $.getJSON(window.location.pathname + "tiles/Overworld/base/.regions/" + key + ".json",
                function (data) {
                    console.log("GetRegion", regionX, regionZ, data);
                    $this._regionInfo[key] = data;
                    if (typeof callback === 'function') {
                        callback(data);
                    }
                });
            return null;
        }

        var data = this._regionInfo[key];

        if (typeof callback === 'function') {
            callback(data);
        }

        return data;
    },

    onMapMouseClick: function (e) {
        var p = this.__latlngToBlock(e.latlng);
        //p.y = -p.y;

        var chunkX = p.x >> 4;
        var chunkZ = p.y >> 4;

        var selected = this._selectedBlockPos;
        if (selected !== null && selected.x === p.x && selected.y === p.y) {
            if (this._selectedOverlay !== null) {
                this._selectedOverlay.remove();
                this._selectedOverlay = null;
            }

            if (this._selectedBlockOverlay !== null) {
                this._selectedBlockOverlay.remove();
                this._selectedBlockOverlay = null;
            }

            this._updateSection("selected", "region_pos", "");
            this._updateSection("selected", "chunk_pos", "");
            this._updateSection("selected", "block_pos", "");
            return;
        }
        this._selectedBlockPos = p;

        this._updateSection("selected", "region_pos", "r." + (chunkX >> 5) + "." + (chunkZ >> 5) + ".mca");
        this._updateSection("selected", "chunk_pos", chunkX + ", " + chunkZ);
        this._updateSection("selected", "block_pos", p.x + ", " + p.y);

        var $this = this;
        this.getRegionInfo(
            p.x >> 9,
            p.y >> 9,
            function (regionInfo) {

                var blockInfo = regionInfo[p.x][p.y];
                console.log(p, blockInfo);

                if (blockInfo === null || blockInfo === undefined) {
                    $this._updateSection("selected", "block_id", "");
                    $this._updateSection("selected", "block_biome", "");
                    $this._updateSection("selected", "block_light", "");
                    return;
                }

                $this._updateSection("selected", "block_id", blockInfo.blockId);
                $this._updateSection("selected", "block_biome", blockInfo.biomeId);
                $this._updateSection("selected", "block_light", blockInfo.lightLevel);
            });

        var chunkBounds = [
            this.__blockToLatlng(L.point(chunkX << 4, chunkZ << 4)),
            this.__blockToLatlng(L.point((chunkX + 1) << 4, (chunkZ + 1) << 4))
        ];

        if (this._selectedOverlay === null) {
            this._selectedOverlay = L.rectangle(chunkBounds,
                {
                    color: '#00838F',
                    weight: 2,
                    opacity: 0.2,
                    fillOpacity: 0.1,
                    pane: 'overlayPane'
                });
            this._selectedOverlay.addTo(this._map);
        }

        this._selectedOverlay.setBounds(chunkBounds);

        var bounds = [
            this.__blockToLatlng(L.point(p.x, p.y)),
            this.__blockToLatlng(L.point(p.x + 1, p.y + 1))
        ];

        if (this._selectedBlockOverlay === null) {
            this._selectedBlockOverlay = L.rectangle(bounds,
                {
                    color: '#00BCD4',
                    weight: 2,
                    opacity: 0.2,
                    fillOpacity: 0.1,
                    pane: 'overlayPane'
                });
            this._selectedBlockOverlay.addTo(this._map);
        }

        this._selectedBlockOverlay.setBounds(bounds);
    },

    onMapMouseOut: function (e) {
        if (this._hoverOverlay !== null) {
            this._hoverOverlay.remove();
            this._hoverOverlay = null;
        }

        if (this._hoverBlockOverlay !== null) {
            this._hoverBlockOverlay.remove();
            this._hoverBlockOverlay = null;
        }
        this._updateSection("hover", "region_pos", "");
        this._updateSection("hover", "chunk_pos", "");
        this._updateSection("hover", "block_pos", "");
    },

    onMapMouseMove: function (e) {
        // Show chunk boundary on hover if zoom is close enough
        var p = this.__latlngToBlock(e.latlng);
        //p.y = -p.y;

        var chunkX = p.x >> 4;
        var chunkZ = p.y >> 4;

        this._updateSection("hover", "region_pos", "r." + (chunkX >> 5) + "." + (chunkZ >> 5) + ".mca");
        this._updateSection("hover", "chunk_pos", chunkX + ", " + chunkZ);
        this._updateSection("hover", "block_pos", p.x + ", " + p.y);

        var chunkBounds = [
            this.__blockToLatlng(L.point(chunkX << 4, chunkZ << 4)),
            this.__blockToLatlng(L.point((chunkX + 1) << 4, (chunkZ + 1) << 4))
        ];

        if (this._hoverOverlay === null) {
            this._hoverOverlay = L.rectangle(chunkBounds,
                {
                    color: '#f1f1f1',
                    weight: 2,
                    opacity: 0.4,
                    fillOpacity: 0.2,
                    pane: 'overlayPane'
                });
            this._hoverOverlay.addTo(this._map);
        }

        this._hoverOverlay.setBounds(chunkBounds);

        var bounds = [
            this.__blockToLatlng(L.point(p.x, p.y)),
            this.__blockToLatlng(L.point(p.x + 1, p.y + 1))
        ];

        if (this._hoverBlockOverlay === null) {
            this._hoverBlockOverlay = L.rectangle(bounds,
                {
                    color: '#E91E63',
                    weight: 2,
                    opacity: 0.4,
                    fillOpacity: 0.2,
                    pane: 'overlayPane'
                });
            this._hoverBlockOverlay.addTo(this._map);
        }

        this._hoverBlockOverlay.setBounds(bounds);

    }
});

L.control.chunkInfo = function (opts) {
    return new L.Control.ChunkInfo(opts);
};