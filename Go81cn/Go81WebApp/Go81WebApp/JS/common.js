//菜单导航固定
var jQuery=$;
jQuery().ready(function () {
	//导航距离屏幕顶部距离
	var _defautlTop = jQuery(".menu").offset().top; 
	//导航距离屏幕左侧距离
	var _defautlLeft = jQuery(".menu").offset().left;
	//导航默认样式记录，还原初始样式时候需要
	var _position = jQuery(".menu").css('position');
	var _top = jQuery(".menu").css('top');
	var _left = jQuery(".menu").css('left');
	var _zIndex = jQuery(".menu").css('z-index');
	//鼠标滚动事件
	jQuery(window).scroll(function () {
		if (jQuery(this).scrollTop() > _defautlTop) {
			//IE6不认识position:fixed，单独用position:absolute模拟
			if (jQuery.browser.msie && jQuery.browser.version == "6.0") {
				jQuery(".menu").css({ 'position': 'absolute', 'top': eval(document.documentElement.scrollTop), 'left': _defautlLeft, 'z-index': 99999 });
				//防止出现抖动
				jQuery("html,body").css({ 'background-image': 'url(about:blank)', 'background-attachment': 'fixed' });
			} else {
				jQuery(".menu").css({ 'position': 'fixed', 'top': 0, 'left': _defautlLeft, 'z-index': 99999 });
			}
		} else {
			jQuery(".menu").css({ 'position': _position, 'top': _top, 'left': _left, 'z-index': _zIndex });
		}
	});
	//结束
	
	//下拉菜单开始
	function megaHoverOver(){
		jQuery(this).find("#sub").stop().fadeTo('fast', 1).show();
			
		//Calculate width of all ul's
		(function(jQuery) { 
			jQuery.fn.calcSubWidth = function() {
				rowWidth = 0;
				//Calculate row
				jQuery(this).find("ul").each(function() {					
					rowWidth += jQuery(this).width(); 
				});	
			};
		})(jQuery); 
		
		if ( jQuery(this).find(".row").length > 0 ) { //If row exists...
			var biggestRow = 0;	
			//Calculate each row
			jQuery(this).find(".row").each(function() {							   
				jQuery(this).calcSubWidth();
				//Find biggest row
				if(rowWidth > biggestRow) {
					biggestRow = rowWidth;
				}
			});
			//Set width
			jQuery(this).find(".row:last").css({'margin':'0'});
			
		} else { //If row does not exist...
			
			jQuery(this).calcSubWidth();
			//Set Width
			jQuery(this).find("#sub").css({'width' : rowWidth});
			
		}
	}
	
	function megaHoverOut(){ 
	 jQuery(this).find("#sub").stop().fadeTo('fast', 0, function() {
		 jQuery(this).hide(); 
	  });
	}

	var config = {    
		 sensitivity: 2, // number = sensitivity threshold (must be 1 or higher)    
		 interval: 100, // number = milliseconds for onMouseOver polling interval    
		 over: megaHoverOver, // function = onMouseOver callback (REQUIRED)    
		 timeout: 500, // number = milliseconds delay before onMouseOut    
		 out: megaHoverOut // function = onMouseOut callback (REQUIRED)    
	};
	
	//jQuery("ul#topnav li #sub").css({'opacity':'0'});
	jQuery("ul#topnav li").hoverIntent(config);
	
	//首页样式
	jQuery(".message_c li:last").addClass("noborder");
	//内容页样式
	jQuery(".nr_l_c_r_li li:last").addClass("noborder");
	jQuery(".nr_reading_b_l li:last").addClass("noborder");
	jQuery(".gy_right2 dl:last").addClass("noborder");
	//专家列表页样式
	jQuery(".zj_right2 li:eq(0)").addClass("hover");
	jQuery("#conneimenu1").removeClass("none");
	
	//重点科室
	jQuery(".ks_fl_t li:eq(0)").addClass("hover");
	jQuery("#conksmenu1").removeClass("none");
	
	//医院品牌
	jQuery("#conyypp1").removeClass("none");
	
	//单病种频道页
	jQuery(".bz_hot_list ul li:eq(3)").addClass("mgr0");
	
	jQuery("#ask_left_fl0 dd div:odd").addClass("noborder");
	jQuery("#ask_left_fl1 dd div:odd").addClass("noborder");
	jQuery("#ask_left_fl2 dd div:odd").addClass("noborder");
	jQuery("#ask_left_fl3 dd div:odd").addClass("noborder");
	jQuery("#ask_left_fl4 dd div:odd").addClass("noborder");
	jQuery("#ask_left_fl5 dd div:odd").addClass("noborder");
	jQuery("#ask_left_fl6 dd div:odd").addClass("noborder");
	jQuery("#ask_left_fl7 dd div:odd").addClass("noborder");
	
	
	//左侧菜单
	if(null!=document.getElementById('zj_leftmenu1')){
		Thishover();	
	}
	if(null!=document.getElementById('zj_leftmenu_about1')){
		Thishover_about();
	}
	if(null!=document.getElementById('zj_leftmenu_jyzn1')){
		Thishover_jyzn();
	}
	
		
	//调用在咨询人数
	if(null!=document.getElementById('myljzx') && null!=document.getElementById('myljzxcount'))
	{
	swtnum();
	swtnumcount();
	setInterval("swtnum()",5000);
	}
});


//专家滚动开始
function zjrolling(rid) {
    var id = "#" + rid,
    len = jQuery('.cont_roll', id).children().length,
    width = 0,
    autoroll;
    for (var j = 0; j < len; j++) {
        jQuery('.cont_roll a', id).eq(j).children("img").attr('alt', j);
    }
    for (var i = len - 1; i >= 0; i--) {
        width += jQuery('.cont_roll', id).children().eq(i).outerWidth(true);
    }
    jQuery('.cont_roll', id).width(width);
    jQuery('.cont_roll a', id).eq(0).addClass("select");
    jQuery('.cont_roll a', id).hover(function() {
        //jQuery(this).addClass("select").siblings().removeClass();
       // var aIndex = jQuery('.cont_roll a.select', id).children("img").attr("alt");
        //jQuery("div.addxz_yd").eq(aIndex).show().siblings("div.addxz_yd").hide();
    },
    function() {
        //setInterval(autoroll, 0);
    })
    jQuery(id).hover(function() {
        clearInterval(autoroll);
        return false;
    },
    function() {
        autoroll = setInterval(roll_left = function() {
            var mleft = jQuery(id + ' .cont_roll').children().first().outerWidth(true);
            jQuery(id + ' .cont_roll').animate({
                marginLeft: "-=" + mleft
            },
            320,
            function() {
                jQuery('.cont_roll', id).children().first().appendTo(jQuery('.cont_roll', id)).parent().css("marginLeft", 0);
                var zjtxtNum = jQuery('.cont_roll a', id).eq(0).children("img").attr('alt');
                jQuery('.cont_roll a', id).eq(0).addClass("select").siblings().removeClass("select");
                jQuery("div.addxz_yd").eq(zjtxtNum).show().siblings("div.addxz_yd").hide();
            });
        },
        4000);
    }).trigger("mouseout");
    jQuery(".prev", id).click(function() {
        var mleft = jQuery(id + ' .cont_roll').children().first().outerWidth(true);
        jQuery(id + ' .cont_roll').animate({
            marginLeft: "-=" + mleft
        },
        320,
        function() {
            jQuery('.cont_roll', id).children().first().appendTo(jQuery('.cont_roll', id)).parent().css("marginLeft", 0);
            var zjtxtNum = jQuery('.cont_roll a', id).eq(0).children("img").attr('alt');
            jQuery('.cont_roll a', id).eq(0).addClass("select").siblings().removeClass("select");
            jQuery("div.addxz_yd").eq(zjtxtNum).show().siblings("div.addxz_yd").hide();
        });
    });
    jQuery(".next", id).click(function() {
        var mleft = jQuery('.cont_roll', id).children().last().outerWidth(true);
        jQuery('.cont_roll', id).css("marginLeft", "-" + mleft + "px").prepend(jQuery('.cont_roll', id).children().last()).animate({
            marginLeft: "+=" + mleft
        },
        320);
        var zjtxtNum = jQuery('.cont_roll a', id).eq(0).children("img").attr('alt');
        jQuery('.cont_roll a', id).eq(0).addClass("select").siblings().removeClass("select");
        jQuery("div.addxz_yd").eq(zjtxtNum).show().siblings("div.addxz_yd").hide();
    });
} 
//滚动-横向带控制(顶尖专家)结束

//医院类切换效果菜单
function Thishover(){
	var myurl=window.location.pathname;
	var thisurl1="/yyjj";
	var thisurl2="/zjtd";
	var thisurl3="/hzzx";
	var thisurl4="/zllf";
	var thisurl5="/xsjl";
	var thisurl6="/zdks";
	var thisurl7="/ryzz";
	var thisurl8="/xjsb";
	var thisurl9="/jyhj";
	var thisurl10="/gyhd";
	var thisurl11="/jyzn";
	for(i=1;i<=11;i++){
		if (myurl.indexOf(eval("thisurl"+i))>=0){
			document.getElementById("zj_leftmenu"+i).className="hover";
		}
  	}
}
//关于我们切换效果菜单
function Thishover_about(){
	var myurl2=window.location.pathname;
	var thisurl21="/data/sitemap.html";
	var thisurl22="/gywm/zxnc";
	var thisurl23="/gywm/yjfk";
	var thisurl24="/gywm/720.html";
	for(i=1;i<=4;i++){
		if (myurl2.indexOf(eval("thisurl2"+i))>=0){
			document.getElementById("zj_leftmenu_about"+i).className="hover";
		}
  	}
}

//关于我们切换效果菜单
function Thishover_jyzn(){
	var myurl3=window.location.pathname;
	var thisurl31="/jyzn/lylx/";
	var thisurl32="/jyzn/2167.html";
	var thisurl33="/jyzn/tjzx/";
	var thisurl34="/jyzn/ybzq/";
	var thisurl35="/jyzn/2153.html";
	var thisurl36="/jyzn/91.html";
	for(i=1;i<=6;i++){
		if (myurl3.indexOf(eval("thisurl3"+i))>=0){
			document.getElementById("zj_leftmenu_jyzn"+i).className="hover";
		}
  	}
}

function patient_nav(){
	var len = jQuery(".patient_nav > .nav_item").length;
	for(var i = 0;i< len;i++){ 
		jQuery(".patient_nav > .nav_item").eq(i).addClass("ch"+(i+1));
	}
	jQuery(".patient_nav > .nav_item:not('.no_tab')").hover(function(){
		jQuery(this).addClass("cur");jQuery("#consubmenu0").addClass("none");
	},
	function(){
		jQuery(this).removeClass("cur");jQuery("#consubmenu0").removeClass("none");
	});
}//首页患者实用导航

//首页切换效果
function set(name,cursel,n){
  for(i=1;i<=n;i++){
	  var menu=document.getElementById(name+i);
	  var con=document.getElementById("con"+name+i);
	  if(menu == null || con == null)continue;
	  menu.className=i==cursel?"hover":"";
	  con.style.display=i==cursel?"block":"none";
  } 
}
//首页切换效果
function setleftmenu(name,cursel,n){
  document.getElementById("consubmenu0").style.display="none";
  for(i=1;i<=n;i++){
	  var menu=document.getElementById(name+i);
	  var con=document.getElementById("con"+name+i);
	  if(menu == null || con == null)continue;
	  menu.className=i==cursel?"hover":"";
	  con.style.display=i==cursel?"block":"none";
  }
}
function setbannerdisplay(name,cursel,n){
  document.getElementById("consubmenu0").style.display="block";
  for(i=1;i<=n;i++){
	  var menu=document.getElementById(name+i);
	  var con=document.getElementById("con"+name+i);
	  if(menu == null || con == null)continue;
	  menu.className="";
	  con.style.display="none";
  }
}

//首页菜单切换
//首页切换效果
function setmenu(name,cursel,n){
  for(i=1;i<=n;i++){
	  var menu=document.getElementById(name+i);
	  if(menu == null)continue;
	  menu.className=i==cursel?"hover":"";
  } 
}


//首页菜单切换

//改变字体大小
function doZoom(size) {
	var zoom = document.all ? document.all['Zoom'] : document.getElementById('Zoom');
	zoom.style.fontSize = size + 'px';
}
//环境切换效果
var zhangxu = {
    $: function(objName) {
        if (document.getElementById) {
            return eval('document.getElementById("' + objName + '")')
        } else {
            return eval('document.all.' + objName)
        }
    },
    isIE: navigator.appVersion.indexOf("MSIE") != -1 ? true: false,
    addEvent: function(l, i, I) {
        if (l.attachEvent) {
            l.attachEvent("on" + i, I)
        } else {
            l.addEventListener(i, I, false)
        }
    },
    delEvent: function(l, i, I) {
        if (l.detachEvent) {
            l.detachEvent("on" + i, I)
        } else {
            l.removeEventListener(i, I, false)
        }
    },
    readCookie: function(O) {
        var o = "",
        l = O + "=";
        if (document.cookie.length > 0) {
            var i = document.cookie.indexOf(l);
            if (i != -1) {
                i += l.length;
                var I = document.cookie.indexOf(";", i);
                if (I == -1) I = document.cookie.length;
                o = unescape(document.cookie.substring(i, I))
            }
        };
        return o
    },
    writeCookie: function(i, l, o, c) {
        var O = "",
        I = "";
        if (o != null) {
            O = new Date((new Date).getTime() + o * 3600000);
            O = "; expires=" + O.toGMTString()
        };
        if (c != null) {
            I = ";domain=" + c
        };
        document.cookie = i + "=" + escape(l) + O + I
    },
    readStyle: function(I, l) {
        if (I.style[l]) {
            return I.style[l]
        } else if (I.currentStyle) {
            return I.currentStyle[l]
        } else if (document.defaultView && document.defaultView.getComputedStyle) {
            var i = document.defaultView.getComputedStyle(I, null);
            return i.getPropertyValue(l)
        } else {
            return null
        }
    }
};

//滚动图片构造函数
//UI&UE Dept. mengjia
//080623
function ScrollPic(scrollContId, arrLeftId, arrRightId, dotListId) {
    this.scrollContId = scrollContId;
    this.arrLeftId = arrLeftId;
    this.arrRightId = arrRightId;
    this.dotListId = dotListId;
    this.dotClassName = "dotItem";
    this.dotOnClassName = "dotItemOn";
    this.dotObjArr = [];
    this.pageWidth = 0;
    this.frameWidth = 0;
    this.speed = 10;
    this.space = 10;
    this.pageIndex = 0;
    this.autoPlay = true;
    this.autoPlayTime = 5;
    var _autoTimeObj, _scrollTimeObj, _state = "ready";
    this.stripDiv = document.createElement("DIV");
    this.listDiv01 = document.createElement("DIV");
    this.listDiv02 = document.createElement("DIV");
    if (!ScrollPic.childs) {
        ScrollPic.childs = []
    };
    this.ID = ScrollPic.childs.length;
    ScrollPic.childs.push(this);
    this.initialize = function() {
        if (!this.scrollContId) {
            throw new Error("必须指定scrollContId.");
            return
        };
        this.scrollContDiv = zhangxu.$(this.scrollContId);
        if (!this.scrollContDiv) {
            throw new Error("scrollContId不是正确的对象。(scrollContId = \"" + this.scrollContId + "\")");
            return
        };
        this.scrollContDiv.style.width = this.frameWidth + "px";
        this.scrollContDiv.style.overflow = "hidden";
        this.listDiv01.innerHTML = this.listDiv02.innerHTML = this.scrollContDiv.innerHTML;
        this.scrollContDiv.innerHTML = "";
        this.scrollContDiv.appendChild(this.stripDiv);
        this.stripDiv.appendChild(this.listDiv01);
        this.stripDiv.appendChild(this.listDiv02);
        this.stripDiv.style.overflow = "hidden";
        this.stripDiv.style.zoom = "1";
        this.stripDiv.style.width = "36855px";
        this.listDiv01.style.cssFloat = "left";
		this.listDiv01.style.styleFloat = "left"; 
        this.listDiv02.style.cssFloat = "left";
		this.listDiv02.style.styleFloat = "left"; 
        zhangxu.addEvent(this.scrollContDiv, "mouseover", Function("ScrollPic.childs[" + this.ID + "].stop()"));
        zhangxu.addEvent(this.scrollContDiv, "mouseout", Function("ScrollPic.childs[" + this.ID + "].play()"));
        if (this.arrLeftId) {
            this.arrLeftObj = zhangxu.$(this.arrLeftId);
            if (this.arrLeftObj) {
                zhangxu.addEvent(this.arrLeftObj, "mousedown", Function("ScrollPic.childs[" + this.ID + "].rightMouseDown()"));
                zhangxu.addEvent(this.arrLeftObj, "mouseup", Function("ScrollPic.childs[" + this.ID + "].rightEnd()"));
                zhangxu.addEvent(this.arrLeftObj, "mouseout", Function("ScrollPic.childs[" + this.ID + "].rightEnd()"))
            }
        };
        if (this.arrRightId) {
            this.arrRightObj = zhangxu.$(this.arrRightId);
            if (this.arrRightObj) {
                zhangxu.addEvent(this.arrRightObj, "mousedown", Function("ScrollPic.childs[" + this.ID + "].leftMouseDown()"));
                zhangxu.addEvent(this.arrRightObj, "mouseup", Function("ScrollPic.childs[" + this.ID + "].leftEnd()"));
                zhangxu.addEvent(this.arrRightObj, "mouseout", Function("ScrollPic.childs[" + this.ID + "].leftEnd()"))
            }
        };
        if (this.dotListId) {
            this.dotListObj = zhangxu.$(this.dotListId);
            if (this.dotListObj) {
                var pages = Math.round(this.listDiv01.offsetWidth / this.frameWidth + 0.4),
                i,
                tempObj;
                for (i = 0; i < pages; i++) {
                    tempObj = document.createElement("span");
                    this.dotListObj.appendChild(tempObj);
                    this.dotObjArr.push(tempObj);
                    if (i == this.pageIndex) {
                        tempObj.className = this.dotClassName
                    } else {
                        tempObj.className = this.dotOnClassName
                    };
                    tempObj.title = "第" + (i + 1) + "页";
                    zhangxu.addEvent(tempObj, "click", Function("ScrollPic.childs[" + this.ID + "].pageTo(" + i + ")"))
                }
            }
        };
        if (this.autoPlay) {
            this.play()
        }
    };
    this.leftMouseDown = function() {
        if (_state != "ready") {
            return
        };
        _state = "floating";
        _scrollTimeObj = setInterval("ScrollPic.childs[" + this.ID + "].moveLeft()", this.speed)
    };
    this.rightMouseDown = function() {
        if (_state != "ready") {
            return
        };
        _state = "floating";
        _scrollTimeObj = setInterval("ScrollPic.childs[" + this.ID + "].moveRight()", this.speed)
    };
    this.moveLeft = function() {
        if (this.scrollContDiv.scrollLeft + this.space >= this.listDiv01.scrollWidth) {
            this.scrollContDiv.scrollLeft = this.scrollContDiv.scrollLeft + this.space - this.listDiv01.scrollWidth
        } else {
            this.scrollContDiv.scrollLeft += this.space
        };
        this.accountPageIndex()
    };
    this.moveRight = function() {
        if (this.scrollContDiv.scrollLeft - this.space <= 0) {
            this.scrollContDiv.scrollLeft = this.listDiv01.scrollWidth + this.scrollContDiv.scrollLeft - this.space
        } else {
            this.scrollContDiv.scrollLeft -= this.space
        };
        this.accountPageIndex()
    };
    this.leftEnd = function() {
        if (_state != "floating") {
            return
        };
        _state = "stoping";
        clearInterval(_scrollTimeObj);
        var fill = this.pageWidth - this.scrollContDiv.scrollLeft % this.pageWidth;
        this.move(fill)
    };
    this.rightEnd = function() {
        if (_state != "floating") {
            return
        };
        _state = "stoping";
        clearInterval(_scrollTimeObj);
        var fill = -this.scrollContDiv.scrollLeft % this.pageWidth;
        this.move(fill)
    };
    this.move = function(num, quick) {
        var thisMove = num / 5;
        if (!quick) {
            if (thisMove > this.space) {
                thisMove = this.space
            };
            if (thisMove < -this.space) {
                thisMove = -this.space
            }
        };
        if (Math.abs(thisMove) < 1 && thisMove != 0) {
            thisMove = thisMove >= 0 ? 1 : -1
        } else {
            thisMove = Math.round(thisMove)
        };
        var temp = this.scrollContDiv.scrollLeft + thisMove;
        if (thisMove > 0) {
            if (this.scrollContDiv.scrollLeft + thisMove >= this.listDiv01.scrollWidth) {
                this.scrollContDiv.scrollLeft = this.scrollContDiv.scrollLeft + thisMove - this.listDiv01.scrollWidth
            } else {
                this.scrollContDiv.scrollLeft += thisMove
            }
        } else {
            if (this.scrollContDiv.scrollLeft - thisMove <= 0) {
                this.scrollContDiv.scrollLeft = this.listDiv01.scrollWidth + this.scrollContDiv.scrollLeft - thisMove
            } else {
                this.scrollContDiv.scrollLeft += thisMove
            }
        };
        num -= thisMove;
        if (Math.abs(num) == 0) {
            _state = "ready";
            if (this.autoPlay) {
                this.play()
            };
            this.accountPageIndex();
            return
        } else {
            this.accountPageIndex();
            setTimeout("ScrollPic.childs[" + this.ID + "].move(" + num + "," + quick + ")", this.speed)
        }
    };
    this.next = function() {
        if (_state != "ready") {
            return
        };
        _state = "stoping";
        this.move(this.pageWidth, true)
    };
    this.play = function() {
        if (!this.autoPlay) {
            return
        };
        clearInterval(_autoTimeObj);
        _autoTimeObj = setInterval("ScrollPic.childs[" + this.ID + "].next()", this.autoPlayTime * 1000)
    };
    this.stop = function() {
        clearInterval(_autoTimeObj)
    };
    this.pageTo = function(num) {
        if (_state != "ready") {
            return
        };
        _state = "stoping";
        var fill = num * this.frameWidth - this.scrollContDiv.scrollLeft;
        this.move(fill, true)
    };
    this.accountPageIndex = function() {
        this.pageIndex = Math.round(this.scrollContDiv.scrollLeft / this.frameWidth);
        if (this.pageIndex > Math.round(this.listDiv01.offsetWidth / this.frameWidth + 0.4) - 1) {
            this.pageIndex = 0
        };
        var i;
        for (i = 0; i < this.dotObjArr.length; i++) {
            if (i == this.pageIndex) {
                this.dotObjArr[i].className = this.dotClassName
            } else {
                this.dotObjArr[i].className = this.dotOnClassName
            }
        }
    }
};
/*end*/

