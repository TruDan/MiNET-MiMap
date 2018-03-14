var MiMapTileLayer = L.TileLayer.extend({
    options: {
        continuousWorld: true
    },

    _getTile: function (x, y, z) {
        var key = this._tileCoordsToKey({
            x: x,
            y: y,
            z: z
        });

        return this._tiles[key];
    },

    redrawTile: function (x, y, z) {
        var tile = this._getTile(x, y, z);

        if (typeof tile === 'undefined') return;

        tile.el.src = this.getTileUrl({
            x: x,
            y: y,
            z: z
        });
        //console.log("Tile Redraw", x, y, z);
    }
});

L.miMapTileLayer = function (url, opts) {
    return new MiMapTileLayer(url, opts);
}