﻿using System.Collections;
using System.Linq;
using System.Net;
using System.Web.WebSockets;
using Go81WebApp.Models.管理器;
using Go81WebApp.Models.数据模型;
using Go81WebApp.Models.数据模型.商品数据模型;
using Go81WebApp.Models.数据模型.用户数据模型;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.PanGu;
using Lucene.Net.QueryParsers;
using Lucene.Net.Search;
using MongoDB;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using Newtonsoft.Json;
using PanGu;
using PanGu.HighLight;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Text;
using System.Web;
using System.Web.Mvc;
using Go81WebApp.Models.数据模型.推广业务数据模型;
using Go81WebApp.Models.管理器.推广业务管理;
using Go81WebApp.Models.管理器.订单管理;
using Go81WebApp.Models.数据模型.订单数据模型;

namespace Go81WebApp.Controllers.门户
{
    public class 商品陈列Controller : Controller
    {
        private static readonly int PAGESIZE = int.Parse(ConfigurationManager.AppSettings["商品每页显示条数"]);
        private 用户基本数据 currentUser
        {
            get
            {
                return HttpContext.获取当前用户();
            }
        }

        [静态化(0, 3)]
        public ActionResult Index()
        {
            IEnumerable<商品分类> goodclass = 商品分类管理.查找子分类();
            /*ViewData["计算机及通信"] = 商品管理.查询商品(0, 3, Query<商品>.Where(o => o.审核数据.审核状态 == 审核状态.审核通过 && o.商品信息.所属商品分类.商品分类ID == 665));
            ViewData["办公物资"] = 商品管理.查询商品(0, 3, Query<商品>.Where(o => o.审核数据.审核状态 == 审核状态.审核通过 && o.商品信息.所属商品分类.商品分类ID == 3));
            ViewData["建筑装修材料"] = 商品管理.查询商品(0, 3, Query<商品>.Where(o => o.审核数据.审核状态 == 审核状态.审核通过 && o.商品信息.所属商品分类.商品分类ID == 1200));
            ViewData["机电设备"] = 商品管理.查询商品(0, 3, Query<商品>.Where(o => o.审核数据.审核状态 == 审核状态.审核通过 && o.商品信息.所属商品分类.商品分类ID == 328));
            ViewData["仪器仪表"] = 商品管理.查询商品(0, 3, Query<商品>.Where(o => o.审核数据.审核状态 == 审核状态.审核通过 && o.商品信息.所属商品分类.商品分类ID == 864));
            ViewData["医疗设备"] = 商品管理.查询商品(0, 3, Query<商品>.Where(o => o.审核数据.审核状态 == 审核状态.审核通过 && o.商品信息.所属商品分类.商品分类ID == 1029));
            */
            ViewData["商品分类"] = goodclass;
            if (WebApiApplication.IsIntranet)
            {
                return View("IndexIntranet");
            }
            return View(goodclass);
        }
        public ActionResult ProductList()
        {
            return View();
        }

        public ActionResult JcLife()
        {
            var sdate = new DateTime(2015, 11, 8);
            //var edate = new DateTime(2015, 11, 15);
            var date = DateTime.Now;
            if (date >= sdate)
            {
                return RedirectToAction("JcLifeIndex");
            }
            else
            {
                return View();
            }
        }

        public ActionResult JcLifeIndex()
        {
            return View();
        }
        public ActionResult ProductContrast()
        {
            string parms = Request.Params["Contrastparmer"];
            //337|336|
            string[] list = parms.Substring(0, parms.Length - 1).Split('|');
            ViewData["contrasecount"] = list.Length;
            IList<商品> Pro_ListAll = new List<商品>();
            foreach (var str in list)
            {
                商品 Pro_List = 商品管理.查找商品(long.Parse(str));
                Pro_ListAll.Add(Pro_List);
            }
            //.商品信息.所属商品分类.商品分类ID

            ViewData["商品分类属性"] = 商品分类管理.查找分类(Pro_ListAll[0].商品信息.所属商品分类.商品分类ID).商品属性模板;

            ViewData["对比商品集合"] = Pro_ListAll;

            return View();
        }

        public ActionResult Part_ProductSearch()
        {
            if (-1 != HttpContext.检查登录())
            {
                ViewData["已登录"] = "1";
            }
            else
            {
                ViewData["已登录"] = "0";
            }
            try
            {
                //IMongoQuery proQ = null;
                ViewData["筛选条件"] = "";
                ViewData["pagecount"] = 1;
                ViewData["currentpage"] = 1;
                ViewData["keyword"] = "";

                TopDocs serchalllist = null;
                string name = Request.QueryString["name"];
                if (!string.IsNullOrEmpty(name))
                {
                    serchalllist = SearchIndex("/Lucene.Net/IndexDic/Product", name);
                }
                IList<商品查询> serchlist = new List<商品查询>();

                var listcount = serchalllist.totalHits > 1000 ? 1000 : serchalllist.totalHits;
                if (serchalllist != null && listcount > 0)
                {
                    int page = 1;
                    int maxpage = Math.Max((listcount + PAGESIZE - 1) / PAGESIZE, 1);

                    int length = PAGESIZE;
                    if (maxpage == page && listcount % PAGESIZE != 0)
                    {
                        length = listcount % PAGESIZE;
                    }

                    //IndexSearcher search = new IndexSearcher(IndexDic("/Lucene.Net/IndexDic/Product"), true);
                    IndexSearcher search = new IndexSearcher(new Lucene.Net.Store.SimpleFSDirectory(new System.IO.DirectoryInfo(IndexDic("/Lucene.Net/IndexDic/Product"))), true);
                    Dictionary<string, string> dic = new Dictionary<string, string>();
                    dic.Add("Name", name);

                    for (int i = 0; i < length; i++)
                    {
                        商品查询 model = new 商品查询();

                        model.Id = long.Parse(search.Doc(serchalllist.scoreDocs[i].doc).Get("NumId"));
                        model.商品信息.商品名 = search.Doc(serchalllist.scoreDocs[i].doc).Get("Name");
                        model.商品数据.商品简介 = search.Doc(serchalllist.scoreDocs[i].doc).Get("Description");
                        model.商品信息.商品图片.Add(search.Doc(serchalllist.scoreDocs[i].doc).Get("Pic"));
                        model.销售信息.价格 = decimal.Parse(search.Doc(serchalllist.scoreDocs[i].doc).Get("Price"));
                        model.商品信息.所属供应商.用户ID = long.Parse(search.Doc(serchalllist.scoreDocs[i].doc).Get("Company"));
                        model.基本数据.修改时间 = Convert.ToDateTime(search.Doc(serchalllist.scoreDocs[i].doc).Get("AddTime"));

                        //新增按地域查找
                        model.所属地域.省份 = search.Doc(serchalllist.scoreDocs[i].doc).Get("Province");
                        model.所属地域.城市 = search.Doc(serchalllist.scoreDocs[i].doc).Get("City");
                        //新增按地域查找

                        serchlist.Add(SetHighlighter(dic, model));
                    }

                    ViewData["pagecount"] = maxpage;
                    ViewData["currentpage"] = page;
                    ViewData["keyword"] = name;
                }
                ViewData["商品搜索显示列表"] = serchlist;
                if (serchlist.Count > 0)
                {
                    var productclass = 商品分类管理.查找分类(商品管理.查找商品(serchlist[0].Id).商品信息.所属商品分类.商品分类ID);
                    var att = productclass.商品属性模板;
                    ViewData["商品属性模板"] = att;
                    ViewData["价格分段"] = productclass.价格分段;
                }


                //IList<商品> serchlist = new List<商品>();

                //for (int i = 0; i < serchalllist.Count; i++)
                //{
                //    商品 model = new 商品();
                //    model.Id = long.Parse(serchalllist[i].Id.ToString());
                //    model.商品信息.商品名 = serchalllist[i].商品信息.商品名;
                //    model.商品信息.商品图片[0]=

                //    model.商品数据.商品简介 = serchalllist[i].商品数据.商品简介;
                //    model.销售信息.价格 = serchalllist[i].销售信息.价格;
                //    model.商品信息.所属供应商.用户ID = serchalllist[i].商品信息.所属供应商.用户ID;
                //    model.基本数据.修改时间 = Convert.ToDateTime(serchalllist[i].基本数据.添加时间);
                //    serchlist.Add(model);

                //}
                //ViewData["商品搜索显示列表"] = serchalllist;
                //ViewData["listcount"] = serchalllist.Count;
                //ViewData["pagesize"] = PAGESIZE;



                return PartialView("Part_Product/Part_ProductSearch");
            }
            catch
            {
                return Content("<script>alert('暂无搜索结果，将前往商品库进行查找！');window.location='/商品陈列/';</script>");
            }
        }

        /// <summary>
        /// 输入商品关键词，并选择筛选条件后的方法，page=1
        /// </summary>
        /// <returns></returns>
        public ActionResult ChangeProductClassByKeyword()
        {

            try
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

                //输入的商品关键词
                string name = Request.QueryString["keyword"];
                //筛选的条件字符串
                var condition = Request.QueryString["condition"];

                //回传参数设定默认值，防止为空
                ViewData["商品搜索显示列表"] = new List<商品查询>();
                ViewData["pagecount"] = 1;
                ViewData["currentpage"] = 1;
                ViewData["keyword"] = name;
                ViewData["筛选条件"] = condition;
                //回传参数设定默认值，防止为空

                if (!string.IsNullOrEmpty(name))
                {
                    serchalllist = SearchIndex("/Lucene.Net/IndexDic/Product", name);
                }
                var listcount = serchalllist.totalHits > 1000 ? 1000 : serchalllist.totalHits;
                if (serchalllist != null && listcount > 0)
                {
                    //满足关键词和筛选条件临时商品集合
                    IList<商品查询> serchlist_temp = new List<商品查询>();

                    //索引处理
                    IndexSearcher search = new IndexSearcher(new Lucene.Net.Store.SimpleFSDirectory(new System.IO.DirectoryInfo(IndexDic("/Lucene.Net/IndexDic/Product"))), true);
                    Dictionary<string, string> dic = new Dictionary<string, string>();
                    dic.Add("Name", name);

                    //先得出满足关键字的商品
                    for (int i = 0; i < listcount && i < 1000; i++)
                    {
                        商品查询 model = new 商品查询();
                        model.Id = long.Parse(search.Doc(serchalllist.scoreDocs[i].doc).Get("NumId"));
                        model.商品信息.商品名 = search.Doc(serchalllist.scoreDocs[i].doc).Get("Name");
                        model.商品数据.商品简介 = search.Doc(serchalllist.scoreDocs[i].doc).Get("Description");
                        model.商品信息.商品图片.Add(search.Doc(serchalllist.scoreDocs[i].doc).Get("Pic"));
                        model.销售信息.价格 = decimal.Parse(search.Doc(serchalllist.scoreDocs[i].doc).Get("Price"));
                        model.商品信息.所属供应商.用户ID = long.Parse(search.Doc(serchalllist.scoreDocs[i].doc).Get("Company"));
                        model.基本数据.修改时间 = Convert.ToDateTime(search.Doc(serchalllist.scoreDocs[i].doc).Get("AddTime"));
                        model.商品数据.商品属性 = (null != search.Doc(serchalllist.scoreDocs[i].doc).Get("Attribute"))
                            ? JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, List<string>>>>(
                                search.Doc(serchalllist.scoreDocs[i].doc).Get("Attribute"))
                            : null;

                        //新增按地域查找
                        model.所属地域.省份 = search.Doc(serchalllist.scoreDocs[i].doc).Get("Province");
                        model.所属地域.城市 = search.Doc(serchalllist.scoreDocs[i].doc).Get("City");
                        //新增按地域查找

                        //满足关键词的商品集合
                        serchlist_temp.Add(SetHighlighter(dic, model));
                    }

                    //进行条件筛选
                    if (!string.IsNullOrEmpty(condition)) //判断筛选条件
                    {
                        var pvs = condition.Split(new char[] { '$' }, StringSplitOptions.RemoveEmptyEntries);
                        foreach (var item in pvs)
                        {
                            var pv = item.Split('|');

                            //满足筛选条件的商品集合
                            serchlist_temp = serchlist_temp.Where(o =>
                            {
                                var ppp = o.商品数据.商品属性;
                                return ppp.ContainsKey(pv[0]) &&
                                       ppp[pv[0]].ContainsKey(pv[1]) &&
                                       ppp[pv[0]][pv[1]].Contains(pv[2]);
                            }).ToList();
                        }
                    }

                    //新增地域价格查找参数
                    var province = Request.QueryString["province"];
                    var city = Request.QueryString["city"];
                    var price = Request.QueryString["price"];
                    //新增地域价格查找参数
                    //////////////////////////////////////////////////////////////////////新增按价格查询
                    if (!string.IsNullOrWhiteSpace(price))
                    {
                        if (price.Contains("及以上"))
                        {
                            price = price.Replace("及以上", "");
                            serchlist_temp = serchlist_temp.Where(o =>
                                {
                                    var p_price = o.销售信息.价格;
                                    return p_price >= decimal.Parse(price);
                                }).ToList();
                        }
                        else
                        {
                            var p = price.Split('-');
                            serchlist_temp = serchlist_temp.Where(o =>
                            {
                                var p_price = o.销售信息.价格;
                                return p_price >= decimal.Parse(p[0]) && p_price <= decimal.Parse(p[1]);
                            }).ToList();
                        }
                    }
                    //////////////////////////////////////////////////////////////////////新增按价格查询
                    //////////////////////////////////////////////////////////////////////新增按地域查询
                    if (!string.IsNullOrWhiteSpace(province))
                    {
                        serchlist_temp = serchlist_temp.Where(o =>
                        {
                            var pro = o.所属地域.省份;
                            return pro == province;
                        }).ToList();

                        if (!string.IsNullOrWhiteSpace(city))
                        {
                            serchlist_temp = serchlist_temp.Where(o =>
                            {
                                var p_city = o.所属地域.城市;
                                return p_city == city;
                            }).ToList();
                        }
                    }
                    //////////////////////////////////////////////////////////////////////新增按地域查询

                    //对筛选后的集合进行分页处理
                    if (serchlist_temp.Any())
                    {
                        int page = 1;
                        int maxpage = Math.Max((serchlist_temp.Count + PAGESIZE - 1) / PAGESIZE, 1);

                        int length = PAGESIZE;
                        if (maxpage == page && serchlist_temp.Count % PAGESIZE != 0)
                        {
                            length = serchlist_temp.Count % PAGESIZE;
                        }

                        //进行分页处理后的数据
                        IList<商品查询> resultlist = new List<商品查询>();
                        for (var i = 0; i < length; i++)
                        {
                            resultlist.Add(serchlist_temp[i]);
                        }
                        //回传参数更改值
                        ViewData["商品搜索显示列表"] = resultlist;
                        ViewData["pagecount"] = maxpage;
                        ViewData["currentpage"] = page;
                        ViewData["keyword"] = name;
                        ViewData["筛选条件"] = condition;
                    }
                }
                return PartialView("Part_Product/Part_ChangeProductClassByKeyword");
            }
            catch
            {
                return Content("<script>alert('暂无搜索结果，将前往商品库进行查找！');window.location='/商品陈列/';</script>");
            }
        }
        public int AddGoodInfo()
        {
            try
            {
                string count = Request.QueryString["num"];
                string id = Request.QueryString["id"];

                //如果用户登录且为个人用户就将数据写入数据库
                if (HttpContext.检查登录() != -1 && currentUser.GetType() == typeof(个人用户))
                {
                    var shopcar = 购物车管理.查询购物车(0, 0, Query<购物车>.Where(o => o.所属用户.用户ID == currentUser.Id));
                    var sp = new 购物车.选购商品();
                    sp.商品.商品ID = long.Parse(id);
                    sp.数量 = int.Parse(count);
                    //判断数据库有该用户的购物车
                    if (shopcar.Any())
                    {
                        var df = shopcar.First();
                        df.选购商品列表.Add(sp);
                        购物车管理.更新购物车(df);
                    }
                    else
                    {
                        var car = new 购物车();
                        car.所属用户.用户ID = currentUser.Id;
                        car.选购商品列表.Add(sp);
                        购物车管理.添加购物车(car);
                    }
                }
                if (Session["Ginfo"] != null)
                {
                    string ginfo = Session["Ginfo"].ToString();
                    if (!ginfo.Contains(id))
                    {
                        Session["Ginfo"] += id + "," + count + "|";
                    }
                }
                else
                {
                    Session["Ginfo"] += id + "," + count + "|";
                }

                return 1;
            }
            catch
            {
                return -1;
            }
        }
        public int DelPurchaseInfo()
        {
            try
            {
                string str = "";
                if (Session["Ginfo"] != null)
                {
                    string id = Request.QueryString["id"];
                    string ginfo = Session["Ginfo"].ToString();
                    string[] infos = ginfo.Split('|');
                    for (int i = 0; i < infos.Length - 1; i++)
                    {
                        if (!infos[i].Contains(id))
                        {
                            str += infos[i] + "|";
                        }
                    }
                    Session["Ginfo"] = str;
                }
                return 1;
            }
            catch
            {
                return -1;
            }
        }
        public long CheckLogin()
        {
            long id = HttpContext.检查登录();
            return id;
        }
        [HttpPost]
        public ActionResult AddPurchaseInfo()
        {
            if (HttpContext.检查登录() != -1 && currentUser.GetType() == typeof(个人用户))
            {
                string str = Request.Form["summary"];
                string[] info = str.Split('|');
                string tip1 = "";
                bool exist = false;
                bool IsChengdu = false;
                for (int i = 0; i < info.Length - 1; i++)//限数量判断
                {
                    long id = long.Parse(info[i].Split(',')[1]);
                    int sum = int.Parse(info[i].Split(',')[0]);
                    if (id == 9147 && sum > 9)
                    {
                        exist = true;
                        tip1 += "订单中存在限购9个的商品";
                        break;
                    }
                }
                for (int i = 0; i < info.Length - 1; i++)//限成都市判断
                {
                    long id = long.Parse(info[i].Split(',')[1]);
                    int sum = int.Parse(info[i].Split(',')[0]);
                    商品 sp = 商品管理.查找商品(long.Parse(info[i].Split(',')[1]));
                    if (sp.销售信息.销售地域.城市 == "成都市" && sp.销售信息.销售地域.城市 != Request.Form["city"])
                    {
                        IsChengdu = true;
                        tip1 += "订单中存在仅限于成都市内供货的商品";
                        break;
                    }
                }
                if (!exist && !IsChengdu)
                {
                    var shopcar = 购物车管理.查询购物车(0, 0, Query<购物车>.Where(o => o.所属用户.用户ID == currentUser.Id));
                    var car = shopcar.First();
                    var shoplist = shopcar.Any() ? car.选购商品列表 : new List<购物车.选购商品>();

                    decimal weight = 0;
                    decimal sum = 0;//使用优惠码前的总价格
                    decimal sumwithcode = 0;//使用优惠码后的总价格
                    订单 order = new 订单();
                    order.收货地址.省份 = Request.Form["province"];
                    order.收货地址.城市 = Request.Form["city"];
                    order.收货地址.区县 = Request.Form["area"];
                    order.联系人 = Request.Form["contactMan"];
                    order.联系电话 = Request.Form["phone"];
                    order.详细地址 = Request.Form["detail"];
                    order.订单所属用户.用户ID = currentUser.Id;
                    for (int i = 0; i < info.Length - 1; i++)
                    {
                        商品 sp = 商品管理.查找商品(long.Parse(info[i].Split(',')[1]));
                        商品订单 o = new 商品订单();
                        o.商品.商品ID = long.Parse(info[i].Split(',')[1]);
                        o.数量 = int.Parse(info[i].Split(',')[0]);
                        sum += int.Parse(info[i].Split(',')[0]) * sp.销售信息.价格;
                        sumwithcode += int.Parse(info[i].Split(',')[0]) * sp.销售信息.军采价;
                        order.商品订单列表.Add(o);
                        weight += (decimal)(sp.商品信息.单位重量 * int.Parse(info[i].Split(',')[0]));
                        //该商品已下单，从购物车里删除
                        var gd = shoplist.Find(p => p.商品.商品ID == long.Parse(info[i].Split(',')[1]));
                        if (gd != null)
                        {
                            shoplist.Remove(gd);
                        }
                    }
                    order.订单商品总价格 = sum;
                    if (sumwithcode < 168)
                    {
                        if (Request.Form["province"] != "新疆维吾尔自治区" && Request.Form["province"] != "西藏自治区" && Request.Form["province"] != "海南省")
                        {
                            order.总运费 = 15;
                        }
                        else
                        {
                            order.总运费 = 15 + 15 * Math.Ceiling(weight);
                        }
                    }
                    else
                    {
                        if (Request.Form["province"] == "新疆维吾尔自治区" || Request.Form["province"] == "西藏自治区" || Request.Form["province"] == "海南省")
                        {
                            order.总运费 = 15 + 15 * Math.Ceiling(weight);
                        }
                    }
                    order.订单总付款 = sumwithcode + order.总运费;
                    order.订单总价格 = sum + order.总运费;
                    购物车管理.更新购物车(car);
                    订单管理.添加订单(order);
                    Session["Ginfo"] = "";
                    return Content("<script>alert('您已成功提交订单，可以到后台去支付订单');window.location='/个人用户后台/PurchaseInfo';</script>");
                }
                else
                {
                    return Content("<script>alert('" + tip1 + "，不能下订单');window.location='/个人用户后台/ShopCar';</script>");
                }
            }
            else
            {
                return Content("<script>alert('目前购物功能只对个人用户开放！如果你不是个人用户，请先注册个人用户账号。');window.location='/注册/Register_Person';</script>");
            }
        }
        public ActionResult Login()
        {
            string temp_session = "";
            if (Session["Ginfo"] != null)
            {
                temp_session = Session["Ginfo"].ToString();
            }
            string uname = Request.Form["uname"];
            string upwd = Request.Form["upwd"];
            var u = this.HttpContext.登录(uname, upwd, false);
            if (u != null)
            {
                Session["Ginfo"] = temp_session;
                return Redirect("/商品陈列/PurchaseInfo");
            }
            else
            {
                Session["Ginfo"] = temp_session;
                return Redirect("/登录/Login");
            }
        }
        public ActionResult PurchaseInfo()
        {
            var ninfo = new Dictionary<long, int>();
            var sp = new 购物车.选购商品();
            var listsp = new List<购物车.选购商品>();

            if (Session["Ginfo"] != null)
            {
                var ginfo = Session["Ginfo"].ToString();
                string[] info = ginfo.Split('|');
                for (int i = 0; i < info.Length - 1; i++)
                {
                    sp.商品.商品ID = long.Parse(info[i].Split(',')[0]);
                    sp.数量 = int.Parse(info[i].Split(',')[1]);
                    listsp.Add(sp);
                    ninfo.Add(long.Parse(info[i].Split(',')[0]), int.Parse(info[i].Split(',')[1]));
                }
            }

            //如果用户登录且为个人用户就将数据写入数据库
            if (HttpContext.检查登录() != -1 && currentUser.GetType() == typeof(个人用户))
            {
                var shopcar = 购物车管理.查询购物车(0, 0, Query<购物车>.Where(o => o.所属用户.用户ID == currentUser.Id));

                //判断数据库有该用户的购物车
                if (shopcar.Any())
                {
                    shopcar.First().选购商品列表 = shopcar.First().选购商品列表.Concat(listsp).ToList();
                    购物车管理.更新购物车(shopcar.First());
                }
                else
                {
                    var car = new 购物车();
                    car.所属用户.用户ID = currentUser.Id;
                    car.选购商品列表 = car.选购商品列表.Concat(listsp).ToList();
                    购物车管理.添加购物车(car);
                }
            }

            return View(ninfo);
        }
        /// <summary>
        /// 输入商品关键词，并选择筛选条件后的方法，page=1
        /// </summary>
        /// <returns></returns>
        public ActionResult ChangeProductClassByKeyword_p()
        {

            try
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

                //输入的商品关键词
                string name = Request.QueryString["keyword"];
                //筛选的条件字符串
                var condition = Request.QueryString["condition"];

                var page = int.Parse(Request.QueryString["page"]);

                //回传参数设定默认值，防止为空
                ViewData["商品搜索显示列表"] = new List<商品查询>();
                ViewData["pagecount"] = 1;
                ViewData["currentpage"] = 1;
                ViewData["keyword"] = name;
                ViewData["筛选条件"] = condition;
                //回传参数设定默认值，防止为空

                if (!string.IsNullOrEmpty(name))
                {
                    serchalllist = SearchIndex("/Lucene.Net/IndexDic/Product", name);
                }
                var listcount = serchalllist.totalHits > 1000 ? 1000 : serchalllist.totalHits;
                if (serchalllist != null && listcount > 0)
                {
                    //满足关键词和筛选条件临时商品集合
                    IList<商品查询> serchlist_temp = new List<商品查询>();

                    //索引处理
                    IndexSearcher search = new IndexSearcher(new Lucene.Net.Store.SimpleFSDirectory(new System.IO.DirectoryInfo(IndexDic("/Lucene.Net/IndexDic/Product"))), true);
                    Dictionary<string, string> dic = new Dictionary<string, string>();
                    dic.Add("Name", name);

                    //先得出满足关键字的商品
                    for (int i = 0; i < listcount && i < 1000; i++)
                    {
                        商品查询 model = new 商品查询();
                        model.Id = long.Parse(search.Doc(serchalllist.scoreDocs[i].doc).Get("NumId"));
                        model.商品信息.商品名 = search.Doc(serchalllist.scoreDocs[i].doc).Get("Name");
                        model.商品数据.商品简介 = search.Doc(serchalllist.scoreDocs[i].doc).Get("Description");
                        model.商品信息.商品图片.Add(search.Doc(serchalllist.scoreDocs[i].doc).Get("Pic"));
                        model.销售信息.价格 = decimal.Parse(search.Doc(serchalllist.scoreDocs[i].doc).Get("Price"));
                        model.商品信息.所属供应商.用户ID = long.Parse(search.Doc(serchalllist.scoreDocs[i].doc).Get("Company"));
                        model.基本数据.修改时间 = Convert.ToDateTime(search.Doc(serchalllist.scoreDocs[i].doc).Get("AddTime"));

                        model.商品数据.商品属性 = (null != search.Doc(serchalllist.scoreDocs[i].doc).Get("Attribute"))
                            ? JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, List<string>>>>(
                                search.Doc(serchalllist.scoreDocs[i].doc).Get("Attribute"))
                            : null;

                        //新增按地域查找
                        model.所属地域.省份 = search.Doc(serchalllist.scoreDocs[i].doc).Get("Province");
                        model.所属地域.城市 = search.Doc(serchalllist.scoreDocs[i].doc).Get("City");
                        //新增按地域查找

                        //满足关键词的商品集合
                        serchlist_temp.Add(SetHighlighter(dic, model));
                    }

                    //进行条件筛选
                    if (!string.IsNullOrEmpty(condition)) //判断筛选条件
                    {
                        var pvs = condition.Split(new char[] { '$' }, StringSplitOptions.RemoveEmptyEntries);
                        foreach (var item in pvs)
                        {
                            var pv = item.Split('|');

                            //满足筛选条件的商品集合
                            serchlist_temp = serchlist_temp.Where(o =>
                            {
                                var ppp = o.商品数据.商品属性;
                                return ppp.ContainsKey(pv[0]) &&
                                       ppp[pv[0]].ContainsKey(pv[1]) &&
                                       ppp[pv[0]][pv[1]].Contains(pv[2]);
                            }).ToList();
                        }
                    }

                    //新增地域价格查找参数
                    var province = Request.QueryString["province"];
                    var city = Request.QueryString["city"];
                    var price = Request.QueryString["price"];
                    //新增地域价格查找参数
                    //////////////////////////////////////////////////////////////////////新增按价格查询
                    if (!string.IsNullOrWhiteSpace(price))
                    {
                        if (price.Contains("及以上"))
                        {
                            price = price.Replace("及以上", "");
                            serchlist_temp = serchlist_temp.Where(o =>
                            {
                                var p_price = o.销售信息.价格;
                                return p_price >= decimal.Parse(price);
                            }).ToList();
                        }
                        else
                        {
                            var p = price.Split('-');
                            serchlist_temp = serchlist_temp.Where(o =>
                            {
                                var p_price = o.销售信息.价格;
                                return p_price >= decimal.Parse(p[0]) && p_price <= decimal.Parse(p[1]);
                            }).ToList();
                        }
                    }
                    //////////////////////////////////////////////////////////////////////新增按价格查询
                    //////////////////////////////////////////////////////////////////////新增按地域查询
                    if (!string.IsNullOrWhiteSpace(province))
                    {
                        serchlist_temp = serchlist_temp.Where(o =>
                        {
                            var pro = o.所属地域.省份;
                            return pro == province;
                        }).ToList();

                        if (!string.IsNullOrWhiteSpace(city))
                        {
                            serchlist_temp = serchlist_temp.Where(o =>
                            {
                                var p_city = o.所属地域.城市;
                                return p_city == city;
                            }).ToList();
                        }
                    }
                    //////////////////////////////////////////////////////////////////////新增按地域查询

                    //对筛选后的集合进行分页处理
                    if (serchlist_temp.Any())
                    {
                        int maxpage = Math.Max((serchlist_temp.Count + PAGESIZE - 1) / PAGESIZE, 1);

                        int length = PAGESIZE;
                        if (maxpage == page && serchlist_temp.Count % PAGESIZE != 0)
                        {
                            length = serchlist_temp.Count % PAGESIZE;
                        }

                        //进行分页处理后的数据
                        IList<商品查询> resultlist = new List<商品查询>();

                        //该页的起始索引
                        var count = (page - 1) * PAGESIZE;
                        for (var i = count; i < count + length; i++)
                        {
                            resultlist.Add(serchlist_temp[i]);
                        }

                        //回传参数更改值
                        ViewData["商品搜索显示列表"] = resultlist;
                        ViewData["pagecount"] = maxpage;
                        ViewData["currentpage"] = page;
                        ViewData["keyword"] = name;
                        ViewData["筛选条件"] = condition;
                    }
                }
                return PartialView("Part_Product/Part_ChangeProductClassByKeyword");
            }
            catch
            {
                return Content("<script>alert('暂无搜索结果，将前往商品库进行查找！');window.location='/商品陈列/';</script>");
            }
        }



        public ActionResult ProductSearch()
        {
            return View();
        }
        public ActionResult Product_Detail()
        {
            try
            {
                if (-1 != HttpContext.检查登录())
                {
                    ViewData["已登录"] = "1";
                }
                else
                {
                    ViewData["已登录"] = "0";
                }
                long id = long.Parse(Request.QueryString["id"]);
                商品 pro_detail = 商品管理.查找商品(id);

                //防止未审核通过的商品出现在前台中
                //pro_detail.审核数据.审核状态 != 审核状态.审核通过 || 用户管理.查找用户<供应商>(pro_detail.商品信息.所属供应商.用户ID, false).审核数据.审核状态 != 审核状态.审核通过
                if (pro_detail.审核数据.审核状态 != 审核状态.审核通过)
                {
                    pro_detail = null;
                }
                if (pro_detail != null)
                {
                    商品管理.增加浏览量(pro_detail.Id);
                    ViewBag.data = (用户管理.查找用户<供应商>(pro_detail.商品信息.所属供应商.用户ID)).信用评级信息.等级;
                    if (pro_detail.商品信息.商品图片.Count == 0)
                    {
                        pro_detail.商品信息.商品图片.Add("/images/noimage.jpg");
                    }
                    var l = 商品管理.查询历史价格数据((long)id);
                    this.ViewBag.L1 = l.Item1.ToJson().Replace("ISODate", "new Date");
                    return View(pro_detail);
                }
                else
                {
                    return Content("<script>window.location='/商品陈列/';</script>");
                }

            }
            catch
            {
                return Content("<script>window.location='/商品陈列/';</script>");
            }

        }
        public ActionResult Product_Inmall()
        {
            try
            {
                if (-1 != HttpContext.检查登录())
                {
                    ViewData["已登录"] = "1";
                }
                else
                {
                    ViewData["已登录"] = "0";
                }
                long id = long.Parse(Request.QueryString["id"]);
                商品 pro_detail = 商品管理.查找商品(id);

                //防止未审核通过的商品出现在前台中
                //pro_detail.审核数据.审核状态 != 审核状态.审核通过 || 用户管理.查找用户<供应商>(pro_detail.商品信息.所属供应商.用户ID, false).审核数据.审核状态 != 审核状态.审核通过
                if (pro_detail.审核数据.审核状态 != 审核状态.审核通过)
                {
                    pro_detail = null;
                }
                if (pro_detail != null)
                {
                    商品管理.增加浏览量(pro_detail.Id);
                    ViewBag.data = (用户管理.查找用户<供应商>(pro_detail.商品信息.所属供应商.用户ID)).信用评级信息.等级;
                    if (pro_detail.商品信息.商品图片.Count == 0)
                    {
                        pro_detail.商品信息.商品图片.Add("/images/noimage.jpg");
                    }
                    var l = 商品管理.查询历史价格数据((long)id);
                    this.ViewBag.L1 = l.Item1.ToJson().Replace("ISODate", "new Date");
                    return View(pro_detail);
                }
                else
                {
                    return Content("<script>window.location='/商品陈列/';</script>");
                }

            }
            catch
            {
                return Content("<script>window.location='/商品陈列/';</script>");
            }

        }
        public class SupplierInfo
        {
            public long Id { get; set; }
            public string Sname { get; set; }
            public string Price { get; set; }
        }
        public ActionResult SameTypeResult()
        {
            try
            {
                int cpage = 1;
                int pgCount = 0;
                if (!string.IsNullOrWhiteSpace(Request.QueryString["page"]))
                {
                    cpage = int.Parse(Request.QueryString["page"]);
                }
                string type = Request.QueryString["tp"];
                List<SupplierInfo> sp = new List<SupplierInfo>();
                IEnumerable<商品> goods = 商品管理.查询商品(0, 0, Query<商品>.Where(m => m.商品信息.精确型号 == type && m.审核数据.审核状态 == 审核状态.审核通过)).OrderBy(m => m.销售信息.价格);
                foreach (var item in goods)
                {
                    if (item.商品信息 != null)
                    {
                        SupplierInfo s = new SupplierInfo();
                        s.Id = item.商品信息.所属供应商.用户ID;
                        s.Price = item.销售信息.价格.ToString("0.00");
                        if (item.商品信息.所属供应商.用户数据 != null)
                        {
                            s.Sname = item.商品信息.所属供应商.用户数据.企业基本信息.企业名称;
                        }
                        else
                        {
                            s.Sname = "信息未完善的供应商";
                        }
                        sp.Add(s);
                    }
                }
                pgCount = sp.Count / 5;
                if (sp.Count % 5 > 0)
                {
                    pgCount++;
                }
                sp = sp.Skip((cpage - 1) * 5).Take(5).ToList();
                JsonResult json = new JsonResult() { Data = new { supplier = sp, pCount = pgCount } };
                return Json(json, JsonRequestBehavior.AllowGet);
            }
            catch
            {
                JsonResult json = new JsonResult() { Data = new { supplier = new List<SupplierInfo>(), pCount = 0 } };
                return Json(json, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult showOtherGys()
        {
            var n = Request.Params["n"];
            if (string.IsNullOrWhiteSpace(n))
            {
                return Content("0");
            }

            var p = Request.Params["p"];
            var c = Request.Params["c"];
            var a = Request.Params["a"];
            var id = Request.Params["id"];

            IMongoQuery q = Query<商品>.EQ(o => o.商品信息.精确型号, n).And(Query<商品>.EQ(o => o.审核数据.审核状态, 审核状态.审核通过));
            if (!string.IsNullOrWhiteSpace(id))
            {
                q = q.And(Query<商品>.NE(o => o.商品信息.所属供应商.用户ID, long.Parse(id)));
            }
            //按照精确型号查出其他供应商的集合（商品ID，商品价格，供应商ID）
            var l = 商品管理.列表商品(0, 0,
                        Fields<商品>.Include(o => o.Id, o => o.销售信息.价格, o => o.商品信息.所属供应商.用户ID),
                        q, includeDisabled: false);

            //按地域重新设定条件q
            q = Query<供应商>.EQ(o => o.审核数据.审核状态, 审核状态.审核通过);
            if (!string.IsNullOrWhiteSpace(p))
            {
                q = q.And(Query<供应商>.EQ(o => o.所属地域.省份, p));
                if (!string.IsNullOrWhiteSpace(c) && c != "不限")
                {
                    q = q.And(Query<供应商>.EQ(o => o.所属地域.城市, c));
                }
                if (!string.IsNullOrWhiteSpace(a) && a != "不限")
                {
                    q = q.And(Query<供应商>.EQ(o => o.所属地域.区县, a));
                }
            }

            //此处作分页处理
            //var pagesize = 10;
            //var page = 1;
            //if (!string.IsNullOrWhiteSpace(Request.Form["page"]))
            //{
            //    int.TryParse(Request.Params["page"], out page);
            //}

            //var listcount = l.Count();
            //var maxpage = Math.Max((listcount + pagesize - 1) / pagesize, 1);
            //var length = pagesize;
            //if (page == maxpage && listcount % pagesize != 0)
            //{
            //    length = listcount % pagesize;
            //}
            //var startnum = (page - 1)*pagesize;

            //得出其他供应商的集合
            var l1 = (null != q) ?
                用户管理.查询用户<供应商>(0, 0,
                Query<供应商>.In(o => o.Id, l.Select(o => o["商品信息"]["所属供应商"]["用户ID"].AsInt64))
                .And(q)) :
                用户管理.查询用户<供应商>(0, 0,
                Query<供应商>.In(o => o.Id, l.Select(o => o["商品信息"]["所属供应商"]["用户ID"].AsInt64)));

            var res = new Dictionary<供应商, IEnumerable<BsonDocument>>();
            foreach (var u in l1)
            {
                res[u] = l.Where(o => o["商品信息"]["所属供应商"]["用户ID"].AsInt64 == u.Id);
            }

            ViewData["供应商匹配商品列表"] = res;

            //ViewData["currentpage"] = page;
            //ViewData["pagecount"] = maxpage;


            return PartialView("Part_Product/Part_OtherGys");

        }

        public ActionResult Part_ProductNav()
        {
            if (!string.IsNullOrWhiteSpace(Request.QueryString["id"]))
            {
                long id = long.Parse(Request.QueryString["id"]);
                var good = new 商品分类();
                if (!string.IsNullOrEmpty(id.ToString()))
                {
                    try
                    {
                        good = 商品分类管理.查找分类(id);
                    }
                    catch
                    {
                        return Content("<script>window.location='/商品陈列/';</script>");
                    }
                }
                return PartialView("Part_Product/Part_ProductNav", good);
            }
            else
            {
                return Content("<script>window.location='/商品陈列/';</script>");
            }
        }
        public ActionResult Part_ProductList()
        {
            try
            {
                ViewData["type"] = 0;
                if (-1 != HttpContext.检查登录())
                {
                    ViewData["已登录"] = "1";
                }
                else
                {
                    ViewData["已登录"] = "0";
                }
                if (!string.IsNullOrWhiteSpace(Request.QueryString["type"]) && Request.QueryString["type"] == "1")
                {
                    ViewData["type"] = 1;
                }
                long id = 0;
                if (!string.IsNullOrWhiteSpace(Request.QueryString["id"]))
                {
                    id = long.Parse(Request.QueryString["id"]);
                }
                long proClass = id;
                IMongoQuery proQ = MongoDB.Driver.Builders.Query.EQ("审核数据.审核状态", 审核状态.审核通过);
                var listcount = (int)(商品管理.计数分类下商品(proClass, 0, 0, proQ, includeDisabled: false));
                var maxpage = Math.Max((listcount + PAGESIZE - 1) / PAGESIZE, 1);

                ViewData["listcount"] = listcount;
                ViewData["pagesize"] = PAGESIZE;
                ViewData["currentpage"] = 1;
                ViewData["pagecount"] = maxpage;
                int length = PAGESIZE;
                if (1 == maxpage)
                {
                    length = listcount;
                }

                ViewData["商品筛选列表"] = 商品管理.查询分类下商品(proClass, 0, length, proQ, includeDisabled: false);
                ViewData["classid"] = id;
                var att = 商品分类管理.查找分类((long)id).商品属性模板;
                if (att == null)
                {
                    return Content("<script>window.location='/商品陈列/';</script>");
                }
                ViewData["商品属性模板"] = att;

                ViewData["商品分类ID"] = proClass;
                ViewData["筛选条件"] = proQ.ToJson();

                ViewData["价格分段"] = 商品分类管理.查找分类((long)id).价格分段;

                //(QueryDocument.Parse("") as IMongoQuery)
                return PartialView("Part_Product/Part_ProductList");
            }
            catch
            {
                return Content("<script>window.location='/商品陈列/';</script>");
            }
        }
        public ActionResult mall()
        {
            long typeId = -1;
            if (!string.IsNullOrWhiteSpace(Request.QueryString["id"]))
            {
                typeId = long.Parse(Request.QueryString["id"].ToString());
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
            long pc = 0;
            if (typeId == -1)
            {
                pc = 商品管理.计数供应商商品(200000001522, 0, 0, Query<商品>.Where(m => m.审核数据.审核状态 == 审核状态.审核通过));
            }
            else
            {
                pc = 商品管理.计数供应商商品(200000001522, 0, 0, Query<商品>.Where(m => m.审核数据.审核状态 == 审核状态.审核通过 && m.商品信息.所属商品分类.商品分类ID == typeId));
            }
            pgCount = pc / 20;
            if (pc % 20 > 0)
            {
                pgCount++;
            }
            ViewData["type"] = typeId;
            ViewData["Pagecount"] = pgCount;
            ViewData["CurrentPage"] = cpg;
            IEnumerable<商品> goods = null;
            if (typeId == -1)
            {
                goods = 商品管理.查询商品(20 * (cpg - 1), 20,
                Query<商品>.Where(m => m.商品信息.所属供应商.用户ID == 200000001522 && m.审核数据.审核状态 == 审核状态.审核通过));
            }
            else
            {
                goods = 商品管理.查询商品(20 * (cpg - 1), 20,
               Query<商品>.Where(m => m.商品信息.所属供应商.用户ID == 200000001522 && m.审核数据.审核状态 == 审核状态.审核通过 && m.商品信息.所属商品分类.商品分类ID == typeId));
            }
            return View(goods);
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
            return PartialView("Part_Product/Part_GoodClass", goodclass);
        }
        public ActionResult Part_Recent()
        {
            if (-1 != HttpContext.检查登录())
            {
                ViewData["已登录"] = "1";
            }
            else
            {
                ViewData["已登录"] = "0";
            }
            List<商品> Recommend_Good = new List<商品>();
            List<long> ids = new List<long>();
            List<long> gid = new List<long>();
            try
            {
                IEnumerable<供应商增值服务申请记录> model = 供应商增值服务申请记录管理.查询供应商增值服务申请记录(0, 10, Query<供应商增值服务申请记录>.Where(o => o.所申请项目名 == "企业推广服务C1位置" && o.是否通过 == 通过状态.通过 && o.结束时间 > DateTime.Now));
                if (model != null && model.Count() != 0)
                {
                    foreach (var item in model)
                    {
                        if (item.所属供应商.用户数据.供应商用户信息.广告商品 != null && item.所属供应商.用户数据.供应商用户信息.广告商品.Count != 0)
                        {
                            foreach (var n in item.所属供应商.用户数据.供应商用户信息.广告商品)
                            {
                                if (n.Key == "企业推广服务C1位置")
                                {
                                    if (n.Value != null && n.Value.Count != 0)
                                    {
                                        foreach (var h in n.Value)
                                        {
                                            Recommend_Good.Add(h.商品.商品);
                                            ids.Add(h.商品.商品ID);
                                            gid.Add(h.商品.商品.商品信息.所属供应商.用户ID);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                if (Recommend_Good.Count < 10)
                {
                    int remain = Recommend_Good.Count;
                    for (int i = remain; i < 10; i++)
                    {
                        IEnumerable<商品> Temp_Good = 商品管理.查询商品(0, 1, Query<商品>.Where(o => !o.采购信息.参与协议采购 && !o.采购信息.参与应急采购 && o.审核数据.审核状态 == 审核状态.审核通过).And(Query<商品>.NotIn(h => h.Id, ids)).And(Query<商品>.NotIn(h => h.商品信息.所属供应商.用户ID, gid)), includeDisabled: false);
                        if (Temp_Good != null && Temp_Good.Count() != 0)
                        {
                            Recommend_Good.Add(Temp_Good.First());
                            gid.Add(Temp_Good.First().商品信息.所属供应商.用户ID);
                        }
                    }
                }
            }
            catch
            {
                Recommend_Good = 商品管理.查询商品(0, 5, MongoDB.Driver.Builders.Query.EQ("审核数据.审核状态", 审核状态.审核通过), false, SortBy<商品>.Descending(o => o.基本数据.修改时间), false).ToList();
            }

            return PartialView("Part_Product/Part_Recent", Recommend_Good);
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
            List<商品> Recommend_Good = new List<商品>();
            List<long> ids = new List<long>();
            List<long> gid = new List<long>();
            try
            {
                IEnumerable<供应商增值服务申请记录> model = 供应商增值服务申请记录管理.查询供应商增值服务申请记录(0, 10, Query<供应商增值服务申请记录>.Where(o => o.所申请项目名 == "企业推广服务C2位置" && o.是否通过 == 通过状态.通过 && o.结束时间 > DateTime.Now));
                if (model != null && model.Count() != 0)
                {
                    foreach (var item in model)
                    {
                        if (item.所属供应商.用户数据.供应商用户信息.广告商品 != null && item.所属供应商.用户数据.供应商用户信息.广告商品.Count != 0)
                        {
                            foreach (var n in item.所属供应商.用户数据.供应商用户信息.广告商品)
                            {
                                if (n.Key == "企业推广服务C2位置")
                                {
                                    if (n.Value != null && n.Value.Count != 0)
                                    {
                                        foreach (var h in n.Value)
                                        {
                                            if (!h.商品.商品.基本数据.已删除 && h.商品.商品.审核数据.审核状态== 审核状态.审核通过 && !h.商品.商品.基本数据.已屏蔽)
                                            {
                                                Recommend_Good.Add(h.商品.商品);
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
                }
                if (Recommend_Good.Count < 10)
                {
                    int remain = Recommend_Good.Count;
                    for (int i = remain; i < 10; i++)
                    {
                        IEnumerable<商品> Temp_Good = 商品管理.查询商品(0, 1, Query<商品>.Where(o => !o.采购信息.参与协议采购 && !o.采购信息.参与应急采购 && o.审核数据.审核状态 == 审核状态.审核通过).And(Query<商品>.NotIn(h => h.Id, ids)).And(Query<商品>.NotIn(h => h.商品信息.所属供应商.用户ID, gid)), includeDisabled: false).OrderByDescending(u => u.销售信息.点击量);
                        if (Temp_Good != null && Temp_Good.Count() != 0)
                        {
                            if (!gid.Contains(Temp_Good.First().商品信息.所属供应商.用户ID))
                            {
                                gid.Add(Temp_Good.First().商品信息.所属供应商.用户ID);
                                Recommend_Good.Add(Temp_Good.First());
                            }
                        }
                    }
                }
                Recommend_Good = Recommend_Good.OrderBy(m => m.销售信息.价格).ToList();
            }
            catch
            {
                Recommend_Good = 商品管理.查询商品(0, 5, MongoDB.Driver.Builders.Query.EQ("审核数据.审核状态", 审核状态.审核通过), false, SortBy<商品>.Descending(o => o.销售信息.点击量), false).OrderBy(m => m.销售信息.价格).ToList();
            }
            return PartialView("Part_Product/Part_Recommend", Recommend_Good);
        }
        public ActionResult Part_Recommend1()
        {
            List<商品> Recommend_Good = new List<商品>();
            try
            {
                Recommend_Good = 商品管理.查询供应商商品(200000001522, 0, 10).ToList();
            }
            catch
            {
                Recommend_Good = 商品管理.查询商品(0, 5, MongoDB.Driver.Builders.Query.EQ("审核数据.审核状态", 审核状态.审核通过), false, SortBy<商品>.Descending(o => o.销售信息.点击量), false).OrderBy(m => m.销售信息.价格).ToList();
            }
            return PartialView("Part_Product/Part_Recommend1", Recommend_Good);
        }
        public ActionResult Part_OtherClass()
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(Request.QueryString["id"]))
                {
                    long id = long.Parse(Request.QueryString["id"]);
                    var good = new 商品分类();
                    if (!string.IsNullOrEmpty(id.ToString()))
                    {
                        good = 商品分类管理.查找分类(id);
                        return PartialView("Part_Product/Part_OtherClass", good);
                    }
                    else
                    {
                        return Content("<script>window.location='/商品陈列/';</script>");
                    }
                }
                else
                {
                    return Content("<script>window.location='/商品陈列/';</script>");
                }
            }
            catch
            {
                return Content("<script>window.location='/商品陈列/';</script>");
            }
        }
        public ActionResult Part_ProductCondition()
        {
            return PartialView("Part_Product/Part_ProductCondition");
        }
        public ActionResult ProductAllList()
        {
            try
            {
                if (-1 != HttpContext.检查登录())
                {
                    ViewData["已登录"] = "1";
                }
                else
                {
                    ViewData["已登录"] = "0";
                }
                long id = 0;
                if (!string.IsNullOrWhiteSpace(Request.QueryString["id"]))
                {
                    id = long.Parse(Request.QueryString["id"]);
                }
                
                IMongoQuery proQ = MongoDB.Driver.Builders.Query.EQ("审核数据.审核状态", 审核状态.审核通过);

                var sjfn = 商品分类管理.查找三级分类(id);
                var arrId = new List<long>();
                foreach (var k in sjfn)
                {
                    arrId.Add(k.Id);
                }

                var q1 = proQ.And(Query<商品>.In(o => o.商品信息.所属商品分类.商品分类ID, arrId));
                var listcount = 商品管理.计数商品(0, 0, q1, includeDisabled: false);
                var maxpage = Math.Max((listcount + PAGESIZE - 1) / PAGESIZE, 1);

                ViewData["listcount"] = listcount;
                ViewData["pagesize"] = PAGESIZE;
                ViewData["currentpage"] = 1;
                ViewData["pagecount"] = maxpage;
                int length = PAGESIZE;
                if (1 == maxpage)
                {
                    length = (int)listcount;
                }

                ViewData["商品筛选列表"] = 商品管理.查询商品(0, length, q1, false, SortBy.Ascending("商品信息.商品名"), includeDisabled: false);
                ViewData["classid"] = id;
                
                ViewData["商品分类ID"] = id;
                ViewData["筛选条件"] = proQ.ToJson();

                ViewData["价格分段"] = 商品分类管理.查找分类((long)id).价格分段;

                return View();
            }
            catch
            {
                return Content("<script>window.location='/商品陈列/';</script>");
            }
        }

        public ActionResult AllListChangePage(int? page)
        {
            if (-1 != HttpContext.检查登录())
            {
                ViewData["已登录"] = "1";
            }
            else
            {
                ViewData["已登录"] = "0";
            }

            long classid = long.Parse(Request.Params["classid"]);
            IMongoQuery proQ = MongoDB.Driver.Builders.Query.EQ("审核数据.审核状态", 审核状态.审核通过);

            var sjfn = 商品分类管理.查找三级分类(classid);
            var arrId = new List<long>();
            foreach (var k in sjfn)
            {
                arrId.Add(k.Id);
            }
            var q1 = proQ.And(Query<商品>.In(o => o.商品信息.所属商品分类.商品分类ID, arrId));

            //////////////////////////////////////////////////////////////////////新增按地域查询
            var province = Request.QueryString["province"];
            var city = Request.QueryString["city"];

            IEnumerable<BsonValue> gysidlist = null;
            if (!string.IsNullOrWhiteSpace(province))
            {
                var q_gys = Query<供应商>.EQ(o => o.所属地域.省份, province);
                if (!string.IsNullOrWhiteSpace(city))
                {
                    q_gys = q_gys.And(Query<供应商>.EQ(o => o.所属地域.城市, city));
                }
                gysidlist = 用户管理.列表用户<供应商>(0, 0,
                    Fields<供应商>.Include(o => o.Id),
                    Query<供应商>.EQ(o => o.审核数据.审核状态, 审核状态.审核通过).And(q_gys)
                ).Select(o => o["_id"]);
            }

            if (gysidlist != null)
            {
                q1 = q1.And(Query<商品>.In(o => o.商品信息.所属供应商.用户ID, gysidlist));
            }
            //////////////////////////////////////////////////////////////////////新增按地域查询



            var LISTCOUNT = (int)商品管理.计数商品(0, 0, q1, includeDisabled: false);

            int MAXPAGE = (int)Math.Max((LISTCOUNT + PAGESIZE - 1) / PAGESIZE, 1);
            if (string.IsNullOrEmpty(page.ToString()) || page < 0 || page > MAXPAGE)
            {
                page = 1;
            }
            ViewData["listcount"] = LISTCOUNT;
            ViewData["pagesize"] = PAGESIZE;
            ViewData["currentpage"] = page;
            ViewData["pagecount"] = MAXPAGE;

            int length = PAGESIZE;
            if (page == MAXPAGE && LISTCOUNT % PAGESIZE != 0)
            {
                length = LISTCOUNT % PAGESIZE;
            }

            ViewData["商品筛选列表"] = 商品管理.查询商品(((int)page - 1) * PAGESIZE, length, q1, false, SortBy.Ascending("商品信息.商品名"), includeDisabled: false);

            ViewData["商品分类ID"] = classid;

            return PartialView("Part_Product/Part_ProductByPage");
        }

        public ActionResult ChangeProductClass_pagechange(int? page)
        {
            if (-1 != HttpContext.检查登录())
            {
                ViewData["已登录"] = "1";
            }
            else
            {
                ViewData["已登录"] = "0";
            }

            ////////////////////////////////////////////////////////////////////////新增按地域查询
            //var province = Request.QueryString["province"];
            //var city = Request.QueryString["city"];

            //IEnumerable<BsonValue> gysidlist = null;
            //if (!string.IsNullOrWhiteSpace(province))
            //{
            //    var q_gys = Query<供应商>.EQ(o => o.所属地域.省份, province);
            //    if (!string.IsNullOrWhiteSpace(city))
            //    {
            //        q_gys = q_gys.And(Query<供应商>.EQ(o => o.所属地域.城市, city));
            //    }
            //    gysidlist = 用户管理.列表用户<供应商>(0, 0,
            //        Fields<供应商>.Include(o => o.Id),
            //        Query<供应商>.EQ(o => o.审核数据.审核状态, 审核状态.审核通过).And(q_gys)
            //    ).Select(o => o["_id"]);
            //}
            ////////////////////////////////////////////////////////////////////////新增按地域查询



            string classid = Request.Params["classid"];
            string condition = Request.Params["condition"];

            IMongoQuery pro_q = new QueryDocument(QueryDocument.Parse(condition).Elements);
            //pro_q.And(MongoDB.Driver.Builders.Query.EQ("审核数据.审核状态", 审核状态.审核通过));

            ////////////////////////////////////////////////////////////////////////新增按地域查询
            //if (gysidlist != null)
            //{
            //    pro_q = pro_q.And(Query<商品>.In(o => o.商品信息.所属供应商.用户ID, gysidlist));
            //}
            ////////////////////////////////////////////////////////////////////////新增按地域查询

            int LISTCOUNT = (int)商品管理.计数分类下商品(long.Parse(classid), 0, 0, pro_q, includeDisabled: false);

            int MAXPAGE = Math.Max((LISTCOUNT + PAGESIZE - 1) / PAGESIZE, 1);
            if (string.IsNullOrEmpty(page.ToString()) || page < 0 || page > MAXPAGE)
            {
                page = 1;
            }
            ViewData["listcount"] = LISTCOUNT;
            ViewData["pagesize"] = PAGESIZE;
            ViewData["currentpage"] = page;
            ViewData["pagecount"] = MAXPAGE;

            int length = PAGESIZE;
            if (page == MAXPAGE && LISTCOUNT % PAGESIZE != 0)
            {
                length = LISTCOUNT % PAGESIZE;
            }

            ViewData["商品筛选列表"] = 商品管理.查询分类下商品(long.Parse(classid), ((int)page - 1) * PAGESIZE, length, pro_q, includeDisabled: false);

            ViewData["商品分类ID"] = classid;
            ViewData["筛选条件"] = condition;

            return PartialView("Part_Product/Part_ProductCondition");
        }

        public ActionResult ChangeProductSearch_pagechange(int? page)
        {
            if (-1 != HttpContext.检查登录())
            {
                ViewData["已登录"] = "1";
            }
            else
            {
                ViewData["已登录"] = "0";
            }
            try
            {
                IList<商品查询> serchlist = new List<商品查询>();//该页显示的集合
                TopDocs serchalllist = null;
                string name = Request.Params["name"];
                if (!string.IsNullOrEmpty(name))
                {
                    serchalllist = SearchIndex("/Lucene.Net/IndexDic/Product", name);//满足该条件的总集合
                }
                var listcount = serchalllist.totalHits > 1000 ? 1000 : serchalllist.totalHits;
                if (serchalllist != null && listcount > 0)
                {
                    int maxpage = Math.Max((listcount + PAGESIZE - 1) / PAGESIZE, 1);

                    if (string.IsNullOrEmpty(page.ToString()) || page < 0 || page > maxpage)
                    {
                        page = 1;
                    }

                    int length = PAGESIZE;
                    if (maxpage == page && listcount % PAGESIZE != 0)
                    {
                        length = listcount % PAGESIZE;
                    }

                    int count = PAGESIZE * ((int)page - 1);

                    //IndexSearcher search = new IndexSearcher(IndexDic("/Lucene.Net/IndexDic/Product"), true);
                    IndexSearcher search = new IndexSearcher(new Lucene.Net.Store.SimpleFSDirectory(new System.IO.DirectoryInfo(IndexDic("/Lucene.Net/IndexDic/Product"))), true);
                    Dictionary<string, string> dic = new Dictionary<string, string>();
                    dic.Add("Name", name);

                    for (int i = count; i < count + length; i++)
                    {
                        商品查询 model = new 商品查询();

                        model.Id = long.Parse(search.Doc(serchalllist.scoreDocs[i].doc).Get("NumId"));
                        model.商品信息.商品名 = search.Doc(serchalllist.scoreDocs[i].doc).Get("Name");
                        model.商品数据.商品简介 = search.Doc(serchalllist.scoreDocs[i].doc).Get("Description");
                        model.商品信息.商品图片.Add(search.Doc(serchalllist.scoreDocs[i].doc).Get("Pic"));
                        model.销售信息.价格 = decimal.Parse(search.Doc(serchalllist.scoreDocs[i].doc).Get("Price"));
                        model.商品信息.所属供应商.用户ID = long.Parse(search.Doc(serchalllist.scoreDocs[i].doc).Get("Company"));
                        model.基本数据.修改时间 = Convert.ToDateTime(search.Doc(serchalllist.scoreDocs[i].doc).Get("AddTime"));

                        //新增按地域查找
                        model.所属地域.省份 = search.Doc(serchalllist.scoreDocs[i].doc).Get("Province");
                        model.所属地域.城市 = search.Doc(serchalllist.scoreDocs[i].doc).Get("City");
                        //新增按地域查找

                        serchlist.Add(SetHighlighter(dic, model));
                    }

                    ViewData["商品搜索显示列表"] = serchlist;
                    ViewData["pagecount"] = maxpage;
                    ViewData["currentpage"] = page;
                    ViewData["keyword"] = name;
                    ViewData["筛选条件"] = "";
                }

                return PartialView("Part_Product/Part_ProductKeyword");
            }
            catch
            {
                return Content("<script>alert('暂无搜索结果，将前往商品库进行查找！');window.location='/商品陈列/';</script>");
            }
        }

        public ActionResult ChangeProductClass()
        {
            if (-1 != HttpContext.检查登录())
            {
                ViewData["已登录"] = "1";
            }
            else
            {
                ViewData["已登录"] = "0";
            }
            //////////////////////////////////////////////////////////////////////新增按地域查询
            var province = Request.QueryString["province"];
            var city = Request.QueryString["city"];

            IEnumerable<BsonValue> gysidlist = null;
            if (!string.IsNullOrWhiteSpace(province))
            {
                var q_gys = Query<供应商>.EQ(o => o.所属地域.省份, province);
                if (!string.IsNullOrWhiteSpace(city))
                {
                    q_gys = q_gys.And(Query<供应商>.EQ(o => o.所属地域.城市, city));
                }
                gysidlist = 用户管理.列表用户<供应商>(0, 0,
                    Fields<供应商>.Include(o => o.Id),
                    Query<供应商>.EQ(o => o.审核数据.审核状态, 审核状态.审核通过).And(q_gys)
                ).Select(o => o["_id"]);
            }
            //////////////////////////////////////////////////////////////////////新增按地域查询

            long id = long.Parse(Request.QueryString["classid"]);

            IMongoQuery q = Query<商品>.EQ(o => o.审核数据.审核状态, 审核状态.审核通过);

            //////////////////////////////////////////////////////////////////////新增按价格查询
            var price = Request.QueryString["price"];
            if (!string.IsNullOrWhiteSpace(price))
            {
                if (price.Contains("及以上"))
                {
                    price = price.Replace("及以上", "");
                    q = q.And(MongoDB.Driver.Builders.Query.Where(
                "function(){" +
                "var p = parseInt(obj.销售信息.价格.split('.')[0]);" +
                "return p >= " + price + ";" +
                "}"));
                    //q = q.And(Query<商品>.GTE(o => o.销售信息.价格, decimal.Parse(price)));
                }
                else
                {
                    var p = price.Split('-');
                    q = q.And(MongoDB.Driver.Builders.Query.Where(
                "function(){" +
                "var p = parseInt(obj.销售信息.价格.split('.')[0]);" +
                "return p >= " + p[0] + " && p <= " + p[1] + ";" +
                "}"));

                    //q = q
                    //    .And(Query<商品>.GTE(o => o.销售信息.价格, decimal.Parse(p[0])))
                    //    .And(Query<商品>.LTE(o => o.销售信息.价格, decimal.Parse(p[1]))); //.Where(o => o.销售信息.价格 >= decimal.Parse(p[0]) && o.销售信息.价格 <= decimal.Parse(p[1])));
                }
            }

            //////////////////////////////////////////////////////////////////////新增按价格查询


            if (!string.IsNullOrEmpty(Request.QueryString["classname"]))//判断筛选条件
            {
                string parms = Request.QueryString["classname"].Substring(0, Request.QueryString["classname"].Length - 1);
                var pvs = parms.Split('$');

                foreach (var item in pvs)
                {
                    var pv = item.Split('|');
                    q = q.And(MongoDB.Driver.Builders.Query.EQ("商品数据.商品属性." + pv[0] + "." + pv[1], pv[2]));
                }
            }
            //////////////////////////////////////////////////////////////////////新增按地域查询
            if (gysidlist != null)
            {
                q = q.And(Query<商品>.In(o => o.商品信息.所属供应商.用户ID, gysidlist));
            }
            //////////////////////////////////////////////////////////////////////新增按地域查询

            int page = 1;

            int LISTCOUNT = (int)商品管理.计数分类下商品(id, 0, 0, q, includeDisabled: false);

            int MAXPAGE = Math.Max((LISTCOUNT + PAGESIZE - 1) / PAGESIZE, 1);

            ViewData["listcount"] = LISTCOUNT;
            ViewData["pagesize"] = PAGESIZE;
            ViewData["currentpage"] = page;
            ViewData["pagecount"] = MAXPAGE;

            int length = PAGESIZE;
            if (page == MAXPAGE && LISTCOUNT % PAGESIZE != 0)
            {
                length = LISTCOUNT % PAGESIZE;
            }
            var x = 商品管理.查询分类下商品(id, ((int)page - 1) * PAGESIZE, PAGESIZE, q, includeDisabled: false);
            ViewData["商品筛选列表"] = 商品管理.查询分类下商品(id, ((int)page - 1) * PAGESIZE, PAGESIZE, q, includeDisabled: false);

            ViewData["商品分类ID"] = id;
            ViewData["筛选条件"] = q.ToJson();

            return PartialView("Part_Product/Part_ProductCondition");
        }

        public ActionResult AutoCompleteList()
        {
            try
            {
                string serchlist = "";
                //IndexSearcher search = new IndexSearcher(IndexDic("/Lucene.Net/IndexDic/ProductCatalog"), true);
                IndexSearcher search = new IndexSearcher(new Lucene.Net.Store.SimpleFSDirectory(new System.IO.DirectoryInfo(IndexDic("/Lucene.Net/IndexDic/ProductCatalog"))), true);
                int count = search.MaxDoc();
                for (int i = 0; i < count; i++)
                {
                    serchlist += search.Doc(i).Get("Name").Trim() + " ";
                }
                return Content(serchlist);
            }
            catch
            {
                return Content("");
            }
        }

        public ActionResult AutoCompleteList_Gys()
        {
            try
            {
                string serchlist = "";
                //IndexSearcher search = new IndexSearcher(IndexDic("/Lucene.Net/IndexDic/ProductCatalog"), true);
                IndexSearcher search = new IndexSearcher(new Lucene.Net.Store.SimpleFSDirectory(new System.IO.DirectoryInfo(IndexDic("/Lucene.Net/IndexDic/GysCatalog"))), true);
                int count = search.MaxDoc();
                for (int i = 0; i < count; i++)
                {
                    serchlist += search.Doc(i).Get("Name").Trim() + " ";
                }
                return Content(serchlist);
            }
            catch
            {
                return Content("");
            }
        }

        /// <summary>
        /// 从索引搜索结果
        /// </summary>
        private TopDocs SearchIndex(string indexdic, string keyword)
        {
            PanGu.Segment.Init(PanGuXmlPath);
            var dic = new Dictionary<string, string>();
            var bQuery = new BooleanQuery();
            var title = string.Empty;
            if (!string.IsNullOrEmpty(keyword))
            {
                title = GetKeyWordsSplitBySpace(keyword);
                QueryParser parse = new MultiFieldQueryParser(Lucene.Net.Util.Version.LUCENE_29, new String[] { "Name" }, PanGuAnalyzer);
                var query = parse.Parse(title);
                parse.SetDefaultOperator(QueryParser.Operator.AND);
                bQuery.Add(query, BooleanClause.Occur.MUST);
                dic.Add("Name", keyword);
            }

            if (bQuery.GetClauses().Length > 0)
            {
                return GetSearchResult(bQuery, indexdic);
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// 获取
        /// </summary>
        /// <param name="bQuery"></param>
        private TopDocs GetSearchResult(BooleanQuery bQuery, string indexdic)
        {
            TopDocs docs = null;
            try
            {
                //IndexSearcher search = new IndexSearcher(IndexDic(indexdic), true);
                IndexSearcher search = new IndexSearcher(new Lucene.Net.Store.SimpleFSDirectory(new System.IO.DirectoryInfo(IndexDic(indexdic))), true);
                Stopwatch stopwatch = Stopwatch.StartNew();
                //SortField构造函数第三个字段true为降序,false为升序
                var sort = new Sort(new SortField[] { SortField.FIELD_SCORE, new SortField("AddTime", SortField.DOC, true) });//按照分数倒叙排序,再按照时间倒叙排序
                docs = search.Search(bQuery, (Lucene.Net.Search.Filter)null, 1000, sort);
                stopwatch.Stop();
                //if (docs != null && docs.totalHits > 0)
                //{
                //    for (int i = 0; i < docs.totalHits; i++)
                //    {

                //        商品 model = new 商品();

                //        model.Id = long.Parse(search.Doc(docs.scoreDocs[i].doc).Get("NumId"));
                //        model.商品信息.商品名 = search.Doc(docs.scoreDocs[i].doc).Get("Name");
                //        model.商品数据.商品简介 = search.Doc(docs.scoreDocs[i].doc).Get("Description");
                //        model.商品信息.商品图片.Add(search.Doc(docs.scoreDocs[i].doc).Get("Pic"));

                //        model.销售信息.价格 = decimal.Parse(search.Doc(docs.scoreDocs[i].doc).Get("Price"));

                //        model.商品信息.所属供应商.用户ID = long.Parse(search.Doc(docs.scoreDocs[i].doc).Get("Company"));
                //        model.基本数据.修改时间 = Convert.ToDateTime(search.Doc(docs.scoreDocs[i].doc).Get("AddTime"));
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
        private 商品查询 SetHighlighter(Dictionary<string, string> dicKeywords, 商品查询 model)
        {
            SimpleHTMLFormatter simpleHTMLFormatter = new PanGu.HighLight.SimpleHTMLFormatter("<font color=\"red\">", "</font>");
            Highlighter highlighter = new PanGu.HighLight.Highlighter(simpleHTMLFormatter, new Segment());
            highlighter.FragmentSize = 50;
            string strTitle = string.Empty;
            dicKeywords.TryGetValue("Name", out strTitle);
            if (!string.IsNullOrEmpty(strTitle))
            {
                string title = highlighter.GetBestFragment(strTitle, model.商品信息.商品名);
                if (!string.IsNullOrEmpty(title))
                    model.商品信息.商品名 = title;
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
        public class 商品查询 : 商品
        {
            public _地域 所属地域 { get; set; }
            public 商品查询()
            {
                this.所属地域 = new _地域();
            }
        }

    }
}
