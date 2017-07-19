function BiomeMap() {

    var $this = this;

    this.map = null;

    this._config = {};

    this.layerControl = null;
    this.coordsControl = null;
    this.levelMeta = {};

    this.loadConfig = function () {
        $.getJSON("/config.json",
            function (config) {
                // init levels
                $this._config = config;

                $this.initMap(config);

                $.each(config.levels,
                    function (key, value) {
                        $this.initLevel(value);
                    });

                $this.layerControl.init();
                $this.coordsControl.init();
            });
    };

    this.getTileUrlFunc = function (levelId, layerId) {
        return function (coord, zoom) {
            var bounds = $this.levelMeta[levelId].bounds;
            var scale = 1 << zoom;

            var minTileX = Math.floor((bounds.min.x >> 9) * scale);
            var minTileZ = Math.floor((bounds.min.z >> 9) * scale);
            var maxTileX = Math.floor((bounds.max.x >> 9) * scale);
            var maxTileZ = Math.floor((bounds.max.z >> 9) * scale);

            //console.log(coord, zoom, bounds.min, bounds.max, minTileX, minTileZ, maxTileX, maxTileZ);

            if (coord.x < minTileX ||
                coord.y < minTileZ ||
                coord.x > maxTileX ||
                coord.y > maxTileZ) {
                return null;
            }


            return "/tiles/" + levelId + "/" + layerId + "/" + zoom + "/" + coord.x + "_" + coord.y + ".png";
        };
    }

    this.initLevel = function (levelConfig) {
        if (levelConfig.enabled !== true) return;

        this.updateLevelMeta(levelConfig.levelId);

        /*var layer = new google.maps.ImageMapType({
            getTileUrl: $this.getTileUrlFunc(levelConfig.levelId, "base"),
            tileSize: new google.maps.Size(levelConfig.tileSize, levelConfig.tileSize),
            maxZoom: levelConfig.maxZoom,
            minZoom: levelConfig.minZoom,
            name: levelConfig.label || levelConfig.levelId
        });*/
        var layer = new BiomeMapLayer({
            getTileUrl: $this.getTileUrlFunc(levelConfig.levelId, "base"),
            tileSize: new google.maps.Size(levelConfig.tileSize, levelConfig.tileSize),
            name: levelConfig.label || levelConfig.levelId,
            maxZoom: levelConfig.maxZoom,
            minZoom: levelConfig.minZoom
        });

        var levelLayerCtrl = this.layerControl.addLayer(levelConfig.levelId, layer);

        $.each(levelConfig.layers,
            function (key, value) {
                if (value.enabled !== true) return;

                var overlayLayer = new BiomeMapLayer({
                    getTileUrl: $this.getTileUrlFunc(levelConfig.levelId, value.layerId),
                    tileSize: new google.maps.Size(levelConfig.tileSize, levelConfig.tileSize),
                    name: value.label || value.levelId,
                    blendMode: value.blendMode
                });

                overlayLayer.baseMapTypes = [levelConfig.levelId];

                var item = levelLayerCtrl.addLayer($this.layerId + "_" + value.layerId, overlayLayer);

                if (value.default === true)
                    item.default = true;
            });
    };

    this.updateLevelMeta = function (levelId) {
        $.getJSON("/meta/" + levelId + ".json",
            function (meta) {
                $this.levelMeta[levelId] = meta;
            });
    }

    this.initMap = function (config) {

        var mapTypeIds = [];

        var maxZoom = Number.MIN_VALUE, minZoom = Number.MAX_VALUE, zoom = 0;
        //console.log(config);
        $.each(config.levels,
            function (key, value) {
                console.log(value);

                if (value.enabled !== true) return;
                mapTypeIds.push(value.levelId);

                minZoom = Math.min(value.minZoom, minZoom);
                maxZoom = Math.max(value.maxZoom, maxZoom);
                zoom = value.defaultZoom;

            });

        minZoom = Math.min(maxZoom, Math.min(minZoom, zoom));
        maxZoom = Math.max(maxZoom, Math.max(minZoom, zoom));
        zoom = Math.min(maxZoom, Math.max(minZoom, zoom));


        $this.map = new google.maps.Map(document.getElementById('map'),
            {
                center: { lat: 0, lng: 0 },
                zoomControl: true,
                zoomControlOptions: { position: google.maps.ControlPosition.RIGHT_BOTTOM },

                minZoom: minZoom,
                maxZoom: maxZoom,
                zoom: zoom,

                streetViewControl: false,

                mapTypeControl: false,

                mapTypeId: mapTypeIds[0],

                //backgroundColor: '#212121',
                noClear: false
            });

        this.layerControl = new LayerControl($this.map,
            {
                position: google.maps.ControlPosition.TOP_LEFT
            });

        this.coordsControl = new CoordsControl($this.map);
    };

    this.loadConfig();
}

function BiomeMapLayer(options) {
    //google.maps.MapType.call(this);
    this.name = options.name;
    this.getTileUrl = options.getTileUrl;
    this.blendMode = options.blendMode;
    this.tileSize = options.tileSize;
    this.minZoom = options.minZoom;
    this.maxZoom = options.maxZoom;

    //this.projection = google.maps.MapCanvasProjection;

}

//BiomeMapLayer.prototype = Object.create(google.maps.MapType.prototype);
//BiomeMapLayer.prototype.constructor = BiomeMapLayer;

BiomeMapLayer.prototype.getTile = function (coord, zoom, ownerDocument) {
    //var div = google.maps.ImageMapType.prototype.getTile.call(this, coord, zoom, ownerDocument);


    var div = ownerDocument.createElement('div');

    div.style.width = this.tileSize.width + 'px';
    div.style.height = this.tileSize.height + 'px';

    var src = this.getTileUrl(coord, zoom);
    if (src !== null) {
        var img = new Image();
        img.style.display = 'none';
        div.appendChild(img);
        img.onload = function () {
            img.style.display = 'inline';
        }
        img.src = src;

    }

    if (this.blendMode !== undefined)
        div.style.mixBlendMode = this.blendMode;
    return div;
};

function initMap() {
    window.biomeMap = new BiomeMap();
};
/*
$(document).ready(function () {
    initMap();
});*/