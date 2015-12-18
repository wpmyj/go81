using System.Diagnostics;
using System.Text;
using FileHelper;
using Gma.QrCodeNet.Encoding.DataEncodation;
using Go81WebApp.Controllers.基本功能;
using Go81WebApp.Models.数据模型.推荐数据模型;
using Go81WebApp.Models.管理器;
using Go81WebApp.Models.数据模型;
using Go81WebApp.Models.数据模型.竞标数据模型;
using Go81WebApp.Models.数据模型.内容数据模型;
using Go81WebApp.Models.数据模型.项目数据模型;
using Go81WebApp.Models.数据模型.消息数据模型;
using Go81WebApp.Models.数据模型.用户数据模型;
using Go81WebApp.Models.管理器.推荐管理;
using Go81WebApp.Models.数据模型.商品数据模型;
using Helpers;
using Ionic.Zip;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.PanGu;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using MongoDB;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.IdGenerators;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text.RegularExpressions;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using NPOI.SS.Formula.Functions;
using PanGu.Framework;
using Go81WebApp.Models.管理器.内容管理;
using System.Drawing;
using System.Drawing.Imaging;
using Gma.QrCodeNet.Encoding;
using Gma.QrCodeNet.Encoding.Windows.Render;
using System.Drawing.Drawing2D;
using Go81WebApp.Models.数据模型.权限数据模型;
using Helpers.印章;
using Newtonsoft.Json;
using Go81WebApp.Models.数据模型.需求计划模型;
using Go81WebApp.Models.管理器.需求计划管理;
using System.Collections.Specialized;
using Go81WebApp.Models.Helpers;
using Go81WebApp.Models.管理器.抽选管理;
using Go81WebApp._Code.Models.数据模型.商品数据模型;
using Go81WebApp.Models.数据模型.统计数据模型;
using Go81WebApp.Models.管理器.统计管理;

namespace Go81WebApp.Controllers.后台
{
    [登录验证]
    [用户类型验证(typeof(单位用户))]
    public class 单位用户后台Controller : Controller
    {
        private static int GG_PAGESIZE = int.Parse(ConfigurationManager.AppSettings["公告每页显示条数"]);
        private static int NEWS_PAGESIZE = int.Parse(ConfigurationManager.AppSettings["新闻每页显示条数"]);
        private static int TZ_PAGESIZE = int.Parse(ConfigurationManager.AppSettings["通知每页显示条数"]);
        private static int ZCFG_PAGESIZE = int.Parse(ConfigurationManager.AppSettings["政策法规每页显示条数"]);
        private static int USERGROUP_PAGESIZE = int.Parse(ConfigurationManager.AppSettings["用户组每页显示个数"].ToString());
        private static int UNITUSER_PAGESIZE = int.Parse(ConfigurationManager.AppSettings["单位用户每页显示个数"].ToString());
        private static int ACCEPTANCE_PAGESIZE = int.Parse(ConfigurationManager.AppSettings["验收单每页显示条数"].ToString());
        private static int GYS_EXAMINE_PAGESIZE = int.Parse(ConfigurationManager.AppSettings["后台审核供应商列表每页显示条数"].ToString());

        private 单位用户 currentUser
        {
            get
            {
                return this.HttpContext.获取当前用户<单位用户>();
            }
        }

        [单一权限验证(权限.修改登录密码)]
        public ActionResult MessageKeyword()
        {
            return View();
        }
        [HttpPost]
        [AllowAnonymous]
        [ValidateInput(false)]

        public ActionResult MessageKeyword(DepartmentAddModel m)
        {
            var oldpass = Request.Form["oldpass"];
            var newpass_r = Request.Form["newpass_r"];
            if (string.IsNullOrWhiteSpace(oldpass) || string.IsNullOrWhiteSpace(newpass_r) ||
                string.IsNullOrWhiteSpace(m.u.登录信息.密码))
            {
                return Content("<script>alert('请将信息输入完整');window.location='/单位用户后台/MessageKeyword';</script>");
            }

            //新输入的密码
            var code = Hash.Compute(m.u.登录信息.密码);

            //原密码与当前密码是否匹配
            if (Hash.Compute(oldpass) == currentUser.登录信息.密码)
            {
                //两次输入的新密码是否一致
                if (m.u.登录信息.密码 == newpass_r)
                {
                    currentUser.登录信息.密码 = code;
                    用户管理.更新用户<单位用户>(currentUser);
                    this.HttpContext.登出();
                    return Content("<script>alert('修改成功，请重新登录！');window.location='/首页/Index';</script>");
                    //return RedirectToAction("Index", "首页");
                }
                else
                {
                    return Content("<script>alert('两次密码不一致！');window.location='/单位用户后台/MessageKeyword';</script>");
                }
            }
            else
            {
                return Content("<script>alert('原密码不正确！');window.location='/单位用户后台/MessageKeyword';</script>");
            }
        }

        [单一权限验证(权限.修改联系人信息)]
        public ActionResult MessageModify()
        {
            var m = currentUser;
            return View(m);

        }
        public ActionResult PurchaseInfo()
        {
            long pgCount = 0;
            int cpg = 0;
            if (!string.IsNullOrWhiteSpace(Request.QueryString["page"]))
            {
                cpg = int.Parse(Request.QueryString["page"]);
            }
            if (cpg <= 0)
            {
                cpg = 1;
            }
            long pc = 购物车管理.计数购物车(0, 0, Query<购物车>.Where(m => m.所属用户.用户ID == currentUser.Id));
            pgCount = pc / 10;
            if (pc % 10 > 0)
            {
                pgCount++;
            }
            ViewData["Pagecount"] = pgCount;
            ViewData["CurrentPage"] = cpg;
            IEnumerable<购物车> cars = 购物车管理.查询购物车(10 * (cpg - 1), 10, Query<购物车>.Where(m => m.所属用户.用户ID == currentUser.Id));
            return View(cars);
        }
        [HttpPost]

        public ActionResult MessageModify(单位用户 model)
        {
            if (ModelState.IsValid)
            {
                currentUser.单位信息.单位名称 = model.单位信息.单位名称;
                currentUser.单位信息.单位代号 = model.单位信息.单位代号;
                currentUser.联系方式.联系人 = model.联系方式.联系人;
                currentUser.联系人职务 = model.联系人职务;
                currentUser.联系方式.固定电话 = model.联系方式.固定电话;
                currentUser.联系方式.手机 = model.联系方式.手机;
                currentUser.单位信息.所属单位 = model.单位信息.所属单位;

                var deliverprovince = Request.Form["deliverprovince"];
                var delivercity = Request.Form["delivercity"];
                var deliverarea = Request.Form["deliverarea"];

                currentUser.所属地域 = new _地域();
                if (!string.IsNullOrWhiteSpace(deliverprovince))
                {
                    currentUser.所属地域.省份 = deliverprovince;
                    if (!string.IsNullOrWhiteSpace(delivercity))
                    {
                        currentUser.所属地域.城市 = delivercity;
                        if (!string.IsNullOrWhiteSpace(deliverarea))
                        {
                            currentUser.所属地域.区县 = deliverarea;
                        }
                    }
                }
                if (currentUser.管理单位.用户ID == 10)
                {
                    用户管理.更新用户<单位用户>(currentUser);
                    return Content("<script>alert('修改成功！');window.location='/单位用户后台/MessageModify';</script>");
                }
                else
                {
                    currentUser.审核数据.审核状态 = 审核状态.未审核;
                    用户管理.更新用户<单位用户>(currentUser);
                    this.HttpContext.登出();
                    return Content("<script>alert('修改成功，您的状态将变为未审核，请等待管理员审核！');window.location='/首页/Index';</script>");
                }
            }
            return View();
        }
        public ActionResult ConsultPrice()
        {
            string id = Request.QueryString["id"];
            ViewData["id"] = id;
            return View();
        }
        public class ConsultInfo
        {
            public long Id { get; set; }
            public string Sname { get; set; }
            public string Gprice { get; set; }
        }
        public ActionResult GetSupplier()
        {
            long id = -1;
            if (!string.IsNullOrWhiteSpace(Request.QueryString["id"]))
            {
                id = long.Parse(Request.QueryString["id"]);
            }
            string province = Request.QueryString["pro"];
            商品 model = 商品管理.查找商品(id);
            IEnumerable<商品> gd = null;
            List<ConsultInfo> info = new List<ConsultInfo>();
            if (model != null)
            {
                gd = 商品管理.查询商品(0, 0, Query<商品>.Where(m => m.商品信息.精确型号 == model.商品信息.精确型号));
                foreach (var item in gd)
                {
                    if (item.商品信息.所属供应商.用户数据.所属地域.省份 == province)
                    {
                        ConsultInfo cinfo = new ConsultInfo();
                        cinfo.Id = item.商品信息.所属供应商.用户数据.Id;
                        cinfo.Sname = item.商品信息.所属供应商.用户数据.企业基本信息.企业名称;
                        cinfo.Gprice =string.Format("{0:c2}",item.销售信息.价格);
                        info.Add(cinfo);
                    }
                }
            }
            JsonResult json = new JsonResult() { Data = info };
            return Json(json, JsonRequestBehavior.AllowGet);
        }
        public ActionResult CounsultStepTwo()
        {
            string gid = Request.Form["gid"];
            string[] ids = gid.Split(',');
            long goodid = long.Parse(Request.Form["pid"]);
            List<long> id = new List<long>();
            for (int i = 0; i < ids.Length - 1; i++)
            {
                id.Add(long.Parse(ids[i]));
            }
            IEnumerable<供应商> gys = 用户管理.查询用户<供应商>(0, 0, Query<供应商>.In(m => m.Id, id));
            询价采购 model = new 询价采购();
            List<_议价列表> consult = new List<_议价列表>();
            foreach (var item in gys)
            {
                _议价列表 yj = new _议价列表();
                yj.供应商.用户ID = item.Id;
                yj.议价商品.商品ID = goodid;
                yj.数量 = 1;
                consult.Add(yj);
            }
            model.议价列表 = consult;
            model.询价商品.商品ID = goodid;
            return View(model);
        }
        [HttpPost]
        public ActionResult CreateInfo()
        {
            询价采购 model = new 询价采购();
            long id = long.Parse(Request.Form["yjsp"]);
            model.询价商品.商品ID = id;
            NameValueCollection coll = Request.Form;
            int count = coll.Count / 3;
            List<_议价列表> list = new List<_议价列表>();
            for (int i = 0; i < count; i++)
            {
                _议价列表 m = new _议价列表();
                m.备注 = Request.Form["note" + i];
                m.供应商.用户ID = long.Parse(Request.Form["gid" + i]);
                m.数量 = int.Parse(Request.Form["number" + i]);
                m.议价商品.商品ID = id;
                list.Add(m);
            }
            model.采购单位.用户ID = currentUser.Id;
            model.议价列表 = list;
            询价采购管理.添加询价采购(model);
            return Redirect("/单位用户后台/ConsultDetail?id=" + model.Id);
        }
        public int ChangePrice()
        {
            try
            {
                string[] arr = Request.QueryString["id"].ToString().Split('|');
                decimal price = decimal.Parse(Request.QueryString["p"]);
                long xid = long.Parse(arr[0]);
                long gid = long.Parse(arr[1]);
                询价采购 model = 询价采购管理.查找询价采购(xid);
                foreach (var item in model.议价列表)
                {
                    if (item.供应商.用户ID == gid)
                    {
                        item.议价 = price;
                    }
                }
                return 询价采购管理.更新询价采购(model) ? 1 : -1;
            }
            catch
            {
                return -1;
            }
        }
        public int DeleteXj()
        {
            try
            {
                string[] arr =Request.QueryString["id"].ToString().Split('|');
                long xid = long.Parse(arr[0]);
                long gid = long.Parse(arr[1]);
                询价采购 model = 询价采购管理.查找询价采购(xid);
                List<_议价列表> items = new List<_议价列表>();
                foreach(var item in model.议价列表)
                {
                    if(item.供应商.用户ID!=gid)
                    {
                        items.Add(item);
                    }
                }
                model.议价列表 = items;
                return 询价采购管理.更新询价采购(model) ? 1 : -1 ;
            }
            catch
            {
                return -1;
            }
        }
        public ActionResult ConsultList()
        {
            IEnumerable<询价采购> model = 询价采购管理.查询询价采购(0, 10);
            List<询价采购> newmodel = new List<询价采购>();
            foreach (var item in model)
            {
                if (item.采购单位.用户ID==currentUser.Id)
                {
                    newmodel.Add(item);
                }
            }
            ViewData["id"] = currentUser.Id;
            return View(newmodel);
        }
        public ActionResult ConsultDetail()
        {
            try
            {
                long id = long.Parse(Request.QueryString["id"]);
                询价采购 model = 询价采购管理.查找询价采购(id);
                return View(model);
            }
            catch
            {
                return Redirect("/商品陈列/");
            }
        }
        public ActionResult UpdateInfo()
        {
            try
            {
                long xid = long.Parse(Request.Form["xid"]);
                string[] ids = Request.Form["gids"].ToString().Split(',');
                询价采购 model = 询价采购管理.查找询价采购(xid);
                List<long> gids = new List<long>();
                for (int i = 0; i < ids.Length - 1;i++ )
                {
                    gids.Add(long.Parse(ids[i]));
                }
                foreach(var item in model.议价列表)
                {
                    if(gids.Contains(item.供应商.用户ID))
                    {
                        item.交易状态 = true;
                    }
                }
                询价采购管理.更新询价采购(model);
                return Redirect("/单位用户后台/AddattachInfo?id="+model.Id);
            }
            catch
            {
                return Redirect("/商品陈列/");
            }
        }
        public ActionResult AddattachInfo()
        {
            long id =long.Parse(Request.QueryString["id"]);
            询价采购 model = 询价采购管理.查找询价采购(id);
            return View(model);
        }
        [HttpPost]
        public ActionResult AddattachInfo_purchase(询价采购 model)
        {
            询价采购 m = 询价采购管理.查找询价采购(model.Id);
            m.订单号 = DateTime.Now.Year.ToString() + DateTime.Now.Month.ToString() + DateTime.Now.Day.ToString() + DateTime.Now.Hour.ToString() + DateTime.Now.Minute.ToString() + DateTime.Now.Second.ToString();
            m.生成日期 = DateTime.Now;
            m.采购单位.用户ID = currentUser.Id;
            m.附加信息 = model.附加信息;
            询价采购管理.更新询价采购(m);
            return Redirect("/单位用户后台/OrderInfo");
        }
        public ActionResult OrderContract()
        {
            long id = long.Parse(Request.QueryString["id"]);
            询价采购 model = 询价采购管理.查找询价采购(id);
            return View(model);
        }
        public ActionResult OrderInfo()
        {
            long pgCount = 0;
            int cpg = 0;
            if (!string.IsNullOrWhiteSpace(Request.QueryString["page"]))
            {
                cpg = int.Parse(Request.QueryString["page"]);
            }
            if (cpg <= 0)
            {
                cpg = 1;
            }
            long pc = 询价采购管理.计数询价采购(0,0);
            pgCount = pc / 10;
            if (pc % 10 > 0)
            {
                pgCount++;
            }
            ViewData["Pagecount"] = pgCount;
            ViewData["CurrentPage"] = cpg;
            IEnumerable<询价采购> model = 询价采购管理.查询询价采购(10*(cpg-1), 10);
            ViewData["orders"] = model;
            return View();
        }
        public ActionResult Del_Department(long id)
        {
            用户管理.删除用户<单位用户>(id);
            return View("DepartmentList");
        }
        public ActionResult ExList()
        {
            return View();
        }
        public ActionResult Print()
        {
            try
            {
                long id = long.Parse(Request.QueryString["id"]);
                专家 model = 用户管理.查找用户<专家>(id);
                if (model == null)
                {
                    return Redirect("/专家抽选/Expert_list");
                }
                else
                {
                    return View(model);
                }
            }
            catch
            {
                return Redirect("/专家抽选/Expert_list");
            }
        }
        public ActionResult Part_Exlist()
        {
            int page = 1;
            int.TryParse(Request.Params["page"], out page);

            long listcount = 用户管理.查询用户<专家>(0, 0).Count();
            long maxpage = Math.Max((listcount + 10 - 1) / 10, 1);

            ViewData["currentpage"] = page;
            ViewData["pagecount"] = maxpage;

            ViewData["专家列表"] = 用户管理.查询用户<专家>(10 * (page - 1), 10);
            return PartialView("Procure_Part/Part_Exlist");
        }
        public ActionResult Part_BackHead()
        {
            var m = currentUser;
            return PartialView("Procure_Part/Part_BackHead", m);
        }
        public string PassDemand()
        {
            try
            {
                long id = long.Parse(Request.QueryString["id"]);
                需求计划 model = 需求计划管理.查找需求计划(id);

                long uid = -1;
                if (model.审批流程单位列表.Any() && model.审批流程单位列表.Last().用户ID != currentUser.Id)
                {
                    for (var i = 0; i < model.审批流程单位列表.Count; i++)
                    {
                        if (model.审批流程单位列表[i].用户ID == currentUser.Id)
                        {
                            uid = model.审批流程单位列表[i + 1].用户ID;
                            break;
                        }
                    }
                    model.当前处理单位链接.用户ID = uid;
                }
                if (model.审核历史列表.Any() && model.审核历史列表.Last().审核者.用户ID == currentUser.Id)
                {
                    model.审核历史列表.Last().审核状态 = 审核状态.审核通过;
                    model.审核历史列表.Last().审核时间 = DateTime.Now;
                    //model.当前处理单位链接.用户ID=
                }
                else
                {
                    需求计划._审核数据 check = new 需求计划._审核数据();
                    check.审核状态 = 审核状态.审核通过;
                    check.审核者.用户ID = currentUser.Id;
                    check.审核时间 = DateTime.Now;
                    model.审核历史列表.Add(check);
                }
                需求计划管理.更新需求计划(model);
                return uid == -1 ? "-1" : "1";
            }
            catch
            {
                return "0";
            }
        }

        [单一权限验证(权限.全部附属账号的采购公告列表, 权限.全部附属账号的验收单列表)]
        public ActionResult SubUnit_Manage()
        {
            var comes = Request.Params["comes"];
            if (comes == "ad")
            {
                ViewData["comes"] = "全部附属账号的采购公告列表";
            }
            else
            {
                ViewData["comes"] = "全部附属账号的验收单列表";
            }

            List<long> uid = new List<long>();
            IEnumerable<单位用户> user = 用户管理.查询用户<单位用户>(0, 0, Query<单位用户>.Where(m => m.所属单位.用户ID == currentUser.Id));
            foreach (var item in user)
            {
                uid.Add(item.Id);
            }
            ViewData["user"] = user;
            return View();
        }
        public class Content
        {
            public string name { get; set; }
            public string time { get; set; }
            public long id { get; set; }
        }
        public class ysd
        {
            public long id { get; set; }
            public string time { get; set; }
            public string supplier { get; set; }
            public string reciever { get; set; }
            public string checker { get; set; }
            public decimal money { get; set; }
            public string status { get; set; }
        }
        public ActionResult SearchUnit()
        {
            long id = long.Parse(Request.QueryString["id"]);
            List<Content> uids = new List<Content>();
            IEnumerable<单位用户> user = 用户管理.查询用户<单位用户>(0, 0, Query<单位用户>.Where(m => m.所属单位.用户ID == id));
            foreach (var item in user)
            {
                Content c = new Content();
                c.id = item.Id;
                if (!string.IsNullOrWhiteSpace(item.单位信息.单位代号))
                {
                    c.name = item.单位信息.单位代号;
                }
                else
                {
                    c.name = item.单位信息.单位名称;
                }
                uids.Add(c);
            }
            JsonResult json = new JsonResult() { Data = new { uid = uids } };
            return Json(json, JsonRequestBehavior.AllowGet);
        }
        public ActionResult SearchYsd()
        {
            long pageCount = 0;//总页数
            List<ysd> ct = new List<ysd>();
            List<long> ids = new List<long>();
            string str = Request.QueryString["id"];
            if (str.Contains(","))
            {
                for (int i = 0; i < str.Split(',').Length - 1; i++)
                {
                    ids.Add(long.Parse(str.Split(',')[i]));
                }
            }
            else
            {
                ids.Add(long.Parse(Request.QueryString["id"]));
            }
            int skip = int.Parse(Request.QueryString["skip"]);
            pageCount = 验收单管理.计数验收单(0, 0, Query<验收单>.In(m => m.审核数据.审核者.用户ID, ids)) / 20;
            if (验收单管理.计数验收单(0, 0, Query<验收单>.In(m => m.审核数据.审核者.用户ID, ids)) % 20 > 0)
            {
                pageCount++;
            }
            IEnumerable<验收单> y = 验收单管理.查询验收单(20 * (skip - 1), 20, Query<验收单>.In(m => m.审核数据.审核者.用户ID, ids));
            foreach (var item in y)
            {
                ysd c = new ysd();
                c.id = item.Id;
                if (item.供应商链接.用户数据 != null)
                {
                    c.supplier = item.供应商链接.用户数据.企业基本信息.企业名称;
                }
                else
                {
                    c.supplier = "未填写企业信息";
                }
                c.reciever = item.收货单位;
                c.money = item.总金额;
                if (item.审核数据.审核者.用户数据 != null)
                {
                    c.checker = item.审核数据.审核者.用户数据.验收单审核单位名称;
                }
                else
                {
                    c.checker = "没有单位信息";
                }
                c.status = item.审核数据.审核状态.ToString();
                c.time = item.基本数据.添加时间.ToString("yyyy/MM/dd");
                ct.Add(c);
            }
            JsonResult json = new JsonResult() { Data = new { info = ct, p = pageCount } };
            return Json(json, JsonRequestBehavior.AllowGet);
        }
        public ActionResult SearchAnnounce()
        {
            long pageCount = 0;//总页数
            List<Content> ct = new List<Content>();
            List<long> ids = new List<long>();
            string str = Request.QueryString["id"];
            if (str.Contains(","))
            {
                for (int i = 0; i < str.Split(',').Length - 1; i++)
                {
                    ids.Add(long.Parse(str.Split(',')[i]));
                }
            }
            else
            {
                ids.Add(long.Parse(Request.QueryString["id"]));
            }
            int skip = int.Parse(Request.QueryString["skip"]);
            pageCount = 公告管理.计数公告(0, 0, Query<公告>.In(m => m.内容基本信息.所有者.用户ID, ids)) / 20;
            if (公告管理.计数公告(0, 0, Query<公告>.In(m => m.内容基本信息.所有者.用户ID, ids)) % 20 > 0)
            {
                pageCount++;
            }
            IEnumerable<公告> y = 公告管理.查询公告(20 * (skip - 1), 20, Query<公告>.In(m => m.内容基本信息.所有者.用户ID, ids));
            foreach (var item in y)
            {
                Content c = new Content();
                c.id = item.Id;
                c.name = item.内容主体.标题;
                c.time = item.基本数据.添加时间.ToString("yyyy/MM/dd");
                ct.Add(c);
            }
            JsonResult json = new JsonResult() { Data = new { info = ct, p = pageCount } };
            return Json(json, JsonRequestBehavior.AllowGet);
        }
        public ActionResult SearchContent()
        {
            long pageCount = 0;//总页数
            List<Content> ct = new List<Content>();
            List<ysd> yd = new List<ysd>();
            List<long> ids = new List<long>();
            string t = Request.QueryString["t"];//1、公告；2、验收单
            string str = Request.QueryString["id"];
            if (str.Contains(","))
            {
                for (int i = 0; i < str.Split(',').Length - 1; i++)
                {
                    ids.Add(long.Parse(str.Split(',')[i]));
                }
            }
            else
            {
                ids.Add(long.Parse(Request.QueryString["id"]));
            }
            int skip = int.Parse(Request.QueryString["skip"]);
            if (t.Contains("2") && !t.Contains("1"))
            {
                pageCount = 验收单管理.计数验收单(0, 0, Query<验收单>.In(m => m.审核数据.审核者.用户ID, ids)) / 20;
                if (验收单管理.计数验收单(0, 0, Query<验收单>.In(m => m.审核数据.审核者.用户ID, ids)) % 20 > 0)
                {
                    pageCount++;
                }
                IEnumerable<验收单> y = 验收单管理.查询验收单(20 * (skip - 1), 20, Query<验收单>.In(m => m.审核数据.审核者.用户ID, ids));
                foreach (var item in y)
                {
                    ysd c = new ysd();
                    c.id = item.Id;
                    if (item.供应商链接.用户数据 != null)
                    {
                        c.supplier = item.供应商链接.用户数据.企业基本信息.企业名称;
                    }
                    else
                    {
                        c.supplier = "未填写企业信息";
                    }
                    c.reciever = item.收货单位;
                    c.money = item.总金额;
                    if (item.审核数据.审核者.用户数据 != null)
                    {
                        c.checker = item.审核数据.审核者.用户数据.验收单审核单位名称;
                    }
                    else
                    {
                        c.checker = "没有单位信息";
                    }
                    c.status = item.审核数据.审核状态.ToString();
                    c.time = item.基本数据.添加时间.ToString("yyyy/MM/dd");
                    yd.Add(c);
                }
                JsonResult json = new JsonResult() { Data = new { info = yd, p = pageCount } };
                return Json(json, JsonRequestBehavior.AllowGet);
            }
            else if (!t.Contains("2") && t.Contains("1"))
            {
                pageCount = 公告管理.计数公告(0, 0, Query<公告>.In(m => m.内容基本信息.所有者.用户ID, ids)) / 20;
                if (公告管理.计数公告(0, 0, Query<公告>.In(m => m.内容基本信息.所有者.用户ID, ids)) % 20 > 0)
                {
                    pageCount++;
                }
                IEnumerable<公告> y = 公告管理.查询公告(20 * (skip - 1), 20, Query<公告>.In(m => m.内容基本信息.所有者.用户ID, ids));
                foreach (var item in y)
                {
                    Content c = new Content();
                    c.id = item.Id;
                    c.name = item.内容主体.标题;
                    c.time = item.基本数据.添加时间.ToString("yyyy/MM/dd");
                    ct.Add(c);
                }
                JsonResult json = new JsonResult() { Data = new { info = ct, p = pageCount } };
                return Json(json, JsonRequestBehavior.AllowGet);
            }
            else
            {
                List<Content> ct1 = new List<Content>();
                long pageCount1 = 0;
                pageCount = 验收单管理.计数验收单(0, 0, Query<验收单>.In(m => m.审核数据.审核者.用户ID, ids)) / 20;
                if (验收单管理.计数验收单(0, 0, Query<验收单>.In(m => m.审核数据.审核者.用户ID, ids)) % 20 > 0)
                {
                    pageCount++;
                }
                IEnumerable<验收单> y = 验收单管理.查询验收单(20 * (skip - 1), 20, Query<验收单>.In(m => m.审核数据.审核者.用户ID, ids));
                foreach (var item in y)
                {
                    ysd c = new ysd();
                    c.id = item.Id;
                    if (item.供应商链接.用户数据 != null)
                    {
                        c.supplier = item.供应商链接.用户数据.企业基本信息.企业名称;
                    }
                    else
                    {
                        c.supplier = "未填写企业信息";
                    }
                    c.reciever = item.收货单位;
                    c.money = item.总金额;
                    if (item.审核数据.审核者.用户数据 != null)
                    {
                        c.checker = item.审核数据.审核者.用户数据.验收单审核单位名称;
                    }
                    else
                    {
                        c.checker = "没有单位信息";
                    }
                    c.status = item.审核数据.审核状态.ToString();
                    c.time = item.基本数据.添加时间.ToString("yyyy/MM/dd");
                    yd.Add(c);
                }
                ///////////////////////
                pageCount1 = 公告管理.计数公告(0, 0, Query<公告>.In(m => m.内容基本信息.所有者.用户ID, ids)) / 20;
                if (公告管理.计数公告(0, 0, Query<公告>.In(m => m.内容基本信息.所有者.用户ID, ids)) % 20 > 0)
                {
                    pageCount1++;
                }
                IEnumerable<公告> g = 公告管理.查询公告(20 * (skip - 1), 20, Query<公告>.In(m => m.内容基本信息.所有者.用户ID, ids));
                foreach (var item in g)
                {
                    Content c = new Content();
                    c.id = item.Id;
                    c.name = item.内容主体.标题;
                    c.time = item.基本数据.添加时间.ToString("yyyy/MM/dd");
                    ct1.Add(c);
                }
                JsonResult json = new JsonResult() { Data = new { ysd = yd, g = ct1, yp = pageCount, gp = pageCount1 } };
                return Json(json, JsonRequestBehavior.AllowGet);
            }
        }
        public static string getdgunitname(long id, long cdrid)
        {
            var xu = 需求计划管理.查找需求计划(id);
            if (xu.并入需求计划链接.需求计划ID == -1 || cdrid == xu.并入需求计划链接.需求计划数据.需求发起单位链接.用户ID)
            {
                return xu.需求计划标题;
            }
            else
            {
                return getdgunitname(xu.并入需求计划链接.需求计划ID, cdrid);
            }
        }
        public long getId(long id)
        {
            需求计划 xu = 需求计划管理.查找需求计划(id);
            if (xu.并入需求计划链接.需求计划ID != -1)
            {
                if (xu.并入需求计划链接.需求计划数据.并入需求计划链接.需求计划ID == -1)
                {
                    return xu.Id;
                }
                else
                {
                    return getId(xu.并入需求计划链接.需求计划ID);
                }
            }
            else
            {
                return xu.Id; ;
            }
        }

        public void ImportExcel()
        {
            var id = Request.Params["id"];
            HttpResponseBase rs = Response;
            TemplateExcel.导出需求计划(long.Parse(id), rs);
        }

        [HttpPost]
        public ActionResult BackOff()
        {
            List<long> pid = new List<long>();//保存审核不通过的需求计划id
            List<long> idArr = new List<long>();//不通过物资Id和原因的集合
            string reason = "";
            long id = long.Parse(Request.Form["id"]);
            需求计划 model = 需求计划管理.查找需求计划(id);
            string r0 = Request.Form["main"];//计划不通过原因
            string r1 = Request.Form["back"];//计划物资不通过原因
            if (!string.IsNullOrWhiteSpace(r0))
            {
                reason += r0.Split(',')[1];
            }
            if (!string.IsNullOrWhiteSpace(r1))
            {
                for (int i = 0; i < r1.Split('|').Length - 1; i++)
                {
                    reason += r1.Split('|')[i].Split(',')[1] + ",";
                    idArr.Add(long.Parse(r1.Split('|')[i].Split(',')[0]));
                }
            }
            model.当前处理单位链接 = model.需求发起单位链接;
            需求计划._审核数据 check = new 需求计划._审核数据();//添加审核数据
            if (model.审核历史列表.Any() && model.审核历史列表.Last().审核者.用户ID == currentUser.Id)
            {
                model.审核历史列表.Last().审核状态 = 审核状态.审核未通过;
                model.审核历史列表.Last().审核时间 = DateTime.Now;
                model.审核历史列表.Last().审核不通过原因 = reason;
                foreach (var item in idArr)
                {
                    需求计划物资链接 a = new 需求计划物资链接();
                    a.需求计划物资ID = item;
                    model.审核历史列表.Last().未通过审批物资列表.Add(a);
                }
            }
            else
            {
                check.审核者.用户ID = currentUser.Id;
                check.审核状态 = 审核状态.审核未通过;
                check.审核不通过原因 = reason;
                check.审核时间 = DateTime.Now;
                foreach (var item in idArr)
                {
                    需求计划物资链接 a = new 需求计划物资链接();
                    a.需求计划物资ID = item;
                    check.未通过审批物资列表.Add(a);
                }
                model.审核历史列表.Add(check);
            }
            //List<用户链接<单位用户>> Ul = new List<用户链接<单位用户>>();
            //if (model.审批流程单位列表.Count != 0)
            //{
            //    foreach (var u in model.审批流程单位列表)
            //    {
            //        if (u.用户ID != currentUser.Id)
            //        {
            //            Ul.Add(u);
            //        }
            //        else
            //        {
            //            break;
            //        }
            //    }
            //    model.审批流程单位列表 = Ul;
            //}
            model.审批流程单位列表 = new List<用户链接<单位用户>>();
            foreach (var item in idArr)
            {
                需求计划物资 wz = 需求计划物资管理.查找需求计划物资(item);
                if (getId(wz.所属需求计划.需求计划ID) != -1)
                {
                    if (!pid.Contains(getId(wz.所属需求计划.需求计划ID)))
                    {
                        pid.Add(getId(wz.所属需求计划.需求计划ID));
                    }
                }
            }
            foreach (var item in model.来源需求计划列表)
            {
                if (!pid.Contains(item.需求计划ID))
                {
                    需求计划 plan = 需求计划管理.查找需求计划(item.需求计划ID);
                    if (plan.审核历史列表.Any())
                    {
                        plan.并入需求计划链接.需求计划ID = -1;
                        plan.审核历史列表.Last().审核者.用户ID = -1;
                        plan.审核历史列表.Last().审核状态 = 审核状态.未审核;
                        需求计划管理.更新需求计划(plan);
                    }
                }
                else
                {
                    需求计划 plan = 需求计划管理.查找需求计划(item.需求计划ID);
                    plan.并入需求计划链接.需求计划ID = -1;
                }
            }
            需求计划管理.更新需求计划(model);
            return Redirect("/单位用户后台/DemandCheck?page=1");
        }
        [HttpPost]
        public ActionResult passOut()
        {
            List<long> pid = new List<long>();//保存审核不通过的需求计划id
            List<long> idArr = new List<long>();
            string reason = "";
            long id = long.Parse(Request.Form["id"]);
            需求计划 model = 需求计划管理.查找需求计划(id);
            string r0 = Request.Form["main"];
            string r1 = Request.Form["back"];
            if (!string.IsNullOrWhiteSpace(r0))
            {
                reason += r0.Split(',')[1];
            }
            if (!string.IsNullOrWhiteSpace(r1))
            {
                for (int i = 0; i < r1.Split('|').Length - 1; i++)
                {
                    reason += r1.Split('|')[i].Split(',')[1] + ",";
                    idArr.Add(long.Parse(r1.Split('|')[i].Split(',')[0]));
                }
            }
            model.当前处理单位链接 = model.需求发起单位链接;
            需求计划._审核数据 check = new 需求计划._审核数据();
            if (model.审核历史列表.Any() && model.审核历史列表.Last().审核者.用户ID == currentUser.Id)
            {
                model.审核历史列表.Last().审核状态 = 审核状态.审核未通过;
                model.审核历史列表.Last().审核时间 = DateTime.Now;
                model.审核历史列表.Last().审核不通过原因 = reason;
                foreach (var item in idArr)
                {
                    需求计划物资链接 a = new 需求计划物资链接();
                    a.需求计划物资ID = item;
                    model.审核历史列表.Last().未通过审批物资列表.Add(a);
                }
            }
            else
            {
                check.审核者.用户ID = currentUser.Id;
                check.审核状态 = 审核状态.审核未通过;
                check.审核不通过原因 = reason;
                check.审核时间 = DateTime.Now;
                foreach (var item in idArr)
                {
                    需求计划物资链接 a = new 需求计划物资链接();
                    a.需求计划物资ID = item;
                    check.未通过审批物资列表.Add(a);
                }
                model.审核历史列表.Add(check);
            }
            foreach (var item in idArr)
            {
                需求计划物资 wz = 需求计划物资管理.查找需求计划物资(item);
                if (getId(wz.所属需求计划.需求计划ID) != -1)
                {
                    if (!pid.Contains(getId(wz.所属需求计划.需求计划ID)))
                    {
                        pid.Add(getId(wz.所属需求计划.需求计划ID));
                    }
                }
            }
            //List<用户链接<单位用户>> Ul = new List<用户链接<单位用户>>();
            //if (model.审批流程单位列表.Count != 0)
            //{
            //    foreach (var u in model.审批流程单位列表)
            //    {
            //        if (u.用户ID != currentUser.Id)
            //        {
            //            Ul.Add(u);
            //        }
            //        else
            //        {
            //            break;
            //        }
            //    }
            //    model.审批流程单位列表 = Ul;
            //}
            model.审批流程单位列表 = new List<用户链接<单位用户>>();
            foreach (var item in model.来源需求计划列表)
            {
                if (!pid.Contains(item.需求计划ID))
                {
                    需求计划 plan = 需求计划管理.查找需求计划(item.需求计划ID);
                    if (plan.审核历史列表.Any())
                    {
                        plan.并入需求计划链接.需求计划ID = -1;
                        plan.审核历史列表.Last().审核者.用户ID = -1;
                        plan.审核历史列表.Last().审核状态 = 审核状态.未审核;
                        需求计划管理.更新需求计划(plan);
                    }
                }
                else
                {
                    需求计划 plan = 需求计划管理.查找需求计划(item.需求计划ID);
                    plan.并入需求计划链接.需求计划ID = -1;
                }
            }
            需求计划管理.更新需求计划(model);
            return Redirect("/单位用户后台/plandetail?page=1");
        }
        public class record
        {
            public string status { get; set; }
            public string time { get; set; }
            public string user { get; set; }
            public long id { get; set; }
        }
        public ActionResult TaskHistory()//需求采购任务审核历史
        {
            try
            {
                List<record> Record = new List<record>();
                long id = long.Parse(Request.QueryString["id"]);
                需求采购任务 model = 需求采购任务管理.查找需求采购任务(id);
                foreach (var item in model.审核历史列表.OrderBy(m => m.审核时间))
                {
                    record r = new record();
                    r.id = item.审核者.用户ID;
                    r.status = item.审核状态.ToString();
                    r.time = item.审核时间.ToString("yyyy/MM/dd");
                    if (!string.IsNullOrWhiteSpace(((单位用户)item.审核者.用户数据).单位信息.单位代号))
                    {
                        r.user = ((单位用户)item.审核者.用户数据).单位信息.单位代号;
                    }
                    else
                    {
                        r.user = ((单位用户)item.审核者.用户数据).单位信息.单位名称;
                    }
                    Record.Add(r);
                }
                foreach (var item in model.审批流程单位列表)
                {
                    bool have = false;
                    foreach (var m in Record)
                    {
                        if (m.id == item.用户ID)
                        {
                            have = true;
                            break;
                        }
                    }
                    if (!have)
                    {
                        record r = new record();
                        r.id = item.用户ID;
                        r.status = "未审核";
                        r.time = "---------";
                        if (!string.IsNullOrWhiteSpace(item.用户数据.单位信息.单位代号))
                        {
                            r.user = item.用户数据.单位信息.单位代号;
                        }
                        else
                        {
                            r.user = item.用户数据.单位信息.单位名称;
                        }
                        Record.Add(r);
                    }
                }
                JsonResult json = new JsonResult() { Data = Record };
                return Json(json, JsonRequestBehavior.AllowGet);
            }
            catch
            {
                return Redirect("/单位用户后台/DemandCheck?page=1");
            }
        }
        public ActionResult History()//需求计划审核历史
        {
            try
            {
                List<record> Record = new List<record>();
                long id = long.Parse(Request.QueryString["id"]);
                需求计划 model = 需求计划管理.查找需求计划(id);
                foreach (var item in model.审核历史列表.OrderBy(m => m.审核时间))
                {
                    record r = new record();
                    r.id = item.审核者.用户ID;
                    r.status = item.审核状态.ToString();
                    r.time = item.审核时间.ToString("yyyy/MM/dd");
                    if (!string.IsNullOrWhiteSpace(((单位用户)item.审核者.用户数据).单位信息.单位代号))
                    {
                        r.user = ((单位用户)item.审核者.用户数据).单位信息.单位代号;
                    }
                    else
                    {
                        r.user = ((单位用户)item.审核者.用户数据).单位信息.单位名称;
                    }
                    Record.Add(r);
                }
                foreach (var item in model.审批流程单位列表)
                {
                    bool have = false;
                    foreach (var m in Record)
                    {
                        if (m.id == item.用户ID)
                        {
                            have = true;
                            break;
                        }
                    }
                    if (!have)
                    {
                        record r = new record();
                        r.id = item.用户ID;
                        r.status = "未审核";
                        r.time = "---------";
                        if (!string.IsNullOrWhiteSpace(item.用户数据.单位信息.单位代号))
                        {
                            r.user = item.用户数据.单位信息.单位代号;
                        }
                        else
                        {
                            r.user = item.用户数据.单位信息.单位名称;
                        }
                        Record.Add(r);
                    }
                }
                JsonResult json = new JsonResult() { Data = Record };
                return Json(json, JsonRequestBehavior.AllowGet);
            }
            catch
            {
                return Redirect("/单位用户后台/DemandCheck?page=1");
            }
        }
        //public void PrintExcel()
        //{
        //    string sid = Request.QueryString["id"];
        //    HttpResponseBase rs = Response;
        //    ExcelHelper.PutMaterialInfo(sid, rs);
        //}
        public void ExportExcel()
        {
            string id = Request.QueryString["id"];
            HttpResponseBase rs = Response;
            ExcelHelper.PutDemandInfo(id, rs);
        }
        //public void PrintDExcel()
        //{
        //    string sid = Request.QueryString["id"];
        //    HttpResponseBase rs = Response;
        //    ExcelHelper.PutDistributeInfo(sid, rs);
        //}
        [HttpPost]
        public ActionResult addChecker(需求计划 m)
        {
            try
            {
                string strid = Request.Form["uid"];
                string topuser = Request.Form["topuser"];
                需求计划 model = 需求计划管理.查找需求计划(m.Id);
                model.编制单位 = m.编制单位;
                model.采购年度 = m.采购年度;
                model.承办部门 = m.承办部门;
                model.建议完成时间 = m.建议完成时间;
                model.联系电话 = m.联系电话;
                model.联系人 = m.联系人;
                model.秘密等级 = m.秘密等级;
                model.需求计划标题 = m.需求计划标题;
                if (model.来源需求计划列表.Any())
                {
                    return Content("<script>alert('合并需求不能再次提交，您可以提交新合并的需求！');window.location='/单位用户后台/demandlist?page=1';</script>");
                }
                else
                {
                    model.审批流程单位列表 = new List<用户链接<单位用户>>();
                    model.审核历史列表 = new List<需求计划._审核数据>();
                    if (!string.IsNullOrWhiteSpace(strid))
                    {
                        for (int i = 0; i < strid.Split(',').Length - 1; i++)
                        {
                            用户链接<单位用户> a = new 用户链接<单位用户>();
                            a.用户ID = long.Parse(strid.Split(',')[i]);
                            model.审批流程单位列表.Add(a);
                        }
                        if (!string.IsNullOrWhiteSpace(topuser) && topuser != "-1")
                        {
                            用户链接<单位用户> a = new 用户链接<单位用户>();
                            a.用户ID = long.Parse(topuser);
                            model.审批流程单位列表.Add(a);
                        }
                        model.当前处理单位链接.用户ID = model.审批流程单位列表.First().用户ID;
                        需求计划管理.更新需求计划(model);
                        return Redirect("/单位用户后台/Demandlist?page=1");
                    }
                    else
                    {
                        return Content("<script>alert('请选择审核单位！');window.location='/单位用户后台/demandlist?page=1';</script>");
                    }
                }
            }
            catch
            {
                return Redirect("/单位用户后台/Demandlist?page=1");
            }
        }
        [HttpPost]
        public ActionResult Task_addChecker(需求采购任务 m)//提交道审核单位
        {
            try
            {
                string strid = Request.Form["uid"];
                string topuser = Request.Form["topuser"];
                需求采购任务 model = 需求采购任务管理.查找需求采购任务(m.Id);
                model.联系电话 = m.联系电话;
                model.联系人 = m.联系人;
                model.建议完成时间 = m.建议完成时间;
                model.受理单位.用户ID = m.受理单位.用户ID;
                model.采购方式 = m.采购方式;
                List<用户链接<单位用户>> ulist = new List<用户链接<单位用户>>();
                if (model.审核历史列表.Any() && model.审核历史列表.Last().审核状态 == 审核状态.审核未通过)
                {
                    model.审核历史列表 = new List<需求计划._审核数据>();
                }
                if (!string.IsNullOrWhiteSpace(strid))
                {
                    for (int i = 0; i < strid.Split(',').Length - 1; i++)
                    {
                        用户链接<单位用户> a = new 用户链接<单位用户>();
                        a.用户ID = long.Parse(strid.Split(',')[i]);
                        ulist.Add(a);
                    }
                    if (!string.IsNullOrWhiteSpace(topuser) && topuser != "-1")
                    {
                        用户链接<单位用户> a = new 用户链接<单位用户>();
                        a.用户ID = long.Parse(topuser);
                        ulist.Add(a);
                    }
                    model.审批流程单位列表 = ulist;
                    model.当前处理单位链接.用户ID = ulist.First().用户ID;
                    需求采购任务管理.更新需求采购任务(model);
                    return Redirect("/单位用户后台/AssignmentTaskAuditList");
                }
                else
                {
                    return Content("<script>alert('请选择审核单位！');window.location='/单位用户后台/demandlist?page=1';</script>");
                }
            }
            catch
            {
                return Redirect("/单位用户后台/Demandlist?page=1");
            }
        }
        public bool ExistChecker()
        {
            long tid = long.Parse(Request.QueryString["tid"]);
            long uid = long.Parse(Request.QueryString["id"]);
            需求采购任务 model = 需求采购任务管理.查找需求采购任务(tid);
            用户链接<单位用户> u = new 用户链接<单位用户>();
            u.用户ID = uid;
            if (model.审批流程单位列表.Contains(u))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        [单一权限验证(权限.我的需求计划列表)]
        public ActionResult Demandlist()
        {
            try
            {
                IEnumerable<单位用户> user = 用户管理.查询用户<单位用户>(0, 0);
                ViewData["user"] = user;
                var t = typeof(采购方式);
                var way = Enum.GetValues(t);
                Dictionary<int, string> ways = new Dictionary<int, string>();
                foreach (var item in way)
                {
                    ways.Add((int)item, Enum.GetName(t, item));
                }
                IEnumerable<需求计划> model = 需求计划管理.查询需求计划(0, 0, Query<需求计划>.Where(o => o.需求发起单位链接.用户ID == currentUser.Id));
                ViewData["u"] = currentUser.联系方式.联系人;
                if (currentUser.用户组.Contains("需求合并"))
                {
                    ViewData["合并需求"] = 1;
                }
                else
                {
                    ViewData["合并需求"] = 0;
                }
                ViewData["采购方式"] = ways;

                return View(model);
            }
            catch
            {
                return Redirect("/单位用户后台/demandlist");
            }
        }
        [单一权限验证(权限.审核需求计划)]
        public ActionResult DemandCheck()
        {
            try
            {
                var t = typeof(秘密等级);
                var vs = Enum.GetValues(t);
                var d = new Dictionary<string, int>();
                foreach (var v in vs)
                {
                    d.Add(Enum.GetName(t, v), (int)v);
                }
                ViewData["秘密等级"] = d;

                IEnumerable<单位用户> user = 用户管理.查询用户<单位用户>(0, 0);
                ViewData["user"] = user;
                ViewData["联系人"] = string.IsNullOrWhiteSpace(currentUser.联系方式.联系人) ? "" : currentUser.联系方式.联系人;

                long currentPg = Request.QueryString["page"] == null ? 1 : long.Parse(Request.QueryString["page"]);
                IEnumerable<需求计划> model1 = 需求计划管理.查询需求计划(20 * ((int)currentPg - 1), 20, Query<需求计划>.Where(m => m.当前处理单位链接.用户ID == currentUser.Id && m.并入需求计划链接.需求计划ID == -1)); //&& m.需求发起单位链接.用户ID != currentUser.Id 
                model1 = model1.Where(m => (!m.审核历史列表.Any() || (m.审核历史列表.Any() && m.审核历史列表.Last().审核状态 != 审核状态.审核未通过)) && (currentUser.用户组.Contains("需求合并") || m.需求发起单位链接.用户ID != currentUser.Id) && !m.流程已完成);
                long sum = model1.Count();
                long pageCount = sum / 20;
                if (sum % 20 > 0)
                {
                    pageCount++;
                }
                ViewData["Pagecount"] = pageCount;
                ViewData["CurrentPage"] = currentPg;
                ViewData["MergePermissions"] = currentUser.用户组.Contains("需求合并") ? "1" : "0";
                return View(model1);
            }
            catch
            {
                return Redirect("/单位用户后台/demandlist");
            }
        }
        public string getMaterial()
        {
            string name = "还有以下物资没有通过：";
            string strId = Request.QueryString["str"];
            List<long> id = new List<long>();
            for (int i = 0; i < strId.Split('|').Length - 1; i++)
            {
                id.Add(long.Parse(strId.Split('|')[i].Split(',')[0]));
            }
            foreach (var item in id)
            {
                name += 需求计划物资管理.查找需求计划物资(item).物资名称 + ",";
            }
            return name;
        }
        public ActionResult DeleteMaterial()
        {
            try
            {
                long id = long.Parse(Request.QueryString["id"]);
                需求计划物资 model = 需求计划物资管理.查找需求计划物资(id);
                需求计划物资管理.删除需求计划物资(id);
                需求计划 plan = 需求计划管理.查找需求计划(model.所属需求计划.需求计划ID);
                plan.物资列表.Remove(plan.物资列表.Find(o => o.需求计划物资ID == id));
                return 需求计划管理.更新需求计划(plan) ? Content("<script>window.location='/单位用户后台/demandlist';</script>") : Content("<script>alert('删除失败！');window.location='/单位用户后台/demandlist';</script>");
            }
            catch
            {
                return Content("<script>alert('删除失败！');window.location='/单位用户后台/demandlist';</script>");
            }
        }
        [HttpPost]
        public ActionResult Modify_Plan(需求计划 model)
        {
            try
            {
                需求计划 m = 需求计划管理.查找需求计划(model.Id);
                m.编制单位 = model.编制单位;
                m.采购年度 = new DateTime(int.Parse(Request.Form["采购年度"]), 1, 1);
                m.承办部门 = model.承办部门;
                m.建议完成时间 = model.建议完成时间;
                m.联系电话 = model.联系电话;
                m.联系人 = model.联系人;
                m.秘密等级 = model.秘密等级;
                m.需求计划标题 = model.需求计划标题;
                需求计划管理.更新需求计划(m);
                return Content("<script>alert('修改成功！');</script>");
            }
            catch
            {
                return Content("<script>alert('修改需求计划信息失败！');</script>");
            }
        }
        [HttpPost]
        public ActionResult ModifyMaterial()
        {
            try
            {
                long id = long.Parse(Request.Form["id"]);
                需求计划 model = 需求计划管理.查找需求计划(id);
                NameValueCollection coll = Request.Form;
                List<int> hlist = new List<int>();
                if (coll.Count != 0)
                {
                    int sum = coll.Count / 10;
                    for (var j = 0; j < sum; j++)
                    {
                        if (!string.IsNullOrWhiteSpace(Request.Form["物资名称" + j].ToString().Split('|')[0]))
                        {
                            long mid = long.Parse(Request.Form["物资名称" + j].ToString().Split('|')[0]);
                            需求计划物资 m = 需求计划物资管理.查找需求计划物资(mid);
                            m.物资名称 = Request.Form["物资名称" + j].Split('|')[1];
                            m.规格型号 = Request.Form["规格型号" + j];
                            m.计量单位 = Request.Form["计量单位" + j];
                            m.单价 = decimal.Parse(Request.Form["单价" + j]);
                            m.数量 = decimal.Parse(Request.Form["数量" + j]);
                            m.预算金额 = decimal.Parse(Request.Form["预算金额" + j]);
                            m.建议采购方式 = Request.Form["建议采购方式" + j];
                            m.技术指标 = Request.Form["技术指标" + j];
                            m.备注 = Request.Form["备注" + j];
                            m.交货期限 = DateTime.Parse(Request.Form["交货期限" + j]);
                            需求计划物资管理.更新需求计划物资(m);
                        }
                        else
                        {
                            需求计划物资 m = new 需求计划物资();
                            需求计划物资链接 ml = new 需求计划物资链接();
                            m.物资名称 = Request.Form["物资名称" + j].Split('|')[1];
                            m.规格型号 = Request.Form["规格型号" + j];
                            m.计量单位 = Request.Form["计量单位" + j];
                            m.单价 = decimal.Parse(Request.Form["单价" + j]);
                            m.数量 = decimal.Parse(Request.Form["数量" + j]);
                            m.预算金额 = decimal.Parse(Request.Form["预算金额" + j]);
                            m.建议采购方式 = Request.Form["建议采购方式" + j];
                            m.技术指标 = Request.Form["技术指标" + j];
                            m.备注 = Request.Form["备注" + j];
                            m.交货期限 = DateTime.Parse(Request.Form["交货期限" + j]);
                            需求计划物资管理.添加需求计划物资(m);
                            ml.需求计划物资ID = m.Id;
                            model.物资列表.Add(ml);
                            需求计划管理.更新需求计划(model);
                        }
                    }
                }
                return Content("<script>alert('修改成功！');window.parent.location.reload();</script>");
            }
            catch
            {
                return Redirect("/单位用户后台/demandlist?page=1");
            }
        }

        public ActionResult Part_Home()
        {
            IEnumerable<站内消息> Msg = 站内消息管理.查询收信人的站内消息(0, 0, currentUser.Id);
            int count = 0;
            foreach (var item in Msg)
            {
                if (item.基本数据.添加时间 > item.收信人.上次阅读时间)
                {
                    count++;
                }
            }
            ViewData["msg"] = 公告管理.计数公告(0, 0, Query<公告>.Where(o => o.审核数据.审核状态 == 审核状态.未审核 && (o.审核数据.审核者.用户ID == currentUser.Id || o.审核数据2.审核者.用户ID == currentUser.Id)));
            ViewData["ysd"] = 验收单管理.计数验收单(0, 0, Query<验收单>.Where(o => o.管理单位审核人.用户ID == currentUser.Id && o.审核数据.审核状态 == 审核状态.未审核 && !o.是否作废));
            ViewData["UnitUser"] = 用户管理.计数用户<单位用户>(0, 0, Query<单位用户>.Where(o => o.审核数据.审核者.用户ID == currentUser.Id && o.审核数据.审核状态 == 审核状态.未审核));
            ViewData["msg_count"] = count;

            var model1 = 需求计划管理.查询需求计划(0, 0, Query<需求计划>.Where(o => o.当前处理单位链接.用户ID == currentUser.Id && o.并入需求计划链接.需求计划ID == -1)); //&& m.需求发起单位链接.用户ID != currentUser.Id 
            model1 = model1.Where(o => (!o.审核历史列表.Any() || (o.审核历史列表.Any() && o.审核历史列表.Last().审核状态 != 审核状态.审核未通过)) && (currentUser.用户组.Contains("需求合并") || o.需求发起单位链接.用户ID != currentUser.Id) && !o.流程已完成);
            ViewData["Demand"] = model1.Count();

            var m = currentUser;
            if (currentUser.Id == 10)
            {
                m = 用户管理.查找用户<单位用户>(currentUser.Id);
            }

            return PartialView("Procure_Part/Part_Home", m);
        }
        [HttpPost]
        public ActionResult ModifyDisitribution()
        {
            try
            {
                long id = long.Parse(Request.Form["id"]);
                需求计划 model = 需求计划管理.查找需求计划(id);
                NameValueCollection coll = Request.Form;
                if (coll.Count != 0)
                {
                    int sum = coll.Count / 9;

                    for (int i = 0; i < sum; i++)
                    {
                        if (!string.IsNullOrWhiteSpace(Request.Form["物资名称" + i].Split('|')[0]))
                        {
                            long did = long.Parse(Request.Form["物资名称" + i].Split('|')[0]);
                            需求计划分发 m = 需求计划分发管理.查找需求计划分发(did);
                            m.备注 = Request.Form["备注" + i];
                            m.物资名称 = Request.Form["物资名称" + i].Split('|')[1];
                            m.收货单位名称 = Request.Form["收货单位名称" + i];
                            m.规格型号 = Request.Form["规格型号" + i];
                            m.分配数量 = decimal.Parse(Request.Form["分配数量" + i]);
                            m.提货方式 = (提货方式)Enum.Parse(typeof(提货方式), Request.Form["提货方式" + i]);
                            m.运输方式 = (运输方式)Enum.Parse(typeof(运输方式), Request.Form["运输方式" + i]);
                            m.到站 = Request.Form["到站" + i];
                            m.计量单位 = Request.Form["计量单位" + i];
                            需求计划分发管理.更新需求计划分发(m);
                        }
                        else
                        {
                            需求计划分发 m = new 需求计划分发();
                            需求计划分发链接 dl = new 需求计划分发链接();
                            m.备注 = Request.Form["备注" + i];
                            m.物资名称 = Request.Form["物资名称" + i].Split('|')[1];
                            m.收货单位名称 = Request.Form["收货单位名称" + i];
                            m.规格型号 = Request.Form["规格型号" + i];
                            m.分配数量 = decimal.Parse(Request.Form["分配数量" + i]);
                            m.提货方式 = (提货方式)Enum.Parse(typeof(提货方式), Request.Form["提货方式" + i]);
                            m.运输方式 = (运输方式)Enum.Parse(typeof(运输方式), Request.Form["运输方式" + i]);
                            m.到站 = Request.Form["到站" + i];
                            m.计量单位 = Request.Form["计量单位" + i];
                            m.所属需求计划.需求计划ID = id;
                            需求计划分发管理.添加需求计划分发(m);
                            dl.需求计划分发ID = m.Id;
                            model.分发列表.Add(dl);
                            需求计划管理.更新需求计划(model);
                        }
                    }
                }
                return Content("<script>alert('修改成功！');window.parent.location.reload();</script>");
            }
            catch
            {
                return Redirect("/单位用户后台/demandlist?page=1");
            }
        }
        [HttpPost]
        public ActionResult AddDemand(需求计划 model)
        {
            try
            {
                model.需求发起单位链接.用户ID = currentUser.Id;
                model.需求计划主标题 = "军队物资集中采购需求计划";
                model.采购年度 = new DateTime(int.Parse(Request.Form["采购年度"]), 1, 1);
                model.提报日期 = DateTime.Now;
                model.当前处理单位链接.用户ID = currentUser.Id;
                return 需求计划管理.添加需求计划(model) ? Content("<script>window.parent.document.getElementById(\'demandId\').value=" + model.Id.ToString() + ";window.parent.document.getElementById(\'demandId1\').value=" + model.Id.ToString() + ";</script>") : Content("<script>alert('添加需求计划失败！');window.location='/单位用户后台/demandlist';</script>");
            }
            catch
            {
                return Content("<script>alert('添加需求计划失败！');window.location='/单位用户后台/demandlist';</script>");
            }
        }
        [HttpPost]
        public ActionResult SubDemand()
        {
            try
            {
                long id = long.Parse(Request.Form["id"]);
                long uid = -1;
                if (Request.Form["name"] != "-1")
                {
                    uid = long.Parse(Request.Form["name"]);
                }
                需求计划 m = 需求计划管理.查找需求计划(id);
                if (!m.分发列表.Any() || !m.物资列表.Any())
                {
                    return Content("<script>alert('没有任何分发项或物资，请添加分发项目。');window.location='/单位用户后台/Demandlist?page=1';</script>");
                }
                else
                {

                    if (uid != -1)
                    {
                        if (m.来源需求计划列表.Any())
                        {
                            return Content("<script>alert('合并需求不能再次提交，您可以提交新合并的需求！');window.location='/单位用户后台/demandlist';</script>");
                        }
                        m.审核历史列表 = new List<需求计划._审核数据>();
                        m.当前处理单位链接.用户ID = uid;
                        m.并入需求计划链接.需求计划ID = -1;
                        return 需求计划管理.更新需求计划(m) ? Content("<script>window.location='/单位用户后台/demandlist';</script>") : Content("<script>alert('提交需求计划失败！');window.location='/单位用户后台/demandlist';</script>");
                    }
                    else
                    {
                        return Content("<script>alert('请选择审核单位！');window.location='/单位用户后台/demandlist';</script>");
                    }
                }
            }
            catch
            {
                return Content("<script>alert('提交需求计划失败！');window.location='/单位用户后台/demandlist';</script>");
            }
        }
        public ActionResult Demand_Detail()
        {
            try
            {
                long id = long.Parse(Request.QueryString["id"]);
                var t = typeof(采购方式);
                var way = Enum.GetValues(t);
                Dictionary<int, string> ways = new Dictionary<int, string>();
                foreach (var item in way)
                {
                    ways.Add((int)item, Enum.GetName(t, item));
                }
                ViewData["采购方式"] = ways;

                var tr = typeof(运输方式);
                var trans = Enum.GetValues(tr);
                Dictionary<int, string> transportation = new Dictionary<int, string>();
                foreach (var item in trans)
                {
                    transportation.Add((int)item, Enum.GetName(tr, item));
                }

                var th = typeof(提货方式);
                var tihuo = Enum.GetValues(th);
                Dictionary<int, string> tihuos = new Dictionary<int, string>();
                foreach (var item in trans)
                {
                    tihuos.Add((int)item, Enum.GetName(th, item));
                }
                var secure = typeof(秘密等级);
                var sec = Enum.GetValues(secure);
                Dictionary<int, string> security = new Dictionary<int, string>();
                foreach (var item in sec)
                {
                    security.Add((int)item, Enum.GetName(secure, item));
                }
                ViewData["运输方式"] = transportation;
                ViewData["提货方式"] = tihuos;
                ViewData["秘密等级"] = security;
                需求计划 model = 需求计划管理.查找需求计划(id);
                if (model == null)
                {
                    return Redirect("/单位用户后台/Demandlist?page=1");
                }
                return View(model);
            }
            catch
            {
                return Redirect("/单位用户后台/Demandlist?page=1");
            }
        }
        [HttpPost]
        public ActionResult AddMaterial()
        {
            try
            {
                long id = long.Parse(Request.Form["id"]);
                需求计划 model = 需求计划管理.查找需求计划(id);
                List<需求计划物资链接> mlist = new List<需求计划物资链接>();
                NameValueCollection coll = Request.Form;
                if (coll.Count != 0)
                {
                    int sum = coll.Count / 10;
                    for (int i = 0; i < sum; i++)
                    {
                        需求计划物资 m = new 需求计划物资();
                        需求计划物资链接 n = new 需求计划物资链接();
                        m.备注 = Request.Form["备注" + i];
                        m.单价 = decimal.Parse(Request.Form["单价" + i]);
                        m.规格型号 = Request.Form["规格型号" + i];
                        m.计量单位 = Request.Form["计量单位" + i];
                        m.技术指标 = Request.Form["技术指标" + i];
                        m.建议采购方式 = Request.Form["建议采购方式" + i];
                        m.交货期限 = DateTime.Parse(Request.Form["交货期限" + i]);
                        m.数量 = int.Parse(Request.Form["数量" + i]);
                        m.物资名称 = Request.Form["物资名称" + i];
                        m.预算金额 = decimal.Parse(Request.Form["预算金额" + i]);
                        m.所属需求计划.需求计划ID = id;
                        需求计划物资管理.添加需求计划物资(m);
                        n.需求计划物资ID = m.Id;
                        mlist.Add(n);
                    }
                    model.物资列表 = mlist;
                }
                需求计划管理.更新需求计划(model);
                return Content("<script>window.parent.document.getElementById(\'demandId1\').value=" + model.Id.ToString() + ";window.parent.document.getElementById(\'demandId\').value=" + model.Id.ToString() + ";window.parent.document.forms['distribute'].submit();</script>");
            }
            catch
            {
                return Redirect("/单位用户后台/demandlist?page=1");
            }
        }

        [单一权限验证(权限.编制需求计划)]
        public ActionResult Add_Demand()
        {
            try
            {
                var t = typeof(采购方式);
                var way = Enum.GetValues(t);
                Dictionary<int, string> ways = new Dictionary<int, string>();
                foreach (var item in way)
                {
                    ways.Add((int)item, Enum.GetName(t, item));
                }
                ViewData["采购方式"] = ways;

                var tr = typeof(运输方式);
                var trans = Enum.GetValues(tr);
                Dictionary<int, string> transportation = new Dictionary<int, string>();
                foreach (var item in trans)
                {
                    transportation.Add((int)item, Enum.GetName(tr, item));
                }

                var th = typeof(提货方式);
                var tihuo = Enum.GetValues(th);
                Dictionary<int, string> tihuos = new Dictionary<int, string>();
                foreach (var item in trans)
                {
                    tihuos.Add((int)item, Enum.GetName(th, item));
                }
                ViewData["unit"] = currentUser.单位信息.单位名称;
                ViewData["运输方式"] = transportation;
                ViewData["提货方式"] = tihuos;
                ViewData["u"] = currentUser.联系方式.联系人;
                return View();
            }
            catch
            {
                return Redirect("/单位用户后台/demandlist?page=1");
            }
        }
        public ActionResult TaskPassOut()
        {
            try
            {
                string reason = Request.Form["reason"];
                long id = long.Parse(Request.Form["id"]);
                需求采购任务 model = 需求采购任务管理.查找需求采购任务(id);
                需求计划._审核数据 m = new 需求计划._审核数据();
                m.审核者.用户ID = currentUser.Id;
                m.审核时间 = DateTime.Now;
                m.审核状态 = 审核状态.审核未通过;
                m.审核不通过原因 = reason;
                model.审核历史列表.Add(m);
                model.当前处理单位链接.用户ID = model.需求发起单位链接.用户ID;
                需求采购任务管理.更新需求采购任务(model);
                return Redirect("/单位用户后台/AssignmentTaskAuditList");
            }
            catch
            {
                return Redirect("/单位用户后台/AssignmentTaskAuditList");
            }
        }
        [HttpPost]
        public ActionResult AddDisitribution()
        {
            try
            {
                long id = long.Parse(Request.Form["id"]);
                需求计划 m = 需求计划管理.查找需求计划(id);
                List<需求计划分发链接> dlist = new List<需求计划分发链接>();
                NameValueCollection coll = Request.Form;
                int sum = coll.Count / 9;
                for (int i = 0; i < sum; i++)
                {
                    需求计划分发 dis = new 需求计划分发();
                    dis.备注 = Request.Form["备注" + i];
                    dis.到站 = Request.Form["到站" + i];
                    dis.分配数量 = decimal.Parse(Request.Form["分配数量" + i]);
                    dis.规格型号 = Request.Form["规格型号" + i];
                    dis.计量单位 = Request.Form["计量单位" + i];
                    dis.收货单位名称 = Request.Form["收货单位名称" + i];
                    dis.提货方式 = (提货方式)Enum.Parse(typeof(提货方式), (string)Request.Form["提货方式" + i]);
                    dis.物资名称 = Request.Form["物资名称" + i];
                    dis.运输方式 = (运输方式)Enum.Parse(typeof(运输方式), Request.Form["运输方式" + i].ToString());
                    dis.所属需求计划.需求计划ID = id;
                    需求计划分发管理.添加需求计划分发(dis);
                    需求计划分发链接 l = new 需求计划分发链接();
                    l.需求计划分发ID = dis.Id;
                    dlist.Add(l);
                }
                m.分发列表 = dlist;
                需求计划管理.更新需求计划(m);
                return RedirectToAction("Demandlist");
            }
            catch
            {
                return Content("<script>alert('添加失败！');window.location='/单位用户后台/demandlist';</script>");
            }

        }
        public string updateDemand()
        {
            try
            {
                long id = long.Parse(Request.QueryString["id"]);
                需求计划 model = 需求计划管理.查找需求计划(id);
                model.流程已完成 = true;
                需求计划管理.更新需求计划(model);
                if (model.并入需求计划链接.需求计划ID == -1)
                {
                    return "-1";
                }
                else
                {
                    return "1";
                }
            }
            catch
            {
                return "0";
            }
        }
        public class Demand
        {
            public long Id { get; set; }
            public long sum { get; set; }//判断能否删除和修改，未1不能删除和修改，否则可以删除和修改
            public string title { get; set; }
            public string time { get; set; }
            public int count { get; set; }
            public string reason { get; set; }
            public string status { get; set; }
            public bool lastId { get; set; }
            public bool finish { get; set; }
            public string contactMan { get; set; }
            public string phone { get; set; }
            public string cgyear { get; set; }
            public int security { get; set; }
            public string depart { get; set; }
            public string bzdw { get; set; }
        }
        public ActionResult DemandItems()
        {
            List<Demand> ds = new List<Demand>();
            int PageCount = 0;
            int CurrentPage = int.Parse(Request.QueryString["p"]);
            string type = Request.QueryString["type"];//2:未提交，3：提交待审核，4：审核通过，5：审核未通过
            IEnumerable<需求计划> d = null;
            switch (type)
            {
                case "1": d = 需求计划管理.查询需求计划(20 * (CurrentPage - 1), 20);
                    d = d.Where(o => o.需求发起单位链接.用户ID == currentUser.Id && !o.来源需求计划列表.Any()); break;
                case "2": d = 需求计划管理.查询需求计划(20 * (CurrentPage - 1), 20);
                    d = d.Where(o => o.需求发起单位链接.用户ID == currentUser.Id && o.当前处理单位链接.用户ID == o.需求发起单位链接.用户ID && o.审核历史列表.Count == 0 && o.审批流程单位列表.Count == 0);
                    break;
                case "3": d = 需求计划管理.查询需求计划(20 * (CurrentPage - 1), 20);
                    d = d.Where(o => (o.审核历史列表.Any() && o.审核历史列表.Last().审核状态 != 审核状态.审核未通过 && o.审批流程单位列表.Count > o.审核历史列表.Count && o.需求发起单位链接.用户ID == currentUser.Id && !o.来源需求计划列表.Any()));
                    break;
                case "4": d = 需求计划管理.查询需求计划(20 * (CurrentPage - 1), 20);
                    d = d.Where(o => o.审核历史列表.Any() && o.审核历史列表.Last().审核状态 == 审核状态.审核通过 && o.审批流程单位列表.Count <= o.审核历史列表.Count && !o.来源需求计划列表.Any() && (o.需求发起单位链接.用户ID == currentUser.Id || currentUser.Id == 10003));
                    break;
                case "5": d = 需求计划管理.查询需求计划(20 * (CurrentPage - 1), 20);
                    d = d.Where(o => o.审核历史列表.Any() && o.审核历史列表.Last().审核状态 == 审核状态.审核未通过 && o.需求发起单位链接.用户ID == currentUser.Id && !o.来源需求计划列表.Any());
                    break;
                case "6": d = 需求计划管理.查询需求计划(20 * (CurrentPage - 1), 20);
                    d = d.Where(o => !o.审核历史列表.Any() && (o.审批流程单位列表.Any() || o.需求发起单位链接.用户ID != o.当前处理单位链接.用户ID) && o.需求发起单位链接.用户ID == currentUser.Id && !o.来源需求计划列表.Any());
                    break;
            }
            if (d != null)
            {
                PageCount = d.Count() / 20;
                if (d.Count() % 20 > 0)
                {
                    PageCount++;
                }
                foreach (var item in d)
                {
                    Demand m = new Demand();
                    m.Id = item.Id;
                    m.title = item.需求计划标题;
                    m.time = item.建议完成时间.ToString("yyyy/MM/dd");
                    m.count = item.来源需求计划列表.Count;
                    if (string.IsNullOrWhiteSpace(item.联系人))
                    {
                        m.contactMan = "";
                    }
                    else
                    {
                        m.contactMan = item.联系人;
                    }
                    if (string.IsNullOrWhiteSpace(item.联系电话))
                    {
                        m.phone = "";
                    }
                    else
                    {
                        m.phone = item.联系电话;
                    }
                    m.lastId = false;
                    if ((item.并入需求计划链接.需求计划ID == -1 || item.并入需求计划链接.需求计划ID != -1) && item.审核历史列表.Any() && item.审核历史列表.Last().审核状态 == 审核状态.审核通过 && item.审批流程单位列表.Count <= item.审核历史列表.Count)
                    {
                        m.lastId = true;
                        m.finish = false;
                        if (item.流程已完成)
                        {
                            m.finish = true;
                        }
                    }
                    if (item.审核历史列表.Any() && item.审批流程单位列表.Any() && item.审核历史列表.Count < item.审批流程单位列表.Count && item.审核历史列表.Last().审核状态 != 审核状态.审核未通过)
                    {
                        m.status = "审核中";
                        m.sum = 1;
                    }
                    else if (item.审核历史列表.Any() && item.审核历史列表.Count >= item.审批流程单位列表.Count && item.审核历史列表.Last().审核状态 == 审核状态.审核通过)
                    {
                        m.lastId = true;
                        m.status = "审核通过";
                    }
                    else if (item.需求发起单位链接.用户ID == item.当前处理单位链接.用户ID && !item.审批流程单位列表.Any() && !item.审核历史列表.Any())
                    {
                        m.status = "未提交";
                        m.sum = 0;
                    }
                    else if (!item.审核历史列表.Any() && item.需求发起单位链接 != item.当前处理单位链接)
                    {
                        m.status = "待审核";
                        m.sum = 1;
                    }
                    else if (item.审核历史列表.Any() && item.审核历史列表.Last().审核状态 == 审核状态.审核未通过)
                    {
                        m.reason = item.审核历史列表.Last().审核不通过原因;
                        m.status = "审核未通过";
                        m.sum = 1;
                    }
                    ds.Add(m);
                }
            }
            JsonResult json = new JsonResult() { Data = new { items = ds, Pcount = PageCount, Cpage = CurrentPage } };
            return Json(json, JsonRequestBehavior.AllowGet);
        }
        public ActionResult DemandItems1()
        {
            List<Demand> ds = new List<Demand>();
            int PageCount = 0;
            int CurrentPage = int.Parse(Request.QueryString["p"]);
            string type = Request.QueryString["type"];//2:未提交，3：审核中，4：审核通过，5：审核未通过
            IEnumerable<需求计划> d = null;
            switch (type)
            {
                case "1": d = 需求计划管理.查询需求计划(20 * (CurrentPage - 1), 20);
                    d = d.Where(o => o.需求发起单位链接.用户ID == currentUser.Id && o.来源需求计划列表.Any()); break;
                case "2": d = 需求计划管理.查询需求计划(20 * (CurrentPage - 1), 20);
                    d = d.Where(o => o.审核历史列表.Count == 0 && o.当前处理单位链接.用户ID == o.需求发起单位链接.用户ID && o.需求发起单位链接.用户ID == currentUser.Id && o.来源需求计划列表.Any());
                    break;
                case "3": d = 需求计划管理.查询需求计划(20 * (CurrentPage - 1), 20);
                    d = d.Where(o => (o.审核历史列表.Any() && o.审核历史列表.Last().审核状态 != 审核状态.审核未通过 && o.审批流程单位列表.Count > o.审核历史列表.Count && o.需求发起单位链接.用户ID == currentUser.Id && o.来源需求计划列表.Any()));
                    break;
                case "4": d = 需求计划管理.查询需求计划(20 * (CurrentPage - 1), 20);
                    d = d.Where(o => o.审核历史列表.Any() && o.审核历史列表.Count == o.审批流程单位列表.Count && o.审核历史列表.Last().审核状态 == 审核状态.审核通过 && o.来源需求计划列表.Any() && (o.需求发起单位链接.用户ID == currentUser.Id || currentUser.Id == 10003));
                    break;
                case "5": d = 需求计划管理.查询需求计划(20 * (CurrentPage - 1), 20);
                    d = d.Where(o => o.审核历史列表.Any() && o.审核历史列表.Last().审核状态 == 审核状态.审核未通过 && o.需求发起单位链接.用户ID == currentUser.Id && o.来源需求计划列表.Any());
                    break;
                case "6": d = 需求计划管理.查询需求计划(20 * (CurrentPage - 1), 20);
                    d = d.Where(o => !o.审核历史列表.Any() && (o.审批流程单位列表.Any() || o.需求发起单位链接.用户ID != o.当前处理单位链接.用户ID) && o.需求发起单位链接.用户ID == currentUser.Id && o.来源需求计划列表.Any());
                    break;
            }
            if (d != null)
            {
                PageCount = d.Count() / 20;
                if (d.Count() % 20 > 0)
                {
                    PageCount++;
                }
                foreach (var item in d)
                {
                    Demand m = new Demand();
                    m.Id = item.Id;
                    m.title = item.需求计划标题;
                    m.time = item.建议完成时间.ToString("yyyy/MM/dd");
                    m.count = item.来源需求计划列表.Count;
                    if (string.IsNullOrWhiteSpace(item.联系人))
                    {
                        m.contactMan = "";
                    }
                    else
                    {
                        m.contactMan = item.联系人;
                    }
                    if (string.IsNullOrWhiteSpace(item.联系电话))
                    {
                        m.phone = "";
                    }
                    else
                    {
                        m.phone = item.联系电话;
                    }
                    m.lastId = false;
                    if ((item.并入需求计划链接.需求计划ID == -1 || item.并入需求计划链接.需求计划ID != -1) && item.审核历史列表.Any() && item.审核历史列表.Last().审核状态 == 审核状态.审核通过 && item.审批流程单位列表.Count == item.审核历史列表.Count)
                    {
                        m.lastId = true;
                        m.finish = false;
                        if (item.流程已完成)
                        {
                            m.finish = true;
                        }
                    }
                    if (item.审核历史列表.Any() && item.审批流程单位列表.Any() && item.审核历史列表.Count < item.审批流程单位列表.Count && item.审核历史列表.Last().审核状态 != 审核状态.审核未通过)
                    {
                        m.status = "审核中";
                        m.sum = 1;
                    }
                    else if (item.审核历史列表.Any() && item.审核历史列表.Count >= item.审批流程单位列表.Count && item.审核历史列表.Last().审核状态 == 审核状态.审核通过)
                    {
                        m.status = "审核通过";
                        m.sum = 1;
                    }
                    else if (!item.审核历史列表.Any() && item.审批流程单位列表.Any())
                    {
                        m.status = "待审核";
                        m.sum = 1;
                    }
                    else if (item.审核历史列表.Any() && item.审核历史列表.Last().审核状态 == 审核状态.审核未通过)
                    {
                        m.status = "审核未通过";
                        m.reason = item.审核历史列表.Last().审核不通过原因;
                        m.sum = 0;
                    }
                    else if (item.需求发起单位链接 == item.当前处理单位链接)
                    {
                        m.status = "未提交";
                        m.sum = 0;
                    }
                    ds.Add(m);
                }
            }
            JsonResult json = new JsonResult() { Data = new { items = ds, Pcount = PageCount, Cpage = CurrentPage } };
            return Json(json, JsonRequestBehavior.AllowGet);
        }
        public string noPasReason()
        {
            try
            {
                long id = long.Parse(Request.QueryString["id"]);
                需求采购任务 m = 需求采购任务管理.查找需求采购任务(id);
                string reason = m.审核历史列表.Last().审核不通过原因;
                return reason;
            }
            catch
            {
                return "不好意思，出错啦！";
            }
        }
        public ActionResult getChildren()
        {
            List<Demand> ds = new List<Demand>();
            long id = int.Parse(Request.QueryString["id"]);
            需求计划 model = 需求计划管理.查找需求计划(id);
            if (model.来源需求计划列表.Any())
            {
                foreach (var item in model.来源需求计划列表)
                {
                    Demand d = new Demand();
                    d.title = item.需求计划数据.需求计划标题;
                    d.contactMan = item.需求计划数据.联系人;
                    d.phone = item.需求计划数据.联系电话;
                    d.status = item.需求计划数据.审核历史列表.Last().审核状态.ToString();
                    d.time = item.需求计划数据.建议完成时间.ToString("yyyy/MM/dd");
                    d.cgyear = item.需求计划数据.采购年度.ToString("yyyy/MM/dd");
                    d.security = (int)item.需求计划数据.秘密等级;
                    d.depart = item.需求计划数据.承办部门;
                    d.bzdw = item.需求计划数据.编制单位;
                    ds.Add(d);
                }
            }
            else
            {
                Demand d = new Demand();
                d.title = model.需求计划标题;
                d.contactMan = model.联系人;
                d.phone = model.联系电话;
                d.time = model.建议完成时间.ToString("yyyy/MM/dd");
                d.cgyear = model.采购年度.ToString("yyyy/MM/dd");
                d.security = (int)model.秘密等级;
                d.depart = model.承办部门;
                d.bzdw = model.编制单位;
                ds.Add(d);
            }
            JsonResult json = new JsonResult() { Data = new { items = ds } };
            return Json(json, JsonRequestBehavior.AllowGet);
        }
        public class distribution
        {
            public string reciever { get; set; }
            public string number { get; set; }
            public string material { get; set; }
            public string cator { get; set; }
            public string percent { get; set; }
            public string discount { get; set; }
            public string getway { get; set; }
            public string transway { get; set; }
            public string destination { get; set; }
            public string note { get; set; }
        }
        public int deletePlan()
        {
            try
            {
                List<long> ids = new List<long>();
                long id = long.Parse(Request.QueryString["id"]);
                需求计划 model = 需求计划管理.查找需求计划(id);
                foreach (var item in model.分发列表)
                {
                    if (!ids.Contains(item.需求计划分发ID))
                    {
                        需求计划分发管理.删除需求计划分发(item.需求计划分发ID);
                        ids.Add(item.需求计划分发ID);
                    }
                }
                foreach (var item in model.物资列表)
                {
                    if (!ids.Contains(item.需求计划物资ID))
                    {
                        需求计划物资管理.删除需求计划物资(item.需求计划物资ID);
                        ids.Add(item.需求计划物资ID);
                    }
                }
                return 需求计划管理.删除需求计划(id) ? 1 : -1;
            }
            catch
            {
                return -1;
            }
        }
        public int del_M_D()
        {
            try
            {
                long id = long.Parse(Request.QueryString["id"]);
                string name = Request.QueryString["s"];
                if (name == "material")
                {
                    需求计划物资 m = 需求计划物资管理.查找需求计划物资(id);
                    需求计划 model = 需求计划管理.查找需求计划(m.所属需求计划.需求计划ID);
                    model.物资列表.Remove(model.物资列表.Find(o => o.需求计划物资ID == id));
                    需求计划管理.更新需求计划(model);
                    需求计划物资管理.删除需求计划物资(id);
                    return 1;
                }
                else
                {
                    需求计划分发 m = 需求计划分发管理.查找需求计划分发(id);
                    需求计划 model = 需求计划管理.查找需求计划(m.所属需求计划.需求计划ID);
                    model.分发列表.Remove(model.分发列表.Find(o => o.需求计划分发ID == id));
                    需求计划管理.更新需求计划(model);
                    需求计划分发管理.删除需求计划分发(id);
                    return 1;
                }
            }
            catch
            {
                return -1;
            }
        }
        public ActionResult disItems()
        {
            List<distribution> dist = new List<distribution>();
            long id = long.Parse(Request.QueryString["id"]);
            需求计划 model = 需求计划管理.查找需求计划(id);
            List<需求计划分发链接> dis = model.分发列表;
            if (dis.Any())
            {
                foreach (var item in dis)
                {
                    distribution d = new distribution();
                    d.reciever = item.需求计划分发数据.收货单位名称;
                    d.number = item.需求计划分发数据.分发编号.ToString();
                    d.material = item.需求计划分发数据.物资名称;
                    d.cator = item.需求计划分发数据.规格型号;
                    d.percent = item.需求计划分发数据.计量单位;
                    d.discount = item.需求计划分发数据.分配数量.ToString();
                    d.getway = item.需求计划分发数据.提货方式.ToString();
                    d.transway = item.需求计划分发数据.运输方式.ToString();
                    d.destination = item.需求计划分发数据.到站;
                    d.note = item.需求计划分发数据.备注;
                    dist.Add(d);
                }
            }
            JsonResult json = new JsonResult() { Data = dist };
            return Json(json, JsonRequestBehavior.AllowGet);
        }
        public ActionResult PlanDetail()
        {
            ViewData["userid"] = currentUser.Id;
            try
            {
                int CurrentPage = int.Parse(Request.QueryString["page"]);
                int count = 0;
                long id = long.Parse(Request.QueryString["id"]);
                需求计划 model = 需求计划管理.查找需求计划(id);
                if (model == null)
                {
                    return Redirect("/单位用户后台/Demandlist?page=1");
                }
                else
                {
                    IEnumerable<需求计划链接> ml = model.来源需求计划列表.Where(m => m.需求计划数据.审核历史列表.Any() && m.需求计划数据.审核历史列表.Last().审核者.用户ID != -1 && m.需求计划数据.审核历史列表.Last().审核状态 == 审核状态.审核通过).Skip(20 * (CurrentPage - 1)).Take(20);
                    count = model.来源需求计划列表.Where(m => m.需求计划数据.审核历史列表.Any() && m.需求计划数据.审核历史列表.Last().审核者.用户ID != -1 && m.需求计划数据.审核历史列表.Last().审核状态 == 审核状态.审核通过).Count() / 20;
                    if (model.来源需求计划列表.Where(m => m.需求计划数据.审核历史列表.Any() && m.需求计划数据.审核历史列表.Last().审核者.用户ID != -1 && m.需求计划数据.审核历史列表.Last().审核状态 == 审核状态.审核通过).Count() % 20 > 0)
                    {
                        count++;
                    }
                    ViewData["reason"] = model.审核历史列表.Last().审核不通过原因;
                    ViewData["mlist"] = model.审核历史列表.Last().未通过审批物资列表;
                    ViewData["id"] = id;
                    ViewData["Pagecount"] = count;
                    ViewData["CurrentPage"] = CurrentPage;
                    return View(ml);
                }
            }
            catch
            {
                return Redirect("/单位用户后台/Demandlist?page=1");
            }
        }
        public ActionResult DeleteDistribution()
        {
            try
            {
                long id = long.Parse(Request.QueryString["id"]);
                需求计划分发 model = 需求计划分发管理.查找需求计划分发(id);
                return 需求计划分发管理.删除需求计划分发(id) ? Content("<script>window.location='/单位用户后台/Distribution?id=" + model.所属需求计划.需求计划ID + "&page=1';</script>") : Content("<script>alert('删除失败！');window.location='/单位用户后台/demandlist';</script>");
            }
            catch
            {
                return Content("<script>alert('删除失败！');window.location='/单位用户后台/demandlist';</script>");
            }
        }
        public ActionResult PartExList()
        {
            long listcount = 用户管理.查询用户<专家>(0, 0).Count();
            long maxpagesize = Math.Max((listcount + 10 - 1) / 10, 1);//15，每页显示15条记录

            int page = 1;
            ViewData["currentpage"] = page;
            ViewData["pagecount"] = maxpagesize;

            ViewData["专家列表"] = 用户管理.查询用户<专家>(10 * (page - 1), 10);
            return PartialView("Procure_Part/PartExList");
        }
        //public class ProdureList
        //{
        //    public long id { get; set; }
        //    public string LoginName { get; set; }
        //    public string ShortName { get; set; }
        //    public string Contact { get; set; }
        //    public string ContactPhone { get; set; }
        //    public string Status { get; set; }
        //    public List<string> Group { get; set; }
        //}
        //public ActionResult Search_Department()
        //{
        //    List<ProdureList> produres = new List<ProdureList>();
        //    string id = Request.QueryString["num"].ToString();
        //    string name = Request.QueryString["name"].ToString();
        //    int pro = int.Parse(Request.QueryString["pro"].ToString());
        //    int catorgray = int.Parse(Request.QueryString["catorgray"].ToString());
        //    string page = Request.QueryString["p"].ToString();
        //    if (string.IsNullOrWhiteSpace(page))
        //    {
        //        page = "1";
        //    }
        //    IMongoQuery q = null;
        //    if (!string.IsNullOrWhiteSpace(id))
        //    {
        //        q = q.And(MongoDB.Driver.Builders.Query.EQ("单位信息.单位编码", id));
        //    }
        //    if (!string.IsNullOrWhiteSpace(name))
        //    {
        //        q = q.And(MongoDB.Driver.Builders.Query.EQ("登录信息.登录名", name));
        //    }
        //    if (catorgray != 0)
        //    {
        //        q = q.And(MongoDB.Driver.Builders.Query.EQ("单位信息.单位级别", catorgray));
        //    }
        //    IEnumerable<单位用户> Depart = 用户管理.查询用户<单位用户>(2 * (int.Parse(page) - 1), 2, q);
        //    long Count = 用户管理.查询用户<单位用户>(0, 0, q).Count() / 2;
        //    if ((用户管理.查询用户<单位用户>(0, 0, q).Count() % 2) > 0)
        //    {
        //        Count++;
        //    }
        //    foreach (var item in Depart)
        //    {
        //        ProdureList p = new ProdureList();
        //        p.id = item.Id;
        //        p.LoginName = item.登录信息.登录名;
        //        p.Contact = item.联系方式.联系人;
        //        p.ContactPhone = item.联系方式.手机;
        //        p.ShortName = item.单位信息.单位代号;
        //        p.Status = item.审核数据.审核状态.ToString();
        //        p.Group = item.用户组;
        //        produres.Add(p);
        //    }
        //    JsonResult json = new JsonResult() { Data = new { User = produres, count = Count } };
        //    return Json(json, JsonRequestBehavior.AllowGet);
        //}
        public ActionResult Search_DepartmentByTime()
        {
            string starttime = Request.QueryString["starttime"];
            string endtime = Request.QueryString["endtime"];
            var pagestr = Request.QueryString["page"];

            IMongoQuery q = MongoDB.Driver.Builders.Query.EQ("审核数据.审核状态", 审核状态.审核通过);
            try
            {
                var page = int.Parse(pagestr);
                if (!string.IsNullOrWhiteSpace(starttime))
                {
                    q = q.And(MongoDB.Driver.Builders.Query<用户基本数据>.GTE(o => o.基本数据.添加时间, Convert.ToDateTime(starttime)));
                }
                if (!string.IsNullOrWhiteSpace(endtime))
                {
                    q = q.And(MongoDB.Driver.Builders.Query<用户基本数据>.LTE(o => o.基本数据.添加时间, Convert.ToDateTime(endtime)));
                }
                var pagesize = 15;

                var liscount = 用户管理.计数用户<单位用户>(0, 0, q);

                var maxpage = Math.Max((liscount + pagesize - 1) / pagesize, 1);
                if (string.IsNullOrEmpty(page.ToString()) || page < 0 || page > maxpage)
                {
                    page = 1;
                }
                ViewData["currentPage"] = page;
                ViewData["pagecount"] = maxpage;


                ViewData["查询单位列表"] = 用户管理.查询用户<单位用户>((page - 1) * pagesize, pagesize, q, includeDisabled: false);
                return PartialView("Procure_Part/Part_Print_UserListTable");
            }
            catch
            {
                return Content("<span style='color:red;'>条件有误，请重新查询！</span>");
            }
        }
        public ActionResult Part_DepartmentListSearch()
        {
            string id = Request.QueryString["num"].ToString();
            string name = Request.QueryString["name"].ToString();
            int catorgray = int.Parse(Request.QueryString["catorgray"].ToString());
            string page = Request.QueryString["p"].ToString();
            IMongoQuery q = null;
            if (!string.IsNullOrWhiteSpace(id))
            {
                q = q.And(MongoDB.Driver.Builders.Query.EQ("单位信息.单位编码", id));
            }
            if (!string.IsNullOrWhiteSpace(name))
            {
                q = q.And(MongoDB.Driver.Builders.Query.EQ("登录信息.登录名", name));
            }
            if (catorgray != 0)
            {
                q = q.And(MongoDB.Driver.Builders.Query.EQ("单位信息.单位级别", catorgray));
            }

            //long listcount = (int)用户管理.计数用户<供应商>(0, 0, q, false);
            long listcount = (int)用户管理.计数用户<单位用户>(0, 0, q, false);
            long maxpagesize = Math.Max((listcount + UNITUSER_PAGESIZE - 1) / UNITUSER_PAGESIZE, 1);
            ViewData["currentpage"] = page;
            ViewData["pagecount"] = maxpagesize;
            ViewData["user"] = 用户管理.查询用户<单位用户>(UNITUSER_PAGESIZE * (int.Parse(page) - 1), UNITUSER_PAGESIZE, q);

            return PartialView("Procure_Part/Part_DepartmentListSearch");
        }
        public ActionResult Part_Department_Detail(int? id)
        {
            单位用户 model = null;
            try
            {
                var temp = 用户管理.查找用户<单位用户>((long)id);
                model = temp;
            }
            catch (Exception)
            {

            }

            return PartialView("Procure_Part/Part_Department_Detail", model);
        }

        [单一权限验证(权限.未审核的附属账号)]
        public ActionResult DepartmentAuditList()
        {
            return View();
        }
        public ActionResult DepartmentAudit()
        {
            return View();
        }
        public ActionResult Department_Detail()
        {
            return View();
        }
        public ActionResult Part_DepartmentAuditList()
        {
            long pgCount = 0;
            int cpg = 0;
            if (!string.IsNullOrWhiteSpace(Request.QueryString["page"]))
            {
                cpg = int.Parse(Request.QueryString["page"]);
            }
            if (cpg <= 0)
            {
                cpg = 1;
            }
            long pc = currentUser.计数未审核管辖单位();
            pgCount = pc / 10;
            if (pc % 10 > 0)
            {
                pgCount++;
            }
            ViewData["Pagecount"] = pgCount;
            ViewData["CurrentPage"] = cpg;
            ViewData["user"] = currentUser.获取未审核管辖单位(10 * (cpg - 1), 10);

            return PartialView("Procure_Part/Part_DepartmentAuditList");
        }
        public ActionResult Part_DepartmentAudit(int? id)
        {
            单位用户 model = null;
            try
            {
                var temp = 用户管理.查找用户<单位用户>((long)id);
                if (temp.审核数据.审核状态 == 审核状态.未审核)
                {
                    model = temp;
                }
            }
            catch (Exception)
            {

            }
            ViewData["用户组列表"] = 用户组管理.查询用户组(0, 0);
            return PartialView("Procure_Part/Part_DepartmentAudit", model);
        }

        [HttpPost]
        public ActionResult DepartmentAudit(string action, int? id)
        {
            if (action == "审核通过")
            {
                var p = Request.Form["usergroup"];
                var m = p.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                var h = 用户管理.查找用户<单位用户>((long)id);
                h.用户组 = new List<string>();
                for (int i = 0; i < m.Length; i++)
                {
                    h.用户组.Add(m[i]);
                }
                用户管理.更新用户<单位用户>(h);
                用户管理.审核单位用户((long)id, currentUser.Id, 审核状态.审核通过);
            }
            else if (action == "审核不通过")
            {
                用户管理.审核单位用户((long)id, currentUser.Id, 审核状态.审核未通过);
            }

            return View("DepartmentAuditList");
        }

        public ActionResult Index()
        {
            IEnumerable<站内消息> Msg = 站内消息管理.查询收信人的站内消息(0, 0, currentUser.Id);
            int count = 0;
            foreach (var item in Msg)
            {
                if (item.基本数据.添加时间 > item.收信人.上次阅读时间)
                {
                    count++;
                }
            }

            IMongoQuery q = null;
            if (currentUser.Id == 11)
            {
                q = Query<供应商>.Where(
                    o => o.审核数据.审核状态 == 审核状态.审核通过 &&
                         o.供应商用户信息.初审数据.审核状态 == 审核状态.未审核 &&
                         o.所属地域.省份.Contains("四川")
                         );
            }
            if (currentUser.Id == 12)
            {
                q = Query<供应商>.Where(
                    o => o.审核数据.审核状态 == 审核状态.审核通过 &&
                         o.供应商用户信息.初审数据.审核状态 == 审核状态.未审核 &&
                         o.所属地域.省份.Contains("重庆")
                         );
            }
            if (currentUser.Id == 13)
            {
                q = Query<供应商>.Where(
                    o => o.审核数据.审核状态 == 审核状态.审核通过 &&
                         o.供应商用户信息.初审数据.审核状态 == 审核状态.未审核 &&
                         o.所属地域.省份.Contains("云南")
                         );
            }
            if (currentUser.Id == 14)
            {
                q = Query<供应商>.Where(
                    o => o.审核数据.审核状态 == 审核状态.审核通过 &&
                         o.供应商用户信息.初审数据.审核状态 == 审核状态.未审核 &&
                         o.所属地域.省份.Contains("贵州")
                         );
            }
            if (currentUser.Id == 15)
            {
                q = Query<供应商>.Where(
                    o => o.审核数据.审核状态 == 审核状态.审核通过 &&
                         o.供应商用户信息.初审数据.审核状态 == 审核状态.未审核 &&
                         o.所属地域.省份.Contains("西藏")
                         );
            }
            if (currentUser.Id == 10002)
            {
                q = Query<供应商>.Where(
                    o => o.审核数据.审核状态 == 审核状态.审核通过 &&
                         o.供应商用户信息.初审数据.审核状态 == 审核状态.审核通过 &&
                         o.供应商用户信息.复审数据.审核状态 == 审核状态.未审核
                         );
            }

            ViewData["业务通知"] = 通知管理.查询通知(0, 5, Query<通知>.Where(o => o.审核数据.审核状态 == 审核状态.审核通过));


            ViewData["gysrk"] = (int)用户管理.计数用户<供应商>(0, 0, q);

#if INTRANET
            ViewData["dcxzj"] = 专家抽选管理.查询专家抽选记录(0, 0, Query.EQ("申请抽选状态", 申请抽选状态.已批准待抽选).And(Query<专家抽选记录>.EQ(o => o.经办人.用户ID, HttpContext.获取当前用户<单位用户>().Id))).Count();
            ViewData["dcxgys"] = 供应商抽选管理.查询供应商抽选历史记录(0, 0, Query.EQ("申请抽选状态", 申请抽选状态.已批准待抽选).And(Query<供应商抽选记录>.EQ(o => o.经办人.用户ID, HttpContext.获取当前用户<单位用户>().Id))).Count();
#endif
            ViewData["msg"] = 公告管理.计数公告(0, 0, Query<公告>.Where(o => o.审核数据.审核状态 == 审核状态.未审核 && (o.审核数据.审核者.用户ID == currentUser.Id || o.审核数据2.审核者.用户ID == currentUser.Id)));
            ViewData["ysd"] = 验收单管理.计数验收单(0, 0, Query<验收单>.Where(o => o.管理单位审核人.用户ID == currentUser.Id && o.审核数据.审核状态 == 审核状态.未审核 && !o.是否作废));
            ViewData["UnitUser"] = 用户管理.计数用户<单位用户>(0, 0, Query<单位用户>.Where(o => o.审核数据.审核者.用户ID == currentUser.Id && o.审核数据.审核状态 == 审核状态.未审核));
            ViewData["msg_count"] = count;

            var model1 = 需求计划管理.查询需求计划(0, 0, Query<需求计划>.Where(o => o.当前处理单位链接.用户ID == currentUser.Id && o.并入需求计划链接.需求计划ID == -1)); //&& m.需求发起单位链接.用户ID != currentUser.Id 
            model1 = model1.Where(o => (!o.审核历史列表.Any() || (o.审核历史列表.Any() && o.审核历史列表.Last().审核状态 != 审核状态.审核未通过)) && (currentUser.用户组.Contains("需求合并") || o.需求发起单位链接.用户ID != currentUser.Id) && !o.流程已完成);
            ViewData["Demand"] = model1.Count();

            var m = currentUser;
            if (currentUser.Id == 10)
            {
                m = 用户管理.查找用户<单位用户>(currentUser.Id);
            }
            return View(m);
        }
        public ActionResult Backlog()
        {
            IEnumerable<站内消息> Msg = 站内消息管理.查询收信人的站内消息(0, 0, currentUser.Id);
            int count = 0;
            foreach (var item in Msg)
            {
                if (item.基本数据.添加时间 > item.收信人.上次阅读时间)
                {
                    count++;
                }
            }

            IMongoQuery q = null;
            if (currentUser.Id == 11)
            {
                q = Query<供应商>.Where(
                    o => o.审核数据.审核状态 == 审核状态.审核通过 &&
                         o.供应商用户信息.初审数据.审核状态 == 审核状态.未审核 &&
                         o.所属地域.省份.Contains("四川")
                         );
            }
            if (currentUser.Id == 12)
            {
                q = Query<供应商>.Where(
                    o => o.审核数据.审核状态 == 审核状态.审核通过 &&
                         o.供应商用户信息.初审数据.审核状态 == 审核状态.未审核 &&
                         o.所属地域.省份.Contains("重庆")
                         );
            }
            if (currentUser.Id == 13)
            {
                q = Query<供应商>.Where(
                    o => o.审核数据.审核状态 == 审核状态.审核通过 &&
                         o.供应商用户信息.初审数据.审核状态 == 审核状态.未审核 &&
                         o.所属地域.省份.Contains("云南")
                         );
            }
            if (currentUser.Id == 14)
            {
                q = Query<供应商>.Where(
                    o => o.审核数据.审核状态 == 审核状态.审核通过 &&
                         o.供应商用户信息.初审数据.审核状态 == 审核状态.未审核 &&
                         o.所属地域.省份.Contains("贵州")
                         );
            }
            if (currentUser.Id == 15)
            {
                q = Query<供应商>.Where(
                    o => o.审核数据.审核状态 == 审核状态.审核通过 &&
                         o.供应商用户信息.初审数据.审核状态 == 审核状态.未审核 &&
                         o.所属地域.省份.Contains("西藏")
                         );
            }
            if (currentUser.Id == 10002)
            {
                q = Query<供应商>.Where(
                    o => o.审核数据.审核状态 == 审核状态.审核通过 &&
                         o.供应商用户信息.初审数据.审核状态 == 审核状态.审核通过 &&
                         o.供应商用户信息.复审数据.审核状态 == 审核状态.未审核
                         );
            }

            ViewData["gysrk"] = (int)用户管理.计数用户<供应商>(0, 0, q);
#if INTRANET
            ViewData["dcxzj"] = 专家抽选管理.查询专家抽选记录(0, 0, Query.EQ("申请抽选状态", 申请抽选状态.已批准待抽选).And(Query<专家抽选记录>.EQ(o => o.经办人.用户ID, HttpContext.获取当前用户<单位用户>().Id))).Count();
            ViewData["dcxgys"] = 供应商抽选管理.查询供应商抽选历史记录(0, 0, Query.EQ("申请抽选状态", 申请抽选状态.已批准待抽选).And(Query<供应商抽选记录>.EQ(o => o.经办人.用户ID, HttpContext.获取当前用户<单位用户>().Id))).Count();
#endif
            ViewData["msg"] = 公告管理.计数公告(0, 0, Query<公告>.Where(o => o.审核数据.审核状态 == 审核状态.未审核 && (o.审核数据.审核者.用户ID == currentUser.Id || o.审核数据2.审核者.用户ID == currentUser.Id)));
            ViewData["ysd"] = 验收单管理.计数验收单(0, 0, Query<验收单>.Where(o => o.管理单位审核人.用户ID == currentUser.Id && o.审核数据.审核状态 == 审核状态.未审核 && !o.是否作废));
            ViewData["UnitUser"] = 用户管理.计数用户<单位用户>(0, 0, Query<单位用户>.Where(o => o.审核数据.审核者.用户ID == currentUser.Id && o.审核数据.审核状态 == 审核状态.未审核));
            ViewData["msg_count"] = count;

            var model1 = 需求计划管理.查询需求计划(0, 0, Query<需求计划>.Where(o => o.当前处理单位链接.用户ID == currentUser.Id && o.并入需求计划链接.需求计划ID == -1)); //&& m.需求发起单位链接.用户ID != currentUser.Id 
            model1 = model1.Where(o => (!o.审核历史列表.Any() || (o.审核历史列表.Any() && o.审核历史列表.Last().审核状态 != 审核状态.审核未通过)) && (currentUser.用户组.Contains("需求合并") || o.需求发起单位链接.用户ID != currentUser.Id) && !o.流程已完成);
            ViewData["Demand"] = model1.Count();
            return View();
        }
        public ActionResult ExpertStatistic()
        {
            long sum = 用户管理.计数用户<专家>(0, 0);
            Dictionary<string, long> Ulist = new Dictionary<string, long>();
            DateTime date = DateTime.Now;
            ViewData["区间查询数"] = 用户管理.计数用户<专家>(0, 0, Query<专家>.Where(m => m.基本数据.添加时间 >= date.AddMonths(-1) && m.基本数据.添加时间 <= date));
            Ulist.Add(date.AddMonths(-1).ToString("yyyy/MM/dd") + "-" + date.ToString("yyyy/MM/dd"), 用户管理.计数用户<专家>(0, 0, Query<专家>.Where(m => m.基本数据.添加时间 >= date.AddMonths(-1) && m.基本数据.添加时间 <= date)));
            Ulist.Add(date.AddMonths(-2).ToString("yyyy/MM/dd") + "-" + date.AddMonths(-1).ToString("yyyy/MM/dd"), 用户管理.计数用户<专家>(0, 0, Query<专家>.Where(m => m.基本数据.添加时间 >= date.AddMonths(-2) && m.基本数据.添加时间 <= date.AddMonths(-1))));
            Ulist.Add(date.AddMonths(-3).ToString("yyyy/MM/dd") + "-" + date.AddMonths(-2).ToString("yyyy/MM/dd"), 用户管理.计数用户<专家>(0, 0, Query<专家>.Where(m => m.基本数据.添加时间 >= date.AddMonths(-3) && m.基本数据.添加时间 <= date.AddMonths(-2))));
            ViewData["专家总数"] = sum;
            ViewData["三月数据"] = Ulist;
            return View();
        }
        public long ExpertStatisticNum()
        {
            try
            {
                DateTime start = DateTime.Parse(Request.QueryString["startdate"]);
                DateTime last = DateTime.Parse(Request.QueryString["enddate"]);
                long count = 用户管理.计数用户<专家>(0, 0, Query<专家>.Where(m => m.基本数据.添加时间 >= start && m.基本数据.添加时间 <= last));
                return count;
            }
            catch
            {
                return 0;
            }
        }

        public class LoginRecord
        {
            public string IpAddr { get; set; }
            public string Result { get; set; }
            public long UserId { get; set; }
            public string LoginTime { get; set; }
            public string InOrEx { get; set; }
        }
        public ActionResult LoginStatistic()
        {
            return View();
        }
        public ActionResult Part_LoginStastic()
        {
            //var now = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
            //var loginlist = 登录统计管理.查询登录统计(0, 0, Query<登录统计>.Where(o => o.登录时间 >= now));
            //ViewData["登录统计"] = loginlist.Take(20);
            //ViewData["统计总数"] = loginlist.Count();
            return PartialView("Procure_Part/Part_LoginStastic");
        }
        public ActionResult Part_StatisticsLoginSearch()
        {
            int page = 1;
            long pageCount = 0;
            List<LoginRecord> Lr = new List<LoginRecord>();
            if (!string.IsNullOrWhiteSpace(Request.QueryString["page"]))
            {
                page = int.Parse(Request.QueryString["page"]);
            }
            var loginuser = Request.Params["loginuser"];
            var loginresult = Request.Params["loginresult"];
            var time = Request.Params["time"];
            var nettype = Request.Params["nettype"];
            var q = Query.Null;

            if (!string.IsNullOrWhiteSpace(nettype))
            {
                if (nettype == "内网")
                {
                    q = q.And(Query<登录统计>.Where(o => o.内网访问));
                }
                else
                {
                    q = q.And(Query<登录统计>.Where(o => !o.内网访问));
                }
            }

            if (!string.IsNullOrWhiteSpace(loginresult))
            {
                if (loginresult == "登录成功")
                {
                    q = q.And(Query<登录统计>.Where(o => o.登录结果 == 登录结果.登录成功));
                }
                else
                {
                    q = q.And(Query<登录统计>.Where(o => o.登录结果 != 登录结果.登录成功));
                }
            }

            if (!string.IsNullOrWhiteSpace(loginuser))
            {
                if (loginuser == "供应商")
                {
                    q = q.And(Query<登录统计>.Where(o => o.用户数据.用户ID >= 200000000000 && o.用户数据.用户ID < 300000000000));
                }
                else if (loginuser == "单位用户")
                {
                    q = q.And(Query<登录统计>.Where(o => o.用户数据.用户ID >= 0 && o.用户数据.用户ID < 100000000000));
                }
                else if (loginuser == "专家")
                {
                    q = q.And(Query<登录统计>.Where(o => o.用户数据.用户ID >= 300000000000 && o.用户数据.用户ID < 400000000000));
                }
                else if (loginuser == "运营团队")
                {
                    q = q.And(Query<登录统计>.Where(o => o.用户数据.用户ID >= 100000000000 && o.用户数据.用户ID < 200000000000));
                }
            }

            if (!string.IsNullOrWhiteSpace(time))
            {
                var date = DateTime.Now;
                if (time == "1")
                {
                    var now = new DateTime(date.Year, date.Month, date.Day);
                    q = q.And(Query<登录统计>.Where(o => o.登录时间 >= now));
                }
                else
                {
                    q = q.And(Query<登录统计>.Where(o => o.登录时间 > date.AddDays(0 - int.Parse(time))));
                }
            }
            IEnumerable<登录统计> loginlist = 登录统计管理.查询登录统计(20 * (page - 1), 20, q);
            foreach (var item in loginlist)
            {
                LoginRecord r = new LoginRecord();
                if (item.内网访问)
                {
                    r.InOrEx = "内网";
                }
                else
                {
                    r.InOrEx = "外网";
                }
                r.IpAddr = item.登录IP;
                r.LoginTime = item.登录时间.ToString("yyyy/MM/dd hh:mm:ss");
                r.Result = item.登录结果.ToString();
                r.UserId = item.用户数据.用户ID;
                Lr.Add(r);
            }
            long pc = 登录统计管理.计数登录统计(0, 0, q);
            pageCount = pc / 20;
            if (pc % 20 > 0)
            {
                pageCount++;
            }
            JsonResult json = new JsonResult() { Data = new { loginuser = Lr, pCount = pageCount, sum = pc } };
            return Json(json, JsonRequestBehavior.AllowGet);
        }


        public ActionResult AdvertiseStatistic()
        {
            return View();
        }
        public ActionResult Part_AdvertiseStastic()
        {
            var now = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
            var loginlist = 广告点击统计管理.查询广告点击统计(0, 0, Query<广告点击统计>.Where(o => o.基本数据.添加时间 > now));
            ViewData["广告点击统计"] = loginlist;
            ViewData["统计总数"] = loginlist.Count();

            return PartialView("Procure_Part/Part_AdvertiseStastic");
        }
        public ActionResult Part_AdvertiseClickSearch()
        {
            var position = Request.Params["position"];
            var type = Request.Params["type"];
            var time = Request.Params["time"];
            var nettype = Request.Params["nettype"];
            var q = Query.Null;

            if (!string.IsNullOrWhiteSpace(nettype))
            {
                if (nettype == "内网")
                {
                    q = q.And(Query<广告点击统计>.Where(o => o.内网访问));
                }
                else
                {
                    q = q.And(Query<广告点击统计>.Where(o => !o.内网访问));
                }
            }

            if (!string.IsNullOrWhiteSpace(position))
            {
                q = q.And(Query<广告点击统计>.Where(o => o.广告位置 == position));
            }

            if (!string.IsNullOrWhiteSpace(type))
            {
                if (type == "商品广告")
                {
                    q = q.And(Query<广告点击统计>.Where(o => o.广告类型 == 广告类型.商品广告));
                }
                else if (type == "供应商广告")
                {
                    q = q.And(Query<广告点击统计>.Where(o => o.广告类型 == 广告类型.供应商广告));
                }
            }

            if (!string.IsNullOrWhiteSpace(time))
            {
                var date = DateTime.Now;
                if (time == "1")
                {
                    var now = new DateTime(date.Year, date.Month, date.Day);
                    q = q.And(Query<广告点击统计>.Where(o => o.基本数据.添加时间 >= now));
                }
                else
                {
                    q = q.And(Query<广告点击统计>.Where(o => o.基本数据.添加时间 > date.AddDays(0 - int.Parse(time))));
                }
            }
            var loginlist = 广告点击统计管理.查询广告点击统计(0, 0, q);
            ViewData["广告点击统计"] = loginlist;
            ViewData["统计总数"] = loginlist.Count();
            return PartialView("Procure_Part/Part_AdvertiseClickSearch");
        }

        public class goodStatistic
        {
            public string name { get; set; }
            public long count { get; set; }
            public int clickNumber { get; set; }
        }
        public ActionResult GoodStatistic()
        {
            IEnumerable<商品分类> types = 商品分类管理.查找子分类();
            Dictionary<long, Dictionary<string, int>> commonCount = new Dictionary<long, Dictionary<string, int>>();
            List<Tuple<string, string, long>> goodCount = new List<Tuple<string, string, long>>();
            foreach (var item in types)
            {
                if (item.分类性质 == 商品分类性质.通用物资)
                {
                    foreach (var son in item.子分类)
                    {
                        foreach (var child in son.子分类)
                        {
                            goodCount.Add(Tuple.Create(son.分类名, child.分类名, 商品管理.计数分类下商品(child.Id, 0, 0, Query<商品>.Where(o => o.审核数据.审核状态 == 审核状态.审核通过), false)));
                        }
                    }
                }
                else
                {
                    foreach (var son in item.子分类)
                    {
                        goodCount.Add(Tuple.Create(item.分类名, son.分类名, 商品管理.计数分类下商品(son.Id, 0, 0, Query<商品>.Where(o => o.审核数据.审核状态 == 审核状态.审核通过), false)));
                    }
                }
            }

            IEnumerable<商品> commongood = 商品管理.查询商品(0, 1000, Query<商品>.Where(o => o.审核数据.审核状态 == 审核状态.审核通过));

            if (commongood != null && commongood.Count() != 0)
            {
                foreach (var item in commongood)
                {
                    if (!string.IsNullOrWhiteSpace(item.商品信息.品牌))
                    {
                        if (commonCount.ContainsKey(item.商品信息.所属商品分类.商品分类.Id))
                        {
                            if (commonCount[item.商品信息.所属商品分类.商品分类.Id].ContainsKey(item.商品信息.品牌))
                            {
                                commonCount[item.商品信息.所属商品分类.商品分类.Id][item.商品信息.品牌]++;
                            }
                            else
                            {
                                commonCount[item.商品信息.所属商品分类.商品分类.Id].Add(item.商品信息.品牌, 1);
                            }
                        }
                        else
                        {
                            Dictionary<string, int> m = new Dictionary<string, int>();
                            m.Add(item.商品信息.品牌, 1);
                            commonCount.Add(item.商品信息.所属商品分类.商品分类.Id, m);
                        }
                    }
                    else
                    {
                        if (item.商品信息.所属商品分类.商品分类 != null)
                        {
                            if (commonCount.ContainsKey(item.商品信息.所属商品分类.商品分类.Id))
                            {
                                if (commonCount[item.商品信息.所属商品分类.商品分类.Id].ContainsKey("无品牌名称"))
                                {
                                    commonCount[item.商品信息.所属商品分类.商品分类.Id]["无品牌名称"]++;
                                }
                                else
                                {
                                    commonCount[item.商品信息.所属商品分类.商品分类.Id].Add("无品牌名称", 1);
                                }
                            }
                            else
                            {
                                Dictionary<string, int> m = new Dictionary<string, int>();
                                m.Add("无品牌名称", 1);
                                commonCount.Add(item.商品信息.所属商品分类.商品分类.Id, m);
                            }
                        }
                    }
                }
            }
            ViewData["commonCount"] = commonCount;
            ViewData["gtype"] = types;
            ViewData["statistic"] = goodCount;
            List<goodStatistic> list = new List<goodStatistic>();
            List<goodStatistic> glist = new List<goodStatistic>();
            long sum = 商品管理.计数商品(0, 0);
            List<goodStatistic> Ulist = new List<goodStatistic>();
            goodStatistic gd = new goodStatistic();
            DateTime date = DateTime.Now;
            IEnumerable<商品> good = null;
            int clicksnumber = 0;
            ViewData["区间查询数"] = 商品管理.计数商品(0, 0, Query<商品>.Where(m => m.基本数据.添加时间 >= date.AddMonths(-1) && m.基本数据.添加时间 <= date));
            ViewData["商品总数"] = 商品管理.计数商品(0, 0);
            good = 商品管理.查询商品(0, 0, Query<商品>.Where(m => m.基本数据.添加时间 >= date.AddMonths(-1) && m.基本数据.添加时间 <= date));
            foreach (var item in good)
            {
                clicksnumber += item.销售信息.点击量;
            }
            gd.name = "已审核商品总数";
            gd.count = 商品管理.计数商品(0, 0, Query<商品>.Where(m => m.审核数据.审核状态 == 审核状态.审核通过));
            gd.clickNumber = clicksnumber;
            list.Add(gd);
            clicksnumber = 0;
            gd = new goodStatistic();
            good = 商品管理.查询商品(0, 0, Query<商品>.Where(m => m.审核数据.审核状态 == 审核状态.审核通过));
            foreach (var item in good)
            {
                clicksnumber += item.销售信息.点击量;
            }
            gd.name = "审核未通过商品总数";
            gd.count = 商品管理.计数商品(0, 0, Query<商品>.Where(m => m.审核数据.审核状态 == 审核状态.审核未通过));
            gd.clickNumber = clicksnumber;
            list.Add(gd);
            clicksnumber = 0;
            gd = new goodStatistic();
            good = 商品管理.查询商品(0, 0, Query<商品>.Where(m => m.审核数据.审核状态 == 审核状态.审核未通过));
            foreach (var item in good)
            {
                clicksnumber += item.销售信息.点击量;
            }
            gd.name = "未审核商品总数";
            gd.count = 商品管理.计数商品(0, 0, Query<商品>.Where(m => m.审核数据.审核状态 == 审核状态.未审核));
            gd.clickNumber = clicksnumber;
            list.Add(gd);
            clicksnumber = 0;
            gd = new goodStatistic();
            good = 商品管理.查询商品(0, 0, Query<商品>.Where(m => m.审核数据.审核状态 == 审核状态.未审核));
            foreach (var item in good)
            {
                clicksnumber += item.销售信息.点击量;
            }
            gd.name = date.AddMonths(-1).ToString("yyyy/MM/dd") + "-" + date.ToString("yyyy/MM/dd");
            gd.count = 商品管理.计数商品(0, 0, Query<商品>.Where(m => m.基本数据.添加时间 >= date.AddMonths(-1) && m.基本数据.添加时间 <= date));
            gd.clickNumber = clicksnumber;
            Ulist.Add(gd);
            clicksnumber = 0;
            gd = new goodStatistic();
            good = 商品管理.查询商品(0, 0, Query<商品>.Where(m => m.基本数据.添加时间 >= date.AddMonths(-2) && m.基本数据.添加时间 <= date.AddMonths(-1)));
            foreach (var item in good)
            {
                clicksnumber += item.销售信息.点击量;
            }
            gd.name = date.AddMonths(-2).ToString("yyyy/MM/dd") + "-" + date.AddMonths(-1).ToString("yyyy/MM/dd");
            gd.count = 商品管理.计数商品(0, 0, Query<商品>.Where(m => m.基本数据.添加时间 >= date.AddMonths(-2) && m.基本数据.添加时间 <= date.AddMonths(-1)));
            gd.clickNumber = clicksnumber;
            Ulist.Add(gd);
            clicksnumber = 0;
            gd = new goodStatistic();
            good = 商品管理.查询商品(0, 0, Query<商品>.Where(m => m.基本数据.添加时间 >= date.AddMonths(-3) && m.基本数据.添加时间 <= date.AddMonths(-2)));
            foreach (var item in good)
            {
                clicksnumber += item.销售信息.点击量;
            }
            gd.name = date.AddMonths(-3).ToString("yyyy/MM/dd") + "-" + date.AddMonths(-2).ToString("yyyy/MM/dd");
            gd.count = 商品管理.计数商品(0, 0, Query<商品>.Where(m => m.基本数据.添加时间 >= date.AddMonths(-3) && m.基本数据.添加时间 <= date.AddMonths(-2)));
            gd.clickNumber = clicksnumber;
            Ulist.Add(gd);
            clicksnumber = 0;
            gd = new goodStatistic();
            good = 商品管理.查询商品(0, 0, Query<商品>.Where(m => m.采购信息.参与普通采购 == true));
            foreach (var item in good)
            {
                clicksnumber += item.销售信息.点击量;
            }
            gd.name = "普通商品";
            gd.count = 商品管理.计数商品(0, 0, Query<商品>.Where(m => m.采购信息.参与普通采购 == true));
            gd.clickNumber = clicksnumber;
            glist.Add(gd);
            clicksnumber = 0;
            gd = new goodStatistic();
            good = 商品管理.查询商品(0, 0, Query<商品>.Where(m => m.采购信息.参与协议采购 == true));
            foreach (var item in good)
            {
                clicksnumber += item.销售信息.点击量;
            }
            gd.name = "协议采购";
            gd.count = 商品管理.计数商品(0, 0, Query<商品>.Where(m => m.采购信息.参与协议采购 == true));
            gd.clickNumber = clicksnumber;
            glist.Add(gd);
            clicksnumber = 0;
            gd = new goodStatistic();
            good = 商品管理.查询商品(0, 0, Query<商品>.Where(m => m.采购信息.参与应急采购 == true));
            foreach (var item in good)
            {
                clicksnumber += item.销售信息.点击量;
            }
            gd.name = "应急采购";
            gd.count = 商品管理.计数商品(0, 0, Query<商品>.Where(m => m.采购信息.参与应急采购 == true));
            gd.clickNumber = clicksnumber;
            glist.Add(gd);
            clicksnumber = 0;
            gd = new goodStatistic();
            good = 商品管理.查询商品(0, 0, Query<商品>.Where(m => m.采购信息.参与应急采购 == false && m.采购信息.参与普通采购 == false && m.采购信息.参与协议采购 == false));
            foreach (var item in good)
            {
                clicksnumber += item.销售信息.点击量;
            }
            gd.name = "未设置类型";
            gd.count = 商品管理.计数商品(0, 0, Query<商品>.Where(m => m.采购信息.参与应急采购 == false && m.采购信息.参与普通采购 == false && m.采购信息.参与协议采购 == false));
            gd.clickNumber = clicksnumber;
            glist.Add(gd);
            clicksnumber = 0;

            ViewData["三月数据列表"] = Ulist;
            ViewData["审核状况列表"] = list;
            ViewData["采购类型列表"] = glist;
            return View();
        }
        public ActionResult GysStatistic()
        {
            string startdate = DateTime.Now.Year + "/" + (DateTime.Now.Month != 1 ? DateTime.Now.Month - 1 : DateTime.Now.Month) + "/1";
            string enddate = DateTime.Now.Year + "/" + (DateTime.Now.Month != 1 ? DateTime.Now.Month - 1 : DateTime.Now.Month) + "/" + DateTime.DaysInMonth(DateTime.Now.Year, (DateTime.Now.Month != 1 ? DateTime.Now.Month - 1 : DateTime.Now.Month));

            IMongoQuery q = null;
            q = Query<供应商>.Where(O => O.基本数据.添加时间 >= Convert.ToDateTime(startdate) && O.基本数据.添加时间 <= Convert.ToDateTime(enddate));
            long listcount = (int)用户管理.计数用户<供应商>(0, 0, q);
            long maxpagesize = Math.Max((listcount + 10 - 1) / 10, 1);//10，每页显示10条记录
            int page = 1;

            ViewData["currentpage"] = page;
            ViewData["pagecount"] = maxpagesize;

            var gys = 用户管理.查询用户<供应商>(0, 0);
            ViewData["供应商总数"] = gys.Count();
            var gys_num = new Dictionary<string, int>();
            var gys_num1 = new Dictionary<string, int>();
            var gys_num2 = new Dictionary<string, int>();
            var gys_num3 = new Dictionary<string, int>();

            gys_num.Add("未预审", gys.Where(o => o.审核数据.审核状态 == 审核状态.未审核).Count());
            gys_num.Add("预审未通过", gys.Where(o => o.审核数据.审核状态 == 审核状态.审核未通过).Count());
            gys_num.Add("预审通过", gys.Where(o => o.审核数据.审核状态 == 审核状态.审核通过).Count());
            gys_num1.Add("应急供应商", gys.Where(o => o.供应商用户信息.应急供应商).Count());
            gys_num1.Add("协议供应商", gys.Where(o => o.供应商用户信息.协议供应商).Count());
            gys_num1.Add("普通供应商", gys.Where(o => o.供应商用户信息.普通供应商).Count());
            gys_num2.Add("同时为应急/普通", gys.Where(o => o.供应商用户信息.应急供应商 && o.供应商用户信息.普通供应商).Count());
            gys_num2.Add("同时为应急/协议", gys.Where(o => o.供应商用户信息.应急供应商 && o.供应商用户信息.协议供应商).Count());
            gys_num2.Add("同时为普通/协议", gys.Where(o => o.供应商用户信息.普通供应商 && o.供应商用户信息.协议供应商).Count());
            gys_num2.Add("同时为普通/协议/应急", gys.Where(o => o.供应商用户信息.普通供应商 && o.供应商用户信息.协议供应商 && o.供应商用户信息.应急供应商).Count());
            gys_num3.Add("未设置认证级别", gys.Where(o => o.供应商用户信息.认证级别 == 供应商.认证级别.未设置).Count());
            gys_num3.Add("一级供应商", gys.Where(o => o.供应商用户信息.认证级别 == 供应商.认证级别.一级供应商).Count());
            gys_num3.Add("二级供应商", gys.Where(o => o.供应商用户信息.认证级别 == 供应商.认证级别.二级供应商).Count());
            gys_num3.Add("军采通会员", gys.Where(o => o.供应商用户信息.认证级别 == 供应商.认证级别.军采通标准会员).Count());
            gys_num3.Add("军采通标准会员", gys.Where(o => o.供应商用户信息.认证级别 == 供应商.认证级别.军采通标准会员).Count());
            gys_num3.Add("军采通高级会员", gys.Where(o => o.供应商用户信息.认证级别 == 供应商.认证级别.军采通商务会员).Count());

            ViewData["按审核状态统计"] = gys_num;
            ViewData["按供应商类型统计"] = gys_num1;
            ViewData["按供应商类型同时存在统计"] = gys_num2;
            ViewData["按认证级别统计"] = gys_num3;

            ViewData["startTime"] = startdate;
            ViewData["endTime"] = enddate;
            ViewData["区间查询数"] = listcount;
            ViewData["所有供应商"] = 用户管理.查询用户<供应商>(10 * (page - 1), 10, q, false, SortBy.Descending("基本数据.添加时间"));
            return View();
        }
        public ActionResult AdStatistic()
        {
            string startdate = DateTime.Now.Year.ToString() + "/"
                + (DateTime.Now.Month != 1 ? DateTime.Now.Month - 1 : DateTime.Now.Month).ToString()
                + "/1";
            string enddate = DateTime.Now.Year.ToString() + "/"
                + (DateTime.Now.Month != 1 ? DateTime.Now.Month - 1 : DateTime.Now.Month).ToString()
                + "/" + DateTime.DaysInMonth(DateTime.Now.Year, (DateTime.Now.Month != 1 ? DateTime.Now.Month - 1 : DateTime.Now.Month));

            IMongoQuery q = null;
            q = Query<公告>.Where(O => O.内容主体.发布时间 >= Convert.ToDateTime(startdate) && O.内容主体.发布时间 <= Convert.ToDateTime(enddate));

            var a = q.And(Query.EQ("公告信息.公告性质", 公告.公告性质.采购公告));
            var b = q.And(Query.EQ("公告信息.公告性质", 公告.公告性质.废标公告));
            var c = q.And(Query.EQ("公告信息.公告性质", 公告.公告性质.技术公告));
            var d = q.And(Query.EQ("公告信息.公告性质", 公告.公告性质.流标公告));
            var e = q.And(Query.EQ("公告信息.公告性质", 公告.公告性质.预公告));
            var f = q.And(Query.EQ("公告信息.公告性质", 公告.公告性质.中标结果公示));
            long listcount = (int)公告管理.计数公告(0, 0, q);
            long maxpagesize = Math.Max((listcount + 5 - 1) / 5, 1);//10，每页显示10条记录

            var Ggs = 公告管理.查询公告(0, 0);
            var g = new Dictionary<string, string>();
            var gnum1 = new Dictionary<string, string>();
            var gnum2 = new Dictionary<string, string>();
            var gnum3 = new Dictionary<string, string>();
            var lv = Ggs.Sum(o => o.点击次数);//所有公告点击次数之和
            g.Add("必读", 公告管理.计数公告(0, 0, Query<公告>.Where(o => o.内容基本信息.重要程度 == 重要程度.必读)).ToString());
            g.Add("特别重要", 公告管理.计数公告(0, 0, Query<公告>.Where(o => o.内容基本信息.重要程度 == 重要程度.特别重要)).ToString());
            g.Add("未指定", 公告管理.计数公告(0, 0, Query<公告>.Where(o => o.内容基本信息.重要程度 == 重要程度.未指定)).ToString());
            g.Add("一般", 公告管理.计数公告(0, 0, Query<公告>.Where(o => o.内容基本信息.重要程度 == 重要程度.一般)).ToString());
            g.Add("重要", 公告管理.计数公告(0, 0, Query<公告>.Where(o => o.内容基本信息.重要程度 == 重要程度.重要)).ToString());

            gnum1.Add("发标公告", 公告管理.计数公告(0, 0, Query<公告>.Where(o => o.公告信息.公告性质 == 公告.公告性质.采购公告)).ToString());
            gnum1.Add("废标公告", 公告管理.计数公告(0, 0, Query<公告>.Where(o => o.公告信息.公告性质 == 公告.公告性质.废标公告)).ToString());
            gnum1.Add("技术公告", 公告管理.计数公告(0, 0, Query<公告>.Where(o => o.公告信息.公告性质 == 公告.公告性质.技术公告)).ToString());
            gnum1.Add("流标公告", 公告管理.计数公告(0, 0, Query<公告>.Where(o => o.公告信息.公告性质 == 公告.公告性质.流标公告)).ToString());
            gnum1.Add("预公告", 公告管理.计数公告(0, 0, Query<公告>.Where(o => o.公告信息.公告性质 == 公告.公告性质.预公告)).ToString());
            gnum1.Add("中标公告", 公告管理.计数公告(0, 0, Query<公告>.Where(o => o.公告信息.公告性质 == 公告.公告性质.中标结果公示)).ToString());

            long ff = Ggs.Where(o => o.公告信息.公告性质 == 公告.公告性质.采购公告).Sum(o => o.点击次数);
            gnum2.Add("发标公告", (Math.Round((float)ff / lv * 100, 2)).ToString());

            ff = Ggs.Where(o => o.公告信息.公告性质 == 公告.公告性质.废标公告).Sum(o => o.点击次数);
            gnum2.Add("废标公告", (Math.Round((float)ff / lv * 100, 2)).ToString());

            ff = Ggs.Where(o => o.公告信息.公告性质 == 公告.公告性质.技术公告).Sum(o => o.点击次数);
            gnum2.Add("技术公告", (Math.Round((float)ff / lv * 100, 2)).ToString());

            ff = Ggs.Where(o => o.公告信息.公告性质 == 公告.公告性质.流标公告).Sum(o => o.点击次数);
            gnum2.Add("流标公告", (Math.Round((float)ff / lv * 100, 2)).ToString());

            ff = Ggs.Where(o => o.公告信息.公告性质 == 公告.公告性质.预公告).Sum(o => o.点击次数);
            gnum2.Add("预公告", (Math.Round((float)ff / lv * 100, 2)).ToString());

            ff = Ggs.Where(o => o.公告信息.公告性质 == 公告.公告性质.中标结果公示).Sum(o => o.点击次数);
            gnum2.Add("中标公告", (Math.Round((float)ff / lv * 100, 2)).ToString());

            gnum3.Add("单一来源", 公告管理.计数公告(0, 0, Query<公告>.Where(o => o.公告信息.公告类别 == 公告.公告类别.单一来源)).ToString());
            gnum3.Add("公开招标", 公告管理.计数公告(0, 0, Query<公告>.Where(o => o.公告信息.公告类别 == 公告.公告类别.公开招标)).ToString());
            gnum3.Add("竞争性谈判", 公告管理.计数公告(0, 0, Query<公告>.Where(o => o.公告信息.公告类别 == 公告.公告类别.竞争性谈判)).ToString());
            gnum3.Add("其他", 公告管理.计数公告(0, 0, Query<公告>.Where(o => o.公告信息.公告类别 == 公告.公告类别.其他)).ToString());
            gnum3.Add("网上竞标", 公告管理.计数公告(0, 0, Query<公告>.Where(o => o.公告信息.公告类别 == 公告.公告类别.网上竞标)).ToString());
            gnum3.Add("未设置", 公告管理.计数公告(0, 0, Query<公告>.Where(o => o.公告信息.公告类别 == 公告.公告类别.未设置)).ToString());
            gnum3.Add("协议采购", 公告管理.计数公告(0, 0, Query<公告>.Where(o => o.公告信息.公告类别 == 公告.公告类别.协议采购)).ToString());
            gnum3.Add("询价采购", 公告管理.计数公告(0, 0, Query<公告>.Where(o => o.公告信息.公告类别 == 公告.公告类别.询价采购)).ToString());
            gnum3.Add("邀请招标", 公告管理.计数公告(0, 0, Query<公告>.Where(o => o.公告信息.公告类别 == 公告.公告类别.邀请招标)).ToString());

            int page = 1;
            int.TryParse(Request.Params["page"], out page);
            ViewData["currentpage"] = page;
            ViewData["pagecount"] = maxpagesize;

            ViewData["按重要程度统计"] = g;
            ViewData["按公告性质统计"] = gnum1;
            ViewData["按公告性质点击率统计"] = gnum2;
            ViewData["按公告类别统计"] = gnum3;

            ViewData["startTime"] = startdate;
            ViewData["endTime"] = enddate;
            ViewData["公告总数"] = 公告管理.计数公告(0, 0);
            ViewData["预公告"] = 公告管理.计数公告(0, 0, e);
            ViewData["发标公告"] = 公告管理.计数公告(0, 0, a);
            ViewData["废标公告"] = 公告管理.计数公告(0, 0, b);
            ViewData["技术公告"] = 公告管理.计数公告(0, 0, c);
            ViewData["流标公告"] = 公告管理.计数公告(0, 0, d);
            ViewData["中标公告"] = 公告管理.计数公告(0, 0, f);
            ViewData["区间查询数"] = listcount;

            ViewData["本月所发公告"] = 公告管理.查询公告(5 * (page - 1), 5, q, false, SortBy.Descending("内容主体.发布时间"));
            return View();
        }

        public ActionResult Part_AdStatistic_ByType()
        {
            var area = Request.Params["area"];//地区，如四川
            var year = Request.Params["year"];
            var type = Request.Params["type"];//公告性质
            var val = 0;
            switch (type)
            {
                case "未设置":
                    val = 0;
                    break;
                case "预公告":
                    val = 1;
                    break;
                case "技术公告":
                    val = 2;
                    break;
                case "发标公告":
                    val = 3;
                    break;
                case "中标公告":
                    val = 4;
                    break;
                case "废标公告":
                    val = 5;
                    break;
                case "流标公告":
                    val = 6;
                    break;
            }

            var ad = 公告管理.查询公告(0, 0, Query<公告>.Where(o => o.公告信息.公告类别 != 公告.公告类别.其他));
            var units = ad.Where(o => o.公告信息.所属地域.省份 != null && o.公告信息.所属地域.省份.Contains(area))
                .Select(o => o.公告信息.需求单位 != null ? o.公告信息.需求单位.Trim() : o.公告信息.需求单位)
                .Distinct();

            var adlist = new Dictionary<string, List<int>>();

            switch (area)
            {
                case "四川":
                    foreach (var k in units)
                    {
                        var 本年各月发布公告数量集合 = new List<int>();
                        for (int i = 1; i <= 12; i++)
                        {
                            var count = ad.Where(o => o.公告信息.需求单位 == k &&
                                                            o.内容主体.发布时间.Month == i &&
                                                            (int)o.公告信息.公告性质 == val &&
                                                            o.公告信息.所属地域.省份 != null &&
                                                            o.公告信息.所属地域.省份.Contains(area) &&
                                                            o.内容主体.发布时间.Year == int.Parse(year)).Count();
                            本年各月发布公告数量集合.Add(count);
                        }
                        if (k == null)
                        {
                            adlist.Add("未填写需求单位", 本年各月发布公告数量集合);
                        }
                        else
                        {
                            adlist.Add(k, 本年各月发布公告数量集合);
                        }
                    }
                    break;
                case "重庆":
                    foreach (var k in units)
                    {
                        var 本年各月发布公告数量集合 = new List<int>();
                        for (int i = 1; i <= 12; i++)
                        {
                            var count = ad.Where(o => o.公告信息.需求单位 == k &&
                                                            o.内容主体.发布时间.Month == i &&
                                                            (int)o.公告信息.公告性质 == val &&
                                                            o.公告信息.所属地域.省份 != null &&
                                                            o.公告信息.所属地域.省份.Contains(area) &&
                                                            o.内容主体.发布时间.Year == int.Parse(year)).Count();
                            本年各月发布公告数量集合.Add(count);
                        }
                        if (k == null)
                        {
                            adlist.Add("未填写需求单位", 本年各月发布公告数量集合);
                        }
                        else
                        {
                            adlist.Add(k, 本年各月发布公告数量集合);
                        }
                    }
                    break;
                case "贵州":
                    foreach (var k in units)
                    {
                        var 本年各月发布公告数量集合 = new List<int>();
                        for (int i = 1; i <= 12; i++)
                        {
                            var count = ad.Where(o => o.公告信息.需求单位 == k &&
                                                            o.内容主体.发布时间.Month == i &&
                                                            (int)o.公告信息.公告性质 == val &&
                                                            o.公告信息.所属地域.省份 != null &&
                                                            o.公告信息.所属地域.省份.Contains(area) &&
                                                            o.内容主体.发布时间.Year == int.Parse(year)).Count();
                            本年各月发布公告数量集合.Add(count);
                        }
                        if (k == null)
                        {
                            adlist.Add("未填写需求单位", 本年各月发布公告数量集合);
                        }
                        else
                        {
                            adlist.Add(k, 本年各月发布公告数量集合);
                        }
                    }
                    break;
                case "云南":
                    foreach (var k in units)
                    {
                        var 本年各月发布公告数量集合 = new List<int>();
                        for (int i = 1; i <= 12; i++)
                        {
                            var count = ad.Where(o => o.公告信息.需求单位 == k &&
                                                            o.内容主体.发布时间.Month == i &&
                                                            (int)o.公告信息.公告性质 == val &&
                                                            o.公告信息.所属地域.省份 != null &&
                                                            o.公告信息.所属地域.省份.Contains(area) &&
                                                            o.内容主体.发布时间.Year == int.Parse(year)).Count();
                            本年各月发布公告数量集合.Add(count);
                        }
                        if (k == null)
                        {
                            adlist.Add("未填写需求单位", 本年各月发布公告数量集合);
                        }
                        else
                        {
                            adlist.Add(k, 本年各月发布公告数量集合);
                        }
                    }
                    break;
                case "西藏":
                    foreach (var k in units)
                    {
                        var 本年各月发布公告数量集合 = new List<int>();
                        for (int i = 1; i <= 12; i++)
                        {
                            var count = ad.Where(o => o.公告信息.需求单位 == k &&
                                                            o.内容主体.发布时间.Month == i &&
                                                            (int)o.公告信息.公告性质 == val &&
                                                            o.公告信息.所属地域.省份 != null &&
                                                            o.公告信息.所属地域.省份.Contains(area) &&
                                                            o.内容主体.发布时间.Year == int.Parse(year)).Count();
                            本年各月发布公告数量集合.Add(count);
                        }
                        if (k == null)
                        {
                            adlist.Add("未填写需求单位", 本年各月发布公告数量集合);
                        }
                        else
                        {
                            adlist.Add(k, 本年各月发布公告数量集合);
                        }
                    }
                    break;
            }
            ViewData["地区"] = area;
            ViewData["区域公告"] = adlist;
            return PartialView("Procure_Part/Part_AdStatistic_ByType");
        }


        public ActionResult OperationInfo()
        {
            var _gys = 用户管理.查询用户<供应商>(0, 0);
            string[] _area = { "成都", "贵阳", "重庆", "昆明" };

            var newdic = new Dictionary<string, int>();
            var newdic1 = new Dictionary<string, int>();

            //供应商入网数量
            var dic = new Dictionary<string, Dictionary<string, int>>();
            var _qjrk = 用户管理.计数用户<供应商>(0, 0, Query<供应商>.Where(o => o.供应商用户信息.入库级别 == 供应商.入库级别.全军库));
            newdic.Add("全军入库", (int)_qjrk);

            var _cdrk = 用户管理.计数用户<供应商>(0, 0, Query<供应商>.Where(o => o.供应商用户信息.入库级别 == 供应商.入库级别.成都军区库));
            newdic.Add("成都军区入库", (int)_cdrk);
            dic.Add("入网供应商总数", newdic);

            newdic = new Dictionary<string, int>();
            foreach (var item in _area)
            {
                var _agreegys = 用户管理.计数用户<供应商>(0, 0, Query<供应商>.Where(o => o.供应商用户信息.协议供应商 && o.所属地域.城市 != null && o.所属地域.城市.Contains(item)));
                newdic.Add(item, (int)_agreegys);
                var _emergys = 用户管理.计数用户<供应商>(0, 0, Query<供应商>.Where(o => o.所属地域.城市 != null && o.所属地域.城市.Contains(item) && o.供应商用户信息.应急供应商));
                newdic1.Add(item, (int)_emergys);
        }
            dic.Add("入网协议供应商总数", newdic);
            dic.Add("入网应急供应商总数", newdic1);
            ViewData["供应商入网数量"] = dic;

            //各行业供应商入网数量
            var dic1 = new Dictionary<string, Dictionary<string, int>>();
            newdic = new Dictionary<string, int>();
            newdic1 = new Dictionary<string, int>();

            //获取商品分类 并按分类通用与专用物资
            var _tywz = new List<string>();
            var _zywz = new List<string>();
            var _classify = 商品分类管理.查找子分类(-1);
            foreach (var item in _classify)
            {
                if (item.分类性质 == 商品分类性质.通用物资) { _tywz.Add(item.分类名); }
                else { _zywz.Add(item.分类名); }
            }

            foreach (var item in _tywz)
            {
                //var _hy = 用户管理.计数用户<供应商>(0, 0, Query<供应商>.Where(o => o.可提供产品类别列表.Count > 0 && o.可提供产品类别列表.Exists(p => p.一级分类.Contains(item))));
                var _hy = _gys.Where(o => o.可提供产品类别列表.Count > 0 && o.可提供产品类别列表.Exists(p => p.一级分类.Contains(item))).Count();
                newdic.Add(item, (int)_hy);
            }
            dic1.Add("通用物资", newdic);
            foreach (var items in _zywz)
            {
                //var _hy = 用户管理.计数用户<供应商>(0,0, Query<供应商>.Where(o => o.可提供产品类别列表.Count > 0 && o.可提供产品类别列表.Exists(p => p.一级分类.Contains(items))));
                var _hy = _gys.Where(o => o.可提供产品类别列表.Count > 0 && o.可提供产品类别列表.Exists(p => p.一级分类.Contains(items))).Count();
                newdic1.Add(items, (int)_hy);
            }
            dic1.Add("专用物资", newdic1);
            ViewData["各行业供应商入网数量"] = dic1;

            return View();
        }

        public ActionResult Part_UnitOrAdStatics()
        {
            string[] _area = { "成都", "贵阳", "重庆", "昆明" };
            var newdic = new Dictionary<string, int>();

            //部队单位用户
            var dic = new Dictionary<string, Dictionary<string, int>>();
            foreach (var item in _area)
            {
                var _allaccount = 用户管理.计数用户<单位用户>(0, 0, Query<单位用户>.Where(o => o.所属地域.城市 != null && o.所属地域.城市.Contains(item)));
                newdic.Add(item, (int)_allaccount);
            }
            dic.Add("单位用户账号申请总数", newdic);

            //公告
            var _ad = 公告管理.查询公告(0, 0);

            //公告月发布数量
            var jk = new List<Tuple<string, string, string>>(); //统计月份，每月数量，某月每天数量

            ////每一年的开始时间与结束时间
            //var _yearStart = new DateTime(DateTime.Now.Year, 1, 1, 0, 0, 0);
            //var _yearEnd = new DateTime(DateTime.Now.Year, 12, 31, 23, 59, 59);
            //某一年每月的开始时间与结束时间
            var _monthStart = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1, 0, 0, 0);
            var _monthEnd = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month), 0, 0, 0);

            var _month = DateTime.Now.Month;
            var _year = DateTime.Now.Year;
            for (int i = 1; i <= 7; i++)
            {
                _monthStart.AddMonths(-1);
                _monthEnd.AddMonths(-1);
                _month--;
                //当月份倒数到前一年12月时年份减去一年，月份设为12月
                if (_month <= 0)
                {
                    _monthStart = new DateTime(DateTime.Now.Year - 1, 12, 1, 0, 0, 0);
                    _monthEnd = new DateTime(DateTime.Now.Year - 1, 12, DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month), 0, 0, 0);
                    _year -= 1;
                    _month = 12;
                }

                var _monthnum = 公告管理.计数公告(0, 0, Query<公告>.GTE(o => o.基本数据.添加时间, _monthStart).And(Query<公告>.LTE(o => o.基本数据.添加时间, _monthEnd)));
                var _perday = Math.Round(_monthnum / 30.0, 1);
                jk.Add(Tuple.Create(_month + "月", _monthnum + "条", _perday + "条"));
            }
            jk.Reverse();
            ViewData["公告月发布数量"] = jk;

            //公告发布类型
            var typs = new Dictionary<string, Dictionary<string, int>>();

            newdic = new Dictionary<string, int>();
            newdic.Add("", (int)公告管理.计数公告(0, 0, Query<公告>.Where(o => o.公告信息.公告类别 == 公告.公告类别.公开招标 &&
                                        (o.公告信息.公告性质 == 公告.公告性质.采购公告 ||
                                        o.公告信息.公告性质 == 公告.公告性质.预公告 ||
                                        o.公告信息.公告性质 == 公告.公告性质.技术公告))));
            typs.Add("公开招标类", newdic);

            newdic = new Dictionary<string, int>();
            newdic.Add("", (int)公告管理.计数公告(0, 0, Query<公告>.Where(o => o.公告信息.公告类别 == 公告.公告类别.公开招标 &&
                                          (o.公告信息.公告性质 == 公告.公告性质.中标结果公示 ||
                                          o.公告信息.公告性质 == 公告.公告性质.流标公告 ||
                                          o.公告信息.公告性质 == 公告.公告性质.废标公告))));
            typs.Add("结果公示类", newdic);

            newdic = new Dictionary<string, int>();
            newdic.Add("邀请招标类", (int)公告管理.计数公告(0, 0, Query<公告>.Where(o => o.公告信息.公告类别 == 公告.公告类别.邀请招标)));
            newdic.Add("询价类", (int)公告管理.计数公告(0, 0, Query<公告>.Where(o => o.公告信息.公告类别 == 公告.公告类别.询价采购)));
            newdic.Add("竞争性谈判类", (int)公告管理.计数公告(0, 0, Query<公告>.Where(o => o.公告信息.公告类别 == 公告.公告类别.竞争性谈判)));
            newdic.Add("协议采购类", (int)公告管理.计数公告(0, 0, Query<公告>.Where(o => o.公告信息.公告类别 == 公告.公告类别.协议采购)));
            newdic.Add("单一来源类", (int)公告管理.计数公告(0, 0, Query<公告>.Where(o => o.公告信息.公告类别 == 公告.公告类别.单一来源)));
            newdic.Add("网上竞标类", (int)公告管理.计数公告(0, 0, Query<公告>.Where(o => o.公告信息.公告类别 == 公告.公告类别.网上竞标)));
            typs.Add("采购类", newdic);
            ViewData["公告发布类型"] = typs;

            //公告发布单位
            //var _units = 公告管理.查询公告(0, 0, Query<公告>.Where(o => o.公告信息.公告类别 != 公告.公告类别.其他))
            //                .Select(o => o.公告信息.需求单位 != null ? o.公告信息.需求单位.Trim() : o.公告信息.需求单位)
            //                .Distinct();
            var _units = 公告管理.查询公告(0, 0, Query<公告>.Where(o => o.公告信息.公告类别 != 公告.公告类别.其他))
                            .Select(o => o.公告信息.需求单位)
                            .Distinct();
            var unitgg = new List<Tuple<string, int, int>>();
            foreach (var item in _units)
            {
                //某单位招投标类公告数量
                var _zbl = 公告管理.计数公告(0, 0, Query<公告>.Where(o => o.公告信息.需求单位 != null && o.公告信息.需求单位 == item &&
                                          o.公告信息.公告类别 == 公告.公告类别.公开招标 &&
                                          o.公告信息.公告性质 != 公告.公告性质.未设置));
                //某单位采购类公告数量
                var _cgl = 公告管理.计数公告(0, 0, Query<公告>.Where(o => o.公告信息.需求单位 != null && o.公告信息.需求单位 == item &&
                                          (o.公告信息.公告类别 == 公告.公告类别.单一来源 ||
                                          o.公告信息.公告类别 == 公告.公告类别.竞争性谈判 ||
                                          o.公告信息.公告类别 == 公告.公告类别.网上竞标 ||
                                          o.公告信息.公告类别 == 公告.公告类别.协议采购 ||
                                          o.公告信息.公告类别 == 公告.公告类别.询价采购 ||
                                          o.公告信息.公告类别 == 公告.公告类别.邀请招标)));
                unitgg.Add(Tuple.Create(item, (int)_zbl, (int)_cgl));
            }
            ViewData["公告发布单位"] = unitgg;
            return PartialView("Procure_Part/Part_UnitOrAdStatics");
        }

        public ActionResult Part_Procure_TreeMenu()
        {
            if (用户管理.查找用户<单位用户>(currentUser.Id).审核数据.审核状态 == 审核状态.未审核)
            {
                return Content("您的用户还未经过审核，请联系管理员进行审核！");
            }
            if (用户管理.查找用户<单位用户>(currentUser.Id).审核数据.审核状态 == 审核状态.审核未通过)
            {
                return Content("您的用户未通过审核，如有疑问请联系管理员！");
            }


            return PartialView("Procure_Part/Part_Procure_TreeMenu");
        }
        public class FindUsers
        {
            public string name { get; set; }
            public string loginName { get; set; }
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
            long pgCount = 0;
            List<FindUsers> us = new List<FindUsers>();
            IMongoQuery q = null;
            if (!string.IsNullOrEmpty(province))
            {
                q = q.And(MongoDB.Driver.Builders.Query.EQ("所属地域.省份", province));
                if (city != "不限" && !string.IsNullOrEmpty(city) && !string.IsNullOrEmpty(area) && area != "不限")
                {
                    q = q.And(MongoDB.Driver.Builders.Query.EQ("所属地域.城市", city));
                    q = q.And(MongoDB.Driver.Builders.Query.EQ("所属地域.区县", area));
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
                pgCount = sum / 12;
                if (sum % 12 > 0)
                {
                    pgCount++;
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
                        FindUsers u = new FindUsers();
                        if (string.IsNullOrWhiteSpace(Department.ElementAt(i).单位信息.单位代号))
                        {
                            u.name = Department.ElementAt(i).单位信息.单位名称;
                        }
                        else
                        {
                            u.name = Department.ElementAt(i).单位信息.单位代号;
                        }
                        u.loginName = Department.ElementAt(i).登录信息.登录名;
                        us.Add(u);
                    }
                }
            }
            else if (catorgray == "专家")
            {
                if (!string.IsNullOrWhiteSpace(username))
                {
                    q = q.And(Query.EQ("身份信息.姓名", new BsonRegularExpression(string.Format("/{0}/i", username))));
                }
                long sum = 用户管理.计数用户<专家>(0, 0, q);
                pgCount = sum / 12;
                if (sum % 12 > 0)
                {
                    pgCount++;
                }
                if (string.IsNullOrEmpty(pagesize))
                {
                    pagesize = "1";
                }
                IEnumerable<专家> Department = 用户管理.查询用户<专家>((int.Parse(pagesize) - 1) * 12, 12, q);
                if (Department != null)
                {
                    for (int i = 0; i < Department.Count(); i++)
                    {
                        FindUsers u = new FindUsers();
                        u.name = Department.ElementAt(i).身份信息.姓名;
                        u.loginName = Department.ElementAt(i).登录信息.登录名;
                        us.Add(u);
                    }
                }
            }
            else if (catorgray == "供应商")
            {
                if (!string.IsNullOrEmpty(industry))
                {
                    q = q.And(Query.EQ("企业基本信息.所属行业", industry));
                }
                if (!string.IsNullOrWhiteSpace(username))
                {
                    q = q.And(Query.EQ("企业基本信息.企业名称", new BsonRegularExpression(string.Format("/{0}/i", username))));
                    //q.And(Query.EQ("企业基本信息.企业名称",username));
                }
                long sum = 用户管理.计数用户<供应商>(0, 0, q);
                pgCount = sum / 12;
                if (sum % 12 > 0)
                {
                    pgCount++;
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
                        FindUsers u = new FindUsers();
                        u.name = Department.ElementAt(i).企业基本信息.企业名称;
                        u.loginName = Department.ElementAt(i).登录信息.登录名;
                        us.Add(u);
                    }
                }
            }
            JsonResult json = new JsonResult() { Data = new { name = us, pageCount = pgCount } };
            return Json(json, JsonRequestBehavior.AllowGet);

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
        public ActionResult Part_DepartmentList()
        {
            try
            {
                long pgCount = 0;
                int cpg = 0;
                if (!string.IsNullOrWhiteSpace(Request.QueryString["page"]))
                {
                    cpg = int.Parse(Request.QueryString["page"]);
                }
                if (cpg <= 0)
                {
                    cpg = 1;
                }
                long pc = currentUser.计数管辖单位();
                pgCount = pc / 10;
                if (pc % 10 > 0)
                {
                    pgCount++;
                }
                ViewData["Pagecount"] = pgCount;
                ViewData["CurrentPage"] = cpg;
                ViewData["user"] = currentUser.获取管辖单位(10 * (cpg - 1), 10);
                ViewData["userid"] = currentUser.Id;
                return PartialView("Procure_Part/Part_DepartmentList");
            }
            catch
            {
                return Content("<script>window.location='/单位用户后台/DepartmentList';</script>");
            }
        }
        public ActionResult Part_DepartmentModify()
        {
            string id = Request.Params["id"];
            ViewData["id"] = id;
            单位用户 unituser = 用户管理.查找用户(long.Parse(id)) as 单位用户;
            ViewData["user"] = currentUser as 单位用户;
            ViewData["用户组列表"] = 用户组管理.查询用户组(0, 0);
            bool IsActor = false;
            foreach (var j in currentUser.用户组)
            {
                if (j.Contains("操作员"))
                {
                    IsActor = true;
                }
            }
            ViewData["所管理用户组"] = 用户组管理.查询用户组(0, 0, Query<用户组>.Where(o => o.所属单位.用户ID == currentUser.Id));
            ViewBag.isActor = IsActor;
            return PartialView("Procure_Part/Part_DepartmentModify", unituser);
        }
        public void DepartmentEdit()
        {
            string id = Request.Params["a"];//待修改单位用户id
            string pwd = Request.Params["b"];//待修改单位用户密码
            string brief = Request.Params["c"];//待修改单位用户简称
            string jb = Request.Params["d"];//待修改单位用户级别
            string yhz = Request.Params["e"];//待修改单位用户用户组
            if (pwd != null && pwd != "")
            {
                用户管理.修改登录密码<单位用户>(long.Parse(id), pwd);
            }
            var user = 用户管理.查找用户<单位用户>(long.Parse(id));
            user.单位信息.单位代号 = brief;
            单位用户.单位级别 dwjb;
            user.单位信息.单位级别 = Enum.TryParse(jb, true, out dwjb)
                ? dwjb
                : 单位用户.单位级别.未设置
                ;

            var _f = yhz.Split(',');
            user.用户组.Clear();
            for (int i = 0; i < _f.Length - 1; i++)
            {

                user.用户组.Add(_f[i]);
            }

            用户管理.更新用户<单位用户>(user);
        }
        ////public string CheckCode()
        ////{
        ////    string code = Request.Cookies["code"].ToString();
        ////    return code;
        ////}
        public ActionResult Part_DepartmentAdd()
        {
            ViewData["jsonUser"] = JsonConvert.SerializeObject(单位用户.单位级别列表);
            ViewData["用户组列表"] = 用户组管理.查询用户组(0, 0);
            //bool IsActor = false;
            //foreach (var j in currentUser.用户组)
            //{
            //    if (j.Contains("操作员"))
            //    {
            //        IsActor = true;
            //    }
            //}
            //ViewData["所管理用户组"] = 用户组管理.查询用户组(0, 0, Query<用户组>.Where(o => o.所属单位.用户ID == currentUser.Id));
            //ViewBag.isActor = IsActor;
            return PartialView("Procure_Part/Part_DepartmentAdd");
        }
        public class User
        {
            public long Id { get; set; }
            public string Name { get; set; }
        }
        public ActionResult SearchUser()
        {
            try
            {
                long id = long.Parse(Request.QueryString["id"]);
                IEnumerable<单位用户> model = 用户管理.查询用户<单位用户>(0, 0, Query<单位用户>.Where(m => m.所属单位.用户ID == id));
                List<User> us = new List<User>();
                foreach (var item in model)
                {
                    User u = new User();
                    u.Id = item.Id;
                    if (!string.IsNullOrWhiteSpace(item.单位信息.单位代号))
                    {
                        u.Name = item.单位信息.单位代号;
                    }
                    else
                    {
                        u.Name = item.单位信息.单位名称;
                    }
                    us.Add(u);
                }
                JsonResult json = new JsonResult() { Data = us };
                return Json(json, JsonRequestBehavior.AllowGet);
            }
            catch
            {
                return Redirect("/单位用户后台/");
            }
        }
        //public ActionResult Part_Depart_GysList()
        //{
        //    return PartialView("Procure_Part/Part_Depart_GysList");
        //}

        //public ActionResult Part_Zb_ReleaseInfo()
        //{
        //    return PartialView("Procure_Part/Part_Zb_ReleaseInfo");
        //}
        public ActionResult Part_Procure_Xttz()
        {
            long pgCount = 0;
            int cpg = 0;
            if (!string.IsNullOrWhiteSpace(Request.QueryString["page"]))
            {
                cpg = int.Parse(Request.QueryString["page"]);
            }
            if (cpg <= 0)
            {
                cpg = 1;
            }
            long pc = 通知管理.计数通知(0, -1);
            pgCount = pc / 10;
            if (pc % 10 > 0)
            {
                pgCount++;
            }
            ViewData["Pagecount"] = pgCount;
            ViewData["CurrentPage"] = cpg;
            ViewData["通知管理"] = 通知管理.查询通知(10 * (cpg - 1), 10);
            return PartialView("Procure_Part/Part_Procure_Xttz");
        }
        public ActionResult Znxx_Sended_Detail()
        {
            return View();
        }
        public ActionResult Part_Znxx_Sended_Detail()
        {
            try
            {
                ViewData["当前用户"] = currentUser.登录信息.登录名;
                long id = long.Parse(Request.QueryString["id"].ToString());
                站内消息管理.更新已读时间(id, true, DateTime.Now);
                站内消息 model = 站内消息管理.查找站内消息(id);
                return PartialView("Procure_Part/Part_Znxx_Sended_Detail", model);
            }
            catch
            {
                return Content("<script>window.location='/单位用户后台/Procure_ZnxxSend';</script>");
            }

        }
        public ActionResult Xttz_Detail()
        {
            return View();
        }
        public ActionResult Part_Msg_Detail(int id)
        {
            投诉 model = 投诉管理.查找投诉(id);
            return PartialView("Procure_Part/Part_Msg_Detail", model);
        }
        public ActionResult Msg_Detail()
        {
            return View();
        }
        //public ActionResult ComplainAdd()
        //{
        //    return View();
        //}
        public ActionResult Part_Xttz_Detail()
        {
            try
            {
                int id = int.Parse(Request.QueryString["id"].ToString());
                var model = 通知管理.查找通知(id);
                return PartialView("Procure_Part/Part_Xttz_Detail", model);
            }
            catch
            {
                return Content("<script>window.location='/单位用户后台/Procure_Xttz';</script>");
            }
        }
        public ActionResult Produre_Reply()
        {
            return View();
        }
        public ActionResult Part_Znxx_Reply()
        {
            try
            {
                long id = long.Parse(Request.QueryString["id"].ToString());
                站内消息管理.更新已读时间(id, false, DateTime.Now);
                var model = 站内消息管理.查找站内消息(id);
                if (model != null)
                {
                    ViewData["当前用户"] = currentUser.登录信息.登录名;
                    return PartialView("Procure_Part/Part_Znxx_Reply", model);
                }
                else
                {
                    return Content("<script>window.location='/单位用户后台/Procure_Znxx';</script>");
                }

            }
            catch
            {
                return Content("<script>window.location='/单位用户后台/Procure_Znxx';</script>");
            }

        }
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Znxx_Msg_Reply(站内消息 model)
        {
            string contents = Request.Form["reply"].ToString();
            var item = new 对话消息();
            foreach (var m in model.对话消息)
            {
                item.发言人.用户ID = currentUser.Id;
                item.消息主体.标题 = m.消息主体.标题;
                item.消息主体.内容 = contents;
            }
            对话消息管理.添加对话消息(item, 站内消息管理.查找站内消息(model.Id));
            return Content("<script>window.location='/单位用户后台/Procure_Znxx';</script>");
        }
        public ActionResult Part_Procure_Znxx()
        {
            try
            {
                long pgCount = 0;
                int cpg = 0;
                if (!string.IsNullOrWhiteSpace(Request.QueryString["page"]))
                {
                    cpg = int.Parse(Request.QueryString["page"]);
                }
                if (cpg <= 0)
                {
                    cpg = 1;
                }
                long pc = 站内消息管理.计数站内消息(0, 0, Query<站内消息>.Where(m => m.收信人.用户ID == currentUser.Id));
                pgCount = pc / 10;
                if (pc % 10 > 0)
                {
                    pgCount++;
                }
                ViewData["Pagecount"] = pgCount;
                ViewData["CurrentPage"] = cpg;
                ViewData["站内消息列表"] = 站内消息管理.查询站内消息(10 * (cpg - 1), 10, Query<站内消息>.Where(o => o.收信人.用户ID == currentUser.Id));
                return PartialView("Procure_Part/Part_Procure_Znxx");
            }
            catch
            {
                return Content("<script>window.location='/单位用户后台/Procure_Znxx';</script>");
            }
        }
        //public ActionResult Procure_Del_Complain(int id)
        //{
        //    投诉管理.删除投诉(id);
        //    return Content("<script>window.location='/单位用户后台/Procure_ComplainList';</script>");
        //}
        public ActionResult ComplainDetail()
        {
            return View();
        }
        public ActionResult Part_ComplainDetail()
        {
            return PartialView("Procure_Part/Part_ComplainDetail");
        }
        public ActionResult Procure_Del_AdDetail()
        {
            try
            {
                long id = long.Parse(Request.QueryString["id"].ToString());
                站内消息管理.删除站内消息(id);
                return Redirect("/单位用户后台/Procure_ZnxxSend");
            }
            catch
            {
                return Redirect("/单位用户后台/Procure_ZnxxSend");
            }

        }
        public ActionResult Part_Procure_ZnxxSend()
        {
            try
            {
                long pgCount = 0;
                int cpg = 0;
                if (!string.IsNullOrWhiteSpace(Request.QueryString["page"]))
                {
                    cpg = int.Parse(Request.QueryString["page"]);
                }
                if (cpg <= 0)
                {
                    cpg = 1;
                }
                long pc = 站内消息管理.计数站内消息(0, 0, Query<站内消息>.Where(m => m.发起者.用户ID == currentUser.Id));
                pgCount = pc / 10;
                if (pc % 10 > 0)
                {
                    pgCount++;
                }
                ViewData["Pagecount"] = pgCount;
                ViewData["CurrentPage"] = cpg;
                ViewData["znxx"] = 站内消息管理.查询站内消息(10 * (cpg - 1), 10, Query<站内消息>.Where(o => o.发起者.用户ID == currentUser.Id)).OrderByDescending(m => m.重要程度).OrderByDescending(m => m.基本数据.修改时间);
                return PartialView("Procure_Part/Part_Procure_ZnxxSend");
            }
            catch
            {
                return Content("<script>window.location='/单位用户后台/Procure_ZnxxSend';</script>");
            }
        }
        public ActionResult Delete_ZNXX()
        {
            try
            {
                long id = long.Parse(Request.QueryString["id"].ToString());
                站内消息管理.删除站内消息(id);
                return Redirect("/单位用户后台/Procure_Znxx");
            }
            catch
            {
                return Redirect("/单位用户后台/Procure_Znxx");
            }

        }
        [ValidateInput(false)]

        public string ZnxxAdd()
        {
            string[] rs = Request.QueryString["r"].ToString().Split(',');
            for (int i = 0; i < rs.Length - 1; i++)
            {
                try
                {
                    if (用户管理.检查用户是否存在(rs[i]) != -1)
                    {
                        站内消息 Msg = new 站内消息();
                        对话消息 Talk = new 对话消息();
                        Msg.收信人.用户ID = 用户管理.检查用户是否存在(rs[i]);
                        int level = int.Parse(Request.QueryString["l"]);
                        int mType = int.Parse(Request.QueryString["m"]);
                        switch (level)
                        {
                            case 0: Msg.重要程度 = 重要程度.未指定; break;
                            case 100: Msg.重要程度 = 重要程度.一般; break;
                            case 200: Msg.重要程度 = 重要程度.重要; break;
                            case 300: Msg.重要程度 = 重要程度.特别重要; break;
                            case 400: Msg.重要程度 = 重要程度.必读; break;
                        }
                        switch (mType)
                        {
                            case 0: Msg.消息类型 = 消息类型.未指定; break;
                            case 100: Msg.消息类型 = 消息类型.普通; break;
                            case 200: Msg.消息类型 = 消息类型.采购信息; break;
                            case 300: Msg.消息类型 = 消息类型.推荐专家和供应商; break;
                        }
                        Msg.发起者.用户ID = currentUser.Id;
                        Talk.发言人.用户ID = currentUser.Id;
                        站内消息管理.添加站内消息(Msg, Msg.发起者.用户ID, Msg.收信人.用户ID);
                        Talk.消息主体.标题 = Request.QueryString["t"];
                        Talk.消息主体.内容 = Request.QueryString["c"];
                        对话消息管理.添加对话消息(Talk, Msg);
                    }
                }
                catch
                {
                    return "发送站内消息失败！";
                }
            }
            return "发送站内消息成功";
        }
        public string ZnxxAdd_xy()
        {

            IEnumerable<供应商> model = 用户管理.查询用户<供应商>(0, 0, Query<供应商>.Where(m => m.供应商用户信息.协议供应商 == true));
            for (int i = 0; i < model.Count(); i++)
            {
                try
                {

                    站内消息 Msg = new 站内消息();
                    对话消息 Talk = new 对话消息();
                    Msg.收信人.用户ID = model.ElementAt(i).Id;
                    int level = int.Parse(Request.QueryString["l"]);
                    int mType = int.Parse(Request.QueryString["m"]);
                    switch (level)
                    {
                        case 0: Msg.重要程度 = 重要程度.未指定; break;
                        case 100: Msg.重要程度 = 重要程度.一般; break;
                        case 200: Msg.重要程度 = 重要程度.重要; break;
                        case 300: Msg.重要程度 = 重要程度.特别重要; break;
                        case 400: Msg.重要程度 = 重要程度.必读; break;
                    }
                    switch (mType)
                    {
                        case 0: Msg.消息类型 = 消息类型.未指定; break;
                        case 100: Msg.消息类型 = 消息类型.普通; break;
                        case 200: Msg.消息类型 = 消息类型.采购信息; break;
                        case 300: Msg.消息类型 = 消息类型.推荐专家和供应商; break;
                    }
                    Msg.发起者.用户ID = currentUser.Id;
                    Talk.发言人.用户ID = currentUser.Id;
                    站内消息管理.添加站内消息(Msg, Msg.发起者.用户ID, Msg.收信人.用户ID);
                    Talk.消息主体.标题 = Request.QueryString["t"];
                    Talk.消息主体.内容 = Request.QueryString["c"];
                    对话消息管理.添加对话消息(Talk, Msg);
                }
                catch
                {
                    return "发送站内消息失败！";
                }
            }
            return "发送站内消息成功";
        }
        public ActionResult Part_Procure_ZnxxAdd()
        {
            ViewData["行业"] = 商品分类管理.查找子分类();
            ViewData["id"] = currentUser.Id;
            return PartialView("Procure_Part/Part_Procure_ZnxxAdd");
        }
        public ActionResult Part_Procure_AdList()
        {
            try
            {
                long pgCount = 0;
                int cpg = 0;
                if (!string.IsNullOrWhiteSpace(Request.QueryString["page"]))
                {
                    cpg = int.Parse(Request.QueryString["page"]);
                }
                if (cpg <= 0)
                {
                    cpg = 1;
                }
                long pc = 公告管理.计数公告(0, -1);
                pgCount = pc / 10;
                if (pc % 10 > 0)
                {
                    pgCount++;
                }
                ViewData["Pagecount"] = pgCount;
                ViewData["CurrentPage"] = cpg;
                ViewData["后台公告列表"] = 公告管理.查询公告(10 * (cpg - 1), 10);
                return PartialView("Procure_Part/Part_Procure_AdList");
            }
            catch
            {
                return Content("/单位用户后台/Procure_AdList");
            }
        }
        public ActionResult Part_Procure_AdAudit()
        {
            long pgCount = 0;
            int cpg = 0;
            if (!string.IsNullOrWhiteSpace(Request.QueryString["page"]))
            {
                cpg = int.Parse(Request.QueryString["page"]);
            }
            if (cpg <= 0)
            {
                cpg = 1;
            }
            long pc = 公告管理.计数公告(0, -1, Query.EQ("审核数据.审核状态", 审核状态.未审核));
            pgCount = pc / 10;
            if (pc % 10 > 0)
            {
                pgCount++;
            }
            ViewData["Pagecount"] = pgCount;
            ViewData["CurrentPage"] = cpg;
            ViewData["未审核公告列表"] = 公告管理.查询公告(10 * (cpg - 1), 10, Query.EQ("审核数据.审核状态", 审核状态.未审核));
            return PartialView("Procure_Part/Part_Procure_AdAudit");
        }
        public ActionResult Part_Procure_ZnxxDetail(int id)
        {
            var model = 站内消息管理.查找站内消息(id);
            return PartialView("Procure_Part/Part_Procure_ZnxxDetail", model);
        }
        public ActionResult Procure_ZnxxDetail(int id)
        {
            return View();
        }
        public ActionResult Part_Procure_AdDetail(int? id)
        {
            公告 model = null;
            if (id != null)
            {
                try
                {
                    model = 公告管理.查找公告((long)id);
                    var c = Request.QueryString["come"];
                    if (null != c && c == "s")
                    {
                        ViewData["come"] = "我的采购公告";
                    }
                    else
                    {
                        ViewData["come"] = "审核采购公告";
                    }

                    //如果为自己的公告，则不能自己进行审核
                    if (model.内容基本信息.所有者.用户ID == currentUser.Id)
                    {
                        ViewData["s"] = "1";
                    }
                    else
                    {
                        ViewData["s"] = "0";
                    }
                }
                catch
                {

                }
            }
            var ybm = 招标采购预报名管理.查询招标采购预报名(0, 0).Select(o => o.所属公告链接.公告ID);

            if (ybm.Contains(model.Id))
            {
                ViewData["开启预报名"] = "1";
            }
            return PartialView("Procure_Part/Part_Procure_AdDetail", model);
        }
        public ActionResult Part_AdListDetail(int? id)
        {
            公告 model = null;
            if (id != null)
            {
                try
                {
                    model = 公告管理.查找公告((long)id);
                }
                catch
                {

                }
            }
            var ybm = 招标采购预报名管理.查询招标采购预报名(0, 0).Select(o => o.所属公告链接.公告ID);

            if (ybm.Contains(model.Id))
            {
                ViewData["开启预报名"] = "1";
            }
            return PartialView("Procure_Part/Part_AdListDetail", model);
        }
        public ActionResult AdListDetail(int? id)
        {
            return View();
        }
        public ActionResult Part_Procure_NewsList()
        {
            long pgCount = 0;
            int cpg = 0;
            if (!string.IsNullOrWhiteSpace(Request.QueryString["page"]))
            {
                cpg = int.Parse(Request.QueryString["page"]);
            }
            if (cpg <= 0)
            {
                cpg = 1;
            }
            long pc = 新闻管理.计数新闻(0, -1);
            pgCount = pc / 10;
            if (pc % 10 > 0)
            {
                pgCount++;
            }
            ViewData["Pagecount"] = pgCount;
            ViewData["CurrentPage"] = cpg;
            ViewData["后台新闻列表"] = 新闻管理.查询新闻(10 * (cpg - 1), 10);

            return PartialView("Procure_Part/Part_Procure_NewsList");
        }
        public ActionResult Part_Procure_NewsList_S()
        {
            long pgCount = 0;
            int cpg = 0;
            if (!string.IsNullOrWhiteSpace(Request.QueryString["page"]))
            {
                cpg = int.Parse(Request.QueryString["page"]);
            }
            if (cpg <= 0)
            {
                cpg = 1;
            }
            long pc = 新闻管理.计数新闻(0, 0, Query<新闻>.Where(m => m.内容基本信息.所有者.用户ID == currentUser.Id));
            pgCount = pc / 10;
            if (pc % 10 > 0)
            {
                pgCount++;
            }
            ViewData["Pagecount"] = pgCount;
            ViewData["CurrentPage"] = cpg;
            ViewData["后台新闻列表"] = 新闻管理.查询新闻(10 * (cpg - 1), 10, Query<新闻>.EQ(o => o.内容基本信息.所有者.用户ID, currentUser.Id));

            return PartialView("Procure_Part/Part_Procure_NewsList_S");
        }
        public ActionResult Part_Procure_NewsList_Audit()
        {
            long pgCount = 0;
            int cpg = 0;
            if (!string.IsNullOrWhiteSpace(Request.QueryString["page"]))
            {
                cpg = int.Parse(Request.QueryString["page"]);
            }
            if (cpg <= 0)
            {
                cpg = 1;
            }
            long pc = 新闻管理.计数新闻(0, 0, Query<新闻>.EQ(o => o.审核数据.审核者.用户ID, currentUser.Id));
            pgCount = pc / 10;
            if (pc % 10 > 0)
            {
                pgCount++;
            }
            ViewData["Pagecount"] = pgCount;
            ViewData["CurrentPage"] = cpg;
            ViewData["后台新闻列表"] = 新闻管理.查询新闻(10 * (cpg - 1), 10, Query<新闻>.EQ(o => o.审核数据.审核者.用户ID, currentUser.Id));

            return PartialView("Procure_Part/Part_Procure_NewsList_Audit");
        }
        public ActionResult Part_Procure_NewsList_AuditDetail(int? id)
        {
            新闻 g = new 新闻();
            if (null != id)
            {
                g = 新闻管理.查找新闻((long)id);
            }
            if (g == null)
            {
                return Content("<script>window.location='/单位用户后台/Procure_NewsList_Audit';</script>");
            }
            return PartialView("Procure_Part/Part_Procure_NewsList_AuditDetail", g);
        }
        [HttpPost]
        public ActionResult Procure_NewsList_AuditDetail(string action, long? id)
        {
            try
            {
                deleteIndex("/Lucene.Net/IndexDic/News", id.ToString());
                var model = 新闻管理.查找新闻((long)id);
                if (action == "审核通过")
                {
                    model.审核数据.审核状态 = 审核状态.审核通过;
                    model.审核数据.审核时间 = DateTime.Now;
                }
                else if (action == "审核不通过")
                {
                    model.审核数据.审核时间 = DateTime.Now;
                    model.审核数据.审核状态 = 审核状态.审核未通过;
                    model.审核数据.未通过理由 = Request.Form["reason"];
                }
                新闻管理.更新新闻(model, false);
                if (model.审核数据.审核状态 == 审核状态.审核通过)
                {
                    CreateIndex(model, "/Lucene.Net/IndexDic/News");
                }
            }
            catch
            {

            }
            return View("Procure_NewsList_Audit");
        }
        public ActionResult Part_Procure_NewsDetail(int? id)
        {
            新闻 g = new 新闻();
            var come = Request.QueryString["come"];
            var comestr = "我的新闻列表";
            if (come == "a")
            {
                comestr = "审核新闻";
            }
            else if (come == "l")
            {
                comestr = "新闻列表";
            }
            if (null != id)
            {
                g = 新闻管理.查找新闻((long)id);
            }
            if (g == null)
            {
                return Content("<script>window.location='/单位用户后台/Procure_NewsList';</script>");
            }
            ViewData["come"] = comestr;
            return PartialView("Procure_Part/Part_Procure_NewsDetail", g);
        }

        public ActionResult Gys_Examine(int? page)
        {
            IEnumerable<供应商> gyslist = null;
            IMongoQuery q = null;
            if (currentUser.Id == 11)
            {
                q = Query<供应商>.Where(
                    o => o.审核数据.审核状态 == 审核状态.审核通过 &&
                         o.供应商用户信息.初审数据.审核状态 == 审核状态.未审核 &&
                         o.所属地域.省份.Contains("四川")
                         );
            }
            if (currentUser.Id == 12)
            {
                q = Query<供应商>.Where(
                    o => o.审核数据.审核状态 == 审核状态.审核通过 &&
                         o.供应商用户信息.初审数据.审核状态 == 审核状态.未审核 &&
                         o.所属地域.省份.Contains("重庆")
                         );
            }
            if (currentUser.Id == 13)
            {
                q = Query<供应商>.Where(
                    o => o.审核数据.审核状态 == 审核状态.审核通过 &&
                         o.供应商用户信息.初审数据.审核状态 == 审核状态.未审核 &&
                         o.所属地域.省份.Contains("云南")
                         );
            }
            if (currentUser.Id == 14)
            {
                q = Query<供应商>.Where(
                    o => o.审核数据.审核状态 == 审核状态.审核通过 &&
                         o.供应商用户信息.初审数据.审核状态 == 审核状态.未审核 &&
                         o.所属地域.省份.Contains("贵州")
                         );
            }
            if (currentUser.Id == 15)
            {
                q = Query<供应商>.Where(
                    o => o.审核数据.审核状态 == 审核状态.审核通过 &&
                         o.供应商用户信息.初审数据.审核状态 == 审核状态.未审核 &&
                         o.所属地域.省份.Contains("西藏")
                         );
            }
            if (currentUser.Id == 10002)
            {
                q = Query<供应商>.Where(
                    o => o.审核数据.审核状态 == 审核状态.审核通过 &&
                         o.供应商用户信息.初审数据.审核状态 == 审核状态.审核通过 &&
                         o.供应商用户信息.复审数据.审核状态 == 审核状态.未审核
                         );
            }

            int listcount = (int)用户管理.计数用户<供应商>(0, 0, q);
            int maxpage = Math.Max((listcount + GYS_EXAMINE_PAGESIZE - 1) / GYS_EXAMINE_PAGESIZE, 1);
            if (string.IsNullOrEmpty(page.ToString()) || page < 0 || page > maxpage)
            {
                page = 1;
            }
            ViewData["currentpage"] = page;
            ViewData["pagecount"] = maxpage;
            ViewData["listcount"] = listcount;
            ViewData["pagesize"] = GYS_EXAMINE_PAGESIZE;
            gyslist = 用户管理.查询用户<供应商>(GYS_EXAMINE_PAGESIZE * (int.Parse(page.ToString()) - 1), GYS_EXAMINE_PAGESIZE, q);
            return View(gyslist);
        }

        [单一权限验证(权限.推荐流程说明)]
        public ActionResult Gys_Expert_Recomed()
        {
            return View();
        }

        public ActionResult Gys_Examined(int? page)
        {

            IEnumerable<供应商> gyslist = null;
            IMongoQuery q = null;
            if (currentUser.Id == 11)
            {
                q = Query<供应商>.Where(
                    o => o.审核数据.审核状态 == 审核状态.审核通过 &&
                         o.供应商用户信息.初审数据.审核状态 != 审核状态.未审核 &&
                         o.所属地域.省份.Contains("四川")
                         );
            }
            if (currentUser.Id == 12)
            {
                q = Query<供应商>.Where(
                    o => o.审核数据.审核状态 == 审核状态.审核通过 &&
                          o.供应商用户信息.初审数据.审核状态 != 审核状态.未审核 &&
                         o.所属地域.省份.Contains("重庆")
                         );
            }
            if (currentUser.Id == 13)
            {
                q = Query<供应商>.Where(
                    o => o.审核数据.审核状态 == 审核状态.审核通过 &&
                          o.供应商用户信息.初审数据.审核状态 != 审核状态.未审核 &&
                         o.所属地域.省份.Contains("云南")
                         );
            }
            if (currentUser.Id == 14)
            {
                q = Query<供应商>.Where(
                    o => o.审核数据.审核状态 == 审核状态.审核通过 &&
                         o.供应商用户信息.初审数据.审核状态 != 审核状态.未审核 &&
                         o.所属地域.省份.Contains("贵州")
                         );
            }
            if (currentUser.Id == 15)
            {
                q = Query<供应商>.Where(
                    o => o.审核数据.审核状态 == 审核状态.审核通过 &&
                          o.供应商用户信息.初审数据.审核状态 != 审核状态.未审核 &&
                         o.所属地域.省份.Contains("西藏")
                         );
            }
            if (currentUser.Id == 10002)
            {
                q = Query<供应商>.Where(
                    o => o.审核数据.审核状态 == 审核状态.审核通过 &&
                         o.供应商用户信息.初审数据.审核状态 == 审核状态.审核通过 &&
                         o.供应商用户信息.复审数据.审核状态 != 审核状态.未审核
                         );
            }

            int listcount = (int)用户管理.计数用户<供应商>(0, 0, q);
            int maxpage = Math.Max((listcount + GYS_EXAMINE_PAGESIZE - 1) / GYS_EXAMINE_PAGESIZE, 1);
            if (string.IsNullOrEmpty(page.ToString()) || page < 0 || page > maxpage)
            {
                page = 1;
            }
            ViewData["currentpage"] = page;
            ViewData["pagecount"] = maxpage;
            ViewData["listcount"] = listcount;
            ViewData["pagesize"] = GYS_EXAMINE_PAGESIZE;
            gyslist = 用户管理.查询用户<供应商>(GYS_EXAMINE_PAGESIZE * (int.Parse(page.ToString()) - 1), GYS_EXAMINE_PAGESIZE, q);
            return View(gyslist);
        }

        public ActionResult Gys_Detail()
        {
            var isExam = Request.Params["a"];
            if (!string.IsNullOrWhiteSpace(isExam))
            {
                ViewData["已审核"] = "1";
            }
            else
            {
                ViewData["已审核"] = "0";
            }

            long id = long.Parse(Request.QueryString["id"]);
            供应商 model = 用户管理.查找用户<供应商>(id) as 供应商;
            return View(model);
        }
        public ActionResult Supplier_Accept(供应商 model)
        {
            try
            {
                供应商 supplier = 用户管理.查找用户<供应商>(model.Id);
                if (currentUser.Id == 10002)
                {
                    supplier.供应商用户信息.复审数据.审核状态 = 审核状态.审核通过;
                    supplier.供应商用户信息.复审数据.审核时间 = DateTime.Now;
                    supplier.供应商用户信息.复审数据.审核者.用户ID = currentUser.Id;
                    supplier.供应商用户信息.复审数据.审核不通过原因 = "";
                    用户管理.更新用户<供应商>(supplier);
                }
                else
                {
                    supplier.供应商用户信息.初审数据.审核状态 = 审核状态.审核通过;
                    supplier.供应商用户信息.初审数据.审核时间 = DateTime.Now;
                    supplier.供应商用户信息.初审数据.审核者.用户ID = currentUser.Id;
                    supplier.供应商用户信息.初审数据.审核不通过原因 = "";
                    用户管理.更新用户<供应商>(supplier);
                }
            }
            catch (Exception)
            {
                return Redirect("/单位用户后台/Gys_Examine");
            }
            return Content("<script>window.location='/单位用户后台/Gys_Examine';</script>");
        }

        public ActionResult Supplier_Refused(供应商 model)//拒绝供应商的加入
        {
            供应商 Newmodel = 用户管理.查找用户<供应商>(model.Id);
            if (currentUser.Id == 10002)
            {
                Newmodel.供应商用户信息.复审数据.审核不通过原因 = model.供应商用户信息.初审数据.审核不通过原因;
                Newmodel.供应商用户信息.复审数据.审核状态 = 审核状态.审核未通过;
                Newmodel.供应商用户信息.复审数据.审核者.用户ID = currentUser.Id;
                Newmodel.供应商用户信息.已提交 = false;
                用户管理.更新用户<供应商>(Newmodel);
            }
            else
            {
                Newmodel.供应商用户信息.初审数据.审核不通过原因 = model.供应商用户信息.初审数据.审核不通过原因;
                Newmodel.供应商用户信息.初审数据.审核状态 = 审核状态.审核未通过;
                Newmodel.供应商用户信息.初审数据.审核者.用户ID = currentUser.Id;
                Newmodel.供应商用户信息.已提交 = false;
                用户管理.更新用户<供应商>(Newmodel);
            }

            deleteIndex("/Lucene.Net/IndexDic/Gys", model.Id.ToString());
            return Content("<script>window.location='/单位用户后台/Gys_Examine';</script>");
        }

        public ActionResult Part_Procure_AdModify(int? id)
        {
            公告 GongGaoModel = null;

            try
            {
                ViewData["行业列表"] = 商品分类管理.查找子分类();
                GongGaoModel = 公告管理.查找公告((long)id);
                if (!string.IsNullOrEmpty(GongGaoModel.公告信息.一级分类))
                {
                    ViewData["行业列表_s"] = 商品分类管理.查找子分类(商品分类管理.查找分类(GongGaoModel.公告信息.一级分类).Id);
                }
                if (!string.IsNullOrEmpty(GongGaoModel.公告信息.二级分类))
                {
                    ViewData["行业列表_t"] = 商品分类管理.查找子分类(商品分类管理.查找分类(GongGaoModel.公告信息.二级分类).Id);
                }

            }
            catch
            {

            }
            return PartialView("Procure_Part/Part_Procure_AdModify", GongGaoModel);
        }
        public ActionResult Part_Procure_AdAdd()
        {
            ViewData["行业列表"] = 商品分类管理.查找子分类();

            IEnumerable<招标采购项目> l = 招标采购项目管理.查询招标采购项目(0, 0, MongoDB.Driver.Builders.Query.EQ("审核数据.审核状态", 审核状态.审核通过).And(MongoDB.Driver.Builders.Query.EQ("中标公告链接.公告ID", -1)).And(MongoDB.Driver.Builders.Query.EQ("流标公告链接.公告ID", -1)).And(MongoDB.Driver.Builders.Query.EQ("废标公告链接.公告ID", -1)));

            ViewData["需求列表"] = l;
            if (currentUser.管理单位.用户ID == 10)
            {
                ViewData["状态"] = "发        布";
            }
            else
            {
                ViewData["状态"] = "提        交";
            }
            string name = "";
            if (!string.IsNullOrWhiteSpace(currentUser.单位信息.单位代号))
            {
                name = currentUser.单位信息.单位代号;
            }
            else
            {
                name = currentUser.单位信息.单位名称;
            }
            ViewData["user"] = name;
            return PartialView("Procure_Part/Part_Procure_AdAdd");
        }
        public ActionResult Part_Procure_NewsModify(int? id)
        {
            新闻 g = null;

            try
            {
                g = 新闻管理.查找新闻((long)id);
            }
            catch
            {

            }
            return PartialView("Procure_Part/Part_Procure_NewsModify", g);
        }
        public ActionResult Part_Procure_NewsAdd()
        {
            return PartialView("Procure_Part/Part_Procure_NewsAdd");
        }
        public ActionResult Part_Procure_TzAdd()
        {
            return PartialView("Procure_Part/Part_Procure_TzAdd");
        }
        public ActionResult Part_Procure_TzDetail(int? id)
        {
            通知 g = new 通知();
            var come = Request.QueryString["come"];
            var comestr = "我的通知列表";
            if (come == "a")
            {
                comestr = "审核通知";
            }
            else if (come == "l")
            {
                comestr = "通知列表";
            }
            if (null != id)
            {
                g = 通知管理.查找通知((long)id);
                if (g == null)
                {
                    return Content("<script>window.location='/单位用户后台/Procure_TzList';</script>");
                }
            }
            ViewData["come"] = comestr;
            return PartialView("Procure_Part/Part_Procure_TzDetail", g);
        }
        public ActionResult Part_Procure_TzList_AuditDetail(int? id)
        {
            通知 g = new 通知();
            if (null != id)
            {
                g = 通知管理.查找通知((long)id);
                if (g == null)
                {
                    return Content("<script>window.location='/单位用户后台/Procure_TzList';</script>");
                }
            }
            return PartialView("Procure_Part/Part_Procure_TzList_AuditDetail", g);
        }
        public ActionResult Part_Procure_TzList()
        {
            long pgCount = 0;
            int cpg = 0;
            if (!string.IsNullOrWhiteSpace(Request.QueryString["page"]))
            {
                cpg = int.Parse(Request.QueryString["page"]);
            }
            if (cpg <= 0)
            {
                cpg = 1;
            }
            long pc = 通知管理.计数通知(0, 0);
            pgCount = pc / 10;
            if (pc % 10 > 0)
            {
                pgCount++;
            }
            ViewData["Pagecount"] = pgCount;
            ViewData["CurrentPage"] = cpg;
            ViewData["后台通知列表"] = 通知管理.查询通知(10 * (cpg - 1), 10);
            return PartialView("Procure_Part/Part_Procure_TzList");
        }
        public ActionResult Part_Procure_TzList_S()
        {
            long pgCount = 0;
            int cpg = 0;
            if (!string.IsNullOrWhiteSpace(Request.QueryString["page"]))
            {
                cpg = int.Parse(Request.QueryString["page"]);
            }
            if (cpg <= 0)
            {
                cpg = 1;
            }
            long pc = 通知管理.计数通知(0, 0, Query<通知>.EQ(o => o.内容基本信息.所有者.用户ID, currentUser.Id));
            pgCount = pc / 10;
            if (pc % 10 > 0)
            {
                pgCount++;
            }
            ViewData["Pagecount"] = pgCount;
            ViewData["CurrentPage"] = cpg;
            ViewData["后台通知列表"] = 通知管理.查询通知(10 * (cpg - 1), 10, Query<通知>.EQ(o => o.内容基本信息.所有者.用户ID, currentUser.Id));

            return PartialView("Procure_Part/Part_Procure_TzList_S");
        }
        public ActionResult Part_Procure_TzList_Audit(int? page)
        {

            int TZ_LISTCOUNT = (int)(通知管理.计数通知(0, 0, Query<通知>.EQ(o => o.审核数据.审核者.用户ID, currentUser.Id)));
            int TZ_MAXPAGE = Math.Max((TZ_LISTCOUNT + TZ_PAGESIZE - 1) / TZ_PAGESIZE, 1);

            if (string.IsNullOrEmpty(page.ToString()) || page < 0 || page > TZ_MAXPAGE)
            {
                page = 1;
            }

            ViewData["listcount"] = TZ_LISTCOUNT;
            ViewData["page"] = page;
            ViewData["pagesize"] = 15;

            ViewData["后台通知列表"] = 通知管理.查询通知(15 * (int.Parse(page.ToString()) - 1), 15, Query<通知>.EQ(o => o.审核数据.审核者.用户ID, currentUser.Id));

            return PartialView("Procure_Part/Part_Procure_TzList_Audit");
        }
        public ActionResult Part_Procure_TzModify(int? id)
        {
            通知 g = null;
            try
            {
                g = 通知管理.查找通知((long)id);
            }
            catch
            {

            }
            return PartialView("Procure_Part/Part_Procure_TzModify", g);
        }
        public ActionResult Part_Procure_ZcfgAdd()
        {
            return PartialView("Procure_Part/Part_Procure_ZcfgAdd");
        }
        public ActionResult Part_Procure_ZcfgDetail(int? id)
        {
            政策法规 g = new 政策法规();
            if (null != id)
            {
                g = 政策法规管理.查找政策法规((long)id);
            }

            return PartialView("Procure_Part/Part_Procure_ZcfgDetail", g);
        }
        public ActionResult Part_Procure_ZcfgList()
        {

            long pgCount = 0;
            int cpg = 0;
            if (!string.IsNullOrWhiteSpace(Request.QueryString["page"]))
            {
                cpg = int.Parse(Request.QueryString["page"]);
            }
            if (cpg <= 0)
            {
                cpg = 1;
            }
            long pc = 政策法规管理.计数政策法规(0, -1);
            pgCount = pc / 10;
            if (pc % 10 > 0)
            {
                pgCount++;
            }
            ViewData["Pagecount"] = pgCount;
            ViewData["CurrentPage"] = cpg;
            ViewData["后台政策法规列表"] = 政策法规管理.查询政策法规(10 * (cpg - 1), 10);
            return PartialView("Procure_Part/Part_Procure_ZcfgList");
        }
        public ActionResult Part_Procure_ZcfgList_S()
        {
            long pgCount = 0;
            int cpg = 0;
            if (!string.IsNullOrWhiteSpace(Request.QueryString["page"]))
            {
                cpg = int.Parse(Request.QueryString["page"]);
            }
            if (cpg <= 0)
            {
                cpg = 1;
            }
            long pc = 政策法规管理.计数政策法规(0, 0, Query<政策法规>.EQ(o => o.内容基本信息.所有者.用户ID, currentUser.Id));
            pgCount = pc / 10;
            if (pc % 10 > 0)
            {
                pgCount++;
            }
            ViewData["Pagecount"] = pgCount;
            ViewData["CurrentPage"] = cpg;
            ViewData["后台政策法规列表"] = 政策法规管理.查询政策法规(10 * (cpg - 1), 10, Query<政策法规>.EQ(o => o.内容基本信息.所有者.用户ID, currentUser.Id));

            return PartialView("Procure_Part/Part_Procure_ZcfgList_S");
        }
        public ActionResult Part_Procure_ZcfgList_Audit()
        {
            long pgCount = 0;
            int cpg = 0;
            if (!string.IsNullOrWhiteSpace(Request.QueryString["page"]))
            {
                cpg = int.Parse(Request.QueryString["page"]);
            }
            if (cpg <= 0)
            {
                cpg = 1;
            }
            long pc = 政策法规管理.计数政策法规(0, 0, Query<政策法规>.EQ(o => o.审核数据.审核者.用户ID, currentUser.Id));
            pgCount = pc / 10;
            if (pc % 10 > 0)
            {
                pgCount++;
            }
            ViewData["Pagecount"] = pgCount;
            ViewData["CurrentPage"] = cpg;
            ViewData["后台政策法规列表"] = 政策法规管理.查询政策法规(10 * (cpg - 1), 10, Query<政策法规>.EQ(o => o.审核数据.审核者.用户ID, currentUser.Id));

            return PartialView("Procure_Part/Part_Procure_ZcfgList_Audit");
        }
        public ActionResult Part_Procure_ZcfgList_AuditDetail(int? id)
        {
            政策法规 g = new 政策法规();
            if (null != id)
            {
                g = 政策法规管理.查找政策法规((long)id);
            }

            return PartialView("Procure_Part/Part_Procure_ZcfgList_AuditDetail", g);
        }
        public ActionResult Part_Procure_ZcfgModify(int? id)
        {
            政策法规 g = null;
            try
            {
                g = 政策法规管理.查找政策法规((long)id);
            }
            catch
            {

            }

            return PartialView("Procure_Part/Part_Procure_ZcfgModify", g);
        }
        public ActionResult DepartmentModify()
        {
            return View();
        }
        [单一权限验证(权限.已审核的附属账号)]
        public ActionResult DepartmentList()
        {
            return View();
        }

        [单一权限验证(权限.新建附属账号)]
        public ActionResult DepartmentAdd()
        {
            return View();
        }

        //public ActionResult Depart_GysList()
        //{
        //    return View();
        //}

        //public ActionResult Zb_ReleaseInfo()
        //{
        //    return View();
        //}
        public ActionResult Procure_Xttz()
        {
            return View();
        }
        public ActionResult Procure_AdList()
        {
            return View();
        }
        public ActionResult Procure_AdAudit()
        {
            return View();
        }

        [单一权限验证(权限.已收消息)]
        public ActionResult Procure_Znxx()
        {
            var model = 站内消息管理.查询发起者的站内消息(0, 0, currentUser.Id);
            return View();
        }
        [单一权限验证(权限.已发消息)]
        public ActionResult Procure_ZnxxSend()
        {
            return View();
        }

        [单一权限验证(权限.新增消息)]
        public ActionResult Procure_ZnxxAdd()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]

        public ActionResult Procure_AdDetail(string action, Models.数据模型.内容数据模型.公告 g)
        {
            公告 m = 公告管理.查找公告(g.Id);
            deleteIndex("/Lucene.Net/IndexDic/GongGao", g.Id.ToString());
            if (action == "审核通过")
            {
                if (currentUser.管理单位.用户ID != 10)
                {
                    //二级（中间）单位进行审核的代码
                    m.审核数据2.审核者.用户ID = currentUser.Id;
                    m.审核数据2.审核状态 = 审核状态.审核通过;
                    m.审核数据2.审核时间 = DateTime.Now;

                    //提交给上级单位继续审核
                    m.审核数据.审核状态 = 审核状态.未审核;
                    m.审核数据.审核者.用户ID = currentUser.管理单位.用户ID;

                    //站内消息
                    站内消息 Msg = new 站内消息();
                    对话消息 Talk = new 对话消息();

                    Msg.重要程度 = 重要程度.特别重要;
                    Msg.消息类型 = 消息类型.公告传递;

                    Msg.发起者.用户ID = currentUser.Id;
                    Talk.发言人.用户ID = currentUser.Id;
                    站内消息管理.添加站内消息(Msg, currentUser.Id, currentUser.管理单位.用户ID);
                    Talk.消息主体.标题 = "待审核公告";
                    Talk.消息主体.内容 = "有一条待审核的公告未处理，<a style='color:red;text-decoration:underline;' href='/单位用户后台/Procure_AdSendList'>点击这里进行审核</a>";
                    对话消息管理.添加对话消息(Talk, Msg);

                    公告管理.更新公告(m, false, false);
                }
                else
                {
                    //最高级（采购处审核后直接发布，创建lucene）
                    m.审核数据.审核者.用户ID = currentUser.Id;
                    m.审核数据.审核状态 = 审核状态.审核通过;
                    m.审核数据.审核时间 = DateTime.Now;

                    公告管理.更新公告(m, false, false);
                    GG_CreateIndex(公告管理.查找公告(g.Id), "/Lucene.Net/IndexDic/GongGao");
                }
            }
            else if (action == "审核不通过")
            {
                if (currentUser.管理单位.用户ID != 10)
                {
                    //二级（中间）单位进行审核的代码
                    m.审核数据2.审核者.用户ID = currentUser.Id;
                    m.审核数据2.审核状态 = 审核状态.审核未通过;
                    m.审核数据2.审核时间 = DateTime.Now;
                    m.审核数据2.未通过理由 = Request.Form["reason"];
                    m.审核数据.审核者.用户ID = -1;
                    m.审核数据.未通过理由 = "";
                    公告管理.更新公告(m, false, false);
                }
                else
                {
                    //最高级
                    m.审核数据.审核者.用户ID = currentUser.Id;
                    m.审核数据.审核状态 = 审核状态.审核未通过;
                    m.审核数据.审核时间 = DateTime.Now;
                    m.审核数据.未通过理由 = Request.Form["reason"];

                    公告管理.更新公告(m, false, false);
                }
            }
            return View("Procure_AdSendList");
        }


        public ActionResult Procure_AdDetail()
        {
            return View();
        }

        [单一权限验证(权限.新增采购公告)]
        public ActionResult Procure_AdAdd()
        {
            return View();
        }
        public ActionResult Procure_NewsList()
        {
            return View();
        }
        public ActionResult Procure_NewsList_S()
        {
            return View();
        }
        public ActionResult Procure_NewsList_Audit()
        {
            return View();
        }
        public ActionResult Procure_NewsList_AuditDetail()
        {
            return View();
        }
        public ActionResult Procure_NewsDetail()
        {
            return View();
        }
        public ActionResult Procure_AdModify()
        {
            try
            {
                公告 model = 公告管理.查找公告(long.Parse(Request.QueryString["id"]));
                if(model.审核数据.审核状态!= 审核状态.审核通过)
                {
                    return View();
                }
                else
                {
                    return Redirect("/单位用户后台/Procure_AdSendList_S");
                }
            }
            catch
            {
                return Redirect("/单位用户后台/Procure_AdSendList_S");
            }
        }
        public ActionResult Procure_NewsModify()
        {
            return View();
        }
        public ActionResult Procure_NewsAdd()
        {
            return View();
        }
        public ActionResult Procure_ZcfgList()
        {
            return View();
        }
        public ActionResult Procure_ZcfgList_S()
        {
            return View();
        }
        public ActionResult Procure_ZcfgList_Audit()
        {
            return View();
        }
        public ActionResult Procure_ZcfgList_AuditDetail()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Procure_ZcfgList_AuditDetail(string action, long? id)
        {
            try
            {
                deleteIndex("/Lucene.Net/IndexDic/Zcfg", id.ToString());
                var model = 政策法规管理.查找政策法规((long)id);
                if (action == "审核通过")
                {
                    model.审核数据.审核状态 = 审核状态.审核通过;
                    model.审核数据.审核时间 = DateTime.Now;
                }
                else if (action == "审核不通过")
                {
                    model.审核数据.审核时间 = DateTime.Now;
                    model.审核数据.审核状态 = 审核状态.审核未通过;
                    model.审核数据.未通过理由 = Request.Form["reason"];
                }
                政策法规管理.更新政策法规(model, false);
                if (model.审核数据.审核状态 == 审核状态.审核通过)
                {
                    CreateIndex(model, "/Lucene.Net/IndexDic/Zcfg");
                }
            }
            catch
            {

            }
            return View("Procure_ZcfgList_Audit");
        }
        public ActionResult Procure_ZcfgDetail()
        {
            return View();
        }

        public ActionResult Procure_ZcfgModify()
        {
            return View();
        }
        public ActionResult Procure_ZcfgAdd()
        {
            return View();
        }

        public ActionResult Procure_TzList()
        {
            return View();
        }

        public ActionResult Procure_TzList_S()
        {
            return View();
        }

        public ActionResult Procure_TzList_Audit()
        {
            return View();
        }
        public ActionResult Procure_TzDetail()
        {
            return View();
        }
        public ActionResult Procure_TzList_AuditDetail()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Procure_TzList_AuditDetail(string action, long? id)
        {
            try
            {
                deleteIndex("/Lucene.Net/IndexDic/Tongzhi", id.ToString());
                var model = 通知管理.查找通知((long)id);
                if (action == "审核通过")
                {
                    model.审核数据.审核状态 = 审核状态.审核通过;
                    model.审核数据.审核时间 = DateTime.Now;
                }
                else if (action == "审核不通过")
                {
                    model.审核数据.审核时间 = DateTime.Now;
                    model.审核数据.审核状态 = 审核状态.审核未通过;
                    model.审核数据.未通过理由 = Request.Form["reason"];
                }
                通知管理.更新通知(model, false);
                if (model.审核数据.审核状态 == 审核状态.审核通过)
                {
                    CreateIndex(model, "/Lucene.Net/IndexDic/Tongzhi");
                }
            }
            catch
            {

            }
            return View("Procure_TzList_Audit");
        }

        public ActionResult Procure_TzModify()
        {
            return View();
        }

        public ActionResult Procure_TzAdd()
        {
            return View();
        }
        public class PartialViewViewModel
        {
            public int id { set; get; }

            public string Name { set; get; }
        }

        #region  权限管理

        public ActionResult User_group_Add()
        {
            return View();
        }

        public ActionResult Usergroup_Mannage()
        {
            return View();
        }
        public JsonResult CheckUserName(string 用户组名)
        {
            var result = 权限管理.用户组已存在(用户组名);
            return Json(!result, JsonRequestBehavior.AllowGet);
        }
        public int delgt()
        {
            try
            {
                long id = long.Parse(Request.QueryString["id"].ToString());
                int index = int.Parse(Request.QueryString["index"].ToString());
                专家 model = 用户管理.查找用户<专家>(id);
                model.可参评物资类别列表.RemoveAt(index);
                用户管理.更新用户<专家>(model);
                return 1;
            }
            catch
            {
                return 0;
            }
        }
        [HttpPost]

        public ActionResult Usergroup_Modify()
        {
            try
            {
                var id = int.Parse(Request.Params["ID"]);
                if (string.IsNullOrEmpty(Request.Params["Name"]))
                {
                    return Content("用户组名不能为空！");
                }
                else if (Request.Params["Name"] != 用户组管理.查找用户组((long)id).用户组名 && 权限管理.用户组已存在(Request.Params["Name"]))
                {
                    return Content("该用户组名已经存在，请重新填写！");
                }

                用户组 model = new 用户组();
                var selectlist = Request.Params["permissionstr"];

                model.Id = id;

                List<string> 权限列表 = new List<string>();
                if (!string.IsNullOrEmpty(selectlist))
                {
                    权限列表 = selectlist.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                }
                model.用户组名 = Request.Params["Name"];
                model.权限列表 = 权限列表;
                model.所属单位.用户ID = currentUser.Id;
                用户组管理.更新用户组(model);
                return Content("success");
            }
            catch
            {
                throw new Exception("出错了");
            }
        }

        public ActionResult Usergroup_Delete(int? id)
        {
            try
            {
                用户组管理.删除用户组((long)id);
                return Content("success");
            }
            catch
            {
                return Content("删除失败！");
            }

        }
        public ActionResult Part_User_group_Add()
        {
            var l = Enum.GetValues(typeof(权限))
                .Cast<权限>()
                .GroupBy(g => (int)g - (int)g % 1000)
                .Select(g => g.ToArray())
                .ToDictionary(g => g[0]/*.ToString()*/, g =>
                    100 != (int)g[1] - (int)g[0]
                        ? g.Skip(1).ToArray()
                        : (object)g.Skip(1)
                            .GroupBy(gg => (int)gg - (int)gg % 100)
                            .Select(gg => gg.ToArray())
                            .ToDictionary(gg => gg[0]/*.ToString()*/, gg => gg.Skip(1).ToArray()));

            ViewData["权限集合"] = l;
            ViewData["当前用户所属用户组"] = currentUser.用户组;
            ViewData["当前用户权限"] = HttpContext.获取当前用户权限列表();

            return PartialView("Procure_Part/Part_User_group_Add");
        }
        public ActionResult Part_Usergroup_Mannage(int? page)
        {

            var l = Enum.GetValues(typeof(权限))
                .Cast<权限>()
                .GroupBy(g => (int)g - (int)g % 1000)
                .Select(g => g.ToArray())
                .ToDictionary(g => g[0]/*.ToString()*/, g =>
                    100 != (int)g[1] - (int)g[0]
                        ? g.Skip(1).ToArray()
                        : (object)g.Skip(1)
                            .GroupBy(gg => (int)gg - (int)gg % 100)
                            .Select(gg => gg.ToArray())
                            .ToDictionary(gg => gg[0]/*.ToString()*/, gg => gg.Skip(1).ToArray()));
            //ViewData["权限集合"] = JsonConvert.SerializeObject(l);
            ViewData["权限集合"] = l;

            bool IsActor = false;//判断是否是操作员账号
            foreach (var k in currentUser.用户组)
            {
                if (k.Contains("操作员"))
                {
                    IsActor = true;
                    ViewBag.isActor = IsActor;

                    ViewData["当前用户权限"] = HttpContext.获取当前用户权限列表();

                    int listcount = (int)(用户组管理.计数用户组(0, 0, Query<用户组>.Where(o => o.所属单位.用户ID == currentUser.Id)));
                    int maxpage = Math.Max((listcount + USERGROUP_PAGESIZE - 1) / USERGROUP_PAGESIZE, 1);

                    if (string.IsNullOrEmpty(page.ToString()) || page < 0 || page > maxpage)
                    {
                        page = 1;
                    }
                    ViewData["listcount"] = listcount;
                    ViewData["pagesize"] = USERGROUP_PAGESIZE;
                    ViewData["currentpage"] = page;
                    ViewData["pagecount"] = maxpage;
                    ViewData["所管理用户组"] = 用户组管理.查询用户组(USERGROUP_PAGESIZE * (int.Parse(page.ToString()) - 1), USERGROUP_PAGESIZE, Query<用户组>.Where(o => o.所属单位.用户ID == currentUser.Id));
                }
                else
                {
                    IsActor = false;
                    ViewBag.isActor = IsActor;
                    int listcount = (int)(用户组管理.计数用户组(0, 0));
                    int maxpage = Math.Max((listcount + USERGROUP_PAGESIZE - 1) / USERGROUP_PAGESIZE, 1);

                    if (string.IsNullOrEmpty(page.ToString()) || page < 0 || page > maxpage)
                    {
                        page = 1;
                    }

                    ViewData["listcount"] = listcount;
                    ViewData["pagesize"] = USERGROUP_PAGESIZE;
                    ViewData["currentpage"] = page;
                    ViewData["pagecount"] = maxpage;

                    ViewData["用户组列表"] = 用户组管理.查询用户组(USERGROUP_PAGESIZE * (int.Parse(page.ToString()) - 1), USERGROUP_PAGESIZE);
                }
            }
            return PartialView("Procure_Part/Part_Usergroup_Mannage");
        }
        public ActionResult Usergroup_Mannage_My()
        {
            return View();
        }
        public ActionResult Part_Usergroup_Mannage_My(int? page)
        {
            ViewData["当前用户权限"] = HttpContext.获取当前用户权限列表();
            int listcount = (int)(用户组管理.计数用户组(0, 0, Query<用户组>.Where(o => o.所属单位.用户ID == currentUser.Id)));
            int maxpage = Math.Max((listcount + USERGROUP_PAGESIZE - 1) / USERGROUP_PAGESIZE, 1);

            if (string.IsNullOrEmpty(page.ToString()) || page < 0 || page > maxpage)
            {
                page = 1;
            }
            ViewData["listcount"] = listcount;
            ViewData["pagesize"] = USERGROUP_PAGESIZE;
            ViewData["currentpage"] = page;
            ViewData["pagecount"] = maxpage;
            ViewData["所管理用户组"] = 用户组管理.查询用户组(USERGROUP_PAGESIZE * (int.Parse(page.ToString()) - 1), USERGROUP_PAGESIZE, Query<用户组>.Where(o => o.所属单位.用户ID == currentUser.Id));
            return PartialView("Procure_Part/Part_Usergroup_Mannage_My");
        }

        public void Usergroup_Add()
        {
            用户组 model = new 用户组();
            //var selectlist = Request.Form["permissionstr"];
            var selectlist = Request.Params["per"];
            var name = Request.Params["name"];
            model.权限列表 = new List<string>();
            if (!string.IsNullOrEmpty(selectlist))
            {
                model.权限列表 = selectlist.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
            }
            model.用户组名 = name;
            model.所属单位.用户ID = currentUser.Id;
            用户组管理.添加用户组(model);

        }

        public ActionResult Roles_Add()
        {
            return View();
        }

        public ActionResult Role_Mannage()
        {
            return View();
        }
        public ActionResult Part_Roles_Add()
        {

            ViewData["用户组集合"] = 用户组管理.查询用户组(0, 0);

            return PartialView("Procure_Part/Part_Roles_Add");
        }
        public ActionResult Part_Role_Mannage(int? page)
        {
            ViewData["用户组集合"] = 用户组管理.查询用户组(0, 0);


            int listcount = (int)(角色管理.计数角色(0, 0));
            int maxpage = Math.Max((listcount + USERGROUP_PAGESIZE - 1) / USERGROUP_PAGESIZE, 1);

            if (string.IsNullOrEmpty(page.ToString()) || page < 0 || page > maxpage)
            {
                page = 1;
            }

            ViewData["listcount"] = listcount;
            ViewData["pagesize"] = USERGROUP_PAGESIZE;

            ViewData["角色列表"] = 角色管理.查询角色(USERGROUP_PAGESIZE * (int.Parse(page.ToString()) - 1), USERGROUP_PAGESIZE);

            return PartialView("Procure_Part/Part_Role_Mannage");
        }

        public void RolesAdd()
        {
            角色 model = new 角色();
            var selectlist = Request.Params["per"];
            var name = Request.Params["name"];
            // var selectlist = Request.Form["permissionstr"];
            model.包含用户组 = new List<string>();
            if (!string.IsNullOrEmpty(selectlist))
            {
                model.包含用户组 = selectlist.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
            }
            model.角色名 = name;
            角色管理.添加角色(model);
        }

        public ActionResult Role_Modify()
        {
            try
            {
                var id = int.Parse(Request.Params["id"]);
                if (string.IsNullOrEmpty(Request.Params["name"]))
                {
                    return Content("角色名不能为空！");
                }
                else if (Request.Params["name"] != 角色管理.查找角色((long)id).角色名 && 权限管理.角色已存在(Request.Params["name"]))
                {
                    return Content("该角色名已经存在，请重新填写！");
                }

                角色 model = new 角色();
                var selectlist = Request.Params["permissionstr"];

                model.Id = id;

                List<string> 包含用户组 = new List<string>();

                if (!string.IsNullOrEmpty(selectlist))
                {
                    包含用户组 = selectlist.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                }
                model.角色名 = Request.Params["name"];
                model.包含用户组 = 包含用户组;

                角色管理.更新角色(model);
                return Content("success");
            }
            catch
            {
                return Content("输入有误，请重新填写！");
            }
        }
        public ActionResult Role_Delete(int? id)
        {
            try
            {
                角色管理.删除角色((long)id);
                return Content("success");
            }
            catch
            {
                return Content("删除失败！");
            }

        }
        #endregion
        /// <summary>
        /// 修改单位信息
        /// </summary>
        /// <returns></returns>
        public ActionResult ModifiDepartment()
        {
            string reqid = Request.Params["id"].ToString();
            try
            {
                if (reqid == "11111")
                {
                    //return Content("ajax正确：ID：11111");
                    PartialViewViewModel retmod = new PartialViewViewModel();

                    retmod.id = 11111;
                    retmod.Name = "ajax正确：ID：11111";
                    return Json(retmod, JsonRequestBehavior.AllowGet);

                }
                else
                {
                    return Content("ajax失败");
                }

            }
            catch
            {
                return Content("ajax失败");
            }
        }

        /// <summary>
        /// 添加公告
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateInput(false)]

        public ActionResult Procure_AdAdd(公告 model)
        {
            //var firstclass = Request.Form["hy"];
            //var secondclass = Request.Form["secondclass"];
            //var thirdclass = Request.Form["thirdclass"];
            //if (firstclass != "请选择行业" && secondclass != "不限")
            //{
            //    model.公告信息.一级分类 = firstclass;
            //    model.公告信息.二级分类 = secondclass;
            //    if (thirdclass != "不限")
            //    {
            //        model.公告信息.三级分类 = thirdclass;
            //    }
            //}

            //var time = Request.Form["publishtime"];
            //var zb_contact = Request.Form["zb_contact"];
            //var isybm = Request.Form["isybm"];
            //if (!string.IsNullOrEmpty(time))
            //{
            //    try
            //    {
            //        model.内容主体.发布时间 = Convert.ToDateTime(time);
            //    }
            //    catch
            //    {
            //        model.内容主体.发布时间 = DateTime.Now;
            //    }
            //}
            //else
            //{
            //    model.内容主体.发布时间 = DateTime.Now;
            //}

            try
            {
                if (!string.IsNullOrWhiteSpace(Request.Form["attachtext"].ToString()))
                {
                    var str = Request.Form["attachtext"].ToString();
                    model.内容主体.附件 = new List<string>(str.Split(new[] { "|" }, StringSplitOptions.RemoveEmptyEntries));
                }
                else
                {
                    model.内容主体.附件 = null;
                }

                if (!string.IsNullOrWhiteSpace(Request.Form["attachtext_s"].ToString()))
                {
                    var str = Request.Form["attachtext_s"].ToString();
                    model.内容主体.图片 = new List<string>(str.Split(new[] { "|" }, StringSplitOptions.RemoveEmptyEntries));
                }
                else
                {
                    model.内容主体.图片 = null;
                }
                model.内容主体.发布时间 = DateTime.Now;
                //model.公告信息.一级分类 = Request.Form["hy"];
                //model.公告信息.所属地域.省份 = Request.Form["deliverprovince"];
                //model.公告信息.所属地域.城市 = Request.Form["delivercity"];
                //model.公告信息.所属地域.区县 = Request.Form["deliverarea"];

                //赋值公告所有者
                
                model.内容基本信息.所有者.用户ID = currentUser.Id;
#if INTRANET
                //如果是采购处或者更高级别的用户，直接审核通过进行发布，创建lucene（成都采购站，西藏采购站或者采购处及其助理员）
                if (currentUser.管理单位.用户ID == 10)
                {
                    model.审核数据.审核状态 = 审核状态.审核通过;
                    model.审核数据.审核时间 = DateTime.Now;
                    model.审核数据.审核者.用户ID = currentUser.Id;

                    //model.审核数据2.审核状态 = 审核状态.审核通过;
                    //model.审核数据2.审核时间 = DateTime.Now;
                    //model.审核数据2.审核者.用户ID = currentUser.Id;
                    model.内容主体.标题 = model.项目信息.项目名称 + "(" + model.项目信息.项目编号 + ")";
                    model.内容主体.发布时间 = DateTime.Now;
                    公告管理.添加公告(model);
                    GG_CreateIndex(model, "/Lucene.Net/IndexDic/GongGao");
                }
                else
                {
                    //站内消息
                    站内消息 Msg = new 站内消息();
                    对话消息 Talk = new 对话消息();

                    Msg.重要程度 = 重要程度.特别重要;
                    Msg.消息类型 = 消息类型.公告传递;

                    Msg.发起者.用户ID = currentUser.Id;
                    Talk.发言人.用户ID = currentUser.Id;

                    Talk.消息主体.标题 = "待审核公告";
                    Talk.消息主体.内容 = "有一条待审核的公告未处理，<a style='color:red;text-decoration:underline;' href='/单位用户后台/Procure_AdSendList'>点击这里进行审核</a>";
                    var sendid = currentUser.Id;


                    //二级单位的公告(昆明采购站，云南采购站，重庆采购站)
                    if (currentUser.管理单位.用户ID == 11)
                    {
                        model.审核数据.审核状态 = 审核状态.未审核;
                        model.审核数据.审核者.用户ID = 11;
                        sendid = 11;
                    }
                    else
                    {
                        //低级单位提交的公告--根据省份提交到各采购站
                        var p = currentUser.所属地域.省份;
                        switch (p)
                        {
                            case "重庆市":
                                model.审核数据2.审核者.用户ID = 12;
                                model.审核数据2.审核状态 = 审核状态.未审核;
                                sendid = 12;
                                break;
                            case "云南省":
                                model.审核数据2.审核者.用户ID = 13;
                                model.审核数据2.审核状态 = 审核状态.未审核;
                                sendid = 13;
                                break;
                            case "贵州省":
                                model.审核数据2.审核者.用户ID = 14;
                                model.审核数据2.审核状态 = 审核状态.未审核;
                                sendid = 14;
                                break;
                            case "西藏自治区":
                                model.审核数据2.审核者.用户ID = 15;
                                model.审核数据2.审核状态 = 审核状态.未审核;
                                sendid = 15;
                                break;
                            default:
                                model.审核数据2.审核者.用户ID = 16;
                                model.审核数据2.审核状态 = 审核状态.未审核;
                                sendid = 16;
                                break;
                        }
                    }
                    model.内容主体.标题 = model.项目信息.项目名称 + "(" + model.项目信息.项目编号 + ")";
                    model.内容主体.发布时间 = DateTime.Now;
                    公告管理.添加公告(model);
                    站内消息管理.添加站内消息(Msg, currentUser.Id, sendid);
                    对话消息管理.添加对话消息(Talk, Msg);
                }
#else
                model.审核数据.审核状态 = 审核状态.未审核;
                model.内容主体.标题 = model.项目信息.项目名称 + "(" + model.项目信息.项目编号 + ")";
                model.内容主体.发布时间 = DateTime.Now;
                公告管理.添加公告(model);
#endif
                //if (!string.IsNullOrEmpty(zb_contact))
                //{
                //    try
                //    {
                //        招标采购项目 z = 招标采购项目管理.查找招标采购项目(int.Parse(zb_contact));
                //        if (model.公告信息.公告版本 == 公告.公告版本.变更 || model.公告信息.公告版本 == 公告.公告版本.更正 || model.公告信息.公告版本 == 公告.公告版本.补遗)
                //        {
                //            公告链接 g = new 公告链接();
                //            g.公告ID = model.Id;
                //            z.修正公告链接.Add(g);
                //        }

                //        switch (model.公告信息.公告性质)
                //        {
                //            case 公告.公告性质.技术公告:
                //                z.技术公告链接.公告ID = model.Id;
                //                break;
                //            case 公告.公告性质.发标公告:
                //                z.招标公告链接.公告ID = model.Id;
                //                break;
                //            case 公告.公告性质.中标公告:
                //                z.中标公告链接.公告ID = model.Id;
                //                break;
                //            case 公告.公告性质.废标公告:
                //                z.废标公告链接.公告ID = model.Id;
                //                break;
                //            case 公告.公告性质.流标公告:
                //                z.流标公告链接.公告ID = model.Id;
                //                break;
                //            default:
                //                break;

                //        }
                //        招标采购项目管理.更新招标采购项目(z);
                //    }
                //    catch
                //    {

                //    }
                //}
                var isybm = Request.Form["isybm"];
                if (isybm == "1")
                {
                    if (model.公告信息.公告性质 == 公告.公告性质.采购公告 && model.公告信息.公告类别 == 公告.公告类别.公开招标)
                    {
                        招标采购预报名 temp = new 招标采购预报名();
                        temp.所属公告链接.公告ID = model.Id;
                        招标采购预报名管理.添加招标采购预报名(temp);
                    }
                }

            }
            catch
            {
                return Content("<script>alert('输入有误!请检查数据格式');location.href='javascript:history.go(-1)';</script>");
            }
            return Content("<script>if(confirm('公告添加成功，点击确定继续添加，点击取消查看')){window.location='/单位用户后台/Procure_AdAdd';}else{window.location='/单位用户后台/Procure_AdSendList_S';}</script>");
        }

        public ActionResult Procure_AdDelete(long? id)//删除所有公告
        {
            try
            {
                string html = Request.Params["h"];
                if (id != null)
                {
                    try
                    {
                        var model = 公告管理.查找公告((long)id);
                        var attach = model.内容主体;
                        if (attach != null)
                        {
                            if (attach.附件 != null && attach.附件.Count > 0)
                            {
                                foreach (var item in attach.附件)
                                {
                                    try
                                    {
                                        UploadPic.DelPic(string.Format("{0}", item));
                                    }
                                    catch
                                    {

                                    }
                                }
                            }
                            if (attach.图片 != null && attach.图片.Count > 0)
                            {
                                foreach (var item in attach.图片)
                                {
                                    try
                                    {
                                        UploadPic.DelPic(string.Format("{0}", item));
                                    }
                                    catch
                                    {

                                    }
                                }
                            }
                        }
                        if (公告管理.删除公告((long)id))
                        {
                            deleteIndex("/Lucene.Net/IndexDic/GongGao", id.ToString());

                            //如有项目包含此公告，则清空该项目的公告链接
                            IList<招标采购项目> m = new List<招标采购项目>();
                            switch (model.公告信息.公告性质)
                            {
                                case 公告.公告性质.技术公告:
                                    m = 招标采购项目管理.查询招标采购项目(0, 1, MongoDB.Driver.Builders.Query<招标采购项目>.EQ(o => o.技术公告链接.公告ID, id)).ToList();
                                    if (m.Any())
                                    {
                                        m[0].技术公告链接 = new 公告链接();
                                        招标采购项目管理.更新招标采购项目(m[0]);
                                    }
                                    break;
                                case 公告.公告性质.采购公告:
                                    m = 招标采购项目管理.查询招标采购项目(0, 1, MongoDB.Driver.Builders.Query<招标采购项目>.EQ(o => o.招标公告链接.公告ID, id)).ToList();
                                    if (m.Any())
                                    {
                                        m[0].招标公告链接 = new 公告链接();
                                        招标采购项目管理.更新招标采购项目(m[0]);
                                    }
                                    break;
                                case 公告.公告性质.中标结果公示:
                                    m = 招标采购项目管理.查询招标采购项目(0, 1, MongoDB.Driver.Builders.Query<招标采购项目>.EQ(o => o.中标公告链接.公告ID, id)).ToList();
                                    if (m.Any())
                                    {
                                        m[0].中标公告链接 = new 公告链接();
                                        招标采购项目管理.更新招标采购项目(m[0]);
                                    }
                                    break;
                                case 公告.公告性质.流标公告:
                                    m = 招标采购项目管理.查询招标采购项目(0, 1, MongoDB.Driver.Builders.Query<招标采购项目>.EQ(o => o.流标公告链接.公告ID, id)).ToList();
                                    if (m.Any())
                                    {
                                        m[0].流标公告链接 = new 公告链接();
                                        招标采购项目管理.更新招标采购项目(m[0]);
                                    }
                                    break;
                                case 公告.公告性质.废标公告:
                                    m = 招标采购项目管理.查询招标采购项目(0, 1, MongoDB.Driver.Builders.Query<招标采购项目>.EQ(o => o.废标公告链接.公告ID, id)).ToList();
                                    if (m.Any())
                                    {
                                        m[0].废标公告链接 = new 公告链接();
                                        招标采购项目管理.更新招标采购项目(m[0]);
                                    }
                                    break;
                                default: break;

                            }

                            IEnumerable<招标采购预报名> bm = 招标采购预报名管理.查询招标采购预报名(0, 1, Query<招标采购预报名>.EQ(o => o.所属公告链接.公告ID, (long)id));
                            if (bm != null && bm.Any())
                            {
                                招标采购预报名 bm_model = bm.ToList()[0];
                                招标采购预报名管理.删除招标采购预报名(bm_model.Id);
                            }

                        }
                    }
                    catch
                    {
                        if (html == "adlist")
                        {
                            return View("Procure_AdList");
                        }
                        else if (html == "adlist_s")
                        {
                            return View("Procure_AdSendList_S");
                        }
                        else
                        {
                            return View("Procure_AdSendList");
                        }
                    }
                }
                if (html == "adlist")
                {
                    return View("Procure_AdList");
                }
                else if (html == "adlist_s")
                {
                    return View("Procure_AdSendList_S");
                }
                else
                {
                    return View("Procure_AdSendList");
                }
            }
            catch
            {
                return Content("<script>location.href='javascript:history.go(-1)';</script>");
            }
        }

        public ActionResult Procure_AdDeleteMy(long? id) //删除我的公告
        {
            try
            {
                string html = Request.Params["h"];
                if (id != null)
                {
                    try
                    {
                        var model = 公告管理.查找公告((long)id);
                        var attach = model.内容主体;
                        if (attach != null)
                        {
                            if (attach.附件 != null && attach.附件.Count > 0)
                            {
                                foreach (var item in attach.附件)
                                {
                                    try
                                    {
                                        UploadPic.DelPic(string.Format("{0}", item));
                                    }
                                    catch
                                    {

                                    }
                                }
                            }
                            if (attach.图片 != null && attach.图片.Count > 0)
                            {
                                foreach (var item in attach.图片)
                                {
                                    try
                                    {
                                        UploadPic.DelPic(string.Format("{0}", item));
                                    }
                                    catch
                                    {

                                    }
                                }
                            }
                        }
                        if (公告管理.删除公告((long)id))
                        {
                            deleteIndex("/Lucene.Net/IndexDic/GongGao", id.ToString());

                            //如有项目包含此公告，则清空该项目的公告链接
                            IList<招标采购项目> m = new List<招标采购项目>();
                            switch (model.公告信息.公告性质)
                            {
                                case 公告.公告性质.技术公告:
                                    m = 招标采购项目管理.查询招标采购项目(0, 1, MongoDB.Driver.Builders.Query<招标采购项目>.EQ(o => o.技术公告链接.公告ID, id)).ToList();
                                    if (m.Any())
                                    {
                                        m[0].技术公告链接 = new 公告链接();
                                        招标采购项目管理.更新招标采购项目(m[0]);
                                    }
                                    break;
                                case 公告.公告性质.采购公告:
                                    m = 招标采购项目管理.查询招标采购项目(0, 1, MongoDB.Driver.Builders.Query<招标采购项目>.EQ(o => o.招标公告链接.公告ID, id)).ToList();
                                    if (m.Any())
                                    {
                                        m[0].招标公告链接 = new 公告链接();
                                        招标采购项目管理.更新招标采购项目(m[0]);
                                    }
                                    break;
                                case 公告.公告性质.中标结果公示:
                                    m = 招标采购项目管理.查询招标采购项目(0, 1, MongoDB.Driver.Builders.Query<招标采购项目>.EQ(o => o.中标公告链接.公告ID, id)).ToList();
                                    if (m.Any())
                                    {
                                        m[0].中标公告链接 = new 公告链接();
                                        招标采购项目管理.更新招标采购项目(m[0]);
                                    }
                                    break;
                                case 公告.公告性质.流标公告:
                                    m = 招标采购项目管理.查询招标采购项目(0, 1, MongoDB.Driver.Builders.Query<招标采购项目>.EQ(o => o.流标公告链接.公告ID, id)).ToList();
                                    if (m.Any())
                                    {
                                        m[0].流标公告链接 = new 公告链接();
                                        招标采购项目管理.更新招标采购项目(m[0]);
                                    }
                                    break;
                                case 公告.公告性质.废标公告:
                                    m = 招标采购项目管理.查询招标采购项目(0, 1, MongoDB.Driver.Builders.Query<招标采购项目>.EQ(o => o.废标公告链接.公告ID, id)).ToList();
                                    if (m.Any())
                                    {
                                        m[0].废标公告链接 = new 公告链接();
                                        招标采购项目管理.更新招标采购项目(m[0]);
                                    }
                                    break;
                                default: break;

                            }

                            IEnumerable<招标采购预报名> bm = 招标采购预报名管理.查询招标采购预报名(0, 1, Query<招标采购预报名>.EQ(o => o.所属公告链接.公告ID, (long)id));
                            if (bm != null && bm.Any())
                            {
                                招标采购预报名 bm_model = bm.ToList()[0];
                                招标采购预报名管理.删除招标采购预报名(bm_model.Id);
                            }

                        }
                    }
                    catch
                    {
                        if (html == "adlist")
                        {
                            return View("Procure_AdList");
                        }
                        else if (html == "adlist_s")
                        {
                            return View("Procure_AdSendList_S");
                        }
                        else
                        {
                            return View("Procure_AdSendList");
                        }
                    }
                }
                if (html == "adlist")
                {
                    return View("Procure_AdList");
                }
                else if (html == "adlist_s")
                {
                    return View("Procure_AdSendList_S");
                }
                else
                {
                    return View("Procure_AdSendList");
                }
            }
            catch
            {
                return Content("<script>location.href='javascript:history.go(-1)';</script>");
            }
        }

        public ActionResult Procure_AdForbid(long? id)
        {
            if (id != null)
            {
                if (公告管理.查找公告((long)id).基本数据.已屏蔽 == false)
                {
                    if (公告管理.屏蔽公告((long)id))
                    {
                        deleteIndex("/Lucene.Net/IndexDic/GongGao", id.ToString());
                    }
                }
                else
                {
                    if (公告管理.屏蔽公告解除((long)id))
                    {
                        CreateIndex(公告管理.查找公告((long)id), "/Lucene.Net/IndexDic/GongGao");
                    }
                }
            }
            return View("Procure_AdList");
        }
        public int resetAd()
        {
            try
            {
                long id = long.Parse(Request.QueryString["id"]);
                公告 model = 公告管理.查找公告(id);
                model.公告信息.是否撤回 = true;
                公告管理.更新公告(model);
                return 1;
            }
            catch
            {
                return -1;
            }
        }
        /// <summary>
        /// 更新公告
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]

        public ActionResult Procure_AdModify(公告 model)
        {
            //对未绑定字段赋值
            var m = 公告管理.查找公告(model.Id);
            model.审核数据 = m.审核数据;
            model.审核数据2 = m.审核数据2;
            model.项目信息.项目标的物 = m.项目信息.项目标的物;
            model.项目信息.预算金额 = m.项目信息.预算金额;
            model.点击次数 = m.点击次数;
            model.内容基本信息 = m.内容基本信息;
            model.公告信息.是否撤回 = false;
           // model.基本数据.修改时间 = DateTime.Now;
            //var firstclass = Request.Form["hy"];
            //var secondclass = Request.Form["secondclass"];
            //var thirdclass = Request.Form["thirdclass"];
            //if (firstclass != "请选择行业" && secondclass != "不限")
            //{
            //    model.公告信息.一级分类 = firstclass;
            //    model.公告信息.二级分类 = secondclass;
            //    if (thirdclass != "不限")
            //    {
            //        model.公告信息.三级分类 = thirdclass;
            //    }
            //}

            //var time = Request.Form["publishtime"];
            //if (!string.IsNullOrEmpty(time))
            //{
            //    try
            //    {
            //        model.内容主体.发布时间 = Convert.ToDateTime(time);

            //    }
            //    catch
            //    {

            //    }
            //}

            try
            {
                //附件
                string attachstr = Request.Form["attachpath"].ToString() + Request.Form["attachtext"];
                if (!string.IsNullOrEmpty(attachstr))
                {
                    attachstr = attachstr.Trim();
                    model.内容主体.附件 = new List<string>(attachstr.Split(new[] { "|" }, StringSplitOptions.RemoveEmptyEntries));
                }
                else
                {
                    model.内容主体.附件 = null;
                }

                //扫描件
                string sm = Request.Form["smPath"].ToString() + Request.Form["attachtext_s"];
                if (!string.IsNullOrWhiteSpace(sm))
                {
                    sm = sm.Trim();
                    model.内容主体.图片 = new List<string>(sm.Split(new[] { "|" }, StringSplitOptions.RemoveEmptyEntries));
                }
                else
                {
                    model.内容主体.图片 = null;
                }
                if (!string.IsNullOrWhiteSpace(Request.Form["delPath"].ToString()))
                {
                    string[] path = Request.Form["delPath"].ToString().Split('|');
                    for (int i = 0; i < path.Length - 1; i++)
                    {
                        if (System.IO.File.Exists(Server.MapPath(@path[i])))
                        {
                            System.IO.File.Delete(Server.MapPath(@path[i]));
                        }
                    }
                }
                //model.公告信息.所属地域.省份 = Request.Form["deliverprovince"];
                //model.公告信息.所属地域.城市 = Request.Form["delivercity"];
                //model.公告信息.所属地域.区县 = Request.Form["deliverarea"];



                //最高级单位进行修改，改变内容,并且把审核数据置为未审核,再审核一次才会显示再页面上
                if (currentUser.管理单位.用户ID == 10)
                {
                    //不是由西藏或成都采购站的下级单位发布的公告
                    model.审核数据.审核状态 = 审核状态.未审核;
                    model.审核数据.未通过理由 = "";
                }
                else
                {
                    //修改自己的公告(昆明采购站、贵阳采购站、重庆采购站、成都采购站直接下级、西藏采购站直接下级)
                    if (model.内容基本信息.所有者.用户ID == currentUser.Id && model.审核数据2.审核者.用户ID == -1 && model.审核数据.审核者.用户ID == 11)
                    {
                        //二级单位修改自己的公告
                        model.审核数据.审核状态 = 审核状态.未审核;
                        model.审核数据.未通过理由 = "";
                    }
                    else
                    {
                        //三级单位修改自己的公告，或者二级单位修改下级单位的公告
                        model.审核数据.审核状态 = 审核状态.未审核;
                        model.审核数据2.审核状态 = 审核状态.未审核;
                        model.审核数据2.未通过理由 = "";
                        model.审核数据.未通过理由 = "";
                        model.审核数据.审核者.用户ID = -1;
                    }
                }
                model.内容主体.标题 = model.项目信息.项目名称 + "(" + model.项目信息.项目编号 + ")";
                公告管理.更新公告(model, false);
                deleteIndex("/Lucene.Net/IndexDic/GongGao", model.Id.ToString());
                deleteIndex("/Lucene.Net/IndexDic/GongGao", "-1");
            }
            catch
            {
                return Content("<script>alert('输入有误!请检查数据格式');location.href='javascript:history.go(-1)';</script>");
            }
            //CreateIndex(model, "/Lucene.Net/IndexDic/GongGao");
            var h = Request.QueryString["h"];
            if (null != h && h == "s")
            {
                return View("Procure_AdSendList_S");
            }
            else
            {
                return View("Procure_AdSendList_S");
            }
        }

        /// <summary>
        /// 添加新闻
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]

        public ActionResult Procure_NewsAdd(Models.数据模型.内容数据模型.新闻 model)
        {
            if (ModelState.IsValid)
            {
                var time = Request.Form["publishtime"];
                if (!string.IsNullOrEmpty(time))
                {
                    try
                    {
                        model.内容主体.发布时间 = Convert.ToDateTime(time);

                    }
                    catch
                    {
                        model.内容主体.发布时间 = DateTime.Now;
                    }
                }
                else
                {
                    model.内容主体.发布时间 = DateTime.Now;
                }
                try
                {
                    if (!string.IsNullOrEmpty(Request.Form["attachtext_s"]))
                    {
                        model.内容主体.图片 = new List<string>(Request.Form["attachtext_s"].Substring(0, Request.Form["attachtext_s"].Length - 1).Split('|'));
                    }

                    //新发布的通知进行状态设置，
                    if (currentUser.管理单位.用户ID == 10)
                    {
                        model.审核数据.审核状态 = 审核状态.审核通过;
                        model.审核数据.审核时间 = DateTime.Now;
                        model.审核数据.审核者.用户ID = currentUser.Id;
                    }
                    else
                    {
                        //站内消息
                        站内消息 Msg = new 站内消息();
                        对话消息 Talk = new 对话消息();

                        Msg.重要程度 = 重要程度.特别重要;
                        Msg.消息类型 = 消息类型.普通;

                        Msg.发起者.用户ID = currentUser.Id;
                        Talk.发言人.用户ID = currentUser.Id;
                        var sendid = 11;

                        Talk.消息主体.标题 = "待审核新闻";
                        Talk.消息主体.内容 = "有一条待审核的新闻未处理，<a style='color:red;text-decoration:underline;' href='/单位用户后台/Procure_NewsList_Audit'>点击这里进行审核</a>";

                        model.审核数据.审核状态 = 审核状态.未审核;
                        model.审核数据.审核者.用户ID = 11;

                        站内消息管理.添加站内消息(Msg, currentUser.Id, sendid);
                        对话消息管理.添加对话消息(Talk, Msg);
                    }
                    model.内容基本信息.所有者.用户ID = currentUser.Id;

                    新闻管理.添加新闻(model);
                    if (model.审核数据.审核状态 == 审核状态.审核通过)
                    {
                        CreateIndex(model, "/Lucene.Net/IndexDic/News");
                    }
                }
                catch
                {
                    return Content("<script>alert('输入有误!请检查数据格式');location.href='javascript:history.go(-1)';</script>");
                }
            }
            return Content("<script>if(confirm('新闻添加成功，点击确定继续添加，点击取消查看')){window.location='/单位用户后台/Procure_NewsAdd';}else{window.location='/单位用户后台/Procure_NewsList_S';}</script>");
        }
        /// <summary>
        /// 删除新闻
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>

        public ActionResult Procure_NewsDelete(long? id)
        {
            if (id != null)
            {
                try
                {
                    var attach = 新闻管理.查找新闻((long)id).内容主体.图片;
                    if (attach != null)
                    {
                        if (attach.Count > 0)
                        {
                            foreach (var item in attach)
                            {
                                try
                                {
                                    UploadPic.DelPic(string.Format("{0}", item));
                                }
                                catch
                                {

                                }
                            }
                        }
                    }

                    if (新闻管理.删除新闻((long)id))
                    {
                        deleteIndex("/Lucene.Net/IndexDic/News", id.ToString());
                    }
                }
                catch
                {
                }
            }
            var comestr = "Procure_NewsList_S";
            var come = Request.QueryString["come"];
            if (come == "l")
            {
                comestr = "Procure_NewsList";
            }
            else if (come == "a")
            {
                comestr = "Procure_NewsList_Audit";
            }
            return View(comestr);
        }
        /// <summary>
        /// 更新新闻
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]

        public ActionResult Procure_NewsModify(Models.数据模型.内容数据模型.新闻 model)
        {
            if (ModelState.IsValid)
            {
                var time = Request.Form["publishtime"];
                if (!string.IsNullOrEmpty(time))
                {
                    try
                    {
                        model.内容主体.发布时间 = Convert.ToDateTime(time);

                    }
                    catch
                    {

                    }
                }

                try
                {
                    var attachstr = Request.Form["oldPath"].ToString() + Request.Form["attachtext_s"].ToString();
                    var deleteattachstr = Request.Form["delPath"].ToString();
                    if (!string.IsNullOrWhiteSpace(deleteattachstr))
                    {
                        string[] delestr = deleteattachstr.Trim().Split('|');
                        foreach (var item in delestr)
                        {
                            try
                            {
                                UploadPic.DelPic(string.Format("{0}", item));
                            }
                            catch
                            {

                            }
                        }
                    }

                    if (!string.IsNullOrEmpty(attachstr))
                    {
                        attachstr = attachstr.Trim();
                        model.内容主体.图片 = new List<string>(attachstr.Substring(0, attachstr.Length - 1).Split('|'));
                    }
                    else
                    {
                        model.内容主体.图片 = null;
                    }

                    //新发布的通知进行状态设置，
                    if (currentUser.管理单位.用户ID == 10)
                    {
                        model.审核数据.审核状态 = 审核状态.审核通过;
                        model.审核数据.审核时间 = DateTime.Now;
                        model.审核数据.审核者.用户ID = currentUser.Id;
                    }
                    else
                    {
                        //站内消息
                        站内消息 Msg = new 站内消息();
                        对话消息 Talk = new 对话消息();

                        Msg.重要程度 = 重要程度.特别重要;
                        Msg.消息类型 = 消息类型.普通;

                        Msg.发起者.用户ID = currentUser.Id;
                        Talk.发言人.用户ID = currentUser.Id;
                        var sendid = 11;

                        Talk.消息主体.标题 = "待审核新闻";
                        Talk.消息主体.内容 = "有一条待审核的新闻未处理，<a style='color:red;text-decoration:underline;' href='/单位用户后台/Procure_NewsList_Audit'>点击这里进行审核</a>";

                        model.审核数据.审核状态 = 审核状态.未审核;
                        model.审核数据.审核者.用户ID = 11;

                        站内消息管理.添加站内消息(Msg, currentUser.Id, sendid);
                        对话消息管理.添加对话消息(Talk, Msg);
                    }
                    if (model.内容基本信息.所有者.用户ID == -1)
                    {
                        model.内容基本信息.所有者.用户ID = currentUser.Id;
                    }

                    新闻管理.更新新闻(model);
                    if (model.审核数据.审核状态 == 审核状态.审核通过)
                    {
                        updateIndex("/Lucene.Net/IndexDic/News", model);
                    }
                    else
                    {
                        deleteIndex("/Lucene.Net/IndexDic/News", model.Id.ToString());
                    }
                }
                catch
                {
                    return Content("<script>alert('输入有误!请检查数据格式');location.href='javascript:history.go(-1)';</script>");
                }
                //CreateIndex(model, "/Lucene.Net/IndexDic/GongGao");
            }
            return View("Procure_NewsList_S");
        }
        /// <summary>
        /// 添加政策法规
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult Procure_ZcfgAdd(Models.数据模型.内容数据模型.政策法规 model)
        {
            if (ModelState.IsValid)
            {
                var time = Request.Form["publishtime"];
                if (!string.IsNullOrEmpty(time))
                {
                    try
                    {
                        model.内容主体.发布时间 = Convert.ToDateTime(time);

                    }
                    catch
                    {
                        model.内容主体.发布时间 = DateTime.Now;
                    }
                }
                else
                {
                    model.内容主体.发布时间 = DateTime.Now;
                }

                try
                {
                    if (!string.IsNullOrWhiteSpace(Request.Form["path"].ToString()))
                    {
                        model.内容主体.附件 = new List<string>(Request.Form["path"].ToString().Substring(0, Request.Form["path"].ToString().Length - 1).Split('|'));
                    }
                    else
                    {
                        model.内容主体.附件 = null;
                    }

                    //新发布的通知进行状态设置，
                    if (currentUser.管理单位.用户ID == 10)
                    {
                        model.审核数据.审核状态 = 审核状态.审核通过;
                        model.审核数据.审核时间 = DateTime.Now;
                        model.审核数据.审核者.用户ID = currentUser.Id;
                    }
                    else
                    {
                        //站内消息
                        站内消息 Msg = new 站内消息();
                        对话消息 Talk = new 对话消息();

                        Msg.重要程度 = 重要程度.特别重要;
                        Msg.消息类型 = 消息类型.普通;

                        Msg.发起者.用户ID = currentUser.Id;
                        Talk.发言人.用户ID = currentUser.Id;
                        var sendid = 11;

                        Talk.消息主体.标题 = "待审核政策法规";
                        Talk.消息主体.内容 = "有一条待审核的政策法规未处理，<a style='color:red;text-decoration:underline;' href='/单位用户后台/Procure_ZcfgList_Audit'>点击这里进行审核</a>";

                        model.审核数据.审核状态 = 审核状态.未审核;
                        model.审核数据.审核者.用户ID = 11;

                        站内消息管理.添加站内消息(Msg, currentUser.Id, sendid);
                        对话消息管理.添加对话消息(Talk, Msg);
                    }
                    model.内容基本信息.所有者.用户ID = currentUser.Id;
                    政策法规管理.添加政策法规(model);
                    //审核通过，创建Lucene
                    if (model.审核数据.审核状态 == 审核状态.审核通过)
                    {
                        CreateIndex(model, "/Lucene.Net/IndexDic/Zcfg");
                    }
                }
                catch
                {
                    return Content("<script>alert('输入有误!请检查数据格式');location.href='javascript:history.go(-1)';</script>");
                }

            }
            return Content("<script>if(confirm(' 政策法规发布成功，点击确定继续发布新政策法规，点击取消查看')){window.location='/单位用户后台/Procure_ZcfgAdd';}else{window.location='/单位用户后台/Procure_ZcfgList_S';}</script>");
        }
        /// <summary>
        /// 删除政策法规
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult Procure_ZcfgDelete(long? id)
        {
            if (id != null)
            {
                try
                {
                    var attach = 政策法规管理.查找政策法规((long)id).内容主体;
                    if (attach != null)
                    {
                        if (attach.附件 != null && attach.附件.Count > 0)
                        {
                            foreach (var item in attach.附件)
                            {
                                try
                                {
                                    UploadPic.DelPic(string.Format("{0}", item));
                                }
                                catch
                                {

                                }
                            }
                        }
                    }

                    if (政策法规管理.删除政策法规((long)id))
                    {
                        deleteIndex("/Lucene.Net/IndexDic/Zcfg", id.ToString());
                    }
                }
                catch
                {
                }
            }
            var comestr = "Procure_ZcfgList_S";
            var come = Request.QueryString["come"];
            if (come == "l")
            {
                comestr = "Procure_ZcfgList";
            }
            else if (come == "a")
            {
                comestr = "Procure_ZcfgList_Audit";
            }
            return View(comestr);
        }
        /// <summary>
        /// 更新政策法规
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult Procure_ZcfgModify(Models.数据模型.内容数据模型.政策法规 model)
        {
            if (ModelState.IsValid)
            {
                var time = Request.Form["publishtime"];
                if (!string.IsNullOrEmpty(time))
                {
                    try
                    {
                        model.内容主体.发布时间 = Convert.ToDateTime(time);

                    }
                    catch
                    {

                    }
                }

                try
                {
                    var attachstr = Request.Form["oldPath"].ToString() + Request.Form["path"].ToString();
                    var deleteattachstr = Request.Form["delPath"];
                    if (!string.IsNullOrEmpty(deleteattachstr))
                    {
                        string[] delestr = deleteattachstr.Trim().Split('|');
                        foreach (var item in delestr)
                        {
                            try
                            {
                                UploadPic.DelPic(string.Format("{0}", item));
                            }
                            catch
                            {

                            }
                        }
                    }

                    if (!string.IsNullOrEmpty(attachstr))
                    {
                        attachstr = attachstr.Trim();
                        model.内容主体.附件 = new List<string>(attachstr.Substring(0, attachstr.Length - 1).Split('|'));
                    }
                    else
                    {
                        model.内容主体.附件 = null;
                    }

                    //新发布的通知进行状态设置，
                    if (currentUser.管理单位.用户ID == 10)
                    {
                        model.审核数据.审核状态 = 审核状态.审核通过;
                        model.审核数据.审核时间 = DateTime.Now;
                        model.审核数据.审核者.用户ID = currentUser.Id;
                    }
                    else
                    {
                        //站内消息
                        站内消息 Msg = new 站内消息();
                        对话消息 Talk = new 对话消息();

                        Msg.重要程度 = 重要程度.特别重要;
                        Msg.消息类型 = 消息类型.普通;

                        Msg.发起者.用户ID = currentUser.Id;
                        Talk.发言人.用户ID = currentUser.Id;
                        var sendid = 11;

                        Talk.消息主体.标题 = "待审核政策法规";
                        Talk.消息主体.内容 = "有一条待审核的政策法规未处理，<a style='color:red;text-decoration:underline;' href='/单位用户后台/Procure_ZcfgList_Audit'>点击这里进行审核</a>";

                        model.审核数据.审核状态 = 审核状态.未审核;
                        model.审核数据.审核者.用户ID = 11;

                        站内消息管理.添加站内消息(Msg, currentUser.Id, sendid);
                        对话消息管理.添加对话消息(Talk, Msg);
                    }
                    if (model.内容基本信息.所有者.用户ID == -1)
                    {
                        model.内容基本信息.所有者.用户ID = currentUser.Id;
                    }

                    政策法规管理.更新政策法规(model);
                    if (model.审核数据.审核状态 == 审核状态.审核通过)
                    {
                        updateIndex("/Lucene.Net/IndexDic/Zcfg", model);
                    }
                    else
                    {
                        deleteIndex("/Lucene.Net/IndexDic/Zcfg", model.Id.ToString());
                    }

                }
                catch
                {
                    return Content("<script>alert('输入有误!请检查数据格式');location.href='javascript:history.go(-1)';</script>");
                }
                //CreateIndex(model, "/Lucene.Net/IndexDic/GongGao");
            }
            return View("Procure_ZcfgList_S");
        }
        /// <summary>
        /// 添加通知
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult Procure_TzAdd(Models.数据模型.内容数据模型.通知 model)
        {
            if (ModelState.IsValid)
            {
                var time = Request.Form["publishtime"];
                if (!string.IsNullOrEmpty(time))
                {
                    try
                    {
                        model.内容主体.发布时间 = Convert.ToDateTime(time);

                    }
                    catch
                    {
                        model.内容主体.发布时间 = DateTime.Now;
                    }
                }
                else
                {
                    model.内容主体.发布时间 = DateTime.Now;
                }

                try
                {
                    if (!string.IsNullOrWhiteSpace(Request.Form["path"].ToString()))
                    {
                        model.内容主体.附件 = new List<string>(Request.Form["path"].ToString().Substring(0, Request.Form["path"].ToString().Length - 1).Split('|'));
                    }
                    else
                    {
                        model.内容主体.附件 = null;
                    }

                    //新发布的通知进行状态设置，
                    if (currentUser.管理单位.用户ID == 10)
                    {
                        model.审核数据.审核状态 = 审核状态.审核通过;
                        model.审核数据.审核时间 = DateTime.Now;
                        model.审核数据.审核者.用户ID = currentUser.Id;
                    }
                    else
                    {
                        //站内消息
                        站内消息 Msg = new 站内消息();
                        对话消息 Talk = new 对话消息();

                        Msg.重要程度 = 重要程度.特别重要;
                        Msg.消息类型 = 消息类型.普通;

                        Msg.发起者.用户ID = currentUser.Id;
                        Talk.发言人.用户ID = currentUser.Id;
                        var sendid = 11;

                        Talk.消息主体.标题 = "待审核通知";
                        Talk.消息主体.内容 = "有一条待审核的通知未处理，<a style='color:red;text-decoration:underline;' href='/单位用户后台/Procure_TzList_Audit'>点击这里进行审核</a>";

                        model.审核数据.审核状态 = 审核状态.未审核;
                        model.审核数据.审核者.用户ID = 11;

                        站内消息管理.添加站内消息(Msg, currentUser.Id, sendid);
                        对话消息管理.添加对话消息(Talk, Msg);
                    }
                    model.内容基本信息.所有者.用户ID = currentUser.Id;
                    通知管理.添加通知(model);
                    //审核通过，创建Lucene
                    if (model.审核数据.审核状态 == 审核状态.审核通过)
                    {
                        CreateIndex(model, "/Lucene.Net/IndexDic/Tongzhi");
                    }
                }
                catch
                {
                    return Content("<script>alert('输入有误!请检查数据格式');location.href='javascript:history.go(-1)';</script>");
                }

            }
            return Content("<script>if(confirm('通知发布成功，点击确定继续发布新通知，点击取消查看')){window.location='/单位用户后台/Procure_TzAdd';}else{window.location='/单位用户后台/Procure_TzList_S';}</script>");
        }
        /// <summary>
        /// 删除通知
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>

        public ActionResult Procure_TzDelete(long? id)
        {
            if (id != null)
            {
                try
                {
                    var attach = 通知管理.查找通知((long)id).内容主体;
                    if (attach != null)
                    {
                        if (attach.附件 != null && attach.附件.Count > 0)
                        {
                            foreach (var item in attach.附件)
                            {
                                try
                                {
                                    UploadPic.DelPic(string.Format("{0}", item));
                                }
                                catch
                                {

                                }
                            }
                        }
                    }

                    if (通知管理.删除通知((long)id))
                    {
                        deleteIndex("/Lucene.Net/IndexDic/Tongzhi", id.ToString());
                    }
                }
                catch
                {
                }
            }
            var comestr = "Procure_TzList_S";
            var come = Request.QueryString["come"];
            if (come == "l")
            {
                comestr = "Procure_TzList";
            }
            else if (come == "a")
            {
                comestr = "Procure_TzList_Audit";
            }
            return View(comestr);
        }
        /// <summary>
        /// 更新通知
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]

        public ActionResult Procure_TzModify(Models.数据模型.内容数据模型.通知 model)
        {
            if (ModelState.IsValid)
            {
                var time = Request.Form["publishtime"];
                if (!string.IsNullOrEmpty(time))
                {
                    try
                    {
                        model.内容主体.发布时间 = Convert.ToDateTime(time);

                    }
                    catch
                    {

                    }
                }

                try
                {
                    var attachstr = Request.Form["oldPath"].ToString() + Request.Form["path"].ToString();
                    var deleteattachstr = Request.Form["delPath"];
                    if (!string.IsNullOrEmpty(deleteattachstr))
                    {
                        string[] delestr = deleteattachstr.Trim().Split('|');
                        foreach (var item in delestr)
                        {
                            try
                            {
                                UploadPic.DelPic(string.Format("{0}", item));
                            }
                            catch
                            {

                            }
                        }
                    }

                    if (!string.IsNullOrEmpty(attachstr))
                    {
                        attachstr = attachstr.Trim();
                        model.内容主体.附件 = new List<string>(attachstr.Substring(0, attachstr.Length - 1).Split('|'));
                    }
                    else
                    {
                        model.内容主体.附件 = null;
                    }

                    //新发布的通知进行状态设置，
                    if (currentUser.管理单位.用户ID == 10)
                    {
                        model.审核数据.审核状态 = 审核状态.审核通过;
                        model.审核数据.审核时间 = DateTime.Now;
                        model.审核数据.审核者.用户ID = currentUser.Id;
                    }
                    else
                    {
                        //站内消息
                        站内消息 Msg = new 站内消息();
                        对话消息 Talk = new 对话消息();

                        Msg.重要程度 = 重要程度.特别重要;
                        Msg.消息类型 = 消息类型.普通;

                        Msg.发起者.用户ID = currentUser.Id;
                        Talk.发言人.用户ID = currentUser.Id;
                        var sendid = 11;

                        Talk.消息主体.标题 = "待审核通知";
                        Talk.消息主体.内容 = "有一条待审核的通知未处理，<a style='color:red;text-decoration:underline;' href='/单位用户后台/Procure_TzList_Audit'>点击这里进行审核</a>";

                        model.审核数据.审核状态 = 审核状态.未审核;
                        model.审核数据.审核者.用户ID = 11;

                        站内消息管理.添加站内消息(Msg, currentUser.Id, sendid);
                        对话消息管理.添加对话消息(Talk, Msg);
                    }
                    if (model.内容基本信息.所有者.用户ID == -1)
                    {
                        model.内容基本信息.所有者.用户ID = currentUser.Id;
                    }

                    通知管理.更新通知(model);
                    if (model.审核数据.审核状态 == 审核状态.审核通过)
                    {
                        updateIndex("/Lucene.Net/IndexDic/Tongzhi", model);
                    }
                    else
                    {
                        deleteIndex("/Lucene.Net/IndexDic/Tongzhi", model.Id.ToString());
                    }

                }
                catch
                {
                    return Content("<script>alert('输入有误!请检查数据格式');location.href='javascript:history.go(-1)';</script>");
                }
                //CreateIndex(model, "/Lucene.Net/IndexDic/GongGao");
            }
            return View("Procure_TzList_S");
        }
        public class DepartmentAddModel
        {
            public 单位用户 u { get; set; }
            //public bool 管理 { get; set; }
            //public bool 监审 { get; set; }
            //public bool 采购 { get; set; }
            //public bool 需求 { get; set; }

            [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "请填写登录名")]
            [System.ComponentModel.DataAnnotations.StringLength(30, MinimumLength = 6, ErrorMessage = "长度为6-30")]
            [System.ComponentModel.DataAnnotations.RegularExpression(@"^[a-zA-Z]\w+$", ErrorMessage = "以字母开头(可用字母、数字或_)")]
            [System.Web.Mvc.Remote("CheckUserName", "注册", ErrorMessage = "该用户名已存在")]
            public string loginName { get; set; }

            [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "单位编码必须填写")]
            [System.Web.Mvc.Remote("CheckUnitCode", "单位用户后台", ErrorMessage = "该单位编码已经存在,请重新填写")]
            public string unitCode { get; set; }

        }
        public JsonResult CheckUnitCode(string unitCode)
        {
            var result = -1 != 用户管理.检查单位编码是否存在(unitCode, true, true);
            return Json(!result, JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// 添加单位用户
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [AllowAnonymous]
        [ValidateInput(false)]

        public ActionResult DepartmentAdd(DepartmentAddModel model)
        {
            if (-1 != 用户管理.检查用户是否存在(model.loginName, true, true))
            {
                return Content("<script>alert('该用用户名已经存在，请重新填写！');location.href='javascript:history.go(-1)';</script>");
            }
            var usergroup = Request.Form["usergroup"];
            bool IsTrue = false;
            if (ModelState.IsValidField("u.单位信息.单位代号") && ModelState.IsValidField("u.登录信息.密码"))
            {
                IsTrue = true;
            }
            if (IsTrue)
            {

                var p = Request["deliverprovince"];
                var c = Request["delivercity"];
                var a = Request["deliverarea"];
                model.u.单位信息.单位编码 = string.Empty;
                model.u.登录信息.登录名 = model.loginName;
                model.u.所属地域.省份 = p;
                model.u.所属地域.城市 = c;
                model.u.所属地域.区县 = a;

                if (!string.IsNullOrWhiteSpace(usergroup))
                {
                    var _f = usergroup.Split(',');
                    for (int i = 0; i < _f.Length - 1; i++)
                    {
                        model.u.用户组.Add(_f[i]);
                    }
                }
                if (Request.Form["admin"] != "-1")
                {
                    model.u.所属单位.用户ID = long.Parse(Request.Form["admin"]);
                }
                用户管理.添加单位用户(model.u,currentUser);
            }
            return Content("<script>alert('添加成功！');location.href='/单位用户后台/departmentadd';</script>");
        }
        //下级单位提交公告模块////////////////////////////////////////
        public ActionResult Procure_AdSend()
        {
            return View();
        }
        public ActionResult Part_Procure_AdSend()
        {
            if (currentUser.管理单位.用户ID == currentUser.Id || currentUser.Id == 11)
            {
                ViewData["flag"] = "0";
            }
            else
            {
                ViewData["flag"] = "1";
            }
            ViewData["行业列表"] = 商品分类管理.查找子分类();

            IEnumerable<招标采购项目> l = 招标采购项目管理.查询招标采购项目(0, 0, MongoDB.Driver.Builders.Query.EQ("审核数据.审核状态", 审核状态.审核通过).And(MongoDB.Driver.Builders.Query.EQ("中标公告链接.公告ID", -1)).And(MongoDB.Driver.Builders.Query.EQ("流标公告链接.公告ID", -1)).And(MongoDB.Driver.Builders.Query.EQ("废标公告链接.公告ID", -1)));

            ViewData["需求列表"] = l;
            return PartialView("Procure_Part/Part_Procure_AdSend");
        }
        /// <summary>
        /// 添加提交公告
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult Procure_AdSend(公告 model)
        {
            if (ModelState.IsValid)
            {
                var firstclass = Request.Form["hy"];
                var secondclass = Request.Form["secondclass"];
                var thirdclass = Request.Form["thirdclass"];
                if (firstclass != "请选择行业" && secondclass != "不限")
                {
                    model.公告信息.一级分类 = firstclass;
                    model.公告信息.二级分类 = secondclass;
                    if (thirdclass != "不限")
                    {
                        model.公告信息.三级分类 = thirdclass;
                    }
                }

                var time = Request.Form["publishtime"];
                if (!string.IsNullOrEmpty(time))
                {
                    try
                    {
                        model.内容主体.发布时间 = Convert.ToDateTime(time);
                    }
                    catch
                    {
                        model.内容主体.发布时间 = DateTime.Now;
                    }
                }
                else
                {
                    model.内容主体.发布时间 = DateTime.Now;
                }

                try
                {
                    if (!string.IsNullOrEmpty(model.内容主体.附件[0]))
                    {
                        var str = model.内容主体.附件[0].Trim();
                        model.内容主体.附件 = new List<string>(str.Substring(0, str.Length - 1).Split('|'));
                    }
                    else
                    {
                        model.内容主体.附件 = null;
                    }
                    model.审核数据.审核状态 = 审核状态.审核通过;
                    //model.公告信息.一级分类 = Request.Form["hy"];
                    model.公告信息.所属地域.省份 = Request.Form["deliverprovince"];
                    model.公告信息.所属地域.城市 = Request.Form["delivercity"];
                    model.公告信息.所属地域.区县 = Request.Form["deliverarea"];


                    try
                    {
                        站内消息 Msg = new 站内消息();
                        Msg.基本数据.备注 = "此公告于 " + DateTime.Now + " 由 " + currentUser.单位信息.单位名称 + " 提交到 " + currentUser.管理单位.用户数据.单位信息.单位名称;

                        对话消息 Talk = new 对话消息();

                        Msg.收信人.用户ID = currentUser.管理单位.用户ID;
                        Msg.发起者.用户ID = currentUser.Id;
                        Talk.发言人.用户ID = currentUser.Id;
                        Msg.消息类型 = 消息类型.公告传递;
                        站内消息管理.添加站内消息(Msg, Msg.发起者.用户ID, Msg.收信人.用户ID);
                        Talk.消息主体.附件 = model.内容主体.附件;

                        Talk.消息主体.内容 = "公告标题：" + model.内容主体.标题 + "<br />" +
                            "发布时间：" + model.内容主体.发布时间 + "<br />" +
                            "相关行业：" + model.公告信息.一级分类 + "<br />" +
                            "所在地域：" + model.公告信息.所属地域.地域 + "<br />" +
                            "项目名称：" + model.项目信息.项目名称 + "<br />" +
                            "项目编号：" + model.项目信息.项目编号 + "<br />" +
                            "公告类别：" + model.公告信息.公告类别 + "<br />" +
                            "公告性质：" + model.公告信息.公告性质 + "<br />" +
                            "公告版本：" + model.公告信息.公告版本 + "<br />" +
                            "公告内容：" + model.内容主体.内容 + "<br />";
                        对话消息管理.添加对话消息(Talk, Msg);
                    }
                    catch
                    {
                        return Content("<script>alert('输入有误!请检查数据格式');location.href='javascript:history.go(-1)';</script>");
                    }

                }
                catch
                {
                    return Content("<script>alert('输入有误!请检查数据格式');location.href='javascript:history.go(-1)';</script>");
                }

            }

            return View("Procure_AdSendList_S");
        }

        public ActionResult Procure_AdSendList()
        {
            return View();
        }
        public ActionResult Part_Procure_AdSendList()
        {
            IMongoQuery q = null;
            if (currentUser.管理单位.用户ID != 10)
            {
                q = Query<公告>.EQ(o => o.审核数据2.审核者.用户ID, currentUser.Id);
            }
            else
            {
                q = Query.Or(new[] { Query<公告>.EQ(o => o.审核数据.审核者.用户ID, currentUser.Id), Query<公告>.EQ(o => o.审核数据2.审核者.用户ID, currentUser.Id) });
            }
            long pgCount = 0;
            int cpg = 0;
            if (!string.IsNullOrWhiteSpace(Request.QueryString["page"]))
            {
                cpg = int.Parse(Request.QueryString["page"]);
            }
            if (cpg <= 0)
            {
                cpg = 1;
            }
            long pc = 公告管理.计数公告(0, 0, q.And(Query<公告>.Where(o => o.公告信息.是否撤回 == false)));
            pgCount = pc / 10;
            if (pc % 10 > 0)
            {
                pgCount++;
            }
            ViewData["Pagecount"] = pgCount;
            ViewData["CurrentPage"] = cpg;
            if (currentUser.管理单位.用户ID != 10)
            {
                ViewData["user"] = "1";
            }
            else
            {
                ViewData["user"] = "0";
            }
            ViewData["用户ID"] = currentUser.Id;
            ViewData["审核公告列表"] = 公告管理.查询公告(10 * (cpg - 1), 10, q.And(Query<公告>.Where(o => o.公告信息.是否撤回 == false)));

            return PartialView("Procure_Part/Part_Procure_AdSendList");
        }

        [单一权限验证(权限.我的采购公告)]
        public ActionResult Procure_AdSendList_S() //我的公告列表
        {
            return View();
        }
        public ActionResult Part_Procure_AdSendList_S()
        {
            try
            {
                IMongoQuery q = Query<公告>.EQ(o => o.内容基本信息.所有者.用户ID, currentUser.Id);
                long pgCount = 0;
                int cpg = 0;
                if (!string.IsNullOrWhiteSpace(Request.QueryString["page"]))
                {
                    cpg = int.Parse(Request.QueryString["page"]);
                }
                if (cpg <= 0)
                {
                    cpg = 1;
                }
                long pc = 公告管理.计数公告(0, 0, q);
                pgCount = pc / 10;
                if (pc % 10 > 0)
                {
                    pgCount++;
                }
                ViewData["Pagecount"] = pgCount;
                ViewData["CurrentPage"] = cpg;
                ViewData["我的公告列表"] = 公告管理.查询公告(10 * (cpg - 1), 10, q);
                return PartialView("Procure_Part/Part_Procure_AdSendList_S");
            }
            catch
            {
                return Content("<script>window.location='/单位用户后台/Procure_AdSendList_S';</script>");
            }
        }


        public ActionResult Procure_AdResend()
        {
            try
            {
                var id = Request.QueryString["id"];
                站内消息 model = 站内消息管理.查找站内消息(long.Parse(id));
                对话消息 d = model.对话消息.First();

                //复制一个消息进行转发
                站内消息 m = model;
                m.Id = -1;
                m.基本数据.备注 += "，于 " + DateTime.Now + " 由 " + currentUser.单位信息.单位名称 + " 转发至 " + currentUser.管理单位.用户数据.单位信息.单位名称;
                站内消息管理.添加站内消息(m, currentUser.Id, currentUser.管理单位.用户ID);

                对话消息 h = new 对话消息();
                h.发言人.用户ID = currentUser.Id;
                h.消息主体 = d.消息主体;
                对话消息管理.添加对话消息(h, m);
                站内消息管理.更新已读时间(m.Id, false, model.基本数据.添加时间);

                return Content("1");
            }
            catch
            {
                return Content("0");
            }
        }
        /// <summary>
        /// 删除通知
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult Procure_AdSendDelete(long? id)
        {
            if (id != null)
            {
                try
                {
                    var m = 站内消息管理.查找站内消息((long)id).对话消息;
                    if (m.Any())
                    {
                        var attach = m.First().消息主体;
                        if (attach != null)
                        {
                            if (attach.附件 != null && attach.附件.Count > 0)
                            {
                                foreach (var item in attach.附件)
                                {
                                    try
                                    {
                                        UploadPic.DelPic(string.Format("{0}", item));
                                    }
                                    catch
                                    {

                                    }
                                }
                            }
                        }
                    }
                    站内消息管理.删除站内消息((long)id);
                }
                catch
                {
                    return Redirect("Procure_AdSendList");
                }
            }
            return Redirect("Procure_AdSendList");
        }

        //下级单位提交公告模块////////////////////////////////////////
        //#if INTRANET
        private static int ZBCGXM_PAGESIZE = 15;
        //////////////////////////////////////////////////////////////////////////////////////////招标采购预报名
        public ActionResult SignUp_List()
        {
            return View();
        }
        public ActionResult Part_SignUp_List()
        {
            if (currentUser.管理单位.用户ID == 10)
            {
                long pgCount = 0;
                int cpg = 0;
                if (!string.IsNullOrWhiteSpace(Request.QueryString["page"]))
                {
                    cpg = int.Parse(Request.QueryString["page"]);
                }
                if (cpg <= 0)
                {
                    cpg = 1;
                }
                long pc = 招标采购预报名管理.计数招标采购预报名(0, 0);
                pgCount = pc / 10;
                if (pc % 10 > 0)
                {
                    pgCount++;
                }
                ViewData["Pagecount"] = pgCount;
                ViewData["CurrentPage"] = cpg;
                ViewData["预报名列表"] = 招标采购预报名管理.查询招标采购预报名(10 * (cpg - 1), 10);
            }
            else
            {
                var p_l = Mongo.列表<公告>(0, 0, Fields<公告>.Include(o => o.Id), Query<公告>.EQ(o => o.内容基本信息.所有者.用户ID, currentUser.Id)).Select(o => o["_id"]); //(IEnumerable<BsonDocument>)(公告管理.查询公告(0, 0, Query<公告>.EQ(o => o.内容基本信息.所有者.用户ID, currentUser.Id)).Select(o => o.Id));
                long pgCount = 0;
                int cpg = 0;
                if (!string.IsNullOrWhiteSpace(Request.QueryString["page"]))
                {
                    cpg = int.Parse(Request.QueryString["page"]);
                }
                if (cpg <= 0)
                {
                    cpg = 1;
                }
                long pc = 招标采购预报名管理.计数招标采购预报名(0, 0, Query<招标采购预报名>.In(o => o.所属公告链接.公告ID, p_l));
                pgCount = pc / 10;
                if (pc % 10 > 0)
                {
                    pgCount++;
                }
                ViewData["Pagecount"] = pgCount;
                ViewData["CurrentPage"] = cpg;
                ViewData["预报名列表"] = 招标采购预报名管理.查询招标采购预报名(10 * (cpg - 1), 10, Query.In("所属公告链接.公告ID", p_l));
            }


            return PartialView("Procure_Part/Part_SignUp_List");
        }

        public ActionResult SignUp_Print()
        {
            return View();
        }

        public ActionResult Part_SignUp_Print(long? id)
        {
            招标采购预报名 model = null;
            try
            {
                model = 招标采购预报名管理.查找招标采购预报名((long)id);
                ViewData["预报名供应商列表"] = model.预报名供应商列表;
            }
            catch
            {
                model = null;
                ViewData["预报名供应商列表"] = new List<招标采购预报名._供应商预报名信息>();
            }
            return PartialView("Procure_Part/Part_SignUp_Print", model);
        }

        public ActionResult Part_SignUp_PrintSearch()
        {
            var type = Request.Params["type"];//审核状态
            var id = Request.Params["id"];//预报名ID
            招标采购预报名 model = null;
            try
            {
                model = 招标采购预报名管理.查找招标采购预报名(long.Parse(id));
                if (type == "全部")
                {
                    ViewData["预报名供应商列表"] = model.预报名供应商列表;
                }
                if (type == "审核通过")
                {
                    ViewData["预报名供应商列表"] = model.预报名供应商列表.Where(o => o.审核数据.审核状态 == 审核状态.审核通过);
                }
                if (type == "审核未通过")
                {
                    ViewData["预报名供应商列表"] = model.预报名供应商列表.Where(o => o.审核数据.审核状态 == 审核状态.审核未通过);
                }
                if (type == "未审核")
                {
                    ViewData["预报名供应商列表"] = model.预报名供应商列表.Where(o => o.审核数据.审核状态 == 审核状态.未审核);
                }
            }
            catch
            {
                model = null;
                ViewData["预报名供应商列表"] = new List<招标采购预报名._供应商预报名信息>();
            }
            return PartialView("Procure_Part/Part_SignUp_PrintSearch", model);
        }
        public ActionResult SignUp_Detail()
        {
            return View();
        }
        [HttpPost]
        public ActionResult SignUp_Detail(招标采购预报名 model)
        {
            try
            {
                var buybs = Request.Form["isbuybscontent"] + Request.Form["isbuybscontent_ed"];
                var sendmail = Request.Form["issendmailcontent"] + Request.Form["issendmailcontent_ed"];
                var m = 招标采购预报名管理.查找招标采购预报名(model.Id);
                if (!string.IsNullOrWhiteSpace(buybs) || !string.IsNullOrWhiteSpace(sendmail))
                {
                    var list = buybs.Split(new[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
                    var list_mail = sendmail.Split(new[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var item in m.预报名供应商列表)
                    {
                        if (list.Contains(item.供应商链接.用户ID.ToString()))
                        {
                            item.已购买标书 = true;
                        }
                        else
                        {
                            item.已购买标书 = false;
                        }

                        if (list_mail.Contains(item.供应商链接.用户ID.ToString()))
                        {
                            item.已发送电子标书 = true;
                        }
                        else
                        {
                            item.已发送电子标书 = false;
                        }
                    }
                }
                else
                {
                    foreach (var item in m.预报名供应商列表)
                    {
                        item.已购买标书 = false;
                    }
                }

                var isclose = Request.Form["closeybm"];
                if (isclose != null && isclose == "1")
                {
                    m.预报名已关闭 = true;
                }
                else
                {
                    m.预报名已关闭 = false;
                }

                招标采购预报名管理.更新招标采购预报名(m, false);
                return Content("<script>alert('设置成功！');window.location='/单位用户后台/SignUp_Detail?id=" + m.Id + "'</script>");
            }
            catch
            {
                return Content("<script>alert('设置失败！');window.location='/单位用户后台/SignUp_List'</script>");
            }
        }
        public ActionResult Part_SignUp_Detail(long? id)
        {
            var pagesize = 20;
            ViewData["currentPage"] = 1;
            ViewData["pagecount"] = 1;
            招标采购预报名 model = null;
            try
            {
                model = 招标采购预报名管理.查找招标采购预报名((long)id);
                var listcount = model.预报名供应商列表.Count;
                var maxpagesize = Math.Max((listcount + pagesize - 1) / pagesize, 1);
                ViewData["currentPage"] = 1;
                ViewData["pagecount"] = maxpagesize;

                var bsstr = "";
                var mailstr = "";
                foreach (var item in model.预报名供应商列表)
                {
                    if (item.已购买标书)
                    {
                        bsstr += item.供应商链接.用户ID.ToString() + "|";
                    }
                    if (item.已发送电子标书)
                    {
                        mailstr += item.供应商链接.用户ID.ToString() + "|";
                    }
                }
                ViewData["已购买标书"] = bsstr;
                ViewData["已发送标书"] = mailstr;

                ViewData["预报名供应商列表"] = model.预报名供应商列表.Take(pagesize);
            }
            catch
            {
                model = null;
                ViewData["已购买标书"] = "";
                ViewData["已发送标书"] = "";
                ViewData["预报名供应商列表"] = new List<Go81WebApp.Models.数据模型.项目数据模型.招标采购预报名._供应商预报名信息>();
            }

            return PartialView("Procure_Part/Part_SignUp_Detail", model);
        }
        public ActionResult Part_SignUp_Detail_Page(long? id, int? page)
        {
            int pagesize = 20;
            ViewData["currentPage"] = 1;
            ViewData["pagecount"] = 1;
            招标采购预报名 model = null;
            try
            {
                model = 招标采购预报名管理.查找招标采购预报名((long)id);

                var listcount = model.预报名供应商列表.Count;
                var maxpagesize = Math.Max((listcount + pagesize - 1) / pagesize, 1);

                if (page == null || page < 1 || page > maxpagesize)
                {
                    page = 1;
                }

                ViewData["currentPage"] = page;
                ViewData["pagecount"] = maxpagesize;

                var bsstr = "";
                var mailstr = "";
                foreach (var item in model.预报名供应商列表)
                {
                    if (item.已购买标书)
                    {
                        bsstr += item.供应商链接.用户ID.ToString() + "|";
                    }
                    if (item.已发送电子标书)
                    {
                        mailstr += item.供应商链接.用户ID.ToString() + "|";
                    }
                }
                ViewData["已购买标书"] = bsstr;
                ViewData["已发送标书"] = mailstr;
                int s_index = (int)(page - 1) * pagesize;
                ViewData["预报名供应商列表"] = model.预报名供应商列表.Skip(s_index).Take(pagesize);
            }
            catch
            {
                model = null;
                ViewData["已购买标书"] = "";
                ViewData["已发送标书"] = "";
                ViewData["预报名供应商列表"] = new List<Go81WebApp.Models.数据模型.项目数据模型.招标采购预报名._供应商预报名信息>();
            }

            return PartialView("Procure_Part/Part_SignUp_Detail_Page", model);
        }
        public ActionResult SignUp_Delete(long? id)
        {
            try
            {
                招标采购预报名管理.删除招标采购预报名((long)id);
                //TD删除后相应改变
            }
            catch
            {

            }
            return View("SignUp_List");
        }
        public ActionResult SignUp_Information()
        {
            return View();
        }
        [HttpPost]
        public ActionResult SignUp_Information(long? id)
        {
            try
            {
                var m = 招标采购预报名管理.查找招标采购预报名((long)id);
                var count = int.Parse(Request.Form["linecount"]);
                if (count > 0)
                {
                    var 供应商所需资料 = new List<招标采购预报名._供应商所需资料>();
                    var chechstr = Request.Form["checkstr"];
                    var checharry = chechstr.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);

                    for (var i = 0; i < count; i++)
                    {
                        var itemmodel = new 招标采购预报名._供应商所需资料();
                        itemmodel.资料名 = Request.Form["informationtitle__" + i];
                        itemmodel.图片 = false;
                        if (checharry[i] == "1")
                        {
                            itemmodel.图片 = true;
                        }
                        供应商所需资料.Add(itemmodel);
                    }
                    m.供应商所需资料 = 供应商所需资料;
                }
                else
                {
                    m.供应商所需资料 = new List<招标采购预报名._供应商所需资料>();
                }
                var isclose = Request.Form["closeybm"];
                if (isclose != null && isclose == "1")
                {
                    m.预报名已关闭 = true;
                }
                else
                {
                    m.预报名已关闭 = false;
                }
                招标采购预报名管理.更新招标采购预报名(m);
                return Content("<script>alert('设置成功！');window.location='/单位用户后台/SignUp_Information?id=" + m.Id + "'</script>");
            }
            catch
            {
                return Content("<script>alert('设置失败！');window.location='/单位用户后台/SignUp_List'</script>");
            }
        }
        public ActionResult Part_SignUp_Information(long? id)
        {
            招标采购预报名 model = null;
            ViewData["已有条件数"] = 1;
            try
            {
                model = 招标采购预报名管理.查找招标采购预报名((long)id);
                if (model.供应商所需资料.Any())
                {
                    ViewData["已有条件数"] = model.供应商所需资料.Count;
                }
            }
            catch
            {
                model = null;
            }
            return PartialView("Procure_Part/Part_SignUp_Information", model);
        }
        public ActionResult SignUp_BiaoShu()
        {
            return View();
        }
        [HttpPost]
        public ActionResult SignUp_BiaoShu(招标采购预报名 model)
        {
            try
            {
                var id = Request.Form["idstr"];
                var m = 招标采购预报名管理.查找招标采购预报名(long.Parse(id));
                var attArr1 = new List<string>();

                var remark = Request.Form["Remarks"];
                m.标书信息.备注 = remark;

                var addattstr = Request.Form["addattstr"];
                var deleteattstr = Request.Form["deleteattstr"];
                var oldattstr = Request.Form["oldattstr"];
                var deleteattArr = deleteattstr.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var item in deleteattArr)
                {
                    if (System.IO.File.Exists(Server.MapPath(item)))
                    {
                        System.IO.File.Delete(Server.MapPath(item));
                    }
                }
                var attstr = oldattstr + addattstr;
                var attArr = attstr.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var item in attArr)
                {
                    attArr1.Add(item);
                }
                m.标书信息.内容 = attArr1;
                var isclose = Request.Form["closeybm"];
                if (isclose != null && isclose == "1")
                {
                    m.预报名已关闭 = true;
                }
                else
                {
                    m.预报名已关闭 = false;
                }
                招标采购预报名管理.更新招标采购预报名(m);
                return Content("<script>alert('设置成功！');window.location='/单位用户后台/SignUp_BiaoShu?id=" + m.Id + "'</script>");
            }
            catch
            {
                return Content("<script>alert('设置失败！');window.location='/单位用户后台/SignUp_List'</script>");
            }
        }
        public ActionResult Part_SignUp_BiaoShu(long? id)
        {
            招标采购预报名 model = null;
            var 已上传附件 = "";
            try
            {
                model = 招标采购预报名管理.查找招标采购预报名((long)id);
                if (model != null && model.标书信息 != null && model.标书信息.内容.Any())
                {
                    foreach (var item in model.标书信息.内容)
                    {
                        已上传附件 += item + "|";
                    }
                }
            }
            catch
            {
                model = null;
            }
            ViewData["已上传附件"] = 已上传附件;
            return PartialView("Procure_Part/Part_SignUp_BiaoShu", model);


            //招标采购预报名 model = null;
            //ViewData["已有标书数"] = 1;
            //try
            //{
            //    model = 招标采购预报名管理.查找招标采购预报名((long)id);
            //    if (model.标书信息.Any())
            //    {
            //        ViewData["已有标书数"] = model.标书信息.Count;
            //    }
            //}
            //catch
            //{
            //    model = null;
            //}
            //return PartialView("Procure_Part/Part_SignUp_BiaoShu", model);
        }
        public ActionResult SignUp_Detail_Gys()
        {
            return View();
        }
        [HttpPost]
        public ActionResult SignUp_Detail_Gys(string action, long? item, long? id)
        {
            try
            {
                var ybm = 招标采购预报名管理.查找招标采购预报名((long)item);
                foreach (var gys in ybm.预报名供应商列表)
                {
                    if (gys.供应商链接.用户ID == (long)id)
                    {
                        if (action == "审核通过")
                        {
                            gys.审核数据.审核状态 = 审核状态.审核通过;
                            gys.审核数据.未通过理由 = string.Empty;
                        }
                        else if (action == "审核不通过")
                        {
                            gys.审核数据.审核状态 = 审核状态.审核未通过;
                            gys.审核数据.未通过理由 = Request.Form["reason"];
                        }
                    }
                }
                招标采购预报名管理.更新招标采购预报名(ybm);
                return Content("<script>alert('设置成功！');window.location='/单位用户后台/SignUp_Detail?id=" + item + "'</script>");
            }
            catch
            {
                return Content("<script>alert('设置失败！');window.location='/单位用户后台/SignUp_List'</script>");
            }
        }
        public ActionResult Part_SignUp_Detail_Gys()
        {
            var item = Request.QueryString["item"];
            var id = Request.QueryString["id"];
            var m = new 招标采购预报名();
            var model = new Dictionary<string, List<string>>();
            ViewData["供应商名称"] = "";
            if (!string.IsNullOrWhiteSpace(item) && !string.IsNullOrWhiteSpace(id))
            {
                m = 招标采购预报名管理.查找招标采购预报名(long.Parse(item));
                ViewData["供应商名称"] = m.预报名供应商列表.Where(o => o.供应商链接.用户ID == long.Parse(id)).First().供应商链接.用户数据.企业基本信息.企业名称;
                model = m.预报名供应商列表.Where(o => o.供应商链接.用户ID == long.Parse(id)).First().供应商提交资料;
            }
            ViewData["该供应商提供的资料"] = model;
            return PartialView("Procure_Part/Part_SignUp_Detail_Gys", m);
        }
        [HttpPost]
        public ActionResult RedirectToRw()
        {
            try
            {
                var idstr = Request.Params["idstr"];
                var idlist = idstr.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var id in idlist)
                {
                    var jh = 需求计划管理.查找需求计划(long.Parse(id));
                    jh.流程已完成 = true;
                    需求计划管理.更新需求计划(jh);
                }
                return Content("1");
            }
            catch
            {
                return Content("0");
            }
        }

        public ActionResult SummaryDemand()
        {
            return View();
        }
        public ActionResult Part_SummaryDemand()
        {
            var t = typeof(秘密等级);
            var vs = Enum.GetValues(t);
            var d = new Dictionary<string, int>();
            foreach (var v in vs)
            {
                d.Add(Enum.GetName(t, v), (int)v);
            }
            ViewData["秘密等级"] = d;

            var idstr = Request.QueryString["id"];
            ViewData["idstr"] = idstr;

            var idlist = idstr.Split(new[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
            var listid = new List<long>();
            foreach (var id in idlist)
            {
                listid.Add(long.Parse(id));
            }

            var demand = 需求计划物资管理.查询需求计划物资(0, 0, Query<需求计划物资>.In(o => o.所属需求计划.需求计划ID, listid), false, SortBy<需求计划物资>.Ascending(o => o.物资名称, o => o.规格型号, o => o.计量单位, o => o.技术指标));

            ViewData["计划详情"] = 需求计划管理.查询需求计划(0, 0, Query<需求计划>.In(o => o.Id, listid));


            var samedemand = demand.GroupBy(o => new { o.物资名称, o.规格型号, o.计量单位, o.技术指标 });
            var demandlist = samedemand.Select(
                g =>
                {
                    var result = new 需求计划物资
                    {
                        物资名称 = g.Key.物资名称,
                        规格型号 = g.Key.规格型号,
                        计量单位 = g.Key.计量单位,
                        技术指标 = g.Key.技术指标,
                        数量 = g.Sum(o => o.数量),
                        交货期限 = g.Min(o => o.交货期限),
                        建议采购方式 = g.Max(o => o.建议采购方式),
                        备注 = string.Join("\r\n", g.Select(o => o.备注)),
                        单价 = g.Max(o => o.单价),
                        预算金额 = g.Sum(o => o.预算金额),
                        来源合并项 = new List<需求计划物资链接>(g.Select(o => new 需求计划物资链接 { 需求计划物资ID = o.Id })),
                        所属需求计划 = new 需求计划链接()
                    };
                    return result;
                }
                );
            IEnumerable<单位用户> user = 用户管理.查询用户<单位用户>(0, 0);
            ViewData["user"] = user;
            ViewData["联系人"] = string.IsNullOrWhiteSpace(currentUser.联系方式.联系人) ? "" : currentUser.联系方式.联系人;

            return PartialView("Procure_Part/Part_SummaryDemand", demandlist);
        }
        public ActionResult SummaryDetail()
        {
            var idstr = Request.Params["id"];
            var idlist = idstr.Split(new[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
            var listid = new List<long>();
            foreach (var id in idlist)
            {
                listid.Add(long.Parse(id));
            }
            var demandlist = 需求计划物资管理.查询需求计划物资(0, 0, Query<需求计划物资>.In(o => o.Id, listid));

            return PartialView("Procure_Part/Part_SummaryDetail", demandlist);
        }
        public ActionResult DemandDetail()
        {
            var idstr = Request.Params["id"];
            var idlist = idstr.Split(new[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
            var listid = new List<long>();
            foreach (var id in idlist)
            {
                listid.Add(long.Parse(id));
            }
            var demandlist = 需求计划物资管理.查询需求计划物资(0, 0, Query<需求计划物资>.In(o => o.Id, listid));
            return PartialView("Procure_Part/Part_DemandDetail", demandlist);
        }
        public ActionResult DemanCheck()
        {
            var idstr = Request.Params["id"];
            var idlist = 需求计划物资管理.查找需求计划物资(long.Parse(idstr)).来源合并项.Select(o => o.需求计划物资ID);
            var demandlist = 需求计划物资管理.查询需求计划物资(0, 0, Query<需求计划物资>.In(o => o.Id, idlist));
            if (demandlist.Any())
            {
                return PartialView("Procure_Part/Part_DemanCheck", demandlist);
            }
            return Content("0");
        }
        public ActionResult SummaryDetail_Recursive()
        {
            var idstr = Request.Params["id"];
            var idlist = 需求计划物资管理.查找需求计划物资(long.Parse(idstr)).来源合并项.Select(o => o.需求计划物资ID);
            var demandlist = 需求计划物资管理.查询需求计划物资(0, 0, Query<需求计划物资>.In(o => o.Id, idlist));
            if (demandlist.Any())
            {
                return PartialView("Procure_Part/Part_SummaryDetail_Recursive", demandlist);
            }
            return Content("0");
        }
        [HttpPost]
        public ActionResult SummaryDemand(long? id)
        {
            ///////////////////审核单位未处理(审批流程单位列表、审核历史列表、当前处理单位链接、)

            var resutstr = Request.Form["resultparm"];
            var resultid = Request.Form["resultid"];
            var auditunitlist = Request.Form["auditunitlist"];

            var 需求计划标题 = Request.Form["需求计划标题"];
            var 建议完成时间 = Request.Form["建议完成时间"];

            var 秘密等级 = Request.Form["秘密等级"];
            var 编制单位 = Request.Form["编制单位"];
            var 承办部门 = Request.Form["承办部门"];
            var 采购年度 = Request.Form["采购年度"];


            var 联系人 = Request.Form["联系人"];
            var 联系电话 = Request.Form["联系电话"];
            //插入数据库操作
            需求计划 xqjh = new 需求计划();
            xqjh.需求发起单位链接.用户ID = currentUser.Id;

            //设置审批流程
            var auditidlist = auditunitlist.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries).ToList();
            if (auditidlist.Contains("p"))
            {
                foreach (var auditid in auditidlist.Where(o => o != "p"))
                {
                    var auditunitid = Request.Form["审核单位__" + auditid];
                    xqjh.审批流程单位列表.Add(new 用户链接<单位用户> { 用户ID = long.Parse(auditunitid) });
                }
                var auditunitidp = Request.Form["审核单位__P"];
                xqjh.审批流程单位列表.Add(new 用户链接<单位用户> { 用户ID = long.Parse(auditunitidp) });
            }
            else
            {
                foreach (var auditid in auditidlist)
                {
                    var auditunitid = Request.Form["审核单位__" + auditid];
                    xqjh.审批流程单位列表.Add(new 用户链接<单位用户> { 用户ID = long.Parse(auditunitid) });
                }
            }

            //设置审批流程
            xqjh.当前处理单位链接 = xqjh.审批流程单位列表.First();

            xqjh.需求计划标题 = 需求计划标题;
            xqjh.建议完成时间 = Convert.ToDateTime(建议完成时间);
            xqjh.联系人 = 联系人;
            xqjh.联系电话 = 联系电话;

            xqjh.秘密等级 = (秘密等级)Enum.Parse(typeof(秘密等级), 秘密等级);
            xqjh.编制单位 = 编制单位;
            xqjh.承办部门 = 承办部门;
            xqjh.采购年度 = new DateTime(int.Parse(采购年度), 1, 1);

            需求计划管理.添加需求计划(xqjh);
            var idlist = resultid.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var jhid in idlist)
            {
                var jhtemp = 需求计划管理.查找需求计划(long.Parse(jhid));
                jhtemp.并入需求计划链接.需求计划ID = xqjh.Id;
                //需求计划._审核数据 audittemp = new 需求计划._审核数据
                //{
                //    审核状态 = 审核状态.审核通过,
                //    审核者 = new 用户链接<用户基本数据> { 用户ID = currentUser.Id },
                //    审核时间 = DateTime.Now
                //};
                //jhtemp.审核历史列表.Add(audittemp);
                需求计划管理.更新需求计划(jhtemp);

                foreach (var jdff in jhtemp.分发列表)
                {
                    xqjh.分发列表.Add(new 需求计划分发链接 { 需求计划分发ID = jdff.需求计划分发ID });
                }

                xqjh.来源需求计划列表.Add(new 需求计划链接 { 需求计划ID = jhtemp.Id });
            }
            var resultlist = resutstr.Split(new[] { "$$$$" }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var dataliststr in resultlist)
            {
                var wz = new 需求计划物资();
                var datalist = dataliststr.Split(new[] { "^^^^" }, StringSplitOptions.None);
                var linklist = datalist[0].Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var link in linklist)
                {
                    wz.来源合并项.Add(new 需求计划物资链接 { 需求计划物资ID = long.Parse(link) });
                }
                wz.物资名称 = datalist[1];
                wz.规格型号 = datalist[2];
                wz.计量单位 = datalist[3];
                wz.技术指标 = datalist[4];
                wz.建议采购方式 = datalist[5];
                wz.单价 = decimal.Parse(datalist[6]);
                wz.数量 = int.Parse(datalist[7]);
                wz.预算金额 = decimal.Parse(datalist[8]);
                wz.交货期限 = Convert.ToDateTime(datalist[9]);
                wz.备注 = datalist[10];
                wz.所属需求计划.需求计划ID = xqjh.Id;

                需求计划物资管理.添加需求计划物资(wz);

                xqjh.物资列表.Add(new 需求计划物资链接 { 需求计划物资ID = wz.Id });
            }
            需求计划管理.更新需求计划(xqjh);
            return RedirectToAction("DemandCheck");
        }
        [HttpPost]
        public ActionResult auditSubmitJh()
        {
            var summaryidlist = Request.Form["summaryidlist"];

            var auditunitlist = Request.Form["auditunitlist"];
            var 需求计划标题 = Request.Form["需求计划标题"];
            var 建议完成时间 = Request.Form["建议完成时间"];
            var 秘密等级 = Request.Form["秘密等级"];
            var 编制单位 = Request.Form["编制单位"];
            var 承办部门 = Request.Form["承办部门"];
            var 采购年度 = Request.Form["采购年度"];
            var 联系人 = Request.Form["联系人"];
            var 联系电话 = Request.Form["联系电话"];
            //插入数据库操作
            需求计划 xqjh = new 需求计划();
            xqjh.需求发起单位链接.用户ID = currentUser.Id;
            //设置审批流程
            var auditidlist = auditunitlist.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries).ToList();
            if (auditidlist.Contains("p"))
            {
                foreach (var auditid in auditidlist.Where(o => o != "p"))
                {
                    var auditunitid = Request.Form["审核单位__" + auditid];
                    xqjh.审批流程单位列表.Add(new 用户链接<单位用户> { 用户ID = long.Parse(auditunitid) });
                }
                var auditunitidp = Request.Form["审核单位__P"];
                xqjh.审批流程单位列表.Add(new 用户链接<单位用户> { 用户ID = long.Parse(auditunitidp) });
            }
            else
            {
                foreach (var auditid in auditidlist)
                {
                    var auditunitid = Request.Form["审核单位__" + auditid];
                    xqjh.审批流程单位列表.Add(new 用户链接<单位用户> { 用户ID = long.Parse(auditunitid) });
                }
            }
            //设置审批流程
            xqjh.当前处理单位链接 = xqjh.审批流程单位列表.First();
            xqjh.需求计划标题 = 需求计划标题;
            xqjh.建议完成时间 = Convert.ToDateTime(建议完成时间);
            xqjh.联系人 = 联系人;
            xqjh.联系电话 = 联系电话;
            xqjh.秘密等级 = (秘密等级)Enum.Parse(typeof(秘密等级), 秘密等级);
            xqjh.编制单位 = 编制单位;
            xqjh.承办部门 = 承办部门;
            xqjh.采购年度 = new DateTime(int.Parse(采购年度), 1, 1);
            需求计划管理.添加需求计划(xqjh);

            var summarylist = summaryidlist.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries).ToList();
            foreach (var jhid in summarylist)
            {
                var jh = 需求计划管理.查找需求计划(long.Parse(jhid));
                jh.并入需求计划链接.需求计划ID = xqjh.Id;
                需求计划管理.更新需求计划(jh);

                xqjh.来源需求计划列表.Add(new 需求计划链接 { 需求计划ID = jh.Id });
                foreach (var ff in jh.分发列表)
                {
                    xqjh.分发列表.Add(new 需求计划分发链接 { 需求计划分发ID = ff.需求计划分发ID });
                }
                foreach (var ff in jh.物资列表)
                {
                    //复制物资，加入来源合并项
                    var wz = ff.需求计划物资数据;
                    wz.Id = -1;
                    wz.来源合并项 = new List<需求计划物资链接>();
                    wz.来源合并项.Add(new 需求计划物资链接 { 需求计划物资ID = ff.需求计划物资ID });
                    wz.所属需求计划.需求计划ID = xqjh.Id;
                    需求计划物资管理.添加需求计划物资(wz);
                    xqjh.物资列表.Add(new 需求计划物资链接 { 需求计划物资ID = wz.Id });
                }
            }
            需求计划管理.更新需求计划(xqjh);
            return RedirectToAction("DemandCheck");
        }

        [单一权限验证(权限.编制采购任务)]
        public ActionResult AssignmentTaskList()
        {
            return View();
        }
        public ActionResult Part_AssignmentTaskList()
        {
            var page = 1;
            var pagesize = 10;

            ///////////待下达任务的需求列表
            var liscount_pre = 需求计划管理.计数需求计划(0, 0,
                Query<需求计划>.Where(
                    o =>
                        //o.审批流程单位列表.Last().用户ID == 10003 && o.审核历史列表.Last().审核者.用户ID == 10003 &&
                        o.审核历史列表.Any(p => p.审核状态 == 审核状态.审核通过) && (o.需求发起单位链接.用户ID == currentUser.Id || o.当前处理单位链接.用户ID == currentUser.Id) && o.并入需求计划链接.需求计划ID == -1 && o.流程已完成 && !o.已下达));

            var maxpage_pre = Math.Max((liscount_pre + pagesize - 1) / pagesize, 1);
            var AssignmentTaskList = 需求计划管理.查询需求计划(0, pagesize,
                Query<需求计划>.Where(
                    o =>
                        //o.审批流程单位列表.Last().用户ID == 10003 && o.审核历史列表.Last().审核者.用户ID == 10003 &&
                        o.审核历史列表.Any(p => p.审核状态 == 审核状态.审核通过) && (o.需求发起单位链接.用户ID == currentUser.Id || o.当前处理单位链接.用户ID == currentUser.Id) && o.并入需求计划链接.需求计划ID == -1 && o.流程已完成 && !o.已下达));

            ViewData["pre_currentPage"] = page;
            ViewData["pre_pagecount"] = maxpage_pre;
            ViewData["pre_AssignmentTaskList"] = AssignmentTaskList;
            ///////////待下达任务的需求列表

            ///////////已下达任务的需求列表
            var liscount_ed = 需求采购任务管理.计数需求采购任务(0, 0, Query<需求采购任务>.Where(o => o.需求发起单位链接.用户ID == currentUser.Id));
            var maxpage_ed = Math.Max((liscount_ed + pagesize - 1) / pagesize, 1);
            var AssignmentTaskList_ed = 需求采购任务管理.查询需求采购任务(0, pagesize, Query<需求采购任务>.Where(o => o.需求发起单位链接.用户ID == currentUser.Id));
            ViewData["ed_currentPage"] = page;
            ViewData["ed_pagecount"] = maxpage_ed;
            ViewData["ed_AssignmentTaskList"] = AssignmentTaskList_ed;
            ///////////已下达任务的需求列表

            ViewData["userid"] = currentUser.Id;
            IEnumerable<单位用户> user = 用户管理.查询用户<单位用户>(0, 0);
            ViewData["user"] = user;
            var t = typeof(采购方式);
            var way = Enum.GetValues(t);
            Dictionary<int, string> ways = new Dictionary<int, string>();
            foreach (var item in way)
            {
                ways.Add((int)item, Enum.GetName(t, item));
            }
            ViewData["采购方式"] = ways;
            return PartialView("Procure_Part/Part_AssignmentTaskList");
        }

        [单一权限验证(权限.受理采购任务)]
        public ActionResult AssignmentTaskList_p()
        {
            return View();
        }
        public ActionResult Part_AssignmentTaskList_p()
        {
            var page = 1;
            var pagesize = 10;

            var list = 需求采购任务管理.查询需求采购任务(0, 0, Query<需求采购任务>.Where(o => o.受理单位.用户ID == currentUser.Id));
            /////////已下达任务的需求列表
            var liscount_ing = list.Where(o => o.审核历史列表.Any() && o.审核历史列表.Count == o.审批流程单位列表.Count && o.审核历史列表.Last().审核状态 == 审核状态.审核通过).Count();//需求采购任务管理.计数需求采购任务(0, 0, Query<需求采购任务>.Where(o => o.受理单位.用户ID == currentUser.Id));
            var maxpage_ing = Math.Max((liscount_ing + pagesize - 1) / pagesize, 1);
            var AssignmentTaskList_ing =
                list.Where(o => o.审核历史列表.Any() && o.审核历史列表.Count == o.审批流程单位列表.Count && o.审核历史列表.Last().审核状态 == 审核状态.审核通过).Take(pagesize);
            ViewData["ing_currentPage"] = page;
            ViewData["ing_pagecount"] = maxpage_ing;
            ViewData["ing_AssignmentTaskList"] = AssignmentTaskList_ing;
            ///////////已下达任务的需求列表
            return PartialView("Procure_Part/Part_AssignmentTaskList_p");
        }
        [单一权限验证(权限.审核采购任务)]
        public ActionResult AssignmentTaskAuditList()
        {
            return View();
        }
        public ActionResult Part_AssignmentTaskAuditList()
        {
            try
            {
                long pgCount = 0;
                int cpg = 0;
                if (!string.IsNullOrWhiteSpace(Request.QueryString["page"]))
                {
                    cpg = int.Parse(Request.QueryString["page"]);
                }
                if (cpg <= 0)
                {
                    cpg = 1;
                }
                long pc = 需求采购任务管理.计数需求采购任务(0, 0, Query<需求采购任务>.Where(o => o.当前处理单位链接.用户ID == currentUser.Id));
                pgCount = pc / 10;
                if (pc % 10 > 0)
                {
                    pgCount++;
                }
                ViewData["Pagecount"] = pgCount;
                ViewData["CurrentPage"] = cpg;
                var list = 需求采购任务管理.查询需求采购任务(10 * (cpg - 1), 10, Query<需求采购任务>.Where(o => o.当前处理单位链接.用户ID == currentUser.Id));
                ViewData["AssignmentTaskList"] = list.Where(o => o.审批流程单位列表.Count > o.审核历史列表.Count);
                return PartialView("Procure_Part/Part_AssignmentTaskAuditList");
            }
            catch
            {
                return Content("<script>window.location='/单位用户后台/AssignmentTaskAuditList';</script>");
            }
        }
        public ActionResult AssignmentTaskAudit()
        {
            return View();
        }
        public ActionResult Part_AssignmentTaskAudit(long? id)
        {
            var rw = 需求采购任务管理.查找需求采购任务((long)id);
            IEnumerable<单位用户> user = 用户管理.查询用户<单位用户>(0, 0);
            ViewData["user"] = user;
            var t = typeof(采购方式);
            var way = Enum.GetValues(t);
            Dictionary<int, string> ways = new Dictionary<int, string>();
            foreach (var item in way)
            {
                ways.Add((int)item, Enum.GetName(t, item));
            }
            ViewData["采购方式"] = ways;
            return PartialView("Procure_Part/Part_AssignmentTaskAudit", rw);
        }
        public int PassTask()
        {
            long id = long.Parse(Request.QueryString["id"]);
            需求采购任务 task = 需求采购任务管理.查找需求采购任务((long)id);
            bool ischeck = false;
            foreach (var item in task.审核历史列表)
            {
                if (item.审核者.用户ID == currentUser.Id)
                {
                    ischeck = true;
                    break;
                }
            }
            if (!ischeck)
            {
                需求计划._审核数据 h = new 需求计划._审核数据();
                h.审核者.用户ID = currentUser.Id;
                h.审核状态 = 审核状态.审核通过;
                h.审核时间 = DateTime.Now;
                task.审核历史列表.Add(h);
                if (task.审批流程单位列表.Any())
                {
                    for (int i = 0; i < task.审批流程单位列表.Count; i++)
                    {
                        if (task.审批流程单位列表[i].用户ID == currentUser.Id)
                        {
                            if ((i + 1) < task.审批流程单位列表.Count)
                            {
                                task.当前处理单位链接.用户ID = task.审批流程单位列表[i + 1].用户ID;
                                break;
                            }
                        }
                    }
                }
            }
            return 需求采购任务管理.更新需求采购任务(task) ? 1 : -1;
        }
        [HttpPost]
        public ActionResult AssignmentTaskAudit(long? id)
        {
            var rw = 需求采购任务管理.查找需求采购任务((long)id);
            if (!rw.审核历史列表.Any() || rw.审核历史列表.Last().审核者.用户ID != currentUser.Id)
            {
                需求计划._审核数据 audit = new 需求计划._审核数据
                {
                    审核时间 = DateTime.Now,
                    审核状态 = 审核状态.审核通过,
                    审核者 = new 用户链接<用户基本数据> { 用户ID = currentUser.Id }
                };
                rw.审核历史列表.Add(audit);
                for (var i = 0; i < rw.审批流程单位列表.Count; i++)
                {
                    if (rw.审批流程单位列表[i].用户ID == currentUser.Id && rw.审批流程单位列表.Last().用户ID != currentUser.Id)
                    {
                        rw.当前处理单位链接.用户ID = rw.审批流程单位列表[i + 1].用户ID;
                        break;
                    }
                }
                需求采购任务管理.更新需求采购任务(rw);
            }
            return RedirectToAction("AssignmentTaskAuditList");
        }
        public ActionResult Part_AssignmentTaskList_pre(int? page)
        {
            var pagesize = 10;

            var liscount_pre = 需求计划管理.计数需求计划(0, 0,
                Query<需求计划>.Where(
                    o =>
                        //o.审批流程单位列表.Last().用户ID == 10003 && o.审核历史列表.Last().审核者.用户ID == 10003 &&
                        o.审核历史列表.Any(p => p.审核状态 == 审核状态.审核通过) && (o.需求发起单位链接.用户ID == currentUser.Id || o.当前处理单位链接.用户ID == currentUser.Id) && o.并入需求计划链接.需求计划ID == -1 && o.流程已完成 && !o.已下达));

            var maxpage_pre = Math.Max((liscount_pre + pagesize - 1) / pagesize, 1);
            if (string.IsNullOrEmpty(page.ToString()) || page < 0 || page > maxpage_pre)
            {
                page = 1;
            }
            var AssignmentTaskList = 需求计划管理.查询需求计划(pagesize * (int.Parse(page.ToString()) - 1), pagesize,
                 Query<需求计划>.Where(
                     o =>
                         //o.审批流程单位列表.Last().用户ID == 10003 && o.审核历史列表.Last().审核者.用户ID == 10003 &&
                         o.审核历史列表.Any(p => p.审核状态 == 审核状态.审核通过) && (o.需求发起单位链接.用户ID == currentUser.Id || o.当前处理单位链接.用户ID == currentUser.Id) && o.并入需求计划链接.需求计划ID == -1 && o.流程已完成 && !o.已下达));

            ViewData["pre_currentPage"] = page;
            ViewData["pre_pagecount"] = maxpage_pre;
            ViewData["pre_AssignmentTaskList"] = AssignmentTaskList;
            return PartialView("Procure_Part/Part_AssignmentTaskList_pre");
        }
        public ActionResult Part_AssignmentTaskList_ed(int? page)
        {
            var pagesize = 10;
            var liscount_ed = 需求采购任务管理.计数需求采购任务(0, 0, Query<需求采购任务>.Where(o => o.需求发起单位链接.用户ID == currentUser.Id));
            var maxpage_ed = Math.Max((liscount_ed + pagesize - 1) / pagesize, 1);
            if (string.IsNullOrEmpty(page.ToString()) || page < 0 || page > maxpage_ed)
            {
                page = 1;
            }
            var AssignmentTaskList_ed = 需求采购任务管理.查询需求采购任务(pagesize * (int.Parse(page.ToString()) - 1), pagesize, Query<需求采购任务>.Where(o => o.需求发起单位链接.用户ID == currentUser.Id));
            ViewData["ed_currentPage"] = page;
            ViewData["ed_pagecount"] = maxpage_ed;
            ViewData["ed_AssignmentTaskList"] = AssignmentTaskList_ed;
            return PartialView("Procure_Part/Part_AssignmentTaskList_ed");
        }
        public ActionResult Part_AssignmentTaskList_ing(int? page)
        {
            var pagesize = 10;

            var list = 需求采购任务管理.查询需求采购任务(0, 0, Query<需求采购任务>.Where(o => o.受理单位.用户ID == currentUser.Id));
            /////////已下达任务的需求列表
            var liscount_ing = list.Count(o => o.审核历史列表.Count == o.审批流程单位列表.Count && o.审核历史列表.Last().审核状态 == 审核状态.审核通过);//需求采购任务管理.计数需求采购任务(0, 0, Query<需求采购任务>.Where(o => o.受理单位.用户ID == currentUser.Id));
            var maxpage_ing = Math.Max((liscount_ing + pagesize - 1) / pagesize, 1);

            if (string.IsNullOrEmpty(page.ToString()) || page < 0 || page > maxpage_ing)
            {
                page = 1;
            }

            var AssignmentTaskList_ing =
                list.Where(o => o.审核历史列表.Count == o.审批流程单位列表.Count && o.审核历史列表.Last().审核状态 == 审核状态.审核通过).Skip(pagesize * ((int)page - 1)).Take(pagesize);
            ViewData["ing_currentPage"] = page;
            ViewData["ing_pagecount"] = maxpage_ing;
            ViewData["ing_AssignmentTaskList"] = AssignmentTaskList_ing;


            //var pagesize = 10;
            //var liscount_ing = 需求采购任务管理.计数需求采购任务(0, 0, Query<需求采购任务>.Where(o => o.受理单位.用户ID == currentUser.Id));
            //var maxpage_ing = Math.Max((liscount_ing + pagesize - 1) / pagesize, 1);
            //if (string.IsNullOrEmpty(page.ToString()) || page < 0 || page > maxpage_ing)
            //{
            //    page = 1;
            //}
            //var AssignmentTaskList_ing = 需求采购任务管理.查询需求采购任务(pagesize * (int.Parse(page.ToString()) - 1), pagesize, Query<需求采购任务>.Where(o => o.受理单位.用户ID == currentUser.Id));
            //ViewData["ing_currentPage"] = page;
            //ViewData["ing_pagecount"] = maxpage_ing;
            //ViewData["ing_AssignmentTaskList"] = AssignmentTaskList_ing;
            return PartialView("Procure_Part/Part_AssignmentTaskList_ing");
        }

        public ActionResult AssignmentTask()
        {
            return View();
        }
        public ActionResult Part_AssignmentTask(long? id)
        {
            IEnumerable<单位用户> user = 用户管理.查询用户<单位用户>(0, 0);
            ViewData["user"] = user;
            var jh = 需求计划管理.查找需求计划((long)id);
            return PartialView("Procure_Part/Part_AssignmentTask", jh);
        }
        [HttpPost]
        public ActionResult AssignmentTask(long? id)
        {
            var topuser = Request.Form["topuser"];
            var tempidliststr = Request.Form["diverseidlidt"];
            var tempidlist = tempidliststr.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var tempid in tempidlist)
            {
                var wzidliststr = Request.Form["wzid___" + tempid];
                var ffidliststr = Request.Form["ffid___" + tempid];
                var ms = Request.Form["ms___" + tempid];
                var dw = Request.Form["dw___" + tempid];

                var cgfs = Request.Form["cgfs___" + tempid];
                var wcsj = Request.Form["wcsj___" + tempid];
                var lxr = Request.Form["lxr___" + tempid];
                var lxdh = Request.Form["lxdh___" + tempid];

                var wzlink = new List<需求计划物资链接>();
                var fflink = new List<需求计划分发链接>();

                var wzidlist = wzidliststr.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
                var ffidlist = ffidliststr.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var wzid in wzidlist)
                {
                    wzlink.Add(new 需求计划物资链接 { 需求计划物资ID = long.Parse(wzid) });
                }
                foreach (var ffid in ffidlist)
                {
                    fflink.Add(new 需求计划分发链接 { 需求计划分发ID = long.Parse(ffid) });
                }
                var xqjh = new 需求采购任务();
                xqjh.分发列表 = fflink;
                xqjh.物资列表 = wzlink;
                xqjh.需求发起单位链接.用户ID = currentUser.Id;
                xqjh.受理单位.用户ID = long.Parse(dw);
                xqjh.联系人 = lxr;
                xqjh.联系电话 = lxdh;
                xqjh.建议完成时间 = Convert.ToDateTime(wcsj);
                xqjh.描述 = ms;
                xqjh.采购方式 = (采购方式)int.Parse(cgfs);

                var auditunitlist = Request.Form["shdw___a"];
                var tempunitlist = auditunitlist.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var unitid in tempunitlist)
                {
                    xqjh.审批流程单位列表.Add(new 用户链接<单位用户> { 用户ID = long.Parse(unitid) });
                }
                if (topuser != "-1" && topuser != "")
                {
                    xqjh.审批流程单位列表.Add(new 用户链接<单位用户> { 用户ID = long.Parse(topuser) });
                }
                xqjh.当前处理单位链接.用户ID = xqjh.审批流程单位列表.First().用户ID;

                需求采购任务管理.添加需求采购任务(xqjh);
            }
            var jh = 需求计划管理.查找需求计划((long)id);
            jh.已下达 = true;
            需求计划管理.更新需求计划(jh);
            return RedirectToAction("AssignmentTaskList");
            //return View("AssignmentTaskList");
        }
        public ActionResult AssignmentTaskDetail()
        {
            return View();
        }
        public ActionResult Part_AssignmentTaskDetail(long? id)
        {
            var rw = 需求采购任务管理.查找需求采购任务((long)id);
            return PartialView("Procure_Part/Part_AssignmentTaskDetail", rw);
        }

        [HttpPost]
        public ActionResult UploadImage_BiaoShu()
        {
            try
            {
                string path1 = "";
                for (int i = 0; i < Request.Files.Count; i++)
                {
                    HttpPostedFileBase file = Request.Files[i];
                    if (file != null && (((file.ContentLength / 1024) / 1024) < 100))
                    {
                        //string filePath = "";
                        string fname = "";
                        fname = UploadAttachment_BiaoShu(file);
                        path1 += fname + "|";
                    }
                }
                ViewData["path"] = path1;
                return View();
            }
            catch
            {
                ViewData["path"] = "上传出错!|";
                return View();
            }
        }
        public string UploadAttachment_BiaoShu(HttpPostedFileBase fileData)
        {
            if (fileData != null)
            {
                string fileName = fileData.FileName;
                var fileName1 = 上传管理.获取上传路径<招标采购预报名>(媒体类型.标书, 路径类型.不带域名根路径);
                if (fileName != "" && fileName != null)
                {
                    string filePath = "";
                    filePath = 上传管理.获取上传路径<招标采购预报名>(媒体类型.标书, 路径类型.服务器本地路径);
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
                return fileName1 + fileName;
            }
            else
            {
                return null;
            }
        }
        public int DeleteImages_Att()
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
        //////////////////////////////////////////////////////////////////////////////////////////////////////////招标采购预报名

        [单一权限验证(权限.全部附属账号列表)]
        public ActionResult Print_UserList()
        {
            return View();
        }
        public ActionResult Part_Print_UserList(int? page)
        {
            var pagesize = 15;

            //var liscount = 用户管理.计数用户<单位用户>(0, 0, Query.EQ("审核数据.审核状态", 审核状态.审核通过));
            var q = Query<单位用户>.Where(o => o.管理单位.用户ID == currentUser.Id && o.审核数据.审核状态 == 审核状态.审核通过);
            var liscount = 用户管理.计数用户<单位用户>(0, 0, q);

            var maxpage = Math.Max((liscount + pagesize - 1) / pagesize, 1);
            if (string.IsNullOrEmpty(page.ToString()) || page < 0 || page > maxpage)
            {
                page = 1;
            }
            ViewData["currentPage"] = page;
            ViewData["pagecount"] = maxpage;

            ViewData["查询单位列表"] = 用户管理.查询用户<单位用户>(((int)page - 1) * pagesize, pagesize, q);
            return PartialView("Procure_Part/Part_Print_UserList");
        }

        //项目需求模块开始---------------------------

        public ActionResult Project_Applay()
        {
            return View();
        }
        public ActionResult Part_Project_Applay()
        {
            return PartialView("Procure_Part/Part_Project_Applay");
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Project_Applay(需求申请 model)
        {
            try
            {
                model.需求提报单位.用户ID = currentUser.Id;
                需求申请管理.添加需求申请(model);
                招标采购项目 m = new 招标采购项目();
                m.需求提报单位.用户ID = currentUser.Id;
                m.需求申请来源.需求申请ID = model.Id;
                招标采购项目管理.添加招标采购项目(m);
            }
            catch
            {
                return Content("<script>alert('添加项目需求申请出现问题！');location.href='javascript:history.go(-1)';</script>");
            }
            return Redirect("/单位用户后台/Project_List");
        }
        public ActionResult Project_List()
        {
            return View();
        }
        public ActionResult Part_Project_List()
        {
            long pgCount = 0;
            int cpg = 0;
            if (!string.IsNullOrWhiteSpace(Request.QueryString["page"]))
            {
                cpg = int.Parse(Request.QueryString["page"]);
            }
            if (cpg <= 0)
            {
                cpg = 1;
            }
            long pc = 招标采购项目管理.计数招标采购项目(0, 0, MongoDB.Driver.Builders.Query.EQ("需求提报单位.用户ID", currentUser.Id));
            pgCount = pc / 10;
            if (pc % 10 > 0)
            {
                pgCount++;
            }
            ViewData["Pagecount"] = pgCount;
            ViewData["CurrentPage"] = cpg;
            ViewData["我的申请列表"] = 招标采购项目管理.查询招标采购项目(10 * (cpg - 1), 10, MongoDB.Driver.Builders.Query.EQ("需求提报单位.用户ID", currentUser.Id));

            return PartialView("Procure_Part/Part_Project_List");
        }

        public ActionResult Project_Modify()
        {
            return View();
        }
        public ActionResult Part_Project_Modify(int? id)
        {
            需求申请 g = null;
            try
            {
                招标采购项目 model = 招标采购项目管理.查找招标采购项目((long)id);
                g = model.需求申请来源.需求申请;
                if (model.审核数据.审核状态 == 审核状态.审核通过)//已审核的招标项目不能再进行修改
                {
                    return Content("<script>alert('修改时发生权限错误，请重试');location.href='javascript:history.go(-1)';</script>");
                }
            }
            catch
            {
                g = null;
            }

            return PartialView("Procure_Part/Part_Project_Modify", g);
        }

        [HttpPost]
        [ValidateInput(false)]

        public ActionResult Project_Modify(需求申请 model)
        {
            try
            {
                需求申请 m = 需求申请管理.查找需求申请(model.Id);
                m.标题 = model.标题;
                m.正文 = model.正文;
                需求申请管理.更新需求申请(m);
                招标采购项目 ml = 招标采购项目管理.查询招标采购项目(0, 1, MongoDB.Driver.Builders.Query.EQ("需求申请来源.需求申请ID", model.Id)).ToList()[0];

                ml.审核数据.审核状态 = 审核状态.未审核;//修改需求后，将项目的状态置未未审核，管理员重新进行审核

                招标采购项目管理.更新招标采购项目(ml);
                return View("Project_List");
            }
            catch
            {
                return Content("<script>alert('修改时发生错误，请重试');location.href='javascript:history.go(-1)';</script>");
            }

        }

        public ActionResult Project_Delete(long? id)
        {
            try
            {
                var nid = 招标采购项目管理.查找招标采购项目((long)id).需求申请来源.需求申请ID;
                需求申请管理.删除需求申请(nid);
                招标采购项目管理.删除招标采购项目((long)id);
                //TD删除后相应改变

                var url = Request.Params["link"];
                if (url == "auditlist")
                {
                    return View("Project_AuditList");
                }
                else if (url == "hislist")
                {
                    return View("Project_HistoryList");
                }
                else if (url == "zblist")
                {
                    return View("Project_ZbList");
                }
                else
                {
                    return View("Project_List");
                }

            }
            catch
            {
                return View("Project_List");
            }
        }
        public ActionResult Project_Detail()
        {
            return View();
        }
        public ActionResult Part_Project_Detail(int? id)
        {
            招标采购项目 g = null;
            try
            {
                if (null != id)
                {
                    g = 招标采购项目管理.查找招标采购项目((long)id);
                    if (g == null)
                    {
                        return Redirect("/单位用户后台/Project_HistoryList");
                    }
                }
            }
            catch
            {

            }
            return PartialView("Procure_Part/Part_Project_Detail", g);
        }

        public ActionResult Project_AuditList()
        {
            return View();
        }
        public ActionResult Part_Project_AuditList()
        {
            //TD:审核需加条件《只有权限审核向本用户提交的数据》
            long pgCount = 0;
            int cpg = 0;
            if (!string.IsNullOrWhiteSpace(Request.QueryString["page"]))
            {
                cpg = int.Parse(Request.QueryString["page"]);
            }
            if (cpg <= 0)
            {
                cpg = 1;
            }
            long pc = 招标采购项目管理.计数招标采购项目(0, 0, Query.EQ("审核数据.审核状态", 审核状态.未审核));
            pgCount = pc / 10;
            if (pc % 10 > 0)
            {
                pgCount++;
            }
            ViewData["Pagecount"] = pgCount;
            ViewData["CurrentPage"] = cpg;
            ViewData["未审核项目列表"] = 招标采购项目管理.查询招标采购项目(10 * (cpg - 1), 10, Query.EQ("审核数据.审核状态", 审核状态.未审核));
            return PartialView("Procure_Part/Part_Project_AuditList");
        }

        public ActionResult Project_AuditDetail()
        {
            return View();
        }
        public ActionResult Part_Project_AuditDetail(int? id)
        {
            招标采购项目 g = null;
            try
            {
                if (null != id)
                {
                    g = 招标采购项目管理.查找招标采购项目((long)id);
                    if (g == null)
                    {
                        return Redirect("/单位用户后台/Project_AuditList");
                    }
                }
            }
            catch
            {
                return Redirect("/单位用户后台/Project_AuditList");
            }
            return PartialView("Procure_Part/Part_Project_AuditDetail", g);
        }
        [HttpPost]
        public ActionResult Project_AuditDetail(招标采购项目 model, string action)
        {
            try
            {

                招标采购项目 m = 招标采购项目管理.查找招标采购项目(model.Id);
                if (action == "审核通过")
                {
                    m.审核数据.审核状态 = 审核状态.审核通过;
                    m.审核数据.审核者.用户ID = currentUser.Id;
                    m.项目编号 = model.项目编号;
                    m.项目名称 = model.项目名称;
                    m.项目类型 = model.项目类型;
                    m.预算金额 = model.预算金额;
                }
                else if (action == "审核不通过")
                {
                    m.审核数据.审核状态 = 审核状态.审核未通过;
                    m.审核数据.审核者.用户ID = currentUser.Id;
                }
                招标采购项目管理.更新招标采购项目(m);
            }
            catch
            {

            }
            return Redirect("/单位用户后台/Project_AuditList");
        }

        public ActionResult Project_HistoryList()
        {
            return View();
        }
        public ActionResult Part_Project_HistoryList()
        {
            long pgCount = 0;
            int cpg = 0;
            if (!string.IsNullOrWhiteSpace(Request.QueryString["page"]))
            {
                cpg = int.Parse(Request.QueryString["page"]);
            }
            if (cpg <= 0)
            {
                cpg = 1;
            }
            long pc = 招标采购项目管理.计数招标采购项目(0, 0);
            pgCount = pc / 10;
            if (pc % 10 > 0)
            {
                pgCount++;
            }
            ViewData["Pagecount"] = pgCount;
            ViewData["CurrentPage"] = cpg;
            ViewData["我的申请列表"] = 招标采购项目管理.查询招标采购项目(10 * (cpg - 1), 10);

            return PartialView("Procure_Part/Part_Project_HistoryList");
        }
        public ActionResult Project_ViewProgress()
        {
            return View();
        }
        public ActionResult Part_Project_ViewProgress(long? id)
        {
            招标采购项目 model = null;
            try
            {
                model = 招标采购项目管理.查找招标采购项目((long)id);
            }
            catch
            {
                model = null;
            }
            return PartialView("Procure_Part/Part_Project_ViewProgress", model);
        }

        public ActionResult Gys_PreScoreList()
        {
            return View();
        }
        public ActionResult Part_Gys_PreScoreList()
        {
            int page = 0;
            if (string.IsNullOrEmpty(Request.QueryString["page"]))
            {
                page = 1;
            }
            else
            {
                page = int.Parse(Request.QueryString["page"]);
            }
            int PageCount = (int)(项目服务记录管理.计数项目服务记录(0, 0, MongoDB.Driver.Builders.Query.NE("服务评价.服务评级", 项目服务记录.服务评级.未填写).And(MongoDB.Driver.Builders.Query.EQ("验收单位链接.用户ID", currentUser.Id)))) / 10;
            if ((int)(项目服务记录管理.计数项目服务记录(0, 0, MongoDB.Driver.Builders.Query.NE("服务评价.服务评级", 项目服务记录.服务评级.未填写).And(MongoDB.Driver.Builders.Query.EQ("验收单位链接.用户ID", currentUser.Id)))) % 10 > 0)
            {
                PageCount++;
            }
            ViewData["CurrentPage"] = page;
            ViewData["Pagecount"] = PageCount;
            ViewData["待评分项目服务列表"] = 项目服务记录管理.查询项目服务记录(10 * (page - 1), 10, MongoDB.Driver.Builders.Query.EQ("服务评价.服务评级", 项目服务记录.服务评级.未填写).And(MongoDB.Driver.Builders.Query.EQ("验收单位链接.用户ID", currentUser.Id)));
            return PartialView("Procure_Part/Part_Gys_PreScoreList");
        }

        public ActionResult Gys_SetScore()
        {
            return View();
        }
        public ActionResult Part_Gys_SetScore(long? id)
        {
            项目服务记录 model = null;
            try
            {
                model = 项目服务记录管理.查找项目服务记录((long)id);
            }
            catch
            {
                model = null;
            }
            return PartialView("Procure_Part/Part_Gys_SetScore", model);
        }

        [HttpPost]
        public ActionResult Gys_SetScore(项目服务记录 model)
        {
            项目服务记录 l = 项目服务记录管理.查找项目服务记录(model.Id);
            l.服务评价.服务评价内容 = Request.Form["content"];
            l.服务评价.服务评级 = model.服务评价.服务评级;
            l.服务评价.评价时间 = DateTime.Now;

            项目服务记录管理.更新项目服务记录(l);

            return View("Gys_AfScoreList");
        }

        public ActionResult Gys_AfScoreList()
        {
            return View();
        }
        public ActionResult Part_Gys_AfScoreList()
        {
            int page = 0;
            if (string.IsNullOrEmpty(Request.QueryString["page"]))
            {
                page = 1;
            }
            else
            {
                page = int.Parse(Request.QueryString["page"]);
            }
            int PageCount = (int)(项目服务记录管理.计数项目服务记录(0, 0, MongoDB.Driver.Builders.Query.NE("服务评价.服务评级", 项目服务记录.服务评级.未填写).And(MongoDB.Driver.Builders.Query.EQ("验收单位链接.用户ID", currentUser.Id)))) / 10;
            if ((int)(项目服务记录管理.计数项目服务记录(0, 0, MongoDB.Driver.Builders.Query.NE("服务评价.服务评级", 项目服务记录.服务评级.未填写).And(MongoDB.Driver.Builders.Query.EQ("验收单位链接.用户ID", currentUser.Id)))) % 10 > 0)
            {
                PageCount++;
            }
            ViewData["CurrentPage"] = page;
            ViewData["Pagecount"] = PageCount;
            ViewData["已评分项目服务列表"] = 项目服务记录管理.查询项目服务记录(10 * (page - 1), 10, MongoDB.Driver.Builders.Query.NE("服务评价.服务评级", 项目服务记录.服务评级.未填写).And(MongoDB.Driver.Builders.Query.EQ("验收单位链接.用户ID", currentUser.Id)));
            return PartialView("Procure_Part/Part_Gys_AfScoreList");
        }

        public ActionResult Gys_ScoreDetail()
        {
            return View();
        }
        public ActionResult Part_Gys_ScoreDetail(long? id)
        {
            项目服务记录 model = null;
            try
            {
                model = 项目服务记录管理.查找项目服务记录((long)id);
            }
            catch
            {
                model = null;
            }
            return PartialView("Procure_Part/Part_Gys_ScoreDetail", model);
        }

        public ActionResult Project_ZbList()
        {
            return View();
        }
        public ActionResult Part_Project_ZbList()
        {
            long pgCount = 0;
            int cpg = 0;
            if (!string.IsNullOrWhiteSpace(Request.QueryString["page"]))
            {
                cpg = int.Parse(Request.QueryString["page"]);
            }
            if (cpg <= 0)
            {
                cpg = 1;
            }
            long pc = 招标采购项目管理.计数招标采购项目(0, 0);
            pgCount = pc / 10;
            if (pc % 10 > 0)
            {
                pgCount++;
            }
            ViewData["Pagecount"] = pgCount;
            ViewData["CurrentPage"] = cpg;
            ViewData["招标项目列表"] = 招标采购项目管理.查询招标采购项目(10 * (cpg - 1), 10);

            return PartialView("Procure_Part/Part_Project_ZbList");
        }

        public ActionResult Project_ZbModify()
        {
            return View();
        }
        public ActionResult Part_Project_ZbModify(int? id)
        {
            招标采购项目 m = null;
            try
            {
                m = 招标采购项目管理.查找招标采购项目((long)id);
                if (m.审核数据.审核状态 != 审核状态.审核通过)//未通过审核的不能进行修改项目名称、编号等
                {
                    return Content("<script>alert('修改时发生权限错误，请重试');location.href='javascript:history.go(-1)';</script>");
                }
            }
            catch
            {
                m = null;
            }
            return PartialView("Procure_Part/Part_Project_ZbModify", m);
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Project_ZbModify(招标采购项目 model)
        {
            try
            {
                招标采购项目 m = 招标采购项目管理.查找招标采购项目(model.Id);//修改需求后，将项目的状态置未未审核，管理员重新进行审核
                m.项目编号 = model.项目编号;
                m.项目名称 = model.项目名称;
                m.项目类型 = model.项目类型;
                m.预算金额 = model.预算金额;

                招标采购项目管理.更新招标采购项目(m);
                return View("Project_ZbList");
            }
            catch
            {
                return Content("<script>alert('修改时发生错误，请重试');location.href='javascript:history.go(-1)';</script>");
            }

        }

        public void ExportBidgys()
        {
            string sid = Request.QueryString["id"];
            HttpResponseBase rs = Response;
            ExcelHelper.OutBidGysExcel(sid, rs);
        }


        ///////////////////网上竞标模块----------------------------------------
        [单一权限验证(权限.新增网上竞价)]
        public ActionResult OnlineBidding_Add()
        {
            return View();
        }
        public ActionResult Part_OnlineBidding_Add()
        {
            ViewData["first"] = 商品分类管理.查找子分类();
            ViewData["招标采购项目列表"] = 招标采购项目管理.查询招标采购项目(0, 0, MongoDB.Driver.Builders.Query.EQ("审核数据.审核状态", 审核状态.审核通过).And(MongoDB.Driver.Builders.Query.EQ("中标公告链接.公告ID", -1)).And(MongoDB.Driver.Builders.Query.EQ("流标公告链接.公告ID", -1)).And(MongoDB.Driver.Builders.Query.EQ("废标公告链接.公告ID", -1)).And(MongoDB.Driver.Builders.Query.EQ("项目类型", 项目类型.网上竞标)));
            return PartialView("Procure_Part/Part_OnlineBidding_Add");
        }
        public class GoodClass
        {
            public string Name { get; set; }
            public string Type { get; set; }
        }
        public ActionResult SelectGood()
        {
            string skip = Request.QueryString["skip"];
            if (string.IsNullOrWhiteSpace(skip))
            {
                skip = "1";
            }
            int count = 0;
            List<GoodClass> g = new List<GoodClass>();
            string alltype = Request.QueryString["tpy"];
            string fname = Request.QueryString["fname"];
            string tn = Request.QueryString["tn"];
            string thirdname = Request.QueryString["thirdname"];
            IEnumerable<商品> goods = null;
            IEnumerable<商品分类> twoClass = null;
            IEnumerable<商品分类> thirdClass = null;
            IEnumerable<商品分类> model = 商品分类管理.查找子分类();
            if (model != null)
            {
                foreach (var item in model)
                {
                    if (item.分类名 == fname)
                    {
                        twoClass = item.子分类;
                        break;
                    }
                }
                if (alltype != "专用物资")
                {
                    if (twoClass != null)
                    {
                        foreach (var m in twoClass)
                        {
                            if (m.分类名 == tn)
                            {
                                thirdClass = m.子分类;
                                break;
                            }
                        }
                    }
                    if (thirdClass != null)
                    {
                        foreach (var n in thirdClass)
                        {
                            if (n.分类名 == thirdname)
                            {
                                goods = n.获取分类下商品列表(false);
                                break;
                            }
                        }
                    }
                }
                else
                {
                    if (twoClass != null)
                    {
                        foreach (var it in twoClass)
                        {
                            if (it.分类名 == tn)
                            {
                                goods = it.获取分类下商品列表(false);
                                break;
                            }
                        }
                    }
                }
            }
            if (goods != null && goods.Count() != 0)
            {
                count = goods.Count() / 10;
                if ((goods.Count() % 10) > 0)
                {
                    count++;
                }
                foreach (var item in goods.Skip(10 * (int.Parse(skip) - 1)).Take(10))
                {
                    GoodClass gs = new GoodClass();
                    gs.Name = item.商品信息.商品名;
                    gs.Type = item.商品信息.精确型号;
                    g.Add(gs);
                }
            }
            JsonResult json = new JsonResult() { Data = new { content = g, count = count } };
            return Json(json, JsonRequestBehavior.AllowGet);
        }
        public ActionResult ThirdClass()
        {
            List<string> Product_Name = new List<string>();
            string alltype = Request.QueryString["tpy"];
            string twoCls = Request.QueryString["twoclass"];
            string name = Request.QueryString["fclass"];
            IEnumerable<商品分类> twoClass = null;
            IEnumerable<商品分类> thirdClass = null;
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
                            if (item.分类名 == twoCls)
                            {
                                thirdClass = item.子分类;
                                break;
                            }
                        }
                    }
                    if (thirdClass != null)
                    {
                        foreach (var m in thirdClass)
                        {
                            Product_Name.Add(m.分类名);
                        }
                    }
                }

            }
            JsonResult json = new JsonResult() { Data = Product_Name };
            return Json(json, JsonRequestBehavior.AllowGet);
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
            string alltype = Request.QueryString["tpy"];
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
        [HttpPost]
        [ValidateInput(false)]

        public ActionResult OnlineBidding_Add(网上竞标 model)
        {
            try
            {
                model.商品图片 = Request.Form["attachtext"].ToString().Split('|')[0];

                if (!string.IsNullOrEmpty(Request.Form["startPrice"]))
                {
                    model.起始价格 = Convert.ToDecimal(Request.Form["startPrice"]);
                }
                网上竞标管理.添加网上竞标(model);
            }
            catch
            {
                return Content("<script>alert('添加网上竞标出现问题！');location.href='javascript:history.go(-1)';</script>");
            }
            return RedirectToAction("OnlineBidding_List");
        }
        [单一权限验证(权限.未完成的网上竞价)]
        public ActionResult OnlineBidding_List()
        {
            return View();
        }
        public ActionResult Part_OnlineBidding_List()
        {
            long pgCount = 0;
            int cpg = 0;
            if (!string.IsNullOrWhiteSpace(Request.QueryString["page"]))
            {
                cpg = int.Parse(Request.QueryString["page"]);
            }
            if (cpg <= 0)
            {
                cpg = 1;
            }
            long pc = (int)(网上竞标管理.计数网上竞标(0, 0, Query<网上竞标>.Where(o => o.报价结束时间 > DateTime.Now)));
            pgCount = pc / 10;
            if (pc % 10 > 0)
            {
                pgCount++;
            }
            ViewData["Pagecount"] = pgCount;
            ViewData["CurrentPage"] = cpg;
            ViewData["网上竞标列表"] = 网上竞标管理.查询网上竞标(10 * (cpg - 1), 10, Query<网上竞标>.Where(o => o.报价结束时间 > DateTime.Now));

            return PartialView("Procure_Part/Part_OnlineBidding_List");
        }

        [单一权限验证(权限.已完成的网上竞价)]
        public ActionResult OnlineBidding_List_Ed()
        {
            return View();
        }
        public ActionResult Part_OnlineBidding_List_Ed(int? page)
        {
            long pgCount = 0;
            int cpg = 0;
            if (!string.IsNullOrWhiteSpace(Request.QueryString["page"]))
            {
                cpg = int.Parse(Request.QueryString["page"]);
            }
            if (cpg <= 0)
            {
                cpg = 1;
            }
            long pc = (int)(网上竞标管理.计数网上竞标(0, 0, Query<网上竞标>.Where(o => o.报价结束时间 < DateTime.Now)));
            pgCount = pc / 10;
            if (pc % 10 > 0)
            {
                pgCount++;
            }
            ViewData["Pagecount"] = pgCount;
            ViewData["CurrentPage"] = cpg;
            ViewData["网上竞标列表"] = 网上竞标管理.查询网上竞标(10 * (cpg - 1), 10, Query<网上竞标>.Where(o => o.报价结束时间 < DateTime.Now));

            return PartialView("Procure_Part/Part_OnlineBidding_List_Ed");
        }

        public void ConfirmBid()
        {
            var jbid = Request.Params["jbid"];//网上竞标ID
            var gysid = Request.Params["gysid"];//中标供应商ID
            var price = Request.Params["price"];//中标供应商报价
            var jb = 网上竞标管理.查找网上竞标(long.Parse(jbid));
            jb.中标供应商链接.用户ID = long.Parse(gysid);
            jb.最终价格 = decimal.Parse(price);
            网上竞标管理.更新网上竞标(jb);
        }

        public ActionResult OnlineBidding_Modify()
        {
            return View();
        }
        public ActionResult Part_OnlineBidding_Modify(int? id)
        {
            ViewData["招标采购项目列表"] = 招标采购项目管理.查询招标采购项目(0, 0, MongoDB.Driver.Builders.Query.EQ("审核数据.审核状态", 审核状态.审核通过).And(MongoDB.Driver.Builders.Query.EQ("中标公告链接.公告ID", -1)).And(MongoDB.Driver.Builders.Query.EQ("流标公告链接.公告ID", -1)).And(MongoDB.Driver.Builders.Query.EQ("废标公告链接.公告ID", -1)).And(MongoDB.Driver.Builders.Query.EQ("项目类型", 项目类型.网上竞标)));
            网上竞标 g = null;
            try
            {
                if (id != null)
                {
                    g = 网上竞标管理.查找网上竞标((long)id);
                }
                if (g != null)
                {
                    if (g.最终价格 > 0)//已审核的招标项目不能再进行修改
                    {
                        return Content("<script>alert('修改时发生权限错误，请重试');location.href='javascript:history.go(-1)';</script>");
                    }
                }
                else
                {
                    return Content("<script>window.location='/单位用户后台/OnlineBidding_List';</script>");
                }
            }
            catch
            {
                g = null;
            }
            return PartialView("Procure_Part/Part_OnlineBidding_Modify", g);
        }

        [HttpPost]
        [ValidateInput(false)]

        public ActionResult OnlineBidding_Modify(网上竞标 m)
        {
            try
            {
                网上竞标 model = 网上竞标管理.查找网上竞标(m.Id);
                if (model.商品图片 != null)
                {
                    if (!string.IsNullOrEmpty(Request.Form["deleteimg"]))
                    {
                        UploadPic.DelPic(string.Format("{0}", Request.Form["deleteimg"]));
                        model.商品图片 = null;
                        if (!string.IsNullOrEmpty(Request.Form["attachtext"]))
                        {
                            model.商品图片 = Request.Form["attachtext"].Split(new[] { '|' }, StringSplitOptions.None)[0];

                        }
                    }
                }
                else
                {
                    if (!string.IsNullOrEmpty(Request.Form["attachtext"]))
                    {
                        model.商品图片 = Request.Form["attachtext"].Split(new[] { '|' }, StringSplitOptions.None)[0];

                    }
                }
                model.商品名称 = m.商品名称;
                model.商品型号 = m.商品型号;
                model.商品描述 = m.商品描述;
                model.商品需求数量 = m.商品需求数量;
                model.起始价格 = m.起始价格;
                model.报价结束时间 = m.报价结束时间;
                网上竞标管理.更新网上竞标(model);
            }
            catch
            {
                return Content("<script>alert('修改出现问题！');location.href='javascript:history.go(-1)';</script>");
            }
            return View("OnlineBidding_List");
        }

        public ActionResult OnlineBidding_Delete(long? id)
        {
            try
            {
                if (id != null)
                {
                    var str = 网上竞标管理.查找网上竞标((long)id).商品图片;
                    if (string.IsNullOrEmpty(str))
                    {
                        UploadPic.DelPic(string.Format("{0}", str));
                    }
                    网上竞标管理.删除网上竞标((long)id);
                    var url = Request.Params["link"];
                    if (url == "edlist")
                    {
                        return View("OnlineBidding_List_Ed");
                    }
                    else
                    {
                        return View("OnlineBidding_List");
                    }
                }
                else
                {
                    return Content("<script>window.location='/单位用户后台/OnlineBidding_List';</script>");
                }
            }
            catch
            {
                return View("OnlineBidding_List");
            }

        }

        public ActionResult OnlineBidding_Detail()
        {
            return View();
        }
        public ActionResult Part_OnlineBidding_Detail(long? id)
        {
            网上竞标 g = null;
            try
            {
                if (id != null)
                {
                    g = 网上竞标管理.查找网上竞标((long)id);
                }
                if (g == null)
                {
                    return Content("<script>window.location='/单位用户后台/OnlineBidding_List';</script>");
                }
            }
            catch
            {
                g = null;
            }
            ViewData["finish"] = Request.Params["finish"];
            return PartialView("Procure_Part/Part_OnlineBidding_Detail", g);
        }

        //推荐管理模块////////////////////////////////////////////////////

        public ActionResult Recommend_Gys()
        {
            return View();
        }
        public ActionResult Part_Recommend_Gys()
        {
            return PartialView("Procure_Part/Part_Recommend_Gys");
        }

        [HttpPost]
        public ActionResult Recommend_Gys(int? id)
        {
            try
            {
                int count = int.Parse(Request.Params["count"]);
                for (int i = 0; i < count; i++)
                {
                    推荐信息 temp = new 推荐信息
                    {
                        推荐类型 = 推荐类型.供应商,
                        名称 = Request.Form["name__" + i],
                        推荐理由 = Request.Form["reason__" + i],
                        推荐人 = new 用户链接<单位用户>
                        {
                            用户ID = currentUser.Id
                        },
                        联系方式 = new _联系方式
                        {
                            联系人 = Request.Form["Cname__" + i],
                            手机 = Request.Form["tel__" + i],
                            固定电话 = Request.Form["phonearea__" + i] + "-" + Request.Form["phonenum__" + i]
                        }
                    };

                    //成都002或者，采购处及采购处助理员推荐，直接审核通过
                    if (currentUser.管理单位.用户ID == 10)
                    {
                        temp.推荐审核数据.推荐状态 = 推荐状态.推荐通过;
                        temp.推荐审核数据.审核时间 = DateTime.Now;
                        temp.推荐审核数据.审核者.用户ID = currentUser.Id;
                    }
                    else
                    {
                        //站内消息
                        站内消息 Msg = new 站内消息();
                        对话消息 Talk = new 对话消息();

                        Msg.重要程度 = 重要程度.特别重要;
                        Msg.消息类型 = 消息类型.推荐专家和供应商;

                        Msg.发起者.用户ID = currentUser.Id;
                        Talk.发言人.用户ID = currentUser.Id;
                        var sendid = 11;

                        Talk.消息主体.标题 = "待审核推荐供应商";
                        Talk.消息主体.内容 = "有一条待审核的推荐供应商未处理，<a style='color:red;text-decoration:underline;' href='/单位用户后台/Recommend_GysList_Audit'>点击这里进行审核</a>";

                        temp.推荐审核数据.推荐状态 = 推荐状态.待联系;
                        temp.推荐审核数据.审核者.用户ID = 11;

                        站内消息管理.添加站内消息(Msg, currentUser.Id, sendid);
                        对话消息管理.添加对话消息(Talk, Msg);
                    }

                    推荐管理.添加推荐信息(temp);
                }

            }
            catch
            {

            }
            return View("Recommend_GysList");
        }

        public ActionResult Delete_Recommend_Gys(long id)
        {
            try
            {
                推荐管理.删除推荐信息(id);
            }
            catch
            {

            }
            return View("Recommend_GysList");
        }
        public ActionResult Recommend_GysList()
        {
            return View();
        }
        public ActionResult Part_Recommend_GysList()
        {
            long pgCount = 0;
            int cpg = 0;
            if (!string.IsNullOrWhiteSpace(Request.QueryString["page"]))
            {
                cpg = int.Parse(Request.QueryString["page"]);
            }
            if (cpg <= 0)
            {
                cpg = 1;
            }
            long pc = 推荐管理.计数推荐信息(0, 0, Query<推荐信息>.Where(o => o.推荐类型 == 推荐类型.供应商 && o.推荐人.用户ID == currentUser.Id));
            pgCount = pc / 10;
            if (pc % 10 > 0)
            {
                pgCount++;
            }
            ViewData["Pagecount"] = pgCount;
            ViewData["CurrentPage"] = cpg;
            ViewData["我推荐的供应商"] = 推荐管理.查询推荐信息(10 * (cpg - 1), 10, Query<推荐信息>.Where(o => o.推荐类型 == 推荐类型.供应商 && o.推荐人.用户ID == currentUser.Id));
            return PartialView("Procure_Part/Part_Recommend_GysList");
        }

        public ActionResult Recommend_Expert()
        {
            return View();
        }
        public ActionResult Part_Recommend_Expert()
        {
            return PartialView("Procure_Part/Part_Recommend_Expert");
        }

        [HttpPost]
        public ActionResult Recommend_Expert(int? id)
        {
            try
            {
                int count = int.Parse(Request.Params["count"]);
                for (int i = 0; i < count; i++)
                {
                    推荐信息 temp = new 推荐信息
                    {
                        推荐类型 = 推荐类型.专家,
                        名称 = Request.Form["name__" + i],
                        推荐理由 = Request.Form["reason__" + i],
                        推荐人 = new 用户链接<单位用户>
                        {
                            用户ID = currentUser.Id
                        },
                        联系方式 = new _联系方式
                        {
                            联系人 = Request.Form["Cname__" + i],
                            手机 = Request.Form["tel__" + i],
                            固定电话 = Request.Form["phonearea__" + i] + "-" + Request.Form["phonenum__" + i]
                        }
                    };

                    //采购处处长推荐，直接审核通过
                    if (currentUser.管理单位.用户ID == 10)
                    {
                        temp.推荐审核数据.推荐状态 = 推荐状态.推荐通过;
                        temp.推荐审核数据.审核时间 = DateTime.Now;
                        temp.推荐审核数据.审核者.用户ID = currentUser.Id;
                    }
                    else
                    {
                        //站内消息
                        站内消息 Msg = new 站内消息();
                        对话消息 Talk = new 对话消息();

                        Msg.重要程度 = 重要程度.特别重要;
                        Msg.消息类型 = 消息类型.推荐专家和供应商;

                        Msg.发起者.用户ID = currentUser.Id;
                        Talk.发言人.用户ID = currentUser.Id;
                        var sendid = 11;

                        Talk.消息主体.标题 = "待审核推荐评审专家";
                        Talk.消息主体.内容 = "有一条待审核的推荐评审专家未处理，<a style='color:red;text-decoration:underline;' href='/单位用户后台/Recommend_ExpertList_Audit'>点击这里进行审核</a>";

                        temp.推荐审核数据.推荐状态 = 推荐状态.待联系;
                        temp.推荐审核数据.审核者.用户ID = 11;

                        站内消息管理.添加站内消息(Msg, currentUser.Id, sendid);
                        对话消息管理.添加对话消息(Talk, Msg);
                    }

                    推荐管理.添加推荐信息(temp);
                }

            }
            catch
            {

            }
            return View("Recommend_ExpertList");
        }

        public ActionResult Delete_Recommend_Expert(long id)
        {
            try
            {
                推荐管理.删除推荐信息(id);
            }
            catch
            {

            }
            return View("Recommend_ExpertList");
        }

        public ActionResult Print_Supplier(int? page)
        {
            var pagesize = 20;
            int listcount = (int)(推荐管理.计数推荐信息(0, 0, Query<推荐信息>.Where(o => o.推荐类型 == 推荐类型.供应商 && o.推荐人.用户ID == currentUser.Id)));
            int maxpage = Math.Max((listcount + pagesize - 1) / pagesize, 1);

            if (string.IsNullOrEmpty(page.ToString()) || page < 0 || page > maxpage)
            {
                page = 1;
            }
            ViewData["page"] = page;
            ViewData["listcount"] = listcount;
            ViewData["maxpage"] = maxpage;
            ViewData["pagesize"] = pagesize;

            ViewData["我推荐的供应商"] = 推荐管理.查询推荐信息(pagesize * (int.Parse(page.ToString()) - 1), pagesize, Query<推荐信息>.Where(o => o.推荐类型 == 推荐类型.供应商 && o.推荐审核数据.推荐状态 == 推荐状态.推荐通过));
            return View();
        }
        public ActionResult Print_Expert(int? page)
        {
            var pagesize = 20;
            int listcount = (int)(推荐管理.计数推荐信息(0, 0, Query<推荐信息>.Where(o => o.推荐类型 == 推荐类型.专家 && o.推荐人.用户ID == currentUser.Id)));
            int maxpage = Math.Max((listcount + pagesize - 1) / pagesize, 1);

            if (string.IsNullOrEmpty(page.ToString()) || page < 0 || page > maxpage)
            {
                page = 1;
            }
            ViewData["page"] = page;
            ViewData["listcount"] = listcount;
            ViewData["maxpage"] = maxpage;
            ViewData["pagesize"] = pagesize;
            ViewData["我推荐的专家"] = 推荐管理.查询推荐信息(pagesize * (int.Parse(page.ToString()) - 1), pagesize, Query<推荐信息>.Where(o => o.推荐类型 == 推荐类型.专家 && o.推荐审核数据.推荐状态 == 推荐状态.推荐通过));
            return View();
        }
        public ActionResult Recommend_ExpertList()
        {
            return View();
        }
        public ActionResult Part_Recommend_ExpertList()
        {
            long pgCount = 0;
            int cpg = 0;
            if (!string.IsNullOrWhiteSpace(Request.QueryString["page"]))
            {
                cpg = int.Parse(Request.QueryString["page"]);
            }
            if (cpg <= 0)
            {
                cpg = 1;
            }
            long pc = 推荐管理.计数推荐信息(0, 0, Query<推荐信息>.Where(o => o.推荐类型 == 推荐类型.专家 && o.推荐人.用户ID == currentUser.Id));
            pgCount = pc / 10;
            if (pc % 10 > 0)
            {
                pgCount++;
            }
            ViewData["Pagecount"] = pgCount;
            ViewData["CurrentPage"] = cpg;
            ViewData["我推荐的专家"] = 推荐管理.查询推荐信息(10 * (cpg - 1), 10, Query<推荐信息>.Where(o => o.推荐类型 == 推荐类型.专家 && o.推荐人.用户ID == currentUser.Id));
            return PartialView("Procure_Part/Part_Recommend_ExpertList");
        }
        public ActionResult Recommend_Detail()
        {
            return View();
        }
        public ActionResult Part_Recommend_Detail(long? id)
        {
            推荐信息 m = new 推荐信息();
            try
            {
                m = 推荐管理.查找推荐信息((long)id);
                if (m.推荐类型 == 推荐类型.供应商)
                {
                    if (Request.QueryString["come"] == "g")
                    {
                        ViewData["r_t"] = "推荐供应商";
                    }
                    else
                    {
                        ViewData["r_t"] = "审核推荐的供应商";
                    }
                }
                else
                {
                    if (Request.QueryString["come"] == "g")
                    {
                        ViewData["r_t"] = "推荐评审专家";
                    }
                    else
                    {
                        ViewData["r_t"] = "审核推荐的评审专家";
                    }

                }
            }
            catch
            {
                m = null;
            }
            return PartialView("Procure_Part/Part_Recommend_Detail", m);
        }

        [HttpPost]
        public ActionResult Recommend_Detail(long? id, string action)
        {
            var c = Request.Form["messagecomfrom"];
            try
            {
                var temp = 推荐管理.查找推荐信息((long)id);

                if (action == "审核通过")
                {
                    //TD:判断用户类型
                    //采购处处长审核通过，直接审核通过
                    if (currentUser.管理单位.用户ID == 10)
                    {
                        temp.推荐审核数据.推荐状态 = 推荐状态.推荐通过;
                        temp.推荐审核数据.审核时间 = DateTime.Now;
                        temp.推荐审核数据.审核者.用户ID = currentUser.Id;
                    }
                    推荐管理.更新推荐信息(temp, false);
                }
                if (action == "审核不通过")
                {
                    //TD:判断用户类型
                    //采购处处长审核不通过，直接审核通过
                    if (currentUser.管理单位.用户ID == 10)
                    {
                        temp.推荐审核数据.推荐状态 = 推荐状态.推荐未通过;
                        temp.推荐审核数据.审核时间 = DateTime.Now;
                        temp.推荐审核数据.审核者.用户ID = currentUser.Id;
                    }

                    推荐管理.更新推荐信息(temp, false);
                }
            }
            catch
            {

            }
            if (null != c && c == "g")
            {
                return View("Recommend_GysList_Audit");
            }
            else
            {
                return View("Recommend_ExpertList_Audit");
            }
        }
        public ActionResult Recommend_ExpertList_Audit()
        {
            return View();
        }
        public ActionResult Part_Recommend_ExpertList_Audit()
        {
            long pgCount = 0;
            int cpg = 0;
            if (!string.IsNullOrWhiteSpace(Request.QueryString["page"]))
            {
                cpg = int.Parse(Request.QueryString["page"]);
            }
            if (cpg <= 0)
            {
                cpg = 1;
            }
            long pc = 推荐管理.计数推荐信息(0, 0, Query<推荐信息>.Where(o => o.推荐类型 == 推荐类型.专家 && (o.推荐审核数据.审核者.用户ID == currentUser.Id || o.推荐审核数据2.审核者.用户ID == currentUser.Id || o.推荐审核数据3.审核者.用户ID == currentUser.Id)));
            pgCount = pc / 10;
            if (pc % 10 > 0)
            {
                pgCount++;
            }
            ViewData["Pagecount"] = pgCount;
            ViewData["CurrentPage"] = cpg;
            ViewData["用户ID"] = currentUser.Id;
            ViewData["审核推荐的专家"] = 推荐管理.查询推荐信息(10 * (cpg - 1), 10, Query<推荐信息>.Where(o => o.推荐类型 == 推荐类型.专家 && (o.推荐审核数据.审核者.用户ID == currentUser.Id || o.推荐审核数据2.审核者.用户ID == currentUser.Id || o.推荐审核数据3.审核者.用户ID == currentUser.Id)));
            return PartialView("Procure_Part/Part_Recommend_ExpertList_Audit");
        }
        public ActionResult Recommend_GysList_Audit()
        {
            return View();
        }
        public ActionResult Part_Recommend_GysList_Audit()
        {
            long pgCount = 0;
            int cpg = 0;
            if (!string.IsNullOrWhiteSpace(Request.QueryString["page"]))
            {
                cpg = int.Parse(Request.QueryString["page"]);
            }
            if (cpg <= 0)
            {
                cpg = 1;
            }
            long pc = 推荐管理.计数推荐信息(0, 0, Query<推荐信息>.Where(o => o.推荐类型 == 推荐类型.供应商 && (o.推荐审核数据.审核者.用户ID == currentUser.Id || o.推荐审核数据2.审核者.用户ID == currentUser.Id || o.推荐审核数据3.审核者.用户ID == currentUser.Id)));
            pgCount = pc / 10;
            if (pc % 10 > 0)
            {
                pgCount++;
            }
            ViewData["Pagecount"] = pgCount;
            ViewData["CurrentPage"] = cpg;
            ViewData["用户ID"] = currentUser.Id;
            ViewData["审核推荐的供应商"] = 推荐管理.查询推荐信息(10 * (cpg - 1), 10, Query<推荐信息>.Where(o => o.推荐类型 == 推荐类型.供应商 && (o.推荐审核数据.审核者.用户ID == currentUser.Id || o.推荐审核数据2.审核者.用户ID == currentUser.Id || o.推荐审核数据3.审核者.用户ID == currentUser.Id)));
            return PartialView("Procure_Part/Part_Recommend_GysList_Audit");

        }
        //推荐管理模块////////////////////////////////////////////////////
        //#endif
        //验收单模块///////
        public ActionResult AcceptanceDetail()
        {
            try
            {
                string id = Request.Params["id"];
                ViewData["comes"] = Request.Params["comes"];
                验收单 ysd = 验收单管理.查找验收单(long.Parse(id));
                if (ysd == null)
                {
                    return Redirect("/单位用户后台/AcceptanceList");
                }
                else
                {
                    return View(ysd);
                }
            }
            catch
            {
                return Redirect("/单位用户后台/AcceptanceList");
            }
        }

        public string ExaminePic()
        {
            var id = Request.Params["id"];//验收单ID
            var type = Request.Params["type"];//审核通过或审核不通过
            var uri = Request.Params["uri"];//回传单路径
            var rp = Request.Params["rp"];//未通过理由
            var ysd = 验收单管理.查找验收单(long.Parse(id));
            var listpic = ysd.验收单扫描件.Find(o => o.回传单路径 == uri);
            if (type == "审核通过")
            {
                listpic.审核数据.审核状态 = 审核状态.审核通过;
                listpic.审核数据.审核时间 = DateTime.Now;
                listpic.审核数据.审核者.用户ID = currentUser.Id;
                验收单管理.更新验收单(ysd);
                return "1";
            }
            if (type == "审核不通过")
            {
                listpic.审核数据.审核状态 = 审核状态.审核未通过;
                listpic.审核数据.审核时间 = DateTime.Now;
                listpic.审核数据.审核者.用户ID = currentUser.Id;
                listpic.审核数据.审核不通过原因 = rp;
                验收单管理.更新验收单(ysd);
                return "0";
            }
            return "0";
        }


        [单一权限验证(权限.已审核的验收单)]
        public ActionResult Acceptance_checked()
        {
            //var q = Query.EQ("审核数据.审核状态", 审核状态.审核通过).And(Query.EQ("是否已经打印", false));
            var q = Query<验收单>.Where(o =>
                o.审核数据.审核状态 != 审核状态.未审核
                && o.管理单位审核人.用户ID == currentUser.Id
                && !o.是否作废);
            long listcount = 验收单管理.计数验收单(0, 0, q);
            long maxpagesize = Math.Max((listcount + ACCEPTANCE_PAGESIZE - 1) / ACCEPTANCE_PAGESIZE, 1);

            int page = 1;
            ViewData["currentpage"] = page;
            ViewData["pagecount"] = maxpagesize;

            ViewData["验收单列表"] = 验收单管理.查询验收单(ACCEPTANCE_PAGESIZE * (page - 1), ACCEPTANCE_PAGESIZE, q, false, SortBy.Descending("基本数据.添加时间"));
            return View();
        }
        public ActionResult AcceptancedDetail()
        {
            try
            {
                string id = Request.Params["id"];
                ViewData["comes"] = Request.Params["comes"];
                验收单 ysd = 验收单管理.查找验收单(long.Parse(id));
                if (ysd == null)
                {
                    return Redirect("/单位用户后台/AcceptanceList");
                }
                else
                {
                    return View(ysd);
                }
            }
            catch
            {
                return Redirect("/单位用户后台/AcceptanceList");
            }
        }

        public ActionResult CancelExamine()
        {
            var id = Request.Params["id"];
            var ysd = 验收单管理.查找验收单(long.Parse(id));
            ysd.审核数据.审核状态 = 审核状态.未审核;
            ysd.审核数据.审核者.用户ID = -1;
            ysd.是否已经打印 = false;
            ysd.验收单扫描件.Clear();
            验收单管理.更新验收单(ysd);
            return RedirectToAction("Acceptance_checked");
        }

        [单一权限验证(权限.未审核的验收单)]
        public ActionResult AcceptanceList()
        {
            var q = Query<验收单>.Where(
                o => o.管理单位审核人.用户ID == currentUser.Id
                    && o.审核数据.审核状态 == 审核状态.未审核
                    && !o.是否作废);

            long listcount = 验收单管理.计数验收单(0, 0, q);
            long maxpagesize = Math.Max((listcount + ACCEPTANCE_PAGESIZE - 1) / ACCEPTANCE_PAGESIZE, 1);

            int page = 1;
            ViewData["currentpage"] = page;
            ViewData["pagecount"] = maxpagesize;

            ViewData["验收单列表"] = 验收单管理.查询验收单(ACCEPTANCE_PAGESIZE * (page - 1), ACCEPTANCE_PAGESIZE, q, false, SortBy.Descending("基本数据.添加时间"));
            return View();
        }

        [单一权限验证(权限.已作废的验收单)]
        public ActionResult AcceptanceList_Avoid()
        {

            var q = Query<验收单>.Where(o => o.管理单位审核人.用户ID == currentUser.Id && o.是否作废);

            long listcount = 验收单管理.计数验收单(0, 0, q);
            long maxpagesize = Math.Max((listcount + ACCEPTANCE_PAGESIZE - 1) / ACCEPTANCE_PAGESIZE, 1);

            int page = 1;
            ViewData["currentpage"] = page;
            ViewData["pagecount"] = maxpagesize;

            ViewData["验收单列表"] = 验收单管理.查询验收单(ACCEPTANCE_PAGESIZE * (page - 1), ACCEPTANCE_PAGESIZE, q, false, SortBy.Descending("基本数据.添加时间"));
            return View();
        }

        [单一权限验证(权限.我的验收单列表)]
        public ActionResult Acceptanced_List()
        {
            //var q = Query.EQ("审核数据.审核状态", 审核状态.审核通过).And(Query.EQ("是否已经打印", false));
            var q = Query<验收单>.Where(o => o.管理单位审核人.用户ID == currentUser.Id);
            long listcount = 验收单管理.计数验收单(0, 0, q);
            long maxpagesize = Math.Max((listcount + ACCEPTANCE_PAGESIZE - 1) / ACCEPTANCE_PAGESIZE, 1);

            int page = 1;
            ViewData["currentpage"] = page;
            ViewData["pagecount"] = maxpagesize;

            ViewData["验收单列表"] = 验收单管理.查询验收单(ACCEPTANCE_PAGESIZE * (page - 1), ACCEPTANCE_PAGESIZE, q, false, SortBy.Descending("基本数据.添加时间"));
            return View();
        }
        public ActionResult Part_AcceptanceGood()
        {
            var q = Query<验收单>.Where(o => o.管理单位审核人.用户ID == currentUser.Id
                && o.审核数据.审核状态 != 审核状态.审核通过
                && !o.是否作废);

            int page = 1;
            var isexamine = Request.Params["isexamine"];

            if (!string.IsNullOrWhiteSpace(Request.Params["page"]))
            {
                int.TryParse(Request.Params["page"], out page);
            }

            if (isexamine == "审核未通过")
            {
                q = q.And(Query<验收单>.Where(o => o.审核数据.审核状态 == 审核状态.审核未通过));
            }
            if (isexamine == "已审核")
            {
                q = q.And(Query<验收单>.Where(o => o.审核数据.审核状态 == 审核状态.审核通过));
            }
            if (isexamine == "未审核")
            {
                q = q.And(Query<验收单>.Where(o => o.审核数据.审核状态 == 审核状态.未审核));
            }

            long listcount = 验收单管理.计数验收单(0, 0, q);
            long maxpage = Math.Max((listcount + ACCEPTANCE_PAGESIZE - 1) / ACCEPTANCE_PAGESIZE, 1);

            ViewData["currentPage"] = page;
            ViewData["pagecount"] = maxpage;

            ViewData["验收单列表"] = 验收单管理.查询验收单(ACCEPTANCE_PAGESIZE * (page - 1), ACCEPTANCE_PAGESIZE, q, false, SortBy.Descending("基本数据.添加时间"));
            return PartialView("Procure_Part/Part_AcceptanceGood");
        }

        public ActionResult Part_AcceptanceVoid()
        {
            var q = Query<验收单>.Where(o => o.管理单位审核人.用户ID == currentUser.Id
                && o.是否作废);

            int page = 1;
            var isexamine = Request.Params["isexamine"];

            if (!string.IsNullOrWhiteSpace(Request.Params["page"]))
            {
                int.TryParse(Request.Params["page"], out page);
            }

            if (isexamine == "审核未通过")
            {
                q = q.And(Query<验收单>.Where(o => o.审核数据.审核状态 == 审核状态.审核未通过));
            }
            if (isexamine == "已审核")
            {
                q = q.And(Query<验收单>.Where(o => o.审核数据.审核状态 == 审核状态.审核通过));
            }
            if (isexamine == "未审核")
            {
                q = q.And(Query<验收单>.Where(o => o.审核数据.审核状态 == 审核状态.未审核));
            }

            long listcount = 验收单管理.计数验收单(0, 0, q);
            long maxpage = Math.Max((listcount + ACCEPTANCE_PAGESIZE - 1) / ACCEPTANCE_PAGESIZE, 1);

            ViewData["currentPage"] = page;
            ViewData["pagecount"] = maxpage;

            ViewData["验收单列表"] = 验收单管理.查询验收单(ACCEPTANCE_PAGESIZE * (page - 1), ACCEPTANCE_PAGESIZE, q, false, SortBy.Descending("基本数据.添加时间"));
            return PartialView("Procure_Part/Part_AcceptanceVoid");
        }

        public ActionResult Part_AcceptanceChecked()
        {
            string status = Request.Params["status"];
            IMongoQuery q = Query<验收单>.Where(o => !o.是否作废)
                    .And(Query<验收单>.Where(o => o.管理单位审核人.用户ID == currentUser.Id))
                    .And(Query<验收单>.Where(o => o.审核数据.审核状态 != 审核状态.未审核));

            if (status == "已完成")
            {
                q = q.And(Query<验收单>.Where(o => o.是否已经打印 == true && o.验收单扫描件.Count > 0));
            }
            if (status == "未打印")
            {
                q = q.And(Query<验收单>.Where(o => o.是否已经打印 == false));
            }
            if (status == "未验收")
            {
                q = q.And(Query<验收单>.Where(o => o.验收单扫描件.Count <= 0));
            }
            long listcount = 验收单管理.计数验收单(0, 0, q);
            long maxpagesize = Math.Max((listcount + ACCEPTANCE_PAGESIZE - 1) / ACCEPTANCE_PAGESIZE, 1);
            string page1 = Request.Params["page"];
            int page;
            if (string.IsNullOrWhiteSpace(page1))
            {
                page = 1;
            }
            else
            {
                int.TryParse(page1, out page);
            }
            ViewData["currentpage"] = page;
            ViewData["pagecount"] = maxpagesize;

            ViewData["验收单列表"] = 验收单管理.查询验收单(ACCEPTANCE_PAGESIZE * (page - 1), ACCEPTANCE_PAGESIZE, q, false, SortBy.Descending("基本数据.添加时间"));
            // var q = Query<验收单>.Where(o=>o.);
            return PartialView("Procure_Part/Part_AcceptanceChecked");
        }
        public ActionResult Part_SearchByCondit()
        {
            string status = Request.Params["status"];
            var isvoid = Request.Params["isvoid"];
            //var q=MongoDB.Driver.Builders.Query();
            IMongoQuery q = null;

            if (status == "已完成")
            {
                q = q.And(Query<验收单>.Where(o => o.是否已经打印 == true && o.验收单扫描件.Count > 0));
            }
            if (status == "未打印")
            {
                q = q.And(Query<验收单>.Where(o => o.是否已经打印 == false));
            }
            if (status == "未验收")
            {
                q = q.And(Query<验收单>.Where(o => o.验收单扫描件.Count <= 0));
            }
            if (isvoid == "已作废")
            {
                q = q.And(Query<验收单>.Where(o => o.是否作废 == true));
            }
            if (isvoid == "未作废")
            {
                q = q.And(Query<验收单>.Where(o => o.是否作废 == false));
            }
            long listcount = 验收单管理.计数验收单(0, 0, q.And(Query<验收单>.Where(m => m.管理单位审核人.用户ID == currentUser.Id)));
            long maxpagesize = Math.Max((listcount + ACCEPTANCE_PAGESIZE - 1) / ACCEPTANCE_PAGESIZE, 1);
            string page1 = Request.Params["page"];
            int page;
            if (string.IsNullOrWhiteSpace(page1))
            {
                page = 1;
            }
            else
            {
                int.TryParse(page1, out page);
            }
            ViewData["currentpage"] = page;
            ViewData["pagecount"] = maxpagesize;

            ViewData["验收单列表"] = 验收单管理.查询验收单(ACCEPTANCE_PAGESIZE * (page - 1), ACCEPTANCE_PAGESIZE, q, false, SortBy.Descending("基本数据.添加时间"));
            // var q = Query<验收单>.Where(o=>o.);
            return PartialView("Procure_Part/Part_SearchByCondit");
        }
        public void ExamineStatus()
        {
            string id = Request.Params["id"];
            string type = Request.Params["sh_type"];
            string reason = Request.Params["reason"];
            var ysd = 验收单管理.查找验收单(long.Parse(id));

            var ysduserlist = 验收单单位列表信息.验收单单位列表;
            var ysduser = ysduserlist.Where(o => o.Id == currentUser.Id);

            if (type == "通过审核")
            {
                ysd.验收单核对码 = RandomString.Generate(6, 3);
                ysd.审核数据.审核状态 = 审核状态.审核通过;
                ysd.审核数据.审核者.用户ID = currentUser.Id;
                ysd.审核数据.审核时间 = DateTime.Now;
                ysd.审核数据.审核不通过原因 = "";
                ysd.管理单位审核人签名 = ysduser.Any() ? ysduser.First().签名图片链接 : "";

            }
            if (type == "不通过审核")
            {
                ysd.审核数据.审核状态 = 审核状态.审核未通过;
                ysd.审核数据.审核者.用户ID = currentUser.Id;
                ysd.审核数据.审核时间 = DateTime.Now;
                ysd.审核数据.审核不通过原因 = reason;
                ysd.管理单位审核人签名 = ysduser.Any() ? ysduser.First().签名图片链接 : "";
            }
            验收单管理.更新验收单(ysd);
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
                    if (long.Parse(id) == 10005 || long.Parse(id) == 10009 || long.Parse(id) == 10008 || long.Parse(id) == 10013 || long.Parse(id) == 20151 || long.Parse(id) == 20150 || long.Parse(id) == 20145 || long.Parse(id) == 20146 || long.Parse(id) == 20137 || long.Parse(id) == 20138 || long.Parse(id) == 20139 || long.Parse(id) == 20140 || long.Parse(id) == 20141 || long.Parse(id) == 20142 || long.Parse(id) == 20143 || long.Parse(id) == 20144 || long.Parse(id) == 20152 || long.Parse(id) == 20149 || long.Parse(id) == 20147 || long.Parse(id) == 20148 || long.Parse(id) == 20261 || long.Parse(id) == 20254)
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
        //验收单模块///////


        //----------------------------------------------------------------------------
        public ActionResult ComplainList()
        {
            return View();
        }
        public ActionResult Suggestiont_Reply_Page()
        {
            return View();
        }
        public ActionResult Part_Suggestion_ReplyPg()//投诉回复页
        {
            try
            {
                ViewData["当前用户"] = currentUser.登录信息.登录名;
                var model = 投诉管理.查找投诉(long.Parse(Request.QueryString["id"]));
                if (model == null)
                {
                    return Content("<script>window.location='/单位用户后台/ComplainList';</script>");
                }
                return PartialView("Procure_Part/Part_Suggestion_ReplyPg", model);
            }
            catch
            {
                return Content("<script>window.location='/单位用户后台/ComplainList';</script>");
            }

        }
        public ActionResult Suggestion_Del()//删除投诉
        {
            try
            {
                long id = long.Parse(Request.QueryString["id"]);
                if (投诉管理.删除投诉(id))
                {
                    return Redirect("/单位用户后台/ComplainList");
                }
                else
                {
                    return Redirect("/单位用户后台/ComplainList");
                }
            }
            catch
            {
                return Content("<script>window.location='/单位用户后台/ComplainList';</script>");
            }

        }
        [ValidateInput(false)]
        public int Suggestions_Reply()//回复投诉
        {
            try
            {
                long id = long.Parse(Request.QueryString["Id"]);
                string title = Request.QueryString["t"];
                string contents = Request.QueryString["c"];
                var item = new 对话消息();
                item.发言人.用户ID = currentUser.Id;
                item.消息主体.标题 = title;
                item.消息主体.内容 = contents;
                投诉 model = 投诉管理.查找投诉(id);
                model.处理状态 = 处理状态.处理中;
                投诉管理.更新投诉(model);
                对话消息管理.添加对话消息(item, model);
                return 1;
            }
            catch
            {
                return 0;
            }
        }
        public ActionResult Part_ComplainList()
        {
            int page = 0;
            if (string.IsNullOrEmpty(Request.QueryString["page"]))
            {
                page = 1;
            }
            else
            {
                page = int.Parse(Request.QueryString["page"]);
            }
            ViewData["CurrentPage"] = page;
            int PageCount = (int)投诉管理.计数投诉(0, 0, Query.EQ("受理单位.用户ID", currentUser.Id)) / 20;
            if ((投诉管理.计数投诉(0, 0, Query.EQ("处理状态", 处理状态.未处理)) % 20) > 0)
            {
                PageCount++;
            }
            ViewData["Pagecount"] = PageCount;
            ViewData["投诉列表"] = 投诉管理.查询投诉(20 * (page - 1), 20, Query.EQ("受理单位.用户ID", currentUser.Id)).OrderByDescending(m => m.基本数据.修改时间);
            return PartialView("Procure_Part/Part_ComplainList");
        }
        public ActionResult TrainingAdd()
        {
            培训资料 model = new 培训资料();
            return View(model);
        }
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult AddTrain(培训资料 model)
        {
            //string content = Request.Form["content"].ToString();
            //model.内容主体.内容=content;
            try
            {
                List<string> pathname = new List<string>();
                string[] path = Request.Form["path"].ToString().Split('|');
                for (int i = 0; i < path.Length - 1; i++)
                {
                    if (System.IO.File.Exists(Server.MapPath(@path[i])))
                    {
                        pathname.Add(path[i]);
                    }
                }
                if (model.内容主体.发布时间 == default(DateTime))
                {
                    model.内容主体.发布时间 = DateTime.Now;
                }
                model.内容主体.附件 = pathname;
                培训资料管理.添加培训资料(model);
                return Content("<script>if(confirm('成功添加培训资料，点击确定继续添加，点击取消进入查看')){window.location='/单位用户后台/TrainingAdd';}else{window.location='/单位用户后台/TrainingList';}</script>");
            }
            catch
            {
                return Redirect("/单位用户后台/TrainingAdd");
            }
        }
        public ActionResult GuideAdd()
        {
            办事指南 guid = new 办事指南();
            return View(guid);
        }
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult AddGuide(办事指南 model)
        {
            try
            {
                List<string> pathname = new List<string>();
                string[] path = Request.Form["path"].ToString().Split('|');
                for (int i = 0; i < path.Length - 1; i++)
                {
                    if (System.IO.File.Exists(Server.MapPath(@path[i])))
                    {
                        pathname.Add(path[i]);
                    }
                }
                if (model.内容主体.发布时间 == default(DateTime))
                {
                    model.内容主体.发布时间 = DateTime.Now;
                }
                model.内容主体.附件 = pathname;
                办事指南管理.添加办事指南(model);
                return Content("<script>if(confirm('成功添加办事指南，点击确定继续添加，点击取消进入查看')){window.location='/单位用户后台/GuideAdd'}else{window.location='/单位用户后台/GuideList'};</script>");
            }
            catch
            {
                return Redirect("/单位用户后台/GuideAdd");
            }
        }
        public int DelPic()
        {
            try
            {
                long id = long.Parse(Request.QueryString["id"]);
                string name = Request.QueryString["name"];
                string path = Request.QueryString["path"];
                if (name == "px")
                {
                    培训资料 model = 培训资料管理.查找培训资料(id);
                    if (System.IO.File.Exists(Server.MapPath(@path)))
                    {
                        System.IO.File.Delete(Server.MapPath(@path));
                        if (model.内容主体.附件 != null && model.内容主体.附件.Count() != 0)
                        {
                            foreach (var item in model.内容主体.附件)
                            {
                                if (item == path)
                                {
                                    model.内容主体.附件.Remove(item);
                                }
                                break;
                            }
                        }
                        培训资料管理.更新培训资料(model);
                        return 1;
                    }
                    else
                    {
                        return 0;
                    }
                }
                else
                {
                    办事指南 model = 办事指南管理.查找办事指南(id);
                    if (System.IO.File.Exists(Server.MapPath(@path)))
                    {
                        System.IO.File.Delete(Server.MapPath(@path));
                        if (model.内容主体.附件 != null && model.内容主体.附件.Count() != 0)
                        {
                            foreach (var item in model.内容主体.附件)
                            {
                                if (item == path)
                                {
                                    model.内容主体.附件.Remove(item);
                                }
                                break;
                            }
                        }
                        办事指南管理.更新办事指南(model);
                        return 1;
                    }
                    else
                    {
                        return 0;
                    }
                }
            }
            catch
            {
                return 0;
            }
        }
        public ActionResult TrainingList()
        {
            try
            {
                long pgCount = 0;
                int cpg = 0;
                if (!string.IsNullOrWhiteSpace(Request.QueryString["page"]))
                {
                    cpg = int.Parse(Request.QueryString["page"]);
                }
                if (cpg <= 0)
                {
                    cpg = 1;
                }
                long pc = 培训资料管理.计数培训资料(0, 0);
                pgCount = pc / 10;
                if (pc % 10 > 0)
                {
                    pgCount++;
                }
                ViewData["Pagecount"] = pgCount;
                ViewData["CurrentPage"] = cpg;
                IEnumerable<培训资料> model = 培训资料管理.查询培训资料(10 * (cpg - 1), 10);
                return View(model);
            }
            catch
            {
                return Redirect("/单位用户后台/TrainingList");
            }
        }
        public ActionResult GuideList()
        {
            try
            {
                long pgCount = 0;
                int cpg = 0;
                if (!string.IsNullOrWhiteSpace(Request.QueryString["page"]))
                {
                    cpg = int.Parse(Request.QueryString["page"]);
                }
                if (cpg <= 0)
                {
                    cpg = 1;
                }
                long pc = 办事指南管理.计数办事指南(0, 0);
                pgCount = pc / 10;
                if (pc % 10 > 0)
                {
                    pgCount++;
                }
                ViewData["Pagecount"] = pgCount;
                ViewData["CurrentPage"] = cpg;
                IEnumerable<办事指南> model = 办事指南管理.查询办事指南(10 * (cpg - 1), 10);
                return View(model);
            }
            catch
            {
                return Redirect("/单位用户后台/GuideList");
            }
        }
        public ActionResult TrainDetail()
        {
            try
            {
                long id = long.Parse(Request.QueryString["id"]);
                培训资料 model = 培训资料管理.查找培训资料(id);
                if (model == null)
                {
                    return Redirect("/单位用户后台/TrainingList");
                }
                else
                {
                    return View(model);
                }
            }
            catch
            {
                return Redirect("/单位用户后台/TrainingList");
            }
        }
        public ActionResult TrainingModify()
        {
            try
            {
                long id = long.Parse(Request.QueryString["id"]);
                培训资料 model = 培训资料管理.查找培训资料(id);
                if (model == null)
                {
                    return Redirect("/单位用户后台/TrainingList");
                }
                return View(model);
            }
            catch
            {
                return Redirect("/单位用户后台/TrainingList");
            }
        }
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult ModifyTrain(培训资料 model)
        {
            try
            {
                培训资料 model1 = 培训资料管理.查找培训资料(model.Id);
                List<string> pathname = new List<string>();
                if (model1.内容主体.附件 != null && model1.内容主体.附件.Count() != 0)
                {
                    pathname = model1.内容主体.附件;
                }
                string[] path = Request.Form["path"].ToString().Split('|');
                for (int i = 0; i < path.Length - 1; i++)
                {
                    if (System.IO.File.Exists(Server.MapPath(@path[i])))
                    {
                        pathname.Add(path[i]);
                    }
                }
                if (model.内容主体.发布时间 == default(DateTime))
                {
                    model.内容主体.发布时间 = DateTime.Now;
                }
                model.内容主体.附件 = pathname;
                培训资料管理.更新培训资料(model);
                return Content("<script>alert('修改成功！');window.location='/单位用户后台/TrainingList';</script>");
            }
            catch
            {
                return Redirect("/单位用户后台/TrainingList");
            }
        }
        public ActionResult DelTrain()
        {
            try
            {
                long id = long.Parse(Request.QueryString["id"]);
                培训资料 model = 培训资料管理.查找培训资料(id);
                if (model.内容主体.附件 != null && model.内容主体.附件.Count() != 0)
                {
                    foreach (var item in model.内容主体.附件)
                    {
                        if (System.IO.File.Exists(Server.MapPath(@item)))
                        {
                            System.IO.File.Delete(Server.MapPath(@item));
                        }
                    }
                }
                培训资料管理.删除培训资料(id);
                return Redirect("/单位用户后台/TrainingList");
            }
            catch
            {
                return Redirect("/单位用户后台/TrainingList");
            }
        }
        public ActionResult guideDetail()
        {
            try
            {
                long id = long.Parse(Request.QueryString["id"]);
                办事指南 model = 办事指南管理.查找办事指南(id);
                if (model == null)
                {
                    return Redirect("/单位用户后台/GuideList");
                }
                else
                {
                    return View(model);
                }
            }
            catch
            {
                return Redirect("/单位用户后台/GuideList");
            }
        }
        public ActionResult GuideModify()
        {
            try
            {
                long id = long.Parse(Request.QueryString["id"]);
                办事指南 model = 办事指南管理.查找办事指南(id);
                if (model == null)
                {
                    return Redirect("/单位用户后台/TrainingList");
                }
                return View(model);
            }
            catch
            {
                return Redirect("/单位用户后台/TrainingList");
            }
        }
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult ModifyGuide(办事指南 model)
        {
            try
            {
                办事指南 model1 = 办事指南管理.查找办事指南(model.Id);
                List<string> pathname = new List<string>();
                if (model1.内容主体.附件 != null && model1.内容主体.附件.Count() != 0)
                {
                    pathname = model1.内容主体.附件;
                }
                string[] path = Request.Form["path"].ToString().Split('|');
                for (int i = 0; i < path.Length - 1; i++)
                {
                    if (System.IO.File.Exists(Server.MapPath(@path[i])))
                    {
                        pathname.Add(path[i]);
                    }
                }
                if (model.内容主体.发布时间 == default(DateTime))
                {
                    model.内容主体.发布时间 = DateTime.Now;
                }
                model.内容主体.附件 = pathname;
                办事指南管理.更新办事指南(model);
                return Content("<script>alert('修改成功！');window.location='/单位用户后台/GuideList';</script>");
            }
            catch
            {
                return Redirect("/单位用户后台/GuideList");
            }
        }
        public ActionResult DelGuide()
        {
            try
            {
                long id = long.Parse(Request.QueryString["id"]);
                办事指南 model = 办事指南管理.查找办事指南(id);
                if (model.内容主体.附件 != null && model.内容主体.附件.Count() != 0)
                {
                    foreach (var item in model.内容主体.附件)
                    {
                        if (System.IO.File.Exists(Server.MapPath(@item)))
                        {
                            System.IO.File.Delete(Server.MapPath(@item));
                        }
                    }
                }
                办事指南管理.删除办事指南(id);
                return Redirect("/单位用户后台/GuideList");
            }
            catch
            {
                return Redirect("/单位用户后台/GuideList");
            }
        }
        [HttpPost]
        public ActionResult UploadImage()
        {
            try
            {
                string path1 = "";
                string name = Request.Form["pic1"].ToString();
                for (int i = 0; i < Request.Files.Count; i++)
                {
                    HttpPostedFileBase file = Request.Files[i];
                    if (file != null && ((file.ContentType == "image/jpeg" || file.ContentType == "image/bmp" || file.ContentType == "image/gif") && (((file.ContentLength / 1024) / 1024) < 100)))
                    {
                        string filePath = "";
                        string fname = "";
                        if (name == "sm")
                        {
                            filePath = 上传管理.获取上传路径<公告>(媒体类型.图片, 路径类型.不带域名根路径);
                            fname = UploadAttachment_Ad_s(file);
                        }
                        else if (name == "newsPic")
                        {
                            filePath = 上传管理.获取上传路径<新闻>(媒体类型.图片, 路径类型.不带域名根路径);
                            fname = UploadAttachment(file);
                        }
                        path1 += filePath + fname + "|";
                    }
                }
                ViewData["path"] = path1;
                return View();
            }
            catch
            {
                ViewData["path"] = "上传出错!|";
                return View();
            }
        }
        [HttpPost]
        public ActionResult UploadImages()
        {
            try
            {
                string path1 = "";
                string name = Request.Form["guide"].ToString();
                for (int i = 0; i < Request.Files.Count; i++)
                {
                    HttpPostedFileBase file = Request.Files[i];
                    if (file != null && (((file.ContentLength / 1024) / 1024) < 100))
                    {
                        string filePath = "";
                        string fname = "";
                        if (name == "guide")
                        {
                            filePath = 上传管理.获取上传路径<办事指南>(媒体类型.附件, 路径类型.不带域名根路径);
                            fname = UploadAttachment1(file, name);
                        }
                        else if (name == "train")
                        {
                            filePath = 上传管理.获取上传路径<培训资料>(媒体类型.附件, 路径类型.不带域名根路径);
                            fname = UploadAttachment1(file, name);
                        }
                        else if (name == "attach")
                        {
                            filePath = 上传管理.获取上传路径<公告>(媒体类型.附件, 路径类型.不带域名根路径);
                            fname = UploadAttachment_Ad(file);
                        }
                        else if (name == "tz")
                        {
                            filePath = 上传管理.获取上传路径<通知>(媒体类型.附件, 路径类型.不带域名根路径);
                            fname = UploadAttachment_Tz(file);
                        }
                        else if (name == "zcfg")
                        {
                            filePath = 上传管理.获取上传路径<政策法规>(媒体类型.附件, 路径类型.不带域名根路径);
                            fname = UploadAttachment_Zcfg(file);
                        }
                        path1 += filePath + fname + "|";
                    }
                }
                ViewData["path"] = path1;
                return View();
            }
            catch
            {
                ViewData["path"] = "上传出错!|";
                return View();
            }
        }
        //\---------------------------------------------------------------------------
        public string UploadAttachment1(HttpPostedFileBase fileData, string str)
        {
            //string extension = string.Empty;
            if (fileData != null)
            {
                string fileName = fileData.FileName;
                if (fileName != "" && fileName != null)
                {
                    string filePath = "";
                    if (str == "guide")
                    {
                        filePath = 上传管理.获取上传路径<办事指南>(媒体类型.附件, 路径类型.服务器本地路径);
                    }
                    else
                    {
                        filePath = 上传管理.获取上传路径<培训资料>(媒体类型.附件, 路径类型.服务器本地路径);
                    }
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
        public int DeleteImages()
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

        public ActionResult ProductClass()
        {
            string requesrid = Request.Params["classid"].ToString();
            try
            {
                string retstr = "";

                var lowerclass = 商品分类管理.查找子分类(long.Parse(requesrid));

                if (lowerclass.Any())
                {
                    foreach (var item in lowerclass)
                    {
                        retstr += "<option class=\"yjfn\" value=\"" + item.分类名 + "\" id=\"" + item.Id + "\">" + item.分类名 + "</option>";
                    }
                }
                return Content(retstr);

            }
            catch
            {
                return Content("加载失败");
            }
        }


        /// <summary>
        /// 上传新闻图片
        /// </summary>
        /// <param name="fileData"></param>
        /// <returns></returns>
        [HttpPost]
        public string UploadAttachment(HttpPostedFileBase fileData)
        {
            //string extension = string.Empty;

            if (fileData != null)
            {
                string fileName = fileData.FileName;
                if (fileName != "" && fileName != null)
                {
                    string filePath = "";
                    filePath = 上传管理.获取上传路径<新闻>(媒体类型.图片, 路径类型.服务器本地路径);
                    //extension = fileData.FileName.Substring(fileData.FileName.LastIndexOf("."), (fileData.FileName.Length - fileData.FileName.LastIndexOf(".")));

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

        /// <summary>
        /// 上传公告附件
        /// </summary>
        /// <param name="fileData"></param>
        /// <returns></returns>
        [HttpPost]
        public string UploadAttachment_Ad(HttpPostedFileBase fileData)//上传公告附件
        {
            //string extension = string.Empty;
            if (fileData != null)
            {
                string fileName = fileData.FileName;
                if (fileName != "" && fileName != null)
                {
                    string filePath = "";
                    filePath = 上传管理.获取上传路径<公告>(媒体类型.附件, 路径类型.服务器本地路径);
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

        /// <summary>
        /// 上传公告扫描件
        /// </summary>
        /// <param name="fileData"></param>
        /// <returns></returns>
        [HttpPost]
        public string UploadAttachment_Ad_s(HttpPostedFileBase fileData)//上传公告扫描件
        {
            //string extension = string.Empty;
            string fileName = string.Empty;
            if (fileData != null)
            {
                fileName = fileData.FileName;
                if (fileName != "" && fileName != null)
                {
                    string filePath = "";
                    filePath = 上传管理.获取上传路径<公告>(媒体类型.图片, 路径类型.服务器本地路径);
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

        /// <summary>
        /// 上传公告附件
        /// </summary>
        /// <param name="fileData"></param>
        /// <returns></returns>
        [HttpPost]
        public string UploadAttachment_Tz(HttpPostedFileBase fileData)
        {
            //string extension = string.Empty;

            if (fileData != null)
            {
                string fileName = fileData.FileName;
                if (fileName != "" && fileName != null)
                {
                    string filePath = "";
                    filePath = 上传管理.获取上传路径<通知>(媒体类型.附件, 路径类型.服务器本地路径);
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
        /// <summary>
        /// 上传政策法规附件
        /// </summary>
        /// <param name="fileData"></param>
        /// <returns></returns>
        [HttpPost]
        public string UploadAttachment_Zcfg(HttpPostedFileBase fileData)
        {
            //string extension = string.Empty;

            if (fileData != null)
            {
                string fileName = fileData.FileName;
                if (fileName != "" && fileName != null)
                {
                    string filePath = "";
                    filePath = 上传管理.获取上传路径<政策法规>(媒体类型.附件, 路径类型.服务器本地路径);
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

        /// <summary>
        /// 传递公告的附件
        /// </summary>
        /// <param name="fileData"></param>
        /// <returns></returns>DeleteAttachment
        [HttpPost]
        public ActionResult UploadAttachment_AdSend(HttpPostedFileBase fileData)
        {
            if (fileData != null)
            {
                string fileName = fileData.FileName;
                string filePath = 上传管理.获取上传路径<站内消息>(媒体类型.附件, 路径类型.服务器本地路径);

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
                return Content(string.Format("{0}{1}", 上传管理.获取上传路径<站内消息>(媒体类型.附件, 路径类型.不带域名根路径), fileName));
            }
            else
            {
                return Content("0");
            }
        }
        /// <summary>
        /// 网上竞标图片上传
        /// </summary>
        /// <param name="fileData"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult UploadAttachment_OB(HttpPostedFileBase fileData)
        {
            if (fileData != null)
            {
                string fileName = fileData.FileName;
                string filePath = 上传管理.获取上传路径<网上竞标>(媒体类型.图片, 路径类型.服务器本地路径);

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
                return Content(string.Format("{0}{1}", 上传管理.获取上传路径<网上竞标>(媒体类型.图片, 路径类型.不带域名根路径), fileName));
            }
            else
            {
                return Content("0");
            }
        }

        [HttpPost]
        /// <summary>
        /// 删除附件
        /// </summary>
        /// <returns></returns>
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

        /// <summary>
        /// 修改公告/通知是删除旧附件
        /// </summary>
        /// <returns></returns>
        public ActionResult DeleteOldAttachment()
        {
            string qid = Request.Params["q"] ?? "";
            try
            {
                if (!string.IsNullOrEmpty(qid))
                {
                    UploadPic.DelPic(string.Format("{0}", qid.Trim()));
                    return Content(qid);
                }
                else
                {
                    return Content("0");
                }
            }
            catch { return Content("0"); }
        }

        /// <summary>
        /// 创建公告索引
        /// </summary>
        private void GG_CreateIndex(公告 g, string indexdic)
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
                IndexWriter writer = new IndexWriter(dir, PanGuAnalyzer, false, IndexWriter.MaxFieldLength.UNLIMITED);
                //IndexWriter writer = new IndexWriter(Indexdic_Server, PanGuAnalyzer, false, Lucene.Net.Index.IndexWriter.MaxFieldLength.LIMITED);
                GG_AddIndex(writer, g);

                writer.Optimize();
                writer.Close();
            }
            catch
            {
                IndexWriter writer = new IndexWriter(dir, PanGuAnalyzer, true, IndexWriter.MaxFieldLength.UNLIMITED);
                //IndexWriter writer = new IndexWriter(Indexdic_Server, PanGuAnalyzer, true, Lucene.Net.Index.IndexWriter.MaxFieldLength.LIMITED);
                GG_AddIndex(writer, g);

                writer.Optimize();
                writer.Close();
            }
        }

        /// <summary>
        /// 创建公告索引
        /// </summary>
        /// <param name="analyzer"></param>
        /// <param name="title"></param>
        /// <param name="content"></param>
        private void GG_AddIndex(IndexWriter writer, 公告 model)
        {
            try
            {
                Document doc = new Document();
                doc.Add(new Field("NumId", model.Id.ToString(), Field.Store.YES, Field.Index.NOT_ANALYZED));//存储且索引
                Field f = new Field("Title", model.内容主体.标题, Field.Store.YES, Field.Index.ANALYZED);
                f.SetBoost(5F);//为标题增加权重
                doc.Add(f);//存储且索引
                doc.Add(new Field("Content", model.内容主体.内容, Field.Store.YES, Field.Index.ANALYZED));//存储且索引
                doc.Add(new Field("AdClass", model.公告信息.公告类别.ToString(), Field.Store.YES, Field.Index.ANALYZED));//存储且索引
                doc.Add(new Field("AdFeature", model.公告信息.公告性质.ToString(), Field.Store.YES, Field.Index.NOT_ANALYZED));//存储且索引
                doc.Add(new Field("AdPro", model.公告信息.所属地域.省份, Field.Store.YES, Field.Index.ANALYZED));//存储且索引
                doc.Add(new Field("AdCity", model.公告信息.所属地域.城市, Field.Store.YES, Field.Index.ANALYZED));//存储且索引
                doc.Add(new Field("AdArea", model.公告信息.所属地域.区县, Field.Store.YES, Field.Index.ANALYZED));//存储且索引
                doc.Add(new Field("AdHy", model.公告信息.一级分类, Field.Store.YES, Field.Index.ANALYZED));//存储且索引
                doc.Add(new Field("AddTime", model.内容主体.发布时间.ToString(), Field.Store.YES, Field.Index.NOT_ANALYZED));//存储且索引
                writer.AddDocument(doc);
            }
            catch
            {
                return;
            }
        }

        /// <summary>
        /// 创建索引
        /// </summary>
        private void CreateIndex(内容基本数据 model, string indexdic)
        {
            string Indexdic_Server = IndexDic(indexdic);

            PanGu.Segment.Init(PanGuXmlPath);
            //var g = model as Go81WebApp.Models.数据模型.内容数据模型.公告;
            //创建索引目录
            if (!Directory.Exists(Indexdic_Server))
            {
                Directory.CreateDirectory(Indexdic_Server);
            }

            Lucene.Net.Store.Directory dir = new Lucene.Net.Store.SimpleFSDirectory(new DirectoryInfo(Indexdic_Server));
            //IndexWriter第三个参数:true指重新创建索引,false指从当前索引追加....此处为追加索引所以为false
            try
            {
                IndexWriter writer = new IndexWriter(dir, PanGuAnalyzer, false, IndexWriter.MaxFieldLength.UNLIMITED);
                //IndexWriter writer = new IndexWriter(Indexdic_Server, PanGuAnalyzer, false, Lucene.Net.Index.IndexWriter.MaxFieldLength.LIMITED);
                AddIndex(writer, model.Id.ToString(), model.内容主体.标题, model.内容主体.内容, model.内容主体.发布时间.ToString());

                writer.Optimize();
                writer.Close();
            }
            catch
            {
                IndexWriter writer = new IndexWriter(dir, PanGuAnalyzer, true, IndexWriter.MaxFieldLength.UNLIMITED);
                //IndexWriter writer = new IndexWriter(Indexdic_Server, PanGuAnalyzer, true, Lucene.Net.Index.IndexWriter.MaxFieldLength.LIMITED);
                AddIndex(writer, model.Id.ToString(), model.内容主体.标题, model.内容主体.内容, model.内容主体.发布时间.ToString());

                writer.Optimize();
                writer.Close();
            }
        }

        /// <summary>
        /// 创建索引
        /// </summary>
        /// <param name="analyzer"></param>
        /// <param name="title"></param>
        /// <param name="content"></param>
        private void AddIndex(IndexWriter writer, string id, string title, string content, string date)
        {
            try
            {
                Document doc = new Document();
                doc.Add(new Field("NumId", id, Field.Store.YES, Field.Index.NOT_ANALYZED));//存储且索引
                //doc.Add(new Field("Title", title, Field.Store.YES, Field.Index.ANALYZED));//存储且索引
                Field f = new Field("Title", title, Field.Store.YES, Field.Index.ANALYZED);
                f.SetBoost(5F);//为标题增加权重
                doc.Add(f);
                doc.Add(new Field("Content", content, Field.Store.YES, Field.Index.ANALYZED));//存储且索引
                doc.Add(new Field("AddTime", date, Field.Store.YES, Field.Index.NOT_ANALYZED));//存储且索引
                writer.AddDocument(doc);
            }
            catch
            {
                return;
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

        protected void updateIndex(string path, 内容基本数据 model)
        {
            string Indexdic_Server = IndexDic(path);
            IndexWriter writer = null;
            Lucene.Net.Store.Directory dir = new Lucene.Net.Store.SimpleFSDirectory(new DirectoryInfo(Indexdic_Server));
            try
            {
                //writer = new IndexWriter(Indexdic_Server, PanGuAnalyzer, false, Lucene.Net.Index.IndexWriter.MaxFieldLength.LIMITED);
                writer = new IndexWriter(dir, PanGuAnalyzer, false, IndexWriter.MaxFieldLength.LIMITED);
            }
            catch
            {
                //writer = new IndexWriter(Indexdic_Server, PanGuAnalyzer, true, Lucene.Net.Index.IndexWriter.MaxFieldLength.LIMITED);
                writer = new IndexWriter(dir, PanGuAnalyzer, true, IndexWriter.MaxFieldLength.LIMITED);
            }

            writer.DeleteDocuments(new Term("NumId", model.Id.ToString()));//删除一条索引
            writer.Optimize();
            writer.Close();

            CreateIndex(model, path);

        }
        protected void deleteIndex(string path, string id)
        {
            string Indexdic_Server = IndexDic(path);
            IndexWriter writer = null;
            Lucene.Net.Store.Directory dir = new Lucene.Net.Store.SimpleFSDirectory(new DirectoryInfo(Indexdic_Server));
            try
            {
                //writer = new IndexWriter(Indexdic_Server, PanGuAnalyzer, false, Lucene.Net.Index.IndexWriter.MaxFieldLength.LIMITED);
                writer = new IndexWriter(dir, PanGuAnalyzer, false, IndexWriter.MaxFieldLength.LIMITED);
            }
            catch
            {
                //writer = new IndexWriter(Indexdic_Server, PanGuAnalyzer, true, Lucene.Net.Index.IndexWriter.MaxFieldLength.LIMITED);
                writer = new IndexWriter(dir, PanGuAnalyzer, true, IndexWriter.MaxFieldLength.LIMITED);
            }
            writer.DeleteDocuments(new Term("NumId", id));//删除一条索引
            writer.Optimize();
            writer.Close();
        }



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

            //strUpper = strUpper.Replace("零拾", "零");
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

        public ActionResult getDownload()
        {
            try
            {
                var time = DateTime.Now.ToLocalTime().ToString("yyyyMMdd-hhmmss");
                var idstr = Request.Params["id"].Split(new[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
                var savepath = Server.MapPath("/App_Uploads/公告下载/");
                if (!Directory.Exists(savepath))
                {
                    Directory.CreateDirectory(savepath);
                }
                using (var zip = new ZipFile { AlternateEncoding = Encoding.UTF8, AlternateEncodingUsage = ZipOption.Always })
                {
                    foreach (var id in idstr)
                    {
                        var model = 公告管理.查找公告(long.Parse(id));
                        //生成html文档级文件夹
                        var htmlfile = getHtmlString(model, time);
                        if (!string.IsNullOrWhiteSpace(htmlfile))
                        {
                            zip.AddFile(htmlfile, model.内容主体.标题);
                        }
                        if (model.内容主体.附件 != null && model.内容主体.附件.Any())
                        {
                            zip.AddFiles(model.内容主体.附件.Select(Server.MapPath), model.内容主体.标题 + "\\附件");
                        }
                        if (model.内容主体.图片 != null && model.内容主体.图片.Any())
                        {
                            zip.AddFiles(model.内容主体.图片.Select(Server.MapPath), model.内容主体.标题 + "\\扫描件");
                        }
                    }

                    zip.Save(savepath + time + ".zip");
                }

                return Content("/App_Uploads/公告下载/" + time + ".zip");
            }
            catch
            {
                return Content("0");
            }
        }

        //生成html文件 放入App_Uploads/公告下载/年月日时分秒 公告名称的一个文件夹
        public string getHtmlString(公告 model, string time)
        {
            try
            {
                if (System.IO.File.Exists(Server.MapPath("/App_Uploads/download.html")))
                {
                    var sr = new StreamReader(Server.MapPath("/App_Uploads/download.html"), Encoding.UTF8);
                    string input = sr.ReadToEnd();
                    sr.Close();
                    input =
                        input.Replace("{0}", model.内容主体.标题)
                            .Replace("{1}", model.公告来源.来源名称)
                            .Replace("{2}", model.公告来源.来源链接)
                            .Replace("{3}", model.内容主体.发布时间.ToString("yyyy-MM-dd"))
                            .Replace("{4}", model.公告信息.一级分类)
                            .Replace("{5}", model.公告信息.二级分类)
                            .Replace("{6}", model.公告信息.三级分类)
                            .Replace("{7}", model.公告信息.所属地域.省份)
                            .Replace("{8}", model.公告信息.所属地域.城市)
                            .Replace("{9}", model.公告信息.所属地域.区县)
                            .Replace("{10}", model.项目信息.项目名称)
                            .Replace("{11}", model.项目信息.项目编号)
                            .Replace("{12}", model.公告信息.公告类别.ToString())
                            .Replace("{13}", model.公告信息.公告性质.ToString())
                            .Replace("{14}", model.公告信息.公告版本.ToString())
                            .Replace("{15}", model.内容主体.内容);
                    var dir = Server.MapPath("/App_Uploads/公告下载/" + time + "/");
                    var file = dir + model.内容主体.标题 + ".html";

                    if (!Directory.Exists(dir))
                    {
                        Directory.CreateDirectory(dir);
                    }

                    if (!System.IO.File.Exists(file))
                    {
                        FileStream fs = System.IO.File.Create(file);
                        fs.Close();
                    }
                    var sw = new StreamWriter(file, false, Encoding.UTF8);
                    sw.Write(input);
                    sw.Close();
                    return file;
                }
                else
                {
                    return "";
                }

            }
            catch
            {
                return "";
            }
        }
    }
}
