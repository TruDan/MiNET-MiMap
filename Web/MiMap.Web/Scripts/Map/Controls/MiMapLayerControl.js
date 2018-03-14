var MiMapLayerControl = MiMapControl.extend({

    options: {
        widgetLabel: "Layer Select"
    },

    _layerControlParts: {
        _baseLayersList: null,
        _overlayLayersList: null
    },

    initialize: function (layers, options) {
        L.Util.setOptions(this, options);

        this._layerControlInputs = [];
        this._layers = [];
        this._lastZIndex = 0;
        this._handlingClick = false;

        for (var i in layers) {
            this._addLayer(layers[i].layer, i, layers[i].overlays);
        }

        //this._updateOverlays();
    },

    _update: function () {
        if (!this._layerControlParts._baseLayersList) { return this; }

        L.DomUtil.empty(this._layerControlParts._baseLayersList);
        L.DomUtil.empty(this._layerControlParts._overlayLayersList);

        this._layerControlInputs = [];
        var i, j, obj, baseLayersCount = 0;

        for (i = 0; i < this._layers.length; i++) {
            obj = this._layers[i];
            this._addBaseLayerItem(obj);

            baseLayersCount++;

            if (this._map.hasLayer(obj.layer)) {
                for (j = 0; j < obj.overlays.length; j++) {
                    this._addOverlayLayerItem(obj.overlays[j]);
                }
            }
        }


        return this;
    },

    _initControl: function (body) {

        this._layerControlParts._baseLayersWrapper = this._createGroup("Layers", body);
        this._layerControlParts._baseLayersList =
            L.DomUtil.create("div", "grouped fields", this._layerControlParts._baseLayersWrapper);

        this._map.on('zoomend', this._checkDisabledLayers, this);

        for (var i = 0; i < this._layers.length; i++) {

            this._layers[i].layer.on('add remove', this._onLayerChange, this);
        }


        this._layerControlParts._overlayLayersWrapper = this._createGroup("Overlays", body);
        this._layerControlParts._overlayLayersList =
            L.DomUtil.create("div", "grouped fields", this._layerControlParts._overlayLayersWrapper);
    },

    _addLayer: function (layer, name, overlayLayers) {
        if (this._map) {
            layer.on('add remove', this._onLayerChange, this);
        }

        var overlays = [];
        for (var i in overlayLayers) {
            if (this._map) {
                overlayLayers[i].on('add remove', this._onLayerChange, this);
            }

            overlays.push({
                layer: overlayLayers[i],
                label: i
            });
        }

        var l = {
            layer: layer,
            label: name,
            overlays: overlays
        };

        this._layers.push(l);
    },

    _addBaseLayerItem: function (obj) {

        var item = this._createItem(obj);

        this._layerControlParts._baseLayersList.appendChild(item);

        this._checkDisabledLayers();
        return item;
    },

    _addOverlayLayerItem: function (obj) {
        var item = this._createItem(obj);

        this._layerControlParts._overlayLayersList.appendChild(item);

        this._checkDisabledLayers();
        return item;
    },

    _getLayer: function (id) {
        for (var i = 0; i < this._layers.length; i++) {

            if (this._layers[i] && L.Util.stamp(this._layers[i].layer) === id) {
                return this._layers[i];
            }

            if (this._layers[i].overlays) {
                for (var j = 0; j < this._layers[i].overlays.length; j++) {

                    if (this._layers[i].overlays[j] && L.Util.stamp(this._layers[i].overlays[j].layer) === id) {
                        return this._layers[i].overlays[j];
                    }
                }
            }
        }
    },

    _createItem: function (obj) {

        var field, wrapper,
            checked = this._map.hasLayer(obj.layer),
            input, text;

        field = L.DomUtil.create("div", "field");



        if (typeof obj.overlays === 'undefined') {
            wrapper = L.DomUtil.create('div', 'ui checkbox', field);
            input = L.DomUtil.create('input');

            input.type = 'checkbox';
            input.defaultChecked = checked;
        } else {
            wrapper = L.DomUtil.create('div', 'ui radio checkbox', field);
            input = this._createRadioElement('leaflet-base-layers', checked);
        }
        text = L.DomUtil.create('label');

        this._layerControlInputs.push(input);
        input.layerId = L.Util.stamp(obj.layer);

        L.DomEvent.on(input, 'click', this._onInputClick, this);

        //var name = document.createElement('span');
        //name.innerHTML = ' ' + obj.name;

        // Helps from preventing layer control flicker when checkboxes are disabled
        // https://github.com/Leaflet/Leaflet/issues/2771
        //var holder = L.DomUtil.create('div', 'control-group');
        //holder.appendChild(label);

        text.innerHTML = obj.label;

        wrapper.appendChild(input);
        wrapper.appendChild(text);

        //var container = isOverlay ? this._overlaysList : this._baseLayersList;
        //container.appendChild(holder);

        this._checkDisabledLayers();
        //componentHandler.upgradeElement(label);

        return field;
    },

    // IE7 bugs out if you create a radio dynamically, so you have to do it this hacky way (see http://bit.ly/PqYLBe)
    _createRadioElement: function (name, checked) {

        var radioHtml = '<input type="radio" name="' +
            name + '"' + (checked ? ' checked="checked"' : '') + '/>';

        var radioFragment = document.createElement('div');
        radioFragment.innerHTML = radioHtml;

        return radioFragment.firstChild;
    },

    _onLayerChange: function (e) {
        if (!this._handlingClick) {
            this._update();
        }

        var obj = this._getLayer(L.Util.stamp(e.target));

        // @namespace Map
        // @section Layer events
        // @event baselayerchange: LayersControlEvent
        // Fired when the base layer is changed through the [layer control](#control-layers).
        // @event overlayadd: LayersControlEvent
        // Fired when an overlay is selected through the [layer control](#control-layers).
        // @event overlayremove: LayersControlEvent
        // Fired when an overlay is deselected through the [layer control](#control-layers).
        // @namespace Control.Layers
        var type = (typeof obj.overlay === 'undefined') ?
            (e.type === 'add' ? 'overlayadd' : 'overlayremove') :
            (e.type === 'add' ? 'baselayerchange' : null);

        if (type) {
            this._map.fire(type, obj);
        }
    },

    _onInputClick: function () {
        var inputs = this._layerControlInputs,
            input, layer, hasLayer;
        var addedLayers = [],
            removedLayers = [];

        this._handlingClick = true;

        for (var i = inputs.length - 1; i >= 0; i--) {
            input = inputs[i];
            layer = this._getLayer(input.layerId).layer;
            hasLayer = this._map.hasLayer(layer);

            if (input.checked && !hasLayer) {
                addedLayers.push(layer);

            } else if (!input.checked && hasLayer) {
                removedLayers.push(layer);
            }
        }

        // Bugfix issue 2318: Should remove all old layers before readding new ones
        for (i = 0; i < removedLayers.length; i++) {
            this._map.removeLayer(removedLayers[i]);
        }
        for (i = 0; i < addedLayers.length; i++) {
            this._map.addLayer(addedLayers[i]);
        }

        this._handlingClick = false;

        this._refocusOnMap();
    },

    _checkDisabledLayers: function () {
        var inputs = this._layerControlInputs,
            input,
            layer,
            zoom = this._map.getZoom();

        for (var i = inputs.length - 1; i >= 0; i--) {
            input = inputs[i];
            layer = this._getLayer(input.layerId).layer;
            input.disabled = (layer.options.minZoom !== undefined && zoom < layer.options.minZoom) ||
                (layer.options.maxZoom !== undefined && zoom > layer.options.maxZoom);

        }
    }
});


L.control.miMapLayerControl = function (baseLayers, opts) {
    return new MiMapLayerControl(baseLayers, opts);
};