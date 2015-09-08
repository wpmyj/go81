using Go81WebApp.Models.管理器;
using Go81WebApp.Models.数据模型;
using Go81WebApp.Models.数据模型.用户数据模型;
using MongoDB;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Go81WebApp.Controllers.门户
{
    public class 机票酒店Controller : Controller
    {
        // GET: 机票酒店
        public ActionResult HotelandTicket()
        {
            ViewData["商圈列表"] = 地理位置数据.商圈;
            IEnumerable<酒店> hotel = 酒店管理.查询酒店(0, 5, Query.EQ("审核数据.审核状态", 审核状态.审核通过));
            ViewData["机票代售点"] = 机票代售点管理.查询机票代售点(0, 5, Query.EQ("审核数据.审核状态", 审核状态.审核通过));
            return View(hotel);
        }
        public ActionResult HotelFilter()
        {
            string hname = Request.Params["a"];//根据关键字查询
            string location = Request.Params["c"];//地理位置
            string range = Request.Params["range"];//范围
            //string num = Request.Params["num"];//数量

            IEnumerable<酒店> enhotel = (hname != null && hname != "" && hname != "undefined")
                ? 酒店管理.查询酒店(0, 10, Query.EQ("酒店基本信息.酒店名", hname))
                : 酒店管理.查询酒店(0, 10, Query.WithinCircle("酒店基本信息.地理位置", double.Parse(location.Split(',')[0]), double.Parse(location.Split(',')[1]), double.Parse(range)));
            if (enhotel != null)
            {
                return View(enhotel);
            }
            else
            {
                return View();
            }
        }
        
        public ActionResult HotelDetial()
        {
            string id=Request.Params["id"];
            酒店 h;
            if(id!=null)
            {
                h = 酒店管理.查找酒店(long.Parse(id));
            }
            else
            {
                return RedirectToAction("HotelFilter");
            }

            return View(h);
        }
        public ActionResult TicketFilter()
        {
            string tname = Request.Params["a"];//根据关键字查询
            string location = Request.Params["c"];//地理位置
            string range = Request.Params["range"];//范围
            string num = Request.Params["num"];//数量

            IEnumerable<机票代售点> enticket = (tname != null && tname != "" && tname != "undefined")
                ? 机票代售点管理.查询机票代售点(0, 10, Query.EQ("代售点名称", tname))
                : 机票代售点管理.查询机票代售点(0, 10);
            return View(enticket);
        }
        public ActionResult TicketDetial()
        {
            string id = Request.Params["id"];
            机票代售点 t;
            
            if (id != null)
            {
                t = 机票代售点管理.查找机票代售点(long.Parse(id));
            }
            else
            {
                return RedirectToAction("TicketFilter");
            }

            return View(t);
        }
        public JsonResult GetBusinessByCity()
        {
            string provence = Request.Params["p"];
            string city=Request.Params["c"];
            var sq = 地理位置数据.商圈;
            Dictionary<string, List<string>> k = new Dictionary<string, List<string>>();
            foreach (var a in sq.Keys)
            {
                if (a.Contains(provence))
                {
                    foreach (var t in sq[a].Keys)
                    {
                        if (t.Contains(city))
                        {
                            foreach (var p in sq[a][t].Keys)
                            {
                                k.Add(p, new List<string>());
                                foreach (var y in sq[a][t][p])
                                {
                                    k[p].Add(y);
                                }
                            }
                        }
                    }
                }
            }
            JsonResult json = new JsonResult() { Data = k };
            return Json(json,JsonRequestBehavior.AllowGet);
        }
    }
}