//左侧导航 代码整理：www.97zzw.com - 97站长网
function subindexMenu() {
    var time;
    $("#js-menu").find("li").hover(function () {
        var la = $(this);
        $(this).css("z-index", "9999");
        time=setTimeout(function () {
            la.find("h3 a").addClass("on");
            la.find(".listbox").show();
        }, 300);
    }, function () {
        clearTimeout(time);
            $(this).removeAttr("style");
            $(this).find("h3 a").removeClass("on");
        $(this).find(".listbox").hide();
    });
}
function BindEnter(obj,btn) {
    if (obj.keyCode == 13) {
        obj.cancelBubble = true;
        obj.returnValue = false;
        document.getElementById(btn).click();
    }
}
    
$(function(){
    subindexMenu();
    var timer;
    $("#classify_bar").hover(function () {
        var bar = $("#classify_bar");
        timer=setTimeout(function() {
            bar.children('#hidden_classify').slideDown('fast');
            bar.children("a").css({ "background": "url(/Images/up.png) right center no-repeat" });
        },500);
    }, function () {
        clearTimeout(timer);
        $("#hidden_classify").hover(function () {
            $(this).css({ "display": "block" });
        }, function () {

            if ($(this).children(".listbox").css("display") == "block") {
                $(this).css({ "display": "block" });
            }
            else {
                $(this).hide().parent("#classify_bar").children("a").css({ "background": "url(/Images/down.png) right center no-repeat " });

            }
        });
        $("#hidden_classify").hide().parent("#classify_bar").children("a").css({ "background": "url(/Images/down.png) right center no-repeat" });
    });
})