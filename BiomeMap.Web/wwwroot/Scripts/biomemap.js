function BiomeMap() {

    var $this = this;

    this.map = null;

    this._config = {};

    this.layerControl = null;
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
            });
    };

    this.getTileUrlFunc = function (levelId, layerId) {
        return function (coord, zoom) {
            var bounds = $this.levelMeta[levelId].bounds;

            var minTileX = Math.floor(bounds.min.x >> (9 - zoom));
            var minTileZ = Math.floor(bounds.min.z >> (9 - zoom));
            var maxTileX = Math.floor(bounds.max.x >> (9 - zoom));
            var maxTileZ = Math.floor(bounds.max.z >> (9 - zoom));

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

        var layer = new google.maps.ImageMapType({
            getTileUrl: $this.getTileUrlFunc(levelConfig.levelId, "base"),
            tileSize: new google.maps.Size(levelConfig.tileSize, levelConfig.tileSize),
            maxZoom: levelConfig.maxZoom,
            minZoom: levelConfig.minZoom,
            name: levelConfig.label || levelConfig.levelId
        });

        var levelLayerCtrl = this.layerControl.addLayer(levelConfig.levelId, layer);

        $.each(levelConfig.layers,
            function (key, value) {
                if (value.enabled !== true) return;

                var overlayLayer = new BiomeMapOverlayLayer({
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
        //console.log(config);
        $.each(config.levels,
            function (key, value) {
                if (value.enabled !== true) return;
                mapTypeIds.push(value.levelId);
            });

        $this.map = new google.maps.Map(document.getElementById('map'),
            {
                center: new google.maps.LatLng(0, 0),
                zoomControl: true,
                zoomControlOptions: { position: google.maps.ControlPosition.LEFT_TOP },

                minZoom: 0,
                maxZoom: 9,
                zoom: 2,

                streetViewControl: false,

                mapTypeControl: true,
                mapTypeControlOptions: {
                    mapTypeIds: mapTypeIds,
                    position: google.maps.ControlPosition.TOP_RIGHT
                },

                mapTypeId: mapTypeIds[0],

                backgroundColor: '#212121'
            });

        this.layerControl = new LayerControl($this.map,
            {
                position: google.maps.ControlPosition.LEFT_TOP
            });
    };

    this.loadConfig();
}

function BiomeMapOverlayLayer(options) {
    google.maps.ImageMapType.call(this, options);
    this.blendMode = options.blendMode;
}

BiomeMapOverlayLayer.prototype = Object.create(google.maps.ImageMapType.prototype);
BiomeMapOverlayLayer.prototype.constructor = BiomeMapOverlayLayer;

BiomeMapOverlayLayer.prototype.getTile = function (coord, zoom, ownerDocument) {
    var div = google.maps.ImageMapType.prototype.getTile.call(this, coord, zoom, ownerDocument);

    if (this.blendMode !== undefined)
        div.style.mixBlendMode = this.blendMode;
    return div;
};

function initMap() {
    window.biomeMap = new BiomeMap();

};

$(document).ready(function () {
    initMap();
});