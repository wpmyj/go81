using System.Linq;
using Go81WebApp.Models.管理器;
using Go81WebApp.Models.管理器.推广业务管理;
using Go81WebApp.Models.数据模型;
using Go81WebApp.Models.数据模型.商品数据模型;
using Go81WebApp.Models.数据模型.推广业务数据模型;
using Go81WebApp.Models.数据模型.用户数据模型;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.PanGu;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers;
using Lucene.Net.Search;
using MongoDB;
using MongoDB.Driver.Builders;
using NPOI.SS.Formula.Functions;
using PanGu;
using PanGu.HighLight;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Text;
using System.Web.Mvc;
using System.Web.WebPages;
using MongoDB.Driver;
using MongoDB.Bson;

namespace Go81WebApp.Controllers.门户
{
    public class 供应商陈列Controller : Controller
    {
        // GET: GYSK
        private static int PAGESIZE = int.Parse(ConfigurationManager.AppSettings["供应商陈列每页显示条数"]);
        private static int NUMBER_B = int.Parse(ConfigurationManager.AppSettings["企业主页商品展示B类窗口商品个数"]);
        private static int NUMBER_C = int.Parse(ConfigurationManager.AppSettings["企业主页商品展示C类窗口商品个数"]);
        private static int NUMBER_D = int.Parse(ConfigurationManager.AppSettings["企业主页商品展示D类窗口商品个数"]);
        private static int NUMBER_E = int.Parse(ConfigurationManager.AppSettings["企业主页商品展示E类窗口商品个数"]);
        private static int NUMBER_F = int.Parse(ConfigurationManager.AppSettings["企业主页商品展示F类窗口商品个数"]);
        private static int NUMBER_G = int.Parse(ConfigurationManager.AppSettings["企业主页商品展示G类窗口商品个数"]);

        public ActionResult Supplier_Destination()
        {
            try
            {
                //用户基本数据 user = this.HttpContext.获取当前用户();
                long id = long.Parse(Request.QueryString["id"].ToString());
                供应商 supplier = 用户管理.查找用户(id) as 供应商;
                //防止未审核的供应商出现在前台页面上
                if (supplier.审核数据.审核状态 != 审核状态.审核通过)
                {
                    supplier = null;
                }
                if (supplier != null)
                {
                    用户管理.增加浏览量(id);
                    if (-1 != HttpContext.检查登录() || WebApiApplication.IsIntranet || (int)supplier.供应商用户信息.认证级别>=4)
                    {
                        ViewData["已登录"] = "1";
                    }
                    else
                    {
                        return RedirectToAction("NeedLogin", "布局", new { type = "供应商陈列/Supplier_Destination", id = supplier.Id });
                        //ViewData["已登录"] = "0";
                    }

                    long 商品总数 = 商品管理.计数供应商商品(id, 0, 0, MongoDB.Driver.Builders.Query.EQ("审核数据.审核状态", 审核状态.审核通过), includeDisabled: false);
                    //if (商品管理.计数供应商商品(id, 0, 0, MongoDB.Driver.Builders.Query.EQ("审核数据.审核状态", 审核状态.审核通过)) % 20 > 0)
                    //{
                    //    PageCount++;
                    //}
                    //ViewData["sum"] = 商品管理.计数供应商商品(id, 0, 0, MongoDB.Driver.Builders.Query.EQ("审核数据.审核状态", 审核状态.审核通过));
                    //ViewData["PageCount"] = PageCount;
                    //ViewData["CurrentPage"] = 1;
                    ViewData["supplier"] = supplier;

                    var 主页展示模板 = 供应商服务记录管理.查询供应商服务记录(0, 0, Query<供应商服务记录>.Where(o => o.所属供应商.用户ID == id));
                    if (主页展示模板.Count() > 0 && 主页展示模板.Any())
                    {
                        var 主页展示模板开通 = 主页展示模板.First();
                        var 展示模板 = 主页展示模板开通.已开通的服务.Find(o => o.所申请项目名.Contains("企业主页商品展示") && o.结束时间 > DateTime.Now);
                        if (展示模板 != null)
                        {
                            if (展示模板.所申请项目名.Contains("A类"))
                            {
                                long PageCount = 商品管理.计数供应商商品(id, 0, 0, MongoDB.Driver.Builders.Query.EQ("审核数据.审核状态", 审核状态.审核通过), includeDisabled: false) / 20;
                                if (商品管理.计数供应商商品(id, 0, 0, MongoDB.Driver.Builders.Query.EQ("审核数据.审核状态", 审核状态.审核通过), includeDisabled: false) % 20 > 0)
                                {
                                    PageCount++;
                                }
                                ViewData["sum"] = 商品管理.计数供应商商品(id, 0, 0, MongoDB.Driver.Builders.Query.EQ("审核数据.审核状态", 审核状态.审核通过), includeDisabled: false);
                                ViewData["PageCount"] = PageCount;
                                ViewData["Goods"] = 商品管理.查询供应商商品(id, 0, 20,
                                    MongoDB.Driver.Builders.Query.EQ("审核数据.审核状态", 审核状态.审核通过), includeDisabled: false);
                            }
                            else if (展示模板.所申请项目名.Contains("B类"))
                            {
                                if (商品总数 > NUMBER_B)//如果该供应商的商品总数大于商品展示的窗口数
                                {
                                    long PageCount = NUMBER_B / 20;
                                    ViewData["sum"] = NUMBER_B;
                                    ViewData["PageCount"] = PageCount;
                                }
                                else
                                {
                                    long PageCount = 商品管理.计数供应商商品(id, 0, 0, MongoDB.Driver.Builders.Query.EQ("审核数据.审核状态", 审核状态.审核通过), includeDisabled: false) / 20;
                                    if (商品管理.计数供应商商品(id, 0, 0, MongoDB.Driver.Builders.Query.EQ("审核数据.审核状态", 审核状态.审核通过), includeDisabled: false) % 20 > 0)
                                    {
                                        PageCount++;
                                    }
                                    ViewData["sum"] = 商品管理.计数供应商商品(id, 0, 0, MongoDB.Driver.Builders.Query.EQ("审核数据.审核状态", 审核状态.审核通过), includeDisabled: false);
                                    ViewData["PageCount"] = PageCount;
                                }
                                ViewData["Goods"] = 商品管理.查询供应商商品(id, 0, 20,
                                    MongoDB.Driver.Builders.Query.EQ("审核数据.审核状态", 审核状态.审核通过), includeDisabled: false);
                            }
                            else if (展示模板.所申请项目名.Contains("C类"))
                            {
                                if (商品总数 > NUMBER_C)
                                {
                                    long PageCount = NUMBER_C / 20;
                                    ViewData["sum"] = NUMBER_C;
                                    ViewData["PageCount"] = PageCount;
                                }
                                else
                                {
                                    long PageCount = 商品管理.计数供应商商品(id, 0, 0, MongoDB.Driver.Builders.Query.EQ("审核数据.审核状态", 审核状态.审核通过), includeDisabled: false) / 20;
                                    if (商品管理.计数供应商商品(id, 0, 0, MongoDB.Driver.Builders.Query.EQ("审核数据.审核状态", 审核状态.审核通过), includeDisabled: false) % 20 > 0)
                                    {
                                        PageCount++;
                                    }
                                    ViewData["sum"] = 商品管理.计数供应商商品(id, 0, 0, MongoDB.Driver.Builders.Query.EQ("审核数据.审核状态", 审核状态.审核通过), includeDisabled: false);
                                    ViewData["PageCount"] = PageCount;
                                }
                                ViewData["Goods"] = 商品管理.查询供应商商品(id, 0, 20,
                                    MongoDB.Driver.Builders.Query.EQ("审核数据.审核状态", 审核状态.审核通过), includeDisabled: false);
                            }
                            else if (展示模板.所申请项目名.Contains("D类"))
                            {
                                if (商品总数 > NUMBER_D)
                                {
                                    long PageCount = NUMBER_D / 20;
                                    ViewData["sum"] = NUMBER_D;
                                    ViewData["PageCount"] = PageCount;
                                }
                                else
                                {
                                    long PageCount = 商品管理.计数供应商商品(id, 0, 0, MongoDB.Driver.Builders.Query.EQ("审核数据.审核状态", 审核状态.审核通过), includeDisabled: false) / 20;
                                    if (商品管理.计数供应商商品(id, 0, 0, MongoDB.Driver.Builders.Query.EQ("审核数据.审核状态", 审核状态.审核通过), includeDisabled: false) % 20 > 0)
                                    {
                                        PageCount++;
                                    }
                                    ViewData["sum"] = 商品管理.计数供应商商品(id, 0, 0, MongoDB.Driver.Builders.Query.EQ("审核数据.审核状态", 审核状态.审核通过), includeDisabled: false);
                                    ViewData["PageCount"] = PageCount;
                                }
                                ViewData["Goods"] = 商品管理.查询供应商商品(id, 0, 20,
                                    MongoDB.Driver.Builders.Query.EQ("审核数据.审核状态", 审核状态.审核通过), includeDisabled: false);
                            }
                            else if (展示模板.所申请项目名.Contains("E类"))
                            {
                                if (商品总数 > NUMBER_E)
                                {
                                    long PageCount = NUMBER_E / 20;
                                    ViewData["sum"] = NUMBER_E;
                                    ViewData["PageCount"] = PageCount;
                                }
                                else
                                {
                                    long PageCount = 商品管理.计数供应商商品(id, 0, 0, MongoDB.Driver.Builders.Query.EQ("审核数据.审核状态", 审核状态.审核通过), includeDisabled: false) / 20;
                                    if (商品管理.计数供应商商品(id, 0, 0, MongoDB.Driver.Builders.Query.EQ("审核数据.审核状态", 审核状态.审核通过), includeDisabled: false) % 20 > 0)
                                    {
                                        PageCount++;
                                    }
                                    ViewData["sum"] = 商品管理.计数供应商商品(id, 0, 0, MongoDB.Driver.Builders.Query.EQ("审核数据.审核状态", 审核状态.审核通过), includeDisabled: false);
                                    ViewData["PageCount"] = PageCount;
                                }
                                ViewData["Goods"] = 商品管理.查询供应商商品(id, 0, 20,
                                    MongoDB.Driver.Builders.Query.EQ("审核数据.审核状态", 审核状态.审核通过), includeDisabled: false);
                            }
                            else if (展示模板.所申请项目名.Contains("F类"))
                            {
                                if (商品总数 > NUMBER_F)
                                {
                                    long PageCount = NUMBER_F / 20;
                                    ViewData["sum"] = NUMBER_F;
                                    ViewData["PageCount"] = PageCount;
                                }
                                else
                                {
                                    long PageCount = 商品管理.计数供应商商品(id, 0, 0, MongoDB.Driver.Builders.Query.EQ("审核数据.审核状态", 审核状态.审核通过), includeDisabled: false) / 20;
                                    if (商品管理.计数供应商商品(id, 0, 0, MongoDB.Driver.Builders.Query.EQ("审核数据.审核状态", 审核状态.审核通过), includeDisabled: false) % 20 > 0)
                                    {
                                        PageCount++;
                                    }
                                    ViewData["sum"] = 商品管理.计数供应商商品(id, 0, 0, MongoDB.Driver.Builders.Query.EQ("审核数据.审核状态", 审核状态.审核通过), includeDisabled: false);
                                    ViewData["PageCount"] = PageCount;
                                }
                                ViewData["Goods"] = 商品管理.查询供应商商品(id, 0, 20,
                                    MongoDB.Driver.Builders.Query.EQ("审核数据.审核状态", 审核状态.审核通过), includeDisabled: false);
                            }
                            else
                            {
                                if (商品总数 > NUMBER_G)
                                {
                                    long PageCount = NUMBER_G / 20;
                                    ViewData["sum"] = NUMBER_G;
                                    ViewData["PageCount"] = PageCount;
                                    ViewData["Goods"] = 商品管理.查询供应商商品(id, 0, NUMBER_G,
                                        MongoDB.Driver.Builders.Query.EQ("审核数据.审核状态", 审核状态.审核通过), includeDisabled: false);
                                }
                                else
                                {
                                    long PageCount = 商品管理.计数供应商商品(id, 0, 0, MongoDB.Driver.Builders.Query.EQ("审核数据.审核状态", 审核状态.审核通过), includeDisabled: false) / 20;
                                    if (商品管理.计数供应商商品(id, 0, 0, MongoDB.Driver.Builders.Query.EQ("审核数据.审核状态", 审核状态.审核通过), includeDisabled: false) % 20 > 0)
                                    {
                                        PageCount++;
                                    }
                                    ViewData["sum"] = 商品管理.计数供应商商品(id, 0, 0, MongoDB.Driver.Builders.Query.EQ("审核数据.审核状态", 审核状态.审核通过), includeDisabled: false);
                                    ViewData["PageCount"] = PageCount;
                                    ViewData["Goods"] = 商品管理.查询供应商商品(id, 0, 20,
                                        MongoDB.Driver.Builders.Query.EQ("审核数据.审核状态", 审核状态.审核通过), includeDisabled: false);
                                }
                            }
                        }
                        else
                        {
                            if (商品总数 > NUMBER_G)
                            {
                                long PageCount = NUMBER_G / 20;
                                ViewData["sum"] = NUMBER_G;
                                ViewData["PageCount"] = PageCount;
                                ViewData["Goods"] = 商品管理.查询供应商商品(id, 0, NUMBER_G,
                                    MongoDB.Driver.Builders.Query.EQ("审核数据.审核状态", 审核状态.审核通过), includeDisabled: false);
                            }
                            else
                            {
                                long PageCount = 商品管理.计数供应商商品(id, 0, 0, MongoDB.Driver.Builders.Query.EQ("审核数据.审核状态", 审核状态.审核通过), includeDisabled: false) / 20;
                                if (商品管理.计数供应商商品(id, 0, 0, MongoDB.Driver.Builders.Query.EQ("审核数据.审核状态", 审核状态.审核通过), includeDisabled: false) % 20 > 0)
                                {
                                    PageCount++;
                                }
                                ViewData["sum"] = 商品管理.计数供应商商品(id, 0, 0, MongoDB.Driver.Builders.Query.EQ("审核数据.审核状态", 审核状态.审核通过), includeDisabled: false);
                                ViewData["PageCount"] = PageCount;
                                ViewData["Goods"] = 商品管理.查询供应商商品(id, 0, 20,
                                    MongoDB.Driver.Builders.Query.EQ("审核数据.审核状态", 审核状态.审核通过), includeDisabled: false);
                            }

                        }
                    }
                    else
                    {
                        if (商品总数 > NUMBER_G)
                        {
                            long PageCount = NUMBER_G / 20;
                            ViewData["sum"] = NUMBER_G;
                            ViewData["PageCount"] = PageCount;
                            ViewData["Goods"] = 商品管理.查询供应商商品(id, 0, NUMBER_G,
                                MongoDB.Driver.Builders.Query.EQ("审核数据.审核状态", 审核状态.审核通过), includeDisabled: false);
                        }
                        else
                        {
                            long PageCount = 商品管理.计数供应商商品(id, 0, 0, MongoDB.Driver.Builders.Query.EQ("审核数据.审核状态", 审核状态.审核通过), includeDisabled: false) / 20;
                            if (商品管理.计数供应商商品(id, 0, 0, MongoDB.Driver.Builders.Query.EQ("审核数据.审核状态", 审核状态.审核通过), includeDisabled: false) % 20 > 0)
                            {
                                PageCount++;
                            }
                            ViewData["sum"] = 商品管理.计数供应商商品(id, 0, 0, MongoDB.Driver.Builders.Query.EQ("审核数据.审核状态", 审核状态.审核通过), includeDisabled: false);
                            ViewData["PageCount"] = PageCount;
                            ViewData["Goods"] = 商品管理.查询供应商商品(id, 0, 20,
                                MongoDB.Driver.Builders.Query.EQ("审核数据.审核状态", 审核状态.审核通过), includeDisabled: false);
                        }
                    }

                    ViewData["CurrentPage"] = 1;
                    List<商品> Hot_Good = new List<商品>();
                    List<long> ids = new List<long>();
                    List<long> gid = new List<long>();
                    int count = 0;
                    IEnumerable<供应商服务记录> model1 = 供应商服务记录管理.查询供应商服务记录(0, 0, Query<供应商服务记录>.Where(o => o.所属供应商.用户ID == id));
                    if (model1 != null && model1.Count() != 0)
                    {
                        if (model1.First().已开通的服务 != null && model1.First().已开通的服务.Count != 0)
                        {
                            foreach (var item in model1.First().已开通的服务)
                            {
                                if (item.所申请项目名 == "标准会员" || item.所申请项目名 == "商务会员")
                                {
                                    count = 10;
                                    break;
                                }
                            }
                        }
                    }
                    ViewData["count"] = count;
                    IEnumerable<供应商增值服务申请记录> model = 供应商增值服务申请记录管理.查询供应商增值服务申请记录(0, 10, Query<供应商增值服务申请记录>.Where(o => o.所申请项目名 == "企业推广服务B3位置" && o.是否通过 == 通过状态.通过 && o.结束时间 > DateTime.Now));
                    if (model != null && model.Count() != 0)
                    {
                        foreach (var item in model)
                        {
                            if (item.所属供应商.用户数据.供应商用户信息.广告商品 != null && item.所属供应商.用户数据.供应商用户信息.广告商品.Count != 0)
                            {
                                foreach (var n in item.所属供应商.用户数据.供应商用户信息.广告商品)
                                {
                                    if (n.Key == "企业推广服务B3位置")
                                    {
                                        if (n.Value != null && n.Value.Count != 0)
                                        {
                                            foreach (var h in n.Value)
                                            {
                                                Hot_Good.Add(h.商品.商品);
                                                ids.Add(h.商品.商品ID);
                                                if (!gid.Contains(h.商品.商品.商品信息.所属供应商.用户ID))
                                                {
                                                    gid.Add(h.商品.商品.商品信息.所属供应商.用户ID);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    if (Hot_Good.Count < 10)
                    {
                        int remain = Hot_Good.Count;
                        for (int i = remain; i < 10; i++)
                        {
                            IEnumerable<商品> Temp_Good = 商品管理.查询商品(0, 1, Query<商品>.Where(o => !o.采购信息.参与协议采购 && !o.采购信息.参与应急采购 && o.审核数据.审核状态 == 审核状态.审核通过).And(Query<商品>.NotIn(h => h.Id, ids)).And(Query<商品>.NotIn(h => h.商品信息.所属供应商.用户ID, gid)), includeDisabled: false).OrderByDescending(u => u.销售信息.点击量);
                            if (Temp_Good != null && Temp_Good.Count() != 0)
                            {
                                gid.Add(Temp_Good.First().商品信息.所属供应商.用户ID);
                                Hot_Good.Add(Temp_Good.First());
                            }
                        }
                    }
                    ViewData["Hot_Sale"] = Hot_Good;
                    return View();
                }
                else
                {
                    return Redirect("/供应商陈列/GysList");
                }

            }
            catch
            {
                return Redirect("/供应商陈列/GysList");
            }
        }
        public class Goods
        {
            public long Id { get; set; }
            public string name { get; set; }
            public int Scan { get; set; }
            public string Price { get; set; }
            public string Picture { get; set; }
        }
        public ActionResult GoodList()
        {
            try
            {
                List<Goods> gs = new List<Goods>();
                long id = long.Parse(Request.QueryString["id"]);
                int CurrentPage = int.Parse(Request.QueryString["p"]);
                long 商品总数 = 商品管理.计数供应商商品(id, 0, 0, MongoDB.Driver.Builders.Query.EQ("审核数据.审核状态", 审核状态.审核通过), includeDisabled: false);
                //if (商品管理.计数供应商商品(id, 0, 0, MongoDB.Driver.Builders.Query.EQ("审核数据.审核状态", 审核状态.审核通过)) % 20 > 0)
                //{
                //    PageCount++;
                //}
                //IEnumerable<商品> goods = 商品管理.查询供应商商品(id, 20 * (CurrentPage - 1), 20, MongoDB.Driver.Builders.Query.EQ("审核数据.审核状态", 审核状态.审核通过));
                long PageCount = 0;
                IEnumerable<商品> goods = null;
                var 主页展示模板 = 供应商服务记录管理.查询供应商服务记录(0, 0, Query<供应商服务记录>.Where(o => o.所属供应商.用户ID == id));
                if (主页展示模板.Count() > 0 && 主页展示模板.Any())
                {
                    var 主页展示模板开通 = 主页展示模板.First();
                    var 展示模板 = 主页展示模板开通.已开通的服务.Find(o => o.所申请项目名.Contains("企业主页商品展示") && o.结束时间 > DateTime.Now);
                    if (展示模板 != null)
                    {
                        if (展示模板.所申请项目名.Contains("A类"))
                        {
                            PageCount = 商品管理.计数供应商商品(id, 0, 0, MongoDB.Driver.Builders.Query.EQ("审核数据.审核状态", 审核状态.审核通过), includeDisabled: false) / 20;
                            if (商品管理.计数供应商商品(id, 0, 0, MongoDB.Driver.Builders.Query.EQ("审核数据.审核状态", 审核状态.审核通过), includeDisabled: false) % 20 > 0)
                            {
                                PageCount++;
                            }
                            ViewData["sum"] = 商品管理.计数供应商商品(id, 0, 0, MongoDB.Driver.Builders.Query.EQ("审核数据.审核状态", 审核状态.审核通过), includeDisabled: false);
                            goods = 商品管理.查询供应商商品(id, 20 * (CurrentPage - 1), 20, MongoDB.Driver.Builders.Query.EQ("审核数据.审核状态", 审核状态.审核通过), includeDisabled: false);
                        }
                        else if (展示模板.所申请项目名.Contains("B类"))
                        {
                            if (商品总数 > NUMBER_B)//如果该供应商的商品总数大于商品展示的窗口数
                            {
                                PageCount = NUMBER_B / 20;
                                ViewData["sum"] = NUMBER_B;
                            }
                            else
                            {
                                PageCount = 商品管理.计数供应商商品(id, 0, 0, MongoDB.Driver.Builders.Query.EQ("审核数据.审核状态", 审核状态.审核通过), includeDisabled: false) / 20;
                                if (商品管理.计数供应商商品(id, 0, 0, MongoDB.Driver.Builders.Query.EQ("审核数据.审核状态", 审核状态.审核通过), includeDisabled: false) % 20 > 0)
                                {
                                    PageCount++;
                                }
                                ViewData["sum"] = 商品管理.计数供应商商品(id, 0, 0, MongoDB.Driver.Builders.Query.EQ("审核数据.审核状态", 审核状态.审核通过), includeDisabled: false);
                                ViewData["PageCount"] = PageCount;
                            }
                            goods = 商品管理.查询供应商商品(id, 20 * (CurrentPage - 1), 20, MongoDB.Driver.Builders.Query.EQ("审核数据.审核状态", 审核状态.审核通过), includeDisabled: false);
                        }
                        else if (展示模板.所申请项目名.Contains("C类"))
                        {
                            if (商品总数 > NUMBER_C)
                            {
                                PageCount = NUMBER_C / 20;
                                ViewData["sum"] = NUMBER_C;
                            }
                            else
                            {
                                PageCount = 商品管理.计数供应商商品(id, 0, 0, MongoDB.Driver.Builders.Query.EQ("审核数据.审核状态", 审核状态.审核通过), includeDisabled: false) / 20;
                                if (商品管理.计数供应商商品(id, 0, 0, MongoDB.Driver.Builders.Query.EQ("审核数据.审核状态", 审核状态.审核通过), includeDisabled: false) % 20 > 0)
                                {
                                    PageCount++;
                                }
                                ViewData["sum"] = 商品管理.计数供应商商品(id, 0, 0, MongoDB.Driver.Builders.Query.EQ("审核数据.审核状态", 审核状态.审核通过), includeDisabled: false);
                                ViewData["PageCount"] = PageCount;
                            }
                            goods = 商品管理.查询供应商商品(id, 20 * (CurrentPage - 1), 20, MongoDB.Driver.Builders.Query.EQ("审核数据.审核状态", 审核状态.审核通过), includeDisabled: false);
                        }
                        else if (展示模板.所申请项目名.Contains("D类"))
                        {
                            if (商品总数 > NUMBER_D)
                            {
                                PageCount = NUMBER_D / 20;
                                ViewData["sum"] = NUMBER_D;
                            }
                            else
                            {
                                PageCount = 商品管理.计数供应商商品(id, 0, 0, MongoDB.Driver.Builders.Query.EQ("审核数据.审核状态", 审核状态.审核通过), includeDisabled: false) / 20;
                                if (商品管理.计数供应商商品(id, 0, 0, MongoDB.Driver.Builders.Query.EQ("审核数据.审核状态", 审核状态.审核通过), includeDisabled: false) % 20 > 0)
                                {
                                    PageCount++;
                                }
                                ViewData["sum"] = 商品管理.计数供应商商品(id, 0, 0, MongoDB.Driver.Builders.Query.EQ("审核数据.审核状态", 审核状态.审核通过), includeDisabled: false);
                                ViewData["PageCount"] = PageCount;
                            }

                            goods = 商品管理.查询供应商商品(id, 20 * (CurrentPage - 1), 20, MongoDB.Driver.Builders.Query.EQ("审核数据.审核状态", 审核状态.审核通过), includeDisabled: false);
                        }
                        else if (展示模板.所申请项目名.Contains("E类"))
                        {
                            if (商品总数 > NUMBER_E)
                            {
                                PageCount = NUMBER_E / 20;
                                ViewData["sum"] = NUMBER_E;
                            }
                            else
                            {
                                PageCount = 商品管理.计数供应商商品(id, 0, 0, MongoDB.Driver.Builders.Query.EQ("审核数据.审核状态", 审核状态.审核通过), includeDisabled: false) / 20;
                                if (商品管理.计数供应商商品(id, 0, 0, MongoDB.Driver.Builders.Query.EQ("审核数据.审核状态", 审核状态.审核通过), includeDisabled: false) % 20 > 0)
                                {
                                    PageCount++;
                                }
                                ViewData["sum"] = 商品管理.计数供应商商品(id, 0, 0, MongoDB.Driver.Builders.Query.EQ("审核数据.审核状态", 审核状态.审核通过), includeDisabled: false);
                                ViewData["PageCount"] = PageCount;
                            }

                            goods = 商品管理.查询供应商商品(id, 20 * (CurrentPage - 1), 20, MongoDB.Driver.Builders.Query.EQ("审核数据.审核状态", 审核状态.审核通过), includeDisabled: false);
                        }
                        else if (展示模板.所申请项目名.Contains("F类"))
                        {
                            if (商品总数 > NUMBER_F)
                            {
                                PageCount = NUMBER_F / 20;
                                ViewData["sum"] = NUMBER_F;
                            }
                            else
                            {
                                PageCount = 商品管理.计数供应商商品(id, 0, 0, MongoDB.Driver.Builders.Query.EQ("审核数据.审核状态", 审核状态.审核通过), includeDisabled: false) / 20;
                                if (商品管理.计数供应商商品(id, 0, 0, MongoDB.Driver.Builders.Query.EQ("审核数据.审核状态", 审核状态.审核通过), includeDisabled: false) % 20 > 0)
                                {
                                    PageCount++;
                                }
                                ViewData["sum"] = 商品管理.计数供应商商品(id, 0, 0, MongoDB.Driver.Builders.Query.EQ("审核数据.审核状态", 审核状态.审核通过), includeDisabled: false);
                                ViewData["PageCount"] = PageCount;
                            }

                            goods = 商品管理.查询供应商商品(id, 20 * (CurrentPage - 1), 20, MongoDB.Driver.Builders.Query.EQ("审核数据.审核状态", 审核状态.审核通过), includeDisabled: false);
                        }
                        else
                        {
                            if (商品总数 > NUMBER_G)
                            {
                                PageCount = NUMBER_G / 20;
                                ViewData["sum"] = NUMBER_G;
                            }
                            else
                            {
                                PageCount = 商品管理.计数供应商商品(id, 0, 0, MongoDB.Driver.Builders.Query.EQ("审核数据.审核状态", 审核状态.审核通过), includeDisabled: false) / 20;
                                if (商品管理.计数供应商商品(id, 0, 0, MongoDB.Driver.Builders.Query.EQ("审核数据.审核状态", 审核状态.审核通过), includeDisabled: false) % 20 > 0)
                                {
                                    PageCount++;
                                }
                                ViewData["sum"] = 商品管理.计数供应商商品(id, 0, 0, MongoDB.Driver.Builders.Query.EQ("审核数据.审核状态", 审核状态.审核通过), includeDisabled: false);
                                ViewData["PageCount"] = PageCount;
                            }
                            goods = 商品管理.查询供应商商品(id, 20 * (CurrentPage - 1), 20, MongoDB.Driver.Builders.Query.EQ("审核数据.审核状态", 审核状态.审核通过), includeDisabled: false);
                        }
                    }
                    else
                    {
                        if (商品总数 > NUMBER_G)
                        {
                            PageCount = NUMBER_G / 20;
                            ViewData["sum"] = NUMBER_G;
                        }
                        else
                        {
                            PageCount = 商品管理.计数供应商商品(id, 0, 0, MongoDB.Driver.Builders.Query.EQ("审核数据.审核状态", 审核状态.审核通过), includeDisabled: false) / 20;
                            if (商品管理.计数供应商商品(id, 0, 0, MongoDB.Driver.Builders.Query.EQ("审核数据.审核状态", 审核状态.审核通过), includeDisabled: false) % 20 > 0)
                            {
                                PageCount++;
                            }
                            ViewData["sum"] = 商品管理.计数供应商商品(id, 0, 0, MongoDB.Driver.Builders.Query.EQ("审核数据.审核状态", 审核状态.审核通过), includeDisabled: false);
                            ViewData["PageCount"] = PageCount;
                        }
                        goods = 商品管理.查询供应商商品(id, 20 * (CurrentPage - 1), 20, MongoDB.Driver.Builders.Query.EQ("审核数据.审核状态", 审核状态.审核通过), includeDisabled: false);
                    }
                }
                else
                {
                    if (商品总数 > NUMBER_G)
                    {
                        PageCount = NUMBER_G / 20;
                        ViewData["sum"] = NUMBER_G;
                    }
                    else
                    {
                        PageCount = 商品管理.计数供应商商品(id, 0, 0, MongoDB.Driver.Builders.Query.EQ("审核数据.审核状态", 审核状态.审核通过), includeDisabled: false) / 20;
                        if (商品管理.计数供应商商品(id, 0, 0, MongoDB.Driver.Builders.Query.EQ("审核数据.审核状态", 审核状态.审核通过), includeDisabled: false) % 20 > 0)
                        {
                            PageCount++;
                        }
                        ViewData["sum"] = 商品管理.计数供应商商品(id, 0, 0, MongoDB.Driver.Builders.Query.EQ("审核数据.审核状态", 审核状态.审核通过), includeDisabled: false);
                        ViewData["PageCount"] = PageCount;
                    }
                    goods = 商品管理.查询供应商商品(id, 20 * (CurrentPage - 1), 20, MongoDB.Driver.Builders.Query.EQ("审核数据.审核状态", 审核状态.审核通过), includeDisabled: false);
                }

                if (goods != null)
                {
                    foreach (var item in goods)
                    {
                        Goods g = new Goods();
                        g.Id = item.Id;
                        g.Price = string.Format("{0:0.00}", item.销售信息.价格);
                        g.Scan = item.销售信息.点击量;
                        g.name = item.商品信息.商品名;
                        if (item.商品信息.商品图片 != null && item.商品信息.商品图片.Count != 0)
                        {
                            g.Picture = item.商品信息.商品图片[0].Replace("original","150X150");
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
        public ActionResult Detail_info()
        {
            try
            {
                long id = long.Parse(Request.QueryString["id"]);
                供应商 model = 用户管理.查找用户<供应商>(id);
                JsonResult json = new JsonResult();
                if (string.IsNullOrWhiteSpace(model.企业基本信息.企业简介))
                {
                    json.Data = "没有简介！" + "<br/><a href='/供应商陈列/Supplier_Destination?id=" + model.Id + "'>了解更多>></a>";
                }
                else
                {
                    if (model.企业基本信息.企业简介.Length > 150)
                    {
                        json.Data = model.企业基本信息.企业简介.Substring(0, 150) + "...<br/><a href='/供应商陈列/Supplier_Destination?id=" + model.Id + "'>了解更多>></a>";
                    }
                    else
                    {
                        json.Data = model.企业基本信息.企业简介 + "...<br/><a href='/供应商陈列/Supplier_Destination?id=" + model.Id + "'>了解更多>></a>";
                    }
                }
                return Json(json, JsonRequestBehavior.AllowGet);
            }
            catch
            {
                JsonResult json = new JsonResult();
                json.Data = "出错啦！";
                return Json(json, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult GysList()
        {
            return View();
        }
        public ActionResult GysList_Inlib()
        {
            return View();
        }
        public ActionResult Part_GysList_Inlib() //供应商列表
        {
            ViewData["行业列表"] = 商品分类管理.查找子分类();
            return PartialView("Part_Gys/Part_GysList_Inlib");
        }
        public ActionResult SearchGys()
        {
            try
            {
                int skip = 1;
                if (!string.IsNullOrWhiteSpace(Request.QueryString["skip"]))
                {
                    skip = int.Parse(Request.QueryString["skip"]);
                }
                string province = Request.QueryString["province"];
                string city = Request.QueryString["city"];
                string area = Request.QueryString["area"];
                string industry = Request.QueryString["factory"];
                string name = Request.QueryString["name"];
                IMongoQuery query = null;
                if (!string.IsNullOrWhiteSpace(province))
                {
                    query = query.And(Query<供应商>.Matches(m => m.所属地域.省份, new BsonRegularExpression(string.Format("/{0}/i", province))));
                }
                if (!string.IsNullOrWhiteSpace(city))
                {
                    query = query.And(Query<供应商>.Matches(m=>m.所属地域.城市, new BsonRegularExpression(string.Format("/{0}/i", city))));
                }
                if (!string.IsNullOrWhiteSpace(area))
                {
                    query = query.And(Query<供应商>.Matches(m => m.所属地域.区县, new BsonRegularExpression(string.Format("/{0}/i", area))));
                }
                if(!string.IsNullOrWhiteSpace(industry))
                {
                    query = query.And(Query<供应商>.Matches(m=>m.企业基本信息.所属行业, new BsonRegularExpression(string.Format("/{0}/i", industry))));
                }
                if (!string.IsNullOrWhiteSpace(name))
                {
                    query = query.And(Query<供应商>.Matches(m => m.企业基本信息.企业名称, new BsonRegularExpression(string.Format("/{0}/i", name))));
                }
                IEnumerable<供应商> user = 用户管理.查询用户<供应商>(20*(skip-1),20,query.And(Query<供应商>.Where(m=>m.供应商用户信息.复审数据.审核状态== 审核状态.审核通过)));
                List<Industry> gys = new List<Industry>();
                foreach(var item in user)
                {
                    Industry a = new Industry();
                    a.Id = item.Id;
                    a.Name = item.企业基本信息.企业名称;
                    gys.Add(a);
                }
                long pCount = 用户管理.计数用户<供应商>(0, 0, query.And(Query<供应商>.Where(m => m.供应商用户信息.复审数据.审核状态 == 审核状态.审核通过))) / 20;
                if (用户管理.计数用户<供应商>(0, 0, query.And(Query<供应商>.Where(m => m.供应商用户信息.复审数据.审核状态 == 审核状态.审核通过))) % 20 > 0)
                {
                    pCount++;
                }
                JsonResult json = new JsonResult() { Data = new {u=gys,p=pCount } };
                return Json(json,JsonRequestBehavior.AllowGet);
            }
            catch
            {
                List<Industry> gys = new List<Industry>();
                JsonResult json = new JsonResult() { Data = new { u = gys} };
                return Json(json, JsonRequestBehavior.AllowGet);
            }
        }
        //public ActionResult Supplier_Destination(long userID)
        //{
        //    ViewData["userID"] = userID;
        //    用户基本数据 userBaseData = 用户管理.查找用户(userID);
        //    return View(userBaseData);
        //}
        public ActionResult Part_GysSelect()
        {
            if (-1 != HttpContext.检查登录())
            {
                ViewData["已登录"] = "1";
            }
            else
            {
                ViewData["已登录"] = "0";
            }
            TopDocs serchalllist = null;
            int page = 1;

            PanGu.Segment.Init(PanGuXmlPath);

            string provence = Request.Params["provence"];//所在省份
            string city = Request.Params["city"];//所在城市
            string area = Request.Params["area"];//所在区县
            string industry = Request.Params["industry"];//所属行业
            string comname = Request.Params["comname"];//企业名称


            if (provence == "不限省份" && industry == "请选择行业" && string.IsNullOrEmpty(comname))
            {
                return Content("0");
            }

            try
            {
                BooleanQuery bQuery = new BooleanQuery();
                if (provence != "不限省份")
                {
                    //Lucene.Net.Search.Query query = new QueryParser("Province", PanGuAnalyzer).Parse(provence);
                    Lucene.Net.Search.Query query = new TermQuery(new Term("Province", provence));
                    bQuery.Add(query, BooleanClause.Occur.MUST);//其中的MUST、SHOULD、MUST_NOT表示与、或、非

                    if (city != "不限城市")
                    {
                        //query = new QueryParser("City", PanGuAnalyzer).Parse(city);
                        query = new TermQuery(new Term("City", city));
                        bQuery.Add(query, BooleanClause.Occur.MUST);

                        if (area != "不限区县")
                        {

                            //query = new QueryParser("Area", PanGuAnalyzer).Parse(area);
                            //query = new TermQuery(new Term("Area", area));
                            query = new QueryParser(Lucene.Net.Util.Version.LUCENE_29, "Area", PanGuAnalyzer).Parse(area);
                            bQuery.Add(query, BooleanClause.Occur.MUST);
                        }
                    }
                }


                if (industry != "请选择行业")
                {
                    Lucene.Net.Search.Query query = new QueryParser(Lucene.Net.Util.Version.LUCENE_29, "Industry", PanGuAnalyzer).Parse(industry);
                    //Lucene.Net.Search.Query query = new TermQuery(new Term("Industry", industry));
                    bQuery.Add(query, BooleanClause.Occur.MUST);
                }


                Dictionary<string, string> dic = new Dictionary<string, string>();
                if (!string.IsNullOrEmpty(comname))
                {
                    //bQuery1.Add(query5, BooleanClause.Occur.MUST);
                    //bQuery.Add(bQuery1, BooleanClause.Occur.MUST);

                    string title = GetKeyWordsSplitBySpace(comname);
                    Lucene.Net.Search.Query query = new QueryParser(Lucene.Net.Util.Version.LUCENE_29, "Name", PanGuAnalyzer).Parse(title);
                    bQuery.Add(query, BooleanClause.Occur.MUST);
                    dic.Add("title", comname);
                }

                if (bQuery != null && bQuery.GetClauses().Length > 0)
                {
                    serchalllist = GetSearchResult(bQuery, dic, "/Lucene.Net/IndexDic/Gys");
                }
                var serchlist = new List<供应商读出Lucene>();
                int listcount = serchalllist.totalHits > 1000 ? 1000 : serchalllist.totalHits;
                int maxpage = Math.Max((listcount + PAGESIZE - 1) / PAGESIZE, 1);
                if (serchalllist != null && listcount > 0)
                {

                    int length = PAGESIZE;
                    if (maxpage == page && listcount % PAGESIZE != 0)
                        length = listcount % PAGESIZE;

                    //IndexSearcher search = new IndexSearcher(IndexDic("/Lucene.Net/IndexDic/Gys"), true);
                    IndexSearcher search = new IndexSearcher(new Lucene.Net.Store.SimpleFSDirectory(new System.IO.DirectoryInfo(IndexDic("/Lucene.Net/IndexDic/Gys"))), true);

                    for (int i = 0; i < length; i++)
                    {
                        供应商 model = new 供应商();
                        model.Id = long.Parse(search.Doc(serchalllist.scoreDocs[i].doc).Get("NumId"));
                        model.企业基本信息.企业名称 = search.Doc(serchalllist.scoreDocs[i].doc).Get("Name");

                        model.企业基本信息.所属行业 = search.Doc(serchalllist.scoreDocs[i].doc).Get("Industry");

                        ////可提供商品类别
                        //供应商._产品类别 lb = new 供应商._产品类别();
                        //lb.一级分类 = search.Doc(serchalllist.scoreDocs[i].doc).Get("Industry");
                        //model.可提供产品类别列表.Add(lb);

                        model.所属地域.省份 = search.Doc(serchalllist.scoreDocs[i].doc).Get("Province");
                        model.所属地域.城市 = search.Doc(serchalllist.scoreDocs[i].doc).Get("City");
                        model.所属地域.区县 = search.Doc(serchalllist.scoreDocs[i].doc).Get("Area");
                        model.企业联系人信息.联系人固定电话 = search.Doc(serchalllist.scoreDocs[i].doc).Get("Telephone");
                        model.企业联系人信息.联系人姓名 = search.Doc(serchalllist.scoreDocs[i].doc).Get("P_Name");

                        var Rzjb = search.Doc(serchalllist.scoreDocs[i].doc).Get("Rzjb");
                        model.供应商用户信息.认证级别 = Rzjb == null ? 供应商.认证级别.未设置 : (供应商.认证级别)(int.Parse(Rzjb));

                        ///////////////////图标拆分
                        var 图标 = search.Doc(serchalllist.scoreDocs[i].doc).Get("Level_Flage").Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                        if (图标[0] == "1")
                        {
                            model.供应商用户信息.年检列表.Add(DateTime.Now.Year.ToString(), new 操作数据());
                        }
                        model.供应商用户信息.应急供应商 = 图标[1] == "1";
                        model.供应商用户信息.协议供应商 = 图标[2] == "1";
                        model.供应商用户信息.入库级别 = (供应商.入库级别)(int.Parse(图标[3]));
                        ///////////////////图标拆分

                        //员工人数
                        model.企业基本信息.员工人数 = (供应商.员工人数)Enum.Parse(typeof(供应商.员工人数), search.Doc(serchalllist.scoreDocs[i].doc).Get("People_Count"));

                        供应商读出Lucene m = new 供应商读出Lucene();
                        m.供应商 = model;
                        m.登记商品数 = search.Doc(serchalllist.scoreDocs[i].doc).Get("Pro_Count");
                        m.历史参标次数 = search.Doc(serchalllist.scoreDocs[i].doc).Get("History_Count");
                        m.经营类型 = search.Doc(serchalllist.scoreDocs[i].doc).Get("Management");
                        m.主营产品 = search.Doc(serchalllist.scoreDocs[i].doc).Get("Pro_Industry");
                        m.资质证书 = search.Doc(serchalllist.scoreDocs[i].doc).Get("Zzzs_Pic");
                        m.厂房及设备图 = search.Doc(serchalllist.scoreDocs[i].doc).Get("Gys_Pic");


                        //////////////////////////////////////////右边商品图片
                        var pro = search.Doc(serchalllist.scoreDocs[i].doc).Get("Show_Product");
                        if (!string.IsNullOrWhiteSpace(pro))
                        {
                            var prolist = pro.Split(new[] { "||||" }, StringSplitOptions.RemoveEmptyEntries);
                            foreach (var item in prolist)
                            {
                                商品 sp = new 商品();
                                var itemlist = item.Split(new[] { "****" }, StringSplitOptions.RemoveEmptyEntries);
                                sp.商品信息.商品图片.Add(itemlist[0]);
                                sp.商品信息.商品名 = itemlist[1];
                                sp.销售信息.价格 = decimal.Parse(itemlist[2]);
                                sp.Id = long.Parse(itemlist[3]);
                                m.商品列表.Add(sp);
                            }
                        }

                        //判断会员类别
                        var 服务记录 = 供应商服务记录管理.查询供应商服务记录(0, 0, Query<供应商服务记录>.Where(o => o.所属供应商.用户ID == model.Id));
                        if (服务记录.Any())
                        {
                            var 已开通服务 = 服务记录.First().已开通的服务;
                            if (已开通服务.Any())
                            {
                                if (已开通服务.Where(o => o.所申请项目名.Contains("商务会员") && o.结束时间 > DateTime.Now).Any())
                                {
                                    m.会员类别 = "商务会员";
                                }
                                else if (已开通服务.Where(o => o.所申请项目名.Contains("标准会员") && o.结束时间 > DateTime.Now).Any())
                                {
                                    m.会员类别 = "标准会员";
                                }
                                else if (已开通服务.Where(o => o.所申请项目名.Contains("基础会员") && o.结束时间 > DateTime.Now).Any())
                                {
                                    m.会员类别 = "基础会员";
                                }
                            }
                        }
                        //判断会员类别
                        serchlist.Add(SetHighlighter(dic, m));
                    }
                }

                ViewData["供应商列表"] = serchlist;

                //ViewData["listcount"] = listcount;
                //ViewData["pagesize"] = 2;
                ViewData["currentpage"] = page;
                ViewData["pagecount"] = maxpage;
            }
            catch
            {
                ViewData["供应商列表"] = new List<供应商读出Lucene>();

                ViewData["currentpage"] = 1;
                ViewData["pagecount"] = 1;
            }


            ViewBag.Provence = provence;
            ViewBag.City = city;
            ViewBag.Area = area;
            ViewBag.Industry = industry;
            ViewBag.Comname = comname;

            return PartialView("Part_Gys/Part_GysSelect");
        }
        public class 供应商读出Lucene
        {
            public 供应商 供应商 { get; set; }
            public string 登记商品数 { get; set; }
            public string 历史参标次数 { get; set; }
            public string 经营类型 { get; set; }
            public string 厂房及设备图 { get; set; }
            public string 资质证书 { get; set; }
            public string 主营产品 { get; set; }
            public string 会员类别 { get; set; }
            public List<商品> 商品列表 { get; set; }
            public 供应商读出Lucene()
            {
                this.供应商 = new 供应商();
                this.商品列表 = new List<商品>();
            }
        }

        public ActionResult Part_GysSelect_pagechange(int? page)
        {
            if (-1 != HttpContext.检查登录())
            {
                ViewData["已登录"] = "1";
            }
            else
            {
                ViewData["已登录"] = "0";
            }

            TopDocs serchalllist = null;

            PanGu.Segment.Init(PanGuXmlPath);

            string provence = Request.Params["provence"];//所在省份
            string city = Request.Params["city"];//所在城市
            string area = Request.Params["area"];//所在区县
            string industry = Request.Params["industry"];//所属行业
            string comname = Request.Params["comname"];//企业名称

            if (provence == "不限省份" && industry == "请选择行业" && string.IsNullOrEmpty(comname))
            {
                return Content("0");
            }

            try
            {
                BooleanQuery bQuery = new BooleanQuery();
                if (provence != "不限省份")
                {
                    //Lucene.Net.Search.Query query = new QueryParser("Province", PanGuAnalyzer).Parse(provence);
                    Lucene.Net.Search.Query query = new TermQuery(new Term("Province", provence));
                    bQuery.Add(query, BooleanClause.Occur.MUST);//其中的MUST、SHOULD、MUST_NOT表示与、或、非

                    if (city != "不限城市")
                    {
                        //query = new QueryParser("City", PanGuAnalyzer).Parse(city);
                        query = new TermQuery(new Term("City", city));
                        bQuery.Add(query, BooleanClause.Occur.MUST);

                        if (area != "不限区县")
                        {
                            //query = new QueryParser("Area", PanGuAnalyzer).Parse(area);
                            //query = new TermQuery(new Term("Area", area));
                            query = new QueryParser(Lucene.Net.Util.Version.LUCENE_29, "Area", PanGuAnalyzer).Parse(area);
                            bQuery.Add(query, BooleanClause.Occur.MUST);
                        }
                    }
                }


                if (industry != "请选择行业")
                {
                    Lucene.Net.Search.Query query = new QueryParser(Lucene.Net.Util.Version.LUCENE_29, "Industry", PanGuAnalyzer).Parse(industry);
                    //Lucene.Net.Search.Query query = new TermQuery(new Term("Industry", industry));
                    bQuery.Add(query, BooleanClause.Occur.MUST);
                }

                Dictionary<string, string> dic = new Dictionary<string, string>();
                if (!string.IsNullOrEmpty(comname))
                {
                    //bQuery1.Add(query5, BooleanClause.Occur.MUST);
                    //bQuery.Add(bQuery1, BooleanClause.Occur.MUST);

                    string title = GetKeyWordsSplitBySpace(comname);
                    Lucene.Net.Search.Query query = new QueryParser(Lucene.Net.Util.Version.LUCENE_29, "Name", PanGuAnalyzer).Parse(title);
                    bQuery.Add(query, BooleanClause.Occur.MUST);
                    dic.Add("title", comname);
                }

                if (bQuery != null && bQuery.GetClauses().Length > 0)
                {
                    serchalllist = GetSearchResult(bQuery, dic, "/Lucene.Net/IndexDic/Gys");
                }

                int listcount = serchalllist.totalHits > 1000 ? 1000 : serchalllist.totalHits;
                int maxpage = Math.Max((listcount + PAGESIZE - 1) / PAGESIZE, 1);
                if (string.IsNullOrEmpty(page.ToString()) || page < 0 || page > maxpage)
                {
                    page = 1;
                }

                IList<供应商读出Lucene> serchlist = new List<供应商读出Lucene>();
                if (serchalllist != null && listcount > 0)
                {
                    int length = PAGESIZE;
                    if (maxpage == page && listcount % PAGESIZE != 0)
                        length = listcount % PAGESIZE;

                    int count = PAGESIZE * ((int)page - 1);
                    //IndexSearcher search = new IndexSearcher(IndexDic("/Lucene.Net/IndexDic/Gys"), true);
                    IndexSearcher search = new IndexSearcher(new Lucene.Net.Store.SimpleFSDirectory(new System.IO.DirectoryInfo(IndexDic("/Lucene.Net/IndexDic/Gys"))), true);
                    for (int i = count; i < count + length; i++)
                    {
                        供应商 model = new 供应商();
                        model.Id = long.Parse(search.Doc(serchalllist.scoreDocs[i].doc).Get("NumId"));
                        model.企业基本信息.企业名称 = search.Doc(serchalllist.scoreDocs[i].doc).Get("Name");

                        model.企业基本信息.所属行业 = search.Doc(serchalllist.scoreDocs[i].doc).Get("Industry");

                        ////可提供商品类别
                        //供应商._产品类别 lb = new 供应商._产品类别();
                        //lb.一级分类 = search.Doc(serchalllist.scoreDocs[i].doc).Get("Industry");
                        //model.可提供产品类别列表.Add(lb);

                        model.所属地域.省份 = search.Doc(serchalllist.scoreDocs[i].doc).Get("Province");
                        model.所属地域.城市 = search.Doc(serchalllist.scoreDocs[i].doc).Get("City");
                        model.所属地域.区县 = search.Doc(serchalllist.scoreDocs[i].doc).Get("Area");
                        model.企业联系人信息.联系人固定电话 = search.Doc(serchalllist.scoreDocs[i].doc).Get("Telephone");
                        model.企业联系人信息.联系人姓名 = search.Doc(serchalllist.scoreDocs[i].doc).Get("P_Name");


                        var Rzjb = search.Doc(serchalllist.scoreDocs[i].doc).Get("Rzjb");
                        model.供应商用户信息.认证级别 = Rzjb == null ? 供应商.认证级别.未设置 : (供应商.认证级别)(int.Parse(Rzjb));

                        ///////////////////图标拆分
                        var 图标 = search.Doc(serchalllist.scoreDocs[i].doc).Get("Level_Flage").Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                        if (图标[0] == "1")
                        {
                            model.供应商用户信息.年检列表.Add(DateTime.Now.Year.ToString(), new 操作数据());
                        }
                        model.供应商用户信息.应急供应商 = 图标[1] == "1";
                        model.供应商用户信息.协议供应商 = 图标[2] == "1";
                        model.供应商用户信息.入库级别 = (供应商.入库级别)(int.Parse(图标[3]));
                        ///////////////////图标拆分

                        //员工人数
                        model.企业基本信息.员工人数 = (供应商.员工人数)Enum.Parse(typeof(供应商.员工人数), search.Doc(serchalllist.scoreDocs[i].doc).Get("People_Count"));

                        供应商读出Lucene m = new 供应商读出Lucene();
                        m.供应商 = model;
                        m.登记商品数 = search.Doc(serchalllist.scoreDocs[i].doc).Get("Pro_Count");
                        m.历史参标次数 = search.Doc(serchalllist.scoreDocs[i].doc).Get("History_Count");
                        m.经营类型 = search.Doc(serchalllist.scoreDocs[i].doc).Get("Management");
                        m.主营产品 = search.Doc(serchalllist.scoreDocs[i].doc).Get("Pro_Industry");
                        m.资质证书 = search.Doc(serchalllist.scoreDocs[i].doc).Get("Zzzs_Pic");
                        m.厂房及设备图 = search.Doc(serchalllist.scoreDocs[i].doc).Get("Gys_Pic");

                        //////////////////////////////////////////右边商品图片
                        var pro = search.Doc(serchalllist.scoreDocs[i].doc).Get("Show_Product");
                        if (!string.IsNullOrWhiteSpace(pro))
                        {
                            var prolist = pro.Split(new[] { "||||" }, StringSplitOptions.RemoveEmptyEntries);
                            foreach (var item in prolist)
                            {
                                商品 sp = new 商品();
                                var itemlist = item.Split(new[] { "****" }, StringSplitOptions.RemoveEmptyEntries);
                                sp.商品信息.商品图片.Add(itemlist[0]);
                                sp.商品信息.商品名 = itemlist[1];
                                sp.销售信息.价格 = decimal.Parse(itemlist[2]);
                                sp.Id = long.Parse(itemlist[3]);
                                m.商品列表.Add(sp);
                            }
                        }
                        //判断会员类别
                        var 服务记录 = 供应商服务记录管理.查询供应商服务记录(0, 0, Query<供应商服务记录>.Where(o => o.所属供应商.用户ID == model.Id));
                        if (服务记录.Any())
                        {
                            var 已开通服务 = 服务记录.First().已开通的服务;
                            if (已开通服务.Any())
                            {
                                if (已开通服务.Where(o => o.所申请项目名.Contains("商务会员") && o.结束时间 > DateTime.Now).Any())
                                {
                                    m.会员类别 = "商务会员";
                                }
                                else if (已开通服务.Where(o => o.所申请项目名.Contains("标准会员") && o.结束时间 > DateTime.Now).Any())
                                {
                                    m.会员类别 = "标准会员";
                                }
                                else if (已开通服务.Where(o => o.所申请项目名.Contains("基础会员") && o.结束时间 > DateTime.Now).Any())
                                {
                                    m.会员类别 = "基础会员";
                                }
                            }
                        }
                        //判断会员类别
                        serchlist.Add(SetHighlighter(dic, m));
                    }
                }
                ViewData["供应商列表"] = serchlist;

                //ViewData["listcount"] = listcount;
                //ViewData["pagesize"] = 2;
                ViewData["currentpage"] = page;
                ViewData["pagecount"] = maxpage;
            }
            catch
            {
                ViewData["供应商列表"] = new List<供应商读出Lucene>();

                ViewData["currentpage"] = 1;
                ViewData["pagecount"] = 1;
            }


            ViewBag.Provence = provence;
            ViewBag.City = city;
            ViewBag.Area = area;
            ViewBag.Industry = industry;
            ViewBag.Comname = comname;

            return PartialView("Part_Gys/Part_GysSelect_Page");
        }

        public ActionResult Part_HotProduct()//热销商品
        {
            List<供应商> supplierList = new List<供应商>();
            List<long> ids = new List<long>();
            IEnumerable<供应商增值服务申请记录> supplier = 供应商增值服务申请记录管理.查询供应商增值服务申请记录(0, 10, Query<供应商增值服务申请记录>.Where(o => o.所申请项目名 == "企业推广服务A3位置" && o.是否通过 == 通过状态.通过 && o.结束时间 > DateTime.Now));
            if (supplier != null && supplier.Count() != 0)
            {
                foreach (var item in supplier)
                {
                    supplierList.Add(item.所属供应商.用户数据);
                    ids.Add(item.所属供应商.用户ID);
                }
            }
            if (supplierList.Count < 10)
            {
                IEnumerable<供应商> gys = 用户管理.查询用户<供应商>(0, 10 - supplierList.Count, Query<供应商>.Where(
                o => o.审核数据.审核状态 == 审核状态.审核通过
                && o.供应商用户信息.供应商图片.Count > 0
                && o.所属地域.省份 != null
                && o.企业基本信息.所属行业 != null
                    && !o.供应商用户信息.协议供应商
                    && !o.供应商用户信息.应急供应商).And(Query<供应商>.NotIn(o => o.Id, ids)), includeDisabled: false);
                if (gys != null && gys.Count() != 0)
                {
                    supplierList = ((IEnumerable<供应商>)supplierList.Concat(gys)).ToList();
                }
            }

            ViewData["热销商品列表"] = supplierList;




            //ViewData["热销商品列表"] = 用户管理.查询用户<供应商>(0, 5, Query<供应商>.Where(
            //    o => o.审核数据.审核状态 == 审核状态.审核通过
            //    && o.供应商用户信息.供应商图片.Count > 0
            //    && o.所属地域.省份 != null
            //    && o.企业基本信息.所属行业 != null
            //    && (o.供应商用户信息.协议供应商 || o.供应商用户信息.应急供应商)), false, SortBy.Descending("供应商用户信息.点击量"), includeDisabled: false);
            //&& (o.供应商用户信息.已入库 || o.供应商用户信息.协议供应商 || o.供应商用户信息.应急供应商)), false, SortBy.Descending("基本数据.添加时间"), includeDisabled: false);
            return PartialView("Part_Gys/Part_HotProduct");
        }
        public ActionResult Part_GysList() //供应商列表
        {
            if (-1 != HttpContext.检查登录())
            {
                ViewData["已登录"] = "1";
            }
            else
            {
                ViewData["已登录"] = "0";
            }
            ViewData["行业列表"] = 商品分类管理.查找子分类();
            var name = Request.QueryString["name"];
            if (!string.IsNullOrWhiteSpace(name))
            {
                string comname = name;//企业名称
                try
                {
                    TopDocs serchalllist = null;
                    int page = 1;

                    PanGu.Segment.Init(PanGuXmlPath);
                    BooleanQuery bQuery = new BooleanQuery();

                    Dictionary<string, string> dic = new Dictionary<string, string>();
                    if (!string.IsNullOrEmpty(comname))
                    {
                        string title = GetKeyWordsSplitBySpace(comname);
                        Lucene.Net.Search.Query query = new QueryParser(Lucene.Net.Util.Version.LUCENE_29, "Name", PanGuAnalyzer).Parse(title);
                        bQuery.Add(query, BooleanClause.Occur.MUST);
                        dic.Add("title", comname);
                    }

                    if (bQuery != null && bQuery.GetClauses().Length > 0)
                    {
                        serchalllist = GetSearchResult(bQuery, dic, "/Lucene.Net/IndexDic/Gys");
                    }
                    var serchlist = new List<供应商读出Lucene>();
                    int listcount = serchalllist.totalHits > 1000 ? 1000 : serchalllist.totalHits;
                    int maxpage = Math.Max((listcount + PAGESIZE - 1) / PAGESIZE, 1);
                    if (serchalllist != null && listcount > 0)
                    {

                        int length = PAGESIZE;
                        if (maxpage == page && listcount % PAGESIZE != 0)
                            length = listcount % PAGESIZE;

                        //IndexSearcher search = new IndexSearcher(IndexDic("/Lucene.Net/IndexDic/Gys"), true);
                        IndexSearcher search = new IndexSearcher(new Lucene.Net.Store.SimpleFSDirectory(new System.IO.DirectoryInfo(IndexDic("/Lucene.Net/IndexDic/Gys"))), true);

                        for (int i = 0; i < length; i++)
                        {
                            供应商 model = new 供应商();
                            model.Id = long.Parse(search.Doc(serchalllist.scoreDocs[i].doc).Get("NumId"));
                            model.企业基本信息.企业名称 = search.Doc(serchalllist.scoreDocs[i].doc).Get("Name");
                            model.企业基本信息.所属行业 = search.Doc(serchalllist.scoreDocs[i].doc).Get("Industry");
                            model.所属地域.省份 = search.Doc(serchalllist.scoreDocs[i].doc).Get("Province");
                            model.所属地域.城市 = search.Doc(serchalllist.scoreDocs[i].doc).Get("City");
                            model.所属地域.区县 = search.Doc(serchalllist.scoreDocs[i].doc).Get("Area");
                            model.企业联系人信息.联系人固定电话 = search.Doc(serchalllist.scoreDocs[i].doc).Get("Telephone");
                            model.企业联系人信息.联系人姓名 = search.Doc(serchalllist.scoreDocs[i].doc).Get("P_Name");

                            var Rzjb = search.Doc(serchalllist.scoreDocs[i].doc).Get("Rzjb");
                            model.供应商用户信息.认证级别 = Rzjb == null ? 供应商.认证级别.未设置 : (供应商.认证级别)(int.Parse(Rzjb));

                            ///////////////////图标拆分
                            var 图标 = search.Doc(serchalllist.scoreDocs[i].doc).Get("Level_Flage").Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                            if (图标[0] == "1")
                            {
                                model.供应商用户信息.年检列表.Add(DateTime.Now.Year.ToString(), new 操作数据());
                            }
                            model.供应商用户信息.应急供应商 = 图标[1] == "1";
                            model.供应商用户信息.协议供应商 = 图标[2] == "1";
                            model.供应商用户信息.入库级别 = (供应商.入库级别)(int.Parse(图标[3]));
                            ///////////////////图标拆分

                            //员工人数
                            model.企业基本信息.员工人数 = (供应商.员工人数)Enum.Parse(typeof(供应商.员工人数), search.Doc(serchalllist.scoreDocs[i].doc).Get("People_Count"));

                            供应商读出Lucene m = new 供应商读出Lucene();
                            m.供应商 = model;
                            m.登记商品数 = search.Doc(serchalllist.scoreDocs[i].doc).Get("Pro_Count");
                            m.历史参标次数 = search.Doc(serchalllist.scoreDocs[i].doc).Get("History_Count");
                            m.经营类型 = search.Doc(serchalllist.scoreDocs[i].doc).Get("Management");
                            m.主营产品 = search.Doc(serchalllist.scoreDocs[i].doc).Get("Pro_Industry");
                            m.资质证书 = search.Doc(serchalllist.scoreDocs[i].doc).Get("Zzzs_Pic");
                            m.厂房及设备图 = search.Doc(serchalllist.scoreDocs[i].doc).Get("Gys_Pic");


                            //////////////////////////////////////////右边商品图片
                            var pro = search.Doc(serchalllist.scoreDocs[i].doc).Get("Show_Product");
                            if (!string.IsNullOrWhiteSpace(pro))
                            {
                                var prolist = pro.Split(new[] { "||||" }, StringSplitOptions.RemoveEmptyEntries);
                                foreach (var item in prolist)
                                {
                                    商品 sp = new 商品();
                                    var itemlist = item.Split(new[] { "****" }, StringSplitOptions.RemoveEmptyEntries);
                                    sp.商品信息.商品图片.Add(itemlist[0]);
                                    sp.商品信息.商品名 = itemlist[1];
                                    sp.销售信息.价格 = decimal.Parse(itemlist[2]);
                                    sp.Id = long.Parse(itemlist[3]);
                                    m.商品列表.Add(sp);
                                }
                            }
                            //判断会员类别
                            var 服务记录 = 供应商服务记录管理.查询供应商服务记录(0, 0, Query<供应商服务记录>.Where(o => o.所属供应商.用户ID == model.Id));
                            if (服务记录.Any())
                            {
                                var 已开通服务 = 服务记录.First().已开通的服务;
                                if (已开通服务.Any())
                                {
                                    if (已开通服务.Where(o => o.所申请项目名.Contains("商务会员") && o.结束时间 > DateTime.Now).Any())
                                    {
                                        m.会员类别 = "商务会员";
                                    }
                                    else if (已开通服务.Where(o => o.所申请项目名.Contains("标准会员") && o.结束时间 > DateTime.Now).Any())
                                    {
                                        m.会员类别 = "标准会员";
                                    }
                                    else if (已开通服务.Where(o => o.所申请项目名.Contains("基础会员") && o.结束时间 > DateTime.Now).Any())
                                    {
                                        m.会员类别 = "基础会员";
                                    }
                                }
                            }
                            //判断会员类别
                            serchlist.Add(SetHighlighter(dic, m));
                        }
                    }

                    ViewData["供应商列表"] = serchlist;
                    ViewData["currentpage"] = page;
                    ViewData["pagecount"] = maxpage;
                }
                catch
                {
                    ViewData["供应商列表"] = new List<供应商读出Lucene>();

                    ViewData["currentpage"] = 1;
                    ViewData["pagecount"] = 1;
                }

                ViewBag.Provence = "不限省份";
                ViewBag.City = "不限城市";
                ViewBag.Area = "不限区县";
                ViewBag.Industry = "请选择行业";
                ViewBag.Comname = comname;

                return PartialView("Part_Gys/Part_GysList_SearchBox");


            }
            else
            {
                //未付费
                var q = MongoDB.Driver.Builders.Query<供应商>.Where(o => o.审核数据.审核状态 == 审核状态.审核通过);
                long listcount = (int)用户管理.计数用户<供应商>(0, 0, q, false);
                long maxpagesize = Math.Max((listcount + PAGESIZE - 1) / PAGESIZE, 1);
                ViewData["currentpage"] = 1;
                ViewData["pagecount"] = maxpagesize;

                ViewData["供应商列表"] = 用户管理.查询用户<供应商>(0, PAGESIZE, q, false, SortBy<供应商>.Descending(o => o.供应商用户信息.认证级别).Descending(o => o.基本数据.修改时间), false);
                return PartialView("Part_Gys/Part_GysList");
            }



        }
        
        public ActionResult Part_SearchByPage() //供应商列表
        {
            if (-1 != HttpContext.检查登录())
            {
                ViewData["已登录"] = "1";
            }
            else
            {
                ViewData["已登录"] = "0";
            }
            int page = 1;
            int.TryParse(Request.Params["page"], out page);

            var q = Query<供应商>.EQ(o => o.审核数据.审核状态, 审核状态.审核通过);
            long listcount = (int)用户管理.计数用户<供应商>(0, 0, q, false);
            long maxpagesize = Math.Max((listcount + PAGESIZE - 1) / PAGESIZE, 1);
            ViewData["currentpage"] = page;
            ViewData["pagecount"] = maxpagesize;

            ViewData["供应商列表"] = 用户管理.查询用户<供应商>(PAGESIZE * (page - 1), PAGESIZE, q, false, SortBy<供应商>.Descending(o => o.供应商用户信息.认证级别).Descending(o => o.基本数据.修改时间), false);
            return PartialView("Part_Gys/Part_SearchByPage");
        }

        public class Industry
        {
            /// <summary>
            /// 分类Id
            /// </summary>
            public long Id { get; set; }
            /// <summary>
            /// 分类名
            /// </summary>
            public string Name { get; set; }
        }

        public JsonResult 查找行业分类()
        {
            string str = string.Empty;
            string parentCatID = Request.Params["parID"];//一级分类ID
            string ejfnID = Request.Params["ejID"];//二级分类ID
            IEnumerable<商品分类> goodclass = 商品分类管理.查找子分类(long.Parse(parentCatID));
            List<Industry> ind = new List<Industry>();
            if (!ejfnID.IsEmpty())
            {
                //str += "<option>不限</option>";
                foreach (var b in goodclass)
                {
                    if (b.Id == long.Parse(ejfnID))
                    {
                        foreach (var f in b.子分类)
                        {
                            ind.Add(new Industry()
                            {
                                Id = f.Id,
                                Name = f.分类名,
                            });
                            //str += "<option id=" + f.Id + ">" + f.分类名 + "</option>";
                        }
                    }
                }
            }
            else
            {
                //str += "<option>不限</option>";
                foreach (var i in goodclass)
                {
                    ind.Add(new Industry()
                    {
                        Id = i.Id,
                        Name = i.分类名,
                    });
                    //str += "<option id=" + i.Id + ">" + i.分类名 + "</option>";
                }
            }
            JsonResult json = new JsonResult() { Data = ind };
            return Json(json, JsonRequestBehavior.AllowGet);
        }
        public ActionResult Part_CreditRecord(long userId)
        {
            用户基本数据 userBaseData = 用户管理.查找用户(userId);
            return PartialView("Part_Gys/Part_CreditRecord", userBaseData);
        }
        public ActionResult Part_GysContact(long userID)
        {
            用户基本数据 userBaseData = 用户管理.查找用户(userID);
            return PartialView("Part_Gys/Part_GysContact", userBaseData);
        }

        //public string SelectGysByCondition()
        //{
        //    string provence = Request.Params["provence"];//所在省份
        //    string city = Request.Params["area"];//所在城市
        //    string industry = Request.Params["industry"];//所属行业
        //    string comname = Request.Params["comname"];//企业名称
        //    IMongoQuery condition = null;
        //    condition = MongoDB.Driver.Builders.Query.GTE("供应商用户信息.认证级别", 供应商.认证级别.已审核用户);
        //    if (!string.IsNullOrEmpty(provence) && provence != "--请选择省份--")//根据省份查找
        //    {
        //        condition = condition.And(MongoDB.Driver.Builders.Query.EQ("所属地域.省份", provence));
        //    }
        //    if (!string.IsNullOrEmpty(city) && city != "不限" && city != "--请选择城市--")//根据城市查找
        //    {
        //        condition = condition.And(MongoDB.Driver.Builders.Query.EQ("所属地域.城市", city));
        //    }
        //    if (!string.IsNullOrEmpty(industry) && industry != "请选择行业")//根据行业查找
        //    {
        //        condition = condition.And(MongoDB.Driver.Builders.Query.EQ("企业基本信息.所属行业", industry));
        //    }
        //    if (!string.IsNullOrEmpty(comname))//根据所有条件查找或根据名称查找
        //    {
        //        condition = condition.And(MongoDB.Driver.Builders.Query.EQ("企业基本信息.企业名称", comname));
        //    }

        //    string str = string.Empty;
        //    IEnumerable<供应商> userdata = 用户管理.查询用户<供应商>(0, 8, condition);

        //    str += "<table cellpadding='0' cellspacing='0'>";
        //    str += "<tr><th width='5%'>编号</th><th width='20%'>企业名称</th><th width='12%'>所属行业</th><th width='15%'>企业地址</th></tr>";
        //    int i = 0;
        //    foreach (var gys in userdata)
        //    {
        //        //str += "<div class='gyspage_list_content'>";
        //        //if (gys.供应商用户信息.供应商图片.Count > 0)
        //        //{
        //        //    str += "<div class='gyspage_list_content_imgdiv'><a href='/供应商陈列/Supplier_Destination?userID=" + gys.Id + "'><img src='"+gys.供应商用户信息.供应商图片[0]+"' /></a></div>";
        //        //}
        //        //else
        //        //{
        //        //    str += "<div class='gyspage_list_content_imgdiv'><a href='/供应商陈列/Supplier_Destination?userID=" + gys.Id + "'><img src='~/images/gys_default_pic.jpg' /></a></div>";
        //        //}
        //        //str += "<div class='gyspage_list_content_listdiv good_paras'>";
        //        //str += "<table class='table'>";
        //        //str += "<tr><td class='lab'>企业名称</td><td><a href='/供应商陈列/Supplier_Destination?userID="+gys.Id+"'>"+gys.企业基本信息.企业名称+"</a></td><td class='lab'>成立时间</td><td>"+gys.企业基本信息.成立时间+"</td></tr>";
        //        //str += "<tr><td class='lab'>所属行业</td><td>" + gys.企业基本信息.所属行业 + "</td><td class='lab'>企业地址</td><td>" + gys.企业基本信息.注册地址 + "</td></tr>";
        //        //str += "<tr><td class='lab'>联系人</td><td>"+gys.企业联系人信息.联系人姓名+"</td><td class='lab'>信用积分</td><td>"+gys.信用评价信息.总评分+"</td></tr>";
        //        //str += "<tr><td class='lab'>企业简介</td><td class='colspan' colspan='3'><p style='width:640px;height:70px;text-align:left; text-indent:2em;margin:0 auto;'>" + gys.企业基本信息.企业简介 + "</p></td></tr>";
        //        //str += "</table></div></div>";
        //        i++;
        //        str += "<tr><td>" + i + "</td>";
        //        str += "<td><a href='/供应商陈列/Supplier_Destination?id=" + gys.Id + "'>" + gys.企业基本信息.企业名称 + "</a></td>";
        //        str += "<td>" + gys.企业基本信息.所属行业 + "</td>";
        //        str += "<td>" + gys.企业基本信息.注册地址 + "</td></tr>";
        //    }
        //    str += "</table>";
        //    return str;
        //}

        /// <summary>
        /// 获取
        /// </summary>
        /// <param name="bQuery"></param>
        private TopDocs GetSearchResult(BooleanQuery bQuery, Dictionary<string, string> dicKeywords, string indexdic)
        {
            TopDocs docs = null;
            try
            {
                //IndexSearcher search = new IndexSearcher(IndexDic(indexdic), true);
                IndexSearcher search = new IndexSearcher(new Lucene.Net.Store.SimpleFSDirectory(new System.IO.DirectoryInfo(IndexDic(indexdic))), true);
                Stopwatch stopwatch = Stopwatch.StartNew();
                //SortField构造函数第三个字段true为降序,false为升序
                //Sort sort = new Sort(new SortField("Title", SortField.DOC, true));
                //docs.scoreDocs[1].score查看分数,排序后分数全部为NaN
                Sort sort = new Sort(new SortField[] { SortField.FIELD_SCORE, new SortField("Rzjb", SortField.INT, true) });//按照分数倒叙排序,再按照时间倒叙排序
                docs = search.Search(bQuery, (Lucene.Net.Search.Filter)null, 1000, sort);
                stopwatch.Stop();
                //if (docs != null && docs.totalHits > 0)
                //{
                //    for (int i = 0; i < docs.totalHits; i++)
                //    {
                //        供应商 model = new 供应商();
                //        model.Id = long.Parse(search.Doc(docs.scoreDocs[i].doc).Get("NumId"));
                //        model.企业基本信息.企业名称 = search.Doc(docs.scoreDocs[i].doc).Get("Name");
                //        model.企业基本信息.所属行业 = search.Doc(docs.scoreDocs[i].doc).Get("Industry");
                //        model.所属地域.省份 = search.Doc(docs.scoreDocs[i].doc).Get("Province");
                //        model.所属地域.城市 = search.Doc(docs.scoreDocs[i].doc).Get("City");
                //        model.所属地域.区县 = search.Doc(docs.scoreDocs[i].doc).Get("Area");
                //        model.企业联系人信息.联系人固定电话 = search.Doc(docs.scoreDocs[i].doc).Get("Telephone");
                //        list.Add(SetHighlighter(dicKeywords, model));
                //    }
                //}
            }
            catch
            {

            }

            return docs;

        }

        /// <summary>
        /// 处理关键字为索引格式
        /// </summary>
        /// <param name="keywords"></param>
        /// <returns></returns>
        private string GetKeyWordsSplitBySpace(string keywords)
        {
            PanGuTokenizer ktTokenizer = new PanGuTokenizer();
            StringBuilder result = new StringBuilder();
            ICollection<WordInfo> words = ktTokenizer.SegmentToWordInfos(keywords);
            foreach (WordInfo word in words)
            {
                if (word == null)
                {
                    continue;
                }
                result.AppendFormat("{0}^{1}.0 ", word.Word, (int)Math.Pow(3, word.Rank));
            }
            return result.ToString().Trim();
        }

        /// <summary>
        /// 设置关键字高亮
        /// </summary>
        /// <param name="dicKeywords">关键字列表</param>
        /// <param name="model">返回的数据模型</param>
        /// <returns></returns>
        private 供应商读出Lucene SetHighlighter(Dictionary<string, string> dicKeywords, 供应商读出Lucene model)
        {
            SimpleHTMLFormatter simpleHTMLFormatter = new PanGu.HighLight.SimpleHTMLFormatter("<font color=\"red\">", "</font>");
            Highlighter highlighter = new PanGu.HighLight.Highlighter(simpleHTMLFormatter, new Segment());
            highlighter.FragmentSize = 50;
            string strTitle = string.Empty;
            dicKeywords.TryGetValue("title", out strTitle);
            if (!string.IsNullOrEmpty(strTitle))
            {
                string title = highlighter.GetBestFragment(strTitle, model.供应商.企业基本信息.企业名称);
                if (!string.IsNullOrEmpty(title))
                    model.供应商.企业基本信息.企业名称 = title;
            }
            return model;
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
    }
}
