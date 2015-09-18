using Go81WebApp.Models.数据模型.推荐数据模型;
using Go81WebApp.Models.管理器;
using Go81WebApp.Models.管理器.推广业务管理;
using Go81WebApp.Models.管理器.推荐管理;
using Go81WebApp.Models.管理器.推送管理;
using Go81WebApp.Models.数据模型;
using Go81WebApp.Models.数据模型.内容数据模型;
using Go81WebApp.Models.数据模型.权限数据模型;
using Go81WebApp.Models.数据模型.商品数据模型;
using Go81WebApp.Models.数据模型.推送数据模型;
using Go81WebApp.Models.数据模型.消息数据模型;
using Go81WebApp.Models.数据模型.用户数据模型;
using Helpers;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.PanGu;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using MailApiPostClass;
using Microsoft.Ajax.Utilities;
using MongoDB;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.WebPages;
using Newtonsoft.Json;

namespace Go81WebApp.Controllers.后台
{
    using FileHelper;
    using Go81WebApp.Models.管理器.下载管理;
    using Go81WebApp.Models.数据模型.项目数据模型;
    using MQ = MongoDB.Driver.Builders.Query;
    using org.in2bits.MyXls;
    using Helpers.印章;
    using System.Drawing;
    using System.Drawing.Imaging;
    using Gma.QrCodeNet.Encoding;
    using Gma.QrCodeNet.Encoding.Windows.Render;
    using Go81WebApp.Models.数据模型.推广业务数据模型;
    using Go81WebApp.Models.管理器.统计管理;
    using Go81WebApp.Models.数据模型.统计数据模型;
    using System.Text;
    using Ionic.Zip;
    using Go81WebApp.Models.Helpers;
    using Ivony.Html.Parser;
    using Ivony.Html;
    using MongoDB.Bson.Serialization;
    using Go81WebApp.Models.数据模型.竞标数据模型;
    using Go81WebApp.Models.管理器.内容管理;
    [登录验证]
    [用户类型验证(typeof(运营团队))]
    public class 运营团队后台Controller : Controller
    {
        private static int NEWS_PAGESIZE = int.Parse(ConfigurationManager.AppSettings["新闻每页显示条数"].ToString());
        private static int ZCFG_PAGESIZE = int.Parse(ConfigurationManager.AppSettings["政策法规每页显示条数"].ToString());

        private static int GG_PAGESIZE = int.Parse(ConfigurationManager.AppSettings["公告每页显示条数"].ToString());
        //private static int GG_OK_MAXPAGE = Math.Max((TZ_LISTCOUNT + GG_PAGESIZE - 1) / GG_PAGESIZE, 1);

        private static int TZ_PAGESIZE = int.Parse(ConfigurationManager.AppSettings["通知每页显示条数"].ToString());
        private static int ZNXX_PAGESIZE = int.Parse(ConfigurationManager.AppSettings["站内消息每页显示条数"].ToString());

        //private static int ZNXX_LISTCOUNT = (int)(站内消息管理.计数站内消息(0, -1));
        //private static int ZNXX_MAXPAGE = Math.Max((ZNXX_LISTCOUNT + ZNXX_PAGESIZE - 1) / ZNXX_PAGESIZE, 1);

        //private static int TZ_LISTCOUNT = (int)(通知管理.计数通知(0, -1));
        //private static int TZ_MAXPAGE = Math.Max((TZ_LISTCOUNT + TZ_PAGESIZE - 1) / TZ_PAGESIZE, 1);

        private static int PRO_PAGESIZE = int.Parse(ConfigurationManager.AppSettings["后台商品列表商品每页显示条数"].ToString());
        private static int GYS_PAGESIZE = int.Parse(ConfigurationManager.AppSettings["后台供应商每页显示条数"].ToString());
        private static int MESSAGE_PAGESIZE = int.Parse(ConfigurationManager.AppSettings["后台推送消息每页显示条数"].ToString());
        private static int QUESTION_PAGESIZE = int.Parse(ConfigurationManager.AppSettings["后台投诉每页显示条数"].ToString());
        private static int DOWNLOAD_PAGESIZE = int.Parse(ConfigurationManager.AppSettings["下载每页显示条数"].ToString());
        private static int USERGROUP_PAGESIZE = int.Parse(ConfigurationManager.AppSettings["用户组每页显示个数"].ToString());
        private static int ZBCGXM_PAGESIZE = 15;
        private static int ACCEPTANCE_PAGESIZE = int.Parse(ConfigurationManager.AppSettings["验收单每页显示条数"].ToString());
        private static int JCTAPPLYPAGESIZE = 20;



        private 运营团队 currentUser
        {
            get { return this.HttpContext.获取当前用户<运营团队>(); }
        }

        ///////////////////////////////////////////////////////////////////////////////////////////////////////从单位用户转移代码
        public ActionResult Del_Department(long id)
        {
            用户管理.删除用户<单位用户>(id);
            return View("DepartmentList");
        }

        public ActionResult Part_BackHead()
        {
            var m = currentUser;
            return PartialView("Part_View/Part_BackHead", m);
        }

        public class Department
        {
            /// <summary>
            /// 单位id
            /// </summary>
            public long Id { get; set; }

            /// <summary>
            /// 登录名
            /// </summary>
            public string loginame { get; set; }
            /// <summary>
            /// 所属单位
            /// </summary>
            public string ssdw { get; set; }
            /// <summary>
            /// 单位名称
            /// </summary>
            public string unitname { get; set; }
            /// <summary>
            /// 联系人
            /// </summary>
            public string contactuser { get; set; }
            /// <summary>
            /// 审核状态
            /// </summary>
            public string status { get; set; }
        }
        public void putOutExcel()
        {
            string sid = Request.QueryString["id"];
            string type = Request.QueryString["type"];
            HttpResponseBase rs = Response;
            ExcelHelper.PutExcel(type, sid, rs);
        }
        public void putOutExcelAll()
        {
            HttpResponseBase rs = Response;
            ExcelHelper.PutExcelAll(rs);
        }
        public void putOutExcel_phone()
        {
            string sid = Request.QueryString["id"];
            HttpResponseBase rs = Response;
            ExcelHelper.PutOutExcel(sid, rs);
        }
        public ActionResult delAddress()
        {
            try
            {
                long id = long.Parse(Request.QueryString["id"]);
                int index = int.Parse(Request.QueryString["index"]);
                供应商 model = 用户管理.查找用户<供应商>(id);
                model.地址信息.RemoveAt(index);
                用户管理.更新用户<供应商>(model);
                return Content("<script>alert('删除成功！');window.location='/运营团队后台/Modify_Supplier_Info?id=" + id + "';</script>");
            }
            catch
            {
                return Redirect("/运营团队后台/Supplier_PssInfo");
            }
        }
        public ActionResult Qualify_Detail()
        {
            return View();
        }
        public ActionResult Part_Qualify_Detail()
        {
            try
            {
                long id = long.Parse(Request.QueryString["id"]);
                供应商 Newmodel = 用户管理.查找用户<供应商>(id);
                int index = int.Parse(Request.QueryString["index"]);
                ViewData["index"] = index;
                return PartialView("Part_View/Part_Qualify_Detail", Newmodel);
            }
            catch
            {
                return Content("<script>window.location='/运营团队后台/Modify_Supplier_Info';</script>");
            }

        }
        public long GoodStatisticNum()
        {
            try
            {
                DateTime start = DateTime.Parse(Request.QueryString["startdate"]);
                DateTime last = DateTime.Parse(Request.QueryString["enddate"]);
                long count = 商品管理.计数商品(0, 0, Query<商品>.Where(m => m.基本数据.添加时间 >= start && m.基本数据.添加时间 <= last));
                return count;
            }
            catch
            {
                return 0;
            }
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
            ViewData["三月数据"] = Ulist;
            ViewData["审核状况"] = list;
            ViewData["采购类型"] = glist;
            return View();
        }
        [HttpPost]
        public ActionResult Modify_qualify(供应商 model)
        {
            try
            {
                int index = int.Parse(Request.Form["index1"].ToString());
                供应商 Newmodel = 用户管理.查找用户<供应商>(model.Id);
                if (index > Newmodel.资质证书列表.Count() || index < 0)
                {
                    return Redirect("/运营团队后台/Modify_Supplier_Info");
                }
                string url = Newmodel.资质证书列表[index].资质证书电子扫描件[0].路径;
                Newmodel.资质证书列表[index] = model.资质证书列表[index];
                Newmodel.资质证书列表[index].资质证书电子扫描件[0].路径 = url;
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
                //deleteIndex_ProductCatalog("/Lucene.Net/IndexDic/GysCatalog", Newmodel.企业基本信息.企业名称);
                return Content("<script>window.location='/运营团队后台/Modify_Supplier_Info';</script>");
            }
            catch
            {
                return Redirect("/运营团队后台/Qualify_Management");
            }
        }

        [HttpPost]
        public ActionResult Add_Service_Evaluate(验收单 model)
        {
            var id = long.Parse(Request.Form["YsdNumber"].ToString());
            验收单 ysd = 验收单管理.查找验收单(id);
            var smjs = Request.Form["smj"].ToString();
            var smjarr = smjs.Split(new[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var item in smjarr)
            {
                ysd.验收单扫描件.Add(new _回传信息 { 回传单路径 = item });
            }
            验收单管理.更新验收单(ysd);
            return Content("<script>alert('添加成功！您可以继续添加');window.location='/运营团队后台/AccepatnceSmj?id=" + id + "';</script>");
        }


        public ActionResult SavePicture()
        {
            try
            {
                string path = "";
                string path1 = "";
                供应商 model = new 供应商();
                if (Request.Form["gysId"] != null)
                {
                    long id = long.Parse(Request.Form["gysId"].ToString());
                    model = 用户管理.查找用户<供应商>(id);
                }

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
                            else if (name == "lawperson")
                            {
                                if (System.IO.File.Exists(Server.MapPath(@model.法定代表人信息.法定代表人身份证电子扫描件)))
                                {
                                    System.IO.File.Delete(Server.MapPath(@model.法定代表人信息.法定代表人身份证电子扫描件));
                                }
                                model.法定代表人信息.法定代表人身份证电子扫描件 = path1;
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
                            else if (name == "photo")
                            {
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
                            string filePath = 上传管理.获取上传路径<验收单>(媒体类型.图片, 路径类型.不带域名根路径);
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
                if (name == "showPic" || name == "ysd")
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
        public string UploadImages_Ysd(HttpPostedFileBase fileData)
        {
            if (fileData != null)
            {
                string fileName = fileData.FileName;
                if (fileName != "" && fileName != null)
                {
                    string filePath = 上传管理.获取上传路径<验收单>(媒体类型.图片, 路径类型.服务器本地路径);

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
        public ActionResult Modify_Supplier_Info()
        {
            try
            {
                ViewData["行业列表"] = 商品分类管理.查找子分类();
                long id = long.Parse(Request.QueryString["id"]);
                供应商 model = 用户管理.查找用户<供应商>(id);
                ViewData["goods"] = 商品管理.查询供应商商品(id).Count();
                if (model == null)
                {
                    return Redirect("/运营团队后台/Supplier_PssInfo");
                }
                return View(model);
            }
            catch
            {
                return Redirect("/运营团队后台/Supplier_PssInfo");
            }
        }
        public ActionResult DeletePic()
        {
            try
            {
                long id = long.Parse(Request.QueryString["id"]);
                string path = Request.QueryString["src"];
                if (System.IO.File.Exists(Server.MapPath(@path)))
                {
                    System.IO.File.Delete(Server.MapPath(@path));
                }
                return Redirect("/运营团队后台/Modify_Supplier_Info?id=" + id.ToString());
            }
            catch
            {
                return Redirect("/运营团队后台/Supplier_PssInfo");
            }
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
            long id = long.Parse(Request.QueryString["id"]);
            bool exist = false;
            供应商 Newmodel = 用户管理.查找用户<供应商>(id);
            IEnumerable<供应商服务记录> service = 供应商服务记录管理.查询供应商服务记录(0, 0, Query<供应商服务记录>.Where(m => m.所属供应商.用户ID == id));
            foreach (var item in service)
            {
                foreach (var item1 in item.已开通的服务)
                {
                    if (item1.所申请项目名 == "商务会员")
                    {
                        exist = true;
                        break;
                    }
                }
            }
            供应商._产品类别 product = new 供应商._产品类别();
            string oldfirst = Request.QueryString["old"];
            string alltype = Request.QueryString["tpy"];
            string FirstType = Request.QueryString["first_type"];
            string SecondType = Request.QueryString["second_type"];
            product.一级分类 = FirstType;
            string[] secondstype = SecondType.Split(',');
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
                    if (!exist)
                    {
                        if (oldfirst != "医疗设备" && oldfirst != "油料设备器材" && oldfirst != "给养器材" && oldfirst != "军用食品" && oldfirst != "被装材料" && oldfirst != "后勤装备" && oldfirst != "药品" && oldfirst != "被装" && oldfirst != "医用耗材" && oldfirst != "军事交通器材" && oldfirst != "基建营房工程器材")
                        {
                            Newmodel.可提供产品类别列表.Clear();
                        }
                    }
                }
                Newmodel.可提供产品类别列表.Add(product);
                Newmodel.企业基本信息.所属行业 += FirstType + ";";
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(oldfirst))
                {
                    if (oldfirst == "医疗设备" || oldfirst == "油料设备器材" || oldfirst == "给养器材" || oldfirst == "军用食品" || oldfirst == "被装材料" || oldfirst == "后勤装备" || oldfirst == "药品" || oldfirst == "被装" || oldfirst == "医用耗材" || oldfirst == "军事交通器材" || oldfirst == "基建营房工程器材")
                    {
                        Newmodel.可提供产品类别列表.Clear();
                    }
                }
                if (Newmodel.可提供产品类别列表 != null && Newmodel.可提供产品类别列表.Count != 0)
                {
                    bool IsExist = false;
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
                        Newmodel.可提供产品类别列表.Add(product);
                        Newmodel.企业基本信息.所属行业 += FirstType + ";";
                    }
                }
                else
                {
                    Newmodel.可提供产品类别列表.Add(product);
                    Newmodel.企业基本信息.所属行业 += FirstType + ";";
                }
            }
            Newmodel.审核数据.审核状态 = 审核状态.审核通过;
            deleteIndex("/Lucene.Net/IndexDic/Gys", Newmodel.Id.ToString());
            //deleteIndex_ProductCatalog("/Lucene.Net/IndexDic/GysCatalog", Newmodel.企业基本信息.企业名称);
            return 用户管理.更新用户<供应商>(Newmodel) ? (Newmodel.可提供产品类别列表.Count - 1).ToString() : "-1";
        }
        public ActionResult delGtype()
        {
            try
            {
                long id = long.Parse(Request.QueryString["id"]);
                int index = int.Parse(Request.QueryString["index"]);
                string name = "";
                string Newfactory = "";
                供应商 model = 用户管理.查找用户<供应商>(id);
                name = model.可提供产品类别列表[index].一级分类;
                if (!string.IsNullOrWhiteSpace(model.企业基本信息.所属行业))
                {
                    string[] factory = model.企业基本信息.所属行业.Split(';');
                    for (int i = 0; i < factory.Length - 1; i++)
                    {
                        if (name != factory[i])
                        {
                            Newfactory += factory[i] + ";";
                        }
                    }
                }
                model.企业基本信息.所属行业 = Newfactory;
                model.可提供产品类别列表.RemoveAt(index);
                deleteIndex("/Lucene.Net/IndexDic/Gys", id.ToString());
                //deleteIndex_ProductCatalog("/Lucene.Net/IndexDic/GysCatalog", model.企业基本信息.企业名称);
                用户管理.更新用户<供应商>(model);
                return Redirect("/运营团队后台/Modify_Supplier_Info?id=" + id.ToString());
            }
            catch
            {
                return Redirect("/运营团队后台/Supplier_PssInfo");
            }
        }
        public string Modify_Good_Type()
        {
            try
            {
                long id = long.Parse(Request.QueryString["id"]);
                供应商 m = 用户管理.查找用户<供应商>(id);
                string Fname = Request.QueryString["first_type"];
                List<string> SecondName = Request.QueryString["second_type"].ToString().Split(',').ToList();
                foreach (var item in m.可提供产品类别列表)
                {
                    if (item.一级分类 == Fname)
                    {
                        item.二级分类 = SecondName;
                        break;
                    }
                }
                用户管理.更新用户<供应商>(m);
                return "修改成功！";
            }
            catch
            {
                return "-1";
            }
        }
        public ActionResult ReSubmit()
        {
            try
            {
                int code = int.Parse(Request.QueryString["code"]);
                long id = long.Parse(Request.QueryString["id"]);
                供应商 model = 用户管理.查找用户<供应商>(id);
                if (model != null)
                {
                    model.供应商用户信息.已提交 = false;
                    model.审核数据.审核状态 = 审核状态.未审核;
                    model.供应商用户信息.初审数据.审核状态 = 审核状态.未审核;
                    model.供应商用户信息.复审数据.审核状态 = 审核状态.未审核;
                    model.供应商用户信息.符合入库标准 = false;
                    用户管理.更新用户<供应商>(model);
                    deleteIndex("/Lucene.Net/IndexDic/Gys", model.Id.ToString());
                    //deleteIndex_ProductCatalog("/Lucene.Net/IndexDic/GysCatalog", model.企业基本信息.企业名称);
                    if (code == 0)
                    {
                        return Redirect("/运营团队后台/Modify_Supplier_Info?id=" + id.ToString());
                    }
                    else
                    {
                        return Redirect("/运营团队后台/Modify_Supplier_Info?id=" + id.ToString());
                    }
                }
                else
                {
                    return Redirect("/运营团队后台/Supplier_PssInfo");
                }
            }
            catch
            {
                return Redirect("/运营团队后台/Supplier_PssInfo");
            }
        }
        public ActionResult Delete_Qualify()
        {
            try
            {
                long id = long.Parse(Request.QueryString["id"]);
                供应商 Newmodel = 用户管理.查找用户<供应商>(id);
                int index = int.Parse(Request.QueryString["index"]);
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
                Newmodel.审核数据.审核状态 = 审核状态.审核通过;
                deleteIndex("/Lucene.Net/IndexDic/Gys", Newmodel.Id.ToString());
                //deleteIndex_ProductCatalog("/Lucene.Net/IndexDic/GysCatalog", Newmodel.企业基本信息.企业名称);
                用户管理.更新用户<供应商>(Newmodel);
                return Redirect("/运营团队后台/Supplier_PssInfo?page=1");
            }
            catch
            {
                return Redirect("/运营团队后台/Supplier_PssInfo?page=1");
            }
        }
        [HttpPost]
        public ActionResult Qualify_Manage(供应商 model)
        {
            long id = long.Parse(Request.Form["id"].ToString());
            供应商 Newmodel = 用户管理.查找用户<供应商>(id);
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
            Newmodel.审核数据.审核状态 = 审核状态.审核通过;
            deleteIndex("/Lucene.Net/IndexDic/Gys", model.Id.ToString());
            //deleteIndex_ProductCatalog("/Lucene.Net/IndexDic/GysCatalog", Newmodel.企业基本信息.企业名称);
            用户管理.更新用户<供应商>(Newmodel);
            return Content("<script>if(confirm('请确保资质证书信息已完善，点击确定请等待审核，点击取消继续完善。')){window.location='/运营团队后台/Supplier_PssInfo?page=1';} else{window.location='/运营团队后台/Supplier_PssInfo?page=1';}</script>");
        }
        [HttpPost]
        public ActionResult Modify_SupplierInfo(供应商 model)
        {
            try
            {
                string ns = null;
                if (Request.Form["nianshen"] != null)
                {
                    ns = Request.Form["nianshen"].ToString();
                }
                List<_地域> area = new List<_地域>();
                供应商 NewModel = 用户管理.查找用户<供应商>(model.Id);
                供应商._供应商用户信息._U盾信息 u = NewModel.供应商用户信息.U盾信息;
                if (model.供应商用户信息.协议供应商)
                {
                    if (NewModel.供应商用户信息.协议供应商所属地区 != null && NewModel.供应商用户信息.协议供应商所属地区.Count() != 0)
                    {
                        area = NewModel.供应商用户信息.协议供应商所属地区;
                    }
                }
                else
                {
                    if (NewModel.供应商用户信息.协议供应商所属地区 != null && NewModel.供应商用户信息.协议供应商所属地区.Count() != 0)
                    {
                        NewModel.供应商用户信息.协议供应商所属地区.Clear();
                    }
                }
                if (NewModel.供应商用户信息.供应商图片 != null && NewModel.供应商用户信息.供应商图片.Count() != 0)
                {
                    for (int i = 0; i < NewModel.供应商用户信息.供应商图片.Count(); i++)
                    {
                        if (!System.IO.File.Exists(Server.MapPath(@NewModel.供应商用户信息.供应商图片[i])))
                        {
                            NewModel.供应商用户信息.供应商图片.Remove(@NewModel.供应商用户信息.供应商图片[i]);
                        }
                    }
                }
                List<string> showPic = NewModel.供应商用户信息.供应商图片;
                string OrganizePic = NewModel.工商注册信息.组织机构代码证电子扫描件;
                string BankPic = NewModel.工商注册信息.基本账户开户银行资信证明电子扫描件;
                string SocialPic = NewModel.工商注册信息.近3年缴纳社会保证金证明电子扫描件;
                string LawPic = NewModel.工商注册信息.近3年有无重大违法记录电子扫描件;
                string SalePic = NewModel.营业执照信息.营业执照电子扫描件;
                string LawPersonPic = NewModel.法定代表人信息.法定代表人身份证电子扫描件;
                string TaxPic = NewModel.税务信息.近3年完税证明电子扫描件;
                string Taxregister = NewModel.营业执照信息.税务登记证电子扫描件;
                for (int i = 0; i < Request.Files.Count; i++)
                {
                    HttpPostedFileBase img = Request.Files[i];
                    if (img != null && img.ContentType == "image/jpeg" && (((img.ContentLength / 1024) / 1024) < 2))
                    {
                        string filePath = 上传管理.获取上传路径<供应商>(媒体类型.图片, 路径类型.不带域名根路径);
                        string fname = UploadAttachment(img);
                        string file_name = filePath + fname;
                        switch (i)
                        {
                            case 0: showPic.Add(file_name); break;
                            case 1: OrganizePic = file_name;
                                if (System.IO.File.Exists(Server.MapPath(@NewModel.工商注册信息.组织机构代码证电子扫描件)))
                                {
                                    System.IO.File.Delete(Server.MapPath(@NewModel.工商注册信息.组织机构代码证电子扫描件));
                                }
                                break;
                            case 2: BankPic = file_name; if (System.IO.File.Exists(Server.MapPath(@NewModel.工商注册信息.基本账户开户银行资信证明电子扫描件)))
                                {
                                    System.IO.File.Delete(Server.MapPath(@NewModel.工商注册信息.基本账户开户银行资信证明电子扫描件));
                                } break;
                            case 3: SocialPic = file_name; if (System.IO.File.Exists(Server.MapPath(@NewModel.工商注册信息.近3年缴纳社会保证金证明电子扫描件)))
                                {
                                    System.IO.File.Delete(Server.MapPath(@NewModel.工商注册信息.近3年缴纳社会保证金证明电子扫描件));
                                } break;
                            case 4: LawPic = file_name; if (System.IO.File.Exists(Server.MapPath(@NewModel.工商注册信息.近3年有无重大违法记录电子扫描件)))
                                {
                                    System.IO.File.Delete(Server.MapPath(@NewModel.工商注册信息.近3年有无重大违法记录电子扫描件));
                                } break;
                            case 5: SalePic = file_name; if (System.IO.File.Exists(Server.MapPath(@NewModel.营业执照信息.营业执照电子扫描件)))
                                {
                                    System.IO.File.Delete(Server.MapPath(@NewModel.营业执照信息.营业执照电子扫描件));
                                } break;
                            case 6: Taxregister = file_name; if (System.IO.File.Exists(Server.MapPath(@NewModel.营业执照信息.税务登记证电子扫描件)))
                                {
                                    System.IO.File.Delete(Server.MapPath(@NewModel.营业执照信息.税务登记证电子扫描件));
                                } break;
                            case 7: LawPersonPic = file_name; if (System.IO.File.Exists(Server.MapPath(@NewModel.法定代表人信息.法定代表人身份证电子扫描件)))
                                {
                                    System.IO.File.Delete(Server.MapPath(@NewModel.法定代表人信息.法定代表人身份证电子扫描件));
                                } break;
                            case 8: TaxPic = file_name; if (System.IO.File.Exists(Server.MapPath(@NewModel.税务信息.近3年完税证明电子扫描件)))
                                {
                                    System.IO.File.Delete(Server.MapPath(@NewModel.税务信息.近3年完税证明电子扫描件));
                                } break;
                        }
                    }
                }
                DateTime d = DateTime.Now;
                if (ns == "1" && !NewModel.供应商用户信息.年检列表.ContainsKey(DateTime.Now.Year.ToString()))
                {
                    model.供应商用户信息.年检列表.Add(d.Year.ToString(), new 操作数据() { 操作时间 = d });
                }
                else
                {
                    model.供应商用户信息.年检列表 = NewModel.供应商用户信息.年检列表;
                    model.供应商用户信息.年检列表.Remove(d.Year.ToString());
                }
                NewModel.所属地域 = model.所属地域;
                NewModel.法定代表人信息 = model.法定代表人信息;
                NewModel.工商注册信息 = model.工商注册信息;
                NewModel.供应商用户信息 = model.供应商用户信息;
                NewModel.供应商用户信息.U盾信息 = u;
                NewModel.联系方式 = model.联系方式;
                NewModel.税务信息 = model.税务信息;
                NewModel.企业基本信息 = model.企业基本信息;
                NewModel.企业联系人信息 = model.企业联系人信息;
                NewModel.营业执照信息 = model.营业执照信息;
                NewModel.信用评级信息 = model.信用评级信息;
                NewModel.供应商用户信息.供应商图片 = showPic;
                NewModel.工商注册信息.组织机构代码证电子扫描件 = OrganizePic;
                NewModel.工商注册信息.近3年缴纳社会保证金证明电子扫描件 = SocialPic;
                NewModel.工商注册信息.近3年有无重大违法记录电子扫描件 = LawPic;
                NewModel.法定代表人信息.法定代表人身份证电子扫描件 = LawPersonPic;
                NewModel.营业执照信息.营业执照电子扫描件 = SalePic;
                NewModel.营业执照信息.税务登记证电子扫描件 = Taxregister;
                NewModel.工商注册信息.基本账户开户银行资信证明电子扫描件 = BankPic;
                NewModel.税务信息.近3年完税证明电子扫描件 = TaxPic;
                if (model.供应商用户信息.协议供应商)
                {
                    if (model.供应商用户信息.协议供应商所属地区 != null && model.供应商用户信息.协议供应商所属地区.Count() != 0)
                    {
                        if (NewModel.供应商用户信息.协议供应商所属地区 != null && NewModel.供应商用户信息.协议供应商所属地区.Count() != 0)
                        {
                            foreach (var item in model.供应商用户信息.协议供应商所属地区)
                            {
                                area.Add(item);
                            }
                        }
                        else
                        {
                            area = model.供应商用户信息.协议供应商所属地区;
                        }
                    }
                }
                NewModel.供应商用户信息.协议供应商所属地区 = area;
                if (NewModel.财务状况信息 != null && NewModel.财务状况信息.Count() != 0)
                {
                    foreach (var item in NewModel.财务状况信息)
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
                if (NewModel.出资人或股东信息 != null && NewModel.出资人或股东信息.Count() != 0)
                {
                    foreach (var item in NewModel.出资人或股东信息)
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
                if (NewModel.售后服务机构列表 != null && NewModel.售后服务机构列表.Count() != 0)
                {
                    foreach (var item in NewModel.售后服务机构列表)
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
                用户管理.更新用户<供应商>(NewModel, false);
                deleteIndex("/Lucene.Net/IndexDic/Gys", NewModel.Id.ToString());
                //deleteIndex_ProductCatalog("/Lucene.Net/IndexDic/GysCatalog", NewModel.企业基本信息.企业名称);
                if (NewModel.审核数据.审核状态 == 审核状态.审核通过)
                {
                    CreateIndex_gys(用户管理.查找用户<供应商>(NewModel.Id), "/Lucene.Net/IndexDic/Gys");
                    //CreateIndex_ProductCatalog(NewModel.企业基本信息.企业名称, "/Lucene.Net/IndexDic/GysCatalog");
                }
                return Content("<script>if(confirm('成功修改信息，点击确定返回供应商列表,点击取消，继续修改')){window.location='/运营团队后台/Supplier_PssInfo';}else{window.location='/运营团队后台/Modify_Supplier_Info?id=" + model.Id + "';}</script>");
            }
            catch
            {
                return Redirect("/运营团队后台/Supplier_PssInfo");
            }
        }
        public int DelArea()
        {
            try
            {
                long id = long.Parse(Request.QueryString["id"]);
                int index = int.Parse(Request.QueryString["index"]);
                供应商 model = 用户管理.查找用户<供应商>(id);
                model.供应商用户信息.协议供应商所属地区.RemoveAt(index);
                用户管理.更新用户<供应商>(model, false);
                deleteIndex("/Lucene.Net/IndexDic/Gys", model.Id.ToString());
                //deleteIndex_ProductCatalog("/Lucene.Net/IndexDic/GysCatalog", model.企业基本信息.企业名称);
                return 1;
            }
            catch
            {
                return 0;
            }
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
        public ActionResult Delete_goods()
        {
            try
            {
                long id = long.Parse(Request.QueryString["id"]);
                int index = int.Parse(Request.QueryString["index"]);
                供应商 model = 用户管理.查找用户<供应商>(id);
                model.可提供产品类别列表.RemoveAt(index);
                用户管理.更新用户<供应商>(model);
                return Redirect("/运营团队后台/Supplier_Detail?id=" + id.ToString());
            }
            catch
            {
                return Redirect("/运营团队后台/Supplier_PssInfo");
            }
        }
        public JsonResult Search_Department()
        {
            long id = 0;
            if (!string.IsNullOrWhiteSpace(Request.QueryString["num"]))
            {
                id = long.Parse(Request.QueryString["num"]);
            }
            string name = Request.QueryString["name"];
            string user = Request.QueryString["u"];
            int cpage =int.Parse(Request.QueryString["cp"]);
            int pageCount = 0;
            // int pro = int.Parse(Request.QueryString["pro"]);
            int catorgray = int.Parse(Request.QueryString["catorgray"]);
            IMongoQuery q = null;
            List<Department> deplist = new List<Department>();
            if(id!=0)
            {
                q = q.And(MQ.EQ("_id", id));
            }
            if (!string.IsNullOrWhiteSpace(name))
            {
                q = q.And(MQ.EQ("单位信息.单位名称", new BsonRegularExpression(string.Format("/{0}/i", name))));
            }
            if (catorgray != 0)
            {
                q = q.And(MQ.EQ("单位信息.单位级别", new BsonRegularExpression(string.Format("/{0}/i", catorgray))));
            }
            if(!string.IsNullOrWhiteSpace(user))
            {
                q = q.And(MQ.EQ("联系方式.联系人", new BsonRegularExpression(string.Format("/{0}/i", user))));
            }
            pageCount = 用户管理.查询用户<单位用户>(0,0,q).Count()/20;
            if (用户管理.查询用户<单位用户>(0, 0,q).Count() % 20>0)
            {
                pageCount++;
            }
            IEnumerable<单位用户> depart = 用户管理.查询用户<单位用户>(20*(cpage-1), 20, q);
            foreach (var item in depart)
            {
                item.单位信息.单位编码 = item.单位信息.单位编码.IsEmpty() ? "" : item.单位信息.单位编码;
                item.单位信息.所属单位=item.单位信息.所属单位.IsNullOrWhiteSpace()?"未填写":item.单位信息.所属单位;
                deplist.Add(new Department()
                {
                    Id = item.Id,
                    loginame = item.登录信息.登录名,
                    ssdw = item.单位信息.所属单位,
                    unitname = item.单位信息.单位名称,
                    contactuser = item.联系方式.联系人,
                    status = item.审核数据.审核状态.ToString(),
                });
            }
            JsonResult json = new JsonResult() { Data = new { deplist, PageCount = pageCount }};
            return Json(json, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Part_DepartmentList()
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
            long pc = 用户管理.计数用户<单位用户>(0,0);
            pgCount = pc / 20;
            if (pc %20 > 0)
            {
                pgCount++;
            }
            ViewData["Pagecount"] = pgCount;
            ViewData["CurrentPage"] = cpg;
            ViewData["user"] = 用户管理.查询用户<单位用户>(20 * (cpg - 1), 20);
            return PartialView("Part_View/Part_DepartmentList");
        }
        public ActionResult Part_DepartmentAuditList(int? page)
        {

            if (string.IsNullOrEmpty(page.ToString()))
            {
                page = 1;
            }
            ViewData["listcount"] = (int)用户管理.计数用户<单位用户>(0, 0, Query.EQ("审核数据.审核状态", 审核状态.未审核));
            ViewData["pagesize"] = 20;
            ViewData["user"] = 用户管理.查询用户<单位用户>(20 * (int.Parse(page.ToString()) - 1), 20, Query.EQ("审核数据.审核状态", 审核状态.未审核));
            return PartialView("Part_View/Part_DepartmentAuditList");
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
            return PartialView("Part_View/Part_DepartmentAudit", model);
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

            return PartialView("Part_View/Part_Department_Detail", model);
        }
        public ActionResult Part_DepartmentModify()
        {
            string id = Request.Params["id"];
            ViewData["id"] = id;
            单位用户 unituser = 用户管理.查找用户(long.Parse(id)) as 单位用户;
            ViewData["user"] = 用户管理.查询用户<单位用户>(0, 0);
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
            
            return PartialView("Part_View/Part_DepartmentModify",unituser);
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateInput(false)]
        public ActionResult DepartmentEdit(单位用户 model)
        {
            bool IsTrue = false;
            if (ModelState.IsValidField("u.单位信息.单位代号"))
            {
                IsTrue = true;
            }
            if (IsTrue)
            {
                var usergroup = Request.Form["usergroup"];
                if (!string.IsNullOrWhiteSpace(usergroup))
                {
                    var group = usergroup.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var name in group)
                    {
                        model.用户组.Add(name);
                    }
                }

                var p = Request["deliverprovince"];
                var c = Request["delivercity"];
                var a = Request["deliverarea"];
                model.所属地域.省份 = p;
                model.所属地域.城市 = c;
                model.所属地域.区县 = a;

                var gldw = Request.Form["gldw"];
                switch (gldw)
                {
                    case "成都":
                        model.管理单位.用户ID = 16;
                        break;
                    case "重庆":
                        model.管理单位.用户ID = 12;
                        break;
                    case "云南":
                        model.管理单位.用户ID = 13;
                        break;
                    case "贵州":
                        model.管理单位.用户ID = 14;
                        break;
                    case "西藏":
                        model.管理单位.用户ID = 15;
                        break;
                    default:
                        model.管理单位.用户ID = 16;
                        break;
                }
                //if (Request.Form["admin"] != "-1")
                //{
                //    model.所属单位.用户ID = long.Parse(Request.Form["admin"]);
                //}

                var dwuser = 用户管理.查找用户<单位用户>(model.Id);
                dwuser.管理单位 = model.管理单位;
                dwuser.单位信息.单位名称 = model.单位信息.单位名称;
                dwuser.单位信息.单位代号 = model.单位信息.单位代号;
                dwuser.所属单位 = model.所属单位;
                dwuser.单位信息.单位级别 = model.单位信息.单位级别;
                dwuser.联系方式.联系人 = model.联系方式.联系人;
                dwuser.联系人职务 = model.联系人职务;
                dwuser.联系方式.固定电话 = model.联系方式.固定电话;
                dwuser.联系方式.手机 = model.联系方式.手机;
                if (dwuser.地址信息.Count > 0)
                {
                    dwuser.地址信息[0].城市 = model.地址信息[0].城市;
                }
                else
                {
                    dwuser.地址信息.Add(new _地址信息 { 城市 = model.地址信息[0].城市 });
                }             
                dwuser.所属地域 = model.所属地域;
                dwuser.用户组 = model.用户组;
                用户管理.更新用户<单位用户>(dwuser,false);
            }
           
            return RedirectToAction("Departmentlist");
        }

        public ActionResult Part_DepartmentAdd()
        {
            IEnumerable<单位用户> user = 用户管理.查询用户<单位用户>(0, 0);
            ViewData["user"] = user;
            ViewData["用户组列表"] = 用户组管理.查询用户组(0, 0);
            return PartialView("Part_View/Part_DepartmentAdd");
        }
        public class User
        {
            public long Id { get; set; }
            public string Name { get; set; }
        }
        public ActionResult SearchUnit()
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
                return Redirect("/注册/Register_Unit");
            }
        }
        public ActionResult Print_UserList()
        {
            return View();
        }
        public ActionResult Part_Print_UserList(int? page)
        {
            var pagesize = 15;

            var liscount = 用户管理.计数用户<单位用户>(0, 0, Query.EQ("审核数据.审核状态", 审核状态.审核通过));

            var maxpage = Math.Max((liscount + pagesize - 1) / pagesize, 1);
            if (string.IsNullOrEmpty(page.ToString()) || page < 0 || page > maxpage)
            {
                page = 1;
            }
            ViewData["currentPage"] = page;
            ViewData["pagecount"] = maxpage;

            ViewData["查询单位列表"] = 用户管理.查询用户<单位用户>(((int)page - 1) * pagesize, pagesize, Query.EQ("审核数据.审核状态", 审核状态.审核通过));
            return PartialView("Part_View/Part_Print_UserList");
        }
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
                return PartialView("Part_View/Part_Print_UserListTable");
            }
            catch
            {
                return Content("<span style='color:red;'>条件有误，请重新查询！</span>");
            }
        }


        public ActionResult Modify_Finance()
        {
            try
            {
                long id = long.Parse(Request.QueryString["id"]);
                int index = int.Parse(Request.QueryString["index"]);
                供应商 Newmodel = 用户管理.查找用户<供应商>(id);
                if (index > Newmodel.财务状况信息.Count() || index < 0)
                {
                    return Redirect("/运营团队后台/Modify_Supplier_Info?id=" + id.ToString());
                }
                ViewData["index"] = index;
                return View(Newmodel);
            }
            catch
            {
                return Redirect("/运营团队后台/Supplier_PssInfo");
            }
        }
        [HttpPost]
        public ActionResult Modify_finance(供应商 model)
        {
            int index = int.Parse(Request.Form["index"].ToString());
            供应商 Newmodel = 用户管理.查找用户<供应商>(model.Id);
            if (index > Newmodel.财务状况信息.Count() || index < 0)
            {
                return Redirect("/运营团队后台/Supplier_PssInfo");
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
                ? Content("<script>alert('修改成功！');window.location='/运营团队后台/Modify_Supplier_Info?id=" + model.Id.ToString() + "';</script>")
                : Content("<script>window.location='/运营团队后台/Modify_Supplier_Info?id=" + model.Id.ToString() + "';</script>")
                ;
        }
        public ActionResult Part_Depart_GysList()
        {
            return PartialView("Part_View/Part_Depart_GysList");
        }

        public ActionResult Part_Zb_ReleaseInfo()
        {
            return PartialView("Part_View/Part_Zb_ReleaseInfo");
        }
        public ActionResult Part_Procure_AdList(int? page)
        {
            var adclass = "";
            var pro = "";
            var city = "";
            var area = "";
            var hy = "";
            var keyword = "";
            var time = -1;

            ViewData["currentpage"] = 1;
            ViewData["pagecount"] = 1;
            int listcount = (int)(公告管理.计数公告(0, -1, Query<公告>.Where(o => o.公告信息.是否撤回 == false)));
            int maxpage = Math.Max((listcount + GG_PAGESIZE - 1) / GG_PAGESIZE, 1);
            if (string.IsNullOrEmpty(page.ToString()) || page < 0 || page > maxpage)
            {
                page = 1;
            }
            ViewData["currentpage"] = page;
            ViewData["pagecount"] = maxpage;

            ViewData["后台公告列表"] = 公告管理.查询公告(GG_PAGESIZE * (int.Parse(page.ToString()) - 1), GG_PAGESIZE, Query<公告>.Where(o => o.公告信息.是否撤回 == false), false, SortBy.Descending("内容主体.发布时间"));
            ViewData["行业列表"] = 商品分类管理.查找子分类();
            ViewBag.Provence = pro;
            ViewBag.City = city;
            ViewBag.Area = area;
            ViewBag.Hy = hy;
            ViewBag.Adclass = adclass;
            ViewBag.keyword = keyword;
            ViewBag.time = time;

            return PartialView("Part_View/Part_Procure_AdList");
        }

        public ActionResult Part_Procure_AdList_Search(int? page)
        {
            var adclass = Request.QueryString["adclass"];
            var pro = Request.QueryString["deliverprovince"];
            var city = Request.QueryString["delivercity"];
            var area = Request.QueryString["deliverarea"];
            var hy = Request.QueryString["hy"];
            var keyword = Request.QueryString["keyword"];
            var time = Request.QueryString["time"];
            ViewData["currentpage"] = 1;
            ViewData["pagecount"] = 1;

            var q = Query.Null;
            if (!string.IsNullOrWhiteSpace(adclass))
            {
                if (adclass == "公开招标" || adclass == "其他")
                {
                    q = q.And(Query<公告>.EQ(o => o.公告信息.公告类别, (公告.公告类别)Enum.Parse(typeof(公告.公告类别), adclass)));
                }
                else
                {
                    var q1 = Query<公告>.In(o => o.公告信息.公告类别, new[] { 公告.公告类别.邀请招标, 公告.公告类别.协议采购, 公告.公告类别.单一来源, 公告.公告类别.询价采购, 公告.公告类别.竞争性谈判, 公告.公告类别.网上竞标 });
                    q = q.And(q1);
                }
            }

            if (!string.IsNullOrWhiteSpace(pro))
            {
                q = q.And(Query<公告>.EQ(o => o.公告信息.所属地域.省份, pro));
                if (!string.IsNullOrWhiteSpace(city))
                {
                    q = q.And(Query<公告>.EQ(o => o.公告信息.所属地域.城市, city));
                    if (!string.IsNullOrWhiteSpace(area))
                    {
                        q = q.And(Query<公告>.EQ(o => o.公告信息.所属地域.区县, area));
                    }
                }
            }

            if (!string.IsNullOrWhiteSpace(hy))
            {
                q = q.And(Query<公告>.EQ(o => o.公告信息.一级分类, hy));
            }

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                q = q.And(Query<公告>.Matches(o => o.内容主体.标题, keyword));
            }

            if (!string.IsNullOrWhiteSpace(time) && time != "-1")
            {
                q = q.And(Query<公告>.Where(o => o.内容主体.发布时间 > DateTime.Now.AddDays(0 - int.Parse(time))));
            }


            int listcount = (int)(公告管理.计数公告(0, 0, q.And(Query<公告>.Where(o => o.公告信息.是否撤回 == false))));
            int maxpage = Math.Max((listcount + GG_PAGESIZE - 1) / GG_PAGESIZE, 1);
            if (string.IsNullOrEmpty(page.ToString()) || page < 0 || page > maxpage)
            {
                page = 1;
            }
            ViewData["currentpage"] = page;
            ViewData["pagecount"] = maxpage;

            ViewData["后台公告列表"] = 公告管理.查询公告(GG_PAGESIZE * (int.Parse(page.ToString()) - 1), GG_PAGESIZE, q.And(Query<公告>.Where(o => o.公告信息.是否撤回 == false)), false, SortBy.Descending("内容主体.发布时间"));
            ViewData["行业列表"] = 商品分类管理.查找子分类();
            ViewBag.Provence = pro;
            ViewBag.City = city;
            ViewBag.Area = area;
            ViewBag.Hy = hy;
            ViewBag.Adclass = adclass;
            ViewBag.keyword = keyword;
            ViewBag.time = time;

            return PartialView("Part_View/Part_Procure_AdList_Search");
        }


        public ActionResult Part_Procure_AdAudit(int? page)
        {

            int GG_ING_LISTCOUNT = (int)(公告管理.计数公告(0, -1, Query.EQ("审核数据.审核状态", 审核状态.未审核).And(Query<公告>.Where(o => o.公告信息.是否撤回 == false))));
            int GG_ING_MAXPAGE = Math.Max((GG_ING_LISTCOUNT + 20 - 1) / 20, 1);
            if (string.IsNullOrEmpty(page.ToString()) || page < 0 || page > GG_ING_MAXPAGE)
            {
                page = 1;
            }
            ViewData["page"] = page;
            ViewData["listcount"] = GG_ING_LISTCOUNT;
            ViewData["pagesize"] = 20;

            ViewData["未审核公告列表"] = 公告管理.查询公告(20 * (int.Parse(page.ToString()) - 1), 20, Query.EQ("审核数据.审核状态", 审核状态.未审核).And(Query<公告>.Where(o => o.公告信息.是否撤回 == false)));

            return PartialView("Part_View/Part_Procure_AdAudit");
        }
        public ActionResult Part_Procure_AdDetail(int? id)
        {
            公告 model = null;
            ViewData["允许报名"] = "0";
            if (id != null)
            {
                try
                {
                    model = 公告管理.查找公告((long)id);

                    IEnumerable<招标采购预报名> bm = 招标采购预报名管理.查询招标采购预报名(0, 1, Query<招标采购预报名>.EQ(o => o.所属公告链接.公告ID, (long)id));
                    if (bm != null && bm.Any())
                    {
                        ViewData["允许报名"] = "1";
                    }
                }
                catch
                {

                }
            }
            return PartialView("Part_View/Part_Procure_AdDetail", model);
        }
        public ActionResult Part_AdListDetail(int? id)
        {
            公告 model = null;
            ViewData["允许报名"] = "0";
            if (id != null)
            {
                try
                {
                    model = 公告管理.查找公告((long)id);
                    IEnumerable<招标采购预报名> bm = 招标采购预报名管理.查询招标采购预报名(0, 1, Query<招标采购预报名>.EQ(o => o.所属公告链接.公告ID, (long)id));
                    if (bm != null && bm.Any())
                    {
                        ViewData["允许报名"] = "1";
                    }
                }
                catch
                {

                }
            }
            return PartialView("Part_View/Part_AdListDetail", model);
        }
        public ActionResult AdListDetail(int? id)
        {
            return View();
        }
        public ActionResult Part_Procure_NewsList(int? page)
        {

            int NEWS_LISTCOUNT = (int)(新闻管理.计数新闻(0, 0));
            int NEWS_MAXPAGE = Math.Max((NEWS_LISTCOUNT + NEWS_PAGESIZE - 1) / NEWS_PAGESIZE, 1);

            if (string.IsNullOrEmpty(page.ToString()) || page < 0 || page > NEWS_MAXPAGE)
            {
                page = 1;
            }
            ViewData["page"] = page;
            ViewData["listcount"] = NEWS_LISTCOUNT;

            ViewData["pagesize"] = NEWS_PAGESIZE;

            ViewData["后台新闻列表"] = 新闻管理.查询新闻(NEWS_PAGESIZE * (int.Parse(page.ToString()) - 1), NEWS_PAGESIZE);

            return PartialView("Part_View/Part_Procure_NewsList");
        }
        public ActionResult Part_Procure_NewsDetail(int? id)
        {
            新闻 g = new 新闻();
            if (null != id)
            {
                g = 新闻管理.查找新闻((long)id);
            }
            return PartialView("Part_View/Part_Procure_NewsDetail", g);
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
            return PartialView("Part_View/Part_Procure_AdModify", GongGaoModel);
        }
        public ActionResult Part_Procure_AdAdd()
        {
            ViewData["行业列表"] = 商品分类管理.查找子分类();

            IEnumerable<招标采购项目> l = 招标采购项目管理.查询招标采购项目(0, 0, MongoDB.Driver.Builders.Query.EQ("审核数据.审核状态", 审核状态.审核通过).And(MongoDB.Driver.Builders.Query.EQ("中标公告链接.公告ID", -1)).And(MongoDB.Driver.Builders.Query.EQ("流标公告链接.公告ID", -1)).And(MongoDB.Driver.Builders.Query.EQ("废标公告链接.公告ID", -1)));

            ViewData["需求列表"] = l;
            return PartialView("Part_View/Part_Procure_AdAdd");
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
            return PartialView("Part_View/Part_Procure_NewsModify", g);
        }
        public ActionResult Part_Procure_NewsAdd()
        {
            return PartialView("Part_View/Part_Procure_NewsAdd");
        }
        public ActionResult Part_Procure_TzAdd()
        {
            return PartialView("Part_View/Part_Procure_TzAdd");
        }
        public ActionResult Part_Procure_TzDetail(int? id)
        {
            通知 g = new 通知();
            if (null != id)
            {
                g = 通知管理.查找通知((long)id);
            }
            return PartialView("Part_View/Part_Procure_TzDetail", g);
        }
        public ActionResult Part_Procure_TzList(int? page)
        {

            int listcount = (int)(通知管理.计数通知(0, 0));
            int maxpage = Math.Max((listcount + TZ_PAGESIZE - 1) / TZ_PAGESIZE, 1);

            if (string.IsNullOrEmpty(page.ToString()) || page < 0 || page > maxpage)
            {
                page = 1;
            }
            ViewData["page"] = page;
            ViewData["listcount"] = listcount;

            ViewData["pagesize"] = TZ_PAGESIZE;

            ViewData["后台通知列表"] = 通知管理.查询通知(TZ_PAGESIZE * (int.Parse(page.ToString()) - 1), TZ_PAGESIZE);

            return PartialView("Part_View/Part_Procure_TzList");
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
            return PartialView("Part_View/Part_Procure_TzModify", g);
        }
        public ActionResult Part_Procure_ZcfgAdd()
        {
            return PartialView("Part_View/Part_Procure_ZcfgAdd");
        }
        public ActionResult Part_Procure_ZcfgDetail(int? id)
        {
            政策法规 g = new 政策法规();
            if (null != id)
            {
                g = 政策法规管理.查找政策法规((long)id);
            }

            return PartialView("Part_View/Part_Procure_ZcfgDetail", g);
        }
        public ActionResult Part_Procure_ZcfgList(int? page)
        {

            if (string.IsNullOrEmpty(page.ToString()))
            {
                page = 1;
            }
            ViewData["page"] = page;
            ViewData["listcount"] = (int)(政策法规管理.计数政策法规(0, 0));
            ViewData["pagesize"] = 15;
            ViewData["后台政策法规列表"] = 政策法规管理.查询政策法规(15 * (int.Parse(page.ToString()) - 1), 15);

            return PartialView("Part_View/Part_Procure_ZcfgList");
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

            return PartialView("Part_View/Part_Procure_ZcfgModify", g);
        }
        public ActionResult DepartmentModify()
        {
            return View();
        }
        public ActionResult DepartmentList()
        {
            return View();
        }
        public ActionResult DepartmentAdd()
        {
            return View();
        }
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
        public ActionResult Depart_GysList()
        {
            return View();
        }

        public ActionResult Zb_ReleaseInfo()
        {
            return View();
        }
        public ActionResult Procure_AdList()
        {

            return View();
        }
        public ActionResult Procure_AdAudit()
        {
            if (currentUser.Id != 100000000001)
            {
                return Content("<script>window.location='/错误页面/NoPrivilege';</script>");
            }
            return View();
        }
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult Procure_AdDetail(string action, Models.数据模型.内容数据模型.公告 g)
        {
            if (action == "审核通过")
            {
                公告管理.审核公告(g.Id, currentUser.Id, 审核状态.审核通过);

                deleteIndex("/Lucene.Net/IndexDic/GongGao", g.Id.ToString());

                GG_CreateIndex(公告管理.查找公告(g.Id), "/Lucene.Net/IndexDic/GongGao");
            }
            else if (action == "审核不通过")
            {
                公告管理.审核公告(g.Id, currentUser.Id, 审核状态.审核未通过);
                deleteIndex("/Lucene.Net/IndexDic/GongGao", g.Id.ToString());
            }
            return View("Procure_AdAudit");
        }


        public ActionResult Procure_AdDetail()
        {
            if (currentUser.Id != 100000000001)
            {
                return Content("<script>window.location='/错误页面/NoPrivilege';</script>");
            }
            return View();
        }
        public ActionResult Procure_AdAdd()
        {
            return View();
        }
        public ActionResult Procure_NewsList()
        {
            return View();
        }
        public ActionResult Procure_NewsDetail()
        {
            return View();
        }
        public ActionResult Procure_AdModify()
        {
            return View();
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
        public ActionResult Procure_TzDetail()
        {
            return View();
        }
        public ActionResult Procure_TzModify()
        {
            return View();
        }
        public ActionResult Procure_TzAdd()
        {
            return View();
        }

        public ActionResult TrainingAdd()
        {
            培训资料 model = new 培训资料();
            return View(model);
        }
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
                return Content("<script>if(confirm('成功添加培训资料，点击确定继续添加，点击取消进入查看')){window.location='/运营团队后台/TrainingAdd';}else{window.location='/运营团队后台/TrainingList';}</script>");
            }
            catch
            {
                return Redirect("/运营团队后台/TrainingAdd");
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
            IEnumerable<培训资料> model = 培训资料管理.查询培训资料(0, 0);
            return View(model);
        }
        public ActionResult TrainDetail()
        {
            try
            {
                long id = long.Parse(Request.QueryString["id"]);
                培训资料 model = 培训资料管理.查找培训资料(id);
                if (model == null)
                {
                    return Redirect("/运营团队后台/TrainingList");
                }
                else
                {
                    return View(model);
                }
            }
            catch
            {
                return Redirect("/运营团队后台/TrainingList");
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
                    return Redirect("/运营团队后台/TrainingList");
                }
                return View(model);
            }
            catch
            {
                return Redirect("/运营团队后台/TrainingList");
            }
        }
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
                return Content("<script>alert('修改成功！');window.location='/运营团队后台/TrainingList';</script>");
            }
            catch
            {
                return Redirect("/运营团队后台/TrainingList");
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
                return Redirect("/运营团队后台/TrainingList");
            }
            catch
            {
                return Redirect("/运营团队后台/TrainingList");
            }
        }
        public ActionResult GuideAdd()
        {
            办事指南 guid = new 办事指南();
            return View(guid);
        }
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
                return Content("<script>if(confirm('成功添加办事指南，点击确定继续添加，点击取消进入查看')){window.location='/运营团队后台/GuideAdd'}else{window.location='/运营团队后台/GuideList'};</script>");
            }
            catch
            {
                return Redirect("/运营团队后台/GuideAdd");
            }
        }
        public ActionResult GuideList()
        {
            IEnumerable<办事指南> model = 办事指南管理.查询办事指南(0, 0);
            return View(model);
        }
        public ActionResult guideDetail()
        {
            try
            {
                long id = long.Parse(Request.QueryString["id"]);
                办事指南 model = 办事指南管理.查找办事指南(id);
                if (model == null)
                {
                    return Redirect("/运营团队后台/GuideList");
                }
                else
                {
                    return View(model);
                }
            }
            catch
            {
                return Redirect("/运营团队后台/GuideList");
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
                    return Redirect("/运营团队后台/TrainingList");
                }
                return View(model);
            }
            catch
            {
                return Redirect("/运营团队后台/TrainingList");
            }
        }
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
                return Content("<script>alert('修改成功！');window.location='/运营团队后台/GuideList';</script>");
            }
            catch
            {
                return Redirect("/运营团队后台/GuideList");
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
                return Redirect("/运营团队后台/GuideList");
            }
            catch
            {
                return Redirect("/运营团队后台/GuideList");
            }
        }

        [HttpPost]
        public ActionResult UploadImagesGT()
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
        public class PartialViewViewModel
        {
            public int id { set; get; }

            public string Name { set; get; }
        }
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
            var zb_contact = Request.Form["zb_contact"];
            var isybm = Request.Form["isybm"];
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
                if (Request.Form["isattach"] != "0" && !string.IsNullOrEmpty(model.内容主体.附件[0]))
                {
                    var str = model.内容主体.附件[0].Trim();
                    model.内容主体.附件 = new List<string>(str.Split(new[] { "|" }, StringSplitOptions.RemoveEmptyEntries));
                }
                else
                {
                    model.内容主体.附件 = null;
                }
                //扫描件
                if (Request.Form["isattach_s"] != "0" && !string.IsNullOrEmpty(model.内容主体.图片[0]))
                {
                    var str = model.内容主体.图片[0].Trim();
                    model.内容主体.图片 = new List<string>(str.Split(new[] { "|" }, StringSplitOptions.RemoveEmptyEntries));
                }
                else
                {
                    model.内容主体.图片 = null;
                }

                //model.公告信息.一级分类 = Request.Form["hy"];
                model.公告信息.所属地域.省份 = Request.Form["deliverprovince"];
                model.公告信息.所属地域.城市 = Request.Form["delivercity"];
                model.公告信息.所属地域.区县 = Request.Form["deliverarea"];
                公告管理.添加公告(model);

                //if (!string.IsNullOrEmpty(zb_contact))
                //{
                //    try
                //    {
                //        招标采购项目 z = 招标采购项目管理.查找招标采购项目(int.Parse(zb_contact));
                //        if (model.公告信息.公告版本 == 公告.公告版本.变更 || model.公告信息.公告版本 == 公告.公告版本.更正)
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
            //CreateIndex(model, "/Lucene.Net/IndexDic/GongGao");
            return View("Procure_AdList");
        }
        public ActionResult Procure_AdDelete(long? id)
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

                        IEnumerable<招标采购预报名> bm = 招标采购预报名管理.查询招标采购预报名(0, 1, Query<招标采购预报名>.EQ(o => o.所属公告链接.公告ID, id));
                        if (bm != null && bm.Any())
                        {
                            招标采购预报名 bm_model = bm.ToList()[0];
                            招标采购预报名管理.删除招标采购预报名(bm_model.Id);
                        }
                    }
                }
                catch
                {
                    if (html == "adaudit")
                    {
                        return View("Procure_AdAudit");
                    }
                    else
                    {
                        return View("Procure_AdList");
                    }
                }
            }
            if (html == "adaudit")
            {
                return View("Procure_AdAudit");
            }
            else
            {
                return View("Procure_AdList");
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

                    }
                }
                else
                {
                    if (公告管理.屏蔽公告解除((long)id))
                    {

                    }
                }
            }
            return View("Procure_AdList");
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
        public ActionResult Procure_AdModify(Models.数据模型.内容数据模型.公告 model)
        {
            //对未绑定字段赋值
            var m = 公告管理.查找公告(model.Id);
            model.审核数据 = m.审核数据;
            model.审核数据2 = m.审核数据2;
            model.项目信息.项目标的物 = m.项目信息.项目标的物;
            model.点击次数 = m.点击次数;
            model.内容基本信息 = m.内容基本信息;

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

                }
            }

            try
            {
                //附件
                var attachstr = Request.Form["attachtext"];
                var deleteattachstr = Request.Form["deletattachtext"];
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
                    model.内容主体.附件 = new List<string>(attachstr.Split(new[] { "|" }, StringSplitOptions.RemoveEmptyEntries));
                }
                else
                {
                    model.内容主体.附件 = null;
                }

                //扫描件
                attachstr = Request.Form["attachtext_s"];
                deleteattachstr = Request.Form["deletattachtext_s"];
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
                    model.内容主体.图片 = new List<string>(attachstr.Split(new[] { "|" }, StringSplitOptions.RemoveEmptyEntries));
                }
                else
                {
                    model.内容主体.图片 = null;
                }

                //model.公告信息.一级分类 = Request.Form["hy"];
                model.公告信息.所属地域.省份 = Request.Form["deliverprovince"];
                model.公告信息.所属地域.城市 = Request.Form["delivercity"];
                model.公告信息.所属地域.区县 = Request.Form["deliverarea"];
                公告管理.更新公告(model);
                deleteIndex("/Lucene.Net/IndexDic/GongGao", model.Id.ToString());
            }
            catch
            {
                return Content("<script>alert('输入有误!请检查数据格式');location.href='javascript:history.go(-1)';</script>");
            }
            //CreateIndex(model, "/Lucene.Net/IndexDic/GongGao");
            return View("Procure_AdList");
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
                    if (!string.IsNullOrEmpty(Request.Form["attachtext"]))
                    {
                        model.内容主体.图片 = new List<string>(Request.Form["attachtext"].Substring(0, Request.Form["attachtext"].Length - 1).Split('|'));
                    }
                    model.审核数据.审核状态 = 审核状态.审核通过;
                    model.审核数据.审核时间 = DateTime.Now;
                    model.审核数据.审核者.用户ID = currentUser.Id;
                    新闻管理.添加新闻(model);

                    CreateIndex(model, "/Lucene.Net/IndexDic/News");
                }
                catch
                {
                    return Content("<script>alert('输入有误!请检查数据格式');location.href='javascript:history.go(-1)';</script>");
                }
            }
            return View("Procure_NewsAdd");
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
                    return View("Procure_NewsList");
                }
            }
            return View("Procure_NewsList");
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
                    var attachstr = Request.Form["attachtext"];
                    var deleteattachstr = Request.Form["deletattachtext"];
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
                        model.内容主体.图片 = new List<string>(attachstr.Substring(0, attachstr.Length - 1).Split('|'));
                    }
                    else
                    {
                        model.内容主体.图片 = null;
                    }
                    model.审核数据.审核状态 = 审核状态.审核通过;
                    model.审核数据.审核时间 = DateTime.Now;
                    model.审核数据.审核者.用户ID = currentUser.Id;
                    新闻管理.更新新闻(model);

                    updateIndex("/Lucene.Net/IndexDic/News", model);
                }
                catch
                {
                    return Content("<script>alert('输入有误!请检查数据格式');location.href='javascript:history.go(-1)';</script>");
                }
                //CreateIndex(model, "/Lucene.Net/IndexDic/GongGao");
            }
            return View("Procure_NewsList");
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
                    if (!string.IsNullOrEmpty(model.内容主体.附件[0]))
                    {
                        model.内容主体.附件 = new List<string>(model.内容主体.附件[0].Substring(0, model.内容主体.附件[0].Length - 1).Split('|'));
                    }

                    else
                    {
                        model.内容主体.附件 = null;
                    }
                    model.审核数据.审核状态 = 审核状态.审核通过;
                    model.审核数据.审核时间 = DateTime.Now;
                    model.审核数据.审核者.用户ID = currentUser.Id;
                    政策法规管理.添加政策法规(model);

                    CreateIndex(model, "/Lucene.Net/IndexDic/Zcfg");
                }
                catch
                {
                    return Content("<script>alert('输入有误!请检查数据格式');location.href='javascript:history.go(-1)';</script>");
                }
            }
            return View("Procure_ZcfgAdd");
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
                    return Content("<script>alert('输入有误!请检查数据格式');location.href='javascript:history.go(-1)';</script>");
                }
            }
            return View("Procure_ZcfgList");
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
                    var attachstr = Request.Form["attachtext"];
                    var deleteattachstr = Request.Form["deletattachtext"];
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
                    model.审核数据.审核状态 = 审核状态.审核通过;
                    model.审核数据.审核时间 = DateTime.Now;
                    model.审核数据.审核者.用户ID = currentUser.Id;
                    政策法规管理.更新政策法规(model);
                    updateIndex("/Lucene.Net/IndexDic/Zcfg", model);
                }
                catch
                {
                    return Content("<script>alert('输入有误!请检查数据格式');location.href='javascript:history.go(-1)';</script>");
                }
                //CreateIndex(model, "/Lucene.Net/IndexDic/GongGao");
            }
            return View("Procure_ZcfgList");
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
                    if (!string.IsNullOrEmpty(model.内容主体.附件[0]))
                    {
                        model.内容主体.附件 = new List<string>(model.内容主体.附件[0].Substring(0, model.内容主体.附件[0].Length - 1).Split('|'));
                    }
                    else
                    {
                        model.内容主体.附件 = null;
                    }
                    model.审核数据.审核状态 = 审核状态.审核通过;
                    model.审核数据.审核时间 = DateTime.Now;
                    model.审核数据.审核者.用户ID = currentUser.Id;
                    通知管理.添加通知(model);

                    CreateIndex(model, "/Lucene.Net/IndexDic/Tongzhi");
                }
                catch
                {
                    return Content("<script>alert('输入有误!请检查数据格式');location.href='javascript:history.go(-1)';</script>");
                }

            }
            return View("Procure_TzList");
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
                    return View("Procure_TzList");
                }
            }
            return View("Procure_TzList");
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
                    var attachstr = Request.Form["attachtext"];
                    var deleteattachstr = Request.Form["deletattachtext"];
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
                    model.审核数据.审核状态 = 审核状态.审核通过;
                    model.审核数据.审核时间 = DateTime.Now;
                    model.审核数据.审核者.用户ID = currentUser.Id;
                    通知管理.更新通知(model);
                    updateIndex("/Lucene.Net/IndexDic/Tongzhi", model);
                }
                catch
                {
                    return Content("<script>alert('输入有误!请检查数据格式');location.href='javascript:history.go(-1)';</script>");
                }
                //CreateIndex(model, "/Lucene.Net/IndexDic/GongGao");
            }
            return View("Procure_TzList");
        }
        public class DepartmentAddModel
        {
            public 单位用户 u { get; set; }
            //public bool 管理 { get; set; }
            //public bool 监审 { get; set; }
            //public bool 采购 { get; set; }
            //public bool 需求 { get; set; }

            [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "*请填写登录名")]
            [System.ComponentModel.DataAnnotations.StringLength(30, MinimumLength = 6, ErrorMessage = "长度为6-30")]
            [System.ComponentModel.DataAnnotations.RegularExpression(@"^[a-zA-Z]\w+$", ErrorMessage = "以字母开头(可用字母、数字或_)")]
            [System.Web.Mvc.Remote("CheckUserName", "注册", ErrorMessage = "该用户名已存在")]
            public string loginName { get; set; }

            [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "单位编码必须填写")]
            [System.Web.Mvc.Remote("CheckUnitCode", "运营团队后台", ErrorMessage = "该单位编码已经存在,请重新填写")]
            public string unitCode { get; set; }

        }
        public ActionResult Check()
        {
            try
            {
                long id = long.Parse(Request.QueryString["id"]);
                string time = Request.QueryString["time"];
                供应商 model = 用户管理.查找用户<供应商>(id);
                if (time == "1")
                {
                    model.供应商用户信息.初审数据.审核状态 = 审核状态.审核通过;
                    model.供应商用户信息.初审数据.审核时间 = DateTime.Now;
                    var are = model.所属地域;
                    model.供应商用户信息.初审数据.审核者.用户ID = 11;
                    if (are != null)
                    {
                        var pro = are.省份;
                        switch (pro)
                        {
                            case "重庆市":
                                model.供应商用户信息.初审数据.审核者.用户ID = 12;
                                break;
                            case "云南省":
                                model.供应商用户信息.初审数据.审核者.用户ID = 13;
                                break;
                            case "贵州省":
                                model.供应商用户信息.初审数据.审核者.用户ID = 14;
                                break;
                            case "西藏自治区":
                                model.供应商用户信息.初审数据.审核者.用户ID = 15;
                                break;
                        }
                    }
                    用户管理.更新用户<供应商>(model, false);
                    return Content("<script>alert('初审通过！');window.location='/运营团队后台/Modify_Supplier_Info?id=" + id.ToString() + "';</script>");
                }
                else
                {
                    model.供应商用户信息.初审数据.审核状态 = 审核状态.审核通过;
                    model.供应商用户信息.初审数据.审核时间 = DateTime.Now;
                    
                    model.供应商用户信息.复审数据.审核者.用户ID = 10002;
                    model.供应商用户信息.复审数据.审核状态 = 审核状态.审核通过;
                    model.供应商用户信息.复审数据.审核时间 = DateTime.Now;
                    var are = model.所属地域;

                    model.供应商用户信息.初审数据.审核者.用户ID = 11;
                    if (are != null)
                    {
                        var pro = are.省份;
                        switch (pro)
                        {
                            case "重庆市":
                                model.供应商用户信息.初审数据.审核者.用户ID = 12;
                                break;
                            case "云南省":
                                model.供应商用户信息.初审数据.审核者.用户ID = 13;
                                break;
                            case "贵州省":
                                model.供应商用户信息.初审数据.审核者.用户ID = 14;
                                break;
                            case "西藏自治区":
                                model.供应商用户信息.初审数据.审核者.用户ID = 15;
                                break;
                        }
                    }
                    用户管理.更新用户<供应商>(model, false);
                    return Content("<script>alert('复审通过！');window.location='/运营团队后台/Modify_Supplier_Info?id=" + id.ToString() + "';</script>");
                }
            }
            catch
            {
                return Redirect("/运营团队后台/Supplier_PssInfo");
            }
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

            bool IsTrue = false;
            if (ModelState.IsValidField("u.单位信息.单位代号") && ModelState.IsValidField("u.登录信息.密码"))
            {
                IsTrue = true;
            }
            if (IsTrue)
            {
                var usergroup = Request.Form["usergroup"];
                if (!string.IsNullOrWhiteSpace(usergroup))
                {
                    var group = usergroup.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var name in group)
                    {
                        model.u.用户组.Add(name);
                    }
                }

                var p = Request["deliverprovince"];
                var c = Request["delivercity"];
                var a = Request["deliverarea"];
                model.u.所属地域.省份 = p;
                model.u.所属地域.城市 = c;
                model.u.所属地域.区县 = a;

                var gldw = Request.Form["gldw"];
                switch (gldw)
                {
                    case "成都":
                        model.u.管理单位.用户ID = 16;
                        break;
                    case "重庆":
                        model.u.管理单位.用户ID = 12;
                        break;
                    case "云南":
                        model.u.管理单位.用户ID = 13;
                        break;
                    case "贵州":
                        model.u.管理单位.用户ID = 14;
                        break;
                    case "西藏":
                        model.u.管理单位.用户ID = 15;
                        break;
                    default:
                        model.u.管理单位.用户ID = 16;
                        break;
                }
                //if (Request.Form["admin"] != "-1")
                //{
                //    model.u.所属单位.用户ID = long.Parse(Request.Form["admin"]);
                //}
                model.u.审核数据.审核状态 = 审核状态.审核通过;
                model.u.登录信息.登录名 = model.loginName;
                model.u.单位信息.单位编码 = model.unitCode;
                用户管理.添加单位用户(model.u, null);
            }
            return RedirectToAction("DepartmentList", "运营团队后台");
        }

        /// <summary>
        /// 上传新闻图片
        /// </summary>
        /// <param name="fileData"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult UploadAttachment_news(HttpPostedFileBase fileData)
        {
            if (fileData != null)
            {
                string filePath = 上传管理.获取上传路径<新闻>(媒体类型.图片, 路径类型.服务器本地路径);

                //string extension = fileData.FileName.Substring(fileData.FileName.LastIndexOf("."), (fileData.FileName.Length - fileData.FileName.LastIndexOf(".")));

                string fileName = fileData.FileName;
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
                return Content(string.Format("{0}{1}", 上传管理.获取上传路径<新闻>(媒体类型.图片, 路径类型.不带域名根路径), fileName));
            }
            else
            {
                return Content("0");
            }
        }
        /// <summary>
        /// 上传政策法规文档
        /// </summary>
        /// <param name="fileData"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult UploadAttachment_Zcfg(HttpPostedFileBase fileData)
        {
            if (fileData != null)
            {
                string filePath = 上传管理.获取上传路径<政策法规>(媒体类型.附件, 路径类型.服务器本地路径);

                //string extension = fileData.FileName.Substring(fileData.FileName.LastIndexOf("."), (fileData.FileName.Length - fileData.FileName.LastIndexOf(".")));

                string fileName = fileData.FileName;
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
                return Content(string.Format("{0}{1}", 上传管理.获取上传路径<政策法规>(媒体类型.附件, 路径类型.不带域名根路径), fileName));
            }
            else
            {
                return Content("0");
            }
        }

        /// <summary>
        /// 上传公告附件
        /// </summary>
        /// <param name="fileData"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult UploadAttachment_Ad(HttpPostedFileBase fileData)
        {
            if (fileData != null)
            {
                string filePath = 上传管理.获取上传路径<公告>(媒体类型.附件, 路径类型.服务器本地路径);

                //string extension = fileData.FileName.Substring(fileData.FileName.LastIndexOf("."), (fileData.FileName.Length - fileData.FileName.LastIndexOf(".")));

                string fileName = fileData.FileName;
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
                return Content(string.Format("{0}{1}", 上传管理.获取上传路径<公告>(媒体类型.附件, 路径类型.不带域名根路径), fileName));
            }
            else
            {
                return Content("0");
            }
        }
        /// <summary>
        /// 上传公告扫描件
        /// </summary>
        /// <param name="fileData"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult UploadAttachment_Ad_s(HttpPostedFileBase fileData)
        {
            if (fileData != null)
            {
                string filePath = 上传管理.获取上传路径<公告>(媒体类型.图片, 路径类型.服务器本地路径);
                string fileName = fileData.FileName;
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
                return Content(string.Format("{0}{1}", 上传管理.获取上传路径<公告>(媒体类型.图片, 路径类型.不带域名根路径), fileName));
            }
            else
            {
                return Content("0");
            }
        }


        /// <summary>
        /// 上传公告附件
        /// </summary>
        /// <param name="fileData"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult UploadAttachment_Tz(HttpPostedFileBase fileData)
        {
            if (fileData != null)
            {
                string filePath = 上传管理.获取上传路径<通知>(媒体类型.附件, 路径类型.服务器本地路径);

                //string extension = fileData.FileName.Substring(fileData.FileName.LastIndexOf("."), (fileData.FileName.Length - fileData.FileName.LastIndexOf(".")));

                string fileName = fileData.FileName;
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
                return Content(string.Format("{0}{1}", 上传管理.获取上传路径<通知>(媒体类型.附件, 路径类型.不带域名根路径), fileName));
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
                //IndexWriter writer = new IndexWriter(Indexdic_Server, PanGuAnalyzer, false, Lucene.Net.Index.IndexWriter.MaxFieldLength.LIMITED);
                IndexWriter writer = new IndexWriter(dir, PanGuAnalyzer, false, IndexWriter.MaxFieldLength.UNLIMITED);
                GG_AddIndex(writer, g);

                writer.Optimize();
                writer.Close();
            }
            catch
            {
                //IndexWriter writer = new IndexWriter(Indexdic_Server, PanGuAnalyzer, true, Lucene.Net.Index.IndexWriter.MaxFieldLength.LIMITED);
                IndexWriter writer = new IndexWriter(dir, PanGuAnalyzer, true, IndexWriter.MaxFieldLength.UNLIMITED);
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
                //IndexWriter writer = new IndexWriter(Indexdic_Server, PanGuAnalyzer, false, Lucene.Net.Index.IndexWriter.MaxFieldLength.LIMITED);
                IndexWriter writer = new IndexWriter(dir, PanGuAnalyzer, false, IndexWriter.MaxFieldLength.UNLIMITED);
                AddIndex(writer, model.Id.ToString(), model.内容主体.标题, model.内容主体.内容, model.内容主体.发布时间.ToString());

                writer.Optimize();
                writer.Close();
            }
            catch
            {
                //IndexWriter writer = new IndexWriter(Indexdic_Server, PanGuAnalyzer, true, Lucene.Net.Index.IndexWriter.MaxFieldLength.LIMITED);
                IndexWriter writer = new IndexWriter(dir, PanGuAnalyzer, true, IndexWriter.MaxFieldLength.UNLIMITED);
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

        protected void updateIndex(string path, 内容基本数据 model)
        {
            string Indexdic_Server = IndexDic(path);
            IndexWriter writer = null;
            Lucene.Net.Store.Directory dir = new Lucene.Net.Store.SimpleFSDirectory(new DirectoryInfo(Indexdic_Server));
            try
            {
                writer = new IndexWriter(dir, PanGuAnalyzer, false, IndexWriter.MaxFieldLength.UNLIMITED);
                //writer = new IndexWriter(Indexdic_Server, PanGuAnalyzer, false, Lucene.Net.Index.IndexWriter.MaxFieldLength.LIMITED);
            }
            catch
            {
                writer = new IndexWriter(dir, PanGuAnalyzer, true, IndexWriter.MaxFieldLength.UNLIMITED);
                //writer = new IndexWriter(Indexdic_Server, PanGuAnalyzer, true, Lucene.Net.Index.IndexWriter.MaxFieldLength.LIMITED);
            }

            writer.DeleteDocuments(new Term("NumId", model.Id.ToString()));//删除一条索引
            writer.Optimize();
            writer.Close();

            CreateIndex(model, path);

        }

        ///////////////////////////////////////////////////////////////////////////////////////////////////////从单位用户转移代码

        public ActionResult Part_AddSupplier_Info(string id)
        {
            try
            {
                ViewData["行业列表"] = 商品分类管理.查找子分类();
                if (id == null)
                {
                    供应商 model = new 供应商();
                    return PartialView("Part_View/Part_AddSupplier_Info", model);
                }
                else
                {
                    var model = 用户管理.查找用户<供应商>(long.Parse(id));
                    if (model != null)
                    {
                        return PartialView("Part_View/Part_AddSupplier_Info", model);
                    }
                    else
                    {
                        供应商 models = new 供应商();
                        return PartialView("Part_View/Part_AddSupplier_Info", models);
                    }

                }
            }
            catch (Exception)
            {
                供应商 model = new 供应商();
                return PartialView("Part_View/Part_AddSupplier_Info", model);
            }
        }
        public ActionResult Add_Supplier_Info()
        {
            return View();
        }

        public long User_exist()
        {
            string name = Request.QueryString["name"];
            long id = 用户管理.检查用户是否存在(name);
            if (id != -1)
            {
                return id;
            }
            else
            {

                return -1;
            }
        }
        [HttpPost]
        public ActionResult AddSupplier(供应商 model)
        {
            bool IsTrue = true;
            try
            {
                if (用户管理.检查用户是否存在<供应商>(model.登录信息.登录名, true, true) != -1)
                {
                    return Content("<script>alert('该用户已存在，请更换登录名！');window.location='/运营团队后台/Add_Supplier_Info';</script>");
                }
                else
                {
                    if (IsTrue)
                    {
                        if (Request.Form["nianshen"] != null)
                        {
                            var ns = short.Parse(Request.Form["nianshen"]);
                            if (ns == 1)
                            {
                                var d = DateTime.Now;
                                model.供应商用户信息.年检列表.Add(d.Year.ToString(), new 操作数据() { 操作时间 = d });
                            }
                        }
                        for (int i = 0; i < Request.Files.Count; i++)
                        {
                            HttpPostedFileBase file = Request.Files[i];
                            if (file != null && ((file.ContentLength / 1024) / 1024) < 2 && file.ContentType == "image/jpeg")
                            {
                                string filePath = 上传管理.获取上传路径<供应商>(媒体类型.图片, 路径类型.不带域名根路径);
                                string fname = UploadAttachment(file);
                                string file_name = filePath + fname;
                                switch (i)
                                {
                                    case 0: model.供应商用户信息.供应商图片.Add(file_name); break;
                                    case 1: model.工商注册信息.组织机构代码证电子扫描件 = file_name; break;
                                    case 2: model.工商注册信息.基本账户开户银行资信证明电子扫描件 = file_name; break;
                                    case 3: model.工商注册信息.近3年缴纳社会保证金证明电子扫描件 = file_name; break;
                                    case 4: model.工商注册信息.近3年有无重大违法记录电子扫描件 = file_name; break;
                                    case 5: model.营业执照信息.营业执照电子扫描件 = file_name; break;
                                    case 6: model.法定代表人信息.法定代表人身份证电子扫描件 = file_name; break;
                                    case 7: model.税务信息.近3年完税证明电子扫描件 = file_name; break;
                                }
                            }
                        }
                        model.审核数据.审核状态 = 审核状态.审核通过;
                        用户管理.添加用户<供应商>(model);
                        deleteIndex("/Lucene.Net/IndexDic/Gys", model.Id.ToString());
                        CreateIndex_gys(用户管理.查找用户<供应商>(model.Id), "/Lucene.Net/IndexDic/Gys");
                        //deleteIndex_ProductCatalog("/Lucene.Net/IndexDic/GysCatalog", model.企业基本信息.企业名称);
                        //CreateIndex_ProductCatalog(用户管理.查找用户<供应商>(model.Id).企业基本信息.企业名称, "/Lucene.Net/IndexDic/GysCatalog");
                        return Redirect("/运营团队后台/Add_Supplier_Info?id=" + model.Id.ToString());
                    }
                    else
                    {
                        return Redirect("/运营团队后台/Add_Supplier_Info");
                    }
                }

            }
            catch
            {
                return Redirect("/运营团队后台/Add_Supplier_Info");
            }

        }
        public string UploadAttachment(HttpPostedFileBase fileData)
        {
            if (fileData != null)
            {
                string fileName = fileData.FileName;
                if (fileData.FileName != "" && fileData.FileName != null)
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
        public ActionResult Part_Home(int? page)
        {
            IEnumerable<站内消息> Msg = 站内消息管理.查询收信人的站内消息(0, 0, currentUser.Id);
            int count = 0;
            foreach (var item in Msg)
            {
                if ((item.基本数据.添加时间 > item.收信人.上次阅读时间) && item.发起者.用户数据 != null)
                {
                    if (item.对话消息 != null && item.对话消息.Count() != 0)
                    {
                        count++;
                    }
                }
            }
            ViewData["tickets"] = 机票代售点管理.计数机票代售点(0, 0, Query.EQ("审核数据.审核状态", 审核状态.未审核));
            ViewData["hotel"] = 酒店管理.计数酒店(0, 0, Query.EQ("审核数据.审核状态", 审核状态.未审核));
            //待审核供应商数
            ViewData["suplier"] = 用户管理.计数用户<供应商>(0, 0, Query<供应商>.Where(o => o.审核数据.审核状态 == 审核状态.未审核 && o.供应商用户信息.已提交));
            //待审核商品数
            var l = 用户管理.列表用户<供应商>(0, 0,
               Fields<供应商>.Include(o => o.Id),
               Query<供应商>.EQ(o => o.审核数据.审核状态, 审核状态.审核通过)
               );
            var q = Query.And(
                    Query.In("商品信息.所属供应商.用户ID", l.Select(o => o["_id"])),
                    Query.EQ("审核数据.审核状态", 审核状态.未审核)
                );
            ViewData["pro_count"] = 商品管理.计数商品(0, 0, q);

            //待添加属性的商品分类
            IEnumerable<商品分类> pro_class = Mongo.查询<商品分类>(0, 0, null, includeDisabled: false);
            IList<商品分类> pro_class_rel = new List<商品分类>();
            foreach (var item in pro_class)
            {
                if (!item.子分类.Any() && !item.商品属性模板.Any())
                {

                    pro_class_rel.Add(item);
                }
            }
            ViewData["pro_class"] = pro_class_rel.Count;



            ViewData["msg_count"] = count;
            ViewData["user"] = currentUser.登录信息.登录名;
            return PartialView("Part_View/Part_Home");
        }
        public ActionResult Index()
        {
            return View();
        }
        public long Save_MsgTemp()
        {
            try
            {
                int catorgray = int.Parse(Request.QueryString["types"].ToString());
                string title = Request.QueryString["title"].ToString();
                string content = Request.QueryString["con"].ToString();
                if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(content))
                {
                    return 0;
                }
                else
                {
                    推送模板 MsgTemp = new 推送模板();
                    MsgTemp.标题 = title;
                    MsgTemp.内容 = content;
                    MsgTemp.推送类型 = (推送类型)catorgray;
                    推送模板管理.添加推送模板(MsgTemp);
                    return MsgTemp.Id;
                }
            }
            catch
            {
                return 0;
            }


        }
        public string GetText()
        {
            try
            {
                long id = long.Parse(Request.QueryString["Id"].ToString());
                推送模板 model = 推送模板管理.查找推送模板(id);
                string str = model.标题 + ",";
                str += model.内容 + ",";
                str += ((int)model.推送类型).ToString();
                return str;
            }
            catch
            {
                return "获取内容失败！";
            }

        }
        public long Modify_MsgTemp()
        {
            try
            {
                long id = long.Parse(Request.QueryString["Id"].ToString());
                int catorgray = int.Parse(Request.QueryString["types"].ToString());
                string title = Request.QueryString["title"].ToString();
                string content = Request.QueryString["con"].ToString();
                if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(content))
                {
                    return 0;
                }
                else
                {
                    推送模板 MsgTemp = new 推送模板();
                    MsgTemp.Id = id;
                    MsgTemp.标题 = title;
                    MsgTemp.内容 = content;
                    MsgTemp.推送类型 = (推送类型)catorgray;
                    推送模板管理.更新推送模板(MsgTemp);
                    return MsgTemp.Id;
                }
            }
            catch
            {
                return 0;
            }

        }
        public ActionResult Del_MsgTemp()
        {
            try
            {
                long id = long.Parse(Request.QueryString["id"].ToString());
                推送模板管理.删除推送模板(id);
            }
            catch
            {
                return Redirect("/运营团队后台/Msg_TemplateManage");
            }
            return Redirect("/运营团队后台/Msg_TemplateManage");
        }
        public ActionResult Part_MsgTemplate(int? page)
        {
            try
            {
                if (string.IsNullOrEmpty(page.ToString()))
                {
                    page = 1;
                }
                ViewData["listcount"] = 推送模板管理.计数推送模板(0, 0);
                ViewData["pagesize"] = 20;
                ViewData["MsgTemplate"] = 推送模板管理.查询推送模板(20 * (int.Parse(page.ToString()) - 1), 20);
            }
            catch
            {
                return PartialView("/运营团队后台/Msg_TemplateManage");
            }
            return PartialView("Part_View/Part_MsgTemplate");
        }
        public ActionResult Msg_TemplateManage()
        {
            return View();
        }
        public void 消息查询(int? page, int pagesize, int listcount, int maxpage, string str)
        {
            if (string.IsNullOrEmpty(page.ToString()) || page < 0 || page > maxpage)
            {
                page = 1;
            }

            ViewData["listcount"] = listcount;

            ViewData["pagesize"] = pagesize;
            switch (str)
            {
                case "系统通知":
                    ViewData["系统通知列表"] = 通知管理.查询通知(pagesize * (int.Parse(page.ToString()) - 1), pagesize);
                    break;
                case "站内消息":
                    ViewData["站内消息列表"] = 站内消息管理.查询站内消息(pagesize * (int.Parse(page.ToString()) - 1), pagesize);
                    break;
                case "已处理投诉":
                    ViewData["已处理投诉列表"] = 投诉管理.查询投诉(pagesize * (int.Parse(page.ToString()) - 1), pagesize, Query.EQ("处理状态", 处理状态.已处理));
                    break;
                case "未处理投诉":
                    ViewData["未处理投诉列表"] = 投诉管理.查询投诉(pagesize * (int.Parse(page.ToString()) - 1), pagesize, Query.EQ("处理状态", 处理状态.未处理));
                    break;
                case "已回复建议":
                    ViewData["已回复建议列表"] = 建议管理.查询建议(pagesize * (int.Parse(page.ToString()) - 1), pagesize, Query.EQ("处理状态", 处理状态.已处理));
                    break;
                case "未回复建议":
                    ViewData["未回复建议列表"] = 建议管理.查询建议(pagesize * (int.Parse(page.ToString()) - 1), pagesize, Query.EQ("处理状态", 处理状态.未处理));
                    break;
            }
        }
        public ActionResult SystemInfo() //系统通知
        {
            return View();
        }
        public ActionResult SystemInfo_Detail() //系统通知详情
        {
            return View();
        }
        public ActionResult ZNXX() //站内消息
        {
            return View();
        }
        public ActionResult ZNXX_Sended() //站内消息-已发送
        {
            return View();
        }
        public ActionResult ZNXX_Add() //站内消息-发送
        {
            return View();
        }
        public ActionResult GoodClassify() //商品目录管理
        {
            return View();
        }
        public 商品分类 SelectClassById()
        {
            string id = Request.Params["id"];
            商品分类 goodclass = 商品分类管理.查找分类(long.Parse(id));
            return goodclass;
        }

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        public void 新建商品目录()
        {
            string parid = Request.Params["parid"];//父分类id
            string text = !string.IsNullOrWhiteSpace(Request.Params["text"])
                ? Request.Params["text"].Trim()
                : Request.Params["text"];  // 商品分类名称
            string IsPost = !string.IsNullOrWhiteSpace(Request.QueryString["ispost"])
                ? Request.QueryString["ispost"].Trim()
                : Request.QueryString["ispost"]; //是否属于协议采购
            string IsFirst = !string.IsNullOrWhiteSpace(Request.QueryString["isfirst"])
                ? Request.QueryString["isfirst"].Trim()
                : Request.QueryString["isfirst"]; //是否属于紧急采购
            string Num = !string.IsNullOrWhiteSpace(Request.Params["num"])
                ? Request.Params["num"].Trim()
                : Request.Params["num"];//分类序号
            var isZhuanyong = Request.Params["iszhuanyong"];
            bool agreement = false;
            bool emergacy = false;
            bool normal = true;
            bool isSpecial = false;

            if (!string.IsNullOrWhiteSpace(text) && !string.IsNullOrWhiteSpace(Num))
            {
                商品分类 GoodClass = new 商品分类();
                GoodClass.分类名 = text;
                if (!string.IsNullOrWhiteSpace(parid))
                {
                    GoodClass.父分类.商品分类ID = long.Parse(parid);
                }
                if (isZhuanyong == "1")
                {
                    //GoodClass.分类性质 = 商品分类性质.专用物资;
                    isSpecial = true;
                }
                else
                {
                    isSpecial = false;
                    //GoodClass.分类性质 = 商品分类性质.通用物资;
                }
                GoodClass.序号 = int.Parse(Num);

                if (IsFirst == "true")
                {
                    emergacy = true;
                    normal = false;
                }
                if (IsPost == "true")
                {
                    agreement = true;
                    normal = false;
                }
                商品分类管理.添加分类(GoodClass.分类名, GoodClass.父分类.商品分类ID, isSpecial, GoodClass.序号, normal, agreement, emergacy);
            }
            else
            {
                throw new Exception("请检查信息是否完整！");
            }
        }
        public void 重命名分类()
        {
            string parid = Request.Params["parid"];//分类id

            string text = !string.IsNullOrWhiteSpace(Request.Params["text"])
                ? Request.Params["text"].Trim()
                : Request.Params["text"];  // 新的商品分类名称

            string IsPost = !string.IsNullOrWhiteSpace(Request.QueryString["ispost"])
                ? Request.QueryString["ispost"].Trim()
                : Request.QueryString["ispost"]; //是否属于协议采购

            string IsFirst = !string.IsNullOrWhiteSpace(Request.QueryString["isfirst"])
                ? Request.QueryString["isfirst"].Trim()
                : Request.QueryString["isfirst"]; //是否属于紧急采购

            string Num = !string.IsNullOrWhiteSpace(Request.Params["numname"])
                ? Request.Params["numname"].Trim()
                : Request.Params["numname"]; //商品分类序号

            if (!string.IsNullOrWhiteSpace(text) && !string.IsNullOrWhiteSpace(Num))
            {
                商品分类 classify = 商品分类管理.查找分类(long.Parse(parid));
                classify.分类名 = text;
                classify.Id = long.Parse(parid);
                classify.序号 = int.Parse(Num);
                if (IsFirst == "true")
                {
                    classify.应急采购 = true;
                    classify.普通采购 = false;
                }
                else
                {
                    classify.应急采购 = false;
                    classify.普通采购 = true;
                }
                if (IsPost == "true")
                {
                    classify.协议采购 = true;
                    classify.普通采购 = false;
                }
                else
                {
                    classify.协议采购 = false;
                    if (classify.应急采购)
                    {
                        classify.普通采购 = false;
                    }
                    else
                    {
                        classify.普通采购 = true;
                    }
                }
                商品分类管理.更新分类(classify);
            }
            else
            {
                throw new Exception("请检查信息是否完整!");
            }
        }
        public string 删除商品目录()
        {
            string a = Request.Params["a"];
            商品分类 商品 = 商品分类管理.查找分类(long.Parse(a));
            if (!商品.子分类.Any())
            {
                商品分类管理.删除分类(long.Parse(a));
                return "删除成功！";
            }
            else
            {
                return "请先删除其子分类！";
            }
        }
        public ActionResult GoodAttrManage()//商品属性管理
        {
            return View();
        }
        [HttpPost]
        public void CreateAttr()
        {
            string id = Request.Params["id"];   //商品分类id
            string sxfnname = !string.IsNullOrWhiteSpace(Request.Params["sxfnname"])
                ? Request.Params["sxfnname"].Trim()
                : Request.Params["sxfnname"]; //商品属性分类名

            string sxname = !string.IsNullOrWhiteSpace(Request.Params["sxname"])
                ? Request.Params["sxname"].Trim()
                : Request.Params["sxname"]; //商品属性名

            string sxzval = !string.IsNullOrWhiteSpace(Request.Params["sxzname"])
                ? Request.Params["sxzname"].Trim()
                : Request.Params["sxzname"]; // 商品属性值名

            string create = Request.Params["create"];//判断什么操作
            string issale = Request.Params["isSale"];//是否为销售属性
            string attrtype = Request.Params["attr_type"];//属性类型：复选、输入
            var b = 商品分类管理.查找分类(long.Parse(id));
            switch (create)
            {
                case "create_sxfn":
                    if (!string.IsNullOrWhiteSpace(sxfnname))
                    {
                        b.商品属性模板.Add(sxfnname, new Dictionary<string, 商品属性数据>());
                        商品分类管理.更新分类(b);
                    }
                    else
                    {
                        throw new Exception("请检查信息是否完整!");
                    }
                    break;
                case "create_sx":
                    if (!string.IsNullOrWhiteSpace(sxfnname) && !string.IsNullOrWhiteSpace(sxname))
                    {
                        b.商品属性模板[sxfnname].Add(sxname, new 商品属性数据());
                        商品分类管理.更新分类(b);
                        if (issale == "true")
                        {
                            b.商品属性模板[sxfnname][sxname].销售属性 = true;
                        }
                        else { b.商品属性模板[sxfnname][sxname].销售属性 = false; }
                        switch (attrtype)
                        {
                            case "0":
                                b.商品属性模板[sxfnname][sxname].属性类型 = 属性类型.复选;
                                break;
                            case "1":
                                b.商品属性模板[sxfnname][sxname].属性类型 = 属性类型.输入;
                                break;
                        }

                        商品分类管理.更新分类(b);
                    }
                    else
                    {
                        throw new Exception("请检查信息是否完整!");
                    }
                    break;
                case "create_sxz":
                    if (!string.IsNullOrWhiteSpace(sxfnname) && !string.IsNullOrWhiteSpace(sxname) && !string.IsNullOrWhiteSpace(sxzval))
                    {
                        b.商品属性模板[sxfnname][sxname].值.Add(sxzval);
                        商品分类管理.更新分类(b);
                    }
                    else
                    {
                        throw new Exception("请检查信息是否完整!");
                    }
                    break;
            }
        }

        public void EditAttr()
        {
            string id = Request.Params["id"];//商品分类id
            string oldname = !string.IsNullOrWhiteSpace(Request.Params["oldname"])
                ? Request.Params["oldname"].Trim()
                : Request.Params["oldname"]; //商品属性值旧名称


            string newname = !string.IsNullOrWhiteSpace(Request.Params["newname"])
                ? Request.Params["newname"].Trim()
                : Request.Params["newname"];//商品属性值新名称

            string sxfnname = !string.IsNullOrWhiteSpace(Request.Params["sxfnname"])
                ? Request.Params["sxfnname"].Trim()
                : Request.Params["sxfnname"];//商品属性分类名

            string sxname = !string.IsNullOrWhiteSpace(Request.Params["sxname"])
                ? Request.Params["sxname"].Trim()
                : Request.Params["sxname"];//商品属性名
            string edit = Request.Params["edit"];//
            string attrType = Request.Params["attrtypes"];//属性类型
            string isSale = Request.Params["issales"];//是否为销售属性

            switch (edit)
            {
                case "edit_sxfn":
                    if (!string.IsNullOrWhiteSpace(oldname) && !string.IsNullOrWhiteSpace(newname))
                    {
                        商品分类管理.重命名模板属性分类(long.Parse(id), oldname, newname);
                    }
                    else
                    {
                        throw new Exception("请检查信息是否完整!");
                    }
                    break;
                case "edit_sx":
                    if (!string.IsNullOrWhiteSpace(sxfnname) && !string.IsNullOrWhiteSpace(oldname) && !string.IsNullOrWhiteSpace(newname))
                    {
                        商品分类 g = 商品分类管理.查找分类(long.Parse(id));
                        if (isSale == "true")
                        {
                            g.商品属性模板[sxfnname][oldname].销售属性 = true;
                        }
                        else { g.商品属性模板[sxfnname][oldname].销售属性 = false; }
                        switch (attrType)
                        {
                            case "0":
                                g.商品属性模板[sxfnname][oldname].属性类型 = 属性类型.复选;
                                break;
                            case "1":
                                g.商品属性模板[sxfnname][oldname].属性类型 = 属性类型.输入;
                                break;
                        }
                        商品分类管理.更新分类(g);
                        商品分类 f = 商品分类管理.查找分类(long.Parse(id));

                        商品属性数据 z = new 商品属性数据();
                        z.必需 = f.商品属性模板[sxfnname][oldname].必需;
                        z.频率 = f.商品属性模板[sxfnname][oldname].频率;
                        z.属性类型 = f.商品属性模板[sxfnname][oldname].属性类型;
                        z.销售属性 = f.商品属性模板[sxfnname][oldname].销售属性;
                        if (f.商品属性模板[sxfnname][oldname].属性类型 == 属性类型.输入)
                        {
                            f.商品属性模板[sxfnname][oldname].值.Clear();
                        }
                        z.值 = f.商品属性模板[sxfnname][oldname].值;
                        f.商品属性模板[sxfnname].Remove(oldname);
                        f.商品属性模板[sxfnname].Add(newname, z);
                        商品分类管理.更新分类(f);
                    }
                    else
                    {
                        throw new Exception("请检查信息是否完整!");
                    }
                    break;
                case "edit_sxz":
                    if (!string.IsNullOrWhiteSpace(sxfnname) && !string.IsNullOrWhiteSpace(oldname) && !string.IsNullOrWhiteSpace(newname))
                    {
                        var d = 商品分类管理.查找分类(long.Parse(id));
                        d.商品属性模板[sxfnname][sxname].值.Remove(oldname);
                        d.商品属性模板[sxfnname][sxname].值.Add(newname);
                        商品分类管理.更新分类(d);
                    }
                    else
                    {
                        throw new Exception("请检查信息是否完整!");
                    }
                    break;
            }
        }

        public void DeleteAttr()
        {
            string id = Request.Params["id"];   //商品分类id

            string sxfnname = !string.IsNullOrWhiteSpace(Request.Params["sxfnname"])
                ? Request.Params["sxfnname"].Trim()
                : Request.Params["sxfnname"];//商品属性分类名

            string sxname = !string.IsNullOrWhiteSpace(Request.Params["sxname"])
                ? Request.Params["sxname"].Trim()
                : Request.Params["sxname"]; //商品属性名

            string sxzval = !string.IsNullOrWhiteSpace(Request.Params["sxzname"])
                ? Request.Params["sxzname"].Trim()
                : Request.Params["sxzname"]; // 商品属性值名

            string delete = Request.Params["del"]; //操作类别
            var a = 商品分类管理.查找分类(long.Parse(id));
            switch (delete)
            {
                case "del_sxfn":
                    a.商品属性模板.Remove(sxfnname);
                    商品分类管理.更新分类(a);
                    break;
                case "del_sx":
                    a.商品属性模板[sxfnname].Remove(sxname);
                    商品分类管理.更新分类(a);
                    break;
                case "del_sxz":
                    a.商品属性模板[sxfnname][sxname].值.Remove(sxzval);
                    商品分类管理.更新分类(a);
                    break;
            }
        }

        public JsonResult GetAttrClass()
        {
            string xid = Request.Params["xid"];
            商品分类 spfnlist = 商品分类管理.查找分类(long.Parse(xid));
            List<string> li = new List<string>();
            foreach (var f in spfnlist.商品属性模板.Keys)
            {
                li.Add(f);
            }
            var json = new JsonResult() { Data = li };
            return Json(json, JsonRequestBehavior.AllowGet);
        }

        private class Attr
        {
            /// <summary>
            /// 属性类型
            /// </summary>
            public string AttrType { get; set; }
            /// <summary>
            /// 销售属性
            /// </summary>
            public bool SalesAttr { get; set; }
            /// <summary>
            /// 属性名称
            /// </summary>
            public string AttrName { get; set; }
        }
        public JsonResult SelectAttr()
        {
            string id = Request.Params["id"];   //商品分类id
            string sxfnname = !string.IsNullOrWhiteSpace(Request.Params["sxclass"])
                ? Request.Params["sxclass"].Trim()
                : Request.Params["sxclass"];//属性分类名
            商品分类 good = 商品分类管理.查找分类(long.Parse(id));
            List<string> list = new List<string>();
            List<Attr> attr = new List<Attr>();
            foreach (var c in good.商品属性模板[sxfnname].Keys)
            {
                list.Add(c);
            }
            foreach (var h in list)
            {
                attr.Add(new Attr()
                {
                    AttrType = good.商品属性模板[sxfnname][h].属性类型.ToString(),
                    SalesAttr = good.商品属性模板[sxfnname][h].销售属性,
                    AttrName = h,
                });
                //str += "<option value=" + good.商品属性模板[sxfnname][h].属性类型 + " name=" + good.商品属性模板[sxfnname][h].销售属性 + ">" + h + "</option>";
            }
            JsonResult json = new JsonResult() { Data = attr };
            return Json(json, JsonRequestBehavior.AllowGet);
        }
        public JsonResult SelectAttrValue()
        {
            string str = string.Empty;
            string id = Request.Params["id"];   //商品分类id
            string sxname = Request.Params["sxname"]; //商品属性名
            string sxfnname = Request.Params["sxfnname"];
            商品分类 good = 商品分类管理.查找分类(long.Parse(id));
            List<string> list = good.商品属性模板[sxfnname][sxname].值;
            //foreach (var d in list)
            //{
            //    str += "<option>" + d + "</option>";
            //}
            JsonResult json = new JsonResult() { Data = list };
            return Json(json, JsonRequestBehavior.AllowGet);
        }
        public ActionResult GoodExamine()//商品审核_待审核
        {
            //var x = 用户管理.查找用户<供应商>(200000000003);
            //x.供应商用户信息.U盾信息.序列号 = "f75f2b344230e3e40102549baffaa943";
            //x.供应商用户信息.U盾信息.加密参数 = "FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF";
            //用户管理.更新用户<供应商>(x, false);

            //List<string> catlog = new List<string>();
            //List<string> catlogdata = new List<string>();
            //var Count1 = 0;
            //IEnumerable<商品> sp = 商品管理.查询商品(0, 0, Query<商品>.Where(o => o.审核数据.审核状态 == 审核状态.审核通过), includeDisabled: false);
            //foreach (var s in sp)
            //{
            //    CreateIndex(s, "/Lucene.Net/IndexDic/Product");
            //    Count1++;
            //    //if (s.商品信息 != null && !string.IsNullOrWhiteSpace(s.商品信息.精确型号))
            //    //{
            //    //    catlog.Add(s.商品信息.精确型号);
            //    //}
            //}
            //var Count = 0;
            //IEnumerable<供应商> gys = 用户管理.查询用户<供应商>(0, 0, Query<供应商>.Where(o => o.审核数据.审核状态 == 审核状态.审核通过), includeDisabled: false);
            //foreach (var g in gys)
            //{
            //    //CreateIndex_ProductCatalog(g.企业基本信息.企业名称, "/Lucene.Net/IndexDic/GysCatalog");
            //    CreateIndex_gys(g, "/Lucene.Net/IndexDic/Gys");
            //    Count++;
            //}

            //foreach (var g in catlog)
            //{
            //    if (!catlogdata.Contains(g))
            //    {
            //        catlogdata.Add(g); ;
            //    }
            //}

            //foreach (var g in catlogdata)
            //{
            //    CreateIndex_ProductCatalog(g, "/Lucene.Net/IndexDic/ProductCatalog");
            //}

            //IEnumerable<商品分类> ProductCatalog = 商品分类管理.查询商品分类(0, 0).Where(o => o.子分类.Count() == 0);
            //foreach (var g in ProductCatalog)
            //{
            //    CreateIndex_ProductCatalog(g.分类名, "/Lucene.Net/IndexDic/ProductCatalog");
            //}

            //IEnumerable<通知> tz = 通知管理.查询通知(0, 0, Query<通知>.EQ(o => o.审核数据.审核状态, 审核状态.审核通过), includeDisabled: false);
            //foreach (var g in tz)
            //{
            //    CreateIndex(g, "/Lucene.Net/IndexDic/Tongzhi");
            //}

            //IEnumerable<新闻> news = 新闻管理.查询新闻(0, 0, Query<新闻>.EQ(o => o.审核数据.审核状态, 审核状态.审核通过), includeDisabled: false);
            //foreach (var g in news)
            //{
            //    CreateIndex(g, "/Lucene.Net/IndexDic/News");
            //}

            //IEnumerable<政策法规> zcfg = 政策法规管理.查询政策法规(0, 0, Query<政策法规>.EQ(o => o.审核数据.审核状态, 审核状态.审核通过), includeDisabled: false);
            //foreach (var g in zcfg)
            //{
            //    CreateIndex(g, "/Lucene.Net/IndexDic/Zcfg");
            //}

            //IEnumerable<公告> gg = 公告管理.查询公告(0, 0, Query<公告>.EQ(o => o.审核数据.审核状态, 审核状态.审核通过), includeDisabled: false);
            //foreach (var g in gg)
            //{
            //    GG_CreateIndex(g, "/Lucene.Net/IndexDic/GongGao");
            //}


            //GetPage("http://go81.cn/WeChat/", "<xml><ToUserName><![CDATA[gh_0437d5a6df08]]></ToUserName><FromUserName><![CDATA[oown0svIiIOCrhpB7brqwH6y2Y8Q]]></FromUserName><CreateTime>" + ConvertDateTimeInt(DateTime.Now) + "</CreateTime><MsgType><![CDATA[text]]></MsgType><Content><![CDATA[A]]></Content><MsgId>6130749585392878399</MsgId></xml>");

            return View();
        }
        private int ConvertDateTimeInt(System.DateTime time)
        {
            System.DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1));
            return (int)(time - startTime).TotalSeconds;
        }
        public string GetPage(string posturl, string postData)
        {
            System.IO.Stream outstream = null;
            System.IO.Stream instream = null;
            StreamReader sr = null;
            System.Net.HttpWebResponse response = null;
            System.Net.HttpWebRequest request = null;
            System.Text.Encoding encoding = System.Text.Encoding.UTF8;
            byte[] data = encoding.GetBytes(postData);
            // 准备请求...
            try
            {
                // 设置参数
                request = System.Net.WebRequest.Create(posturl) as System.Net.HttpWebRequest;
                System.Net.CookieContainer cookieContainer = new System.Net.CookieContainer();
                request.CookieContainer = cookieContainer;
                request.AllowAutoRedirect = true;
                request.Method = "POST";
                request.ContentType = "application/x-www-form-urlencoded";
                request.ContentLength = data.Length;
                outstream = request.GetRequestStream();
                outstream.Write(data, 0, data.Length);
                outstream.Close();
                //发送请求并获取相应回应数据
                response = request.GetResponse() as System.Net.HttpWebResponse;
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


        public ActionResult GoodExamine_Ed()//商品审核_待审核
        {
            return View();
        }
        public ActionResult GoodExamineDetail()//商品审核_待审核_详情
        {
            return View();
        }
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult GoodExamineDetail(string action, Models.数据模型.商品数据模型.商品 g)//商品审核_待审核_详情
        {
            商品 sp = 商品管理.查找商品(g.Id);
            if (action == "审核通过")
            {
                try
                {
                    //商品审核时，检查属性模板，有新值时更新属性模板
                    商品分类 pro_class = 商品分类管理.查找分类(sp.商品信息.所属商品分类.商品分类ID);
                    foreach (var arr in pro_class.商品属性模板)
                    {
                        foreach (var item in arr.Value)
                        {
                            var vals = sp.商品数据.商品属性[arr.Key][item.Key];
                            if (item.Value.属性类型 == 属性类型.复选)
                            {
                                foreach (var selfitem in vals)
                                {
                                    if (!item.Value.值.Contains(selfitem))
                                    {
                                        pro_class.商品属性模板[arr.Key][item.Key].值.Add(selfitem);
                                    }
                                }
                            }
                        }
                    }
                    商品分类管理.更新分类(pro_class);
                }
                catch
                {

                }

                商品管理.审核商品(g.Id, this.HttpContext.获取当前用户<运营团队>().Id, 审核状态.审核通过);
                deleteIndex("/Lucene.Net/IndexDic/Product", g.Id.ToString());
                CreateIndex(sp, "/Lucene.Net/IndexDic/Product");

            }
            else if (action == "审核不通过")
            {
                商品管理.审核商品(g.Id, this.HttpContext.获取当前用户<运营团队>().Id, 审核状态.审核未通过);

                deleteIndex("/Lucene.Net/IndexDic/Product", g.Id.ToString());
            }
            return View("GoodExamine");
        }
        public ActionResult GoodNotPass()//商品审核_未通过
        {
            return View();
        }
        public ActionResult GoodNotPassDetail()//商品审核_未通过_详情
        {
            return View();
        }
        public ActionResult GoodNotPassFeedback()//商品审核_未通过_反馈
        {
            return View();
        }
        public ActionResult GoodExaminePass()//商品审核_已通过
        {
            return View();
        }
        public ActionResult GoodExaminePassDetail()//商品审核_已通过_详情
        {
            return View();
        }
        public ActionResult User_Info()
        {
            return View();
        }
        public ActionResult Left_List()//左侧树形控件
        {

            return PartialView("Part_View/Left_List");
        }
        public ActionResult Part_SystemInfo(int? page) //系统通知
        {
            int listcount = (int)通知管理.计数通知(0, 0);
            int maxpage = Math.Max((listcount + TZ_PAGESIZE - 1) / TZ_PAGESIZE, 1);

            消息查询(page, TZ_PAGESIZE, listcount, maxpage, "系统通知");
            return PartialView("Part_View/Part_SystemInfo");
        }
        public ActionResult Print_Detail()
        {
            try
            {
                long id = long.Parse(Request.QueryString["id"]);
                供应商 model = 用户管理.查找用户<供应商>(id);
                if (model != null)
                {
                    return View(model);
                }
                else
                {
                    return Redirect("/运营团队后台/Supplier_PssInfo");
                }
            }
            catch
            {
                return Redirect("/运营团队后台/Supplier_PssInfo");
            }

        }
        public ActionResult Part_SystemInfo_Detail(long id) //系统通知详情
        {
            通知 tz = 通知管理.查找通知(id);
            return PartialView("Part_View/Part_SystemInfo_Detail", tz);
        }
        public ActionResult Gys_Del_Msg()
        {
            try
            {
                long id = long.Parse(Request.QueryString["id"].ToString());
                站内消息管理.删除站内消息(id);
                return Content("<script>window.location='/运营团队后台/Znxx';</script>");
            }
            catch
            {
                return Content("<script>window.location='/运营团队后台/Znxx';</script>");
            }

        }
        public ActionResult Part_ZNXX(int? page) //站内消息
        { //SortBy.Ascending

            if (string.IsNullOrEmpty(page.ToString()))
            {
                page = 1;
            }
            ViewData["page"] = page;
            long c = 站内消息管理.计数站内消息(0, 0, Query<站内消息>.Where(m => m.收信人.用户ID == currentUser.Id));
            ViewData["listcount"] = 站内消息管理.计数站内消息(0, 0, Query<站内消息>.Where(m => m.收信人.用户ID == currentUser.Id));
            ViewData["pagesize"] = 20;
            ViewData["站内消息列表"] = 站内消息管理.查询收信人的站内消息(20 * (int.Parse(page.ToString()) - 1), 20, currentUser.Id).OrderByDescending(m => m.重要程度).OrderByDescending(m => m.基本数据.修改时间);
            return PartialView("Part_View/Part_ZNXX");
        }
        [ValidateInput(false)]
        public string Send_Message()
        {
            string[] rs = Request.QueryString["r"].ToString().Split(',');
            for (int i = 0; i < rs.Length - 1; i++)
            {
                if (用户管理.检查用户是否存在(rs[i]) != -1)
                {
                    站内消息 Msg = new 站内消息();
                    对话消息 Talk = new 对话消息();
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
                    Talk.发言人.用户ID = currentUser.Id;
                    Msg.发起者.用户ID = currentUser.Id;
                    站内消息管理.添加站内消息(Msg, Msg.发起者.用户ID, Msg.收信人.用户ID);
                    Talk.消息主体.标题 = Request.QueryString["t"].ToString();
                    Talk.消息主体.内容 = Request.QueryString["c"].ToString();
                    对话消息管理.添加对话消息(Talk, Msg);
                }
            }
            return "添加消息成功!";
        }
        public ActionResult Part_ZNXX_Add() //站内消息_发送消息
        {
            ViewData["行业"] = 商品分类管理.查找子分类();
            return PartialView("Part_View/Part_ZNXX_Add");
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
        public ActionResult Supplier_Delete(long id)
        {
            try
            {
                IEnumerable<商品> good = 商品管理.查询供应商商品(id);
                if (用户管理.删除用户<供应商>(id))
                {
                    foreach (var item in good)
                    {
                        //TD:商品精确型号
                        deleteIndex("/Lucene.Net/IndexDic/Product", item.Id.ToString());
                        商品管理.删除商品(item.Id);
                    }

                    deleteIndex("/Lucene.Net/IndexDic/Gys", id.ToString());
                    //deleteIndex_ProductCatalog("/Lucene.Net/IndexDic/GysCatalog", 用户管理.查找用户<供应商>(id).企业基本信息.企业名称);
                }
                return Redirect("/运营团队后台/Supplier_PssInfo");
            }
            catch
            {
                return Redirect("/运营团队后台/Supplier_PssInfo");
            }

        }

        public class SearchUser
        {
            /// <summary>
            /// 用户Id
            /// </summary>
            public long Id { get; set; }
            /// <summary>
            /// 登录名
            /// </summary>
            public string LoginName { get; set; }
            /// <summary>
            /// 最近登录时间
            /// </summary>
            public string LastLogin { get; set; }
            /// <summary>
            /// 真实名称
            /// </summary>
            public string RealName { get; set; }
        }
        public JsonResult Search_User()
        {
            int skip;
            if (string.IsNullOrWhiteSpace(Request.QueryString["p"]))
            {
                skip = 1;
            }
            else
            {
                skip = int.Parse(Request.QueryString["p"]);
            }
            string name = Request.QueryString["n"];
            string catogray = Request.QueryString["catogray"];
            long PgCount = 1;
            List<SearchUser> usr = new List<SearchUser>();
            switch (catogray)
            {
                case "单位用户": IEnumerable<单位用户> Depart = 用户管理.查询用户<单位用户>(0, 0, Query.And(Query.EQ("登录信息.登录名", new BsonRegularExpression(string.Format("/{0}/i", name)))));
                    PgCount = 用户管理.计数用户<单位用户>(0, 0, Query.And(Query.EQ("登录信息.登录名", new BsonRegularExpression(string.Format("/{0}/i", name))))) / 20;
                    if ((用户管理.计数用户<单位用户>(0, 0, Query.And(Query.EQ("登录信息.登录名", new BsonRegularExpression(string.Format("/{0}/i", name))))) % 20) > 0)
                    {
                        PgCount++;
                    }
                    if (Depart != null)
                    {
                        foreach (var item in Depart)
                        {
                            SearchUser u0 = new SearchUser();
                            u0.Id = item.Id;
                            u0.LoginName = item.登录信息.登录名;
                            u0.RealName = item.单位信息.单位代号;
                            u0.LastLogin = item.基本数据.修改时间.ToString("yyyy-MM-dd");
                            usr.Add(u0);
                        }
                    }
                    break;
                case "运营团队": IEnumerable<运营团队> Manager = 用户管理.查询用户<运营团队>(0, 0, Query.And(Query.EQ("登录信息.登录名", new BsonRegularExpression(string.Format("/{0}/i", name)))));
                    PgCount = 用户管理.计数用户<运营团队>(0, 0, Query.And(Query.EQ("登录信息.登录名", new BsonRegularExpression(string.Format("/{0}/i", name))))) / 20;
                    if ((用户管理.计数用户<运营团队>(0, 0, Query.And(Query.EQ("登录信息.登录名", new BsonRegularExpression(string.Format("/{0}/i", name))))) % 20) > 0)
                    {
                        PgCount++;
                    }
                    if (Manager != null)
                    {
                        foreach (var item in Manager)
                        {
                            SearchUser u1 = new SearchUser();
                            u1.Id = item.Id;
                            u1.LoginName = item.登录信息.登录名;
                            u1.RealName = item.运营团队工作人员信息.姓名;
                            u1.LastLogin = item.基本数据.修改时间.ToString("yyyy-MM-dd");
                            usr.Add(u1);
                        }
                    }
                    break;
                case "供应商": IEnumerable<供应商> Supplier = 用户管理.查询用户<供应商>(20 * (skip - 1), 20, Query.And(Query.EQ("企业基本信息.企业名称", new BsonRegularExpression(string.Format("/{0}/i", name)))));
                    PgCount = 用户管理.计数用户<供应商>(0, 0, Query.And(Query.EQ("企业基本信息.企业名称", new BsonRegularExpression(string.Format("/{0}/i", name))))) / 20;
                    if ((用户管理.计数用户<供应商>(0, 0, Query.And(Query.EQ("企业基本信息.企业名称", new BsonRegularExpression(string.Format("/{0}/i", name))))) % 20) > 0)
                    {
                        PgCount++;
                    }
                    if (Supplier != null)
                    {
                        foreach (var item in Supplier)
                        {
                            SearchUser u2 = new SearchUser();
                            u2.Id = item.Id;
                            u2.LoginName = item.登录信息.登录名;
                            u2.RealName = item.企业基本信息.企业名称;
                            u2.LastLogin = item.基本数据.修改时间.ToString("yyyy-MM-dd");
                            usr.Add(u2);
                        }
                    }
                    break;
                case "个人用户": IEnumerable<个人用户> Single = 用户管理.查询用户<个人用户>(20 * (skip - 1), 20, Query.And(Query.EQ("登录信息.登录名", new BsonRegularExpression(string.Format("/{0}/i", name)))));
                    PgCount = 用户管理.计数用户<个人用户>(0, 0, Query.And(Query.EQ("登录信息.登录名", new BsonRegularExpression(string.Format("/{0}/i", name))))) / 20;
                    if ((用户管理.计数用户<个人用户>(0, 0, Query.And(Query.EQ("登录信息.登录名", new BsonRegularExpression(string.Format("/{0}/i", name))))) % 20 > 0))
                    {
                        PgCount++;
                    }
                    if (Single != null)
                    {
                        foreach (var item in Single)
                        {
                            SearchUser u2 = new SearchUser();
                            u2.Id = item.Id;
                            u2.LoginName = item.登录信息.登录名;
                            u2.RealName = item.个人信息.姓名;
                            u2.LastLogin = item.基本数据.修改时间.ToString("yyyy-MM-dd");
                            usr.Add(u2);
                        }
                    }
                    break;
            }
            JsonResult json = new JsonResult() { Data = new { u = usr, page = PgCount } };
            return Json(json, JsonRequestBehavior.AllowGet);
        }
        public ActionResult Part_Msg_Detail()
        {
            try
            {
                ViewData["当前用户"] = currentUser.登录信息.登录名;
                int id = int.Parse(Request.QueryString["id"].ToString());
                站内消息管理.更新已读时间(id, false, DateTime.Now);
                var model = 站内消息管理.查找站内消息(id);
                return PartialView("Part_View/Part_Msg_Detail", model);
            }
            catch
            {
                return Content("<script>window.location='/运营团队后台/ZNXX';</script>");
            }

        }
        public ActionResult Msg_Detail()
        {
            return View();
        }
        public ActionResult Del_Msg(int id)
        {
            站内消息管理.删除站内消息(id);
            return Content("<script>window.location='/运营团队后台/ZNXX_Sended';</script>");
        }

        public ActionResult Part_ZNXX_Sended(int? page)//站内消息_已发消息
        {

            if (string.IsNullOrEmpty(page.ToString()))
            {
                page = 1;
            }
            ViewData["page"] = page;
            ViewData["listcount"] = 站内消息管理.计数站内消息(0, 0, Query.EQ("发起者.用户ID", currentUser.Id));
            ViewData["pagesize"] = 20;
            ViewData["已发送站内消息"] = 站内消息管理.查询发起者的站内消息(20 * (int.Parse(page.ToString()) - 1), 20, currentUser.Id).OrderByDescending(m => m.重要程度).OrderByDescending(m => m.基本数据.修改时间);
            return PartialView("Part_View/Part_ZNXX_Sended");
        }
        public ActionResult 查找站内消息(long xxid)
        {
            站内消息 Znxx = 站内消息管理.查找站内消息(xxid);
            return Json(Znxx);
        }
        public ActionResult Part_GoodClassify()//商品目录管理
        {
            ViewData["商品分类"] = 商品分类管理.查找子分类();
            return PartialView("Part_View/Part_GoodClassify");
        }
        public ActionResult Part_GoodAttrManage(long xid, string name)//商品目录管理_属性管理
        {
            ViewData["商品分类"] = 商品分类管理.查找子分类();
            ViewBag.Xid = xid;
            ViewBag.Name = name;
            return PartialView("Part_View/Part_GoodAttrManage");
        }
        public class AttrCopy
        {
            public long Id { get; set; }
            public string Name { get; set; }
        }
        public JsonResult GetSonClass()
        {
            string id = Request.Params["id"];
            List<AttrCopy> li = new List<AttrCopy>();
            Dictionary<string, List<AttrCopy>> dic = new Dictionary<string, List<AttrCopy>>();
            if (id == "ZY")
            {
                var gd = 商品分类管理.查找子分类();
                foreach (var m in gd)
                {
                    if (m.分类性质 == 商品分类性质.专用物资)
                    {
                        dic.Add(m.分类名, new List<AttrCopy>());
                        foreach (var p in m.子分类)
                        {
                            AttrCopy d = new AttrCopy();
                            d.Id = p.Id;
                            d.Name = p.分类名;
                            dic[m.分类名].Add(d);
                        }
                    }
                }
            }
            else
            {
                var good = 商品分类管理.查找分类(long.Parse(id));
                foreach (var k in good.子分类)
                {
                    dic.Add(k.分类名, new List<AttrCopy>());
                    foreach (var p in k.子分类)
                    {
                        AttrCopy d = new AttrCopy();
                        d.Id = p.Id;
                        d.Name = p.分类名;
                        dic[k.分类名].Add(d);
                    }
                }
            }
            JsonResult json = new JsonResult() { Data = dic };
            return Json(json, JsonRequestBehavior.AllowGet);
        }
        public void CopyAttr()
        {
            string id = Request.Params["id"];
            string toid = Request.Params["toid"];
            var g = 商品分类管理.查找分类(long.Parse(id));
            var tog = 商品分类管理.查找分类(long.Parse(toid));
            tog.商品属性模板 = g.商品属性模板;
            商品分类管理.更新分类(tog);
        }
        public ActionResult Part_GoodExamine(int? page)//商品审核_待审核
        {

            var l = 用户管理.列表用户<供应商>(0, 0,
                Fields<供应商>.Include(o => o.Id),
                Query<供应商>.EQ(o => o.审核数据.审核状态, 审核状态.审核通过)
                ).Select(o => o["_id"]);
            var q = Query.And(
                    Query.In("商品信息.所属供应商.用户ID", l),
                    Query.EQ("审核数据.审核状态", 审核状态.未审核)
                );

            string name = "";
            if (Request.QueryString["name"] != null)
            {
                name = Request.QueryString["name"];
            }
            ViewData["name"] = name;
            int count = (int)商品管理.计数商品(0, 0, q);
            int maxpage = Math.Max((count + PRO_PAGESIZE - 1) / PRO_PAGESIZE, 1);
            if (string.IsNullOrEmpty(page.ToString()) || page < 0 || page > maxpage)
            {
                page = 1;
            }

            ViewData["currentpage"] = page;
            ViewData["pagecount"] = maxpage;

            ViewData["listcount"] = count;
            ViewData["pagesize"] = PRO_PAGESIZE;
            ViewData["商品信息"] = 商品管理.查询商品(PRO_PAGESIZE * ((int)page - 1), PRO_PAGESIZE, q);

            return PartialView("Part_View/Part_GoodExamine");
        }
        [HttpPost]
        public ActionResult doProductShelves()
        {
            try
            {
                var id = Request.Params["id"];
                var status = Request.Params["status"];
                var changetype = Request.Params["changetype"];


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
                                s.基本数据.已屏蔽 = false;
                                //TD：删除并新建Lucene
                                deleteIndex("/Lucene.Net/IndexDic/Product", s.Id.ToString());
                                CreateIndex(s, "/Lucene.Net/IndexDic/Product");
                                商品管理.更新商品(s, setUnverified: false);
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
                                sp.基本数据.已屏蔽 = false;
                                //TD：删除并新建Lucene
                                deleteIndex("/Lucene.Net/IndexDic/Product", sp.Id.ToString());
                                CreateIndex(sp, "/Lucene.Net/IndexDic/Product");
                                商品管理.更新商品(sp, setUnverified: false);
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
            catch
            {
                return Content("发生错误，请重试！");
            }
        }
        /// <summary>
        /// 运营团队后台筛选商品ajax方法
        /// </summary>
        /// <returns></returns>
        public ActionResult getSearchPro()
        {
            var gys = Request.QueryString["gys"];
            var name = Request.QueryString["name"];
            var auditval = Request.QueryString["auditval"];
            var gys_auditval = Request.QueryString["gys_auditval"];
            int page = 1;
            int.TryParse(Request.Params["page"], out page);
            var isdelete = Request.QueryString["isdelete"];
            var iszhongbiao = Request.QueryString["iszhongbiao"];

            //未选条件则直接跳转
            if (string.IsNullOrWhiteSpace(gys) && string.IsNullOrWhiteSpace(name) && auditval == "0" && isdelete == "0" && gys_auditval == "1" && iszhongbiao == "-1")
            {
                return Content("0");
            }
            IMongoQuery q = null;

            //若输入供应商,则先得出满足条件的供应商的ID 集合
            if (!string.IsNullOrWhiteSpace(gys_auditval) && gys_auditval != "-1")
            {
                var g = 用户管理.列表用户<供应商>(0, 0,
                    Fields<供应商>.Include(o => o.Id),
                    Query<供应商>.EQ(o => o.审核数据.审核状态, (审核状态)int.Parse(gys_auditval))
                    );
                q = q.And(Query<商品>.In(o => o.商品信息.所属供应商.用户ID, g.Select(o => o["_id"].AsInt64)));
            }
            if (!string.IsNullOrWhiteSpace(gys))
            {
                var g = 用户管理.列表用户<供应商>(0, 0,
                        Fields<供应商>.Include(o => o.Id),
                        Query<供应商>.Matches(o => o.企业基本信息.企业名称,
                        new BsonRegularExpression(string.Format("/{0}/i", gys))));

                q = q.And(Query<商品>.In(o => o.商品信息.所属供应商.用户ID, g.Select(o => o["_id"].AsInt64)));
            }

            if (!string.IsNullOrWhiteSpace(name))
            {
                q = q.And(Query<商品>.Matches(o => o.商品信息.商品名,
                        new BsonRegularExpression(string.Format("/{0}/i", name))));
            }
            if (auditval != "-1")
            {
                q = q.And(Query<商品>.EQ(o => o.审核数据.审核状态, (审核状态)int.Parse(auditval)));
            }

            if (isdelete == "1")
            {
                q = q.And(Query<商品>.EQ(o => o.基本数据.已删除, true));
            }
            if (iszhongbiao != "-1")
            {
                var zb = iszhongbiao == "1";
                q = q.And(Query<商品>.EQ(o => o.中标商品, zb));
            }

            int listcount = (int)(商品管理.计数商品(0, 0, q, includeDeleted: isdelete != "0"));
            int maxpage = Math.Max((listcount + PRO_PAGESIZE - 1) / PRO_PAGESIZE, 1);

            ViewData["currentpage"] = page;
            ViewData["pagecount"] = maxpage;
            ViewData["商品信息"] = 商品管理.查询商品(PRO_PAGESIZE * (int.Parse(page.ToString()) - 1), PRO_PAGESIZE, q, includeDeleted: isdelete != "0");

            return PartialView("Part_View/getSearchPro");

        }
        public ActionResult Part_GoodExamine_Ed(int? page)//商品审核_已审核
        {

            //var l = 用户管理.列表用户<供应商>(0, 0,
            //    new string[] { "_id" },
            //    Query.GTE("供应商用户信息.认证级别", 供应商.认证级别.已审核用户)
            //    ).Select(o => o["_id"]);
            var q = Query.EQ("审核数据.审核状态", 审核状态.审核通过);


            int count = (int)商品管理.计数商品(0, 0, q);
            int maxpage = Math.Max((count + PRO_PAGESIZE - 1) / PRO_PAGESIZE, 1);
            if (string.IsNullOrEmpty(page.ToString()) || page < 0 || page > maxpage)
            {
                page = 1;
            }
            ViewData["page"] = page;
            ViewData["listcount"] = count;
            ViewData["pagesize"] = PRO_PAGESIZE;
            ViewData["未审核商品信息"] = 商品管理.查询商品(PRO_PAGESIZE * (int.Parse(page.ToString()) - 1), PRO_PAGESIZE, q);

            return PartialView("Part_View/Part_GoodExamine_Ed");
        }
        public ActionResult Part_GoodExamineDetail()//商品审核_待审核_详情
        {
            try
            {
                long id = long.Parse(Request.QueryString["id"].ToString());
                商品 pro_detail = 商品管理.查找商品((long)id);
                if (pro_detail != null)
                {
                    ViewBag.data = (用户管理.查找用户<供应商>(pro_detail.商品信息.所属供应商.用户ID)).信用评级信息.等级;
                    if (pro_detail.商品信息.商品图片.Count == 0)
                    {
                        pro_detail.商品信息.商品图片.Add("/images/noimage.jpg");
                    }
                    var l = 商品管理.查询历史价格数据((long)id);
                    this.ViewBag.L1 = l.Item1.ToJson().Replace("ISODate", "new Date");

                    return PartialView("Part_View/Part_GoodExamineDetail", pro_detail);
                }
                else
                {
                    //return PartialView("Part_View/Part_GoodExamineDetail", pro_detail);
                    return Content("<script>window.location='/运营团队后台/GoodExamine';</script>");
                }

            }
            catch
            {
                //return PartialView("Part_View/Part_GoodExamineDetail", pro_detail);
                return Content("<script>window.location='/运营团队后台/GoodExamine';</script>");
            }


        }
        public ActionResult GoodExamineXiajia(int? id)//商品审核_待审核_详情
        {
            商品管理.审核商品((long)id, this.HttpContext.获取当前用户<运营团队>().Id, 审核状态.审核未通过);

            deleteIndex("/Lucene.Net/IndexDic/Product", id.ToString());

            return View("GoodExamine");
        }
        public ActionResult Part_GoodNotPass()//商品审核_未通过
        {
            return PartialView("Part_View/Part_GoodNotPass");
        }
        public ActionResult Part_GoodNotPassDetail()//商品审核_未通过_详情
        {
            return PartialView("Part_View/Part_GoodNotPassDetail");
        }
        public ActionResult Part_GoodNotPassFeedback()//商品审核_未通过_反馈
        {
            return PartialView("Part_View/Part_GoodNotPassFeedback");
        }
        public ActionResult Part_GoodExaminePass()//商品审核_已通过
        {
            return PartialView("Part_View/Part_GoodExaminePass");
        }
        public ActionResult Part_GoodExaminePassDetail()//商品审核_已通过_详情
        {
            return PartialView("Part_View/Part_GoodExaminePassDetail");
        }
        public ActionResult Right_User_List()
        {
            return PartialView("Part_View/Right_User_List");
        }
        public ActionResult Part_Supplier_PssInfo()//已审核供应商右侧列表
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
            int PageCount = (int)用户管理.计数用户<供应商>(0, 0) / 20;
            if ((用户管理.计数用户<供应商>(0, 0) % 20) > 0)
            {
                PageCount++;
            }
            ViewData["allSupplier"] = 用户管理.计数用户<供应商>(0, 0);
            ViewData["checkedSupplier"] = 用户管理.计数用户<供应商>(0, 0, Query<供应商>.Where(m => m.审核数据.审核状态 == 审核状态.审核通过));
            ViewData["waitSupplier"] = 用户管理.计数用户<供应商>(0, 0, Query<供应商>.Where(m => m.审核数据.审核状态 == 审核状态.未审核));
            ViewData["nopassSupplier"] = 用户管理.计数用户<供应商>(0, 0, Query<供应商>.Where(m => m.审核数据.审核状态 == 审核状态.审核未通过));
            ViewData["Pagecount"] = PageCount;
            ViewData["supplier"] = 用户管理.查询用户<供应商>(20 * (int.Parse(page.ToString()) - 1), 20).OrderBy(m => m.基本数据.添加时间);
            ViewData["gys"] = 用户管理.查询用户<供应商>(0, 0, Query<供应商>.Where(o => o.审核数据.审核状态 == 审核状态.未审核 && o.供应商用户信息.已提交));
            return PartialView("Part_View/Supplier_Info_List");
        }
        public ActionResult Ad_User_Infos()//添加网站用户右侧页面
        {
            ViewData["list"] = GenerateList(地址类型.联络地址.ToString());
            return PartialView("Part_View/Ad_User_Infos");
        }
        public static SelectList GenerateList(String option)
        {
            List<SelectListItem> items = new List<SelectListItem>()
            { 
                new SelectListItem(){Text=地址类型.发货地址.ToString(),Value=地址类型.发货地址.ToString()},
                new SelectListItem(){Text=地址类型.工作地址.ToString(),Value=地址类型.工作地址.ToString()},
                new SelectListItem(){Text=地址类型.单位地址.ToString(),Value=地址类型.单位地址.ToString()},
                new SelectListItem(){Text=地址类型.经营地址.ToString(),Value=地址类型.经营地址.ToString()},
                new SelectListItem(){Text=地址类型.居住地址.ToString(),Value=地址类型.居住地址.ToString()},
                new SelectListItem(){Text=地址类型.联络地址.ToString(),Value=地址类型.联络地址.ToString()},
                new SelectListItem(){Text=地址类型.收货地址.ToString(),Value=地址类型.收货地址.ToString()},
                new SelectListItem(){Text=地址类型.其他.ToString(),Value=地址类型.其他.ToString()},
                new SelectListItem(){Text=地址类型.未指定.ToString(),Value=地址类型.未指定.ToString()},
            };
            SelectList selectlist = new SelectList(items, "Value", "Text", option);
            return selectlist;
        }
        public ActionResult 添加用户(运营团队 user)
        {
            用户管理.添加用户(user);
            return Content("<script>window.location='User_Info_List'</script>");
        }
        public ActionResult Supplier_Nopss_Reason_Info()//供应商未通过审核的详情
        {
            return PartialView("Part_View/Supplier_Nopss_Reason_Info");
        }
        public ActionResult Supplier_Pss_DetPart()//通过审核的供应商详细信息
        {
            long id = long.Parse(Request.QueryString["id"].ToString());
            var model = 用户管理.查找用户(id);
            return PartialView("Part_View/Supplier_Pss_DetPart", model);
        }
        public ActionResult Supplier_Pss_Detail()//通过审核的供应商详情页
        {
            return View();
        }
        public ActionResult Supplier_Nopss_Reason()//供应商未通过审核的原因
        {
            return View();
        }
        public ActionResult Expert_List()//网站用户页面
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
            int PageCount = (int)用户管理.计数用户<专家>(0, 0) / 20;
            if ((用户管理.计数用户<专家>(0, 0) % 20) > 0)
            {
                PageCount++;
            }
            ViewData["expert"] = 用户管理.计数用户<专家>(0, 0);
            ViewData["checked"] = 用户管理.计数用户<专家>(0, 0, Query<专家>.Where(m => m.审核数据.审核状态 == 审核状态.审核通过));
            ViewData["wait"] = 用户管理.计数用户<专家>(0, 0, Query<专家>.Where(m => m.审核数据.审核状态 == 审核状态.未审核));
            ViewData["Pagecount"] = PageCount;
            ViewData["supplier"] = 用户管理.查询用户<专家>(20 * (int.Parse(page.ToString()) - 1), 20);
            return View();
        }
        public ActionResult Delete_User(long id)
        {
            bool IsDel = false;
            if (用户管理.查找用户<供应商>(id) != null)
            {
                deleteIndex("/Lucene.Net/IndexDic/Gys", id.ToString());
                //deleteIndex_ProductCatalog("/Lucene.Net/IndexDic/GysCatalog", 用户管理.查找用户<供应商>(id).企业基本信息.企业名称);
            }

            IsDel = 用户管理.删除用户(id);//TODO
            if (IsDel == true)
            {
                return Content("<script>window.location='/运营团队后台/User_Info_List';</script>");
            }
            else
            {
                return Content("<script>window.location='/运营团队后台/User_Info_List';</script>");
            }
        }
        public ActionResult User_Infoes_Detail()//网站用户右侧详情
        {
            try
            {
                long id = long.Parse(Request.QueryString["id"].ToString());
                return PartialView("Part_View/User_Infoes_Detail", 用户管理.查找用户(id));
            }
            catch
            {
                return Content("<script>window.location='User_Info_List'</script>");
            }

        }
        public ActionResult Part_gys_info1(int? page)
        {
            long listcount = 用户管理.计数用户<供应商>(0, 0);
            long maxpage = Math.Max((listcount + 20 - 1) / 20, 1);
            if (string.IsNullOrEmpty(page.ToString()) || page < 0 || page > maxpage)
            {
                page = 1;
            }

            ViewData["listcount"] = listcount;
            ViewData["pagesize"] = 20;
            ViewData["currentpage"] = page;
            ViewData["pagecount"] = maxpage;
            ViewData["supplier"] = 用户管理.查询用户<供应商>(20 * (int.Parse(page.ToString()) - 1), 20);
            return PartialView("Part_View/Part_gys_info1");
        }
        public ActionResult Part_gys_info(int? page)
        {
            long listcount = 用户管理.计数用户<供应商>(0, 0);
            long maxpage = Math.Max((listcount + 20 - 1) / 20, 1);
            if (string.IsNullOrEmpty(page.ToString()) || page < 0 || page > maxpage)
            {
                page = 1;
            }

            ViewData["listcount"] = listcount;
            ViewData["pagesize"] = 20;
            ViewData["currentpage"] = page;
            ViewData["pagecount"] = maxpage;
            ViewData["supplier"] = 用户管理.查询用户<供应商>(20 * (int.Parse(page.ToString()) - 1), 20);
            return PartialView("Part_View/Part_gys_info");
        }
        public ActionResult Part_depart_info1(int? page)
        {
            long listcount = 用户管理.计数用户<单位用户>(0, 0);
            long maxpage = Math.Max((listcount + 20 - 1) / 20, 1);
            if (string.IsNullOrEmpty(page.ToString()) || page < 0 || page > maxpage)
            {
                page = 1;
            }

            ViewData["listcount"] = listcount;
            ViewData["pagesize"] = 20;
            ViewData["currentpage"] = page;
            ViewData["pagecount"] = maxpage;
            ViewData["department"] = 用户管理.查询用户<单位用户>(20 * (int.Parse(page.ToString()) - 1), 20);
            return PartialView("Part_View/Part_depart_info1");
        }
        public ActionResult Part_depart_info(int? page)
        {
            long listcount = 用户管理.计数用户<单位用户>(0, 0);
            long maxpage = Math.Max((listcount + 20 - 1) / 20, 1);
            if (string.IsNullOrEmpty(page.ToString()) || page < 0 || page > maxpage)
            {
                page = 1;
            }

            ViewData["listcount"] = listcount;
            ViewData["pagesize"] = 20;
            ViewData["currentpage"] = page;
            ViewData["pagecount"] = maxpage;
            ViewData["department"] = 用户管理.查询用户<单位用户>(20 * (int.Parse(page.ToString()) - 1), 20);
            return PartialView("Part_View/Part_depart_info");
        }
        public ActionResult Part_single_info1(int? page)
        {
            long listcount = 用户管理.计数用户<个人用户>(0, 0);
            long maxpage = Math.Max((listcount + 20 - 1) / 20, 1);
            if (string.IsNullOrEmpty(page.ToString()) || page < 0 || page > maxpage)
            {
                page = 1;
            }

            ViewData["listcount"] = listcount;
            ViewData["pagesize"] = 20;
            ViewData["currentpage"] = page;
            ViewData["pagecount"] = maxpage;
            ViewData["single"] = 用户管理.查询用户<个人用户>(20 * (int.Parse(page.ToString()) - 1), 20);
            return PartialView("Part_View/Part_single_info1");
        }
        public ActionResult Part_single_info(int? page)
        {
            long listcount = 用户管理.计数用户<个人用户>(0, 0);
            long maxpage = Math.Max((listcount + 20 - 1) / 20, 1);
            if (string.IsNullOrEmpty(page.ToString()) || page < 0 || page > maxpage)
            {
                page = 1;
            }

            ViewData["listcount"] = listcount;
            ViewData["pagesize"] = 20;
            ViewData["currentpage"] = page;
            ViewData["pagecount"] = maxpage;
            ViewData["single"] = 用户管理.查询用户<个人用户>(20 * (int.Parse(page.ToString()) - 1), 20);
            return PartialView("Part_View/Part_single_info");
        }
        public ActionResult Part_expert_info(int? page)
        {
            long listcount = 用户管理.计数用户<专家>(0, 0);
            long maxpage = Math.Max((listcount + 20 - 1) / 20, 1);
            if (string.IsNullOrEmpty(page.ToString()) || page < 0 || page > maxpage)
            {
                page = 1;
            }

            ViewData["listcount"] = listcount;
            ViewData["pagesize"] = 20;
            ViewData["currentpage"] = page;
            ViewData["pagecount"] = maxpage;
            ViewData["expert"] = 用户管理.查询用户<专家>(20 * (int.Parse(page.ToString()) - 1), 20);
            return PartialView("Part_View/Part_expert_info");
        }
        public ActionResult Part_expert_info1(int? page)
        {
            long listcount = 用户管理.计数用户<专家>(0, 0);
            long maxpage = Math.Max((listcount + 20 - 1) / 20, 1);
            if (string.IsNullOrEmpty(page.ToString()) || page < 0 || page > maxpage)
            {
                page = 1;
            }

            ViewData["listcount"] = listcount;
            ViewData["pagesize"] = 20;
            ViewData["currentpage"] = page;
            ViewData["pagecount"] = maxpage;
            ViewData["expert"] = 用户管理.查询用户<专家>(20 * (int.Parse(page.ToString()) - 1), 20);
            return PartialView("Part_View/Part_expert_info1");
        }
        public ActionResult Part_manage_info1(int? page)
        {
            long listcount = 用户管理.计数用户<运营团队>(0, 0);
            long maxpage = Math.Max((listcount + 20 - 1) / 20, 1);
            if (string.IsNullOrEmpty(page.ToString()) || page < 0 || page > maxpage)
            {
                page = 1;
            }

            ViewData["listcount"] = listcount;
            ViewData["pagesize"] = 20;
            ViewData["currentpage"] = page;
            ViewData["pagecount"] = maxpage;
            ViewData["manager"] = 用户管理.查询用户<运营团队>(20 * (int.Parse(page.ToString()) - 1), 20);
            return PartialView("Part_View/Part_manage_info1");
        }
        public ActionResult Part_manage_info(int? page)
        {
            long listcount = 用户管理.计数用户<运营团队>(0, 0);
            long maxpage = Math.Max((listcount + 20 - 1) / 20, 1);
            if (string.IsNullOrEmpty(page.ToString()) || page < 0 || page > maxpage)
            {
                page = 1;
            }

            ViewData["listcount"] = listcount;
            ViewData["pagesize"] = 20;
            ViewData["currentpage"] = page;
            ViewData["pagecount"] = maxpage;
            ViewData["manager"] = 用户管理.查询用户<运营团队>(20 * (int.Parse(page.ToString()) - 1), 20);
            return PartialView("Part_View/Part_manage_info");
        }
        public ActionResult User_Part_InfoLst()//网站用户右侧列表
        {
            return View();//TODO
        }
        public ActionResult Suggestion_Part_Detail(int id)//投诉详情右侧页面
        {
            投诉 model = 投诉管理.查找投诉(id);
            return PartialView("Part_View/Suggestion_Part_Detail", model);
        }
        public ActionResult Suggestion_Detail()//投诉详情页面
        {
            return View();
        }
        public ActionResult Suggestion_Pss_List(int? page)//已处理投诉右侧列表页面
        {
            if (string.IsNullOrEmpty(page.ToString()))
            {
                page = 1;
            }
            ViewData["page"] = page;
            ViewData["listcount"] = 投诉管理.计数投诉(0, 0, Query.EQ("处理状态", 处理状态.已处理));
            ViewData["pagesize"] = 20;
            ViewData["投诉列表"] = 投诉管理.查询投诉(20 * (int.Parse(page.ToString()) - 1), 20, Query.EQ("处理状态", 处理状态.已处理)).OrderByDescending(m => m.基本数据.修改时间);
            return PartialView("Part_View/Suggestion_Pss_List");
        }

        public ActionResult 查找投诉(long id)
        {
            投诉 ts = 投诉管理.查找投诉(id);
            return Json(ts);
        }
        public ActionResult 删除投诉(long id)
        {
            投诉管理.删除投诉(id);
            return RedirectToAction("Suggestion");
        }
        public ActionResult Suggest_Detail()//已处理投诉右侧页面
        {
            return View();
        }
        public ActionResult Part_Suggest_Detail()//已处理投诉右侧页面
        {
            try
            {
                int id = int.Parse(Request.QueryString["id"].ToString());
                投诉 model = 投诉管理.查找投诉(id);
                return PartialView("Part_View/Part_Suggest_Detail", model);
            }
            catch
            {
                return Content("<script>window.location='/运营团队后台/Suggestion'</script>");
            }
        }
        //public ActionResult R_Detail()//建议详情
        //{
        //    return View();
        //}
        //public ActionResult Requstion_Detail()//建议详情
        //{
        //    try
        //    {
        //        int id = int.Parse(Request.QueryString["id"].ToString());
        //        建议 model = 建议管理.查找建议(id);
        //        return PartialView("Part_View/Part_Suggest_Detail", model);
        //    }
        //    catch
        //    {
        //        return Content("<script>window.location='/运营团队后台/Requestion_Replied'</script>");
        //    }

        //}
        public ActionResult Suggestion_Part_NoPss(int? page)//未处理投诉右侧页面
        {

            if (string.IsNullOrEmpty(page.ToString()))
            {
                page = 1;
            }
            ViewData["page"] = page;
            ViewData["listcount"] = 投诉管理.计数投诉(0, 0, Query.EQ("处理状态", 处理状态.未处理));
            ViewData["pagesize"] = 20;
            ViewData["未处理投诉列表"] = 投诉管理.查询投诉(20 * (int.Parse(page.ToString()) - 1), 20, Query.EQ("处理状态", 处理状态.未处理)).OrderByDescending(m => m.基本数据.修改时间);
            return PartialView("Part_View/Suggestion_Part_NoPss");
        }
        public ActionResult Requestion_Replied_List(int? page)//已回复建议右侧列表页面
        {
            if (string.IsNullOrEmpty(page.ToString()))
            {
                page = 1;
            }
            ViewData["page"] = page;
            ViewData["listcount"] = 建议管理.计数建议(0, 0);
            ViewData["pagesize"] = 20;
            ViewData["建议列表"] = 建议管理.查询受理单位收到的建议(20 * (int.Parse(page.ToString()) - 1), 20, currentUser.Id, 处理状态.已处理).OrderByDescending(m => m.基本数据.修改时间);
            return PartialView("Part_View/Requestion_Replied_List");
        }
        [ValidateInput(false)]
        public int Requstion_Reply()//回复建议
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
                建议 model = 建议管理.查找建议(id);
                model.处理状态 = 处理状态.处理中;
                建议管理.更新建议(model);
                对话消息管理.添加对话消息(item, model);
                return 1;
            }
            catch
            {
                return 0;
            }

        }
        public ActionResult Part_Requst_Reply()//建议回复页
        {
            try
            {
                var model = 建议管理.查找建议(int.Parse(Request.QueryString["id"].ToString()));
                return PartialView("Part_View/Part_Requst_Reply", model);
            }
            catch
            {
                return Redirect("/运营团队后台/Requestion_No_Replied");
            }

        }
        public ActionResult Requst_Reply()//建议回复页
        {
            return View();
        }
        public ActionResult Part_Suggestion_Dealing(int? page)
        {
            long listcount = 投诉管理.计数投诉(0, 0, Query.EQ("处理状态", 处理状态.处理中));
            long maxpage = Math.Max((listcount + QUESTION_PAGESIZE - 1) / QUESTION_PAGESIZE, 1);

            if (string.IsNullOrEmpty(page.ToString()) || page < 0 || page > maxpage)
            {
                page = 1;
            }
            ViewData["page"] = page;
            ViewData["listcount"] = listcount;
            ViewData["pagesize"] = QUESTION_PAGESIZE;
            ViewData["投诉"] = 投诉管理.查询投诉(QUESTION_PAGESIZE * (int.Parse(page.ToString()) - 1), QUESTION_PAGESIZE, Query.EQ("处理状态", 处理状态.处理中)).OrderByDescending(m => m.基本数据.修改时间);
            return PartialView("Part_View/Part_Suggestion_Dealing");
        }
        public ActionResult Part_Requestion_Dealing(int? page)
        {
            long listcount = 建议管理.计数建议(0, 0, Query.EQ("处理状态", 处理状态.处理中));
            long maxpage = Math.Max((listcount + QUESTION_PAGESIZE - 1) / QUESTION_PAGESIZE, 1);

            if (string.IsNullOrEmpty(page.ToString()) || page < 0 || page > maxpage)
            {
                page = 1;
            }
            ViewData["page"] = page;
            ViewData["listcount"] = listcount;
            ViewData["pagesize"] = maxpage;
            ViewData["建议"] = 建议管理.查询建议(QUESTION_PAGESIZE * (int.Parse(page.ToString()) - 1), QUESTION_PAGESIZE, Query.EQ("处理状态", 处理状态.处理中)).OrderByDescending(m => m.基本数据.修改时间);
            return PartialView("Part_View/Part_Requestion_Dealing");
        }
        public ActionResult Suggestion_Dealing()
        {
            return View();
        }
        public ActionResult Requestion_Dealing()
        {
            return View();
        }
        public ActionResult Requestion_Part_No_Replied(int? page)//未回复建议右侧页面
        {

            if (string.IsNullOrEmpty(page.ToString()))
            {
                page = 1;
            }
            ViewData["page"] = page;
            ViewData["listcount"] = 建议管理.计数建议(0, 0, Query.EQ("处理状态", 处理状态.未处理));
            ViewData["pagesize"] = 20;
            ViewData["我的建议"] = 建议管理.查询建议(20 * (int.Parse(page.ToString()) - 1), 20, Query.EQ("处理状态", 处理状态.未处理)).OrderByDescending(m => m.基本数据.修改时间);
            return PartialView("Part_View/Requestion_Part_No_Replied");
        }
        public ActionResult Supplier_Refused(供应商 model)//拒绝供应商的加入
        {
            供应商 Newmodel = 用户管理.查找用户<供应商>(model.Id);
            Newmodel.审核数据.审核不通过原因 = model.审核数据.审核不通过原因;
            Newmodel.审核数据.审核状态 = 审核状态.审核未通过;
            Newmodel.供应商用户信息.已提交 = false;
            用户管理.更新用户<供应商>(Newmodel);
            deleteIndex("/Lucene.Net/IndexDic/Gys", model.Id.ToString());
            //deleteIndex_ProductCatalog("/Lucene.Net/IndexDic/GysCatalog", Newmodel.企业基本信息.企业名称);
            return Content("<script>window.location='/运营团队后台/Supplier_PssInfo';</script>");
        }
        public ActionResult Msg_Sent()
        {
            return View();
        }
        public ActionResult Part_Msg_Sent()
        {
            try
            {
                long id = long.Parse(Request.QueryString["id"].ToString());
                站内消息管理.更新已读时间(id, true, DateTime.Now);
                站内消息 model = 站内消息管理.查找站内消息(id);
                return PartialView("Part_View/Part_Msg_Sent", model);
            }
            catch
            {
                return Content("<script>window.location='/运营团队后台/ZNXX_Sended';</script>");
            }

        }
        [HttpPost]
        public ActionResult Supplier_Accept(供应商 model)//接受供应商的加入
        {
            //供应商.修改认证级别(id, 供应商.认证级别.已审核用户);
            try
            {
                var d = DateTime.Now;
                供应商 supplier = 用户管理.查找用户<供应商>(model.Id);
                if (Request.Form["ns"] == "1" && !model.供应商用户信息.年检列表.ContainsKey(DateTime.Now.Year.ToString()))
                {
                    model.供应商用户信息.年检列表.Add(d.Year.ToString(), new 操作数据() { 操作时间 = d });

                }
                else
                {
                    model.供应商用户信息.年检列表 = supplier.供应商用户信息.年检列表;
                    model.供应商用户信息.年检列表.Remove(d.Year.ToString());

                }
                if (model.供应商用户信息.协议供应商)
                {
                    if (model.供应商用户信息.协议供应商所属地区 != null && model.供应商用户信息.协议供应商所属地区.Count() != 0)
                    {
                        if (supplier.供应商用户信息.协议供应商所属地区 == null || supplier.供应商用户信息.协议供应商所属地区.Count() == 0)
                        {
                            supplier.供应商用户信息.协议供应商所属地区 = model.供应商用户信息.协议供应商所属地区;
                        }
                        else
                        {
                            foreach (var item in model.供应商用户信息.协议供应商所属地区)
                            {
                                supplier.供应商用户信息.协议供应商所属地区.Add(item);
                            }
                        }
                    }
                }
                supplier.供应商用户信息.年检列表 = model.供应商用户信息.年检列表;
                supplier.供应商用户信息.应急供应商 = model.供应商用户信息.应急供应商;
                supplier.供应商用户信息.协议供应商 = model.供应商用户信息.协议供应商;
                supplier.供应商用户信息.用户来源 = model.供应商用户信息.用户来源;
                supplier.供应商用户信息.入库级别 = model.供应商用户信息.入库级别;
                supplier.供应商用户信息.认证级别 = model.供应商用户信息.认证级别;
                supplier.供应商用户信息.符合入库标准 = model.供应商用户信息.符合入库标准;
                supplier.信用评级信息.等级 = 1;
                supplier.审核数据.审核状态 = 审核状态.审核通过;
                supplier.审核数据.审核时间 = DateTime.Now;
                supplier.供应商用户信息.已提交 = true;
                supplier.审核数据.审核者.用户ID = currentUser.Id;
                用户管理.更新用户<供应商>(supplier);
                //用户管理.认证供应商(model.Id, currentUser.Id, 供应商.认证级别.已审核用户);
                deleteIndex("/Lucene.Net/IndexDic/Gys", model.Id.ToString());
                CreateIndex_gys(用户管理.查找用户<供应商>(model.Id), "/Lucene.Net/IndexDic/Gys");
                //deleteIndex_ProductCatalog("/Lucene.Net/IndexDic/GysCatalog", supplier.企业基本信息.企业名称);
                CreateIndex_ProductCatalog(supplier.企业基本信息.企业名称, "/Lucene.Net/IndexDic/GysCatalog");
                //判断是否发送短信
                if (Request.Form["isauditgyssendmial"] != null && Request.Form["isauditgyssendmial"] == "1")
                {
                    var UserNumber = supplier.企业联系人信息.联系人手机;//收信人列表
                    string MessageContent = "供应商您好，贵公司基本信息已经通过预审，请登录后台在线打印申报材料，请于工作时间内前往成都物资采购站提交申请表及核验原件，具体请查看供应商注册及须知。预约电话请咨询028-86686673（张助理）。（已通过部队审核的供应商，无需重复提审，请及时录入商品信息）";//短信内容
                    if (model.供应商用户信息.符合入库标准)
                    {
                        if (supplier.供应商用户信息.所属管理单位 == 供应商.采购管理单位.成都军区物资采购机构_重庆)
                        {
                            MessageContent = "供应商您好，贵公司基本信息已经通过预审，请登录后台在线打印申报材料，请于工作时间内前往重庆物资采购站提交申请表及核验原件，具体请查看供应商注册及须知。预约电话请咨询023-68778041（丁文元,助理）。（已通过部队审核的供应商，无需重复提审，请及时录入商品信息）";//短信内容
                        }
                        else if (supplier.供应商用户信息.所属管理单位 == 供应商.采购管理单位.成都军区物资采购机构_昆明)
                        {
                            MessageContent = "供应商您好，贵公司基本信息已经通过预审，请登录后台在线打印申报材料，请于工作时间内前往昆明物资采购站提交申请表及核验原件，具体请查看供应商注册及须知。预约电话请咨询0871-64783672（谢渝,助理）。（已通过部队审核的供应商，无需重复提审，请及时录入商品信息）";//短信内容
                        }
                        else if (supplier.供应商用户信息.所属管理单位 == 供应商.采购管理单位.成都军区物资采购机构_贵阳)
                        {
                            MessageContent = "供应商您好，贵公司基本信息已经通过预审，请登录后台在线打印申报材料，请于工作时间内前往贵阳物资采购站提交申请表及核验原件，具体请查看供应商注册及须知。预约电话请咨询13984817881（姚祖光,助理）。（已通过部队审核的供应商，无需重复提审，请及时录入商品信息）";//短信内容
                        }
                        else if (supplier.供应商用户信息.所属管理单位 == 供应商.采购管理单位.西藏军区物资采购中心)
                        {
                            MessageContent = "供应商您好，贵公司基本信息已经通过预审，请登录后台在线打印申报材料，请于工作时间内前往西藏物资采购中心提交申请表及核验原件，具体请查看供应商注册及须知。预约电话请咨询0891-6738571（梁助理）。（已通过部队审核的供应商，无需重复提审，请及时录入商品信息）";//短信内容
                        }
                    }
                    else
                    {
                        MessageContent = "供应商您好，恭喜您已经成为西南物资采购网的入网供应商。由于您所提供的资料还不符合申请入库的标准，故不能打印入库申请表格。如有任何疑问，请拨打网站客服电话咨询，谢谢。回复TD拒收";//短信内容
                    }
                    //供应商您好，贵企业基本信息已经通过预审，请登录后台在线打印申报材料，请于工作时间内前往{xxxxx}物资采购站提交申请表及核验原件{xx}具体请查看供应商注册及须知{xx}预约电话请咨询{xxxxxxxxxxxxxxxxxxxxxxxxxxx}已通过部队审核的供应商{xx}无需重复提审{xx}请及时录入商品信息{x}
                    string retstr = MailApiPost.PostDataGetHtml(UserNumber, MessageContent);
                }

            }
            catch (Exception)
            {
                return Redirect("/运营团队后台/Supplier_PssInfo");
            }
            return Content("<script>window.location='/运营团队后台/Supplier_PssInfo';</script>");
        }
        public ActionResult Accept()
        {
            try
            {
                long id = long.Parse(Request.QueryString["id"]);
                供应商 model = 用户管理.查找用户<供应商>(id);
                model.审核数据.审核时间 = DateTime.Now;
                model.审核数据.审核者.用户ID = currentUser.Id;
                model.审核数据.审核状态 = 审核状态.审核通过;
                model.供应商用户信息.已提交 = true;
                用户管理.更新用户<供应商>(model);
                if (Request.QueryString["msg"] != null && Request.QueryString["msg"] == "1")
                {
                    var UserNumber = model.企业联系人信息.联系人手机;//收信人列表
                    string MessageContent = "供应商您好，贵公司基本信息已经通过预审，请登录后台在线打印申报材料，请于工作时间内前往成都物资采购站提交申请表及核验原件，具体请查看供应商注册及须知。预约电话请咨询028-86686673（张助理）。（已通过部队审核的供应商，无需重复提审，请及时录入商品信息）";//短信内容
                    if (model.供应商用户信息.符合入库标准)
                    {
                        if (model.供应商用户信息.所属管理单位 == 供应商.采购管理单位.成都军区物资采购机构_重庆)
                        {
                            MessageContent = "供应商您好，贵公司基本信息已经通过预审，请登录后台在线打印申报材料，请于工作时间内前往重庆物资采购站提交申请表及核验原件，具体请查看供应商注册及须知。预约电话请咨询023-68778041（丁文元,助理）。（已通过部队审核的供应商，无需重复提审，请及时录入商品信息）";//短信内容
                        }
                        else if (model.供应商用户信息.所属管理单位 == 供应商.采购管理单位.成都军区物资采购机构_昆明)
                        {
                            MessageContent = "供应商您好，贵公司基本信息已经通过预审，请登录后台在线打印申报材料，请于工作时间内前往昆明物资采购站提交申请表及核验原件，具体请查看供应商注册及须知。预约电话请咨询0871-64783672（谢渝,助理）。（已通过部队审核的供应商，无需重复提审，请及时录入商品信息）";//短信内容
                        }
                        else if (model.供应商用户信息.所属管理单位 == 供应商.采购管理单位.成都军区物资采购机构_贵阳)
                        {
                            MessageContent = "供应商您好，贵公司基本信息已经通过预审，请登录后台在线打印申报材料，请于工作时间内前往贵阳物资采购站提交申请表及核验原件，具体请查看供应商注册及须知。预约电话请咨询13984817881（姚祖光,助理）。（已通过部队审核的供应商，无需重复提审，请及时录入商品信息）";//短信内容
                        }
                        else if (model.供应商用户信息.所属管理单位 == 供应商.采购管理单位.西藏军区物资采购中心)
                        {
                            MessageContent = "供应商您好，贵公司基本信息已经通过预审，请登录后台在线打印申报材料，请于工作时间内前往西藏物资采购中心提交申请表及核验原件，具体请查看供应商注册及须知。预约电话请咨询0891-6738571（梁助理）。（已通过部队审核的供应商，无需重复提审，请及时录入商品信息）";//短信内容
                        }
                    }
                    else
                    {
                        MessageContent = "供应商您好，恭喜您已经成为西南物资采购网的入网供应商。由于您所提供的资料还不符合申请入库的标准，故不能打印入库申请表格。如有任何疑问，请拨打网站客服电话咨询，谢谢。回复TD拒收";//短信内容
                    }
                    //供应商您好，贵企业基本信息已经通过预审，请登录后台在线打印申报材料，请于工作时间内前往{xxxxx}物资采购站提交申请表及核验原件{xx}具体请查看供应商注册及须知{xx}预约电话请咨询{xxxxxxxxxxxxxxxxxxxxxxxxxxx}已通过部队审核的供应商{xx}无需重复提审{xx}请及时录入商品信息{x}
                    string retstr = MailApiPost.PostDataGetHtml(UserNumber, MessageContent);
                }
                deleteIndex("/Lucene.Net/IndexDic/Gys", model.Id.ToString());
                CreateIndex_gys(model, "/Lucene.Net/IndexDic/Gys");
                return Content("<script>alert('成功审核供应商！');window.location='/运营团队后台/Modify_Supplier_Info?id=" + id.ToString() + "';</script>");
            }
            catch
            {
                return Redirect("/运营团队后台/");
            }
        }
        [HttpPost]
        public ActionResult Supplier_Accept_Detail(供应商 model)//接受供应商的加入
        {
            //供应商.修改认证级别(id, 供应商.认证级别.已审核用户);
            try
            {
                var d = DateTime.Now;
                供应商 supplier = 用户管理.查找用户<供应商>(model.Id);
                if (Request.Form["ns"] == "1" && !model.供应商用户信息.年检列表.ContainsKey(DateTime.Now.Year.ToString()))
                {
                    model.供应商用户信息.年检列表.Add(d.Year.ToString(), new 操作数据() { 操作时间 = d });
                }
                else
                {
                    model.供应商用户信息.年检列表 = supplier.供应商用户信息.年检列表;
                    model.供应商用户信息.年检列表.Remove(d.Year.ToString());
                }
                if (model.供应商用户信息.协议供应商)
                {
                    if (model.供应商用户信息.协议供应商所属地区 != null && model.供应商用户信息.协议供应商所属地区.Count() != 0)
                    {
                        if (supplier.供应商用户信息.协议供应商所属地区 == null || supplier.供应商用户信息.协议供应商所属地区.Count() == 0)
                        {
                            supplier.供应商用户信息.协议供应商所属地区 = model.供应商用户信息.协议供应商所属地区;
                        }
                        else
                        {
                            foreach (var item in model.供应商用户信息.协议供应商所属地区)
                            {
                                supplier.供应商用户信息.协议供应商所属地区.Add(item);
                            }
                        }
                    }
                }
                supplier.供应商用户信息.年检列表 = model.供应商用户信息.年检列表;
                supplier.供应商用户信息.应急供应商 = model.供应商用户信息.应急供应商;
                supplier.供应商用户信息.协议供应商 = model.供应商用户信息.协议供应商;
                supplier.供应商用户信息.用户来源 = model.供应商用户信息.用户来源;
                supplier.供应商用户信息.入库级别 = model.供应商用户信息.入库级别;
                supplier.供应商用户信息.认证级别 = model.供应商用户信息.认证级别;
                supplier.信用评级信息.等级 = 1;
                supplier.审核数据.审核状态 = 审核状态.审核通过;
                supplier.供应商用户信息.已提交 = true;
                supplier.审核数据.审核时间 = DateTime.Now;
                supplier.审核数据.审核者.用户ID = currentUser.Id;
                用户管理.更新用户<供应商>(supplier, false);
                //用户管理.认证供应商(model.Id, currentUser.Id, 供应商.认证级别.已审核用户);
                deleteIndex("/Lucene.Net/IndexDic/Gys", model.Id.ToString());
                //deleteIndex_ProductCatalog("/Lucene.Net/IndexDic/GysCatalog", supplier.企业基本信息.企业名称);
                if (supplier.审核数据.审核状态 == 审核状态.审核通过)
                {
                    CreateIndex_gys(用户管理.查找用户<供应商>(model.Id), "/Lucene.Net/IndexDic/Gys");
                    //CreateIndex_ProductCatalog(supplier.企业基本信息.企业名称,"/Lucene.Net/IndexDic/GysCatalog");
                }
                //string UserNumber = supplier.企业联系人信息.联系人手机;//收信人列表
                //string MessageContent = "供应商您好，贵企业基本信息已经通过预审，请登录后台在线打印申报材料，请于下周工作时间内前往成都物资采购站提交申请表及核验原件，具体请查看供应商注册及须知。预约电话请咨询028-86598698（苏志超助理）。（已通过部队审核的供应商，无需重复提审，请及时录入商品信息）";//短信内容

                //string retstr = MailApiPost.PostDataGetHtml(UserNumber, MessageContent);
            }
            catch (Exception)
            {
                return Redirect("/运营团队后台/Supplier_PssInfo");
            }


            return Content("<script>window.location='/运营团队后台/Supplier_PssInfo';</script>");
        }

        public ActionResult Supplier_Refuse(long id)
        {
            //TD
            //用户管理.认证供应商(id, currentUser.Id, 供应商.认证级别.审核未通过用户);
            用户管理.认证供应商(id, currentUser.Id, 供应商.认证级别.一级供应商);
            return Redirect("/运营团队后台/Supplier_WaitInfo");
        }
        public class Supplier
        {
            public long Id;
            public string Name;
            public string time;
            public string Industry;
            public string Area;
            public bool IsSubmit;
            public string Cls;
            public string status;
            public string number;
        }
        [HttpPost]
        public ActionResult AddFinancial(供应商 model)
        {
            供应商 Newmodel = 用户管理.查找用户<供应商>(model.Id);
            Newmodel.财务状况信息.Add(model.财务状况信息[0]);
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
            用户管理.更新用户<供应商>(Newmodel);
            deleteIndex("/Lucene.Net/IndexDic/Gys", model.Id.ToString());
            //deleteIndex_ProductCatalog("/Lucene.Net/IndexDic/GysCatalog", Newmodel.企业基本信息.企业名称);
            return Redirect("/运营团队后台/Modify_Supplier_Info?id=" + model.Id.ToString());
        }
        [HttpPost]
        public ActionResult AddServiceDepart(供应商 model)
        {
            供应商 Newmodel = 用户管理.查找用户<供应商>(model.Id);
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

            用户管理.更新用户<供应商>(Newmodel);
            deleteIndex("/Lucene.Net/IndexDic/Gys", model.Id.ToString());
            //deleteIndex_ProductCatalog("/Lucene.Net/IndexDic/GysCatalog", Newmodel.企业基本信息.企业名称);
            return Redirect("/运营团队后台/Modify_Supplier_Info?id=" + model.Id.ToString());
        }
        [HttpPost]
        public ActionResult AddInvestor(供应商 model)
        {
            供应商 Newmodel = 用户管理.查找用户<供应商>(model.Id);
            Newmodel.出资人或股东信息.Add(model.出资人或股东信息[0]);
            foreach (var item in Newmodel.出资人或股东信息)
            {
                if (string.IsNullOrWhiteSpace(item.出资人或股东性质) || string.IsNullOrWhiteSpace(item.出资人或股东姓名) || string.IsNullOrWhiteSpace(item.身份证号码) || string.IsNullOrWhiteSpace(item.国籍))
                {
                    item.已填写完整 = false;
                    break;
                }
                else
                {
                    item.已填写完整 = true;
                }
            }
            用户管理.更新用户<供应商>(Newmodel);
            deleteIndex("/Lucene.Net/IndexDic/Gys", model.Id.ToString());
            //deleteIndex_ProductCatalog("/Lucene.Net/IndexDic/GysCatalog", Newmodel.企业基本信息.企业名称);
            return Redirect("/运营团队后台/Modify_Supplier_Info?id=" + model.Id.ToString());
        }
        [HttpPost]
        public ActionResult AddToubiao(供应商 model)
        {
            供应商 Newmodel = 用户管理.查找用户<供应商>(model.Id);
            Newmodel.历史参标记录.Add(model.历史参标记录[0]);
            用户管理.更新用户<供应商>(Newmodel);
            deleteIndex("/Lucene.Net/IndexDic/Gys", model.Id.ToString());
            //deleteIndex_ProductCatalog("/Lucene.Net/IndexDic/GysCatalog", Newmodel.企业基本信息.企业名称);
            return Redirect("/运营团队后台/Modify_Supplier_Info?id=" + model.Id.ToString());
        }
        public ActionResult Delete_Servicdepartment()
        {
            try
            {
                long id = long.Parse(Request.QueryString["id"]);
                int index = int.Parse(Request.QueryString["index"]);
                供应商 Newmodel = 用户管理.查找用户<供应商>(id);
                if (index > Newmodel.售后服务机构列表.Count() || index < 0)
                {
                    return Redirect("/运营团队后台/Modify_Supplier_Info?id=" + id.ToString());
                }
                Newmodel.售后服务机构列表.RemoveAt(index);
                deleteIndex("/Lucene.Net/IndexDic/Gys", Newmodel.Id.ToString());
                //deleteIndex_ProductCatalog("/Lucene.Net/IndexDic/GysCatalog", Newmodel.企业基本信息.企业名称);
                return 用户管理.更新用户<供应商>(Newmodel)
                    ? Content("<script>alert('删除成功！');window.location='/运营团队后台/Modify_Supplier_Info?id=" + id.ToString() + "';</script>")
                    : Content("<script>window.location='/运营团队后台/Modify_Supplier_Info?id=" + id.ToString() + "';</script>")
                    ;
            }
            catch
            {
                return Redirect("/供应商后台/Service_Management");
            }
        }
        public ActionResult Delete_Investor()
        {
            long id = long.Parse(Request.QueryString["id"]);
            int index = int.Parse(Request.QueryString["index"]);
            供应商 Newmodel = 用户管理.查找用户<供应商>(id);
            if (index > Newmodel.出资人或股东信息.Count() || index < 0)
            {
                return Redirect("/运营团队后台/Modify_Supplier_Info?id=" + id.ToString());
            }
            Newmodel.出资人或股东信息.RemoveAt(index);
            deleteIndex("/Lucene.Net/IndexDic/Gys", Newmodel.Id.ToString());
            //deleteIndex_ProductCatalog("/Lucene.Net/IndexDic/GysCatalog", Newmodel.企业基本信息.企业名称);
            return 用户管理.更新用户<供应商>(Newmodel)
                ? Content("<script>alert('删除成功！');window.location='/运营团队后台/Modify_Supplier_Info?id=" + id.ToString() + "';</script>")
                : Content("<script>window.location='/运营团队后台/Modify_Supplier_Info?id=" + id.ToString() + "';</script>")
                ;
        }
        public ActionResult Delete_Finance()
        {
            long id = long.Parse(Request.QueryString["id"]);
            int index = int.Parse(Request.QueryString["index"]);
            供应商 Newmodel = 用户管理.查找用户<供应商>(id);
            if (index > Newmodel.财务状况信息.Count() || index < 0)
            {
                return Redirect("/运营团队后台/Modify_Supplier_Info?id=" + id.ToString());
            }
            Newmodel.财务状况信息.RemoveAt(index);
            deleteIndex("/Lucene.Net/IndexDic/Gys", Newmodel.Id.ToString());
            //deleteIndex_ProductCatalog("/Lucene.Net/IndexDic/GysCatalog", Newmodel.企业基本信息.企业名称);
            return 用户管理.更新用户<供应商>(Newmodel)
                ? Content("<script>alert('删除成功！');window.location='/运营团队后台/Modify_Supplier_Info?id=" + id.ToString() + "';</script>")
                : Content("<script>window.location='/运营团队后台/Modify_Supplier_Info?id=" + id.ToString() + "';</script>")
                ;
        }
        public ActionResult Delete_Toubiao()
        {
            try
            {
                long id = long.Parse(Request.QueryString["id"]);
                供应商 model = 用户管理.查找用户<供应商>(id);
                if (model == null)
                {
                    return Redirect("/运营团队后台/Modify_Supplier_Info?id=" + id.ToString());
                }
                int index = int.Parse(Request.QueryString["index"]);
                model.历史参标记录.RemoveAt(index);
                用户管理.更新用户<供应商>(model);
                deleteIndex("/Lucene.Net/IndexDic/Gys", model.Id.ToString());
                //deleteIndex_ProductCatalog("/Lucene.Net/IndexDic/GysCatalog", model.企业基本信息.企业名称);
                return Redirect("/运营团队后台/Modify_Supplier_Info?id=" + id.ToString());
            }
            catch
            {
                return Redirect("/运营团队后台/Supplier_PssInfo");
            }
        }
        //public ActionResult Search_Waiting_Supplier()
        //{
        //    int skip = int.Parse(Request.QueryString["skip"]);
        //    int pageSize = 0;
        //    List<Supplier> Slist = new List<Supplier>();
        //    long id;
        //    string name;
        //    string factory;
        //    string province;
        //    string city;
        //    string area;
        //    IMongoQuery q = null;
        //    if (Request.QueryString["num"] != "")
        //    {
        //        id = long.Parse(Request.QueryString["num"]);
        //        q = q.And(Query.EQ("_id", id));
        //    }
        //    name = Request.QueryString["name"];
        //    factory = Request.QueryString["factory"];
        //    province = Request.QueryString["provice"];
        //    city = Request.QueryString["city"];
        //    area = Request.QueryString["area"];
        //    if (name != "")
        //    {
        //        q = q.And(Query.EQ("企业基本信息.企业名称", new BsonRegularExpression(string.Format("/{0}/i", name))));
        //    }
        //    if (factory != "")
        //    {
        //        q = q.And(Query.EQ("企业基本信息.所属行业", new BsonRegularExpression(string.Format("/{0}/i", factory))));
        //    }
        //    if (province != "")
        //    {
        //        if (province != "不限省份")
        //        {
        //            q = q.And(Query.EQ("所属地域.省份", new BsonRegularExpression(string.Format("/{0}/i", province))));
        //        }
        //    }
        //    if (city != "")
        //    {
        //        if (city != "不限城市")
        //        {
        //            q = q.And(Query.EQ("所属地域.城市", new BsonRegularExpression(string.Format("/{0}/i", city))));
        //        }
        //    }
        //    if (area != "")
        //    {
        //        if (area != "不限区县")
        //        {
        //            q = q.And(Query.EQ("所属地域.区县", new BsonRegularExpression(string.Format("/{0}/i", area))));
        //        }
        //    }
        //    q = q.And(Query.EQ("审核数据.审核状态", 审核状态.未审核));
        //    var supplier = 用户管理.查询用户<供应商>(20 * (skip - 1), 20, q);
        //    pageSize = (用户管理.查询用户<供应商>(0, 0, q)).Count() / 20;
        //    int maintain = (用户管理.查询用户<供应商>(0, 0, q)).Count() % 20;
        //    if (maintain > 0)
        //    {
        //        pageSize++;
        //    }
        //    foreach (var item in supplier)
        //    {
        //        Supplier s = new Supplier();
        //        s.Cls = item.供应商用户信息.认证级别.ToString();
        //        s.Id = item.Id;
        //        s.Name = item.企业基本信息.企业名称;
        //        s.Area = item.所属地域.省份 + item.所属地域.城市 + item.所属地域.区县;
        //        s.IsSubmit = item.供应商用户信息.已提交;
        //        s.Industry = item.企业基本信息.所属行业;
        //        Slist.Add(s);
        //    }
        //    JsonResult js = new JsonResult() { Data = new { Slist = Slist, pageSize = pageSize } };
        //    return Json(js, JsonRequestBehavior.AllowGet);
        //}
        public ActionResult Modify_Toubiao()
        {
            try
            {
                long id = long.Parse(Request.QueryString["id"]);
                int index = int.Parse(Request.QueryString["index"]);
                供应商 Newmodel = 用户管理.查找用户<供应商>(id);
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
            供应商 Newmodel = 用户管理.查找用户<供应商>(model.Id);
            if (index > Newmodel.历史参标记录.Count() || index < 0)
            {
                return Redirect("/运营团队后台/Modify_Supplier_Info?id=" + model.Id.ToString() + "");
            }
            Newmodel.历史参标记录[index] = model.历史参标记录[0];
            用户管理.更新用户<供应商>(Newmodel);
            deleteIndex("/Lucene.Net/IndexDic/Gys", model.Id.ToString());
            //deleteIndex_ProductCatalog("/Lucene.Net/IndexDic/GysCatalog", Newmodel.企业基本信息.企业名称);
            return Redirect("/运营团队后台/Modify_Supplier_Info?id=" + model.Id.ToString());
        }
        public ActionResult Modify_Investor()
        {
            try
            {
                long id = long.Parse(Request.QueryString["id"]);
                int index = int.Parse(Request.QueryString["index"]);
                供应商 Newmodel = 用户管理.查找用户<供应商>(id);
                if (index > Newmodel.出资人或股东信息.Count() || index < 0)
                {
                    return Redirect("/运营团队后台/Modify_Supplier_Info?id=" + Newmodel.Id.ToString() + "");
                }
                ViewData["index"] = index;
                return View(Newmodel);
            }
            catch
            {
                return Redirect("/运营团队后台/Supplier_PssInfo");
            }
        }
        [HttpPost]
        public ActionResult Modify_investor(供应商 model)
        {
            try
            {
                int index = int.Parse(Request.Form["index"].ToString());
                供应商 Newmodel = 用户管理.查找用户<供应商>(model.Id);
                if (index > Newmodel.出资人或股东信息.Count() || index < 0)
                {
                    return Redirect("/运营团队后台/Modify_Supplier_Info?id=" + Newmodel.Id.ToString() + "");
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
                    ? Content("<script>alert('修改成功！');window.location='/运营团队后台/Modify_Supplier_Info?id=" + model.Id + "';</script>")
                    : Content("<script>window.location='/运营团队后台/Modify_Supplier_Info?id=" + model.Id + "';</script>")
                    ;
            }
            catch
            {
                return Redirect("/运营团队后台/Supplier_PssInfo");
            }
        }
        public ActionResult Modify_Servicdepartment()
        {
            try
            {
                long id = long.Parse(Request.QueryString["id"]);
                int index = int.Parse(Request.QueryString["index"]);
                供应商 Newmodel = 用户管理.查找用户<供应商>(id);
                if (index > Newmodel.售后服务机构列表.Count() || index < 0)
                {
                    return Redirect("/运营团队后台/Supplier_PssInfo");
                }
                ViewData["index"] = index;
                return View(Newmodel);
            }
            catch
            {
                return Redirect("/运营团队后台/Supplier_PssInfo");
            }
        }
        [HttpPost]
        public ActionResult Modify_servicdepartment(供应商 model)
        {
            try
            {
                int index = int.Parse(Request.Form["index"].ToString());
                供应商 Newmodel = 用户管理.查找用户<供应商>(model.Id);
                if (index > Newmodel.售后服务机构列表.Count() || index < 0)
                {
                    return Redirect("/运营团队后台/Modify_Supplier_Info?id=" + model.Id.ToString() + "");
                }
                Newmodel.售后服务机构列表[index] = model.售后服务机构列表[0];
                return 用户管理.更新用户<供应商>(Newmodel)
                    ? Content("<script>alert('修改成功！');window.location='/运营团队后台/Modify_Supplier_Info?id=" + model.Id.ToString() + "';</script>")
                    : Content("<script>window.location='/运营团队后台/Modify_Supplier_Info?id=" + model.Id.ToString() + "';</script>")
                    ;
            }
            catch
            {
                return Redirect("/运营团队后台/Supplier_PssInfo");
            }
        }
        public ActionResult Search_Supplier()
        {
            DateTime stime = new DateTime(2014, 7, 1);
            DateTime etime = new DateTime(2016, 7, 1);
            if (!string.IsNullOrWhiteSpace(Request.QueryString["stime"]))
            {
                stime = DateTime.Parse(Request.QueryString["stime"]);
            }
            if (!string.IsNullOrWhiteSpace(Request.QueryString["etime"]))
            {
                etime = DateTime.Parse(Request.QueryString["etime"]);
            }
            int skip = int.Parse(Request.QueryString["skip"]);
            int pageSize = 0;
            IMongoQuery condition = null;
            List<Supplier> Slist = new List<Supplier>();
            string name;
            string factory;
            int cls;
            string province;
            string city;
            string area;
            int isSub = int.Parse(Request.QueryString["sub"]);
            province = Request.QueryString["provice"];
            city = Request.QueryString["city"];
            area = Request.QueryString["area"];
            int sta = int.Parse(Request.QueryString["sts"]);
            name = Request.QueryString["name"];
            factory = Request.QueryString["factory"];
            cls = int.Parse(Request.QueryString["cls"]);
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
            if (cls != 1)
            {
                condition = condition.And(Query.EQ("供应商用户信息.认证级别", cls));
            }
            if (sta != -1)
            {
                condition = condition.And(Query.EQ("审核数据.审核状态", sta));
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
            if (isSub == 1)
            {
                supplier = 用户管理.查询用户<供应商>(0, 0, condition).OrderBy(m => m.基本数据.添加时间).Where(m => m.基本数据.添加时间 >= stime && m.基本数据.添加时间 <= etime).Skip(20 * (skip - 1)).Take(20);
                pageSize = (用户管理.查询用户<供应商>(0, 0, condition)).Where(m => m.基本数据.添加时间 >= stime && m.基本数据.添加时间 <= etime).Count() / 20;
                maintain = (用户管理.查询用户<供应商>(0, 0, condition)).Where(m => m.基本数据.添加时间 >= stime && m.基本数据.添加时间 <= etime).Count() % 20;
            }
            else
            {
                supplier = 用户管理.查询用户<供应商>(0, 0).OrderBy(m => m.供应商用户信息.已提交 == false).Skip(20 * (skip - 1)).Take(20).OrderByDescending(m => m.基本数据.添加时间);
                pageSize = (用户管理.查询用户<供应商>(0, 0)).Count() / 20;
                maintain = (用户管理.查询用户<供应商>(0, 0)).Count() % 20;
            }
            if (maintain > 0)
            {
                pageSize++;
            }
            foreach (var item in supplier)
            {
                Supplier s = new Supplier();
                s.Cls = item.供应商用户信息.认证级别.ToString();
                s.time = item.基本数据.添加时间.ToString("yyyy-MM-dd");
                s.Id = item.Id;
                s.Name = item.企业基本信息.企业名称;
                if (string.IsNullOrWhiteSpace(item.企业基本信息.所属行业))
                {
                    s.Industry = "";
                }
                else
                {
                    s.Industry = item.企业基本信息.所属行业;
                }
                if (item.供应商用户信息.U盾信息 != null && !string.IsNullOrWhiteSpace(item.供应商用户信息.U盾信息.序列号))
                {
                    s.number = "已购买";
                }
                else
                {
                    s.number = "未购买";
                }
                s.status = item.审核数据.审核状态.ToString();
                s.IsSubmit = item.供应商用户信息.已提交;
                Slist.Add(s);
            }
            JsonResult js = new JsonResult() { Data = new { Slist = Slist, pageSize = pageSize } };
            return Json(js, JsonRequestBehavior.AllowGet);
        }
        public ActionResult Del_gysPic()
        {
            try
            {
                long id = long.Parse(Request.QueryString["id"]);
                int index = int.Parse(Request.QueryString["index"]);
                供应商 Newmodel = 用户管理.查找用户<供应商>(id);
                if (System.IO.File.Exists(Server.MapPath(Newmodel.供应商用户信息.供应商图片.ElementAt(index))))
                {
                    System.IO.File.Delete(Server.MapPath(Newmodel.供应商用户信息.供应商图片.ElementAt(index)));
                    Newmodel.供应商用户信息.供应商图片.RemoveAt(index);
                    用户管理.更新用户<供应商>(Newmodel);
                }
                return Redirect("/运营团队后台/Modify_Supplier_Info?id=" + id.ToString());
            }
            catch
            {
                return Redirect("/运营团队后台/");
            }
        }


        public int Del_YsdPic()
        {
            try
            {
                long id = long.Parse(Request.QueryString["id"]);  //验收单号
                var index = Request.QueryString["index"];      //扫描件
                var Newmodel = 验收单管理.查找验收单(id);
                if (System.IO.File.Exists(Server.MapPath(@index)))
                {
                    System.IO.File.Delete(Server.MapPath(@index));
                    var item = Newmodel.验收单扫描件.Find(o => o.回传单路径 == index);
                    Newmodel.验收单扫描件.Remove(item);
                    验收单管理.更新验收单(Newmodel);
                }
                return 1;
            }
            catch
            {
                return 0;
            }
        }
        public int DelYsdSmj()
        {
            string uri = Request.QueryString["n"];
            long id = long.Parse(Request.QueryString["gid"].ToString());
            var model = 验收单管理.查找验收单(id);
            if (model.验收单扫描件.Count > 0 && !string.IsNullOrWhiteSpace(uri))
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
        public ActionResult Requstion_Del()//删除建议
        {
            try
            {
                int id = int.Parse(Request.QueryString["id"]);
                if (建议管理.删除建议(id))
                {
                    return View("Requestion_No_Replied");
                }
                else
                {
                    return View("Requestion_No_Replied");
                }
            }
            catch
            {
                return Content("<script>window.location='/运营团队后台/Requestion_No_Replied';</script>");
            }
        }
        public ActionResult Suggestion_Del()//删除投诉
        {
            try
            {
                int id = int.Parse(Request.QueryString["id"]);
                if (投诉管理.删除投诉(id))
                {
                    return View("Suggestion_NoPss");
                }
                else
                {
                    return View("Suggestion_NoPss");
                }
            }
            catch
            {
                return Content("<script>window.location='/运营团队后台/Suggestion_NoPss';</script>");
            }

        }
        public ActionResult Request_Detail()//建议详情
        {
            return View();
        }
        public ActionResult Part_Request_Detail()//建议详情
        {
            try
            {
                int id = int.Parse(Request.QueryString["id"]);
                建议 model = 建议管理.查找建议(id);
                return PartialView("Part_View/Part_Request_Detail", model);
            }
            catch
            {
                return Content("<script>window.location='/运营团队后台/Requestion_Replied';</script>");
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

            //string contents = Request.Form["reply"].ToString();

        }
        public ActionResult Part_Suggestion_ReplyPg()//投诉回复页
        {
            try
            {
                var model = 投诉管理.查找投诉(int.Parse(Request.QueryString["id"]));
                return PartialView("Part_View/Part_Suggestion_ReplyPg", model);
            }
            catch
            {
                return Redirect("/运营团队后台/Suggestion_NoPss");
            }

        }
        public ActionResult Suggestiont_Reply_Page()
        {
            return View();
        }
        public ActionResult Requestion_No_Replied()//未回复建议页面
        {
            return View();
        }
        public ActionResult Requestion_Replied()//已回复建议页面
        {

            return View();
        }

        public ActionResult Suggestion_Reply()//投诉回复页面
        {
            return View();
        }
        public ActionResult Suggestion_NoPss()//未处理投诉页面
        {
            return View();
        }
        public ActionResult Suggestion()//已处理投诉页面
        {
            return View();
        }
        public ActionResult Supplier_PssInfo()//已审核供应商页面
        {
            return View();
        }
        public ActionResult Ad_User_Info()//添加网站用户
        {
            return View();
        }

        public ActionResult PushMessageResultStaticPage()//推送消息发送显示结果页面
        {
            return View();
        }

        public ActionResult Part_SendPushList(int? page, int? type)//公告推送消息列表
        {
            IMongoQuery q = null;
            if (type != -1)
            {
                q = q.And(Query.EQ("审核数据.推送信息状态", type));
            }

            int PUSH_LISTCOUNT = (int)公告推送管理.计数公告推送(0, 0, q);
            int PUSH_MAXPAGE = Math.Max((PUSH_LISTCOUNT + 5 - 1) / 5, 1);
            if (string.IsNullOrEmpty(page.ToString()) || page < 0 || page > PUSH_LISTCOUNT)
            {
                page = 1;
            }

            ViewData["adpushcurrentpage"] = page;
            ViewData["adpushpagecount"] = PUSH_MAXPAGE;
            ViewData["公告推送列表"] = 公告推送管理.查询公告推送(5 * (int.Parse(page.ToString()) - 1), 5, q);

            return PartialView("Part_View/Part_SendPushList");
        }
        public ActionResult Part_SendPushList_Us(int? page, int? type)//一般推送消息列表
        {
            IMongoQuery q = null;
            if (type != -1)
            {
                q = q.And(Query.EQ("审核数据.推送信息状态", type));
            }

            int PUSH_LISTCOUNT = (int)一般推送管理.计数一般推送(0, 0, q);
            int PUSH_MAXPAGE = Math.Max((PUSH_LISTCOUNT + 5 - 1) / 5, 1);
            if (string.IsNullOrEmpty(page.ToString()) || page < 0 || page > PUSH_LISTCOUNT)
            {
                page = 1;
            }

            ViewData["uspushcurrentpage"] = page;
            ViewData["uspushpagecount"] = PUSH_MAXPAGE;
            ViewData["一般推送列表"] = 一般推送管理.查询一般推送(5 * (int.Parse(page.ToString()) - 1), 5, q);

            return PartialView("Part_View/Part_SendPushList_Us");
        }
        public ActionResult Part_SendAdList(int? page)//公告列表
        {
            int listcount = (int)(公告管理.计数公告(0, 0, Query.EQ("审核数据.审核状态", 审核状态.审核通过)));
            int maxpage = Math.Max((listcount + MESSAGE_PAGESIZE - 1) / MESSAGE_PAGESIZE, 1);
            if (string.IsNullOrEmpty(page.ToString()) || page < 0 || page > maxpage)
            {
                page = 1;
            }

            ViewData["listcount"] = listcount;
            ViewData["pagesize"] = MESSAGE_PAGESIZE;
            ViewData["currentpage"] = page;
            ViewData["pagecount"] = maxpage;
            ViewData["后台公告列表"] = 公告管理.查询公告(MESSAGE_PAGESIZE * (int.Parse(page.ToString()) - 1), MESSAGE_PAGESIZE);

            return PartialView("Part_View/Part_SendAdList");
        }
        public ActionResult SendMessageList()
        {
            return View();
        }
        public ActionResult Part_SendPushListTypeChange(int? type)
        {
            IMongoQuery q = null;
            if (type != -1)
            {
                q = q.And(Query.EQ("审核数据.推送信息状态", type));
            }

            int page = 1;
            int listcount = (int)公告推送管理.计数公告推送(0, 0, q);
            int maxpage = Math.Max((listcount + MESSAGE_PAGESIZE - 1) / MESSAGE_PAGESIZE, 1);

            ViewData["adpushcurrentpage"] = page;
            ViewData["adpushpagecount"] = maxpage;
            ViewData["公告推送列表"] = 公告推送管理.查询公告推送(0, MESSAGE_PAGESIZE, q);
            return PartialView("Part_View/Part_SendPushList");
        }
        public ActionResult Part_SendPushListTypeChange_Us(int? type)
        {
            IMongoQuery q = null;
            if (type != -1)
            {
                q = q.And(Query.EQ("审核数据.推送信息状态", type));
            }

            int page = 1;
            int listcount = (int)一般推送管理.计数一般推送(0, 0, q);
            int maxpage = Math.Max((listcount + MESSAGE_PAGESIZE - 1) / MESSAGE_PAGESIZE, 1);

            ViewData["uspushcurrentpage"] = page;
            ViewData["uspushpagecount"] = maxpage;
            ViewData["一般推送列表"] = 一般推送管理.查询一般推送(0, MESSAGE_PAGESIZE, q);
            return PartialView("Part_View/Part_SendPushList_Us");
        }
        public ActionResult Part_SendMessageList()
        {
            int page = 1;

            int listcount = (int)公告管理.计数公告(0, 0, Query.EQ("审核数据.审核状态", 审核状态.审核通过));
            int maxpage = Math.Max((listcount + MESSAGE_PAGESIZE - 1) / MESSAGE_PAGESIZE, 1);

            ViewData["currentpage"] = page;
            ViewData["pagecount"] = maxpage;
            ViewData["后台公告列表"] = 公告管理.查询公告(0, MESSAGE_PAGESIZE, Query.EQ("审核数据.审核状态", 审核状态.审核通过));

            //公告推送信息列表
            listcount = (int)公告推送管理.计数公告推送(0, 0, Query.EQ("审核数据.推送信息状态", 推送信息状态.未提交));
            maxpage = Math.Max((listcount + MESSAGE_PAGESIZE - 1) / MESSAGE_PAGESIZE, 1);

            ViewData["adpushcurrentpage"] = page;
            ViewData["adpushpagecount"] = maxpage;
            ViewData["公告推送列表"] = 公告推送管理.查询公告推送(0, MESSAGE_PAGESIZE, Query.EQ("审核数据.推送信息状态", 推送信息状态.未提交));

            //一般推送信息列表
            listcount = (int)一般推送管理.计数一般推送(0, 0, Query.EQ("审核数据.推送信息状态", 推送信息状态.未提交));
            maxpage = Math.Max((listcount + MESSAGE_PAGESIZE - 1) / MESSAGE_PAGESIZE, 1);

            ViewData["uspushcurrentpage"] = page;
            ViewData["uspushpagecount"] = maxpage;
            ViewData["一般推送列表"] = 一般推送管理.查询一般推送(0, MESSAGE_PAGESIZE, Query.EQ("审核数据.推送信息状态", 推送信息状态.未提交));

            return PartialView("Part_View/Part_SendMessageList");
        }
        public ActionResult PushMessageDetail()
        {
            return View();
        }
        public ActionResult Part_PushMessageDetail(int? id)
        {
            公告 g = null;
            if (null != id)
            {
                g = 公告管理.查找公告((long)id);
            }
            ViewData["站内消息模板"] = 推送模板管理.查询推送模板(0, 0, Query.EQ("推送类型", 推送类型.站内消息));
            ViewData["短信消息模板"] = 推送模板管理.查询推送模板(0, 0, Query.EQ("推送类型", 推送类型.短信消息));
            ViewData["邮件消息模板"] = 推送模板管理.查询推送模板(0, 0, Query.EQ("推送类型", 推送类型.邮件消息));
            ViewData["微信消息模板"] = 推送模板管理.查询推送模板(0, 0, Query.EQ("推送类型", 推送类型.微信消息));

            ViewData["行业列表"] = 商品分类管理.查找子分类();
            return PartialView("Part_View/Part_PushMessageDetail", g);
        }
        public ActionResult Part_PushMessageModify(int? id)
        {
            ViewData["行业列表"] = 商品分类管理.查找子分类();
            公告推送 g = 公告推送管理.查找公告推送((long)id);
            ViewData["公告推送信息"] = g;

            return PartialView("Part_View/Part_PushMessageModify");
        }
        public ActionResult PushMessageModify()
        {
            return View();
        }
        public ActionResult Part_PushMessageUsModify(int? id)
        {
            ViewData["行业列表"] = 商品分类管理.查找子分类();
            一般推送 g = 一般推送管理.查找一般推送((long)id);
            ViewData["一般推送信息"] = g;

            return PartialView("Part_View/Part_PushMessageUsModify");
        }
        public ActionResult PushMessageUsModify()
        {
            return View();
        }
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult PushMessageModify(string action, int? id)
        {
            string gysidlist = Request.Form["gysidlist"];
            string znxx = Request.Form["znxxtextarea"];
            string message = Request.Form["messagetextarea"];
            string mail = Request.Form["mailtextarea"];
            string wx = Request.Form["weixinmailtextarea"];
            string title = Request.Form["pushtitle"];
            string Adid = Request["connectionid"];

            公告推送 model = new 公告推送();
            model.Id = (long)id;
            model.操作人员.用户ID = this.HttpContext.获取当前用户<运营团队>().Id;
            model.关联公告.公告ID = long.Parse(Adid);
            if (!string.IsNullOrEmpty(gysidlist))
            {
                List<用户链接<用户基本数据>> i = new List<用户链接<用户基本数据>>();
                List<string> idlist = gysidlist.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries).ToList();

                List<string> maillist = new List<string>();
                List<string> messagelist = new List<string>();
                List<string> weixinlist = new List<string>();
                foreach (var item in idlist)
                {
                    maillist.Add(用户管理.查找用户(long.Parse(item)).联系方式.电子邮件);
                    messagelist.Add(用户管理.查找用户(long.Parse(item)).联系方式.手机);
                    weixinlist.Add(用户管理.查找用户(long.Parse(item)).联系方式.微信);
                }
                model.电子邮件推送数据.收信人列表 = maillist;
                model.短信推送数据.收信人列表 = messagelist;
                model.微信推送数据.收信人列表 = weixinlist;
                model.站内消息推送数据.收信人列表 = idlist.Select(m => new 用户链接<用户基本数据>() { 用户ID = long.Parse(m) }).ToList();
            }
            model.电子邮件推送数据.内容 = mail;
            model.电子邮件推送数据.标题 = title;
            model.短信推送数据.内容 = message;
            model.微信推送数据.内容 = wx;
            model.微信推送数据.标题 = title;
            model.站内消息推送数据.内容 = znxx;
            model.站内消息推送数据.标题 = title;

            if (action == "保存")
            {
                model.审核数据.推送信息状态 = 推送信息状态.未提交;
                公告推送管理.更新公告推送(model);
            }
            else if (action == "提交")
            {
                model.审核数据.推送信息状态 = 推送信息状态.未审核;
                公告推送管理.更新公告推送(model);
            }

            return View("SendMessageList");
        }
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult PushMessageUsModify(string action, int? id)
        {
            string gysidlist = Request.Form["gysidlist"];
            string znxx = Request.Form["znxxtextarea"];
            string message = Request.Form["messagetextarea"];
            string mail = Request.Form["mailtextarea"];
            string wx = Request.Form["weixinmailtextarea"];
            string title = Request.Form["pushtitle"];
            一般推送 model = new 一般推送();
            model.Id = (long)id;
            model.操作人员.用户ID = this.HttpContext.获取当前用户<运营团队>().Id;

            if (!string.IsNullOrEmpty(gysidlist))
            {
                List<用户链接<用户基本数据>> i = new List<用户链接<用户基本数据>>();
                List<string> idlist = gysidlist.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries).ToList();

                List<string> maillist = new List<string>();
                List<string> messagelist = new List<string>();
                List<string> weixinlist = new List<string>();
                foreach (var item in idlist)
                {
                    maillist.Add(用户管理.查找用户(long.Parse(item)).联系方式.电子邮件);
                    messagelist.Add(用户管理.查找用户(long.Parse(item)).联系方式.手机);
                    weixinlist.Add(用户管理.查找用户(long.Parse(item)).联系方式.微信);
                }

                model.电子邮件推送数据.收信人列表 = maillist;
                model.短信推送数据.收信人列表 = messagelist;
                model.微信推送数据.收信人列表 = weixinlist;
                model.站内消息推送数据.收信人列表 = idlist.Select(m => new 用户链接<用户基本数据>() { 用户ID = long.Parse(m) }).ToList();

            }
            model.电子邮件推送数据.内容 = mail;
            model.电子邮件推送数据.标题 = title;
            model.短信推送数据.内容 = message;
            model.微信推送数据.内容 = wx;
            model.微信推送数据.标题 = title;
            model.站内消息推送数据.内容 = znxx;
            model.站内消息推送数据.标题 = title;

            if (action == "保存")
            {
                model.审核数据.推送信息状态 = 推送信息状态.未提交;
                一般推送管理.更新一般推送(model);
            }
            else if (action == "提交")
            {
                model.审核数据.推送信息状态 = 推送信息状态.未审核;
                一般推送管理.更新一般推送(model);
            }
            return View("SendMessageList");
        }

        [HttpPost]
        public ActionResult GetGysListByCondition()
        {
            string place = Request.Params["place"];
            string hy = Request.Params["hy"];

            var q = Query<供应商>.EQ(o => o.审核数据.审核状态, 审核状态.审核通过);
            if (place != "||")
            {
                string[] a = place.Split('|');
                if (!string.IsNullOrEmpty(a[0]))
                {
                    q = q.And(Query.EQ("所属地域.省份", a[0]));
                    if (!string.IsNullOrEmpty(a[1]) && a[1] != "不限")
                    {
                        q = q.And(Query.EQ("所属地域.城市", a[1]));
                        if (!string.IsNullOrEmpty(a[2]) && a[2] != "不限")
                        {
                            q = q.And(Query.EQ("所属地域.区县", a[2]));
                        }
                    }
                }
            }
            if (hy != "请选择行业")
            {
                q = q.And(Query.EQ("企业基本信息.所属行业", hy));
            }

            ViewData["供应商集合"] = 用户管理.查询用户<供应商>(0, 0, q);

            return PartialView("Part_View/Part_PushDetailGysList");
        }
        public ActionResult PushAdMessageDelete(int? id)
        {
            公告推送管理.删除公告推送((long)id);
            return View("SendMessageList");
        }
        public ActionResult PushUsMessageDelete(int? id)
        {
            一般推送管理.删除一般推送((long)id);
            return View("SendMessageList");
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult PushMessageDetail(string action, int? id)
        {
            string gysidlist = Request.Form["gysidlist"];
            string znxx = Request.Form["znxxtextarea"];
            string message = Request.Form["messagetextarea"];
            string mail = Request.Form["mailtextarea"];
            string wx = Request.Form["weixinmailtextarea"];
            string title = Request.Form["pushtitle"];

            if (null != id)
            {
                公告推送 model = new 公告推送();
                model.操作人员.用户ID = this.HttpContext.获取当前用户<运营团队>().Id;
                model.关联公告.公告ID = (long)id;
                if (!string.IsNullOrEmpty(gysidlist))
                {
                    List<用户链接<用户基本数据>> i = new List<用户链接<用户基本数据>>();
                    List<string> idlist = gysidlist.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries).ToList();

                    List<string> maillist = new List<string>();
                    List<string> messagelist = new List<string>();
                    List<string> weixinlist = new List<string>();
                    foreach (var item in idlist)
                    {
                        maillist.Add(用户管理.查找用户(long.Parse(item)).联系方式.电子邮件);
                        messagelist.Add(用户管理.查找用户(long.Parse(item)).联系方式.手机);
                        weixinlist.Add(用户管理.查找用户(long.Parse(item)).联系方式.微信);
                    }

                    model.电子邮件推送数据.收信人列表 = maillist;
                    model.短信推送数据.收信人列表 = messagelist;
                    model.微信推送数据.收信人列表 = weixinlist;
                    model.站内消息推送数据.收信人列表 = idlist.Select(m => new 用户链接<用户基本数据>() { 用户ID = long.Parse(m) }).ToList();

                }
                model.电子邮件推送数据.内容 = mail;
                model.电子邮件推送数据.标题 = title;
                model.短信推送数据.内容 = message;
                model.微信推送数据.内容 = wx;
                model.微信推送数据.标题 = title;
                model.站内消息推送数据.内容 = znxx;
                model.站内消息推送数据.标题 = title;

                if (action == "保存")
                {
                    model.审核数据.推送信息状态 = 推送信息状态.未提交;
                    公告推送管理.添加公告推送(model);
                }
                else if (action == "提交")
                {
                    model.审核数据.推送信息状态 = 推送信息状态.未审核;
                    公告推送管理.添加公告推送(model);
                }
            }
            else
            {
                一般推送 model = new 一般推送();
                model.操作人员.用户ID = this.HttpContext.获取当前用户<运营团队>().Id;

                if (!string.IsNullOrEmpty(gysidlist))
                {
                    List<用户链接<用户基本数据>> i = new List<用户链接<用户基本数据>>();
                    List<string> idlist = gysidlist.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries).ToList();

                    List<string> maillist = new List<string>();
                    List<string> messagelist = new List<string>();
                    List<string> weixinlist = new List<string>();
                    foreach (var item in idlist)
                    {
                        maillist.Add(用户管理.查找用户(long.Parse(item)).联系方式.电子邮件);
                        messagelist.Add(用户管理.查找用户(long.Parse(item)).联系方式.手机);
                        weixinlist.Add(用户管理.查找用户(long.Parse(item)).联系方式.微信);
                    }

                    model.电子邮件推送数据.收信人列表 = maillist;
                    model.短信推送数据.收信人列表 = messagelist;
                    model.微信推送数据.收信人列表 = weixinlist;
                    model.站内消息推送数据.收信人列表 = idlist.Select(m => new 用户链接<用户基本数据>() { 用户ID = long.Parse(m) }).ToList();

                }
                model.电子邮件推送数据.内容 = mail;
                model.电子邮件推送数据.标题 = title;
                model.短信推送数据.内容 = message;
                model.微信推送数据.内容 = wx;
                model.微信推送数据.标题 = title;
                model.站内消息推送数据.内容 = znxx;
                model.站内消息推送数据.标题 = title;

                if (action == "保存")
                {
                    model.审核数据.推送信息状态 = 推送信息状态.未提交;
                    一般推送管理.添加一般推送(model);
                }
                else if (action == "提交")
                {
                    model.审核数据.推送信息状态 = 推送信息状态.未审核;
                    一般推送管理.添加一般推送(model);
                }
            }
            return View("SendMessageList");
        }

        public ActionResult SendMessageAudit()
        {
            return View();
        }
        public ActionResult Part_SendMessageAudit(int? type, int? id)
        {
            try
            {
                if (type != null)
                {
                    公告推送 ad = 公告推送管理.查找公告推送((long)id);
                    return PartialView("Part_View/Part_SendMessageAudit", ad);
                }
                else
                {
                    一般推送 ad = 一般推送管理.查找一般推送((long)id);
                    return PartialView("Part_View/Part_SendMessageAudit", ad);
                }
            }
            catch
            {
                return Content("<script>alert('输入有误!请检查数据格式');location.href='javascript:history.go(-1)';</script>");
            }
        }
        public ActionResult SendMessageAuditList()
        {
            return View();
        }
        public ActionResult Part_SendMessageAuditList()
        {
            int page = 1;
            //公告推送信息列表
            int listcount = (int)公告推送管理.计数公告推送(0, 0, Query.EQ("审核数据.推送信息状态", 推送信息状态.未审核));
            int maxpage = Math.Max((listcount + MESSAGE_PAGESIZE - 1) / MESSAGE_PAGESIZE, 1);

            ViewData["adpushcurrentpage"] = page;
            ViewData["adpushpagecount"] = maxpage;
            ViewData["公告推送列表"] = 公告推送管理.查询公告推送(0, MESSAGE_PAGESIZE, Query.EQ("审核数据.推送信息状态", 推送信息状态.未审核));

            //一般推送信息列表
            listcount = (int)一般推送管理.计数一般推送(0, 0, Query.EQ("审核数据.推送信息状态", 推送信息状态.未审核));
            maxpage = Math.Max((listcount + MESSAGE_PAGESIZE - 1) / MESSAGE_PAGESIZE, 1);

            ViewData["uspushcurrentpage"] = page;
            ViewData["uspushpagecount"] = maxpage;
            ViewData["一般推送列表"] = 一般推送管理.查询一般推送(0, MESSAGE_PAGESIZE, Query.EQ("审核数据.推送信息状态", 推送信息状态.未审核));

            return PartialView("Part_View/Part_SendMessageAuditList");
        }
        public ActionResult Part_SendPushAuditList(int? page)//公告推送消息列表
        {
            int listcount = (int)公告推送管理.计数公告推送(0, 0, Query.EQ("审核数据.推送信息状态", 推送信息状态.未审核));
            int maxpage = Math.Max((listcount + MESSAGE_PAGESIZE - 1) / MESSAGE_PAGESIZE, 1);
            if (string.IsNullOrEmpty(page.ToString()) || page < 0 || page > maxpage)
            {
                page = 1;
            }

            ViewData["adpushcurrentpage"] = page;
            ViewData["adpushpagecount"] = maxpage;
            ViewData["公告推送列表"] = 公告推送管理.查询公告推送(MESSAGE_PAGESIZE * (int.Parse(page.ToString()) - 1), MESSAGE_PAGESIZE, Query.EQ("审核数据.推送信息状态", 推送信息状态.未审核));

            return PartialView("Part_View/Part_SendPushAuditList");
        }
        public ActionResult Part_SendPushAuditList_Us(int? page)//一般推送消息列表
        {
            int listcount = (int)一般推送管理.计数一般推送(0, 0, Query.EQ("审核数据.推送信息状态", 推送信息状态.未审核));
            int maxpage = Math.Max((listcount + MESSAGE_PAGESIZE - 1) / MESSAGE_PAGESIZE, 1);
            if (string.IsNullOrEmpty(page.ToString()) || page < 0 || page > maxpage)
            {
                page = 1;
            }

            ViewData["uspushcurrentpage"] = page;
            ViewData["uspushpagecount"] = maxpage;
            ViewData["一般推送列表"] = 一般推送管理.查询一般推送(MESSAGE_PAGESIZE * (int.Parse(page.ToString()) - 1), MESSAGE_PAGESIZE, Query.EQ("审核数据.推送信息状态", 推送信息状态.未审核));

            return PartialView("Part_View/Part_SendPushAuditList_Us");
        }

        [HttpPost]
        public ActionResult SendMessageAudit(string action, int? id)
        {
            if (action == "审核通过并发送")
            {
                string UserNumber = "";

                if (Request.Form["connectionid"] != null)
                {
                    公告推送 ad = 公告推送管理.查找公告推送((long)id);
                    foreach (var item in ad.短信推送数据.收信人列表)
                    {
                        UserNumber += item + ",";
                    }
                    UserNumber = UserNumber.Substring(0, UserNumber.Length - 1);//收信人列表
                    //UserNumber = "18382061371,";

                    string MessageContent = ad.短信推送数据.内容;//短信内容
                    string rethtml = MailApiPost.PostDataGetHtml(UserNumber, MessageContent);//result=2&description=账号无效或未开户


                    string[] retlist = rethtml.Split('&');
                    if (retlist[0].Replace("result=", "") != "0")
                    {
                        return
                            Content("<script>alert('" + retlist[1].Replace("description=", "") +
                                    "');location.href='javascript:history.go(-1)';</script>");
                    }
                    else
                    {
                        公告推送管理.审核公告推送((long)id, currentUser.Id, 推送信息状态.审核通过);

                        ViewData["sendmessageresult"] = rethtml;
                        return View("PushMessageResultStaticPage");
                    }


                }
                else
                {
                    一般推送 ad = 一般推送管理.查找一般推送((long)id);
                    foreach (var item in ad.短信推送数据.收信人列表)
                    {
                        UserNumber += item + ",";
                    }
                    UserNumber = UserNumber.Substring(0, UserNumber.Length - 1);//收信人列表
                    string MessageContent = ad.短信推送数据.内容;//短信内容
                    string rethtml = MailApiPost.PostDataGetHtml(UserNumber, MessageContent);//result=2&description=账号无效或未开户
                    string[] retlist = rethtml.Split('&');
                    if (retlist[0].Replace("result=", "") != "0")
                    {
                        return
                            Content("<script>alert('" + retlist[1].Replace("description=", "") +
                                    "');location.href='javascript:history.go(-1)';</script>");
                    }
                    else
                    {
                        一般推送管理.审核一般推送((long)id, currentUser.Id, 推送信息状态.审核通过);

                        ViewData["sendmessageresult"] = rethtml;
                        return View("PushMessageResultStaticPage");
                    }


                }
            }
            if (action == "审核不通过")
            {
                if (Request.Form["connectionid"] != null)
                {
                    公告推送管理.审核公告推送((long)id, currentUser.Id, 推送信息状态.审核未通过);
                }
                else
                {
                    一般推送管理.审核一般推送((long)id, currentUser.Id, 推送信息状态.审核未通过);
                }
            }

            return View("SendMessageAuditList");
        }

        public ActionResult Part_SkanExam()
        {
            var id = Request.Params["id"];
            var gys = 用户管理.查找用户<供应商>(long.Parse(id));
            return PartialView("Part_View/Part_SkanExam", gys);
        }

        public string Check_User(string name)
        {
            string catorgray = Request.QueryString["t"].ToString();
            if (catorgray == "供应商")
            {
                if (用户管理.检查用户是否存在<供应商>(name) != -1)
                {
                    return "1";
                }
                else
                {
                    return "0";
                }
            }
            else if (catorgray == "单位用户")
            {
                if (用户管理.检查用户是否存在<单位用户>(name) != -1)
                {
                    return "1";
                }
                else
                {
                    return "0";
                }
            }
            else
            {
                if (用户管理.检查用户是否存在<运营团队>(name) != -1)
                {
                    return "1";
                }
                else
                {
                    return "0";
                }
            }
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
                q = q.And(Query.EQ("单位信息.单位代号", new BsonRegularExpression(string.Format("/{0}/i", username))));
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
                q = q.And(Query.EQ("运营团队工作人员信息.姓名", new BsonRegularExpression(string.Format("/{0}/i", username))));
                long sum = 用户管理.计数用户<运营团队>(0, 0, q);
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
                IEnumerable<运营团队> Department = 用户管理.查询用户<运营团队>((int.Parse(pagesize) - 1) * 12, 12, q);
                if (Department != null)
                {
                    for (int i = 0; i < Department.Count(); i++)
                    {
                        if (Department.ElementAt(i).运营团队工作人员信息.姓名 != currentUser.运营团队工作人员信息.姓名)
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
                IEnumerable<供应商> Department = 用户管理.查询用户<供应商>((int.Parse(pagesize) - 1) * 12, 12, q);
                if (Department != null)
                {
                    for (int i = 0; i < Department.Count(); i++)
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
            JsonResult json = new JsonResult() { Data = new FindUsers() { name = allName, count = list_count, loginName = LoginName } };
            return Json(json, JsonRequestBehavior.AllowGet);

        }
        /// <summary>
        /// 发送站内消息
        /// </summary>
        /// <param name="talkmsg"></param>
        /// <param name="receiver">收信人</param>
        public void SendZnMessage(string title, string content, string receiver)
        {
            站内消息 Msg = new 站内消息();
            对话消息 Talk = new 对话消息();
            Msg.收信人.用户ID = 用户管理.检查用户是否存在(receiver);
            Msg.发起者.用户ID = currentUser.Id;
            站内消息管理.添加站内消息(Msg, Msg.发起者.用户ID, Msg.收信人.用户ID);
            Talk.消息主体.标题 = title;
            Talk.消息主体.内容 = content;
            对话消息管理.添加对话消息(Talk, Msg);
        }

        public ActionResult DownloadAdd()
        {
            return View();
        }
        public ActionResult Part_DownloadAdd()
        {
            return PartialView("Part_View/Part_DownloadAdd");
        }
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult DownloadAdd(下载 model)
        {
            if (ModelState.IsValid)
            {
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
                    if (model.下载类型 == 下载类型.标书)
                    {
                        model.下载验证码 = RandomString.Generate();
                    }
                    下载管理.添加下载(model);
                }
                catch
                {
                    return Content("<script>alert('输入有误!请检查数据格式');location.href='javascript:history.go(-1)';</script>");
                }
                //CreateIndex(model, "/Lucene.Net/IndexDic/GongGao");
            }

            return View("DownloadManage");
        }
        public ActionResult AddRecommendgys()
        {
            try
            {
                long Id = long.Parse(Request.QueryString["id"]);
                ViewData["id"] = Id;
                return View();
            }
            catch
            {
                return Redirect("/运营团队后台/Supplier_PssInfo?page=1");
            }
        }
        public ActionResult AddRecommend_good()
        {
            try
            {
                long Id = long.Parse(Request.QueryString["id"]);
                ViewData["id"] = Id;
                return View();
            }
            catch
            {
                return Redirect("/运营团队后台/GoodExamine");
            }
        }
        public ActionResult Del_Recommendgys()
        {
            try
            {
                long id = long.Parse(Request.QueryString["id"]);
                供应商推广管理.管理器.删除(id);
                return Redirect("/运营团队后台/Recommendgys");
            }
            catch
            {
                return Redirect("/运营团队后台/Recommendgys");
            }
        }
        public ActionResult Del_Recommendgood()
        {
            try
            {
                long id = long.Parse(Request.QueryString["id"]);
                商品推广管理.管理器.删除(id);
                return Redirect("/运营团队后台/RecommendGood");
            }
            catch
            {
                return Redirect("/运营团队后台/RecommendGood");
            }
        }
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult AddRecommend(供应商推广 model)
        {
            try
            {
                IEnumerable<供应商推广> g = 供应商推广管理.管理器.查询(0, 0, Query<供应商推广>.Where(m => m.供应商.用户ID == model.供应商.用户ID && m.结束时间 <= DateTime.Now)).OrderByDescending(m => m.显示级别);
                if (g != null && g.Count() != 0)
                {
                    return Content("<script>alert('该供应商已经添加过广告');window.location='/运营团队后台/Recommendgys_Detail?pos=" + ((int)g.ElementAt(0).广告位置).ToString() + "&page=1'</script>");
                }
                else
                {
                    供应商推广管理.管理器.添加(model);
                    return Redirect("/运营团队后台/Recommendgys");
                }
            }
            catch
            {
                return Redirect("/运营团队后台/Recommendgys");
            }
        }
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult AddRecommendGood(商品推广 model)
        {
            try
            {
                IEnumerable<商品推广> g = 商品推广管理.管理器.查询(0, 0, Query<商品推广>.Where(m => m.商品.商品ID == model.商品.商品ID && m.结束时间 <= DateTime.Now)).OrderByDescending(m => m.显示级别);
                if (g != null && g.Count() != 0)
                {
                    return Content("<script>alert('该商品已经添加过广告');window.location=/运营团队后台/Recommendgood_Detail?pos=" + ((int)g.ElementAt(0).广告位置).ToString() + "&page=1'</script>");
                }
                else
                {
                    商品推广管理.管理器.添加(model);
                    return Redirect("/运营团队后台/RecommendGood");
                }
            }
            catch
            {
                return Redirect("/运营团队后台/RecommendGood");
            }
        }
        public ActionResult Edit_Recommendgys()
        {
            try
            {
                long id = long.Parse(Request.QueryString["id"]);
                供应商推广 model = 供应商推广管理.管理器.查找(id);
                return View(model);
            }
            catch
            {
                return Redirect("/运营团队后台/Recommendgys");
            }
        }
        public ActionResult Edit_Recommendgood()
        {
            try
            {
                long id = long.Parse(Request.QueryString["id"]);
                商品推广 model = 商品推广管理.管理器.查找(id);
                return View(model);
            }
            catch
            {
                return Redirect("/运营团队后台/RecommendGood");
            }
        }
        [HttpPost]
        public ActionResult EditRecommend(供应商推广 model)
        {
            try
            {
                供应商推广管理.管理器.更新(model);
                return Redirect("/运营团队后台/Recommendgys_Detail?pos=" + (int)model.广告位置 + "&page=1");
            }
            catch
            {
                return Redirect("/运营团队后台/Recommendgys");
            }
        }
        [HttpPost]
        public ActionResult EditRecommend_Good(商品推广 model)
        {
            try
            {
                商品推广管理.管理器.更新(model);
                return Redirect("/运营团队后台/Recommendgood_Detail?pos=" + (int)model.广告位置 + "&page=1");
            }
            catch
            {
                return Redirect("/运营团队后台/RecommendGood");
            }
        }
        public ActionResult Recommendgys()
        {
            ViewData["new"] = 供应商推广管理.管理器.计数(0, 0, Query<供应商推广>.Where(m => (int)m.广告位置 == 0));
            ViewData["honest"] = 供应商推广管理.管理器.计数(0, 0, Query<供应商推广>.Where(m => (int)m.广告位置 == 1));
            return View();
        }
        public ActionResult RecommendGood()
        {
            ViewData["new"] = 商品推广管理.管理器.计数(0, 0, Query<商品推广>.Where(m => (int)m.广告位置 == 0));
            ViewData["honest"] = 商品推广管理.管理器.计数(0, 0, Query<商品推广>.Where(m => (int)m.广告位置 == 1));
            return View();
        }
        public ActionResult Recommendgood_Detail()
        {
            try
            {
                int pos = int.Parse(Request.QueryString["pos"]);
                int page = 0;
                if (string.IsNullOrEmpty(Request.QueryString["page"]))
                {
                    page = 1;
                }
                else
                {
                    page = int.Parse(Request.QueryString["page"]);
                }
                ViewData["pos"] = pos;
                ViewData["CurrentPage"] = page;
                IEnumerable<商品推广> model = 商品推广管理.管理器.查询(10 * (page - 1), 10, Query<商品推广>.Where(m => (int)m.广告位置 == pos)).OrderByDescending(m => m.显示级别);
                long PageCount = model.Count() / 10;
                if ((model.Count() % 10) > 0)
                {
                    PageCount++;
                }
                ViewData["Pagecount"] = PageCount;
                ViewData["good"] = model;
                return View();
            }
            catch
            {
                return Redirect("/运营团队后台/RecommendGood");
            }
        }
        public ActionResult Recommendgys_Detail()
        {
            try
            {
                int pos = int.Parse(Request.QueryString["pos"]);
                int page = 0;
                if (string.IsNullOrEmpty(Request.QueryString["page"]))
                {
                    page = 1;
                }
                else
                {
                    page = int.Parse(Request.QueryString["page"]);
                }
                ViewData["pos"] = pos;
                ViewData["CurrentPage"] = page;
                IEnumerable<供应商推广> model = 供应商推广管理.管理器.查询(10 * (page - 1), 10, Query<供应商推广>.Where(m => (int)m.广告位置 == pos)).OrderByDescending(m => m.显示级别);
                long PageCount = model.Count() / 10;
                if ((model.Count() % 10) > 0)
                {
                    PageCount++;
                }
                ViewData["Pagecount"] = PageCount;
                ViewData["supplier"] = model;
                return View();
            }
            catch
            {
                return Redirect("/运营团队后台/Recommendgys");
            }
        }
        public class RecommendgysList
        {
            public long Id { get; set; }
            public long sId { get; set; }
            public string gysName { get; set; }
            public string name { get; set; }
            public string cls { get; set; }
            public string stime { get; set; }
            public string etime { get; set; }
            public string Isend { get; set; }
        }
        public ActionResult RecmmondGood()
        {
            DateTime stime = new DateTime();
            DateTime etime = new DateTime();
            if (!string.IsNullOrWhiteSpace(Request.QueryString["stime"]))
            {
                stime = Convert.ToDateTime(Request.QueryString["stime"]);
            }
            if (!string.IsNullOrWhiteSpace(Request.QueryString["etime"]))
            {
                etime = Convert.ToDateTime(Request.QueryString["etime"]);
            }
            List<RecommendgysList> rlist = new List<RecommendgysList>();
            int pos = int.Parse(Request.QueryString["pos"]);
            int page = 0;
            if (string.IsNullOrEmpty(Request.QueryString["page"]))
            {
                page = 1;
            }
            else
            {
                page = int.Parse(Request.QueryString["page"]);
            }
            ViewData["pos"] = pos;
            ViewData["CurrentPage"] = page;
            IEnumerable<商品推广> model = 商品推广管理.管理器.查询(20 * (page - 1), 20, Query<商品推广>.Where(m => (int)m.广告位置 == pos && m.显示时间 >= stime && m.结束时间 <= etime)).OrderByDescending(m => m.显示级别);
            long sum = 商品推广管理.管理器.计数(0, 0, Query<商品推广>.Where(m => (int)m.广告位置 == pos && m.显示时间 >= stime && m.结束时间 <= etime));
            long PageCount = sum / 20;
            if ((sum % 20) > 0)
            {
                PageCount++;
            }
            if (model != null && model.Count() != 0)
            {
                foreach (var item in model)
                {
                    RecommendgysList Recommendgood = new RecommendgysList();
                    Recommendgood.Id = item.Id;
                    Recommendgood.sId = item.商品.商品ID;
                    Recommendgood.gysName = item.商品.商品.商品信息.所属供应商.用户数据.企业基本信息.企业名称;
                    Recommendgood.name = item.商品.商品.商品信息.商品名;
                    Recommendgood.stime = item.显示时间.ToString("yyyy-MM-dd HH:mm:ss");
                    Recommendgood.etime = item.结束时间.ToString("yyyy-MM-dd HH:mm:ss");
                    if (item.结束时间 <= DateTime.Now)
                    {
                        Recommendgood.Isend = "已结束";
                    }
                    else
                    {
                        Recommendgood.Isend = "进行中...";
                    }
                    Recommendgood.cls = item.显示级别.ToString();
                    rlist.Add(Recommendgood);
                }
            }
            JsonResult json = new JsonResult() { Data = new { gys = rlist, pageSize = PageCount, currentPage = page } };
            return Json(json, JsonRequestBehavior.AllowGet);
        }
        public ActionResult RecmmondGys()
        {
            DateTime stime = new DateTime();
            DateTime etime = new DateTime();
            if (!string.IsNullOrWhiteSpace(Request.QueryString["stime"]))
            {
                stime = Convert.ToDateTime(Request.QueryString["stime"]);
            }
            if (!string.IsNullOrWhiteSpace(Request.QueryString["etime"]))
            {
                etime = Convert.ToDateTime(Request.QueryString["etime"]);
            }
            List<RecommendgysList> rlist = new List<RecommendgysList>();
            int pos = int.Parse(Request.QueryString["pos"]);
            int page = 0;
            if (string.IsNullOrEmpty(Request.QueryString["page"]))
            {
                page = 1;
            }
            else
            {
                page = int.Parse(Request.QueryString["page"]);
            }
            ViewData["pos"] = pos;
            ViewData["CurrentPage"] = page;
            IEnumerable<供应商推广> model = 供应商推广管理.管理器.查询(20 * (page - 1), 20, Query<供应商推广>.Where(m => (int)m.广告位置 == pos && m.显示时间 >= stime && m.结束时间 <= etime)).OrderByDescending(m => m.显示级别);
            long sum = 供应商推广管理.管理器.计数(0, 0, Query<供应商推广>.Where(m => (int)m.广告位置 == pos && m.显示时间 >= stime && m.结束时间 <= etime));
            long PageCount = sum / 20;
            if ((sum % 20) > 0)
            {
                PageCount++;
            }
            if (model != null && model.Count() != 0)
            {
                foreach (var item in model)
                {
                    RecommendgysList Recommendgys = new RecommendgysList();
                    Recommendgys.Id = item.Id;
                    Recommendgys.sId = item.供应商.用户ID;
                    Recommendgys.name = item.供应商.用户数据.企业基本信息.企业名称;
                    Recommendgys.stime = item.显示时间.ToString("yyyy-MM-dd HH:mm:ss");
                    Recommendgys.etime = item.结束时间.ToString("yyyy-MM-dd HH:mm:ss");
                    if (item.结束时间 <= DateTime.Now)
                    {
                        Recommendgys.Isend = "已结束";
                    }
                    else
                    {
                        Recommendgys.Isend = "进行中...";
                    }
                    Recommendgys.cls = item.显示级别.ToString();
                    rlist.Add(Recommendgys);
                }
            }
            JsonResult json = new JsonResult() { Data = new { gys = rlist, pageSize = PageCount, currentPage = page } };
            return Json(json, JsonRequestBehavior.AllowGet);
        }
        public ActionResult DownloadManage()
        {
            return View();
        }
        public ActionResult Part_DownloadManage(int? page)
        {

            int listcount = (int)(下载管理.计数下载(0, -1));
            int maxpage = Math.Max((listcount + DOWNLOAD_PAGESIZE - 1) / DOWNLOAD_PAGESIZE, 1);

            if (string.IsNullOrEmpty(page.ToString()) || page < 0 || page > maxpage)
            {
                page = 1;
            }

            ViewData["listcount"] = listcount;

            ViewData["pagesize"] = DOWNLOAD_PAGESIZE;

            ViewData["后台下载列表"] = 下载管理.查询下载(DOWNLOAD_PAGESIZE * (int.Parse(page.ToString()) - 1), DOWNLOAD_PAGESIZE);

            return PartialView("Part_View/Part_DownloadManage");
        }

        public ActionResult DownloadDetail()
        {
            return View();
        }
        public ActionResult Part_DownloadDetail(int? id)
        {
            下载 g = null;
            try
            {
                g = 下载管理.查找下载((long)id);
            }
            catch
            {

            }

            return PartialView("Part_View/Part_DownloadDetail", g);
        }

        public ActionResult DownloadModify()
        {
            return View();
        }
        public ActionResult Part_DownloadModify(int? id)
        {
            下载 g = null;
            try
            {
                g = 下载管理.查找下载((long)id);
            }
            catch
            {

            }

            return PartialView("Part_View/Part_DownloadModify", g);
        }

        /// <summary>
        /// 更新下载
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult DownloadModify(下载 model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var attachstr = Request.Form["attachtext"];
                    var deleteattachstr = Request.Form["deletattachtext"];
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
                    if (model.下载类型 == 下载类型.标书)
                    {
                        model.下载验证码 = RandomString.Generate();
                    }
                    else
                    {
                        model.下载验证码 = null;
                    }
                    下载管理.更新下载(model);
                }
                catch
                {
                    return Content("<script>alert('输入有误!请检查数据格式');location.href='javascript:history.go(-1)';</script>");
                }
            }
            return View("DownloadManage");
        }

        /// <summary>
        /// 删除下载
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult DownloadDelete(long? id)
        {
            if (id != null)
            {
                try
                {
                    var attach = 下载管理.查找下载((long)id).内容主体;
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

                    下载管理.删除下载((long)id);

                }
                catch
                {
                    return Content("<script>alert('输入有误!请检查数据格式');location.href='javascript:history.go(-1)';</script>");
                }
            }
            return View("DownloadManage");
        }

        public ActionResult SignUp_List()
        {
            return View();
        }
        public ActionResult Part_SignUp_List(int? page)
        {

            int signup_listcount = (int)(招标采购预报名管理.计数招标采购预报名(0, 0));
            int signup_maxpage = Math.Max((signup_listcount + 15 - 1) / 15, 1);

            if (string.IsNullOrEmpty(page.ToString()) || page < 0 || page > signup_maxpage)
            {
                page = 1;
            }

            ViewData["listcount"] = signup_listcount;
            ViewData["page"] = page;
            ViewData["pagesize"] = 15;

            ViewData["预报名列表"] = 招标采购预报名管理.查询招标采购预报名(15 * (int.Parse(page.ToString()) - 1), 15);

            return PartialView("Part_View/Part_SignUp_List");
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
            return PartialView("Part_View/Part_SignUp_Print", model);
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
            return PartialView("Part_View/Part_SignUp_PrintSearch", model);
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
                return Content("<script>alert('设置成功！');window.location='/运营团队后台/SignUp_Detail?id=" + m.Id + "'</script>");
            }
            catch
            {
                return Content("<script>alert('设置失败！');window.location='/运营团队后台/SignUp_List'</script>");
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

            return PartialView("Part_View/Part_SignUp_Detail", model);
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

            return PartialView("Part_View/Part_SignUp_Detail_Page", model);
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
                return Content("<script>alert('设置成功！');window.location='/运营团队后台/SignUp_Information?id=" + m.Id + "'</script>");
            }
            catch
            {
                return Content("<script>alert('设置失败！');window.location='/运营团队后台/SignUp_List'</script>");
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
            return PartialView("Part_View/Part_SignUp_Information", model);
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
                return Content("<script>alert('设置成功！');window.location='/运营团队后台/SignUp_BiaoShu?id=" + m.Id + "'</script>");
            }
            catch
            {
                return Content("<script>alert('设置失败！');window.location='/运营团队后台/SignUp_List'</script>");
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
            return PartialView("Part_View/Part_SignUp_BiaoShu", model);


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
            //return PartialView("Part_View/Part_SignUp_BiaoShu", model);
        }
        public ActionResult SignUp_U()
        {
            return View();
        }
        [HttpPost]
        public ActionResult SignUp_U(供应商 model)
        {
            try
            {
                var gys = 用户管理.查找用户<供应商>(model.Id);
                gys.供应商用户信息.U盾信息.序列号 = Request.Form["seriesnumber"];
                gys.供应商用户信息.U盾信息.加密参数 = Request.Form["codeparam"];
                gys.供应商用户信息.U盾信息.年检开始时间 = Convert.ToDateTime(Request.Form["starttime"]);
                gys.供应商用户信息.U盾信息.年检结束时间 = Convert.ToDateTime(Request.Form["endtime"]);
                if (gys.供应商用户信息.U盾信息.年检开始时间 >= gys.供应商用户信息.U盾信息.年检结束时间)
                {
                    return Content("<script>alert('结束时间必须大于开始时间！');window.location='/运营团队后台/SignUp_U?id=" + model.Id + "'</script>");
                }

                var status = Request.Form["auditstatus"];
                if (status == "1")
                {
                    gys.供应商用户信息.U盾信息.审核数据.审核状态 = 审核状态.审核通过;
                    gys.供应商用户信息.U盾信息.审核数据.审核者.用户ID = currentUser.Id;
                }
                else if (status == "2")
                {
                    gys.供应商用户信息.U盾信息.审核数据.审核状态 = 审核状态.审核未通过;
                    gys.供应商用户信息.U盾信息.审核数据.审核不通过原因 = Request.Form["reason"];
                    gys.供应商用户信息.U盾信息.审核数据.审核者.用户ID = currentUser.Id;
                }
                else
                {
                    gys.供应商用户信息.U盾信息.审核数据.审核状态 = 审核状态.未审核;
                }
                用户管理.更新用户<供应商>(gys, false);
                return Content("<script>alert('设置成功！');window.location='/运营团队后台/SignUp_U?id=" + model.Id + "'</script>");
            }
            catch
            {
                return Content("<script>alert('存储结果错误！');window.location='/运营团队后台/SignUp_U?id=" + model.Id + "'</script>");
            }
        }
        public ActionResult Part_SignUp_U(long? id)
        {
            var model = 用户管理.查找用户<供应商>((long)id);
            return PartialView("Part_View/Part_SignUp_U", model);
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
                return Content("<script>alert('设置成功！');window.location='/运营团队后台/SignUp_Detail?id=" + item + "'</script>");
            }
            catch
            {
                return Content("<script>alert('设置失败！');window.location='/运营团队后台/SignUp_List'</script>");
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
            return PartialView("Part_View/Part_SignUp_Detail_Gys", m);
        }

        //用戶組管理部份
        public ActionResult Usergroup_Add()
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

                用户组管理.更新用户组(model);
                return Content("success");
            }
            catch
            {
                return Content("输入有误，请重新填写！");
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
        public ActionResult Part_Usergroup_Add()
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

            return PartialView("Part_View/Part_Usergroup_Add");
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



            return PartialView("Part_View/Part_Usergroup_Mannage");
        }

        [HttpPost]
        public ActionResult UsergroupAdd()
        {
            用户组 model = new 用户组();
            var selectlist = Request.Params["per"];
            var name = Request.Params["name"];
            model.权限列表 = new List<string>();
            if (!string.IsNullOrEmpty(selectlist))
            {
                model.权限列表 = selectlist.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
            }
            model.用户组名 = name;
            用户组管理.添加用户组(model);

            return View("Usergroup_Mannage");

        }
        public ActionResult Role_Add()
        {
            return View();
        }
        public ActionResult Role_Mannage()
        {
            return View();
        }
        public ActionResult Part_Role_Add()
        {

            ViewData["用户组集合"] = 用户组管理.查询用户组(0, 0);

            return PartialView("Part_View/Part_Role_Add");
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

            return PartialView("Part_View/Part_Role_Mannage");
        }

        [HttpPost]
        public ActionResult Role_Add(角色 model)
        {
            var selectlist = Request.Form["permissionstr"];
            model.包含用户组 = new List<string>();
            if (!string.IsNullOrEmpty(selectlist))
            {
                model.包含用户组 = selectlist.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
            }
            角色管理.添加角色(model);
            return View("Role_Mannage");
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
        public JsonResult CheckRoleName(string 角色名)
        {
            var result = 权限管理.角色已存在(角色名);
            return Json(!result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult User_PermissionEdit(long? id)
        {
            try
            {
                ViewData["用户组集合"] = 用户组管理.查询用户组(0, 0);
                ViewData["角色列表"] = 角色管理.查询角色(0, 0);
                return View(用户管理.查找用户((long)id));
            }
            catch
            {
                return Content("<script>window.location='User_Info_List'</script>");
            }
        }

        public ActionResult OperationInfo()
        {
            var _gys = 用户管理.查询用户<供应商>(0, 0);
            string[] _area = { "成都", "贵阳", "重庆", "昆明" };
            var newdic = new Dictionary<string, int>();
            var newdic1 = new Dictionary<string, int>();

            //供应商入网数量
            var dic = new Dictionary<string, Dictionary<string, int>>();
            var _qjrk = _gys.Where(o => o.供应商用户信息.入库级别 == 供应商.入库级别.全军库).Count();
            newdic.Add("全军入库", _qjrk);
            var _cdrk = _gys.Where(o => o.供应商用户信息.入库级别 == 供应商.入库级别.成都军区库).Count();
            newdic.Add("成都军区入库", _cdrk);
            dic.Add("入网供应商总数", newdic);

            newdic = new Dictionary<string, int>();
            foreach (var item in _area)
            {
                var _agreegys = _gys.Where(o => o.供应商用户信息.协议供应商 && o.所属地域.城市 != null && o.所属地域.城市.Contains(item)).Count();
                newdic.Add(item, _agreegys);
                var _emergys = _gys.Where(o => o.所属地域.城市 != null && o.所属地域.城市.Contains(item) && o.供应商用户信息.应急供应商).Count();
                newdic1.Add(item, _emergys);
            }
            dic.Add("入网协议供应商总数", newdic);
            dic.Add("入网应急供应商总数", newdic1);
            ViewData["供应商入网数量"] = dic;

            //各行业供应商入网数量
            var dic1 = new Dictionary<string, Dictionary<string, int>>();
            newdic = new Dictionary<string, int>();
            newdic1 = new Dictionary<string, int>();
            string[] _tywz = { "办公设备", "文体器材", "计算机及通信", "电气设备", "机械设备", "家具", "仪器仪表", "原材料", "建筑装修材料", "车辆" };
            string[] _zywz = { "医疗设备", "油料设备器材", "给养器材", "军用食品", "后勤装备", "药品类", "被装", "医用耗材", "军事交通器材", "基建营房工程器材" };
            foreach (var item in _tywz)
            {
                var _hy = _gys.Where(o => o.可提供产品类别列表.Count > 0 && o.可提供产品类别列表.Exists(p => p.一级分类.Contains(item))).Count();
                newdic.Add(item, _hy);
            }
            dic1.Add("通用物资", newdic);
            foreach (var items in _zywz)
            {
                var _hy = _gys.Where(o => o.可提供产品类别列表.Count > 0 && o.可提供产品类别列表.Exists(p => p.一级分类.Contains(items))).Count();
                newdic1.Add(items, _hy);
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
            var _unitaccount = 用户管理.查询用户<单位用户>(0, 0);
            var dic = new Dictionary<string, Dictionary<string, int>>();
            foreach (var item in _area)
            {
                var _allaccount = _unitaccount.Where(o => o.所属地域.城市 != null && o.所属地域.城市.Contains(item)).Count();
                newdic.Add(item, _allaccount);
            }
            dic.Add("单位用户账号申请总数", newdic);

            //公告
            var _ad = 公告管理.查询公告(0, 0);

            //公告月发布数量
            var jk = new List<Tuple<string, string, string>>(); //统计月份，每月数量，某月每天数量
            var _month = DateTime.Now.Month;
            var _year = DateTime.Now.Year;
            for (int i = 1; i <= 7; i++)
            {
                _month--;
                if (_month <= 0)
                {
                    _year -= 1;
                    _month = 12;
                }
                var _monthnum = _ad.Where(o => o.基本数据.添加时间.Year == _year && o.基本数据.添加时间.Month == _month).Count();
                var _perday = Math.Round(_monthnum / 30.0, 1);
                jk.Add(Tuple.Create(_month + "月", _monthnum + "条", _perday + "条"));
            }
            jk.Reverse();
            ViewData["公告月发布数量"] = jk;

            //公告发布类型
            var typs = new Dictionary<string, Dictionary<string, int>>();

            newdic = new Dictionary<string, int>();
            newdic.Add("", _ad.Where(o => o.公告信息.公告类别 == 公告.公告类别.公开招标 &&
                                        (o.公告信息.公告性质 == 公告.公告性质.采购公告 ||
                                        o.公告信息.公告性质 == 公告.公告性质.预公告 ||
                                        o.公告信息.公告性质 == 公告.公告性质.技术公告)).Count());
            typs.Add("公开招标类", newdic);

            newdic = new Dictionary<string, int>();
            newdic.Add("", _ad.Where(o => o.公告信息.公告类别 == 公告.公告类别.公开招标 &&
                                          (o.公告信息.公告性质 == 公告.公告性质.中标结果公示 ||
                                          o.公告信息.公告性质 == 公告.公告性质.流标公告 ||
                                          o.公告信息.公告性质 == 公告.公告性质.废标公告)).Count());
            typs.Add("结果公示类", newdic);

            newdic = new Dictionary<string, int>();
            newdic.Add("邀请招标类", _ad.Where(o => o.公告信息.公告类别 == 公告.公告类别.邀请招标).Count());
            newdic.Add("询价类", _ad.Where(o => o.公告信息.公告类别 == 公告.公告类别.询价采购).Count());
            newdic.Add("竞争性谈判类", _ad.Where(o => o.公告信息.公告类别 == 公告.公告类别.竞争性谈判).Count());
            newdic.Add("协议采购类", _ad.Where(o => o.公告信息.公告类别 == 公告.公告类别.协议采购).Count());
            newdic.Add("单一来源类", _ad.Where(o => o.公告信息.公告类别 == 公告.公告类别.单一来源).Count());
            newdic.Add("网上竞标类", _ad.Where(o => o.公告信息.公告类别 == 公告.公告类别.网上竞标).Count());
            typs.Add("采购类", newdic);
            ViewData["公告发布类型"] = typs;

            //公告发布单位
            var _units = _ad.Where(o => o.公告信息.公告类别 != 公告.公告类别.其他)
                            .Select(o => o.公告信息.需求单位 != null ? o.公告信息.需求单位.Trim() : o.公告信息.需求单位)
                            .Distinct();
            var unitgg = new List<Tuple<string, int, int>>();
            foreach (var item in _units)
            {
                //某单位招投标类公告数量
                var _zbl = _ad.Where(o => (o.公告信息.需求单位 != null ? o.公告信息.需求单位.Trim() : o.公告信息.需求单位) == item &&
                                          o.公告信息.公告类别 == 公告.公告类别.公开招标 &&
                                          o.公告信息.公告性质 != 公告.公告性质.未设置).Count();
                //某单位采购类公告数量
                var _cgl = _ad.Where(o => (o.公告信息.需求单位 != null ? o.公告信息.需求单位.Trim() : o.公告信息.需求单位) == item &&
                                          (o.公告信息.公告类别 == 公告.公告类别.单一来源 ||
                                          o.公告信息.公告类别 == 公告.公告类别.竞争性谈判 ||
                                          o.公告信息.公告类别 == 公告.公告类别.网上竞标 ||
                                          o.公告信息.公告类别 == 公告.公告类别.协议采购 ||
                                          o.公告信息.公告类别 == 公告.公告类别.询价采购 ||
                                          o.公告信息.公告类别 == 公告.公告类别.邀请招标)).Count();
                unitgg.Add(Tuple.Create(item, _zbl, _cgl));
            }
            ViewData["公告发布单位"] = unitgg;
            return PartialView("Part_View/Part_UnitOrAdStatics");
        }

        public ActionResult Part_GoodInTypes()
        {
            IEnumerable<商品分类> types = 商品分类管理.查找子分类();
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
            ViewData["gtype"] = types;
            ViewData["statistic"] = goodCount;
            return PartialView("Part_View/Part_GoodInTypes");
        }
        [HttpPost]
        public ActionResult User_PermissionEdit()
        {
            var id = Request.QueryString["id"];
            var type = Request.QueryString["type"];

            var rolestr = Request.Form["roleparm"];
            var usergroupstr = Request.Form["usergroupparm"];
            try
            {
                var model = 用户管理.查找用户(long.Parse(id));
                model.角色 = new List<string>();
                if (!string.IsNullOrEmpty(rolestr))
                {
                    model.角色 = rolestr.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                }

                model.用户组 = new List<string>();
                if (!string.IsNullOrEmpty(usergroupstr))
                {
                    model.用户组 = usergroupstr.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                }
                switch (type)
                {
                    case "depart":
                        用户管理.更新用户<单位用户>((单位用户)model);
                        break;
                    case "gys":
                        用户管理.更新用户<供应商>((供应商)model);
                        break;
                    case "manage":
                        用户管理.更新用户<运营团队>((运营团队)model);
                        break;
                    case "single":
                        用户管理.更新用户<个人用户>((个人用户)model);
                        break;
                    default:
                        return Content("<script>alert('操作失败，请重试');window.location='/运营团队后台/User_Info_List';</script>");
                }

                return Content("<script>alert('修改成功');window.location='/运营团队后台/User_Info_List';</script>");
            }
            catch
            {
                return Content("<script>alert('操作失败，请重试');window.location='/运营团队后台/User_Info_List';</script>");
            }
        }
        public ActionResult redirect1()
        {
            return Redirect("http://gsxt.saic.gov.cn/");
            //return Content("<script>window.open('http://gsxt.saic.gov.cn/');location.href='javascript:history.go(-1)';</script>");
        }
        public ActionResult redirect2()
        {
            return Redirect("http://www.sccredit.gov.cn/");
            //return Content("<script>window.open('http://www.sccredit.gov.cn/');location.href='javascript:history.go(-1)';</script>");
        }

        public ActionResult ProjectService_List()
        {
            return View();
        }
        public ActionResult Part_ProjectService_List(int? page)
        {

            int pro_listcount = (int)(项目服务记录管理.计数项目服务记录(0, 0));
            int pro_maxpage = Math.Max((pro_listcount + 15 - 1) / 15, 1);

            if (string.IsNullOrEmpty(page.ToString()) || page < 0 || page > pro_maxpage)
            {
                page = 1;
            }

            ViewData["listcount"] = pro_listcount;
            ViewData["page"] = page;
            ViewData["pagesize"] = 15;

            ViewData["待评分项目服务列表"] = 项目服务记录管理.查询项目服务记录(15 * (int.Parse(page.ToString()) - 1), 15);
            return PartialView("Part_View/Part_ProjectService_List");
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
            }
            catch
            {
                model = null;
            }
            return PartialView("Part_View/Part_ProjectService_Detail", model);
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

        //推荐管理模块////////////////////////////////////////////////////
        public ActionResult Recommend_ExpertList_Ed()
        {
            return View();
        }
        public ActionResult Part_Recommend_ExpertList_Ed(int? page)
        {
            int rec_listcount = (int)(推荐管理.计数推荐信息(0, 0, Query<推荐信息>.Where(o => o.推荐类型 == 推荐类型.专家 && o.推荐审核数据.推荐状态 != 推荐状态.待联系)));
            int rec_maxpage = Math.Max((rec_listcount + ZBCGXM_PAGESIZE - 1) / ZBCGXM_PAGESIZE, 1);

            if (string.IsNullOrEmpty(page.ToString()) || page < 0 || page > rec_maxpage)
            {
                page = 1;
            }

            ViewData["listcount"] = rec_listcount;
            ViewData["page"] = page;
            ViewData["pagesize"] = ZBCGXM_PAGESIZE;

            ViewData["已联系推荐专家"] = 推荐管理.查询推荐信息(ZBCGXM_PAGESIZE * (int.Parse(page.ToString()) - 1), ZBCGXM_PAGESIZE, Query<推荐信息>.Where(o => o.推荐类型 == 推荐类型.专家 && o.推荐审核数据.推荐状态 != 推荐状态.待联系));
            return PartialView("Part_View/Part_Recommend_ExpertList_Ed");
        }
        public ActionResult Recommend_ExpertList_Pre()
        {
            return View();
        }
        public ActionResult Part_Recommend_ExpertList_Pre(int? page)
        {
            int rec_listcount = (int)(推荐管理.计数推荐信息(0, 0, Query<推荐信息>.Where(o => o.推荐类型 == 推荐类型.专家 && o.推荐审核数据.推荐状态 == 推荐状态.待联系)));
            int rec_maxpage = Math.Max((rec_listcount + ZBCGXM_PAGESIZE - 1) / ZBCGXM_PAGESIZE, 1);

            if (string.IsNullOrEmpty(page.ToString()) || page < 0 || page > rec_maxpage)
            {
                page = 1;
            }

            ViewData["listcount"] = rec_listcount;
            ViewData["page"] = page;
            ViewData["pagesize"] = ZBCGXM_PAGESIZE;

            ViewData["待联系推荐专家"] = 推荐管理.查询推荐信息(ZBCGXM_PAGESIZE * (int.Parse(page.ToString()) - 1), ZBCGXM_PAGESIZE, Query<推荐信息>.Where(o => o.推荐类型 == 推荐类型.专家 && o.推荐审核数据.推荐状态 == 推荐状态.待联系));
            return PartialView("Part_View/Part_Recommend_ExpertList_Pre");
        }
        public ActionResult Recommend_GysList_Ed()
        {
            return View();
        }
        public ActionResult Part_Recommend_GysList_Ed(int? page)
        {
            int rec_listcount = (int)(推荐管理.计数推荐信息(0, 0, Query<推荐信息>.Where(o => o.推荐类型 == 推荐类型.供应商 && o.推荐审核数据.推荐状态 != 推荐状态.待联系)));
            int rec_maxpage = Math.Max((rec_listcount + ZBCGXM_PAGESIZE - 1) / ZBCGXM_PAGESIZE, 1);

            if (string.IsNullOrEmpty(page.ToString()) || page < 0 || page > rec_maxpage)
            {
                page = 1;
            }

            ViewData["listcount"] = rec_listcount;
            ViewData["page"] = page;
            ViewData["pagesize"] = ZBCGXM_PAGESIZE;

            ViewData["已联系推荐供应商"] = 推荐管理.查询推荐信息(ZBCGXM_PAGESIZE * (int.Parse(page.ToString()) - 1), ZBCGXM_PAGESIZE, Query<推荐信息>.Where(o => o.推荐类型 == 推荐类型.供应商 && o.推荐审核数据.推荐状态 != 推荐状态.待联系));
            return PartialView("Part_View/Part_Recommend_GysList_Ed");
        }
        public ActionResult Recommend_GysList_Pre()
        {
            return View();
        }
        public ActionResult Part_Recommend_GysList_Pre(int? page)
        {
            int rec_listcount = (int)(推荐管理.计数推荐信息(0, 0, Query<推荐信息>.Where(o => o.推荐类型 == 推荐类型.供应商 && o.推荐审核数据.推荐状态 == 推荐状态.待联系)));
            int rec_maxpage = Math.Max((rec_listcount + ZBCGXM_PAGESIZE - 1) / ZBCGXM_PAGESIZE, 1);

            if (string.IsNullOrEmpty(page.ToString()) || page < 0 || page > rec_maxpage)
            {
                page = 1;
            }

            ViewData["listcount"] = rec_listcount;
            ViewData["page"] = page;
            ViewData["pagesize"] = ZBCGXM_PAGESIZE;

            ViewData["待联系推荐供应商"] = 推荐管理.查询推荐信息(ZBCGXM_PAGESIZE * (int.Parse(page.ToString()) - 1), ZBCGXM_PAGESIZE, Query<推荐信息>.Where(o => o.推荐类型 == 推荐类型.供应商 && o.推荐审核数据.推荐状态 == 推荐状态.待联系));

            return PartialView("Part_View/Part_Recommend_GysList_Pre");
        }

        public ActionResult Recommend_GysDetail()
        {
            return View();
        }
        public ActionResult Part_Recommend_GysDetail(long? id)
        {
            推荐信息 model = null;
            try
            {
                model = 推荐管理.查找推荐信息((long)id);
            }
            catch
            {
                model = null;
            }
            return PartialView("Part_View/Part_Recommend_GysDetail", model);
        }
        [HttpPost]
        public ActionResult Recommend_GysDetail(推荐信息 model, string action)
        {
            推荐信息 m = 推荐管理.查找推荐信息(model.Id);
            m.推荐审核数据.审核者.用户ID = currentUser.Id;
            if (action == "推荐通过")
            {
                m.推荐审核数据.推荐状态 = 推荐状态.推荐通过;
            }
            if (action == "推荐不通过")
            {
                m.推荐审核数据.推荐状态 = 推荐状态.推荐未通过;
            }

            推荐管理.更新推荐信息(m);
            return View("Recommend_GysList_Pre");
        }
        public ActionResult Recommend_ExpertDetail()
        {
            return View();
        }
        public ActionResult Part_Recommend_ExpertDetail(long? id)
        {
            推荐信息 model = null;
            try
            {
                model = 推荐管理.查找推荐信息((long)id);
            }
            catch
            {
                model = null;
            }
            return PartialView("Part_View/Part_Recommend_ExpertDetail", model);
        }
        [HttpPost]
        public ActionResult Recommend_ExpertDetail(推荐信息 model, string action)
        {
            推荐信息 m = 推荐管理.查找推荐信息(model.Id);
            m.推荐审核数据.审核者.用户ID = currentUser.Id;
            if (action == "推荐通过")
            {
                m.推荐审核数据.推荐状态 = 推荐状态.推荐通过;
            }
            if (action == "推荐不通过")
            {
                m.推荐审核数据.推荐状态 = 推荐状态.推荐未通过;
            }

            推荐管理.更新推荐信息(m);
            return View("Recommend_ExpertList_Pre");
        }
        public ActionResult Delete_Recommend(long id)
        {
            try
            {
                推荐管理.删除推荐信息(id);
            }
            catch
            {

            }
            return View("Recommend_GysList_Pre");
        }
        //推荐管理模块////////////////////////////////////////////////////

        /// <summary>
        /// 上传下载文件
        /// </summary>
        /// <param name="fileData"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult UploadAttachment_Download(HttpPostedFileBase fileData)
        {
            if (fileData != null)
            {
                string filePath = 上传管理.获取上传路径<下载>(媒体类型.附件, 路径类型.服务器本地路径);

                //string extension = fileData.FileName.Substring(fileData.FileName.LastIndexOf("."), (fileData.FileName.Length - fileData.FileName.LastIndexOf(".")));

                string fileName = fileData.FileName;
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
                return Content(string.Format("{0}{1}", 上传管理.获取上传路径<下载>(媒体类型.附件, 路径类型.不带域名根路径), fileName));
            }
            else
            {
                return Content("0");
            }
        }

        public ActionResult TicketExamine(int? page)
        {
            var l = 用户管理.列表用户<供应商>(0, 0,
                Fields<供应商>.Include(o => o.Id),
                Query<供应商>.EQ(o => o.审核数据.审核状态, 审核状态.审核通过)
                );
            var q = Query.And(
                    Query.In("所属供应商.用户ID", l.Select(o => o["_id"])),
                    Query.EQ("审核数据.审核状态", 审核状态.未审核)
                );

            int count = (int)机票代售点管理.计数机票代售点(0, 0, q);
            int maxpage = Math.Max((count + 15 - 1) / 15, 1);
            if (string.IsNullOrEmpty(page.ToString()) || page < 0 || page > maxpage)
            {
                page = 1;
            }
            ViewData["page"] = page;
            ViewData["listcount"] = count;
            ViewData["pagesize"] = 15;
            ViewData["代售点列表"] = 机票代售点管理.查询机票代售点(15 * (int.Parse(page.ToString()) - 1), 15, q);
            return View();
        }
        public ActionResult TicketExamined(int? page)
        {
            var q = Query.EQ("审核数据.审核状态", 审核状态.审核通过);
            int count = (int)机票代售点管理.计数机票代售点(0, 0, q);
            int maxpage = Math.Max((count + 15 - 1) / 15, 1);
            if (string.IsNullOrEmpty(page.ToString()) || page < 0 || page > maxpage)
            {
                page = 1;
            }
            ViewData["page"] = page;
            ViewData["listcount"] = count;
            ViewData["pagesize"] = 15;
            ViewData["代售点列表"] = 机票代售点管理.查询机票代售点(15 * (int.Parse(page.ToString()) - 1), 15, q);
            return View();
        }
        public ActionResult TicketDetail()
        {
            string id = Request.Params["id"];
            机票代售点 k = 机票代售点管理.查找机票代售点(long.Parse(id));
            return View(k);
        }
        public ActionResult HotelExamine(int? page)
        {
            var l = 用户管理.列表用户<供应商>(0, 0,
                Fields<供应商>.Include(o => o.Id),
                Query<供应商>.EQ(o => o.审核数据.审核状态, 审核状态.审核通过)
                );
            var q = Query.And(
                    Query.In("所属供应商.用户ID", l.Select(o => o["_id"])),
                    Query.EQ("审核数据.审核状态", 审核状态.未审核)
                );

            int count = (int)酒店管理.计数酒店(0, 0, q);
            int maxpage = Math.Max((count + 15 - 1) / 15, 1);
            if (string.IsNullOrEmpty(page.ToString()) || page < 0 || page > maxpage)
            {
                page = 1;
            }
            ViewData["page"] = page;
            ViewData["listcount"] = count;
            ViewData["pagesize"] = 15;
            ViewData["酒店列表"] = 酒店管理.查询酒店(15 * (int.Parse(page.ToString()) - 1), 15, q);
            return View();
        }
        public ActionResult HotelExamined(int? page)
        {
            var q = Query.EQ("审核数据.审核状态", 审核状态.审核通过);


            int count = (int)酒店管理.计数酒店(0, 0, q);
            int maxpage = Math.Max((count + 15 - 1) / 15, 1);
            if (string.IsNullOrEmpty(page.ToString()) || page < 0 || page > maxpage)
            {
                page = 1;
            }
            ViewData["page"] = page;
            ViewData["listcount"] = count;
            ViewData["pagesize"] = 15;
            ViewData["酒店列表"] = 酒店管理.查询酒店(15 * (int.Parse(page.ToString()) - 1), 15, q);
            return View();
        }
        public ActionResult HotelDetail()
        {
            string id = Request.Params["id"];
            ViewData["通过状态"] = Request.Params["action"];
            酒店 k = 酒店管理.查找酒店(long.Parse(id));
            return View(k);
        }
        public void HotelDel()
        {
            string id = Request.Params["id"];
            酒店管理.删除酒店(long.Parse(id));
        }
        public void Hotel_Ticket_Examine()
        {
            string id = Request.Params["id"];//酒店Id
            string exatype = Request.Params["type"];//审核类型
            if (exatype == "审核通过")
            {
                var k = 酒店管理.查找酒店(long.Parse(id));
                k.审核数据.审核状态 = 审核状态.审核通过;
                酒店管理.更新酒店(k);
            }
            if (exatype == "审核不通过")
            {
                var k = 酒店管理.查找酒店(long.Parse(id));
                k.审核数据.审核状态 = 审核状态.审核未通过;
                酒店管理.更新酒店(k);
            }
        }

        public void Ticket_Examine()
        {
            string id = Request.Params["id"];//酒店Id
            string exatype = Request.Params["type"];//审核类型
            if (exatype == "审核通过")
            {
                var k = 机票代售点管理.查找机票代售点(long.Parse(id));
                k.审核数据.审核状态 = 审核状态.审核通过;
                机票代售点管理.更新机票代售点(k);
            }
            if (exatype == "审核不通过")
            {
                var k = 机票代售点管理.查找机票代售点(long.Parse(id));
                k.审核数据.审核状态 = 审核状态.审核未通过;
                机票代售点管理.更新机票代售点(k);
            }
        }
        public ActionResult GoodAttrStatistic()
        {
            IEnumerable<商品分类> goodclass = 商品分类管理.查找子分类();
            return View(goodclass);
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
            g.Add("必读", Ggs.Where(o => o.内容基本信息.重要程度 == 重要程度.必读).Count().ToString());
            g.Add("特别重要", Ggs.Where(o => o.内容基本信息.重要程度 == 重要程度.特别重要).Count().ToString());
            g.Add("未指定", Ggs.Where(o => o.内容基本信息.重要程度 == 重要程度.未指定).Count().ToString());
            g.Add("一般", Ggs.Where(o => o.内容基本信息.重要程度 == 重要程度.一般).Count().ToString());
            g.Add("重要", Ggs.Where(o => o.内容基本信息.重要程度 == 重要程度.重要).Count().ToString());

            gnum1.Add("发标公告", Ggs.Where(o => o.公告信息.公告性质 == 公告.公告性质.采购公告).Count().ToString());
            gnum1.Add("废标公告", Ggs.Where(o => o.公告信息.公告性质 == 公告.公告性质.废标公告).Count().ToString());
            gnum1.Add("技术公告", Ggs.Where(o => o.公告信息.公告性质 == 公告.公告性质.技术公告).Count().ToString());
            gnum1.Add("流标公告", Ggs.Where(o => o.公告信息.公告性质 == 公告.公告性质.流标公告).Count().ToString());
            gnum1.Add("预公告", Ggs.Where(o => o.公告信息.公告性质 == 公告.公告性质.预公告).Count().ToString());
            gnum1.Add("中标公告", Ggs.Where(o => o.公告信息.公告性质 == 公告.公告性质.中标结果公示).Count().ToString());

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

            gnum3.Add("单一来源", Ggs.Where(o => o.公告信息.公告类别 == 公告.公告类别.单一来源).Count().ToString());
            gnum3.Add("公开招标", Ggs.Where(o => o.公告信息.公告类别 == 公告.公告类别.公开招标).Count().ToString());
            gnum3.Add("竞争性谈判", Ggs.Where(o => o.公告信息.公告类别 == 公告.公告类别.竞争性谈判).Count().ToString());
            gnum3.Add("其他", Ggs.Where(o => o.公告信息.公告类别 == 公告.公告类别.其他).Count().ToString());
            gnum3.Add("网上竞标", Ggs.Where(o => o.公告信息.公告类别 == 公告.公告类别.网上竞标).Count().ToString());
            gnum3.Add("未设置", Ggs.Where(o => o.公告信息.公告类别 == 公告.公告类别.未设置).Count().ToString());
            gnum3.Add("协议采购", Ggs.Where(o => o.公告信息.公告类别 == 公告.公告类别.协议采购).Count().ToString());
            gnum3.Add("询价采购", Ggs.Where(o => o.公告信息.公告类别 == 公告.公告类别.询价采购).Count().ToString());
            gnum3.Add("邀请招标", Ggs.Where(o => o.公告信息.公告类别 == 公告.公告类别.邀请招标).Count().ToString());

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
        public ActionResult Part_AdStatistic()
        {
            string startdate = Request.Params["startdate"];
            string enddate = Request.Params["enddate"];
            long listcount;
            IMongoQuery q = null;
            q = Query<公告>.Where(O => O.内容主体.发布时间 >= Convert.ToDateTime(startdate) && O.内容主体.发布时间 <= Convert.ToDateTime(enddate));

            var a = q.And(Query.EQ("公告信息.公告性质", 公告.公告性质.采购公告));
            var b = q.And(Query.EQ("公告信息.公告性质", 公告.公告性质.废标公告));
            var c = q.And(Query.EQ("公告信息.公告性质", 公告.公告性质.技术公告));
            var d = q.And(Query.EQ("公告信息.公告性质", 公告.公告性质.流标公告));
            var e = q.And(Query.EQ("公告信息.公告性质", 公告.公告性质.预公告));
            var f = q.And(Query.EQ("公告信息.公告性质", 公告.公告性质.中标结果公示));
            listcount = (int)公告管理.计数公告(0, 0, q);

            int page = 1;
            int.TryParse(Request.Params["page"], out page);

            ViewData["currentpage"] = page;
            ViewData["pagecount"] = Math.Max((listcount + 5 - 1) / 5, 1);
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

            return PartialView("Part_View/Part_AdStatistic");
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
            return PartialView("Part_View/Part_AdStatistic_ByType");
        }

        public ActionResult AcceptanceStatistic()
        {
            var year = DateTime.Now.Year;
            //设置每一年的起始时间与结束时间
            var startDate = new DateTime(year, 1, 1, 0, 0, 0);
            var endDate = new DateTime(year, 12, 31, 23, 59, 59);

            var dic = new List<Tuple<string, int, string>>();  // 月份，本月新增验收单数量，本月新增占全年新增的百分比
            //var 各个供应商验收单数量 = new List<Tuple<string, int, int, int>>();//供应商名,验收单总数,已作废数量,未作废数量
            var 本年新增验收单 = 验收单管理.查询验收单(0, 0, Query<验收单>.Where(o => o.基本数据.添加时间 > startDate && o.基本数据.添加时间 < endDate));
            var 本年新增数量 = 验收单管理.计数验收单(0, 0, Query<验收单>.Where(o => o.基本数据.添加时间 > startDate && o.基本数据.添加时间 < endDate));

            //统计每年各个月份新增验收单数量及占全年的百分比
            for (int i = 0; i < 12; i++)
            {
                var 本月新增数量 = 本年新增验收单.Where(o => o.基本数据.添加时间.Month == i + 1).Count();
                if (本年新增数量 <= 0 || 本月新增数量 <= 0)
                {
                    dic.Add(Tuple.Create((i + 1).ToString(), 0, "0"));
                }
                else
                {
                    dic.Add(Tuple.Create((i + 1).ToString(), (int)本月新增数量, Math.Round((float)本月新增数量 / 本年新增数量 * 100).ToString()));
                }
            }
            //统计各个供应商所添加验收单数量
            //foreach (var k in ysdlist)
            //{
            //    if (k.供应商链接.用户数据 != null)
            //    {
            //        var 供应商已存在 = 各个供应商验收单数量.Where(o => o.Item1 == k.供应商链接.用户数据.企业基本信息.企业名称);
            //        if (供应商已存在.Count() <= 0)
            //        {
            //            var 验收单数量 = ysdlist.Where(o => o.供应商链接.用户ID == k.供应商链接.用户ID).Count();
            //            var 已作废数量 = ysdlist.Where(o => o.供应商链接.用户ID == k.供应商链接.用户ID && o.是否作废 == true).Count();
            //            各个供应商验收单数量.Add(Tuple.Create(k.供应商链接.用户数据.企业基本信息.企业名称, 验收单数量, 已作废数量, 验收单数量 - 已作废数量));
            //        }
            //    }
            //    else
            //    {
            //        各个供应商验收单数量.Add(Tuple.Create("所属企业不存在",0,0,0));
            //    }

            //}
            //var 所属企业不存在 = 各个供应商验收单数量.Where(o => o.Item1 == "所属企业不存在").Count();
            //各个供应商验收单数量.Add(Tuple.Create("所属企业不存在", 所属企业不存在, 0, 0));
            ViewData["年度统计"] = dic;
            ViewData["本年新增数量"] = 本年新增数量;
            //ViewData["各个供应商验收单数量"] = 各个供应商验收单数量.OrderByDescending(o => o.Item2).Take(10);
            ViewData["所有验收单数量"] = 验收单管理.计数验收单(0, 0);
            return View();
        }

        public ActionResult Part_AcceptanceStatistic()
        {
            var year = Request.Params["year"];
            var gys_name = Request.Params["gys"];
            var ysdlist = 验收单管理.查询验收单(0, 0);
            var dic = new List<Tuple<string, string, string>>();
            decimal 某供应商全年成交总金额 = 0;
            if (!string.IsNullOrWhiteSpace(gys_name))
            {
                某供应商全年成交总金额 = ysdlist.Where(
                    o => o.供应商链接.用户数据!=null 
                        && o.供应商链接.用户数据.企业基本信息.企业名称.Contains(gys_name)
                        && o.基本数据.添加时间.Year == int.Parse(year)
                        && o.审核数据.审核状态 == 审核状态.审核通过
                        && o.是否已经打印 == true
                        && o.是否作废 == false)
                    .Sum(o => o.总金额);

                for (int i = 0; i < 12; i++)
                {
                    var 本月成交总金额 = ysdlist.Where(
                        o => o.供应商链接.用户数据!=null 
                            && o.供应商链接.用户数据.企业基本信息.企业名称.Contains(gys_name)
                            && o.基本数据.添加时间.Year == int.Parse(year)
                            && o.基本数据.添加时间.Month == i + 1
                            && o.审核数据.审核状态 == 审核状态.审核通过
                            && o.是否已经打印 == true
                            && o.是否作废 == false)
                        .Sum(o => o.总金额);
                    if (某供应商全年成交总金额 <= 0 || 本月成交总金额 <= 0)
                    {
                        dic.Add(Tuple.Create((i + 1).ToString(), "0", "0"));
                    }
                    else
                    {
                        dic.Add(Tuple.Create((i + 1).ToString(), 本月成交总金额.ToString("C"), Math.Round(double.Parse(本月成交总金额.ToString("F2")) / double.Parse(某供应商全年成交总金额.ToString()) * 100).ToString()));
                    }
                }
                ViewBag.IsGys = true;
                ViewData["年度统计"] = dic;
                ViewData["本年成交总金额"] = 某供应商全年成交总金额.ToString("C");
            }
            else
            {
                var 本年新增数量 = ysdlist.Where(o => o.基本数据.添加时间.Year == int.Parse(year)).Count();
                for (int i = 0; i < 12; i++)
                {
                    var 本月新增数量 = ysdlist.Where(o => o.基本数据.添加时间.Year == int.Parse(year) && o.基本数据.添加时间.Month == i + 1).Count();
                    if (本年新增数量 <= 0 || 本月新增数量 <= 0)
                    {
                        dic.Add(Tuple.Create((i + 1).ToString(), "0", "0"));
                    }
                    else
                    {
                        dic.Add(Tuple.Create((i + 1).ToString(), 本月新增数量.ToString(), Math.Round(double.Parse(本月新增数量.ToString("F2")) / 本年新增数量 * 100).ToString()));
                    }
                }
                ViewBag.IsGys = false;
                ViewData["年度统计"] = dic;
                ViewData["本年新增数量"] = 本年新增数量;
            }
            return PartialView("Part_View/Part_AcceptanceStatistic");
        }
        public ActionResult Part_AcceptanceStatisticUnit()
        {
            var year = Request.Params["year"];
            var ysdlist = 验收单管理.查询验收单(0, 0);
            var unitlist = new List<Tuple<string, decimal>>();
            var units = ysdlist.Select(o => o.收货单位).Distinct();
            foreach (var k in units)
            {
                var account = 0M;
                if (!string.IsNullOrWhiteSpace(year))
                {
                    account = ysdlist.Where(o => o.收货单位 == k && !o.是否作废 && o.基本数据.添加时间.Year == int.Parse(year)).Sum(o => o.总金额);
                }
                else
                {
                    account = ysdlist.Where(o => o.收货单位 == k && !o.是否作废).Sum(o => o.总金额);
                }
                unitlist.Add(Tuple.Create(k, account));
            }
            if (!string.IsNullOrWhiteSpace(year))
            {
                ViewData["年份"] = year;
            }
            else
            {
                ViewData["年份"] = "0";
            }
            ViewData["年度统计"] = unitlist.OrderByDescending(o => o.Item2);
            return PartialView("Part_View/Part_AcceptanceStatisticUnit");
        }


        public class GysPrintNum
        {
            public string GysName { get; set; }
            public int PrintNumber { get; set; }
        }
        public string GysPrintNumber()
        {
            var ysd = 验收单管理.查询验收单(0, 0);
            var gys = ysd.Select(o => o.供应商链接.用户ID).Distinct();
            var listnumber = new List<GysPrintNum>();
            foreach (var k in gys)
            {
                var count = ysd.Where(o => o.供应商链接.用户ID == k && o.审核数据.审核状态 == 审核状态.审核通过 && o.是否已经打印 && !o.是否作废).Count();
                var gysname = 用户管理.查找用户<供应商>(k, includeDeleted: true);
                listnumber.Add(new GysPrintNum() { GysName = gysname.企业基本信息.企业名称, PrintNumber = count });

            }
            listnumber = listnumber.OrderByDescending(o => o.PrintNumber).ToList();
            var jsonstr = JsonConvert.SerializeObject(listnumber);
            return jsonstr;
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
        public ActionResult Part_GysStatisticNum()
        {
            string startdate = Request.Params["startdate"];
            string enddate = Request.Params["enddate"];
            var state = Request.Params["state"];//审核状态
            long listcount;
            IMongoQuery q = null;
            q = Query<供应商>.Where(O => O.基本数据.添加时间 >= Convert.ToDateTime(startdate) && O.基本数据.添加时间 <= Convert.ToDateTime(enddate));
            if (state == "未审核")
            {
                q = q.And(Query<供应商>.Where(o => o.审核数据.审核状态 == 审核状态.未审核));
            }
            if (state == "审核未通过")
            {
                q = q.And(Query<供应商>.Where(o => o.审核数据.审核状态 == 审核状态.审核未通过));
            }
            if (state == "审核通过")
            {
                q = q.And(Query<供应商>.Where(o => o.审核数据.审核状态 == 审核状态.审核通过));
            }
            listcount = (int)用户管理.计数用户<供应商>(0, 0, q);
            int page = 1;
            int.TryParse(Request.Params["page"], out page);

            ViewData["currentpage"] = page;
            ViewData["pagecount"] = Math.Max((listcount + 10 - 1) / 10, 1);
            ViewData["startTime"] = startdate;
            ViewData["endTime"] = enddate;
            ViewData["区间查询数"] = listcount;
            ViewData["供应商总数"] = (int)用户管理.计数用户<供应商>(0, 0);

            if (!startdate.IsEmpty() && !enddate.IsEmpty())
            {
                ViewData["查询供应商数量"] = 用户管理.查询用户<供应商>(10 * (page - 1), 10, q, false, SortBy.Descending("基本数据.添加时间"));
            }
            else
            {
                ViewData["查询供应商数量"] = 用户管理.查询用户<供应商>(10 * (page - 1), 10);
            }
            return PartialView("Part_View/Part_GysStatisticNum");

        }
        public ActionResult ExpertStatistic()
        {
            string startdate = DateTime.Now.Year.ToString() + "/" + (DateTime.Now.Month != 1 ? DateTime.Now.Month - 1 : DateTime.Now.Month).ToString() + "/1";
            string enddate = DateTime.Now.Year.ToString() + "/" + (DateTime.Now.Month != 1 ? DateTime.Now.Month - 1 : DateTime.Now.Month).ToString() + "/" + DateTime.DaysInMonth(DateTime.Now.Year, (DateTime.Now.Month != 1 ? DateTime.Now.Month - 1 : DateTime.Now.Month));

            IMongoQuery q = null;
            q = Query<专家>.Where(O => O.基本数据.添加时间 >= Convert.ToDateTime(startdate) && O.基本数据.添加时间 <= Convert.ToDateTime(enddate));
            long listcount = (int)用户管理.计数用户<专家>(0, 0, q);
            long maxpagesize = Math.Max((listcount + 10 - 1) / 10, 1);//10，每页显示10条记录
            int page = 1;
            ViewData["currentpage"] = page;
            ViewData["pagecount"] = maxpagesize;
            ViewData["startTime"] = startdate;
            ViewData["endTime"] = enddate;
            ViewData["专家总数"] = (int)用户管理.计数用户<专家>(0, 0);
            ViewData["区间查询数"] = listcount;
            ViewData["所有专家"] = 用户管理.查询用户<专家>(10 * (page - 1), 10, q, false, SortBy.Descending("基本数据.添加时间"));
            return View();
        }

        public ActionResult Part_ExpertStatisticNum()
        {
            string startdate = Request.Params["startdate"];
            string enddate = Request.Params["enddate"];
            long listcount;
            IMongoQuery q = null;
            q = Query<专家>.Where(O => O.基本数据.添加时间 >= Convert.ToDateTime(startdate) && O.基本数据.添加时间 <= Convert.ToDateTime(enddate));

            listcount = (int)用户管理.计数用户<专家>(0, 0, q);

            int page = 1;
            int.TryParse(Request.Params["page"], out page);

            ViewData["currentpage"] = page;
            ViewData["pagecount"] = Math.Max((listcount + 10 - 1) / 10, 1);
            ViewData["startTime"] = startdate;
            ViewData["endTime"] = enddate;
            ViewData["区间查询数"] = listcount;
            ViewData["专家总数"] = (int)用户管理.计数用户<专家>(0, 0);

            if (!startdate.IsEmpty() && !enddate.IsEmpty())
            {
                ViewData["查询专家数量"] = 用户管理.查询用户<专家>(10 * (page - 1), 10, q, false, SortBy.Descending("基本数据.添加时间"));
            }
            else
            {
                ViewData["查询专家数量"] = 用户管理.查询用户<专家>(10 * (page - 1), 10);
            }
            return PartialView("Part_View/Part_ExpertStatisticNum");

        }

        public ActionResult RegistrationCount()
        {
            return View();
        }
        public ActionResult Part_RegistrationCount()
        {
            ViewData["currentpage"] = 1;
            ViewData["pagecount"] = 1;
            return PartialView("Part_View/Part_RegistrationCount");
        }

        public ActionResult GetRegistrationCount()
        {
            string startdate = Request.Params["startdate"];
            string enddate = Request.Params["enddate"];

            IList<供应商> gys = new List<供应商>();

            if (!string.IsNullOrWhiteSpace(startdate) && !string.IsNullOrWhiteSpace(enddate))
            {
                var m = 招标采购预报名管理.查询招标采购预报名(0, 0);
                foreach (var item in m)
                {
                    foreach (var it in item.预报名供应商列表)
                    {
                        if (it.报名时间 > Convert.ToDateTime(startdate) && it.报名时间 < Convert.ToDateTime(enddate))
                        {
                            gys.Add(it.供应商链接.用户数据);
                        }
                    }
                }
                ViewData["供应商总数"] = gys.Count;
                ViewData["预报名供应商"] = gys;
                return PartialView("Part_View/Part_RegistrationCount_pre");
            }
            else
            {
                return Content("请求失败");
            }


        }
        public ActionResult RegistrationManange()
        {
            return View();
        }
        public ActionResult Part_RegistrationManange(int? page)
        {
            int listcount = (int)(招标采购预报名管理.计数招标采购预报名(0, 0));
            int maxpage = Math.Max((listcount + 15 - 1) / 15, 1);

            if (string.IsNullOrEmpty(page.ToString()) || page < 0 || page > maxpage)
            {
                page = 1;
            }
            ViewData["page"] = page;
            ViewData["listcount"] = listcount;

            ViewData["pagesize"] = 15;

            ViewData["预报名列表"] = 招标采购预报名管理.查询招标采购预报名(15 * (int.Parse(page.ToString()) - 1), 15);
            return PartialView("Part_View/Part_RegistrationManange");
        }
        public ActionResult RegistrationDetail()
        {
            return View();
        }
        public ActionResult Part_RegistrationDetail(long? id)
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

                ViewData["预报名供应商列表"] = model.预报名供应商列表.Take(pagesize);
            }
            catch
            {
                model = null;
                ViewData["预报名供应商列表"] = new List<Go81WebApp.Models.数据模型.项目数据模型.招标采购预报名._供应商预报名信息>();
            }
            return PartialView("Part_View/Part_RegistrationDetail", model);
        }

        public ActionResult RegistrationDelete(long? id)
        {
            try
            {
                招标采购预报名管理.删除招标采购预报名((long)id);
            }
            catch
            {

            }
            return View("RegistrationManange");
        }

        public ActionResult RegistrationChange()
        {
            var id = Request.Params["id"];
            try
            {
                招标采购预报名 model = 招标采购预报名管理.查找招标采购预报名(long.Parse(id));
                model.预报名已关闭 = !model.预报名已关闭;
                招标采购预报名管理.更新招标采购预报名(model, false);
                return Content("1");
            }
            catch
            {
                return Content("0");
            }
        }

        ///////////////////////////////////////////////////////////////////商品修改
        public ActionResult Gys_Product_Modify()
        {
            return View();
        }
        public ActionResult Part_Gys_Product_Modify(int? id)
        {
            try
            {
                var pro_info = 商品管理.查找商品((long)id);
                if (currentUser.GetType() != typeof(运营团队) && pro_info.商品信息.所属供应商.用户ID != currentUser.Id)
                {
                    return Content("<script>top.location.href='/错误页面/NoPrivilege'</script>");
                }
                var att = 商品分类管理.查找分类(pro_info.商品信息.所属商品分类.商品分类ID).商品属性模板;

                ViewData["classid"] = pro_info.商品信息.所属商品分类.商品分类ID;
                ViewData["商品属性模板"] = att;
                ViewData["pro_id"] = id;

                //获取当前的分类已有精确型号
                var model = 商品管理.查询分类下商品(pro_info.商品信息.所属商品分类.商品分类ID, 0, 0,
                    Query<商品>.Where(o => o.审核数据.审核状态 == 审核状态.审核通过 && o.商品信息.精确型号 != null && o.商品信息.精确型号 != string.Empty));
                Dictionary<string, List<string>> model_str = new Dictionary<string, List<string>>();

                if (model != null && model.Any())
                {
                    foreach (var item in model)
                    {
                        if (!string.IsNullOrWhiteSpace(item.商品信息.品牌))
                        {
                            if (model_str.ContainsKey(item.商品信息.品牌))
                            {
                                if (!model_str[item.商品信息.品牌].Contains(item.商品信息.精确型号))
                                {
                                    model_str[item.商品信息.品牌].Add(item.商品信息.精确型号);
                                }
                            }
                            else
                            {
                                model_str.Add(item.商品信息.品牌, new List<string> { item.商品信息.精确型号 });
                            }
                        }
                        else
                        {
                            if (model_str.ContainsKey("其他品牌"))
                            {
                                if (!model_str["其他品牌"].Contains(item.商品信息.精确型号))
                                {
                                    model_str["其他品牌"].Add(item.商品信息.精确型号);
                                }
                            }
                            else
                            {
                                model_str.Add("其他品牌", new List<string> { item.商品信息.精确型号 });
                            }
                        }
                    }
                }
                ViewData["精确型号"] = model_str;
                //获取当前的分类已有精确型号

                return PartialView("Part_View/Part_Gys_Product_Modify", pro_info);
            }
            catch
            {
                return Content("<script>location.href='javascript:history.go(-1)';</script>");
            }

        }
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult Gys_Product_Modify(string submitcontent, 商品 model)
        {
            try
            {
                //商品 Pro_Model = new 商品();

                商品 Pro_Model = 商品管理.查找商品(model.Id);
                if (Pro_Model.中标商品)
                {
                    Pro_Model.中标信息 = new List<商品._中标信息>();
                    //判断是否有项目编号
                    var proliststr = Request.Form["proliststr"];
                    var ggidlist = new List<long>();
                    if (!string.IsNullOrWhiteSpace(proliststr))
                    {
                        Pro_Model.基本数据.已屏蔽 = false;
                        var pronamelist = proliststr.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
                        foreach (var proname in pronamelist)
                        {
                            Pro_Model.中标信息.Add(new 商品._中标信息 { 
                                  中标项目编号= Request.Form["pronum___" + proname],
                                  中标数量 = int.Parse(Request.Form["procount___" + proname]),
                                  中标金额 = decimal.Parse(Request.Form["proprice___" + proname]),
                            });

                            var gg_id = long.Parse(Request.Form["ggid___" + proname]);
                            ggidlist.Add(gg_id);
                            if (gg_id != -1)
                            {
                                var gg = 公告管理.查找公告(gg_id);
                                var linklist = gg.中标商品链接.Select(o => o.商品ID);
                                if (!linklist.Contains(Pro_Model.Id))
                                {
                                    gg.中标商品链接.Add(new 商品链接 { 商品ID = Pro_Model.Id });
                                    公告管理.更新公告(gg, false, false);
                                }
                            }
                        }
                        var ggtemplist = 公告管理.查询公告(0, 0, Query<公告>.Where(o => o.中标商品链接.Any(p => p.商品ID == Pro_Model.Id)).And(Query<公告>.NotIn(o => o.Id, ggidlist)));
                        foreach (var ggtemp in ggtemplist)
                        {
                            ggtemp.中标商品链接.RemoveAll(o => o.商品ID == Pro_Model.Id);
                            公告管理.更新公告(ggtemp, false, false);
                        }
                    }
                }

                var exactmodel = Pro_Model.商品信息.精确型号;
                var deleteattachstr = Request.Form["deletattachtext"];

                //Pro_Model.Id = int.Parse(Request.Form["idsttrsrr"]);
                //Pro_Model.商品信息.所属供应商.用户ID = 商品管理.查找商品(Pro_Model.Id).商品信息.所属供应商.用户ID;

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
                    Pro_Model.商品信息.品牌 = model.商品信息.品牌.Trim();
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
                if (!string.IsNullOrEmpty(Request.Form["pro_imgstr"]))
                {
                    Pro_Model.商品信息.商品图片 = new List<string>(Request.Form["pro_imgstr"].Substring(0, Request.Form["pro_imgstr"].Length - 1).Split('|'));
                }
                Pro_Model.商品数据.商品简介 = model.商品数据.商品简介;
                Pro_Model.商品数据.商品详情 = model.商品数据.商品详情;
                Pro_Model.商品数据.售后服务 = model.商品数据.售后服务;
                Pro_Model.采购信息 = model.采购信息;
                if (submitcontent == "审核通过")
                {
                    Pro_Model.商品信息.精确型号 = Request.Form["model_str"].Trim();
                }
                if (submitcontent == "审核不通过")
                {
                    Pro_Model.基本数据.已屏蔽 = true;
                }

                商品管理.更新商品(Pro_Model, false, false);
                //CreateIndex(Pro_Model, "/Lucene.Net/IndexDic/Product");

                deleteIndex("/Lucene.Net/IndexDic/Product", Pro_Model.Id.ToString());

                if (!string.IsNullOrEmpty(deleteattachstr))
                {
                    string[] delestr = deleteattachstr.Trim().Split('|');
                    foreach (var item in delestr)
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
                //进行审核
                if (submitcontent == "审核通过")
                {
                    try
                    {
                        //商品审核时，检查属性模板，有新值时更新属性模板
                        商品分类 pro_class = 商品分类管理.查找分类(Pro_Model.商品信息.所属商品分类.商品分类ID);
                        foreach (var arr in pro_class.商品属性模板)
                        {
                            foreach (var item in arr.Value)
                            {
                                var vals = Pro_Model.商品数据.商品属性[arr.Key][item.Key];
                                if (item.Value.属性类型 == 属性类型.复选)
                                {
                                    foreach (var selfitem in vals)
                                    {
                                        if (!item.Value.值.Contains(selfitem))
                                        {
                                            pro_class.商品属性模板[arr.Key][item.Key].值.Add(selfitem.Trim());
                                        }
                                    }
                                }
                            }
                        }
                        商品分类管理.更新分类(pro_class);
                    }
                    catch
                    {

                    }

                    商品管理.审核商品(Pro_Model.Id, this.HttpContext.获取当前用户<运营团队>().Id, 审核状态.审核通过);
                    deleteIndex("/Lucene.Net/IndexDic/Product", Pro_Model.Id.ToString());
                    CreateIndex(Pro_Model, "/Lucene.Net/IndexDic/Product");

                    //更新精确型号的索引
                    if (Pro_Model.商品信息.精确型号 != null)
                    {
                        var productofclass = 商品管理.查询分类下商品(Pro_Model.商品信息.所属商品分类.商品分类ID, 0, 2, Query<商品>.EQ(o => o.审核数据.审核状态, 审核状态.审核通过).And(Query<商品>.EQ(o => o.商品信息.精确型号, Pro_Model.商品信息.精确型号))).Count();
                        if (productofclass < 2)
                        {
                            //该商品是该精确型号的第一个商品,写入索引
                            CreateIndex_ProductCatalog(Pro_Model.商品信息.精确型号.Trim(), "/Lucene.Net/IndexDic/ProductCatalog");
                        }
                    }

                }
                else if (submitcontent == "审核不通过")
                {
                    商品管理.审核商品(Pro_Model.Id, this.HttpContext.获取当前用户<运营团队>().Id, 审核状态.审核未通过, Request.Form["notpassreadon"]);

                    deleteIndex("/Lucene.Net/IndexDic/Product", Pro_Model.Id.ToString());
                }
                //进行审核

                return RedirectToAction("GoodExamine", "运营团队后台");
            }
            catch
            {
                return Content("<script>alert('输入有误!请检查数据格式');location.href='javascript:history.go(-1)';</script>");
            }
        }
        [HttpPost]
        public ActionResult UploadImages(HttpPostedFileBase fileData)
        {
            if (fileData != null)
            {
                //string filePath1 = Server.MapPath(ConfigurationManager.AppSettings["上传文件根路径"]);
                string filePath = 上传管理.获取上传路径<商品>(媒体类型.图片, 路径类型.服务器本地路径);
                //获取文件后缀名
                //string extension = fileData.FileName.Substring(fileData.FileName.LastIndexOf("."), (fileData.FileName.Length - fileData.FileName.LastIndexOf(".")));
                //按年月生成文件夹
                //string folderName = StrHelp.MakeFolderName();

                //生成新文件名
                //string fileName = StrHelp.MakeFileRndName() + extension.ToLower();
                string fileName = fileData.FileName;
                if (fileName.LastIndexOf("\\") > -1)
                {
                    fileName = fileName.Substring(fileName.LastIndexOf("\\") + 1);
                }
                fileName = 上传管理.获取文件名(fileName).Replace("{", "").Replace("}", "");
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

                //缩略图550(限宽)
                //UploadPic.MakeThumbnail(string.Format("{0}original\\{1}", filePath, fileName), string.Format("{0}550\\{1}", filePath, fileName), 550, 0, "W");

                return Content(string.Format("{0}/original/{1}", 上传管理.获取上传路径<商品>(媒体类型.图片, 路径类型.不带域名根路径), fileName));
            }
            else
            {
                return Content("0");
            }
        }
        public int DeleteImages1()
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
        public ActionResult DeleteImages()
        {

            string qid = Request.Params["q"] ?? "";
            string name = Request.Params["n"] ?? "";
            try
            {
                if (!string.IsNullOrEmpty(qid) && !string.IsNullOrEmpty(name))
                {
                    UploadPic.DelPic(string.Format("{0}", name));
                    UploadPic.DelPic(string.Format("{0}", name.Replace("original", "350X350")));
                    UploadPic.DelPic(string.Format("{0}", name.Replace("original", "150X150")));
                    UploadPic.DelPic(string.Format("{0}", name.Replace("original", "50X50")));
                    return Content("1");
                }
                else
                {
                    return Content("0");
                }
            }
            catch { return Content("0"); }
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
        ///////////////////////////////////////////////////////////////////商品修改

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

                供应商服务记录 增值服务记录 = null;
                var 增值服务记录列表 = 供应商服务记录管理.查询供应商服务记录(0, 0, Query<供应商服务记录>.Where(o => o.所属供应商.用户ID == model.Id));
                if (增值服务记录列表.Count() > 0)
                {
                    增值服务记录 = 增值服务记录列表.First();
                }
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

        public ActionResult AccountInfoManage()
        {
            var 已申请军采通供应商 = 供应商服务记录管理.查询供应商服务记录(0, 0, Query<供应商服务记录>.Where(o => o.申请中的服务.Count() > 0));
            var 供应商 = new List<供应商>();
            if (已申请军采通供应商.Count() > 0)
            {
                foreach (var item in 已申请军采通供应商)
                {
                    var gys = 用户管理.查找用户<供应商>(item.所属供应商.用户ID);
                    供应商.Add(gys);
                }
            }
            long listcount = 供应商.Count();
            long maxpagesize = Math.Max((listcount + JCTAPPLYPAGESIZE - 1) / JCTAPPLYPAGESIZE, 1);
            int page = 1;
            ViewData["currentpage"] = page;
            ViewData["pagecount"] = maxpagesize;

            供应商 = 供应商.Skip(JCTAPPLYPAGESIZE * (page - 1)).Take(JCTAPPLYPAGESIZE).ToList();
            return View(供应商);
        }
        public ActionResult Part_AccountInfoManage()
        {
            var 已申请军采通供应商 = 供应商服务记录管理.查询供应商服务记录(0, 0, Query<供应商服务记录>.Where(o => o.申请中的服务.Count() > 0));
            var 供应商 = new List<供应商>();
            if (已申请军采通供应商.Count() > 0)
            {
                foreach (var item in 已申请军采通供应商)
                {
                    var gys = 用户管理.查找用户<供应商>(item.所属供应商.用户ID);
                    供应商.Add(gys);
                }
            }
            long listcount = 供应商.Count();
            long maxpagesize = Math.Max((listcount + JCTAPPLYPAGESIZE - 1) / JCTAPPLYPAGESIZE, 1);
            var page = int.Parse(Request.Params["page"]);
            if (string.IsNullOrEmpty(page.ToString()) || page < 0 || page > maxpagesize)
            {
                page = 1;
            }

            ViewData["currentpage"] = page;
            ViewData["pagecount"] = maxpagesize;

            供应商 = 供应商.Skip(JCTAPPLYPAGESIZE * (page - 1)).Take(JCTAPPLYPAGESIZE).ToList();
            return PartialView("Part_View/Part_AccountInfoManage", 供应商);
        }

        public ActionResult ValueAddedService()
        {
            var id = Request.Params["id"];//供应商ID
            var 可申请 = new List<Tuple<string, decimal, 扣费类型, 通过状态, long>>();//服务名/金额/扣费类型/通过状态/id/开通个数、结束时间
            var 申请中 = new List<Tuple<string, decimal, 扣费类型, 通过状态, long, int>>();
            var 已开通 = new List<Tuple<string, decimal, 扣费类型, 通过状态, long, int, int, Tuple<DateTime>>>();
            var f = 供应商服务记录管理.查询供应商服务记录(0, 0, Query<供应商服务记录>.Where(o => o.所属供应商.用户ID == long.Parse(id)));
            if (f != null && f.Count() <= 0)
            {
                var a = new 供应商服务记录();
                a.所属供应商.用户ID = long.Parse(id);
                foreach (var j in 扣费规则.规则列表)
                {
                    var b = new 供应商增值服务申请记录();
                    b.所申请项目名 = j.扣费项目名;
                    b.所属供应商.用户ID = long.Parse(id);
                    a.可申请的服务.Add(b);
                    可申请.Add(Tuple.Create(j.扣费项目名, j.扣费金额, j.扣费类型, b.是否通过, b.Id));
                }
                供应商服务记录管理.添加供应商服务记录(a);

                ViewData["已开通服务"] = a.已开通的服务;
                ViewData["可申请服务"] = 可申请;
                ViewData["申请中服务"] = a.申请中的服务;
            }
            else
            {
                if (f != null)
                {
                    foreach (var i in f.First().可申请的服务)
                    {
                        var i_0 = 扣费规则.规则列表.Where(o => o.扣费项目名 == i.所申请项目名).First();
                        可申请.Add(Tuple.Create(i_0.扣费项目名, i_0.扣费金额, i_0.扣费类型, i.是否通过, i.Id));
                    }
                    foreach (var i in f.First().申请中的服务)
                    {
                        var i_0 = 扣费规则.规则列表.Where(o => o.扣费项目名 == i.所申请项目名).First();
                        申请中.Add(Tuple.Create(i_0.扣费项目名, i_0.扣费金额, i_0.扣费类型, i.是否通过, i.Id, i.开通个数));
                    }
                    foreach (var i in f.First().已开通的服务)
                    {
                        var i_0 = 扣费规则.规则列表.Where(o => o.扣费项目名 == i.所申请项目名).First();
                        // var rest = Tuple.Create();
                        已开通.Add(Tuple.Create(i_0.扣费项目名, i_0.扣费金额, i_0.扣费类型, i.是否通过, i.Id, i.服务期限, i.开通个数, i.签订时间));
                    }
                }

                ViewData["已开通服务"] = 已开通;
                ViewData["可申请服务"] = 可申请;
                ViewData["申请中服务"] = 申请中;
            }

            ViewData["供应商ID"] = id;

            var 余额 = 供应商充值余额管理.查询供应商充值余额(0, 0, Query<供应商充值余额>.Where(o => o.所属供应商.用户ID == long.Parse(id)));
            var 供应商账户余额 = new 供应商充值余额();
            if (余额 != null && 余额.Count() > 0)
            {
                供应商账户余额 = 余额.First();
            }
            else
            {
                供应商账户余额.所属供应商.用户ID = long.Parse(id);
                供应商账户余额.可用余额 = 0;
                供应商充值余额管理.添加供应商充值余额(供应商账户余额);
            }

            return View(供应商账户余额);
        }
        [HttpPost]
        public string ApplyApproval()
        {
            var id = Request.Params["ApplyContractId"];//供应商ID
            var applyid = Request.Params["ApplyServiceId"];//所申请项目id
            var signtime = Request.Params["signtime"];//签订时间
            var type_operate = Request.Params["type"];//操作类型 1：通过/2：开通/3：撤销申请
            var 服务列表 = 供应商服务记录管理.查询供应商服务记录(0, 0, Query<供应商服务记录>.Where(o => o.所属供应商.用户ID == long.Parse(id))).Count() > 0
                ? 供应商服务记录管理.查询供应商服务记录(0, 0, Query<供应商服务记录>.Where(o => o.所属供应商.用户ID == long.Parse(id))).First()
                : new 供应商服务记录();
            var gh = 服务列表.申请中的服务.Find(o => o.Id == long.Parse(applyid));
            if (int.Parse(type_operate) == 1)
            {
                gh.是否通过 = 通过状态.通过;
                供应商服务记录管理.更新供应商服务记录(服务列表);
                var a = 供应商增值服务申请记录管理.查找供应商增值服务申请记录(long.Parse(applyid));
                a.是否通过 = 通过状态.通过;
                a.签订时间 = gh.签订时间;
                供应商增值服务申请记录管理.更新供应商增值服务申请记录(a);
                return "批准成功！";
            }
            if (int.Parse(type_operate) == 2)
            {
                gh.签订时间 = DateTime.Parse(signtime);
                var gys = 用户管理.查找用户<供应商>(long.Parse(id));
                var tt = 供应商充值余额管理.查询供应商充值余额(0, 0, Query<供应商充值余额>.Where(o => o.所属供应商.用户ID == long.Parse(id)));
                var 充值余额 = tt.Count() > 0 ? tt.First() : new 供应商充值余额();
                var 待扣费项目 = 扣费规则.规则列表.Where(o => o.扣费项目名 == gh.所申请项目名).First();
                var 待扣费金额 = (decimal)0;
                var 扣费 = new 供应商计费项目扣费记录();
                var 申请记录 = 供应商增值服务申请记录管理.查找供应商增值服务申请记录(gh.Id);
                申请记录.签订时间 = gh.签订时间;
                扣费.扣费服务项目 = 待扣费项目.扣费项目名;
                扣费.扣费类型 = 待扣费项目.扣费类型;
                扣费.扣费时间 = gh.签订时间;
                扣费.所属供应商.用户ID = long.Parse(id);
                if (待扣费项目.扣费类型 == 扣费类型.按年扣费)
                {
                    扣费.所属年 = gh.服务期限;
                    gh.结束时间 = 扣费.扣费时间.AddYears(gh.服务期限);
                    申请记录.结束时间 = gh.结束时间;
                    if (待扣费项目.扣费项目名.Contains("商务会员")) //将标准会员升级为商务会员
                    {
                        gys.供应商用户信息.认证级别 = Go81WebApp.Models.数据模型.用户数据模型.供应商.认证级别.军采通商务会员;
                        var 标准会员 = 服务列表.已开通的服务.Find(o => o.所申请项目名.Contains("标准会员"));
                        var 标准会员扣费 = 供应商计费项目扣费记录管理.查询供应商计费项目扣费记录(0, 0, Query<供应商计费项目扣费记录>.Where(o => o.扣费服务项目.Contains("标准会员") && o.所属供应商.用户ID == long.Parse(id)), false, SortBy.Descending("基本数据.添加时间"));
                        var 标准会员扣费金额 = 标准会员扣费.Count() > 0 ? 标准会员扣费.First() : new 供应商计费项目扣费记录();
                        var 已服务时间 = toResult(DateTime.Now, 标准会员扣费金额.扣费时间, diffResultFormat.mm) + 1;
                        if (标准会员 != null)
                        {
                            待扣费金额 = 待扣费项目.扣费金额 * gh.服务期限 - 标准会员扣费金额.扣费金额 - 标准会员扣费金额.扣费金额 / gh.服务期限 / 12 * 已服务时间;
                        }
                        else
                        {
                            待扣费金额 = 待扣费项目.扣费金额 * 扣费.所属年;
                        }
                    }
                    else if (待扣费项目.扣费项目名.Contains("标准会员")) //将基础会员升级为标准会员
                    {
                        gys.供应商用户信息.认证级别 = 供应商.认证级别.军采通标准会员;
                        var 基础会员 = 服务列表.已开通的服务.Find(o => o.所申请项目名.Contains("基础会员"));
                        var 基础会员扣费 = 供应商计费项目扣费记录管理.查询供应商计费项目扣费记录(0, 0, Query<供应商计费项目扣费记录>.Where(o => o.扣费服务项目.Contains("基础会员") && o.所属供应商.用户ID == long.Parse(id)), false, SortBy.Descending("基本数据.添加时间"));
                        var 基础会员扣费金额 = 基础会员扣费.Count() > 0 ? 基础会员扣费.First() : new 供应商计费项目扣费记录();
                        var 已服务时间 = toResult(DateTime.Now, 基础会员扣费金额.扣费时间, diffResultFormat.mm) + 1;
                        if (基础会员 != null)
                        {
                            待扣费金额 = 待扣费项目.扣费金额 * gh.服务期限 - 基础会员扣费金额.扣费金额 + 基础会员扣费金额.扣费金额 / gh.服务期限 / 12 * 已服务时间;
                        }
                        else
                        {
                            待扣费金额 = 待扣费项目.扣费金额 * 扣费.所属年;
                        }
                    }
                    else
                    {
                        if (待扣费项目.扣费项目名.Contains("基础会员"))
                        {
                            gys.供应商用户信息.认证级别 = 供应商.认证级别.军采通基础会员;
                        }
                        待扣费金额 = 待扣费项目.扣费金额 * 扣费.所属年;
                    }
                }
                if (待扣费项目.扣费类型 == 扣费类型.按月扣费)
                {
                    gh.结束时间 = 扣费.扣费时间.AddMonths(gh.服务期限);
                    申请记录.结束时间 = gh.结束时间;
                    扣费.所属月 = gh.服务期限;
                    if (待扣费项目.扣费项目名.Contains("企业主页商品展示"))
                    {
                        var 已开通的企业主页展示窗口 = 服务列表.已开通的服务.Find(o => o.所申请项目名.Contains("企业主页商品展示"));
                        var 企业主页展示窗口扣费 = 供应商计费项目扣费记录管理.查询供应商计费项目扣费记录(0, 0, Query<供应商计费项目扣费记录>.Where(o => o.扣费服务项目 == 待扣费项目.扣费项目名 && o.所属供应商.用户ID == long.Parse(id)), false, SortBy.Descending("基本数据.添加时间"));
                        var 企业主页展示窗口扣费金额 = 企业主页展示窗口扣费.Count() > 0 ? 企业主页展示窗口扣费.First() : new 供应商计费项目扣费记录();
                        var 已服务时间 = toResult(DateTime.Now, 企业主页展示窗口扣费金额.扣费时间, diffResultFormat.mm) + 1;
                        if (企业主页展示窗口扣费.Count() <= 0) { 已服务时间 = 0; }
                        if (已开通的企业主页展示窗口 != null)
                        {
                            var 价格 = 扣费规则.规则列表.Where(o => o.扣费项目名 == 已开通的企业主页展示窗口.所申请项目名).First();
                            待扣费金额 = 待扣费项目.扣费金额 * gh.服务期限 - 企业主页展示窗口扣费金额.扣费金额 + 价格.扣费金额 * 已服务时间;
                        }
                        else
                        {
                            待扣费金额 = 待扣费项目.扣费金额 * 扣费.所属月;
                        }
                    }
                    else
                    {
                        待扣费金额 = 待扣费项目.扣费金额 * 扣费.所属月;
                    }
                }
                if (待扣费项目.扣费类型 == 扣费类型.按次扣费)
                {
                    扣费.所属日 = gh.服务期限;
                    gh.结束时间 = 扣费.扣费时间.AddDays(gh.服务期限);
                    申请记录.结束时间 = gh.结束时间;
                    待扣费金额 = 待扣费项目.扣费金额 * 扣费.所属日;
                }
                扣费.扣费金额 = 待扣费金额;
                if (待扣费金额 > 充值余额.可用余额)
                {
                    return "余额不足，无法开通！";
                }
                else
                {
                    if (gh.所申请项目名.Contains("企业主页商品展示"))
                    {
                        var qyzsck = 服务列表.已开通的服务.Find(o => o.所申请项目名.Contains("企业主页商品展示"));
                        if (qyzsck != null)
                        {
                            服务列表.已开通的服务.Remove(qyzsck);
                            服务列表.可申请的服务.Add(qyzsck);
                        }
                    }
                    if (gh.所申请项目名.Contains("企业推广服务B1"))
                    {
                        var qytgfw = 服务列表.已开通的服务.Find(o => o.所申请项目名.Contains("企业推广服务B1"));
                        if (qytgfw != null)
                        {
                            服务列表.已开通的服务.Remove(qytgfw);
                            服务列表.可申请的服务.Add(qytgfw);
                        }
                    }
                    if (gh.所申请项目名.Contains("基础会员"))
                    {
                        var swhy = 服务列表.已开通的服务.Find(o => o.所申请项目名.Contains("商务会员"));
                        var bzhy = 服务列表.已开通的服务.Find(o => o.所申请项目名.Contains("标准会员"));
                        if (swhy != null)
                        {
                            服务列表.已开通的服务.Remove(swhy);
                            服务列表.可申请的服务.Add(swhy);
                        }
                        if (bzhy != null)
                        {
                            服务列表.已开通的服务.Remove(bzhy);
                            服务列表.可申请的服务.Add(bzhy);
                        }
                    }
                    if (gh.所申请项目名.Contains("标准会员"))
                    {
                        //开通会员赠送相关服务 ,如果之前开通了这些服务，比较期结束时间，保留结束时间大的服务
                        if (!服务列表.已开通的服务.Exists(o => o.所申请项目名 == "企业主页商品展示A类窗口" || o.所申请项目名 == "企业主页商品展示B类窗口" || o.所申请项目名 == "企业主页商品展示C类窗口" || o.所申请项目名 == "企业主页商品展示D类窗口" || o.所申请项目名 == "企业主页商品展示E类窗口"))
                        {
                            if (!服务列表.已开通的服务.Exists(o => o.所申请项目名 == "企业主页商品展示F类窗口"))
                            {
                                //如果已开通了低级模板如F G类则清除
                                var 已有的低级模板 = 服务列表.已开通的服务.Where(o => o.所申请项目名.Contains("企业主页商品展示"));
                                if (已有的低级模板.Count() > 0)
                                {
                                    var 低级模板 = 已有的低级模板.First();
                                    服务列表.已开通的服务.Remove(低级模板);
                                }
                                var 赠送服务1 = new 供应商增值服务申请记录();
                                赠送服务1.所申请项目名 = "企业主页商品展示F类窗口";
                                赠送服务1.所属供应商.用户ID = long.Parse(id);
                                赠送服务1.是否通过 = 通过状态.通过;
                                赠送服务1.签订时间 = DateTime.Now;
                                赠送服务1.服务期限 = 6;
                                赠送服务1.结束时间 = DateTime.Now.AddMonths(赠送服务1.服务期限);
                                服务列表.已开通的服务.Add(赠送服务1);
                                服务列表.可申请的服务.Remove(服务列表.可申请的服务.Find(o => o.所申请项目名 == "企业主页商品展示F类窗口"));
                            }
                            else
                            {
                                var 已有的赠送服务 = 服务列表.已开通的服务.Find(o => o.所申请项目名 == "企业主页商品展示F类窗口");
                                if (已有的赠送服务.结束时间 < DateTime.Now.AddMonths(6))
                                {
                                    已有的赠送服务.服务期限 = 6;
                                    已有的赠送服务.结束时间 = DateTime.Now.AddMonths(6);
                                }
                            }

                        }
                        if (!服务列表.已开通的服务.Exists(o => o.所申请项目名 == "手机短信招标采购类")) //以前未开通则添加
                        {
                            var 赠送服务2 = new 供应商增值服务申请记录();
                            赠送服务2.所申请项目名 = "手机短信招标采购类";
                            赠送服务2.所属供应商.用户ID = long.Parse(id);
                            赠送服务2.是否通过 = 通过状态.通过;
                            赠送服务2.签订时间 = DateTime.Now;
                            赠送服务2.服务期限 = 6;
                            赠送服务2.结束时间 = DateTime.Now.AddMonths(赠送服务2.服务期限);
                            服务列表.已开通的服务.Add(赠送服务2);
                            服务列表.可申请的服务.Remove(服务列表.可申请的服务.Find(o => o.所申请项目名 == "手机短信招标采购类"));
                        }
                        else   //以前开通过，比较结束时间，保留较晚的
                        {
                            var 已有的赠送服务 = 服务列表.已开通的服务.Find(o => o.所申请项目名 == "手机短信招标采购类");
                            if (已有的赠送服务.结束时间 < DateTime.Now.AddMonths(6))
                            {
                                已有的赠送服务.服务期限 = 6;
                                已有的赠送服务.结束时间 = DateTime.Now.AddMonths(6);
                            }
                        }

                        var swhy = 服务列表.已开通的服务.Find(o => o.所申请项目名.Contains("商务会员"));
                        var jchy = 服务列表.已开通的服务.Find(o => o.所申请项目名.Contains("基础会员"));
                        if (swhy != null)
                        {
                            服务列表.已开通的服务.Remove(swhy);
                            服务列表.可申请的服务.Add(swhy);
                        }
                        if (jchy != null)
                        {
                            服务列表.已开通的服务.Remove(jchy);
                            服务列表.可申请的服务.Add(jchy);
                        }
                    }
                    if (gh.所申请项目名.Contains("商务会员"))
                    {
                        //开通会员赠送相关服务 ,如果之前开通了这些服务，比较期结束时间，保留结束时间大的服务

                        //如果已开通高级模板如A类 B类，则保留高级模板
                        if (!服务列表.已开通的服务.Exists(o => o.所申请项目名 == "企业主页商品展示A类窗口" || o.所申请项目名 == "企业主页商品展示B类窗口" || o.所申请项目名 == "企业主页商品展示C类窗口" || o.所申请项目名 == "企业主页商品展示D类窗口"))
                        {
                            if (!服务列表.已开通的服务.Exists(o => o.所申请项目名 == "企业主页商品展示E类窗口"))
                            {
                                //如果已开通了低级模板如F G类则清除
                                var 已有的低级模板 = 服务列表.已开通的服务.Where(o => o.所申请项目名.Contains("企业主页商品展示"));
                                if (已有的低级模板.Count() < 0)
                                {
                                    var 低级模板 = 已有的低级模板.First();
                                    服务列表.已开通的服务.Remove(低级模板);
                                }
                                var 赠送服务1 = new 供应商增值服务申请记录();
                                赠送服务1.所申请项目名 = "企业主页商品展示E类窗口";
                                赠送服务1.所属供应商.用户ID = long.Parse(id);
                                赠送服务1.是否通过 = 通过状态.通过;
                                赠送服务1.签订时间 = DateTime.Now;
                                赠送服务1.服务期限 = 6;
                                赠送服务1.结束时间 = DateTime.Now.AddMonths(赠送服务1.服务期限);
                                服务列表.已开通的服务.Add(赠送服务1);
                                服务列表.可申请的服务.Remove(服务列表.可申请的服务.Find(o => o.所申请项目名 == "企业主页商品展示E类窗口"));
                            }
                            else
                            {
                                var 已有的赠送服务 = 服务列表.已开通的服务.Find(o => o.所申请项目名 == "企业主页商品展示E类窗口");
                                if (已有的赠送服务.结束时间 < DateTime.Now.AddMonths(6))
                                {
                                    已有的赠送服务.服务期限 = 6;
                                    已有的赠送服务.结束时间 = DateTime.Now.AddMonths(6);
                                }
                            }
                        }
                        if (!服务列表.已开通的服务.Exists(o => o.所申请项目名 == "手机短信动态与提醒")) //以前未开通则添加
                        {
                            var 赠送服务2 = new 供应商增值服务申请记录();
                            赠送服务2.所申请项目名 = "手机短信动态与提醒";
                            赠送服务2.所属供应商.用户ID = long.Parse(id);
                            赠送服务2.是否通过 = 通过状态.通过;
                            赠送服务2.签订时间 = DateTime.Now;
                            赠送服务2.服务期限 = 6;
                            赠送服务2.结束时间 = DateTime.Now.AddMonths(赠送服务2.服务期限);
                            服务列表.已开通的服务.Add(赠送服务2);
                            服务列表.可申请的服务.Remove(服务列表.可申请的服务.Find(o => o.所申请项目名 == "手机短信动态与提醒"));
                        }
                        else   //以前开通过，比较结束时间，保留较晚的
                        {
                            var 已有的赠送服务 = 服务列表.已开通的服务.Find(o => o.所申请项目名 == "手机短信动态与提醒");
                            if (已有的赠送服务.结束时间 < DateTime.Now.AddMonths(6))
                            {
                                已有的赠送服务.服务期限 = 6;
                                已有的赠送服务.结束时间 = DateTime.Now.AddMonths(6);
                            }
                        }
                        if (!服务列表.已开通的服务.Exists(o => o.所申请项目名 == "手机短信业务对接")) //以前未开通则添加
                        {
                            var 赠送服务2 = new 供应商增值服务申请记录();
                            赠送服务2.所申请项目名 = "手机短信业务对接";
                            赠送服务2.所属供应商.用户ID = long.Parse(id);
                            赠送服务2.是否通过 = 通过状态.通过;
                            赠送服务2.签订时间 = DateTime.Now;
                            赠送服务2.服务期限 = 6;
                            赠送服务2.结束时间 = DateTime.Now.AddMonths(赠送服务2.服务期限);
                            服务列表.已开通的服务.Add(赠送服务2);
                            服务列表.可申请的服务.Remove(服务列表.可申请的服务.Find(o => o.所申请项目名 == "手机短信业务对接"));
                        }
                        else   //以前开通过，比较结束时间，保留较晚的
                        {
                            var 已有的赠送服务 = 服务列表.已开通的服务.Find(o => o.所申请项目名 == "手机短信业务对接");
                            if (已有的赠送服务.结束时间 < DateTime.Now.AddMonths(6))
                            {
                                已有的赠送服务.服务期限 = 6;
                                已有的赠送服务.结束时间 = DateTime.Now.AddMonths(6);
                            }
                        }
                        var bzhy = 服务列表.已开通的服务.Find(o => o.所申请项目名.Contains("标准会员"));
                        var jchy = 服务列表.已开通的服务.Find(o => o.所申请项目名.Contains("基础会员"));
                        if (bzhy != null)
                        {
                            服务列表.已开通的服务.Remove(bzhy);
                            服务列表.可申请的服务.Add(bzhy);
                        }
                        if (jchy != null)
                        {
                            服务列表.已开通的服务.Remove(jchy);
                            服务列表.可申请的服务.Add(jchy);
                        }
                    }

                    服务列表.申请中的服务.Remove(gh);
                    服务列表.已开通的服务.Add(gh);
                    供应商增值服务申请记录管理.更新供应商增值服务申请记录(申请记录);
                    供应商服务记录管理.更新供应商服务记录(服务列表);
                    充值余额.可用余额 -= 待扣费金额;
                    供应商计费项目扣费记录管理.添加供应商计费项目扣费记录(扣费);
                    供应商充值余额管理.更新供应商充值余额(充值余额);
                    用户管理.更新用户<供应商>(gys);
                    deleteIndex("/Lucene.Net/IndexDic/Gys", id);
                    if (gys.审核数据.审核状态 == 审核状态.审核通过)
                    {
                        CreateIndex_gys(gys, "/Lucene.Net/IndexDic/Gys");
                    }

                    return "开通成功！";
                }
            }
            if (int.Parse(type_operate) == 3)
            {
                gh.是否通过 = 通过状态.未通过;
                gh.开通个数 = 0;

                gh.未通过原因 = "客服撤销";
                gh.服务期限 = 0;
                服务列表.申请中的服务.Remove(gh);
                服务列表.可申请的服务.Add(gh);
                供应商服务记录管理.更新供应商服务记录(服务列表);
                return "撤销成功！";
            }
            return "操作失败！";
        }


        public class MessageEdit
        {
            public string Msg { get; set; }
            public string ExpireTime { get; set; }
        }
        public string EditApplyService()
        {
            var gysid = Request.Params["gysid"];      //供应商ID
            var servicename = Request.Params["servicename"];  //服务名
            var signtime = Request.Params["signtime"];       //签订时间
            var times = Request.Params["times"];//开通时长
            var msg = new MessageEdit();
            var _gys_service = 供应商服务记录管理.查询供应商服务记录(0, 0, Query<供应商服务记录>.Where(o => o.所属供应商.用户ID == long.Parse(gysid)));
            if (_gys_service.Any() && _gys_service.Count() > 0)
            {
                var _first = _gys_service.First();
                var _service = _first.已开通的服务.Find(o => o.所申请项目名 == servicename);
                var _feerule = 扣费规则.规则列表.Where(o => o.扣费项目名 == servicename).First();
                _service.签订时间 = DateTime.Parse(signtime);
                _service.服务期限 = int.Parse(times);

                var _feefirst = new 供应商计费项目扣费记录();
                var _fee = 供应商计费项目扣费记录管理.查询供应商计费项目扣费记录(0, 0, Query<供应商计费项目扣费记录>.Where(o => o.Id == _service.Id));
                if (_fee.Any() && _fee.Count() > 0)
                {
                    _feefirst = _fee.First();
                    _feefirst.扣费时间 = DateTime.Parse(signtime);
                }

                if (_feerule.扣费类型 == 扣费类型.按年扣费)
                {
                    _service.结束时间 = _service.签订时间.AddYears(_service.服务期限);
                    _feefirst.所属年 = _service.服务期限;
                }
                if (_feerule.扣费类型 == 扣费类型.按月扣费)
                {
                    _service.结束时间 = _service.签订时间.AddMonths(_service.服务期限);
                    _feefirst.所属月 = _service.服务期限;
                }
                if (_feerule.扣费类型 == 扣费类型.按次扣费)
                {
                    _service.结束时间 = _service.签订时间.AddDays(_service.服务期限);
                    _feefirst.所属日 = _service.服务期限;
                }

                var _applyfirst = new 供应商增值服务申请记录();
                var _apply = 供应商增值服务申请记录管理.查询供应商增值服务申请记录(0, 0, Query<供应商增值服务申请记录>.Where(o => o.Id == _service.Id));
                if (_apply.Any() && _apply.Count() > 0)
                {
                    _applyfirst = _apply.First();
                    _applyfirst.签订时间 = DateTime.Parse(signtime);
                    _applyfirst.服务期限 = int.Parse(times);
                    _applyfirst.结束时间 = _service.结束时间;
                }

                供应商增值服务申请记录管理.更新供应商增值服务申请记录(_applyfirst);
                供应商计费项目扣费记录管理.更新供应商计费项目扣费记录(_feefirst);
                供应商服务记录管理.更新供应商服务记录(_first);

                msg.Msg = "修改成功！";
                msg.ExpireTime = _service.结束时间.ToString("yyyy/MM/dd HH:mm:ss");
                return JsonConvert.SerializeObject(msg);
            }
            else
            {
                msg.Msg = "未找到该条信息！";
                return JsonConvert.SerializeObject(msg);
            }
        }

        //清除所有过期服务
        public void RemoveAllExpired()
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

        //清除单个过期服务
        public void RemoveOneExpired()
        {
            var gysid = Request.Params["id"];//供应商ID
            var _servicename = Request.Params["service"];//服务名

            var 服务列表 = 供应商服务记录管理.查询供应商服务记录(0, 0, Query<供应商服务记录>.Where(o => o.所属供应商.用户ID == long.Parse(gysid)));
            if (服务列表.Count() > 0)
            {
                var 服务记录 = 服务列表.First();
                var k = 服务记录.已开通的服务.Find(o => o.所申请项目名 == _servicename);
                服务记录.已开通的服务.Remove(k);
                var n = new 供应商增值服务申请记录();
                n.所属供应商.用户ID = long.Parse(gysid);
                n.所申请项目名 = k.所申请项目名;
                服务记录.可申请的服务.Add(n);
                供应商服务记录管理.更新供应商服务记录(服务记录);
            }
        }
        [HttpPost]
        public int ApplyAddValue()
        {
            var id = Request.Params["gysid"];//供应商ID
            var kd = Request.Params["str"];//待开通服务及其时长
            var arr = kd.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            var kf = new Dictionary<供应商增值服务申请记录, decimal>();
            var 余额 = 供应商充值余额管理.查询供应商充值余额(0, 0, Query<供应商充值余额>.Where(o => o.所属供应商.用户ID == long.Parse(id))).First();
            foreach (var jk in arr)
            {
                var 开通服务名 = jk.Split(':')[0];
                var 开通时长 = "";
                var 开通个数 = "";
                var account = 扣费规则.规则列表.Where(o => o.扣费项目名 == 开通服务名).First();
                var g = new 供应商增值服务申请记录();
                g.所申请项目名 = 开通服务名;
                g.所属供应商.用户ID = long.Parse(id);
                if (开通服务名.Contains("添加商品一级分类") || 开通服务名.Contains("添加商品二级分类"))
                {
                    开通时长 = jk.Split(':')[1].Split(' ')[0];
                    开通个数 = jk.Split(':')[1].Split(' ')[1];
                    g.开通个数 = int.Parse(开通个数);
                }
                else
                {
                    开通时长 = jk.Split(':')[1];
                    开通个数 = jk.Split(':')[1];
                    g.开通个数 = 1;
                }
                g.服务期限 = int.Parse(开通时长);

                kf.Add(g, account.扣费金额);
            }
            var 待扣费金额 = kf.Sum(o => o.Key.服务期限 * o.Value * o.Key.开通个数);
            if (余额.可用余额 < 待扣费金额)
            {
                return 0;//申请失败
            }
            else
            {
                var 服务 = new 供应商服务记录();
                var m = 供应商服务记录管理.查询供应商服务记录(0, 0, Query<供应商服务记录>.Where(o => o.所属供应商.用户ID == long.Parse(id)));
                if (m.Count() > 0)
                {
                    服务 = m.First();
                    foreach (var mm in kf)
                    {
                        if (mm.Key.所申请项目名.Contains("企业主页商品展示"))
                        {
                            var qyzsck = 服务.申请中的服务.Find(o => o.所申请项目名.Contains("企业主页商品展示"));
                            if (qyzsck != null)
                            {
                                服务.申请中的服务.Remove(qyzsck);
                                服务.可申请的服务.Add(qyzsck);
                            }
                        }
                        if (mm.Key.所申请项目名.Contains("企业推广服务B1"))
                        {
                            var qytgfw = 服务.申请中的服务.Find(o => o.所申请项目名.Contains("企业推广服务B1"));
                            if (qytgfw != null)
                            {
                                服务.申请中的服务.Remove(qytgfw);
                                服务.可申请的服务.Add(qytgfw);
                            }
                        }
                        if (mm.Key.所申请项目名.Contains("基础会员"))
                        {
                            var swhy = 服务.申请中的服务.Find(o => o.所申请项目名.Contains("商务会员"));
                            var bzhy = 服务.申请中的服务.Find(o => o.所申请项目名.Contains("标准会员"));
                            if (swhy != null)
                            {
                                服务.申请中的服务.Remove(swhy);
                                服务.可申请的服务.Add(swhy);
                            }
                            if (bzhy != null)
                            {
                                服务.申请中的服务.Remove(bzhy);
                                服务.可申请的服务.Add(bzhy);
                            }
                        }
                        if (mm.Key.所申请项目名.Contains("标准会员"))
                        {
                            var swhy = 服务.申请中的服务.Find(o => o.所申请项目名.Contains("商务会员"));
                            var jchy = 服务.申请中的服务.Find(o => o.所申请项目名.Contains("基础会员"));
                            if (swhy != null)
                            {
                                服务.申请中的服务.Remove(swhy);
                                服务.可申请的服务.Add(swhy);
                            }
                            if (jchy != null)
                            {
                                服务.申请中的服务.Remove(jchy);
                                服务.可申请的服务.Add(jchy);
                            }
                        }
                        if (mm.Key.所申请项目名.Contains("商务会员"))
                        {
                            var bzhy = 服务.申请中的服务.Find(o => o.所申请项目名.Contains("标准会员"));
                            var jchy = 服务.申请中的服务.Find(o => o.所申请项目名.Contains("基础会员"));
                            if (bzhy != null)
                            {
                                服务.申请中的服务.Remove(bzhy);
                                服务.可申请的服务.Add(bzhy);
                            }
                            if (jchy != null)
                            {
                                服务.申请中的服务.Remove(jchy);
                                服务.可申请的服务.Add(jchy);
                            }
                        }
                        服务.申请中的服务.Add(mm.Key);
                        var jk = 服务.可申请的服务.Find(o => o.所申请项目名 == mm.Key.所申请项目名);
                        服务.可申请的服务.Remove(jk);
                        供应商增值服务申请记录管理.添加供应商增值服务申请记录(mm.Key);
                        供应商服务记录管理.更新供应商服务记录(服务);
                    }
                    return 1;
                }
                else
                {
                    var gh = new 供应商服务记录();
                    foreach (var yy in kf)
                    {
                        gh.申请中的服务.Add(yy.Key);
                        供应商服务记录管理.添加供应商服务记录(gh);
                        供应商增值服务申请记录管理.添加供应商增值服务申请记录(yy.Key);
                    }
                    return 1;
                }
            }
        }
        public ActionResult TicketList()
        {
            try
            {
                int currentpage = 0;
                if (Request.QueryString["page"] != null)
                {
                    currentpage = int.Parse(Request.QueryString["page"]);
                }
                else
                {
                    currentpage = 1;
                }
                long sum = 机票验收单管理.计数机票验收单(0, 0, Query<机票验收单>.Where(m => !m.是否作废));
                long pagecount = sum / 20;
                if (sum % 20 > 0)
                {
                    pagecount++;
                }
                ViewData["Pagecount"] = pagecount;
                ViewData["CurrentPage"] = currentpage;
                IEnumerable<机票验收单> model = 机票验收单管理.查询机票验收单(20 * (currentpage - 1), 20, Query<机票验收单>.Where(m => !m.是否作废));
                return View(model);
            }
            catch
            {
                return Redirect("/运营团队后台/");
            }

        }
        public ActionResult Uploadyzd()
        {
            try
            {
                long id = long.Parse(Request.QueryString["id"].ToString());
                机票验收单 model = 机票验收单管理.查找机票验收单(id);
                if (model == null)
                {
                    return Redirect("/运营团队后台/");
                }
                else
                {
                    ViewData["id"] = id;
                    return View(model);
                }
            }
            catch
            {
                return Redirect("/运营团队后台/");
            }
        }
        [HttpPost]
        public ActionResult AddTicket()
        {
            try
            {
                string reason = Request.Form["reason"];
                long id = long.Parse(Request.Form["id"].ToString());
                string url = Request.Form["picture"].ToString().Split('|')[0];
                int index = int.Parse(Request.Form["index"].ToString().Split(',')[0]);
                机票验收单 model = 机票验收单管理.查找机票验收单(id);
                if (!string.IsNullOrWhiteSpace(model.服务列表[index].验证附件路径))
                {
                    System.IO.File.Delete(Server.MapPath(@model.服务列表[index].验证附件路径));
                }
                model.服务列表[index].验证附件路径 = url;
                switch (reason)
                {
                    case "已验证": model.服务列表[index].验证状态 = 机票验证状态.已验证; break;
                    case "待验证": model.服务列表[index].验证状态 = 机票验证状态.待验证; break;
                    case "查无此票": model.服务列表[index].验证状态 = 机票验证状态.查无此票; break;
                    case "价格不匹配": model.服务列表[index].验证状态 = 机票验证状态.价格不匹配; break;
                    case "行程不匹配": model.服务列表[index].验证状态 = 机票验证状态.行程不匹配; break;
                }
                机票验收单管理.更新机票验收单(model);
                return Content("<script>alert('成功验证！');</script>");
            }
            catch
            {
                return Content("<script>alert('验证失败！');</script>");
            }
        }
        public string SetCheck()
        {
            try
            {
                string reason = Request.QueryString["r"];
                long id = long.Parse(Request.QueryString["id"].ToString());
                string name = Request.QueryString["n"];
                int index = int.Parse(Request.QueryString["index"].ToString().Split(',')[0]);
                机票验收单 model = 机票验收单管理.查找机票验收单(id);
                switch (name)
                {
                    case "待验证": model.服务列表[index].验证状态 = 机票验证状态.待验证; break;
                    case "查无此票": model.服务列表[index].验证状态 = 机票验证状态.查无此票; break;
                    case "价格不匹配": model.服务列表[index].验证状态 = 机票验证状态.价格不匹配; break;
                    case "行程不匹配": model.服务列表[index].验证状态 = 机票验证状态.行程不匹配; break;
                }
                机票验收单管理.更新机票验收单(model);
                return "1";
            }
            catch
            {
                return "0";
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
                        //生成html文档及文件夹
                        var htmlfile = getHtmlString(model, time);
                        if (!string.IsNullOrWhiteSpace(htmlfile))
                        {
                            zip.AddFile(htmlfile, model.内容主体.标题);
                        }

                        //生成数据的JS文件
                        var JSfile = getCollJsFile(model, time);
                        if (!string.IsNullOrWhiteSpace(JSfile))
                        {
                            zip.AddFile(JSfile, model.内容主体.标题);
                        }

                        //打包正文里面的图片
                        var jp = new JumonyParser();
                        var ht = model.内容主体.内容;
                        var doc = jp.Parse(ht);
                        var imgPaths = doc.Find("img[src]").Select(img => img.Attribute("src").AttributeValue);
                        if (imgPaths.Any())
                        {
                            foreach (var att in imgPaths)
                            {
                                zip.AddFile(Server.MapPath(att), model.内容主体.标题 + Path.GetDirectoryName(att));
                            }
                        }

                        //打包附件
                        if (model.内容主体.附件 != null && model.内容主体.附件.Any())
                        {
                            foreach (var att in model.内容主体.附件)
                            {
                                zip.AddFile(Server.MapPath(att), model.内容主体.标题 + Path.GetDirectoryName(att));
                                //zip.AddFile(Server.MapPath(att), model.内容主体.标题 + "\\附件\\" + Path.GetDirectoryName(att));
                            }
                            //zip.AddFiles(model.内容主体.附件.Select(Server.MapPath), model.内容主体.标题 + "\\附件");
                        }

                        //打包扫描件
                        if (model.内容主体.图片 != null && model.内容主体.图片.Any())
                        {
                            foreach (var att in model.内容主体.图片)
                            {
                                zip.AddFile(Server.MapPath(att), model.内容主体.标题 + Path.GetDirectoryName(att));
                                //zip.AddFile(Server.MapPath(att), model.内容主体.标题 + "\\扫描件\\" + Path.GetDirectoryName(att));
                            }
                            //zip.AddFiles(model.内容主体.图片.Select(Server.MapPath), model.内容主体.标题 + "\\扫描件");
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
        //生成某行数据的JS 文件
        public string getCollJsFile(公告 model, string time)
        {
            try
            {
                var input = model.ToBsonDocument();
                var dir = Server.MapPath("/App_Uploads/公告下载/" + time + "/" + Guid.NewGuid() + "/");
                var file = dir + "document.js";
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
                sw.WriteLine(input);
                sw.Close();
                return file;
            }
            catch
            {
                return "";
            }
        }
        public ActionResult Procure_AdUpload()
        {
            return View();
        }
        public ActionResult Part_Procure_AdUpload()
        {
            return PartialView("Part_View/Part_Procure_AdUpload");
        }
        [HttpPost]
        public ActionResult UploadAdMessage()
        {
            HttpPostedFileBase file = Request.Files[0];
            if (file != null)
            {
                //string fname = UploadAttachment(file);
                string fileName = file.FileName;
                if (!string.IsNullOrWhiteSpace(fileName))
                {
                    var time = DateTime.Now.ToLocalTime().ToString("yyyyMMdd-hhmmss");
                    var filePath = "D:\\TempAd\\" + time + "\\";
                    //string filePath1 = 上传管理.获取上传路径<商品>(媒体类型.文档, 路径类型.服务器本地路径);
                    if (fileName.LastIndexOf("\\") > -1)
                    {
                        fileName = fileName.Substring(fileName.LastIndexOf("\\") + 1);
                    }
                    fileName = 上传管理.获取文件名(fileName).Replace("{", "").Replace("}", "");
                    if (!Directory.Exists(filePath))
                    {
                        Directory.CreateDirectory(filePath);
                    }
                    var filedir = string.Format("{0}{1}", filePath, fileName);
                    file.SaveAs(filedir);
                    if (!ZipFile.IsZipFile(filePath + fileName))
                    {
                        System.IO.File.Delete(filePath + fileName);
                        return Content("<span style='color:red'>请上传格式为zip的压缩包</span><script>window.parent.document.getElementById('waitfor').style.display='none';</script>");
                    }
                    var zip = ZipFile.Read(filedir, new ReadOptions { Encoding = Encoding.Default });
                    //判断有多少条公告
                    var dirlist = zip.Entries.Where(zf => zf.FileName.EndsWith("document.js")).Select(o => o.FileName.Replace("document.js", ""));
                    var guid = Guid.NewGuid();
                    foreach (var dir in dirlist)
                    {
                        var ziptemp = zip.Entries.Where(zf =>
                                !zf.IsDirectory
                                && zf.FileName.StartsWith(dir)
                                );
                        foreach (var zipt in ziptemp)
                        {
                            zipt.Extract(@"D:\TempAd\" + time + "\\");
                            var path = zipt.FileName.Replace(dir, "");
                            //JS文件，则转换为Json插入数据库
                            if (path == "document.js")
                            {
                                var sr = new StreamReader(@"D:\TempAd\" + time + "\\" + dir.Substring(0, dir.Length - 1) + "\\document.js", Encoding.UTF8);
                                var strLine = sr.ReadLine();
                                if (null != strLine)
                                {
                                    var gg = BsonSerializer.Deserialize<公告>(strLine);
                                    gg.Id = -1;
                                    公告管理.添加公告(gg);
                                    if (gg.审核数据.审核状态 == 审核状态.审核通过)
                                    {
                                        GG_CreateIndex(gg, "/Lucene.Net/IndexDic/GongGao");
                                    }
                                }
                            }
                            else if (path != dir.Substring(0, dir.Length - 1) + ".html")
                            {
                                //附件或图片，则转移到相应的文件夹
                                var serverpath = Server.MapPath("/" + path);
                                var serverdir = System.IO.Path.GetDirectoryName(serverpath);
                                if (!Directory.Exists(serverdir))
                                {
                                    Directory.CreateDirectory(serverdir);
                                }
                                if (System.IO.File.Exists(serverpath)) System.IO.File.Delete(serverpath);
                                System.IO.File.Move(@"D:\TempAd\" + time + "\\" + zipt.FileName, serverpath);
                            }
                        }
                    }
                    //System.IO.File.Delete(filePath + fileName);
                    return Content("<span style='color:#31BD3C'>上传成功!</span><script>window.parent.document.getElementById('waitfor').style.display='none';</script>");
                }
            }
            return Content("<span style='color:red'>未选择文件</span><script>window.parent.document.getElementById('waitfor').style.display='none';</script>");
        }

        [HttpPost]
        public ActionResult UploadImages1()
        {
            try
            {
                string path = "";
                HttpPostedFileBase fileData = null;
                for (int i = 0; i < Request.Files.Count; i++)
                {
                    fileData = Request.Files[i];
                    if (fileData != null)
                    {
                        string filePath = "";
                        string filePath1 = "";
                        filePath = 上传管理.获取上传路径<机票代售点>(媒体类型.图片, 路径类型.服务器本地路径);
                        filePath1 = 上传管理.获取上传路径<机票代售点>(媒体类型.图片, 路径类型.不带域名根路径);
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
                            //下面这句代码缺少的话，上传成功后上传队列的显示不会自动消失
                            path += filePath1 + "original/" + fileName + "|";
                            //缩略图550(限宽)
                            //UploadPic.MakeThumbnail(string.Format("{0}original\\{1}", filePath, fileName), string.Format("{0}550\\{1}", filePath, fileName), 550, 0, "W");
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
        [HttpPost]
        public string GysRecharge()
        {
            var amount = Request.Params["amount"];//待充值金额
            var id = Request.Params["gysid"];//供应商ID
            var 充值记录 = new 供应商充值记录();
            充值记录.充值方式 = 充值方式.后台直充;
            充值记录.充值金额 = decimal.Parse(amount);
            充值记录.充值时间 = DateTime.Now;
            //充值记录.供应商转款账号 = Request.QueryString["buyer_email"];
            //充值记录.收款账号 = Request.QueryString["seller_email"];
            充值记录.所属供应商.用户ID = long.Parse(id);


            var 余额记录 = 供应商充值余额管理.查询供应商充值余额(0, 0, Query<供应商充值余额>.Where(o => o.所属供应商.用户ID == long.Parse(id)));
            if (余额记录.Count() > 0)
            {
                var model = 余额记录.First();
                model.可用余额 += 充值记录.充值金额;
                供应商充值余额管理.更新供应商充值余额(model);
                供应商充值记录管理.添加供应商充值记录(充值记录);
                return "充值成功！";
            }
            else
            {
                var 充值余额 = new 供应商充值余额();
                充值余额.可用余额 = 充值记录.充值金额;
                充值余额.所属供应商.用户ID = long.Parse(id);
                供应商充值余额管理.添加供应商充值余额(充值余额);
                供应商充值记录管理.添加供应商充值记录(充值记录);
                return "充值成功！";
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

        /// <summary>
        /// 计算日期间隔
        /// </summary>
        /// <param name="d1">要参与计算的其中一个日期</param>
        /// <param name="d2">要参与计算的另一个日期</param>
        /// <param name="drf">决定返回值形式的枚举</param>
        /// <returns>一个代表年月日的int数组，具体数组长度与枚举参数drf有关</returns>
        public static int toResult(DateTime d1, DateTime d2, diffResultFormat drf)
        {
            #region 数据初始化
            DateTime max;
            DateTime min;
            int year;
            int month;
            int tempYear, tempMonth;
            if (d1 > d2)
            {
                max = d1;
                min = d2;
            }
            else
            {
                max = d2;
                min = d1;
            }
            tempYear = max.Year;
            tempMonth = max.Month;
            if (max.Month < min.Month)
            {
                tempYear--;
                tempMonth = tempMonth + 12;
            }
            year = tempYear - min.Year;
            month = tempMonth - min.Month;
            #endregion
            #region 按条件计算
            if (drf == diffResultFormat.dd)
            {
                TimeSpan ts = max - min;
                return ts.Days;
            }
            if (drf == diffResultFormat.mm)
            {
                return month + year * 12;
            }
            if (drf == diffResultFormat.yy)
            {
                return year;
            }
            return year;
            #endregion
        }
        /// <summary>
        /// 关于返回值形式的枚举
        /// </summary>
        public enum diffResultFormat
        {
            /// <summary>
            /// 年数和月数
            /// </summary>
            yymm,
            /// <summary>
            /// 年数
            /// </summary>
            yy,
            /// <summary>
            /// 月数
            /// </summary>
            mm,
            /// <summary>
            /// 天数
            /// </summary>
            dd,
        }

        public ActionResult Acceptanced_List()
        {
            //var q = Query.EQ("审核数据.审核状态", 审核状态.审核通过).And(Query.EQ("是否已经打印", false));
            //var q = Query<验收单>.Where(o => o.审核数据.审核状态 == 审核状态.审核通过);
            long listcount = 验收单管理.计数验收单(0, 0);
            long maxpagesize = Math.Max((listcount + ACCEPTANCE_PAGESIZE - 1) / ACCEPTANCE_PAGESIZE, 1);

            int page = 1;
            ViewData["currentpage"] = page;
            ViewData["pagecount"] = maxpagesize;

            ViewData["验收单列表"] = 验收单管理.查询验收单(ACCEPTANCE_PAGESIZE * (page - 1), ACCEPTANCE_PAGESIZE, null, false, SortBy.Descending("基本数据.添加时间"));
            return View();
        }
        public ActionResult AcceptancedDetail()
        {
            try
            {
                string id = Request.Params["id"];
                验收单 ysd = 验收单管理.查找验收单(long.Parse(id));
                if (ysd == null)
                {
                    return Redirect("/运营团队后台/AcceptanceList");
                }
                else
                {
                    return View(ysd);
                }
            }
            catch
            {
                return Redirect("/运营团队后台/AcceptanceList");
            }
        }
        public void ChangePrint()
        {
            var id = Request.Params["id"];
            var ysd = 验收单管理.查找验收单(long.Parse(id));
            if (ysd.是否已经打印)
            {
                ysd.是否已经打印 = false;
            }
            else
            {
                ysd.是否已经打印 = true;
                ysd.打印信息.Add(new _打印信息() { 打印时间 = DateTime.Now });
            }
            验收单管理.更新验收单(ysd);
        }

        public ActionResult AccepatnceSmj()
        {
            var id = Request.Params["id"];//验收单ID
            var ysd = 验收单管理.查找验收单(long.Parse(id));
            return View(ysd);
        }

        public ActionResult CancelExamine()
        {
            var id = Request.Params["id"];
            var ysd = 验收单管理.查找验收单(long.Parse(id));
            ysd.审核数据.审核状态 = 审核状态.未审核;
            ysd.审核数据.审核者.用户ID = -1;
            验收单管理.更新验收单(ysd);
            return RedirectToAction("Acceptanced_List");
        }

        public ActionResult OnlineBidding_Add()
        {
            return View();
        }
        public ActionResult OnlineBidding_Detail()
        {
            return View();
        }
        public ActionResult OnlineBidding_List()
        {
            return View();
        }
        public ActionResult OnlineBidding_List_Ed()
        {
            return View();
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
                        return Content("<script>alert('修改时发生错误，请重试');location.href='javascript:history.go(-1)';</script>");
                    }
                }
                else
                {
                    return Content("<script>window.location='/运营团队后台/OnlineBidding_List';</script>");
                }
            }
            catch
            {
                g = null;
            }
            ViewData["first"] = 商品分类管理.查找子分类();
            return PartialView("Part_View/Part_OnlineBidding_Modify", g);
        }
        public ActionResult Part_OnlineBidding_List_Ed(int? page)
        {
            int pro_listcount = (int)(网上竞标管理.计数网上竞标(0, 0, Query<网上竞标>.Where(o => o.报价结束时间 < DateTime.Now)));
            int pro_maxpage = Math.Max((pro_listcount + ZBCGXM_PAGESIZE - 1) / ZBCGXM_PAGESIZE, 1);

            if (string.IsNullOrEmpty(page.ToString()) || page < 0 || page > pro_maxpage)
            {
                page = 1;
            }

            ViewData["listcount"] = pro_listcount;
            ViewData["page"] = page;
            ViewData["pagesize"] = ZBCGXM_PAGESIZE;

            ViewData["网上竞标列表"] = 网上竞标管理.查询网上竞标(ZBCGXM_PAGESIZE * (int.Parse(page.ToString()) - 1), ZBCGXM_PAGESIZE, Query<网上竞标>.Where(o => o.报价结束时间 < DateTime.Now));
            return PartialView("Part_View/Part_OnlineBidding_List_Ed");
        }
        public ActionResult Part_OnlineBidding_List(int? page)
        {
            int pro_listcount = (int)(网上竞标管理.计数网上竞标(0, 0, Query<网上竞标>.Where(o => o.报价结束时间 > DateTime.Now)));
            int pro_maxpage = Math.Max((pro_listcount + ZBCGXM_PAGESIZE - 1) / ZBCGXM_PAGESIZE, 1);

            if (string.IsNullOrEmpty(page.ToString()) || page < 0 || page > pro_maxpage)
            {
                page = 1;
            }

            ViewData["listcount"] = pro_listcount;
            ViewData["page"] = page;
            ViewData["pagesize"] = ZBCGXM_PAGESIZE;

            ViewData["网上竞标列表"] = 网上竞标管理.查询网上竞标(ZBCGXM_PAGESIZE * (int.Parse(page.ToString()) - 1), ZBCGXM_PAGESIZE, Query<网上竞标>.Where(o => o.报价结束时间 > DateTime.Now));
            return PartialView("Part_View/Part_OnlineBidding_List");
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
            return PartialView("Part_View/Part_OnlineBidding_Detail", g);
        }
        public ActionResult Part_OnlineBidding_Add()
        {
            ViewData["first"] = 商品分类管理.查找子分类();
            ViewData["招标采购项目列表"] = 招标采购项目管理.查询招标采购项目(0, 0, MongoDB.Driver.Builders.Query.EQ("审核数据.审核状态", 审核状态.审核通过).And(MongoDB.Driver.Builders.Query.EQ("中标公告链接.公告ID", -1)).And(MongoDB.Driver.Builders.Query.EQ("流标公告链接.公告ID", -1)).And(MongoDB.Driver.Builders.Query.EQ("废标公告链接.公告ID", -1)).And(MongoDB.Driver.Builders.Query.EQ("项目类型", 项目类型.网上竞标)));
            return PartialView("Part_View/Part_OnlineBidding_Add");
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
        public ActionResult DeleteAttachmentJb()
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
        public ActionResult ThirdClassJb()
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
                count = goods.Count() / 8;
                if ((goods.Count() % 8) > 0)
                {
                    count++;
                }
                foreach (var item in goods.Skip(int.Parse(skip) - 1).Take(8))
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
        [HttpPost]
        [ValidateInput(false)]
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
                if (!string.IsNullOrEmpty(Request.Form["startPrice"]))
                {
                    m.起始价格 = Convert.ToDecimal(Request.Form["startPrice"]);

                }
                model.商品名称 = m.商品名称;
                model.所属行业 = m.所属行业;
                model.商品型号 = m.商品型号;
                model.商品描述 = m.商品描述;
                model.起始价格 = m.起始价格;
                model.商品需求数量 = m.商品需求数量;
                model.交货时间 = m.交货时间;
                model.报价结束时间 = m.报价结束时间;
                model.联系方式.联系人 = m.联系方式.联系人;
                model.联系方式.手机 = m.联系方式.手机;
                model.送货地址 = m.送货地址;
                网上竞标管理.更新网上竞标(model);
            }
            catch
            {
                return Content("<script>alert('修改出现问题！');location.href='javascript:history.go(-1)';</script>");
            }
            return RedirectToAction("OnlineBidding_List");
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
                    颜色 = System.Drawing.Color.Red
                };
                if (reg.IsMatch(txt))
                {
                    t.透明度 = 180;
                    t.半径 = 60;
                    t.边框宽度 = 1;
                    t.中央五角星 = false;
                    t.颜色 = System.Drawing.Color.Blue;
                }
                else
                {
                    if (long.Parse(id) == 10005 || long.Parse(id) == 10009 || long.Parse(id) == 10013 || long.Parse(id) == 20151 || long.Parse(id) == 20150 || long.Parse(id) == 20145 || long.Parse(id) == 20146 || long.Parse(id) == 20137 || long.Parse(id) == 20138 || long.Parse(id) == 20139 || long.Parse(id) == 20140 || long.Parse(id) == 20141 || long.Parse(id) == 20142 || long.Parse(id) == 20143 || long.Parse(id) == 20144 || long.Parse(id) == 20152 || long.Parse(id) == 20149 || long.Parse(id) == 20147 || long.Parse(id) == 20148 || long.Parse(id) == 20261)
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
        public ActionResult Part_SearchByCondit()
        {
            var status = Request.Params["status"];
            var gysname = Request.Params["gys"];
            var ysdnumber = Request.Params["ysdnum"];
            //var q=MongoDB.Driver.Builders.Query();
            IMongoQuery q = null;
            IEnumerable<BsonValue> p_l = null;
            if (!string.IsNullOrWhiteSpace(ysdnumber))
            {
                q = Query<验收单>.Where(o => o.Id == long.Parse(ysdnumber.Trim()));
            }
            else
            {
                if (status == "审核通过")
                {
                    q = Query<验收单>.Where(o => o.审核数据.审核状态 == 审核状态.审核通过);
                }
                if (status == "已完成")
                {
                    q = Query<验收单>.Where(o => o.审核数据.审核状态 == 审核状态.审核通过 && o.是否已经打印 && o.验收单扫描件.Count > 0);
                }
                if (status == "未打印")
                {
                    q = Query<验收单>.Where(o => o.是否已经打印 == false);
                }
                if (status == "未验收")
                {
                    q = Query<验收单>.Where(o => o.验收单扫描件.Count <= 0);
                }
                if (!string.IsNullOrWhiteSpace(gysname))
                {
                    p_l = 用户管理.列表用户<供应商>(0, 0,
                        Fields<供应商>.Include(o => o.Id),
                        Query<供应商>.EQ(o => o.审核数据.审核状态, 审核状态.审核通过).And(Query.EQ("企业基本信息.企业名称", new BsonRegularExpression(string.Format("/{0}/i", gysname))))
                        ).Select(o => o["_id"]);
                    q = q.And(Query<验收单>.In(o => o.供应商链接.用户ID, p_l));
                }
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
            return PartialView("Part_View/Part_SearchByCondit");
        }


        public ActionResult StatisticsLogin()
        {
            return View();
        }
        public ActionResult Part_StatisticsLogin()
        {
            var now = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
            var loginlist = 登录统计管理.查询登录统计(0, 0, Query<登录统计>.Where(o => o.登录时间 >= now));
            ViewData["登录统计"] = loginlist;
            ViewData["统计总数"] = loginlist.Count();
            return PartialView("Part_View/Part_StatisticsLogin");
        }
        public ActionResult Part_StatisticsLoginSearch()
        {
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
            var loginlist = 登录统计管理.查询登录统计(0, 0, q);
            ViewData["登录统计"] = loginlist;
            ViewData["统计总数"] = loginlist.Count();
            return PartialView("Part_View/Part_StatisticsLoginSearch");
        }
        public ActionResult StatisticsClick()
        {
            return View();
        }
        public ActionResult Part_StatisticsClick()
        {
            var now = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
            var loginlist = 广告点击统计管理.查询广告点击统计(0, 0, Query<广告点击统计>.Where(o => o.基本数据.添加时间 > now));
            ViewData["广告点击统计"] = loginlist;
            ViewData["统计总数"] = loginlist.Count();

            return PartialView("Part_View/Part_StatisticsClick");
        }
        public ActionResult Part_StatisticsClickSearch()
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
            return PartialView("Part_View/Part_StatisticsClickSearch");
        }

        public ActionResult DbExport()
        {
            return View();
        }
        public ActionResult Part_DbExport()
        {
            return PartialView("Part_View/Part_DbExport");
        }
        public ActionResult DbInport()
        {
            return View();
        }
        public ActionResult Part_DbInport()
        {
            return PartialView("Part_View/Part_DbInport");
        }
        public ActionResult SynchroYsd()
        {
            return View();
        }
        public ActionResult Part_SynchroYsd()
        {
            return PartialView("Part_View/Part_SynchroYsd");
        }
        public ActionResult doDbExport()
        {
            var type = Request.Params["type"];
            var retstr = string.Empty;
            switch (type)
            {
                case "单位用户":
                    retstr = DataSynchroHelper.ExpColl<单位用户>();
                    break;
                case "验收单":
                    retstr = DataSynchroHelper.ExpColl<验收单>();
                    break;
                case "公告":
                    retstr = DataSynchroHelper.ExpColl<公告>();
                    break;
                case "商品":
                    retstr = DataSynchroHelper.ExpColl<商品>();
                    break;
                case "商品分类":
                    retstr = DataSynchroHelper.ExpColl<商品分类>();
                    break;

                case "商品历史销售信息":
                    retstr = DataSynchroHelper.ExpColl<商品历史销售信息>();
                    break;
                case "供应商":
                    retstr = DataSynchroHelper.ExpColl<供应商>();
                    break;
                case "办事指南":
                    retstr = DataSynchroHelper.ExpColl<办事指南>();
                    break;
                case "培训资料":
                    retstr = DataSynchroHelper.ExpColl<培训资料>();
                    break;
                case "通知":
                    retstr = DataSynchroHelper.ExpColl<通知>();
                    break;

                case "政策法规":
                    retstr = DataSynchroHelper.ExpColl<政策法规>();
                    break;
                case "登录统计":
                    retstr = DataSynchroHelper.ExpColl<登录统计>();
                    break;
                case "广告点击统计":
                    retstr = DataSynchroHelper.ExpColl<广告点击统计>();
                    break;
                case "下载":
                    retstr = DataSynchroHelper.ExpColl<下载>();
                    break;
                case "用户组":
                    retstr = DataSynchroHelper.ExpColl<用户组>();
                    break;

                case "专家":
                    retstr = DataSynchroHelper.ExpColl<专家>();
                    break;

                case "专家可评标专业":
                    retstr = DataSynchroHelper.ExpColl<专家可评标专业>();
                    break;
#if INTRANET
                case "专家抽选记录":
                    retstr = DataSynchroHelper.ExpColl<专家抽选记录>();
                    break;
                case "供应商抽选记录":
                    retstr = DataSynchroHelper.ExpColl<供应商抽选记录>();
                    break;
#endif
                case "供应商充值记录":
                    retstr = DataSynchroHelper.ExpColl<供应商充值记录>();
                    break;

                case "供应商充值余额":
                    retstr = DataSynchroHelper.ExpColl<供应商充值余额>();
                    break;
                case "供应商服务记录":
                    retstr = DataSynchroHelper.ExpColl<供应商服务记录>();
                    break;
                case "供应商计费项目扣费记录":
                    retstr = DataSynchroHelper.ExpColl<供应商计费项目扣费记录>();
                    break;
                case "供应商增值服务申请记录":
                    retstr = DataSynchroHelper.ExpColl<供应商增值服务申请记录>();
                    break;
            }



            if (type == "单位用户")
            {

            }
            return Content(retstr);
        }



        public ActionResult ExpertBatchmodify()
        {
            long listcount = 用户管理.计数用户<专家>(0, 0);
            long maxpagesize = Math.Max((listcount + PRO_PAGESIZE - 1) / PRO_PAGESIZE, 1);

            int page = 1;
            ViewData["currentpage"] = page;
            ViewData["pagecount"] = maxpagesize;
            ViewData["goodType"] = 商品分类管理.查找子分类();
            ViewData["专家特殊类别"] = 专家可评标专业.非商品分类评审专业;
            var _zj = 用户管理.查询用户<专家>(PRO_PAGESIZE * (page - 1), PRO_PAGESIZE, null, false, SortBy.Descending("基本数据.添加时间"));
            return View(_zj);
        }
        public ActionResult Part_ExpertBatchModify()
        {
            var _expert_name = Request.Params["expertname"];
            if (!string.IsNullOrWhiteSpace(_expert_name))
            {
                var name = _expert_name.Trim();
                var _zj = 用户管理.查询用户<专家>(0, 0, Query<专家>.Where(o => o.身份信息.姓名.Contains(name)));
                ViewData["有无页码"] = 0;
                ViewData["currentpage"] = null;
                return PartialView("Part_View/Part_ExpertBatchModify", _zj);
            }
            else
            {
                long listcount = 用户管理.计数用户<专家>(0, 0);
                long maxpagesize = Math.Max((listcount + PRO_PAGESIZE - 1) / PRO_PAGESIZE, 1);
                var page = int.Parse(Request.Params["page"]);
                if (string.IsNullOrEmpty(page.ToString()) || page < 0 || page > maxpagesize)
                {
                    page = 1;
                }
                ViewData["有无页码"] = 1;
                ViewData["currentpage"] = page;
                ViewData["pagecount"] = maxpagesize;
                var _zj = 用户管理.查询用户<专家>(PRO_PAGESIZE * (page - 1), PRO_PAGESIZE, null, false, SortBy.Descending("基本数据.添加时间"));
                return PartialView("Part_View/Part_ExpertBatchModify", _zj);
            }

        }

        public class Expert
        {
            public long ID { get; set; }
            /// <summary>
            /// 姓名
            /// </summary>
            public string Name { get; set; }
            /// <summary>
            /// 民族
            /// </summary>
            public string Minzu { get; set; }
            /// <summary>
            /// 专家级别
            /// </summary>
            public string Zjjb { get; set; }
            /// <summary>
            /// 审核状态
            /// </summary>
            public string Shzt { get; set; }
        }
        //根据专家姓名查找专家
        public string SearchExpert()
        {
            var name = Request.Params["expertname"];
            var elist = new List<Expert>();
            if (!string.IsNullOrWhiteSpace(name))
            {
                var names = name.Trim();
                var _zj = 用户管理.查询用户<专家>(0, 0, Query<专家>.Where(o => o.身份信息.姓名.Contains(names)));
                foreach (var k in _zj)
                {
                    elist.Add(new Expert()
                    {
                        ID = k.Id,
                        Name = k.身份信息.姓名,
                        Zjjb = k.身份信息.专家级别.ToString(),
                        Minzu = k.身份信息.民族.ToString(),
                        Shzt = k.审核数据.审核状态.ToString(),
                    });
                }
                return JsonConvert.SerializeObject(elist);

            }
            return "查找失败！";
        }


        public void DelExpert()
        {
            var id = Request.Params["id"];//专家ID
            用户管理.删除用户<专家>(long.Parse(id));
        }
        /// <summary>
        /// 批量修改专家
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult ExpertBatchModify()
        {
            var ids = Request.Form["Id"].Split(',');
            var names = Request.Form["身份信息.姓名"].Split(',');
            var sexs = Request.Form["身份信息.性别"].Split(',');
            var zjcards = Request.Form["身份信息.专家证号"].Split(',');
            var cardtype = Request.Form["身份信息.证件类型"].Split(',');
            var cardnumbers = Request.Form["身份信息.证件号"].Split(',');
            var degrees = Request.Form["学历信息.专业技术职称"].Split(',');
            var schools = Request.Form["学历信息.毕业院校"].Split(',');
            var maxdegree = Request.Form["学历信息.最高学历"].Split(',');
            var workadress = Request.Form["工作经历信息.工作单位"].Split(',');
            var workcontent = Request.Form["工作经历信息.从事专业"].Split(',');
            var telnumbers = Request.Form["联系方式.手机"].Split(',');
            var kpbtype = Request.Form["可参评物资类别列表"].Split(',');
            for (int i = 0; i < ids.Length; i++)
            {
                var zj = 用户管理.查找用户<专家>(long.Parse(ids[i]));
                zj.身份信息.姓名 = names[i];
                zj.身份信息.性别 = (性别)Enum.Parse(typeof(性别), sexs[i]);
                zj.身份信息.专家证号 = zjcards[i];
                zj.身份信息.证件类型 = (证件类型)Enum.Parse(typeof(证件类型), cardtype[i]);
                zj.身份信息.证件号 = cardnumbers[i];
                zj.学历信息.专业技术职称 = (专业技术职称)Enum.Parse(typeof(专业技术职称), degrees[i]);
                zj.学历信息.毕业院校 = schools[i];
                zj.学历信息.最高学历 = (学历)Enum.Parse(typeof(学历), maxdegree[i]);
                zj.工作经历信息.工作单位 = workadress[i];
                zj.工作经历信息.从事专业 = workcontent[i];
                zj.联系方式.手机 = telnumbers[i];
                var az = kpbtype[i].Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
                var cplb = new List<供应商._产品类别>();
                foreach (var y in az)
                {
                    var yj = y.Split(':')[0];
                    var ej = y.Split(':')[1].Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                    var cp = new 供应商._产品类别();
                    var listr = new List<string>();
                    foreach (var j in ej)
                    {
                        listr.Add(j);
                    }
                    cp.一级分类 = yj;
                    cp.二级分类 = listr;
                    cplb.Add(cp);
                }
                zj.可参评物资类别列表 = cplb;
                用户管理.更新用户<专家>(zj);
            }
            return RedirectToAction("ExpertBatchmodify");
        }

        [HttpPost]
        public string ExpertModifyOne()
        {
            var id = Request.Params["id"];
            var names = Request.Params["names"];
            var sexs = Request.Params["sexs"];
            var zjcards = Request.Params["zjcards"];
            var cardtype = Request.Params["cardtype"];
            var cardnumbers = Request.Params["cardnumbers"];
            var degrees = Request.Params["degrees"];
            var schools = Request.Params["schools"];
            var maxdegree = Request.Params["maxdegree"];
            var workadress = Request.Params["workadress"];
            var workcontent = Request.Params["workcontent"];
            var telnumber = Request.Params["telnumber"];
            var kpbtype = Request.Params["kpbtype"];

            var zj = 用户管理.查找用户<专家>(long.Parse(id));
            zj.身份信息.姓名 = names;
            zj.身份信息.性别 = (性别)Enum.Parse(typeof(性别), sexs);
            zj.身份信息.专家证号 = zjcards;
            zj.身份信息.证件类型 = (证件类型)Enum.Parse(typeof(证件类型), cardtype);
            zj.身份信息.证件号 = cardnumbers;
            zj.学历信息.专业技术职称 = (专业技术职称)Enum.Parse(typeof(专业技术职称), degrees);
            zj.学历信息.毕业院校 = schools;
            zj.学历信息.最高学历 = (学历)Enum.Parse(typeof(学历), maxdegree);
            zj.工作经历信息.工作单位 = workadress;
            zj.工作经历信息.从事专业 = workcontent;
            zj.联系方式.手机 = telnumber;
            var az = kpbtype.Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
            var cplb = new List<供应商._产品类别>();
            foreach (var y in az)
            {
                var yj = y.Split(':')[0];
                var ej = y.Split(':')[1].Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                var cp = new 供应商._产品类别();
                var listr = new List<string>();
                foreach (var j in ej)
                {
                    listr.Add(j);
                }
                cp.一级分类 = yj;
                cp.二级分类 = listr;
                cplb.Add(cp);
            }
            zj.可参评物资类别列表 = cplb;
            用户管理.更新用户<专家>(zj);
            return "保存成功！";
        }
        [HttpPost]
        public string AddDfzy()
        {
            var _val = Request.Params["zyml"];
            if (!string.IsNullOrWhiteSpace(_val))
            {
                专家可评标专业.添加可评标专业(_val);
                return "1";
            }
            return "0";
        }
        /// <summary>
        /// 创建商品分类索引--用于搜索框智能提示
        /// </summary>
        private void CreateIndex_ProductCatalog(string catalogName, string indexdic)
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
                AddIndex_ProductCatalog(writer, catalogName);

                writer.Optimize();
                writer.Close();
            }
            catch
            {
                //IndexWriter writer = new IndexWriter(Indexdic_Server, PanGuAnalyzer, true, Lucene.Net.Index.IndexWriter.MaxFieldLength.LIMITED);
                IndexWriter writer = new IndexWriter(dir, PanGuAnalyzer, true, IndexWriter.MaxFieldLength.UNLIMITED);
                AddIndex_ProductCatalog(writer, catalogName);

                writer.Optimize();
                writer.Close();
            }

        }

        /// <summary>
        /// 创建商品分类索引--用于搜索框智能提示
        /// </summary>
        /// <param name="analyzer"></param>
        /// <param name="content"></param>
        private void AddIndex_ProductCatalog(IndexWriter writer, string catalogName)
        {
            try
            {
                Document doc = new Document();
                doc.Add(new Field("Name", catalogName.Replace(" ", ""), Field.Store.YES, Field.Index.NOT_ANALYZED));//存储且索引
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

        protected void deleteIndex(string path, string id)
        {
            string Indexdic_Server = IndexDic(path);
            IndexWriter writer = null;
            Lucene.Net.Store.Directory dir = new Lucene.Net.Store.SimpleFSDirectory(new DirectoryInfo(Indexdic_Server));
            try
            {
                writer = new IndexWriter(dir, PanGuAnalyzer, false, IndexWriter.MaxFieldLength.UNLIMITED);
                //writer = new IndexWriter(Indexdic_Server, PanGuAnalyzer, false, Lucene.Net.Index.IndexWriter.MaxFieldLength.LIMITED);
            }
            catch
            {
                writer = new IndexWriter(dir, PanGuAnalyzer, true, IndexWriter.MaxFieldLength.UNLIMITED);
                //writer = new IndexWriter(Indexdic_Server, PanGuAnalyzer, true, Lucene.Net.Index.IndexWriter.MaxFieldLength.LIMITED);
            }
            writer.DeleteDocuments(new Term("NumId", id));//删除一条索引
            writer.Optimize();
            writer.Close();
        }

        //public static string PostDataGetHtml(string uri, string postData)
        //{
        //    try
        //    {
        //        byte[] data = Encoding.GetEncoding(936).GetBytes(postData);

        //        Uri uRI = new Uri(uri);
        //        HttpWebRequest req = WebRequest.Create(uRI) as HttpWebRequest;
        //        req.Method = "POST";
        //        req.KeepAlive = true;
        //        req.ContentType = "application/x-www-form-urlencoded";
        //        req.ContentLength = data.Length;
        //        req.AllowAutoRedirect = true;

        //        Stream outStream = req.GetRequestStream();
        //        outStream.Write(data, 0, data.Length);
        //        outStream.Close();

        //        HttpWebResponse res = req.GetResponse() as HttpWebResponse;
        //        Stream inStream = res.GetResponseStream();
        //        StreamReader sr = new StreamReader(inStream, Encoding.GetEncoding(936));
        //        // sr  = HttpUtility.UrlEncode(sr, System.Text.Encoding.GetEncoding(936))

        //        string htmlResult = sr.ReadToEnd();

        //        return htmlResult;
        //    }
        //    catch (Exception ex)
        //    {
        //        return "网络错误：" + ex.Message.ToString();
        //    }
        //}
    }
}
