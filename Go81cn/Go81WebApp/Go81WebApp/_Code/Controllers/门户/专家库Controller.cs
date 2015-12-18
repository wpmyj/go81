using Go81WebApp.Models.管理器;
using Go81WebApp.Models.管理器.抽选管理;
using Go81WebApp.Models.数据模型;
using Go81WebApp.Models.数据模型.用户数据模型;
using MongoDB;
using MongoDB.Driver.Builders;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Go81WebApp.Models.数据模型.内容数据模型;
using Go81WebApp.Models.数据模型.消息数据模型;
using Newtonsoft.Json;
using MongoDB.Driver;

namespace Go81WebApp.Controllers.门户
{
    public class 专家库Controller : Controller
    {
        private static int PAGESIZE = int.Parse(ConfigurationManager.AppSettings["抽取列表每页显示个数"]);
        // GET: expert
        public ActionResult Index()
        {
#if !INTRANET
            ViewData["sum"]=用户管理.计数用户<专家>(0, 0);
            ViewData["techSum"] = 用户管理.计数用户<专家>(0, 0, MongoDB.Driver.Builders.Query.EQ("身份信息.专家类型", 专家类型.技术));
            ViewData["lawSum"] = 用户管理.计数用户<专家>(0, 0, MongoDB.Driver.Builders.Query.EQ("身份信息.专家类型", 专家类型.法律));
            ViewData["comSum"] = 用户管理.计数用户<专家>(0, 0, MongoDB.Driver.Builders.Query.EQ("身份信息.专家类型", 专家类型.经济));

            //通知部分

            通知 model = 通知管理.查找通知(30);
            ViewData["专家通知列表"] = 通知管理.查询通知(0, 0, Query<通知>.Where(o => o.通知信息.通知所属 == 通知.通知所属.专家));

            return View(model);
#else
            return View("IndexIntranet");
#endif
        }

#if INTRANET
        private 单位用户 currentUser
        {
            get
            {
                return this.HttpContext.获取当前用户<单位用户>();
            }
        }
#if INTRANET
        public class Expert
        {
            public long ID { get; set; }
            /// <summary>
            /// 姓名
            /// </summary>
            public string Name { get; set; }
            /// <summary>
            /// 职称
            /// </summary>
            public string Minzu { get; set; }
            /// <summary>
            /// 专家类别
            /// </summary>
            public string type { get; set; }
            /// <summary>
            /// 专家级别
            /// </summary>
            public string Zjjb { get; set; }
            /// <summary>
            /// 审核状态
            /// </summary>
            public string Shzt { get; set; }
            public string rk { get; set; }
        }
        //根据专家姓名查找专家
        public JsonResult SearchExpert()
        {
            try
            {
                IMongoQuery q = null;
                var name = Request.Params["expertname"];
                string status = Request.Params["sta"];
                if(!string.IsNullOrWhiteSpace(status)&&status!="0")
                {
                    q = Query.And(Query<专家>.Where(m => (int)m.身份信息.专家类型 == int.Parse(status)));
                }
                if(!string.IsNullOrWhiteSpace(name))
                {
                    q = Query.And(Query<专家>.Where(m => m.身份信息.姓名 == name));
                }
                List<Expert> elist = new List<Expert>();
                long pageCount = 0;
                int cpg = 1;
                if (!string.IsNullOrWhiteSpace(Request.QueryString["cp"]))
                {
                    cpg = int.Parse(Request.QueryString["cp"]);
                }
                string names = name.Trim();
                pageCount = 用户管理.计数用户<专家>(0, 0,q) / 15;
                if (用户管理.计数用户<专家>(0, 0,q) % 15 > 0) 
                {
                    pageCount++;
                }
                IEnumerable<专家> _zj = 用户管理.查询用户<专家>(15 * (cpg - 1), 15,q);
                foreach (var k in _zj)
                {
                    Expert exp = new Expert();
                    exp.ID = k.Id;
                    exp.Name=k.身份信息.姓名;
                    exp.Zjjb=k.身份信息.专家级别.ToString();
                    exp.Minzu = k.身份信息.民族.ToString();
                    exp.type = k.身份信息.专家类型.ToString();
                    exp.Shzt = k.审核数据.审核状态.ToString();
                    if(k.入库类型!= 入库类型.入库)
                    {
                        exp.rk = "未入库";
                    }
                    else
                    {
                        exp.rk = "已入库";
                    }
                    elist.Add(exp);
                }
                JsonResult json = new JsonResult() { Data = new { expert = elist,pCount=pageCount } };
                return Json(json, JsonRequestBehavior.AllowGet);
            }
            catch
            {
                JsonResult json = new JsonResult() { Data = new { expert = new List<Expert>(),pCount=0 } };
                return Json(json, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult ExpertList()
        {
            IEnumerable<专家> exp = null;
            try
            {
                long pgCount = 0;
                int cpg = 0;
                string area = Request.QueryString["area"];
                if (!string.IsNullOrWhiteSpace(Request.QueryString["page"]))
                {
                    cpg = int.Parse(Request.QueryString["page"]);
                }
                if (cpg <= 0)
                {
                    cpg = 1;
                }
                long pc = 用户管理.计数用户<专家>(0, 0, Query<专家>.Where(m => m.所属地域.省份.Contains(area)));
                pgCount = pc / 15;
                if (pc % 15 > 0)
                {
                    pgCount++;
                }
                ViewData["Pagecount"] = pgCount;
                ViewData["CurrentPage"] = cpg;
                exp = 用户管理.查询用户<专家>(15 * (cpg - 1), 15, Query<专家>.Where(m => m.所属地域.省份.Contains(area)));
                ViewData["supplier"] = exp;
                ViewData["area"] = area;
                return View();
            }
            catch
            {
                ViewData["supplier"] = exp;
                return View();
            }
        }
#endif

        /// <summary>
        /// 内网专家库首页
        /// </summary>
        /// <returns>视图位置</returns>
        public ActionResult IndexIntranet()
        {
            return View();
        }
        public long CountExpert()
        {
            try
            {
                string area = Request.QueryString["area"];
                long count = 用户管理.计数用户<专家>(0, 0, Query<专家>.Where(m => m.所属地域.省份.Contains(area)));
                return count;
            }
            catch
            {
                return -1;
            }
        }
        /// <summary>
        /// 内网专家库首页--分布页
        /// </summary>
        /// <returns>分布页路径</returns>
        public ActionResult Part_IndexIntranet()
        {
            ViewData["登录"] = "0";
            if (HttpContext.检查登录() != -1)
            {
                ViewData["登录"] = "1";
                ViewData["单位用户登录"] = HttpContext.检查登录() <= 100000000000 ? "1" : "0";
            }
            return PartialView("Part_Expert/Part_IndexIntranet");
        }


        public ActionResult Part_Expert_Applay()
        {
            ViewData["goodType"] = 专家可评标专业分类.评审专业;// 商品分类管理.查找子分类();
            ViewData["经办人"] = HttpContext.获取当前用户<单位用户>().联系方式.联系人;

            var t = typeof(专业技术职称);
            var vs = Enum.GetValues(t);
            var d = new Dictionary<string, int>();
            foreach (var v in vs)
            {
                d.Add(Enum.GetName(t, v), (int)v);
            }
            ViewData["专业技术职称"] = d;

            t = typeof(学位);
            vs = Enum.GetValues(t);
            d = new Dictionary<string, int>();
            foreach (var v in vs)
            {
                d.Add(Enum.GetName(t, v), (int)v);
            }
            ViewData["最高学位"] = d;

            t = typeof(学历);
            vs = Enum.GetValues(t);
            d = new Dictionary<string, int>();
            foreach (var v in vs)
            {
                d.Add(Enum.GetName(t, v), (int)v);
            }
            ViewData["最高学历"] = d;

            t = typeof(专家类型);
            vs = Enum.GetValues(t);
            d = new Dictionary<string, int>();
            foreach (var v in vs)
            {
                d.Add(Enum.GetName(t, v), (int)v);
            }
            ViewData["专家类型"] = d;

            t = typeof(专家类别);
            vs = Enum.GetValues(t);
            d = new Dictionary<string, int>();
            foreach (var v in vs)
            {
                d.Add(Enum.GetName(t, v), (int)v);
            }
            ViewData["专家类别"] = d;

            t = typeof(专家级别);
            vs = Enum.GetValues(t);
            d = new Dictionary<string, int>();
            foreach (var v in vs)
            {
                d.Add(Enum.GetName(t, v), (int)v);
            }
            ViewData["专家级别"] = d;

            //ViewData["专家特殊类别"] = 专家可评标专业.非商品分类评审专业;
            var 所属单位 = 用户管理.查找用户<单位用户>(currentUser.Id).单位信息.所属单位;
            ViewData["所属单位"] = 所属单位 == null ? "" : 所属单位;

            return PartialView("Part_Expert/Part_Expert_Applay");
        }

        [单一权限验证(权限.新增评审专家抽取申请)]
        public ActionResult Expert_Applay()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Expert_Applay(int? id)
        {
            专家抽选记录 model = new 专家抽选记录
            {
                申请时间 = DateTime.Now,
                项目编号 = Request.Form["pro_num"],
                项目名称 = Request.Form["pro_name"],
                评标时间 = Convert.ToDateTime(Request.Form["pro_time"]),
                总计预定抽选专家数 = int.Parse(Request.Form["预定抽选总数"]),
                操作人姓名 = Request.Form["prooperate_name"],
                操作人电话 = Request.Form["prooperate_tel"]
            };
            model.经办人.用户ID = this.HttpContext.获取当前用户<单位用户>().Id;
            model.申请抽选状态 = 申请抽选状态.已提交待批准;

            //屏蔽列表
            var outlist = Request.Form["outlistcontent"];
            var l = new List<用户链接<专家>>();
            if (!string.IsNullOrWhiteSpace(outlist))
            {
                foreach (var item in outlist.Split(new[] { "|" }, StringSplitOptions.RemoveEmptyEntries))
                {
                    var z = new 用户链接<专家>
                    {
                        用户ID = long.Parse(item)
                    };
                    l.Add(z);
                }
            }
            model.回避专家列表 = l;

            int count = int.Parse(Request.Form["条件总数"]);
            var condition = new List<专家抽选记录._专家抽选条件>();
            for (int i = 1; i <= count; i++)
            {
                var t_condition = new 专家抽选记录._专家抽选条件();
                t_condition.需要专家数量 = int.Parse(Request.Form["预定抽选个数__" + i]);

                //专家类别
                var tempparmlist = Request.Form["专家类别__" + i];
                if (!string.IsNullOrWhiteSpace(tempparmlist))
                {
                    foreach (var temp in tempparmlist.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        t_condition.专家类别.Add((专家类别)(int.Parse(temp)));
                    }
                }

                //专家级别
                tempparmlist = Request.Form["专家级别__" + i];
                if (!string.IsNullOrWhiteSpace(tempparmlist))
                {
                    foreach (var temp in tempparmlist.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        t_condition.专家级别.Add((专家级别)(int.Parse(temp)));
                    }
                }

                //专家类型
                tempparmlist = Request.Form["专家类型__" + i];
                if (!string.IsNullOrWhiteSpace(tempparmlist))
                {
                    foreach (var temp in tempparmlist.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        t_condition.专家类型.Add((专家类型)(int.Parse(temp)));
                    }
                }
                //最高学历
                tempparmlist = Request.Form["最高学历__" + i];
                if (!string.IsNullOrWhiteSpace(tempparmlist))
                {
                    foreach (var temp in tempparmlist.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        t_condition.学历要求.Add((学历)(int.Parse(temp)));
                    }
                }
                //最高学位
                tempparmlist = Request.Form["最高学位__" + i];
                if (!string.IsNullOrWhiteSpace(tempparmlist))
                {
                    foreach (var temp in tempparmlist.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        t_condition.学位要求.Add((学位)(int.Parse(temp)));
                    }
                }
                //专业技术职称
                tempparmlist = Request.Form["专业技术职称__" + i];
                if (!string.IsNullOrWhiteSpace(tempparmlist))
                {
                    if (tempparmlist != "-100")
                    {
                        foreach (var temp in tempparmlist.Split(new[] { "|" }, StringSplitOptions.RemoveEmptyEntries))
                        {
                            t_condition.专业技术职称.Add((专业技术职称)(int.Parse(temp)));
                        }
                    }
                }

                //可评标物质类别匹配模式
                tempparmlist = Request.Form["可评标物质类别匹配模式__" + i];
                if (!string.IsNullOrWhiteSpace(tempparmlist) && tempparmlist == "1")
                {
                    t_condition.模糊查询 = Request.Form["可评标物质类别__" + i];
                }
                else
                {
                    //可评标物质类别
                    tempparmlist = Request.Form["可评标物质类别__" + i];
                    if (!string.IsNullOrWhiteSpace(tempparmlist))
                    {
                        var t = new 专家抽选记录._专家抽选条件._评标产品类别();
                        var 可评标物质类别 = tempparmlist.Split(new[] { "#" }, StringSplitOptions.RemoveEmptyEntries);
                        if (可评标物质类别[1] == "1")
                        {
                            t.二级分类可用专家不足返回一级分类 = true;
                        }
                        else
                        {
                            t.二级分类可用专家不足返回一级分类 = false;
                        }

                        var p = 可评标物质类别[0].Split(new[] { "*" }, StringSplitOptions.RemoveEmptyEntries);
                        t.一级分类 = p[0];
                        t.二级分类 = new List<string>();
                        for (var j = 1; j < p.Length; j++)
                        {
                            t.二级分类.Add(p[j]);
                        }

                        t_condition.可评标产品类别.Add(t);
                    }
                }

                //抽选描述
                t_condition.描述 = Request.Form["抽选描述__" + i];
                //所在地域
                t_condition.所在地区.省份 = Request.Form["省份__" + i];
                t_condition.所在地区.城市 = Request.Form["城市__" + i];
                t_condition.所在地区.区县 = Request.Form["区县__" + i];

                condition.Add(t_condition);
            }
            model.专家抽选条件 = condition;
            专家抽选管理.添加专家抽选历史(model);

            //站内消息
            站内消息 Msg = new 站内消息();
            对话消息 Talk = new 对话消息();

            Msg.重要程度 = 重要程度.特别重要;
            Msg.消息类型 = 消息类型.普通;
            var u_id = this.HttpContext.获取当前用户<单位用户>().Id;

            Msg.发起者.用户ID = u_id;
            Talk.发言人.用户ID = u_id;
            站内消息管理.添加站内消息(Msg, u_id, 10002);
            Talk.消息主体.标题 = "待审核抽取评审专家";
            Talk.消息主体.内容 = "有一条待审核的申请抽取评审专家未处理，<a style='color:red;text-decoration:underline;' href='/专家抽选/Expert_ApplayAuditList'>点击这里进行审核</a>";
            对话消息管理.添加对话消息(Talk, Msg);

            return RedirectToAction("IndexIntranet");
        }

        [单一权限验证(权限.我可进行的评审专家抽取申请)]
        public ActionResult Expert_Choose()
        {
            return View();
        }

        public ActionResult Part_Expert_Choose(int? id)
        {
            专家抽选记录 model = new 专家抽选记录();
            try
            {
                model = 专家抽选管理.查找专家抽选记录((long)id);
                var str = "";
                foreach (var m in model.专家抽选条件)
                {
                    str += m.需要专家数量 + ",";
                }
                ViewData["抽取总数列表"] = str.Substring(0, str.Length - 1);
            }
            catch
            {
                return Content("<script>alert('页面有误');location.href='javascript:history.go(-1)';</script>");
            }
            return PartialView("Part_Expert/Part_Expert_Choose", model);
        }













        public ActionResult Part_Expert_Applay_S()
        {
            int page = 1;
            int pre_listcount = 专家抽选管理.查询专家抽选记录(0, 0, Query.EQ("申请抽选状态", 申请抽选状态.已提交待批准).And(Query<专家抽选记录>.EQ(o => o.经办人.用户ID, HttpContext.获取当前用户<单位用户>().Id))).Count();
            int pre_maxpage = Math.Max((pre_listcount + PAGESIZE - 1) / PAGESIZE, 1);
            ViewData["pre_currentPage"] = page;
            ViewData["pre_pagecount"] = pre_maxpage;
            ViewData["专家抽取待批准列表"] = 专家抽选管理.查询专家抽选记录(0, PAGESIZE, Query.EQ("申请抽选状态", 申请抽选状态.已提交待批准).And(Query<专家抽选记录>.EQ(o => o.经办人.用户ID, HttpContext.获取当前用户<单位用户>().Id)));


            pre_listcount = 专家抽选管理.查询专家抽选记录(0, 0, Query.EQ("申请抽选状态", 申请抽选状态.已批准待抽选).And(Query<专家抽选记录>.EQ(o => o.经办人.用户ID, HttpContext.获取当前用户<单位用户>().Id))).Count();
            pre_maxpage = Math.Max((pre_listcount + PAGESIZE - 1) / PAGESIZE, 1);
            ViewData["ing_currentPage"] = page;
            ViewData["ing_pagecount"] = pre_maxpage;
            ViewData["专家抽取已批准列表"] = 专家抽选管理.查询专家抽选记录(0, PAGESIZE, Query.EQ("申请抽选状态", 申请抽选状态.已批准待抽选).And(Query<专家抽选记录>.EQ(o => o.经办人.用户ID, HttpContext.获取当前用户<单位用户>().Id)));

            pre_listcount = 专家抽选管理.查询专家抽选记录(0, 0, Query.EQ("申请抽选状态", 申请抽选状态.已完成抽选).And(Query<专家抽选记录>.EQ(o => o.经办人.用户ID, HttpContext.获取当前用户<单位用户>().Id))).Count();
            pre_maxpage = Math.Max((pre_listcount + PAGESIZE - 1) / PAGESIZE, 1);
            ViewData["ed_currentPage"] = page;
            ViewData["ed_pagecount"] = pre_maxpage;
            ViewData["已完成的抽选列表"] = 专家抽选管理.查询专家抽选记录(0, PAGESIZE, Query.EQ("申请抽选状态", 申请抽选状态.已完成抽选).And(Query<专家抽选记录>.EQ(o => o.经办人.用户ID, HttpContext.获取当前用户<单位用户>().Id)));

            pre_listcount = 专家抽选管理.查询专家抽选记录(0, 0, Query.EQ("申请抽选状态", 申请抽选状态.未获批准).And(Query<专家抽选记录>.EQ(o => o.经办人.用户ID, HttpContext.获取当前用户<单位用户>().Id))).Count();
            pre_maxpage = Math.Max((pre_listcount + PAGESIZE - 1) / PAGESIZE, 1);
            ViewData["no_currentPage"] = page;
            ViewData["no_pagecount"] = pre_maxpage;
            ViewData["专家抽取未获批准列表"] = 专家抽选管理.查询专家抽选记录(0, PAGESIZE, Query.EQ("申请抽选状态", 申请抽选状态.未获批准).And(Query<专家抽选记录>.EQ(o => o.经办人.用户ID, HttpContext.获取当前用户<单位用户>().Id)));

            return PartialView("Part_Expert/Part_Expert_Applay_S");

        }

        public ActionResult Part_Expert_Applay_S_pre(int? page)
        {
            int listcount = 专家抽选管理.查询专家抽选记录(0, 0, Query.EQ("申请抽选状态", 申请抽选状态.已提交待批准).And(Query<专家抽选记录>.EQ(o => o.经办人.用户ID, HttpContext.获取当前用户<单位用户>().Id))).Count();
            int maxpage = Math.Max((listcount + PAGESIZE - 1) / PAGESIZE, 1);
            if (string.IsNullOrEmpty(page.ToString()) || page < 0 || page > maxpage)
            {
                page = 1;
            }

            ViewData["pre_currentPage"] = page;
            ViewData["pre_pagecount"] = maxpage;
            ViewData["专家抽取待批准列表"] = 专家抽选管理.查询专家抽选记录(PAGESIZE * (int.Parse(page.ToString()) - 1), PAGESIZE, Query.EQ("申请抽选状态", 申请抽选状态.已提交待批准).And(Query<专家抽选记录>.EQ(o => o.经办人.用户ID, HttpContext.获取当前用户<单位用户>().Id)));

            return PartialView("Part_Expert/Part_Expert_Applay_S_pre");
        }

        public ActionResult Part_Expert_Applay_S_ing(int? page)
        {
            int listcount = 专家抽选管理.查询专家抽选记录(0, 0, Query.EQ("申请抽选状态", 申请抽选状态.已批准待抽选).And(Query<专家抽选记录>.EQ(o => o.经办人.用户ID, HttpContext.获取当前用户<单位用户>().Id))).Count();
            int maxpage = Math.Max((listcount + PAGESIZE - 1) / PAGESIZE, 1);

            if (string.IsNullOrEmpty(page.ToString()) || page < 0 || page > maxpage)
            {
                page = 1;
            }
            ViewData["ing_currentPage"] = page;
            ViewData["ing_pagecount"] = maxpage;
            ViewData["专家抽取已批准列表"] = 专家抽选管理.查询专家抽选记录(PAGESIZE * (int.Parse(page.ToString()) - 1), PAGESIZE, Query.EQ("申请抽选状态", 申请抽选状态.已批准待抽选).And(Query<专家抽选记录>.EQ(o => o.经办人.用户ID, HttpContext.获取当前用户<单位用户>().Id)));

            return PartialView("Part_Expert/Part_Expert_Applay_S_ing");
        }

        public ActionResult Part_Expert_Applay_S_no(int? page)
        {
            int listcount = 专家抽选管理.查询专家抽选记录(0, 0, Query.EQ("申请抽选状态", 申请抽选状态.未获批准).And(Query<专家抽选记录>.EQ(o => o.经办人.用户ID, HttpContext.获取当前用户<单位用户>().Id))).Count();
            int maxpage = Math.Max((listcount + PAGESIZE - 1) / PAGESIZE, 1);
            if (string.IsNullOrEmpty(page.ToString()) || page < 0 || page > maxpage)
            {
                page = 1;
            }
            ViewData["no_currentPage"] = page;
            ViewData["no_pagecount"] = maxpage;
            ViewData["专家抽取未获批准列表"] = 专家抽选管理.查询专家抽选记录(PAGESIZE * (int.Parse(page.ToString()) - 1), PAGESIZE, Query.EQ("申请抽选状态", 申请抽选状态.未获批准).And(Query<专家抽选记录>.EQ(o => o.经办人.用户ID, HttpContext.获取当前用户<单位用户>().Id)));

            return PartialView("Part_Expert/Part_Expert_Applay_S_no");
        }

        public ActionResult Part_Expert_Applay_S_ed(int? page)
        {
            int listcount = 专家抽选管理.查询专家抽选记录(0, 0, Query.EQ("申请抽选状态", 申请抽选状态.已完成抽选).And(Query<专家抽选记录>.EQ(o => o.经办人.用户ID, HttpContext.获取当前用户<单位用户>().Id))).Count();
            int maxpage = Math.Max((listcount + PAGESIZE - 1) / PAGESIZE, 1);
            if (string.IsNullOrEmpty(page.ToString()) || page < 0 || page > maxpage)
            {
                page = 1;
            }
            ViewData["ed_currentPage"] = page;
            ViewData["ed_pagecount"] = maxpage;
            ViewData["已完成的抽选列表"] = 专家抽选管理.查询专家抽选记录(PAGESIZE * (int.Parse(page.ToString()) - 1), PAGESIZE, Query.EQ("申请抽选状态", 申请抽选状态.已完成抽选).And(Query<专家抽选记录>.EQ(o => o.经办人.用户ID, HttpContext.获取当前用户<单位用户>().Id)));

            return PartialView("Part_Expert/Part_Expert_Applay_S_ed");
        }
        [HttpPost]
        public ActionResult Expert_Choose_PrintStatus()
        {
            try
            {
                var id = Request.Params["id"];
                var h = 专家抽选管理.查找专家抽选记录(long.Parse(id));
                h.是否已打印 = true;
                专家抽选管理.更新专家抽选记录(h, false);
                return Content("1");
            }
            catch
            {
                return Content("0");
            }
        }
        public ActionResult Expert_Choose_Print()
        {
            return View();
        }
        public ActionResult Part_Expert_Choose_Print(int? id)
        {
            try
            {
                if (id == null)
                {
                    return Content("<script>window.location='/专家库/IndexIntranet'</script>");
                }
                else
                {
                    专家抽选记录 model = 专家抽选管理.查找专家抽选记录((long)id);
                    if (model != null)
                    {
                        return PartialView("Part_Expert/Part_Expert_Choose_Print", model);
                    }
                    else
                    {
                        return Content("<script>window.location='/专家库/IndexIntranet'</script>");
                    }
                }
            }
            catch
            {
                return Content("<script>window.location='/专家库/IndexIntranet'</script>");
            }
        }

        public ActionResult Expert_HistoryDetail()
        {
            return View();
        }
        public ActionResult Part_Expert_HistoryDetail(int? id)
        {
            try
            {
                if (id == null)
                {
                    return Content("<script>window.location='/专家库/IndexIntranet'</script>");
                }
                else
                {
                    专家抽选记录 model = 专家抽选管理.查找专家抽选记录((long)id);
                    if (model != null)
                    {
                        return PartialView("Part_Expert/Part_Expert_HistoryDetail", model);
                    }
                    else
                    {
                        return Content("<script>window.location='/专家库/IndexIntranet'</script>");
                    }
                }
            }
            catch
            {
                return Content("<script>window.location='/专家库/IndexIntranet'</script>");
            }
        }




        public ActionResult Expert_Scoring()
        {
            return View();
        }
        public ActionResult Part_Expert_Scoring(long? id)
        {
            try
            {
                if (id == null)
                {
                    return Content("<script>window.location='/专家库/IndexIntranet'</script>");
                }
                else
                {
                    专家抽选记录 model = 专家抽选管理.查找专家抽选记录((long)id);
                    if (model != null)
                    {
                        return PartialView("Part_Expert/Part_Expert_Scoring", model);
                    }
                    else
                    {
                        return Content("<script>window.location='/专家库/IndexIntranet'</script>");
                    }
                }
            }
            catch
            {
                return Content("<script>window.location='/专家库/IndexIntranet'</script>");
            }
        }
        [HttpPost]
        public ActionResult Expert_Scoring(long? id)
        {
            try
            {
                var parmstr = Request.Form["parmstr"];
                专家抽选记录 model = 专家抽选管理.查找专家抽选记录((long)id);
                if (!string.IsNullOrWhiteSpace(parmstr) && !model.是否已评分 && model.抽选专家列表 != null)
                {
                    var scoretemplist = parmstr.Split(new[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
                    var i = 0;
                    foreach (var m in model.抽选专家列表)
                    {
                        if (m.预定出席)
                        {
                            var scorelist = scoretemplist[i].Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                            m.专家评分.专家水平评分 = (专家水平评分)int.Parse(scorelist[0]);
                            m.专家评分.专家评标态度评分 = (专家评标态度评分)int.Parse(scorelist[1]);
                            i++;
                        }
                    }
                    model.是否已评分 = true;
                    专家抽选管理.更新专家抽选记录(model, false);
                }
            }
            catch
            {

            }
            return RedirectToAction("IndexIntranet");
        }

        public ActionResult Expert_ApplayCancel()
        {
            return View();
        }
        public ActionResult Part_Expert_ApplayCancel(int? id)
        {
            专家抽选记录 model = new 专家抽选记录();
            if (id != null)
            {
                model = 专家抽选管理.查找专家抽选记录((long)id);
            }
            return PartialView("Part_Expert/Part_Expert_ApplayCancel", model);
        }
        public ActionResult Expert_ApplayCancelAction(long? id)
        {
            try
            {
                var model = 专家抽选管理.查找专家抽选记录((long)id);
                model.基本数据.已屏蔽 = true;
                专家抽选管理.更新专家抽选记录(model, false);
            }
            catch
            {

            }
            return View("IndexIntranet");
        }
#endif
    }
}