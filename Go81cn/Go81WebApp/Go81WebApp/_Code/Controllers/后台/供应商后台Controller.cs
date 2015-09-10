using Com.Alipay;
using FileHelper;
using Gma.QrCodeNet.Encoding;
using Gma.QrCodeNet.Encoding.Windows.Render;
using Go81WebApp.Models.Helpers;
using Go81WebApp.Models.管理器;
using Go81WebApp.Models.管理器.推广业务管理;
using Go81WebApp.Models.数据模型;
using Go81WebApp.Models.数据模型.竞标数据模型;
using Go81WebApp.Models.数据模型.内容数据模型;
using Go81WebApp.Models.数据模型.商品数据模型;
using Go81WebApp.Models.数据模型.推广业务数据模型;
using Go81WebApp.Models.数据模型.项目数据模型;
using Go81WebApp.Models.数据模型.消息数据模型;
using Go81WebApp.Models.数据模型.用户数据模型;
using Helpers.印章;
using Ionic.Zip;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.PanGu;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using MongoDB;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel.DataAnnotations;
using System.Configuration;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
namespace Go81WebApp.Controllers.后台
{
    [登录验证]
    [用户类型验证(typeof(供应商))]
    public class 供应商后台Controller : Controller
    {
        private static int GG_PAGESIZE = int.Parse(ConfigurationManager.AppSettings["公告每页显示条数"]);
        private static int TZ_PAGESIZE = int.Parse(ConfigurationManager.AppSettings["通知每页显示条数"]);
        private static int ZNXX_PAGESIZE = int.Parse(ConfigurationManager.AppSettings["站内消息每页显示条数"]);
        private static int PRO_PAGESIZE = int.Parse(ConfigurationManager.AppSettings["后台商品列表商品每页显示条数"]);
        private static int ACCEPTA_PAGESIZE = int.Parse(ConfigurationManager.AppSettings["验收单每页显示条数"]);
        private 供应商 currentUser
        {
            get
            {
                return HttpContext.获取当前用户<供应商>();
            }
        }
        public 供应商服务记录 增值服务记录
        {
            get
            {
                var 服务记录 = 供应商服务记录管理.查询供应商服务记录(0, 0, Query<供应商服务记录>.Where(o => o.所属供应商.用户ID == currentUser.Id));
                if (服务记录.Count() > 0)
                {
                    return 服务记录.First();
                }
                else
                {
                    return null;
                }
            }
            
        }
        public void 清除过期服务()
        {
            var 服务列表 = 供应商服务记录管理.查询供应商服务记录(0, 0, Query<供应商服务记录>.Where(o => o.所属供应商.用户ID == currentUser.Id));
            if (服务列表.Count() > 0)
            {
                var 服务记录 = 服务列表.First();
                foreach (var k in 服务记录.已开通的服务.ToArray())
                {
                    var 服务 = 供应商计费项目扣费记录管理.查询供应商计费项目扣费记录(0, 0, Query<供应商计费项目扣费记录>.Where(o => o.所属供应商.用户ID == currentUser.Id && o.扣费服务项目 == k.所申请项目名), false, SortBy.Descending("基本数据.添加时间")).First();
                    var 该服务是否过期 = false;
                    var 服务到期时间 = DateTime.Now;
                    switch (服务.扣费类型)
                    {
                        case 扣费类型.按年扣费:
                            服务到期时间 = 服务.扣费时间.AddYears(服务.所属年);
                            if (DateTime.Now > 服务到期时间.AddDays(-1)) { 该服务是否过期 = true; }
                            break;
                        case 扣费类型.按月扣费:
                            服务到期时间 = 服务.扣费时间.AddMonths(服务.所属月);
                            if (DateTime.Now > 服务到期时间.AddDays(-1)) { 该服务是否过期 = true; }
                            break;
                        case 扣费类型.按次扣费:
                            if (服务.所属日 <= 0) { 该服务是否过期 = true; }
                            break;
                    }
                    if (该服务是否过期) { 服务记录.已开通的服务.Remove(k); 服务记录.可申请的服务.Add(k); }
                }
                供应商服务记录管理.更新供应商服务记录(服务记录);
            }
        }
        public ActionResult Index()
        {
            //清除过期服务();
            return View();
        }

        public ActionResult Part_BackHead()
        {
            var m = currentUser;
            return PartialView("Gys_Part/Part_BackHead", m);
        }
        public ActionResult Backlog()
        {
            供应商 Newmodel = 用户管理.查找用户<供应商>(currentUser.Id);
            IEnumerable<站内消息> Msg = 站内消息管理.查询收信人的站内消息(0, 0, Newmodel.Id);
            int count = 0;
            foreach (var item in Msg)
            {
                if (item.基本数据.添加时间 > item.收信人.上次阅读时间 && item.发起者.用户数据 != null)
                {
                    count++;
                }
            }
            ViewBag.Toubiao = 招标采购预报名管理.计数招标采购预报名(0, 0, Query<招标采购预报名>.EQ(o => o.预报名已关闭, false));
            ViewBag.GoodCount = 商品管理.计数供应商商品(Newmodel.Id);
            ViewBag.cls = Newmodel.信用评级信息.等级;
            ViewBag.BaseInfo = Newmodel.企业基本信息.已填写完整;
            ViewBag.VipInfo = Newmodel.企业联系人信息.已填写完整;
            ViewBag.SalesInfo = Newmodel.工商注册信息.已填写完整;
            ViewBag.Busniss = Newmodel.营业执照信息.已填写完整;
            ViewBag.TaxInfo = Newmodel.税务信息.已填写完整;
            ViewBag.LawPerson = Newmodel.法定代表人信息.已填写完整;
            ViewBag.goodType = Newmodel.可提供产品类别列表.Count();
            ViewBag.PrintMessage = (Newmodel.审核数据.审核状态 == 审核状态.审核通过 && !Newmodel.供应商用户信息.符合入库标准) ? "由于您所提供的资料还不符合申请入库的标准，故不能打印入库申请表格。如有任何疑问，请拨打网站客服电话咨询，谢谢。" : "";
            if (Newmodel.出资人或股东信息 != null)
            {
                if (Newmodel.出资人或股东信息.Count() == 0)
                {
                    ViewBag.Investor = false;
                }
                else
                {
                    foreach (var item in Newmodel.出资人或股东信息)
                    {
                        if (item.已填写完整 == false)
                        {
                            ViewBag.Investor = false;
                            break;
                        }
                        else
                        {
                            ViewBag.Investor = true;
                        }
                    }
                }
            }
            else
            {
                ViewBag.Investor = false;
            }


            if (Newmodel.财务状况信息 != null)
            {
                if (Newmodel.财务状况信息.Count() == 0)
                {
                    ViewBag.Financial = false;
                }
                else
                {
                    foreach (var item in Newmodel.财务状况信息)
                    {
                        if (item.已填写完整 == false)
                        {
                            ViewBag.Financial = false;
                            break;
                        }
                        else
                        {
                            ViewBag.Financial = true;
                        }
                    }
                }
            }
            else
            {
                ViewBag.Financial = false;
            }
            if (Newmodel.资质证书列表 != null)
            {
                if (Newmodel.资质证书列表.Count() == 0)
                {
                    ViewBag.qualify = false;
                }
                else
                {
                    foreach (var item in Newmodel.资质证书列表)
                    {
                        if (item.已填写完整 == false)
                        {
                            ViewBag.qualify = false;
                            break;
                        }
                    }
                }
            }
            else
            {
                ViewBag.qualify = false;
            }
            if (Newmodel.售后服务机构列表 != null)
            {
                if (Newmodel.售后服务机构列表.Count() == 0)
                {
                    ViewBag.servicedepart = false;
                }
                else
                {
                    foreach (var item in Newmodel.售后服务机构列表)
                    {
                        if (item.已填写完整 == false)
                        {
                            ViewBag.servicedepart = false;
                            break;
                        }
                    }
                }
            }
            else
            {
                ViewBag.servicedepart = false;
            }
            if (Newmodel.供应商用户信息.U盾信息 == null || string.IsNullOrWhiteSpace(Newmodel.供应商用户信息.U盾信息.加密参数) || string.IsNullOrWhiteSpace(Newmodel.供应商用户信息.U盾信息.序列号))
            {
                ViewData["U_message"] = "您还没有<a href='/jct/ApplyVip' style='text-decoration:underline; font-weight:bold;' target='_blank'>申请安全认证和办理U盾</a>";
            }
            ViewData["msg_count"] = count;
            ViewData["user"] = Newmodel.登录信息.登录名;

            var BjCount = 网上竞标管理.查询网上竞标(0, 0, Query<网上竞标>.Where(o => o.所属行业 == (currentUser.企业基本信息.所属行业 != null ? currentUser.企业基本信息.所属行业.Replace(";", "") : "") && o.报价结束时间 > DateTime.Now)).Count();
            ViewData["可报价数量"] = BjCount;

            //验收单回传单
            var ysdpic = 验收单管理.查询验收单(0, 0, Query<验收单>.Where(o => !o.是否作废 && o.审核数据.审核状态 == 审核状态.审核通过 && o.是否已经打印 && o.供应商链接.用户ID == currentUser.Id));
            ViewData["回传单不合格数量"] = ysdpic.Where(o => o.扫描件审核状态 == "审核未通过").Count();


            return View(Newmodel);
        }

        public ActionResult AddGoods()
        {
            //判断是否能使用批量上传工具
            ViewData["批量上传"] = "0";
            if (增值服务记录 != null && 增值服务记录.已开通的服务.Any())
            {
                var 批量上传 = 增值服务记录.已开通的服务.Where(o => (o.所申请项目名.Contains("标准会员") && o.结束时间 > DateTime.Now) || (o.所申请项目名.Contains("商务会员") && o.结束时间 > DateTime.Now) || (o.所申请项目名.Contains("批量上传商品") && o.结束时间 > DateTime.Now));
                if (批量上传.Any())
                {
                    ViewData["批量上传"] = "1";
                }
            }

            return View();
        }

        [HttpPost]
        public string GetImg()
        {
            return "";
        }
        public ActionResult Part_Home()
        {
            供应商 Newmodel = 用户管理.查找用户<供应商>(currentUser.Id);
            IEnumerable<站内消息> Msg = 站内消息管理.查询收信人的站内消息(0, 0, Newmodel.Id);
            int count = 0;
            foreach (var item in Msg)
            {
                if (item.基本数据.添加时间 > item.收信人.上次阅读时间 && item.发起者.用户数据 != null)
                {
                    count++;
                }
            }
            ViewBag.Toubiao = 招标采购预报名管理.计数招标采购预报名(0, 0, Query<招标采购预报名>.EQ(o => o.预报名已关闭, false));
            ViewBag.GoodCount = 商品管理.计数供应商商品(Newmodel.Id);
            ViewBag.cls = Newmodel.信用评级信息.等级;
            ViewBag.BaseInfo = Newmodel.企业基本信息.已填写完整;
            ViewBag.VipInfo = Newmodel.企业联系人信息.已填写完整;
            ViewBag.SalesInfo = Newmodel.工商注册信息.已填写完整;
            ViewBag.Busniss = Newmodel.营业执照信息.已填写完整;
            ViewBag.TaxInfo = Newmodel.税务信息.已填写完整;
            ViewBag.LawPerson = Newmodel.法定代表人信息.已填写完整;
            ViewBag.goodType = Newmodel.可提供产品类别列表.Count();
            ViewBag.PrintMessage = (Newmodel.审核数据.审核状态 == 审核状态.审核通过 && !Newmodel.供应商用户信息.符合入库标准) ? "由于您所提供的资料还不符合申请入库的标准，故不能打印入库申请表格。如有任何疑问，请拨打网站客服电话咨询，谢谢。" : "";
            if (Newmodel.出资人或股东信息 != null)
            {
                if (Newmodel.出资人或股东信息.Count() == 0)
                {
                    ViewBag.Investor = false;
                }
                else
                {
                    foreach (var item in Newmodel.出资人或股东信息)
                    {
                        if (item.已填写完整 == false)
                        {
                            ViewBag.Investor = false;
                            break;
                        }
                        else
                        {
                            ViewBag.Investor = true;
                        }
                    }
                }
            }
            else
            {
                ViewBag.Investor = false;
            }


            if (Newmodel.财务状况信息 != null)
            {
                if (Newmodel.财务状况信息.Count() == 0)
                {
                    ViewBag.Financial = false;
                }
                else
                {
                    foreach (var item in Newmodel.财务状况信息)
                    {
                        if (item.已填写完整 == false)
                        {
                            ViewBag.Financial = false;
                            break;
                        }
                        else
                        {
                            ViewBag.Financial = true;
                        }
                    }
                }
            }
            else
            {
                ViewBag.Financial = false;
            }
            if (Newmodel.资质证书列表 != null)
            {
                if (Newmodel.资质证书列表.Count() == 0)
                {
                    ViewBag.qualify = false;
                }
                else
                {
                    foreach (var item in Newmodel.资质证书列表)
                    {
                        if (item.已填写完整 == false)
                        {
                            ViewBag.qualify = false;
                            break;
                        }
                    }
                }
            }
            else
            {
                ViewBag.qualify = false;
            }
            if (Newmodel.售后服务机构列表 != null)
            {
                if (Newmodel.售后服务机构列表.Count() == 0)
                {
                    ViewBag.servicedepart = false;
                }
                else
                {
                    foreach (var item in Newmodel.售后服务机构列表)
                    {
                        if (item.已填写完整 == false)
                        {
                            ViewBag.servicedepart = false;
                            break;
                        }
                    }
                }
            }
            else
            {
                ViewBag.servicedepart = false;
            }
            if (Newmodel.供应商用户信息.U盾信息 == null || string.IsNullOrWhiteSpace(Newmodel.供应商用户信息.U盾信息.加密参数) || string.IsNullOrWhiteSpace(Newmodel.供应商用户信息.U盾信息.序列号))
            {
                ViewData["U_message"] = "您还没有<a href='/jct/ApplyVip' style='text-decoration:underline; font-weight:bold;' target='_blank'>申请安全认证和办理U盾</a>";
            }
            ViewData["msg_count"] = count;
            ViewData["user"] = Newmodel.登录信息.登录名;

            var BjCount = 网上竞标管理.查询网上竞标(0, 0, Query<网上竞标>.Where(o => o.所属行业 == (currentUser.企业基本信息.所属行业 != null ? currentUser.企业基本信息.所属行业.Replace(";", "") : "") && o.报价结束时间 > DateTime.Now)).Count();
            ViewData["可报价数量"] = BjCount;
            return PartialView("Gys_Part/Part_Home", Newmodel);
        }
        public ActionResult Part_Gys_TreeMenu()
        {

            return PartialView("Gys_Part/Part_Gys_TreeMenu");
        }
        public ActionResult ChoosePicture()
        {
            try
            {
                bool exist = false;
                string name = Request.QueryString["name"];
                foreach (var item in 扣费规则.规则列表)
                {
                    if (name == item.扣费项目名)
                    {
                        exist = true;
                        break;
                    }
                }
                if (exist)
                {
                    List<string> ser = new List<string>();
                    供应商 model = 用户管理.查找用户<供应商>(currentUser.Id);
                    IEnumerable<供应商服务记录> service = 供应商服务记录管理.查询供应商服务记录(0, 0, Query<供应商服务记录>.Where(o => o.所属供应商.用户ID == currentUser.Id));
                    if (model.供应商用户信息.广告商品 != null && model.供应商用户信息.广告商品.Count() != 0)
                    {
                        if (model.供应商用户信息.广告商品.ContainsKey(name))
                        {
                            List<供应商._供应商用户信息._广告商品> ad = model.供应商用户信息.广告商品.Where(o => o.Key == name).First().Value;
                            ViewData["pics"] = ad;
                        }
                    }
                    else
                    {
                        ViewData["pics"] = "";
                    }
                    if (service != null && service.Count() != 0)
                    {
                        供应商服务记录 record = service.First();
                        if (record.申请中的服务 != null && record.申请中的服务.Count != 0)
                        {
                            foreach (var item in record.申请中的服务)
                            {
                                ser.Add(item.所申请项目名);
                            }
                        }
                        if (record.已开通的服务 != null && record.已开通的服务.Count != 0)
                        {
                            foreach (var m in record.已开通的服务)
                            {
                                ser.Add(m.所申请项目名);
                            }
                        }
                    }
                    if (ser.Count != 0)
                    {
                        if (ser.Contains("标准会员"))
                        {
                            ViewData["name"] = name;
                            ViewData["type"] = "standard";
                            ViewData["id"] = currentUser.Id;
                            return View();
                        }
                        else if (ser.Contains("商务会员"))
                        {
                            ViewData["name"] = name;
                            ViewData["type"] = "business";
                            ViewData["id"] = currentUser.Id;
                            return View();
                        }
                        else if (ser.Contains("企业推广服务B1-1位置"))
                        {
                            ViewData["name"] = name;
                            ViewData["type"] = "static";
                            ViewData["id"] = currentUser.Id;
                            return View();
                        }
                        else if (ser.Contains("企业推广服务B1-2位置"))
                        {
                            ViewData["name"] = name;
                            ViewData["type"] = "dynamic";
                            ViewData["id"] = currentUser.Id;
                            return View();
                        }
                        else
                        {
                            return Content("<script>alert('没有这项服务！');window.location='/供应商后台/';</script>");
                        }
                    }
                    else
                    {
                        return Content("<script>alert('没有这项服务！');window.location='/供应商后台/';</script>");
                    }
                }
                else
                {
                    return Redirect("/供应商后台/");
                }
            }
            catch
            {
                return Redirect("/供应商后台/");
            }
        }
        [登录验证]
        public ActionResult Showproduct()
        {
            try
            {
                bool exist = false;
                ViewData["pics"] = new List<供应商._供应商用户信息._广告商品>();
                string name = Request.QueryString["name"];
                ViewData["name"] = name;
                foreach (var m in 扣费规则.规则列表)
                {
                    if (m.扣费项目名 == name)
                    {
                        exist = true;
                        break;
                    }
                }
                if (exist)
                {
                    供应商 model = 用户管理.查找用户<供应商>(currentUser.Id);
                    if (model.供应商用户信息.广告商品 != null && model.供应商用户信息.广告商品.Count() != 0)
                    {
                        foreach (var item in model.供应商用户信息.广告商品)
                        {
                            if (item.Key == name)
                            {
                                ViewData["pics"] = item.Value;
                                break;
                            }
                        }
                    }
                    ViewData["id"] = currentUser.Id;
                    return View();
                }
                else
                {
                    return Redirect("/供应商后台/");
                }
            }
            catch
            {
                return Redirect("/供应商后台/");
            }
        }
        public class Goods
        {
            public long Id { get; set; }
            public string name { get; set; }
            public string Introduction { get; set; }
            public string Price { get; set; }
            public string Picture { get; set; }
        }
        public ActionResult GoodList()
        {
            try
            {
                List<Goods> gs = new List<Goods>();
                string name = "";
                long id = long.Parse(Request.QueryString["id"]);
                int CurrentPage = int.Parse(Request.QueryString["p"]);
                if(!string.IsNullOrWhiteSpace(Request.QueryString["name"]))
                {
                    name=Request.QueryString["name"];
                }
                long PageCount = 商品管理.计数供应商商品(id, 0, 0, Query.EQ("商品信息.商品名", new BsonRegularExpression(string.Format("/{0}/i", name))).And(Query.EQ("审核数据.审核状态", 审核状态.审核通过))) / 20;
                if (商品管理.计数供应商商品(id, 0, 0, Query.EQ("商品信息.商品名", new BsonRegularExpression(string.Format("/{0}/i", name))).And(Query.EQ("审核数据.审核状态", 审核状态.审核通过))) % 20 > 0)
                {
                    PageCount++;
                }
                IEnumerable<商品> goods = 商品管理.查询供应商商品(id, 20 * (CurrentPage - 1), 20, Query.EQ("商品信息.商品名", new BsonRegularExpression(string.Format("/{0}/i", name))).And(Query.EQ("审核数据.审核状态", 审核状态.审核通过)));
                if (goods != null)
                {
                    foreach (var item in goods)
                    {
                        Goods g = new Goods();
                        g.Id = item.Id;
                        g.Price = string.Format("{0:0.00}", item.销售信息.价格);
                        g.Introduction = item.商品数据.商品简介;
                        g.name = item.商品信息.商品名;
                        if (item.商品信息.商品图片 != null && item.商品信息.商品图片.Count != 0)
                        {
                            g.Picture = item.商品信息.商品图片[0];
                        }
                        else
                        {
                            g.Picture = "";
                        }
                        gs.Add(g);
                    }
                }
                JsonResult json = new JsonResult() { Data = new { Newgood = gs, Pcount = PageCount, Cpage = CurrentPage } };
                return Json(json, JsonRequestBehavior.AllowGet);
            }
            catch
            {
                JsonResult json = new JsonResult() { Data = new { Newgood = new List<Goods>(), Pcount = 1, Cpage = 1 } };
                return Json(json, JsonRequestBehavior.AllowGet);
            }
        }
        public int SavePoint()
        {
            try
            {
                供应商 Newmodel = 用户管理.查找用户<供应商>(currentUser.Id);
                string x = Request.QueryString["x"];
                string y = Request.QueryString["y"];
                Newmodel.企业基本信息.地图坐标 = x + "," + y;
                用户管理.更新用户<供应商>(Newmodel);
                return 1;
            }
            catch
            {
                return 0;
            }
        }
        public int DeletePic()
        {
            try
            {
                string uri = Request.QueryString["uri"];
                if (System.IO.File.Exists(Server.MapPath(@uri)))
                {
                    System.IO.File.Delete(Server.MapPath(@uri));
                    return 1;
                }
                else
                {
                    return 0;
                }
            }
            catch
            {
                return 0;
            }
        }
        [HttpPost]
        public ActionResult SavePicture()
        {
            try
            {
                string path = "";
                string path1 = "";
                供应商 model = 用户管理.查找用户<供应商>(currentUser.Id);
                string name = Request.Form["pic1"].ToString();
                for (int i = 0; i < Request.Files.Count; i++)
                {
                    HttpPostedFileBase file = Request.Files[i];
                    if (file != null && (file.ContentType == "image/jpeg" || file.ContentType == "image/pjpeg" || file.ContentType == "image/x-png" || file.ContentType == "image/png") && (((file.ContentLength / 1024) / 1024) < 2))
                    {
                        if (name != "ysd")
                        {
                            string filePath = 上传管理.获取上传路径<供应商>(媒体类型.图片, 路径类型.不带域名根路径);
                            string fname = UploadAttachment(file);
                            path += filePath + fname + "|";
                            path1 = filePath + fname;
                            if (name == "showPic")
                            {
                                model.供应商用户信息.供应商图片.Add(filePath + fname);
                            }
                            else if(name=="photo")
                            {
                                if (System.IO.File.Exists(Server.MapPath(@model.登录信息.头像)))
                                {
                                    System.IO.File.Delete(Server.MapPath(@model.登录信息.头像));
                                }
                                model.登录信息.头像 = filePath + fname;
                            }
                            else if (name == "lawperson")
                            {
                                if (System.IO.File.Exists(Server.MapPath(@model.法定代表人信息.法定代表人身份证电子扫描件)))
                                {
                                    System.IO.File.Delete(Server.MapPath(@model.法定代表人信息.法定代表人身份证电子扫描件));
                                }
                                model.法定代表人信息.法定代表人身份证电子扫描件 = path1;
                            }
                            else if (name == "taxRegister")
                            {
                                if (System.IO.File.Exists(Server.MapPath(@model.营业执照信息.税务登记证电子扫描件)))
                                {
                                    System.IO.File.Delete(Server.MapPath(@model.营业执照信息.税务登记证电子扫描件));
                                }
                                model.营业执照信息.税务登记证电子扫描件 = path1;
                            }
                            else if (name == "tax")
                            {
                                if (System.IO.File.Exists(Server.MapPath(@model.税务信息.近3年完税证明电子扫描件)))
                                {
                                    System.IO.File.Delete(Server.MapPath(@model.税务信息.近3年完税证明电子扫描件));
                                }
                                model.税务信息.近3年完税证明电子扫描件 = path1;
                            }
                            else if (name == "orgnize")
                            {
                                if (System.IO.File.Exists(Server.MapPath(@model.工商注册信息.组织机构代码证电子扫描件)))
                                {
                                    System.IO.File.Delete(Server.MapPath(@model.工商注册信息.组织机构代码证电子扫描件));
                                }
                                model.工商注册信息.组织机构代码证电子扫描件 = path1;
                            }
                            else if (name == "bank")
                            {
                                if (System.IO.File.Exists(Server.MapPath(@model.工商注册信息.基本账户开户银行资信证明电子扫描件)))
                                {
                                    System.IO.File.Delete(Server.MapPath(@model.工商注册信息.基本账户开户银行资信证明电子扫描件));
                                }
                                model.工商注册信息.基本账户开户银行资信证明电子扫描件 = path1;
                            }
                            else if (name == "social")
                            {
                                if (System.IO.File.Exists(Server.MapPath(@model.工商注册信息.近3年缴纳社会保证金证明电子扫描件)))
                                {
                                    System.IO.File.Delete(Server.MapPath(@model.工商注册信息.近3年缴纳社会保证金证明电子扫描件));
                                }
                                model.工商注册信息.近3年缴纳社会保证金证明电子扫描件 = path1;
                            }
                            else if (name == "illegal")
                            {
                                if (System.IO.File.Exists(Server.MapPath(@model.工商注册信息.近3年有无重大违法记录电子扫描件)))
                                {
                                    System.IO.File.Delete(Server.MapPath(@model.工商注册信息.近3年有无重大违法记录电子扫描件));
                                }
                                model.工商注册信息.近3年有无重大违法记录电子扫描件 = path1;
                            }
                            else if (name == "sale")
                            {
                                if (System.IO.File.Exists(Server.MapPath(@model.营业执照信息.营业执照电子扫描件)))
                                {
                                    System.IO.File.Delete(Server.MapPath(@model.营业执照信息.营业执照电子扫描件));
                                }
                                model.营业执照信息.营业执照电子扫描件 = path1;
                            }
                            else if (name == "contract")
                            {
                                IEnumerable<订购合同上传记录> record = 订购合同上传记录管理.查询订购合同上传记录(0, 0, Query<订购合同上传记录>.Where(o => o.所属供应商.用户ID == currentUser.Id));
                                if (record != null && record.Count() != 0)
                                {
                                    订购合同上传记录 contract = record.Last();
                                    contract.合同.合同路径.Add(path1);
                                    订购合同上传记录管理.更新订购合同上传记录(contract);
                                }
                            }
                            else
                            {
                                string[] str = name.Split(',');
                                int index = int.Parse(str[1]);
                                model.资质证书列表[index].资质证书电子扫描件[0].路径 = path1;
                            }
                        }
                        else
                        {
                            string filePath = 上传管理.获取上传路径<验收单>(媒体类型.附件, 路径类型.不带域名根路径);
                            string fname = UploadImages_Ysd(file);
                            path += filePath + fname + "|";
                            path1 = filePath + fname;
                        }
                    }
                    else
                    {
                        ViewData["path"] = "上传失败";
                        return View();
                    }
                }
                if (name == "showPic"||name=="ysd")
                {
                    ViewBag.type = "2";
                }
                else
                { ViewBag.type = "1"; }
                ViewBag.photo = path1;
                用户管理.更新用户<供应商>(model);
                deleteIndex("/Lucene.Net/IndexDic/Gys", model.Id.ToString());
                //deleteIndex_ProductCatalog("/Lucene.Net/IndexDic/GysCatalog", model.企业基本信息.企业名称);
                ViewData["path"] = path;
                return View();
            }
            catch
            {
                ViewData["path"] = "上传出错!|";
                return View();
            }
        }
        public ActionResult VipManagement()
        {
            IEnumerable<供应商增值服务申请记录> model = 供应商增值服务申请记录管理.查询供应商增值服务申请记录(0, 0, Query<供应商增值服务申请记录>.Where(o => o.所属供应商.用户ID == currentUser.Id));
            return View(model);
        }
        public ActionResult Delete_Toubiao()
        {
            try
            {
                供应商 model = 用户管理.查找用户<供应商>(currentUser.Id);
                int index = int.Parse(Request.QueryString["index"]);
                model.历史参标记录.RemoveAt(index);
                用户管理.更新用户<供应商>(model);
                deleteIndex("/Lucene.Net/IndexDic/Gys", model.Id.ToString());
                //deleteIndex_ProductCatalog("/Lucene.Net/IndexDic/GysCatalog", model.企业基本信息.企业名称);
                return Redirect("/供应商后台/Toubiao");
            }
            catch
            {
                return Redirect("/供应商后台/Toubiao");
            }
        }
        [HttpPost]
        public ActionResult Toubiao_Manage(供应商 m)
        {
            供应商 model = 用户管理.查找用户<供应商>(currentUser.Id);
            model.历史参标记录.Add(m.历史参标记录[0]);
            用户管理.更新用户<供应商>(model);
            deleteIndex("/Lucene.Net/IndexDic/Gys", model.Id.ToString());
            //deleteIndex_ProductCatalog("/Lucene.Net/IndexDic/GysCatalog", model.企业基本信息.企业名称);
            return Content("<script>if(confirm('请确保招投标信息已完善，点击确定请等待审核，点击取消继续完善。')){window.location='/供应商后台/Gys_Product_Type';} else{window.location='/供应商后台/toubiao';}</script>");
        }
        public ActionResult Modify_Toubiao()
        {
            try
            {
                int index = int.Parse(Request.QueryString["index"]);
                供应商 Newmodel = 用户管理.查找用户<供应商>(currentUser.Id);
                if (index > Newmodel.历史参标记录.Count() || index < 0)
                {
                    return Redirect("/供应商后台/Toubiao");
                }
                ViewData["index"] = index;
                return View(Newmodel);
            }
            catch
            {
                return Redirect("/供应商后台/Toubiao");
            }
        }
        [HttpPost]
        public ActionResult Modify_toubiao(供应商 model)
        {
            int index = int.Parse(Request.Form["index"].ToString());
            供应商 Newmodel = 用户管理.查找用户<供应商>(currentUser.Id);
            if (index > Newmodel.历史参标记录.Count() || index < 0)
            {
                return Redirect("/供应商后台/Toubiao");
            }
            Newmodel.历史参标记录[index] = model.历史参标记录[0];
            用户管理.更新用户<供应商>(Newmodel);
            deleteIndex("/Lucene.Net/IndexDic/Gys", model.Id.ToString());
            //deleteIndex_ProductCatalog("/Lucene.Net/IndexDic/GysCatalog", model.企业基本信息.企业名称);
            return Redirect("/供应商后台/Toubiao");
        }
        public ActionResult Toubiao()
        {
            供应商 model = 用户管理.查找用户<供应商>(currentUser.Id);
            return View(model);
        }
        public ActionResult Notice()
        {
            return View();
        }
        public class subMsg
        {
            public int 固定可订阅数 { get; set; }
            public int 已购买订阅数 { get; set; }
            public int 总计可订阅数 { get; set; }
            public 供应商._消息订阅信息 m { get; set; }
        }
        [HttpPost]
        public ActionResult Save_Bk_Msg(subMsg model)
        {
            供应商 Newmodel = 用户管理.查找用户<供应商>(currentUser.Id);
            for (int i = 0; i < model.m.订阅列表.Count; i++)
            {
                if (model.m.订阅列表[i].三级分类 == "没有三级分类")
                {
                    model.m.订阅列表[i].三级分类 = "";
                }
            }
            Newmodel.消息订阅信息 = model.m;
            Newmodel.已购买订阅数 = model.已购买订阅数;
            //currentUser.更新订阅信息();
            用户管理.更新用户<供应商>(Newmodel);
            return Redirect("/供应商后台/Book_Msg");
        }
        public ActionResult Del_Msg()
        {
            try
            {
                int index = int.Parse(Request.QueryString["index"].ToString());
                供应商 Newmodel = 用户管理.查找用户<供应商>(currentUser.Id);
                Newmodel.消息订阅信息.订阅列表.RemoveAt(index);
                用户管理.更新用户<供应商>(Newmodel);
                return Redirect("/供应商后台/Book_Msg");
            }
            catch
            {
                return Redirect("/供应商后台/Book_Msg");
            }

        }
        public ActionResult Part_Book_Msg()
        {
            var model = new subMsg()
            {
                固定可订阅数 = currentUser.固定可订阅数,
                已购买订阅数 = currentUser.已购买订阅数,
                总计可订阅数 = currentUser.总计可订阅数,
                m = currentUser.消息订阅信息
            };
            ViewData["一级分类"] = 商品分类管理.查找子分类();
            if ((int)currentUser.供应商用户信息.认证级别 >= 4)
            {
                ViewBag.IsJunCaiT = true;
            }
            return PartialView("Gys_Part/Part_Book_Msg", model);
        }
        public ActionResult typeWithNum()
        {
            List<string> type1 = new List<string>();
            IEnumerable<商品分类> goods = 商品分类管理.查找子分类();
            if (goods != null)
            {
                for (int i = 0; i < goods.Count(); i++)
                {
                    type1.Add(goods.ElementAt(i).分类名);
                }
            }
            JsonResult json = new JsonResult() { Data = type1 };
            return Json(json, JsonRequestBehavior.AllowGet);
        }
        public ActionResult types()
        {
            string name = Request.QueryString["n"].ToString();
            List<string> type1 = new List<string>();
            IEnumerable<商品分类> goods = 商品分类管理.查找子分类();
            if (goods != null)
            {
                for (int i = 0; i < goods.Count(); i++)
                {
                    if (goods.ElementAt(i).分类名 == name)
                    {
                        IEnumerable<商品分类> SecondGoods = goods.ElementAt(i).子分类;
                        if (SecondGoods != null)
                        {
                            for (int j = 0; j < SecondGoods.Count(); j++)
                            {
                                type1.Add(SecondGoods.ElementAt(j).分类名);
                            }
                        }
                    }
                }
            }
            JsonResult json = new JsonResult() { Data = type1 };
            return Json(json, JsonRequestBehavior.AllowGet);
        }
        public ActionResult types1()
        {
            string cls = Request.QueryString["cls"].ToString();
            string name = Request.QueryString["n"].ToString();
            List<string> type1 = new List<string>();
            IEnumerable<商品分类> ThirdCls = null;
            IEnumerable<商品分类> SecondCls = null;
            IEnumerable<商品分类> goods = 商品分类管理.查找子分类();
            if (goods != null)
            {
                for (int i = 0; i < goods.Count(); i++)
                {
                    if (goods.ElementAt(i).分类名 == cls)
                    {
                        SecondCls = goods.ElementAt(i).子分类;
                        break;
                    }
                }
            }
            if (SecondCls != null)
            {
                for (int j = 0; j < SecondCls.Count(); j++)
                {
                    if (name == SecondCls.ElementAt(j).分类名)
                    {
                        ThirdCls = SecondCls.ElementAt(j).子分类;
                        break;
                    }
                }
            }
            if (ThirdCls != null)
            {
                for (int k = 0; k < ThirdCls.Count(); k++)
                {
                    type1.Add(ThirdCls.ElementAt(k).分类名);
                }
            }
            if (type1.Count() == 0)
            {
                type1.Add("没有三级分类");
            }
            JsonResult json = new JsonResult() { Data = type1 };
            return Json(json, JsonRequestBehavior.AllowGet);
        }
        public ActionResult Print_Detail()
        {
            供应商 Newmodel = 用户管理.查找用户<供应商>(currentUser.Id);
            if (Newmodel.审核数据.审核状态 == 审核状态.未审核)
            {
                return Content("<script>alert('请等待审核，审核通过后才能打印资料');location.href='javascript:history.go(-1)';</script>");
            }
            else if (!Newmodel.供应商用户信息.符合入库标准)
            {
                return Content("<script>alert('由于您所提供的资料还不符合申请入库的标准，故不能打印入库申请表格。如有任何疑问，请拨打网站客服电话咨询，谢谢。');location.href='javascript:history.go(-1)';</script>");
            }
            //供应商 model = (供应商)currentUser;
            return View(Newmodel);
        }
        public ActionResult NoticeAboutApply()
        {
            return View();
        }

        public ActionResult AccountInfoManage()
        {
            //var 已开通服务列表 = new List<供应商计费项目扣费记录>();

            var list = 供应商服务记录管理.查询供应商服务记录(0, 0, Query<供应商服务记录>.Where(o => o.所属供应商.用户ID == currentUser.Id));
            var _t = new 供应商服务记录();
            if (list.Count() > 0)
            {
                _t = list.First();
            }
            else
            {
                _t.所属供应商.用户ID = currentUser.Id;
                foreach (var j in 扣费规则.规则列表)
                {
                    var b = new 供应商增值服务申请记录();
                    b.所申请项目名 = j.扣费项目名;
                    b.所属供应商.用户ID = currentUser.Id;
                    _t.可申请的服务.Add(b);
                }
                供应商服务记录管理.添加供应商服务记录(_t);
            }
            
            ViewData["供应商服务"] = _t;
            ViewData["会员级别"] = currentUser.供应商用户信息.认证级别;
            ViewData["已申请服务"] = _t.申请中的服务;
            ViewData["已开通服务"] = _t.已开通的服务;


            var 余额 = 供应商充值余额管理.查询供应商充值余额(0, 0, Query<供应商充值余额>.Where(o => o.所属供应商.用户ID == currentUser.Id));
            var 供应商账户余额 = new 供应商充值余额();
            if (余额.Count() > 0)
            {
                供应商账户余额 = 余额.First();
            }
            else
            {
                供应商账户余额.所属供应商.用户ID = currentUser.Id;
                供应商账户余额.可用余额 = 0;
                供应商充值余额管理.添加供应商充值余额(供应商账户余额);
            }

            return View(供应商账户余额);
        }

        public ActionResult ValueAddedService()
        {
            var t = 供应商服务记录管理.查询供应商服务记录(0, 0, Query<供应商服务记录>.Where(o => o.所属供应商.用户ID == currentUser.Id));
            var 申请记录列表 = t.Count() > 0 ? t.First() : new 供应商服务记录();
            var list = new List<Tuple<string, 扣费类型, decimal, 通过状态, int>>();
            //var model = new List<供应商增值服务申请记录>();
            if (申请记录列表 != null)
            {
                if (申请记录列表.申请中的服务.Count == 0)
                {
                    return Content("<script>alert('您还没有审核中的服务');window.location='/供应商后台/';</script>");
                }
                else
                {
                    foreach (var k in 申请记录列表.申请中的服务)
                    {
                        var m = 扣费规则.规则列表.Where(o => o.扣费项目名 == k.所申请项目名).First();
                        //var p = 供应商计费项目扣费记录管理.查询供应商计费项目扣费记录(0, 0, Query<供应商计费项目扣费记录>.Where(o => o.扣费服务项目 == k.所申请项目名), false, SortBy.Descending("基本数据.添加时间")).First();
                        list.Add(Tuple.Create(k.所申请项目名, m.扣费类型, m.扣费金额, k.是否通过, k.服务期限));
                        // model.Add(k);
                    }
                    ViewData["扣费项目"] = list;
                    return View();
                }
            }
            else
            {
                return Content("<script>alert('您还没有审核中的服务');window.location='/供应商后台/';</script>");
            }
        }

        public int OpenAddValue()
        {
            var kd = Request.Params["str"];//待开通服务及其时长

            var kf = new List<供应商计费项目扣费记录>();
            var 余额 = 供应商充值余额管理.查询供应商充值余额(0, 0, Query<供应商充值余额>.Where(o => o.所属供应商.用户ID == currentUser.Id)).First();
            var arr = kd.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var jk in arr)
            {
                var 开通服务名 = jk.Split(':')[0];
                var 开通时长 = jk.Split(':')[1];
                var account = 扣费规则.规则列表.Where(o => o.扣费项目名 == 开通服务名).First();

                var kfj = new 供应商计费项目扣费记录();
                kfj.所属供应商.用户ID = currentUser.Id;
                kfj.扣费服务项目 = 开通服务名;
                kfj.扣费金额 = account.扣费金额;
                kfj.扣费类型 = account.扣费类型;
                kfj.扣费时间 = DateTime.Now;
                if (account.扣费类型 == 扣费类型.按年扣费) { kfj.所属年 = int.Parse(开通时长); }
                if (account.扣费类型 == 扣费类型.按月扣费) { kfj.所属月 = int.Parse(开通时长); }
                if (account.扣费类型 == 扣费类型.按次扣费) { kfj.所属日 = int.Parse(开通时长); }
                kf.Add(kfj);
            }
            var 待扣费金额 = kf.Where(o => o.扣费类型 == 扣费类型.按年扣费).Sum(o => o.扣费金额 * o.所属年);
            待扣费金额 += kf.Where(o => o.扣费类型 == 扣费类型.按月扣费).Sum(o => o.扣费金额 * o.所属月);
            待扣费金额 += kf.Where(o => o.扣费类型 == 扣费类型.按次扣费).Sum(o => o.扣费金额 * o.所属日);

            if (余额.可用余额 < 待扣费金额)
            {
                return 0;//开通失败
            }
            else
            {
                余额.可用余额 -= 待扣费金额;
                var 服务 = new 供应商服务记录();
                var 供应商服务 = 供应商服务记录管理.查询供应商服务记录(0, 0, Query<供应商服务记录>.Where(o => o.所属供应商.用户ID == currentUser.Id));
                if (供应商服务.Count() > 0)
                {
                    服务 = 供应商服务.First();
                }
                else
                {
                    服务.所属供应商.用户ID = currentUser.Id;
                    foreach (var j in 扣费规则.规则列表)
                    {
                        var b = new 供应商增值服务申请记录();
                        b.所申请项目名 = j.扣费项目名;
                        b.所属供应商.用户ID = currentUser.Id;
                        服务.可申请的服务.Add(b);
                    }
                    供应商服务记录管理.添加供应商服务记录(服务);
                }
                foreach (var m in kf)
                {
                    供应商计费项目扣费记录管理.添加供应商计费项目扣费记录(m);
                    var p = 服务.可申请的服务.Find(o => o.所申请项目名 == m.扣费服务项目);
                    服务.可申请的服务.Remove(p);
                    if (m.扣费类型 == 扣费类型.按年扣费) { p.服务期限 = m.所属年; }
                    if (m.扣费类型 == 扣费类型.按月扣费) { p.服务期限 = m.所属月; }
                    if (m.扣费类型 == 扣费类型.按次扣费) { p.服务期限 = m.所属日; }
                    p.是否通过 = 通过状态.通过;
                    if (m.扣费服务项目.Contains("企业主页商品展示"))
                    {
                        var qyzsck = 服务.已开通的服务.Find(o => o.所申请项目名.Contains("企业主页商品展示"));
                        if (qyzsck != null)
                        {
                            服务.已开通的服务.Remove(qyzsck);
                            服务.可申请的服务.Add(qyzsck);
                        }
                    }
                    if (m.扣费服务项目.Contains("企业推广服务B1"))
                    {
                        var qytgfw = 服务.已开通的服务.Find(o => o.所申请项目名.Contains("企业推广服务B1"));
                        if (qytgfw != null)
                        {
                            服务.已开通的服务.Remove(qytgfw);
                            服务.可申请的服务.Add(qytgfw);
                        }
                    }
                    if (m.扣费服务项目.Contains("标准会员"))
                    {
                        var rzhy = 服务.已开通的服务.Find(o => o.所申请项目名.Contains("商务会员"));
                        if (rzhy != null)
                        {
                            服务.已开通的服务.Remove(rzhy);
                            服务.可申请的服务.Add(rzhy);
                        }
                    }
                    if (m.扣费服务项目.Contains("商务会员"))
                    {
                        var swhy = 服务.已开通的服务.Find(o => o.所申请项目名.Contains("标准会员"));
                        if (swhy != null)
                        {
                            服务.已开通的服务.Remove(swhy);
                            服务.可申请的服务.Add(swhy);
                        }
                    }
                    服务.已开通的服务.Add(p);
                }
                供应商服务记录管理.更新供应商服务记录(服务);
                供应商充值余额管理.更新供应商充值余额(余额);
                return 1;//开通成功并扣费
            }
        }

        public ActionResult OnlineBiddingManage()
        {
            var wsjb = 网上竞标管理.查询网上竞标(0, 0, Query<网上竞标>.Where(o => o.报价结束时间 < DateTime.Now));
            var jb = new List<网上竞标>();
            foreach (var k in wsjb) 
            {
                var t = k.报价供应商列表.Find(o => o.报价供应商.用户ID == currentUser.Id);
                if (t != null)
                {
                    jb.Add(k);
                }
            }
            return View(jb);
        }
        public ActionResult OnlineBiddingDetail()
        {
            var id = Request.Params["id"]; //报价项目ID
            var bj = 网上竞标管理.查找网上竞标(long.Parse(id));
            ViewData["供应商ID"] = currentUser.Id;
            return View(bj);
        }
        public ActionResult OnlineBiddingProject()
        {
            var jbgg = new List<网上竞标>();
            var comes = Request.Params["comes"];
            if (comes == "y")
            {
                jbgg = 网上竞标管理.查询网上竞标(0, 0, Query<网上竞标>.Where(o => o.报价结束时间 < DateTime.Now && o.所属行业 == currentUser.企业基本信息.所属行业.Replace(";", ""))).ToList();
            }
            else if (comes == "w")
            {
                jbgg = 网上竞标管理.查询网上竞标(0, 0, Query<网上竞标>.Where(o => o.报价结束时间 > DateTime.Now && o.所属行业 == currentUser.企业基本信息.所属行业.Replace(";", ""))).ToList();
            }
            else
            {
                jbgg = 网上竞标管理.查询网上竞标(0, 0, Query<网上竞标>.Where(o => o.所属行业 == currentUser.企业基本信息.所属行业.Replace(";", ""))).ToList();
            }
            return View(jbgg);
        }
        public ActionResult OnlineBiddingProjectDetail()
        {
            var id = Request.Params["a"];//网上竞标ID
            var zbxm = 网上竞标管理.查找网上竞标(long.Parse(id));
            ViewData["供应商ID"] = currentUser.Id;
            return View(zbxm);
        }

        public ActionResult OnlineBiddingHistory()
        {
            var jblist=网上竞标管理.查询网上竞标(0,0,Query<网上竞标>.Where(o=>o.报价结束时间 < DateTime.Now && o.所属行业 == currentUser.企业基本信息.所属行业.Replace(";","")));
            return View(jblist);
        }

        public string GysQuote()
        {
            var id=Request.Params["id"];//报价项目ID
            var price=Request.Params["price"];//供应商报价
            var notes = Request.Params["notes"];//报价备注
            var tax = Request.Params["tax"];//税率
            var other = Request.Params["other"];//其他费用
            var frei = Request.Params["frei"];//供应商报价
            var shfu = Request.Params["shfu"];//售后服务
            var saddress=Request.Params["sadress"];//发货地点
            var bj = 网上竞标管理.查找网上竞标(long.Parse(id));
            var ybj = bj.报价供应商列表.Find(o => o.报价供应商.用户ID == currentUser.Id);
            if (ybj != null)
            {
                //if (ybj.报价 < decimal.Parse(price) || bj.起始价格<decimal.Parse(price))
                //{
                //    return "您的本次报价高于起始价格或您的上一次报价，请重新输入！";
                //}
                ybj.报价 = decimal.Parse(price);
                ybj.运杂费 = !string.IsNullOrWhiteSpace(frei) ? decimal.Parse(frei) : 0M;
                ybj.其他费用 = !string.IsNullOrWhiteSpace(other) ? decimal.Parse(other) : 0M;
                ybj.售后服务 = shfu;
                ybj.税率 = !string.IsNullOrWhiteSpace(tax) ? decimal.Parse(tax) : 0;
                ybj.备注 = notes;
                ybj.发货地点 = saddress;
                ybj.总价 = ybj.报价 * bj.商品需求数量 + ybj.报价 * bj.商品需求数量 * ybj.税率 + ybj.运杂费 + ybj.其他费用;
            }
            else
            {
                //if (bj.起始价格 < decimal.Parse(price)) 
                //{
                //    return "您的本次报价高于起始价格，请重新输入！";
                //}
                var bjgys = new 网上竞标._报价供应商();
                bjgys.报价 = decimal.Parse(price);
                bjgys.运杂费 = !string.IsNullOrWhiteSpace(frei) ? decimal.Parse(frei) : 0M;
                bjgys.其他费用 = !string.IsNullOrWhiteSpace(other) ? decimal.Parse(other) : 0M;
                bjgys.售后服务 = shfu;
                bjgys.税率 = !string.IsNullOrWhiteSpace(tax) ? decimal.Parse(tax) : 0;
                bjgys.备注 = notes;
                bjgys.发货地点 = saddress;
                bjgys.报价供应商.用户ID = currentUser.Id;
                bjgys.总价 = bjgys.报价 * bj.商品需求数量 + bjgys.报价 * bj.商品需求数量 * bjgys.税率 + bjgys.运杂费 + bjgys.其他费用;
                bj.报价供应商列表.Add(bjgys);
            }
            网上竞标管理.更新网上竞标(bj);
            return "报价成功！";
        }

        public ActionResult Book_Msg()
        {
            return View();
        }
        public ActionResult Location()
        {
            return View();
        }
        public ActionResult Part_Gys_Znxx(int? page)
        {

            if (string.IsNullOrEmpty(page.ToString()))
            {
                page = 1;
            }
            ViewData["page"] = page;
            ViewData["listcount"] = 站内消息管理.计数站内消息(0, 0, MongoDB.Driver.Builders.Query.EQ("收信人.用户ID", currentUser.Id));
            ViewData["pagesize"] = 20;
            ViewData["站内消息列表"] = 站内消息管理.查询收信人的站内消息(20 * (int.Parse(page.ToString()) - 1), 20, currentUser.Id).OrderByDescending(m => m.重要程度);
            return PartialView("Gys_Part/Part_Gys_Znxx");
        }
        [ValidateInput(false)]
        public string Send_Message()
        {
            try
            {
                string reciever = Request.QueryString["r"].ToString();
                string[] rs = reciever.Split(',');
                for (int i = 0; i < rs.Length - 1; i++)
                {
                    if (用户管理.检查用户是否存在(rs[i]) != -1)
                    {
                        站内消息 Msg = new 站内消息();
                        对话消息 Talk = new 对话消息();
                        Talk.发言人.用户ID = currentUser.Id;
                        Msg.收信人.用户ID = 用户管理.检查用户是否存在(rs[i]);
                        int level = int.Parse(Request.QueryString["l"].ToString());
                        switch (level)
                        {
                            case 0: Msg.重要程度 = 重要程度.未指定; break;
                            case 100: Msg.重要程度 = 重要程度.一般; break;
                            case 200: Msg.重要程度 = 重要程度.重要; break;
                            case 300: Msg.重要程度 = 重要程度.特别重要; break;
                            case 400: Msg.重要程度 = 重要程度.必读; break;
                        }
                        Msg.发起者.用户ID = currentUser.Id;
                        站内消息管理.添加站内消息(Msg, Msg.发起者.用户ID, Msg.收信人.用户ID);
                        Talk.消息主体.标题 = Request.QueryString["t"].ToString();
                        Talk.消息主体.内容 = Request.QueryString["c"].ToString();
                        对话消息管理.添加对话消息(Talk, Msg);
                    }
                }
                return "添加消息成功！";
            }
            catch
            {
                return "添加消息失败！";
            }

        }
        public ActionResult Gys_Del_Complain(int id)
        {
            if (投诉管理.删除投诉(id))
            {
                return View("Gys_ComplainList");
            }
            else
            {
                return View("Gys_ComplainList");
            }
        }
        public ActionResult Gys_Del_Msg(int id)
        {
            站内消息管理.删除站内消息(id);
            return Redirect("/供应商后台/Msg_Sent");
        }
        public ActionResult Part_Gys_ZnxxAdd()
        {
            ViewData["行业"] = 商品分类管理.查找子分类();
            return PartialView("Gys_Part/Part_Gys_ZnxxAdd");
        }
        public ActionResult Part_Msg_Sent(int? page)
        {

            if (string.IsNullOrEmpty(page.ToString()))
            {
                page = 1;
            }
            ViewData["page"] = page;
            ViewData["listcount"] = 站内消息管理.计数站内消息(0, 0, MongoDB.Driver.Builders.Query.EQ("发起者.用户ID", currentUser.Id));
            ViewData["pagesize"] = 20;
            ViewData["站内消息列表"] = 站内消息管理.查询发起者的站内消息(20 * (int.Parse(page.ToString()) - 1), 20, currentUser.Id).OrderByDescending(m => m.重要程度).OrderByDescending(m => m.基本数据.修改时间);
            return PartialView("Gys_Part/Part_Msg_Sent");
        }
        public ActionResult Part_Zb_Detail(int? id)
        {
            var model = new 公告();
            try
            {
                model = 公告管理.查找公告((int)id);
            }
            catch
            {

            }
            return PartialView("Gys_Part/Part_Zb_Detail", model);
        }
        public ActionResult Zb_Detail()
        {
            return View();
        }
        public ActionResult getDT()
        {
            IMongoQuery q = Query.EQ("审核数据.审核状态", 审核状态.审核通过);
            string keyword = Request.Params["keyword"];
            string hy = Request.Params["hy"];
            string starttime = Request.Params["starttime"];
            string endtime = Request.Params["endtime"];

            if (!string.IsNullOrEmpty(keyword))
            {
                q = q.And(Query.Matches("内容主体.标题", new BsonRegularExpression(string.Format("/{0}/i", keyword))));
            }
            if (!string.IsNullOrEmpty(hy))
            {
                q = q.And(Query.EQ("行业", hy));
            }
            if (!string.IsNullOrEmpty(starttime))
            {
                q = q.And(Query.GTE("内容主体.发布时间", Convert.ToDateTime(starttime)));
            }
            if (!string.IsNullOrEmpty(endtime))
            {
                q = q.And(Query.LTE("内容主体.发布时间", Convert.ToDateTime(endtime)));
            }

            ViewData["公告查询列表"] = 公告管理.查询公告(0, GG_PAGESIZE, q);

            ViewData["condition"] = q.ToJson();
            ViewData["currentpage"] = 1;
            ViewData["pagecount"] = Math.Max((公告管理.计数公告(0, 0, q) + GG_PAGESIZE - 1) / GG_PAGESIZE, 1);

            return PartialView("Gys_Part/Part_Gys_Ztb_SearchCondition");
        }
        public ActionResult Part_Gys_Ztb_SearchCondition(int? page)
        {
            IMongoQuery q = null;
            if (!string.IsNullOrEmpty(Request.Params["condition"]))
            {
                q = new QueryDocument(QueryDocument.Parse(Request.Params["condition"]).Elements);
            }

            int listcount = (int)(公告管理.计数公告(0, 0, q));
            int maxpage = Math.Max((listcount + GG_PAGESIZE - 1) / GG_PAGESIZE, 1);

            if (string.IsNullOrEmpty(page.ToString()) || page < 0 || page > maxpage)
            {
                page = 1;
            }
            ViewData["currentpage"] = page;
            ViewData["pagecount"] = maxpage;

            ViewData["公告查询列表"] = 公告管理.查询公告(GG_PAGESIZE * (int.Parse(page.ToString()) - 1), GG_PAGESIZE, q);

            ViewData["condition"] = Request.Params["condition"];
            return PartialView("Gys_Part/Part_Gys_Ztb_SearchCondition");
        }


        [ValidateInput(false)]
        public int Gys_Msg_Reply()
        {
            try
            {
                long id = long.Parse(Request.QueryString["Id"].ToString());
                string title = Request.QueryString["t"].ToString();
                string contents = Request.QueryString["c"].ToString();
                var item = new 对话消息();
                item.发言人.用户ID = currentUser.Id;
                item.消息主体.标题 = title;
                item.消息主体.内容 = contents;
                对话消息管理.添加对话消息(item, 站内消息管理.查找站内消息(id));
                return 1;
            }
            catch
            {
                return 0;
            }
        }
        public ActionResult Part_Gys_Reply()
        {
            try
            {
                long id = long.Parse(Request.QueryString["id"].ToString());
                站内消息管理.更新已读时间(id, false, DateTime.Now);
                var model = 站内消息管理.查找站内消息(id);
                if (model != null)
                {
                    ViewData["当前用户"] = currentUser.登录信息.登录名;
                    return PartialView("Gys_Part/Part_Gys_Reply", model);
                }
                else
                {
                    return Content("<script>window.location='/供应商后台/Gys_Znxx';</script>");
                }

            }
            catch
            {
                return Content("<script>window.location='/供应商后台/Gys_Znxx';</script>");
            }

        }
        public ActionResult Gys_Reply()
        {
            return View();
        }
        public ActionResult Msg_Sent()
        {
            return View();
        }
        public ActionResult Part_Gys_Xttz(int? page)
        {
            int listcount = (int)通知管理.计数通知(0, 0);
            int maxpage = Math.Max((listcount + TZ_PAGESIZE - 1) / TZ_PAGESIZE, 1);

            if (string.IsNullOrEmpty(page.ToString()) || page < 0 || page > maxpage)
            {
                page = 1;
            }
            ViewData["page"] = page;
            ViewData["listcount"] = listcount;
            ViewData["pagesize"] = TZ_PAGESIZE;
            ViewData["通知管理"] = 通知管理.查询通知(TZ_PAGESIZE * (int.Parse(page.ToString()) - 1), TZ_PAGESIZE);
            return PartialView("Gys_Part/Part_Gys_Xttz");
        }

        public ActionResult Part_Gys_Ztb_Search_Zb(int? page)
        {
            ViewData["行业列表"] = 商品分类管理.查找子分类();
            IMongoQuery q = Query.EQ("审核数据.审核状态", 审核状态.审核通过);

            int listcount = (int)公告管理.计数公告(0, 0, q);
            int maxpage = Math.Max((listcount + GG_PAGESIZE - 1) / GG_PAGESIZE, 1);


            if (string.IsNullOrEmpty(page.ToString()) || page < 0 || page > maxpage)
            {
                page = 1;
            }
            ViewData["page"] = page;
            ViewData["listcount"] = listcount;
            ViewData["pagesize"] = GG_PAGESIZE;
            ViewData["公告管理"] = 公告管理.查询公告(GG_PAGESIZE * (int.Parse(page.ToString()) - 1), GG_PAGESIZE, q);
            return PartialView("Gys_Part/Part_Gys_Ztb_Search_Zb");
        }

        public ActionResult Part_Gys_Ztb_Search_Cg(int? page)
        {
            //
            //if (string.IsNullOrEmpty(page.ToString()) || page < 0 || page > GG_OK_MAXPAGE)
            //{
            //    page = 1;
            //}
            ////ViewData["listcount"] = GONGGAOLIST;
            //ViewData["listcount"] = 公告管理.计数公告(0, 0, Query.NE("公告信息.公告类别", 公告.公告类别.公开招标).And(Query.EQ("审核数据.审核状态", 审核状态.审核通过)));
            //ViewData["pagesize"] = GG_PAGESIZE;
            //ViewData["公告管理"] = 公告管理.查询公告(GG_PAGESIZE * (int.Parse(page.ToString()) - 1), GG_PAGESIZE, Query.NE("公告信息.公告类别", 公告.公告类别.公开招标).And(Query.EQ("审核数据.审核状态", 审核状态.审核通过)));

            return PartialView("Gys_Part/Part_Gys_Ztb_Search_Cg");
        }

        public ActionResult Part_Gys_Ztb_Dygl()
        {
            return PartialView("Gys_Part/Part_Gys_Ztb_Dygl");
        }

        public ActionResult Part_Gys_Product_Detail(int? id)
        {
            try
            {
                商品 pro_detail = 商品管理.查找商品((long)id);
                if (pro_detail.商品信息.所属供应商.用户ID != currentUser.Id)
                {
                    return Content("<script>top.location.href='/错误页面/NoPrivilege'</script>");
                }
                ViewBag.data = (用户管理.查找用户<供应商>(pro_detail.商品信息.所属供应商.用户ID)).信用评级信息.等级;
                if (pro_detail.商品信息.商品图片.Count == 0)
                {
                    pro_detail.商品信息.商品图片.Add("/images/noimage.jpg");
                }
                var l = 商品管理.查询历史价格数据((long)id);
                this.ViewBag.L1 = l.Item1.ToJson().Replace("ISODate", "new Date");

                return PartialView("Gys_Part/Part_Gys_Product_Detail", pro_detail);
            }
            catch
            {
                return Content("<script>location.href='javascript:history.go(-1)';</script>");
            }
        }
        public ActionResult Gys_Product_Delete(int? id)
        {
            商品 model = 商品管理.查找商品((long)id);
            if (model.商品信息.所属供应商.用户ID != currentUser.Id)
            {
                return Content("<script>top.location.href='/错误页面/NoPrivilege'</script>");
            }
            if (model.商品信息.商品图片 != null && model.商品信息.商品图片.Count > 0)
            {
                foreach (var item in model.商品信息.商品图片)
                {
                    try
                    {
                        UploadPic.DelPic(string.Format("{0}", item));
                        UploadPic.DelPic(string.Format("{0}", item.Replace("original", "350X350")));
                        UploadPic.DelPic(string.Format("{0}", item.Replace("original", "150X150")));
                        UploadPic.DelPic(string.Format("{0}", item.Replace("original", "50X50")));
                    }
                    catch
                    {

                    }
                }
            }
            if (商品管理.删除商品((long)id))
            {
                deleteIndex("/Lucene.Net/IndexDic/Product", id.ToString());
            }
            return RedirectToAction("Gys_Product_List");
        }

        public ActionResult Part_Gys_Spgl_Update()
        {
            return PartialView("Gys_Part/Part_Gys_Spgl_Update");
        }

        //带参数传递
        //public ActionResult Part_Gys_News_Show(int id, string name)
        //{
        //    //PartialViewViewModel model = new PartialViewViewModel();
        //    //model.id = id;
        //    //model.Name = name;
        //    //return PartialView("Gys_Part/Part_Gys_News_Show", model);

        //    return PartialView("Gys_Part/Part_Gys_News_Show");
        //}

        public ActionResult Part_Gys_News_Show()
        {
            try
            {
                int id = int.Parse(Request.QueryString["id"].ToString());
                var model = 通知管理.查找通知(id);
                //PartialViewViewModel model = new PartialViewViewModel();
                //model.id = id;
                //model.Name = name;
                //return PartialView("Gys_Part/Part_Gys_News_Show", model);
                return PartialView("Gys_Part/Part_Gys_News_Show", model);
            }
            catch
            {
                return Content("<script>window.location='Gys_Xttz';</script>");
            }
        }

        public ActionResult Part_Gys_Register()
        {
            return PartialView("Gys_Part/Part_Gys_Register");
        }

        public ActionResult Part_Gys_Product_List(int? page)
        {
            ViewData["sum"] = 商品管理.计数供应商商品(currentUser.Id, 0, 0, Query<商品>.Where(o => o.审核数据.审核状态 == 审核状态.审核通过));
            var q = Query<商品>.Where(o => o.审核数据.审核状态 == 审核状态.未审核);
            供应商 model = 用户管理.查找用户<供应商>(currentUser.Id);
            int listcount = (int)商品管理.计数供应商商品(this.currentUser.Id, 0, 0,q);
            int maxpage = Math.Max((listcount + PRO_PAGESIZE - 1) / PRO_PAGESIZE, 1);

            if (string.IsNullOrEmpty(page.ToString()) || page < 0 || page > maxpage)
            {
                page = 1;
            }
            ViewData["page"] = page;
            ViewData["listcount"] = listcount;
            ViewData["pagesize"] = PRO_PAGESIZE;
            ViewData["供应商商品信息"] = 商品管理.查询供应商商品(this.currentUser.Id, PRO_PAGESIZE * (int.Parse(page.ToString()) - 1), PRO_PAGESIZE,q);

            //判断是否能使用批量上传工具
            ViewData["批量上传"] = "0";
            if (增值服务记录 != null && 增值服务记录.已开通的服务.Any())
            {
                var 批量上传 = 增值服务记录.已开通的服务.Where(o => (o.所申请项目名.Contains("标准会员") && o.结束时间 > DateTime.Now) || (o.所申请项目名.Contains("商务会员") && o.结束时间 > DateTime.Now) || (o.所申请项目名.Contains("批量上传商品") && o.结束时间 > DateTime.Now));
                if (批量上传.Any())
                {
                    ViewData["批量上传"] = "1";
                }
            }
           
            //判断是否能选择展示商品
            ViewData["选择展示商品"] = "0";
            if (增值服务记录 != null && 增值服务记录.已开通的服务.Any())
            {
                var 选择展示商品 = 增值服务记录.已开通的服务.Where(o => (o.所申请项目名.Contains("标准会员") && o.结束时间 > DateTime.Now) || (o.所申请项目名.Contains("商务会员") && o.结束时间 > DateTime.Now) || (o.所申请项目名.Contains("企业推广服务B1-1位置") && o.结束时间 > DateTime.Now) || (o.所申请项目名.Contains("企业推广服务B1-2位置") && o.结束时间 > DateTime.Now));
                if (选择展示商品.Any())
                {
                    ViewData["选择展示商品"] = "1";
                }
            }

            //判断能上架商品个数
            ViewData["可上架商品个数"] = "G";
            if (增值服务记录 != null && 增值服务记录.已开通的服务.Any())
            {
                var 已开通服务 = 增值服务记录.已开通的服务;
                if (已开通服务.Where(o => o.所申请项目名.Contains("企业主页商品展示A类窗口") && o.结束时间 > DateTime.Now).Any())
                {
                    ViewData["可上架商品个数"] = "A";
                }
                else if (已开通服务.Where(o => o.所申请项目名.Contains("企业主页商品展示B类窗口") && o.结束时间 > DateTime.Now).Any())
                {
                    ViewData["可上架商品个数"] = "B";
                }
                else if (已开通服务.Where(o => o.所申请项目名.Contains("企业主页商品展示C类窗口") && o.结束时间 > DateTime.Now).Any())
                {
                    ViewData["可上架商品个数"] = "C";
                }
                else if (已开通服务.Where(o => o.所申请项目名.Contains("企业主页商品展示D类窗口") && o.结束时间 > DateTime.Now).Any())
                {
                    ViewData["可上架商品个数"] = "D";
                }
                else if (已开通服务.Where(o => o.所申请项目名.Contains("企业主页商品展示E类窗口") && o.结束时间 > DateTime.Now).Any())
                {
                    ViewData["可上架商品个数"] = "E";
                }
                else if (已开通服务.Where(o => o.所申请项目名.Contains("企业主页商品展示F类窗口") && o.结束时间 > DateTime.Now).Any())
                {
                    ViewData["可上架商品个数"] = "F";
                }
                else
                {
                    ViewData["可上架商品个数"] = "G";
                }
            }


            return PartialView("Gys_Part/Part_Gys_Product_List", model);
        }

        public ActionResult Gys_Product_Listed()
        {
            return View();
        }
        public ActionResult Part_Gys_Product_Listed(int? page)
        {
            ViewData["sum"] = 商品管理.计数供应商商品(currentUser.Id, 0, 0, Query<商品>.Where(o => o.审核数据.审核状态 == 审核状态.审核通过));
            var q = Query<商品>.Where(o => o.审核数据.审核状态 != 审核状态.未审核);
            供应商 model = 用户管理.查找用户<供应商>(currentUser.Id);
            int listcount = (int)商品管理.计数供应商商品(this.currentUser.Id, 0, 0, q);
            int maxpage = Math.Max((listcount + PRO_PAGESIZE - 1) / PRO_PAGESIZE, 1);

            if (string.IsNullOrEmpty(page.ToString()) || page < 0 || page > maxpage)
            {
                page = 1;
            }
            ViewData["page"] = page;
            ViewData["listcount"] = listcount;
            ViewData["pagesize"] = PRO_PAGESIZE;
            ViewData["供应商商品信息"] = 商品管理.查询供应商商品(this.currentUser.Id, PRO_PAGESIZE * (int.Parse(page.ToString()) - 1), PRO_PAGESIZE, q);

            //判断是否能使用批量上传工具
            ViewData["批量上传"] = "0";
            if (增值服务记录 != null && 增值服务记录.已开通的服务.Any())
            {
                var 批量上传 = 增值服务记录.已开通的服务.Where(o => (o.所申请项目名.Contains("标准会员") && o.结束时间 > DateTime.Now) || (o.所申请项目名.Contains("商务会员") && o.结束时间 > DateTime.Now) || (o.所申请项目名.Contains("批量上传商品") && o.结束时间 > DateTime.Now));
                if (批量上传.Any())
                {
                    ViewData["批量上传"] = "1";
                }
            }

            //判断是否能选择展示商品
            ViewData["选择展示商品"] = "0";
            if (增值服务记录 != null && 增值服务记录.已开通的服务.Any())
            {
                var 选择展示商品 = 增值服务记录.已开通的服务.Where(o => (o.所申请项目名.Contains("标准会员") && o.结束时间 > DateTime.Now) || (o.所申请项目名.Contains("商务会员") && o.结束时间 > DateTime.Now) || (o.所申请项目名.Contains("企业推广服务B1-1位置") && o.结束时间 > DateTime.Now) || (o.所申请项目名.Contains("企业推广服务B1-2位置") && o.结束时间 > DateTime.Now));
                if (选择展示商品.Any())
                {
                    ViewData["选择展示商品"] = "1";
                }
            }

            //判断能上架商品个数
            ViewData["可上架商品个数"] = "G";
            if (增值服务记录 != null && 增值服务记录.已开通的服务.Any())
            {
                var 已开通服务 = 增值服务记录.已开通的服务;
                if (已开通服务.Where(o => o.所申请项目名.Contains("企业主页商品展示A类窗口") && o.结束时间 > DateTime.Now).Any())
                {
                    ViewData["可上架商品个数"] = "A";
                }
                else if (已开通服务.Where(o => o.所申请项目名.Contains("企业主页商品展示B类窗口") && o.结束时间 > DateTime.Now).Any())
                {
                    ViewData["可上架商品个数"] = "B";
                }
                else if (已开通服务.Where(o => o.所申请项目名.Contains("企业主页商品展示C类窗口") && o.结束时间 > DateTime.Now).Any())
                {
                    ViewData["可上架商品个数"] = "C";
                }
                else if (已开通服务.Where(o => o.所申请项目名.Contains("企业主页商品展示D类窗口") && o.结束时间 > DateTime.Now).Any())
                {
                    ViewData["可上架商品个数"] = "D";
                }
                else if (已开通服务.Where(o => o.所申请项目名.Contains("企业主页商品展示E类窗口") && o.结束时间 > DateTime.Now).Any())
                {
                    ViewData["可上架商品个数"] = "E";
                }
                else if (已开通服务.Where(o => o.所申请项目名.Contains("企业主页商品展示F类窗口") && o.结束时间 > DateTime.Now).Any())
                {
                    ViewData["可上架商品个数"] = "F";
                }
                else
                {
                    ViewData["可上架商品个数"] = "G";
                }
            }
            return PartialView("Gys_Part/Part_Gys_Product_Listed", model);
        }

        public ActionResult Gys_Product_Shelved()
        {
            return View();
        }
        public ActionResult Part_Gys_Product_Shelved(int? page)
        {
            ViewData["sum"] = 商品管理.计数供应商商品(currentUser.Id, 0, 0, Query<商品>.Where(o => o.审核数据.审核状态 == 审核状态.审核通过));
            var q = Query<商品>.Where(o => o.审核数据.审核状态 == 审核状态.审核通过 && !o.基本数据.已屏蔽);
            供应商 model = 用户管理.查找用户<供应商>(currentUser.Id);
            int listcount = (int)商品管理.计数供应商商品(this.currentUser.Id, 0, 0, q);
            int maxpage = Math.Max((listcount + PRO_PAGESIZE - 1) / PRO_PAGESIZE, 1);

            if (string.IsNullOrEmpty(page.ToString()) || page < 0 || page > maxpage)
            {
                page = 1;
            }
            ViewData["page"] = page;
            ViewData["listcount"] = listcount;
            ViewData["pagesize"] = PRO_PAGESIZE;
            ViewData["供应商商品信息"] = 商品管理.查询供应商商品(this.currentUser.Id, PRO_PAGESIZE * (int.Parse(page.ToString()) - 1), PRO_PAGESIZE, q);

            //判断是否能使用批量上传工具
            ViewData["批量上传"] = "0";
            if (增值服务记录 != null && 增值服务记录.已开通的服务.Any())
            {
                var 批量上传 = 增值服务记录.已开通的服务.Where(o => (o.所申请项目名.Contains("标准会员") && o.结束时间 > DateTime.Now) || (o.所申请项目名.Contains("商务会员") && o.结束时间 > DateTime.Now) || (o.所申请项目名.Contains("批量上传商品") && o.结束时间 > DateTime.Now));
                if (批量上传.Any())
                {
                    ViewData["批量上传"] = "1";
                }
            }

            //判断是否能选择展示商品
            ViewData["选择展示商品"] = "0";
            if (增值服务记录 != null && 增值服务记录.已开通的服务.Any())
            {
                var 选择展示商品 = 增值服务记录.已开通的服务.Where(o => (o.所申请项目名.Contains("标准会员") && o.结束时间 > DateTime.Now) || (o.所申请项目名.Contains("商务会员") && o.结束时间 > DateTime.Now) || (o.所申请项目名.Contains("企业推广服务B1-1位置") && o.结束时间 > DateTime.Now) || (o.所申请项目名.Contains("企业推广服务B1-2位置") && o.结束时间 > DateTime.Now));
                if (选择展示商品.Any())
                {
                    ViewData["选择展示商品"] = "1";
                }
            }

            //判断能上架商品个数
            ViewData["可上架商品个数"] = "G";
            if (增值服务记录 != null && 增值服务记录.已开通的服务.Any())
            {
                var 已开通服务 = 增值服务记录.已开通的服务;
                if (已开通服务.Where(o => o.所申请项目名.Contains("企业主页商品展示A类窗口") && o.结束时间 > DateTime.Now).Any())
                {
                    ViewData["可上架商品个数"] = "A";
                }
                else if (已开通服务.Where(o => o.所申请项目名.Contains("企业主页商品展示B类窗口") && o.结束时间 > DateTime.Now).Any())
                {
                    ViewData["可上架商品个数"] = "B";
                }
                else if (已开通服务.Where(o => o.所申请项目名.Contains("企业主页商品展示C类窗口") && o.结束时间 > DateTime.Now).Any())
                {
                    ViewData["可上架商品个数"] = "C";
                }
                else if (已开通服务.Where(o => o.所申请项目名.Contains("企业主页商品展示D类窗口") && o.结束时间 > DateTime.Now).Any())
                {
                    ViewData["可上架商品个数"] = "D";
                }
                else if (已开通服务.Where(o => o.所申请项目名.Contains("企业主页商品展示E类窗口") && o.结束时间 > DateTime.Now).Any())
                {
                    ViewData["可上架商品个数"] = "E";
                }
                else if (已开通服务.Where(o => o.所申请项目名.Contains("企业主页商品展示F类窗口") && o.结束时间 > DateTime.Now).Any())
                {
                    ViewData["可上架商品个数"] = "F";
                }
                else
                {
                    ViewData["可上架商品个数"] = "G";
                }
            }
            return PartialView("Gys_Part/Part_Gys_Product_Shelved", model);
        }
        [HttpPost]
        public ActionResult doProductShelves()
        {
            try
            {
                var id = Request.Params["id"];
                var status = Request.Params["status"];
                var changetype = Request.Params["changetype"];

                //判断该供应商能上架和已上架商品数是否达到限制
                var 已上架商品总数 = 商品管理.计数供应商商品(currentUser.Id, 0, 0, Query<商品>.EQ(o => o.基本数据.已屏蔽, false));
                var 可上架商品总数 = 8;
                if (增值服务记录 != null && 增值服务记录.已开通的服务.Any())
                {
                    var 企业主页商品个数A = 增值服务记录.已开通的服务.Where(o => (o.所申请项目名.Contains("企业主页商品展示A类窗口") && o.结束时间 > DateTime.Now));
                    var 企业主页商品个数B = 增值服务记录.已开通的服务.Where(o => (o.所申请项目名.Contains("企业主页商品展示B类窗口") && o.结束时间 > DateTime.Now));
                    var 企业主页商品个数C = 增值服务记录.已开通的服务.Where(o => (o.所申请项目名.Contains("企业主页商品展示C类窗口") && o.结束时间 > DateTime.Now));
                    var 企业主页商品个数D = 增值服务记录.已开通的服务.Where(o => (o.所申请项目名.Contains("企业主页商品展示D类窗口") && o.结束时间 > DateTime.Now));
                    var 企业主页商品个数E = 增值服务记录.已开通的服务.Where(o => (o.所申请项目名.Contains("企业主页商品展示E类窗口") && o.结束时间 > DateTime.Now));
                    var 企业主页商品个数F = 增值服务记录.已开通的服务.Where(o => (o.所申请项目名.Contains("企业主页商品展示F类窗口") && o.结束时间 > DateTime.Now));
                    可上架商品总数 = 企业主页商品个数A.Any() ? Int32.MaxValue : 企业主页商品个数B.Any() ? 200 : 企业主页商品个数C.Any() ? 80 : 企业主页商品个数D.Any() ? 60 : 企业主页商品个数E.Any() ? 40 : 企业主页商品个数F.Any() ? 20 : 8;
                }
                //已达到个数，不能再进行上架
                if (已上架商品总数 >= 可上架商品总数 && status=="1")
                {
                    return Content("您目前订购的企业展示模板可上架" + 可上架商品总数 + "个商品，已达到" + 可上架商品总数 + "个，如需上架更多商品，请前往军采通查看“更多企业展示模板”，或详询网站客服。！");
                }
                else
                {
                    //选择全部商品
                    if (changetype == "2")
                    {
                        var sp = 商品管理.查询供应商商品(long.Parse(id), 0, 0);
                        foreach (var s in sp)
                        {
                            if (status == "0")
                            {
                                s.基本数据.已屏蔽 = true;
                                //TD：删除Lucene
                                deleteIndex("/Lucene.Net/IndexDic/Product", s.Id.ToString());
                                商品管理.更新商品(s, setUnverified: false);
                            }
                            else
                            {
                                if (s.审核数据.审核状态 == 审核状态.审核通过)
                                {
                                    if(已上架商品总数 >= 可上架商品总数)
                                    {
                                        return Content("overflow");
                                    }
                                    s.基本数据.已屏蔽 = false;
                                    //TD：删除并新建Lucene
                                    deleteIndex("/Lucene.Net/IndexDic/Product", s.Id.ToString());
                                    CreateIndex(s, "/Lucene.Net/IndexDic/Product");
                                    商品管理.更新商品(s, setUnverified: false);
                                    已上架商品总数++;
                                }
                            }
                        }
                    }
                    else
                    {
                        var idstr = id.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
                        foreach (var idlist in idstr)
                        {
                            var sp = 商品管理.查找商品(long.Parse(idlist));
                            if (status == "0")
                            {
                                sp.基本数据.已屏蔽 = true;
                                //TD：删除Lucene
                                deleteIndex("/Lucene.Net/IndexDic/Product", sp.Id.ToString());
                                商品管理.更新商品(sp, setUnverified: false);
                            }
                            else
                            {
                                if (sp.审核数据.审核状态 == 审核状态.审核通过)
                                {
                                     if(已上架商品总数 >= 可上架商品总数)
                                    {
                                        return Content("overflow");
                                    }
                                    sp.基本数据.已屏蔽 = false;
                                    //TD：删除并新建Lucene
                                    deleteIndex("/Lucene.Net/IndexDic/Product", sp.Id.ToString());
                                    CreateIndex(sp, "/Lucene.Net/IndexDic/Product");
                                    商品管理.更新商品(sp, setUnverified: false);
                                    已上架商品总数++;
                                }
                                //else
                                //{
                                //    if (idlist.Count > 1)
                                //    {
                                //        return Content("该页有未审核通过的商品，本页审核通过的商品已上架！");
                                //    }
                                //    else
                                //    {
                                //        return Content("该商品还未审核通过，不能上架！");
                                //    }
                                //}
                            }
                        }
                    }
                    return Content("0");
                }

               
               
            }
            catch
            {
                return Content("发生错误，请重试！");
            }
        }

        public ActionResult AddListGood()
        {
            if (增值服务记录 == null)
            {
                return RedirectToAction("Gys_Product_List");
            }
            else
            {
                var k = 增值服务记录.已开通的服务.Where(o => o.所申请项目名.Contains("标准会员") || o.所申请项目名.Contains("商务会员") || o.所申请项目名.Contains("批量上传商品"));
                if (k.Count() <= 0)
                {
                    return RedirectToAction("Gys_Product_List");
                }
            }
            var classli = currentUser.可提供产品类别列表;
            return View(classli);
        }
        public ActionResult AddListGood1()
        {
            if (增值服务记录 == null)
            {
                return RedirectToAction("Gys_Product_List");
            }
            else
            {
                var k = 增值服务记录.已开通的服务.Where(o => o.所申请项目名.Contains("标准会员") || o.所申请项目名.Contains("商务会员") || o.所申请项目名.Contains("批量上传商品"));
                if (k.Count() <= 0)
                {
                    return RedirectToAction("Gys_Product_List");
                }
            }
            return View();
        }

        public ActionResult Part_Gys_Product_Add(string classname)
        {
            try
            {
                ViewData["是否中标商品"] = Request.Params["bid"];
                商品分类 m = 商品分类管理.查找分类(long.Parse(classname));
                var att = m.商品属性模板;
                ViewData["商品属性模板"] = att;
                ViewData["ID"] = m.Id;
                return PartialView("Gys_Part/Part_Gys_Product_Add");
            }
            catch
            {
                return Content("<script>top.location.href='/供应商后台/Gys_Product_AddStep1'</script>");
            }
        }
        [ValidateInput(false)]
        public string Gys_ComplainsAdd()
        {
            IEnumerable<单位用户> model = 用户管理.查询用户<单位用户>(0, 0);

            //投诉管理.添加投诉(Msg, Msg.发起者.用户ID, 100000000001);
            if (model != null && model.Count() != 0)
            {
                foreach (var item in model)
                {
                    投诉 Msg = new 投诉();
                    Msg.处理状态 = 处理状态.未处理;
                    对话消息 Talk = new 对话消息();
                    Talk.发言人.用户ID = currentUser.Id;
                    Msg.发起者.用户ID = currentUser.Id;
                    投诉管理.添加投诉(Msg, Msg.发起者.用户ID, item.Id);
                    Talk.消息主体.标题 = Request.QueryString["t"].ToString();
                    Talk.消息主体.内容 = Request.QueryString["c"].ToString();
                    对话消息管理.添加对话消息(Talk, Msg);
                }
            }
            return "添加投诉成功!";
        }
        public ActionResult Part_Gys_ComplainAdd()
        {
            return PartialView("Gys_Part/Part_Gys_ComplainAdd");
        }

        public ActionResult Part_Gys_ComplainList(int? page)
        {

            if (string.IsNullOrEmpty(page.ToString()))
            {
                page = 1;
            }
            ViewData["page"] = page;
            ViewData["listcount"] = 投诉管理.计数投诉(0, 0, Query.EQ("发起者.用户ID", currentUser.Id));
            ViewData["pagesize"] = 20;
            ViewData["投诉管理"] = 投诉管理.查询发起者的投诉(20 * (int.Parse(page.ToString()) - 1), 20, currentUser.Id, 处理状态.全部).OrderByDescending(m => m.基本数据.修改时间);
            return PartialView("Gys_Part/Part_Gys_ComplainList");
        }

        public ActionResult Part_Gys_ComplainDetail()
        {
            try
            {
                long id = long.Parse(Request.QueryString["id"].ToString());
                投诉 model = 投诉管理.查找投诉(id);
                if (model != null)
                {
                    ViewData["当前用户"] = currentUser.登录信息.登录名;
                    return PartialView("Gys_Part/Part_Gys_ComplainDetail", model);
                }
                else
                {
                    return Content("<script>window.location='/供应商后台/Gys_ComplainList'</script>");
                }

            }
            catch
            {
                return Content("<script>window.location='/供应商后台/Gys_ComplainList'</script>");
            }

        }

        public ActionResult Part_Gys_ConsultAdd()
        {
            return PartialView("Gys_Part/Part_Gys_ConsultAdd");
        }

        public ActionResult Part_Gys_ConsultList()
        {
            return PartialView("Gys_Part/Part_Gys_ConsultList");
        }

        public ActionResult Part_Gys_ConsultDetail()
        {
            return PartialView("Gys_Part/Part_Gys_ConsultDetail");
        }

        public ActionResult Part_Gys_GuideOne()
        {
            return PartialView("Gys_Part/Part_Gys_GuideOne");
        }

        public ActionResult Part_Gys_GuideThr()
        {
            return PartialView("Gys_Part/Part_Gys_GuideThr");
        }
        public ActionResult Part_Gys_GuideTwo()
        {
            return PartialView("Gys_Part/Part_Gys_GuideTwo");
        }
        [ValidateInput(false)]
        public string SuggestionAdd()
        {
            建议 Msg = new 建议();
            Msg.处理状态 = 处理状态.未处理;
            对话消息 Talk = new 对话消息();
            Talk.发言人.用户ID = currentUser.Id;
            Msg.发起者.用户ID = currentUser.Id;
            建议管理.添加建议(Msg, Msg.发起者.用户ID, 100000000001);
            Talk.消息主体.标题 = Request.QueryString["t"].ToString();
            Talk.消息主体.内容 = Request.QueryString["c"].ToString();
            对话消息管理.添加对话消息(Talk, Msg);
            return "添加建议成功!";
        }
        public ActionResult Part_Gys_SuggestAdd()
        {
            return PartialView("Gys_Part/Part_Gys_SuggestAdd");
        }
        public ActionResult SuggestDelete(int id)
        {
            建议管理.删除建议(id);
            return View("Gys_SuggestList");
        }
        public ActionResult Part_Gys_SuggestList(int? page)
        {

            if (string.IsNullOrEmpty(page.ToString()))
            {
                page = 1;
            }
            ViewData["page"] = page;
            ViewData["listcount"] = 建议管理.计数建议(0, 0, Query.EQ("发起者.用户ID", currentUser.Id));
            ViewData["pagesize"] = 20;
            ViewData["我的建议"] = 建议管理.查询发起者的建议(20 * (int.Parse(page.ToString()) - 1), 20, currentUser.Id, 处理状态.全部);
            return PartialView("Gys_Part/Part_Gys_SuggestList");
        }
        public ActionResult Part_Gys_SuggestDetail()
        {
            try
            {
                long id = long.Parse(Request.QueryString["id"].ToString());
                ViewData["当前用户"] = currentUser.登录信息.登录名;
                建议 model = 建议管理.查找建议(id);
                if (model != null)
                {
                    return PartialView("Gys_Part/Part_Gys_SuggestDetail", model);
                }
                else
                {
                    return Content("<script>window.location='/供应商后台/Gys_SuggestList'</script>");
                }

            }
            catch
            {
                return Content("<script>window.location='/供应商后台/Gys_SuggestList'</script>");
            }

        }
        [ValidateInput(false)]
        public int Complain_Reply()
        {
            try
            {
                long id = long.Parse(Request.QueryString["Id"].ToString());
                string title = Request.QueryString["t"].ToString();
                string contents = Request.QueryString["c"].ToString();
                var item = new 对话消息();
                item.发言人.用户ID = currentUser.Id;
                item.消息主体.标题 = title;
                item.消息主体.内容 = contents;
                对话消息管理.添加对话消息(item, 投诉管理.查找投诉(id));
                return 1;
            }
            catch
            {
                return 0;
            }
        }
        [ValidateInput(false)]
        public int Suggest_Reply()
        {
            try
            {
                long id = long.Parse(Request.QueryString["Id"].ToString());
                string title = Request.QueryString["t"].ToString();
                string contents = Request.QueryString["c"].ToString();
                var item = new 对话消息();
                item.发言人.用户ID = currentUser.Id;
                item.消息主体.标题 = title;
                item.消息主体.内容 = contents;
                对话消息管理.添加对话消息(item, 建议管理.查找建议(id));
                return 1;
            }
            catch
            {
                return 0;
            }
        }
        public ActionResult Gys_Product_Modify_Price()
        {
            return View();
        }
        public ActionResult Part_Gys_Product_Modify_Price(int? id)
        {
            try
            {
                var comes = Request.Params["comes"];
                if (comes == "d")
                {
                    ViewData["comes"] = "已审核的商品信息";
                }
                else if(comes=="s")
                {
                    ViewData["comes"] = "未审核的商品信息";
                }
                else
                {
                    ViewData["comes"] = "当前上架的商品信息";
                }

                var pro_info = 商品管理.查找商品((long)id);
                ViewData["price"] = pro_info.销售信息.价格;
                ViewData["price_march"] = pro_info.销售信息.价格属性组合;
                ViewData["p_id"] = pro_info.Id;

                //获取当前的分类已有精确型号

                return PartialView("Gys_Part/Part_Gys_Product_Modify_Price");
            }
            catch
            {
                return Content("<script>location.href='javascript:history.go(-1)';</script>");
            }

        }
        [HttpPost]
        public ActionResult Gys_Product_Modify_Price(int? id)
        {
            try
            {
                var p_id = long.Parse(Request.Form["p_id"]);
                var p_price = decimal.Parse(Request.Form["price"]);
                var att_march = new 商品._价格属性组合();

                string price_sttr = Request.Form["pricesttrsrr"]; //尺寸|颜色^1|155|红色&2|155|黄色&3|160|红色&4|160|黄色&

                if (!string.IsNullOrEmpty(price_sttr))
                {
                    string[] price_a1 = price_sttr.Split(new[] { "^^^^" }, StringSplitOptions.None);
                    //price_a1[0]:尺寸|颜色、尺寸
                    string tempindexstr = "||||";
                    if (price_a1[0].IndexOf(tempindexstr) > -1)
                    {
                        att_march.设置属性组合列表(price_a1[0].Split(new[] { tempindexstr }, StringSplitOptions.None));
                    }
                    else
                    {
                        att_march.设置属性组合列表(price_a1[0]);
                    }
                    //price_a1[1]:1|155|红色&2|155|黄色&3|160|红色&4|160|黄色&

                    string[] price_a2 = price_a1[1].Substring(0, price_a1[1].Length - 4)
                        .Split(new[] { "&&&&" }, StringSplitOptions.None); //price_a2[i]:4#160|黄色、4#160
                    for (int i = 0; i < price_a2.Length; i++)
                    {
                        string[] price_a3 = price_a2[i].Split(new[] { "####" }, StringSplitOptions.None);
                        //price_a3[0]:4 price_a3[1]:160|黄色、160
                        if (!string.IsNullOrEmpty(price_a3[0]))
                        {
                            var price_march = decimal.Parse(price_a3[0]);
                            if (price_a3[1].IndexOf(tempindexstr) > -1)
                            {
                                att_march.添加组合(price_march,
                                    price_a3[1].Split(new[] { tempindexstr }, StringSplitOptions.None));
                            }
                            else
                            {
                                att_march.添加组合(price_march, price_a3[1]);
                            }
                        }
                    }
                }

                商品管理.更新商品价格(p_id, p_price, att_march);
                var Pro_Model = 商品管理.查找商品(p_id);
                deleteIndex("/Lucene.Net/IndexDic/Product", p_id.ToString());
                CreateIndex(Pro_Model, "/Lucene.Net/IndexDic/Product");
            }
            catch
            {

            }
            return View("Gys_Product_List");
        }
        public ActionResult Part_Gys_Product_Modify(int? id)
        {
            try
            {
                var comes = Request.Params["comes"];
                if (comes == "d")
                {
                    ViewData["comes"] = "已审核的商品信息";
                }
                else if(comes=="s")
                {
                    ViewData["comes"] = "未审核的商品信息";
                }
                else
                {
                    ViewData["comes"] = "当前上架的商品信息";
                }

                var pro_info = 商品管理.查找商品((long)id);
                if (pro_info.商品信息.所属供应商.用户ID != currentUser.Id)
                {
                    return Content("<script>top.location.href='/错误页面/NoPrivilege'</script>");
                }

                var att = 商品分类管理.查找分类(pro_info.商品信息.所属商品分类.商品分类ID).商品属性模板;

                ViewData["classid"] = pro_info.商品信息.所属商品分类.商品分类ID;
                ViewData["商品属性模板"] = att;
                ViewData["pro_id"] = id;
                return PartialView("Gys_Part/Part_Gys_Product_Modify", pro_info);
            }
            catch
            {
                return Content("<script>location.href='javascript:history.go(-1)';</script>");
            }

        }

        public ActionResult Part_Gys_Product_Price(int? id)
        {
            //var att = 商品管理.查找商品((long)id).商品数据.商品属性;
            //ViewData["商品属性"] = att;
            ViewData["商品属性"] = null;
            ViewData["ID"] = id;
            return PartialView("Gys_Part/Part_Gys_Product_Price");
        }
        public ActionResult Gys_Product_Price(int? id)
        {
            return View();
        }
        [HttpPost]
        public ActionResult Gys_Vip_Manage(供应商 model)
        {
            供应商 Newmodel = 用户管理.查找用户<供应商>(currentUser.Id);
            if (Newmodel.审核数据.审核状态 == 审核状态.审核通过)
            {
                return Content("<script>alert('审核通过的用户不能再修改信息！');window.location='/供应商后台/';</script>");
            }
            Newmodel.地址信息 = model.地址信息;
            Newmodel.企业联系人信息 = model.企业联系人信息;
            if (string.IsNullOrWhiteSpace(Newmodel.企业联系人信息.联系人固定电话) || string.IsNullOrWhiteSpace(Newmodel.企业联系人信息.联系人手机) || string.IsNullOrWhiteSpace(Newmodel.企业联系人信息.联系人姓名))
            {
                Newmodel.企业联系人信息.已填写完整 = false;
            }
            else
            {
                Newmodel.企业联系人信息.已填写完整 = true;
            }
            bool IsTrue = true;
            for (int i = 0; i < model.地址信息.Count(); i++)
            {
                if (!ModelState.IsValidField("地址信息[" + i.ToString() + "].地址类型") || !ModelState.IsValidField("地址信息[" + i.ToString() + "].省份")
                    || !ModelState.IsValidField("地址信息[" + i.ToString() + "].城市") || !ModelState.IsValidField("地址信息[" + i.ToString() + "].区县")
                    || !ModelState.IsValidField("地址信息[" + i.ToString() + "].街道") || !ModelState.IsValidField("地址信息[" + i.ToString() + "].邮政编码"))
                {
                    IsTrue = false;
                    break;
                }
            }
            if (IsTrue)
            {
                Newmodel.审核数据.审核状态 = 审核状态.未审核;
                return 用户管理.更新用户<供应商>(Newmodel)
                    ? Content("<script>if(confirm('请确保联系人信息已完善，点击确定请等待审核，点击取消继续完善。')){window.location='/供应商后台/Gys_Manage';} else{window.location='/供应商后台/Vip_Manage';}</script>")
                    : Content("<script>window.location='/供应商后台/Vip_Manage';</script>");
                ;
            }
            else
            {
                return Redirect("/供应商后台/Vip_Manage");
            }
        }
        public ActionResult Part_Vip_Manage()
        {
            供应商 model = 用户管理.查找用户<供应商>(currentUser.Id);
            return PartialView("Gys_Part/Part_Vip_Manage", model);
        }

        public string CheckUser()
        {
            string name = Request.QueryString["n"];
            long id = 用户管理.检查用户是否存在<供应商>(name);
            if (id == -1)
            {
                return "用户不存在！";
            }
            else
            {
                return "";
            }
        }
        [HttpPost]
        public ActionResult Vip_Pwd_Manage()//供应商密码修改
        {
            try
            {
                string p = Request.Form["p"].ToString();
                string p1 = Request.Form["p1"].ToString();
                string photo = Request.Form["pic"].ToString();
                供应商 model = 用户管理.查找用户<供应商>(currentUser.Id);
                if (p1 != p || p == "")
                {
                    return Content("<script>alert('两次密码不一致，请重新修改');window.location='/供应商后台/Vip_Password_Manage'</script>");
                }
                else
                {
                    用户管理.修改登录密码<供应商>(model.Id, Request.Form["p"].ToString());
                    供应商 Nmodel = 用户管理.查找用户<供应商>(currentUser.Id);
                    if (!string.IsNullOrWhiteSpace(photo))
                        Nmodel.登录信息.头像 = photo;
                    用户管理.更新用户<供应商>(Nmodel);
                    return Content("<script>alert('密码修改成功！请重新登录');window.location='/登录/LogOut';</script>");
                }
            }
            catch
            {
                return Redirect("/供应商后台/Vip_Password_Manage");
            }
        }
        public ActionResult Part_Vip_Pwd_Manage()//密码管理局部页
        {
            供应商 Newmodel = 用户管理.查找用户<供应商>(currentUser.Id);
            return PartialView("Gys_Part/Part_Vip_Pwd_Manage", Newmodel);
        }
        public ActionResult Part_Gys_Manage()//供应商基本信息管理部分视图
        {
            供应商 user = 用户管理.查找用户<供应商>(currentUser.Id);
            if (user.供应商用户信息.供应商图片 != null && user.供应商用户信息.供应商图片.Count() != 0)
            {
                for (int i = 0; i < user.供应商用户信息.供应商图片.Count(); i++)
                {
                    if (!System.IO.File.Exists(Server.MapPath(@user.供应商用户信息.供应商图片[i])))
                    {
                        user.供应商用户信息.供应商图片.RemoveAt(i);
                    }
                }
            }
            用户管理.更新用户<供应商>(user);
            deleteIndex("/Lucene.Net/IndexDic/Gys", currentUser.Id.ToString());
            //deleteIndex_ProductCatalog("/Lucene.Net/IndexDic/GysCatalog", user.企业基本信息.企业名称);
            //ViewData["行业列表"] = 商品分类管理.查找子分类();
            return PartialView("Gys_Part/Part_Gys_Manage", user);
        }
        public ActionResult Part_Gys_Product_AddStep1()//供应商基本信息管理部分视图
        {
            //判断是添加中标商品还是一般商品
            var isbid = Request.Params["id"];
            if (isbid == "0")
            {
                ViewData["是否中标商品"] = 0;
                var firstclass = 用户管理.查找用户<供应商>(currentUser.Id).可提供产品类别列表;
                return PartialView("Gys_Part/Part_Gys_Product_AddStep1", firstclass);
            }
            else
            {
                ViewData["是否中标商品"] = 1;
                var allclass = 商品分类管理.查找子分类();
                return PartialView("Gys_Part/Part_Gys_Product_AddStep1", allclass);
            }
            
            //ViewData["类别列表"] = 用户管理.查找用户<供应商>(currentUser.Id).可提供产品类别列表;

            //return PartialView("Gys_Part/Part_Gys_Product_AddStep1", firstclass);
        }
        public ActionResult Gys_Product_AddStep1()//供应商基本信息管理部分视图
        {
            return View();
        }
        [HttpPost]
        public ActionResult Financial_Manage(供应商 model)//财务税务处理
        {
            供应商 Newmodel = 用户管理.查找用户<供应商>(currentUser.Id);
            if (Newmodel.审核数据.审核状态 == 审核状态.审核通过)
            {
                return Content("<script>alert('审核通过的用户不能再修改信息！');window.location='/供应商后台/';</script>");
            }
            Newmodel.财务状况信息.Add(model.财务状况信息[0]);
            Newmodel.审核数据.审核状态 = 审核状态.未审核;
            deleteIndex("/Lucene.Net/IndexDic/Gys", Newmodel.Id.ToString());
            //deleteIndex_ProductCatalog("/Lucene.Net/IndexDic/GysCatalog", model.企业基本信息.企业名称);
            if (Newmodel.财务状况信息 != null && Newmodel.财务状况信息.Count() != 0)
            {
                foreach (var item in Newmodel.财务状况信息)
                {
                    if (string.IsNullOrWhiteSpace(item.资产总额.ToString()) ||
                       string.IsNullOrWhiteSpace(item.负债总额.ToString()) ||
                       string.IsNullOrWhiteSpace(item.净利润.ToString()) ||
                       string.IsNullOrWhiteSpace(item.年份.ToString()) ||
                       string.IsNullOrWhiteSpace(item.销售收入.ToString()))
                    {
                        item.已填写完整 = false;
                        break;
                    }
                    else
                    {
                        item.已填写完整 = true;
                    }
                }
            }
            return 用户管理.更新用户<供应商>(Newmodel)
                ? Content("<script>if(confirm('请确保财务信息以完善，点击确定请等待审核，点击取消继续添加。')){window.location='/供应商后台/Tax_Management';} else{window.location='/供应商后台/Gys_Financial_Manage';}</script>")
                : Content("<script>window.location='/供应商后台/Gys_Financial_Manage';</script>")
                ;
        }
        public ActionResult Delete_Finance()
        {
            int index = int.Parse(Request.QueryString["index"]);
            供应商 Newmodel = 用户管理.查找用户<供应商>(currentUser.Id);
            Newmodel.财务状况信息.RemoveAt(index);
            deleteIndex("/Lucene.Net/IndexDic/Gys", Newmodel.Id.ToString());
            //deleteIndex_ProductCatalog("/Lucene.Net/IndexDic/GysCatalog", Newmodel.企业基本信息.企业名称);
            return 用户管理.更新用户<供应商>(Newmodel)
                ? Content("<script>alert('删除成功！');window.location='/供应商后台/Gys_Financial_Manage';</script>")
                : Content("<script>window.location='/供应商后台/Gys_Financial_Manage';</script>")
                ;
        }
        public ActionResult Modify_Finance()
        {
            try
            {
                int index = int.Parse(Request.QueryString["index"]);
                供应商 Newmodel = 用户管理.查找用户<供应商>(currentUser.Id);
                if (index > Newmodel.财务状况信息.Count() || index < 0)
                {
                    return Redirect("/供应商后台/Gys_Financial_Manage");
                }
                ViewData["index"] = index;
                return View(Newmodel);
            }
            catch
            {
                return Redirect("/供应商后台/Gys_Financial_Manage");
            }
        }
        [HttpPost]
        public ActionResult Modify_finance(供应商 model)
        {
            int index = int.Parse(Request.Form["index"].ToString());
            供应商 Newmodel = 用户管理.查找用户<供应商>(currentUser.Id);
            if (Newmodel.审核数据.审核状态 == 审核状态.审核通过)
            {
                return Content("<script>alert('审核通过的用户不能再修改信息！');window.location='/供应商后台/';</script>");
            }
            if (index > Newmodel.财务状况信息.Count() || index < 0)
            {
                return Redirect("/供应商后台/Gys_Financial_Manage");
            }
            Newmodel.财务状况信息[index] = model.财务状况信息[0];
            if (Newmodel.财务状况信息 != null && Newmodel.财务状况信息.Count() != 0)
            {
                foreach (var item in Newmodel.财务状况信息)
                {
                    if (string.IsNullOrWhiteSpace(item.资产总额.ToString()) ||
                       string.IsNullOrWhiteSpace(item.负债总额.ToString()) ||
                       string.IsNullOrWhiteSpace(item.净利润.ToString()) ||
                       string.IsNullOrWhiteSpace(item.年份.ToString()) ||
                       string.IsNullOrWhiteSpace(item.销售收入.ToString()))
                    {
                        item.已填写完整 = false;
                        break;
                    }
                    else
                    {
                        item.已填写完整 = true;
                    }
                }
            }
            return 用户管理.更新用户<供应商>(Newmodel)
                ? Content("<script>alert('修改成功！');window.location='/供应商后台/Gys_Financial_Manage';</script>")
                : Content("<script>window.location='/供应商后台/Gys_Financial_Manage';</script>")
                ;
        }
        public ActionResult Modify_Investor()
        {
            try
            {

                int index = int.Parse(Request.QueryString["index"]);
                供应商 Newmodel = 用户管理.查找用户<供应商>(currentUser.Id);
                if (index > Newmodel.出资人或股东信息.Count() || index < 0)
                {
                    return Redirect("/供应商后台/Investor_Management");
                }
                ViewData["index"] = index;
                return View(Newmodel);
            }
            catch
            {
                return Redirect("/供应商后台/Investor_Management");
            }
        }
        [HttpPost]
        public ActionResult Modify_investor(供应商 model)
        {
            try
            {
                供应商 Newmodel = 用户管理.查找用户<供应商>(currentUser.Id);
                if (Newmodel.审核数据.审核状态 == 审核状态.审核通过)
                {
                    return Content("<script>alert('审核通过的用户不能再修改信息！');window.location='/供应商后台/';</script>");
                }
                int index = int.Parse(Request.Form["index"].ToString());
                if (index > Newmodel.出资人或股东信息.Count() || index < 0)
                {
                    return Redirect("/供应商后台/Investor_Management");
                }
                Newmodel.出资人或股东信息[index] = model.出资人或股东信息[0];
                if (Newmodel.出资人或股东信息 != null && Newmodel.出资人或股东信息.Count() != 0)
                {
                    foreach (var item in Newmodel.出资人或股东信息)
                    {
                        if (string.IsNullOrWhiteSpace(item.出资人或股东姓名) ||
                           string.IsNullOrWhiteSpace(item.出资人或股东性质) ||
                            string.IsNullOrWhiteSpace(item.身份证号码))
                        {
                            item.已填写完整 = false;
                            break;
                        }
                        else
                        {
                            item.已填写完整 = true;
                        }
                    }
                }
                return 用户管理.更新用户<供应商>(Newmodel)
                    ? Content("<script>alert('修改成功！');window.location='/供应商后台/Investor_Management';</script>")
                    : Content("<script>window.location='/供应商后台/Investor_Management';</script>")
                    ;
            }
            catch
            {
                return Redirect("/供应商后台/Investor_Management");
            }
        }
        public ActionResult Delete_Investor()
        {
            供应商 Newmodel = 用户管理.查找用户<供应商>(currentUser.Id);
            if (Newmodel.审核数据.审核状态 == 审核状态.审核通过)
            {
                return Content("<script>alert('审核通过的用户不能再修改信息！');window.location='/供应商后台/';</script>");
            }
            int index = int.Parse(Request.QueryString["index"]);
            if (index > Newmodel.出资人或股东信息.Count() || index < 0)
            {
                return Redirect("/供应商后台/Investor_Management");
            }
            Newmodel.出资人或股东信息.RemoveAt(index);
            deleteIndex("/Lucene.Net/IndexDic/Gys", Newmodel.Id.ToString());
            //deleteIndex_ProductCatalog("/Lucene.Net/IndexDic/GysCatalog", Newmodel.企业基本信息.企业名称);
            return 用户管理.更新用户<供应商>(Newmodel)
                ? Content("<script>alert('删除成功！');window.location='/供应商后台/Investor_Management';</script>")
                : Content("<script>window.location='/供应商后台/Investor_Management';</script>")
                ;
        }
        public ActionResult Delete_Servicdepartment()
        {
            try
            {
                供应商 Newmodel = 用户管理.查找用户<供应商>(currentUser.Id);
                if (Newmodel.审核数据.审核状态 == 审核状态.审核通过)
                {
                    return Content("<script>alert('审核通过的用户不能再修改信息！');window.location='/供应商后台/';</script>");
                }
                int index = int.Parse(Request.QueryString["index"]);
                if (index > Newmodel.售后服务机构列表.Count() || index < 0)
                {
                    return Redirect("/供应商后台/Service_Management");
                }
                Newmodel.售后服务机构列表.RemoveAt(index);
                deleteIndex("/Lucene.Net/IndexDic/Gys", Newmodel.Id.ToString());
                //deleteIndex_ProductCatalog("/Lucene.Net/IndexDic/GysCatalog", Newmodel.企业基本信息.企业名称);
                return 用户管理.更新用户<供应商>(Newmodel)
                    ? Content("<script>alert('删除成功！');window.location='/供应商后台/Service_Management';</script>")
                    : Content("<script>window.location='/供应商后台/Service_Management';</script>")
                    ;
            }
            catch
            {
                return Redirect("/供应商后台/Service_Management");
            }
        }
        public ActionResult Modify_Servicdepartment()
        {
            try
            {
                int index = int.Parse(Request.QueryString["index"]);
                供应商 Newmodel = 用户管理.查找用户<供应商>(currentUser.Id);
                if (index > Newmodel.售后服务机构列表.Count() || index < 0)
                {
                    return Redirect("/供应商后台/Service_Management");
                }
                ViewData["index"] = index;
                return View(Newmodel);
            }
            catch
            {
                return Redirect("/供应商后台/Service_Management");
            }
        }
        [HttpPost]
        public ActionResult Modify_servicdepartment(供应商 model)
        {
            try
            {
                if(currentUser.审核数据.审核状态== 审核状态.审核通过)
                {
                    return Content("<script>alert('审核通过的用户不能再修改信息！');window.location='/供应商后台/';</script>");
                }
                int index = int.Parse(Request.Form["index"].ToString());
                供应商 Newmodel = 用户管理.查找用户<供应商>(currentUser.Id);
                if (index > Newmodel.售后服务机构列表.Count() || index < 0)
                {
                    return Redirect("/供应商后台/Service_Management");
                }
                Newmodel.售后服务机构列表[index] = model.售后服务机构列表[0];
                Newmodel.审核数据.审核状态 = 审核状态.未审核;
                if (Newmodel.售后服务机构列表 != null && Newmodel.售后服务机构列表.Count() != 0)
                {
                    foreach (var item in Newmodel.售后服务机构列表)
                    {
                        if (string.IsNullOrWhiteSpace(item.售后服务机构名称) || string.IsNullOrWhiteSpace(item.所在地.省份) || string.IsNullOrWhiteSpace(item.所在地.城市) || string.IsNullOrWhiteSpace(item.所在地.区县) || string.IsNullOrWhiteSpace(item.负责人联系方式.联系人) || string.IsNullOrWhiteSpace(item.负责人联系方式.手机))
                        {
                            item.已填写完整 = false;
                            break;
                        }
                        else
                        {
                            item.已填写完整 = true;
                        }
                    }
                }
                return 用户管理.更新用户<供应商>(Newmodel)
                    ? Content("<script>alert('修改成功！');window.location='/供应商后台/Service_Management';</script>")
                    : Content("<script>window.location='/供应商后台/Service_Management';</script>")
                    ;
            }
            catch
            {
                return Redirect("/供应商后台/Service_Management");
            }
        }

        public class TplData
        {
            public long Id;
            public List<List<DataContent>> datalist;
            public class DataContent
            {
                public 商品 p;
                public string imgDir;
            }
            public TplData(List<List<Tuple<商品, string>>> data)
            {
                datalist = new List<List<DataContent>>();
                foreach (var data2 in data)
                {
                    var dlist = new List<DataContent>();
                    datalist.Add(dlist);
                    foreach (var tuple in data2)
                        dlist.Add(new DataContent { p = tuple.Item1, imgDir = tuple.Item2 });
                }
            }
        }
        [HttpPost]
        public ActionResult UploadGood()
        {
            HttpPostedFileBase file = Request.Files[0];
            if (file != null)
            {
                if (file.ContentType == "application/vnd.ms-excel" || file.ContentType == "application/vnd.openxmlformats-officedocument.wordprocessingml.document" || file.ContentType == "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
                {
                    //string fname = UploadAttachment(file);
                    string fileName = file.FileName;
                    if (!string.IsNullOrWhiteSpace(fileName))
                    {
                        string filePath = 上传管理.获取上传路径<商品>(媒体类型.文档, 路径类型.服务器本地路径);

                        //extension = fileData.FileName.Substring(fileData.FileName.LastIndexOf("."), (fileData.FileName.Length - fileData.FileName.LastIndexOf(".")));
                        if (fileName.LastIndexOf("\\") > -1)
                        {
                            fileName = fileName.Substring(fileName.LastIndexOf("\\") + 1);
                        }
                        fileName = 上传管理.获取文件名(fileName).Replace("{", "").Replace("}", "");
                        if (!Directory.Exists(filePath))
                        {
                            Directory.CreateDirectory(filePath);
                        }

                        file.SaveAs(string.Format("{0}\\{1}", filePath, fileName));
                        var ret = TemplateExcel.读取商品(filePath + fileName, currentUser.Id);
                        var dt = new TplData(ret) { Id = Mongo.NextId<TplData>() };
                        Mongo.Coll<TplData>().Save(dt);
                        System.IO.File.Delete(filePath + fileName);
                        return Content("<input type='hidden' id='pldata_id' value='" + dt.Id + "'/><span style='color:#31BD3C'>上传成功!</span><script>window.parent.document.getElementById('waitfor').style.display='none';</script>");
                    }
                    else
                    {
                        return Content("<span style='color:red'>上传失败!请检查文件是否为excel文件.</span><script>window.parent.document.getElementById('waitfor').style.display='none';</script>");
                    }
                }
            }
            return Content("<span style='color:red'>上传失败!请检查文件是否为excel文件.</span><script>window.parent.document.getElementById('waitfor').style.display='none';</script>");
        }

        class TplPic
        {
            public long Id;
            public string path;
        }
        [HttpPost]
        public ActionResult UploadPicures()
        {
            HttpPostedFileBase file = Request.Files[0];
            if (file != null)
            {
                //string fname = UploadAttachment(file);
                string fileName = file.FileName;
                if (!string.IsNullOrWhiteSpace(fileName))
                {
                    string filePath = 上传管理.获取上传路径<商品>(媒体类型.文档, 路径类型.服务器本地路径);

                    //extension = fileData.FileName.Substring(fileData.FileName.LastIndexOf("."), (fileData.FileName.Length - fileData.FileName.LastIndexOf(".")));
                    if (fileName.LastIndexOf("\\") > -1)
                    {
                        fileName = fileName.Substring(fileName.LastIndexOf("\\") + 1);
                    }
                    fileName = 上传管理.获取文件名(fileName).Replace("{", "").Replace("}", "");
                    if (!Directory.Exists(filePath))
                    {
                        Directory.CreateDirectory(filePath);
                    }

                    file.SaveAs(string.Format("{0}\\{1}", filePath, fileName));
                    if (!ZipFile.IsZipFile(filePath + fileName))
                    {
                        System.IO.File.Delete(filePath + fileName);
                        return Content("<span style='color:red'>请上传格式为zip的压缩包</span><script>window.parent.document.getElementById('waitfor').style.display='none';</script>");
                    }
                    var dt = new TplPic() { Id = Mongo.NextId<TplPic>(), path = filePath + fileName };
                    Mongo.Coll<TplPic>().Save(dt);
                    return Content("<input type='hidden' id='plpic_id' value='" + dt.Id + "'/><span style='color:#31BD3C'>上传成功!</span><script>window.parent.document.getElementById('waitfor').style.display='none';</script>");
                }
            }
            return Content("<span style='color:red'>请上传格式为zip的压缩包</span><script>window.parent.document.getElementById('waitfor').style.display='none';</script>");
        }
        //[AllowAnonymous]
        //[HttpPost]
        public ActionResult UploadExcelGood()
        {
            try
            {
                var 图片不正确商品列表 = new List<商品>();
                //已存在商品-excel中
                var 已存在商品 = new List<商品>();
                //已存在商品-数据库中
                var 数据库已存在商品 = new List<商品>();
                var 有效商品 = new List<商品>();
                var gd = Request.Params["datas"];
                var pic = Request.Params["pics"];

                var goods = Mongo.Coll<TplData>().Find(Query.EQ("_id", long.Parse(gd))).FirstOrDefault();
                var pics = Mongo.Coll<TplPic>().Find(Query.EQ("_id", long.Parse(pic))).FirstOrDefault();

                var flag = true;
                var ImgExts = new[] { "jpg", "jpeg", "png", "gif" };

                //判断压缩包是否含相应图片
                var zip = ZipFile.Read(pics.path, new ReadOptions { Encoding = Encoding.Default });
                foreach (var excellist in goods.datalist)
                {
                    foreach (var excel in excellist)
                    {
                        if (!string.IsNullOrWhiteSpace(excel.imgDir))
                        {
                            var count = zip.Entries.Count(zf =>
                                !zf.IsDirectory
                                    && zf.FileName.StartsWith(excel.imgDir + "/")
                                    && ImgExts.Contains((zf.FileName.Split('.').Last().ToLower()))
                                );
                            //var count4 = zip.Entries.Count(zf =>
                            //  !zf.IsDirectory
                            //  && zf.FileName.StartsWith(excel.imgDir + "/")
                            //  && ImgExts.Contains(Path.GetExtension(zf.FileName))
                            //  );

                            if (count == 0 && !图片不正确商品列表.Contains(excel.p))
                            {
                                flag = false;
                                图片不正确商品列表.Add(excel.p);
                            }
                        }
                    }
                }

                //判断是否存在商品
                foreach (var excellist in goods.datalist)
                {
                    if (excellist.Any())
                    {
                        var 商品分类 = excellist[0].p.商品信息.所属商品分类;
                        var 已有该分类商品集合 = 商品管理.查询分类下商品(商品分类.商品分类ID, 0, 0, Query<商品>.Where(o => o.商品信息.所属供应商.用户ID == currentUser.Id));
                        foreach (var excel in excellist)
                        {
                            foreach (var sp in 已有该分类商品集合)
                            {
                                if (!图片不正确商品列表.Contains(excel.p) && !已存在商品.Contains(excel.p) && sp.商品信息.商品名 == excel.p.商品信息.商品名)
                                {
                                    flag = false;
                                    已存在商品.Add(excel.p);
                                    数据库已存在商品.Add(sp);
                                }
                            }
                        }
                    }
                }

                //压缩包图片能对印上，且商品不存在，直接添加商品
                if (flag)
                {
                    //临时文件夹生成GUID文件夹，防止重名
                    var guid = Guid.NewGuid();
                    foreach (var excellist in goods.datalist)
                    {
                        foreach (var excel in excellist)
                        {
                            var sp = excel.p;
                            var 图片列表 = zip.Entries.Where(zf =>
                                !zf.IsDirectory
                                && zf.FileName.StartsWith(excel.imgDir + "/")
                                && ImgExts.Contains((zf.FileName.Split('.').Last().ToLower()))
                                );
                            foreach (var entry in 图片列表)
                            {
                                //Extract解压zip文件包的方法，参数是保存解压后文件的路基
                                entry.Extract(@"D:\TempPic\" + guid + "\\");

                                string filePath = 上传管理.获取上传路径<商品>(媒体类型.图片, 路径类型.服务器本地路径);
                                string filePath1 = 上传管理.获取上传路径<商品>(媒体类型.图片, 路径类型.不带域名根路径);
                                string fileName = 上传管理.获取文件名(entry.FileName.Split('/').Last()).Replace("{", "").Replace("}", "");
                                if (fileName.LastIndexOf("\\") > -1)
                                {
                                    fileName = fileName.Substring(fileName.LastIndexOf("\\") + 1);
                                }
                                string originalpath = string.Format("{0}original", filePath);//原图路径
                                if (!Directory.Exists(originalpath))
                                {
                                    Directory.CreateDirectory(originalpath);
                                }
                                //保存原图
                                System.IO.File.Move(@"D:\TempPic\" + guid + "\\" + entry.FileName, string.Format("{0}\\{1}", originalpath, fileName));

                                //缩略图350*350
                                UploadPic.MakeThumbnail(string.Format("{0}\\{1}", originalpath, fileName), string.Format("{0}350X350\\{1}", filePath, fileName), 350, 350, "Cut");

                                //缩略图150*150
                                UploadPic.MakeThumbnail(string.Format("{0}\\{1}", originalpath, fileName), string.Format("{0}150X150\\{1}", filePath, fileName), 150, 150, "Cut");

                                //缩略图50*50
                                UploadPic.MakeThumbnail(string.Format("{0}\\{1}", originalpath, fileName), string.Format("{0}50X50\\{1}", filePath, fileName), 50, 50, "Cut");

                                //图片的相对路径，存入数据库的值
                                var path = filePath1 + "original/" + fileName;
                                sp.商品信息.商品图片.Add(path);
                            }
                            商品管理.添加商品(sp, sp.商品信息.所属商品分类.商品分类ID, currentUser.Id);
                        }
                    }
                    return Content("导入成功！");
                }
                else
                {
                    foreach (var excellist in goods.datalist)
                    {
                        foreach (var excel in excellist)
                        {
                            if (!已存在商品.Contains(excel.p) && !有效商品.Contains(excel.p) && !图片不正确商品列表.Contains(excel.p))
                            {
                                有效商品.Add(excel.p);
                            }
                        }
                    }
                    ViewData["图片不正确商品列表"] = 图片不正确商品列表;
                    ViewData["已存在商品"] = 已存在商品;
                    ViewData["有效商品"] = 有效商品;
                    ViewData["数据库已存在商品"] = 数据库已存在商品;
                    return PartialView("/Views/默认主题/后台/供应商后台/Gys_Part/Part_UploadExcel.cshtml");
                }
            }
            catch
            {
                return Content("导入失败！请检查数据是否完整、正确。");
            }
        }

        /// <summary>
        /// 更新存在的商品
        /// </summary>
        public void UpdateExistGood()
        {
            var gd = Request.Params["datas"];
            var pic = Request.Params["pics"];
            var id = Request.Params["id"];
            var goods = Mongo.Coll<TplData>().Find(Query.EQ("_id", long.Parse(gd))).FirstOrDefault();
            var pics = Mongo.Coll<TplPic>().Find(Query.EQ("_id", long.Parse(pic))).FirstOrDefault();

            var ImgExts = new[] { "jpg", "jpeg", "png", "gif" };
            var 需更新商品的商品名列表 = new List<string>();
            var idlist = id.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var item in idlist)
            {
                需更新商品的商品名列表.Add(商品管理.查找商品(long.Parse(item)).商品信息.商品名);
            }
            var sortnum = 0;
            var zip = ZipFile.Read(pics.path, new ReadOptions { Encoding = Encoding.Default });
            var guid = Guid.NewGuid();
            foreach (var item in 需更新商品的商品名列表)
            {
                foreach (var excellist in goods.datalist)
                {
                    foreach (var excel in excellist)
                    {
                        if (item == excel.p.商品信息.商品名)
                        {
                            var sp_id = long.Parse(idlist[sortnum]);
                            //删除商品原图片
                            var 原商品图片 = 商品管理.查找商品(sp_id).商品信息.商品图片;
                            foreach (var sp_pic in 原商品图片)
                            {
                                try
                                {
                                    UploadPic.DelPic(string.Format("{0}", sp_pic));
                                    UploadPic.DelPic(string.Format("{0}", sp_pic.Replace("original", "350X350")));
                                    UploadPic.DelPic(string.Format("{0}", sp_pic.Replace("original", "150X150")));
                                    UploadPic.DelPic(string.Format("{0}", sp_pic.Replace("original", "50X50")));
                                }
                                catch
                                {

                                }
                            }
                            var sp = excel.p;
                            sp.Id = sp_id;
                            var 图片列表 = zip.Entries.Where(zf =>
                                    !zf.IsDirectory
                                    && zf.FileName.StartsWith(excel.imgDir + "/")
                                    && ImgExts.Contains((zf.FileName.Split('.').Last().ToLower()))
                                    );
                            foreach (var entry in 图片列表)
                            {
                                //Extract解压zip文件包的方法，参数是保存解压后文件的路基
                                entry.Extract(@"D:\TempPic\" + guid + "\\");

                                string filePath = 上传管理.获取上传路径<商品>(媒体类型.图片, 路径类型.服务器本地路径);
                                string filePath1 = 上传管理.获取上传路径<商品>(媒体类型.图片, 路径类型.不带域名根路径);
                                string fileName = 上传管理.获取文件名(entry.FileName.Split('/').Last()).Replace("{", "").Replace("}", "");
                                if (fileName.LastIndexOf("\\") > -1)
                                {
                                    fileName = fileName.Substring(fileName.LastIndexOf("\\") + 1);
                                }
                                string originalpath = string.Format("{0}original", filePath);//原图路径
                                if (!Directory.Exists(originalpath))
                                {
                                    Directory.CreateDirectory(originalpath);
                                }
                                //保存原图
                                System.IO.File.Move(@"D:\TempPic\" + guid + "\\" + entry.FileName, string.Format("{0}\\{1}", originalpath, fileName));

                                //缩略图350*350
                                UploadPic.MakeThumbnail(string.Format("{0}\\{1}", originalpath, fileName), string.Format("{0}350X350\\{1}", filePath, fileName), 350, 350, "Cut");

                                //缩略图150*150
                                UploadPic.MakeThumbnail(string.Format("{0}\\{1}", originalpath, fileName), string.Format("{0}150X150\\{1}", filePath, fileName), 150, 150, "Cut");

                                //缩略图50*50
                                UploadPic.MakeThumbnail(string.Format("{0}\\{1}", originalpath, fileName), string.Format("{0}50X50\\{1}", filePath, fileName), 50, 50, "Cut");

                                //图片的相对路径，存入数据库的值
                                var path = filePath1 + "original/" + fileName;
                                sp.商品信息.商品图片.Add(path);
                            }
                            商品管理.更新商品(sp);
                            sortnum++;
                        }
                    }
                }
            }
        }
        /// <summary>
        /// 导入与图片一致的商品
        /// </summary>
        public void ImportGood()
        {
            var 图片不正确商品列表 = new List<商品>();
            //已存在商品-excel中
            var 已存在商品 = new List<商品>();
            var 有效商品 = new List<商品>();

            var gd = Request.Params["datas"];
            var pic = Request.Params["pics"];

            var goods = Mongo.Coll<TplData>().Find(Query.EQ("_id", long.Parse(gd))).FirstOrDefault();
            var pics = Mongo.Coll<TplPic>().Find(Query.EQ("_id", long.Parse(pic))).FirstOrDefault();

            var ImgExts = new[] { "jpg", "jpeg", "png", "gif" };

            //判断压缩包是否含相应图片
            var zip = ZipFile.Read(pics.path, new ReadOptions { Encoding = Encoding.Default });

            foreach (var excellist in goods.datalist)
            {
                foreach (var excel in excellist)
                {
                    if (!string.IsNullOrWhiteSpace(excel.imgDir))
                    {
                        var count = zip.Entries.Count(zf =>
                            !zf.IsDirectory
                            && zf.FileName.StartsWith(excel.imgDir + "/")
                            && ImgExts.Contains((zf.FileName.Split('.').Last().ToLower()))
                            );
                        //var count4 = zip.Entries.Count(zf =>
                        //  !zf.IsDirectory
                        //  && zf.FileName.StartsWith(excel.imgDir + "/")
                        //  && ImgExts.Contains(Path.GetExtension(zf.FileName))
                        //  );

                        if (count == 0 && !图片不正确商品列表.Contains(excel.p))
                        {
                            图片不正确商品列表.Add(excel.p);
                        }
                    }
                }
            }

            //判断是否存在商品
            foreach (var excellist in goods.datalist)
            {
                if (excellist.Any())
                {
                    var 商品分类 = excellist[0].p.商品信息.所属商品分类;
                    var 已有该分类商品集合 = 商品管理.查询分类下商品(商品分类.商品分类ID, 0, 0, Query<商品>.Where(o => o.商品信息.所属供应商.用户ID == currentUser.Id));
                    foreach (var excel in excellist)
                    {
                        foreach (var sp in 已有该分类商品集合)
                        {
                            if (!图片不正确商品列表.Contains(excel.p) && !已存在商品.Contains(excel.p) && sp.商品信息.商品名 == excel.p.商品信息.商品名)
                            {
                                已存在商品.Add(excel.p);
                            }
                        }
                    }
                }
            }

            foreach (var excellist in goods.datalist)
            {
                foreach (var excel in excellist)
                {
                    if (!已存在商品.Contains(excel.p) && !有效商品.Contains(excel.p) && !图片不正确商品列表.Contains(excel.p))
                    {
                        有效商品.Add(excel.p);
                    }
                }
            }

            //临时文件夹生成GUID文件夹，防止重名
            var guid = Guid.NewGuid();
            foreach (var excellist in goods.datalist)
            {
                foreach (var excel in excellist)
                {
                    if (有效商品.Contains(excel.p))
                    {
                        var sp = excel.p;
                        var 图片列表 = zip.Entries.Where(zf =>
                            !zf.IsDirectory
                            && zf.FileName.StartsWith(excel.imgDir + "/")
                            && ImgExts.Contains((zf.FileName.Split('.').Last().ToLower()))
                            );
                        foreach (var entry in 图片列表)
                        {
                            //Extract解压zip文件包的方法，参数是保存解压后文件的路基
                            entry.Extract(@"D:\TempPic\" + guid + "\\");

                            string filePath = 上传管理.获取上传路径<商品>(媒体类型.图片, 路径类型.服务器本地路径);
                            string filePath1 = 上传管理.获取上传路径<商品>(媒体类型.图片, 路径类型.不带域名根路径);
                            string fileName = 上传管理.获取文件名(entry.FileName.Split('/').Last()).Replace("{", "").Replace("}", "");
                            if (fileName.LastIndexOf("\\") > -1)
                            {
                                fileName = fileName.Substring(fileName.LastIndexOf("\\") + 1);
                            }
                            string originalpath = string.Format("{0}original", filePath);//原图路径
                            if (!Directory.Exists(originalpath))
                            {
                                Directory.CreateDirectory(originalpath);
                            }
                            //保存原图
                            System.IO.File.Move(@"D:\TempPic\" + guid + "\\" + entry.FileName, string.Format("{0}\\{1}", originalpath, fileName));

                            //缩略图350*350
                            UploadPic.MakeThumbnail(string.Format("{0}\\{1}", originalpath, fileName), string.Format("{0}350X350\\{1}", filePath, fileName), 350, 350, "Cut");

                            //缩略图150*150
                            UploadPic.MakeThumbnail(string.Format("{0}\\{1}", originalpath, fileName), string.Format("{0}150X150\\{1}", filePath, fileName), 150, 150, "Cut");

                            //缩略图50*50
                            UploadPic.MakeThumbnail(string.Format("{0}\\{1}", originalpath, fileName), string.Format("{0}50X50\\{1}", filePath, fileName), 50, 50, "Cut");

                            //图片的相对路径，存入数据库的值
                            var path = filePath1 + "original/" + fileName;
                            sp.商品信息.商品图片.Add(path);
                        }
                        商品管理.添加商品(sp, sp.商品信息.所属商品分类.商品分类ID, currentUser.Id);
                    }
                }
            }
        }
        /// <summary>
        /// 下载模板
        /// </summary>
        public void SealTemp()
        {
            var id = Request.QueryString["id"];
            var idlist = id.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
            long[] idcollec = new long[idlist.Length];
            for (int i = 0; i < idlist.Length; i++)
            {
                idcollec[i] = long.Parse(idlist[i]);
            }
            Response.ContentType = "application/ms-excel";
            Response.Charset = "GB2312";
            Response.ContentEncoding = System.Text.Encoding.UTF8;
            Response.AddHeader("Content-Disposition", "attachment; filename=" + Server.UrlEncode("Template.xls"));
            var binData = TemplateExcel.xls.商品分类(idcollec);
            Response.OutputStream.Write(binData, 0, binData.Length);
            Response.Flush();
            Response.End();
        }

        public void SealWithText()
        {
            try
            {
                string txt = Request.QueryString["txt"];
                string basestr = Request.QueryString["base"];
                string id = Request.QueryString["id"];
                Regex reg = new Regex("[0-9,a-z,A-Z]{7,}");
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
                    if (long.Parse(id) == 10005 || long.Parse(id) == 10009 || long.Parse(id) == 10008 || long.Parse(id) == 10013 || long.Parse(id) == 20151 || long.Parse(id) == 20150 || long.Parse(id) == 20145 || long.Parse(id) == 20146 || long.Parse(id) == 20137 || long.Parse(id) == 20138 || long.Parse(id) == 20139 || long.Parse(id) == 20140 || long.Parse(id) == 20141 || long.Parse(id) == 20142 || long.Parse(id) == 20143 || long.Parse(id) == 20144 || long.Parse(id) == 20152 || long.Parse(id) == 20149 || long.Parse(id) == 20147 || long.Parse(id) == 20148 || long.Parse(id) == 20261 || long.Parse(id)==20254)
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
            }
        }

        public static bool GetQRCode(string strContent, MemoryStream ms)
        {
            ErrorCorrectionLevel Ecl = ErrorCorrectionLevel.M; //误差校正水平   
            string Content = strContent;//待编码内容  
            QuietZoneModules QuietZones = QuietZoneModules.Two;  //空白区域   
            int ModuleSize = 2;//大小  
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



        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Gys_Info_Edit(供应商 model)
        {
            供应商 Newmodel = 用户管理.查找用户<供应商>(currentUser.Id);
            if (Newmodel.审核数据.审核状态 == 审核状态.审核通过)
            {
                return Content("<script>alert('审核通过的用户不能再修改信息！');window.location='/供应商后台/';</script>");
            }
            model.供应商用户信息.广告商品 = Newmodel.供应商用户信息.广告商品;
            List<string> img = null;
            if (model.供应商用户信息.所属管理单位 == 0)
            {
                model.供应商用户信息.所属管理单位 = 供应商.采购管理单位.成都军区物资采购机构_成都;
            }
            string[] url = Request.Form["picture"].ToString().Split('|');
            Newmodel.供应商用户信息.供应商图片.Clear();
            for (int i = 0; i < url.Length - 1;i++ )
            {
                Newmodel.供应商用户信息.供应商图片.Add(url[i]);
            }
            img = Newmodel.供应商用户信息.供应商图片;
            供应商._供应商用户信息._U盾信息 u = Newmodel.供应商用户信息.U盾信息;
            Newmodel.企业基本信息 = model.企业基本信息;
            if (model.所属地域.省份 == "不限省份")
            {
                model.所属地域.省份 = "";
            }
            if (model.所属地域.城市 == "不限城市")
            {
                model.所属地域.城市 = "";
            }
            if (model.所属地域.区县 == "不限区县")
            {
                model.所属地域.区县 = "";
            }
            Newmodel.所属地域 = model.所属地域;
            Newmodel.供应商用户信息 = model.供应商用户信息;
            Newmodel.供应商用户信息.U盾信息 = u;
            Newmodel.供应商用户信息.供应商图片 = img;
            Newmodel.审核数据.审核状态 = 审核状态.未审核;
            if (string.IsNullOrWhiteSpace(Newmodel.企业基本信息.企业名称) || string.IsNullOrWhiteSpace(Newmodel.企业基本信息.注册地址) || string.IsNullOrWhiteSpace(Newmodel.企业基本信息.邮政编码) || Newmodel.企业基本信息.经济性质 == 供应商.经济性质.未设置 || Newmodel.企业基本信息.经营子类型 == 供应商.经营子类型.未填写 || Newmodel.供应商用户信息.供应商图片.Count() == 0)
            {
                Newmodel.企业基本信息.已填写完整 = false;
            }
            else
            {
                Newmodel.企业基本信息.已填写完整 = true;
            }
            deleteIndex("/Lucene.Net/IndexDic/Gys", model.Id.ToString());
            //deleteIndex_ProductCatalog("/Lucene.Net/IndexDic/GysCatalog", model.企业基本信息.企业名称);
            return 用户管理.更新用户<供应商>(Newmodel)
                ? Content("<script>if(confirm('您以完善企业基本信息，点击确定请等待审核，点击取消继续完善。')){window.location='/供应商后台/Law_Person';} else{window.location='/供应商后台/Gys_Manage';}</script>")
                : Content("<script>window.location='/供应商后台/Gys_Manage';</script>")
                ;
        }
        public ActionResult Service_Evaluate()
        {
            //return PartialView("Gys_Part/Service_Evaluate");
            return View();
        }
        public ActionResult Part_Service_Evaluate()
        {
            
            if (currentUser.审核数据.审核状态 == 审核状态.审核通过)
            {
                ViewBag.Shen = true;
            }
            else
            {
                ViewBag.Shen = false;
            }
            ViewData["行业列表"] = 商品分类管理.查找子分类();
            IEnumerable<招标采购项目> l = 招标采购项目管理.查询招标采购项目(0, 0, MongoDB.Driver.Builders.Query.EQ("审核数据.审核状态", 审核状态.审核通过).And(MongoDB.Driver.Builders.Query.NE("中标公告链接.公告ID", -1)));
            ViewData["需求列表"] = l;
            验收单 item = new 验收单();


            var Iysd = new List<验收单>();
            var ysd_number = 验收单管理.查询验收单(0, 0, Query<验收单>.Where(o => o.是否已经打印 && !o.是否作废 && o.供应商链接.用户ID == currentUser.Id));
            foreach (var k in ysd_number)
            {
                if ((k.打印信息.Count > 0 && k.打印信息.Last().打印时间.AddDays(10) > DateTime.Now) || k.扫描件审核状态=="审核未通过")
                {
                    Iysd.Add(k);
                }
            }
            ViewData["验收单号"] = Iysd;
            //TD:先检查用户是否持有U盾，且未过期
            var payysd = "0";
            var gys = 用户管理.查找用户<供应商>(currentUser.Id);
            if (!string.IsNullOrWhiteSpace(gys.供应商用户信息.U盾信息.序列号) && !string.IsNullOrWhiteSpace(gys.供应商用户信息.U盾信息.加密参数) && gys.供应商用户信息.U盾信息.年检结束时间 > DateTime.Now)
            {
                payysd = "1";
            }
            ViewData["id"] = currentUser.Id;
            ViewData["是否持有U盾"] = payysd;
            if (payysd == "1")
            {
                Random randomGenerator = new Random(DateTime.Now.Millisecond);
                String RndStr = "";
                for (int i = 0; i < 32; i++)
                    RndStr += Convert.ToChar(randomGenerator.Next(97, 122));
                ViewData["Message"] = RndStr;
            }
            ViewData["供应商ID"] = currentUser.Id;
            //先检查用户是否持有U盾，且未过期

            return PartialView("Gys_Part/Part_Service_Evaluate", item);
        }

        public class Acceptance
        {
            [Required(ErrorMessage = "请填写验收单位编码")]
            public string 验收单位编码 { get; set; }
            [Required(ErrorMessage = "请填写验收人姓名")]
            public _联系方式 验收人 { get; set; }
            [Required(ErrorMessage = "请填写验收时间")]
            //[RegularExpression("",ErrorMessage="请填写正确的日期")]
            public DateTime 验收时间 { get; set; }
            public 验收单 Y { get; set; }
            public Acceptance()
            {
                this.Y = new 验收单();
            }
        }

        public ActionResult AddAcceptForm()
        {
            var q = Query.Or(Query<单位用户>.In(o => o.Id, new long[] { 10001,20292, 10003, 10004, 10005, 10007, 10008, 10009, 10011, 10012, 10013, 10014, 10015, 20057, 20151, 20150, 20145, 20146, 20137, 20138, 20139, 20140, 20141, 20142, 20143, 20144, 20152, 20149, 20147, 20148, 20261,20254 }));
            ViewData["审核单位列表"] = 用户管理.查询用户<单位用户>(0, 0, q);
            ViewData["商品列表"] = 商品管理.查询供应商商品(currentUser.Id, 0, 0, Query<商品>.Where(o => o.审核数据.审核状态 == 审核状态.审核通过));
            ViewBag.user = currentUser.企业联系人信息.联系人姓名;

            //TD:先检查用户是否持有U盾，且未过期
            var payysd = "0";
            var gys = 用户管理.查找用户<供应商>(currentUser.Id);
            if (!string.IsNullOrWhiteSpace(gys.供应商用户信息.U盾信息.序列号) && !string.IsNullOrWhiteSpace(gys.供应商用户信息.U盾信息.加密参数) && gys.供应商用户信息.U盾信息.年检结束时间 > DateTime.Now)
            {
                payysd = "1";
            }

            ViewData["是否持有U盾"] = payysd;
            if (payysd == "1")
            {
                Random randomGenerator = new Random(DateTime.Now.Millisecond);
                String RndStr = "";
                for (int i = 0; i < 32; i++)
                    RndStr += Convert.ToChar(randomGenerator.Next(97, 122));
                ViewData["Message"] = RndStr;
            }
            ViewData["供应商ID"] = currentUser.Id;
            //先检查用户是否持有U盾，且未过期
            //return PartialView("Gys_Part/AddAcceptForm");
            return View();
        }

        public ActionResult CheckSecurity()
        {
            //TD:先检查用户是否持有U盾，且未过期
            var payysd = "0";
            var gys = 用户管理.查找用户<供应商>(currentUser.Id);
            if (!string.IsNullOrWhiteSpace(gys.供应商用户信息.U盾信息.序列号) && !string.IsNullOrWhiteSpace(gys.供应商用户信息.U盾信息.加密参数) && gys.供应商用户信息.U盾信息.年检结束时间 > DateTime.Now)
            {
                payysd = "1";
            }

            ViewData["是否持有U盾"] = payysd;
            if (payysd == "1")
            {
                Random randomGenerator = new Random(DateTime.Now.Millisecond);
                String RndStr = "";
                for (int i = 0; i < 32; i++)
                    RndStr += Convert.ToChar(randomGenerator.Next(97, 122));
                ViewData["Message"] = RndStr;
            }
            //先检查用户是否持有U盾，且未过期
            ViewData["type"] = Request.QueryString["type"];
            return View();
        }
        [HttpPost]
        public ActionResult CheckUOfYsdx(int? xxx)
        {
            var returnstr = "";
            var flage = false;
            //JS读取的U盾序列号
            string hidid = Request.Params["HidIAID"];// this.HidIAID.Value;
            //前台JS加密后的加密串
            string Digest = Request.Params["HidDigest"];// this.HidDigest.Value;

            var keyValue = currentUser.供应商用户信息.U盾信息.加密参数;
            var Sequencenumber = currentUser.供应商用户信息.U盾信息.序列号;

            //判断JS读取的U盾序列号和数据库的序列号是否匹配
            if (hidid != Sequencenumber)
            {
                returnstr = "U盾信息不匹配！";
            }
            else
            {
                if (keyValue != "" && keyValue != null)
                {
                    //对前台的随机数和数据库的加密参数进行加密
                    string CDigest = Sha1(Request.Params["ssid"] + keyValue);
                    //两个加密串匹配则判断成功
                    if (Digest.Equals(CDigest))
                    {
                        flage = true;
                    }
                    else
                    {
                        returnstr = "登陆失败";
                    }
                }
                else
                {
                    returnstr = "此账号不存在";
                }
            }

            if (flage)
            {
                var type= Request.Params["type"];
                if(string.IsNullOrWhiteSpace(type) || (type!="Add" && type!="Upload" && type!="UploadList")){
                    return Content("参数错误,请不要修改url");
                }
                if(type =="List"){
                    return AddAcceptanceForm();
                }
                else if(type =="Upload"){
                    return Service_Evaluate();
                }
                else if(type =="Upload"){
                    return ProjectService_List();
                }
                else if (type == "Add")
                {
                    return AddAcceptForm();
                }
                return Content("参数错误,请不要修改url");
                //returnstr = "1";
            }
            else
            {
                return Content(returnstr);
            }
           
        }

        [HttpPost]
        public ActionResult CheckUOfYsd(int? xxx)
        {
            var returnstr = "";
            var flage = false;
            //JS读取的U盾序列号
            string hidid = Request.Params["HidIAID"];// this.HidIAID.Value;
            //前台JS加密后的加密串
            string Digest = Request.Params["HidDigest"];// this.HidDigest.Value;

            var keyValue = currentUser.供应商用户信息.U盾信息.加密参数;

            //用户可能持有多个U盾
            var Sequencenumber = currentUser.供应商用户信息.U盾信息.序列号.Split(new char[] {'|'},
                StringSplitOptions.RemoveEmptyEntries);

            //判断JS读取的U盾序列号和数据库的序列号是否匹配
            if (!Sequencenumber.Contains(hidid))
            {
                returnstr = "U盾信息不匹配！";
            }
            else
            {
                if (keyValue != "" && keyValue != null)
                {
                    //对前台的随机数和数据库的加密参数进行加密
                    string CDigest = Sha1(Request.Params["ssid"] + keyValue);
                    //两个加密串匹配则判断成功
                    if (Digest.Equals(CDigest))
                    {
                        flage = true;
                    }
                    else
                    {
                        returnstr = "登陆失败";
                    }
                }
                else
                {
                    returnstr = "此账号不存在";
                }
            }

            if (flage)
            {
                returnstr = "1";
            }
            return Content(returnstr);
        }

        public ActionResult AddAcceptanceForm()
        {
            var q1 = Query<验收单>.Where(o => o.供应商链接.用户ID == currentUser.Id);
            var comes = Request.Params["comes"];
            if (comes == "w")
            {
                q1 = q1.And(Query<验收单>.Where(o => o.审核数据.审核状态 == 审核状态.未审核 && !o.是否作废));
                ViewData["comes"] = "未审核验收单";
            }
            else if (comes == "y")
            {
                q1 = q1.And(Query<验收单>.Where(o => o.审核数据.审核状态 != 审核状态.未审核));
                ViewData["comes"] = "已审核验收单";
            }
            else if (comes == "f")
            {
                q1 = q1.And(Query<验收单>.Where(o => o.是否作废));
                ViewData["comes"] = "已作废验收单";
            }
            else
            {
                ViewData["comes"] = "全部验收单列表";
            }
            //var q = Query.Or(Query.EQ("单位信息.单位名称", "采购处"), Query.EQ("单位信息.单位名称", "13军"), Query.EQ("单位信息.单位名称", "14军"), Query.EQ("单位信息.单位名称", "云南省军区"));
            var q = Query.Or(Query<单位用户>.In(o => o.Id, new long[] { 10003, 10013, 10008, 10011, 10004, 10005, 10015, 10007, 10012, 10014, 10009,20261 }));
            long p = 用户管理.计数用户<单位用户>(0, 0);
            ViewData["商品列表"] = 商品管理.查询供应商商品(currentUser.Id, 0, 0, Query<商品>.Where(o => o.审核数据.审核状态 == 审核状态.审核通过));
            ViewData["审核单位列表"] = 用户管理.查询用户<单位用户>(0, 0, q);


            //ViewData["收货单位列表"] = 用户管理.查询用户<单位用户>(0, 0, Query.EQ("单位信息.单位名称", "成都军区后勤部军需物资油料部采购管理处"));
            //ViewData["收货单位列表"] = 用户管理.查询用户<单位用户>(0, 0, Query.Or(Query<单位用户>.In(o => o.Id, new long[] { 10001 })));
            long listcount = 验收单管理.计数验收单(0, 0, q1);
            long maxpagesize = Math.Max((listcount + ACCEPTA_PAGESIZE - 1) / ACCEPTA_PAGESIZE, 1);

            int page = 1;
            ViewData["currentpage"] = page;
            ViewData["pagecount"] = maxpagesize;
            ViewBag.user = currentUser.企业联系人信息.联系人姓名;
            ViewData["验收单列表"] = 验收单管理.查询验收单(ACCEPTA_PAGESIZE * (page - 1), ACCEPTA_PAGESIZE, q1, false, SortBy.Descending("基本数据.添加时间"));
            if (currentUser.审核数据.审核状态 == 审核状态.审核通过)
            {
                ViewBag.Shen = true;
            }
            else
            {
                ViewBag.Shen = false;
            }

            //TD:先检查用户是否持有U盾，且未过期
            var payysd = "0";
            var gys = 用户管理.查找用户<供应商>(currentUser.Id);
            if (!string.IsNullOrWhiteSpace(gys.供应商用户信息.U盾信息.序列号) && !string.IsNullOrWhiteSpace(gys.供应商用户信息.U盾信息.加密参数) && gys.供应商用户信息.U盾信息.年检结束时间 > DateTime.Now)
            {
                payysd = "1";
            }
            ViewData["是否持有U盾"] = payysd;
            if (payysd == "1")
            {
                Random randomGenerator = new Random(DateTime.Now.Millisecond);
                String RndStr = "";
                for (int i = 0; i < 32; i++)
                    RndStr += Convert.ToChar(randomGenerator.Next(97, 122));
                ViewData["Message"] = RndStr;
            }
            ViewData["供应商ID"] = currentUser.Id;
            var 是否有未上传验收单 = false;
            var cc = 验收单管理.查询验收单(0, 0, Query<验收单>.Where(o => o.供应商链接.用户ID == currentUser.Id && o.是否已经打印 && !o.是否作废 && o.验收单扫描件.Count<=0));
            foreach (var k in cc)
            {
                if (k.打印信息.Count > 0 && k.打印信息.Last().打印时间.AddDays(10) < DateTime.Now)
                {
                    是否有未上传验收单 = true;
                    break;
                }
            }
            ViewData["是否有未上传验收单"] = 是否有未上传验收单;
            //先检查用户是否持有U盾，且未过期
            //return PartialView("Gys_Part/AddAcceptanceForm");
            return View();

        }

        public ActionResult AcceptanceTicketList()
        {
            long listcount = 机票验收单管理.计数机票验收单(0, 0, Query<机票验收单>.Where(o => o.供应商链接.用户ID == currentUser.Id));
            long maxpagesize = Math.Max((listcount + ACCEPTA_PAGESIZE - 1) / ACCEPTA_PAGESIZE, 1);

            int page = 1;
            ViewData["currentpage"] = page;
            ViewData["pagecount"] = maxpagesize;
            ViewData["验收单列表"] = 机票验收单管理.查询机票验收单(ACCEPTA_PAGESIZE * (page - 1), ACCEPTA_PAGESIZE, Query<机票验收单>.Where(o=>o.供应商链接.用户ID==currentUser.Id && !o.是否作废), false, SortBy.Descending("基本数据.添加时间"));
            if (currentUser.审核数据.审核状态 == 审核状态.审核通过)
            {
                ViewBag.Shen = true;
            }
            else
            {
                ViewBag.Shen = false;
            }

            var payysd = "0";
            var gys = 用户管理.查找用户<供应商>(currentUser.Id);
            if (!string.IsNullOrWhiteSpace(gys.供应商用户信息.U盾信息.序列号) && !string.IsNullOrWhiteSpace(gys.供应商用户信息.U盾信息.加密参数) && gys.供应商用户信息.U盾信息.年检结束时间 > DateTime.Now)
            {
                payysd = "1";
            }
            ViewData["是否持有U盾"] = payysd;
            if (payysd == "1")
            {
                Random randomGenerator = new Random(DateTime.Now.Millisecond);
                String RndStr = "";
                for (int i = 0; i < 32; i++)
                    RndStr += Convert.ToChar(randomGenerator.Next(97, 122));
                ViewData["Message"] = RndStr;
            }
            return View();
        }
        public ActionResult Part_AcceptanceTicketList()
        {
            int page = 1;
            var isvoid = Request.Params["isvoid"];

            if (!string.IsNullOrWhiteSpace(Request.Params["page"]))
            {
                int.TryParse(Request.Params["page"], out page);
            }
            var q = Query.EQ("供应商链接.用户ID", currentUser.Id);
            if (isvoid == "未作废")
            {
                q = q.And(Query<机票验收单>.Where(o => o.是否作废 == false));
            }
            if (isvoid == "已作废")
            {
                q = q.And(Query<机票验收单>.Where(o => o.是否作废 == true));
            }
            long listcount = 机票验收单管理.计数机票验收单(0, 0, q);
            long maxpage = Math.Max((listcount + ACCEPTA_PAGESIZE - 1) / ACCEPTA_PAGESIZE, 1);

            ViewData["currentpage"] = page;
            ViewData["pagecount"] = maxpage;

            ViewData["验收单列表"] = 机票验收单管理.查询机票验收单(ACCEPTA_PAGESIZE * (page - 1), ACCEPTA_PAGESIZE, q, false, SortBy.Descending("基本数据.添加时间"));

            return PartialView("Gys_Part/Part_AcceptanceTicketList");
        }

        public ActionResult AddAcceptFormTicket() 
        {
            return View();
        }
        
        public ActionResult Part_AcceptSkan()
        {
            var id = Request.Params["id"];//验收单id
            var ysd = 机票验收单管理.查找机票验收单(long.Parse(id));
            return PartialView("Gys_Part/Part_AcceptSkan",ysd);
        }

        public ActionResult AcceptDetialTicket()
        {
            var id = Request.Params["id"];//机票验收单id
            var jpysd = 机票验收单管理.查找机票验收单(long.Parse(id));
            return View(jpysd);
        }
        [HttpPost]
        public ActionResult AddAcceptenceTicket(机票验收单 model)
        {
            foreach (var item in model.服务列表.ToArray())
            {
                item.计量单位 = "张";
                item.数量 = 1;
                if (item.单价 == 0 || item.行程单号 == "" || item.客户姓名 == "")
                {
                    model.服务列表.Remove(item);
                }
            }
            model.供应商链接.用户ID = currentUser.Id;
            机票验收单管理.添加机票验收单(model);

            return RedirectToAction("AddAcceptFormTicket");
        }

        public ActionResult EditFormTicket()
        {
            try
            {
                string id = Request.Params["id"];
                机票验收单 ysd = 机票验收单管理.查找机票验收单(long.Parse(id));
                if (ysd == null)
                {
                    return Redirect("/供应商后台/AcceptanceTicketList");
                }
                return View(ysd);
            }
            catch
            {
                return Redirect("/供应商后台/AcceptanceTicketList");
            } 
        }

        [HttpPost]
        public ActionResult EditformTicket(机票验收单 ysd)
        {
            List<机票验收单._机票服务条目> list = new List<机票验收单._机票服务条目>();
            if (ysd.服务列表 != null && ysd.服务列表.Count() != 0)
            {
                for (int k = 0; k < ysd.服务列表.Count(); k++)//去除物资列表中信息为空的内容
                {
                    if (ysd.服务列表[k].单价 == 0 || ysd.服务列表[k].行程单号 == "" || ysd.服务列表[k].客户姓名 == "")
                    {
                        continue;
                    }
                    else
                    {
                        ysd.服务列表[k].数量 = 1;
                        ysd.服务列表[k].计量单位 = "张";
                        list.Add(ysd.服务列表[k]);
                    }
                }
            }
            机票验收单 y = 机票验收单管理.查找机票验收单(ysd.Id);
            y.服务列表 = list;
            y.供货单位承办人 = ysd.供货单位承办人;
            y.供货单位承办人电话 = ysd.供货单位承办人电话;
            
            机票验收单管理.更新机票验收单(y);
            return Redirect("/供应商后台/EditFormTicket?id=" + ysd.Id);
        }

        public ActionResult DelAccepTicketlist()
        {
            string index = Request.Params["index"];
            string id = Request.Params["id"];
            机票验收单 ysd = 机票验收单管理.查找机票验收单(long.Parse(id));
            ysd.服务列表.RemoveAt(int.Parse(index));
            机票验收单管理.更新机票验收单(ysd);
            return Redirect("/供应商后台/EditFormTicket?id=" + id);
        }

        public ActionResult DelAcceptTicket()
        {
            string id = Request.Params["id"];
            var ysd = 机票验收单管理.查找机票验收单(long.Parse(id));
            ysd.是否作废 = true;
            机票验收单管理.更新机票验收单(ysd);
            return RedirectToAction("AcceptanceTicketList");
        }
        public void EditPrintTicket()
        {
            string id = Request.Params["id"];
            var jpysd = 机票验收单管理.查找机票验收单(long.Parse(id));
            jpysd.是否已经打印 = true;
            jpysd.打印信息.Add(new _打印信息()
            {
                打印时间 = DateTime.Now,
            });
            机票验收单管理.更新机票验收单(jpysd);
        }
        [HttpPost]
        public ActionResult AddAcceptence(验收单 model)
        {
            string[] attachPath = Request.Form["pic"].ToString().Split('|');
            string[] price = Request.Form["attachSum"].ToString().Split(',');
            var cost = Request.Form["costJson"];
            List<验收单._物资或服务条目> list = new List<验收单._物资或服务条目>();
            foreach (var k in model.物资服务列表)//去除物资列表中信息为空的内容
            {
                if (k.单价 >= 0 && k.数量 >= 0 && k.总价 >= 0 && k.商品链接.商品ID != -1)
                {
                    list.Add(k);
                }
            }

            验收单 ysd = new 验收单();
            ysd.供应商链接.用户ID = currentUser.Id;
            ysd.收货单位 = model.收货单位;
            ysd.管理单位审核人.用户ID = model.管理单位审核人.用户ID;
            ysd.供货单位开票人 = model.供货单位开票人;

            ysd.物资服务列表 = list;
            if (!string.IsNullOrWhiteSpace(cost))
            {
                var costs = cost.Split(';');
                foreach (var k in costs)
                {
                    ysd.其他费用.Add(new _其他费用()
                    {
                        费用名称 = k.Split(',')[0],
                        金额 = decimal.Parse(k.Split(',')[1]),
                    });
                }
            }

            ysd.维修费 = model.维修费;
            ysd.运费 = model.运费;
            ysd.服务费 = model.服务费;
            ysd.某批物资名称 = model.某批物资名称;

            for (int j = 0; j < attachPath.Length - 1; j++)
            {
                _验收单附件 attach = new _验收单附件();
                attach.附件路径 = attachPath[j];
                attach.价格 = decimal.Parse(price[j]);
                ysd.验收单附件.Add(attach);
            }
            if (!string.IsNullOrWhiteSpace(ysd.收货单位) && ysd.管理单位审核人.用户ID != -1 && !string.IsNullOrWhiteSpace(ysd.供货单位开票人))
            {
                验收单管理.添加验收单(ysd);
            }
            else
            {
                throw new Exception("信息不完善！");
            }
            return RedirectToAction("AddAcceptanceForm?comes=w");

        }
        public void EditPrint()
        {
            string id = Request.Params["id"];
            验收单 ysd = 验收单管理.查找验收单(long.Parse(id));
            ysd.是否已经打印 = true;
            ysd.打印信息.Add(new _打印信息()
            {
                打印时间 = DateTime.Now,
            });
            验收单管理.更新验收单(ysd);

        }
        public int setShowPicture()
        {
            try
            {
                string name = Request.QueryString["n"];
                string[] id = Request.QueryString["num"].Split(',');
                供应商 model = 用户管理.查找用户<供应商>(currentUser.Id);
                if (id.Length - 1 > 6)
                {
                    return -1;
                }
                else
                {
                    if (model.供应商用户信息.广告商品 != null && model.供应商用户信息.广告商品.Count != 0)
                    {
                        List<供应商._供应商用户信息._广告商品> advertise = new List<供应商._供应商用户信息._广告商品>();
                        for (int i = 0; i < id.Length - 1; i++)
                        {
                            供应商._供应商用户信息._广告商品 ad = new 供应商._供应商用户信息._广告商品();
                            ad.商品.商品ID = long.Parse(id[i]);
                            advertise.Add(ad);
                        }
                        if (model.供应商用户信息.广告商品.ContainsKey(name))
                        {
                            model.供应商用户信息.广告商品.Remove(name);
                            model.供应商用户信息.广告商品.Add(name, advertise);
                        }
                        else
                        {
                            model.供应商用户信息.广告商品.Add(name, advertise);
                        }
                    }
                    else
                    {
                        Dictionary<string, List<供应商._供应商用户信息._广告商品>> adver = new Dictionary<string, List<供应商._供应商用户信息._广告商品>>();
                        List<供应商._供应商用户信息._广告商品> advertise = new List<供应商._供应商用户信息._广告商品>();
                        for (int i = 0; i < id.Length - 1; i++)
                        {
                            供应商._供应商用户信息._广告商品 ad = new 供应商._供应商用户信息._广告商品();
                            ad.商品.商品ID = long.Parse(id[i]);
                            advertise.Add(ad);
                        }
                        adver.Add(name, advertise);
                        model.供应商用户信息.广告商品 = adver;
                    }
                    用户管理.更新用户<供应商>(model);
                    if (model.审核数据.审核状态 == 审核状态.审核通过)
                    {
                        deleteIndex("/Lucene.Net/IndexDic/Gys", model.Id.ToString());
                        CreateIndex_gys(用户管理.查找用户<供应商>(model.Id), "/Lucene.Net/IndexDic/Gys");
                    }
                    return 1;
                }
            }
            catch
            {
                return -1;
            }
        }
        public int advertiseproduct()
        {
            try
            {
                string name = Request.QueryString["n"];
                string[] product = Request.QueryString["num"].Split(',');
                供应商 model = 用户管理.查找用户<供应商>(currentUser.Id);
                if (product.Length - 1 > 2)
                {
                    return -1;
                }
                else
                {
                    List<供应商._供应商用户信息._广告商品> ms = new List<供应商._供应商用户信息._广告商品>();
                    for (int i = 0; i < product.Length - 1; i++)
                    {
                        供应商._供应商用户信息._广告商品 m = new 供应商._供应商用户信息._广告商品();
                        m.商品.商品ID = long.Parse(product[i]);
                        ms.Add(m);
                    }
                    if (model.供应商用户信息.广告商品.ContainsKey(name))
                    {
                        model.供应商用户信息.广告商品.Remove(name);
                        model.供应商用户信息.广告商品.Add(name, ms);
                    }
                    else
                    {
                        model.供应商用户信息.广告商品.Add(name, ms);
                    }
                    用户管理.更新用户<供应商>(model);
                    return 1;
                }
            }
            catch
            {
                return -1;
            }
        }
        public ActionResult Part_AcceptanceGood()
        {
            int page = 1;
            var type = Request.Params["type"];
            //var isvoid = Request.Params["isvoid"];
            //var isexamine = Request.Params["isexamine"];

            if (!string.IsNullOrWhiteSpace(Request.Params["page"]))
            {
                int.TryParse(Request.Params["page"], out page);
            }
            var q = Query.EQ("供应商链接.用户ID", currentUser.Id);

            if (type == "已作废验收单")
            {
                q = q.And(Query<验收单>.Where(o => o.是否作废));
            }
            if (type == "已审核验收单")
            {
                q = q.And(Query<验收单>.Where(o => o.审核数据.审核状态 != 审核状态.未审核));
            }
            if (type == "未审核验收单")
            {
                q = q.And(Query<验收单>.Where(o => o.审核数据.审核状态 == 审核状态.未审核 && !o.是否作废));
            }
            long listcount = 验收单管理.计数验收单(0, 0, q);
            long maxpage = Math.Max((listcount + ACCEPTA_PAGESIZE - 1) / ACCEPTA_PAGESIZE, 1);

            ViewData["currentpage"] = page;
            ViewData["pagecount"] = maxpage;

            ViewData["验收单列表"] = 验收单管理.查询验收单(ACCEPTA_PAGESIZE * (page - 1), ACCEPTA_PAGESIZE, q, false, SortBy.Descending("基本数据.添加时间"));
            return PartialView("Gys_Part/Part_AcceptanceGood");
        }
        //public void AddAcceptGood()
        //{
        //    string id = Request.Params["id"];
        //    string g_name = Request.Params["g_name"];
        //    string g_num = Request.Params["g_num"];
        //    string g_price = Request.Params["g_price"];
        //    验收单 ysd = 验收单管理.查找验收单(long.Parse(id));
        //    验收单._物资或服务条目 d = new 验收单._物资或服务条目();
        //    d.商品链接.商品ID = long.Parse(g_name);
        //    d.数量 = int.Parse(g_num);
        //    d.单价 = decimal.Parse(g_price);
        //    d.总价 = d.数量 * d.单价;
        //    ysd.物资服务列表.Add(d);
        //    验收单管理.更新验收单(ysd);
        //}
        public ActionResult EditForm()
        {
            try
            {
                var q = Query.Or(Query<单位用户>.In(o => o.Id, new long[] { 10003, 10011, 10004, 10005, 10015, 10007, 10012, 10014, 10009 }));
                ViewData["审核单位列表"] = 用户管理.查询用户<单位用户>(0, 0, q);
                ViewData["商品列表"] = 商品管理.查询供应商商品(currentUser.Id);
                ViewData["收货单位列表"] = 用户管理.查询用户<单位用户>(0, 0);
                string id = Request.Params["id"];
                验收单 ysd = 验收单管理.查找验收单(long.Parse(id));
                if (ysd == null)
                {
                    return Redirect("/供应商后台/AddAcceptanceForm");
                }
                ViewData["供应商ID"] = currentUser.Id;
                return View(ysd);
            }
            catch
            {
                return Redirect("/供应商后台/AddAcceptanceForm");
            }
        }

        [HttpPost]
        public ActionResult Editform(验收单 ysd)
        {
            string[] attachPath = Request.Form["pic"].ToString().Split('|');
            string[] price = Request.Form["attachSum"].ToString().Split(',');
            List<验收单._物资或服务条目> list = new List<验收单._物资或服务条目>();
            if (ysd.物资服务列表 != null && ysd.物资服务列表.Count() != 0)
            {
                for (int k = 0; k < ysd.物资服务列表.Count(); k++)//去除物资列表中信息为空的内容
                {
                    if (ysd.物资服务列表[k].单价 == 0 && ysd.物资服务列表[k].数量 == 0)
                    {
                        continue;
                    }
                    else
                    {
                        list.Add(ysd.物资服务列表[k]);
                    }
                }
            }
            验收单 y = 验收单管理.查找验收单(ysd.Id);
            y.供货单位开票人 = ysd.供货单位开票人;
            y.管理单位审核人.用户ID = ysd.管理单位审核人.用户ID;
            y.收货单位 = ysd.收货单位;
            y.某批物资名称 = ysd.某批物资名称;
            List<_其他费用> other_cost = new List<_其他费用>();
            foreach (var mm in ysd.其他费用)
            {
                if (!string.IsNullOrWhiteSpace(mm.费用名称)&&mm.金额!=0)
                {
                    other_cost.Add(mm);
                }
            }
            y.其他费用 = other_cost;
            y.服务费 = ysd.服务费;
            y.维修费 = ysd.维修费;
            y.运费 = ysd.运费;
            y.物资服务列表 = list;
            List<_验收单附件> ysdlist = new List<_验收单附件>();
            if (y.验收单附件 != null)
            {
                ysdlist = y.验收单附件;
            }
            for (int j = 0; j < attachPath.Length - 1; j++)
            {
                _验收单附件 attach = new _验收单附件();
                attach.附件路径 = attachPath[j];
                attach.价格 = decimal.Parse(price[j]);
                ysdlist.Add(attach);
            }
            if (y.验收单附件 != null)
            {
                for (int m = 0; m < y.验收单附件.Count(); m++)
                {
                    if (!System.IO.File.Exists(Server.MapPath(@y.验收单附件[m].附件路径)))
                    {
                        y.验收单附件.Remove(y.验收单附件[m]);
                    }
                }
            }
            //y.物资服务列表 = ysd.物资服务列表;
            验收单管理.更新验收单(y);
            return Redirect("/供应商后台/EditForm?id=" + ysd.Id);
        }
        public ActionResult DelResourse()
        {
            string index = Request.Params["index"];
            string id = Request.Params["id"];
            验收单 ysd = 验收单管理.查找验收单(long.Parse(id));
            ysd.物资服务列表.RemoveAt(int.Parse(index));
            验收单管理.更新验收单(ysd);
            return Redirect("/供应商后台/EditForm?id=" + id);
        }
        public ActionResult DelAccept()
        {
            string id = Request.Params["id"];
            var ysd = 验收单管理.查找验收单(long.Parse(id));
            ysd.是否作废 = true;
            验收单管理.更新验收单(ysd);
            return RedirectToAction("AddAcceptanceForm");
        }
        public ActionResult AcceptDetial()
        {
            string id = Request.Params["id"];
            验收单 ysd = 验收单管理.查找验收单(long.Parse(id));
            //if(string.IsNullOrEmpty(ysd.验收单二维码))
            //{ 
            //    QRCodeHandler qr = new QRCodeHandler();
            //    //string path = AppDomain.CurrentDomain.SetupInformation.ApplicationBase;    //文件目录
            //    string path = 上传管理.获取上传路径<验收单>(媒体类型.图片, 路径类型.服务器本地路径);
            //    string p = 上传管理.获取上传路径<验收单>(媒体类型.图片, 路径类型.不带域名根路径);
            //    string filename="ysdqr"+Guid.NewGuid()+".jpg";
            //    string qrString = "http://www.go81.cn/%E9%A6%96%E9%A1%B5/SearchAcceptance?id=" + id;    //二维码字符串 
            //    string filePath = path + filename;      //二维码文件名 
            //    if (!Directory.Exists(path))
            //    {
            //        Directory.CreateDirectory(path);
            //    }
            //    qr.CreateQRCode(qrString, "Byte", 2, 0, "H", filePath);   //生成
            //    ysd.验收单二维码 = p + filename;
            //    验收单管理.更新验收单(ysd);
            //}
            var 是否有未上传验收单 = false;
            var cc = 验收单管理.查询验收单(0, 0, Query<验收单>.Where(o => o.供应商链接.用户ID == currentUser.Id && o.是否已经打印 && !o.是否作废 && o.验收单扫描件.Count <= 0));
            foreach (var k in cc)
            {
                if (k.打印信息.Count > 0 && k.打印信息.Last().打印时间.AddDays(10) < DateTime.Now)
                {
                    是否有未上传验收单 = true;
                    break;
                }
            }
            ViewData["是否有未上传验收单"] = 是否有未上传验收单;
            return View(ysd);
        }
        public ActionResult ProjectService_List()
        {
            //return PartialView("Gys_Part/ProjectService_List");
            return View();
        }
        public ActionResult Part_ProjectService_List(int? page)
        {
            //int pro_listcount = (int)(项目服务记录管理.计数项目服务记录(0, 0, Query.EQ("供应商链接.用户ID", currentUser.Id)));
            int pro_listcount = (int)(验收单管理.计数验收单(0, 0, Query<验收单>.EQ(o => o.供应商链接.用户ID, currentUser.Id).And(Query<验收单>.Where(o => o.验收单扫描件.Count>0))));
            int pro_maxpage = Math.Max((pro_listcount + 15 - 1) / 15, 1);

            if (string.IsNullOrEmpty(page.ToString()) || page < 0 || page > pro_maxpage)
            {
                page = 1;
            }

            ViewData["listcount"] = pro_listcount;
            ViewData["page"] = page;
            ViewData["pagesize"] = 15;
            if (currentUser.审核数据.审核状态 == 审核状态.审核通过)
            {
                ViewBag.Shen = true;
            }
            else
            {
                ViewBag.Shen = false;
            }
            //ViewData["待评分项目服务列表"] = 项目服务记录管理.查询项目服务记录(15 * (int.Parse(page.ToString()) - 1), 15, Query.EQ("供应商链接.用户ID", currentUser.Id));
            ViewData["验收单列表"] = 验收单管理.查询验收单(0, 20, Query<验收单>.EQ(o => o.供应商链接.用户ID, currentUser.Id).And(Query<验收单>.Where(o => o.验收单扫描件.Count>0)));

            //TD:先检查用户是否持有U盾，且未过期
            var payysd = "0";
            var gys = 用户管理.查找用户<供应商>(currentUser.Id);
            if (!string.IsNullOrWhiteSpace(gys.供应商用户信息.U盾信息.序列号) && !string.IsNullOrWhiteSpace(gys.供应商用户信息.U盾信息.加密参数) && gys.供应商用户信息.U盾信息.年检结束时间 > DateTime.Now)
            {
                payysd = "1";
            }

            ViewData["是否持有U盾"] = payysd;
            if (payysd == "1")
            {
                Random randomGenerator = new Random(DateTime.Now.Millisecond);
                String RndStr = "";
                for (int i = 0; i < 32; i++)
                    RndStr += Convert.ToChar(randomGenerator.Next(97, 122));
                ViewData["Message"] = RndStr;
            }
            ViewData["供应商ID"] = currentUser.Id;
            //先检查用户是否持有U盾，且未过期

            return PartialView("Gys_Part/Part_ProjectService_List");
        }
        public ActionResult ProjectService_Detail()
        {
            return View();
        }
        public ActionResult Part_ProjectService_Detail(long? id)
        {
            项目服务记录 model = null;
            try
            {
                model = 项目服务记录管理.查找项目服务记录((long)id);
                if (model.供应商链接.用户ID != currentUser.Id)
                {
                    return Content("<script>alert('权限不足');location.href='javascript:history.go(-1)';</script>");
                }
            }
            catch
            {
                model = null;
            }
            return PartialView("Gys_Part/Part_ProjectService_Detail", model);
        }

        public class YsdSmj 
        {
            public long id { get; set; } //所属供应商ID
            public string path { get; set; }//扫描件路径
            public string examine { get; set; }//回传单审核状态

            public string reason { get; set; }//未通过理由
        }
        public string GetSmj() 
        {
            var id=Request.Params["id"];
            var list = new List<YsdSmj>();
            if (id!="请选择验收单号")
            {
                var ysd = 验收单管理.查找验收单(long.Parse(id));
                foreach (var k in ysd.验收单扫描件)
                {
                    list.Add(new YsdSmj()
                    {
                        id = ysd.供应商链接.用户ID,
                        path = k.回传单路径,
                        examine = k.审核数据.审核状态.ToString(),
                        reason = !string.IsNullOrWhiteSpace(k.审核数据.审核不通过原因) ? k.审核数据.审核不通过原因 : "",
                    });
                }
                var jsonstr = JsonConvert.SerializeObject(list);
                return jsonstr;
            }
            return "{}";
        }

        public ActionResult ProjectService_Delete(long? id)
        {
            try
            {
                项目服务记录 model = 项目服务记录管理.查找项目服务记录((long)id);
                if (model.服务评价.服务评级 != 项目服务记录.服务评级.未填写)
                {
                    return Content("<script>alert('不能删除已评分的服务');location.href='javascript:history.go(-1)';</script>");
                }
                if (model.供应商链接.用户ID != currentUser.Id)
                {
                    return Content("<script>alert('权限不足');location.href='javascript:history.go(-1)';</script>");
                }

                项目服务记录管理.删除项目服务记录((long)id);
                return View("ProjectService_List");
            }
            catch
            {
                return Content("<script>alert('删除失败');location.href='javascript:history.go(-1)';</script>");
            }


        }

        public int DelYsdSmj() 
        {
            string uri = Request.QueryString["n"];
            long id = long.Parse(Request.QueryString["gid"].ToString());
            var model = 验收单管理.查找验收单(id);
            if (model.验收单扫描件.Count>0 && !string.IsNullOrWhiteSpace(uri))
            {
                var item = model.验收单扫描件.Find(o => o.回传单路径 == uri);
                model.验收单扫描件.Remove(item);
                if (System.IO.File.Exists(Server.MapPath(@uri)))
                {
                    System.IO.File.Delete(Server.MapPath(@uri));
                    
                }
                验收单管理.更新验收单(model);
                return 1;
            }
            else
            {
                return 0;
            }
        }

        public int Del_gysPic()
        {
            try
            {
                string uri = Request.QueryString["n"];
                long id =long.Parse(Request.QueryString["gid"].ToString());
                供应商 model = 用户管理.查找用户<供应商>(id);
                if(model.供应商用户信息.供应商图片!=null)
                {
                    foreach (var item in model.供应商用户信息.供应商图片)
                    {
                        if(item==uri)
                        {
                            model.供应商用户信息.供应商图片.Remove(item);
                            break;
                        }
                    }
                }
                if (System.IO.File.Exists(Server.MapPath(@uri)))
                {
                    System.IO.File.Delete(Server.MapPath(@uri));
                    用户管理.更新用户<供应商>(model);
                    return 1;
                }
                else
                {
                    return 0;
                }
            }
            catch
            {
                return 0;
            }
        }
        public ActionResult CompanyWithCar()
        {
            List<租车企业> model = 租车企业管理.查询租车企业(0, 0, Query.EQ("所属供应商.用户ID", currentUser.Id)).ToList();
            if (model.Count() == 0 || model == null)
            {
                租车企业 m = new 租车企业();
                model.Add(m);
            }
            return View(model);
        }
        [HttpPost]
        public ActionResult AddCar(租车企业._车辆信息 model)
        {
            租车企业 CarInfo = 租车企业管理.查找租车企业(currentUser.Id);
            if (CarInfo == null)
            {
                CarInfo = new 租车企业();
            }
            if (Request.Files.Count == 0)
            {
                return Content("<script>alert('请上传车辆图片');window.location='/供应商后台/CompanyWithCar';</script>");
            }
            else
            {
                for (int i = 0; i < Request.Files.Count; i++)
                {
                    HttpPostedFileBase file = Request.Files[i];
                    if (file != null && file.ContentType == "image/jpeg" && (((file.ContentLength / 1024) / 1024) < 2))
                    {
                        string filePath = 上传管理.获取上传路径<供应商>(媒体类型.图片, 路径类型.不带域名根路径);
                        string fname = UploadAttachment(file);
                        model.图片.Add(filePath + fname);
                    }
                }
                CarInfo.车辆信息.Add(model);
                租车企业管理.更新租车企业(CarInfo);
                return Redirect("/供应商后台/CompanyWithCar");
            }

        }
        [HttpPost]
        public ActionResult AddCompany(List<租车企业> model)
        {
            string pos = Request.Form["pt"].ToString();
            if (Request.Files.Count == 0)
            {
                return Content("<script>alert('请上传企业图片');window.location='/供应商后台/CompanyWithCar';</script>");
            }
            else
            {
                for (int i = 0; i < Request.Files.Count; i++)
                {
                    HttpPostedFileBase file = Request.Files[i];
                    if (file != null && file.ContentType == "image/jpeg" && (((file.ContentLength / 1024) / 1024) < 2))
                    {
                        string filePath = 上传管理.获取上传路径<供应商>(媒体类型.图片, 路径类型.不带域名根路径);
                        string fname = UploadAttachment(file);
                        model[0].企业信息.企业图片 = filePath + fname;
                    }
                }
                if (!string.IsNullOrWhiteSpace(pos))
                {
                    double[] point = new double[2];
                    point[0] = double.Parse(pos.Split(',')[0]);
                    point[1] = double.Parse(pos.Split(',')[1]);
                    model[0].企业信息.地理位置 = point;
                }
                model[0].所属供应商.用户ID = currentUser.Id;
                model[0].审核数据.审核状态 = 审核状态.未审核;
                租车企业管理.添加租车企业(model[0]);
                return Redirect("/供应商后台/CompanyWithCar");
            }
        }
        public string Check_User()
        {
            string id = Request.QueryString["id"].ToString();
            IEnumerable<单位用户> Part_User = 用户管理.查询用户<单位用户>(0, 0, Query.EQ("单位信息.单位编码", id));
            if (Part_User.Count() != 1)
            {
                return "用户不存在！";
            }
            else
            {
                return "";
            }
        }
        public ActionResult delGtype()
        {
            try
            {
                int index = int.Parse(Request.QueryString["index"]);
                供应商 model = 用户管理.查找用户<供应商>(currentUser.Id);
                model.可提供产品类别列表.RemoveAt(index);
                用户管理.更新用户<供应商>(model);
                return Redirect("/供应商后台/Gys_Product_Type");
            }
            catch
            {
                return Redirect("/供应商后台/Gys_Product_Type");
            }
        }
        [HttpPost]
        public ActionResult Add_Service_Evaluate(验收单 model)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(Request.Form["selectSh_unit"]))
                {
                    return Content("<script>alert('没有这个验收单位编码！');window.location='/供应商后台/Service_Evaluate';</script>");
                }
                else
                {
                    验收单 ysd = 验收单管理.查找验收单(long.Parse(Request.Form["selectSh_unit"].ToString()));
                    var smjs = Request.Form["smj"].ToString();
                    var smjarr = smjs.Split(new[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var item in smjarr)
                    {
                        ysd.验收单扫描件.Add(new _回传信息 { 回传单路径 = item });
                    }
                    

                    //if (!string.IsNullOrEmpty(Request.Form["zb_contact"]))
                    //{
                    //    try
                    //    {
                    //        model.所属项目.招标采购项目ID = int.Parse(Request.Form["zb_contact"]);
                    //    }
                    //    catch
                    //    {

                    //    }
                    //}

                    //model.服务信息.验收时间 = DateTime.Now;
                    //model.供应商链接.用户ID = currentUser.Id;
                    验收单管理.更新验收单(ysd);
                    return Content("<script>alert('添加成功！您可以继续添加');window.location='/供应商后台/Service_Evaluate';</script>");
                }

            }
            catch
            {
                return Redirect("/供应商后台/AddAcceptanceForm");
            }
        }
        [HttpPost]
        public string UploadAttachment(HttpPostedFileBase fileData)
        {
            //string extension = string.Empty;

            if (fileData != null)
            {
                string fileName = fileData.FileName;
                if (fileName != "" && fileName != null)
                {
                    string filePath = 上传管理.获取上传路径<供应商>(媒体类型.图片, 路径类型.服务器本地路径);

                    //extension = fileData.FileName.Substring(fileData.FileName.LastIndexOf("."), (fileData.FileName.Length - fileData.FileName.LastIndexOf(".")));
                    if (fileName.LastIndexOf("\\") > -1)
                    {
                        fileName = fileName.Substring(fileName.LastIndexOf("\\") + 1);
                    }
                    fileName = 上传管理.获取文件名(fileName).Replace("{", "").Replace("}", "");
                    if (!Directory.Exists(filePath))
                    {
                        Directory.CreateDirectory(filePath);
                    }
                    fileData.SaveAs(string.Format("{0}\\{1}", filePath, fileName));
                }
                return fileName;
            }
            else
            {
                return null;
            }
        }

        public ActionResult Tax_Management()
        {
            return View();
        }

        public ActionResult Part_tax_Management()
        {
            供应商 model = 用户管理.查找用户<供应商>(currentUser.Id);
            return PartialView("Gys_Part/Part_tax_Management", model);
        }
        public ActionResult Gys_enroll()
        {
            return View();
        }
        public ActionResult Part_Gys_enroll()
        {
            try
            {
                int CurrentPage = int.Parse(Request.QueryString["page"]);
                IEnumerable<招标采购预报名> model = 招标采购预报名管理.查询招标采购预报名(0, 0);
                List<招标采购预报名> model1 = new List<招标采购预报名>();
                foreach (var item in model)
                {
                    if (item.预报名供应商列表.Exists(m => m.供应商链接.用户ID == currentUser.Id))
                    {
                        model1.Add(item);
                    }
                }
                int Pagecount = model1.Count() / 20;
                if (model1.Count() % 20 > 0)
                {
                    Pagecount++;
                }
                ViewData["Pagecount"] = Pagecount;
                ViewData["CurrentPage"] = CurrentPage;
                ViewData["id"] = currentUser.Id;
                IEnumerable<招标采购预报名> model2 = model1.Skip(20 * (CurrentPage - 1)).Take(20);
                return PartialView("Gys_Part/Part_Gys_enroll", model2);
            }
            catch
            {
                return Content("<script>window.location='/供应商后台/index';</script>");
            }
        }
        public ActionResult Editenroll()
        {
            try
            {
                long id = long.Parse(Request.QueryString["id"]);
                供应商 model = 用户管理.查找用户<供应商>(currentUser.Id);
                招标采购预报名 Newmodel = 招标采购预报名管理.查找招标采购预报名(id);
                if (Newmodel != null)
                {
                    if (Newmodel.预报名供应商列表 != null && Newmodel.预报名供应商列表.Count() != 0)
                    {
                        foreach (var item in Newmodel.预报名供应商列表)
                        {
                            if (item.供应商链接.用户ID == currentUser.Id)
                            {
                                ViewData["info"] = item;
                                break;
                            }
                        }
                    }
                }
                if (Newmodel == null)
                {
                    return Redirect("/供应商后台/gys_enroll?page=1");
                }
                ViewData["supplier"] = model;
                return View(Newmodel);
            }
            catch
            {
                return Redirect("/供应商后台/gys_enroll?page=1");
            }
        }
        [HttpPost]
        public ActionResult AddEnroll(招标采购预报名 model)
        {
            int index = 0;
            Dictionary<string, List<string>> Info = new Dictionary<string, List<string>>();
            List<string> pic = new List<string>();
            招标采购预报名 Newmodel = 招标采购预报名管理.查找招标采购预报名(model.Id);
            if (Newmodel != null)
            {
                for (int i = 0; i < Newmodel.供应商所需资料.Count; i++)
                {
                    if (!Newmodel.供应商所需资料[i].图片)
                    {
                        Info.Add(Newmodel.供应商所需资料[i].资料名, new List<string>() { Request.Form["zl" + i.ToString()] });
                    }
                    else
                    {
                        List<string> urls = new List<string>();
                        string[] url = Request.Form["picture" + i.ToString()].ToString().Split('|');
                        for (int m = 0; m < url.Length - 1; m++)
                        {
                            urls.Add(url[m]);
                        }
                        Info.Add(Newmodel.供应商所需资料[i].资料名, urls);
                    }
                }
                招标采购预报名._供应商预报名信息 model1 = null;
                if (Newmodel.预报名供应商列表 != null && Newmodel.预报名供应商列表.Count() != 0)
                {
                    for (int i = 0; i < Newmodel.预报名供应商列表.Count(); i++)
                    {
                        if (Newmodel.预报名供应商列表[i].供应商链接.用户ID == currentUser.Id)
                        {
                            index = i;
                            model1 = Newmodel.预报名供应商列表[i];
                        }
                    }
                    if (model1 != null)
                    {
                        model1.供应商提交资料 = Info;
                    }
                    Newmodel.预报名供应商列表[index] = model1;
                    招标采购预报名管理.更新招标采购预报名(Newmodel);
                }
            }
            return Redirect("/供应商后台/gys_enroll?page=1");
        }

        public ActionResult enrollMaterial()
        {
            try
            {
                供应商 model_gys = 用户管理.查找用户<供应商>(currentUser.Id);
                long Id = long.Parse(Request.QueryString["id"]);
                招标采购预报名 model = 招标采购预报名管理.查找招标采购预报名(Id);
                ViewData["Id"] = currentUser.Id;
                ViewData["supplier"] = model_gys;
                return View(model);
            }
            catch
            {
                return Redirect("/供应商后台/index");
            }
        }
        public class Eroll
        {
            public long id { get; set; }
            public string name { get; set; }
            public string time { get; set; }
        }
        public ActionResult Search_Eroll()
        {
            List<Eroll> eroll = new List<Eroll>();
            int page = int.Parse(Request.QueryString["p"]);
            long pages = 0;
            IEnumerable<招标采购预报名> model = 招标采购预报名管理.查询招标采购预报名(0, 0);
            List<招标采购预报名> model1 = new List<招标采购预报名>();
            foreach (var item in model)
            {
                if (item.预报名供应商列表.Exists(m => m.供应商链接.用户ID == currentUser.Id))
                {
                    model1.Add(item);
                }
            }
            pages = model1.Count() / 10;
            if (model1.Count() % 10 > 0)
            {
                pages++;
            }
            if (model1 != null)
            {
                foreach (var item in model1.Skip(10 * (page - 1)).Take(10))
                {
                    Eroll el = new Eroll();
                    el.id = item.所属公告链接.公告ID;
                    el.name = item.所属公告链接.公告.项目信息.项目名称;
                    foreach (var m in item.预报名供应商列表)
                    {
                        el.time = m.报名时间.ToString("yyyy-MM-dd");
                    }
                    eroll.Add(el);
                }
            }
            JsonResult json = new JsonResult() { Data = new { dt = eroll, p = pages } };
            return Json(json, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult Tax_Manage(供应商 model)
        {
            供应商 Newmodel = 用户管理.查找用户<供应商>(currentUser.Id);
            if (!System.IO.File.Exists(Server.MapPath(@Newmodel.税务信息.近3年完税证明电子扫描件)))
            {
                Newmodel.税务信息.近3年完税证明电子扫描件 = "";
            }
            string imgUrl = Newmodel.税务信息.近3年完税证明电子扫描件;
            Newmodel.税务信息 = model.税务信息;
            Newmodel.税务信息.近3年完税证明电子扫描件 = imgUrl;
            if (string.IsNullOrWhiteSpace(Newmodel.税务信息.地税税务登记证号) ||
                string.IsNullOrWhiteSpace(Newmodel.税务信息.地税税务登记证发证机关) ||
                string.IsNullOrWhiteSpace(Newmodel.税务信息.国税税务登记证号) ||
                string.IsNullOrWhiteSpace(Newmodel.税务信息.国税税务登记证发证机关) || string.IsNullOrWhiteSpace(Newmodel.税务信息.近3年完税证明电子扫描件))
            {
                Newmodel.税务信息.已填写完整 = false;
            }
            else
            {
                Newmodel.税务信息.已填写完整 = true;
            }
            deleteIndex("/Lucene.Net/IndexDic/Gys", model.Id.ToString());
            //deleteIndex_ProductCatalog("/Lucene.Net/IndexDic/GysCatalog", model.企业基本信息.企业名称);
            用户管理.更新用户<供应商>(Newmodel);
            return Content("<script>if(confirm('请确保税务信息以完善，点击确定请等待审核，点击取消继续添加。')){window.location='/供应商后台/toubiao';} else{window.location='/供应商后台/Tax_Management';}</script>");
        }
        public ActionResult Law_Person()
        {
            return View();
        }
        public ActionResult Part_Law_Person()
        {
            供应商 model = 用户管理.查找用户<供应商>(currentUser.Id);
            return PartialView("Gys_Part/Part_Law_Person", model);
        }
        public ActionResult Completing()
        {
            return View();
        }
        public ActionResult Part_Completing()
        {
            供应商 Newmodel = 用户管理.查找用户<供应商>(currentUser.Id);
            ViewBag.BaseInfo = Newmodel.企业基本信息.已填写完整;
            ViewBag.VipInfo = Newmodel.企业联系人信息.已填写完整;
            ViewBag.SalesInfo = Newmodel.工商注册信息.已填写完整;
            ViewBag.Busniss = Newmodel.营业执照信息.已填写完整;
            ViewBag.TaxInfo = Newmodel.税务信息.已填写完整;
            ViewBag.LawPerson = Newmodel.法定代表人信息.已填写完整;
            ViewBag.goodType = Newmodel.可提供产品类别列表.Count();
            ViewBag.GoodCount = 商品管理.计数供应商商品(Newmodel.Id);
            if (currentUser.出资人或股东信息 != null)
            {
                if (Newmodel.出资人或股东信息.Count() == 0)
                {
                    ViewBag.Investor = false;
                }
                else
                {
                    foreach (var item in Newmodel.出资人或股东信息)
                    {
                        if (item.已填写完整 == false)
                        {
                            ViewBag.Investor = false;
                            break;
                        }
                        else
                        {
                            ViewBag.Investor = true;
                        }
                    }
                }
            }
            else
            {
                ViewBag.Investor = false;
            }

            if (Newmodel.财务状况信息 != null)
            {
                if (Newmodel.财务状况信息.Count() == 0)
                {
                    ViewBag.Financial = false;
                }
                else
                {
                    foreach (var item in Newmodel.财务状况信息)
                    {
                        if (item.已填写完整 == false)
                        {
                            ViewBag.Financial = false;
                            break;
                        }
                        else
                        {
                            ViewBag.Financial = true;
                        }
                    }
                }
            }
            if (Newmodel.资质证书列表 != null)
            {
                if (Newmodel.资质证书列表.Count() == 0)
                {
                    ViewBag.qualify = false;
                }
                else
                {
                    foreach (var item in Newmodel.资质证书列表)
                    {
                        if (item.已填写完整 == false)
                        {
                            ViewBag.qualify = false;
                            break;
                        }
                    }
                }
            }
            else
            {
                ViewBag.qualify = false;
            }
            ViewBag.ysd = 验收单管理.查询验收单(0, 0, Query<验收单>.Where(o => o.供应商链接.用户ID == currentUser.Id && (o.审核数据.审核状态 == 审核状态.审核未通过 || o.是否已经打印 == false || o.验收单扫描件.Count>0)));
            if (Newmodel.供应商用户信息.U盾信息 == null || string.IsNullOrWhiteSpace(Newmodel.供应商用户信息.U盾信息.加密参数) || string.IsNullOrWhiteSpace(Newmodel.供应商用户信息.U盾信息.序列号))
            {
                ViewData["U_message"] = "您还没有<a href='/jct/ApplyVip' style='text-decoration:underline; font-weight:bold;' target='_blank'>申请安全认证和办理U盾</a>";
            }
            return PartialView("Gys_Part/Part_Completing", Newmodel);
        }
        [HttpPost]
        public ActionResult Law_Person_Manage(供应商 model)
        {
            try
            {
                供应商 Newmodel = 用户管理.查找用户<供应商>(currentUser.Id);
                string path = Newmodel.法定代表人信息.法定代表人身份证电子扫描件;
                Newmodel.法定代表人信息 = model.法定代表人信息;
                Newmodel.法定代表人信息.法定代表人身份证电子扫描件 = path;
                if (string.IsNullOrWhiteSpace(Newmodel.法定代表人信息.法定代表人姓名) || string.IsNullOrWhiteSpace(Newmodel.法定代表人信息.法定代表人身份证电子扫描件))
                {
                    Newmodel.法定代表人信息.已填写完整 = false;
                }
                else
                {
                    Newmodel.法定代表人信息.已填写完整 = true;
                }
                Newmodel.审核数据.审核状态 = 审核状态.未审核;
                deleteIndex("/Lucene.Net/IndexDic/Gys", Newmodel.Id.ToString());
                //deleteIndex_ProductCatalog("/Lucene.Net/IndexDic/GysCatalog", model.企业基本信息.企业名称);
                用户管理.更新用户<供应商>(Newmodel);
                return Content("<script>if(confirm('请确保法人代表信息以完善，点击确定请等待审核，点击取消继续完善。')){window.location='/供应商后台/Gys_Sales_Manage';} else{window.location='/供应商后台/Law_Person';}</script>");
            }
            catch
            {
                return Content("<script>if(confirm('请确保法人代表信息以完善，点击确定请等待审核，点击取消继续完善。')){window.location='/供应商后台/Gys_Sales_Manage';} else{window.location='/供应商后台/Law_Person';}</script>");
            }
        }
        public ActionResult Investor_Management()
        {
            return View();
        }

        public ActionResult Part_Investor_Management()
        {
            供应商 model = 用户管理.查找用户<供应商>(currentUser.Id);
            return PartialView("Gys_Part/Part_Investor_Management", model);
        }
        [HttpPost]
        public ActionResult Investor_Manage(供应商 model)
        {
            供应商 Newmodel = 用户管理.查找用户<供应商>(currentUser.Id);
            if (Newmodel.审核数据.审核状态 == 审核状态.审核通过)
            {
                return Content("<script>alert('审核通过的用户不能再修改信息！');window.location='/供应商后台/';</script>");
            }
            Newmodel.出资人或股东信息.Add(model.出资人或股东信息[0]);
            Newmodel.审核数据.审核状态 = 审核状态.未审核;
            deleteIndex("/Lucene.Net/IndexDic/Gys", model.Id.ToString());
            //deleteIndex_ProductCatalog("/Lucene.Net/IndexDic/GysCatalog", model.企业基本信息.企业名称);
            if (Newmodel.出资人或股东信息 != null && Newmodel.出资人或股东信息.Count() != 0)
            {
                foreach (var item in Newmodel.出资人或股东信息)
                {
                    if (string.IsNullOrWhiteSpace(item.出资人或股东姓名) ||
                       string.IsNullOrWhiteSpace(item.出资人或股东性质) ||
                        string.IsNullOrWhiteSpace(item.身份证号码))
                    {
                        item.已填写完整 = false;
                        break;
                    }
                    else
                    {
                        item.已填写完整 = true;
                    }
                }
            }
            用户管理.更新用户<供应商>(Newmodel);
            return Content("<script>if(confirm('已成功添加投资人信息，点击确定请等待审核，点击取消继续添加。')){window.location='/供应商后台/Gys_Financial_Manage';} else{window.location='/供应商后台/Investor_Management';}</script>");
        }
        public ActionResult Qualify_Management()
        {
            return View();
        }
        public ActionResult Part_Qualify_Management()
        {
            供应商 model = 用户管理.查找用户<供应商>(currentUser.Id);
            if (model.资质证书列表 != null && model.资质证书列表.Count() != 0)
            {
                for (int i = 0; i < model.资质证书列表.Count(); i++)
                {
                    if (model.资质证书列表[i].资质证书电子扫描件 != null && model.资质证书列表[i].资质证书电子扫描件.Count() != 0)
                    {
                        if (!System.IO.File.Exists(Server.MapPath(@model.资质证书列表[i].资质证书电子扫描件[0].路径)))
                        {
                            model.资质证书列表[i].资质证书电子扫描件[0].路径 = "";
                        }
                    }
                }

            }
            return PartialView("Gys_Part/Part_Qualify_Management", model);
        }
        [HttpPost]
        public ActionResult Modify_qualify(供应商 model)
        {
            try
            {
                供应商 Newmodel = 用户管理.查找用户<供应商>(currentUser.Id);
                if (Newmodel.审核数据.审核状态 == 审核状态.审核通过)
                {
                    return Content("<script>alert('审核通过的用户不能再修改信息！');window.location='/供应商后台/';</script>");
                }
                int index = int.Parse(Request.Form["index1"].ToString());
                if (index > Newmodel.资质证书列表.Count() || index < 0)
                {
                    return Redirect("/供应商后台/Qualify_Management");
                }
                string url = Newmodel.资质证书列表[index].资质证书电子扫描件[0].路径;
                Newmodel.资质证书列表[index] = model.资质证书列表[index];
                Newmodel.资质证书列表[index].资质证书电子扫描件[0].路径 = url;
                Newmodel.审核数据.审核状态 = 审核状态.未审核;
                if (Newmodel.资质证书列表 != null && Newmodel.资质证书列表.Count() != 0)
                {
                    foreach (var item in Newmodel.资质证书列表)
                    {
                        if (string.IsNullOrWhiteSpace(item.名称) || string.IsNullOrWhiteSpace(item.发证机构) || string.IsNullOrWhiteSpace(item.等级))
                        {
                            item.已填写完整 = false;
                            break;
                        }
                        else
                        {
                            item.已填写完整 = true;
                        }
                    }
                }
                用户管理.更新用户<供应商>(Newmodel);
                deleteIndex("/Lucene.Net/IndexDic/Gys", model.Id.ToString());
                //deleteIndex_ProductCatalog("/Lucene.Net/IndexDic/GysCatalog", model.企业基本信息.企业名称);
                return Content("<script>if(confirm('请确保资质证书信息已完善，点击确定请等待审核，点击取消继续完善。')){window.location='/供应商后台/Service_Management';} else{window.location='/供应商后台/Qualify_Management';}</script>");
            }
            catch
            {
                return Redirect("/供应商后台/Qualify_Management");
            }
        }
        public ActionResult Delete_Qualify()
        {
            try
            {
                供应商 Newmodel = 用户管理.查找用户<供应商>(currentUser.Id);
                int index = int.Parse(Request.QueryString["id"]);
                if (Newmodel.资质证书列表[index].资质证书电子扫描件 != null && Newmodel.资质证书列表[index].资质证书电子扫描件.Count() != 0)
                {
                    for (int i = 0; i < Newmodel.资质证书列表[index].资质证书电子扫描件.Count(); i++)
                    {
                        if (System.IO.File.Exists(Server.MapPath(Newmodel.资质证书列表[index].资质证书电子扫描件.ElementAt(i).路径)))
                        {
                            System.IO.File.Delete(Server.MapPath(Newmodel.资质证书列表[index].资质证书电子扫描件.ElementAt(i).路径));
                        }
                    }
                }
                Newmodel.资质证书列表.RemoveAt(index);
                Newmodel.审核数据.审核状态 = 审核状态.未审核;
                deleteIndex("/Lucene.Net/IndexDic/Gys", currentUser.Id.ToString());
                //deleteIndex_ProductCatalog("/Lucene.Net/IndexDic/GysCatalog", Newmodel.企业基本信息.企业名称);
                用户管理.更新用户<供应商>(Newmodel);
                return Redirect("/供应商后台/Qualify_Management");
            }
            catch
            {
                return Redirect("/供应商后台/Qualify_Management");
            }
        }
        [HttpPost]
        public ActionResult Qualify_Manage(供应商 model)
        {
            供应商 Newmodel = 用户管理.查找用户<供应商>(currentUser.Id);
            if (Newmodel.审核数据.审核状态 == 审核状态.审核通过)
            {
                return Content("<script>alert('审核通过的用户不能再修改信息！');window.location='/供应商后台/';</script>");
            }
            for (int j = 0; j < Request.Files.Count; j++)
            {
                HttpPostedFileBase img = Request.Files[j];
                if (img != null && img.FileName != "" && ((img.ContentLength / 1024) / 1024) < 2 && img.ContentType == "image/jpeg")
                {
                    string filePath = 上传管理.获取上传路径<供应商>(媒体类型.图片, 路径类型.不带域名根路径);
                    string fname = UploadAttachment(img);
                    string file_name = filePath + fname;
                    供应商._电子扫描件 q = new 供应商._电子扫描件();
                    q.名称 = model.资质证书列表[0].资质证书电子扫描件[0].名称;
                    q.上传日期 = DateTime.Now;
                    q.路径 = file_name;
                    q.说明 = model.资质证书列表[0].资质证书电子扫描件[0].说明;
                    model.资质证书列表[0].资质证书电子扫描件[0].上传日期 = DateTime.Now;
                    model.资质证书列表[0].资质证书电子扫描件[0].路径 = file_name;
                }
                else
                {
                    供应商._电子扫描件 q = new 供应商._电子扫描件();
                    q.名称 = model.资质证书列表[0].资质证书电子扫描件[0].名称;
                    q.上传日期 = DateTime.Now;
                    q.路径 = "";
                    q.说明 = model.资质证书列表[0].资质证书电子扫描件[0].说明;
                    //Newmodel.资质证书列表[0].资质证书电子扫描件.Add(q);
                }
            }
            Newmodel.资质证书列表.Add(model.资质证书列表[0]);
            if (Newmodel.资质证书列表 != null && Newmodel.资质证书列表.Count() != 0)
            {
                foreach (var item in Newmodel.资质证书列表)
                {
                    if (string.IsNullOrWhiteSpace(item.名称) || string.IsNullOrWhiteSpace(item.发证机构) || string.IsNullOrWhiteSpace(item.等级))
                    {
                        item.已填写完整 = false;
                        break;
                    }
                    else
                    {
                        item.已填写完整 = true;
                    }
                }
            }
            Newmodel.审核数据.审核状态 = 审核状态.未审核;
            deleteIndex("/Lucene.Net/IndexDic/Gys", model.Id.ToString());
            //deleteIndex_ProductCatalog("/Lucene.Net/IndexDic/GysCatalog", model.企业基本信息.企业名称);
            用户管理.更新用户<供应商>(Newmodel);
            return Content("<script>if(confirm('请确保资质证书信息已完善，点击确定请等待审核，点击取消继续完善。')){window.location='/供应商后台/Service_Management';} else{window.location='/供应商后台/Qualify_Management';}</script>");
        }
        public ActionResult Qualify_Detail()
        {
            return View();
        }
        public ActionResult Part_Qualify_Detail()
        {
            try
            {
                供应商 Newmodel = 用户管理.查找用户<供应商>(currentUser.Id);
                int index = int.Parse(Request.QueryString["id"]);
                ViewData["index"] = index;
                return PartialView("Gys_Part/Part_Qualify_Detail", Newmodel);
            }
            catch
            {
                return Content("<script>window.location='/供应商后台/Qualify_Management';</script>");
            }

        }
        public ActionResult AddPic()
        {
            try
            {
                供应商 Newmodel = 用户管理.查找用户<供应商>(currentUser.Id);
                int index = int.Parse(Request.Form["index"].ToString());
                if (Newmodel.资质证书列表[index].资质证书电子扫描件 != null && Newmodel.资质证书列表[index].资质证书电子扫描件.Count() != 0)
                {
                    for (int i = 0; i < Newmodel.资质证书列表[index].资质证书电子扫描件.Count(); i++)
                    {
                        if (!System.IO.File.Exists(Server.MapPath(@Newmodel.资质证书列表[index].资质证书电子扫描件[i].路径)))
                        {
                            Newmodel.资质证书列表[index].资质证书电子扫描件.RemoveAt(i);
                        }
                    }
                }
                int count = Newmodel.资质证书列表[index].资质证书电子扫描件.Count();
                Newmodel.资质证书列表[index].资质证书电子扫描件[count - 1].名称 = Request.Form["name"].ToString();
                Newmodel.资质证书列表[index].资质证书电子扫描件[count - 1].说明 = Request.Form["direct"].ToString();
                Newmodel.审核数据.审核状态 = 审核状态.未审核;
                deleteIndex("/Lucene.Net/IndexDic/Gys", currentUser.Id.ToString());
                //deleteIndex_ProductCatalog("/Lucene.Net/IndexDic/GysCatalog", Newmodel.企业基本信息.企业名称);
                用户管理.更新用户<供应商>(Newmodel);
                return Redirect("/供应商后台/Qualify_Detail?id=" + index.ToString());
            }
            catch
            {
                return Redirect("/供应商后台/Qualify_Management");
            }

        }
        public ActionResult Del_Pic()
        {
            try
            {
                供应商 Newmodel = 用户管理.查找用户<供应商>(currentUser.Id);
                if (Newmodel.审核数据.审核状态 == 审核状态.审核通过)
                {
                    return Content("<script>alert('审核通过的用户不能再修改信息！');window.location='/供应商后台/';</script>");
                }
                int index = int.Parse(Request.QueryString["index"].ToString());
                int id = int.Parse(Request.QueryString["id"].ToString());
                if (System.IO.File.Exists(Server.MapPath(Newmodel.资质证书列表[index].资质证书电子扫描件[id].路径)))
                {
                    System.IO.File.Delete(Server.MapPath(@Newmodel.资质证书列表[index].资质证书电子扫描件[id].路径));
                }
                Newmodel.资质证书列表[index].资质证书电子扫描件.RemoveAt(id);
                Newmodel.审核数据.审核状态 = 审核状态.未审核;
                deleteIndex("/Lucene.Net/IndexDic/Gys", currentUser.Id.ToString());
                //deleteIndex_ProductCatalog("/Lucene.Net/IndexDic/GysCatalog", Newmodel.企业基本信息.企业名称);
                用户管理.更新用户<供应商>(Newmodel);
                return Redirect("/供应商后台/Qualify_Detail?id=" + index.ToString());
            }
            catch
            {
                return Redirect("/供应商后台/Qualify_Management");
            }
        }
        public ActionResult SubmitAndCheck()
        {
            供应商 model = 用户管理.查找用户<供应商>(currentUser.Id);
            if (model.可提供产品类别列表 == null || model.可提供产品类别列表.Count() == 0)
            {
                return Content("<script>alert('您还没有选择可供商品分类，请选择可供商品分类');window.location='/供应商后台/Gys_Product_Type';</script>");
            }
            else
            {
                if (!model.供应商用户信息.已提交)
                {
                    List<商品> goods = 商品管理.查询供应商商品(currentUser.Id).ToList<商品>();
                    if (goods == null || goods.Count() < 5)
                    {
                        return Content("<script>alert('您至少需要添加5件商品才能提交预审');window.location='/供应商后台/Gys_Product_AddStep1';</script>");
                    }
                    else if (model.可提供产品类别列表 == null || model.可提供产品类别列表.Count() == 0)
                    {
                        return Content("<script>alert('您还没有选择可供商品类别');window.location='/供应商后台/Gys_Product_Type';</script>");
                    }
                    else
                    {
                        return View(model);
                    }
                }
                else
                {
                    return Content("<script>alert('您已经提交过，不能再次提交');window.location='/供应商后台/';</script>");
                }
            }

        }
        public ActionResult WaitForCheck()
        {
            供应商 model = 用户管理.查找用户<供应商>(currentUser.Id);
            model.审核数据.审核状态 = 审核状态.未审核;
            model.供应商用户信息.已提交 = true;
            deleteIndex("/Lucene.Net/IndexDic/Gys", currentUser.Id.ToString());
            //deleteIndex_ProductCatalog("/Lucene.Net/IndexDic/GysCatalog", model.企业基本信息.企业名称);
            用户管理.更新用户<供应商>(model);
            return Content("<script>alert('您已成功提交所有资料，请等待带审核。');window.location='/供应商后台/';</script>");
        }
        [HttpPost]
        public ActionResult UploadTicketPic(HttpPostedFileBase fileData)
        {
            if (fileData != null)
            {
                string fileName = fileData.FileName;
                string filePath = 上传管理.获取上传路径<机票代售点>(媒体类型.图片, 路径类型.服务器本地路径);

                //string extension = fileData.FileName.Substring(fileData.FileName.LastIndexOf("."), (fileData.FileName.Length - fileData.FileName.LastIndexOf(".")));
                if (fileName.LastIndexOf("\\") > -1)
                {
                    fileName = fileName.Substring(fileName.LastIndexOf("\\") + 1);
                }
                fileName = 上传管理.获取文件名(fileName).Replace("{", "").Replace("}", "");
                if (!Directory.Exists(filePath))
                {
                    Directory.CreateDirectory(filePath);
                }
                fileData.SaveAs(string.Format("{0}\\{1}", filePath, fileName));
                return Content(string.Format("{0}{1}", 上传管理.获取上传路径<机票代售点>(媒体类型.图片, 路径类型.不带域名根路径), fileName));
            }
            else
            {
                return Content("0");
            }
        }
        [HttpPost]
        public ActionResult UploadPhoto(HttpPostedFileBase fileData)
        {
            if (fileData != null)
            {
                string filePath = 上传管理.获取上传路径<酒店>(媒体类型.图片, 路径类型.服务器本地路径);

                //string extension = fileData.FileName.Substring(fileData.FileName.LastIndexOf("."), (fileData.FileName.Length - fileData.FileName.LastIndexOf(".")));

                string fileName = 上传管理.获取文件名(fileData.FileName).Replace("{", "").Replace("}", "");
                if (!Directory.Exists(filePath))
                {
                    Directory.CreateDirectory(filePath);
                }
                fileData.SaveAs(string.Format("{0}\\{1}", filePath, fileName));
                return Content(string.Format("{0}{1}", 上传管理.获取上传路径<酒店>(媒体类型.图片, 路径类型.不带域名根路径), fileName));
            }
            else
            {
                return Content("0");
            }
        }
        [HttpPost]
        public ActionResult DeleteAttachment()
        {
            string qid = Request.Params["q"] ?? "";
            string name = Request.Params["n"] ?? "";
            try
            {
                if (!string.IsNullOrEmpty(qid) && !string.IsNullOrEmpty(name))
                {
                    UploadPic.DelPic(string.Format("{0}", name));
                    return Content(qid);
                }
                else
                {
                    return Content("0");
                }
            }
            catch { return Content("0"); }
        }
        public ActionResult Service_Management()
        {
            return View();
        }
        public ActionResult Part_Service_Management()
        {
            供应商 model = 用户管理.查找用户<供应商>(currentUser.Id);
            return PartialView("Gys_Part/Part_Service_Management", model);
        }
        [HttpPost]
        public ActionResult Service_Manage(供应商 model)
        {
            供应商 Newmodel = 用户管理.查找用户<供应商>(currentUser.Id);
            if (Newmodel.审核数据.审核状态 == 审核状态.审核通过)
            {
                return Content("<script>alert('审核通过的用户不能再修改信息！');window.location='/供应商后台/';</script>");
            }
            Newmodel.售后服务机构列表.Add(model.售后服务机构列表[0]);
            if (Newmodel.售后服务机构列表 != null && Newmodel.售后服务机构列表.Count() != 0)
            {
                foreach (var item in Newmodel.售后服务机构列表)
                {
                    if (string.IsNullOrWhiteSpace(item.售后服务机构名称) || string.IsNullOrWhiteSpace(item.所在地.省份) || string.IsNullOrWhiteSpace(item.所在地.城市) || string.IsNullOrWhiteSpace(item.所在地.区县) || string.IsNullOrWhiteSpace(item.负责人联系方式.联系人) || string.IsNullOrWhiteSpace(item.负责人联系方式.手机))
                    {
                        item.已填写完整 = false;
                        break;
                    }
                    else
                    {
                        item.已填写完整 = true;
                    }
                }
            }
            Newmodel.审核数据.审核状态 = 审核状态.未审核;
            deleteIndex("/Lucene.Net/IndexDic/Gys", model.Id.ToString());
            //deleteIndex_ProductCatalog("/Lucene.Net/IndexDic/GysCatalog", model.企业基本信息.企业名称);
            用户管理.更新用户<供应商>(Newmodel);
            return Content("<script>if(confirm('请确保售后服务机构信息以完善，点击确定请等待审核，点击取消继续添加。')){window.location='/供应商后台/Investor_Management';} else{window.location='/供应商后台/Service_Management';}</script>");
        }
        public ActionResult Del_Service()
        {
            try
            {
                供应商 Newmodel = 用户管理.查找用户<供应商>(currentUser.Id);
                if (Newmodel.审核数据.审核状态 == 审核状态.审核通过)
                {
                    return Content("<script>alert('审核通过的用户不能再修改信息！');window.location='/供应商后台/';</script>");
                }
                int index = int.Parse(Request.QueryString["index"]);
                Newmodel.售后服务机构列表.RemoveAt(index);
                Newmodel.审核数据.审核状态 = 审核状态.未审核;
                deleteIndex("/Lucene.Net/IndexDic/Gys", currentUser.Id.ToString());
                //deleteIndex_ProductCatalog("/Lucene.Net/IndexDic/GysCatalog", Newmodel.企业基本信息.企业名称);
                用户管理.更新用户<供应商>(Newmodel);
            }
            catch
            {
                return Redirect("/供应商后台/Service_Management");
            }
            return Redirect("/供应商后台/Service_Management");
        }
        public ActionResult Del_Investor()
        {
            try
            {
                供应商 Newmodel = 用户管理.查找用户<供应商>(currentUser.Id);
                if (Newmodel.审核数据.审核状态 == 审核状态.审核通过)
                {
                    return Content("<script>alert('审核通过的用户不能再修改信息！');window.location='/供应商后台/';</script>");
                }
                int index = int.Parse(Request.QueryString["index"]);
                Newmodel.出资人或股东信息.RemoveAt(index);
                Newmodel.审核数据.审核状态 = 审核状态.未审核;
                deleteIndex("/Lucene.Net/IndexDic/Gys", currentUser.Id.ToString());
                //deleteIndex_ProductCatalog("/Lucene.Net/IndexDic/GysCatalog", Newmodel.企业基本信息.企业名称);
                用户管理.更新用户<供应商>(Newmodel);
            }
            catch
            {
                return Content("<script>if(confirm('请确保出资人信息以完善，点击确定请等待审核，点击取消继续添加。')){window.location='/供应商后台/Gys_Financial_Manage';} else{window.location='/供应商后台/Investor_Management';}</script>");
            }
            return Content("<script>if(confirm('请确保出资人信息以完善，点击确定请等待审核，点击取消继续添加。')){window.location='/供应商后台/Gys_Financial_Manage';} else{window.location='/供应商后台/Investor_Management';}</script>");
        }
        public ActionResult Del_Finacial()
        {
            try
            {
                供应商 Newmodel = 用户管理.查找用户<供应商>(currentUser.Id);
                if (Newmodel.审核数据.审核状态 == 审核状态.审核通过)
                {
                    return Content("<script>alert('审核通过的用户不能再修改信息！');window.location='/供应商后台/';</script>");
                }
                int index = int.Parse(Request.QueryString["id"]);
                Newmodel.财务状况信息.RemoveAt(index);
                Newmodel.审核数据.审核状态 = 审核状态.未审核;
                deleteIndex("/Lucene.Net/IndexDic/Gys", currentUser.Id.ToString());
                //deleteIndex_ProductCatalog("/Lucene.Net/IndexDic/GysCatalog", Newmodel.企业基本信息.企业名称);
                用户管理.更新用户<供应商>(Newmodel);
                return Redirect("/供应商后台/Gys_Financial_Manage");
            }
            catch
            {
                return Redirect("/供应商后台/Gys_Financial_Manage");
            }
        }
        public ActionResult PartGys_Financial_Manage()//财务税务管理
        {
            供应商 user = 用户管理.查找用户<供应商>(currentUser.Id);
            return PartialView("Gys_Part/PartGys_Financial_Manage", user);
        }
        public ActionResult Gys_Product_Type()
        {
            return View();
        }
        public ActionResult Part_Gys_Product_Type()
        {
            IEnumerable<供应商服务记录> Service = 供应商服务记录管理.查询供应商服务记录(0, 0, Query<供应商服务记录>.Where(o => o.所属供应商.用户ID == currentUser.Id));
            if (Service != null && Service.Count() != 0)
            {
                ViewData["service"] = Service.First();
            }
            else
            {
                ViewData["service"] = new 供应商服务记录();
            }
            供应商 m = 用户管理.查找用户<供应商>(currentUser.Id);
            IEnumerable<商品分类> model = 商品分类管理.查找子分类();
            ViewData["goodType"] = model;
            return PartialView("Gys_Part/Part_Gys_Product_Type", m);
        }
        public string Modify_Good_Type()
        {
            try
            {
                供应商 m = 用户管理.查找用户<供应商>(currentUser.Id);
                string Fname = Request.QueryString["first_type"];
                string[] SecondName = Request.QueryString["second_type"].ToString().Split(',');
                供应商._产品类别 GoodType = m.可提供产品类别列表.Find(o => o.一级分类 == Fname);
                GoodType.二级分类.Clear();
                for (int k = 0; k < SecondName.Length - 1; k++)
                {
                    GoodType.二级分类.Add(SecondName[k]);
                }
                if (GoodType.二级分类.Count() > 5)
                {
                    return "二级分类不能超过5个";
                }
                else
                {
                    用户管理.更新用户<供应商>(m);
                    return "修改成功！";
                }
            }
            catch
            {
                return "出错啦！请您重新修改。";
            }
        }
        public ActionResult Search_Type()
        {
            List<string> Product_Name = new List<string>();
            string name = Request.QueryString["n"];
            IEnumerable<商品分类> model = 商品分类管理.查找子分类();
            //IEnumerable<商品分类> model1 = null;
            if (name == "通用物资")
            {
                if (model != null)
                {
                    foreach (var item in model)
                    {
                        if (item.分类性质.ToString() != "专用物资")
                        {
                            Product_Name.Add(item.分类名);
                        }
                    }
                }
            }
            else if (name == "专用物资")
            {
                if (model != null)
                {
                    foreach (var item in model)
                    {
                        if (item.分类性质.ToString() == name)
                        {
                            Product_Name.Add(item.分类名);
                        }
                    }
                }
            }
            JsonResult json = new JsonResult() { Data = Product_Name };
            return Json(json, JsonRequestBehavior.AllowGet);
        }
        public ActionResult TwoClass()
        {
            string firstClass = Request.QueryString["fclass"];
            List<string> Product_Name = new List<string>();
            IEnumerable<商品分类> model = 商品分类管理.查找子分类();
            IEnumerable<商品分类> twoClass = null;
            foreach (var item in model)
            {
                if (item.分类名 == firstClass)
                {
                    twoClass = item.子分类;
                    break;
                }
            }
            if (twoClass != null)
            {
                foreach (var item in twoClass)
                {
                    Product_Name.Add(item.分类名);
                }
            }
            JsonResult json = new JsonResult() { Data = Product_Name };
            return Json(json, JsonRequestBehavior.AllowGet);
        }
        public ActionResult ThirdClass()
        {
            List<string> Product_Name = new List<string>();
            string alltype = Request.QueryString["tpy"];
            string name = Request.QueryString["n"];
            IEnumerable<商品分类> twoClass = null;
            //IEnumerable<商品分类> thirdClass = null;
            IEnumerable<商品分类> model = 商品分类管理.查找子分类();
            if (model != null)
            {
                if (alltype == "专用物资")
                {
                    foreach (var item in model)
                    {
                        if (item.分类名 == name)
                        {
                            twoClass = item.子分类;
                            break;
                        }
                    }
                    if (twoClass != null)
                    {
                        foreach (var item in twoClass)
                        {
                            Product_Name.Add(item.分类名);
                        }
                    }
                }
                else
                {
                    foreach (var item in model)
                    {
                        if (item.分类名 == name)
                        {
                            twoClass = item.子分类;
                            break;
                        }
                    }
                    if (twoClass != null)
                    {
                        foreach (var item in twoClass)
                        {
                            Product_Name.Add(item.分类名);
                        }
                    }
                }

            }
            JsonResult json = new JsonResult() { Data = Product_Name };
            return Json(json, JsonRequestBehavior.AllowGet);
        }
        public string Add_Good_Type()
        {
            供应商 Newmodel = 用户管理.查找用户<供应商>(currentUser.Id);
            供应商._产品类别 product = new 供应商._产品类别();
            bool exist = false;
            string oldfirst = Request.QueryString["old"];
            string alltype = Request.QueryString["tpy"];
            string FirstType = Request.QueryString["first_type"];
            string SecondType = Request.QueryString["second_type"];
            product.一级分类 = FirstType;
            string[] secondstype = SecondType.Split(',');
            if (Newmodel.可提供产品类别列表 != null && Newmodel.可提供产品类别列表.Count() != 0)
            {
                for (int n = 0; n < Newmodel.可提供产品类别列表.Count; n++)
                {
                    if (FirstType == Newmodel.可提供产品类别列表[n].一级分类)
                    {
                        exist = true;
                        break;
                    }
                }
            }
            if (!exist)
            {
                bool IsExist = false;
                if (Newmodel.可提供产品类别列表 != null && Newmodel.可提供产品类别列表.Count != 0)
                {
                    foreach (var item in Newmodel.可提供产品类别列表)
                    {
                        if (item.一级分类 == FirstType)
                        {
                            IsExist = true;
                            break;
                        }
                    }
                    if (!IsExist)
                    {
                        Newmodel.企业基本信息.所属行业 += FirstType + ";";
                    }
                }
                else
                {
                    Newmodel.企业基本信息.所属行业 += FirstType + ";";
                }
                if (product.二级分类 == null)
                {
                    product.二级分类 = new List<string>();
                }
                for (int i = 0; i < secondstype.Length - 1; i++)
                {
                    product.二级分类.Add(secondstype[i]);
                }
                if (alltype == "专用物资")
                {
                    if (!string.IsNullOrWhiteSpace(oldfirst))
                    {
                        if (oldfirst == "医疗设备" || oldfirst == "油料设备器材" || oldfirst == "给养器材" || oldfirst == "军用食品" || oldfirst == "被装材料" || oldfirst == "后勤装备" || oldfirst == "药品" || oldfirst == "被装" || oldfirst == "医用耗材" || oldfirst == "军事交通器材" || oldfirst == "基建营房工程器材")
                        {
                            Newmodel.可提供产品类别列表.Add(product);
                        }
                        else
                        {
                            Newmodel.可提供产品类别列表.Clear();
                            Newmodel.可提供产品类别列表.Add(product);
                        }
                    }
                    else
                    {
                        Newmodel.可提供产品类别列表.Add(product);
                    }
                }
                else
                {
                    if (!string.IsNullOrWhiteSpace(oldfirst))
                    {
                        if (oldfirst == "医疗设备" || oldfirst == "油料设备器材" || oldfirst == "给养器材" || oldfirst == "军用食品" || oldfirst == "被装材料" || oldfirst == "后勤装备" || oldfirst == "药品" || oldfirst == "被装" || oldfirst == "医用耗材" || oldfirst == "军事交通器材" || oldfirst == "基建营房工程器材")
                        {
                            Newmodel.可提供产品类别列表.Clear();
                            Newmodel.可提供产品类别列表.Add(product);
                        }
                        else
                        {
                            Newmodel.可提供产品类别列表.Add(product);
                        }
                    }
                    else
                    {
                        Newmodel.可提供产品类别列表.Add(product);
                    }
                }
                Newmodel.审核数据.审核状态 = 审核状态.未审核;
                deleteIndex("/Lucene.Net/IndexDic/Gys", currentUser.Id.ToString());
                return 用户管理.更新用户<供应商>(Newmodel) ? (Newmodel.可提供产品类别列表.Count - 1).ToString() : "-1";
            }
            else
            {
                return "-1";
            }
        }
        [HttpPost]
        public ActionResult Sales_Manage(供应商 model)
        {
            供应商 Newmodel = 用户管理.查找用户<供应商>(currentUser.Id);
            if (Newmodel.审核数据.审核状态 == 审核状态.审核通过)
            {
                return Content("<script>alert('审核通过的用户不能再修改信息！');window.location='/供应商后台/';</script>");
            }
            if (!System.IO.File.Exists(Server.MapPath(@Newmodel.工商注册信息.组织机构代码证电子扫描件)))
            {
                Newmodel.工商注册信息.组织机构代码证电子扫描件 = "";
            }
            if (!System.IO.File.Exists(Server.MapPath(@Newmodel.工商注册信息.基本账户开户银行资信证明电子扫描件)))
            {
                Newmodel.工商注册信息.基本账户开户银行资信证明电子扫描件 = "";
            }
            if (!System.IO.File.Exists(Server.MapPath(@Newmodel.工商注册信息.近3年缴纳社会保证金证明电子扫描件)))
            {
                Newmodel.工商注册信息.近3年缴纳社会保证金证明电子扫描件 = "";
            }
            if (!System.IO.File.Exists(Server.MapPath(@Newmodel.工商注册信息.组织机构代码证电子扫描件)))
            {
                Newmodel.工商注册信息.组织机构代码证电子扫描件 = "";
            }
            if (!System.IO.File.Exists(Server.MapPath(@Newmodel.营业执照信息.税务登记证电子扫描件)))
            {
                Newmodel.营业执照信息.税务登记证电子扫描件 = "";
            }
            string gs_Reg = Newmodel.工商注册信息.组织机构代码证电子扫描件;
            string gs_bankCode = Newmodel.工商注册信息.基本账户开户银行资信证明电子扫描件;
            string gs_bzj = Newmodel.工商注册信息.近3年缴纳社会保证金证明电子扫描件;
            string str = Newmodel.工商注册信息.近3年有无重大违法记录电子扫描件;
            string sales = Newmodel.营业执照信息.营业执照电子扫描件;
            string Tax = Newmodel.营业执照信息.税务登记证电子扫描件;
            Newmodel.营业执照信息 = model.营业执照信息;
            Newmodel.工商注册信息 = model.工商注册信息;
            Newmodel.工商注册信息.组织机构代码证电子扫描件 = gs_Reg;
            Newmodel.工商注册信息.基本账户开户银行资信证明电子扫描件 = gs_bankCode;
            Newmodel.工商注册信息.近3年缴纳社会保证金证明电子扫描件 = gs_bzj;
            Newmodel.工商注册信息.近3年有无重大违法记录电子扫描件 = str;
            Newmodel.营业执照信息.营业执照电子扫描件 = sales;
            Newmodel.营业执照信息.税务登记证电子扫描件 = Tax;
            if (string.IsNullOrWhiteSpace(Newmodel.工商注册信息.基本账户开户银行) ||
                string.IsNullOrWhiteSpace(Newmodel.工商注册信息.基本账户开户银行资信证明电子扫描件) ||
                string.IsNullOrWhiteSpace(Newmodel.工商注册信息.基本账户银行帐号) ||
                string.IsNullOrWhiteSpace(Newmodel.工商注册信息.近3年缴纳社会保证金证明电子扫描件) ||
                string.IsNullOrWhiteSpace(Newmodel.工商注册信息.组织机构代码证电子扫描件) ||
                string.IsNullOrWhiteSpace(Newmodel.工商注册信息.组织机构代码))
            {
                Newmodel.工商注册信息.已填写完整 = false;
            }
            else
            {
                Newmodel.工商注册信息.已填写完整 = true;
            }
            if (string.IsNullOrWhiteSpace(Newmodel.营业执照信息.营业执照电子扫描件) ||
                string.IsNullOrWhiteSpace(Newmodel.营业执照信息.营业执照发证机关) ||
                string.IsNullOrWhiteSpace(Newmodel.营业执照信息.经营范围) || string.IsNullOrWhiteSpace(Newmodel.营业执照信息.税务登记证电子扫描件))
            {
                Newmodel.营业执照信息.已填写完整 = false;
            }
            else
            {
                Newmodel.营业执照信息.已填写完整 = true;
            }
            Newmodel.审核数据.审核状态 = 审核状态.未审核;
            deleteIndex("/Lucene.Net/IndexDic/Gys", model.Id.ToString());
            //deleteIndex_ProductCatalog("/Lucene.Net/IndexDic/GysCatalog", model.企业基本信息.企业名称);
            用户管理.更新用户<供应商>(Newmodel);
            return Content("<script>if(confirm('请确保营业信息已完成，点击确定请等待审核，点击取消继续完善。')){window.location='/供应商后台/Qualify_Management';} else{window.location='/供应商后台/Gys_Sales_Manage';}</script>");
        }
        public ActionResult Part_Sales_Manage()//营业管理
        {
            var model = 用户管理.查找用户<供应商>(currentUser.Id);
            return PartialView("Gys_Part/Part_Sales_Manage", model);
        }
        public ActionResult Gys_Sales_Manage()//营业管理
        {
            return View();
        }
        public ActionResult Gys_Financial_Manage()//财务税务管理
        {
            return View();
        }
        public ActionResult Gys_Manage()//基本信息管理
        {
            return View();
        }
        public ActionResult Vip_Password_Manage()
        {
            return View();
        }
        public ActionResult Vip_Manage()
        {
            return View();
        }
        public ActionResult Gys_Znxx()
        {
            return View();
        }
        public ActionResult Gys_ZnxxAdd()
        {
            return View();
        }
        public ActionResult Gys_Xttz()
        {
            return View();
        }

        public ActionResult Gys_Ztb_Search_Zb()
        {
            return View();
        }

        public ActionResult Gys_Ztb_Search_Cg()
        {
            return View();
        }

        public ActionResult Gys_Ztb_Dygl()
        {
            return View();
        }

        public ActionResult Gys_Product_Detail()
        {
            return View();
        }

        public ActionResult Gys_Spgl_Update()
        {
            return View();
        }

        public ActionResult Gys_News_Show()
        {
            return View();
        }

        public ActionResult Gys_Register()
        {
            return View();
        }
        public ActionResult Gys_Product_List()
        {
            return View();
        }

        public ActionResult Gys_Product_Add()
        {
            return View();
        }

        public ActionResult Gys_ComplainAdd()
        {
            return View();
        }
        public ActionResult Gys_ComplainList()
        {
            return View();
        }
        public ActionResult Gys_ComplainDetail()
        {
            return View();
        }
        public ActionResult Gys_ConsultAdd()
        {
            return View();
        }
        public ActionResult Gys_ConsultList()
        {
            return View();
        }
        public ActionResult Gys_ConsultDetail()
        {
            return View();
        }
        public ActionResult Gys_GuideOne()
        {
            return View();
        }
        public ActionResult Gys_GuideThr()
        {
            return View();
        }
        public ActionResult Gys_GuideTwo()
        {
            return View();
        }
        public ActionResult Gys_SuggestAdd()
        {
            return View();
        }
        public ActionResult Gys_SuggestList()
        {
            return View();
        }
        public ActionResult Gys_SuggestDetail()
        {
            return View();
        }
        public ActionResult Gys_Product_Modify()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult Gys_Product_Modify(商品 model)
        {

            try
            {
                //商品 Pro_Model = new 商品();
                商品 Pro_Model = 商品管理.查找商品(long.Parse(Request.Form["idsttrsrr"]));
                if (Pro_Model.中标商品)
                {
                    Pro_Model.中标信息 = new List<商品._中标信息>();
                    //判断是否有项目编号
                    var proliststr = Request.Form["proliststr"];
                    if (!string.IsNullOrWhiteSpace(proliststr))
                    {
                        Pro_Model.基本数据.已屏蔽 = false;
                        var pronamelist = proliststr.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
                        foreach (var proname in pronamelist)
                        {
                            Pro_Model.中标信息.Add(new 商品._中标信息
                            {
                                中标项目编号 = Request.Form["pronum___" + proname],
                                中标数量 = int.Parse(Request.Form["procount___" + proname]),
                                中标金额 = decimal.Parse(Request.Form["proprice___" + proname])
                            });
                        }
                    }
                }
                //Pro_Model.Id = int.Parse(Request.Form["idsttrsrr"]);
                //Pro_Model.商品信息.所属供应商.用户ID = this.currentUser.Id;
                //Pro_Model.商品信息.所属商品分类.商品分类ID = long.Parse(Request.Form["classsttrsrr"]);
                string price_sttr = Request.Form["pricesttrsrr"];   //尺寸|颜色^1|155|红色&2|155|黄色&3|160|红色&4|160|黄色&

                if (!string.IsNullOrEmpty(price_sttr))
                {
                    string[] price_a1 = price_sttr.Split(new[] { "^^^^" }, StringSplitOptions.None);//price_a1[0]:尺寸|颜色、尺寸
                    Pro_Model.销售信息.价格属性组合 = new 商品._价格属性组合();
                    string tempindexstr = "||||";
                    if (price_a1[0].IndexOf(tempindexstr) > -1)
                    {
                        Pro_Model.销售信息.价格属性组合.设置属性组合列表(price_a1[0].Split(new[] { tempindexstr }, StringSplitOptions.None));
                    }
                    else
                    {
                        Pro_Model.销售信息.价格属性组合.设置属性组合列表(price_a1[0]);
                    }
                    //price_a1[1]:1|155|红色&2|155|黄色&3|160|红色&4|160|黄色&

                    string[] price_a2 = price_a1[1].Substring(0, price_a1[1].Length - 4).Split(new[] { "&&&&" }, StringSplitOptions.None); //price_a2[i]:4#160|黄色、4#160
                    for (int i = 0; i < price_a2.Length; i++)
                    {
                        string[] price_a3 = price_a2[i].Split(new[] { "####" }, StringSplitOptions.None);  //price_a3[0]:4 price_a3[1]:160|黄色、160
                        if (!string.IsNullOrEmpty(price_a3[0]))
                        {
                            var price_march = decimal.Parse(price_a3[0]);
                            if (price_a3[1].IndexOf(tempindexstr) > -1)
                            {
                                Pro_Model.销售信息.价格属性组合.添加组合(price_march, price_a3[1].Split(new[] { tempindexstr }, StringSplitOptions.None));
                            }
                            else
                            {
                                Pro_Model.销售信息.价格属性组合.添加组合(price_march, price_a3[1]);
                            }
                        }
                    }
                }
                if (!string.IsNullOrEmpty(Request.Form["sttrsrr"]))
                {
                    Dictionary<string, Dictionary<string, List<string>>> proatt = new Dictionary<string, Dictionary<string, List<string>>>();
                    string sttr = Request.Form["sttrsrr"];
                    sttr = sttr.Substring(0, sttr.Length - 4);
                    string[] a1 = sttr.Split(new[] { "$$$$" }, StringSplitOptions.None);
                    for (int i = 0; i < a1.Length; i++)
                    {
                        string a2 = a1[i];//   测试分类222|测试属性999^值11#值22#值33#值44#值55#值66#*测试属性888^值111#值222#值333#*
                        string[] a3 = a2.Split(new[] { "||||" }, StringSplitOptions.None);   //a3[0]:测试分类222  a3[1]:测试属性999^值11#值22#值33#值44#值55#值66#*测试属性888^值111#值222#值333#*
                        //尺寸^*颜色^*材料^丝绸#纯棉#*重量^8kg#*

                        //string a4=a3[1];//测试属性999^值11#值22#值33#值44#值55#值66#测试属性888^值111#值222#值333#
                        string[] a4 = a3[1].Substring(0, a3[1].Length - 4).Split(new[] { "****" }, StringSplitOptions.None);    //a4[i]:测试属性888^值111#值222#值333#
                        Dictionary<string, List<string>> temp = new Dictionary<string, List<string>>();
                        for (int j = 0; j < a4.Length; j++)
                        {
                            if (a4[j].Substring(a4[j].Length - 4) != "^^^^")
                            {
                                string[] a5 = a4[j].Split(new[] { "^^^^" }, StringSplitOptions.None);    //a5[0]:测试属性888   a5[1]:值111#值222#值333#
                                var a_value = a5[1].Substring(0, a5[1].Length - 4);
                                if (a_value == "")
                                {
                                    temp.Add(a5[0], new List<string>());
                                }
                                else
                                {
                                    string[] a6 = a_value.Split(new[] { "####" }, StringSplitOptions.None);
                                    a6 = a6.Select(o => o.Trim()).ToArray();
                                    temp.Add(a5[0], new List<string>(a6));
                                }
                            }
                            else
                            {
                                temp.Add(a4[j].Substring(0, a4[j].Length - 4), new List<string>());
                            }
                        }
                        proatt.Add(a3[0], temp);
                    }
                    Pro_Model.商品数据.商品属性 = proatt;
                }
                Pro_Model.商品信息.商品名 = model.商品信息.商品名.Trim();
                if (!string.IsNullOrWhiteSpace(model.商品信息.品牌))
                {
                    Pro_Model.商品信息.品牌 = model.商品信息.品牌.Trim().ToLower();
                }
                else
                {
                    Pro_Model.商品信息.品牌 = model.商品信息.品牌;
                }
                if (!string.IsNullOrWhiteSpace(model.商品信息.型号))
                {
                    Pro_Model.商品信息.型号 = model.商品信息.型号.Trim();
                }
                else
                {
                    Pro_Model.商品信息.型号 = model.商品信息.型号;
                }
                Pro_Model.商品信息.计量单位 = model.商品信息.计量单位.Trim();
                Pro_Model.销售信息.价格 = model.销售信息.价格;
                if (!string.IsNullOrWhiteSpace(Request.Form["oldpicture"].ToString()))
                {
                    Pro_Model.商品信息.商品图片 = new List<string>(Request.Form["oldpicture"].Substring(0, Request.Form["oldpicture"].Length - 1).Split('|'));
                }
                if (!string.IsNullOrEmpty(Request.Form["pro_imgstr"]))
                {
                    string[] img = Request.Form["pro_imgstr"].Substring(0, Request.Form["pro_imgstr"].Length - 1).Split('|');
                    foreach (var item in img)
                    {
                        if (System.IO.File.Exists(Server.MapPath(@item)))
                        {
                            Pro_Model.商品信息.商品图片.Add(item);
                        }
                    }
                }
                Pro_Model.商品数据.商品简介 = model.商品数据.商品简介;
                Pro_Model.商品数据.商品详情 = model.商品数据.商品详情;
                Pro_Model.商品数据.售后服务 = model.商品数据.售后服务;
                Pro_Model.基本数据.已屏蔽 = true;
                商品管理.更新商品(Pro_Model);
                //CreateIndex(Pro_Model, "/Lucene.Net/IndexDic/Product");

                deleteIndex("/Lucene.Net/IndexDic/Product", Pro_Model.Id.ToString());

                return RedirectToAction("Gys_Product_List", "供应商后台");
            }
            catch
            {
                return Content("<script>alert('输入有误!请检查数据格式');location.href='javascript:history.go(-1)';</script>");
            }
        }


        /// <summary>
        /// 添加商品
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult Gys_Product_Add(商品 model)
        {
            商品 Pro_Model = new 商品();

            try
            {
                string price_sttr = Request.Form["pricesttrsrr"];

                if (!string.IsNullOrEmpty(price_sttr))
                {
                    string[] price_a1 = price_sttr.Split(new[] { "^^^^" }, StringSplitOptions.None);
                    //price_a1[0]:组合名称（系列、系列||||型号、系列||||型号||||颜色）

                    Pro_Model.销售信息.价格属性组合 = new 商品._价格属性组合();
                    string tempindexstr = "||||";
                    if (price_a1[0].IndexOf(tempindexstr) > -1)
                    {
                        Pro_Model.销售信息.价格属性组合.设置属性组合列表(price_a1[0].Split(new[] { tempindexstr }, StringSplitOptions.None));
                    }
                    else
                    {
                        Pro_Model.销售信息.价格属性组合.设置属性组合列表(price_a1[0]);
                    }

                    //price_a1[1]:价格####属性1||||属性2||||属性3&&&&、价格####属性1||||属性2&&&&、价格####属性1&&&&（无、）

                    string[] price_a2 = price_a1[1].Split(new[] { "&&&&" }, StringSplitOptions.None);
                    //price_a2[i]:价格####属性1||||属性2||||属性3、价格####属性1||||属性2、价格####属性1

                    for (int i = 0; i < price_a2.Length; i++)
                    {
                        string[] price_a3 = price_a2[i].Split(new[] { "####" }, StringSplitOptions.None);
                        //price_a3[0]:价格    price_a3[1]:属性1||||属性2||||属性3、属性1||||属性2、属性1

                        if (!string.IsNullOrEmpty(price_a3[0]))
                        {
                            var price_march = decimal.Parse(price_a3[0]);
                            if (price_a3[1].IndexOf(tempindexstr) > -1)
                            {
                                Pro_Model.销售信息.价格属性组合.添加组合(price_march, price_a3[1].Split(new[] { tempindexstr }, StringSplitOptions.None));
                            }
                            else
                            {
                                Pro_Model.销售信息.价格属性组合.添加组合(price_march, price_a3[1]);
                            }
                        }
                    }
                }
                if (!string.IsNullOrEmpty(Request.Form["sttrsrr"]))
                {
                    Dictionary<string, Dictionary<string, List<string>>> proatt = new Dictionary<string, Dictionary<string, List<string>>>();
                    string sttr = Request.Form["sttrsrr"];
                    sttr = sttr.Substring(0, sttr.Length - 4);
                    string[] a1 = sttr.Split(new[] { "$$$$" }, StringSplitOptions.None);
                    for (int i = 0; i < a1.Length; i++)
                    {
                        string a2 = a1[i];//   测试分类222|测试属性999^值11#值22#值33#值44#值55#值66#*测试属性888^值111#值222#值333#*
                        string[] a3 = a2.Split(new[] { "||||" }, StringSplitOptions.None);   //a3[0]:测试分类222  a3[1]:测试属性999^值11#值22#值33#值44#值55#值66#*测试属性888^值111#值222#值333#*
                        //尺寸^*颜色^*材料^丝绸#纯棉#*重量^8kg#*

                        //string a4=a3[1];//测试属性999^值11#值22#值33#值44#值55#值66#测试属性888^值111#值222#值333#
                        string[] a4 = a3[1].Substring(0, a3[1].Length - 4).Split(new[] { "****" }, StringSplitOptions.None);    //a4[i]:测试属性888^值111#值222#值333#
                        Dictionary<string, List<string>> temp = new Dictionary<string, List<string>>();
                        for (int j = 0; j < a4.Length; j++)
                        {
                            if (a4[j].Substring(a4[j].Length - 4) != "^^^^")
                            {
                                string[] a5 = a4[j].Split(new[] { "^^^^" }, StringSplitOptions.None);    //a5[0]:测试属性888   a5[1]:值111#值222#值333#
                                var a_value = a5[1].Substring(0, a5[1].Length - 4);
                                if (a_value == "")
                                {
                                    temp.Add(a5[0], new List<string>());
                                }
                                else
                                {
                                    string[] a6 = a_value.Split(new[] { "####" }, StringSplitOptions.None);
                                    a6 = a6.Select(o => o.Trim()).ToArray();
                                    temp.Add(a5[0], new List<string>(a6));
                                }
                            }
                            else
                            {
                                temp.Add(a4[j].Substring(0, a4[j].Length - 4), new List<string>());
                            }
                        }
                        proatt.Add(a3[0], temp);
                    }
                    Pro_Model.商品数据.商品属性 = proatt;
                }
                Pro_Model.商品信息.商品名 = model.商品信息.商品名.Trim();

                if (!string.IsNullOrWhiteSpace(model.商品信息.品牌))
                {
                    Pro_Model.商品信息.品牌 = model.商品信息.品牌.Trim().ToLower();
                }
                Pro_Model.商品信息.型号 = model.商品信息.型号.Trim();
                Pro_Model.商品信息.计量单位 = model.商品信息.计量单位.Trim();
                Pro_Model.销售信息.价格 = model.销售信息.价格;
                List<string> picUrl = new List<string>();
                string[] url = Request.Form["pro_imgstr"].ToString().Split('|');
                for (int k = 0; k < url.Length - 1; k++)
                {
                    if (System.IO.File.Exists(Server.MapPath(@url[k])))
                    {
                        picUrl.Add(url[k]);
                    }
                }
                if (picUrl != null && picUrl.Count() != 0)
                {
                    Pro_Model.商品信息.商品图片 = picUrl;
                }
                Pro_Model.商品数据.商品简介 = model.商品数据.商品简介;
                Pro_Model.商品数据.商品详情 = model.商品数据.商品详情;
                Pro_Model.商品数据.售后服务 = model.商品数据.售后服务;
                if (currentUser.供应商用户信息.协议供应商)
                {
                    Pro_Model.采购信息.参与协议采购 = true;
                }
                if (currentUser.供应商用户信息.应急供应商)
                {
                    Pro_Model.采购信息.参与应急采购 = true;
                }
                Pro_Model.基本数据.已屏蔽 = true;
                //判断是否有项目编号 中标信息
                var proliststr = Request.Form["proliststr"];
                if (!string.IsNullOrWhiteSpace(proliststr))
                {
                    Pro_Model.基本数据.已屏蔽 = false;
                    Pro_Model.中标商品 = true;
                    var pronamelist = proliststr.Split(new[] {'|'}, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var proname in pronamelist)
                    {
                        Pro_Model.中标信息.Add(new 商品._中标信息 { 
                            中标项目编号 = Request.Form["pronum___" + proname],
                            中标数量 = int.Parse(Request.Form["procount___" + proname]),
                            中标金额 =decimal.Parse(Request.Form["proprice___" + proname])
                        });
                    }
                }
                商品管理.添加商品(Pro_Model, long.Parse(Request.Form["idsttrsrr"]), currentUser.Id);
                //CreateIndex(Pro_Model, "/Lucene.Net/IndexDic/Product");

                return RedirectToAction("Gys_Product_List", "供应商后台");
            }
            catch
            {
                return Content("<script>alert('输入有误!请检查数据格式');location.href='javascript:history.go(-1)';</script>");
            }
        }
        [HttpPost]
        public ActionResult UploadAttach_ysd()
        {
            try
            {
                供应商 model = 用户管理.查找用户<供应商>(currentUser.Id);
                Dictionary<string, List<string>> url = new Dictionary<string, List<string>>();
                List<string> str = new List<string>();
                bool exist = false;
                if (model.历史投标补充资料 != null && model.历史投标补充资料.Count() != 0)
                {
                    url = model.历史投标补充资料;
                }
                string name = "";
                if (Request.Form["picture"] != null)
                {
                    name = Request.Form["picture"].ToString();
                }
                //long id = 0;
                string which = "";
                if (Request.Form["which"] != null)
                {
                    which = Request.Form["which"].ToString();
                }
                string path = "";
                for (int j = 0; j < Request.Files.Count; j++)
                {
                    HttpPostedFileBase fileData = Request.Files[j];
                    string filePath = 上传管理.获取上传路径<验收单>(媒体类型.附件, 路径类型.不带域名根路径);
                    string fname = UploadImages_Ysd(fileData);
                    path += filePath + fname + "|";
                    ViewData["path"] = path;
                    ViewData["which"] = which;
                    str.Add(filePath + fname);
                }
                if (!string.IsNullOrWhiteSpace(name))
                {
                    if (str.Count() != 0)
                    {
                        if (url.Count() != 0)
                        {
                            foreach (var i in url)
                            {
                                if (i.Key == name)
                                {
                                    exist = true;
                                    break;
                                }
                            }
                            if (!exist)
                            {
                                url.Add(name, str);
                            }
                            else
                            {
                                foreach (var m in str)
                                {
                                    url[name].Add(m);
                                }
                            }
                        }
                        else
                        {
                            url.Add(name, str);
                        }
                        model.历史投标补充资料 = url;
                    }
                    用户管理.更新用户<供应商>(model);
                }
            }
            catch
            {
                ViewData["path"] = "出错|";
            }
            return View();
        }
        public string Geturi()
        {
            string url = "";
            try
            {
                供应商 model = 用户管理.查找用户<供应商>(currentUser.Id);
                string which = "";
                if (!string.IsNullOrWhiteSpace(Request.QueryString["which"]))
                {
                    which = Request.QueryString["which"];
                    switch (which)
                    {
                        case "lawperson": if (System.IO.File.Exists(Server.MapPath(@model.法定代表人信息.法定代表人身份证电子扫描件)))
                            {
                                url += model.法定代表人信息.法定代表人身份证电子扫描件 + "|";
                            }
                            break;
                        case "qualify":
                            if (model.资质证书列表 != null && model.资质证书列表.Count() != 0)
                            {
                                for (int a = 0; a < model.资质证书列表.Count(); a++)
                                {
                                    if (model.资质证书列表[a].资质证书电子扫描件 != null && model.资质证书列表[a].资质证书电子扫描件.Count != 0)
                                    {
                                        foreach (var item in model.资质证书列表[a].资质证书电子扫描件)
                                        {
                                            url += item.路径 + "|";
                                        }
                                    }
                                }
                            }
                            break;
                        case "tax": if (System.IO.File.Exists(Server.MapPath(@model.税务信息.近3年完税证明电子扫描件)))
                            {
                                url += model.税务信息.近3年完税证明电子扫描件 + "|";
                            } break;
                        case "sale":
                            if (System.IO.File.Exists(Server.MapPath(@model.营业执照信息.营业执照电子扫描件)))
                            {
                                url += model.营业执照信息.营业执照电子扫描件 + "|";
                            }
                            if (System.IO.File.Exists(Server.MapPath(@model.工商注册信息.基本账户开户银行资信证明电子扫描件)))
                            {
                                url += model.工商注册信息.基本账户开户银行资信证明电子扫描件 + "|";
                            }
                            if (System.IO.File.Exists(Server.MapPath(@model.工商注册信息.近3年缴纳社会保证金证明电子扫描件)))
                            {
                                url += model.工商注册信息.近3年缴纳社会保证金证明电子扫描件 + "|";
                            }
                            if (System.IO.File.Exists(Server.MapPath(@model.工商注册信息.近3年有无重大违法记录电子扫描件)))
                            {
                                url += model.工商注册信息.近3年有无重大违法记录电子扫描件 + "|";
                            }
                            if (System.IO.File.Exists(Server.MapPath(@model.工商注册信息.组织机构代码证电子扫描件)))
                            {
                                url += model.工商注册信息.组织机构代码证电子扫描件 + "|";
                            }
                            break;
                        case "other":
                            if (currentUser.历史投标补充资料 != null && currentUser.历史投标补充资料.Count() != 0)
                            {
                                foreach (var item in currentUser.历史投标补充资料)
                                {
                                    string[] str = item.Value.ToArray();
                                    for (int n = 0; n < str.Length; n++)
                                    {
                                        if (System.IO.File.Exists(Server.MapPath(@str[n])))
                                        {
                                            url += str[n] + "|";
                                        }
                                    }
                                }
                            }
                            break;
                    }
                }
            }
            catch
            {
                url = "";
            }
            return url;
        }
        [HttpPost]
        public ActionResult UploadImages()
        {
            try
            {
                string url = "";
                if (Request.Form["pic1"] != null)
                {
                    url = Request.Form["pic1"].ToString();
                }
                string path = "";
                HttpPostedFileBase fileData = null;
                for (int i = 0; i < Request.Files.Count; i++)
                {
                    fileData = Request.Files[i];
                    if (fileData != null)
                    {
                        string filePath = "";
                        string filePath1 = "";
                        if (!string.IsNullOrWhiteSpace(url))
                        {
                            if(url=="机票")
                            {
                                filePath = 上传管理.获取上传路径<订购合同上传记录>(媒体类型.图片, 路径类型.服务器本地路径);
                                filePath1 = 上传管理.获取上传路径<订购合同上传记录>(媒体类型.图片, 路径类型.不带域名根路径);
                            }
                            else
                            {
                                filePath = 上传管理.获取上传路径<商品>(媒体类型.图片, 路径类型.服务器本地路径);
                                filePath1 = 上传管理.获取上传路径<商品>(媒体类型.图片, 路径类型.不带域名根路径);
                            }
                        }
                        else
                        {
                            filePath = 上传管理.获取上传路径<商品>(媒体类型.图片, 路径类型.服务器本地路径);
                            filePath1 = 上传管理.获取上传路径<商品>(媒体类型.图片, 路径类型.不带域名根路径);
                        }
                        //string filePath1 = Server.MapPath(ConfigurationManager.AppSettings["上传文件根路径"]);
                        //获取文件后缀名
                        string extension = fileData.FileName.Substring(fileData.FileName.LastIndexOf("."), (fileData.FileName.Length - fileData.FileName.LastIndexOf(".")));
                        //按年月生成文件夹
                        //string folderName = StrHelp.MakeFolderName();

                        //类型判断
                        if ((fileData.ContentType == "image/jpeg" || fileData.ContentType == "image/pjpeg" || fileData.ContentType == "image/x-png" || fileData.ContentType == "image/png") && ((fileData.ContentLength / 1024) / 1024) < 2)
                        {
                            //生成新文件名
                            //string fileName = StrHelp.MakeFileRndName() + extension.ToLower();
                            string fileName = 上传管理.获取文件名(fileData.FileName).Replace("{", "").Replace("}", "");
                            if (fileName.LastIndexOf("\\") > -1)
                            {
                                fileName = fileName.Substring(fileName.LastIndexOf("\\") + 1);
                            }
                            string originalpath = string.Format("{0}original", filePath);//原图路径
                            if (!Directory.Exists(originalpath))
                            {
                                Directory.CreateDirectory(originalpath);
                            }
                            //保存原图
                            fileData.SaveAs(string.Format("{0}\\{1}", originalpath, fileName));

                            //缩略图350*350
                            UploadPic.MakeThumbnail(string.Format("{0}\\{1}", originalpath, fileName), string.Format("{0}350X350\\{1}", filePath, fileName), 350, 350, "Cut");

                            //缩略图150*150
                            UploadPic.MakeThumbnail(string.Format("{0}\\{1}", originalpath, fileName), string.Format("{0}150X150\\{1}", filePath, fileName), 150, 150, "Cut");

                            //缩略图50*50
                            UploadPic.MakeThumbnail(string.Format("{0}\\{1}", originalpath, fileName), string.Format("{0}50X50\\{1}", filePath, fileName), 50, 50, "Cut");
                            //下面这句代码缺少的话，上传成功后上传队列的显示不会自动消失
                            path += filePath1 + "original/" + fileName + "|";
                            //缩略图550(限宽)
                            //UploadPic.MakeThumbnail(string.Format("{0}original\\{1}", filePath, fileName), string.Format("{0}550\\{1}", filePath, fileName), 550, 0, "W");
                        }
                        else
                        {
                            path = "出错|文件格式或大小不对！";
                        }
                    }
                }
                ViewData["path"] = path;
            }
            catch
            {
                ViewData["path"] = "出错|";
            }
            return View();
        }

        /// <summary>
        /// 上传验收单
        /// </summary>
        /// <param name="fileData"></param>
        /// <returns></returns>
        [HttpPost]
        public string UploadImages_Ysd(HttpPostedFileBase fileData)
        {
            if (fileData != null)
            {
                string fileName = fileData.FileName;
                if (fileName != "" && fileName != null)
                {
                    string filePath = 上传管理.获取上传路径<验收单>(媒体类型.附件, 路径类型.服务器本地路径);

                    //extension = fileData.FileName.Substring(fileData.FileName.LastIndexOf("."), (fileData.FileName.Length - fileData.FileName.LastIndexOf(".")));
                    if (fileName.LastIndexOf("\\") > -1)
                    {
                        fileName = fileName.Substring(fileName.LastIndexOf("\\") + 1);
                    }
                    fileName = 上传管理.获取文件名(fileName).Replace("{", "").Replace("}", "");
                    if (!Directory.Exists(filePath))
                    {
                        Directory.CreateDirectory(filePath);
                    }
                    fileData.SaveAs(string.Format("{0}\\{1}", filePath, fileName));
                }
                return fileName;
            }
            else
            {
                return null;
            }
        }
        public int DeleteAttach_ysd()
        {
            string strname = Request.QueryString["n"].ToString();
            try
            {
                if (System.IO.File.Exists(Server.MapPath(@strname)))
                {
                    System.IO.File.Delete(Server.MapPath(@strname));
                }
                return 1;
            }
            catch { return 0; }
        }
        public int DeleteImages()
        {
            long id = 0;
            if (!string.IsNullOrWhiteSpace(Request.QueryString["gid"]))
            {
                id = long.Parse(Request.QueryString["gid"]);
            }
            string strname = Request.QueryString["n"].ToString();
            int start = strname.LastIndexOf('/') + 1;
            int length = strname.Length - start;
            string name = strname.Substring(start, length);
            try
            {
                商品 model = 商品管理.查找商品(id);
                if (model != null)
                {
                    for (int i = 0; i < model.商品信息.商品图片.Count(); i++)
                    {
                        if (model.商品信息.商品图片[i] == strname)
                        {
                            model.商品信息.商品图片.Remove(strname);
                            break;
                        }
                    }
                    商品管理.更新商品(model);
                }
                if (!string.IsNullOrEmpty(name))
                {
                    System.IO.File.Delete(Server.MapPath(@strname));
                    System.IO.File.Delete(Server.MapPath(@strname.Replace("original", "350X350")));
                    System.IO.File.Delete(Server.MapPath(@strname.Replace("original", "150X150")));
                    System.IO.File.Delete(Server.MapPath(@strname.Replace("original", "50X50")));
                    return 1;
                }
                else
                {
                    return 0;
                }
            }
            catch { return 0; }
        }
        public ActionResult ProductClass()
        {
            //专用物资
            string sp = Request.QueryString["sp"];
            //firt/second
            string type = Request.QueryString["type"];

            //是否多选
            var isdx = Request.Params["isdx"];
            //选择的ID
            string requesrid = Request.Params["classid"];
            //选择的分类名
            string requesrname = Request.Params["classname"];
            try
            {
                string retstr = "";
                if (type == "first")
                {
                    供应商 NewModel = 用户管理.查找用户<供应商>(currentUser.Id);
                    //所有二级分类
                    var 当前二级分类 = 商品分类管理.查找子分类(long.Parse(requesrid));
                    foreach (var m in NewModel.可提供产品类别列表)
                    {
                        if (m.一级分类 == requesrname && m.二级分类.Any())
                        {
                            retstr += "<ul>";
                            foreach (var 二级分类 in 当前二级分类)
                            {
                                if (m.二级分类.Contains(二级分类.分类名))
                                {
                                    retstr += "<li id='" + 二级分类.Id + "' lang='second' onclick='GetSonClass(this);'>" + 二级分类.分类名 + "</li>";
                                }
                            }
                            retstr += "</ul>";

                        }
                    }
                }
                else if (type == "second")
                {
                    if (string.IsNullOrWhiteSpace(sp))
                    {
                        var lowerclass = 商品分类管理.查找子分类(long.Parse(requesrid));
                        if (lowerclass.Any())
                        {
                            retstr += "<ul>";
                            foreach (var item in lowerclass)
                            {
                                retstr += "<li id='" + item.Id + "' onclick='GetSonClass(this);'>" + item.分类名 + "</li>";
                            }
                            retstr += "</ul>";
                            //context.Response.Write("<div class='gys_ztbsearch_class'>选择类别：</div><div class='gys_ztbsearch_class_list'> <a href='#'>全部</a> <a href='#'>资质1</a> <a href='#'>资质1</a> <a href='#'>资质1</a> <a href='#'>资质1</a> <a href='#'>资质1</a> <a href='#'>资质1</a> </div>");
                        }
                        else
                        {
                            if (isdx == "true")
                            {
                                retstr = "<font onclick='Delme(this)' color='red' id='" + requesrid + "'>" + requesrname + " <s class='gys_ztbsearch_classboxspan'></s></font>";
                            }
                            else
                            {
                                retstr = "<span style=' color: #006400; padding: 10px 0 10px 0; font-size: 14px; font-weight: bold;'>选择的商品类别是<font color='red'>" + requesrname + "</font></span>";
                            }
                        }
                    }
                    else
                    {
                        if (isdx == "true")
                        {
                            retstr = "<font onclick='Delme(this)' color='red' id='" + requesrid + "'>" + requesrname + " <s class='gys_ztbsearch_classboxspan'></s></font>";
                        }
                        else
                        {
                            retstr = "<span style=' color: #006400; padding: 10px 0 10px 0; font-size: 14px; font-weight: bold;'>选择的商品类别是<font color='red'>" + requesrname + "</font></span>";
                        }
                    }
                }
                else
                {
                    if (isdx == "true")
                    {
                        retstr = "<font onclick='Delme(this)' color='red' id='" + requesrid + "'>" + requesrname + " <s class='gys_ztbsearch_classboxspan'></s></font>";
                    }
                    else
                    {
                        retstr = "<span style=' color: #006400; padding: 10px 0 10px 0; font-size: 14px; font-weight: bold;'>选择的商品类别是<font color='red'>" + requesrname + "</font></span>";
                    }
                }
                return Content(retstr);

            }
            catch
            {
                return Content("加载失败");
            }
        }

        public string GetClass()
        {
            var id = Request.Params["id"];
            var ap = Request.Params["ap"];
            IEnumerable<商品分类> classify = null;
            if (id != "101")    //不为专用物资
            {
                classify = 商品分类管理.查找子分类(long.Parse(id));
            }
            else
            {
                classify = 商品分类管理.查找子分类().Where(o => o.分类性质 == 商品分类性质.专用物资);
            }
            var str = "<ul>";
            foreach (var k in classify)
            {
                if (ap == "first")
                {
                    str += "<li id=" + k.Id + " lang='second' onclick='GetClass(this);'>" + k.分类名 + "</li>";
                }
                if (ap == "second")
                {
                    str += "<li id=" + k.Id + " lang='third' onclick='GetClass(this);'>" + k.分类名 + "</li>";
                }
               
            }
            str += "</ul>";
            return str; 
        }

        public class FindUsers
        {
            public List<string> name { get; set; }
            public List<string> loginName { get; set; }
            public long count { get; set; }
        }

        public ActionResult Find_Users()
        {
            string username = Request.QueryString["user"].ToString();
            string pagesize = Request.QueryString["p"].ToString();
            string catorgray = Request.QueryString["cator"].ToString();
            string province = Request.QueryString["prov"].ToString();
            string city = Request.QueryString["cit"].ToString();
            string area = Request.QueryString["ar"].ToString();
            string industry = Request.QueryString["indus"].ToString();
            long list_count = 0;
            List<string> allName = new List<string>();
            List<string> LoginName = new List<string>();
            IMongoQuery q = null;
            if (!string.IsNullOrEmpty(province))
            {
                q = q.And(Query.EQ("所属地域.省份", province));
                if (city != "不限" && !string.IsNullOrEmpty(city) && !string.IsNullOrEmpty(area) && area != "不限")
                {
                    q = q.And(Query.EQ("所属地域.城市", city));
                    q = q.And(Query.EQ("所属地域.区县", area));
                }

            }
            if (catorgray == "单位用户")
            {
                if (!string.IsNullOrWhiteSpace(username))
                {
                    q = q.And(Query.EQ("单位信息.单位代号", new BsonRegularExpression(string.Format("/{0}/i", username))));
                    q = q.And(Query.EQ("单位信息.单位名称", new BsonRegularExpression(string.Format("/{0}/i", username))));
                }
                long sum = 用户管理.计数用户<单位用户>(0, 0, q);
                list_count = sum / 12;
                long list_maintain = sum % 12;
                if (list_maintain > 0)
                {
                    list_count++;
                }
                if (string.IsNullOrEmpty(pagesize))
                {
                    pagesize = "1";
                }
                IEnumerable<单位用户> Department = 用户管理.查询用户<单位用户>((int.Parse(pagesize) - 1) * 12, 12, q);
                if (Department != null)
                {
                    for (int i = 0; i < Department.Count(); i++)
                    {
                        if (string.IsNullOrWhiteSpace(Department.ElementAt(i).单位信息.单位代号))
                        {
                            allName.Add(Department.ElementAt(i).单位信息.单位名称);
                        }
                        else
                        {
                            allName.Add(Department.ElementAt(i).单位信息.单位代号);
                        }
                        LoginName.Add(Department.ElementAt(i).登录信息.登录名);
                    }
                }
            }
            else if (catorgray == "运营团队")
            {
                if (!string.IsNullOrWhiteSpace(username))
                {
                    q = q.And(Query.EQ("运营团队工作人员信息.姓名", new BsonRegularExpression(string.Format("/{0}/i", username))));
                }
                long sum = 用户管理.计数用户<运营团队>(0, 0, q);
                list_count = sum / 12;
                long list_maintain = sum % 12;
                if (list_maintain > 0)
                {
                    list_count++;
                }
                if (string.IsNullOrEmpty(pagesize))
                {
                    pagesize = "1";
                }
                IEnumerable<运营团队> Department = 用户管理.查询用户<运营团队>((int.Parse(pagesize) - 1) * 12, 12, q);
                if (Department != null)
                {
                    for (int i = 0; i < Department.Count(); i++)
                    {
                        if (string.IsNullOrWhiteSpace(Department.ElementAt(i).运营团队工作人员信息.姓名))
                        {
                            allName.Add("匿名用户");
                        }
                        else
                        {
                            allName.Add(Department.ElementAt(i).运营团队工作人员信息.姓名);
                        }
                        LoginName.Add(Department.ElementAt(i).登录信息.登录名);
                    }
                }
            }
            else if (catorgray == "供应商")
            {
                供应商 Newmodel = 用户管理.查找用户<供应商>(currentUser.Id);
                if (!string.IsNullOrEmpty(industry))
                {
                    q = q.And(Query.EQ("企业基本信息.所属行业", industry));
                }
                if (!string.IsNullOrWhiteSpace(username))
                {
                    q = q.And(Query.EQ("企业基本信息.企业名称", new BsonRegularExpression(string.Format("/{0}/i", username))));
                }
                long sum = 用户管理.计数用户<供应商>(0, 0, q);
                list_count = (sum--) / 12;
                long list_maintain = sum % 12;
                if (list_maintain > 0)
                {
                    list_count++;
                }
                if (string.IsNullOrEmpty(pagesize))
                {
                    pagesize = "1";
                }
                IEnumerable<供应商> Department = 用户管理.查询用户<供应商>((int.Parse(pagesize) - 1) * 12, 12, q);
                if (Department != null)
                {
                    for (int i = 0; i < Department.Count(); i++)
                    {
                        if (Newmodel.企业基本信息.企业名称 != Department.ElementAt(i).企业基本信息.企业名称)
                        {
                            if (string.IsNullOrWhiteSpace(Department.ElementAt(i).企业基本信息.企业名称))
                            {
                                allName.Add("暂无名称");
                            }
                            else
                            {
                                allName.Add(Department.ElementAt(i).企业基本信息.企业名称);
                            }
                            LoginName.Add(Department.ElementAt(i).登录信息.登录名);
                        }
                    }
                }
            }
            JsonResult json = new JsonResult() { Data = new FindUsers() { name = allName, count = list_count, loginName = LoginName } };
            return Json(json, JsonRequestBehavior.AllowGet);

        }
        public ActionResult Part_Msg_Detail()
        {
            try
            {
                long id = long.Parse(Request.QueryString["id"].ToString());
                站内消息管理.更新已读时间(id, true, DateTime.Now);
                站内消息 model = 站内消息管理.查找站内消息(id);
                if (model != null)
                {
                    ViewData["当前用户"] = currentUser.登录信息.登录名;
                    return PartialView("Gys_Part/Part_Msg_Detail", model);
                }
                else
                {
                    return Content("<script>window.location='/供应商后台/Msg_Sent';</script>");
                }
            }
            catch
            {
                return Content("<script>window.location='/供应商后台/Msg_Sent';</script>");
            }


        }
        public ActionResult Msg_Detail()
        {
            return View();
        }

        public ActionResult HotelAdd()
        {
            return View();
        }
        public ActionResult Roomlist()
        {
            return View();
        }
        public ActionResult RoomEdit()
        {
            int index = int.Parse(Request.Params["index"]);
            IEnumerable<酒店> hotel = 酒店管理.查询酒店(0, 0, Query.EQ("所属供应商.用户ID", currentUser.Id));
            Room room = new Room();
            foreach (var k in hotel)
            {
                room.Id = index;
                room.房型 = k.房间信息[index].房型;
                room.床型 = k.房间信息[index].床型;
                room.价格 = k.房间信息[index].价格;
                room.简介 = k.房间信息[index].简介;
                room.早餐 = k.房间信息[index].早餐;
                room.图片 = k.房间信息[index].图片;
                room.房间设施.吹风机 = k.房间信息[index].房间设施.吹风机;
                room.房间设施.二十四小时热水 = k.房间信息[index].房间设施.二十四小时热水;
                room.房间设施.国际长途通话 = k.房间信息[index].房间设施.国际长途通话;
                room.房间设施.空调 = k.房间信息[index].房间设施.空调;
                room.房间设施.宽带上网 = k.房间信息[index].房间设施.宽带上网;
                room.房间设施.免费国内长途通话 = k.房间信息[index].房间设施.免费国内长途通话;
                room.房间设施.免费市内电话 = k.房间信息[index].房间设施.免费市内电话;
                room.房间设施.暖气 = k.房间信息[index].房间设施.暖气;
            }
            //if (!string.IsNullOrEmpty(Request.Form["attachtext"]))
            //{
            //    var a = Request.Form["attachtext"];
            //    var f = a.Substring(0, a.Length - 1).Split('|');
            //    foreach (var k in f)
            //    {
            //        room.图片.Add(k);
            //    }
            //}
            return View(room);
        }
        public ActionResult Part_Hotellist()
        {
            IEnumerable<酒店> hotel = 酒店管理.查询酒店(0, 0, Query.EQ("所属供应商.用户ID", currentUser.Id));
            酒店 g = new 酒店();
            foreach (var k in hotel)
            {
                g = k;
            }
            if (g.所属供应商.用户数据 == null)
            {
                return Content("<script>alert('请先添加酒店信息!');window.location.href='/供应商后台/HotelEdit'</script>");
            }
            long listcount = g.房间信息.Count;
            long maxpagesize = Math.Max((listcount + 15 - 1) / 15, 1);//15，每页显示15条记录

            int page = 1;
            ViewData["currentpage"] = page;
            ViewData["pagecount"] = maxpagesize;
            ViewData["房间列表"] = g;

            return PartialView("Gys_Part/Part_Hotellist");
        }
        public ActionResult Part_SearchByPage()
        {
            int page = 1;
            int.TryParse(Request.Params["page"], out page);

            IEnumerable<酒店> hotel = 酒店管理.查询酒店(0, 0, Query.EQ("所属供应商.用户ID", currentUser.Id));
            酒店 g = new 酒店();
            foreach (var k in hotel)
            {
                g = k;
            }

            int listcount = g.房间信息.Count;
            int maxpage = Math.Max((listcount + 15 - 1) / 15, 1);

            ViewData["currentpage"] = page;
            ViewData["pagecount"] = maxpage;

            ViewData["房间列表"] = g;
            return PartialView("Gys_Part/Part_SearchByPage");
        }
        public ActionResult HotelEdit()
        {
            IEnumerable<酒店> hotel = 酒店管理.查询酒店(0, 0, Query.EQ("所属供应商.用户ID", currentUser.Id));
            酒店 h = new 酒店();
            foreach (var k in hotel)
            {
                h = k;
            }
            return View(h);
        }
        public ActionResult HotelDetial()
        {
            ViewData["h"] = 酒店管理.查询酒店(0, 0, Query.EQ("所属供应商.用户ID", currentUser.Id));
            return View();
        }
        public ActionResult TicketAdd()
        {
            机票代售点 model = new 机票代售点();
            return View(model);
        }
        [HttpPost]
        public ActionResult AddTicket(机票代售点 model)
        {
            string point = Request.Form["point"].ToString();
            double[] pt = new double[2];
            if (!string.IsNullOrEmpty(Request.Form["picture"]))
            {
                var a = Request.Form["picture"];
                var f = a.Substring(0, a.Length - 1).Split('|');
                foreach (var k in f)
                {
                    model.照片.Add(k);
                }
            }
            if (string.IsNullOrWhiteSpace(point.Split(',')[0]))
            {
                pt[0] = 0;
                pt[1] = 0;
            }
            else
            {
                pt[0] = double.Parse(point.Split(',')[0]);
                pt[1] = double.Parse(point.Split(',')[1]);
            }
            model.地理位置 = pt;
            model.所属供应商.用户ID = currentUser.Id;
            model.审核数据.审核状态 = 审核状态.未审核;
            机票代售点管理.添加机票代售点(model);
            return Content("<script>alert('成功添加机票代售点');window.location='/供应商后台/TicketAdd';</script>");
        }
        public ActionResult Modify_Ticket()
        {
            try
            {
                long id = long.Parse(Request.QueryString["id"]);
                机票代售点 model = 机票代售点管理.查找机票代售点(id);
                return View(model);
            }
            catch
            {
                return Redirect("/供应商后台/Ticketlist");
            }
        }
        [HttpPost]
        public ActionResult ModifyTicket(机票代售点 model)
        {
            try
            {
                List<string> picture = new List<string>();
                double[] p = new double[2];
                string point = Request.Form["point"].ToString();
                if (string.IsNullOrWhiteSpace(point.Split(',')[0]) || string.IsNullOrWhiteSpace(point.Split(',')[1]))
                {
                    p = 机票代售点管理.查找机票代售点(model.Id).地理位置;
                }
                else
                {
                    p[0] = double.Parse(point.Split(',')[0]);
                    p[1] = double.Parse(point.Split(',')[1]);
                }
                if (!string.IsNullOrEmpty(Request.Form["picture"]))
                {
                    var a = Request.Form["picture"];
                    var f = a.Substring(0, a.Length - 1).Split('|');
                    foreach (var k in f)
                    {
                        picture.Add(k);
                    }
                }
                model.照片 = picture;
                model.地理位置 = p;
                model.审核数据.审核状态 = 审核状态.未审核;
                if (机票代售点管理.更新机票代售点(model))
                {
                    return Content("<script>alert('修改成功！');window.location='/供应商后台/Ticketlist';</script>");
                }
                else
                {
                    return Content("<script>alert('修改失败！');window.location='/供应商后台/Ticketlist';</script>");
                }
            }
            catch
            {
                return Content("<script>alert('修改失败！');window.location='/供应商后台/Ticketlist';</script>");
            }
        }
        public ActionResult DelTicketPic()
        {
            try
            {
                long id = long.Parse(Request.QueryString["id"]);
                int index = int.Parse(Request.QueryString["index"]);
                机票代售点 model = 机票代售点管理.查找机票代售点(id);
                if (System.IO.File.Exists(Server.MapPath(@model.照片.ElementAt(index))))
                {
                    System.IO.File.Delete(Server.MapPath(@model.照片.ElementAt(index)));
                    model.照片.RemoveAt(index);
                    机票代售点管理.更新机票代售点(model);
                }
                return Redirect("/供应商后台/Modify_Ticket?id=" + id.ToString());
            }
            catch
            {
                return Redirect("/供应商后台/Ticketlist");
            }
        }
        public ActionResult Ticket_Detail()
        {
            long id = long.Parse(Request.QueryString["id"]);
            机票代售点 model = 机票代售点管理.查找机票代售点(id);
            return View(model);
        }
        public ActionResult delete_ticket()
        {
            try
            {
                long id = long.Parse(Request.QueryString["id"]);
                机票代售点 model = 机票代售点管理.查找机票代售点(id);
                if (model.照片 != null && model.照片.Count() != 0)
                {
                    foreach (var item in model.照片)
                    {
                        if (System.IO.File.Exists(Server.MapPath(@item)))
                        {
                            System.IO.File.Delete(Server.MapPath(@item));
                        }
                    }
                }
                机票代售点管理.删除机票代售点(id);
                return Redirect("/供应商后台/Ticketlist");
            }
            catch
            {
                return Redirect("/供应商后台/Ticketlist");
            }
        }
        public ActionResult Ticketlist()
        {
            try
            {
                IEnumerable<机票代售点> model = 机票代售点管理.查询机票代售点(0, 0, Query.EQ("所属供应商.用户ID", currentUser.Id)).Skip(0).Take(10);
                return View(model);
            }
            catch
            {
                return Redirect("/供应商后台/TicketAdd");
            }
        }
        public class Tickets
        {
            public long id { get; set; }
            public string name { get; set; }
            public string address { get; set; }
            public string sq { get; set; }
            public string Istrue { get; set; }
        }
        public ActionResult SearchTicket()
        {
            List<Tickets> tks = new List<Tickets>();
            int page = int.Parse(Request.QueryString["p"]);
            IEnumerable<机票代售点> model = 机票代售点管理.查询机票代售点(0, 0, Query.EQ("所属供应商.用户ID", currentUser.Id)).Skip(10 * (page - 1)).Take(10);
            long pager = 机票代售点管理.查询机票代售点(0, 0, Query.EQ("所属供应商.用户ID", currentUser.Id)).Count() / 10;
            if ((机票代售点管理.查询机票代售点(0, 0, Query.EQ("所属供应商.用户ID", currentUser.Id)).Count() % 10) > 0)
            {
                pager++;
            }
            if (model != null)
            {
                foreach (var item in model)
                {
                    if (item.审核数据.审核状态 == 审核状态.审核通过)
                    {
                        Tickets tk = new Tickets() { id = item.Id, name = item.代售点名称, address = item.地址, sq = item.所属地域.省份 + item.所属地域.城市 + item.所属地域.区县, Istrue = "通过审核" };
                        tks.Add(tk);
                    }
                    else
                    {
                        Tickets tk = new Tickets() { id = item.Id, name = item.代售点名称, address = item.地址, sq = item.所属地域.省份 + item.所属地域.城市 + item.所属地域.区县, Istrue = "未审核" };
                        tks.Add(tk);
                    }
                }
            }
            JsonResult json = new JsonResult() { Data = new { dt = tks, p = pager } };
            return Json(json, JsonRequestBehavior.AllowGet);
        }
        public class Room
        {
            public long Id { get; set; }//酒店Id
            [Required(ErrorMessage = "请填写房型")]
            public string 房型 { get; set; }
            [Required(ErrorMessage = "请填写价格")]
            public string 价格 { get; set; }
            [Required(ErrorMessage = "请填写床型")]
            public string 床型 { get; set; }
            public string 早餐 { get; set; }
            public string 简介 { get; set; }
            public List<string> 图片 { get; set; }
            public _房间设施1 房间设施 { get; set; }
            public Room()
            {
                this.图片 = new List<string>();
                this.房间设施 = new _房间设施1();
            }
        }
        public class _房间设施1
        {
            public bool 宽带上网 { get; set; }
            public bool 免费市内电话 { get; set; }
            public bool 空调 { get; set; }
            public bool 国际长途通话 { get; set; }
            public bool 免费国内长途通话 { get; set; }
            public bool 吹风机 { get; set; }
            public bool 暖气 { get; set; }
            public bool 二十四小时热水 { get; set; }
        }
        public ActionResult AddRoom(Room h)
        {
            IEnumerable<酒店> hotel = 酒店管理.查询酒店(0, 0, Query.EQ("所属供应商.用户ID", currentUser.Id));
            酒店 ht = new 酒店();
            foreach (var k in hotel)
            {
                ht = k;
            }
            if (!string.IsNullOrEmpty(Request.Form["attachtext"]))
            {
                var a = Request.Form["attachtext"];
                var f = a.Substring(0, a.Length - 1).Split('|');
                foreach (var k in f)
                {
                    h.图片.Add(k);
                }
            }
            酒店._房间信息 m = new 酒店._房间信息();
            m.房型 = h.房型;
            m.床型 = h.床型;
            m.房间设施.吹风机 = h.房间设施.吹风机;
            m.房间设施.二十四小时热水 = h.房间设施.二十四小时热水;
            m.房间设施.国际长途通话 = h.房间设施.国际长途通话;
            m.房间设施.空调 = h.房间设施.空调;
            m.房间设施.宽带上网 = h.房间设施.宽带上网;
            m.房间设施.免费国内长途通话 = h.房间设施.免费国内长途通话;
            m.房间设施.免费市内电话 = h.房间设施.免费市内电话;
            m.房间设施.暖气 = h.房间设施.暖气;
            m.价格 = h.价格;
            m.简介 = h.简介;
            m.图片 = h.图片;
            m.早餐 = h.早餐;
            ht.房间信息.Add(m);
            酒店管理.更新酒店(ht);
            return RedirectToAction("Roomlist");
        }
        public ActionResult EditRoom(Room ht)
        {
            int index = (int)ht.Id;
            string roompic = Request.Form["attachtext"];
            IEnumerable<酒店> h = 酒店管理.查询酒店(0, 0, Query.EQ("所属供应商.用户ID", currentUser.Id));
            酒店 hotel = new 酒店();
            foreach (var p in h)
            {
                hotel = p;
            }
            if (!string.IsNullOrEmpty(roompic))
            {
                var f = roompic.Substring(0, roompic.Length - 1).Split('|');
                foreach (var k in f)
                {
                    hotel.房间信息[index].图片.Add(k);
                }
            }
            hotel.房间信息[index].房型 = ht.房型;
            hotel.房间信息[index].价格 = ht.价格;
            hotel.房间信息[index].床型 = ht.床型;
            hotel.房间信息[index].早餐 = ht.早餐;
            hotel.房间信息[index].简介 = ht.简介;
            hotel.房间信息[index].房间设施.吹风机 = ht.房间设施.吹风机;
            hotel.房间信息[index].房间设施.二十四小时热水 = ht.房间设施.二十四小时热水;
            hotel.房间信息[index].房间设施.国际长途通话 = ht.房间设施.国际长途通话;
            hotel.房间信息[index].房间设施.空调 = ht.房间设施.空调;
            hotel.房间信息[index].房间设施.宽带上网 = ht.房间设施.宽带上网;
            hotel.房间信息[index].房间设施.免费国内长途通话 = ht.房间设施.免费国内长途通话;
            hotel.房间信息[index].房间设施.免费市内电话 = ht.房间设施.免费市内电话;
            hotel.房间信息[index].房间设施.暖气 = ht.房间设施.暖气;
            酒店管理.更新酒店(hotel);
            return RedirectToAction("Roomlist");
        }
        public ActionResult DelRoom()
        {
            IEnumerable<酒店> k = 酒店管理.查询酒店(0, 0, Query.EQ("所属供应商.用户ID", currentUser.Id));
            酒店 h = new 酒店();
            foreach (var p in k)
            {
                h = p;
            }
            string index = Request.Params["index"];
            h.房间信息.RemoveAt(int.Parse(index));
            酒店管理.更新酒店(h);
            return RedirectToAction("Roomlist");
        }
        public void DelRoomImg()
        {
            string v = Request.Params["index"];
            var g = v.Split('|');
            IEnumerable<酒店> hotel = 酒店管理.查询酒店(0, 0, Query.EQ("所属供应商.用户ID", currentUser.Id));
            酒店 h = new 酒店();
            foreach (var p in hotel)
            {
                h = p;
            }
            h.房间信息[int.Parse(g[0])].图片.RemoveAt(int.Parse(g[1]));
            酒店管理.更新酒店(h);
        }
        /// <summary>
        /// 添加酒店
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult AddHotel(酒店 hotel)
        {
            if (!string.IsNullOrEmpty(Request.Form["attachtext"]))
            {
                var a = Request.Form["attachtext"];
                var f = a.Substring(0, a.Length - 1).Split('|');
                foreach (var k in f)
                {
                    hotel.酒店基本信息.照片.Add(k);
                }
            }
            if (!string.IsNullOrEmpty(Request.Form["localtion"]))
            {
                var b = Request.Form["localtion"];
                var c = b.Split(',');
                hotel.酒店基本信息.地理位置 = new double[] { double.Parse(c[0]), double.Parse(c[1]) };
            }
            hotel.所属供应商.用户ID = currentUser.Id;
            hotel.审核数据.审核状态 = 审核状态.未审核;
            酒店管理.添加酒店(hotel);
            return RedirectToAction("Hotellist");
        }
        public ActionResult EditHotel(酒店 ht)
        {
            IEnumerable<酒店> hotel = 酒店管理.查询酒店(0, 0, Query.EQ("所属供应商.用户ID", currentUser.Id));
            酒店 h = new 酒店();
            if (hotel != null && hotel.Any())
            {
                foreach (var k in hotel)
                {
                    h = k;
                }
            }
            if (!string.IsNullOrEmpty(Request.Form["attachtext"]))
            {
                var a = Request.Form["attachtext"];
                var f = a.Substring(0, a.Length - 1).Split('|');
                foreach (var k in f)
                {
                    h.酒店基本信息.照片.Add(k);
                }
            }
            if (!string.IsNullOrEmpty(Request.Form["localtion"]))
            {
                var b = Request.Form["localtion"];
                var c = b.Split(',');
                h.酒店基本信息.地理位置 = new double[] { double.Parse(c[0]), double.Parse(c[1]) };
            }
            h.酒店基本信息.Wifi = ht.酒店基本信息.Wifi;
            h.酒店基本信息.地址 = ht.酒店基本信息.地址;
            h.酒店基本信息.简介 = ht.酒店基本信息.简介;
            h.酒店基本信息.交通信息 = ht.酒店基本信息.交通信息;
            h.酒店基本信息.酒店名 = ht.酒店基本信息.酒店名;
            h.酒店基本信息.联系电话 = ht.酒店基本信息.联系电话;
            h.酒店基本信息.所属地域 = ht.酒店基本信息.所属地域;
            h.酒店基本信息.所属商圈 = ht.酒店基本信息.所属商圈;
            h.酒店基本信息.免费停车场 = ht.酒店基本信息.免费停车场;
            h.酒店基本信息.入住和离店时间 = ht.酒店基本信息.入住和离店时间;
            h.酒店设施 = ht.酒店设施;
            h.酒店服务 = ht.酒店服务;
            h.审核数据.审核状态 = 审核状态.未审核;
            if (hotel != null && hotel.Any())
            {
                酒店管理.更新酒店(h);
            }
            else
            {
                h.所属供应商.用户ID = currentUser.Id;
                酒店管理.添加酒店(h);
            }
            return RedirectToAction("HotelEdit");
        }
        public ActionResult DelHotel()
        {
            string id = Request.Params["id"];
            酒店管理.删除酒店(long.Parse(id));
            return RedirectToAction("Hotellist");
        }
        public void DelHotelImg()
        {
            string index = Request.Params["index"];
            string id = Request.Params["id"];
            酒店 h = 酒店管理.查找酒店(long.Parse(id));
            if (System.IO.File.Exists(Server.MapPath(h.酒店基本信息.照片.ElementAt(int.Parse(index)))))
            {
                System.IO.File.Delete(Server.MapPath(h.酒店基本信息.照片.ElementAt(int.Parse(index))));
                h.酒店基本信息.照片.RemoveAt(int.Parse(index));
                酒店管理.更新酒店(h);
            }
            //JsonResult json = new JsonResult() { Data = h.照片 };
            //return Json(json, JsonRequestBehavior.AllowGet);
        }
        public ActionResult RoomDetial()
        {
            string index = Request.Params["index"];
            IEnumerable<酒店> k = 酒店管理.查询酒店(0, 0, Query.EQ("所属供应商.用户ID", currentUser.Id));
            foreach (var p in k)
            {
                ViewData["房间信息"] = p.房间信息[int.Parse(index)];
            }
            return View();
        }
        ///////////////////////////////////////////////////////////////////////////////下载标书代码
        public ActionResult SignUp_BiaoShuDownload()
        {
            return View();
        }

        public ActionResult Part_SignUp_BiaoShuDownload(long? id)
        {
            招标采购预报名 model = null;
            try
            {
                model = 招标采购预报名管理.查找招标采购预报名((long)id);
                var gys = 用户管理.查找用户<供应商>(currentUser.Id);
                var flage = true;

                if (gys.供应商用户信息.U盾信息 != null && !string.IsNullOrWhiteSpace(gys.供应商用户信息.U盾信息.序列号) && !string.IsNullOrWhiteSpace(gys.供应商用户信息.U盾信息.加密参数) && gys.供应商用户信息.U盾信息.年检结束时间 > DateTime.Now)
                {
                    ViewData["已购买U盾"] = "1";
                }
                else
                {
                    flage = false;
                    ViewData["已购买U盾"] = "0";
                }

                var signupmessage = model.预报名供应商列表.Where(o => o.供应商链接.用户ID == currentUser.Id);
                if (signupmessage.Any() && signupmessage.First().审核数据.审核状态 == 审核状态.审核通过)
                {
                    ViewData["审核通过"] = "1";
                }
                else
                {
                    flage = false;
                    ViewData["审核通过"] = "0";
                }

                if (signupmessage.Any() && signupmessage.First().已购买标书 == true)
                {
                    ViewData["已购买标书"] = "1";
                }
                else
                {
                    flage = false;
                    ViewData["已购买标书"] = "0";
                }

                if (flage)
                {
                    Random randomGenerator = new Random(DateTime.Now.Millisecond);
                    String RndStr = "";
                    for (int i = 0; i < 32; i++)
                        RndStr += Convert.ToChar(randomGenerator.Next(97, 122));
                    ViewData["Message"] = RndStr;
                }

            }
            catch
            {
                model = null;
            }
            return PartialView("Gys_Part/Part_SignUp_BiaoShuDownload", model);
        }

        [HttpPost]
        public ActionResult CheckUOfBiaoShu(int? xxx)
        {
            var flage = false;
            //JS读取的U盾序列号
            string hidid = Request.Params["HidIAID"];// this.HidIAID.Value;
            //前台JS加密后的加密串
            string Digest = Request.Params["HidDigest"];// this.HidDigest.Value;

            var keyValue = currentUser.供应商用户信息.U盾信息.加密参数;
            var Sequencenumber = currentUser.供应商用户信息.U盾信息.序列号.Split(new char[] {'|'},
                StringSplitOptions.RemoveEmptyEntries);

            //判断JS读取的U盾序列号和数据库的序列号是否匹配
            if (!Sequencenumber.Contains(hidid))
            {
                ViewData["错误信息"] = "U盾信息不匹配！";
            }
            else
            {
                if (keyValue != "" && keyValue != null)
                {
                    //对前台的随机数和数据库的加密参数进行加密
                    string CDigest = Sha1(Request.Params["ssid"] + keyValue);
                    //两个加密串匹配则判断成功
                    if (Digest.Equals(CDigest))
                    {
                        var id = long.Parse(Request.Params["id"]);
                        var biaoshu = 招标采购预报名管理.查找招标采购预报名(id).标书信息;
                        if (biaoshu != null && biaoshu.内容.Any())
                        {
                            ViewData["标书信息"] = biaoshu;
                            flage = true;
                        }
                        else
                        {
                            ViewData["错误信息"] = "未获取到标书文件！";
                        }
                    }
                    else
                    {
                        ViewData["错误信息"] = "登陆失败";
                    }
                }
                else
                {
                    ViewData["错误信息"] = "此账号不存在";
                }
            }

            if (flage)
            {
                ViewData["检查状态"] = "1";
            }
            else
            {
                ViewData["检查状态"] = "0";
            }
            return PartialView("Gys_Part/SignUp_GysDownloadBiaoShu");
        }

        public static string Sha1(string str)
        {
            return System.Web.Security.FormsAuthentication.HashPasswordForStoringInConfigFile(str, "sha1").ToLower();
        }
        ///////////////////////////////////////////////////////////////////////////////下载标书代码




        /////////////////////////////////////////////////U盾测试程序
        //public ActionResult U_Index()
        //{
        //    Random randomGenerator = new Random(DateTime.Now.Millisecond);
        //    String RndStr = "";
        //    for (int i = 0; i < 32; i++)
        //        RndStr += Convert.ToChar(randomGenerator.Next(97, 122));
        //    ViewData["Message"] = RndStr;

        //    return View();
        //}

        //[HttpPost]
        //public ActionResult U_Index(int? xxx)
        //{
        //    var returnstr = "";


        //    string id = Request.Form["HidIAID"];// this.HidIAID.Value;
        //    string Digest = Request.Form["HidDigest"];// this.HidDigest.Value;
        //    string keyValue = currentUser.供应商用户信息.U盾信息.加密参数;
        //    if (keyValue != "" && keyValue != null)
        //    {
        //        string CDigest = Sha1(Request.Form["ssid"] + keyValue);
        //        if (Digest.Equals(CDigest))
        //        {
        //            returnstr = "登陆成功";
        //            //Alertto("登陆成功", "LoginOk.aspx");
        //        }
        //        else
        //        {
        //            returnstr = "登陆失败";
        //            //Alertto("登陆失败", "Login.aspx");
        //        }
        //    }
        //    else
        //    {
        //        returnstr = "此账号不存在";
        //        //Alertto("此账号不存在", "Login.aspx");
        //    }
        //    return Content(returnstr);
        //}

        //public static string Sha1(string str)
        //{
        //    return System.Web.Security.FormsAuthentication.HashPasswordForStoringInConfigFile(str, "sha1").ToLower();
        //}
        /////////////////////////////////////////////////U盾测试程序



        /// <summary>
        /// 金额大小写转换
        /// </summary>
        /// <param name="strAmount"></param>
        /// <returns></returns>
        public static string MoneyToUpper(string strAmount)
        {
            string functionReturnValue = null;
            bool IsNegative = false; // 是否是负数
            if (strAmount.Trim().Substring(0, 1) == "-")
            {
                // 是负数则先转为正数
                strAmount = strAmount.Trim().Remove(0, 1);
                IsNegative = true;
            }
            string strLower = null;
            string strUpart = null;
            string strUpper = null;
            int iTemp = 0;
            // 保留两位小数 123.489→123.49　　123.4→123.4
            strAmount = Math.Round(double.Parse(strAmount), 2).ToString();
            if (strAmount.IndexOf(".") > 0)
            {
                if (strAmount.IndexOf(".") == strAmount.Length - 2)
                {
                    strAmount = strAmount + "0";
                }
            }
            else
            {
                strAmount = strAmount + ".00";
            }
            strLower = strAmount;
            iTemp = 1;
            strUpper = "";
            while (iTemp <= strLower.Length)
            {
                switch (strLower.Substring(strLower.Length - iTemp, 1))
                {
                    case ".":
                        strUpart = "元";
                        break;
                    case "0":
                        strUpart = "零";
                        break;
                    case "1":
                        strUpart = "壹";
                        break;
                    case "2":
                        strUpart = "贰";
                        break;
                    case "3":
                        strUpart = "叁";
                        break;
                    case "4":
                        strUpart = "肆";
                        break;
                    case "5":
                        strUpart = "伍";
                        break;
                    case "6":
                        strUpart = "陆";
                        break;
                    case "7":
                        strUpart = "柒";
                        break;
                    case "8":
                        strUpart = "捌";
                        break;
                    case "9":
                        strUpart = "玖";
                        break;
                }

                switch (iTemp)
                {
                    case 1:
                        strUpart = strUpart + "分";
                        break;
                    case 2:
                        strUpart = strUpart + "角";
                        break;
                    case 3:
                        strUpart = strUpart + "";
                        break;
                    case 4:
                        strUpart = strUpart + "";
                        break;
                    case 5:
                        strUpart = strUpart + "拾";
                        break;
                    case 6:
                        strUpart = strUpart + "佰";
                        break;
                    case 7:
                        strUpart = strUpart + "仟";
                        break;
                    case 8:
                        strUpart = strUpart + "万";
                        break;
                    case 9:
                        strUpart = strUpart + "拾";
                        break;
                    case 10:
                        strUpart = strUpart + "佰";
                        break;
                    case 11:
                        strUpart = strUpart + "仟";
                        break;
                    case 12:
                        strUpart = strUpart + "亿";
                        break;
                    case 13:
                        strUpart = strUpart + "拾";
                        break;
                    case 14:
                        strUpart = strUpart + "佰";
                        break;
                    case 15:
                        strUpart = strUpart + "仟";
                        break;
                    case 16:
                        strUpart = strUpart + "万";
                        break;
                    default:
                        strUpart = strUpart + "";
                        break;
                }

                strUpper = strUpart + strUpper;
                iTemp = iTemp + 1;
            }
            //while (iTemp <= 11)//
            //{
            //    switch (iTemp)
            //    {
            //        case 1:
            //            strUpper = "\t  分" + strUpper;
            //            break;
            //        case 2:
            //            strUpper = "\t   角" + strUpper;
            //            break;
            //        case 3:
            //            strUpper = "\t" + strUpper;
            //            break;
            //        case 4:
            //            strUpper = "\t" + strUpper;
            //            break;
            //        case 5:
            //            strUpper = "\t   拾" + strUpper;
            //            break;
            //        case 6:
            //            strUpper = "\t   佰" + strUpper;
            //            break;
            //        case 7:
            //            strUpper = "\t   仟" + strUpper;
            //            break;
            //        case 8:
            //            strUpper = "\t   万" + strUpper;
            //            break;
            //        case 9:
            //            strUpper = "\t   拾" + strUpper;
            //            break;
            //        case 10:
            //            strUpper = "\t   佰" + strUpper;
            //            break;
            //        case 11:
            //            strUpper = "\t   仟" + strUpper;
            //            break;
            //        //case 12:
            //        //    strUpper = "亿" + strUpper;
            //        //    break;
            //        //case 13:
            //        //    strUpper = "拾" + strUpper;
            //        //    break;
            //        //case 14:
            //        //    strUpper = "佰" + strUpper;
            //        //    break;
            //        //case 15:
            //        //    strUpper = "仟" + strUpper;
            //        //    break;
            //        //case 16:
            //        //    strUpper = "万" + strUpper;
            //        //    break;
            //        default:
            //            strUpper = "" + strUpper;
            //            break;
            //    }
            //    ++iTemp;
            //}
            //strUpper = strUpper.Replace("零拾", "零");//数目为0时用零代替
            //strUpper = strUpper.Replace("零佰", "零");
            //strUpper = strUpper.Replace("零仟", "零");
            //strUpper = strUpper.Replace("零零零", "零");
            //strUpper = strUpper.Replace("零零", "零");
            //strUpper = strUpper.Replace("零角零分", "整");
            //strUpper = strUpper.Replace("零分", "整");
            //strUpper = strUpper.Replace("零角", "零");
            //strUpper = strUpper.Replace("零亿零万零元", "亿元圆");
            //strUpper = strUpper.Replace("亿零万零元", "亿元");
            //strUpper = strUpper.Replace("零亿零万", "亿");
            //strUpper = strUpper.Replace("零万零元", "万元");
            //strUpper = strUpper.Replace("零亿", "亿");
            //strUpper = strUpper.Replace("零万", "万");
            //strUpper = strUpper.Replace("零元", "元");
            //strUpper = strUpper.Replace("零零", "零");

            // 对壹圆以下的金额的处理
            if (strUpper.Substring(0, 1) == "元")
            {
                strUpper = strUpper.Substring(1, strUpper.Length - 1);
            }
            if (strUpper.Substring(0, 1) == "零")
            {
                strUpper = strUpper.Substring(1, strUpper.Length - 1);
            }
            if (strUpper.Substring(0, 1) == "角")
            {
                strUpper = strUpper.Substring(1, strUpper.Length - 1);
            }
            if (strUpper.Substring(0, 1) == "分")
            {
                strUpper = strUpper.Substring(1, strUpper.Length - 1);
            }
            //if (strUpper.Substring(0, 1) == "整")
            //{
            //    strUpper = "零元整";
            //}
            functionReturnValue = strUpper;

            if (IsNegative == true)
            {
                return "负" + functionReturnValue;
            }
            else
            {
                return functionReturnValue;
            }
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// 索引存放目录
        /// </summary>
        protected string IndexDic(string IndexPath)
        {
            return Server.MapPath(IndexPath);
        }

        /// <summary>
        /// 盘古分词器
        /// </summary>
        protected Analyzer PanGuAnalyzer
        {
            get { return new PanGuAnalyzer(); }
        }

        /// <summary>
        /// 盘古分词的配置文件
        /// </summary>
        protected string PanGuXmlPath
        {
            get
            {
                return Server.MapPath("/Lucene.Net/PanGu/PanGu.xml");
            }
        }
        protected void deleteIndex(string path, string id)
        {
            string Indexdic_Server = IndexDic(path);
            IndexWriter writer = null;
            Lucene.Net.Store.Directory dir = new Lucene.Net.Store.SimpleFSDirectory(new DirectoryInfo(Indexdic_Server));
            try
            {
                //writer = new IndexWriter(Indexdic_Server, PanGuAnalyzer, false, Lucene.Net.Index.IndexWriter.MaxFieldLength.LIMITED);
                writer = new IndexWriter(dir, PanGuAnalyzer, false, IndexWriter.MaxFieldLength.UNLIMITED);
            }
            catch
            {
                //writer = new IndexWriter(Indexdic_Server, PanGuAnalyzer, true, Lucene.Net.Index.IndexWriter.MaxFieldLength.LIMITED);
                writer = new IndexWriter(dir, PanGuAnalyzer, true, IndexWriter.MaxFieldLength.UNLIMITED);
            }
            writer.DeleteDocuments(new Term("NumId", id));//删除一条索引
            writer.Optimize();
            writer.Close();
        }
        /// <summary>
        /// 创建供应商索引
        /// </summary>
        /// <param name="model"></param>
        /// <param name="indexdic"></param>
        private void CreateIndex_gys(供应商 model, string indexdic)
        {
            string Indexdic_Server = IndexDic(indexdic);

            PanGu.Segment.Init(PanGuXmlPath);
            //创建索引目录
            if (!Directory.Exists(Indexdic_Server))
            {
                Directory.CreateDirectory(Indexdic_Server);
            }
            Lucene.Net.Store.Directory dir = new Lucene.Net.Store.SimpleFSDirectory(new DirectoryInfo(Indexdic_Server));
            //IndexWriter第三个参数:true指重新创建索引,false指从当前索引追加....此处为追加索引所以为false
            try
            {
                //Lucene.Net.Store.FSDirectory directory = Lucene.Net.Store.FSDirectory.Open(new DirectoryInfo("/Lucene.Net/IndexDic/Gys"), new Lucene.Net.Store.NativeFSLockFactory());
                //bool isUpdate = IndexReader.IndexExists(directory);


                //IndexWriter writer = new IndexWriter(directory, PanGuAnalyzer, isUpdate, Lucene.Net.Index.IndexWriter.MaxFieldLength.LIMITED);
                //AddIndex_gys(writer, model);
                //IndexWriter writer = new IndexWriter(Indexdic_Server, PanGuAnalyzer, false, Lucene.Net.Index.IndexWriter.MaxFieldLength.LIMITED);

                IndexWriter writer = new IndexWriter(dir, PanGuAnalyzer, false, IndexWriter.MaxFieldLength.UNLIMITED);
                AddIndex_gys(writer, model);

                writer.Optimize();
                writer.Close();
            }
            catch
            {
                //IndexWriter writer = new IndexWriter(Indexdic_Server, PanGuAnalyzer, true, Lucene.Net.Index.IndexWriter.MaxFieldLength.LIMITED);
                IndexWriter writer = new IndexWriter(dir, PanGuAnalyzer, true, IndexWriter.MaxFieldLength.UNLIMITED);
                AddIndex_gys(writer, model);

                writer.Optimize();
                writer.Close();
            }


            //string Indexdic_Server = IndexDic(indexdic);

            //PanGu.Segment.Init(PanGuXmlPath);
            ////创建索引目录
            //Lucene.Net.Store.FSDirectory directory = Lucene.Net.Store.FSDirectory.Open(new DirectoryInfo(Indexdic_Server), new Lucene.Net.Store.NativeFSLockFactory());
            //bool isUpdate = IndexReader.IndexExists(directory);

            //IndexWriter writer = new IndexWriter(directory, PanGuAnalyzer, isUpdate, Lucene.Net.Index.IndexWriter.MaxFieldLength.LIMITED);
            //AddIndex_gys(writer, model);

            //writer.Optimize();
            //writer.Close();

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="model"></param>
        private void AddIndex_gys(IndexWriter writer, 供应商 model)
        {
            try
            {
                Document doc = new Document();
                doc.Add(new Field("NumId", model.Id.ToString(), Field.Store.YES, Field.Index.NOT_ANALYZED));//存储不索引

                if (model.所属地域.省份 != null)
                {
                    Field f = new Field("Province", model.所属地域.省份, Field.Store.YES, Field.Index.ANALYZED);//所属省份
                    f.SetBoost(3F);
                    doc.Add(f);
                }

                if (model.所属地域.城市 != null)
                {
                    Field f = new Field("City", model.所属地域.城市, Field.Store.YES, Field.Index.ANALYZED);//所属城市
                    f.SetBoost(3F);
                    doc.Add(f);
                }

                if (model.所属地域.区县 != null)
                {
                    Field f = new Field("Area", model.所属地域.区县, Field.Store.YES, Field.Index.ANALYZED);//所属区县
                    f.SetBoost(3F);
                    doc.Add(f);
                }

                if (model.企业联系人信息 != null && model.企业联系人信息.联系人固定电话 != null)
                {
                    doc.Add(new Field("Telephone", model.企业联系人信息.联系人固定电话, Field.Store.YES, Field.Index.NOT_ANALYZED));//注册地址    存储不索引
                }

                if (model.企业联系人信息 != null && model.企业联系人信息.联系人姓名 != null)
                {
                    doc.Add(new Field("P_Name", model.企业联系人信息.联系人姓名, Field.Store.YES, Field.Index.NOT_ANALYZED));//存储不索引
                }
                if (model.企业基本信息 != null && model.企业基本信息.所属行业 != null)
                {
                    Field f = new Field("Industry", model.企业基本信息.所属行业, Field.Store.YES, Field.Index.ANALYZED);//所属行业
                    f.SetBoost(3F);
                    doc.Add(f);
                }

                //if (model.可提供产品类别列表 != null && model.可提供产品类别列表.Any())
                //{
                //    var pro_industry = "";
                //    foreach (var item in model.可提供产品类别列表)
                //    {
                //        pro_industry += item.一级分类 + ";";
                //    }
                //    Field f = new Field("Pro_Industry", pro_industry, Field.Store.YES, Field.Index.ANALYZED);//可提供产品分类
                //    doc.Add(f);
                //}

                if (model.可提供产品类别列表 != null && model.可提供产品类别列表.Any())
                {
                    var pro_industry = "";
                    foreach (var item in model.可提供产品类别列表)
                    {
                        foreach (var it in item.二级分类)
                        {
                            pro_industry += it + ";";
                        }
                    }
                    Field f = new Field("Pro_Industry", pro_industry, Field.Store.YES, Field.Index.ANALYZED);//可提供产品分类
                    doc.Add(f);
                }

                if (model.企业基本信息.企业名称 != null)
                {
                    Field f = new Field("Name", model.企业基本信息.企业名称, Field.Store.YES, Field.Index.ANALYZED);//企业名称
                    f.SetBoost(5F);
                    doc.Add(f);
                }

                var 认证级别 = ((int)model.供应商用户信息.认证级别).ToString();
                doc.Add(new Field("Rzjb", 认证级别.ToString(), Field.Store.YES, Field.Index.NOT_ANALYZED));//存储不索引

                //各图标字符串组合，顺序 ：年检、应急、协议、入库
                var 图标 = "";
                if (model.供应商用户信息.年检列表 != null && model.供应商用户信息.年检列表.Any() &&
                   model.供应商用户信息.年检列表.ContainsKey(DateTime.Now.Year.ToString()))
                {
                    图标 += "1,";
                }
                else
                {
                    图标 += "0,";
                }
                if (model.供应商用户信息.应急供应商)
                {
                    图标 += "1,";
                }
                else
                {
                    图标 += "0,";
                }
                if (model.供应商用户信息.协议供应商)
                {
                    图标 += "1,";
                }
                else
                {
                    图标 += "0,";
                }
                图标 += ((int)model.供应商用户信息.入库级别).ToString();

                Field f1 = new Field("Level_Flage", 图标, Field.Store.YES, Field.Index.NOT_ANALYZED);
                doc.Add(f1);
                //各图标字符串组合，顺序 ：认证、年检、应急、协议、入库

                //员工人数
                doc.Add(new Field("People_Count", model.企业基本信息.员工人数.ToString(), Field.Store.YES, Field.Index.NOT_ANALYZED));//存储不索引
                //商品总数
                var pro_count = 商品管理.计数供应商商品(model.Id, 0, 0, Query<商品>.EQ(o => o.审核数据.审核状态, 审核状态.审核通过), false);
                doc.Add(new Field("Pro_Count", pro_count.ToString(), Field.Store.YES, Field.Index.NOT_ANALYZED));//存储不索引
                //历史参标次数
                var history_count = model.历史参标记录.Count();
                doc.Add(new Field("History_Count", history_count.ToString(), Field.Store.YES, Field.Index.NOT_ANALYZED));//存储不索引
                //资质证书
                var zzzs = model.资质证书列表.Any() && model.资质证书列表[0].资质证书电子扫描件.Any() && !string.IsNullOrWhiteSpace(model.资质证书列表[0].资质证书电子扫描件[0].路径) ? model.资质证书列表[0].资质证书电子扫描件[0].路径 : "";
                doc.Add(new Field("Zzzs_Pic", zzzs, Field.Store.YES, Field.Index.NOT_ANALYZED));//存储不索引
                //厂房及设备图
                var gyspic = model.供应商用户信息.供应商图片.Any() ? model.供应商用户信息.供应商图片.Last() : "";
                doc.Add(new Field("Gys_Pic", gyspic, Field.Store.YES, Field.Index.NOT_ANALYZED));//存储不索引
                //右边商品展示
                var showprostr = "";
                var showpropic = new List<商品链接>();
                if (增值服务记录 != null)
                {
                    var 广告商品B1_1 = 增值服务记录.已开通的服务.Where(o => o.所申请项目名.Contains("企业推广服务B1-1位置") && o.结束时间 > DateTime.Now);
                    var 广告商品B1_2 = 增值服务记录.已开通的服务.Where(o => o.所申请项目名.Contains("企业推广服务B1-2位置") && o.结束时间 > DateTime.Now);
                    var 商务会员 = 增值服务记录.已开通的服务.Where(o => o.所申请项目名.Contains("商务会员") && o.结束时间 > DateTime.Now);
                    var 标准会员 = 增值服务记录.已开通的服务.Where(o => o.所申请项目名.Contains("标准会员") && o.结束时间 > DateTime.Now);

                    var 广告商品数 = (广告商品B1_2.Any() || 商务会员.Any()) ? 6 : (广告商品B1_1.Any() || 标准会员.Any()) ? 3 : 0;
                    if (广告商品数 == 6)
                    {
                        if (model.供应商用户信息.广告商品.ContainsKey("企业推广服务B1-2位置"))
                        {
                            showpropic = model.供应商用户信息.广告商品["企业推广服务B1-2位置"].Select(o => o.商品).ToList();
                        }
                        else if (model.供应商用户信息.广告商品.ContainsKey("商务会员"))
                        {
                            showpropic = model.供应商用户信息.广告商品["商务会员"].Select(o => o.商品).ToList();
                        }
                    }
                    if (广告商品数 == 3)
                    {
                        if (model.供应商用户信息.广告商品.ContainsKey("企业推广服务B1-1位置"))
                        {
                            showpropic = model.供应商用户信息.广告商品["企业推广服务B1-1位置"].Select(o => o.商品).ToList();
                        }
                        else if (model.供应商用户信息.广告商品.ContainsKey("标准会员"))
                        {
                            showpropic = model.供应商用户信息.广告商品["标准会员"].Select(o => o.商品).ToList();
                        }
                    }

                    //没有选择展示商品，默认选取审核通过的前3/6个商品作为展示
                    if (广告商品数 > 0 && showpropic.Count < 广告商品数)
                    {
                        var sp = 商品管理.查询供应商商品(model.Id, 0, 广告商品数 - showpropic.Count, Query<商品>.EQ(o => o.审核数据.审核状态, 审核状态.审核通过), includeDisabled: false);
                        foreach (var s in sp)
                        {
                            商品链接 p = new 商品链接();
                            p.商品ID = s.Id;
                            showpropic.Add(p);
                        }
                    }

                    if (showpropic.Any())
                    {
                        foreach (var itemlist in showpropic)
                        {
                            var item = itemlist.商品;
                            //图片
                            if (item.商品信息.商品图片.Any())
                            {
                                showprostr += item.商品信息.商品图片[0] + "****";
                            }
                            else
                            {
                                showprostr += "/Images/noimage.jpg****";
                            }
                            //名称
                            showprostr += item.商品信息.商品名 + "****";
                            //价格
                            showprostr += item.销售信息.价格 + "****";
                            //ID
                            showprostr += item.Id + "****||||";
                        }
                    }
                }
                doc.Add(new Field("Show_Product", showprostr, Field.Store.YES, Field.Index.NOT_ANALYZED));//存储不索引

                //经营类型
                doc.Add(new Field("Management", model.企业基本信息.经营类型 + "/" + model.企业基本信息.经营子类型, Field.Store.YES, Field.Index.NOT_ANALYZED));//存储不索引

                writer.AddDocument(doc);
            }
            catch
            {
                return;
            }
        }

        /// <summary>
        /// 创建商品索引
        /// </summary>
        private void CreateIndex(商品 model, string indexdic)
        {
            string Indexdic_Server = IndexDic(indexdic);

            PanGu.Segment.Init(PanGuXmlPath);
            //创建索引目录
            if (!Directory.Exists(Indexdic_Server))
            {
                Directory.CreateDirectory(Indexdic_Server);
            }
            Lucene.Net.Store.Directory dir = new Lucene.Net.Store.SimpleFSDirectory(new DirectoryInfo(Indexdic_Server));
            //IndexWriter第三个参数:true指重新创建索引,false指从当前索引追加....此处为追加索引所以为false
            try
            {
                //IndexWriter writer = new IndexWriter(Indexdic_Server, PanGuAnalyzer, false, Lucene.Net.Index.IndexWriter.MaxFieldLength.LIMITED);
                IndexWriter writer = new IndexWriter(dir, PanGuAnalyzer, false, IndexWriter.MaxFieldLength.UNLIMITED);
                AddIndex(writer, model);

                writer.Optimize();
                writer.Close();
            }
            catch
            {
                //IndexWriter writer = new IndexWriter(Indexdic_Server, PanGuAnalyzer, true, Lucene.Net.Index.IndexWriter.MaxFieldLength.LIMITED);
                IndexWriter writer = new IndexWriter(dir, PanGuAnalyzer, true, IndexWriter.MaxFieldLength.UNLIMITED);
                AddIndex(writer, model);

                writer.Optimize();
                writer.Close();
            }

        }
        /// <summary>
        /// 创建商品索引
        /// </summary>
        /// <param name="analyzer"></param>
        /// <param name="title"></param>
        /// <param name="content"></param>
        private void AddIndex(IndexWriter writer, 商品 model)
        {
            try
            {
                Document doc = new Document();
                doc.Add(new Field("NumId", model.Id.ToString(), Field.Store.YES, Field.Index.NOT_ANALYZED));//存储且索引

                Field f = new Field("Name", model.商品信息.商品名.Replace(" ", ""), Field.Store.YES, Field.Index.ANALYZED);
                f.SetBoost(5F);//为标题增加权重
                doc.Add(f);

                f = new Field("ExactModel", model.商品信息.精确型号.Replace(" ", ""), Field.Store.YES, Field.Index.ANALYZED);
                f.SetBoost(4F);//为精确型号增加权重
                doc.Add(f);

                doc.Add(new Field("Description", model.商品数据.商品简介.Replace(" ", ""), Field.Store.YES, Field.Index.NOT_ANALYZED));//存储且索引

                if (model.商品信息.商品图片.Count > 0)
                {
                    doc.Add(new Field("Pic", model.商品信息.商品图片[0], Field.Store.YES, Field.Index.NOT_ANALYZED));//存储且索引
                }
                else
                {
                    doc.Add(new Field("Pic", "/images/noimage.jpg", Field.Store.YES, Field.Index.NOT_ANALYZED));//存储且索引
                }
                doc.Add(new Field("Price", model.销售信息.价格.ToString(), Field.Store.YES, Field.Index.NOT_ANALYZED));//存储且索引
                doc.Add(new Field("Company", model.商品信息.所属供应商.用户ID.ToString(), Field.Store.YES, Field.Index.NOT_ANALYZED));//存储且索引
                doc.Add(new Field("Attribute", JsonConvert.SerializeObject(model.商品数据.商品属性), Field.Store.YES, Field.Index.NOT_ANALYZED));//存储且索引
                doc.Add(new Field("AddTime", model.基本数据.修改时间.ToString("yyyy-MM-dd"), Field.Store.YES, Field.Index.NOT_ANALYZED));//存储且索引

                //新增按地域查找
                doc.Add(new Field("Province", model.商品信息.所属供应商.用户数据.所属地域.省份, Field.Store.YES, Field.Index.NOT_ANALYZED));//存储且索引
                doc.Add(new Field("City", model.商品信息.所属供应商.用户数据.所属地域.城市, Field.Store.YES, Field.Index.NOT_ANALYZED));//存储且索引
                //新增按地域查找

                writer.AddDocument(doc);
            }
            catch
            {
                return;
            }
        }
    }
}
