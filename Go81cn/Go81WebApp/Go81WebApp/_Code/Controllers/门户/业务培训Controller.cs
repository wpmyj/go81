using Go81WebApp.Models.管理器;
using Go81WebApp.Models.数据模型.内容数据模型;
using MongoDB.Driver.Builders;
using System;
using System.Collections;
using System.Configuration;
using System.Web.Mvc;
using System.Collections.Generic;
using Go81WebApp.Models.管理器.内容管理;
using Go81WebApp.Models.数据模型;

namespace Go81WebApp.Controllers.门户
{
    public class 业务培训Controller : Controller
    {
        private static int PAGESIZE = int.Parse(ConfigurationManager.AppSettings["下载每页显示条数"]);
        //
        // GET: /下载/
        public ActionResult TrainingList()
        {
            return View();
        }
        public ActionResult Part_TrainingList(int? page)
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

            int listcount = (int)(培训资料管理.计数培训资料(0, 0,null, false));
            int maxpage = Math.Max((listcount + PAGESIZE - 1) / PAGESIZE, 1);
            if (p > maxpage)
            {
                p = 1;
            }


            ViewData["currentpage"] = p;
            ViewData["listcount"] = listcount;
            ViewData["pagesize"] = PAGESIZE;
            ViewData["pagecount"] = maxpage;

            ViewData["业务培训列表"] = 培训资料管理.查询培训资料(PAGESIZE * (p - 1), PAGESIZE,null, false, SortBy.Descending("内容主体.发布时间"), includeDisabled: false);
            return PartialView("Part_Training/Part_TrainingList");
        }
        public ActionResult TrainingDetail()
        {
            return View();
        }
        public ActionResult Part_TrainingDetail()
        {
            培训资料 model = null;
            try
            {
                model = 培训资料管理.查找培训资料(int.Parse(Request.QueryString["id"]));
            }
            catch
            {

            }
            return PartialView("Part_Training/Part_TrainingDetail", model);
        }


        public ActionResult Part_SearchByPage()
        {
            int page = 1;
            int.TryParse(Request.Params["page"], out page);

            int listcount = (int)(培训资料管理.计数培训资料(0, 0,null, false));
            int maxpage = Math.Max((listcount + PAGESIZE - 1) / PAGESIZE, 1);

            ViewData["currentpage"] = page;
            ViewData["pagecount"] = maxpage;

            ViewData["业务培训列表"] = 培训资料管理.查询培训资料(PAGESIZE * (page - 1), PAGESIZE,null, false, SortBy.Descending("内容主体.发布时间"), false);
            return PartialView("Part_Training/Part_SearchByPage");
        }

        public ActionResult Part_OtherList()
        {
            ViewData["最近业务培训"] = 培训资料管理.查询培训资料(0, 10,null, false, SortBy.Descending("内容主体.发布时间"));
            return PartialView("Part_Training/Part_OtherList");
        }

        //public ActionResult Download_Detail()
        //{
        //    return View();
        //}
        //public ActionResult Part_Download_Detail(int? id)
        //{
        //    下载 g = null;
        //    ViewData["登录"] = "0";
        //    try
        //    {
        //        下载管理.增加点击次数((long)id);
        //        g = 下载管理.查找下载((long)id);
        //        g.下载验证码 = "";
        //        if (HttpContext.检查登录() != -1)
        //        {
        //            ViewData["登录"] = "1";
        //        }
        //    }
        //    catch
        //    {

        //    }

        //    return PartialView("Part_Download/Part_Download_Detail", g);
        //}

        //public ActionResult Part_Download_OtherList()
        //{
        //    ViewData["下载"] = 下载管理.查询下载(0, 10, null, false, SortBy<下载>.Descending(o => o.下载次数), false);
        //    return PartialView("Part_Download/Part_Download_OtherList");
        //}
       
        //public JsonResult GetValue()
        //{
        //    List<DownLoads> data = new List<DownLoads>();
        //    int page = int.Parse(Request.QueryString["p"]);
        //    if (page == 0)
        //    {
        //        page = 1;
        //    }
        //    int type = int.Parse(Request.QueryString["type"]);
        //    long count = 下载管理.计数下载(0, 0, Query<下载>.Where(m => (int)m.下载类型 == type),includeDisabled:false);
        //    long pagesize = count / 20;
        //    if (count % 20 > 0)
        //    {
        //        pagesize++;
        //    }
        //    IEnumerable<下载> files = 下载管理.查询下载(20 * (page - 1), 20, Query<下载>.Where(m => (int)m.下载类型 == type),includeDisabled:false);
        //    if (files != null)
        //    {
        //        foreach (var item in files)
        //        {
        //            DownLoads d = new DownLoads();
        //            d.Id = item.Id;
        //            d.Title = item.内容主体.标题;
        //            d.Clicks = item.下载次数;
        //            data.Add(d);
        //        }
        //    }
        //    JsonResult json = new JsonResult() { Data = new { Pcount = pagesize, Cpage = page, datas = data } };
        //    return Json(json, JsonRequestBehavior.AllowGet);
        //}

        //public ActionResult CheckYsd()
        //{
        //    var id = Request.QueryString["id"];
        //    var code = Request.Params["num"];
        //    if (string.IsNullOrWhiteSpace(id) || string.IsNullOrWhiteSpace(code))
        //    {
        //        return Content("0");
        //    }
        //    else
        //    {
        //        try
        //        {
        //            var model = 下载管理.查找下载(long.Parse(id));
        //            if (model.下载验证码 == code && model.内容主体.附件.Count > 0)
        //            {
        //                return Content(model.内容主体.附件[0]);
        //            }
        //            else
        //            {
        //                return Content("0");
        //            }
        //        }
        //        catch
        //        {
        //            return Content("0");
        //        }
        //    }
        //}
    }
}