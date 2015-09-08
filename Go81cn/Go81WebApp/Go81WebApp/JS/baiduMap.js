
//地图事件设置函数：
function setMapEvent(mapName) {
    mapName.enableDragging();//启用地图拖拽事件，默认启用(可不写)
    mapName.enableScrollWheelZoom();//启用地图滚轮放大缩小
    mapName.enableDoubleClickZoom();//启用鼠标双击放大，默认启用(可不写)
    mapName.enableKeyboard();//启用键盘上下左右键移动地图
}

//地图控件添加函数：
function addMapControl(mapName) {
    //向地图中添加缩放控件
    var ctrl_nav = new BMap.NavigationControl({ anchor: BMAP_ANCHOR_TOP_LEFT, type: BMAP_NAVIGATION_CONTROL_LARGE });
    mapName.addControl(ctrl_nav);
    //向地图中添加缩略图控件
    var ctrl_ove = new BMap.OverviewMapControl({ anchor: BMAP_ANCHOR_BOTTOM_RIGHT, isOpen: 1 });
    mapName.addControl(ctrl_ove);
    //向地图中添加比例尺控件
    var ctrl_sca = new BMap.ScaleControl({ anchor: BMAP_ANCHOR_BOTTOM_LEFT });
    mapName.addControl(ctrl_sca);
}
//标注点数组

//创建marker
function addMarker(mapName,xPos,yPos) {
    var markerArr = [{ title: "", content: "", point: xPos+"|"+yPos, isOpen: 0, icon: { w: 21, h: 21, l: 0, t: 0, x: 6, lb: 5 } }];
    for (var i = 0; i < markerArr.length; i++) {
        var json = markerArr[i];
        var p0 = json.point.split("|")[0];
        var p1 = json.point.split("|")[1];
        var point = new BMap.Point(p0, p1);
        //var iconImg = createIcon(json.icon);
        var iconImg=new BMap.Icon("../Images/marker.png", new BMap.Size(50, 50));
        var marker = new BMap.Marker(point, { icon: iconImg });
        var iw = createInfoWindow(i,xPos,yPos);
        marker.setAnimation(BMAP_ANIMATION_BOUNCE); //跳动的动画
        var label = new BMap.Label(json.title, { "offset": new BMap.Size(json.icon.lb - json.icon.x + 10, -20) });
        //marker.setLabel(label);
        mapName.addOverlay(marker);
        label.setStyle({
            borderColor: "#808080",
            color: "#333",
            cursor: "pointer"
        });

        (function () {
            var index = i;
            var _iw = createInfoWindow(i,xPos,yPos);
            var _marker = marker;
            _marker.addEventListener("click", function () {
                this.openInfoWindow(_iw);
            });
            _iw.addEventListener("open", function () {
                _marker.getLabel().hide();
            })
            _iw.addEventListener("close", function () {
                _marker.getLabel().show();
            })
            label.addEventListener("click", function () {
                _marker.openInfoWindow(_iw);
            })
            if (!!json.isOpen) {
                label.hide();
                _marker.openInfoWindow(_iw);
            }
        })()
    }
}
//创建InfoWindow
function createInfoWindow(i,xPos,yPos) {
    var markerArr = [{ title: "", content: "", point: xPos|yPos, isOpen: 0, icon: { w: 21, h: 21, l: 0, t: 0, x: 6, lb: 5 } }];
    var json = markerArr[i];
    var iw = new BMap.InfoWindow("<b class='iw_poi_title' title='" + json.title + "'>" + json.title + "</b><div class='iw_poi_content'>" + json.content + "</div>");
    return iw;
}
//创建一个Icon
function createIcon(json) {
    var icon = new BMap.Icon("http://app.baidu.com/map/images/us_mk_icon.png", new BMap.Size(json.w, json.h), { imageOffset: new BMap.Size(-json.l, -json.t), infoWindowOffset: new BMap.Size(json.lb + 5, 1), offset: new BMap.Size(json.x, json.h) })
    return icon;
}
//initMap();//创建和初始化地图
//map.addEventListener('click', function (e) {
//    var info = new BMap.InfoWindow('', { width: 260 });
//    var projection = this.getMapType().getProjection();
//    var lngLat = e.point;
//    var lngLatStr;
//    var geoc = new BMap.Geocoder();
//    geoc.getLocation(lngLat, function(rs){
//        var addComp = rs.addressComponents;
//        lngLatStr=addComp.province + "" + addComp.city + "" + addComp.district + "" + addComp.street + "" + addComp.streetNumber;
//        info.setContent(lngLatStr);
//        map.openInfoWindow(info, lngLat);
//    });
//    $("#hidloc_t").val(lngLat.lng+","+lngLat.lat);
//});
//map.addEventListener("dragend", function (e) {
//    $("#loc_ing").html("当前位置：" + e.point.lng + ", " + e.point.lat);
//    //alert("当前位置：" + e.point.lng + ", " + e.point.lat);
//});