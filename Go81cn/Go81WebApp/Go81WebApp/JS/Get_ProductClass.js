function getElementPosition(e) {
    var x = 0, y = 0;
    while (e != null) {
        x += e.offsetLeft;
        y += e.offsetTop;
        e = e.offsetParent;
    }
    return { x: x, y: y };
}

function GetSonClass(a) {
    var sp = $("#special").val();
    var type = a.lang;
    var isdx = document.getElementById("firstclass").getAttribute("name");
    var ullist = a.parentNode.getElementsByTagName("li");
    for (var i = 0; i < ullist.length; i++) {
        if (ullist[i] == a) {
            ullist[i].style.background = "rgb(83, 126, 43)";
            ullist[i].style.color = "black";
        }
        else {
            ullist[i].style.background = "#fff";
        }
    }


    var classname = a.parentNode.parentNode.getAttribute("id");

    //一级分类则直接加载已选二级列表
    if (classname == "firstclass") {
        document.getElementById("thirdclass").style.display = 'none';
        document.getElementById("secondclass").style.display = 'block';

        //清空结果
        if (document.getElementById("classidbox").value != "") {
            document.getElementById("classresult").style.display = 'none';
            document.getElementById("classidbox").value = "";
            if (isdx == "true") {
                $("#classresult span").children().remove();
            } else {
                $("#classresult").html("");
            }
        }

        //使二级分类未选
        var secondlis = document.getElementById("secondclass").getElementsByTagName("li");
        for (var j = 0; j < secondlis.length; j++) {
            secondlis[j].style.background = "#fff";
        }
        
        //return;
    }
    $.get("ProductClass?classid=" + encodeURI(a.getAttribute("id") + "&classname=" + a.innerHTML+"&type="+type+"&sp="+sp+"&isdx="+isdx), function (response) {
        if (classname == "firstclass") {
            document.getElementById("secondclass").style.display = 'block';
            $("#secondclass").html(response);
        }

        if (classname == "secondclass") {

            //判断是否是专用物资且只有二级商品目录
            if (response.indexOf("选择的商品类别是") > 0 && response.indexOf("<span") > -1) {
                document.getElementById("classresult").style.display = 'block';
                if (isdx == "true") {
                    var isexist = false;
                    var arr = $("#classidbox").val().split("|");
                    $.each(arr, function (index, dome) {
                        if (a.getAttribute("id") != dome) {
                            isexist = false;
                        } else {
                            isexist = true;
                            return false;
                        }
                    });
                    if (!isexist) {
                        $("#classresult span").append(response);
                        document.getElementById("classidbox").value += a.getAttribute("id") + "|";
                        isexistd = true;
                    }
                } else {
                    document.getElementById("classidbox").value = a.getAttribute("id");
                    $("#classresult").html(response);
                }
            }
            else {
                document.getElementById("thirdclass").style.display = 'block';
                $("#thirdclass").html(response);

                //清空结果
                if (document.getElementById("classidbox").value != "") {
                    document.getElementById("classresult").style.display = 'none';
                    document.getElementById("classidbox").value = "";
                    if (isdx == "true") {
                        $("#classresult span").children().remove();
                    } else {
                        $("#classresult").html("");
                    }
                }

                //使3级分类未选
                var thirdlis = document.getElementById("thirdclass").getElementsByTagName("li");
                for (var k = 0; k < thirdlis.length; k++) {
                    thirdlis[k].style.background = "#fff";
                }
            }
        }
        if (classname == "thirdclass") {
            document.getElementById("classresult").style.display = 'block';
            if (isdx == "true") {
                var isexistd = false;
                var arr1 = $("#classidbox").val().split("|");
                $.each(arr1, function (index, domele) {
                    if (a.getAttribute("id") != domele) {
                        isexistd = false;
                    } else {
                        isexistd = true;
                        return false;
                    }
                });
                if (!isexistd) {
                    $("#classresult span").append(response);
                    document.getElementById("classidbox").value += a.getAttribute("id") + "|";
                    isexist = true;
                }
            }
            else {
                document.getElementById("classidbox").value = a.getAttribute("id");
                $("#classresult").html(response);
            }
        }
    });

}
function ChechClassId(a) {
    var classid = document.getElementById("classidbox").value;
    var isbid = document.getElementById("isbid").value;
    if (classid == "") {
        document.getElementById("classresult").style.display = 'block';
        $("#classresult").html("<font color='red'>请先选择商品分类(如无商品分类信息，请在企业信息管理-->可提供商品类别栏目进行添加)</font>");
    }
    else {
        window.location.href = "Gys_Product_Add?classname=" + classid + "&bid=" + isbid;
    }
}
function GetLowerClass(a) {
    if (a.getAttribute("tag") == "0") {
        var classname = a.getAttribute("name");
        var name = "#" + classname;
        //var tmd = document.getElementById("tmd");
        //$(".listbox").css("left", getElementPosition(tmd).x + tmd.clientWidth - 2);

        $.get("/首页/IndexProductClass?classid=" + encodeURI(a.getAttribute("name")), function (response) {
            //document.getElementsById(classname.toString()).innerHTML = response;
            $(name).html(response);
            a.setAttribute("tag", "1")
        });
    }
}

//$(function () {
//    $("#secondclass").ajaxStart(function () {
//        $(this).html("<img src='../../Images/ajax-loader.gif' width='20' height='20'style='border:none;'/>正在加载..."); //ajax全局事件
//    })
//}
//)

