
    function change_box(th, con_id) {
        $("#sp span").each(
            function () {
                $(this).css("background-color", "orange");
            });
        $(th).css({ "color": "white", "background-color": "red" });
        $("#User_Info div").each(function(){$(this).css("z-index","1");});
        switch (con_id) {
            case 'Login': $("#" + con_id).css({"z-index":"10","border-top-color":"red","border-top-style":"solid","border-top-width":"1px"}); break;
            case 'Address': $("#" + con_id).css({ "z-index": "10", "border-top-color": "red", "border-top-style": "solid", "border-top-width": "1px" }); break;
            case 'Contact': $("#" + con_id).css({ "z-index": "10", "border-top-color": "red", "border-top-style": "solid", "border-top-width": "1px" }); break;
            case 'Part_Man': $("#" + con_id).css({ "z-index": "10", "border-top-color": "red", "border-top-style": "solid", "border-top-width": "1px" }); break;
            case 'Group': $("#" + con_id).css({ "z-index": "10", "border-top-color": "red", "border-top-style": "solid", "border-top-width": "1px" }); break;
        }
    }

    function checks(th, str) {
        $(".Tab a").each(function () {
            $(this).css({ "color": "black", "background-color": "#D3D1D1" });
        });
        $(th).addClass("tabhover").css({ "color": "white", "background-color": "#64844E" });
        $(".tb2,.tb3,.tb4,.tb5,.tb6,.tb7,.tb8,.tb9").hide();
        $("."+str).show();
    }
   $("a.modify").click(function(){
        $(":text").each(function(){$(this).css({"background-color":"white","border":"1px solid black"});$(this).removeAttr("disabled");});
        $("select").each(function(){$(this).css("border","1px solid black");$(this).removeAttr("disabled");});
        $("textarea").each(function(){$(this).css({"background-color":"white","border":"1px solid black"});$(this).removeAttr("disabled");});
   })
