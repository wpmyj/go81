using Go81WebApp.Models.管理器;
using Go81WebApp.Models.数据模型;
using Go81WebApp.Models.数据模型.内容数据模型;
using Go81WebApp.Models.数据模型.项目数据模型;
using Go81WebApp.Models.数据模型.用户数据模型;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.PanGu;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers;
using Lucene.Net.Search;
using MongoDB.Driver.Builders;
using PanGu;
using PanGu.HighLight;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Web.Mvc;

namespace Go81WebApp.Controllers.门户
{
    public class 公告Controller : Controller
    {
        private static int PAGESIZE = int.Parse(ConfigurationManager.AppSettings["公告每页显示条数"]);

        [静态化]
        public ActionResult AnnounceList()
        {
            return View();
        }

        public ActionResult AnnounceSearch()
        {
            return View();
        }

        [HttpPost]
        public ActionResult AnnounceListSearch()
        {
            return View("AnnounceSearch");
        }
        public ActionResult AnnounceDetail(long? id)
        {
            return View("AnnounceDetail");
        }
        /// <summary>
        /// 预报名的函数
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public ActionResult SignUpForAd(long id)
        {
            用户基本数据 user = this.HttpContext.获取当前用户();
            //已登录且登录用户为供应商
            if (-1 != this.HttpContext.检查登录() && user.GetType() == typeof(供应商))
            {
                IEnumerable<招标采购预报名> l = 招标采购预报名管理.查询招标采购预报名(0, 1, MongoDB.Driver.Builders.Query<招标采购预报名>.EQ(o => o.所属公告链接.公告ID, id));
                if (l != null && l.Any())
                {
                    招标采购预报名 temp = l.ToList()[0];//已存在时只需增加供应商链接

                    //已经关闭，则返回提示
                    if (temp.预报名已关闭)
                    {
                        return Content("此次报名已经关闭！");
                    }

                    //该用户已报名，则不再处理
                    if (temp.预报名供应商列表.Exists(o => o.供应商链接.用户ID == user.Id))
                    {
                        return Content("您已参加此报名，无需重复报名！<a target='_blank' style='padding-left:20px; text-decoration:underline; color:red;' href='/供应商后台/Gys_enroll?page=1'>点击这里进行查看</a>");
                    }

                    招标采购预报名._供应商预报名信息 q = new 招标采购预报名._供应商预报名信息();
                    q.报名时间 = DateTime.Now;
                    q.供应商链接.用户ID = user.Id;
                    q.审核数据.审核状态 = 审核状态.审核通过;
                    q.审核数据.审核时间 = DateTime.Now;

                    temp.预报名供应商列表.Add(q);
                    招标采购预报名管理.更新招标采购预报名(temp);
                    return Content("报名成功!<a target='_blank' style='padding-left:20px; text-decoration:underline; color:red;' href='/供应商后台/Gys_enroll?page=1'>点击这里进行查看</a>");
                }
                else
                {
                    return Content("不存在该预报名！");
                }
            }
            else
            {
                return Content("抱歉，只有已登录供应商用户才能进行报名！");
            }
        }
        public ActionResult Part_AnnounceList(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                id = "公开招标";
            }
            ViewData["公告类型"] = id;

            var pro = "";
            var city = "";
            var area = "";
            var hy = "";
            var keyword = "";
            var time = -1;
            var exactdate = "";
            var startdate = "";
            var enddate = "";

            TopDocs serchalllist = SearchIndex("/Lucene.Net/IndexDic/GongGao", pro, city, area, hy, id, keyword);

            if (serchalllist != null && serchalllist.totalHits > 0)
            {
                IList<公告> serchlisttemp = new List<公告>();
                Dictionary<string, string> dic = new Dictionary<string, string>();
                dic.Add("title", keyword);
                //IndexSearcher search = new IndexSearcher(IndexDic("/Lucene.Net/IndexDic/GongGao"), true);
                IndexSearcher search = new IndexSearcher(new Lucene.Net.Store.SimpleFSDirectory(new System.IO.DirectoryInfo(IndexDic("/Lucene.Net/IndexDic/GongGao"))), true);
                for (int i = 0; i < serchalllist.totalHits && i < 1000; i++)
                {
                    公告 model = new 公告();
                    model.Id = long.Parse(search.Doc(serchalllist.scoreDocs[i].doc).Get("NumId"));
                    model.内容主体.标题 = search.Doc(serchalllist.scoreDocs[i].doc).Get("Title");
                    model.公告信息.公告类别 = (公告.公告类别)Enum.Parse(typeof(公告.公告类别), search.Doc(serchalllist.scoreDocs[i].doc).Get("AdClass"));
                    model.公告信息.公告性质 = (公告.公告性质)Enum.Parse(typeof(公告.公告性质), search.Doc(serchalllist.scoreDocs[i].doc).Get("AdFeature"));
                    model.公告信息.所属地域.省份 = search.Doc(serchalllist.scoreDocs[i].doc).Get("AdPro");
                    model.公告信息.所属地域.城市 = search.Doc(serchalllist.scoreDocs[i].doc).Get("AdCity");
                    model.公告信息.所属地域.区县 = search.Doc(serchalllist.scoreDocs[i].doc).Get("AdArea");
                    model.公告信息.一级分类 = search.Doc(serchalllist.scoreDocs[i].doc).Get("AdHy");
                    model.内容主体.发布时间 = Convert.ToDateTime(search.Doc(serchalllist.scoreDocs[i].doc).Get("AddTime"));
                    serchlisttemp.Add(SetHighlighter(dic, model));
                }
                getsearch(1, time, exactdate, serchlisttemp, startdate, enddate, id);

            }
            else
            {
                ViewData["currentpage"] = 1;
                ViewData["pagecount"] = 1;
            }
            ViewBag.Provence = pro;
            ViewBag.City = city;
            ViewBag.Area = area;
            ViewBag.Hy = hy;
            ViewBag.Adclass = id;
            ViewBag.keyword = keyword;
            ViewBag.time = time;
            ViewBag.exactdate = exactdate;
            ViewBag.startdate = startdate;
            ViewBag.enddate = enddate;
            ViewData["行业列表"] = 商品分类管理.查找子分类();

            return PartialView("Part_Announce/Part_AnnounceList");
        }

        public ActionResult Part_SearchByPage()
        {
            var pro = Request.Params["deliverprovince"];
            var city = Request.Params["delivercity"];
            var area = Request.Params["deliverarea"];
            var hy = Request.Params["hy"];
            var adclass = Request.Params["adclass"];
            var keyword = Request.Params["keyword"];
            int time = int.Parse(Request.Params["time"]);
            int page = int.Parse(Request.Params["page"]);
            var exactdate = Request.Params["exactdate"];
            var startdate = Request.Params["startdate"];
            var enddate = Request.Params["enddate"];

            TopDocs serchalllist = SearchIndex("/Lucene.Net/IndexDic/GongGao", pro, city, area, hy, adclass, keyword);

            if (serchalllist != null && serchalllist.totalHits > 0)
            {
                IList<公告> serchlisttemp = new List<公告>();
                Dictionary<string, string> dic = new Dictionary<string, string>();
                dic.Add("title", keyword);
                //IndexSearcher search = new IndexSearcher(IndexDic("/Lucene.Net/IndexDic/GongGao"), true);
                IndexSearcher search = new IndexSearcher(new Lucene.Net.Store.SimpleFSDirectory(new System.IO.DirectoryInfo(IndexDic("/Lucene.Net/IndexDic/GongGao"))), true);
                for (int i = 0; i < serchalllist.totalHits && i < 1000; i++)
                {
                    公告 model = new 公告();
                    model.Id = long.Parse(search.Doc(serchalllist.scoreDocs[i].doc).Get("NumId"));
                    model.内容主体.标题 = search.Doc(serchalllist.scoreDocs[i].doc).Get("Title");
                    model.公告信息.公告类别 = (公告.公告类别)Enum.Parse(typeof(公告.公告类别), search.Doc(serchalllist.scoreDocs[i].doc).Get("AdClass"));
                    model.公告信息.公告性质 = (公告.公告性质)Enum.Parse(typeof(公告.公告性质), search.Doc(serchalllist.scoreDocs[i].doc).Get("AdFeature"));
                    model.公告信息.所属地域.省份 = search.Doc(serchalllist.scoreDocs[i].doc).Get("AdPro");
                    model.公告信息.所属地域.城市 = search.Doc(serchalllist.scoreDocs[i].doc).Get("AdCity");
                    model.公告信息.所属地域.区县 = search.Doc(serchalllist.scoreDocs[i].doc).Get("AdArea");
                    model.公告信息.一级分类 = search.Doc(serchalllist.scoreDocs[i].doc).Get("AdHy");
                    model.内容主体.发布时间 = Convert.ToDateTime(search.Doc(serchalllist.scoreDocs[i].doc).Get("AddTime")); ;
                    serchlisttemp.Add(SetHighlighter(dic, model));
                }
                getsearch(page, time, exactdate, serchlisttemp, startdate, enddate, adclass);
            }
            else
            {
                ViewData["currentpage"] = 1;
                ViewData["pagecount"] = 1;
            }
            return PartialView("Part_Announce/Part_SearchByPage");
        }
        public ActionResult Part_AnnounceDetail()
        {
            //通知详情
            公告 model = null;
            ViewData["登录"] = "0";
            ViewData["是否可以报名"] = "0";
            try
            {
                公告 g = 公告管理.查找公告(int.Parse(Request.QueryString["id"]));

                if (g.审核数据.审核状态 == 审核状态.审核通过)
                {
                    model = g;
                    model.点击次数 += 1;
                    公告管理.更新公告(model, false, false);

                    IEnumerable<招标采购预报名> m = 招标采购预报名管理.查询招标采购预报名(0, 1, MongoDB.Driver.Builders.Query<招标采购预报名>.EQ(o => o.所属公告链接.公告ID, model.Id));
                    if (model.公告信息.公告类别 == 公告.公告类别.公开招标 && model.公告信息.公告性质 == 公告.公告性质.采购公告 && m != null && m.Any() && !m.First().预报名已关闭)
                    {
                        ViewData["是否可以报名"] = "1";
                    }
                    //TD:是否  招标采购项目--》项目类型 中的网上竞标
                }
                if (HttpContext.检查登录() != -1 || WebApiApplication.IsIntranet)
                {
                    ViewData["登录"] = "1";
                }
            }
            catch
            {

            }
            return PartialView("Part_Announce/Part_AnnounceDetail", model);
        }
        public ActionResult Part_OtherList()
        {
            ViewData["最近公告"] = 公告管理.查询公告(0, 10,
               MongoDB.Driver.Builders.Query.EQ("审核数据.审核状态", 审核状态.审核通过), false, SortBy.Descending("内容主体.发布时间"), false);
            return PartialView("Part_Announce/Part_OtherList");
        }
        public ActionResult Part_AnnounceSearch()
        {
            var pro = Request.Form["deliverprovince"];
            var city = Request.Form["delivercity"];
            var area = Request.Form["deliverarea"];
            var hy = Request.Form["hy"];
            var adclass = Request.Form["adclass"];
            var keyword = Request.Params["keyword"];
            var time = int.Parse(Request.Form["time"]);
            var exactdate = Request.Form["exactdate"];
            var startdate = Request.Form["startdate"];
            var enddate = Request.Form["enddate"];

            TopDocs serchalllist = SearchIndex("/Lucene.Net/IndexDic/GongGao", pro, city, area, hy, adclass, keyword);

            if (serchalllist != null && serchalllist.totalHits > 0)
            {
                IList<公告> serchlisttemp = new List<公告>();
                Dictionary<string, string> dic = new Dictionary<string, string>();
                dic.Add("title", keyword);
                //IndexSearcher search = new IndexSearcher(IndexDic("/Lucene.Net/IndexDic/GongGao"), true);
                IndexSearcher search = new IndexSearcher(new Lucene.Net.Store.SimpleFSDirectory(new System.IO.DirectoryInfo(IndexDic("/Lucene.Net/IndexDic/GongGao"))), true);
                for (int i = 0; i < serchalllist.totalHits && i < 1000; i++)
                {
                    公告 model = new 公告();
                    model.Id = long.Parse(search.Doc(serchalllist.scoreDocs[i].doc).Get("NumId"));
                    model.内容主体.标题 = search.Doc(serchalllist.scoreDocs[i].doc).Get("Title");
                    model.公告信息.公告类别 = (公告.公告类别)Enum.Parse(typeof(公告.公告类别), search.Doc(serchalllist.scoreDocs[i].doc).Get("AdClass"));
                    model.公告信息.公告性质 = (公告.公告性质)Enum.Parse(typeof(公告.公告性质), search.Doc(serchalllist.scoreDocs[i].doc).Get("AdFeature"));
                    model.公告信息.所属地域.省份 = search.Doc(serchalllist.scoreDocs[i].doc).Get("AdPro");
                    model.公告信息.所属地域.城市 = search.Doc(serchalllist.scoreDocs[i].doc).Get("AdCity");
                    model.公告信息.所属地域.区县 = search.Doc(serchalllist.scoreDocs[i].doc).Get("AdArea");
                    model.公告信息.一级分类 = search.Doc(serchalllist.scoreDocs[i].doc).Get("AdHy");
                    model.内容主体.发布时间 = Convert.ToDateTime(search.Doc(serchalllist.scoreDocs[i].doc).Get("AddTime"));
                    serchlisttemp.Add(SetHighlighter(dic, model));
                }
                getsearch(1, time, exactdate, serchlisttemp, startdate, enddate, adclass);

            }
            else
            {
                ViewData["currentpage"] = 1;
                ViewData["pagecount"] = 1;
            }
            ViewBag.Provence = pro;
            ViewBag.City = city;
            ViewBag.Area = area;
            ViewBag.Hy = hy;
            ViewBag.Adclass = adclass;
            ViewBag.keyword = keyword;
            ViewBag.time = time;
            ViewBag.exactdate = exactdate;
            ViewBag.startdate = startdate;
            ViewBag.enddate = enddate;
            ViewData["行业列表"] = 商品分类管理.查找子分类();

            return PartialView("Part_Announce/Part_AnnounceSearch");
        }

        public void getsearch(int page, int days, string exactdate, IList<公告> serchlisttemp, string startdate, string enddate, string adclass)
        {
            serchlisttemp = serchlisttemp.OrderByDescending(o => o.内容主体.发布时间).ToList();
            var datenow = Convert.ToDateTime(DateTime.Now.ToShortDateString());
            IList<公告> serchlist = new List<公告>();
            try
            {

                if (days != -1)
                {
                    if (!string.IsNullOrWhiteSpace(exactdate))
                    {
                        var exactdatetime = Convert.ToDateTime(exactdate);
                        if (exactdatetime <= datenow && exactdatetime.AddDays(days) >= datenow)
                        {
                            foreach (var item in serchlisttemp)
                            {
                                if (exactdatetime.ToShortDateString() == item.内容主体.发布时间.ToShortDateString())
                                {
                                    serchlist.Add(item);
                                }
                            }
                        }
                    }
                    else
                    {
                        foreach (var item in serchlisttemp)
                        {
                            TimeSpan t = datenow - item.内容主体.发布时间;
                            if (t.TotalDays <= days)
                            {
                                serchlist.Add(item);
                            }
                        }
                    }
                }
                else
                {
                    if (!string.IsNullOrWhiteSpace(exactdate))
                    {
                        var exactdatetime = Convert.ToDateTime(exactdate);
                        foreach (var item in serchlisttemp)
                        {
                            if (exactdatetime.ToShortDateString() == item.内容主体.发布时间.ToShortDateString())
                            {
                                serchlist.Add(item);
                            }
                        }
                    }
                    else
                    {
                        serchlist = serchlisttemp;
                    }
                }

                if (!string.IsNullOrWhiteSpace(startdate))
                {
                    serchlist = serchlist.Where(o => o.内容主体.发布时间 >= Convert.ToDateTime(startdate)).ToList();
                }
                if (!string.IsNullOrWhiteSpace(enddate))
                {
                    serchlist = serchlist.Where(o => o.内容主体.发布时间 <= Convert.ToDateTime(enddate)).ToList();
                }

                if (adclass == "中标商品公告查询")
                {
                    serchlist = serchlist.Where(o => o.中标商品链接.Any(p=>p.商品ID!=-1)).ToList();
                }
            }
            catch
            {

            }

            if (serchlist.Count > 0)
            {
                IList<公告> serchlist_t = new List<公告>();
                int maxpage = Math.Max((serchlist.Count + PAGESIZE - 1) / PAGESIZE, 1);

                int length = PAGESIZE;
                if (maxpage == page && serchlist.Count % PAGESIZE != 0)
                    length = serchlist.Count % PAGESIZE;

                int count = PAGESIZE * (page - 1);

                for (int i = count; i < count + length; i++)
                {
                    serchlist_t.Add(serchlist[i]);
                }
                ViewData["currentpage"] = page;
                ViewData["pagecount"] = maxpage;
                ViewData["公告搜索显示列表"] = serchlist_t;
            }
            else
            {
                ViewData["currentpage"] = 1;
                ViewData["pagecount"] = 1;
            }
        }

        public ActionResult AnnounceBidGood()
        {
            var id = Request.Params["id"];
            var ad = 公告管理.查找公告(long.Parse(id));
            if (ad != null) 
            { 
                if (-1 != HttpContext.检查登录())
                {
                    ViewData["已登录"] = "1";
                }
                else
                {
                    ViewData["已登录"] = "0";
                }
                return View(ad);
            }
            return Redirect("/公告/AnnounceDetail?id=" + id);
        }

        public ActionResult Part_SearchByCondition()
        {
            var pro = Request.Params["deliverprovince"];
            var city = Request.Params["delivercity"];
            var area = Request.Params["deliverarea"];
            var hy = Request.Params["hy"];
            var adclass = Request.Params["adclass"];
            var keyword = Request.Params["keyword"];
            int time = int.Parse(Request.Params["time"]);
            int page = int.Parse(Request.Params["page"]);
            var exactdate = Request.Params["exactdate"];
            var startdate = Request.Params["startdate"];
            var enddate = Request.Params["enddate"];

            TopDocs serchalllist = SearchIndex("/Lucene.Net/IndexDic/GongGao", pro, city, area, hy, adclass, keyword);

            if (serchalllist != null && serchalllist.totalHits > 0)
            {
                IList<公告> serchlisttemp = new List<公告>();
                Dictionary<string, string> dic = new Dictionary<string, string>();
                dic.Add("title", keyword);
                //IndexSearcher search = new IndexSearcher(IndexDic("/Lucene.Net/IndexDic/GongGao"), true);
                IndexSearcher search = new IndexSearcher(new Lucene.Net.Store.SimpleFSDirectory(new System.IO.DirectoryInfo(IndexDic("/Lucene.Net/IndexDic/GongGao"))), true);
                for (int i = 0; i < serchalllist.totalHits && i < 1000; i++)
                {
                    公告 model = new 公告();
                    model.Id = long.Parse(search.Doc(serchalllist.scoreDocs[i].doc).Get("NumId"));
                    model.内容主体.标题 = search.Doc(serchalllist.scoreDocs[i].doc).Get("Title");
                    model.公告信息.公告类别 = (公告.公告类别)Enum.Parse(typeof(公告.公告类别), search.Doc(serchalllist.scoreDocs[i].doc).Get("AdClass"));
                    model.公告信息.公告性质 = (公告.公告性质)Enum.Parse(typeof(公告.公告性质), search.Doc(serchalllist.scoreDocs[i].doc).Get("AdFeature"));
                    model.公告信息.所属地域.省份 = search.Doc(serchalllist.scoreDocs[i].doc).Get("AdPro");
                    model.公告信息.所属地域.城市 = search.Doc(serchalllist.scoreDocs[i].doc).Get("AdCity");
                    model.公告信息.所属地域.区县 = search.Doc(serchalllist.scoreDocs[i].doc).Get("AdArea");
                    model.公告信息.一级分类 = search.Doc(serchalllist.scoreDocs[i].doc).Get("AdHy");
                    model.内容主体.发布时间 = Convert.ToDateTime(search.Doc(serchalllist.scoreDocs[i].doc).Get("AddTime"));
                    serchlisttemp.Add(SetHighlighter(dic, model));
                }

                getsearch(page, time, exactdate, serchlisttemp, startdate, enddate, adclass);
            }
            else
            {
                ViewData["currentpage"] = 1;
                ViewData["pagecount"] = 1;
            }

            return PartialView("Part_Announce/Part_SearchByCondition");
        }

        /// <summary>
        /// 从索引搜索结果
        /// </summary>
        private TopDocs SearchIndex(string indexdic, string pro, string city, string area, string hy, string adclass, string keyword)
        {
            PanGu.Segment.Init(PanGuXmlPath);
            Dictionary<string, string> dic = new Dictionary<string, string>();
            BooleanQuery bQuery = new BooleanQuery();


            if (!string.IsNullOrEmpty(pro))
            {
                //Lucene.Net.Search.Query query = new QueryParser("AdPro", PanGuAnalyzer).Parse(pro);
                Lucene.Net.Search.Query query = new TermQuery(new Term("AdPro", pro));
                bQuery.Add(query, BooleanClause.Occur.MUST);//其中的MUST、SHOULD、MUST_NOT表示与、或、非

                if (!string.IsNullOrEmpty(city) && city != "不限")
                {
                    query = new QueryParser(Lucene.Net.Util.Version.LUCENE_29, "AdCity", PanGuAnalyzer).Parse(city);
                    //query = new TermQuery(new Term("AdCity", city));
                    bQuery.Add(query, BooleanClause.Occur.MUST);

                    if (!string.IsNullOrEmpty(area) && area != "不限")
                    {
                        query = new QueryParser(Lucene.Net.Util.Version.LUCENE_29, "AdArea", PanGuAnalyzer).Parse(area);
                        //query = new TermQuery(new Term("AdArea", area));
                        bQuery.Add(query, BooleanClause.Occur.MUST);
                    }
                }
            }
            if (!string.IsNullOrEmpty(hy))
            {
                Lucene.Net.Search.Query query = new QueryParser(Lucene.Net.Util.Version.LUCENE_29, "AdHy", PanGuAnalyzer).Parse(hy);
                //Lucene.Net.Search.Query query = new TermQuery(new Term("AdHy", hy));
                bQuery.Add(query, BooleanClause.Occur.MUST);
            }
            if (!string.IsNullOrEmpty(adclass) && adclass != "中标商品公告查询")
            {
                if (adclass == "公开招标" || adclass == "其他")
                {
                    //Lucene.Net.Search.Query query = new QueryParser("AdClass", PanGuAnalyzer).Parse(adclass);
                    Lucene.Net.Search.Query query = new TermQuery(new Term("AdClass", adclass));
                    bQuery.Add(query, BooleanClause.Occur.MUST);
                }
                else
                {
                    BooleanQuery bQueryt = new BooleanQuery();
                    //Lucene.Net.Search.Query query = new TermQuery(new Term("AdClass", "公开招标"));
                    //bQuery.Add(query, BooleanClause.Occur.MUST_NOT);

                    ////Lucene.Net.Search.Query query = new QueryParser("AdClass", PanGuAnalyzer).Parse(adclass);
                    //query = new TermQuery(new Term("AdClass", "其他"));
                    //bQuery.Add(query, BooleanClause.Occur.MUST_NOT);
                    Lucene.Net.Search.Query query = new QueryParser(Lucene.Net.Util.Version.LUCENE_29, "AdClass", PanGuAnalyzer).Parse("邀请");
                    bQueryt.Add(query, BooleanClause.Occur.SHOULD);

                    //Lucene.Net.Search.Query query = new QueryParser("AdClass", PanGuAnalyzer).Parse(adclass);
                    query = new QueryParser(Lucene.Net.Util.Version.LUCENE_29, "AdClass", PanGuAnalyzer).Parse("协议采购");
                    bQueryt.Add(query, BooleanClause.Occur.SHOULD);

                    query = new QueryParser(Lucene.Net.Util.Version.LUCENE_29, "AdClass", PanGuAnalyzer).Parse("单一来源");
                    bQueryt.Add(query, BooleanClause.Occur.SHOULD);

                    query = new QueryParser(Lucene.Net.Util.Version.LUCENE_29, "AdClass", PanGuAnalyzer).Parse("询价采购");
                    bQueryt.Add(query, BooleanClause.Occur.SHOULD);

                    query = new QueryParser(Lucene.Net.Util.Version.LUCENE_29, "AdClass", PanGuAnalyzer).Parse("竞争性谈判");
                    bQueryt.Add(query, BooleanClause.Occur.SHOULD);
                    bQuery.Add(bQueryt, BooleanClause.Occur.MUST);

                }
            }
            else
            {
                BooleanQuery bQueryt = new BooleanQuery();
                Lucene.Net.Search.Query query = new QueryParser(Lucene.Net.Util.Version.LUCENE_29, "AdClass", PanGuAnalyzer).Parse("公开招标");
                bQueryt.Add(query, BooleanClause.Occur.SHOULD);

                //Lucene.Net.Search.Query query = new QueryParser("AdClass", PanGuAnalyzer).Parse(adclass);
                query = new QueryParser(Lucene.Net.Util.Version.LUCENE_29, "AdClass", PanGuAnalyzer).Parse("其他");
                bQueryt.Add(query, BooleanClause.Occur.SHOULD);

                query = new QueryParser(Lucene.Net.Util.Version.LUCENE_29, "AdClass", PanGuAnalyzer).Parse("邀请");
                bQueryt.Add(query, BooleanClause.Occur.SHOULD);

                //Lucene.Net.Search.Query query = new QueryParser("AdClass", PanGuAnalyzer).Parse(adclass);
                query = new QueryParser(Lucene.Net.Util.Version.LUCENE_29, "AdClass", PanGuAnalyzer).Parse("协议采购");
                bQueryt.Add(query, BooleanClause.Occur.SHOULD);

                query = new QueryParser(Lucene.Net.Util.Version.LUCENE_29, "AdClass", PanGuAnalyzer).Parse("单一来源");
                bQueryt.Add(query, BooleanClause.Occur.SHOULD);

                query = new QueryParser(Lucene.Net.Util.Version.LUCENE_29, "AdClass", PanGuAnalyzer).Parse("询价采购");
                bQueryt.Add(query, BooleanClause.Occur.SHOULD);

                query = new QueryParser(Lucene.Net.Util.Version.LUCENE_29, "AdClass", PanGuAnalyzer).Parse("竞争性谈判");
                bQueryt.Add(query, BooleanClause.Occur.SHOULD);
                bQuery.Add(bQueryt, BooleanClause.Occur.MUST);
            }

            if (!string.IsNullOrEmpty(keyword))
            {
                string title = string.Empty;
                if ((keyword != null && keyword != ""))
                {
                    title = GetKeyWordsSplitBySpace(keyword);
                    //QueryParser parse = new MultiFieldQueryParser(Lucene.Net.Util.Version.LUCENE_29, new String[] { "Title","Content"}, PanGuAnalyzer);
                    QueryParser parse = new MultiFieldQueryParser(Lucene.Net.Util.Version.LUCENE_29, new String[] { "Title"}, PanGuAnalyzer);
                    var query = parse.Parse(title);
                    parse.SetDefaultOperator(QueryParser.Operator.AND);
                    bQuery.Add(query, BooleanClause.Occur.MUST);
                    dic.Add("title", keyword);

                }
            }
            //if (bQuery != null && bQuery.GetClauses().Length > 0)
            //{
            return GetSearchResult(bQuery, dic, indexdic);
            //}
            //else
            //{
            //    return null;
            //}
        }

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
                //Sort sort = new Sort(new SortField[] { SortField.FIELD_SCORE, new SortField("AddTime", SortField.DOC, true) });//按照分数倒叙排序,再按照时间倒叙排序
                Sort sort = new Sort(new SortField[] { SortField.FIELD_SCORE});//按照分数倒叙排序
                docs = search.Search(bQuery, (Lucene.Net.Search.Filter)null, 1000, sort);
                stopwatch.Stop();
                //if (docs != null && docs.totalHits > 0)
                //{
                //    for (int i = 0; i < docs.totalHits; i++)
                //    {
                //        公告 model = new 公告();
                //        model.Id = long.Parse(search.Doc(docs.scoreDocs[i].doc).Get("NumId"));
                //        model.内容主体.标题 = search.Doc(docs.scoreDocs[i].doc).Get("Title");
                //        model.公告信息.公告类别 = (公告.公告类别)Enum.Parse(typeof(公告.公告类别), search.Doc(docs.scoreDocs[i].doc).Get("AdClass"));
                //        model.公告信息.公告性质 = (公告.公告性质)Enum.Parse(typeof(公告.公告性质), search.Doc(docs.scoreDocs[i].doc).Get("AdFeature"));
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
        private 公告 SetHighlighter(Dictionary<string, string> dicKeywords, 公告 model)
        {
            SimpleHTMLFormatter simpleHTMLFormatter = new PanGu.HighLight.SimpleHTMLFormatter("<font color=\"red\">", "</font>");
            Highlighter highlighter = new PanGu.HighLight.Highlighter(simpleHTMLFormatter, new Segment());
            highlighter.FragmentSize = 50;
            string strTitle = string.Empty;
            dicKeywords.TryGetValue("title", out strTitle);
            if (!string.IsNullOrEmpty(strTitle))
            {
                string title = highlighter.GetBestFragment(strTitle, model.内容主体.标题);
                if (!string.IsNullOrEmpty(title))
                    model.内容主体.标题 = title;
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
