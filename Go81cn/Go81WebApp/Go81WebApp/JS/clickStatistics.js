function clickStatistics(position, type, id, gysid) {
    var parm = "position=" + position + "&type=" + type + "&id=" + id + "&gysid=" + gysid;
    $.ajax({
        cache: false,
        async: false,
        url: "/首页/clickStatistics",
        data: parm,
    });
}