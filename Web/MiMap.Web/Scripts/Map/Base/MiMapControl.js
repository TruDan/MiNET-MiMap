var MiMapControl = L.Control.extend({
    options: {
        widgetLabel: ""
    },

    _controlParts: {
        _root: null,

        head: null,
        body: null,

        _handle: null,
        _label: null,
    },

    onAdd: function (map) {

        var body = this._createControl(this.options.widgetLabel);

        var inner = this._initControl(body);

        if (typeof inner !== 'undefined' && inner !== null) {
            body.appendChild(inner);
        }

        this._map = map;

        var control = this.getControl();
        // componentHandler.upgradeElement(control);
        return control;
    },

    _initControl: function () {
        return null;
    },


    _createControl: function (label) {
        if (typeof label === 'undefined') {
            label = "";
        }

        this._controlParts._root = L.DomUtil.create('div', 'mimap-control leaflet-control');


        var __map = this._map;

        // Disable dragging when user's cursor enters the element
        this._controlParts._root.addEventListener('mouseover', function () {
            __map.dragging.disable();
        });

        // Re-enable dragging when user's cursor leaves the element
        this._controlParts._root.addEventListener('mouseout', function () {
            __map.dragging.enable();
        });

        this._controlParts.head = L.DomUtil.create('div', 'mimap-control-header', this._controlParts._root);

        this._controlParts._handle = L.DomUtil.create('i', 'mimap-control-handle material-icons', this._controlParts.head);
        this._controlParts._handle.innerHTML = "drag_handle";

        this._controlParts._label = L.DomUtil.create('h1', '', this._controlParts.head);
        this._controlParts._label.innerHTML = label;

        this._controlParts.body = L.DomUtil.create('div', 'mimap-control-body', this._controlParts._root);

        return this._controlParts.body;
    },

    getControl: function () {
        return this._controlParts._root;
    },


    _createGroup: function (label, bodyElement) {
        if (typeof label === 'undefined') {
            label = "";
        }

        var rootDom = L.DomUtil.create('div', 'mimap-control-group', bodyElement);

        var labelDom = L.DomUtil.create('h1', '', rootDom);
        labelDom.innerHTML = label;

        var groupBodyDom = L.DomUtil.create('div', 'mimap-control-group-body ui form', rootDom);

        return groupBodyDom;
    }
});