using Go81WebApp.Models.管理器;
using Go81WebApp.Models.管理器.下载管理;
using Go81WebApp.Models.数据模型.内容数据模型;
using MongoDB.Driver.Builders;
using System;
using System.Collections;
using System.Configuration;
using System.Web.Mvc;
using System.Collections.Generic;

namespace Go81WebApp.Controllers.门户
{
    public class 下载Controller : Controller
    {
        private static int DOWNLOAD_PAGESIZE = int.Parse(ConfigurationManager.AppSettings["下载每页显示条数"]);
        //
        // GET: /下载/
        public ActionResult Download_List()
        {
            return View();
        }
        public void AddDownCount()
        {
            string id = Request.Params["id"];
            try
            {
                下载管理.增加下载次数(long.Parse(id));
            }
            catch
            {

            }
        }

        public ActionResult Part_Download_List()
        {
            ViewData["普通文件"] = 下载管理.查询下载(0, 20, Query<下载>.Where(m => m.下载类型 == 下载类型.普通), includeDisabled: false);
            return PartialView("Part_Download/Part_Download_List");
        }

        public ActionResult Download_Detail()
        {
            return View();
        }
        public ActionResult Part_Download_Detail(int? id)
        {
            下载 g = null;
            ViewData["登录"] = "0";
            try
            {
                下载管理.增加点击次数((long)id);
                g = 下载管理.查找下载((long)id);
                g.下载验证码 = "";
                if (HttpContext.检查登录() != -1)
                {
                    ViewData["登录"] = "1";
                }
            }
            catch
            {

            }

            return PartialView("Part_Download/Part_Download_Detail", g);
        }

        public ActionResult Part_Download_OtherList()
        {
            ViewData["下载"] = 下载管理.查询下载(0, 10, null, false, SortBy<下载>.Descending(o => o.下载次数), false);
            return PartialView("Part_Download/Part_Download_OtherList");
        }
        public class DownLoads
        {
            public int Clicks { get; set; }
            public long Id { get; set; }
            public string Title { get; set; }
        }
        public JsonResult GetValue()
        {
            List<DownLoads> data = new List<DownLoads>();
            int page = int.Parse(Request.QueryString["p"]);
            if (page == 0)
            {
                page = 1;
            }
            int type = int.Parse(Request.QueryString["type"]);
            long count = 下载管理.计数下载(0, 0, Query<下载>.Where(m => (int)m.下载类型 == type),includeDisabled:false);
            long pagesize = count / 20;
            if (count % 20 > 0)
            {
                pagesize++;
            }
            IEnumerable<下载> files = 下载管理.查询下载(20 * (page - 1),20, Query<下载>.Where(m => (int)m.下载类型 == type),includeDisabled:false);
            if (files != null)
            {
                foreach (var item in files)
                {
                    DownLoads d = new DownLoads();
                    d.Id = item.Id;
                    d.Title = item.内容主体.标题;
                    d.Clicks = item.下载次数;
                    data.Add(d);
                }
            }
            JsonResult json = new JsonResult() { Data = new { Pcount = pagesize, Cpage = page, datas = data } };
            return Json(json, JsonRequestBehavior.AllowGet);
        }

        public ActionResult CheckYsd()
        {
            var id = Request.QueryString["id"];
            var code = Request.Params["num"];
            if (string.IsNullOrWhiteSpace(id) || string.IsNullOrWhiteSpace(code))
            {
                return Content("0");
            }
            else
            {
                try
                {
                    var model = 下载管理.查找下载(long.Parse(id));
                    if (model.下载验证码 == code && model.内容主体.附件.Count > 0)
                    {
                        return Content(model.内容主体.附件[0]);
                    }
                    else
                    {
                        return Content("0");
                    }
                }
                catch
                {
                    return Content("0");
                }
            }
        }
    }
}