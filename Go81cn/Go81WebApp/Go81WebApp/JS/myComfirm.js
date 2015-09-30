
/*
	调用方式：
	$(function(){ 
		$("input#aa").click(function(){ 
			if($("#comfirm-c").length>0){ 
                    $("#comfirm-c").fadeIn(200);
                }
                else{
                    $("body").myComfirm({ 
                        index:"comfirm-c",
                        title:"警告",
                        btncloseid:"btnclose",
                        btnsubid:"btnsub",
                        img:"url('../Images/alert-comfirm.png') -80px 70px", //提示信息相应图片  必须
                        content:"输入的价格不能高于商品参考价格！",  //提示信息内容 必须
                        iscomfirm:false,
                        onComfirm:function(){ 
                            $("#comfirm-c").fadeOut();
                        },
                        onCancel:function(){
                            
                        },
                    });
                }
		});
	});
*/
/*
  对话框
*/
;(function($){ 
	$.fn.myComfirm=function(options){ 
		var opts=$.extend({}, $.fn.myComfirm.defaults, options);
		$.fn.setHtml(opts);
		$("#"+opts.btncloseid).on('click', function() {
			$.fn.myComfirm.close(opts.onCancel);
		});
		$("#"+opts.btnsubid).on('click', function() {
			$.fn.myComfirm.comfirm(opts.onComfirm);
		});
	};
	$.fn.myComfirm.defaults = {
	    index: 'comfirm-a', //用于区分多次调用myComfirm，页面上如果调用了多次comfirm插件，可以用index来区分, 如果多次调用则必须
	    btncloseid: "btnClose", //定义取消按钮id
	    btnsubid: "btnSub", //定义确定按钮id
	    title: "提示", //提示框标题  可选
	    img: "warn.png", //提示信息相应图片  必须
	    content: "你确定要这样做吗？",  //提示信息内容 必须
	    iscomfirm: false, //是否提交数据，true 为是  false 为否
	    onComfirm: function () { }, //点击确定按钮时触发事件 必须
	    onCancel: function () { }, //点击取消按钮时触发事件 必须
	};
	$.fn.setHtml=function(opts){ 
		if(opts.iscomfirm){ 
			$("body").append("<div class='my-comfirm-modal-out' id='"+opts.index+"'><div class='my-comfirm-modal'></div><div class='my-comfirm-modal-content'><h3></h3><hr><div class='my-comfirm-info'><img src='../Images/tm.png' /><p></p></div><hr><div class='my-comfirm-btn'><input id='"+opts.btnsubid+"' class='btn-sub' type='button' value='确   定'><input id='"+opts.btncloseid+"' class='btn-close' type='button' value='取   消'></div></div>");
		}
	    else{ 
		    $("body").append("<div class='my-comfirm-modal-out' id='" + opts.index + "'><div class='my-comfirm-modal'></div><div class='my-comfirm-modal-content'><h3></h3><hr><div class='my-comfirm-info'><img src='../Images/tm.png' /></span><p></p></div><hr><div class='my-comfirm-btn'><input id='" + opts.btnsubid + "' class='btn-sub' type='button' value='确   定'>");
	    }
	 	$("#"+opts.index).find('h3').html(opts.title);
	 	$("#"+opts.index).find('.my-comfirm-modal-content > .my-comfirm-info > img').css({
	 		background: opts.img,
	 	}).end().find('.my-comfirm-modal-content > .my-comfirm-info >p').html(opts.content);
	};
	$.fn.myComfirm.close=function(callback){ 
	    callback();
	};
	$.fn.myComfirm.comfirm=function(callback){ 
		callback();
	};
})(jQuery);


/*
  单位级别联动
  1.0.0
  DuBo
*/
;(function ($) {
    var UnitLevel = function (jsonData, tagId) {
        var _this_ = this;

        var reg = new RegExp("&quot;", "g"); //创建正则RegExp对象
        var data = JSON.parse(jsonData.replace(reg, '"'));

        var select = $("<select></select>");
        select.css({ "width": "100px", "margin-left": "3px" }).bind("change", function () {
            _this_.searchUser(this, tagId, data);
        });

        var optionDefault = $("<option></option>");
        optionDefault.val("-1").html("请选择所属单位");

        select.append(optionDefault);

        //循环数组慎用for in,IE下容易出问题，即便是高版本IE
        for (var item = 0; item < data.length; item++) {
            var option = $("<option></option>");
            option.val(data[item].id).html(data[item].name);
            select.append(option);
        }
        $("#" + tagId).append(select);

    };
    UnitLevel.prototype = {
        //获取当前匹配id的下级单位列表
        //id：当前单位的id
        //list：当前单位的下级单位列表
        htmlSplice: function (id, list) {
            for (i in list) {
                if (parseInt(id) === list[i].id) {
                    return list[i].下级单位列表;
                }
            }
        },
        //select选项改变事件调用
        //e：触发当前事件的标签
        //tagId：触发当前事件标签的父标签
        searchUser: function (e, tagId, jsonData) {
            var _this_ = this;
            var id = $(e).val();
            var oDiv = $("#" + tagId);
            var count = $(e).prevAll().length;
            //移除当前select后面所有的select
            oDiv.children().eq(count).nextAll().remove();
            //如果选择的值是请选择所属单位，则移除该select,只有第一个select的时候不移除
            if (id == "-1") {
                $(e).nextAll().remove(); return false;
            }
            //根据当前选择的select前面所有select的个数加上该select，确定循环几级，并返回当前级别的下一级列表
            var arr = jsonData;
            for (var ik = 0; ik < count + 1; ik++) {
                var id = oDiv.children().eq(ik).val();
                arr = this.htmlSplice(id, arr);
            }
            //循环下级列表，拼接html串
            if (arr.length > 0) {
                var select = $("<select></select>");
                select.css({ "width": "100px", "margin-left": "3px" }).bind("change", function () {
                    _this_.searchUser(this, tagId, jsonData);
                });

                var optionDefault = $("<option></option>");
                optionDefault.val("-1").html("请选择所属单位");

                select.append(optionDefault);
                for (var item = 0; item < arr.length; item++) {
                    var option = $("<option></option>");
                    option.val(arr[item].id).html(arr[item].name);
                    select.append(option);
                }
                $("#" + tagId).append(select);
            }
        }
    };

    //初始化插件
    //data:后台传回的经过JsonConvert序列化的json字符串
    //tagId:触发当前事件select标签的父标签
    UnitLevel.Init = function (data, tagId) {
        new this(data, tagId);
    };

    window["UnitLevel"] = UnitLevel;
})(jQuery);