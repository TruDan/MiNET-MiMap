L.Control.ChunkInfo = L.Control.extend({
    _hoverOverlay: null,

    _e: {},
    _element: null,
    _map: null,

    _selectedChunk: null,
    _selectedOverlay: null,

    onAdd: function (map) {
        this._map = map;

        this._element = L.DomUtil.create('div', 'leaflet-control-chunkinfo');
        this._element.style.position = 'relative';


        this._e.hover = L.DomUtil.create('div', 'leaflet-control-chunkinfo-hover');
        this._element.appendChild(this._e.hover);

        this._e.hover_block = L.DomUtil.create('dd');
        var hover_block_dl = L.DomUtil.create('dl', 'leaflet-control-chunkinfo-hover-block leaflet-control-chunkinfo-block');
        var hover_block_dt = L.DomUtil.create('dt');
        hover_block_dt.innerHTML = "Block";

        hover_block_dl.appendChild(hover_block_dt);
        hover_block_dl.appendChild(this._e.hover_block);
        this._e.hover.appendChild(hover_block_dl);

        this._e.hover_chunk = L.DomUtil.create('dd');
        var hover_chunk_dl = L.DomUtil.create('dl', 'leaflet-control-chunkinfo-hover-chunk leaflet-control-chunkinfo-chunk');
        var hover_chunk_dt = L.DomUtil.create('dt');
        hover_chunk_dt.innerHTML = "Chunk";

        hover_chunk_dl.appendChild(hover_chunk_dt);
        hover_chunk_dl.appendChild(this._e.hover_chunk);
        this._e.hover.appendChild(hover_chunk_dl);

        this._e.hover_region = L.DomUtil.create('dd');
        var hover_region_dl = L.DomUtil.create('dl', 'leaflet-control-chunkinfo-hover-region leaflet-control-chunkinfo-region');
        var hover_region_dt = L.DomUtil.create('dt');
        hover_region_dt.innerHTML = "Region";

        hover_region_dl.appendChild(hover_region_dt);
        hover_region_dl.appendChild(this._e.hover_region);
        this._e.hover.appendChild(hover_region_dl);

        this._e.selected = L.DomUtil.create('div', 'leaflet-control-chunkinfo-selected');
        this._element.appendChild(this._e.selected);

        var selected_chunk_dl = L.DomUtil.create('dl', 'leaflet-control-chunkinfo-selected-chunk leaflet-control-chunkinfo-chunk');
        this._e.selected_chunk = L.DomUtil.create('dd');
        var selected_chunk_dt = L.DomUtil.create('dt');
        selected_chunk_dt.innerHTML = "Chunk";

        selected_chunk_dl.appendChild(selected_chunk_dt);
        selected_chunk_dl.appendChild(this._e.selected_chunk);
        this._e.selected.appendChild(selected_chunk_dl);

        this._bindEvents(map);

        return this._element;
    },

    onRemove: function (map) {
        this._unbindEvents(map);
    },

    _bindEvents: function (map) {
        map.on("click", this.onMapMouseClick.bind(this));
        map.on("mousemove", this.onMapMouseMove.bind(this));
    },

    _unbindEvents: function (map) {
        map.off("click", this.onMapMouseClick.bind(this));
        map.off("mousemove", this.onMapMouseMove.bind(this));
    },

    __latlngToBlock: function (latlng) {
        var zoom = this._map.getZoom();
        var s = (256 / 512) * (1 << zoom); // Size of chunk at zoom 0

        var p = this._map.project(latlng, zoom);

        var x = Math.floor(p.x / s);
        var y = Math.floor(p.y / s);

        return L.point(x, -y);
    },

    __blockToLatlng: function (block) {
        var zoom = this._map.getZoom();
        var s = (256 / 512) * (1 << zoom); // Size of chunk at zoom 0

        return this._map.unproject(L.point(block.x * s, -block.y * s), zoom);
    },

    onMapMouseClick: function (e) {
        var p = this.__latlngToBlock(e.latlng);
        var chunkX = p.x >> 4;
        var chunkZ = p.y >> 4;

        this._e.selected_chunk.innerHTML = chunkX + ", " + chunkZ;

        var bounds = [
            this.__blockToLatlng(L.point(chunkX << 4, chunkZ << 4)),
            this.__blockToLatlng(L.point((chunkX + 1) << 4, (chunkZ + 1) << 4))
        ];

        if (this._selectedOverlay === null) {
            this._selectedOverlay = L.rectangle(bounds,
                {
                    color: '#EC407A',
                    weight: 2,
                    dashArray: '5, 5',
                    lineCap: 'square',
                    lineJoin: 'square',
                    fillOpacity: 0.1,
                    pane: 'overlayPane'
                });
            this._selectedOverlay.addTo(this._map);
        }

        this._selectedOverlay.setBounds(bounds);
    },

    onMapMouseMove: function (e) {
        // Show chunk boundary on hover if zoom is close enough
        var p = this.__latlngToBlock(e.latlng);
        var chunkX = p.x >> 4;
        var chunkZ = p.y >> 4;

        var regionX = chunkX >> 5;
        var regionZ = chunkZ >> 5;

        this._e.hover_block.innerHTML = p.x + ", " + p.y;
        this._e.hover_chunk.innerHTML = chunkX + ", " + chunkZ;
        this._e.hover_region.innerHTML = regionX + ", " + regionZ;

        var bounds = [
            this.__blockToLatlng(L.point(chunkX << 4, chunkZ << 4)),
            this.__blockToLatlng(L.point((chunkX + 1) << 4, (chunkZ + 1) << 4))
        ];

        if (this._hoverOverlay === null) {
            this._hoverOverlay = L.rectangle(bounds,
                {
                    color: '#f1f1f1',
                    weight: 2,
                    fillOpacity: 0.2,
                    pane: 'overlayPane'
                });
            this._hoverOverlay.addTo(this._map);
        }

        this._hoverOverlay.setBounds(bounds);

    }
});

L.control.chunkInfo = function (opts) {
    return new L.Control.ChunkInfo(opts);
}