function LayerControl(map, options) {
    var $this = this;

    this._map = map;
    this.$ctrl = $('<div class="gControl layersControl"></div>');

    var _defaultOptions = {
        position: google.maps.ControlPosition.TOP_RIGHT
    };

    this.layers = [];

    this.options = $.extend({}, _defaultOptions, options);


    this.addLayer = function (layerId, label) {
        var itemGroup = new LayerControlItemGroup($this._map, layerId, label);
        $this.layers.push(itemGroup);
        $this.$ctrl.append(itemGroup.$ctrl);
        return itemGroup;
    };

    this.init = function () {
        this._map.controls[this.options.position].push(this.getControl());

        $.each($this.layers,
            function (key, value) {
                value.init();
            });
    };

    this.getControl = function () {
        return $('<div class="gControlWrapper" />').append(this.$ctrl)[0];
    }

}

function LayerControlItemGroup(map, layerId, layer) {
    var $this = this;

    this._map = map;
    this.layerId = layerId;
    this.layer = layer;

    this.$ctrl = $('<div class="gControlItemGroup layersControlItemGroup"></div>');
    this.$headerCtrl = $('<div class="gControlItem layersControlItemGroupHeader">' + layer.name + '</div>');
    this.$listCtrl = $('<div class="gControlItemList layersControlItemList"></div>');

    this.layers = [];

    this.addLayer = function (overlayLayerId, overlayLayer) {
        var item = new LayerControlItem($this._map, overlayLayerId, overlayLayer);
        $this.layers.push(item);
        $this.$listCtrl.append(item.$ctrl);
        return item;
    };
    this.show = function (visible) {
        visible = typeof visible !== 'undefined' ? visible : $this._isVisible;
        if ($this.$ctrl.is(':visible') && $this._isValidMapType()) {
            $this.$ctrl.show();
        } else {
            $this.$ctrl.hide();
        }

        $this._isVisible = visible;
    };

    this._isValidMapType = function () {
        return $.inArray($this._map.getMapTypeId(), ($this.layer && $this.layer.baseMapTypes) || []) > -1;
    };

    this._activateOverlay = function () {
        var i, om = $this._map.overlayMapTypes;

        $this.show();

        if ($this._isActive && $this._isValidMapType()) {
            if (!$this._isOnMap) {
                om.insertAt(0, $this.layer);
                $this._isOnMap = true;
            }
        } else {
            for (i = 0; i < om.getLength(); i++) {
                console.log(om);
                if (om.getAt(i).name === 'Relief') {
                    om.removeAt(i);
                    i--;
                }
            }
            $this._isOnMap = false;
        }
    };

    this.toggle = function () {
        $this._isActive = !$this._isActive;

        if ($this._isActive) {
            $this.$ctrl.addClass('active');
        } else {
            $this.$ctrl.removeClass('active');
        }

        $this._activateOverlay();
    };

    this.init = function () {
        $.each($this.layers,
            function (key, value) {
                value.init();
            });
    };

    this.$ctrl.append(this.$headerCtrl);
    this.$ctrl.append(this.$listCtrl);
    this._map.mapTypes.set(this.layerId, this.layer);
}

function LayerControlItem(map, layerId, layer) {
    var $this = this;

    this._map = map;
    this.layerId = layerId;
    this.layer = layer;
    this.default = false;

    this._isActive = false;
    this._isVisible = true;
    this._isOnMap = false;

    this.$ctrl = $('<div class="gControlItem layersControlItem"><div class="checkbox" role="checkbox"><div></div></div><label>' + $this.layer.name + '</label></div>');;

    this.show = function (visible) {
        visible = typeof visible !== 'undefined' ? visible : $this._isVisible;
        if (visible && $this._isValidMapType()) {
            $this.$ctrl.show();
        } else {
            $this.$ctrl.hide();
        }

        $this._isVisible = visible;
    };

    this._isValidMapType = function () {
        return $.inArray($this._map.getMapTypeId(), ($this.layer && $this.layer.baseMapTypes) || []) > -1;
    };

    this._activateOverlay = function () {
        var i, om = $this._map.overlayMapTypes;

        $this.show();

        if ($this._isActive && $this._isValidMapType()) {
            if (!$this._isOnMap) {
                om.insertAt(0, $this.layer);
                $this._isOnMap = true;
            }
        } else {
            for (i = 0; i < om.getLength(); i++) {
                console.log(om);
                if (om.getAt(i) === $this.layer) {
                    om.removeAt(i);
                    i--;
                }
            }
            $this._isOnMap = false;
        }
    };

    this.toggle = function () {
        $this._isActive = !$this._isActive;

        if ($this._isActive) {
            $this.$ctrl.addClass('active');
        } else {
            $this.$ctrl.removeClass('active');
        }

        $this._activateOverlay();
    };

    this.init = function () {
        if (this.default === true)
            this.toggle();
    }

    this.$ctrl.click(this.toggle.bind(this));
    google.maps.event.addListener(this._map, 'maptypeid_changed', this._activateOverlay.bind(this));
}