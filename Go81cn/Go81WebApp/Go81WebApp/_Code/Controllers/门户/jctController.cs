using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Go81WebApp.Models.管理器;
using Go81WebApp.Models.数据模型.消息数据模型;
using Go81WebApp.Models.数据模型.用户数据模型;
using Go81WebApp.Models.数据模型;
using System.Text.RegularExpressions;
using Com.Alipay;
using System.Collections.Specialized;
using System.IO;
using Go81WebApp.Models.数据模型.推广业务数据模型;
using Go81WebApp.Models.管理器.推广业务管理;
using MongoDB.Driver.Builders;
namespace Go81WebApp._Code.Controllers.门户
{
    public class jctController : Controller
    {
        // GET: 军采通
        private 用户基本数据 currentUser
        {
            get
            {
                return HttpContext.获取当前用户();
            }
        }
        public string DealMultipleService(string viptype, int year)
        {
            供应商增值服务申请记录 model = new 供应商增值服务申请记录();
            IEnumerable<供应商服务记录> model2 = 供应商服务记录管理.查询供应商服务记录(0, 0, Query<供应商服务记录>.Where(o => o.所属供应商.用户ID == currentUser.Id));
            if (viptype == "auth" || viptype == "business" || viptype == "basic")
            {
                    long r = 供应商服务记录管理.计数供应商服务记录(0, 0, Query<供应商增值服务申请记录>.Where(o => o.所申请项目名 == viptype && o.结束时间 > DateTime.Now&&o.所属供应商.用户ID==currentUser.Id));
                    if(r==0)
                    {
                        switch (viptype)
                        {
                            case "basic":
                                model.所属供应商.用户ID = currentUser.Id;
                                model.服务期限 = year;
                                model.所申请项目名 = "基础会员";
                                供应商增值服务申请记录管理.添加供应商增值服务申请记录(model); break;
                            case "auth":
                                model.所属供应商.用户ID = currentUser.Id;
                                model.服务期限 = year;
                                model.所申请项目名 = "标准会员";
                                供应商增值服务申请记录管理.添加供应商增值服务申请记录(model); break;
                            case "business":
                                model.所属供应商.用户ID = currentUser.Id;
                                model.所申请项目名 = "商务会员";
                                model.服务期限 = year;
                                供应商增值服务申请记录管理.添加供应商增值服务申请记录(model);
                                break;
                        }
                    }
                if (model2 != null && model2.Count() != 0)
                {
                    供应商服务记录 model4 = model2.First();
                    if (model4.可申请的服务.Count != 0)
                    {
                        var a = model4.可申请的服务.Find(o => o.所申请项目名 == model.所申请项目名);
                        model4.可申请的服务.Remove(a);
                    }
                    switch (viptype)
                    {
                        case "auth":
                            if (model4.申请中的服务.Find(o => o.所申请项目名 == "基础会员") != null)
                            {
                                var m = model4.申请中的服务.Find(o => o.所申请项目名 == "基础会员");
                                model4.申请中的服务.Remove(m);
                            }
                            model4.申请中的服务.Add(model);
                            model4.所属供应商.用户ID = currentUser.Id;
                            供应商服务记录管理.更新供应商服务记录(model4);
                            break;
                        case "business":
                            if (model4.申请中的服务.Find(o => o.所申请项目名 == "标准会员") != null)
                            {
                                var m = model4.申请中的服务.Find(o => o.所申请项目名 == "标准会员");
                                model4.申请中的服务.Remove(m);
                            }
                            else if (model4.申请中的服务.Find(o => o.所申请项目名 == "基础会员") != null)
                            {
                                var m = model4.申请中的服务.Find(o => o.所申请项目名 == "基础会员");
                                model4.申请中的服务.Remove(m);
                            }
                            model4.申请中的服务.Add(model);
                            model4.所属供应商.用户ID = currentUser.Id;
                            供应商服务记录管理.更新供应商服务记录(model4);
                            break;
                    }
                }
                else
                {
                    供应商服务记录 model3 = new 供应商服务记录();
                    foreach (var item in 扣费规则.规则列表)
                    {
                        var b = new 供应商增值服务申请记录();
                        b.所申请项目名 = item.扣费项目名;
                        b.所属供应商.用户ID = currentUser.Id;
                        model3.可申请的服务.Add(b);
                    }
                    var a = model3.可申请的服务.Find(o => o.所申请项目名 == model.所申请项目名);
                    model3.可申请的服务.Remove(a);
                    model3.所属供应商.用户ID = currentUser.Id;
                    model3.申请中的服务.Add(model);
                    供应商服务记录管理.添加供应商服务记录(model3);
                }
                if (viptype == "auth")
                {
                    return "您已成功申请军采通标准会员，请耐心等待网站的审核！";
                }
                else if (viptype == "business")
                {
                    return "您已成功申请军采通商务会员，请耐心等待网站的审核！";
                }
                else if (viptype == "basic")
                {
                    return "您已成功申请军采通基础会员，请耐心等待网站的审核！";
                }
                else
                {
                    return "申请会员出错！";
                }
            }
            else
            {
                return "申请会员出错！";
            }
        }
        public string DealSingleService(string single, string multiple)
        {
            var model = new 供应商增值服务申请记录();
            if (!string.IsNullOrWhiteSpace(single) || !string.IsNullOrWhiteSpace(multiple))
            {
                if (!string.IsNullOrWhiteSpace(single))
                {
                    IEnumerable<供应商服务记录> record = 供应商服务记录管理.查询供应商服务记录(0, 0, Query<供应商服务记录>.Where(o => o.所属供应商.用户ID == currentUser.Id));
                    if (record != null && record.Count() != 0)
                    {
                        model.所申请项目名 = single.Split(',')[0];
                        model.所属供应商.用户ID = currentUser.Id;
                        model.服务期限 = int.Parse(single.Split(',')[1]);
                        供应商增值服务申请记录管理.添加供应商增值服务申请记录(model);
                        供应商服务记录 record1 = record.First();
                        if (record1.可申请的服务.Find(o => o.所申请项目名 == model.所申请项目名) != null)
                        {
                            var a = record1.可申请的服务.Find(o => o.所申请项目名 == model.所申请项目名);
                            record1.可申请的服务.Remove(a);
                        }
                        record1.申请中的服务.Add(model);
                        供应商服务记录管理.更新供应商服务记录(record1);

                    }
                    else
                    {
                        model.所申请项目名 = single.Split(',')[0];
                        model.所属供应商.用户ID = currentUser.Id;
                        model.服务期限 = int.Parse(single.Split(',')[1]);
                        供应商增值服务申请记录管理.添加供应商增值服务申请记录(model);
                        供应商服务记录 record2 = new 供应商服务记录();
                        foreach (var item in 扣费规则.规则列表)
                        {
                            var b = new 供应商增值服务申请记录();
                            b.所申请项目名 = item.扣费项目名;
                            b.所属供应商.用户ID = currentUser.Id;
                            record2.可申请的服务.Add(b);
                        }
                        if (record2.可申请的服务.Find(o => o.所申请项目名 == model.所申请项目名) != null)
                        {
                            var a = record2.可申请的服务.Find(o => o.所申请项目名 == model.所申请项目名);
                            record2.可申请的服务.Remove(a);
                        }
                        record2.所属供应商.用户ID = currentUser.Id;
                        record2.申请中的服务.Add(model);
                        供应商服务记录管理.添加供应商服务记录(record2);
                    }
                }
                if (!string.IsNullOrWhiteSpace(multiple))
                {
                    IEnumerable<供应商服务记录> record = 供应商服务记录管理.查询供应商服务记录(0, 0, Query<供应商服务记录>.Where(o => o.所属供应商.用户ID == currentUser.Id));
                    string[] service = multiple.Split('|');
                    for (int m = 0; m < service.Length - 1; m++)
                    {
                        model = new 供应商增值服务申请记录();
                        if (record != null && record.Count() != 0)
                        {
                            model.所申请项目名 = service[m].Split(',')[0];
                            model.所属供应商.用户ID = currentUser.Id;
                            model.服务期限 = int.Parse(service[m].Split(',')[1]);
                            if (供应商增值服务申请记录管理.计数供应商增值服务申请记录(0, 0, Query<供应商增值服务申请记录>.Where(o => o.所属供应商.用户ID == currentUser.Id && o.结束时间 > DateTime.Now))==0)
                            {
                                供应商增值服务申请记录管理.添加供应商增值服务申请记录(model);
                            }
                            供应商服务记录 record1 = record.First();
                            if (record1.可申请的服务.Find(o => o.所申请项目名 == model.所申请项目名) != null)
                            {
                                var a = record1.可申请的服务.Find(o => o.所申请项目名 == model.所申请项目名);
                                record1.可申请的服务.Remove(a);
                            }
                            record1.申请中的服务.Add(model);
                            供应商服务记录管理.更新供应商服务记录(record1);
                        }
                        else
                        {
                            model.所申请项目名 = service[m].Split(',')[0];
                            model.所属供应商.用户ID = currentUser.Id;
                            model.服务期限 = int.Parse(service[m].Split(',')[1]);
                            if (供应商增值服务申请记录管理.计数供应商增值服务申请记录(0, 0, Query<供应商增值服务申请记录>.Where(o => o.所属供应商.用户ID == currentUser.Id && o.结束时间 > DateTime.Now)) == 0)
                            {
                                供应商增值服务申请记录管理.添加供应商增值服务申请记录(model);
                            }
                            供应商服务记录 record2 = new 供应商服务记录();
                            foreach (var item in 扣费规则.规则列表)
                            {
                                var b = new 供应商增值服务申请记录();
                                b.所申请项目名 = item.扣费项目名;
                                b.所属供应商.用户ID = currentUser.Id;
                                record2.可申请的服务.Add(b);
                            }
                            if (record2.可申请的服务.Find(o => o.所申请项目名 == model.所申请项目名) != null)
                            {
                                var a = record2.可申请的服务.Find(o => o.所申请项目名 == model.所申请项目名);
                                record2.可申请的服务.Remove(a);
                            }
                            record2.所属供应商.用户ID = currentUser.Id;
                            record2.申请中的服务.Add(model);
                            供应商服务记录管理.添加供应商服务记录(record2);
                        }
                    }
                }
                return "您已成功申请您选择的军采通服务，请耐心等待网站的审核";
            }
            else
            {
                return "选择出错！";
            }
        }
        public ActionResult ExistName()
        {
            if (currentUser.GetType() != typeof(Go81WebApp.Models.数据模型.用户数据模型.供应商))
            {
                return Content("-1");
            }
            else
            {
                bool exist = false;
                string name = Request.QueryString["n"];
                IEnumerable<供应商服务记录> model = 供应商服务记录管理.查询供应商服务记录(0, 0, Query<供应商服务记录>.Where(o => o.所属供应商.用户ID == currentUser.Id));
                if (model != null && model.Count() != 0)
                {
                    供应商服务记录 m = model.First();
                    if (m.申请中的服务 != null && m.申请中的服务.Count != 0)
                    {
                        foreach (var item in m.申请中的服务)
                        {
                            if (item.所申请项目名 == name)
                            {
                                exist = true;
                                break;
                            }
                        }
                    }
                    if (m.已开通的服务 != null && m.已开通的服务.Count != 0)
                    {
                        foreach (var item in m.已开通的服务)
                        {
                            if (item.所申请项目名 == name)
                            {
                                exist = true;
                                break;
                            }
                        }
                    }
                }
                if (exist)
                {
                    return Content("-1");
                }
                else
                {
                    return Content("0");
                }
            }
        }
        public ActionResult IsExist()
        {
            if (currentUser.GetType() != typeof(Go81WebApp.Models.数据模型.用户数据模型.供应商))
            {
                return Content("此功能只适用于供应商，如果您是供应商，请先登录");
            }
            else
            {
                IEnumerable<供应商服务记录> model = 供应商服务记录管理.查询供应商服务记录(0, 0, Query<供应商服务记录>.Where(o => o.所属供应商.用户ID == currentUser.Id));
                string type = Request.QueryString["type"];
                if (type == "business")
                {
                    if (model != null && model.Count() != 0)
                    {
                        供应商服务记录 a = model.First();
                        if (a.申请中的服务.Find(o => o.所申请项目名 == "商务会员") != null)
                        {
                            return Content("您已申请过军采通商务会员，您还可以申请单项服务。");
                        }
                        else
                        {
                            return Content("");
                        }
                    }
                    else
                    {
                        return Content("");
                    }
                }
                else if (type == "auth")
                {
                    if (model != null && model.Count() != 0)
                    {
                        供应商服务记录 b = model.First();
                        if (b.申请中的服务.Find(o => o.所申请项目名 == "商务会员") != null)
                        {
                            return Content("已申请商务会员后不能再更改成标准会员！");
                        }
                        else
                        {
                            return Content("");
                        }
                    }
                    else
                    {
                        return Content("");
                    }
                }
                else if (type == "basic")
                {
                    if (model != null && model.Count() != 0)
                    {
                        供应商服务记录 b = model.First();
                        if (b.申请中的服务.Find(o => o.所申请项目名 == "标准会员") != null || b.申请中的服务.Find(o => o.所申请项目名 == "商务会员") != null)
                        {
                            return Content("已申请高级会员后不能再更改成基础会员！");
                        }
                        else
                        {
                            return Content("");
                        }
                    }
                    else
                    {
                        return Content("");
                    }
                }
                else
                {
                    return Content("出错！");
                }
            }
        }
        [登录验证]
        [HttpPost]
        public ActionResult ApplyService()
        {
            string result = "";
            string viptype = Request.Form["viptype"].ToString();
            int year = int.Parse(Request.Form["year"].ToString());
            IEnumerable<供应商增值服务申请记录> Newmodel = 供应商增值服务申请记录管理.查询供应商增值服务申请记录(0, 0, Query<供应商增值服务申请记录>.Where(o => o.基本数据.已删除 == false));
            if (Newmodel != null && Newmodel.Count() != 0)
            {
                result = DealMultipleService(viptype, year);
                return Content("<script>alert('" + result + "');window.location='/jct/applyvip';</script>");
            }
            else
            {
                result = DealMultipleService(viptype, year);
                return Content("<script>alert('" + result + "');window.location='/jct/applyvip';</script>");
            }
        }
        [登录验证]
        [HttpPost]
        public ActionResult SingleService()
        {
            string result = "";
            string single = Request.Form["single"];
            string multiple = Request.Form["multiple"];
            IEnumerable<供应商增值服务申请记录> Newmodel = 供应商增值服务申请记录管理.查询供应商增值服务申请记录(0, 0, Query<供应商增值服务申请记录>.Where(o => o.基本数据.已删除 == false));
            if (Newmodel != null && Newmodel.Count() != 0)
            {
                result = DealSingleService(single, multiple);
                return Content("<script>alert('" + result + "');window.location='/jct/applyvip';</script>");
            }
            else
            {
                result = DealSingleService(single, multiple);
                return Content("<script>alert('" + result + "');window.location='/jct/applyvip';</script>");
            }
        }
        public ActionResult JcContract()
        {
            return View();
        }
        public ActionResult ApplyVip()
        {
            if (currentUser.GetType() != typeof(Go81WebApp.Models.数据模型.用户数据模型.供应商))
            {
                ViewData["name"] = new List<string>();
                ViewData["appliedService"] = new List<string>();
                ViewData["history"] = 0;
                return View();
            }
            else
            {
                List<string> names = new List<string>();
                List<string> appliedService = new List<string>();
                供应商 Newmodel = 用户管理.查找用户<供应商>(currentUser.Id);
                IEnumerable<供应商服务记录> model = 供应商服务记录管理.查询供应商服务记录(0, 0, Query<供应商增值服务申请记录>.Where(o => o.所属供应商.用户ID == currentUser.Id));
                if (model != null && model.Count() != 0)
                {
                    if (model.First().申请中的服务 != null && model.First().申请中的服务.Count != 0)
                    {
                        foreach (var item in model.First().申请中的服务)
                        {
                            names.Add(item.所申请项目名);
                        }
                    }
                    if (model.First().已开通的服务 != null && model.First().已开通的服务.Count != 0)
                    {
                        foreach (var m in model.First().已开通的服务)
                        {
                            appliedService.Add(m.所申请项目名);
                        }
                    }
                }
                ViewData["appliedService"] = appliedService;
                ViewData["name"] = names;
                return View();
            }
        }
        public ActionResult Payment_Result()
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
                    var 当前用户 = currentUser as 供应商;
                    var 会员级别 = 当前用户.供应商用户信息.认证级别.ToString();
                    double amount = 0D;
                    switch (会员级别)
                    {
                        case "军采通标准会员":
                            switch (account)
                            {
                                case "2850":
                                    amount = double.Parse(account) / 0.95;
                                    break;
                                case "5400":
                                    amount = double.Parse(account) / 0.9;
                                    break;
                                case "8500":
                                    amount = double.Parse(account) / 0.85;
                                    break;
                                case "16000":
                                    amount = double.Parse(account) / 0.8;
                                    break;
                            }
                            break;
                        case "军采通高级会员":
                            switch (account)
                            {
                                case "2700":
                                    amount = double.Parse(account) / 0.9;
                                    break;
                                case "5100":
                                    amount = double.Parse(account) / 0.85;
                                    break;
                                case "8000":
                                    amount = double.Parse(account) / 0.8;
                                    break;
                                case "15000":
                                    amount = double.Parse(account) / 0.75;
                                    break;
                            }
                            break;
                        default:
                            switch (account)
                            {
                                case "2940":
                                    amount = double.Parse(account) * 0.98;
                                    break;
                                case "5700":
                                    amount = double.Parse(account) * 0.95;
                                    break;
                                case "9200":
                                    amount = double.Parse(account) * 0.92;
                                    break;
                                case "18000":
                                    amount = double.Parse(account) * 0.9;
                                    break;
                            }
                            break;
                    }


                    var 充值记录 = new 供应商充值记录();
                    充值记录.充值方式 = 充值方式.支付宝;
                    充值记录.充值金额 = (decimal)amount;
                    充值记录.充值时间 = DateTime.Now;
                    充值记录.供应商转款账号 = Request.QueryString["buyer_email"];
                    充值记录.收款账号 = Request.QueryString["seller_email"];
                    充值记录.所属供应商.用户ID = currentUser.Id;
                    供应商充值记录管理.添加供应商充值记录(充值记录);

                    var 余额记录 = 供应商充值余额管理.查询供应商充值余额(0, 0, Query<供应商充值余额>.Where(o => o.所属供应商.用户ID == currentUser.Id));
                    if (余额记录.Count() > 0)
                    {
                        var model = 余额记录.First();
                        model.可用余额 += 充值记录.充值金额;
                        供应商充值余额管理.更新供应商充值余额(model);
                    }
                    else
                    {
                        var 充值余额 = new 供应商充值余额();
                        充值余额.可用余额 = 充值记录.充值金额;
                        充值余额.所属供应商.用户ID = currentUser.Id;
                        供应商充值余额管理.添加供应商充值余额(充值余额);
                    }


                    //——请根据您的业务逻辑来编写程序（以下代码仅作参考）——
                    //获取支付宝的通知返回参数，可参考技术文档中页面跳转同步通知参数列表

                    //商户订单号

                    string out_trade_no = Request.QueryString["out_trade_no"];

                    //支付宝交易号

                    string trade_no = Request.QueryString["trade_no"];

                    //交易状态
                    string trade_status = Request.QueryString["trade_status"];


                    if (Request.QueryString["trade_status"] == "TRADE_FINISHED" || Request.QueryString["trade_status"] == "TRADE_SUCCESS")
                    {
                        ViewData["Trade_Status"] = 1;//1充值成功，0充值失败
                        ViewData["result"] = amount; //Request.QueryString["total_fee"]; //充值金额
                        //ViewData["result"] = "用户号为" + Request.QueryString["buyer_id"] + "的你已购买价值为" + Request.QueryString["total_fee"] + "军采通产品，流水号位" + Request.QueryString["trade_no"] + "交易状态：" + Request.QueryString["trade_status"] + "验证ID为：" + Request.QueryString["notify_id"];
                    }
                    else
                    {
                        ViewData["Trade_Status"] = 0;
                        //Response.Write("trade_status=" + Request.QueryString["trade_status"]);
                    }

                    //——请根据您的业务逻辑来编写程序（以上代码仅作参考）——

                    /////////////////////////////////////////////////////////////////////////////////////////////////////////////
                }
                else//验证失败
                {
                    //Response.Write("验证失败");
                    ViewData["Trade_Status"] = 0;
                }
            }
            else
            {
                ViewData["result"] = "无返回参数";
                //Response.Write("无返回参数");
            }
            return View();
        }
        public bool WriteTxt(string str)
        {
            try
            {
                FileStream fs = new FileStream(Server.MapPath("/AliPAY.txt"), FileMode.Append);
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
        public void Notify()
        {
            SortedDictionary<string, string> sPara = GetRequestPost();

            if (sPara.Count > 0)//判断是否有带返回参数
            {
                Notify aliNotify = new Notify();
                bool verifyResult = aliNotify.Verify(sPara, Request.Form["notify_id"], Request.Form["sign"]);
                // WriteTxt(Request.Form["notify_id"]+"\r\n" + Request.Form["sign"]);
                if (verifyResult)//验证成功
                {
                    /////////////////////////////////////////////////////////////////////////////////////////////////////////////
                    //请在这里加上商户的业务逻辑程序代码
                    var account = Request.QueryString["total_fee"];
                    var 当前用户 = currentUser as 供应商;
                    var 会员级别 = 当前用户.供应商用户信息.认证级别.ToString();
                    double amount = 0D;
                    switch (会员级别)
                    {
                        case "军采通标准会员":
                            switch (account)
                            {
                                case "2850":
                                    amount = double.Parse(account) / 0.95;
                                    break;
                                case "5400":
                                    amount = double.Parse(account) / 0.9;
                                    break;
                                case "8500":
                                    amount = double.Parse(account) / 0.85;
                                    break;
                                case "16000":
                                    amount = double.Parse(account) / 0.8;
                                    break;
                            }
                            break;
                        case "军采通高级会员":
                            switch (account)
                            {
                                case "2700":
                                    amount = double.Parse(account) / 0.9;
                                    break;
                                case "5100":
                                    amount = double.Parse(account) / 0.85;
                                    break;
                                case "8000":
                                    amount = double.Parse(account) / 0.8;
                                    break;
                                case "15000":
                                    amount = double.Parse(account) / 0.75;
                                    break;
                            }
                            break;
                        default:
                            switch (account)
                            {
                                case "2940":
                                    amount = double.Parse(account) * 0.98;
                                    break;
                                case "5700":
                                    amount = double.Parse(account) * 0.95;
                                    break;
                                case "9200":
                                    amount = double.Parse(account) * 0.92;
                                    break;
                                case "18000":
                                    amount = double.Parse(account) * 0.9;
                                    break;
                            }
                            break;
                    }


                    var 充值记录 = new 供应商充值记录();
                    充值记录.充值方式 = 充值方式.支付宝;
                    充值记录.充值金额 = (decimal)amount;
                    充值记录.充值时间 = DateTime.Now;
                    充值记录.供应商转款账号 = Request.QueryString["buyer_email"];
                    充值记录.收款账号 = Request.QueryString["seller_email"];
                    充值记录.所属供应商.用户ID = currentUser.Id;
                    供应商充值记录管理.添加供应商充值记录(充值记录);

                    var 余额记录 = 供应商充值余额管理.查询供应商充值余额(0, 0, Query<供应商充值余额>.Where(o => o.所属供应商.用户ID == currentUser.Id));
                    if (余额记录.Count() > 0)
                    {
                        var model = 余额记录.First();
                        model.可用余额 += 充值记录.充值金额;
                        供应商充值余额管理.更新供应商充值余额(model);
                    }
                    else
                    {
                        var 充值余额 = new 供应商充值余额();
                        充值余额.可用余额 = 充值记录.充值金额;
                        充值余额.所属供应商.用户ID = currentUser.Id;
                        供应商充值余额管理.添加供应商充值余额(充值余额);
                    }



                    //——请根据您的业务逻辑来编写程序（以下代码仅作参考）——
                    //获取支付宝的通知返回参数，可参考技术文档中服务器异步通知参数列表

                    //商户订单号

                    string out_trade_no = Request.Form["out_trade_no"];

                    //支付宝交易号

                    string trade_no = Request.Form["trade_no"];

                    //交易状态
                    string trade_status = Request.Form["trade_status"];


                    if (Request.Form["trade_status"] == "TRADE_FINISHED" || Request.Form["trade_status"] == "TRADE_SUCCESS")
                    {
                        WriteTxt("订单号：" + out_trade_no + "交易号：" + trade_no + "交易状态：" + trade_status);
                    }
                    else
                    {
                    }
                    //——请根据您的业务逻辑来编写程序（以上代码仅作参考）——
                    Response.Write("success");  //请不要修改或删除

                    /////////////////////////////////////////////////////////////////////////////////////////////////////////////
                }
                else//验证失败
                {
                    Response.Write("fail");
                }
            }
            else
            {
                Response.Write("无通知参数");
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
        public void SendMessage1()
        {
            var account = Request.Form.Get("AmountRecharge").Replace("元", "").Trim();

            var 当前用户 = currentUser as 供应商;

            var 会员级别 = 当前用户.供应商用户信息.认证级别.ToString();
            double amount = 0D;
            switch (会员级别)
            {
                case "军采通标准会员":
                    switch (account)
                    {
                        case "3000":
                            amount = double.Parse(account) * 0.95;
                            break;
                        case "6000":
                            amount = double.Parse(account) * 0.9;
                            break;
                        case "10000":
                            amount = double.Parse(account) * 0.85;
                            break;
                        case "20000":
                            amount = double.Parse(account) * 0.8;
                            break;
                    }
                    break;
                case "军采通高级会员":
                    switch (account)
                    {
                        case "3000":
                            amount = double.Parse(account) * 0.9;
                            break;
                        case "6000":
                            amount = double.Parse(account) * 0.85;
                            break;
                        case "10000":
                            amount = double.Parse(account) * 0.8;
                            break;
                        case "20000":
                            amount = double.Parse(account) * 0.75;
                            break;
                    }
                    break;
                default:
                    switch (account)
                    {
                        case "3000":
                            amount = double.Parse(account) * 0.98;
                            break;
                        case "6000":
                            amount = double.Parse(account) * 0.95;
                            break;
                        case "10000":
                            amount = double.Parse(account) * 0.92;
                            break;
                        case "20000":
                            amount = double.Parse(account) * 0.9;
                            break;
                    }
                    break;
            }

            //要先判断充值金额是否为空
            //if (string.IsNullOrWhiteSpace(account))
            //{

            //}
            //支付类型
            string payment_type = "1";
            //必填，不能修改
            //服务器异步通知页面路径
            string notify_url = "http://www.go81.cn/jct/Notify";
            //需http://格式的完整路径，不能加?id=123这类自定义参数

            //页面跳转同步通知页面路径
            string return_url = "http://www.go81.cn/jct/Payment_Result";
            //需http://格式的完整路径，不能加?id=123这类自定义参数，不能写成http://localhost/
            //商户订单号
            string out_trade_no = Guid.NewGuid().ToString();
            //商户网站订单系统中唯一订单号，必填

            //订单名称   
            string subject = "军采通服务".Trim();
            //必填

            //付款金额
            string total_fee = account.ToString().Trim();
            //必填

            //订单描述

            string body = "军采通转为入网供应商提供高级服务".Trim();


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
            
            //建立请求
            string sHtmlText = Submit.BuildRequest(sParaTemp, "get", "确认");
            Response.Write(sHtmlText);
        }
        public ActionResult buildWebsite()
        {
            return View();
        }
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult Pushmsg()
        {
            return View();
        }
        public ActionResult purchaseInfo()
        {
            return View();
        }
        public ActionResult Assistance()
        {
            return View();
        }
        public ActionResult Credit_Service()
        {
            return View();
        }
        public ActionResult Finance_Service()
        {
            return View();
        }
        public ActionResult PurchaseOnline()
        {
            return View();
        }
        public ActionResult Project_Management()
        {
            return View();
        }
        public ActionResult Part_PublicPurNav()
        {
            return PartialView("Part_PublicPur/Part_PublicPurNav");
        }
        public ActionResult PublicDdc()
        {
            return View();
        }
        public ActionResult vipService()
        {
            return View();
        }
        public ActionResult Security()
        {
            return View();
        }
        public ActionResult PublicPurBusiness()
        {
            return View();
        }
        public ActionResult SendMessage()
        {
            if (currentUser.GetType() != typeof(Go81WebApp.Models.数据模型.用户数据模型.供应商))
            {
                return Content("<script>alert('此功能只适用于供应商，如果您是供应商，请先登录');window.location='/登录/Login?ReturnUrl=/jct/Applyvip';</script>");
            }
            else
            {
                Regex rg = new Regex(@"\d{2,5}-\d{7,8}|^1[345689][0-9]{9}");
                站内消息 Msg = new 站内消息();
                对话消息 Talk = new 对话消息();
                Talk.发言人.用户ID = currentUser.Id;
                Msg.收信人.用户ID = 100000000001;
                Msg.重要程度 = 重要程度.一般;
                Msg.发起者.用户ID = currentUser.Id;
                站内消息管理.添加站内消息(Msg, Msg.发起者.用户ID, Msg.收信人.用户ID);
                Talk.消息主体.标题 = "申请加入军采通会员";
                供应商 model = 用户管理.查找用户<供应商>(currentUser.Id);
                if (string.IsNullOrWhiteSpace(Request.Form["phone"]))
                {
                    return Content("<script>alert('请输入手机或电话号码号码，以便我们及时联系您');window.location='/jct/Applyvip?sure=true';</script>");
                }
                else
                {
                    if (model != null)
                    {
                        Talk.消息主体.内容 = model.企业基本信息.企业名称 + "申请加入军采通会员，申请者联系方式：" + model.企业联系人信息.联系人手机 + "," + model.企业联系人信息.联系人固定电话;
                        对话消息管理.添加对话消息(Talk, Msg);
                        return Content("<script>alert('您已申请了军采通会员，请等待工作人员的回复！');window.location='/jct/';</script>");
                    }
                    else
                    {
                        Talk.消息主体.内容 = currentUser.登录信息.登录名 + "申请加入军采通会员，申请者联系方式：" + Request.Form["phone"];
                        if (rg.IsMatch(Request.Form["phone"].ToString()))
                        {
                            对话消息管理.添加对话消息(Talk, Msg);
                            return Content("<script>alert('您已申请了军采通会员，请等待工作人员的回复！');window.location='/jct/';</script>");
                        }
                        else
                        {
                            return Content("<script>alert('请输入正确的联系号码！');window.location='/jct/';</script>");
                        }
                    }
                }
            }
        }
    }
}