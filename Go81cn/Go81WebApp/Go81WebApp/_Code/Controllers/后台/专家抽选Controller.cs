using System.Configuration;
using System.Net;
using System.Runtime.InteropServices.ComTypes;
using Gma.QrCodeNet.Encoding.DataEncodation;
using Go81WebApp.Models.管理器;
using Go81WebApp.Models.数据模型.用户数据模型;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using FileHelper;
using System.IO;
using Go81WebApp.Models.数据模型;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using MongoDB;
using Go81WebApp.Models.管理器.抽选管理;
using MongoDB.Driver.Linq;
using Go81WebApp.Models.数据模型.商品数据模型;
using Go81WebApp.Models.数据模型.消息数据模型;

namespace Go81WebApp.Controllers.后台
{
#if INTRANET
    [登录验证]
    [用户类型验证(typeof(单位用户),typeof(运营团队))]
    public class 专家抽选Controller : Controller
    {

        private static int PAGESIZE =  int.Parse(ConfigurationManager.AppSettings["抽取列表每页显示个数"]);
        private 单位用户 currentUser
        {
            get
            {
                return this.HttpContext.获取当前用户<单位用户>();
            }
        }
        public ActionResult Expert_list()
        {
            return View("/Views/默认主题/后台/单位用户后台/Expert_list.cshtml");
        }
        public ActionResult Expert_Add()
        {
            return View("/Views/默认主题/后台/单位用户后台/Expert_Add.cshtml");
        }
        public ActionResult Expert_Edit()
        {
            //ViewData["id"] = id;//待修改专家的id
            return View("/Views/默认主题/后台/单位用户后台/Expert_Edit.cshtml");
        }

        [单一权限验证(权限.评审专家全部抽取记录列表)]
        public ActionResult Expert_History()
        {
            ViewData["历史专家"] = 用户管理.查询用户<专家>(0, 0, Query.Null);
            return View("/Views/默认主题/后台/单位用户后台/Expert_History.cshtml");
        }
        public ActionResult Expert_History_Detail(long id)
        {
            ViewData["id"] = id;
            return View("/Views/默认主题/后台/单位用户后台/Expert_History_Detail.cshtml");
        }
        public ActionResult Expert_HistoryDetail()
        {
            return View("/Views/默认主题/后台/单位用户后台/Expert_HistoryDetail.cshtml");
        }
        public ActionResult Part_Expert_HistoryDetail(int? id)
        {
            if (Request.QueryString["c"] != null && Request.QueryString["c"] == "s")
            {
                ViewData["come"] = "我可进行的抽取申请";
            }
            else
            {
                ViewData["come"] = "全部抽取记录列表";
            }
            try
            {
                if (id == null)
                {
                    return Content("<script>window.location='/专家抽选/Expert_History'</script>");
                }
                else
                {
                    专家抽选记录 model = 专家抽选管理.查找专家抽选记录((long)id);
                    if (model != null)
                    {
                        return PartialView("/Views/默认主题/后台/单位用户后台/Procure_Part/Part_Expert_HistoryDetail.cshtml", model);
                    }
                    else
                    {
                        return Content("<script>window.location='/专家抽选/Expert_History'</script>");
                    }
                }
            }
            catch
            {
                return Content("<script>window.location='/专家抽选/Expert_History'</script>");
            }
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
            return View("/Views/默认主题/后台/单位用户后台/Expert_Choose_Print.cshtml");
        }
        public ActionResult Part_Expert_Choose_Print(int? id)
        {
            if (Request.QueryString["c"] != null && Request.QueryString["c"] == "s")
            {
                ViewData["come"] = "我可进行的抽取申请";
            }
            else if (Request.QueryString["c"] != null && Request.QueryString["c"] == "d")
            {
                ViewData["come"] = "我已完成的抽取申请";
            }
            else
            {
                ViewData["come"] = "全部抽取记录列表";
            }

            try
            {
                if (id == null)
                {
                    return Content("<script>window.location='/专家抽选/Expert_ApplayAuditList'</script>");
                }
                else
                {
                    专家抽选记录 model = 专家抽选管理.查找专家抽选记录((long)id);
                    if (model != null)
                    {
                        return PartialView("/Views/默认主题/后台/单位用户后台/Procure_Part/Part_Expert_Choose_Print.cshtml", model);
                    }
                    else
                    {
                        return Content("<script>window.location='/专家抽选/Expert_ApplayAuditList'</script>");
                    }
                }
            }
            catch
            {
                return Content("<script>window.location='/专家抽选/Expert_ApplayAuditList'</script>");
            }
        }
        public ActionResult Gys_Choose_Print()
        {
            return View("/Views/默认主题/后台/单位用户后台/Gys_Choose_Print.cshtml");
        }
        public ActionResult Part_Gys_Choose_Print(int? id)
        {
            if (Request.QueryString["c"] != null && Request.QueryString["c"] == "s")
            {
                ViewData["come"] = "我提交的供应商抽取申请";
            }
            else
            {
                ViewData["come"] = "供应商历史抽取记录";
            }
            try
            {
                if (id == null)
                {
                    return Content("<script>window.location='/专家抽选/GysChoose_ApplayAuditList'</script>");
                }
                else
                {
                    供应商抽选记录 model = 供应商抽选管理.查找供应商抽选历史记录((long)id);
                    if (model != null)
                    {
                        return PartialView("/Views/默认主题/后台/单位用户后台/Procure_Part/Part_Gys_Choose_Print.cshtml", model);
                    }
                    else
                    {
                        return Content("<script>window.location='/专家抽选/GysChoose_ApplayAuditList'</script>");
                    }
                }
            }
            catch
            {
                return Content("<script>window.location='/专家抽选/GysChoose_ApplayAuditList'</script>");
            }
        }
        public ActionResult Expert_Select()
        {
            return View("/Views/默认主题/后台/单位用户后台/Expert_Select.cshtml");
        }
        public ActionResult Expert_Choose()
        {
            return View("/Views/默认主题/后台/单位用户后台/Expert_Choose.cshtml");
        }
        public ActionResult Part_Expert_TreeMenu()
        {
            return PartialView("/Views/默认主题/后台/单位用户后台/Procure_Part/Part_Expert_TreeMenu.cshtml");
        }
        public ActionResult Part_Expert_list()
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
                long pc = 用户管理.计数用户<专家>(0, 0);
                pgCount = pc / 10;
                if (pc % 10 > 0)
                {
                    pgCount++;
                }
                ViewData["Pagecount"] = pgCount;
                ViewData["CurrentPage"] = cpg;
            ViewData["专家列表"] = 用户管理.查询用户<专家>(10 * (cpg- 1), 10);
            return PartialView("/Views/默认主题/后台/单位用户后台/Procure_Part/Part_Expert_list.cshtml");
        }
        public ActionResult Part_Expert_Edit()
        {
            try
            {
                if (Request.QueryString["id"] != null)
                {
                    long id = long.Parse(Request.QueryString["id"]);
                    ViewData["id"] = id;
                    ViewData["goodType"] = 商品分类管理.查找子分类();
                    专家 expert = 用户管理.查找用户<专家>(id);
                    if (expert != null)
                    {
                        return PartialView("/Views/默认主题/后台/单位用户后台/Procure_Part/Part_Expert_Add.cshtml", expert);
                    }
                    else
                    {
                        return Content("<script>window.location='/专家抽选/Expert_list';</script>");
                    }
                }
                else
                {
                    return Content("<script>window.location='/专家抽选/Expert_list';</script>");
                }
            }
            catch
            {
                return Content("<script>window.location='/专家抽选/Expert_list';</script>");
            }
        }
        public ActionResult Part_Expert_Add()
        {
            ViewData["goodType"] = 商品分类管理.查找子分类();

            return PartialView("/Views/默认主题/后台/单位用户后台/Procure_Part/Part_Expert_Edit.cshtml");
        }
        public ActionResult Part_Expert_History(int? page)
        {
            var q = Query<专家抽选记录>.EQ(o => o.申请抽选状态, 申请抽选状态.已完成抽选);
            if (Request.Params["isscore"] != null)
            {
                var 是否评分 = false;
                if (Request.Params["是否评分"] == "1")
                {
                    是否评分 = true;
                }
                q = q.And(Query<专家抽选记录>.EQ(o => o.是否已评分, 是否评分));
            }

            int listcount = 专家抽选管理.查询专家抽选记录(0, 0, q).Count();
            int pagesize = 15;
            int maxpagesize = Math.Max((listcount + pagesize - 1) / pagesize, 1);
            if (string.IsNullOrEmpty(page.ToString()) || page < 0 || page > maxpagesize)
            {
                page = 1;
            }

            ViewData["currentpage"] = page;
            ViewData["pagecount"] = maxpagesize;
            ViewData["历史抽选专家列表"] = 专家抽选管理.查询专家抽选记录(pagesize * (int.Parse(page.ToString()) - 1), pagesize, q);
            return PartialView("/Views/默认主题/后台/单位用户后台/Procure_Part/Part_Expert_History.cshtml");
        }
        public ActionResult Part_Expert_History_Page(int? page)
        {
            var q = Query<专家抽选记录>.EQ(o => o.申请抽选状态, 申请抽选状态.已完成抽选);
            if (Request.Params["isscore"] != null && Request.Params["isscore"] != "-1")
            {
                var 是否评分 = false;
                if (Request.Params["isscore"] == "1")
                {
                    是否评分 = true;
                }
                q = q.And(Query<专家抽选记录>.EQ(o => o.是否已评分, 是否评分));
            }

            int listcount = 专家抽选管理.查询专家抽选记录(0, 0, q).Count();
            int pagesize = 15;
            int maxpagesize = Math.Max((listcount + pagesize - 1) / pagesize, 1);
            if (string.IsNullOrEmpty(page.ToString()) || page < 0 || page > maxpagesize)
            {
                page = 1;
            }

            ViewData["currentpage"] = page;
            ViewData["pagecount"] = maxpagesize;
            ViewData["历史抽选专家列表"] = 专家抽选管理.查询专家抽选记录(pagesize * (int.Parse(page.ToString()) - 1), pagesize, q);
            return PartialView("/Views/默认主题/后台/单位用户后台/Procure_Part/Part_Expert_History_Page.cshtml");
        }
        public ActionResult Part_Expert_History_Detail(long id)
        {
            try
            {
                专家 expertdetail = 用户管理.查找用户<专家>(id);
                if (expertdetail != null)
                {
                    return PartialView("/Views/默认主题/后台/单位用户后台/Procure_Part/Part_Expert_History_Detail.cshtml", expertdetail);
                }
                else
                {
                    return Content("<script>window.location='/专家抽选/Expert_list';</script>");
                }
            }
            catch
            {
                return Content("<script>window.location='/专家抽选/Expert_list';</script>");
            }
        }
        public ActionResult Part_Expert_Select(int? id)
        {
            专家抽选记录 model = 专家抽选管理.查找专家抽选记录((long)id);

            //Enum.GetNames();
            var t = typeof(专业技术职称);
            var vs = Enum.GetValues(t);
            //var d = new Dictionary<string, int>();
            var r = "";
            foreach (var v in vs)
            {
                //d.Add(Enum.GetName(t, v), (int)v);
                r += "<input type='checkbox' name='专业技术职称' id='" + Enum.GetName(t, v) + "' value='" + (int)v + "'>" + Enum.GetName(t, v);
            }
            //ViewData["专家技术职称"] = d;
            ViewData["专家技术职称"] = r;

            t = typeof(学位);
            vs = Enum.GetValues(t);
            //d = new Dictionary<string, int>();
            r = "";
            foreach (var v in vs)
            {
                //d.Add(Enum.GetName(t, v), (int)v);
                r += "<input type='checkbox' name='最高学位' id='" + Enum.GetName(t, v) + "' value='" + (int)v + "'>" + Enum.GetName(t, v);
            }
            //ViewData["最高学位"] = d;
            ViewData["最高学位"] = r;

            t = typeof(专家类型);
            vs = Enum.GetValues(t);
            //d = new Dictionary<string, int>();
            r = "";
            foreach (var v in vs)
            {
                //d.Add(Enum.GetName(t, v), (int)v);
                r += "<input type='checkbox' name='专家类型' id='" + Enum.GetName(t, v) + "' value='" + (int)v + "'>" + Enum.GetName(t, v);
            }
            //ViewData["专家类型"] = d;
            ViewData["专家类型"] = r;

            t = typeof(专家类别);
            vs = Enum.GetValues(t);
            //d = new Dictionary<string, int>();
            r = "";
            foreach (var v in vs)
            {
                //d.Add(Enum.GetName(t, v), (int)v);
                r += "<input type='checkbox' name='专家类别' id='" + Enum.GetName(t, v) + "' value='" + (int)v + "'>" + Enum.GetName(t, v);
            }
            //ViewData["专家类别"] = d;
            ViewData["专家类别"] = r;

            return PartialView("/Views/默认主题/后台/单位用户后台/Procure_Part/Part_Expert_Select.cshtml", model);
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
            return PartialView("/Views/默认主题/后台/单位用户后台/Procure_Part/Part_Expert_Choose.cshtml", model);
        }
        public class SearchParam
        {
            public List<专家> list { get; set; }
            public int n { get; set; }//抽选批次

            public int trcount { get; set; }//当前最大行号

            public int turn { get; set; }//当前轮次

            //public string condition { get; set; }//条件
        }
        public ActionResult SearchByCondition()
        {
            try
            {
                var q = Query.Null;

                var id = long.Parse(Request.Params["id"]);
                var model = 专家抽选管理.查找专家抽选记录(id);

                //当前抽取的条件数（第n个条件）
                var time = int.Parse(Request.Params["time"]);
                var condition = model.专家抽选条件[time];

                //专业技术职称
                if (condition.专业技术职称 != null && condition.专业技术职称.Any())
                {
                    var item = new List<IMongoQuery>();
                    foreach (var temp in condition.专业技术职称)
                    {
                        var t = Query<专家>.EQ(o => o.学历信息.专业技术职称, temp);
                        item.Add(t);
                    }
                    q = q.And(Query.Or(item));
                }

                //专家类别
                if (condition.专家类别 != null && condition.专家类别.Any())
                {
                    var item = new List<IMongoQuery>();
                    foreach (var temp in condition.专家类别)
                    {
                        var t = Query<专家>.EQ(o => o.身份信息.专家类别, temp);
                        item.Add(t);
                    }
                    q = q.And(Query.Or(item));
                }

                //专家级别
                if (condition.专家级别 != null && condition.专家级别.Any())
                {
                    var item = new List<IMongoQuery>();
                    foreach (var temp in condition.专家级别)
                    {
                        var t = Query<专家>.EQ(o => o.身份信息.专家级别, temp);
                        item.Add(t);
                    }
                    q = q.And(Query.Or(item));
                }

                //专家类型
                if (condition.专家类型 != null && condition.专家类型.Any())
                {
                    var item = new List<IMongoQuery>();
                    foreach (var temp in condition.专家类型)
                    {
                        var t = Query<专家>.EQ(o => o.身份信息.专家类型, temp);
                        item.Add(t);
                    }
                    q = q.And(Query.Or(item));
                }

                //学位要求
                if (condition.学位要求 != null && condition.学位要求.Any())
                {
                    var item = new List<IMongoQuery>();
                    foreach (var temp in condition.学位要求)
                    {
                        var t = Query<专家>.EQ(o => o.学历信息.最高学位, temp);
                        item.Add(t);
                    }
                    q = q.And(Query.Or(item));
                }

                //学历要求
                if (condition.学历要求 != null && condition.学历要求.Any())
                {
                    var item = new List<IMongoQuery>();
                    foreach (var temp in condition.学历要求)
                    {
                        var t = Query<专家>.EQ(o => o.学历信息.最高学历, temp);
                        item.Add(t);
                    }
                    q = q.And(Query.Or(item));
                }

                //所在地域
                if (!string.IsNullOrWhiteSpace(condition.所在地区.省份))
                {
                    q = q.And(Query<专家>.EQ(o => o.所属地域.省份, condition.所在地区.省份));
                    if (!string.IsNullOrWhiteSpace(condition.所在地区.城市))
                    {
                        q = q.And(Query<专家>.EQ(o => o.所属地域.城市, condition.所在地区.城市));
                        if (!string.IsNullOrWhiteSpace(condition.所在地区.区县))
                        {
                            q = q.And(Query<专家>.EQ(o => o.所属地域.区县, condition.所在地区.区县));

                        }
                    }
                }

                //排除专家
                //当前页面已抽取的专家列表
                var outlist = Request.Params["outlist"].Split(new[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
                var out_id_page = new List<long>();
                if (outlist.Any())
                {
                    foreach (var l in outlist)
                    {
                        out_id_page.Add(long.Parse(l));
                    }
                }
                //已屏蔽专家
                var out_id = new List<long>();
                if (model.回避专家列表 != null && model.回避专家列表.Any())
                {
                    foreach (var l in model.回避专家列表)
                    {
                        out_id.Add(l.用户ID);
                    }
                }

                //保存类别以外的条件
                var q_final = q;
                var list = new List<专家>();

                //选择的是精确抽选
                if (string.IsNullOrWhiteSpace(condition.模糊查询))
                {
                    //满足二级分类的条件
                    var qc2 = Query.Null;
                    //满足一级分类的条件
                    var qc1 = Query.Null;
                    //可评标产品类别
                    if (condition.可评标产品类别 != null && condition.可评标产品类别.Any() && condition.可评标产品类别[0].二级分类.Any())
                    {
                        if (condition.可评标产品类别[0].一级分类 == "（地方政府专家库评审专业目录）")
                        {
                            //qc1 = Query<专家>.Where(o => o.可参评物资类别列表.Any(oc => condition.可评标产品类别[0].二级分类.Contains(oc.一级分类)));
                            //list = 专家抽选管理.抽选专家(1, q_final.And(qc1), out_id_page, out_id, 用户管理.查找用户<单位用户>(currentUser.Id).单位信息.所属单位);
                            qc1 = Query<专家>.Where(o => o.可参评物资类别列表.Any(oc => oc.二级分类.ContainsAny(condition.可评标产品类别[0].二级分类)));
                            list = 专家抽选管理.抽选专家(1, q_final.And(qc1), out_id_page, out_id, 用户管理.查找用户<单位用户>(currentUser.Id).单位信息.所属单位);
                        }
                        else
                        {
                            var c1 = condition.可评标产品类别[0].一级分类;
                            var c2 = condition.可评标产品类别[0].二级分类;
                            var cb = condition.可评标产品类别[0].二级分类可用专家不足返回一级分类;

                            //满足二级分类的条件
                            qc2 = Query<专家>.Where(o => o.可参评物资类别列表.Any(oc => oc.二级分类.ContainsAny(c2)));
                            //满足一级分类的条件
                            qc1 = Query<专家>.Where(o => o.可参评物资类别列表.Any(oc => oc.一级分类 == c1));

                            //二级类别的结果
                            list = 专家抽选管理.抽选专家(1, q_final.And(qc2), out_id_page, out_id, 用户管理.查找用户<单位用户>(currentUser.Id).单位信息.所属单位);
                            //二级不够，再查一级
                            if (!list.Any() && condition.可评标产品类别[0].二级分类可用专家不足返回一级分类)
                            {
                                list = 专家抽选管理.抽选专家(1, q_final.And(qc1), out_id_page, out_id, 用户管理.查找用户<单位用户>(currentUser.Id).单位信息.所属单位);
                            }
                        }
                    }
                }
                else
                {
                    var q1 = Query<专家>.Where(o => o.可参评物资类别列表.Any(oc => oc.一级分类.Contains(condition.模糊查询)));
                    var q2 = Query.Where(
                        "function(){" +
                        " for (var k1 in obj.可参评物资类别列表)" +
                        " for (var k2 in obj.可参评物资类别列表[k1].二级分类)" +
                        " if (obj.可参评物资类别列表[k1].二级分类[k2].indexOf('" + condition.模糊查询 + "') != -1) return true;" +
                        " return false;" +
                        " }");
                    var q_march = q1.Or(q2);

                    list = 专家抽选管理.抽选专家(1, q_final.And(q_march), out_id_page, out_id, 用户管理.查找用户<单位用户>(currentUser.Id).单位信息.所属单位);
                }
                return PartialView("/Views/默认主题/后台/单位用户后台/Procure_Part/Part_Expert_Entry.cshtml", list);
            }
            catch
            {
                return PartialView("/Views/默认主题/后台/单位用户后台/Procure_Part/Part_Expert_Entry.cshtml", new List<专家>());
            }
        }
        public ActionResult SearchByCondition_Gys()
        {
            try
            {
                var q = Query.Null;

                var id = long.Parse(Request.Params["id"]);
                var model = 供应商抽选管理.查找供应商抽选历史记录(id);

                //当前抽取的条件数（第n个条件）
                var time = int.Parse(Request.Params["time"]);
                var condition = model.供应商抽选条件[time];

                //所在地域
                if (!string.IsNullOrWhiteSpace(condition.所在地区[0].省份))
                {
                    q = q.And(Query<供应商>.EQ(o => o.所属地域.省份, condition.所在地区[0].省份));
                    if (!string.IsNullOrWhiteSpace(condition.所在地区[0].城市))
                    {
                        q = q.And(Query<供应商>.EQ(o => o.所属地域.城市, condition.所在地区[0].城市));
                        if (!string.IsNullOrWhiteSpace(condition.所在地区[0].区县))
                        {
                            q = q.And(Query<供应商>.EQ(o => o.所属地域.区县, condition.所在地区[0].区县));
                        }
                    }
                }

                //排除专家
                //当前页面已抽取的专家列表
                var outlist = Request.Params["outlist"].Split(new[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
                var out_id_page = new List<long>();
                if (outlist.Any())
                {
                    foreach (var l in outlist)
                    {
                        out_id_page.Add(long.Parse(l));
                    }
                }
                //已屏蔽专家
                var out_id = new List<long>();
                if (model.回避供应商列表 != null && model.回避供应商列表.Any())
                {
                    foreach (var l in model.回避供应商列表)
                    {
                        out_id.Add(l.用户ID);
                    }
                }


                //满足二级分类的条件
                var qc2 = Query.Null;
                //满足一级分类的条件
                var qc1 = Query.Null;
                //可评标产品类别
                if (condition.可提供产品类别 != null && condition.可提供产品类别.Any())
                {
                    if (condition.可提供产品类别[0].二级分类.Any())
                    {
                        var c1 = condition.可提供产品类别[0].一级分类;
                        var c2 = condition.可提供产品类别[0].二级分类;
                        var cb = condition.可提供产品类别[0].二级分类可用供应商不足返回一级分类;

                        //满足二级分类的条件
                        qc2 = Query<供应商>.Where(o => o.可提供产品类别列表.Any(oc => oc.二级分类.ContainsAny(c2)));
                        //满足一级分类的条件
                        qc1 = Query<供应商>.Where(o => o.可提供产品类别列表.Any(oc => oc.一级分类 == c1));
                    }
                }

                //保存类别以外的条件
                var q_final = q;

                //二级类别的结果
                var list = 供应商抽选管理.抽选供应商(1, q_final.And(qc2), out_id_page, out_id);
                //二级不够，再查一级
                if (!list.Any() && condition.可提供产品类别[0].二级分类可用供应商不足返回一级分类)
                {
                    list = 供应商抽选管理.抽选供应商(1, q_final.And(qc1), out_id_page, out_id);
                }
                return PartialView("/Views/默认主题/后台/单位用户后台/Procure_Part/Part_GysChoose_Entry.cshtml", list);
            }
            catch
            {
                return PartialView("/Views/默认主题/后台/单位用户后台/Procure_Part/Part_GysChoose_Entry.cshtml", new List<供应商>());
            }
        }

        [HttpPost]
        public ActionResult SearchByCondition_Temp()
        {
            try
            {
                var kpbwzlb = Request.Params["kpbwzlb"];
                var province = Request.Params["province"];
                var city = Request.Params["city"];
                var area = Request.Params["area"];
                var zyjszc = Request.Params["zyjszc"];
                var leibie = Request.Params["leibie"];
                var jibie = Request.Params["jibie"];
                var leixing = Request.Params["leixing"];
                var xueli = Request.Params["xueli"];
                var xuewei = Request.Params["xuewei"];

                var 所属单位 = 用户管理.查找用户<单位用户>(currentUser.Id).单位信息.所属单位;

                var now = DateTime.Now;
                //筛选条件，专家3个月内不能被同一单位抽取，一年内不能被同一单位抽取3次
                var querydate = Query.Where(
                            " function(){" +
                            " var count = obj.历史抽取信息." + 所属单位 + ".length;" +
                            " if (count == 0) return true;" +
                            " if (count >0 && count <3 && new Date(obj.历史抽取信息." + 所属单位 + "[count-1]) < new Date(\"" + now.AddMonths(-3).ToString() + "\")) return true;" +
                            " if (count >=3 &&  new Date(obj.历史抽取信息." + 所属单位 + "[count-1]) < new Date(\"" + now.AddMonths(-3).ToString() + "\") && new Date(obj.历史抽取信息." + 所属单位 + "[count-3]) < new Date(\"" + now.AddYears(-1).ToString() + "\")) return true;" +
                            " return false;" +
                            " }");
                var q = Query<专家>.EQ(o => o.审核数据.审核状态, 审核状态.审核通过).And(Query.NotExists("历史抽取信息." + 所属单位).Or(querydate));

                //专业技术职称
                if (!string.IsNullOrWhiteSpace(zyjszc) && zyjszc != "-100")
                {
                    var item = new List<IMongoQuery>();
                    foreach (var temp in zyjszc.Split(new[] { "|" }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        var t = Query<专家>.EQ(o => o.学历信息.专业技术职称, (专业技术职称)int.Parse(temp));
                        item.Add(t);
                    }
                    q = q.And(Query.Or(item));
                }

                //专家类别
                if (!string.IsNullOrWhiteSpace(leibie))
                {
                    var item = new List<IMongoQuery>();
                    foreach (var temp in leibie.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        var t = Query<专家>.EQ(o => o.身份信息.专家类别, (专家类别)int.Parse(temp));
                        item.Add(t);
                    }
                    q = q.And(Query.Or(item));
                }

                //专家级别
                if (!string.IsNullOrWhiteSpace(jibie))
                {
                    var item = new List<IMongoQuery>();
                    foreach (var temp in jibie.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        var t = Query<专家>.EQ(o => o.身份信息.专家级别, (专家级别)int.Parse(temp));
                        item.Add(t);
                    }
                    q = q.And(Query.Or(item));
                }

                //专家类型
                if (!string.IsNullOrWhiteSpace(leixing))
                {
                    var item = new List<IMongoQuery>();
                    foreach (var temp in leixing.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        var t = Query<专家>.EQ(o => o.身份信息.专家类型, (专家类型)int.Parse(temp));
                        item.Add(t);
                    }
                    q = q.And(Query.Or(item));
                }

                //学位要求
                if (!string.IsNullOrWhiteSpace(xuewei))
                {
                    var item = new List<IMongoQuery>();
                    foreach (var temp in xuewei.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        var t = Query<专家>.EQ(o => o.学历信息.最高学位, (学位)int.Parse(temp));
                        item.Add(t);
                    }
                    q = q.And(Query.Or(item));
                }

                //学历要求
                if (!string.IsNullOrWhiteSpace(xueli))
                {
                    var item = new List<IMongoQuery>();
                    foreach (var temp in xueli.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        var t = Query<专家>.EQ(o => o.学历信息.最高学历, (学历)int.Parse(temp));
                        item.Add(t);
                    }
                    q = q.And(Query.Or(item));
                }

                //所在地域
                if (!string.IsNullOrWhiteSpace(province))
                {
                    q = q.And(Query<专家>.EQ(o => o.所属地域.省份, province));
                    if (!string.IsNullOrWhiteSpace(city))
                    {
                        q = q.And(Query<专家>.EQ(o => o.所属地域.城市, city));
                        if (!string.IsNullOrWhiteSpace(area))
                        {
                            q = q.And(Query<专家>.EQ(o => o.所属地域.区县, area));

                        }
                    }
                }

                //已屏蔽专家
                //当前页面已抽取的专家列表
                var outlist = Request.Params["outlist"].Split(new[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
                var out_id_page = new List<long>();
                if (outlist.Any())
                {
                    foreach (var l in outlist)
                    {
                        out_id_page.Add(long.Parse(l));
                    }
                }

                var q_final = q;
                long listcount = 0;
                //可评标产品类别
                var pblb = kpbwzlb.Split('#');

                if (pblb.Count() == 1)
                {
                    var q1 = Query<专家>.Where(o => o.可参评物资类别列表.Any(oc => oc.一级分类.Contains(kpbwzlb)));
                    var q2 = Query.Where(
                        "function(){" +
                        " for (var k1 in obj.可参评物资类别列表)" +
                        " for (var k2 in obj.可参评物资类别列表[k1].二级分类)" +
                        " if (obj.可参评物资类别列表[k1].二级分类[k2].indexOf('" + kpbwzlb + "') != -1) return true;" +
                        " return false;" +
                        " }");
                    var q_march = q1.Or(q2);

                    listcount = 用户管理.计数用户<专家>(0, 0, q_final.And(q_march));
                }
                else
                {
                    //满足二级分类的条件
                    var qc2 = Query.Null;
                    //满足一级分类的条件
                    var qc1 = Query.Null;
                    var pblb_lb = pblb[0].Split(new[] { "*" }, StringSplitOptions.RemoveEmptyEntries).ToList();
                    var c1 = pblb_lb[0];
                    pblb_lb.RemoveAt(0);
                    var cb = pblb[1];
                    if (c1 == "（地方政府专家库评审专业目录）")
                    {
                        //qc1 = Query<专家>.Where(o => o.可参评物资类别列表.Any(oc => pblb_lb.Contains(oc.一级分类)));
                        //listcount = 用户管理.计数用户<专家>(0, 0, q_final.And(qc1));
                        qc2 = Query<专家>.Where(o => o.可参评物资类别列表.Any(oc => oc.二级分类.ContainsAny(pblb_lb)));
                        listcount = 用户管理.计数用户<专家>(0, 0, q_final.And(qc2));
                    }
                    else
                    {
                        //满足二级分类的条件
                        qc2 = Query<专家>.Where(o => o.可参评物资类别列表.Any(oc => oc.二级分类.ContainsAny(pblb_lb)));
                        //满足一级分类的条件
                        qc1 = Query<专家>.Where(o => o.可参评物资类别列表.Any(oc => oc.一级分类 == c1));

                        //保存类别以外的条件

                        //二级类别的结果
                        //返回上级，直接查上级个数
                        if (cb == "1")
                        {
                            listcount = 用户管理.计数用户<专家>(0, 0, q_final.And(qc1));
                        }
                        else
                        {
                            listcount = 用户管理.计数用户<专家>(0, 0, q_final.And(qc2));
                        }
                    }
                }
                return Content(listcount.ToString());
            }
            catch
            {
                return Content("0");
            }
        }
        public ActionResult SearchByCondition_Temp_Gys()
        {
            try
            {
                var kpbwzlb = Request.Params["kpbwzlb"];
                var province = Request.Params["province"];
                var city = Request.Params["city"];
                var area = Request.Params["area"];

                var q = Query<供应商>.EQ(o => o.审核数据.审核状态, 审核状态.审核通过);

                //所在地域
                if (!string.IsNullOrWhiteSpace(province))
                {
                    q = q.And(Query<供应商>.EQ(o => o.所属地域.省份, province));
                    if (!string.IsNullOrWhiteSpace(city))
                    {
                        q = q.And(Query<供应商>.EQ(o => o.所属地域.城市, city));
                        if (!string.IsNullOrWhiteSpace(area))
                        {
                            q = q.And(Query<供应商>.EQ(o => o.所属地域.区县, area));

                        }
                    }
                }

                //已屏蔽供应商
                //当前页面已抽取的供应商列表
                var outlist = Request.Params["outlist"].Split(new[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
                var out_id_page = new List<long>();
                if (outlist.Any())
                {
                    foreach (var l in outlist)
                    {
                        out_id_page.Add(long.Parse(l));
                    }
                }


                //满足二级分类的条件
                var qc2 = Query.Null;
                //满足一级分类的条件
                var qc1 = Query.Null;
                //可评标产品类别
                var pblb = kpbwzlb.Split('#');
                var pblb_lb = pblb[0].Split(new[] { "*" }, StringSplitOptions.RemoveEmptyEntries).ToList();


                var c1 = pblb_lb[0];
                pblb_lb.RemoveAt(0);
                var cb = pblb[1];

                //满足二级分类的条件
                qc2 = Query<供应商>.Where(o => o.可提供产品类别列表.Any(oc => oc.二级分类.ContainsAny(pblb_lb)));
                //满足一级分类的条件
                qc1 = Query<供应商>.Where(o => o.可提供产品类别列表.Any(oc => oc.一级分类 == c1));

                //保存类别以外的条件
                var q_final = q;

                long listcount = 0;
                //二级类别的结果

                //返回上级，直接查上级个数
                if (cb == "1")
                {
                    listcount = 用户管理.计数用户<供应商>(0, 0, q_final.And(qc1));
                }
                else
                {
                    listcount = 用户管理.计数用户<供应商>(0, 0, q_final.And(qc2));
                }
                return Content(listcount.ToString());
            }
            catch
            {
                return Content("0");
            }
        }
        [HttpPost]

        public ActionResult AddExpert(专家 model)
        {
            HttpPostedFileBase img = null;
            if (Request.Files.Count > 0)
            {
                for (int i = 0; i < Request.Files.Count; i++)
                {
                    img = Request.Files[i];
                    string filePath = 上传管理.获取上传路径<专家>(媒体类型.图片, 路径类型.不带域名根路径);
                    string fname = UploadAttachment(img);
                    string file_name = filePath + fname;
                    switch (i)
                    {
                        //case 0: model.身份信息.专家证电子扫描件 = file_name; break;
                        //case 1: model.身份信息.证件电子扫描件 = file_name; break;
                        //case 2: model.身份信息.本人照片电子扫描件 = file_name; break;
                        //case 3: model.学历信息.职称证书电子扫描件 = file_name; break;
                    }
                }
            }
            if (model.所属地域.地域 == "不限省份不限城市不限区县")
            {
                model.所属地域.省份 = "";
                model.所属地域.城市 = "";
                model.所属地域.区县 = "";
            }
            if (model.所属地域.城市 == "不限城市")
            {
                model.所属地域.城市 = "";
                model.所属地域.区县 = "";
            }
            if (model.所属地域.区县 == "不限区县")
            {
                model.所属地域.区县 = "";
            }
            var t = DateTime.Now;
            model.登录信息.登录名 = string.Format("zj{0:D2}{1:D3}{2:D5}", t.Year % 100, t.DayOfYear, (int)t.TimeOfDay.TotalSeconds);
            model.登录信息.密码 = "000000";
            List<供应商._产品类别> list = new List<供应商._产品类别>();
            if (model.可参评物资类别列表 != null && model.可参评物资类别列表.Count() != 0)
            {
                if (model.可参评物资类别列表.Count() == 1)
                {
                    if (model.可参评物资类别列表[0].二级分类[0] == "-1")
                    {
                        供应商._产品类别 mm = new 供应商._产品类别();
                        mm.一级分类 = model.可参评物资类别列表[0].一级分类;
                        mm.二级分类 = new List<string>();
                        list.Add(mm);
                    }
                    else
                    {
                        model.可参评物资类别列表[0].二级分类 = model.可参评物资类别列表[0].二级分类.Where(s => !string.IsNullOrEmpty(s)).ToList();
                        if (model.可参评物资类别列表[0].二级分类.Count > 0)
                        {
                            供应商._产品类别 mm = new 供应商._产品类别();
                            mm.一级分类 = model.可参评物资类别列表[0].一级分类;
                            mm.二级分类 = model.可参评物资类别列表[0].二级分类;
                            list.Add(mm);
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < model.可参评物资类别列表.Count; i++)
                    {
                        model.可参评物资类别列表[i].二级分类 = model.可参评物资类别列表[i].二级分类.Where(s => !string.IsNullOrEmpty(s)).ToList();
                        if (model.可参评物资类别列表[i].二级分类.Count > 0)
                        {
                            供应商._产品类别 mm = new 供应商._产品类别();
                            mm.一级分类 = model.可参评物资类别列表[i].一级分类;
                            mm.二级分类 = model.可参评物资类别列表[i].二级分类;
                            list.Add(mm);
                        }
                    }
                }
            }
            model.可参评物资类别列表 = list;
            model.审核数据.审核状态 = 审核状态.审核通过;
            用户管理.添加用户<专家>(model);
            return Content("<script>alert('成功添加了一位专家，您可以继续添加。');window.location='Expert_Add';</script>");
            //return RedirectToAction("Expert_Add");
        }
        public string UploadAttachment(HttpPostedFileBase fileData)
        {
            //string extension = string.Empty;
            string fileName = string.Empty;
            if (fileData != null)
            {
                if (fileData.FileName != "" && fileData.FileName != null)
                {
                    string filePath = 上传管理.获取上传路径<专家>(媒体类型.图片, 路径类型.服务器本地路径);
                    //extension = fileData.FileName.Substring(fileData.FileName.LastIndexOf("."), (fileData.FileName.Length - fileData.FileName.LastIndexOf(".")));
                    fileName = 上传管理.获取文件名(fileData.FileName).Replace("{", "").Replace("}", "");
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

        public ActionResult DeleteExpert(long id)
        {
            string link = Request.Params["link"];
            用户管理.删除用户<专家>(id);
            return RedirectToAction("Expert_list", new { link = link });
        }

        public ActionResult EditExpert(专家 model)
        {
            string id = Request.Params["id"];
            专家 expert = 用户管理.查找用户<专家>(long.Parse(id));

            HttpPostedFileBase img = null;
            for (int i = 0; i < Request.Files.Count; i++)
            {
                img = Request.Files[i];
                if (img.FileName != null && img.FileName != "")
                {
                    string filePath = 上传管理.获取上传路径<专家>(媒体类型.图片, 路径类型.不带域名根路径);
                    string fname = UploadAttachment(img);
                    string file_name = filePath + fname;
                    switch (i)
                    {
                        //case 0: model.身份信息.证件电子扫描件 = file_name; break;
                        //case 1: model.身份信息.本人照片电子扫描件 = file_name; break;
                        //case 2: model.身份信息.专家证电子扫描件 = file_name; break;
                        //case 3: model.学历信息.职称证书电子扫描件 = file_name; break;
                    }
                }
                else
                {
                    switch (i)
                    {
                        case 0:
                            model.身份信息.证件电子扫描件 = expert.身份信息.证件电子扫描件;
                            break;
                        case 1:
                            model.身份信息.本人照片电子扫描件 = expert.身份信息.本人照片电子扫描件;
                            break;
                        case 2:
                            model.身份信息.专家证电子扫描件 = expert.身份信息.专家证电子扫描件;
                            break;
                        case 3:
                            model.学历信息.职称证书电子扫描件 = expert.学历信息.职称证书电子扫描件;
                            break;
                    }
                }
            }

            if (model.所属地域.地域 == "不限省份不限城市不限区县")
            {
                model.所属地域.省份 = "";
                model.所属地域.城市 = "";
                model.所属地域.区县 = "";
            }
            if (model.所属地域.城市 == "不限城市")
            {
                model.所属地域.城市 = "";
                model.所属地域.区县 = "";
            }
            if (model.所属地域.区县 == "不限区县")
            {
                model.所属地域.区县 = "";
            }
            List<供应商._产品类别> list = new List<供应商._产品类别>();
            //if (expert.可参评物资类别列表 != null && expert.可参评物资类别列表.Count() != 0)
            //{
                //if (expert.可参评物资类别列表[0].二级分类.Count()>0 && expert.可参评物资类别列表[0].二级分类[0] != "-1")
                //{
                    list = expert.可参评物资类别列表;
                //}
            //}
            if (model.可参评物资类别列表 != null && model.可参评物资类别列表.Count() != 0)
            {
                //if (model.可参评物资类别列表[0].二级分类.Count() > 0 && model.可参评物资类别列表[0].二级分类[0] == "-1")
                //{
                //    list = model.可参评物资类别列表;
                //}
                //else
                //{
                    foreach (var item in model.可参评物资类别列表)
                    {
                        供应商._产品类别 mm = new 供应商._产品类别();
                        mm.一级分类 = item.一级分类;
                        mm.二级分类 = item.二级分类;
                        list.Add(mm);
                    }
                //}
            }
            expert.所属管理单位 = model.所属管理单位;
            expert.所属地域 = model.所属地域;
            expert.可参评物资类别列表 = list;
            expert.工作经历信息 = model.工作经历信息;
            expert.身份信息 = model.身份信息;
            expert.学历信息 = model.学历信息;
            expert.审核数据.审核状态 = 审核状态.审核通过;
            用户管理.更新用户<专家>(expert);
            return Content("<script>alert('修改成功！');window.location='运营团队后台/Expert_List';</script>");
            //return RedirectToAction("Expert_list");
        }

        public ActionResult Expert_Choose_Select(long? id)
        {
            try
            {
                var time = Request.Params["time"];
                var parm_str = Request.Params["parm_str"];

                var model = 专家抽选管理.查找专家抽选记录((long)id);
                model.操作时间 = Convert.ToDateTime(time);
                model.确认时间 = DateTime.Now;

                var list = parm_str.Split(new[] { "~~" }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var temp in list)
                {
                    var t = temp.Split('~');
                    var z = new 专家抽选记录._专家抽选条目();
                    z.抽选时间 = Convert.ToDateTime(t[0]);
                    z.专家.用户ID = long.Parse(t[1]);
                    if (t[2] == "1")
                    {
                        z.预定出席 = true;
                        z.不能出席原因 = string.Empty;

                        //更新专家的历史抽取信息
                        var zj = z.专家.用户数据;
                        //zj.上次出席评标时间 = z.抽选时间;
                        var 所属单位 = 用户管理.查找用户<单位用户>(currentUser.Id).单位信息.所属单位;
                        if (zj.历史抽取信息.ContainsKey(所属单位))
                        {
                            zj.历史抽取信息[所属单位].Add(z.抽选时间);
                        }
                        else
                        {
                            zj.历史抽取信息.Add(所属单位, new List<DateTime>() { z.抽选时间 });
                        }
                        用户管理.更新用户<专家>(zj, false);
                    }
                    else
                    {
                        z.预定出席 = false;
                        if (string.IsNullOrWhiteSpace(t[3]) || t[3] == "请输入原因")
                        {
                            z.不能出席原因 = string.Empty;
                        }
                        else
                        {
                            z.不能出席原因 = t[3];
                        }
                    }
                    model.抽选专家列表.Add(z);
                }
                model.申请抽选状态 = 申请抽选状态.已完成抽选;
                专家抽选管理.更新专家抽选记录(model);

                return Content("1");
            }
            catch
            {
                return Content("0");
            }

        }

        public ActionResult Gys_Choose_Select(long? id)
        {
            try
            {
                var time = Request.Params["time"];
                var parm_str = Request.Params["parm_str"];

                var model = 供应商抽选管理.查找供应商抽选历史记录((long)id);
                model.操作时间 = Convert.ToDateTime(time);
                model.确认时间 = DateTime.Now;

                var list = parm_str.Split(new[] { "~~" }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var temp in list)
                {
                    var t = temp.Split('~');
                    var z = new 供应商抽选记录._供应商抽选条目();
                    z.抽选时间 = Convert.ToDateTime(t[0]);
                    z.供应商.用户ID = long.Parse(t[1]);
                    if (t[2] == "1")
                    {
                        z.预定出席 = true;
                        z.不能出席原因 = string.Empty;
                    }
                    else
                    {
                        z.预定出席 = false;
                        if (string.IsNullOrWhiteSpace(t[3]) || t[3] == "请输入原因")
                        {
                            z.不能出席原因 = string.Empty;
                        }
                        else
                        {
                            z.不能出席原因 = t[3];
                        }
                    }
                    model.抽选供应商列表.Add(z);
                }
                model.申请抽选状态 = 申请抽选状态.已完成抽选;
                供应商抽选管理.更新供应商抽选历史记录(model);

                return Content("1");
            }
            catch
            {
                return Content("0");
            }

        }

        [HttpPost]
        public ActionResult SaveHistory()
        {
            ////try
            ////{
            //    专家抽选记录 model = new 专家抽选记录();

            //    List<DateTime> timelist = new List<DateTime>();
            //    //%2|2014-07-15 17:51:20^6|89|1|^7|64|1|^%

            //    string str = Request.Params["parm"];
            //    string[] a1 = str.Substring(0, str.Length - 1).Split('%');

            //    //a1[i] 1|2014-07-15 17:50:56^1|50|0,不能出席1|^2|33|1|^3|80|0,不能出席2|^4|70|1|^5|92|1|^
            //    foreach (var item in a1)
            //    {
            //    _专家抽选批次 numlist = new _专家抽选批次();

            //        string[] a2 = item.Substring(0, item.Length - 1).Split('^');
            //        //a2[0] 1|2014-07-15 17:50:56   a2[i] 1|50|0,不能出席1     2|33|1
            //    string []numstr = a2[0].Split('|');
            //        //historylist.抽取批次 = int.Parse(numstr.Split('|')[0]);
            //    string[] conditonlist = numstr[2].Split('_');//抽取条件
            //    if (!string.IsNullOrEmpty(conditonlist[0]))
            //    {
            //        int i = int.Parse(conditonlist[0]);
            //        numlist.专业技术职称 = (专业技术职称)i;
            //    }

            //    if (!string.IsNullOrEmpty(conditonlist[1]))
            //    {
            //        int i = int.Parse(conditonlist[1]);
            //        numlist.最高学位 = (学位)i;
            //    }

            //    if (!string.IsNullOrEmpty(conditonlist[2]))
            //    {
            //        numlist.从事专业 = conditonlist[2];
            //    }

            //    if (!string.IsNullOrEmpty(conditonlist[3]))
            //    {
            //        int i = int.Parse(conditonlist[3]);
            //        numlist.专家类型 = (专家类型)i;
            //    }

            //    if (!string.IsNullOrEmpty(conditonlist[4]))
            //    {
            //        int i = int.Parse(conditonlist[4]);
            //        numlist.专家类别 = (专家类别)i;
            //    }

            //    if (!string.IsNullOrEmpty(conditonlist[5]))
            //    {
            //        numlist.所在地区.省份 = conditonlist[5];
            //    }

            //    if (!string.IsNullOrEmpty(conditonlist[6]))
            //    {
            //        numlist.所在地区.城市 = conditonlist[6];
            //    }

            //    if (!string.IsNullOrEmpty(conditonlist[7]))
            //    {
            //        numlist.所在地区.区县 = conditonlist[7];
            //    }
            //    numlist.抽选操作时间 = Convert.ToDateTime(numstr[1]);

            //    //timelist.Add(Convert.ToDateTime(numstr.Split('|')[1]));

            //        for (int i = 1; i < a2.Length; i++)
            //        {
            //            string[] a3 = a2[i].Split('|');

            //            专家 zj = new 专家();
            //            zj = 专家管理.查找专家(long.Parse(a3[1]));
            //        int 抽取批次 = int.Parse(numstr[0]);
            //            int 序号 = int.Parse(a3[0]);
            //            bool 预定出席 = true;
            //            string 不能出席原因 = "";
            //            if (a3[2] != "1")
            //            {
            //                预定出席 = false;
            //                不能出席原因 = a3[2].Substring(2, a3[2].Length - 2);
            //            }

            //            _专家抽选记录 historylist = new _专家抽选记录()
            //            {
            //                不能出席原因 = 不能出席原因,
            //                抽取批次 = 抽取批次,
            //                联系方式 = zj.联系方式.手机 + "," + zj.联系方式.固定电话,
            //                所在地区 = zj.所属地域,
            //                性别 = zj.身份信息.性别,
            //                姓名 = zj.身份信息.姓名,
            //                序号 = 序号,
            //                预定出席 = 预定出席,
            //                证件号 = zj.身份信息.证件号,
            //                证件类型 = zj.身份信息.证件类型,
            //                专家类别 = zj.身份信息.专家类别,
            //                专业技术职称 = zj.学历信息.专业技术职称,
            //                最高学历 = zj.学历信息.最高学历,
            //                最高学位 = zj.学历信息.最高学位
            //            };
            //            historylist.专家.专家ID = zj.Id;
            //        numlist.专家抽选记录.Add(historylist);

            //        }
            //    model.专家抽选批次.Add(numlist);

            //    }
            //    model.操作人.用户ID = this.HttpContext.获取当前用户<单位用户>().Id;
            ////model.抽选操作时间 = timelist;
            //    //model.确认人.用户ID = this.HttpContext.获取当前用户<单位用户>().Id;

            //    model.项目名称 = Request.Params["pro_name"];
            //    model.项目编号 = Request.Params["pro_num"];
            //    model.评标时间 = Convert.ToDateTime(Request.Params["pro_time"]);
            //    model.屏蔽列表 = Request.Params["otherlist"];
            //    model.预定抽选专家数 = int.Parse(Request.Params["count"]);

            //    专家管理.添加专家抽选历史(model);

            return Content("0");
            //}
            //catch
            //{
            //    return Content("-1");
            //}
        }

        public ActionResult Part_Expert_ApplayOutList(int? page)
        {
            var name = Request.QueryString["name"];
            int pagesize = 12;

            var q = Query<专家>.EQ(o => o.审核数据.审核状态, 审核状态.审核通过);

            if (!string.IsNullOrWhiteSpace(name))
            {
                q = q.And(Query<专家>.EQ(o => o.身份信息.姓名, name));
            }

            var listcount = 用户管理.列表用户<专家>(0, 0, Fields<专家>.Include(o => o.Id, o => o.身份信息.性别, o => o.工作经历信息.工作单位, o => o.身份信息.姓名, o => o.联系方式.手机), q).Count();
            int maxpage = Math.Max((listcount + pagesize - 1) / pagesize, 1);
            try
            {
                if (page == null || page > maxpage || page < 1)
                {
                    page = 1;
                }
            }
            catch
            {
                page = 1;
            }
            ViewData["currentpage"] = page;
            ViewData["pagecount"] = maxpage;
            ViewData["屏蔽专家"] = 用户管理.列表用户<专家>(pagesize * ((int)page - 1), pagesize, Fields<专家>.Include(o => o.Id, o => o.身份信息.性别, o => o.工作经历信息.工作单位, o => o.身份信息.姓名, o => o.学历信息.取得现技术职称时间), q);
            return PartialView("/Views/默认主题/后台/单位用户后台/Procure_Part/Part_Expert_ApplayOutList.cshtml");
        }
        public ActionResult Part_Gys_ApplayOutList(int? page)
        {
            var name = Request.QueryString["name"];
            int pagesize = 12;
            var q = Query.Null;


            if (!string.IsNullOrWhiteSpace(name))
            {
                q = q.And(Query<供应商>.EQ(o => o.企业基本信息.企业名称, new BsonRegularExpression(string.Format("/{0}/i", name))));
            }

            var listcount = 用户管理.列表用户<供应商>(0, 0, Fields<供应商>.Include(o => o.Id, o => o.企业基本信息.企业名称, o => o.所属地域.省份, o => o.所属地域.城市, o => o.所属地域.区县, o => o.企业联系人信息.联系人姓名, o => o.企业联系人信息.联系人固定电话, o => o.企业联系人信息.联系人手机), q).Count();
            int maxpage = Math.Max((listcount + pagesize - 1) / pagesize, 1);
            try
            {
                if (page == null || page > maxpage || page < 1)
                {
                    page = 1;
                }
            }
            catch
            {
                page = 1;
            }
            ViewData["currentpage"] = page;
            ViewData["pagecount"] = maxpage;
            ViewData["屏蔽供应商"] = 用户管理.列表用户<供应商>(pagesize * ((int)page - 1), pagesize, Fields<供应商>.Include(o => o.Id, o => o.企业基本信息.企业名称, o => o.所属地域.省份, o => o.所属地域.城市, o => o.所属地域.区县, o => o.企业联系人信息.联系人姓名, o => o.企业联系人信息.联系人固定电话, o => o.企业联系人信息.联系人手机), q);
            return PartialView("/Views/默认主题/后台/单位用户后台/Procure_Part/Part_Gys_ApplayOutList.cshtml");
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

            return PartialView("/Views/默认主题/后台/单位用户后台/Procure_Part/Part_Expert_Applay.cshtml");
        }

        [单一权限验证(权限.新增评审专家抽取申请)]
        public ActionResult Expert_Applay()
        {
            return View("/Views/默认主题/后台/单位用户后台/Expert_Applay.cshtml");
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


            return RedirectToAction("Expert_Applay_S");
        }
        [单一权限验证(权限.我可审核的评审专家抽取申请)]
        public ActionResult Expert_ApplayAuditList()
        {
            return View("/Views/默认主题/后台/单位用户后台/Expert_ApplayAuditList.cshtml");
        }
        public ActionResult Part_Expert_ApplayAuditList()
        {
            int page = 1;

            int pre_listcount = 专家抽选管理.查询专家抽选记录(0, 0, Query<专家抽选记录>.Where(o=>o.申请抽选状态==申请抽选状态.已提交待批准 && !o.是否一体机抽取 && !o.基本数据.已屏蔽)).Count();
            int pre_maxpage = Math.Max((pre_listcount + PAGESIZE - 1) / PAGESIZE, 1);
            ViewData["pre_currentPage"] = page;
            ViewData["pre_pagecount"] = pre_maxpage;
            ViewData["专家抽取待批准列表"] = 专家抽选管理.查询专家抽选记录(0, PAGESIZE, Query<专家抽选记录>.Where(o => o.申请抽选状态 == 申请抽选状态.已提交待批准 && !o.是否一体机抽取 && !o.基本数据.已屏蔽));

            pre_listcount = 专家抽选管理.查询专家抽选记录(0, 0, Query.EQ("申请抽选状态", 申请抽选状态.已批准待抽选)).Count();
            pre_maxpage = Math.Max((pre_listcount + PAGESIZE - 1) / PAGESIZE, 1);
            ViewData["ing_currentPage"] = page;
            ViewData["ing_pagecount"] = pre_maxpage;
            ViewData["专家抽取已批准列表"] = 专家抽选管理.查询专家抽选记录(0, PAGESIZE, Query.EQ("申请抽选状态", 申请抽选状态.已批准待抽选));

            pre_listcount = 专家抽选管理.查询专家抽选记录(0, 0, Query.EQ("申请抽选状态", 申请抽选状态.未获批准)).Count();
            pre_maxpage = Math.Max((pre_listcount + PAGESIZE - 1) / PAGESIZE, 1);
            ViewData["no_currentPage"] = page;
            ViewData["no_pagecount"] = pre_maxpage;
            ViewData["专家抽取未获批准列表"] = 专家抽选管理.查询专家抽选记录(0, PAGESIZE, Query.EQ("申请抽选状态", 申请抽选状态.未获批准));

            return PartialView("/Views/默认主题/后台/单位用户后台/Procure_Part/Part_Expert_ApplayAuditList.cshtml");
        }

        public ActionResult Part_Expert_ApplayAuditList_pre(int? page)
        {
            int listcount = 专家抽选管理.查询专家抽选记录(0, 0, Query<专家抽选记录>.Where(o => o.申请抽选状态 == 申请抽选状态.已提交待批准 && !o.是否一体机抽取 && !o.基本数据.已屏蔽)).Count();
            int maxpage = Math.Max((listcount + PAGESIZE - 1) / PAGESIZE, 1);
            if (string.IsNullOrEmpty(page.ToString()) || page < 0 || page > maxpage)
            {
                page = 1;
            }

            ViewData["pre_currentPage"] = page;
            ViewData["pre_pagecount"] = maxpage;
            ViewData["专家抽取待批准列表"] = 专家抽选管理.查询专家抽选记录(PAGESIZE * (int.Parse(page.ToString()) - 1), PAGESIZE, Query<专家抽选记录>.Where(o => o.申请抽选状态 == 申请抽选状态.已提交待批准 && !o.是否一体机抽取 && !o.基本数据.已屏蔽));

            return PartialView("/Views/默认主题/后台/单位用户后台/Procure_Part/Part_Expert_ApplayAuditList_pre.cshtml");
        }

        public ActionResult Part_Expert_ApplayAuditList_ing(int? page)
        {
            int listcount = 专家抽选管理.查询专家抽选记录(0, 0, Query.EQ("申请抽选状态", 申请抽选状态.已批准待抽选)).Count();
            int maxpage = Math.Max((listcount + PAGESIZE - 1) / PAGESIZE, 1);

            if (string.IsNullOrEmpty(page.ToString()) || page < 0 || page > maxpage)
            {
                page = 1;
            }
            ViewData["ing_currentPage"] = page;
            ViewData["ing_pagecount"] = maxpage;
            ViewData["专家抽取已批准列表"] = 专家抽选管理.查询专家抽选记录(PAGESIZE * (int.Parse(page.ToString()) - 1), PAGESIZE, Query.EQ("申请抽选状态", 申请抽选状态.已批准待抽选));

            return PartialView("/Views/默认主题/后台/单位用户后台/Procure_Part/Part_Expert_ApplayAuditList_ing.cshtml");
        }

        public ActionResult Part_Expert_ApplayAuditList_no(int? page)
        {
            int listcount = 专家抽选管理.查询专家抽选记录(0, 0, Query.EQ("申请抽选状态", 申请抽选状态.未获批准)).Count();
            int maxpage = Math.Max((listcount + PAGESIZE - 1) / PAGESIZE, 1);
            if (string.IsNullOrEmpty(page.ToString()) || page < 0 || page > maxpage)
            {
                page = 1;
            }
            ViewData["no_currentPage"] = page;
            ViewData["no_pagecount"] = maxpage;
            ViewData["专家抽取未获批准列表"] = 专家抽选管理.查询专家抽选记录(PAGESIZE * (int.Parse(page.ToString()) - 1), PAGESIZE, Query.EQ("申请抽选状态", 申请抽选状态.未获批准));

            return PartialView("/Views/默认主题/后台/单位用户后台/Procure_Part/Part_Expert_ApplayAuditList_no.cshtml");
        }
        public ActionResult Expert_ApplayAudit()
        {
            return View("/Views/默认主题/后台/单位用户后台/Expert_ApplayAudit.cshtml");
        }
        public ActionResult Part_Expert_ApplayAudit(int? id)
        {
            专家抽选记录 model = new 专家抽选记录();
            if (id != null)
            {
                model = 专家抽选管理.查找专家抽选记录((long)id);
            }
            return PartialView("/Views/默认主题/后台/单位用户后台/Procure_Part/Part_Expert_ApplayAudit.cshtml", model);
        }
        [HttpPost]
        public ActionResult Expert_ApplayAudit(string action, int? id)
        {
            if (action == "审核通过")
            {
                专家抽选管理.批准专家抽选申请((long)id, this.HttpContext.获取当前用户<单位用户>().Id, 申请抽选状态.已批准待抽选);
            }

            if (action == "审核不通过")
            {
                专家抽选管理.批准专家抽选申请((long)id, this.HttpContext.获取当前用户<单位用户>().Id, 申请抽选状态.未获批准);
            }

            return View("/Views/默认主题/后台/单位用户后台/Expert_ApplayAuditList.cshtml");
        }

        //------------------供应商抽取函数-------------------------------

        [单一权限验证(权限.新增供应商抽取申请)]
        public ActionResult GysChoose_Applay()
        {
            return View("/Views/默认主题/后台/单位用户后台/GysChoose_Applay.cshtml");
        }
        public ActionResult Part_GysChoose_Applay()
        {
            ViewData["goodType"] = 商品分类管理.查找子分类();
            ViewData["经办人"] = HttpContext.获取当前用户<单位用户>().联系方式.联系人;

            return PartialView("/Views/默认主题/后台/单位用户后台/Procure_Part/Part_GysChoose_Applay.cshtml");
        }
        [HttpPost]

        public ActionResult GysChoose_Applay(int? id)
        {
            供应商抽选记录 model = new 供应商抽选记录
            {
                申请时间 = DateTime.Now,
                项目编号 = Request.Form["pro_num"],
                项目名称 = Request.Form["pro_name"],
                评标时间 = Convert.ToDateTime(Request.Form["pro_time"]),
                总计预定抽选供应商数 = int.Parse(Request.Form["预定抽选总数"]),
                操作人姓名 = Request.Form["prooperate_name"],
                操作人电话 = Request.Form["prooperate_tel"]
            };
            model.经办人.用户ID = this.HttpContext.获取当前用户<单位用户>().Id;
            model.申请抽选状态 = 申请抽选状态.已提交待批准;

            //屏蔽列表
            var outlist = Request.Form["outlistcontent"];
            var l = new List<用户链接<供应商>>();
            if (!string.IsNullOrWhiteSpace(outlist))
            {
                foreach (var item in outlist.Split(new[] { "|" }, StringSplitOptions.RemoveEmptyEntries))
                {
                    var z = new 用户链接<供应商>
                    {
                        用户ID = long.Parse(item)
                    };
                    l.Add(z);
                }
            }
            model.回避供应商列表 = l;

            int count = int.Parse(Request.Form["条件总数"]);
            var condition = new List<供应商抽选记录._供应商抽选条件>();
            for (int i = 1; i <= count; i++)
            {
                var t_condition = new 供应商抽选记录._供应商抽选条件();
                t_condition.需要供应商数量 = int.Parse(Request.Form["预定抽选个数__" + i]);

                //可提供物质类别
                var tempparmlist = Request.Form["可评标物质类别__" + i];
                if (!string.IsNullOrWhiteSpace(tempparmlist))
                {
                    var t = new 供应商抽选记录._供应商抽选条件._提供产品类别();
                    var 可评标物质类别 = tempparmlist.Split(new[] { "#" }, StringSplitOptions.RemoveEmptyEntries);
                    if (可评标物质类别[1] == "1")
                    {
                        t.二级分类可用供应商不足返回一级分类 = true;
                    }
                    else
                    {
                        t.二级分类可用供应商不足返回一级分类 = false;
                    }

                    var p = 可评标物质类别[0].Split(new[] { "*" }, StringSplitOptions.RemoveEmptyEntries);
                    t.一级分类 = p[0];
                    t.二级分类 = new List<string>();
                    for (var j = 1; j < p.Length; j++)
                    {
                        t.二级分类.Add(p[j]);
                    }

                    t_condition.可提供产品类别.Add(t);
                }
                //抽选描述
                t_condition.描述 = Request.Form["抽选描述__" + i];
                //所在地域
                var place = new _地域
                {
                    省份 = Request.Form["省份__" + i],
                    城市 = Request.Form["城市__" + i],
                    区县 = Request.Form["区县__" + i]
                };

                t_condition.所在地区.Add(place);

                condition.Add(t_condition);
            }
            model.供应商抽选条件 = condition;
            供应商抽选管理.添加供应商抽选历史(model);

            return RedirectToAction("GysChoose_Applay_S");
        }

        [单一权限验证(权限.我可审核的供应商抽取申请)]
        public ActionResult GysChoose_ApplayAuditList()
        {
            return View("/Views/默认主题/后台/单位用户后台/GysChoose_ApplayAuditList.cshtml");
        }
        public ActionResult Part_GysChoose_ApplayAuditList()
        {
            int page = 1;

            int pre_listcount = 供应商抽选管理.查询供应商抽选历史记录(0, 0, Query.EQ("申请抽选状态", 申请抽选状态.已提交待批准).And(Query<供应商抽选记录>.EQ(o => o.基本数据.已屏蔽, false))).Count();
            int pre_maxpage = Math.Max((pre_listcount + PAGESIZE - 1) / PAGESIZE, 1);
            ViewData["pre_currentPage"] = page;
            ViewData["pre_pagecount"] = pre_maxpage;
            ViewData["供应商抽取待批准列表"] = 供应商抽选管理.查询供应商抽选历史记录(0, PAGESIZE, Query.EQ("申请抽选状态", 申请抽选状态.已提交待批准).And(Query<供应商抽选记录>.EQ(o => o.基本数据.已屏蔽, false)));


            pre_listcount = 供应商抽选管理.查询供应商抽选历史记录(0, 0, Query.EQ("申请抽选状态", 申请抽选状态.已批准待抽选)).Count();
            pre_maxpage = Math.Max((pre_listcount + PAGESIZE - 1) / PAGESIZE, 1);
            ViewData["ing_currentPage"] = page;
            ViewData["ing_pagecount"] = pre_maxpage;
            ViewData["供应商抽取已批准列表"] = 供应商抽选管理.查询供应商抽选历史记录(0, PAGESIZE, Query.EQ("申请抽选状态", 申请抽选状态.已批准待抽选));

            pre_listcount = 供应商抽选管理.查询供应商抽选历史记录(0, 0, Query.EQ("申请抽选状态", 申请抽选状态.未获批准)).Count();
            pre_maxpage = Math.Max((pre_listcount + PAGESIZE - 1) / PAGESIZE, 1);
            ViewData["no_currentPage"] = page;
            ViewData["no_pagecount"] = pre_maxpage;
            ViewData["供应商抽取未获批准列表"] = 供应商抽选管理.查询供应商抽选历史记录(0, PAGESIZE, Query.EQ("申请抽选状态", 申请抽选状态.未获批准));

            return PartialView("/Views/默认主题/后台/单位用户后台/Procure_Part/Part_GysChoose_ApplayAuditList.cshtml");
        }
        public ActionResult Part_GysChoose_ApplayAuditList_pre(int? page)
        {
            int listcount = 供应商抽选管理.查询供应商抽选历史记录(0, 0, Query.EQ("申请抽选状态", 申请抽选状态.已提交待批准).And(Query<供应商抽选记录>.EQ(o => o.基本数据.已屏蔽, false))).Count();
            int maxpage = Math.Max((listcount + PAGESIZE - 1) / PAGESIZE, 1);
            if (string.IsNullOrEmpty(page.ToString()) || page < 0 || page > maxpage)
            {
                page = 1;
            }

            ViewData["pre_currentPage"] = page;
            ViewData["pre_pagecount"] = maxpage;
            ViewData["供应商抽取待批准列表"] = 供应商抽选管理.查询供应商抽选历史记录(PAGESIZE * (int.Parse(page.ToString()) - 1), PAGESIZE, Query.EQ("申请抽选状态", 申请抽选状态.已提交待批准).And(Query<供应商抽选记录>.EQ(o => o.基本数据.已屏蔽, false)));

            return PartialView("/Views/默认主题/后台/单位用户后台/Procure_Part/Part_GysChoose_ApplayAuditList_pre.cshtml");
        }

        public ActionResult Part_GysChoose_ApplayAuditList_ing(int? page)
        {
            int listcount = 供应商抽选管理.查询供应商抽选历史记录(0, 0, Query.EQ("申请抽选状态", 申请抽选状态.已批准待抽选)).Count();
            int maxpage = Math.Max((listcount + PAGESIZE - 1) / PAGESIZE, 1);

            if (string.IsNullOrEmpty(page.ToString()) || page < 0 || page > maxpage)
            {
                page = 1;
            }
            ViewData["ing_currentPage"] = page;
            ViewData["ing_pagecount"] = maxpage;
            ViewData["供应商抽取已批准列表"] = 供应商抽选管理.查询供应商抽选历史记录(PAGESIZE * (int.Parse(page.ToString()) - 1), PAGESIZE, Query.EQ("申请抽选状态", 申请抽选状态.已批准待抽选));

            return PartialView("/Views/默认主题/后台/单位用户后台/Procure_Part/Part_GysChoose_ApplayAuditList_ing.cshtml");
        }

        public ActionResult Part_GysChoose_ApplayAuditList_no(int? page)
        {
            int listcount = 供应商抽选管理.查询供应商抽选历史记录(0, 0, Query.EQ("申请抽选状态", 申请抽选状态.未获批准)).Count();
            int maxpage = Math.Max((listcount + PAGESIZE - 1) / PAGESIZE, 1);
            if (string.IsNullOrEmpty(page.ToString()) || page < 0 || page > maxpage)
            {
                page = 1;
            }
            ViewData["no_currentPage"] = page;
            ViewData["no_pagecount"] = maxpage;
            ViewData["供应商抽取未获批准列表"] = 供应商抽选管理.查询供应商抽选历史记录(PAGESIZE * (int.Parse(page.ToString()) - 1), PAGESIZE, Query.EQ("申请抽选状态", 申请抽选状态.未获批准));

            return PartialView("/Views/默认主题/后台/单位用户后台/Procure_Part/Part_GysChoose_ApplayAuditList_no.cshtml");
        }

        public ActionResult GysChoose_ApplayAudit()
        {
            return View("/Views/默认主题/后台/单位用户后台/GysChoose_ApplayAudit.cshtml");
        }
        public ActionResult Part_GysChoose_ApplayAudit(int? id)
        {
            供应商抽选记录 model = new 供应商抽选记录();
            if (id != null)
            {
                model = 供应商抽选管理.查找供应商抽选历史记录((long)id);
            }
            return PartialView("/Views/默认主题/后台/单位用户后台/Procure_Part/Part_GysChoose_ApplayAudit.cshtml", model);
        }
        [HttpPost]

        public ActionResult GysChoose_ApplayAudit(string action, int? id)
        {
            if (action == "审核通过")
            {
                供应商抽选管理.批准供应商抽选申请((long)id, this.HttpContext.获取当前用户<单位用户>().Id, 申请抽选状态.已批准待抽选);
            }

            if (action == "审核不通过")
            {
                供应商抽选管理.批准供应商抽选申请((long)id, this.HttpContext.获取当前用户<单位用户>().Id, 申请抽选状态.未获批准);
            }

            return View("/Views/默认主题/后台/单位用户后台/GysChoose_ApplayAuditList.cshtml");
        }

        public ActionResult GysChoose()
        {
            return View("/Views/默认主题/后台/单位用户后台/GysChoose.cshtml");
        }
        public ActionResult Part_GysChoose(int? id)
        {
            供应商抽选记录 model = new 供应商抽选记录();
            try
            {
                model = 供应商抽选管理.查找供应商抽选历史记录((long)id);
                var str = "";
                foreach (var m in model.供应商抽选条件)
                {
                    str += m.需要供应商数量 + ",";
                }
                ViewData["抽取总数列表"] = str.Substring(0, str.Length - 1);
            }
            catch
            {
                return Content("<script>alert('页面有误');location.href='javascript:history.go(-1)';</script>");
            }

            return PartialView("/Views/默认主题/后台/单位用户后台/Procure_Part/Part_GysChoose.cshtml", model);
        }

        [HttpPost]
        public ActionResult GysChoose(int? id)
        {
            供应商抽选记录 model = new 供应商抽选记录();
            try
            {
                model = 供应商抽选管理.查找供应商抽选历史记录((long)id);
            }
            catch
            {
                return Content("<script>alert('页面有误');location.href='javascript:history.go(-1)';</script>");
            }

            int count = 0;
            int.TryParse(Request.Form["turncount"], out count);
            for (int i = 0; i < count; i++)
            {
                var flag = true;
                _地域 place = new _地域 { 省份 = Request.Form["deliverprovince___" + i], 城市 = Request.Form["delivercity___" + i], 区县 = Request.Form["deliverarea___" + i] };
                if (string.IsNullOrEmpty(Request.Form["deliverprovince___" + i]))
                {
                    place.省份 = "不限";
                    place.城市 = "不限";
                    place.区县 = "不限";
                }

                ////model.专家抽选轮次[i].从事专业 = Request.Form["从事专业__" + i];
                ////model.专家抽选轮次[i].专业技术职称 = (专业技术职称)int.Parse(Request.Form["专业技术职称___" + i]);
                ////model.专家抽选轮次[i].屏蔽列表 = Request.Form["回避列表__" + i];
                //model.供应商抽选轮次[i].所在地区 = place;
                //model.供应商抽选轮次[i].预定抽选供应商数 = int.Parse(Request.Form["抽取个数__" + i]);
                ////model.专家抽选轮次[i].专家类别 = (专家类别)int.Parse(Request.Form["专家类别___" + i]);
                ////model.专家抽选轮次[i].专家类型 = (专家类型)int.Parse(Request.Form["专家类型___" + i]);
                ////model.专家抽选轮次[i].最低要求学位 = (学位)int.Parse(Request.Form["最高学位___" + i]);
                //model.供应商抽选轮次[i].所属行业 = Request.Form["hy___" + i];

                string str = Request.Form["historyturn__" + i];
                //str "1|2014-07-24 09:57:29|^1|146|1^2|116|1^3|131|1^4|113|0,88888888888^5|132|0,666666666^6|156|1^7|106|1^8|150|1^%2|2014-07-24 09:57:47|^9|136|1^10|151|0,9999^%3|2014-07-24 09:57:53|^11|145|1^%"

                string[] a1 = str.Substring(0, str.Length - 1).Split('%');
                for (int aj = 0; aj < a1.Length; aj++)
                {
                    //if (a1[aj].Substring(0, a1[aj].Length - 1).Split('^')[1] == "isnot_enough")
                    //{
                    //    model.供应商抽选轮次[i].本轮抽选结果 = 本轮供应商抽选结果.可用供应商数量不足;
                    //    if (i == 0)
                    //    {
                    //        operatetime = a1[0].Split('|')[1];
                    //    }
                    //    flag = false;
                    //}
                }

                if (flag)
                {
                    //model.供应商抽选轮次[i].本轮抽选结果 = 本轮供应商抽选结果.正常;
                    ////a1[i] 1|2014-07-24 09:57:29|^1|146|1^2|116|1^3|131|1^4|113|0,88888888888^5|132|0,666666666^6|156|1^7|106|1^8|150|1^
                    ////a1[i] 2|2014-07-24 09:57:47|^9|136|1^10|151|0,9999^
                    //foreach (var item in a1)
                    //{
                    //    _供应商抽选批次 numlist = new _供应商抽选批次();

                    //    string[] a2 = item.Substring(0, item.Length - 1).Split('^');
                    //    //a2[0] 1|2014-07-24 09:57:29|   a2[i] 4|113|0,88888888888     1|146|1
                    //    string[] numstr = a2[0].Split('|');
                    //    //historylist.抽取批次 = int.Parse(numstr.Split('|')[0]);
                    //    numlist.抽选批次 = int.Parse(numstr[0]);
                    //    numlist.抽选时间 = Convert.ToDateTime(numstr[1]);

                    //    for (int j = 1; j < a2.Length; j++)
                    //    {
                    //        string[] a3 = a2[j].Split('|');

                    //        供应商 zj = new 供应商();
                    //        zj = 用户管理.查找用户<供应商>(long.Parse(a3[1]));

                    //        int 序号 = int.Parse(a3[0]);
                    //        bool 预定出席 = true;
                    //        string 不能出席原因 = "";
                    //        if (a3[2] != "1")
                    //        {
                    //            预定出席 = false;
                    //            不能出席原因 = a3[2].Substring(2, a3[2].Length - 2);
                    //        }

                    //        _供应商抽选条目 historylist = new _供应商抽选条目()
                    //        {
                    //            不能出席原因 = 不能出席原因,
                    //            联系方式 = zj.联系方式.手机 + "," + zj.联系方式.固定电话,
                    //            所在地区 = zj.所属地域,
                    //            //性别 = zj.身份信息.性别,
                    //            //姓名 = zj.身份信息.姓名,
                    //            序号 = 序号,
                    //            预定出席 = 预定出席,

                    //            //证件号 = zj.身份信息.证件号,
                    //            //证件类型 = zj.身份信息.证件类型,
                    //            //专家类别 = zj.身份信息.专家类别,
                    //            //专业技术职称 = zj.学历信息.专业技术职称,
                    //            //最高学历 = zj.学历信息.最高学历,
                    //            //最高学位 = zj.学历信息.最高学位,
                    //            //从事专业 = zj.工作经历信息.从事专业,
                    //            //专家类型 = zj.身份信息.专家类型
                    //        };
                    //        historylist.供应商.用户ID = zj.Id;
                    //        numlist.供应商列表.Add(historylist);
                    //    }
                    //    model.供应商抽选轮次[i].供应商抽选批次.Add(numlist);
                    //}
                }
            }

            model.申请抽选状态 = 申请抽选状态.已完成抽选;
            //model.操作人.用户ID = this.HttpContext.获取当前用户<单位用户>().Id;
            //if (string.IsNullOrEmpty(operatetime))
            //{
            //    model.操作时间 = model.供应商抽选轮次[0].供应商抽选批次[0].抽选时间;
            //}
            //else
            //{
            //    model.操作时间 = Convert.ToDateTime(operatetime);
            //}
            //model.确认人.用户ID = this.HttpContext.获取当前用户<单位用户>().Id;
            model.确认时间 = DateTime.Now;

            供应商抽选管理.更新供应商抽选历史记录(model);

            return View("/Views/默认主题/后台/单位用户后台/GysChoose_ApplayAuditList.cshtml");
        }
        //public ActionResult SearchByCondition_Gys()
        //{
        //    return Content("0");
        //}
        public class SearchParam_gys
        {
            public List<供应商> list { get; set; }
            public int n { get; set; }//抽选批次

            public int trcount { get; set; }//当前最大行号

            public int turn { get; set; }//当前轮次

            //public string condition { get; set; }//条件
        }

        [单一权限验证(权限.供应商全部抽取记录列表)]
        public ActionResult GysChoose_HistoryList()
        {
            return View("/Views/默认主题/后台/单位用户后台/GysChoose_HistoryList.cshtml");
        }
        public ActionResult Part_GysChoose_HistoryList()
        {
            IEnumerable<供应商抽选记录> hisrecord = 供应商抽选管理.查询供应商抽选历史记录(0, 0, Query.EQ("申请抽选状态", 申请抽选状态.已完成抽选));
            int page = 0;
            if (string.IsNullOrEmpty(Request.QueryString["page"]))
            {
                page = 1;
            }
            else
            {
                page = int.Parse(Request.QueryString["page"]);
            }
            int PageCount = hisrecord.Count() / 10;
            if (hisrecord.Count() % 10 > 0)
            {
                PageCount++;
            }
            ViewData["CurrentPage"] = page;
            ViewData["Pagecount"] = PageCount;
            ViewData["历史抽选供应商列表"] = 供应商抽选管理.查询供应商抽选历史记录(10 * (page- 1), 10, Query.EQ("申请抽选状态", 申请抽选状态.已完成抽选));
            return PartialView("/Views/默认主题/后台/单位用户后台/Procure_Part/Part_GysChoose_HistoryList.cshtml");
        }
        public ActionResult Part_GysChoose_HistoryDetail(int? id)
        {
            if (Request.QueryString["c"] != null && Request.QueryString["c"] == "s")
            {
                ViewData["come"] = "我提交的供应商抽取申请";
            }
            else
            {
                ViewData["come"] = "供应商历史抽取记录";
            }
            try
            {
                if (id == null)
                {
                    return Content("<script>window.location='/专家抽选/GysChoose_HistoryList'</script>");
                }
                else
                {
                    供应商抽选记录 model = 供应商抽选管理.查找供应商抽选历史记录((long)id);
                    if (model != null)
                    {
                        return PartialView("/Views/默认主题/后台/单位用户后台/Procure_Part/Part_GysChoose_HistoryDetail.cshtml", model);
                    }
                    else
                    {
                        return Content("<script>window.location='/专家抽选/GysChoose_HistoryList'</script>");
                    }
                }
            }
            catch
            {
                return Content("<script>window.location='/专家抽选/GysChoose_HistoryList'</script>");
            }
        }
        public ActionResult GysChoose_HistoryDetail()
        {
            return View("/Views/默认主题/后台/单位用户后台/GysChoose_HistoryDetail.cshtml");
        }

        public ActionResult Expert_Scoring()
        {
            return View("/Views/默认主题/后台/单位用户后台/Expert_Scoring.cshtml");
        }
        public ActionResult Part_Expert_Scoring(long? id)
        {
            if (Request.QueryString["c"] != null && Request.QueryString["c"] == "s")
            {
                ViewData["come"] = "我可进行的抽取申请";
            }
            else if (Request.QueryString["c"] != null && Request.QueryString["c"] == "d")
            {
                ViewData["come"] = "我已完成的抽取申请";
            }
            else
            {
                ViewData["come"] = "全部抽取记录列表";
            }
            try
            {
                if (id == null)
                {
                    return Content("<script>window.location='/专家抽选/Expert_History'</script>");
                }
                else
                {
                    专家抽选记录 model = 专家抽选管理.查找专家抽选记录((long)id);
                    if (model != null)
                    {
                        return PartialView("/Views/默认主题/后台/单位用户后台/Procure_Part/Part_Expert_Scoring.cshtml", model);
                    }
                    else
                    {
                        return Content("<script>window.location='/专家抽选/Expert_History'</script>");
                    }
                }
            }
            catch
            {
                return Content("<script>window.location='/专家抽选/Expert_History'</script>");
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
            return RedirectToAction("Expert_Applay_S");
        }
        [单一权限验证(权限.我可进行的评审专家抽取申请)]
        public ActionResult Expert_Applay_S()
        {
            return View("/Views/默认主题/后台/单位用户后台/Expert_Applay_S.cshtml");
        }
        public ActionResult Part_Expert_Applay_S()
        {
            int page = 1;

            int pre_listcount = 专家抽选管理.查询专家抽选记录(0, 0, Query.EQ("申请抽选状态", 申请抽选状态.已提交待批准).And(Query<专家抽选记录>.EQ(o=>o.是否一体机抽取,false)).And(Query<专家抽选记录>.EQ(o => o.经办人.用户ID, HttpContext.获取当前用户<单位用户>().Id))).Count();
            int pre_maxpage = Math.Max((pre_listcount + PAGESIZE - 1) / PAGESIZE, 1);
            ViewData["pre_currentPage"] = page;
            ViewData["pre_pagecount"] = pre_maxpage;
            ViewData["专家抽取待批准列表"] = 专家抽选管理.查询专家抽选记录(0, PAGESIZE, Query.EQ("申请抽选状态", 申请抽选状态.已提交待批准).And(Query<专家抽选记录>.EQ(o => o.是否一体机抽取, false)).And(Query<专家抽选记录>.EQ(o => o.经办人.用户ID, HttpContext.获取当前用户<单位用户>().Id)));


            pre_listcount = 专家抽选管理.查询专家抽选记录(0, 0, Query.EQ("申请抽选状态", 申请抽选状态.已批准待抽选).And(Query<专家抽选记录>.EQ(o => o.经办人.用户ID, HttpContext.获取当前用户<单位用户>().Id))).Count();
            pre_maxpage = Math.Max((pre_listcount + PAGESIZE - 1) / PAGESIZE, 1);
            ViewData["ing_currentPage"] = page;
            ViewData["ing_pagecount"] = pre_maxpage;
            ViewData["专家抽取已批准列表"] = 专家抽选管理.查询专家抽选记录(0, PAGESIZE, Query.EQ("申请抽选状态", 申请抽选状态.已批准待抽选).And(Query<专家抽选记录>.EQ(o => o.经办人.用户ID, HttpContext.获取当前用户<单位用户>().Id)));

            

            pre_listcount = 专家抽选管理.查询专家抽选记录(0, 0, Query.EQ("申请抽选状态", 申请抽选状态.未获批准).And(Query<专家抽选记录>.EQ(o => o.经办人.用户ID, HttpContext.获取当前用户<单位用户>().Id))).Count();
            pre_maxpage = Math.Max((pre_listcount + PAGESIZE - 1) / PAGESIZE, 1);
            ViewData["no_currentPage"] = page;
            ViewData["no_pagecount"] = pre_maxpage;
            ViewData["专家抽取未获批准列表"] = 专家抽选管理.查询专家抽选记录(0, PAGESIZE, Query.EQ("申请抽选状态", 申请抽选状态.未获批准).And(Query<专家抽选记录>.EQ(o => o.经办人.用户ID, HttpContext.获取当前用户<单位用户>().Id)));

            return PartialView("/Views/默认主题/后台/单位用户后台/Procure_Part/Part_Expert_Applay_S.cshtml");
        }

        [单一权限验证(权限.我已完成的评审专家抽取申请)]
        public ActionResult Expert_Applay_ed()
        {
            return View("/Views/默认主题/后台/单位用户后台/Expert_Applay_ed.cshtml");
        }
        public ActionResult Part_Expert_Applay_ed()
        {
            int page = 1;
            int  pre_listcount = 专家抽选管理.查询专家抽选记录(0, 0, Query.EQ("申请抽选状态", 申请抽选状态.已完成抽选).And(Query<专家抽选记录>.EQ(o => o.经办人.用户ID, HttpContext.获取当前用户<单位用户>().Id))).Count();
            int  pre_maxpage = Math.Max((pre_listcount + PAGESIZE - 1) / PAGESIZE, 1);
            ViewData["ed_currentPage"] = page;
            ViewData["ed_pagecount"] = pre_maxpage;
            ViewData["已完成的抽选列表"] = 专家抽选管理.查询专家抽选记录(0, PAGESIZE, Query.EQ("申请抽选状态", 申请抽选状态.已完成抽选).And(Query<专家抽选记录>.EQ(o => o.经办人.用户ID, HttpContext.获取当前用户<单位用户>().Id)));
            return PartialView("/Views/默认主题/后台/单位用户后台/Procure_Part/Part_Expert_Applay_ed.cshtml");
        }


        public ActionResult Part_Expert_Applay_S_pre(int? page)
        {
            int listcount = 专家抽选管理.查询专家抽选记录(0, 0, Query.EQ("申请抽选状态", 申请抽选状态.已提交待批准).And(Query<专家抽选记录>.EQ(o => o.是否一体机抽取, false)).And(Query<专家抽选记录>.EQ(o => o.经办人.用户ID, HttpContext.获取当前用户<单位用户>().Id))).Count();
            int maxpage = Math.Max((listcount + PAGESIZE - 1) / PAGESIZE, 1);
            if (string.IsNullOrEmpty(page.ToString()) || page < 0 || page > maxpage)
            {
                page = 1;
            }

            ViewData["pre_currentPage"] = page;
            ViewData["pre_pagecount"] = maxpage;
            ViewData["专家抽取待批准列表"] = 专家抽选管理.查询专家抽选记录(PAGESIZE * (int.Parse(page.ToString()) - 1), PAGESIZE, Query.EQ("申请抽选状态", 申请抽选状态.已提交待批准).And(Query<专家抽选记录>.EQ(o => o.是否一体机抽取, false)).And(Query<专家抽选记录>.EQ(o => o.经办人.用户ID, HttpContext.获取当前用户<单位用户>().Id)));

            return PartialView("/Views/默认主题/后台/单位用户后台/Procure_Part/Part_Expert_Applay_S_pre.cshtml");
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

            return PartialView("/Views/默认主题/后台/单位用户后台/Procure_Part/Part_Expert_Applay_S_ing.cshtml");
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

            return PartialView("/Views/默认主题/后台/单位用户后台/Procure_Part/Part_Expert_Applay_S_no.cshtml");
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

            return PartialView("/Views/默认主题/后台/单位用户后台/Procure_Part/Part_Expert_Applay_S_ed.cshtml");
        }
        public ActionResult Expert_ApplayCancel()
        {
            return View("/Views/默认主题/后台/单位用户后台/Expert_ApplayCancel.cshtml");
        }
        public ActionResult Part_Expert_ApplayCancel(int? id)
        {
            专家抽选记录 model = new 专家抽选记录();
            if (id != null)
            {
                model = 专家抽选管理.查找专家抽选记录((long)id);
            }
            return PartialView("/Views/默认主题/后台/单位用户后台/Procure_Part/Part_Expert_ApplayCancel.cshtml", model);
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
            return View("/Views/默认主题/后台/单位用户后台/Expert_Applay_S.cshtml");
        }

        [单一权限验证(权限.我可进行的供应商抽取申请, 权限.我已完成的供应商抽取申请)]
        public ActionResult GysChoose_Applay_S()
        {
            return View("/Views/默认主题/后台/单位用户后台/GysChoose_Applay_S.cshtml");
        }
        public ActionResult Part_GysChoose_Applay_S()
        {
            int page = 1;

            int pre_listcount = 供应商抽选管理.查询供应商抽选历史记录(0, 0, Query.EQ("申请抽选状态", 申请抽选状态.已提交待批准).And(Query<供应商抽选记录>.EQ(o => o.经办人.用户ID, HttpContext.获取当前用户<单位用户>().Id))).Count();
            int pre_maxpage = Math.Max((pre_listcount + PAGESIZE - 1) / PAGESIZE, 1);
            ViewData["pre_currentPage"] = page;
            ViewData["pre_pagecount"] = pre_maxpage;
            ViewData["供应商抽取待批准列表"] = 供应商抽选管理.查询供应商抽选历史记录(0, PAGESIZE, Query.EQ("申请抽选状态", 申请抽选状态.已提交待批准).And(Query<供应商抽选记录>.EQ(o => o.经办人.用户ID, HttpContext.获取当前用户<单位用户>().Id)));


            pre_listcount = 供应商抽选管理.查询供应商抽选历史记录(0, 0, Query.EQ("申请抽选状态", 申请抽选状态.已批准待抽选).And(Query<供应商抽选记录>.EQ(o => o.经办人.用户ID, HttpContext.获取当前用户<单位用户>().Id))).Count();
            pre_maxpage = Math.Max((pre_listcount + PAGESIZE - 1) / PAGESIZE, 1);
            ViewData["ing_currentPage"] = page;
            ViewData["ing_pagecount"] = pre_maxpage;
            ViewData["供应商抽取已批准列表"] = 供应商抽选管理.查询供应商抽选历史记录(0, PAGESIZE, Query.EQ("申请抽选状态", 申请抽选状态.已批准待抽选).And(Query<供应商抽选记录>.EQ(o => o.经办人.用户ID, HttpContext.获取当前用户<单位用户>().Id)));

            pre_listcount = 供应商抽选管理.查询供应商抽选历史记录(0, 0, Query.EQ("申请抽选状态", 申请抽选状态.未获批准).And(Query<供应商抽选记录>.EQ(o => o.经办人.用户ID, HttpContext.获取当前用户<单位用户>().Id))).Count();
            pre_maxpage = Math.Max((pre_listcount + PAGESIZE - 1) / PAGESIZE, 1);
            ViewData["no_currentPage"] = page;
            ViewData["no_pagecount"] = pre_maxpage;
            ViewData["供应商抽取未获批准列表"] = 供应商抽选管理.查询供应商抽选历史记录(0, PAGESIZE, Query.EQ("申请抽选状态", 申请抽选状态.未获批准).And(Query<供应商抽选记录>.EQ(o => o.经办人.用户ID, HttpContext.获取当前用户<单位用户>().Id)));

            pre_listcount = 供应商抽选管理.查询供应商抽选历史记录(0, 0, Query.EQ("申请抽选状态", 申请抽选状态.已完成抽选).And(Query<供应商抽选记录>.EQ(o => o.经办人.用户ID, HttpContext.获取当前用户<单位用户>().Id))).Count();
            pre_maxpage = Math.Max((pre_listcount + PAGESIZE - 1) / PAGESIZE, 1);
            ViewData["ed_currentPage"] = page;
            ViewData["ed_pagecount"] = pre_maxpage;
            ViewData["供应商抽取已抽选列表"] = 供应商抽选管理.查询供应商抽选历史记录(0, PAGESIZE, Query.EQ("申请抽选状态", 申请抽选状态.已完成抽选).And(Query<供应商抽选记录>.EQ(o => o.经办人.用户ID, HttpContext.获取当前用户<单位用户>().Id)));

            return PartialView("/Views/默认主题/后台/单位用户后台/Procure_Part/Part_GysChoose_Applay_S.cshtml");
        }

        public ActionResult Part_GysChoose_ApplayAuditList_pre_S(int? page)
        {
            int listcount = 供应商抽选管理.查询供应商抽选历史记录(0, 0, Query.EQ("申请抽选状态", 申请抽选状态.已提交待批准).And(Query<供应商抽选记录>.EQ(o => o.经办人.用户ID, HttpContext.获取当前用户<单位用户>().Id))).Count();
            int maxpage = Math.Max((listcount + PAGESIZE - 1) / PAGESIZE, 1);
            if (string.IsNullOrEmpty(page.ToString()) || page < 0 || page > maxpage)
            {
                page = 1;
            }

            ViewData["pre_currentPage"] = page;
            ViewData["pre_pagecount"] = maxpage;
            ViewData["供应商抽取待批准列表"] = 供应商抽选管理.查询供应商抽选历史记录(PAGESIZE * (int.Parse(page.ToString()) - 1), PAGESIZE, Query.EQ("申请抽选状态", 申请抽选状态.已提交待批准).And(Query<供应商抽选记录>.EQ(o => o.经办人.用户ID, HttpContext.获取当前用户<单位用户>().Id)));

            return PartialView("/Views/默认主题/后台/单位用户后台/Procure_Part/Part_GysChoose_ApplayAuditList_pre_S.cshtml");
        }

        public ActionResult Part_GysChoose_ApplayAuditList_ing_S(int? page)
        {
            int listcount = 供应商抽选管理.查询供应商抽选历史记录(0, 0, Query.EQ("申请抽选状态", 申请抽选状态.已批准待抽选).And(Query<供应商抽选记录>.EQ(o => o.经办人.用户ID, HttpContext.获取当前用户<单位用户>().Id))).Count();
            int maxpage = Math.Max((listcount + PAGESIZE - 1) / PAGESIZE, 1);

            if (string.IsNullOrEmpty(page.ToString()) || page < 0 || page > maxpage)
            {
                page = 1;
            }
            ViewData["ing_currentPage"] = page;
            ViewData["ing_pagecount"] = maxpage;
            ViewData["供应商抽取已批准列表"] = 供应商抽选管理.查询供应商抽选历史记录(PAGESIZE * (int.Parse(page.ToString()) - 1), PAGESIZE, Query.EQ("申请抽选状态", 申请抽选状态.已批准待抽选).And(Query<供应商抽选记录>.EQ(o => o.经办人.用户ID, HttpContext.获取当前用户<单位用户>().Id)));

            return PartialView("/Views/默认主题/后台/单位用户后台/Procure_Part/Part_GysChoose_ApplayAuditList_ing_S.cshtml");
        }

        public ActionResult Part_GysChoose_ApplayAuditList_no_S(int? page)
        {
            int listcount = 供应商抽选管理.查询供应商抽选历史记录(0, 0, Query.EQ("申请抽选状态", 申请抽选状态.未获批准).And(Query<供应商抽选记录>.EQ(o => o.经办人.用户ID, HttpContext.获取当前用户<单位用户>().Id))).Count();
            int maxpage = Math.Max((listcount + PAGESIZE - 1) / PAGESIZE, 1);
            if (string.IsNullOrEmpty(page.ToString()) || page < 0 || page > maxpage)
            {
                page = 1;
            }
            ViewData["no_currentPage"] = page;
            ViewData["no_pagecount"] = maxpage;
            ViewData["供应商抽取未获批准列表"] = 供应商抽选管理.查询供应商抽选历史记录(PAGESIZE * (int.Parse(page.ToString()) - 1), PAGESIZE, Query.EQ("申请抽选状态", 申请抽选状态.未获批准).And(Query<供应商抽选记录>.EQ(o => o.经办人.用户ID, HttpContext.获取当前用户<单位用户>().Id)));

            return PartialView("/Views/默认主题/后台/单位用户后台/Procure_Part/Part_GysChoose_ApplayAuditList_no_S.cshtml");
        }
        public ActionResult Part_GysChoose_ApplayAuditList_ed_S(int? page)
        {
            int listcount = 供应商抽选管理.查询供应商抽选历史记录(0, 0, Query.EQ("申请抽选状态", 申请抽选状态.已完成抽选).And(Query<供应商抽选记录>.EQ(o => o.经办人.用户ID, HttpContext.获取当前用户<单位用户>().Id))).Count();
            int maxpage = Math.Max((listcount + PAGESIZE - 1) / PAGESIZE, 1);
            if (string.IsNullOrEmpty(page.ToString()) || page < 0 || page > maxpage)
            {
                page = 1;
            }
            ViewData["ed_currentPage"] = page;
            ViewData["ed_pagecount"] = maxpage;
            ViewData["供应商抽取已抽选列表"] = 供应商抽选管理.查询供应商抽选历史记录(PAGESIZE * (int.Parse(page.ToString()) - 1), PAGESIZE, Query.EQ("申请抽选状态", 申请抽选状态.已完成抽选).And(Query<供应商抽选记录>.EQ(o => o.经办人.用户ID, HttpContext.获取当前用户<单位用户>().Id)));

            return PartialView("/Views/默认主题/后台/单位用户后台/Procure_Part/Part_GysChoose_ApplayAuditList_ed_S.cshtml");
        }
        public ActionResult Gys_ApplayCancel()
        {
            return View("/Views/默认主题/后台/单位用户后台/Gys_ApplayCancel.cshtml");
        }
        public ActionResult Part_Gys_ApplayCancel(int? id)
        {
            供应商抽选记录 model = new 供应商抽选记录();
            if (id != null)
            {
                model = 供应商抽选管理.查找供应商抽选历史记录((long)id);
            }
            return PartialView("/Views/默认主题/后台/单位用户后台/Procure_Part/Part_Gys_ApplayCancel.cshtml", model);
        }
        public ActionResult Gys_ApplayCancelAction(long? id)
        {
            try
            {
                var model = 供应商抽选管理.查找供应商抽选历史记录((long)id);
                model.基本数据.已屏蔽 = true;
                供应商抽选管理.更新供应商抽选历史记录(model);
            }
            catch
            {

            }
            return View("/Views/默认主题/后台/单位用户后台/GysChoose_Applay_S.cshtml");
        }
    }
#endif
}