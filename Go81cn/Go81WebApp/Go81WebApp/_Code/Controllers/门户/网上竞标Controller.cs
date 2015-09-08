using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Go81WebApp.Models.管理器;
using Go81WebApp.Models.数据模型.内容数据模型;
using MongoDB.Driver.Builders;
using Go81WebApp.Models.数据模型;
using MongoDB;
using MongoDB.Driver;
using Go81WebApp.Models.数据模型.商品数据模型;
using Go81WebApp.Models.数据模型.竞标数据模型;
using Go81WebApp.Models.数据模型.用户数据模型;
using Newtonsoft.Json;
namespace Go81WebApp.Controllers.门户
{
    //[登录验证]
    public class 网上竞标Controller : Controller
    {
        private 供应商 currentUser
        {
            get
            {
                return HttpContext.获取当前用户<供应商>();
            }
        }
        // GET: 网上竞标
        public ActionResult Index(string page)
        {
            if(string.IsNullOrEmpty(page))
            {
                page = "0";
            }
            var q = Query.And(
                    Query.EQ("公告信息.公告类别", 公告.公告类别.网上竞标),
                    Query.EQ("审核数据.审核状态", 审核状态.审核通过)
                );
            var q1 = Query.And(Query.EQ("通知信息.通知所属", 通知.通知所属.网上竞标));
            ViewData["通知"] = 通知管理.查询通知(0, 10,q1, false, SortBy.Descending("内容主体.发布时间"), false);
            ViewData["网上竞标标公告"] = 公告管理.查询公告(0, 8, q.And(Query.Or(new[] { Query.EQ("公告信息.公告性质", 公告.公告性质.预公告), Query.EQ("公告信息.公告性质", 公告.公告性质.采购公告), Query.EQ("公告信息.公告性质", 公告.公告性质.技术公告), Query.EQ("公告信息.公告性质", 公告.公告性质.废标公告), Query.EQ("公告信息.公告性质", 公告.公告性质.流标公告), Query.EQ("公告信息.公告性质", 公告.公告性质.采购公告) })), false, SortBy.Descending("内容主体.发布时间"), false);
            ViewData["预公告"] = 公告管理.查询公告(0, 8, q.And(Query.EQ("公告信息.公告性质", 公告.公告性质.预公告)), false, SortBy.Descending("内容主体.发布时间"), false);
            ViewData["技术公告"] = 公告管理.查询公告(0, 8, q.And(Query.EQ("公告信息.公告性质", 公告.公告性质.技术公告)), false, SortBy.Descending("内容主体.发布时间"), false);
            ViewData["发标公告"] = 公告管理.查询公告(0, 8, q.And(Query.EQ("公告信息.公告性质", 公告.公告性质.采购公告)), false, SortBy.Descending("内容主体.发布时间"), false);
            ViewData["中标公告"] = 公告管理.查询公告(0, 8, q.And(Query.EQ("公告信息.公告性质", 公告.公告性质.采购公告)), false, SortBy.Descending("内容主体.发布时间"), false);
            ViewData["废标公告"] = 公告管理.查询公告(0, 8, q.And(Query.EQ("公告信息.公告性质", 公告.公告性质.废标公告)), false, SortBy.Descending("内容主体.发布时间"), false);
            ViewData["流标公告"] = 公告管理.查询公告(0, 8, q.And(Query.EQ("公告信息.公告性质", 公告.公告性质.流标公告)), false, SortBy.Descending("内容主体.发布时间"), false);
            ViewData["网上竞标"] = 网上竞标管理.查询网上竞标(int.Parse(page), (int.Parse(page)) * 10);
            return View();
        }
        public ActionResult Part_Bidetial()
        {
            string id = Request.Params["a"];
            网上竞标 d = 网上竞标管理.查找网上竞标(long.Parse(id));
            ViewData["当前用户"] = currentUser.Id;
            return PartialView("Part_BidInter/Part_Bidetial",d);
        }
        [登录验证]
        public ActionResult BidDetial()
        {
            try
            {
                string id = Request.Params["a"];
                
                网上竞标 g = 网上竞标管理.查找网上竞标(long.Parse(id));
                if(g==null)
                {
                    return Redirect("/网上竞标/");
                }
                return View(g);
            }
            catch
            {
                return Redirect("/网上竞标/");
            }
        }
        public ActionResult Part_BidList(string page)
        {
            if (string.IsNullOrEmpty(page))
            {
                page = "0";
            }
            var q = Query.And(
                    Query.EQ("公告信息.公告类别", 公告.公告类别.网上竞标),
                    Query.EQ("审核数据.审核状态", 审核状态.审核通过)
                );
            ViewData["currentpage"] = page;
            ViewData["pagecount"] = 1;
            ViewData["竞价公告"] = 公告管理.查询公告(0, 8, q.And(Query.Or(new[] { Query.EQ("公告信息.公告性质", 公告.公告性质.预公告), Query.EQ("公告信息.公告性质", 公告.公告性质.采购公告), Query.EQ("公告信息.公告性质", 公告.公告性质.技术公告), Query.EQ("公告信息.公告性质", 公告.公告性质.废标公告), Query.EQ("公告信息.公告性质", 公告.公告性质.流标公告), Query.EQ("公告信息.公告性质", 公告.公告性质.采购公告) })), false, SortBy.Descending("内容主体.发布时间"), false);
            return PartialView("Part_BidInter/Part_BidList");
        }
        public ActionResult Bidlist()       
        {
            return View();
        }

        //public string GetAuctionNum()
        //{
        //    string id=Request.Params["id"];
        //    网上竞标 w = 网上竞标管理.查找网上竞标(long.Parse(id));
        //    var t = 0;
        //    var st = DateTime.Now;  //当前轮报价开始时间
        //    var e = DateTime.Now;   //当前轮报价结束时间
        //    var ntime = DateTime.Now; //下一轮报价开始时间
        //    var utime = DateTime.Now;//上一轮报价结束时间
        //    for (int i = 1; i <= w.计划报价轮次; i++)
        //    {
        //        st = w.报价开始时间.AddMinutes((i - 1) * (w.每轮报价间隔时间 + w.每轮报价投标时间));
        //        e = st.AddMinutes(w.每轮报价投标时间);
        //        ntime = w.报价开始时间.AddMinutes( i* (w.每轮报价间隔时间 + w.每轮报价投标时间));
        //        utime = st.AddMinutes(-w.每轮报价间隔时间);
        //        if (DateTime.Compare(st, DateTime.Now) < 0 && DateTime.Compare(e, DateTime.Now) > 0)
        //        {
        //            t = i;
        //            break;
        //        }   
        //        if (DateTime.Compare(DateTime.Now, utime) > 0 && DateTime.Compare(DateTime.Now, st) < 0)
        //        {
        //            //st = w.报价开始时间.AddMinutes((i - 1) * (w.每轮报价间隔时间 + w.每轮报价投标时间));
        //            //e = st.AddMinutes(w.每轮报价投标时间);
        //            //ntime = w.报价开始时间.AddMinutes((i + 1) * (w.每轮报价间隔时间 + w.每轮报价投标时间));
        //            //utime = st.AddMinutes(-w.每轮报价间隔时间);
        //            t = i;
        //            break;
        //        }
        //    }
        //    var str = t + "|" + st.ToString("yyyy-MM-dd HH:mm:ss") + "|" + e.ToString("yyyy-MM-dd HH:mm:ss") + "|" + ntime.ToString("yyyy-MM-dd HH:mm:ss")+"|"+utime.ToString("yyyy-MM-dd HH:mm:ss");//轮次|当前轮报价开始时间|当前轮报价结束时间|下一轮报价开始时间|上一轮报价结束时间
        //    return str;
        //}

        //竞标者每轮报价操作
        //public string QuoteBid()
        //{
        //    var round = Request.Params["round"];//报价轮次
        //    var id = Request.Params["id"];//竞标id
        //    var price = Request.Params["price"];//报价
        //    var _jb = 网上竞标管理.查找网上竞标(long.Parse(id));
            
        //    var _ak = _jb.报价轮次[int.Parse(round)].报价记录;
        //    if (_ak.Exists(o => o.报价供应商链接.用户ID == currentUser.Id))
        //    {
        //        var _scbj = 0M;
        //        if (round != "0")
        //        {
        //            var _isexist = _jb.报价轮次[int.Parse(round) - 1].报价记录.Where(o => o.报价供应商链接.用户ID == currentUser.Id);//当前报价者上一轮报价
        //            if (_isexist.Count() > 0) { _scbj = _isexist.First().报价; }
        //        }
        //        var _pk = _ak.Find(o => o.报价供应商链接.用户ID == currentUser.Id);
        //        if (_pk.报价 < decimal.Parse(price) || (_scbj !=0  && _scbj < decimal.Parse(price)) || _jb.起始价格<decimal.Parse(price)) //若目前报价高于本轮上次报价 或上一轮报价则提示
        //        {
        //            return "您的当前报价高于上次报价或起始价格，请重新输入";
        //        }
        //        _pk.报价 = decimal.Parse(price);
        //        _pk.报价时间 = DateTime.Now;
        //    }
        //    else
        //    {
        //        var _bjmx = new 网上竞标._报价记录();
        //        var _scbj = 0M;
        //        if (round != "0")
        //        {
        //            var _isexist = _jb.报价轮次[int.Parse(round) - 1].报价记录.Where(o => o.报价供应商链接.用户ID == currentUser.Id);//当前报价者上一轮报价
        //            if (_isexist.Count() > 0) { _scbj = _isexist.First().报价; }
        //        }
        //        if (_jb.起始价格 < decimal.Parse(price) || (_scbj!=0 && _scbj<decimal.Parse(price)))
        //        {
        //            return "您的当前报价高于上次报价或起始价格，请重新输入";
        //        }
        //        _bjmx.报价 = decimal.Parse(price);
        //        _bjmx.报价时间 = DateTime.Now;
        //        _bjmx.报价供应商链接.用户ID = currentUser.Id;
        //        _ak.Add(_bjmx);
        //    }
        //    网上竞标管理.更新网上竞标(_jb);
        //    return "报价成功！";
        //}

        //每轮报价结束后统计最优报价及供应商并写入本轮最终价及下一轮开始价
        //public void CurrentBidStat()
        //{
        //    var id = Request.Params["id"];//竞标ID
        //    var round=int.Parse(Request.Params["round"]);//报价轮次
        //    var jb = 网上竞标管理.查找网上竞标(long.Parse(id));
        //    //本轮竞报价记录中竞标价最低的记录
        //    var zy = jb.报价轮次[round - 1].报价记录.OrderByDescending(o => o.报价).Last();
        //    //本轮终止价为报价记录中最低价格
        //    jb.报价轮次[round - 1].本轮终止价 = zy.报价;
        //    //本轮最优供应商为报价记录钟报价最低的供应商
        //    jb.报价轮次[round - 1].本轮最优价供应商.用户ID = zy.报价供应商链接.用户ID;
        //    //判断当前轮次是否是最后一轮，不是才写入下一轮的起始价
        //    if (jb.报价轮次.Count != round)
        //    {
        //        jb.报价轮次[round].本轮起始价 = jb.报价轮次[round - 1].本轮终止价;
        //    }
        //    网上竞标管理.更新网上竞标(jb);
        //}

        //public class RealBj
        //{
        //    public decimal Price { get; set; }
        //    public string GysName { get; set; }
        //    public string BjTime { get; set; }
        //}
        ////获取实时报价
        //public string GetRealBj()
        //{
        //    var id = Request.Params["id"];//竞标id
        //    var round=Request.Params["round"];//报价轮次
        //    var _wsjb = 网上竞标管理.查找网上竞标(long.Parse(id));
        //    var _bj = _wsjb.报价轮次[int.Parse(round)];
        //    if (_bj.报价记录.Any() && _bj.报价记录.Count() > 0)
        //    {
        //        var _zxbj = _bj.报价记录.OrderByDescending(o => o.报价时间).First();//获取当前轮次最新报价
        //        var bj = new RealBj()
        //        {
        //            BjTime = _zxbj.报价时间.ToString("yyyy-MM-dd HH:mm:ss"),
        //            GysName = _zxbj.报价供应商链接.用户数据.企业基本信息.企业名称,
        //            Price = _zxbj.报价,
        //        };
        //        return JsonConvert.SerializeObject(bj);
        //    }
        //    else
        //    {
        //        return "";
        //    }
        //}

    }
}