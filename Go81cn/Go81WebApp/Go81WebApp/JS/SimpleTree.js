/*
Author���źƻ�
Date��2011.11.27 0:33
Version��SimpleTree 1.1
*/

$(function(){
    $.fn.extend({
        SimpleTree:function(options){

            //��ʼ������
            var option = $.extend({
                click:function(a){ }
            },options);
			
            option.tree=this;	/* �ڲ�����������ӶԵ�ǰ�˵��������ã��Ա��ڶ�����ʹ�øò˵��� */
			
            option._init=function(){
                /*
				 * ��ʼ���˵�չ��״̬���Լ��ֲ�ڵ����ʽ
				 */				
                this.tree.find("ul ul").hide();	/* ���������Ӽ��˵� */
                this.tree.find("ul ul").prev("li").removeClass("open");	/* �Ƴ������Ӽ��˵����ڵ�� open ��ʽ */
                this.tree.find("ul ul[show='false']").prev("li").children("span").css({"background": "url('/Images/tree_icons.png') -99px -4px" });     ///?
				
				
                //this.tree.find("ul ul[show='true']").show();	/* ��ʾ show ����Ϊ true ���Ӽ��˵� */
                //this.tree.find("ul ul[show='true']").prev("li").addClass("open");	/* ��� show ����Ϊ true ���Ӽ��˵����ڵ�� open ��ʽ */
                //this.tree.find("ul ul[show='true']").prev("li").children("span").html("-");
            }/* option._init() End */
			
            /* �������г����Ӳ���Ӧ�����¼� */
            //this.find("a").click(function () {
            //    if ($(this).attr("href") == "#") {
            //        $(this).parents("li").click();
            //        return false;
            //    }
            //});
			
            /* �˵��� <li> ���ܵ��� */
            this.find("li").click(function (event) {
                /*
				 * �������˵��� <li>
				 * 1.�����û��Զ���ĵ����¼������� <li> ��ǩ�еĵ�һ����������Ϊ�������ݹ�ȥ
				 * 2.�޸ĵ�ǰ�˵����������Ӳ˵�����ʾ״̬��������� true ��������Ϊ false������������Ϊ true��
				 * 3.���³�ʼ���˵�
				 */
                var a = $(this).find("a")[0];
                if(typeof(a)!="undefined")
                    option.click(a);	/* �����ȡ�ĳ����Ӳ��� undefined���򴥷����� */
     
                /* 
				 * �����ǰ�ڵ���������Ӳ˵��������� show ���Ե�ֵΪ true�����޸��� show ����Ϊ false
				 * �����޸��� show ����Ϊ true
				 */

                var ntie = $(this).next("ul").attr("show") || $(this).children("ul:first").attr("show");

                if (ntie == "true") {
                    //�ִ������
                    $(this).next("ul").attr("show", "false");
                    $(this).next("ul ul").slideUp(200);	/* ���������Ӽ��˵� */
                    $(this).next("ul ul").prev("li").removeClass("open");	/*�Ƴ������Ӽ��˵����ڵ�� open ��ʽ */
                    $(this).next("ul ul[show='false']").prev("li").children("span").css({ "background": "url('/Images/tree_icons.png') -99px -4px" });//?
                  
                    //IE�Ͱ汾�����
                    $(this).children("ul").attr("show", "false");
                    $(this).children("ul ul").slideUp(200);	/* ���������Ӽ��˵� */
                    $(this).children("ul").parent("li").removeClass("open");	/*�Ƴ������Ӽ��˵����ڵ�� open ��ʽ */
                    $(this).children("ul ul[show='false']").parent("li").children("span").css({ "background": "url('/Images/tree_icons.png') -99px -4px" });//?
                } else {
                    //IE�Ͱ汾�����
                    $(this).children("ul").attr("show", "true");
                    $(this).children("ul[show='true']").slideDown(200);	/* ��ʾshow ����Ϊ true ���Ӽ��˵� */
                    $(this).addClass("open");
                    /* ��� show ����Ϊ true ���Ӽ��˵����ڵ�� open ��ʽ */
                    $(this).children("ul[show='true']").parent().children("span").css({ "background": "url('/Images/tree_icons.png') -115px -4px" }); //?

                    //�ִ������
                    $(this).next("ul").attr("show", "true");
                    $(this).next("ul[show='true']").slideDown(200);	/* ��ʾshow ����Ϊ true ���Ӽ��˵� */
                    $(this).next("ul[show='true']").prev("li").addClass("open");
                    /* ��� show ����Ϊ true ���Ӽ��˵����ڵ�� open ��ʽ */
                    $(this).next("ul[show='true']").prev("li").children("span").css({ "background": "url('/Images/tree_icons.png') -115px -4px" }); //?
                  
                }
                
                //��ֹ�¼�ð��
                if (event.stopPropagation) {
                    event.stopPropagation();   
                } else {
                    event.cancelBubble = true; 
                }
                /* ��ʼ���˵� */
                //option._init();
            });
			
            this.find("li").hover(
				function(){
				    $(this).addClass("hover");
				},
				function(){
				    $(this).removeClass("hover");
				}
			);
			
            /* �������и��ڵ���ʽ */
            this.find("ul").prev("li").addClass("folder");
			
            /* ���ýڵ㡰�Ƿ�����ӽڵ㡱���� */
            this.find("li").find("a").attr("hasChild",false);
            this.find("ul").prev("li").find("a").attr("hasChild",true);
			
            /* ��ʼ���˵� */
            option._init();
			
        }/* SimpleTree Function End */
		
    });
});