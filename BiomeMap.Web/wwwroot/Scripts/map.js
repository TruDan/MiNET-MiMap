function BiomeMap() {
    var $this = this;

    this.map = null;
    this._config = {};

    this.minimap = null;
    this.chunkinfo = null;
    this.hash = null;

    this.socket = new BiomeMapWebSocket();

    this.levelMeta = {};
    this.layers = {};

    this.activeLayer = null;

    this.init = function (config) {
        // init levels
        $this._config = config;

        $this.initMap(config);

        var firstLevelId = null;
        $.each(config.levels,
            function (key, value) {
                if (firstLevelId === null) firstLevelId = value.levelId;
                $this.initLevel(value);
            });

        $this.socket.init($this.map);

        $this.setLevel(firstLevelId);
    };

    this.__getCrs = function () {
        return L.extend({},
            L.CRS.Simple,
            {
                //infinite: true,
                projection: {
                    project: function (latlng) {
                        // Direct translation of lat -> x, lng -> y.
                        return new L.Point(latlng.lat, latlng.lng);
                    },
                    unproject: function (point) {
                        // Direct translation of x -> lat, y -> lng.
                        return new L.LatLng(point.x, point.y);
                    }
                },
                // a = 1; b = 2; c = 1; d = 0
                // x = a * x + b; y = c * y + d
                // End result is 1:1 values during transformation.
                transformation: new L.Transformation(1, 0, 1, 0),
                scale: function (zoom) {
                    // Equivalent to 2 raised to the power of zoom, but faster.
                    return (1 << zoom);
                }
            });
    }

    this.initMap = function (config) {
        $this.map = L.map('map',
            {
                crs: $this.__getCrs(),
                attributionControl: false
            });

        $this.map.setView([0, 0], 0);
        $this.map.on('zoomend', this.onMapZoomEnd.bind(this));

        $this.chunkinfo = L.control.chunkInfo({ position: 'topright' });
        $this.chunkinfo.addTo($this.map);

        $this.hash = new L.Hash($this.map);
    };

    this.onMapZoomEnd = function (e) {
        $this.socket.subscribeTiles(true, $this.map.getZoom());
    };

    this.createLayerForLevel = function (levelId, minZoom, maxZoom) {
        return L.tileLayer(window.location.pathname + "tiles/{levelid}/{layerid}/{z}/{x}_{y}.png",
            {
                id: levelId,
                levelid: levelId,
                layerid: 'base',
                minZoom: 0,
                maxZoom: 18,
                minNativeZoom: minZoom,
                maxNativeZoom: maxZoom,
                noWrap: true
            });
    }

    this.initLevel = function (levelConfig) {
        if (levelConfig.enabled !== true) return;

        var layer = this.createLayerForLevel(levelConfig.levelId, levelConfig.minZoom, levelConfig.maxZoom);

        $this.layers[levelConfig.levelId] = layer;

    }

    this.updateLevelMeta = function (levelId, meta) {
        $this.levelMeta[levelId] = meta;

        $this.applyLevelMeta(levelId);
    }

    this.applyLevelMeta = function (levelId) {
        if (!(levelId in $this.levelMeta)) return;

        var meta = $this.levelMeta[levelId];

        var layer = $this.layers[levelId];
        console.log(levelId,
            [
                meta.bounds.min.x, meta.bounds.min.z,
                meta.bounds.max.x, meta.bounds.max.z
            ]);

        var b = [
            meta.bounds.min.x * meta.tileSize.width, meta.bounds.min.z * meta.tileSize.height,
            meta.bounds.max.x * meta.tileSize.width, meta.bounds.max.z * meta.tileSize.height
        ];

        //layer.getSource().setExtent(b);
        //layer.setExtent(b);
        //layer.getSource().getProjection().setExtent(b);
    }

    this.setLevel = function (levelId) {
        if (!(levelId in $this.layers)) return;
        var layer = $this.layers[levelId];

        var miniMapLayer = L.tileLayer(window.location.pathname + "tiles/{levelid}/{layerid}/{z}/{x}_{y}.png",
            {
                id: levelId,
                levelid: levelId,
                layerid: 'base',
                minZoom: -5,
                maxZoom: 5,
                minNativeZoom: 0,
                maxNativeZoom: 0,
                noWrap: true
            });

        miniMapLayer.opacity = 0.75;

        if (this.activeLayer !== null) {
            this.activeLayer.remove();
        }

        this.activeLayer = layer;

        if (this.minimap === null) {
            this.minimap = new L.Control.MiniMap(miniMapLayer,
                {
                    aimingRectOptions: {
                        color: '#4DD0E1',
                        weight: 1,
                        opacity: 1,
                        dashArray: '5, 5',
                        lineCap: 'square',
                        lineJoin: 'square',

                        fillColor: '#4DD0E1',
                        fillOpacity: 0.1
                    },
                    mapOptions: {
                        crs: $this.__getCrs(),
                        attributionControl: false
                    }
                });
            this.minimap.addTo($this.map);
        } else {
            this.minimap.changeLayer(miniMapLayer);
        }

        layer.addTo($this.map);

        console.log("Setting Level to ", levelId, layer, miniMapLayer);
    }

    this.refreshTile = function (layerId, tileX, tileY, tileZoom) {
        //console.log(layerId, this.activeLayer.options.id, this.activeLayer);
        if (layerId !== this.activeLayer.options.id) return;

        if (tileZoom !== $this.map.getZoom()) return;

        var url = (window.location.pathname + "tiles/{levelid}/{layerid}/{z}/{x}_{y}.png");
        url = url.replace('{levelid}', layerId);
        url = url.replace('{layerid}', 'base');
        url = url.replace('{z}', tileZoom);
        url = url.replace('{x}', tileX);
        url = url.replace('{y}', tileY);

        $('img[src^="' + url + '"]').attr('src', url + "?ts=" + (new Date().getTime()));
        //console.debug("Refreshed ", layerId, tileX, tileY, tileZoom);
    }

    this.socket.open();
}

function initMap() {
    window.biomeMap = new BiomeMap();
};