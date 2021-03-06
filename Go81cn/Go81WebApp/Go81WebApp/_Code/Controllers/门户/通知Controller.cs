﻿using Go81WebApp.Models.管理器;
using Go81WebApp.Models.数据模型;
using Go81WebApp.Models.数据模型.内容数据模型;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.PanGu;
using Lucene.Net.QueryParsers;
using Lucene.Net.Search;
using MongoDB;
using MongoDB.Driver.Builders;
using PanGu;
using PanGu.HighLight;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace Go81WebApp.Controllers.门户
{
    public class 通知Controller : Controller
    {
        private static int PAGESIZE = int.Parse(ConfigurationManager.AppSettings["通知每页显示条数"]);

        [静态化]
        public ActionResult NoticeList()
        {
            return View();
        }
        public ActionResult NoticeDetail()
        {
            return View();
        }
        public ActionResult NoticeSearch()
        {
            return View();
        }
        public ActionResult Exposure()
        {
            return View();
        }
        public ActionResult Part_Exposure(int? page)
        {
            int p = 1;
            try
            {
                p = int.Parse(page.ToString());
            }
            catch
            {
                p = 1;
            }
            
            int listcount = (int)(通知管理.计数通知(0, 0, Query<通知>.Where(o=>o.审核数据.审核状态 == 审核状态.审核通过 && (o.通知信息.通知所属 == 通知.通知所属.黑名单 || o.通知信息.通知所属 == 通知.通知所属.光荣榜)), false));
            int maxpage = Math.Max((listcount + PAGESIZE - 1) / PAGESIZE, 1);
            if (p > maxpage)
            {
                p = 1;
            }


            ViewData["page"] = p;
            ViewData["listcount"] = listcount;
            ViewData["pagesize"] = PAGESIZE;
            ViewData["maxpage"] = maxpage;

            //通知列表
            ViewData["曝光台列表"] = 通知管理.查询通知(PAGESIZE * (p - 1), PAGESIZE, Query<通知>.Where(o => o.审核数据.审核状态 == 审核状态.审核通过 && (o.通知信息.通知所属 == 通知.通知所属.黑名单 || o.通知信息.通知所属 == 通知.通知所属.光荣榜)), false, SortBy.Descending("内容主体.发布时间"), false);
            return PartialView("Part_Notice/Part_Exposure");
        }
        public ActionResult Part_SearchByPageExposure()
        {
            int page = 1;
            int.TryParse(Request.Params["page"], out page);

            int listcount = (int)(通知管理.计数通知(0, 0, MongoDB.Driver.Builders.Query.EQ("通知信息.通知所属", 通知.通知所属.黑名单), false));
            int maxpage = Math.Max((listcount + PAGESIZE - 1) / PAGESIZE, 1);

            ViewData["currentpage"] = page;
            ViewData["pagecount"] = maxpage;

            //通知列表
            ViewData["曝光台列表"] = 通知管理.查询通知(PAGESIZE * (page - 1), PAGESIZE, MongoDB.Driver.Builders.Query.EQ("通知信息.通知所属",通知.通知所属.黑名单), false, SortBy.Descending("内容主体.发布时间"), false);
            return PartialView("Part_Notice/Part_SearchByPageExposure");
        }
        public ActionResult ExposureDetail()
        {
            return View();
        }
        public ActionResult Part_ExposureDetail()
        {
            通知 model = null;
            try
            {
                model = 通知管理.查找通知(int.Parse(Request.QueryString["id"]));
            }
            catch
            {

            }
            return PartialView("Part_Notice/Part_ExposureDetail",model);
        }
        public ActionResult Part_NoticeList()
        {
            int listcount = (int)(通知管理.计数通知(0, 0,Query<通知>.EQ(o=>o.审核数据.审核状态, 审核状态.审核通过),false));
            int maxpage = Math.Max((listcount + PAGESIZE - 1) / PAGESIZE, 1);

            ViewData["currentpage"] = 1;
            ViewData["pagecount"] = maxpage;

            //通知列表
            ViewData["通知列表"] = 通知管理.查询通知(0, PAGESIZE, Query<通知>.EQ(o => o.审核数据.审核状态, 审核状态.审核通过), false, SortBy.Descending("内容主体.发布时间"), false);
            return PartialView("Part_Notice/Part_NoticeList");
        }
        public ActionResult Part_SearchByPage()
        {
            int page = 1;
            int.TryParse(Request.Params["page"], out page);

            int listcount = (int)(通知管理.计数通知(0, 0, Query<通知>.EQ(o => o.审核数据.审核状态, 审核状态.审核通过), false));
            int maxpage = Math.Max((listcount + PAGESIZE - 1) / PAGESIZE, 1);

            ViewData["currentpage"] = page;
            ViewData["pagecount"] = maxpage;

            //通知列表
            ViewData["通知列表"] = 通知管理.查询通知(PAGESIZE * (page - 1), PAGESIZE, Query<通知>.EQ(o => o.审核数据.审核状态, 审核状态.审核通过), false, SortBy.Descending("内容主体.发布时间"), false);
            return PartialView("Part_Notice/Part_SearchByPage");
        }

        public ActionResult Part_NoticeDetail()
        {
            //通知详情
            通知 model = null;
            try
            {
                model = 通知管理.查找通知(int.Parse(Request.QueryString["id"]));
            }
            catch
            {

            }
            return PartialView("Part_Notice/Part_NoticeDetail", model);
        }
        public ActionResult Part_OtherList()
        {
            ViewData["最近通知"] = 通知管理.查询通知(0, 10, Query<通知>.EQ(o=>o.审核数据.审核状态 , 审核状态.审核通过), false, SortBy.Descending("内容主体.发布时间"));
            return PartialView("Part_Notice/Part_OtherList");
        }

        [HttpPost]
        public ActionResult NoticeListSearch()
        {
            return View("NoticeSearch");
        }
        public ActionResult Part_NoticeSearch()
        {
            ViewData["searchtext"] = Request.Form["searchtext"];

            TopDocs serchalllist = SearchIndex("/Lucene.Net/IndexDic/Tongzhi", Request.Form["searchtext"]);
            int page = 1;

            ViewData["currentpage"] = page;
            ViewData["pagecount"] = page;

            IList<通知> serchlist = new List<通知>();
            var listcount = serchalllist.totalHits > 1000 ? 1000 : serchalllist.totalHits;
            if (serchalllist != null && listcount > 0)
            {
                int maxpage = Math.Max((listcount + PAGESIZE - 1) / PAGESIZE, 1);

                int length = PAGESIZE;
                if (maxpage == page && listcount % PAGESIZE != 0)
                    length = listcount % PAGESIZE;

                //IndexSearcher search = new IndexSearcher(IndexDic("/Lucene.Net/IndexDic/Tongzhi"), true);
                IndexSearcher search = new IndexSearcher(new Lucene.Net.Store.SimpleFSDirectory(new System.IO.DirectoryInfo(IndexDic("/Lucene.Net/IndexDic/Tongzhi"))), true);
                Dictionary<string, string> dic = new Dictionary<string, string>();
                dic.Add("title", Request.Form["searchtext"]);
                for (int i = 0; i < length; i++)
                {
                    通知 model = new 通知();
                    model.Id = long.Parse(search.Doc(serchalllist.scoreDocs[i].doc).Get("NumId"));
                    model.内容主体.标题 = search.Doc(serchalllist.scoreDocs[i].doc).Get("Title");
                    model.内容主体.发布时间 = Convert.ToDateTime(search.Doc(serchalllist.scoreDocs[i].doc).Get("AddTime"));
                    serchlist.Add(SetHighlighter(dic, model));
                }
                //ViewData["currentpage"] = page;
                ViewData["pagecount"] = maxpage;
            }
            ViewData["通知搜索显示列表"] = serchlist;

            return PartialView("Part_Notice/Part_NoticeSearch");
        }
        public ActionResult Part_SearchByCondition()
        {
            int page = int.Parse(Request.Params["page"]);
            string keyword = Request.Params["keyword"];

            TopDocs serchalllist = SearchIndex("/Lucene.Net/IndexDic/Tongzhi", keyword);

            IList<通知> serchlist = new List<通知>();
            var listcount = serchalllist.totalHits > 1000 ? 1000 : serchalllist.totalHits;
            if (serchalllist != null && listcount > 0)
            {
                int maxpage = Math.Max((listcount + PAGESIZE - 1) / PAGESIZE, 1);

                int length = PAGESIZE;
                if (maxpage == page && listcount % PAGESIZE != 0)
                {
                    length = listcount % PAGESIZE;
                }

                int count = PAGESIZE * ((int)page - 1);
                //IndexSearcher search = new IndexSearcher(IndexDic("/Lucene.Net/IndexDic/Tongzhi"), true);
                IndexSearcher search = new IndexSearcher(new Lucene.Net.Store.SimpleFSDirectory(new System.IO.DirectoryInfo(IndexDic("/Lucene.Net/IndexDic/Tongzhi"))), true);
                Dictionary<string, string> dic = new Dictionary<string, string>();
                dic.Add("title", keyword);
                for (int i = count; i < count + length; i++)
                {
                    通知 model = new 通知();
                    model.Id = long.Parse(search.Doc(serchalllist.scoreDocs[i].doc).Get("NumId"));
                    model.内容主体.标题 = search.Doc(serchalllist.scoreDocs[i].doc).Get("Title");
                    model.内容主体.发布时间 = Convert.ToDateTime(search.Doc(serchalllist.scoreDocs[i].doc).Get("AddTime"));
                    serchlist.Add(SetHighlighter(dic, model));
                }

                ViewData["currentpage"] = page;
                ViewData["pagecount"] = maxpage;
            }
            ViewData["通知搜索显示列表"] = serchlist;

            return PartialView("Part_Notice/Part_SearchByCondition");
        }

        /// <summary>
        /// 从索引搜索结果
        /// </summary>
        private TopDocs SearchIndex(string indexdic, string keyword)
        {
            PanGu.Segment.Init(PanGuXmlPath);
            Dictionary<string, string> dic = new Dictionary<string, string>();
            BooleanQuery bQuery = new BooleanQuery();
            string title = string.Empty;
            if ((keyword != null && keyword != ""))
            {
                title = GetKeyWordsSplitBySpace(keyword);
                QueryParser parse = new MultiFieldQueryParser(Lucene.Net.Util.Version.LUCENE_29, new String[] { "Title", "Content" }, PanGuAnalyzer);
                Lucene.Net.Search.Query query = parse.Parse(title);
                parse.SetDefaultOperator(QueryParser.Operator.AND);
                bQuery.Add(query, BooleanClause.Occur.MUST);
                dic.Add("title", keyword);
            }
            if (bQuery != null && bQuery.GetClauses().Length > 0)
            {
                return GetSearchResult(bQuery, dic, indexdic);
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
        private TopDocs GetSearchResult(BooleanQuery bQuery, Dictionary<string, string> dicKeywords, string indexdic)
        {
            TopDocs docs = null;
            try
            {
                //IndexSearcher search = new IndexSearcher(IndexDic(indexdic), true);
                IndexSearcher search = new IndexSearcher(new Lucene.Net.Store.SimpleFSDirectory(new System.IO.DirectoryInfo(IndexDic(indexdic))), true);
                Stopwatch stopwatch = Stopwatch.StartNew();
                //SortField构造函数第三个字段true为降序,false为升序
                Sort sort = new Sort(new SortField[] { SortField.FIELD_SCORE, new SortField("AddTime", SortField.DOC, true) });//按照分数倒叙排序,再按照时间倒叙排序
                docs = search.Search(bQuery, (Lucene.Net.Search.Filter)null, 1000, sort);
                stopwatch.Stop();
                //if (docs != null && docs.totalHits > 0)
                //{
                //    for (int i = 0; i < docs.totalHits; i++)
                //    {
                //        通知 model = new 通知();
                //        model.Id = long.Parse(search.Doc(docs.scoreDocs[i].doc).Get("NumId"));
                //        model.内容主体.标题 = search.Doc(docs.scoreDocs[i].doc).Get("Title");
                //        model.基本数据.修改时间 = Convert.ToDateTime(search.Doc(docs.scoreDocs[i].doc).Get("AddTime"));
                //        //list.Add(model);
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
        private 通知 SetHighlighter(Dictionary<string, string> dicKeywords, 通知 model)
        {
            SimpleHTMLFormatter simpleHTMLFormatter = new PanGu.HighLight.SimpleHTMLFormatter("<font color=\"red\">", "</font>");
            Highlighter highlighter = new PanGu.HighLight.Highlighter(simpleHTMLFormatter, new Segment());
            highlighter.FragmentSize = 50;
            string strTitle = string.Empty;
            string strContent = string.Empty;
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
