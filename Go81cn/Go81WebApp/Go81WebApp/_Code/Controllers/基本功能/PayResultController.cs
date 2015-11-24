using Go81WebApp._Code.Models.Helpers;
using Go81WebApp.Models.管理器;
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
using Go81WebApp.Models.管理器.订单管理;
using Go81WebApp.Models.数据模型.订单数据模型;
using Com.Alipay;
using System.Collections.Specialized;
using Go81WebApp.Models.管理器.统计管理;


namespace Go81WebApp.Controllers.基本功能
{
    public class PayResultController : Controller
    {
        private 个人用户 currentUser
        {
            get
            {
                return HttpContext.获取当前用户<个人用户>();
            }
        }
        public ActionResult Payment_Result()
        {
            try
            {
                SortedDictionary<string, string> sPara = GetRequestGet();
                if (sPara.Count > 0)//判断是否有带返回参数
                {
                    Notify aliNotify = new Notify();
                    bool verifyResult = aliNotify.Verify(sPara, Request.QueryString["notify_id"], Request.QueryString["sign"]);
                    if (verifyResult)//验证成功
                    {
                        /////////////////////////////////////////////////////////////////////////////////////////////////////////////
                        //请在这里加上商户的业务逻辑程序代码
                        var account = Request.QueryString["total_fee"];
                        string id = Request.QueryString["extra_common_param"];//传回的商品id和采购数量
                        //——请根据您的业务逻辑来编写程序（以下代码仅作参考）——
                        //获取支付宝的通知返回参数，可参考技术文档中页面跳转同步通知参数列表

                        //商户订单号

                        string out_trade_no = Request.QueryString["out_trade_no"];

                        //支付宝交易号

                        string trade_no = Request.QueryString["trade_no"];

                        //交易状态
                        string trade_status = Request.QueryString["trade_status"];
                        string ordercode="";
                        if(!string.IsNullOrWhiteSpace(Request.QueryString["body"]))
                        {
                            ordercode=Request.QueryString["body"];//优惠码
                        }

                        if (Request.QueryString["trade_status"] == "TRADE_FINISHED" || Request.QueryString["trade_status"] == "TRADE_SUCCESS")
                        {
                            if (订单管理.计数订单(0, 0, Query<订单>.Where(m => m.Id == long.Parse(id))) == 0)
                            {
                                ViewData["Trade_Status"] = 0;
                                ViewData["result"] = "不存在此订单";
                            }
                            else
                            {
                                订单 model = 订单管理.查找订单(long.Parse(id));
                                if (!model.已付款)
                                {
                                    if (优惠码管理.计数优惠码(0, 0, Query<优惠码>.Where(m => m.Code == ordercode && m.已使用 == false)) > 0)
                                    {
                                        model.使用优惠码 = true;
                                        优惠码 yh = 优惠码管理.查询优惠码(0, 0, Query<优惠码>.Where(m => m.已使用 == false && m.Code == ordercode)).First();
                                        yh.已使用 = true;
                                        优惠码管理.更新优惠码(yh);
                                    }
                                    model.已付款 = true;
                                    订单管理.更新订单(model);
                                    ViewData["userid"] = model.订单所属用户.用户ID;
                                }
                                else
                                {
                                    ViewData["userid"] = -1;
                                }
                                ViewData["Trade_Status"] = 1;//1订单付款成功，0订单付款失败
                                ViewData["result"] = "您已经成功购买价值为"+ account.ToString() + "的商品。";
                            }
                        }
                        else
                        {
                            ViewData["userid"] = -1;
                            ViewData["Trade_Status"] = 0;
                            ViewData["result"] = "交易为完成";
                            //Response.Write("trade_status=" + Request.QueryString["trade_status"]);
                        }

                        //——请根据您的业务逻辑来编写程序（以上代码仅作参考）——

                        /////////////////////////////////////////////////////////////////////////////////////////////////////////////
                    }
                    else//验证失败
                    {
                        //Response.Write("验证失败");
                        ViewData["userid"] = -1;
                        ViewData["Trade_Status"] = 0;
                        ViewData["result"] = "验证失败";
                    }
                }
                else
                {
                    ViewData["userid"] = -1;
                    ViewData["Trade_Status"] = 0;
                    ViewData["result"] = "无返回参数";
                    //Response.Write("无返回参数");
                }
                return View();
            }
            catch
            {
                ViewData["userid"] = -1;
                ViewData["Trade_Status"] = 0;
                ViewData["result"] = "支付出错！";
                return View();
            }
        }
        public void Notify()
        {
            SortedDictionary<string, string> sPara = GetRequestPost();
            if (sPara.Count > 0)//判断是否有带返回参数
            {
                Notify aliNotify = new Notify();
                bool verifyResult = aliNotify.Verify(sPara, Request.Form["notify_id"], Request.Form["sign"]);
                if (verifyResult)//验证成功
                {
                    /////////////////////////////////////////////////////////////////////////////////////////////////////////////
                    //请在这里加上商户的业务逻辑程序代码
                    var account = Request.Form["total_fee"];
                    string ordercode = "";
                    if (!string.IsNullOrWhiteSpace(Request.Form["body"]))
                    {
                        ordercode = Request.Form["body"];//优惠码
                    }
                    string id = Request.Form["extra_common_param"];//传回的商品id和采购数量
                    //——请根据您的业务逻辑来编写程序（以下代码仅作参考）——
                    //获取支付宝的通知返回参数，可参考技术文档中页面跳转同步通知参数列表

                    //商户订单号

                    string out_trade_no = Request.Form["out_trade_no"];

                    //支付宝交易号

                    string trade_no = Request.Form["trade_no"];

                    //交易状态
                    string trade_status = Request.Form["trade_status"];

                    if (Request.Form["trade_status"] == "TRADE_FINISHED" || Request.Form["trade_status"] == "TRADE_SUCCESS")
                    {
                        if (订单管理.计数订单(0, 0, Query<订单>.Where(m => m.Id == long.Parse(id))) != 0)
                        {
                            订单 model = 订单管理.查找订单(long.Parse(id));
                            if (!model.已付款)
                            {
                                if (优惠码管理.计数优惠码(0, 0, Query<优惠码>.Where(m => m.Code == ordercode&&m.已使用==false))>0)
                                {
                                    model.使用优惠码 = true;
                                    优惠码 yh=优惠码管理.查询优惠码(0, 0, Query<优惠码>.Where(m => m.已使用 == false && m.Code == ordercode)).First();
                                    yh.已使用 = true;
                                    优惠码管理.更新优惠码(yh);
                                }
                                model.已付款 = true;
                                订单管理.更新订单(model);
                            }
                        }
                    }
                    //——请根据您的业务逻辑来编写程序（以上代码仅作参考）——

                    /////////////////////////////////////////////////////////////////////////////////////////////////////////////
                }
            }
        }
        [HttpPost]
        public void SendMessage1()
        {
            try
            {
                double amount = 0.01;//计算总价格
                long id = 0;
                string ordercode = "";
                if (!string.IsNullOrWhiteSpace(Request.Form["money"]))
                {
                    amount = double.Parse(Request.Form["money"]);//订单总价格
                }
                if (!string.IsNullOrWhiteSpace(Request.Form["orderid"]))
                {
                    id = long.Parse(Request.Form["orderid"]);//订单Id
                }
                if (!string.IsNullOrWhiteSpace(Request.Form["yhm"]))
                {
                    ordercode = Request.Form["yhm"];//订单优惠码
                }
                long counter=优惠码管理.计数优惠码(0, 0, Query<优惠码>.Where(m => m.Code == ordercode&&m.已使用==false));
                long count = 订单管理.计数订单(0, 0, Query<订单>.Where(m => m.Id == id && m.已付款 == false && m.订单所属用户.用户ID == currentUser.Id));
                if (count != 0)
                {
                    //支付类型
                    string payment_type = "1";
                    //必填，不能修改
                    //服务器异步通知页面路径
                    string notify_url = "http://www.go81.cn/PayResult/Notify";
                    //需http://格式的完整路径，不能加?id=123这类自定义参数

                    //页面跳转同步通知页面路径
                    string return_url = "http://www.go81.cn/PayResult/Payment_Result";
                    //需http://格式的完整路径，不能加?id=123这类自定义参数，不能写成http://localhost/
                    //商户订单号
                    string out_trade_no = Guid.NewGuid().ToString();
                    //商户网站订单系统中唯一订单号，必填

                    //订单名称   
                    string subject = "商品采购".Trim();
                    //必填
                    //订单描述
                    string body ="";
                    if (counter > 0)
                    {
                        body = ordercode;
                        amount -= 5;
                    }
                    else
                    {
                        body = "商品付款信息";
                    }
                    //付款金额
                    string total_fee = amount.ToString().Trim();
                    //必填
                    ////////////////////////////////////////////////////////////////////////////////////////////////

                    //把请求参数打包成数组
                    SortedDictionary<string, string> sParaTemp = new SortedDictionary<string, string>();
                    sParaTemp.Add("partner", Config.Partner);
                    //sParaTemp.Add("seller_email", Config.Seller_email);
                    sParaTemp.Add("seller_id", Config.Partner);
                    sParaTemp.Add("_input_charset", Config.Input_charset.ToLower());
                    sParaTemp.Add("service", "create_direct_pay_by_user");
                    sParaTemp.Add("payment_type", payment_type);
                    sParaTemp.Add("notify_url", notify_url);
                    sParaTemp.Add("return_url", return_url);
                    sParaTemp.Add("out_trade_no", out_trade_no);
                    sParaTemp.Add("subject", subject);
                    sParaTemp.Add("total_fee", total_fee);
                    sParaTemp.Add("body", body);
                    sParaTemp.Add("extra_common_param", id.ToString());
                    //建立请求
                    string sHtmlText = Submit.BuildRequest(sParaTemp, "get", "确认");
                    Response.Write(sHtmlText);
                }
                else
                {
                    Response.Write("此订单不存在或支付过，您可以查看你的订单详情！");
                }
            }
            catch
            {
                Response.Write("此订单不存在或支付过，您可以查看你的订单详情！");
            }
        }
        public SortedDictionary<string, string> GetRequestPost()
        {
            int i = 0;
            SortedDictionary<string, string> sArray = new SortedDictionary<string, string>();
            NameValueCollection coll;
            //Load Form variables into NameValueCollection variable.
            coll = Request.Form;

            // Get names of all forms into a string array.
            String[] requestItem = coll.AllKeys;

            for (i = 0; i < requestItem.Length; i++)
            {
                sArray.Add(requestItem[i], Request.Form[requestItem[i]]);
            }
            return sArray;
        }
        public SortedDictionary<string, string> GetRequestGet()
        {
            int i = 0;
            SortedDictionary<string, string> sArray = new SortedDictionary<string, string>();
            NameValueCollection coll;
            //Load Form variables into NameValueCollection variable.
            coll = Request.QueryString;

            // Get names of all forms into a string array.
            String[] requestItem = coll.AllKeys;

            for (i = 0; i < requestItem.Length; i++)
            {
                sArray.Add(requestItem[i], Request.QueryString[requestItem[i]]);
            }

            return sArray;
        }
    }

}


