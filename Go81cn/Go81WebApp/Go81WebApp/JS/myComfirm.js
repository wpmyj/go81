
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