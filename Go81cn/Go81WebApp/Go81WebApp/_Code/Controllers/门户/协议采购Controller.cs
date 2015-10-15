using System.Web.Helpers;
using Go81WebApp.Models.管理器;
using Go81WebApp.Models.数据模型;
using Go81WebApp.Models.数据模型.商品数据模型;
using Go81WebApp.Models.数据模型.用户数据模型;
using MongoDB;
using MongoDB.Bson;
using MongoDB.Driver.Builders;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web.Mvc;

namespace Go81WebApp.Controllers.门户
{
    public class 协议采购Controller : Controller
    {
        private static int PAGESIZE = int.Parse(ConfigurationManager.AppSettings["供应商陈列每页显示条数"]);
        private static int PAGESIZE_AGREEGOOD = int.Parse(ConfigurationManager.AppSettings["协议采购商品每页显示条数"]);

        [静态化(0, 3)]
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult Agreement_Good(string id)
        {
            //try
            //{
            //    if (!string.IsNullOrWhiteSpace(id)&&!string.IsNullOrWhiteSpace(Request.Params["name"]))
            //    {
            //        ViewData["id"] = id;//商品类别id
            //        ViewData["商品名称"] = Request.Params["name"];//商品类别名称
            //        return View();
            //    }
            //    else
            //    {
            //        return Redirect("/协议采购/");
            //    }
            //}
            //catch
            //{
            //    return Redirect("/协议采购/");
            //}
            return View();
        }
        public ActionResult Agreement_Good_Class()
        {
            return View();
        }
        public ActionResult Agreement_Gys()
        {
            return View();
        }
        public ActionResult Part_Agreement_Gys()
        {
            if (-1 != HttpContext.检查登录())
            {
                ViewData["已登录"] = "1";
            }
            else
            {
                ViewData["已登录"] = "0";
            }
            var q = MongoDB.Driver.Builders.Query<供应商>.GTE(o => o.审核数据.审核状态, 审核状态.审核通过);
            q = q.And(MongoDB.Driver.Builders.Query.EQ("供应商用户信息.协议供应商", true));
            //q = q.And(MongoDB.Driver.Builders.Query.EQ("所属地域.城市", "成都市"));
            q = q.And(MongoDB.Driver.Builders.Query<供应商>
                .Where(o => o.供应商用户信息.协议供应商所属地区.Any(oc => oc.省份 == "四川省" && oc.城市 == "成都市")));
            long listcount = (int)用户管理.计数用户<供应商>(0, 0, q);
            long maxpagesize = Math.Max((listcount + PAGESIZE - 1) / PAGESIZE, 1);

            int page = 1;
            ViewData["currentpage"] = page;
            ViewData["pagecount"] = maxpagesize;
            IEnumerable<商品分类> goodtype = 商品分类管理.查找分类(1).子分类;
            ViewData["供应商列表"] = 用户管理.查询用户<供应商>(PAGESIZE * (page - 1), PAGESIZE, q, false, SortBy<供应商>.Descending(o => o.供应商用户信息.认证级别).Descending(o => o.基本数据.修改时间), false);
            ViewData["goodtype"] = goodtype;
            return PartialView("Part_Agreement/Part_Agreement_Gys");
        }
        public ActionResult Part_AgreementgysCondition()
        {
            if (-1 != HttpContext.检查登录())
            {
                ViewData["已登录"] = "1";
            }
            else
            {
                ViewData["已登录"] = "0";
            }
            var province = Request.Params["pro"];
            var city = Request.Params["c"];
            var page = int.Parse(Request.Params["page"]);
            var gys_name = Request.Params["gys"];

            var q = MongoDB.Driver.Builders.Query<供应商>.EQ(o => o.审核数据.审核状态, 审核状态.审核通过);
            q = q.And(MongoDB.Driver.Builders.Query<供应商>.EQ(o => o.供应商用户信息.协议供应商, true));
            q = q.And(MongoDB.Driver.Builders.Query<供应商>
                .Where(o => o.供应商用户信息.协议供应商所属地区.Any(oc => oc.省份 == province && (oc.城市 == city || oc.区县 == city))));

            if (!string.IsNullOrWhiteSpace(gys_name))
            {
                q = q.And(Query<供应商>.Matches(o => o.企业基本信息.企业名称, gys_name));
            }
            long listcount = (int)用户管理.计数用户<供应商>(0, 0, q);
            long maxpagesize = Math.Max((listcount + PAGESIZE - 1) / PAGESIZE, 1);

            if (string.IsNullOrEmpty(page.ToString()) || page < 0 || page > maxpagesize)
            {
                page = 1;
            }
            ViewBag.Provence = province;
            ViewBag.Page = page;
            ViewBag.City = city;
            //ViewData["province"] = province;
            //ViewData["city"] = city;
            //ViewData["page"] = page + 1;
            ViewData["currentpage"] = page;
            ViewData["pagecount"] = maxpagesize;
            IEnumerable<供应商> gys = 用户管理.查询用户<供应商>(PAGESIZE * (page - 1), PAGESIZE, q, false, SortBy<供应商>.Descending(o => o.供应商用户信息.认证级别).Descending(o => o.基本数据.修改时间), false);
            List<供应商> list = gys.ToList();
            ViewData["供应商列表"] = list;
            
            return PartialView("Part_Agreement/Part_AgreementgysCondition");
        }
        public ActionResult Part_GoodClass()
        {
            //var gc = new Dictionary<string,> Dictionary<商品分类, long>();//商品分类，商品分类下商品数量    
            //var _dc = new Dictionary<商品分类, long>();
            //var _uc = new Dictionary<商品分类, long>();
            long id = -1;
            if (!string.IsNullOrWhiteSpace(Request.QueryString["id"]) && Request.QueryString["id"] == "1")
            {
                id = long.Parse(Request.QueryString["id"]);
            }
            ViewData["id"] = id;
            var goodclass = 商品分类管理.查找子分类();
            //foreach (var k in goodclass)
            //{
            //    if (k.分类性质 == 商品分类性质.通用物资)
            //    {
            //        foreach (var p in k.子分类)
            //        {
            //            foreach (var t in p.子分类)
            //            {
            //                var _gnumber = 商品管理.计数分类下商品(t.Id, 0, 0, Query<商品>.EQ(o => o.审核数据.审核状态, 审核状态.审核通过), false);
            //                _dc.Add(t, _gnumber);
            //            }
            //        }
            //    }
            //    else
            //    {
            //        foreach (var h in k.子分类)
            //        {
            //            var _gnum = 商品管理.计数分类下商品(h.Id, 0, 0, Query<商品>.EQ(o => o.审核数据.审核状态, 审核状态.审核通过), false);
            //            _uc.Add(h, _gnum);
            //        }
            //    }
            //}
            //gc.Add("通用物资", _dc);
            //gc.Add("专用物资", _uc);
            return PartialView("Part_Agreement/Part_GoodClass", goodclass);
        }
        public ActionResult Agreement_Good_Details()
        {
            ViewData["分类ID"] = Request.Params["fid"];
            ViewData["商品ID"] = Request.Params["pid"];
            return View();
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
            IEnumerable<商品> good = 商品管理.查询商品(0, 10, Query.EQ("审核数据.审核状态", 审核状态.审核通过));
            return PartialView("Part_Agreement/Part_Recommend", good);
        }
        public ActionResult Part_Agreement_Good(string id) 
        {
            try
            {
                ViewData["id"] = id;//商品类别id
                ViewData["商品名称"] = Request.Params["name"];//商品类别名称

                //商品分类 goodclass = 商品分类管理.查找分类(long.Parse(id)); //查找商品品牌
                IEnumerable<商品> goodclass = 商品管理.查询分类下商品(long.Parse(id), 0, 0, Query<商品>.Where(o => o.采购信息.参与协议采购 && o.审核数据.审核状态 == 审核状态.审核通过));
                List<string> brand = new List<string>();
                foreach (var i in goodclass)
                {
                    if (i.商品信息.品牌 != null)
                    {
                        brand.Add(i.商品信息.品牌.ToLower().Replace(" ", ""));
                    }
                }

                ViewData["品牌"] = brand.Distinct().ToList();
                return PartialView("Part_Agreement/Part_Agreement_Good");
            }
            catch
            {
                return Content("<script>window.location='/协议采购/';</script>");
            }
        }
        public ActionResult Part_Agreement_Good_Class()
        {
            return PartialView("Part_Agreement/Part_Agreement_Good_Class");
        }
        public ActionResult Part_Agreement_Good_Details(string id)
        {
            string fid = Request.Params["fid"];//商品分类id
            商品 god = 商品管理.查找商品(long.Parse(id)); //商品id
            IEnumerable<商品> good = 商品管理.查询分类下商品(long.Parse(fid), 0, 0, Query<商品>.Where(
                    o => o.商品信息.型号 == god.商品信息.型号
                    && o.商品信息.品牌 != null
                    ), false, SortBy.Ascending("销售信息.价格"));
            ViewData["分类ID"] = fid;
            var l = 商品管理.查询历史价格数据(long.Parse(id));
            this.ViewBag.L1 = l.Item1.ToJson().Replace("ISODate", "new Date");
            List<商品> list = new List<商品>();
            List<商品> listgood = new List<商品>();
            List<供应商> listgys = new List<供应商>();
            //foreach (var k in good)
            //{ 
            //    if( k.销售信息.价格==god.销售信息.价格)
            //    { list.Add(k); }
            //}

            foreach (var e in good)
            {
                if (e.商品信息.品牌.Replace(" ", "") == god.商品信息.品牌.Replace(" ", ""))
                { listgood.Add(e); }
            }

            foreach (var s in good)
            {
                if (s.商品信息.所属供应商.用户数据 != null && s.商品信息.所属供应商.用户数据.供应商用户信息.协议供应商)
                {
                    if (s.商品信息.品牌 == god.商品信息.品牌 && s.商品信息.所属供应商.用户数据.所属地域.省份 != null && s.商品信息.所属供应商.用户数据.所属地域.省份.Contains("四川"))
                    {
                        供应商 gys = 用户管理.查找用户(s.商品信息.所属供应商.用户ID) as 供应商;
                        if (listgys == null || listgys.Count == 0)
                        {
                            listgys.Add(gys);
                        }
                        else
                        {
                            bool SameName = false;
                            for (int j = 0; j < listgys.Count; j++)
                            {
                                for (int k = 0; k < listgys.Count; k++)
                                {
                                    if (listgys[k].Id == gys.Id)
                                    {
                                        SameName = true;
                                        break;
                                    }
                                    else
                                    {
                                        SameName = false;
                                    }

                                }
                                if (!SameName)
                                {
                                    listgys.Add(gys);
                                }   
                            }
                        }
                    }
                }
                else
                {
                    continue;
                }
            }
            ViewData["同价位商品"] = list;
            ViewData["同品牌商品"] = listgood;
            ViewData["供应商列表"] = listgys;
            if (Request.Params["come"] != null && Request.Params["come"] == "gys")
            {
                ViewData["链接来源"] = "2";
            }
            else
            {
                ViewData["链接来源"] = "1";
            }

            return PartialView("Part_Agreement/Part_Agreement_Good_Details", god);
        }

        public ActionResult Part_Agreement_Good_Page()
        {
            string provence = Request.Params["provence"];
            string city = Request.Params["city"];
            string price = Request.Params["price"];
            string gys = Request.Params["gys"];
            string id = Request.Params["id"];//商品类别id
            string type = Request.Params["type"];//品牌名称
            // string id = "665";
            // string type = "联想";
            int page = 1;
            if (!string.IsNullOrWhiteSpace(Request.Params["page"]))
            {
                int.TryParse(Request.Params["page"], out page);
            }
            var q = Query<商品>.Where(
                o => o.采购信息.参与协议采购 
                    && o.审核数据.审核状态 == 审核状态.审核通过 
                    && o.商品信息.品牌 != null
                    );

            IEnumerable<BsonValue> p_l = null;
            var q_gys = Query.Null;
            if (!string.IsNullOrEmpty(gys))//根据所有条件查找或根据名称查找
            {
                q_gys = q_gys.And(Query<供应商>.Matches(o => o.企业基本信息.企业名称, gys));
            }

            if (!string.IsNullOrEmpty(provence) && provence != "不限省份")//根据省份查找
            {
                
                q_gys = q_gys.And(Query<供应商>.Where(o=>o.供应商用户信息.协议供应商所属地区.Any(p=>p.省份.Contains(provence))));
                if (!string.IsNullOrEmpty(city) && city != "不限城市")//根据城市查找
                {
                    q_gys = q_gys.And(Query<供应商>.Where(o=>o.供应商用户信息.协议供应商所属地区.Any(p=>p.城市.Contains(city))));
                }
            }
            if (q_gys != null)
            { 
                p_l = 用户管理.列表用户<供应商>(0, 0,
                    Fields<供应商>.Include(o => o.Id),
                    Query<供应商>.EQ(o => o.审核数据.审核状态, 审核状态.审核通过).And(q_gys)
                    ).Select(o => o["_id"]);
            }
            if (!string.IsNullOrEmpty(price))//根据价格查找
            {
            }
            List<商品> good = null;
            if (p_l != null)
            {
                //good = 商品管理.查询分类下商品(long.Parse(id), 0, 0, q.And(Query<商品>.In(o => o.商品信息.所属供应商.用户ID, p_l)), false, SortBy.Ascending("销售信息.价格"));
                good = 商品管理.查询分类下商品(long.Parse(id), 0, 0, q.And(Query<商品>.In(o => o.商品信息.所属供应商.用户ID, p_l))).ToList();
                good.Sort((_1, _2) => _1.销售信息.价格 > _2.销售信息.价格?1:_1.销售信息.价格 < _2.销售信息.价格?-1:0);
            }
            else
            {
                //good = 商品管理.查询分类下商品(long.Parse(id), 0, 0, q, false, SortBy.Ascending("销售信息.价格"));
                good = 商品管理.查询分类下商品(long.Parse(id), 0, 0, q).ToList();
                good.Sort((_1, _2) => _1.销售信息.价格 > _2.销售信息.价格?1:_1.销售信息.价格 < _2.销售信息.价格?-1:0);
            }
            var listagree = new Dictionary<商品, List<string>>();
            
            foreach (var k in good)
            {
                var brand = k.商品信息.品牌 != null ? k.商品信息.品牌.ToLower().Replace(" ", "") : null;
                var t = string.IsNullOrWhiteSpace(type)
                    ? string.IsNullOrWhiteSpace(type)
                    : (!string.IsNullOrWhiteSpace(type) && brand == type);
                if (t)
                {
                    var gh = 商品管理.查询分类下商品(long.Parse(id), 0, 0, q.And(Query<商品>.Where(p => p.商品信息.型号!=null && p.商品信息.型号 == k.商品信息.型号))).ToList();
                    gh.Sort((_1, _2) => _1.销售信息.价格 > _2.销售信息.价格 ? 1 : _1.销售信息.价格 < _2.销售信息.价格 ? -1 : 0);

                    var min_Price = k.销售信息.价格.ToString();
                    var max_Price = k.销售信息.价格.ToString();
                    if (gh.Any())
                    {
                        min_Price = gh.First().销售信息.价格.ToString();
                        max_Price = gh.Last().销售信息.价格.ToString();
                    }
                    var li = new List<string>();
                    li.Add(min_Price);
                    li.Add(max_Price);
                    listagree.Add(k, li);
                }
            }
            long listcount = listagree.Count();
            long maxpagesize = Math.Max((listcount + PAGESIZE_AGREEGOOD - 1) / PAGESIZE_AGREEGOOD, 1);

            ViewData["currentPage"] = page;
            ViewData["pagecount"] = maxpagesize;
            ViewData["商品分类ID"] = id;
            int start_index = PAGESIZE_AGREEGOOD * (page - 1);

            ViewData["分类商品"] = listagree.Skip(start_index).Take(PAGESIZE_AGREEGOOD).ToList();
            return PartialView("Part_Agreement/Part_Agreement_Good_Page");
        }
        public class GysSameArea
        {
            /// <summary>
            /// 商品价格
            /// </summary>
            public decimal Price { get; set; }
            /// <summary>
            /// 库存状态
            /// </summary>
            public string Kczt { get; set; }
            /// <summary>
            /// 销量
            /// </summary>
            public int Sales { get; set; }
            /// <summary>
            /// 供应商ID
            /// </summary>
            public long GysId { get; set; }
            /// <summary>
            /// 企业名称
            /// </summary>
            public string GysName { get; set; }
            /// <summary>
            /// 信用分
            /// </summary>
            public double TotalScore { get; set; }
        }

        public JsonResult GetGysByArea()
        {
            //同一分类\品牌\型号的商品 所拥有的 不同地区的供应商
            string id = Request.Params["id"]; //商品id
            string fnid = Request.Params["fnid"];//商品分类id
            string area = Request.Params["area"]; //地区

            商品 god = 商品管理.查找商品(long.Parse(id));
            IEnumerable<商品> good = 商品管理.查询分类下商品(long.Parse(fnid), 0, 0, Query<商品>.Where(
                    o => o.商品信息.型号 == god.商品信息.型号
                    && o.商品信息.品牌 != null
                    ), false, SortBy.Ascending("销售信息.价格"));
            List<供应商> listgys = new List<供应商>();
            List<GysSameArea> gyssame = new List<GysSameArea>();
            foreach (var s in good)
            {
                if (s.商品信息.所属供应商.用户数据 != null && s.商品信息.所属供应商.用户数据.供应商用户信息.协议供应商)
                {
                    if (s.商品信息.品牌.Replace(" ", "") == god.商品信息.品牌.Replace(" ", "")
                        && ((s.商品信息.所属供应商.用户数据.所属地域.省份 != null && s.商品信息.所属供应商.用户数据.所属地域.省份.Contains(area))
                        || (s.商品信息.所属供应商.用户数据.所属地域.城市 != null && s.商品信息.所属供应商.用户数据.所属地域.城市.Contains(area))
                        ))
                    {
                        供应商 gys = 用户管理.查找用户(s.商品信息.所属供应商.用户ID) as 供应商;
                        if (listgys == null || listgys.Count == 0)
                        {
                            listgys.Add(gys);
                        }
                        else
                        {
                            bool SameName = false;
                            for (int j = 0; j < listgys.Count; j++)
                            {
                                for (int k = 0; k < listgys.Count; k++)
                                {
                                    if (listgys[k].Id == gys.Id)
                                    {
                                        SameName = true;
                                        break;
                                    }
                                    else
                                    {
                                        SameName = false;
                                    }

                                }
                                if (!SameName)
                                {
                                    listgys.Add(gys);
                                }
                            }
                        }
                    }
                }
            }
            for (int r = 0; r < listgys.Count; r++)
            {
                decimal price = 0;
                string kczt = "";
                int sales = 0;
                foreach (var k in good)
                {
                    if (k.商品信息.所属供应商.用户数据 != null)
                    {
                        if (k.商品信息.所属供应商.用户数据.Id == listgys[r].Id)
                        {
                            price = k.销售信息.价格;
                            kczt = k.销售信息.库存状态.ToString();
                            sales = k.销售信息.销量;
                            break;
                        }
                    }
                }
                gyssame.Add(new GysSameArea()
                {
                    GysId = listgys[r].Id,
                    Price = price,
                    Kczt = kczt,
                    GysName = listgys[r].企业基本信息.企业名称,
                    Sales = sales,
                    TotalScore = listgys[r].信用评级信息.积分,
                });
            }
            JsonResult json = new JsonResult() { Data = gyssame };
            return Json(json, JsonRequestBehavior.AllowGet);
        }
    }
}