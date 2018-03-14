function MiMapWidgetManager(map) {

    this._map = map;

    var $this = this;

    this.init = function (configs) {
        for (var config in configs) {
            $this.loadWidget(config);
        }
    };

    this.loadWidget = function (widgetData) {
        var widget = new MiMapWidget(widgetData);
        widget.addTo($this._map);
    };

    $.getJSON("widgets/config.json", this.init.bind(this));

}

var MiMapWidget = MiMapControl.extend({

    _elementsConfig: [],

    initialize: function (data) {
        this.options.position = data.position.toLowerCase();
        this.options.widgetLabel = data.label;
        this.options.moveable = data.moveable;
        this.options.collapsible = data.collapsible;
        this.options.iscollapsed = data.iscollapsed;

        // Load elements
        this._elementsConfig = data.elements;
    },

    _initControl: function (body) {

        for (var element in this._elementsConfig) {
            this.addElement(body, element);
        }

    },

    addElement: function (container, e) {

        if (typeof (e.elements) !== "undefined") {
            // it's a group.
            var dom = this._createGroup(e.label, container);

            for (var childElement in e.elements) {
                this.addElement(dom, childElement);
            }

        } else {

            if (e.elementType.toLowerCase() == 'text') {
                return this._createTextElement(container, e);
            }
        }
    },

    _createGroupElement: function (container, element) {
        var rootDom = L.DomUtil.create("ul", "mdl-list", container);


    },

    _createTextElement: function (container, element) {
        var dom = L.DomUtil.create("label", container);
        var text = L.DomUtil.create("")
    }

});