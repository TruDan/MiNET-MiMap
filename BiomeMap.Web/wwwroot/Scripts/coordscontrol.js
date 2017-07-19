function CoordsControl(map, options) {
    var $this = this;

    this._map = map;
    this.$ctrl = $('<div class="gControl coordsControl"></div>');
    this.$coords = $('<div class="gControlItem"></div>');

    this.$ctrl.append(this.$coords);

    var _defaultOptions = {
        position: google.maps.ControlPosition.TOP_CENTER
    };

    this.options = $.extend({}, _defaultOptions, options);

    this.init = function () {
        this._map.controls[this.options.position].push(this.getControl());
        this._map.addListener('mousemove', this.onMouseMove.bind(this));
    }

    this.project = function (latLng) {
        var siny = Math.sin(latLng.lat() * Math.PI / 180);

        // Truncating to 0.9999 effectively limits latitude to 89.189. This is
        // about a third of a tile past the edge of the world tile.
        siny = Math.min(Math.max(siny, -0.9999), 0.9999);
        var size = this.getTileSize();

        return new google.maps.Point(
            size.width * (0.5 + latLng.lng() / 360),
            size.height * (0.5 - Math.log((1 + siny) / (1 - siny)) / (4 * Math.PI)));
    };

    this.getTileSize = function () {
        return $this._map.mapTypes[$this._map.getMapTypeId()].tileSize;
    }

    this.onMouseMove = function (event) {
        var scale = 1 << $this._map.getZoom();
        var size = this.getTileSize();

        var worldCoordinate = this.project(event.latLng);

        var pixelCoordinate = new google.maps.Point(
            Math.floor(worldCoordinate.x * scale),
            Math.floor(worldCoordinate.y * scale));

        var tileCoordinate = new google.maps.Point(
            Math.floor(worldCoordinate.x * scale / this.getTileSize().width),
            Math.floor(worldCoordinate.y * scale / this.getTileSize().height));

        $this.$coords.html("X=" + pixelCoordinate.x + ", Z=" + pixelCoordinate.y + "<br />" + tileCoordinate.x + ", " + tileCoordinate.y);
    }

    this.getControl = function () {
        return $('<div class="gControlWrapper" />').append(this.$ctrl)[0];
    }
}