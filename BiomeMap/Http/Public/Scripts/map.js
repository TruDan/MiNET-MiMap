
var TILE_SIZE = 256;

function loadLevel(levelId) {
    $.getJSON("/tiles/" + levelId + "/meta.json",
        function(levelData) {

            var bounds = {
                Left: levelData.Bounds.Min.X >> 4,
                Right: levelData.Bounds.Max.X >> 4,
                Top: levelData.Bounds.Min.Z >> 4,
                Bottom: levelData.Bounds.Max.Z >> 4
            };

            var size = levelData.TileSize.split(",").map(function (item) {
                return item.trim();
            });

            var levelMapLayer = new google.maps.ImageMapType({
                getTileUrl: function (coord, zoom) {

                    var x = (coord.x >> zoom);
                    var y = (coord.y >> zoom);
                    //console.log(coord, { x: x, y: y }, bounds);

                    //if (x < bounds.Left || x > bounds.Right || y < bounds.Top || y > bounds.Bottom)
                    //    return null;
                    
                    return "/tiles/" + levelData.Id + "/base/" + zoom + "/" + coord.x + "_" + coord.y + ".png?rand=" + new Date().getMilliseconds();
                },
                tileSize: new google.maps.Size(size[0], size[1]),
                maxZoom: levelData.MaxZoom,
                minZoom: levelData.MinZoom,
                name: levelData.Name || levelData.Id
            });
            map.mapTypes.set(levelData.Id, levelMapLayer);


            // Biomes


        });
}

function project(latLng) {
    var siny = Math.sin(latLng.lat() * Math.PI / 180);

    // Truncating to 0.9999 effectively limits latitude to 89.189. This is
    // about a third of a tile past the edge of the world tile.
    siny = Math.min(Math.max(siny, -0.9999), 0.9999);

    return new google.maps.Point(
        TILE_SIZE * (0.5 + latLng.lng() / 360),
        TILE_SIZE * (0.5 - Math.log((1 + siny) / (1 - siny)) / (4 * Math.PI)));
}

function initMap() {
    var map = new google.maps.Map(document.getElementById('map'), {
        center: new google.maps.LatLng(0, 0),
        minZoom: 0,
        maxZoom: 3,
        zoom: 0,
        isPng: true,

        zoomControl: true,
        streetViewControl: false,
        mapTypeControl: true,

        mapTypeControlOptions: {
            mapTypeIds: ['Overworld'],
            style: google.maps.MapTypeControlStyle.DROPDOWN_MENU
        },

        backgroundColor: '#212121'
    });
    window.map = map;

    

    /*
    var moonMapType = new google.maps.ImageMapType({
        getTileUrl: function (coord, zoom) {
            return "/tiles/" + zoom + "/" + coord.x + "_"+ coord.y + ".png?rand=" + new Date().getMilliseconds();
        },
        tileSize: new google.maps.Size(256, 256),
        maxZoom: 9,
        minZoom: 0,
        name: 'Overworld'
    });
    */
    //map.mapTypes.set('overworld', moonMapType);
    //map.setMapTypeId('overworld');

    loadLevel("Overworld");
    map.setMapTypeId('Overworld');

    /*var overlay = new google.maps.OverlayView();
    overlay.draw = function () { };
    overlay.setMap(map);

    // Show the lat and lng under the mouse cursor.
    var coordsDiv = document.getElementById('coords');
    map.controls[google.maps.ControlPosition.TOP_CENTER].push(coordsDiv);
    map.addListener('mousemove', function (event) {
        var p = overlay.getProjection().fromLatLngToDivPixel(event.latLng); 

        coordsDiv.textContent = "(x: " + p.x + ", y: " + p.y +")";
    });*/

    /** @constructor */
    function CoordMapType(tileSize) {
        this.tileSize = tileSize;
    }

    CoordMapType.prototype.getTile = function (coord, zoom, ownerDocument) {
        if (zoom < 1) return null;

        var tiles = 32 >> zoom;
        var size = (this.tileSize.width / tiles) + "px";

        var table = ownerDocument.createElement('table');
        table.cellSpacing = 0;
        table.style.borderCollapse = 'collapse';
        table.style.width = this.tileSize.width + 'px';
        table.style.height = this.tileSize.height + 'px';
        //table.style.borderCollapse = 'collapse';

        for (var i = 0; i < tiles; i++) {
            var tr = ownerDocument.createElement('tr');
            table.appendChild(tr);

            for (var j = 0; j < tiles; j++) {
                var td = ownerDocument.createElement('td');
                
                //td.style.width = size;
                //td.style.height = size;

                //td.style.padding = 0;
                //td.style.margin = 0;
                td.style.borderStyle = 'solid';
                td.style.borderWidth = '1px';
                td.style.borderColor = 'rgba(0, 0, 0, .1)';
                tr.appendChild(td);
            }
        }

        //var div = ownerDocument.createElement('div');
        //div.style.width = this.tileSize.width + 'px';
        //div.style.height = this.tileSize.height + 'px';
        //div.style.fontSize = '10';
        //div.style.borderStyle = 'solid';
        //div.style.borderWidth = '0px';
        //div.style.borderColor = 'rgba(255, 255, 255, .5)';
        //div.appendChild(table);
        return table;
    };

    // Insert this overlay map type as the first overlay map type at
    // position 0. Note that all overlay map types appear on top of
    // their parent base map.
    //map.overlayMapTypes.insertAt(
     //   0, new CoordMapType(new google.maps.Size(256, 256)));
}