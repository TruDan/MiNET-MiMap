
function MiMapWebSocket() {
    var $this = this;

    this._map = map;

    var _defaultOptions = {

    };


    this.init = function (map, options) {
        this._map = map;
        this.options = $.extend({}, _defaultOptions, options);
        //this._map.controls[this.options.position].push(this.getControl());
    }

    var socket = new ReconnectingWebSocket("ws://" + window.location.hostname + ":" + window.location.port,
        null,
        {
            automaticOpen: false
        });

    socket.onopen = function () {
        $this.onopen();
        console.log("Opened");
    }
    socket.onclose = function () {
        $this.onclose();
        console.log("Closed");
    }

    socket.onmessage = function (msg) {

        var packet = JSON.parse(msg.data);

        switch (packet.id) {
            case 1: // MapConfigPacket
                //window.MiMap.init(packet.config);
                break;

            case 2: // TileUpdatePacket
                window.MiMap.refreshTile(packet.layerId, packet.tile.x, packet.tile.y, packet.tile.zoom);
                break;

            case 3: // LevelMetaPacket
                window.MiMap.updateLevelMeta(packet.levelId, packet.meta);
                break;

            case 4: // ListPlayersPacket

                break;


            case 0:
            default:
                console.log("Unknown Packet.", msg);
                return;
        }
    }

    this.subscribeTiles = function (enabled, zoomLevel) {
        $this.send({
            id: 5,
            subscribe: enabled,
            currentZoomLevel: zoomLevel
        });
    }

    this.send = function (packet) {
        socket.send(JSON.stringify(packet));
    }

    this.open = function () {
        socket.open();
    }


    this.$ctrl = $('<div class="gControl socketStatusControl"></div>');
    this.$html_connected = $("<div class=\"gControlItem\"><i class=\"material-icons md-18\">cloud</i> <span>Connected</span></div>");
    this.$html_disconnected = $("<div class=\"gControlItem\"><i class=\"material-icons md-18\">cloud_off</i> <span>Connection Error</span></div>");

    this.$ctrl.append(this.$html_disconnected);

    this.onopen = function () {
        this.$ctrl.html(this.$html_connected);
    }

    this.onclose = function () {
        this.$ctrl.html(this.$html_disconnected);
    }

    this.getControl = function () {
        return $('<div class="gControlWrapper" />').append(this.$ctrl)[0];
    }
}