$(function () {
    var targetHtml = "<div class='modal fade' style='display:block;position: relative;*+top:expression(eval(document.compatMode && document.compatMode=='CSS1Compat') ? documentElement.scrollTop + (documentElement.clientHeight/25) - 70 : document.body.scrollTop + (document.body.clientHeight/25) - 70);'>"
                   + "<div class='modal-dialog' style='margin: 25% auto;'>"
                   + "<div class='modal-content' style='background: transparent;'>"
                   + "<div class='modal-body' style='background: #fff;'>"
                   + "<img style='width:30px; vertical-align: middle;' src='../Images/ajax-loader.gif' />"
                   + "</div></div></div></div>";


    $(".btn_target").click(function () {
        $("body").html(targetHtml);
    });
});


