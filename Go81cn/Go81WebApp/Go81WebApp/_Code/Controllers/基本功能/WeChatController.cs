using System.Data;
using System.Data.Metadata.Edm;
using Go81WebApp.Models.管理器.统计管理;
using Go81WebApp._Code.Models.Helpers;
using Go81WebApp.Models.管理器;
using Go81WebApp.Models.管理器.微信管理;
using Go81WebApp.Models.数据模型.微信数据模型;
using Go81WebApp.Models.数据模型.项目数据模型;
using Go81WebApp.Models.数据模型.用户数据模型;
using MailApiPostClass;
using MongoDB;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using System.Xml;
using System.Linq;
using Go81WebApp.Models.数据模型;
using Helpers.印章;
using System.Drawing;
using System.Drawing.Imaging;
using Gma.QrCodeNet.Encoding.Windows.Render;
using Gma.QrCodeNet.Encoding;
using Helpers;
using Go81WebApp.Models.数据模型.商品数据模型;
using Go81WebApp._Code.Models.数据模型.微信数据模型;
using Go81WebApp.Models.管理器.推广业务管理;
using Go81WebApp.Models.数据模型.推广业务数据模型;


namespace Go81WebApp.Controllers.基本功能
{
    public class WeChatController : Controller
    {
        const string Token = "sd87512382";//您的token 
        public static string AppID = ConfigurationManager.AppSettings["微信AppID"];
        public static string AppSecret = ConfigurationManager.AppSettings["微信AppSecret"];





        /// <summary>
        /// 验证签名，成为开发者
        /// </summary>
        public void Index()
        {
            //获取AccessToken
            string access_token = GetAccessToken.AccessToken;
            //绑定菜单
            string i = GetPage("https://api.weixin.qq.com/cgi-bin/menu/create?access_token=" + access_token, getMenuJson());

            string postStr = "";
            //若为POST方式，则为微信用户发过来的消息对消息进行处理
            if (Request.HttpMethod.ToLower() == "post")
            {
                Stream s = System.Web.HttpContext.Current.Request.InputStream;
                byte[] b = new byte[s.Length];
                s.Read(b, 0, (int)s.Length);
                //接收到的消息，为XML格式
                postStr = Encoding.UTF8.GetString(b);
                WriteTxt("接收消息：" + postStr + "\r\n\r\n");
                if (!string.IsNullOrEmpty(postStr))
                {
                    try
                    {
                        //封装请求类
                        XmlDocument doc = new XmlDocument();
                        doc.LoadXml(postStr);
                        XmlElement rootElement = doc.DocumentElement;

                        XmlNode MsgType = rootElement.SelectSingleNode("MsgType");

                        微信消息 requestXML = new 微信消息();
                        requestXML.ToUserName = rootElement.SelectSingleNode("ToUserName").InnerText;
                        requestXML.FromUserName = rootElement.SelectSingleNode("FromUserName").InnerText;

                        Session["weChatOpenId"] = requestXML.FromUserName;

                        requestXML.CreateTime = rootElement.SelectSingleNode("CreateTime").InnerText;
                        requestXML.MsgType = MsgType.InnerText;

                        //文本消息
                        if (requestXML.MsgType == "text")
                        {
                            requestXML.Content = rootElement.SelectSingleNode("Content").InnerText;
                            requestXML.MsgId = rootElement.SelectSingleNode("MsgId").InnerText;
                        }
                        else if (requestXML.MsgType == "location")
                        {
                            requestXML.Location_X = rootElement.SelectSingleNode("Location_X").InnerText;
                            requestXML.Location_Y = rootElement.SelectSingleNode("Location_Y").InnerText;
                            requestXML.Scale = rootElement.SelectSingleNode("Scale").InnerText;
                            requestXML.Label = rootElement.SelectSingleNode("Label").InnerText;
                        }
                        else if (requestXML.MsgType == "image")
                        {
                            requestXML.PicUrl = rootElement.SelectSingleNode("PicUrl").InnerText;
                        }
                        else if (requestXML.MsgType == "event")
                        {
                            requestXML.Wxevent = rootElement.SelectSingleNode("Event").InnerText;
                            requestXML.EventKey = rootElement.SelectSingleNode("EventKey").InnerText;
                        }
                       // WriteTxt("----------粉丝发送过来的消息，消息类型：" + requestXML.MsgType + "----------：" + postStr);
                        //回复消息
                        ResponseMsg(requestXML);
                    }
                    catch
                    {
                        Response.Write("");
                        WriteTxt("已报错------------\r\n");
                    }
                }
                else
                {
                    Response.Write("");
                    WriteTxt("消息为空\r\n");
                }
            }
            else
            {
                WriteTxt("Get方式\r\n");
                Valid();
            }
        }
        public ActionResult WeChatBindUser()
        {
            ViewData["已绑定"] = "0";
            var BindEdStr = "";
            //判断该用户是否已绑定
            var openid = Request.QueryString["openid"];
            var user = 微信管理.查询微信用户(0, 0, Query<微信用户>.EQ(o => o.openId, openid));
            if (user.Count() > 0)
            {
                var userid = user.First().用户链接.用户ID;
                var userinfo = 用户管理.查找用户(userid);

                if (userinfo != null)
                {
                    if (typeof(供应商) == userinfo.GetType())
                    {
                        BindEdStr = "您已绑定过用户。<br />您绑定的用户为：" + (userinfo as 供应商).企业基本信息.企业名称 + "。<br />如绑定有误或取消绑定，请联系网站客服！";
                    }
                    else if (typeof(单位用户) == userinfo.GetType())
                    {
                        BindEdStr = "您已绑定过用户。<br />您绑定的用户为：" + (userinfo as 单位用户).单位信息.单位名称 + "。<br />如绑定有误或取消绑定，请联系网站客服！";
                    }
                }
                else
                {
                    BindEdStr = "您已绑定过用户。<br />您绑定的用户未审核通过或已删除。<br />如绑定有误或取消绑定，请联系网站客服！";
                }

                ViewData["已绑定"] = "1";
            }
            ViewData["绑定信息"] = BindEdStr;
            ViewData["weChatUserOpenId"] = openid;
            return PartialView("WeChatBindUser");
        }
        /// <summary>  
        /// 验证微信签名  
        /// </summary>  
        /// * 将token、timestamp、nonce三个参数进行字典序排序  
        /// * 将三个参数字符串拼接成一个字符串进行sha1加密  
        /// * 开发者获得加密后的字符串可与signature对比，标识该请求来源于微信。  
        /// <returns></returns>  
        private bool CheckSignature()
        {
            string signature = Request.QueryString["signature"].ToString();
            string timestamp = Request.QueryString["timestamp"].ToString();
            string nonce = Request.QueryString["nonce"].ToString();
            string[] ArrTmp = { Token, timestamp, nonce };
            Array.Sort(ArrTmp);     //字典排序  
            string tmpStr = string.Join("", ArrTmp);
            tmpStr = FormsAuthentication.HashPasswordForStoringInConfigFile(tmpStr, "SHA1");
            tmpStr = tmpStr.ToLower();
            if (tmpStr == signature)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private void Valid()
        {
            string echoStr = Request.QueryString["echoStr"].ToString();
            if (CheckSignature())
            {
                if (!string.IsNullOrEmpty(echoStr))
                {
                    Response.Write(echoStr);
                    Response.End();
                }
            }
        }

        /// <summary>
        /// 回复消息(微信信息返回)
        /// </summary>
        /// <param name="weixinXML"></param>
        private void ResponseMsg(微信消息 requestXML)
        {
            //string resxml = "<xml><ToUserName><![CDATA[" + requestXML.FromUserName + "]]></ToUserName><FromUserName><![CDATA[" + requestXML.ToUserName + "]]></FromUserName><CreateTime>" + ConvertDateTimeInt(DateTime.Now) + "</CreateTime>";
            //try
            //{
            //    if (requestXML.MsgType == "text")
            //    {
            //        int count = 0;
            //        if (requestXML.Content.Trim() == "单图")
            //        {
            //            count = 1;
            //            resxml += "<MsgType><![CDATA[news]]></MsgType><ArticleCount>" + count + "</ArticleCount><Articles>";
            //            resxml += "<item><Title><![CDATA[这里是标题]]></Title><Description><![CDATA[这里是描述，当是单图文的时候描述才会展示]]></Description><PicUrl><![CDATA[http://imgsrc.baidu.com/product_name/pic/item/32374836acaf2edd470a31508d1001e93801934a.jpg]]></PicUrl><Url><![CDATA[http://www.baidu.com]]></Url></item>";//URL是点击之后跳转去那里，这里跳转到百度
            //            resxml += "</Articles><FuncFlag>0</FuncFlag></xml>";
            //        }
            //        else if (requestXML.Content.Trim() == "多图")
            //        {
            //            count = 10;
            //            resxml += "<MsgType><![CDATA[news]]></MsgType><ArticleCount>" + count + "</ArticleCount><Articles>";
            //            for (int i = 0; i < count; i++)
            //            {
            //                resxml += "<item><Title><![CDATA[这里是标题" + (i + 1) + "]]></Title><Description><![CDATA[这里是描述，当是单图文的时候描述才会展示]]></Description><PicUrl><![CDATA[http://imgsrc.baidu.com/product_name/pic/item/32374836acaf2edd470a31508d1001e93801934a.jpg]]></PicUrl><Url><![CDATA[http://www.baidu.com]]></Url></item>";//URL是点击之后跳转去那里，这里跳转到百度
            //            }
            //            resxml += "</Articles><FuncFlag>0</FuncFlag></xml>";
            //        }
            //        else
            //        {
            //            resxml += "<MsgType><![CDATA[text]]></MsgType><Content><![CDATA[您发过来的是文字类型消息\n哈哈]]></Content><FuncFlag>0</FuncFlag></xml>";
            //        }
            //    }
            //    else if (requestXML.MsgType == "location")
            //    {
            //        resxml += "<MsgType><![CDATA[text]]></MsgType><Content><![CDATA[您发过来的是地理消息\n哈哈]]></Content><FuncFlag>0</FuncFlag></xml>";
            //    }
            //    else if (requestXML.MsgType == "image") 
            //    {
            //        resxml += "<MsgType><![CDATA[text]]></MsgType><Content><![CDATA[您发一张图片过来我怎么识别啊，您以为我有眼睛啊\n哈哈]]></Content><FuncFlag>0</FuncFlag></xml>";
            //    }
            //    else if (requestXML.MsgType == "event")
            //    {
            //        if (requestXML.Wxevent == "unsubscribe")
            //        {
            //            //取消关注

            //        }
            //        else
            //        {
            //            //新增关注
            //            resxml += "<MsgType><![CDATA[text]]></MsgType><Content><![CDATA[谢谢您关注我。]]></Content><FuncFlag>0</FuncFlag></xml>";
            //        }
            //    }
            //}
            //catch (Exception ex)
            //{
            //    WriteTxt("异常：" + ex.Message);
            //}
            //WriteTxt("返回给粉丝的消息：" + resxml);
            //Response.Write(resxml);
            //Response.End();
            try
            {
                string resxml = "";
                if (requestXML.MsgType == "text")
                {
                    //var wechatuser = 优惠码管理.计数优惠码(0, 0, Query<优惠码>.Where(o => o.WeChatUser == requestXML.FromUserName));
                    //if (wechatuser > 0)
                    //{
                    //    resxml = "<xml><ToUserName><![CDATA[" + requestXML.FromUserName +
                    //                        "]]></ToUserName><FromUserName><![CDATA[" + requestXML.ToUserName +
                    //                        "]]></FromUserName><CreateTime>" + ConvertDateTimeInt(DateTime.Now) +
                    //                        "</CreateTime><MsgType><![CDATA[text]]></MsgType><Content><![CDATA[您已经获取过优惠码，不能再次获取！]]></Content><FuncFlag>0</FuncFlag></xml>";
                    //}
                    //else
                    //{
                    //    var discountCode = GenerateDisCountCode("unit");
                    //    resxml = "<xml><ToUserName><![CDATA[" + requestXML.FromUserName +
                    //                        "]]></ToUserName><FromUserName><![CDATA[" + requestXML.ToUserName +
                    //                        "]]></FromUserName><CreateTime>" + ConvertDateTimeInt(DateTime.Now) +
                    //                        "</CreateTime><MsgType><![CDATA[text]]></MsgType><Content><![CDATA[您的优惠码为：" +
                    //                        discountCode + "]]></Content><FuncFlag>0</FuncFlag></xml>";
                    //    var code = new 优惠码();
                    //    code.WeChatUser = requestXML.FromUserName;
                    //    code.Code = discountCode;
                    //    优惠码管理.添加优惠码(code);
                    //}
                    resxml = "<xml><ToUserName><![CDATA[" + requestXML.FromUserName +
                                            "]]></ToUserName><FromUserName><![CDATA[" + requestXML.ToUserName +
                                            "]]></FromUserName><CreateTime>" + ConvertDateTimeInt(DateTime.Now) +
                                            "</CreateTime><MsgType><![CDATA[text]]></MsgType><Content><![CDATA[未找到相关信息！]]></Content><FuncFlag>0</FuncFlag></xml>";
                }
                else if (requestXML.MsgType == "location")
                {
                    string city = GetMapInfo(requestXML.Location_X, requestXML.Location_Y);
                    if (city == "0")
                    {
                        resxml = "<xml><ToUserName><![CDATA[" + requestXML.FromUserName + "]]></ToUserName><FromUserName><![CDATA[" + requestXML.ToUserName + "]]></FromUserName><CreateTime>" + ConvertDateTimeInt(DateTime.Now) + "</CreateTime><MsgType><![CDATA[text]]></MsgType><Content><![CDATA[Sorry，没有找到" + city + " 的相关产品信息]]></Content><FuncFlag>0</FuncFlag></xml>";
                    }
                    else
                    {
                        resxml = "<xml><ToUserName><![CDATA[" + requestXML.FromUserName + "]]></ToUserName><FromUserName><![CDATA[" + requestXML.ToUserName + "]]></FromUserName><CreateTime>" + ConvertDateTimeInt(DateTime.Now) + "</CreateTime><MsgType><![CDATA[text]]></MsgType><Content><![CDATA[Sorry，这是 " + city + " 的产品信息：圈圈叉叉]]></Content><FuncFlag>0</FuncFlag></xml>";
                    }
                }
                else if (requestXML.MsgType == "image")
                {
                    //返回10以内条  
                    int size = 5;
                    resxml = "<xml><ToUserName><![CDATA[" + requestXML.FromUserName + "]]></ToUserName><FromUserName><![CDATA[" + requestXML.ToUserName + "]]></FromUserName><CreateTime>" + ConvertDateTimeInt(DateTime.Now) + "</CreateTime><MsgType><![CDATA[news]]></MsgType><Content><![CDATA[]]></Content><ArticleCount>" + size + "</ArticleCount><Articles>";
                    List<string> list = new List<string>();
                    //假如有20条查询的返回结果  
                    for (int i = 0; i < 20; i++)
                    {
                        list.Add("1");
                    }
                    string[] piclist = new string[] { "/Images/tu.jpg", "/Images/ticket1.jpg", "/Images/ticket2.jpg", "/Images/ticket3.jpg", "/Images/tu.jpg", "/Images/ticket1.jpg", "/Images/ticket2.jpg", "/Images/ticket3.jpg", "/Images/ticket2.jpg", "/Images/ticket3.jpg" };

                    for (int i = 0; i < size && i < list.Count; i++)
                    {
                        WriteTxt(piclist[i]);
                        Response.Write(piclist[i]);
                        resxml += "<item><Title><![CDATA[沈阳-黑龙江]]></Title><Description><![CDATA[元旦特价：￥300 市场价：￥400]]></Description><PicUrl><![CDATA[" + "http://www.go81.cn/" + piclist[i] + "]]></PicUrl><Url><![CDATA[http://www.go81.cn/新闻/NewsDetail?id=64]]></Url></item>";
                    }
                    resxml += "</Articles><FuncFlag>1</FuncFlag></xml>";
                }
                else if (requestXML.MsgType == "event")
                {
                    if (requestXML.Wxevent == "unsubscribe")
                    {
                        //用户取消关注事件，如该用户已经绑定过，则彻底删除该用户的绑定信息
                        var openid = requestXML.FromUserName;
                        var user = 微信管理.查询微信用户(0, 0, Query<微信用户>.EQ(o => o.openId, openid));
                        if (user.Count() > 0)
                        {
                            微信管理.物理删除微信用户(user.First().Id);
                        }
                    }
                    else if (requestXML.Wxevent == "subscribe")
                    {
                        //用户关注事件，发送初始文本
                        resxml = "<xml><ToUserName><![CDATA[" + requestXML.FromUserName + "]]></ToUserName><FromUserName><![CDATA[" + requestXML.ToUserName + "]]></FromUserName><CreateTime>" + ConvertDateTimeInt(DateTime.Now) + "</CreateTime><MsgType><![CDATA[text]]></MsgType><Content><![CDATA[感谢关注西南物资采购网。本公众平台主要功能包括成都军区物资采购信息发布、供应商与评审专家入库申请、数据搜集、资格预审、在库管理、协议供应商商品公示、应急供应商统一协调管理、服务型采购信息提供等。]]></Content><FuncFlag>0</FuncFlag></xml>";
                    }
                    else if (requestXML.Wxevent.ToLower() == "click")
                    {
                        //检查用户
                        var openid = requestXML.FromUserName;
                        var user = 微信管理.查询微信用户(0, 0, Query<微信用户>.EQ(o => o.openId, openid));
                        用户基本数据 userinfo = null;
                        Type usertype = null;
                        if (user.Count() > 0)
                        {
                            var userid = user.First().用户链接.用户ID;
                            userinfo = 用户管理.查找用户(userid);
                            if (userinfo != null)
                            {
                                usertype = userinfo.GetType();
                            }
                        }
                        //检查用户

                        switch (requestXML.EventKey)
                        {
                            //点击验收单事件
                            #region 点击验收单事件
                            case "DoWechatSearchYsd":
                                try
                                {
                                    //var user = 微信管理.查询微信用户(0, 0, Query<微信用户>.EQ(o => o.openId, openid));
                                    if (user.Count() > 0)
                                    {
                                        //var userid = user.First().用户链接.用户ID;
                                        //var userinfo = 用户管理.查找用户(userid);
                                        if (userinfo != null)
                                        {
                                            //var usertype = userinfo.GetType();
                                            if (typeof(供应商) == usertype)
                                            {
                                                //验收单总和
                                                var ysd_count = 验收单管理.计数验收单(0, 0, Query<验收单>.EQ(o => o.供应商链接.用户ID, userinfo.Id));
                                                //已作废验收单数
                                                var ysd_count_e = 验收单管理.计数验收单(0, 0, Query<验收单>.EQ(o => o.供应商链接.用户ID, userinfo.Id).And(Query<验收单>.EQ(o => o.是否作废, true)));
                                                //审核通过验收单数
                                                var ysd_count_y = 验收单管理.计数验收单(0, 0, Query<验收单>.Where(o => o.供应商链接.用户ID == userinfo.Id && o.是否作废 == false && o.审核数据.审核状态 == 审核状态.审核通过));
                                                //审核未通过验收单数
                                                var ysd_count_n = 验收单管理.计数验收单(0, 0, Query<验收单>.Where(o => o.供应商链接.用户ID == userinfo.Id && o.是否作废 == false && o.审核数据.审核状态 == 审核状态.审核未通过));
                                                //未审核验收单数
                                                var ysd_count_d = 验收单管理.计数验收单(0, 0, Query<验收单>.Where(o => o.供应商链接.用户ID == userinfo.Id && o.是否作废 == false && o.审核数据.审核状态 == 审核状态.未审核));

                                                //绑定页面，传入openid
                                                resxml = "<xml><ToUserName><![CDATA[" + openid + "]]></ToUserName><FromUserName><![CDATA[" + requestXML.ToUserName + "]]></FromUserName><CreateTime>" + ConvertDateTimeInt(DateTime.Now) + "</CreateTime><MsgType><![CDATA[news]]></MsgType><Content><![CDATA[]]></Content><ArticleCount>1</ArticleCount><Articles>";

                                                resxml += "<item><Title><![CDATA[验收单信息]]></Title><Description><![CDATA[供应商您好，关于您的验收单信息如下：\n提交验收单总数：" + ysd_count + "\n待审核验收单数：" + ysd_count_d + "\n审核通过验收单数：" + ysd_count_y + "\n审核未通过验收单数：" + ysd_count_n + "\n已作废验收单数：" + ysd_count_e + "\n\n点击此消息查看详情。]]></Description><PicUrl><![CDATA[]]></PicUrl><Url><![CDATA[http://www.go81.cn/wechat/GysYsdList?openid=" + openid + "]]></Url></item>";
                                                resxml += "</Articles><FuncFlag>1</FuncFlag></xml>";
                                            }
                                            else if (typeof(单位用户) == usertype)
                                            {
                                                //验收单总和
                                                var ysd_count = 验收单管理.计数验收单(0, 0, Query<验收单>.EQ(o => o.管理单位审核人.用户ID, userinfo.Id));
                                                //已作废验收单数
                                                var ysd_count_e = 验收单管理.计数验收单(0, 0, Query<验收单>.EQ(o => o.管理单位审核人.用户ID, userinfo.Id).And(Query<验收单>.EQ(o => o.是否作废, true)));
                                                //有效验收单数
                                                var ysd_count_o = ysd_count - ysd_count_e;
                                                //审核通过验收单数
                                                var ysd_count_y = 验收单管理.计数验收单(0, 0, Query<验收单>.Where(o => o.管理单位审核人.用户ID == userinfo.Id && o.是否作废 == false && o.审核数据.审核状态 == 审核状态.审核通过));
                                                //审核未通过验收单数
                                                var ysd_count_n = 验收单管理.计数验收单(0, 0, Query<验收单>.Where(o => o.管理单位审核人.用户ID == userinfo.Id && o.是否作废 == false && o.审核数据.审核状态 == 审核状态.审核未通过));
                                                //未审核验收单数
                                                var ysd_count_d = 验收单管理.计数验收单(0, 0, Query<验收单>.Where(o => o.管理单位审核人.用户ID == userinfo.Id && o.是否作废 == false && o.审核数据.审核状态 == 审核状态.未审核));

                                                //绑定页面，传入openid
                                                resxml = "<xml><ToUserName><![CDATA[" + openid + "]]></ToUserName><FromUserName><![CDATA[" + requestXML.ToUserName + "]]></FromUserName><CreateTime>" + ConvertDateTimeInt(DateTime.Now) + "</CreateTime><MsgType><![CDATA[news]]></MsgType><Content><![CDATA[]]></Content><ArticleCount>1</ArticleCount><Articles>";

                                                resxml += "<item><Title><![CDATA[验收单信息]]></Title><Description><![CDATA[单位用户您好，关于您的验收单信息如下：\n提交验收单总数：" + ysd_count + "\n有效验收单数：" + ysd_count_o + "\n待审核验收单数：" + ysd_count_d + "\n审核通过验收单数：" + ysd_count_y + "\n审核未通过验收单数：" + ysd_count_n + "\n已作废验收单数：" + ysd_count_e + "\n\n点击此消息查看详情。]]></Description><PicUrl><![CDATA[]]></PicUrl><Url><![CDATA[http://www.go81.cn/wechat/UnitYsdList?openid=" + openid + "]]></Url></item>";
                                                resxml += "</Articles><FuncFlag>1</FuncFlag></xml>";
                                            }
                                            else
                                            {
                                                //绑定页面，传入openid
                                                resxml = "<xml><ToUserName><![CDATA[" + openid + "]]></ToUserName><FromUserName><![CDATA[" + requestXML.ToUserName + "]]></FromUserName><CreateTime>" + ConvertDateTimeInt(DateTime.Now) + "</CreateTime><MsgType><![CDATA[news]]></MsgType><Content><![CDATA[]]></Content><ArticleCount>1</ArticleCount><Articles>";
                                                resxml += "<item><Title><![CDATA[验收单信息]]></Title><Description><![CDATA[未找到关于您的验收单信息！]]></Description><PicUrl><![CDATA[]]></PicUrl><Url><![CDATA[]]></Url></item>";
                                                resxml += "</Articles><FuncFlag>1</FuncFlag></xml>";
                                            }
                                        }
                                        else
                                        {
                                            //绑定页面，传入openid
                                            resxml = "<xml><ToUserName><![CDATA[" + openid + "]]></ToUserName><FromUserName><![CDATA[" + requestXML.ToUserName + "]]></FromUserName><CreateTime>" + ConvertDateTimeInt(DateTime.Now) + "</CreateTime><MsgType><![CDATA[news]]></MsgType><Content><![CDATA[]]></Content><ArticleCount>1</ArticleCount><Articles>";
                                            resxml += "<item><Title><![CDATA[验收单信息]]></Title><Description><![CDATA[未查找到您绑定的用户信息，您绑定的用户可能处于未审核状态，如有疑问，请联系客服！]]></Description><PicUrl><![CDATA[]]></PicUrl><Url><![CDATA[]]></Url></item>";
                                            resxml += "</Articles><FuncFlag>1</FuncFlag></xml>";
                                        }
                                    }
                                    else
                                    {
                                        //绑定页面，传入openid
                                        resxml = "<xml><ToUserName><![CDATA[" + openid + "]]></ToUserName><FromUserName><![CDATA[" + requestXML.ToUserName + "]]></FromUserName><CreateTime>" + ConvertDateTimeInt(DateTime.Now) + "</CreateTime><MsgType><![CDATA[news]]></MsgType><Content><![CDATA[]]></Content><ArticleCount>1</ArticleCount><Articles>";
                                        resxml += "<item><Title><![CDATA[验收单信息]]></Title><Description><![CDATA[您还未绑定用户，请点击菜单-->绑定用户进行绑定\n或点击此信息进行用户绑定。]]></Description><PicUrl><![CDATA[]]></PicUrl><Url><![CDATA[http://www.go81.cn/wechat/WeChatBindUser?openid=" + openid + "]]></Url></item>";
                                        resxml += "</Articles><FuncFlag>1</FuncFlag></xml>";
                                    }
                                }
                                catch
                                {
                                    //绑定页面，传入openid
                                    resxml = "<xml><ToUserName><![CDATA[" + openid + "]]></ToUserName><FromUserName><![CDATA[" + requestXML.ToUserName + "]]></FromUserName><CreateTime>" + ConvertDateTimeInt(DateTime.Now) + "</CreateTime><MsgType><![CDATA[news]]></MsgType><Content><![CDATA[]]></Content><ArticleCount>1</ArticleCount><Articles>";
                                    resxml += "<item><Title><![CDATA[验收单信息]]></Title><Description><![CDATA[未找到关于您的验收单。]]></Description><PicUrl><![CDATA[]]></PicUrl><Url><![CDATA[]]></Url></item>";
                                    resxml += "</Articles><FuncFlag>1</FuncFlag></xml>";
                                }
                                #endregion
                                break;
                            #region 用户绑定
                            case "doBindUserInfo":
                                //绑定用户，先判断该用户是否已绑定
                                if (user.Count() > 0)
                                {
                                    //该用户已经绑定，显示该用户的绑定信息，不载入绑定页面
                                    var BindEdStr = "";
                                    if (userinfo != null)
                                    {
                                        if (typeof(供应商) == usertype)
                                        {
                                            BindEdStr = "您绑定的用户为：" + (userinfo as 供应商).企业基本信息.企业名称 + "。\n如绑定有误或取消绑定，请联系网站客服！";
                                        }
                                        else if (typeof(单位用户) == usertype)
                                        {
                                            BindEdStr = "您绑定的用户为：" + (userinfo as 单位用户).单位信息.单位名称 + "。\n如绑定有误或取消绑定，请联系网站客服！";
                                        }
                                    }
                                    else
                                    {
                                        BindEdStr = "您绑定的用户未审核通过或已删除。\n如绑定有误或取消绑定，请联系网站客服！";
                                    }
                                    //显示提示信息
                                    resxml = "<xml><ToUserName><![CDATA[" + openid + "]]></ToUserName><FromUserName><![CDATA[" + requestXML.ToUserName + "]]></FromUserName><CreateTime>" + ConvertDateTimeInt(DateTime.Now) + "</CreateTime><MsgType><![CDATA[news]]></MsgType><Content><![CDATA[]]></Content><ArticleCount>1</ArticleCount><Articles>";
                                    resxml += "<item><Title><![CDATA[您的用户已经绑定过]]></Title><Description><![CDATA[" + BindEdStr + "]]></Description><PicUrl><![CDATA[]]></PicUrl><Url><![CDATA[]]></Url></item>";
                                    resxml += "</Articles><FuncFlag>1</FuncFlag></xml>";
                                }
                                else
                                {
                                    //该用户未绑定，则载入绑定页面，传入openid
                                    resxml = "<xml><ToUserName><![CDATA[" + openid + "]]></ToUserName><FromUserName><![CDATA[" + requestXML.ToUserName + "]]></FromUserName><CreateTime>" + ConvertDateTimeInt(DateTime.Now) + "</CreateTime><MsgType><![CDATA[news]]></MsgType><Content><![CDATA[]]></Content><ArticleCount>1</ArticleCount><Articles>";
                                    resxml += "<item><Title><![CDATA[点击此信息进行用户绑定]]></Title><Description><![CDATA[点击这里进行用户绑定]]></Description><PicUrl><![CDATA[]]></PicUrl><Url><![CDATA[http://www.go81.cn/wechat/WeChatBindUser?openid=" + openid + "]]></Url></item>";
                                    resxml += "</Articles><FuncFlag>1</FuncFlag></xml>";
                                }
                                 #endregion
                                break;
                           
                               #region 优惠券获取
                            case "get":
                                var wechatuser = 优惠码管理.计数优惠码(0, 0, Query<优惠码>.Where(o => o.WeChatUser == requestXML.FromUserName));
                                if (wechatuser > 0)
                                {
                                    resxml = "<xml><ToUserName><![CDATA[" + requestXML.FromUserName +
                                             "]]></ToUserName><FromUserName><![CDATA[" + requestXML.ToUserName +
                                             "]]></FromUserName><CreateTime>" + ConvertDateTimeInt(DateTime.Now) +
                                             "</CreateTime><MsgType><![CDATA[text]]></MsgType><Content><![CDATA[您已经获取过优惠码，不能再次获取！]]></Content><FuncFlag>0</FuncFlag></xml>";
                                }
                                else
                                {
                                    var discountcode = GenerateDisCountCode("gys");
                                    resxml = "<xml><ToUserName><![CDATA[" + requestXML.FromUserName +
                                             "]]></ToUserName><FromUserName><![CDATA[" + requestXML.ToUserName +
                                             "]]></FromUserName><CreateTime>" + ConvertDateTimeInt(DateTime.Now) +
                                             "</CreateTime><MsgType><![CDATA[text]]></MsgType><Content><![CDATA[您的优惠码为：" +
                                             discountcode + "]]></Content><FuncFlag>0</FuncFlag></xml>";
                                    var code = new 优惠码();
                                    code.WeChatUser = requestXML.FromUserName;
                                    code.Code = discountcode;
                                    code.已使用 = false;
                                    优惠码管理.添加优惠码(code);
                                }
                                
                                break;
                            //case "unit":
                            //    resxml = "<xml><ToUserName><![CDATA[" + requestXML.FromUserName +
                            //             "]]></ToUserName><FromUserName><![CDATA[" + requestXML.ToUserName +
                            //             "]]></FromUserName><CreateTime>" + ConvertDateTimeInt(DateTime.Now) +
                            //             "</CreateTime><MsgType><![CDATA[text]]></MsgType><Content><![CDATA[请输入您的官兵用户信息，格式为：gbyh:+（单位拼音缩写或番号）。    如：gbyh：70000或gbyh：xnwzcgw。请点击左下角键盘符号返回输入界面进行输入。]]></Content><FuncFlag>0</FuncFlag></xml>";
                            //    break;
                            case "my":
                                var mycode = 优惠码管理.查询优惠码(0, 0,
                                    Query<优惠码>.Where(o => o.WeChatUser == requestXML.FromUserName));
                                if (mycode.Any())
                                {
                                    resxml = "<xml><ToUserName><![CDATA[" + requestXML.FromUserName +
                                             "]]></ToUserName><FromUserName><![CDATA[" + requestXML.ToUserName +
                                             "]]></FromUserName><CreateTime>" + ConvertDateTimeInt(DateTime.Now) +
                                             "</CreateTime><MsgType><![CDATA[text]]></MsgType><Content><![CDATA[您的优惠码为：" +
                                             mycode.First().Code + "]]></Content><FuncFlag>0</FuncFlag></xml>";
                                }
                                else
                                {
                                    resxml = "<xml><ToUserName><![CDATA[" + requestXML.FromUserName +
                                             "]]></ToUserName><FromUserName><![CDATA[" + requestXML.ToUserName +
                                             "]]></FromUserName><CreateTime>" + ConvertDateTimeInt(DateTime.Now) +
                                             "</CreateTime><MsgType><![CDATA[text]]></MsgType><Content><![CDATA[你尚未获取优惠码！]]></Content><FuncFlag>0</FuncFlag></xml>";
                                }
                                    
                                break;
                               #endregion
                            default:
                                resxml = "<xml><ToUserName><![CDATA[" + openid +
                                         "]]></ToUserName><FromUserName><![CDATA[" + requestXML.ToUserName +
                                         "]]></FromUserName><CreateTime>" + ConvertDateTimeInt(DateTime.Now) +
                                         "</CreateTime><MsgType><![CDATA[text]]></MsgType><Content><![CDATA[点击菜单事件]]></Content><FuncFlag>0</FuncFlag></xml>";
                                break;
                        }
                    }
                }
                WriteTxt(resxml);
                Response.Write(resxml);
            }
            catch (Exception ex)
            {
                WriteTxt("异常：" + ex.Message + "Struck:" + ex.StackTrace.ToString());
            }
        }



        /// <summary>
        /// unix时间转换为datetime
        /// </summary>
        /// <param name="timeStamp"></param>
        /// <returns></returns>
        private DateTime UnixTimeToTime(string timeStamp)
        {
            DateTime dtStart = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1));
            long lTime = long.Parse(timeStamp + "0000000");
            TimeSpan toNow = new TimeSpan(lTime);
            return dtStart.Add(toNow);
        }

        /// <summary>
        /// datetime转换为unixtime
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        private int ConvertDateTimeInt(System.DateTime time)
        {
            System.DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1));
            return (int)(time - startTime).TotalSeconds;
        }

        /// <summary>
        /// 记录bug，以便调试
        /// </summary>
        /// 
        /// <returns></returns>
        public bool WriteTxt(string str)
        {
            try
            {
                FileStream fs = new FileStream(Server.MapPath("/bugLog.txt"), FileMode.Append);
                StreamWriter sw = new StreamWriter(fs);
                //开始写入
                sw.WriteLine(str);
                //清空缓冲区
                sw.Flush();
                //关闭流
                sw.Close();
                fs.Close();
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }
        /// <summary>  
        /// 调用百度地图，返回坐标信息  
        /// </summary>  
        /// <param name="y">经度</param>  
        /// <param name="x">纬度</param>  
        /// <returns></returns>  
        public string GetMapInfo(string x, string y)
        {
            try
            {
                string res = string.Empty;
                string parame = string.Empty;
                string url = "http://maps.googleapis.com/maps/api/geocode/xml";
                parame = "latlng=" + x + "," + y + "&language=zh-CN&sensor=false";//此key为个人申请  
                res = webRequestPost(url, parame);

                XmlDocument doc = new XmlDocument();

                doc.LoadXml(res);
                XmlElement rootElement = doc.DocumentElement;
                string Status = rootElement.SelectSingleNode("status").InnerText;
                if (Status == "OK")
                {
                    //仅获取城市  
                    XmlNodeList xmlResults = rootElement.SelectSingleNode("/GeocodeResponse").ChildNodes;
                    for (int i = 0; i < xmlResults.Count; i++)
                    {
                        XmlNode childNode = xmlResults[i];
                        if (childNode.Name == "status")
                        {
                            continue;
                        }

                        string city = "0";
                        for (int w = 0; w < childNode.ChildNodes.Count; w++)
                        {
                            for (int q = 0; q < childNode.ChildNodes[w].ChildNodes.Count; q++)
                            {
                                XmlNode childeTwo = childNode.ChildNodes[w].ChildNodes[q];

                                if (childeTwo.Name == "long_name")
                                {
                                    city = childeTwo.InnerText;
                                }
                                else if (childeTwo.InnerText == "locality")
                                {
                                    return city;
                                }
                            }
                        }
                        return city;
                    }
                }
            }
            catch (Exception ex)
            {
                WriteTxt("map异常:" + ex.Message.ToString() + "Struck:" + ex.StackTrace.ToString());
                return "0";
            }

            return "0";
        }
        /// <summary>  
        /// Post 提交调用抓取  
        /// </summary>  
        /// <param name="url">提交地址</param>  
        /// <param name="param">参数</param>  
        /// <returns>string</returns>  
        public static string webRequestPost(string url, string param)
        {
            byte[] bs = System.Text.Encoding.UTF8.GetBytes(param);

            HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(url + "?" + param);
            req.Method = "Post";
            req.Timeout = 120 * 1000;
            req.ContentType = "application/x-www-form-urlencoded;";
            req.ContentLength = bs.Length;

            using (Stream reqStream = req.GetRequestStream())
            {
                reqStream.Write(bs, 0, bs.Length);
                reqStream.Flush();
            }
            using (WebResponse wr = req.GetResponse())
            {
                //在这里对接收到的页面内容进行处理   

                Stream strm = wr.GetResponseStream();

                StreamReader sr = new StreamReader(strm, System.Text.Encoding.UTF8);

                string line;

                System.Text.StringBuilder sb = new System.Text.StringBuilder();

                while ((line = sr.ReadLine()) != null)
                {
                    sb.Append(line + System.Environment.NewLine);
                }
                sr.Close();
                strm.Close();
                return sb.ToString();
            }
        }

        public string GetPage(string posturl, string postData)
        {
            Stream outstream = null;
            Stream instream = null;
            StreamReader sr = null;
            HttpWebResponse response = null;
            HttpWebRequest request = null;
            Encoding encoding = Encoding.UTF8;
            byte[] data = encoding.GetBytes(postData);
            // 准备请求...
            try
            {
                // 设置参数
                request = WebRequest.Create(posturl) as HttpWebRequest;
                CookieContainer cookieContainer = new CookieContainer();
                request.CookieContainer = cookieContainer;
                request.AllowAutoRedirect = true;
                request.Method = "POST";
                request.ContentType = "application/x-www-form-urlencoded";
                request.ContentLength = data.Length;
                outstream = request.GetRequestStream();
                outstream.Write(data, 0, data.Length);
                outstream.Close();
                //发送请求并获取相应回应数据
                response = request.GetResponse() as HttpWebResponse;
                //直到request.GetResponse()程序才开始向目标网页发送Post请求
                instream = response.GetResponseStream();
                sr = new StreamReader(instream, encoding);
                //返回结果网页（html）代码
                string content = sr.ReadToEnd();
                string err = string.Empty;
                return content;
            }
            catch (Exception ex)
            {
                string err = ex.Message;
                Response.Write(err);
                return string.Empty;
            }
        }


        /// <summary>  
        /// 写日志(用于跟踪)  
        /// </summary>  
        private void WriteLog(string strMemo)
        {
            string filename = Server.MapPath("/logs/log.txt");
            if (!Directory.Exists(Server.MapPath("//logs//")))
                Directory.CreateDirectory("//logs//");
            StreamWriter sr = null;
            try
            {
                if (!System.IO.File.Exists(filename))
                {
                    sr = System.IO.File.CreateText(filename);
                }
                else
                {
                    sr = System.IO.File.AppendText(filename);
                }
                sr.WriteLine(strMemo);
            }
            catch
            {

            }
            finally
            {
                if (sr != null)
                    sr.Close();
            }
        }
        public string getMenuJson()
        {
            string weixinMenu = "";
            weixinMenu = @" {  
     ""button"":[  
     {  
           ""name"":""采购商城"",  
           ""sub_button"":[  
            {  
               ""type"":""view"",  
               ""name"":""供应商"",  
                ""url"":""http://www.go81.cn/wechat/GysList""  
            },  
            {  
               ""type"":""view"",  
               ""name"":""商品"",  
               ""url"":""http://www.go81.cn/wechat/Goodlist""  
            },  
            {  
               ""type"":""view"",  
               ""name"":""热门推荐"",  
               ""url"":""http://go81.cn/wechat/Hot_Recommend""
            }]  
       },  
      {  
           ""name"":""入库申请"",  
           ""sub_button"":[  
            {  
               ""type"":""view"",  
               ""name"":""供应商"",  
                ""url"":""http://go81.cn/wechat/GysApply""  
            },  
            {  
               ""type"":""view"",  
               ""name"":""评审专家"",  
               ""url"":""http://go81.cn/wechat/ExpertApply""  
            }]  
       },  
      {  
            ""name"":""军采通"",  
           ""sub_button"":[  
            {  
               ""type"":""view"",  
               ""name"":""会员中心"",  
                ""url"":""http://go81.cn/jct""  
            },  
            {  
               ""type"":""view"",  
               ""name"":""已订购的服务"",  
               ""url"":""http://go81.cn/wechat/MyService""  
            },  
            {  
               ""type"":""view"",  
               ""name"":""关于我们"",  
               ""url"":""http://go81.cn/wechat/AboutUs""
            },  
            {  
               ""type"":""view"",  
               ""name"":""进入网站"",  
               ""url"":""http://go81.cn""
            }]  
 } ";


           // ""name"":""优惠码"",
           //""sub_button"":[  
           //     {  
           //        ""type"":""click"",  
           //        ""name"":""获取优惠码"",  
           //         ""key"":""get""  
           //     },
           //     {  
           //        ""type"":""click"",  
           //        ""name"":""我的优惠码"",  
           //        ""key"":""my""  
           //     }]  
           //}] 







             








            //            weixinMenu = @" {  
            //     ""button"":[  
            //     {    
            //          ""type"":""click"",  
            //          ""name"":""验收单"",  
            //          ""key"":""DoWechatSearchYsd""  
            //      },  
            //      {  
            //           ""type"":""view"",  
            //           ""name"":""商品库"",  
            //           ""url"":""http://go81.cn/商品陈列""  
            //      },  
            //      {  
            //           ""name"":""菜单"",  
            //           ""sub_button"":[  
            //            {  
            //               ""type"":""view"",  
            //               ""name"":""招标公告"",  
            //                ""url"":""http://go81.cn/公告/AnnounceList/公开招标""  
            //            },  
            //            {  
            //               ""type"":""view"",  
            //               ""name"":""采购公告"",  
            //               ""url"":""http://go81.cn/公告/AnnounceList/采购招标""  
            //            },  
            //            {  
            //               ""type"":""click"",  
            //               ""name"":""绑定用户"",  
            //               ""key"":""doBindUserInfo""  
            //            }]  
            //       }]  
            // } ";

            return weixinMenu;
        }

        /// <summary>
        /// 验证时检查登录名、密码是否正确
        /// </summary>
        /// <returns>绑定的手机号码</returns>
        [HttpPost]
        public ActionResult CheckLoginMessage()
        {
            var username = Request.Params["username"];
            var password = Request.Params["password"];
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                return Content("信息未填写完整！");
            }
            用户基本数据 u;
            if (!用户管理.验证登录名和密码(username, password, out u) || u == null)
            {
                return Content("用户名或密码错误！");
            }
            Session["weChatUserId"] = u.Id;

            var retstr = "";
            if (u is 单位用户)
            {
                var user = u as 单位用户;
                if (!string.IsNullOrWhiteSpace(user.联系方式.手机))
                {
                    retstr = user.联系方式.手机;
                }
            }
            if (u is 供应商)
            {
                var user = u as 供应商;
                if (!string.IsNullOrWhiteSpace(user.企业联系人信息.联系人手机))
                {
                    retstr = user.企业联系人信息.联系人手机;
                }
            }
            if (u is 运营团队)
            {
                var user = u as 供应商;
                retstr = "18382061371";
            }
            if (retstr == "")
            {
                return Content("该账号还未填写手机号，无法实现绑定，如有疑问，请联系客服！");
            }
            return Content(retstr);
        }

        /// <summary>
        /// 验证时发送的短信验证码
        /// </summary>
        /// <returns>短信验证码</returns>
        [HttpPost]
        public ActionResult SendCode()
        {
            var UserNumber = Request.Params["phone"];
            if (string.IsNullOrWhiteSpace(UserNumber))
            {
                return Content("号码为空！");
            }
            UserNumber = "18382061371";//收信人列表
            var code = "";
            System.Random R = new Random();
            for (int i = 0; i < 6; i++)
            {
                code += R.Next(10).ToString();
            }
            string MessageContent = code + "（西南物资采购网微信绑定动态验证码）工作人员不会向您索要，请勿向任何人泄露。";
            string retstr = MailApiPost.PostDataGetHtml(UserNumber, MessageContent);
            return Content(code);
        }

        /// <summary>
        /// 完成验证，写入数据库
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult CheckSuccess()
        {
            var openid = Request.Params["openid"];
            var userid = Session["weChatUserId"].ToString();
            if (string.IsNullOrWhiteSpace(openid) || string.IsNullOrWhiteSpace(userid))
            {
                return Content("绑定时出现错误，错误代码0x2546");
            }
            if (微信管理.计数微信用户(0, 0, Query<微信用户>.EQ(o => o.openId, openid)) > 0)
            {
                return Content("该账号已经绑定过，无需重复绑定！如有疑问，请联系客服！");
            }
            try
            {
                微信用户 wu = new 微信用户();
                wu.验证时间 = DateTime.Now;
                wu.已验证 = true;
                wu.用户链接.用户ID = long.Parse(userid);
                wu.openId = openid;
                微信管理.添加微信用户(wu);
                return Content("1");
            }
            catch
            {
                return Content("绑定时出现错误，错误代码0x6548");
            }
        }
        public void SealWithText()
        {
            try
            {
                string txt = Request.QueryString["txt"];
                string basestr = Request.QueryString["base"];
                string id = Request.QueryString["id"];
                System.Text.RegularExpressions.Regex reg = new System.Text.RegularExpressions.Regex("[0-9,a-z,A-Z]{7,}");
                var t = new 圆形公章()
                {
                    透明度 = 180,
                    颜色 = Color.Red
                };
                if (reg.IsMatch(txt))
                {
                    t.透明度 = 180;
                    t.半径 = 60;
                    t.边框宽度 = 1;
                    t.中央五角星 = false;
                    t.颜色 = Color.Blue;
                }
                else
                {
                    if (long.Parse(id) == 10005 || long.Parse(id) == 10008 || long.Parse(id) == 10009 || long.Parse(id) == 10013)
                    {
                        t.自动计算签名字体大小 = false;
                        t.业务签名.字体大小 = 15.6F;
                        t.八一五角星 = true;
                    }
                }
                t.旋转角度 = (int)(60 - ((long.Parse(id) % 10000) * 17) % 120);
                t.单位签名.文字 = txt;
                t.业务签名.文字 = basestr;
                Image bitmap = t.绘制结果;
                HttpContext.Response.ContentType = "image/png";
                bitmap.Save(HttpContext.Response.OutputStream, ImageFormat.Png);
            }
            catch
            {
                throw new Exception("印章生成失败！");
            }
        }
        public static bool GetQRCode(string strContent, MemoryStream ms)
        {
            ErrorCorrectionLevel Ecl = ErrorCorrectionLevel.M; //误差校正水平   
            string Content = strContent;//待编码内容  
            QuietZoneModules QuietZones = QuietZoneModules.Two;  //空白区域   
            int ModuleSize = 3;//大小  
            var encoder = new QrEncoder(Ecl);
            QrCode qr;
            if (encoder.TryEncode(Content, out qr))//对内容进行编码，并保存生成的矩阵  
            {
                var render = new GraphicsRenderer(new FixedModuleSize(ModuleSize, QuietZones));
                render.WriteToStream(qr.Matrix, ImageFormat.Png, ms);
            }
            else
            {
                return false;
            }
            return true;
        }
        public void OutPutQrCode()
        {
            string context = Request.Params["str"];
            string strCon = "http://www.go81.cn/%E9%A6%96%E9%A1%B5/SearchAcceptance?id=" + context;
            using (var ms = new MemoryStream())
            {
                string stringtest = strCon;
                GetQRCode(stringtest, ms);
                HttpContext.Response.ContentType = "image/Png";
                HttpContext.Response.OutputStream.Write(ms.GetBuffer(), 0, (int)ms.Length);
            }
        }

        public ActionResult UnitYsdlist()
        {
            try
            {
                var openid = Request.QueryString["openid"];
                var wechatuser = 微信管理.查询微信用户(0, 0, Query<微信用户>.EQ(o => o.openId, openid));
                if (wechatuser.Any())
                {
                    long listcount = 验收单管理.计数验收单(0, 0, Query<验收单>.EQ(m => m.管理单位审核人.用户ID, wechatuser.First().用户链接.用户ID).And(Query<验收单>.EQ(o => o.是否作废, false)), includeDisabled: false);
                    long PageCount = listcount / 10;
                    if (listcount % 10 > 0)
                    {
                        PageCount++;
                    }
                    int CurrentPage = 1;
                    if (Request.QueryString["page"] != null)
                    {
                        CurrentPage = int.Parse(Request.QueryString["page"].ToString());
                    }
                    var ysd = 验收单管理.查询验收单(10 * (CurrentPage - 1), 10, Query<验收单>.EQ(m => m.管理单位审核人.用户ID, wechatuser.First().用户链接.用户ID).And(Query<验收单>.EQ(o => o.是否作废, false)), includeDisabled: false);
                    ViewData["Pagecount"] = PageCount;
                    ViewData["CurrentPage"] = CurrentPage;
                    ViewData["openid"] = openid;
                    return PartialView("UnitYsdlist", ysd);
                }
                else
                {
                    return PartialView("UnitYsdlist");
                }
            }
            catch
            {
                return PartialView("UnitYsdlist");
            }
        }
        public ActionResult GysYsdList()
        {
            try
            {
                var openid = Request.QueryString["openid"];
                var wechatuser = 微信管理.查询微信用户(0, 0, Query<微信用户>.EQ(o => o.openId, openid));
                if (wechatuser.Any())
                {
                    long listcount = 验收单管理.计数验收单(0, 0, Query<验收单>.EQ(m => m.供应商链接.用户ID, wechatuser.First().用户链接.用户ID), includeDisabled: false);
                    long PageCount = listcount / 10;
                    if (listcount % 10 > 0)
                    {
                        PageCount++;
                    }
                    int CurrentPage = 1;
                    if (Request.QueryString["page"] != null)
                    {
                        CurrentPage = int.Parse(Request.QueryString["page"].ToString());
                    }
                    var ysd = 验收单管理.查询验收单(10 * (CurrentPage - 1), 10, Query<验收单>.EQ(m => m.供应商链接.用户ID, wechatuser.First().用户链接.用户ID), includeDisabled: false);
                    ViewData["Pagecount"] = PageCount;
                    ViewData["CurrentPage"] = CurrentPage;
                    ViewData["openid"] = openid;
                    return PartialView("GysYsdList", ysd);
                }
                else
                {
                    return PartialView("GysYsdList");
                }
            }
            catch
            {
                return PartialView("GysYsdList");
            }
        }
        public ActionResult GoodlistWithType()
        {
            long id =long.Parse(Request.QueryString["id"]);
            商品分类 t = 商品分类管理.查找分类(id);
            IEnumerable<商品分类> gt = null;
            if(t.父分类.商品分类.父分类.商品分类!=null)
            {
                gt = t.父分类.商品分类.父分类.商品分类.子分类;
            }
            else
            {
                gt = t.父分类.商品分类.子分类;
            }
            ViewData["id"] = id;
            ViewData["gt"] =gt;
            return View();
        }
        public ActionResult Goodlist()
        {
            IEnumerable<商品分类> gt = 商品分类管理.查找子分类();
            ViewData["gt"] = gt;
            return View();
        }
        public ActionResult GysList()
        {
            try
            {
                long pgCount = 0;
                int currentPg = 1;
                if (!string.IsNullOrWhiteSpace(Request.QueryString["cPg"]))
                {
                    currentPg = int.Parse(Request.QueryString["cPg"]);
                }
                pgCount = 用户管理.计数用户<供应商>(0, 0);
                IEnumerable<供应商> model = 用户管理.查询用户<供应商>(10 * (currentPg - 1), 10);
                ViewData["行业列表"] = 商品分类管理.查找子分类();
                return View(model);
            }
            catch
            {
                return RedirectToAction("Index");
            }
        }
        public class Supplier
        {
            public long Id;
            public string Name;
            public string Area;
            public string Product;
            public string employer;
            public string Sales;
            public string Contact;
            public string Picture;
        }
        public ActionResult Search_Supplier()
        {
            int skip = int.Parse(Request.QueryString["skip"]);
            int pageSize = 0;
            IMongoQuery condition = null;
            List<Supplier> Slist = new List<Supplier>();
            string name;
            string factory;
            string province;
            string city;
            string area;
            province = Request.QueryString["provice"];
            city = Request.QueryString["city"];
            area = Request.QueryString["area"];
            name = Request.QueryString["name"];
            factory = Request.QueryString["factory"];
            if (province != "")
            {
                if (province != "不限省份")
                {
                    condition = condition.And(Query.EQ("所属地域.省份", new BsonRegularExpression(string.Format("/{0}/i", province))));
                }
            }
            if (city != "")
            {
                if (city != "不限城市")
                {
                    condition = condition.And(Query.EQ("所属地域.城市", new BsonRegularExpression(string.Format("/{0}/i", city))));
                }
            }
            if (area != "")
            {
                if (area != "不限区县")
                {
                    condition = condition.And(Query.EQ("所属地域.区县", new BsonRegularExpression(string.Format("/{0}/i", area))));
                }
            }
            if (name != "")
            {
                condition = condition.And(Query.EQ("企业基本信息.企业名称", new BsonRegularExpression(string.Format("/{0}/i", name))));
            }
            if (factory != "")
            {
                condition = condition.And(Query.EQ("企业基本信息.所属行业", new BsonRegularExpression(string.Format("/{0}/i", factory))));
            }
            IEnumerable<供应商> supplier = null;
            int maintain = 0;
            supplier = 用户管理.查询用户<供应商>(10 * (skip - 1), 10, condition.And(Query<供应商>.Where(o => o.审核数据.审核状态 == 审核状态.审核通过)));
            pageSize = 用户管理.查询用户<供应商>(0, 0, condition.And(Query<供应商>.Where(o=>o.审核数据.审核状态== 审核状态.审核通过))).Count() / 10;
            maintain = 用户管理.查询用户<供应商>(0, 0, condition.And(Query<供应商>.Where(o => o.审核数据.审核状态 == 审核状态.审核通过))).Count() % 10;
            if (maintain > 0)
            {
                pageSize++;
            }
            foreach (var item in supplier)
            {
                Supplier s = new Supplier();
                s.Id = item.Id;
                s.Name = item.企业基本信息.企业名称;
                s.Area = item.所属地域.地域;
                s.Contact = item.企业联系人信息.联系人固定电话;
                s.employer = item.企业基本信息.员工人数.ToString();
                s.Sales = item.企业基本信息.经营类型.ToString() + "/" + item.企业基本信息.经营子类型.ToString();
                var m = "";
                foreach (var h in item.可提供产品类别列表)
                {
                    foreach (var h_s in h.二级分类)
                    {
                        m += h_s + ";";
                    }
                }
                if(item.供应商用户信息.供应商图片.Any())
                {
                    s.Picture = item.供应商用户信息.供应商图片.First();
                }
                else
                {
                    s.Picture = "~/Images/digitpicture.png";
                }
                if(!string.IsNullOrWhiteSpace(m))
                {
                    s.Product = m;
                }
                else
                {
                    s.Product ="未填写";
                }
                Slist.Add(s);
            }
            JsonResult js = new JsonResult() { Data = new { Slist = Slist, pageSize = pageSize } };
            return Json(js, JsonRequestBehavior.AllowGet);
        }
        public ActionResult Gysdetail()
        {
            try
            {
                long id = long.Parse(Request.QueryString["id"]);
                供应商 model = 用户管理.查找用户<供应商>(id);
                return View(model);
            }
            catch
            {
                return RedirectToAction("GysList");
            }
        }
        public class Goods
        {
            public long Id { get; set; }
            public string name { get; set; }
            public string price { get; set; }
            public string url { get; set; }
        }
        public ActionResult Search_Goods()
        {
            List<Goods> g = new List<Goods>();
            int skip = int.Parse(Request.QueryString["skip"]);
            long tid =long.Parse(Request.QueryString["id"]);
            string name="";
            if (!string.IsNullOrWhiteSpace(Request.QueryString["name"]))
            {
                name= Request.QueryString["name"];
            }
            IMongoQuery condition = null;
            if(!string.IsNullOrWhiteSpace(name))
            {
                condition = condition.And(Query.EQ("商品信息.商品名", new BsonRegularExpression(string.Format("/{0}/i", name))));
            }
            long pageSize = 0;
            long maintain = 0;
            IEnumerable<商品> gs = 商品管理.查询分类下商品(tid,18 * (skip - 1), 18, condition.And(Query<商品>.Where(m => m.审核数据.审核状态 == 审核状态.审核通过)));
            pageSize = 商品管理.计数分类下商品(tid,0, 0, condition.And(Query<商品>.Where(m => m.审核数据.审核状态 == 审核状态.审核通过))) / 18;
            maintain = 商品管理.计数分类下商品(tid,0, 0, condition.And(Query<商品>.Where(m => m.审核数据.审核状态 == 审核状态.审核通过))) % 18;
            if(maintain>0)
            {
                pageSize++;
            }
            foreach(var item in gs)
            {
                Goods m = new Goods();
                m.Id = item.Id;
                m.name = item.商品信息.商品名;
                m.price = item.销售信息.价格.ToString();
                if(item.商品信息.商品图片.Any())
                {
                    m.url = item.商品信息.商品图片.First().Replace("original", "150X150");
                }
                else
                {
                    m.url = "~/Images/digitpicture.png";
                }
                g.Add(m);
            }
            JsonResult json = new JsonResult() { Data = new { Glist = g, pageSize = pageSize }};
            return Json(json, JsonRequestBehavior.AllowGet);
        }
        public ActionResult Search_Gys_Goods()
        {
            List<Goods> g = new List<Goods>();
            long uid = long.Parse(Request.QueryString["id"]);
            int skip = int.Parse(Request.QueryString["skip"]);
            long pageSize = 0;
            long maintain = 0;
            IEnumerable<商品> gs = 商品管理.查询供应商商品(uid,18 * (skip - 1), 18,Query<商品>.Where(m => m.审核数据.审核状态 == 审核状态.审核通过));
            pageSize = 商品管理.计数供应商商品(uid, 0, 0,Query<商品>.Where(m => m.审核数据.审核状态 == 审核状态.审核通过)) / 18;
            maintain = 商品管理.计数供应商商品(uid, 0, 0, Query<商品>.Where(m => m.审核数据.审核状态 == 审核状态.审核通过)) % 18;
            if (maintain > 0)
            {
                pageSize++;
            }
            foreach (var item in gs)
            {
                Goods m = new Goods();
                m.Id = item.Id;
                m.name = item.商品信息.商品名;
                m.price = item.销售信息.价格.ToString();
                if (item.商品信息.商品图片.Any())
                {
                    m.url = item.商品信息.商品图片.First().Replace("original", "150X150"); ;
                }
                else
                {
                    m.url = "~/Images/digitpicture.png";
                }
                g.Add(m);
            }
            JsonResult json = new JsonResult() { Data = new { Glist = g, pageSize = pageSize } };
            return Json(json, JsonRequestBehavior.AllowGet);
        }
        public ActionResult Gooddetail()
        {
            try
            {
                long id = long.Parse(Request.QueryString["id"]);
                商品 model = 商品管理.查找商品(id);
                return View(model);
            }
            catch
            {
                return RedirectToAction("Goodlist");
            }
        }
        public ActionResult Hot_Recommend()
        {
            try
            {
                IEnumerable<供应商> gys = 用户管理.查询用户<供应商>(0,300,Query<供应商>.Where(m=>m.审核数据.审核状态== 审核状态.审核通过)).OrderByDescending(o => o.供应商用户信息.点击量).Take(12);
                IEnumerable<商品> good = 商品管理.查询商品(0, 300, Query<供应商>.Where(m => m.审核数据.审核状态 == 审核状态.审核通过)).OrderByDescending(o => o.销售信息.点击量).Take(12);
                ViewData["gys"] = gys;
                ViewData["good"] = good;
                return View();
            }
            catch
            {
                return RedirectToAction("GysList");
            }
        }
        public ActionResult ExpertApply()
        {
            入库申请用户 model = new 入库申请用户();
            return View(model);
        }
        public ActionResult GysApply()
        {
            入库申请用户 model = new 入库申请用户();
            return View(model);
        }
        public ActionResult AboutUs()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Apply(入库申请用户 model)
        {
            string type = Request.Form["cator"];
            switch(type)
            {
                case "supplier": model.申请类型 = 申请类型.供应商; break;
                case "expert": model.申请类型 = 申请类型.专家; break;
            }
            if(ModelState.IsValid)
            {
                入库申请用户管理.添加入库申请用户(model);
            }
            else
            {
                if (type == "supplier")
                {
                    return Content("<script>alert('请认真填写联系方式！');window.location='/WeChat/GysApply';</script>");
                }
                else
                {
                    return Content("<script>alert('请认真填写联系方式！');window.location='/WeChat/ExpertApply';</script>");
                }
            }
            if (type == "supplier")
            {
                return Content("<script>alert('您已成功提交了您的联系方式，稍后我们会和您联系');window.location='/WeChat/GysApply';</script>");
            }
            else
            {
                return Content("<script>alert('您已成功提交了您的联系方式，稍后我们会和您联系');window.location='/WeChat/ExpertApply';</script>");
            }
        }
        public ActionResult MyService()
        {
            用户基本数据 u=this.HttpContext.获取当前用户();
            if(u==null)
            {
                return RedirectToAction("Login");
            }
            else
            {
                if(u.GetType()!=typeof(供应商))
                {
                    return RedirectToAction("Login");
                }
                else
                {
                    IEnumerable<供应商服务记录> model=供应商服务记录管理.查询供应商服务记录(0, 0);
                    return View(model);
                }
            }
        }
        public ActionResult Login()
        {
            return View();
        }
        [HttpPost]
        public ActionResult LoginIn()
        {
            string name = Request.Form["umane"];
            string pwd = Request.Form["pwd"];
            if(!string.IsNullOrWhiteSpace(name)&&!string.IsNullOrWhiteSpace(pwd))
            {
                用户基本数据 u = 登录管理.登录(this.HttpContext, name, pwd, true);
                if(u!=null)
                {
                    if(u.GetType()==typeof(供应商))
                    {
                        return RedirectToAction("MyService");
                    }
                    else
                    {
                        return RedirectToAction("Goodlist");
                    }
                }
                else
                {
                    return RedirectToAction("Goodlist");
                }
            }
            else
            {
                return Content("<script>alert('请正确填写你的登录信息');</script>");
            }
        }
        public ActionResult YsdDetail(long? id)
        {
            var ysd = 验收单管理.查找验收单((long)id);
            var a = Request.QueryString["a"];//1:单位用户   0:其他用户
            ViewData["审核验收单"] = "0";
            if (a != null && a == "1")
            {
                ViewData["审核验收单"] = "1";
            }
            var openid = Request.QueryString["openid"];
            ViewData["openiddetail"] = openid;
            var user = 微信管理.查询微信用户(0, 0, Query<微信用户>.EQ(o => o.openId, openid));
            if (user.Count() > 0)
            {
                var userid = user.First().用户链接.用户ID;
                if (ysd.供应商链接.用户ID == userid || ysd.管理单位审核人.用户ID == userid)
                {
                    return PartialView("YsdDetail", ysd);
                }
            }
            return PartialView("YsdDetail");
        }
        public ActionResult DoYsdDetail()
        {
            var id = Request.Params["id"];
            var action = Request.Params["action"];

            var ysd = 验收单管理.查找验收单(long.Parse(id));
            var reason = Request.Params["reason"];

            var openid = Request.Params["openid"];
            var user = 微信管理.查询微信用户(0, 0, Query<微信用户>.EQ(o => o.openId, openid));
            if (user.Count() > 0 && user.First().用户链接.用户ID == ysd.管理单位审核人.用户ID)
            {
                ysd.审核数据.审核者.用户ID = ysd.管理单位审核人.用户ID;
                ysd.验收单核对码 = RandomString.Generate(6, 3);
                ysd.审核数据.审核时间 = DateTime.Now;
                if (action == "通过审核")
                {
                    ysd.审核数据.审核状态 = 审核状态.审核通过;
                    ysd.审核数据.审核不通过原因 = "";
                }
                else if (action == "不通过审核")
                {
                    ysd.审核数据.审核状态 = 审核状态.审核未通过;
                    ysd.审核数据.审核不通过原因 = reason;
                }
                验收单管理.更新验收单(ysd);
            }
            return Content("<script>window.location='/WeChat/UnitYsdlist?openid=" + openid + "';</script>");
        }

        /// <summary>
        /// 生成优惠码
        /// </summary>
        /// <returns></returns>
        public string GenerateDisCountCode(string type)
        {
            var str = "";
            var d = DateTime.Now;
            var random = new Random();
            var rd = random.Next(10, 10000);
            if (type == "gys")
            {
                str = string.Format("TGMG{0}{1}{2}{3}{4}", d.Hour, d.Minute, d.Second, d.Millisecond, rd);
            }
            else if(type == "unit")
            {
                str = string.Format("CDJQ{0}{1}{2}{3}{4}", d.Hour, d.Minute, d.Second, d.Millisecond, rd);
            }
            return str;
        }
    }

}


