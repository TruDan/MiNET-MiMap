function BiomeMap() {

    var $this = this;

    this.map = null;

    this._config = {};

    this.layerControl = null;
    this.coordsControl = null;
    this.levelMeta = {};
    this.layers = {};
    this.socket = new BiomeMapWebSocket();

    this.init = function (config) {
        // init levels
        $this._config = config;

        $this.initMap(config);

        $.each(config.levels,
            function (key, value) {
                $this.initLevel(value);
            });

        $this.layerControl.init();
        $this.coordsControl.init();
        $this.socket.init($this.map);
    };

    this.getTileUrlFunc = function (levelId, layerId) {
        return function (coord, zoom) {
            var bounds = $this.levelMeta[levelId].bounds;
            var scale = 1 << zoom;

            var minTileX = Math.floor((bounds.min.x << zoom) >> 9);
            var minTileZ = Math.floor((bounds.min.z << zoom) >> 9);
            var maxTileX = Math.floor((bounds.max.x << zoom) >> 9);
            var maxTileZ = Math.floor((bounds.max.z << zoom) >> 9);

            //console.log(coord, zoom, bounds.min, bounds.max, minTileX, minTileZ, maxTileX, maxTileZ);

            if (coord.x < minTileX ||
                coord.y < minTileZ ||
                coord.x > maxTileX ||
                coord.y > maxTileZ) {
                return null;
            }


            return window.location.pathname + "tiles/" + levelId + "/" + layerId + "/" + zoom + "/" + coord.x + "_" + coord.y + ".png";
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
        $this.layers[levelConfig.levelId] = layer;

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
                $this.layers[levelConfig.levelId + "_" + value.layerId] = overlayLayer;

                overlayLayer.baseMapTypes = [levelConfig.levelId];

                var item = levelLayerCtrl.addLayer($this.layerId + "_" + value.layerId, overlayLayer);

                if (value.default === true)
                    item.default = true;
            });
    };

    this.updateLevelMeta = function (levelId, meta) {
        $this.levelMeta[levelId] = meta;
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

                backgroundColor: '#212121',
                noClear: false
            });

        this.layerControl = new LayerControl($this.map,
            {
                position: google.maps.ControlPosition.TOP_LEFT
            });

        this.coordsControl = new CoordsControl($this.map);
    };

    this.refreshTile = function (layerId, tileX, tileY, tileZoom) {
        if (layerId in $this.layers) {
            var layer = $this.layers[layerId];
            layer.refreshTile(tileX, tileY, tileZoom);
        }
    };

    this.socket.open();
}

function BiomeMapLayer(options) {
    //google.maps.MapType.call(this);
    this.name = options.name;
    this.getTileUrl = options.getTileUrl;
    this.blendMode = options.blendMode;
    this.tileSize = options.tileSize;
    this.minZoom = options.minZoom;
    this.maxZoom = options.maxZoom;
    this.loadedTiles = {};

    //this.projection = google.maps.MapCanvasProjection;

}

//BiomeMapLayer.prototype = Object.create(google.maps.MapType.prototype);
//BiomeMapLayer.prototype.constructor = BiomeMapLayer;

BiomeMapLayer.prototype.getTile = function (coord, zoom, ownerDocument) {
    var tileId = 'x_' + coord.x + '_y_' + coord.y + '_zoom_' + zoom;
    //var div = google.maps.ImageMapType.prototype.getTile.call(this, coord, zoom, ownerDocument);


    var tile = ownerDocument.createElement('div');
    tile.style.backgroundPosition = 'center center';
    tile.style.backgroundRepeat = 'no-repeat';
    tile.style.textAlign = 'center';
    tile.style.display = 'flex';
    tile.style.flexDirection = 'column';
    tile.style.justifyContent = 'center';
    tile.style.borderWidth = '1px';
    tile.style.borderColor = '#000000';
    tile.style.borderStyle = 'solid';
    tile.style.color = '#f1f1f1';
    tile.style.opacity = '0.25';
    tile.style.textShadow = '0 0 3px #000000';
    tile.innerHTML = "<span>" + tileId + "</span>";

    tile.style.width = this.tileSize.width + 'px';
    tile.style.height = this.tileSize.height + 'px';

    if (this.blendMode !== undefined)
        tile.style.mixBlendMode = this.blendMode;


    var tileUrl = this.getTileUrl(coord, zoom);

    tile.tileId = tileId; //	do not use 'id' as new custom property as it's a native property of all HTML elements
    tile.tileUrl = tileUrl;

    this.loadedTiles[tileId] = tile;

    if (tileUrl !== null) {
        tileUrl += '?timestamp=' + new Date().getTime();

        var img = new Image();
        img.onload = function () {
            tile.style.backgroundImage = 'url(' + tileUrl + ')';
            tile.style.opacity = '1';
            img.onload = null;
            img = null;
        }
        img.src = tileUrl;
    }

    return tile;
};

BiomeMapLayer.prototype.refreshTiles = function () {
    for (var tileId in this.loadedTiles) {
        this.refreshTileById(tileId);
    }
};

BiomeMapLayer.prototype.refreshTile = function (x, y, zoom) {
    var tileId = 'x_' + x + '_y_' + y + '_zoom_' + zoom;
    this.refreshTileById(tileId);
}

BiomeMapLayer.prototype.refreshTileById = function (tileId) {
    function onloadCallback(tile2, tileUrl2) {
        return function () {
            tile2.style.backgroundImage = 'url(' + tileUrl2 + ')';
        };
    }

    var tile = this.loadedTiles[tileId];
    if (tile !== undefined && tile !== null) {
        var tileUrl = tile.tileUrl + '?timestamp=' + new Date().getTime();
        var img = new Image();
        img.onload = onloadCallback(tile, tileUrl);
        img.src = tileUrl;
    }
};

BiomeMapLayer.prototype.releaseTile = function (tile) {
    delete this.loadedTiles[tile.tileId];
    tile = null;
};

function initMap() {
    window.biomeMap = new BiomeMap();
    //window.biomeMap.map.backgroundColor = "#212121";
};
/*
$(document).ready(function () {
    initMap();
});*/