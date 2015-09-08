using System.Linq;
using Go81WebApp.Models.管理器;
using Go81WebApp.Models.数据模型;
using Go81WebApp.Models.数据模型.商品数据模型;
using Go81WebApp.Models.数据模型.用户数据模型;
using MongoDB;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Web.Mvc;
using MongoDB.Driver.Builders;

namespace Go81WebApp.Controllers.门户
{
    
    public class 应急采购Controller : Controller
    {
        private static int PAGESIZE = int.Parse(ConfigurationManager.AppSettings["供应商陈列每页显示条数"]);
        // GET: 应急采购

        public ActionResult Home()
        {
            return View();
        }
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Part_GoodClass()
        {
            IEnumerable<商品分类> goodclass = 商品分类管理.查找子分类();
            return PartialView("Part_Emergency/Part_GoodClass", goodclass);
        }

        public ActionResult Part_Recommend()
        {
            if (-1 != HttpContext.检查登录())
            {
                ViewData["已登录"] = "1";
            }
            else
            {
                ViewData["已登录"] = "0";
            }
            IEnumerable<商品> good = 商品管理.查询商品(0, 5, MongoDB.Driver.Builders.Query.EQ("审核数据.审核状态", 审核状态.审核通过));
            return PartialView("Part_Emergency/Part_Recommend", good);
        }

        public ActionResult GysList()
        {
            return View();
        }

        public ActionResult Part_GysList()
        {
            if (-1 != HttpContext.检查登录())
            {
                ViewData["已登录"] = "1";
            }
            else
            {
                ViewData["已登录"] = "0";
            }
            var q = MongoDB.Driver.Builders.Query<供应商>.EQ(o=>o.审核数据.审核状态, 审核状态.审核通过);
                q = q.And(MongoDB.Driver.Builders.Query.EQ("供应商用户信息.应急供应商", true));
           
            long listcount = (int)用户管理.计数用户<供应商>(0, 0, q,false);
            long maxpagesize = Math.Max((listcount + PAGESIZE - 1) / PAGESIZE, 1);
            ViewData["currentpage"] = 1;
            ViewData["pagecount"] = maxpagesize;

            ViewData["供应商列表"] = 用户管理.查询用户<供应商>(0, PAGESIZE, q, false, SortBy<供应商>.Descending(o => o.供应商用户信息.认证级别).Descending(o => o.基本数据.修改时间), false);
            ViewData["行业列表"] = 商品分类管理.查找子分类();
            return PartialView("Part_Emergency/Part_GysList");
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
            var q = MongoDB.Driver.Builders.Query<供应商>.EQ(o => o.审核数据.审核状态, 审核状态.审核通过);
            q = q.And(MongoDB.Driver.Builders.Query.EQ("供应商用户信息.应急供应商", true));
            int.TryParse(Request.Params["page"], out page);

            string provence = Request.Params["provence"];//所在省份
            string city = Request.Params["city"];//所在城市
            string area = Request.Params["area"];//所在区县
            string industry = Request.Params["industry"];//所属行业
            string comname = Request.Params["comname"];//企业名称
            if(!string.IsNullOrWhiteSpace(city) || !string.IsNullOrWhiteSpace(provence) || !string.IsNullOrWhiteSpace(area) || !string.IsNullOrWhiteSpace(industry) || !string.IsNullOrWhiteSpace(comname))
            { 
                if (provence == "不限省份" && industry == "请选择行业" && string.IsNullOrEmpty(comname))
                {
                    return Content("0");
                }

                if (provence != "不限省份")
                {
                    q = q.And(MongoDB.Driver.Builders.Query.EQ("所属地域.省份", provence));

                    if (city != "不限城市")
                    {
                        q = q.And(MongoDB.Driver.Builders.Query.EQ("所属地域.城市", city));

                        if (area != "不限区县")
                        {
                            q = q.And(MongoDB.Driver.Builders.Query.EQ("所属地域.区县", area));
                        }
                    }
                }

                if (industry != "请选择行业")
                {
                    q = q.And(MongoDB.Driver.Builders.Query<供应商>.Where(
                        o => o.可提供产品类别列表.Any(k => k.一级分类 == industry
                        )));
                }
                if (!string.IsNullOrEmpty(comname))
                {
                    q = q.And(MongoDB.Driver.Builders.Query.Matches("企业基本信息.企业名称", new BsonRegularExpression(string.Format("/{0}/i", comname))));
                }
            }
            long listcount = (int)用户管理.计数用户<供应商>(0, 0, q, false);
            long maxpagesize = Math.Max((listcount + PAGESIZE - 1) / PAGESIZE, 1);
            ViewData["currentpage"] = page;
            ViewData["pagecount"] = maxpagesize;

            ViewData["供应商列表"] = 用户管理.查询用户<供应商>(PAGESIZE * (page - 1), PAGESIZE, q, false, SortBy<供应商>.Descending(o => o.供应商用户信息.认证级别).Descending(o => o.基本数据.修改时间), false);
            return PartialView("Part_Emergency/Part_SearchByPage");
        }


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
            int page = 1;
            var q = MongoDB.Driver.Builders.Query<供应商>.EQ(o=>o.审核数据.审核状态, 审核状态.审核通过);
            q = q.And(MongoDB.Driver.Builders.Query.EQ("供应商用户信息.应急供应商", true));

            string provence = Request.Params["provence"];//所在省份
            string city = Request.Params["city"];//所在城市
            string area = Request.Params["area"];//所在区县
            string industry = Request.Params["industry"];//所属行业
            string comname = Request.Params["comname"];//企业名称
            //string definename = Request.Params["definename"];//自定义行业

            if (provence == "不限省份" && industry == "请选择行业" && string.IsNullOrEmpty(comname))
            {
                return Content("0");
            }

            if (provence != "不限省份")
            {
                q = q.And(MongoDB.Driver.Builders.Query.EQ("所属地域.省份", provence));

                if (city != "不限城市")
                {
                    q = q.And(MongoDB.Driver.Builders.Query.EQ("所属地域.城市", city));

                    if (area != "不限区县")
                    {
                        q = q.And(MongoDB.Driver.Builders.Query.EQ("所属地域.区县", area));
                    }
                }
            }

            if (industry != "请选择行业")
            {
                q = q.And(MongoDB.Driver.Builders.Query<供应商>.Where(
                    o => o.可提供产品类别列表.Any(k => k.一级分类 == industry
                    )));
            }
            if (!string.IsNullOrEmpty(comname))
            {
                q = q.And(MongoDB.Driver.Builders.Query.Matches("企业基本信息.企业名称", new BsonRegularExpression(string.Format("/{0}/i", comname))));
            }
            //if (!string.IsNullOrEmpty(definename))
            //{
            //    q = q.And(MongoDB.Driver.Builders.Query.Matches("企业基本信息.所属行业", new BsonRegularExpression(string.Format("/{0}/i", comname))));
            //}

            long listcount = (int)用户管理.计数用户<供应商>(0, 0, q);
            long maxpagesize = Math.Max((listcount + PAGESIZE - 1) / PAGESIZE, 1);

            ViewData["供应商列表"] = 用户管理.查询用户<供应商>(PAGESIZE * (page - 1), PAGESIZE, q, false, SortBy<供应商>.Descending(o => o.供应商用户信息.认证级别).Descending(o => o.基本数据.修改时间), false);

            ViewData["currentpage"] = page;
            ViewData["pagecount"] = maxpagesize;


            ViewBag.Provence = provence;
            ViewBag.City = city;
            ViewBag.Area = area;
            ViewBag.Industry = industry;
            ViewBag.Comname = comname;

            return PartialView("Part_Emergency/Part_GysSelect");
        }
        //public ActionResult Part_GysSelect_pagechange(int? page)
        //{
        //    var q = MongoDB.Driver.Builders.Query<供应商>.EQ(o=>o.审核数据.审核状态, 审核状态.审核通过);
        //    q = q.And(MongoDB.Driver.Builders.Query.EQ("供应商用户信息.应急供应商", true));
        //    string provence = Request.Params["provence"];//所在省份
        //    string city = Request.Params["city"];//所在城市
        //    string area = Request.Params["area"];//所在区县
        //    string industry = Request.Params["industry"];//所属行业
        //    string comname = Request.Params["comname"];//企业名称
        //    //string definename = Request.Params["definename"];//自定义行业

        //    if (provence == "不限省份" && industry == "请选择行业" && string.IsNullOrEmpty(comname))
        //    {
        //        return Content("0");
        //    }

        //    if (provence != "不限省份")
        //    {
        //        //q = q.And(MongoDB.Driver.Builders.Query<供应商>.Where(o=>o.可提供产品类别列表.Any(k=>k.一级分类 == "")));

        //        //q = q.And(MongoDB.Driver.Builders.Query<供应商>.Where(o => o.可提供产品类别列表.Any(k => k.二级分类.Contains(""))));

        //        q = q.And(MongoDB.Driver.Builders.Query.EQ("所属地域.省份", provence));

        //        if (city != "不限城市")
        //        {
        //            q = q.And(MongoDB.Driver.Builders.Query.EQ("所属地域.城市", city));

        //            if (area != "不限区县")
        //            {
        //                q = q.And(MongoDB.Driver.Builders.Query.EQ("所属地域.区县", area));
        //            }
        //        }
        //    }

        //    if (industry != "请选择行业")
        //    {
        //        q = q.And(MongoDB.Driver.Builders.Query<供应商>.Where(o => o.可提供产品类别列表.Any(k => k.一级分类 == industry)));
        //        //q = q.And(MongoDB.Driver.Builders.Query.EQ("企业基本信息.所属行业", industry));
        //    }
        //    if (!string.IsNullOrEmpty(comname))
        //    {
        //        q = q.And(MongoDB.Driver.Builders.Query.Matches("企业基本信息.企业名称", new BsonRegularExpression(string.Format("/{0}/i", comname))));
        //    }

        //    //if (!string.IsNullOrEmpty(definename))
        //    //{
        //    //    q = q.And(MongoDB.Driver.Builders.Query.Matches("企业基本信息.所属行业", new BsonRegularExpression(string.Format("/{0}/i", comname))));
        //    //}

        //    long listcount = (int)用户管理.计数用户<供应商>(0, 0, q);
        //    long maxpagesize = Math.Max((listcount + PAGESIZE - 1) / PAGESIZE, 1);

        //    ViewData["供应商列表"] = 用户管理.查询用户<供应商>(PAGESIZE * (int.Parse(page.ToString()) - 1), PAGESIZE, q);

        //    ViewData["currentpage"] = page;
        //    ViewData["pagecount"] = maxpagesize;

        //    ViewBag.Provence = provence;
        //    ViewBag.City = city;
        //    ViewBag.Area = area;
        //    ViewBag.Industry = industry;
        //    ViewBag.Comname = comname;

        //    return PartialView("Part_Emergency/Part_GysSelect");
        //}
    }
}