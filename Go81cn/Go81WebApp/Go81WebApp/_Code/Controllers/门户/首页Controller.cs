using System.Net;
using System.Text;
using Com.Alipay;
using Go81WebApp.Controllers.基本功能;
using Go81WebApp.Models.数据模型.项目数据模型;
using Go81WebApp.Models.管理器;
using Go81WebApp.Models.数据模型;
using Go81WebApp.Models.数据模型.内容数据模型;
using Go81WebApp.Models.数据模型.商品数据模型;
using Go81WebApp.Models.数据模型.用户数据模型;
using MongoDB;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Drawing;
using System.Drawing.Imaging;
using Helpers.印章;
using MongoDB.Bson;
using Go81WebApp.Models.数据模型.推广业务数据模型;
using Go81WebApp.Models.管理器.推广业务管理;
using System;
using System.Text.RegularExpressions;
using System.IO;
using Gma.QrCodeNet.Encoding;
using Gma.QrCodeNet.Encoding.Windows.Render;
using Go81WebApp.Models.数据模型.消息数据模型;
using Go81WebApp.Models.数据模型.统计数据模型;
using Go81WebApp.Models.管理器.统计管理;
using Newtonsoft.Json;
using NPOI.SS.Formula.Functions;
using Go81WebApp.Models.管理器.下载管理;
using Go81WebApp.Models.数据模型.竞标数据模型;
using Go81WebApp.Models.管理器.内容管理;

namespace Go81WebApp.Controllers.门户
{
    public class 首页Controller : Controller
    {
        public string GetPage(string posturl, string postData)
        {
            Stream outstream = null;
            Stream instream = null;
            StreamReader sr = null;
            HttpWebResponse response = null;
            HttpWebRequest request = null;
            Encoding encoding = Encoding.UTF8;
            byte[] data = encoding.GetBytes(postData);
            // 准备请求...
            try
            {
                // 设置参数
                request = WebRequest.Create(posturl) as HttpWebRequest;
                CookieContainer cookieContainer = new CookieContainer();
                request.CookieContainer = cookieContainer;
                request.AllowAutoRedirect = true;
                request.Method = "POST";
                request.ContentType = "application/x-www-form-urlencoded";
                request.ContentLength = data.Length;
                outstream = request.GetRequestStream();
                outstream.Write(data, 0, data.Length);
                outstream.Close();
                //发送请求并获取相应回应数据
                response = request.GetResponse() as HttpWebResponse;
                //直到request.GetResponse()程序才开始向目标网页发送Post请求
                instream = response.GetResponseStream();
                sr = new StreamReader(instream, encoding);
                //返回结果网页（html）代码
                string content = sr.ReadToEnd();
                string err = string.Empty;
                return content;
            }
            catch (Exception ex)
            {
                string err = ex.Message;
                Response.Write(err);
                return string.Empty;
            }
        }

        public String GetPageCode(String PageURL, String Charset)
        {
            String strHtml = "";
            try
            {
                //连接到目标网页
                System.Net.HttpWebRequest wreq = (System.Net.HttpWebRequest)System.Net.WebRequest.Create(PageURL);
                wreq.Method = "GET";
                //wreq.ContentType = "application/json";
                wreq.UserAgent = "Mozilla/5.0 (Windows; U; Windows NT 6.1; zh-CN; rv:1.9.2.8) Gecko/20100722 Firefox/3.6.8";
                System.Net.HttpWebResponse wresp = (System.Net.HttpWebResponse)wreq.GetResponse();
                //采用流读取，并确定编码方式
                System.IO.Stream s = wresp.GetResponseStream();
                System.IO.StreamReader objReader = new System.IO.StreamReader(s, System.Text.Encoding.GetEncoding(Charset));
                strHtml = objReader.ReadToEnd();
                objReader.Close();
                return strHtml;
            }
            catch (System.Net.WebException ex)
            {
                System.Net.HttpWebResponse res = ex.Response as System.Net.HttpWebResponse;

                if (res.StatusCode == System.Net.HttpStatusCode.InternalServerError)
                {
                    System.IO.Stream s = res.GetResponseStream();
                    System.IO.StreamReader objReader = new System.IO.StreamReader(s, System.Text.Encoding.GetEncoding(Charset));
                    strHtml = objReader.ReadToEnd();
                    objReader.Close();
                }
                else
                {
                    strHtml = ex.Message;
                }
                return strHtml;
            }
        }
        public ActionResult Part_Index_LeftMenu()
        {


            var firstclass = 商品分类管理.查找子分类();

            return PartialView("Part_Index_LeftMenu", firstclass);
        }
        public ActionResult Part_Index_Content()
        {
            //通知部分--出去曝光台
            ViewData["通知"] = 通知管理.查询通知(0, 8, Query<通知>.Where(o => o.通知信息.通知所属 != 通知.通知所属.黑名单 && o.审核数据.审核状态 == 审核状态.审核通过), false, SortBy.Descending("内容主体.发布时间"), false);

            //排序《按照修改时间倒序排序》
            var q = Query.And(
                    Query.EQ("公告信息.公告类别", 公告.公告类别.公开招标),
                    Query.EQ("审核数据.审核状态", 审核状态.审核通过)
                );
            var picnews = 新闻管理.查询图片新闻(0, 5, Query<新闻>.EQ(o => o.审核数据.审核状态, 审核状态.审核通过));
            ViewData["图片新闻动态"] = picnews;
            //公开招标部分 
            ViewData["全部公开招标公告"] = 公告管理.查询公告(0, 8, q.And(Query.Or(new[] { Query.EQ("公告信息.公告性质", 公告.公告性质.预公告), Query.EQ("公告信息.公告性质", 公告.公告性质.采购公告), Query.EQ("公告信息.公告性质", 公告.公告性质.技术公告), Query.EQ("公告信息.公告性质", 公告.公告性质.需求公告), Query.EQ("公告信息.公告性质", 公告.公告性质.补遗公告), Query.EQ("公告信息.公告性质", 公告.公告性质.更正公告), Query.EQ("公告信息.公告性质", 公告.公告性质.变更公告), Query.EQ("公告信息.公告性质", 公告.公告性质.答疑公告) })), false, SortBy.Descending("内容主体.发布时间"), false);

            ViewData["预公告"] = 公告管理.查询公告(0, 8, q.And(Query.EQ("公告信息.公告性质", 公告.公告性质.预公告)), false, SortBy.Descending("内容主体.发布时间"), false);
            ViewData["技术公告"] = 公告管理.查询公告(0, 8, q.And(Query.EQ("公告信息.公告性质", 公告.公告性质.技术公告)), false, SortBy.Descending("内容主体.发布时间"), false);
            ViewData["发标公告"] = 公告管理.查询公告(0, 8, q.And(Query.EQ("公告信息.公告性质", 公告.公告性质.采购公告)), false, SortBy.Descending("内容主体.发布时间"), false);

            ViewData["中标公告"] = 公告管理.查询公告(0, 8, Query<公告>.Where(m => m.审核数据.审核状态 == 审核状态.审核通过 && m.公告信息.公告性质 == 公告.公告性质.中标结果公示), false, SortBy.Descending("内容主体.发布时间"), false);

            //ViewData["中标公告"] = 公告管理.查询公告(0, 8, Query.EQ("审核数据.审核状态", 审核状态.审核通过).And(Query.EQ("公告信息.公告性质", 公告.公告性质.中标结果公示)), false, SortBy.Descending("内容主体.发布时间"), false);
            //ViewData["废标公告"] = 公告管理.查询公告(0, 8, q.And(Query.EQ("公告信息.公告性质", 公告.公告性质.废标公告)), false, SortBy.Descending("内容主体.发布时间"), false);
            ViewData["废标公告"] = 公告管理.查询公告(0, 8, Query<公告>.Where(m => m.公告信息.公告性质 == 公告.公告性质.废标公告 && m.审核数据.审核状态 == 审核状态.审核通过), false, SortBy.Descending("内容主体.发布时间"), false);
            ViewData["流标公告"] = 公告管理.查询公告(0, 8, Query<公告>.Where(m => m.公告信息.公告性质 == 公告.公告性质.流标公告 && m.审核数据.审核状态 == 审核状态.审核通过), false, SortBy.Descending("内容主体.发布时间"), false);
            //ViewData["流标公告"] = 公告管理.查询公告(0, 8, q.And(Query.EQ("公告信息.公告性质", 公告.公告性质.流标公告)), false, SortBy.Descending("内容主体.发布时间"), false);

            //ViewData["全部结果公示"] = 公告管理.查询公告(0, 8, Query.Or(new[] { Query.EQ("审核数据.审核状态", 审核状态.审核通过).And(Query.EQ("公告信息.公告性质", 公告.公告性质.中标结果公示)), q.And(Query.EQ("公告信息.公告性质", 公告.公告性质.废标公告)), q.And(Query.EQ("公告信息.公告性质", 公告.公告性质.流标公告)) }), false, SortBy.Descending("内容主体.发布时间"), false);
            ViewData["全部结果公示"] = 公告管理.查询公告(0, 8, Query<公告>.Where(m => m.审核数据.审核状态 == 审核状态.审核通过 && (m.公告信息.公告性质 == 公告.公告性质.废标公告 || m.公告信息.公告性质 == 公告.公告性质.流标公告 || m.公告信息.公告性质 == 公告.公告性质.中标结果公示)), false, SortBy.Descending("内容主体.发布时间"), false);
            //采购部分
            var q1 = Query.EQ("审核数据.审核状态", 审核状态.审核通过).And(Query.NE("公告信息.公告性质", 公告.公告性质.中标结果公示)).And(Query.NE("公告信息.公告性质", 公告.公告性质.废标公告)).And(Query.NE("公告信息.公告性质", 公告.公告性质.流标公告));
            ViewData["全部采购公告"] = 公告管理.查询公告(0, 8, q1.And(Query.NE("公告信息.公告类别", 公告.公告类别.公开招标)).And(Query.NE("公告信息.公告类别", 公告.公告类别.其他)).And(Query.NE("公告信息.公告类别", 公告.公告类别.单一来源)), false, SortBy.Descending("内容主体.发布时间"), false);
            ViewData["邀请招标"] = 公告管理.查询公告(0, 8, q1.And(Query.EQ("公告信息.公告类别", 公告.公告类别.邀请招标)), false, SortBy.Descending("内容主体.发布时间"), false);
            ViewData["单一来源"] = 公告管理.查询公告(0, 8, q1.And(Query.EQ("公告信息.公告类别", 公告.公告类别.单一来源)), false, SortBy.Descending("内容主体.发布时间"), false);
            ViewData["询价采购"] = 公告管理.查询公告(0, 8, q1.And(Query.EQ("公告信息.公告类别", 公告.公告类别.询价采购)), false, SortBy.Descending("内容主体.发布时间"), false);
            ViewData["竞争性谈判"] = 公告管理.查询公告(0, 8, q1.And(Query.EQ("公告信息.公告类别", 公告.公告类别.竞争性谈判)), false, SortBy.Descending("内容主体.发布时间"), false);

            //政策法规部分
            ViewData["政策法规"] = 政策法规管理.查询政策法规(0, 8, Query<政策法规>.EQ(o => o.审核数据.审核状态, 审核状态.审核通过), false, SortBy.Descending("内容主体.发布时间"), false);
            return PartialView();
        }
        public ActionResult Part_Index_Bottom()
        {
            if (-1 != HttpContext.检查登录())
            {
                ViewData["已登录"] = "1";
            }
            else
            {
                ViewData["已登录"] = "0";
            }
            var picnews = 新闻管理.查询图片新闻(0, 5, Query<新闻>.EQ(o => o.审核数据.审核状态, 审核状态.审核通过));
            ViewData["图片新闻动态"] = picnews;

            ViewData["listcount"] = picnews.Count();
            //排序《按照修改时间倒序排序》
            //新闻动态部分
            ViewData["新闻动态"] = 新闻管理.查询新闻(0, 9, Query<新闻>.EQ(o => o.审核数据.审核状态, 审核状态.审核通过), false, SortBy.Descending("内容主体.发布时间"), false);
            //政策法规部分
            ViewData["政策法规"] = 政策法规管理.查询政策法规(0, 9, Query<政策法规>.EQ(o => o.审核数据.审核状态, 审核状态.审核通过), false, SortBy.Descending("内容主体.发布时间"), false);

            //采购部分
            var q = Query.EQ("审核数据.审核状态", 审核状态.审核通过).And(Query.NE("公告信息.公告性质", 公告.公告性质.中标结果公示));
            ViewData["全部采购公告"] = 公告管理.查询公告(0, 8, q.And(Query.NE("公告信息.公告类别", 公告.公告类别.公开招标)).And(Query.NE("公告信息.公告类别", 公告.公告类别.其他)), false, SortBy.Descending("内容主体.发布时间"), false);
            ViewData["邀请招标"] = 公告管理.查询公告(0, 8, q.And(Query.EQ("公告信息.公告类别", 公告.公告类别.邀请招标)), false, SortBy.Descending("内容主体.发布时间"), false);
            ViewData["协议采购"] = 公告管理.查询公告(0, 8, q.And(Query.EQ("公告信息.公告类别", 公告.公告类别.协议采购)), false, SortBy.Descending("内容主体.发布时间"), false);
            ViewData["单一来源"] = 公告管理.查询公告(0, 8, q.And(Query.EQ("公告信息.公告类别", 公告.公告类别.单一来源)), false, SortBy.Descending("内容主体.发布时间"), false);
            ViewData["询价采购"] = 公告管理.查询公告(0, 8, q.And(Query.EQ("公告信息.公告类别", 公告.公告类别.询价采购)), false, SortBy.Descending("内容主体.发布时间"), false);
            ViewData["竞争性谈判"] = 公告管理.查询公告(0, 8, q.And(Query.EQ("公告信息.公告类别", 公告.公告类别.竞争性谈判)), false, SortBy.Descending("内容主体.发布时间"), false);

            ViewData["行业动态"] = 公告管理.查询公告(0, 8, q.And(Query.EQ("公告信息.公告类别", 公告.公告类别.其他)), false, SortBy.Descending("内容主体.发布时间"), false);

            var p_l = 用户管理.列表用户<供应商>(0, 0,
                Fields<供应商>.Include(o => o.Id),
                Query<供应商>.EQ(o => o.审核数据.审核状态, 审核状态.审核通过).And(Query<供应商>.Where(o => !o.供应商用户信息.协议供应商 && !o.供应商用户信息.应急供应商))
                ).Select(o => o["_id"]);
            var p_q = Query.And(
                    Query.In("商品信息.所属供应商.用户ID", p_l),
                    q
                );

            //通知部分--出去曝光台
            ViewData["曝光台"] = 通知管理.查询通知(0, 10, Query<通知>.Where(o => o.通知信息.通知所属 == 通知.通知所属.黑名单 && o.审核数据.审核状态 == 审核状态.审核通过), false, SortBy.Descending("内容主体.发布时间"), false);

            var sp2 = new List<商品>();
            List<long> gids = new List<long>();
            List<long> ids = new List<long>();
            var now = DateTime.Now;

            var 供应商服务记录 = 供应商服务记录管理.查询供应商服务记录(0, 0, null, false, SortBy<供应商服务记录>.Ascending(o => o.基本数据.修改时间));
            var A2广告位置服务记录 = 供应商服务记录.Where(o => o.已开通的服务.Exists(p => p.所申请项目名 == "企业推广服务A2位置" && p.签订时间 < now && p.结束时间 > now));
            if (A2广告位置服务记录.Count() > 0)
            {
                foreach (var u in A2广告位置服务记录)
                {
                    var A2位置 = u.已开通的服务.Find(o => o.所申请项目名 == "企业推广服务A2位置" && o.签订时间 < now && o.结束时间 > now);
                    if (A2位置 != null)
                    {
                        var 供应商 = 用户管理.查找用户<供应商>(u.所属供应商.用户ID);
                        var 广告商品 = 供应商.供应商用户信息.广告商品.Where(o => o.Key.Contains("企业推广服务A2位置"));
                        if (广告商品 != null && 广告商品.Count() > 0)
                        {
                            foreach (var t in 广告商品.First().Value)
                            {
                                sp2.Add(t.商品.商品);
                                gids.Add(t.商品.商品ID);
                                ids.Add(供应商.Id);
                            }
                        }
                        else
                        {
                            IEnumerable<商品> gd = 商品管理.查询商品(0, 1, Query<商品>.Where(o => o.商品信息.所属供应商.用户ID == u.所属供应商.用户ID && o.审核数据.审核状态 == 审核状态.审核通过), false, SortBy.Descending("销售信息.点击量"), false);
                            if (gd != null && gd.Count() != 0)
                            {
                                sp2.Add(gd.First());
                                gids.Add(gd.First().Id);
                                ids.Add(gd.First().商品信息.所属供应商.用户ID);
                            }
                        }
                    }
                }
            }
            var count = sp2.Count();
            ViewData["A2类别"] = "1";
            if (count < 10) //如果推广商品小于10个，则按点击量排序选择一定量的占位商品
            {
                var 补充商品 = 商品管理.查询商品(0, 0, p_q.And(Query<商品>.EQ(o => o.审核数据.审核状态, 审核状态.审核通过)).And(Query<商品>.NotIn(o => o.Id, gids)), false, SortBy.Descending("销售信息.点击量"), false);
                foreach (var sp1 in 补充商品)
                {
                    if (ids.Contains(sp1.商品信息.所属供应商.用户ID)) continue;
                    sp2.Add(sp1);
                    gids.Add(sp1.Id);
                    ids.Add(sp1.商品信息.所属供应商.用户ID);
                    ++count;
                    if (count == 10) break;
                }
            }
            else if (count > 10 && count <= 20) //如果推广商品>10 <20，则按点击量排序选择一定量的占位商品
            {
                ViewData["A2类别"] = "2";
                if (count < 20)
                {
                    var 补充商品 = 商品管理.查询商品(0, 0, p_q.And(Query<商品>.EQ(o => o.审核数据.审核状态, 审核状态.审核通过)).And(Query<商品>.NotIn(o => o.Id, gids)), false, SortBy.Descending("销售信息.点击量"), false);
                    foreach (var sp1 in 补充商品)
                    {
                        if (ids.Contains(sp1.商品信息.所属供应商.用户ID)) continue;
                        sp2.Add(sp1);
                        gids.Add(sp1.Id);
                        ids.Add(sp1.商品信息.所属供应商.用户ID);
                        ++count;
                        if (count == 20) break;
                    }
                }
            }
            else
            {
                ViewData["A2类别"] = "3";
                if (count < 30)
                {
                    var 补充商品 = 商品管理.查询商品(0, 0, p_q.And(Query<商品>.EQ(o => o.审核数据.审核状态, 审核状态.审核通过)).And(Query<商品>.NotIn(o => o.Id, gids)), false, SortBy.Descending("销售信息.点击量"), false);
                    foreach (var sp1 in 补充商品)
                    {
                        if (ids.Contains(sp1.商品信息.所属供应商.用户ID)) continue;
                        sp2.Add(sp1);
                        gids.Add(sp1.Id);
                        ids.Add(sp1.商品信息.所属供应商.用户ID);
                        ++count;
                        if (count == 30) break;
                    }
                }
                if (count > 30)
                {
                    sp2 = sp2.Take(30).ToList();
                }
            }
            ViewData["首页主推商品信息"] = (IEnumerable<商品>)sp2;


            /////////////////////////原网上竞价处修改
            var sp_add = new List<商品>();
            var w = 供应商服务记录管理.查询供应商服务记录(0, 0);//找出开通了A1广告位置的供应商增值服务记录
            var g = w.Where(o => o.已开通的服务.Exists(p => p.所申请项目名 == "企业推广服务A1位置" && p.签订时间 < now && p.结束时间 > now && p.是否通过 == 通过状态.通过));
            foreach (var k in g)//通过供应商服务记录的所属供应商ID找出相应供应商，并把该供应商选择的广告位置展示商品列出来
            {
                var 供应商 = 用户管理.查找用户<供应商>(k.所属供应商.用户ID);
                var 广告商品 = 供应商.供应商用户信息.广告商品.Where(o => o.Key.Contains("企业推广服务A1位置"));
                if (广告商品 != null && 广告商品.Count() > 0)
                {
                    foreach (var t in 广告商品.First().Value)
                    {
                        sp_add.Add(t.商品.商品);
                        gids.Add(t.商品.商品ID);
                        ids.Add(供应商.Id);
                    }
                }
                else
                {
                    IEnumerable<商品> gd = 商品管理.查询商品(0, 1, Query<商品>.Where(o => o.商品信息.所属供应商.用户ID == k.所属供应商.用户ID && o.审核数据.审核状态 == 审核状态.审核通过), false, SortBy.Descending("销售信息.点击量"), false);
                    if (gd != null && gd.Count() != 0)
                    {
                        sp_add.Add(gd.First());
                        gids.Add(gd.First().Id);
                        ids.Add(gd.First().商品信息.所属供应商.用户ID);
                    }
                }
            }
            var countsp = sp_add.Count();
            if (countsp < 6)
            {
                var 补充商品1 = 商品管理.查询商品(0, 0, p_q.And(Query<商品>.EQ(o => o.审核数据.审核状态, 审核状态.审核通过)).And(Query<商品>.NotIn(o => o.Id, gids)), false, SortBy.Descending("销售信息.点击量"), false);
                foreach (var sp1 in 补充商品1)
                {
                    if (ids.Contains(sp1.商品信息.所属供应商.用户ID)) continue;
                    sp_add.Add(sp1);
                    ids.Add(sp1.商品信息.所属供应商.用户ID);
                    ++countsp;
                    if (countsp == 6) break;
                }

            }
            ViewData["新增推荐商品"] = (IEnumerable<商品>)sp_add;
            /////////////////////////原网上竞价处修改
            List<供应商> supplierList = new List<供应商>();
            ids = new List<long>();
            IEnumerable<供应商增值服务申请记录> supplier = 供应商增值服务申请记录管理.查询供应商增值服务申请记录(0, 0, Query<供应商增值服务申请记录>.Where(o => o.所申请项目名 == "企业推广服务A3位置" && o.是否通过 == 通过状态.通过 && o.签订时间 < now && o.结束时间 > now), false, SortBy<供应商增值服务申请记录>.Ascending(o => o.基本数据.修改时间));
            if (supplier != null && supplier.Count() != 0)
            {
                foreach (var item in supplier)
                {
                    supplierList.Add(item.所属供应商.用户数据);
                    ids.Add(item.所属供应商.用户ID);
                }
            }
            ViewData["A3类别"] = "1";
            var suppliercount = supplierList.Count;
            if (suppliercount < 10)
            {
                IEnumerable<供应商> gys = 用户管理.查询用户<供应商>(0, 10 - suppliercount, Query<供应商>.Where(
                o => o.审核数据.审核状态 == 审核状态.审核通过
                && o.供应商用户信息.供应商图片.Count > 0
                && o.所属地域.省份 != null
                && o.企业基本信息.所属行业 != null
                    && !o.供应商用户信息.协议供应商
                    && o.Id != 200000000462
                    && !o.供应商用户信息.应急供应商).And(Query<供应商>.NotIn(o => o.Id, ids)), includeDisabled: false);
                if (gys != null && gys.Count() != 0)
                {
                    foreach (var item in gys)
                    {
                        supplierList.Add(item);
                    }
                }
            }
            else if (suppliercount > 10 && suppliercount <= 20)
            {
                ViewData["A3类别"] = "2";
                if (suppliercount < 20)
                {
                    IEnumerable<供应商> gys = 用户管理.查询用户<供应商>(0, 20 - suppliercount, Query<供应商>.Where(
               o => o.审核数据.审核状态 == 审核状态.审核通过
               && o.供应商用户信息.供应商图片.Count > 0
               && o.所属地域.省份 != null
               && o.企业基本信息.所属行业 != null
                   && !o.供应商用户信息.协议供应商
                   && o.Id != 200000000462
                   && !o.供应商用户信息.应急供应商).And(Query<供应商>.NotIn(o => o.Id, ids)), includeDisabled: false);
                    if (gys != null && gys.Count() != 0)
                    {
                        foreach (var item in gys)
                        {
                            supplierList.Add(item);
                        }
                    }
                }
            }
            else
            {
                ViewData["A3类别"] = "3";
                if (suppliercount < 30)
                {
                    IEnumerable<供应商> gys = 用户管理.查询用户<供应商>(0, 30 - suppliercount, Query<供应商>.Where(
               o => o.审核数据.审核状态 == 审核状态.审核通过
               && o.供应商用户信息.供应商图片.Count > 0
               && o.所属地域.省份 != null
               && o.企业基本信息.所属行业 != null
                   && !o.供应商用户信息.协议供应商
                   && o.Id != 200000000462
                   && !o.供应商用户信息.应急供应商).And(Query<供应商>.NotIn(o => o.Id, ids)), includeDisabled: false);
                    if (gys != null && gys.Count() != 0)
                    {
                        foreach (var item in gys)
                        {
                            supplierList.Add(item);
                        }
                    }
                }
                if (suppliercount > 30)
                {
                    supplierList = supplierList.Take(30).ToList();
                }
            }

            ViewData["供应商信息"] = supplierList;
            //ViewData["供应商信息"] = 用户管理.查询用户<供应商>(0, 10, Query<供应商>.GTE(o => o.供应商用户信息.认证级别, 供应商.认证级别.已审核用户).And(Query<供应商>.GT(o => o.供应商用户信息.供应商图片.Count, 0)).And(Query<供应商>.NE(o => o.所属地域.地域, null)).And(Query<供应商>.NE(o => o.企业基本信息.所属行业, null)), includeDisabled: false);
            //ViewData["供应商信息"] = 用户管理.查询用户<供应商>(0, 10, Query<供应商>.Where(
            //    o => o.审核数据.审核状态 == 审核状态.审核通过
            //    && o.供应商用户信息.供应商图片.Count > 0
            //    && o.所属地域.省份 != null
            //    && o.企业基本信息.所属行业 != null
            //    , false, SortBy.Descending("基本数据.添加时间"), includeDisabled: false);
            ////IEnumerable<供应商推广> model= 供应商推广管理.管理器.查询(0, 0, Query<供应商推广>.Where(m => m.广告位置 == 供应商推广._广告位置.新入库&&m.结束时间>DateTime.Now));
            //List<供应商> gys = new List<供应商>();
            //if(model!=null&&model.Count()!=0)
            //{
            //    foreach(var item in model)
            //    {
            //        gys.Add(item.供应商.用户数据);
            //    }
            //}
            //ViewData["新入库供应商信息"] = (IEnumerable<供应商>)gys;
            //ViewData["新入库供应商信息"] = 用户管理.查询用户<供应商>(0, 10, Query<供应商>.Where(
            //    o => o.审核数据.审核状态 == 审核状态.审核通过
            //    && o.供应商用户信息.供应商图片.Count > 0
            //    && o.所属地域.省份 != null
            //    && o.企业基本信息.所属行业 != null
            //    && (/*o.供应商用户信息.已入库 ||*/ o.供应商用户信息.协议供应商 || o.供应商用户信息.应急供应商))
            //.And(Query<供应商>.NotIn(o => o.Id, new List<long> { 200000000594, 200000000596, 200000000598, 200000000600 }))
            //    , false, SortBy.Descending("基本数据.添加时间"), includeDisabled: false);

            return PartialView();
        }
        public ActionResult Part_Index_Bottom1()
        {
            if (-1 != HttpContext.检查登录())
            {
                ViewData["已登录"] = "1";
            }
            else
            {
                ViewData["已登录"] = "0";
            }
            var picnews = 新闻管理.查询图片新闻(0, 5, Query<新闻>.EQ(o => o.审核数据.审核状态, 审核状态.审核通过));
            ViewData["图片新闻动态"] = picnews;

            ViewData["listcount"] = picnews.Count();
            //排序《按照修改时间倒序排序》
            //新闻动态部分
            ViewData["新闻动态"] = 新闻管理.查询新闻(0, 9, Query<新闻>.EQ(o => o.审核数据.审核状态, 审核状态.审核通过), false, SortBy.Descending("内容主体.发布时间"), false);
            //政策法规部分
            ViewData["政策法规"] = 政策法规管理.查询政策法规(0, 9, Query<政策法规>.EQ(o => o.审核数据.审核状态, 审核状态.审核通过), false, SortBy.Descending("内容主体.发布时间"), false);

            //采购部分
            var q = Query.EQ("审核数据.审核状态", 审核状态.审核通过).And(Query.NE("公告信息.公告性质", 公告.公告性质.中标结果公示));
            ViewData["全部采购公告"] = 公告管理.查询公告(0, 8, q.And(Query.NE("公告信息.公告类别", 公告.公告类别.公开招标)).And(Query.NE("公告信息.公告类别", 公告.公告类别.其他)), false, SortBy.Descending("内容主体.发布时间"), false);
            ViewData["邀请招标"] = 公告管理.查询公告(0, 8, q.And(Query.EQ("公告信息.公告类别", 公告.公告类别.邀请招标)), false, SortBy.Descending("内容主体.发布时间"), false);
            ViewData["协议采购"] = 公告管理.查询公告(0, 8, q.And(Query.EQ("公告信息.公告类别", 公告.公告类别.协议采购)), false, SortBy.Descending("内容主体.发布时间"), false);
            ViewData["单一来源"] = 公告管理.查询公告(0, 8, q.And(Query.EQ("公告信息.公告类别", 公告.公告类别.单一来源)), false, SortBy.Descending("内容主体.发布时间"), false);
            ViewData["询价采购"] = 公告管理.查询公告(0, 8, q.And(Query.EQ("公告信息.公告类别", 公告.公告类别.询价采购)), false, SortBy.Descending("内容主体.发布时间"), false);
            ViewData["竞争性谈判"] = 公告管理.查询公告(0, 8, q.And(Query.EQ("公告信息.公告类别", 公告.公告类别.竞争性谈判)), false, SortBy.Descending("内容主体.发布时间"), false);

            ViewData["行业动态"] = 公告管理.查询公告(0, 8, q.And(Query.EQ("公告信息.公告类别", 公告.公告类别.其他)), false, SortBy.Descending("内容主体.发布时间"), false);

            var p_l = 用户管理.列表用户<供应商>(0, 0,
                Fields<供应商>.Include(o => o.Id),
                Query<供应商>.EQ(o => o.审核数据.审核状态, 审核状态.审核通过).And(Query<供应商>.Where(o => !o.供应商用户信息.协议供应商 && !o.供应商用户信息.应急供应商))
                ).Select(o => o["_id"]);
            var p_q = Query.And(
                    Query.In("商品信息.所属供应商.用户ID", p_l),
                    q
                );

            //通知部分--出去曝光台
            ViewData["曝光台"] = 通知管理.查询通知(0, 10, Query<通知>.Where(o => o.通知信息.通知所属 == 通知.通知所属.黑名单 && o.审核数据.审核状态 == 审核状态.审核通过), false, SortBy.Descending("内容主体.发布时间"), false);

            var sp2 = new List<商品>();
            List<long> gids = new List<long>();
            List<long> ids = new List<long>();
            var now = DateTime.Now;

            var 供应商服务记录 = 供应商服务记录管理.查询供应商服务记录(0, 0, null, false, SortBy<供应商服务记录>.Ascending(o => o.基本数据.修改时间));
            var A2广告位置服务记录 = 供应商服务记录.Where(o => o.已开通的服务.Exists(p => p.所申请项目名 == "企业推广服务A2位置" && p.签订时间 < now && p.结束时间 > now));
            if (A2广告位置服务记录.Count() > 0)
            {
                foreach (var u in A2广告位置服务记录)
                {
                    var A2位置 = u.已开通的服务.Find(o => o.所申请项目名 == "企业推广服务A2位置" && o.签订时间 < now && o.结束时间 > now);
                    if (A2位置 != null)
                    {
                        var 供应商 = 用户管理.查找用户<供应商>(u.所属供应商.用户ID);
                        var 广告商品 = 供应商.供应商用户信息.广告商品.Where(o => o.Key.Contains("企业推广服务A2位置"));
                        if (广告商品 != null && 广告商品.Count() > 0)
                        {
                            foreach (var t in 广告商品.First().Value)
                            {
                                sp2.Add(t.商品.商品);
                                gids.Add(t.商品.商品ID);
                                ids.Add(供应商.Id);
                            }
                        }
                        else
                        {
                            IEnumerable<商品> gd = 商品管理.查询商品(0, 1, Query<商品>.Where(o => o.商品信息.所属供应商.用户ID == u.所属供应商.用户ID && o.审核数据.审核状态 == 审核状态.审核通过), false, SortBy.Descending("销售信息.点击量"), false);
                            if (gd != null && gd.Count() != 0)
                            {
                                sp2.Add(gd.First());
                                gids.Add(gd.First().Id);
                                ids.Add(gd.First().商品信息.所属供应商.用户ID);
                            }
                        }
                    }
                }
            }
            var count = sp2.Count();
            ViewData["A2类别"] = "1";
            if (count < 10) //如果推广商品小于10个，则按点击量排序选择一定量的占位商品
            {
                var 补充商品 = 商品管理.查询商品(0, 0, p_q.And(Query<商品>.EQ(o => o.审核数据.审核状态, 审核状态.审核通过)).And(Query<商品>.NotIn(o => o.Id, gids)), false, SortBy.Descending("销售信息.点击量"), false);
                foreach (var sp1 in 补充商品)
                {
                    if (ids.Contains(sp1.商品信息.所属供应商.用户ID)) continue;
                    sp2.Add(sp1);
                    gids.Add(sp1.Id);
                    ids.Add(sp1.商品信息.所属供应商.用户ID);
                    ++count;
                    if (count == 10) break;
                }
            }
            else if (count > 10 && count <= 20) //如果推广商品>10 <20，则按点击量排序选择一定量的占位商品
            {
                ViewData["A2类别"] = "2";
                if (count < 20)
                {
                    var 补充商品 = 商品管理.查询商品(0, 0, p_q.And(Query<商品>.EQ(o => o.审核数据.审核状态, 审核状态.审核通过)).And(Query<商品>.NotIn(o => o.Id, gids)), false, SortBy.Descending("销售信息.点击量"), false);
                    foreach (var sp1 in 补充商品)
                    {
                        if (ids.Contains(sp1.商品信息.所属供应商.用户ID)) continue;
                        sp2.Add(sp1);
                        gids.Add(sp1.Id);
                        ids.Add(sp1.商品信息.所属供应商.用户ID);
                        ++count;
                        if (count == 20) break;
                    }
                }
            }
            else
            {
                ViewData["A2类别"] = "3";
                if (count < 30)
                {
                    var 补充商品 = 商品管理.查询商品(0, 0, p_q.And(Query<商品>.EQ(o => o.审核数据.审核状态, 审核状态.审核通过)).And(Query<商品>.NotIn(o => o.Id, gids)), false, SortBy.Descending("销售信息.点击量"), false);
                    foreach (var sp1 in 补充商品)
                    {
                        if (ids.Contains(sp1.商品信息.所属供应商.用户ID)) continue;
                        sp2.Add(sp1);
                        gids.Add(sp1.Id);
                        ids.Add(sp1.商品信息.所属供应商.用户ID);
                        ++count;
                        if (count == 30) break;
                    }
                }
                if (count > 30)
                {
                    sp2 = sp2.Take(30).ToList();
                }
            }
            ViewData["首页主推商品信息"] = (IEnumerable<商品>)sp2;


            /////////////////////////原网上竞价处修改
            var sp_add = new List<商品>();
            var w = 供应商服务记录管理.查询供应商服务记录(0, 0);//找出开通了A1广告位置的供应商增值服务记录
            var g = w.Where(o => o.已开通的服务.Exists(p => p.所申请项目名 == "企业推广服务A1位置" && p.签订时间 < now && p.结束时间 > now && p.是否通过 == 通过状态.通过));
            foreach (var k in g)//通过供应商服务记录的所属供应商ID找出相应供应商，并把该供应商选择的广告位置展示商品列出来
            {
                var 供应商 = 用户管理.查找用户<供应商>(k.所属供应商.用户ID);
                var 广告商品 = 供应商.供应商用户信息.广告商品.Where(o => o.Key.Contains("企业推广服务A1位置"));
                if (广告商品 != null && 广告商品.Count() > 0)
                {
                    foreach (var t in 广告商品.First().Value)
                    {
                        sp_add.Add(t.商品.商品);
                        gids.Add(t.商品.商品ID);
                        ids.Add(供应商.Id);
                    }
                }
                else
                {
                    IEnumerable<商品> gd = 商品管理.查询商品(0, 1, Query<商品>.Where(o => o.商品信息.所属供应商.用户ID == k.所属供应商.用户ID && o.审核数据.审核状态 == 审核状态.审核通过), false, SortBy.Descending("销售信息.点击量"), false);
                    if (gd != null && gd.Count() != 0)
                    {
                        sp_add.Add(gd.First());
                        gids.Add(gd.First().Id);
                        ids.Add(gd.First().商品信息.所属供应商.用户ID);
                    }
                }
            }
            var countsp = sp_add.Count();
            if (countsp < 6)
            {
                var 补充商品1 = 商品管理.查询商品(0, 0, p_q.And(Query<商品>.EQ(o => o.审核数据.审核状态, 审核状态.审核通过)).And(Query<商品>.NotIn(o => o.Id, gids)), false, SortBy.Descending("销售信息.点击量"), false);
                foreach (var sp1 in 补充商品1)
                {
                    if (ids.Contains(sp1.商品信息.所属供应商.用户ID)) continue;
                    sp_add.Add(sp1);
                    ids.Add(sp1.商品信息.所属供应商.用户ID);
                    ++countsp;
                    if (countsp == 6) break;
                }

            }
            ViewData["新增推荐商品"] = (IEnumerable<商品>)sp_add;
            /////////////////////////原网上竞价处修改
            List<供应商> supplierList = new List<供应商>();
            ids = new List<long>();
            IEnumerable<供应商增值服务申请记录> supplier = 供应商增值服务申请记录管理.查询供应商增值服务申请记录(0, 0, Query<供应商增值服务申请记录>.Where(o => o.所申请项目名 == "企业推广服务A3位置" && o.是否通过 == 通过状态.通过 && o.签订时间 < now && o.结束时间 > now), false, SortBy<供应商增值服务申请记录>.Ascending(o => o.基本数据.修改时间));
            if (supplier != null && supplier.Count() != 0)
            {
                foreach (var item in supplier)
                {
                    supplierList.Add(item.所属供应商.用户数据);
                    ids.Add(item.所属供应商.用户ID);
                }
            }
            ViewData["A3类别"] = "1";
            var suppliercount = supplierList.Count;
            if (suppliercount < 10)
            {
                IEnumerable<供应商> gys = 用户管理.查询用户<供应商>(0, 10 - suppliercount, Query<供应商>.Where(
                o => o.审核数据.审核状态 == 审核状态.审核通过
                && o.供应商用户信息.供应商图片.Count > 0
                && o.所属地域.省份 != null
                && o.企业基本信息.所属行业 != null
                    && !o.供应商用户信息.协议供应商
                    && o.Id != 200000000462
                    && !o.供应商用户信息.应急供应商).And(Query<供应商>.NotIn(o => o.Id, ids)), includeDisabled: false);
                if (gys != null && gys.Count() != 0)
                {
                    foreach (var item in gys)
                    {
                        supplierList.Add(item);
                    }
                }
            }
            else if (suppliercount > 10 && suppliercount <= 20)
            {
                ViewData["A3类别"] = "2";
                if (suppliercount < 20)
                {
                    IEnumerable<供应商> gys = 用户管理.查询用户<供应商>(0, 20 - suppliercount, Query<供应商>.Where(
               o => o.审核数据.审核状态 == 审核状态.审核通过
               && o.供应商用户信息.供应商图片.Count > 0
               && o.所属地域.省份 != null
               && o.企业基本信息.所属行业 != null
                   && !o.供应商用户信息.协议供应商
                   && o.Id != 200000000462
                   && !o.供应商用户信息.应急供应商).And(Query<供应商>.NotIn(o => o.Id, ids)), includeDisabled: false);
                    if (gys != null && gys.Count() != 0)
                    {
                        foreach (var item in gys)
                        {
                            supplierList.Add(item);
                        }
                    }
                }
            }
            else
            {
                ViewData["A3类别"] = "3";
                if (suppliercount < 30)
                {
                    IEnumerable<供应商> gys = 用户管理.查询用户<供应商>(0, 30 - suppliercount, Query<供应商>.Where(
               o => o.审核数据.审核状态 == 审核状态.审核通过
               && o.供应商用户信息.供应商图片.Count > 0
               && o.所属地域.省份 != null
               && o.企业基本信息.所属行业 != null
                   && !o.供应商用户信息.协议供应商
                   && o.Id != 200000000462
                   && !o.供应商用户信息.应急供应商).And(Query<供应商>.NotIn(o => o.Id, ids)), includeDisabled: false);
                    if (gys != null && gys.Count() != 0)
                    {
                        foreach (var item in gys)
                        {
                            supplierList.Add(item);
                        }
                    }
                }
                if (suppliercount > 30)
                {
                    supplierList = supplierList.Take(30).ToList();
                }
            }

            ViewData["供应商信息"] = supplierList;
            //ViewData["供应商信息"] = 用户管理.查询用户<供应商>(0, 10, Query<供应商>.GTE(o => o.供应商用户信息.认证级别, 供应商.认证级别.已审核用户).And(Query<供应商>.GT(o => o.供应商用户信息.供应商图片.Count, 0)).And(Query<供应商>.NE(o => o.所属地域.地域, null)).And(Query<供应商>.NE(o => o.企业基本信息.所属行业, null)), includeDisabled: false);
            //ViewData["供应商信息"] = 用户管理.查询用户<供应商>(0, 10, Query<供应商>.Where(
            //    o => o.审核数据.审核状态 == 审核状态.审核通过
            //    && o.供应商用户信息.供应商图片.Count > 0
            //    && o.所属地域.省份 != null
            //    && o.企业基本信息.所属行业 != null
            //    , false, SortBy.Descending("基本数据.添加时间"), includeDisabled: false);
            ////IEnumerable<供应商推广> model= 供应商推广管理.管理器.查询(0, 0, Query<供应商推广>.Where(m => m.广告位置 == 供应商推广._广告位置.新入库&&m.结束时间>DateTime.Now));
            //List<供应商> gys = new List<供应商>();
            //if(model!=null&&model.Count()!=0)
            //{
            //    foreach(var item in model)
            //    {
            //        gys.Add(item.供应商.用户数据);
            //    }
            //}
            //ViewData["新入库供应商信息"] = (IEnumerable<供应商>)gys;
            //ViewData["新入库供应商信息"] = 用户管理.查询用户<供应商>(0, 10, Query<供应商>.Where(
            //    o => o.审核数据.审核状态 == 审核状态.审核通过
            //    && o.供应商用户信息.供应商图片.Count > 0
            //    && o.所属地域.省份 != null
            //    && o.企业基本信息.所属行业 != null
            //    && (/*o.供应商用户信息.已入库 ||*/ o.供应商用户信息.协议供应商 || o.供应商用户信息.应急供应商))
            //.And(Query<供应商>.NotIn(o => o.Id, new List<long> { 200000000594, 200000000596, 200000000598, 200000000600 }))
            //    , false, SortBy.Descending("基本数据.添加时间"), includeDisabled: false);

            return PartialView();
        }
        [静态化]
        [GZipResponse]
        public ActionResult Index()
        {
#if INTRANET
            return View("Intranet");
#else
            return View();
#endif
        }

        public ActionResult OnlineSever()
        {
            return View();
        }

        public ActionResult Part_OnlineSever()
        {
            return PartialView();
        }
        public ActionResult CheckYsd()
        {
            var num = Request.Params["num"];
            var code = Request.Params["code"];
            var cost = Request.Params["cost"];
            if (!string.IsNullOrWhiteSpace(num) && !string.IsNullOrWhiteSpace(code) && !string.IsNullOrWhiteSpace(cost))
            {
                var list = 验收单管理.查询验收单(0, 1, Query<验收单>.Where(o => o.验收单号 == num && o.验收单核对码 == code));
                if (list != null && list.Any())
                {
                    if (list.First().总金额 == decimal.Parse(cost))
                    {
                        return Content("该验收单与数据库一致");
                    }
                    else
                    {
                        return Content("该验收单金额与数据库不一致");
                    }
                }
                else
                {
                    return Content("该单号验收单不存在");
                }
            }
            else
            {
                return Content("请将信息输入完整");
            }

        }
        public ActionResult SearchAcceptance()
        {
            string id = Request.Params["id"];
            if (!string.IsNullOrEmpty(id))
            {
                验收单 ysd = 验收单管理.查找验收单(long.Parse(id));
                return View(ysd);
            }
            else
            {
                return RedirectToAction("Index");
            }

        }
        public class GysBrief
        {
            public string 图片 { get; set; }
            public string 主营产品 { get; set; }
            public string 认证级别 { get; set; }
            public string 经营类型 { get; set; }
            public string 员工人数 { get; set; }
        }
        public JsonResult ShowBrief()
        {
            var id = Request.Params["id"];
            var gys = 用户管理.查找用户<供应商>(long.Parse(id));
            var m = "";
            foreach (var h in gys.可提供产品类别列表)
            {
                m += h.一级分类 + " ";

            }
            if (gys.供应商用户信息.供应商图片 == null || gys.供应商用户信息.供应商图片.Count() == 0)
            {
                gys.供应商用户信息.供应商图片 = new List<string>();
                gys.供应商用户信息.供应商图片.Add("");
            }
            var brief = new GysBrief()
            {
                图片 = gys.供应商用户信息.供应商图片.Last(),
                主营产品 = m,
                认证级别 = gys.供应商用户信息.认证级别.ToString(),
                经营类型 = gys.企业基本信息.经营类型.ToString() + "/" + gys.企业基本信息.经营子类型,
                员工人数 = gys.企业基本信息.员工人数.ToString(),
            };
            JsonResult json = new JsonResult() { Data = brief };
            return Json(json, JsonRequestBehavior.AllowGet);
        }

        #region  内网首页
        public ActionResult Intranet()
        {
            return View();
        }
        public ActionResult Part_Index_LeftMenu_Intranet()
        {
            //var x = GetPageCode("192.168.1.177/api/expert/PostLogin?LoginName=cdjqcgc002&LoginPwd=gsd_20156302", "UTF-8");

            //var v = GetPageCode("http://www.suning.com/webapp/wcs/stores/comboPrice/9265_000000000928635487_showComoboStatus.html", "UTF-8").Replace("\"", ""); ;
            //var m = GetPage("http://localhost:49918/api/expert/PostLogin", "LoginName=cdjqcgc002&LoginPwd=1234567");
            //var x = GetPage("http://localhost:49918/api/expert/Part_Expert_Applay", "tokenId=" + m);
            //var a = GetPage("http://localhost:49918/api/expert/Part_Expert_ApplayOutList", "page=1&name=王仕平&tokenId=" + m);

            ////京东价格解析
            //var jd_getcode = GetPageCode("http://p.3.cn/prices/get?skuid=J_1514843515", "UTF-8").Replace("\"", "");
            //var jd_index1 = jd_getcode.IndexOf("p:");
            //if (jd_index1 != -1)
            //{
            //    var jd_str = jd_getcode.Substring(jd_index1 + 2);
            //    var jd_index2 = jd_str.IndexOf(",");
            //    if (jd_index2 != -1)
            //    {
            //        //解析出的价格
            //        var jd_price = jd_str.Substring(0, jd_index2);
            //    }
            //}
            ////苏宁价格解析
            //var suning_getcode = GetPageCode("http://www.suning.com/webapp/wcs/stores/ItemPrice/131547158__9265_12132_1.html", "UTF-8").Replace("\"", "");
            //var suning_index1 = suning_getcode.IndexOf("netPrice:");
            //if (suning_index1 != -1)
            //{
            //    var suning_str = suning_getcode.Substring(suning_index1 + 9);
            //    var suning_index2 = suning_str.IndexOf(",");
            //    if (suning_index2 != -1)
            //    {
            //        //解析出的价格
            //        var suning_price = suning_str.Substring(0, suning_index2);
            //    }
            //}


            var firstclass = 商品分类管理.查找子分类();
            return PartialView(firstclass);
        }
        public ActionResult Part_Index_Content_Intranet()
        {
            //通知部分--出去曝光台
            ViewData["通知"] = 通知管理.查询通知(0, 10, Query<通知>.Where(o => o.通知信息.通知所属 != 通知.通知所属.黑名单 && o.审核数据.审核状态 == 审核状态.审核通过), false, SortBy.Descending("内容主体.发布时间"), false);

            //排序《按照修改时间倒序排序》
            var q = Query.And(
                    Query.EQ("公告信息.公告类别", 公告.公告类别.公开招标),
                    Query.EQ("审核数据.审核状态", 审核状态.审核通过)
                );

            var picnews = 新闻管理.查询图片新闻(0, 5, Query<新闻>.EQ(o => o.审核数据.审核状态, 审核状态.审核通过));
            ViewData["图片新闻动态"] = picnews;
            //ViewData["listcount"] = picnews.Count();
            //政策法规部分
            ViewData["政策法规"] = 政策法规管理.查询政策法规(0, 9, Query<政策法规>.EQ(o => o.审核数据.审核状态, 审核状态.审核通过), false, SortBy.Descending("内容主体.发布时间"), false);


            //公开招标部分 
            //ViewData["全部公开招标公告"] = 公告管理.查询公告(0, 8, q.And(Query.Or(new[] { Query.EQ("公告信息.公告性质", 公告.公告性质.预公告), Query.EQ("公告信息.公告性质", 公告.公告性质.采购公告), Query.EQ("公告信息.公告性质", 公告.公告性质.技术公告), Query.EQ("公告信息.公告性质", 公告.公告性质.需求公告) })), false, SortBy.Descending("内容主体.发布时间"), false);
            ViewData["全部公开招标公告"] = 公告管理.查询公告(0, 8, q.And(Query.Or(new[] { Query.EQ("公告信息.公告性质", 公告.公告性质.预公告), Query.EQ("公告信息.公告性质", 公告.公告性质.采购公告), Query.EQ("公告信息.公告性质", 公告.公告性质.技术公告), Query.EQ("公告信息.公告性质", 公告.公告性质.需求公告), Query.EQ("公告信息.公告性质", 公告.公告性质.补遗公告), Query.EQ("公告信息.公告性质", 公告.公告性质.更正公告), Query.EQ("公告信息.公告性质", 公告.公告性质.变更公告), Query.EQ("公告信息.公告性质", 公告.公告性质.答疑公告) })), false, SortBy.Descending("内容主体.发布时间"), false);

            ViewData["预公告"] = 公告管理.查询公告(0, 8, q.And(Query.Or(new[] { Query.EQ("公告信息.公告性质", 公告.公告性质.预公告), Query.EQ("公告信息.公告性质", 公告.公告性质.需求公告) })), false, SortBy.Descending("内容主体.发布时间"), false);

            ViewData["技术公告"] = 公告管理.查询公告(0, 8, q.And(Query<公告>.Where(o => o.公告信息.公告性质 != 公告.公告性质.采购公告 ||
                                                                                    o.公告信息.公告性质 != 公告.公告性质.需求公告)), false, SortBy.Descending("内容主体.发布时间"), false);

            ViewData["发标公告"] = 公告管理.查询公告(0, 8, q.And(Query.EQ("公告信息.公告性质", 公告.公告性质.采购公告)), false, SortBy.Descending("内容主体.发布时间"), false);
            /*ViewData["中标公告"] = 公告管理.查询公告(0, 8, Query.EQ("审核数据.审核状态", 审核状态.审核通过).And(Query.EQ("公告信息.公告性质", 公告.公告性质.中标结果公示)), false, SortBy.Descending("内容主体.发布时间"), false);
            ViewData["废标公告"] = 公告管理.查询公告(0, 8, q.And(Query.EQ("公告信息.公告性质", 公告.公告性质.废标公告)), false, SortBy.Descending("内容主体.发布时间"), false);
            ViewData["流标公告"] = 公告管理.查询公告(0, 8, q.And(Query.EQ("公告信息.公告性质", 公告.公告性质.流标公告)), false, SortBy.Descending("内容主体.发布时间"), false);
            */
            ViewData["中标公告"] = 公告管理.查询公告(0, 8, Query<公告>.Where(m => m.审核数据.审核状态 == 审核状态.审核通过 && m.公告信息.公告性质 == 公告.公告性质.中标结果公示), false, SortBy.Descending("内容主体.发布时间"), false);
            ViewData["废标公告"] = 公告管理.查询公告(0, 8, Query<公告>.Where(m => m.公告信息.公告性质 == 公告.公告性质.废标公告 && m.审核数据.审核状态 == 审核状态.审核通过), false, SortBy.Descending("内容主体.发布时间"), false);
            ViewData["流标公告"] = 公告管理.查询公告(0, 8, Query<公告>.Where(m => m.公告信息.公告性质 == 公告.公告性质.流标公告 && m.审核数据.审核状态 == 审核状态.审核通过), false, SortBy.Descending("内容主体.发布时间"), false);
            //采购部分
            var q1 = Query.EQ("审核数据.审核状态", 审核状态.审核通过).And(Query.NE("公告信息.公告性质", 公告.公告性质.中标结果公示)).And(Query.NE("公告信息.公告性质", 公告.公告性质.流标公告)).And(Query.NE("公告信息.公告性质", 公告.公告性质.废标公告));
            ViewData["全部采购公告"] = 公告管理.查询公告(0, 8, q1.And(Query.NE("公告信息.公告类别", 公告.公告类别.公开招标)).And(Query.NE("公告信息.公告类别", 公告.公告类别.其他)).And(Query.NE("公告信息.公告类别", 公告.公告类别.单一来源)), false, SortBy.Descending("内容主体.发布时间"), false);
            ViewData["邀请招标"] = 公告管理.查询公告(0, 8, q1.And(Query.EQ("公告信息.公告类别", 公告.公告类别.邀请招标)), false, SortBy.Descending("内容主体.发布时间"), false);
            ViewData["单一来源"] = 公告管理.查询公告(0, 8, q1.And(Query.EQ("公告信息.公告类别", 公告.公告类别.单一来源)), false, SortBy.Descending("内容主体.发布时间"), false);
            ViewData["询价采购"] = 公告管理.查询公告(0, 8, q1.And(Query.EQ("公告信息.公告类别", 公告.公告类别.询价采购)), false, SortBy.Descending("内容主体.发布时间"), false);
            ViewData["竞争性谈判"] = 公告管理.查询公告(0, 8, q1.And(Query.EQ("公告信息.公告类别", 公告.公告类别.竞争性谈判)), false, SortBy.Descending("内容主体.发布时间"), false);

            ViewData["通知"] = 通知管理.查询通知(0, 8, Query<通知>.Where(o => o.通知信息.通知所属 != 通知.通知所属.黑名单 && o.审核数据.审核状态 == 审核状态.审核通过), false, SortBy.Descending("内容主体.发布时间"), false);

            ViewData["全部结果公示"] = 公告管理.查询公告(0, 8, Query.Or(new[] { Query.EQ("审核数据.审核状态", 审核状态.审核通过).And(Query.EQ("公告信息.公告性质", 公告.公告性质.中标结果公示)), q.And(Query.EQ("公告信息.公告性质", 公告.公告性质.废标公告)), q.And(Query.EQ("公告信息.公告性质", 公告.公告性质.流标公告)) }), false, SortBy.Descending("内容主体.发布时间"), false);
            return PartialView();
        }
        public ActionResult Part_Index_Bottom_Intranet()
        {
            //var picnews = 新闻管理.查询图片新闻(0, 5, Query<新闻>.EQ(o => o.审核数据.审核状态, 审核状态.审核通过));
            //ViewData["图片新闻动态"] = picnews;
            //ViewData["listcount"] = picnews.Count();

            ViewData["通知"] = 通知管理.查询通知(0, 9, Query<通知>.Where(o => o.通知信息.通知所属 != 通知.通知所属.黑名单 && o.审核数据.审核状态 == 审核状态.审核通过), false, SortBy.Descending("内容主体.发布时间"), false);

            //排序《按照修改时间倒序排序》
            //新闻动态部分
            ViewData["新闻动态"] = 新闻管理.查询新闻(0, 9, Query<新闻>.EQ(o => o.审核数据.审核状态, 审核状态.审核通过), false, SortBy.Descending("内容主体.发布时间"), false);
            //政策法规部分
            ViewData["政策法规"] = 政策法规管理.查询政策法规(0, 9, Query<政策法规>.EQ(o => o.审核数据.审核状态, 审核状态.审核通过), false, SortBy.Descending("内容主体.发布时间"), false);
            ViewData["服务指南"] = 办事指南管理.查询办事指南(0, 8, null, false, SortBy.Descending("内容主体.发布时间"), false);
            ViewData["网上竞标"] = 网上竞标管理.查询网上竞标(0, 6);
            //采购部分
            var q = Query.EQ("审核数据.审核状态", 审核状态.审核通过).And(Query.NE("公告信息.公告性质", 公告.公告性质.中标结果公示));
            ViewData["全部采购公告"] = 公告管理.查询公告(0, 8, q.And(Query.NE("公告信息.公告类别", 公告.公告类别.公开招标)).And(Query.NE("公告信息.公告类别", 公告.公告类别.其他)), false, SortBy.Descending("内容主体.发布时间"), false);
            ViewData["邀请招标"] = 公告管理.查询公告(0, 8, q.And(Query.EQ("公告信息.公告类别", 公告.公告类别.邀请招标)), false, SortBy.Descending("内容主体.发布时间"), false);
            ViewData["协议采购"] = 公告管理.查询公告(0, 8, q.And(Query.EQ("公告信息.公告类别", 公告.公告类别.协议采购)), false, SortBy.Descending("内容主体.发布时间"), false);
            ViewData["单一来源"] = 公告管理.查询公告(0, 8, q.And(Query.EQ("公告信息.公告类别", 公告.公告类别.单一来源)), false, SortBy.Descending("内容主体.发布时间"), false);
            ViewData["询价采购"] = 公告管理.查询公告(0, 8, q.And(Query.EQ("公告信息.公告类别", 公告.公告类别.询价采购)), false, SortBy.Descending("内容主体.发布时间"), false);
            ViewData["竞争性谈判"] = 公告管理.查询公告(0, 8, q.And(Query.EQ("公告信息.公告类别", 公告.公告类别.竞争性谈判)), false, SortBy.Descending("内容主体.发布时间"), false);

            ViewData["行业动态"] = 公告管理.查询公告(0, 8, q.And(Query.EQ("公告信息.公告类别", 公告.公告类别.其他)), false, SortBy.Descending("内容主体.发布时间"), false);

            var p_l = 用户管理.列表用户<供应商>(0, 0,
               Fields<供应商>.Include(o => o.Id),
               Query<供应商>.EQ(o => o.审核数据.审核状态, 审核状态.审核通过).And(Query<供应商>.Where(o => !o.供应商用户信息.协议供应商 && !o.供应商用户信息.应急供应商))
               ).Select(o => o["_id"]);
            var p_q = Query.And(
                    Query.In("商品信息.所属供应商.用户ID", p_l),
                    q
                );


            //通知部分--出去曝光台
            ViewData["曝光台"] = 通知管理.查询通知(0, 10, Query<通知>.Where(o => o.通知信息.通知所属 == 通知.通知所属.黑名单 && o.审核数据.审核状态 == 审核状态.审核通过), false, SortBy.Descending("内容主体.发布时间"), false);


            var sp2 = new List<商品>();
            List<long> gids = new List<long>();
            List<long> ids = new List<long>();
            var now = DateTime.Now;

            var 供应商服务记录 = 供应商服务记录管理.查询供应商服务记录(0, 0, null, false, SortBy<供应商服务记录>.Ascending(o => o.基本数据.修改时间));
            var A2广告位置服务记录 = 供应商服务记录.Where(o => o.已开通的服务.Exists(p => p.所申请项目名 == "企业推广服务A2位置" && p.签订时间 < now && p.结束时间 > now));
            if (A2广告位置服务记录.Count() > 0)
            {
                foreach (var u in A2广告位置服务记录)
                {
                    var A2位置 = u.已开通的服务.Find(o => o.所申请项目名 == "企业推广服务A2位置" && o.签订时间 < now && o.结束时间 > now);
                    if (A2位置 != null)
                    {
                        var 供应商 = 用户管理.查找用户<供应商>(u.所属供应商.用户ID);
                        var 广告商品 = 供应商.供应商用户信息.广告商品.Where(o => o.Key.Contains("企业推广服务A2位置"));
                        if (广告商品 != null && 广告商品.Count() > 0)
                        {
                            foreach (var t in 广告商品.First().Value)
                            {
                                sp2.Add(t.商品.商品);
                                gids.Add(t.商品.商品ID);
                                ids.Add(供应商.Id);
                            }
                        }
                        else
                        {
                            IEnumerable<商品> gd = 商品管理.查询商品(0, 1, Query<商品>.Where(o => o.商品信息.所属供应商.用户ID == u.所属供应商.用户ID && o.审核数据.审核状态 == 审核状态.审核通过), false, SortBy.Descending("销售信息.点击量"), false);
                            if (gd != null && gd.Count() != 0)
                            {
                                sp2.Add(gd.First());
                                gids.Add(gd.First().Id);
                                ids.Add(gd.First().商品信息.所属供应商.用户ID);
                            }
                        }
                    }
                }
            }
            var count = sp2.Count();
            ViewData["A2类别"] = "1";
            if (count < 10) //如果推广商品小于10个，则按点击量排序选择一定量的占位商品
            {
                var 补充商品 = 商品管理.查询商品(0, 0, p_q.And(Query<商品>.EQ(o => o.审核数据.审核状态, 审核状态.审核通过)).And(Query<商品>.NotIn(o => o.Id, gids)), false, SortBy.Descending("销售信息.点击量"), false);
                foreach (var sp1 in 补充商品)
                {
                    if (ids.Contains(sp1.商品信息.所属供应商.用户ID)) continue;
                    sp2.Add(sp1);
                    gids.Add(sp1.Id);
                    ids.Add(sp1.商品信息.所属供应商.用户ID);
                    ++count;
                    if (count == 10) break;
                }
            }
            else if (count > 10 && count <= 20) //如果推广商品>10 <20，则按点击量排序选择一定量的占位商品
            {
                ViewData["A2类别"] = "2";
                if (count < 20)
                {
                    var 补充商品 = 商品管理.查询商品(0, 0, p_q.And(Query<商品>.EQ(o => o.审核数据.审核状态, 审核状态.审核通过)).And(Query<商品>.NotIn(o => o.Id, gids)), false, SortBy.Descending("销售信息.点击量"), false);
                    foreach (var sp1 in 补充商品)
                    {
                        if (ids.Contains(sp1.商品信息.所属供应商.用户ID)) continue;
                        sp2.Add(sp1);
                        gids.Add(sp1.Id);
                        ids.Add(sp1.商品信息.所属供应商.用户ID);
                        ++count;
                        if (count == 20) break;
                    }
                }
            }
            else
            {
                ViewData["A2类别"] = "3";
                if (count < 30)
                {
                    var 补充商品 = 商品管理.查询商品(0, 0, p_q.And(Query<商品>.EQ(o => o.审核数据.审核状态, 审核状态.审核通过)).And(Query<商品>.NotIn(o => o.Id, gids)), false, SortBy.Descending("销售信息.点击量"), false);
                    foreach (var sp1 in 补充商品)
                    {
                        if (ids.Contains(sp1.商品信息.所属供应商.用户ID)) continue;
                        sp2.Add(sp1);
                        gids.Add(sp1.Id);
                        ids.Add(sp1.商品信息.所属供应商.用户ID);
                        ++count;
                        if (count == 30) break;
                    }
                }
                if (count > 30)
                {
                    sp2 = sp2.Take(30).ToList();
                }
            }
            ViewData["首页主推商品信息"] = (IEnumerable<商品>)sp2;

            List<供应商> supplierList = new List<供应商>();
            ids = new List<long>();
            IEnumerable<供应商增值服务申请记录> supplier = 供应商增值服务申请记录管理.查询供应商增值服务申请记录(0, 0, Query<供应商增值服务申请记录>.Where(o => o.所申请项目名 == "企业推广服务A3位置" && o.是否通过 == 通过状态.通过 && o.签订时间 < now && o.结束时间 > now), false, SortBy<供应商增值服务申请记录>.Ascending(o => o.基本数据.修改时间));
            if (supplier != null && supplier.Count() != 0)
            {
                foreach (var item in supplier)
                {
                    supplierList.Add(item.所属供应商.用户数据);
                    ids.Add(item.所属供应商.用户ID);
                }
            }
            ViewData["A3类别"] = "1";
            var suppliercount = supplierList.Count;
            if (suppliercount < 10)
            {
                IEnumerable<供应商> gys = 用户管理.查询用户<供应商>(0, 10 - suppliercount, Query<供应商>.Where(
                o => o.审核数据.审核状态 == 审核状态.审核通过
                && o.供应商用户信息.供应商图片.Count > 0
                && o.所属地域.省份 != null
                && o.企业基本信息.所属行业 != null
                    && !o.供应商用户信息.协议供应商
                    && o.Id != 200000000462
                    && !o.供应商用户信息.应急供应商).And(Query<供应商>.NotIn(o => o.Id, ids)), includeDisabled: false);
                if (gys != null && gys.Count() != 0)
                {
                    foreach (var item in gys)
                    {
                        supplierList.Add(item);
                    }
                }
            }
            else if (suppliercount > 10 && suppliercount <= 20)
            {
                ViewData["A3类别"] = "2";
                if (suppliercount < 20)
                {
                    IEnumerable<供应商> gys = 用户管理.查询用户<供应商>(0, 20 - suppliercount, Query<供应商>.Where(
               o => o.审核数据.审核状态 == 审核状态.审核通过
               && o.供应商用户信息.供应商图片.Count > 0
               && o.所属地域.省份 != null
               && o.企业基本信息.所属行业 != null
                   && !o.供应商用户信息.协议供应商
                   && o.Id != 200000000462
                   && !o.供应商用户信息.应急供应商).And(Query<供应商>.NotIn(o => o.Id, ids)), includeDisabled: false);
                    if (gys != null && gys.Count() != 0)
                    {
                        foreach (var item in gys)
                        {
                            supplierList.Add(item);
                        }
                    }
                }
            }
            else
            {
                ViewData["A3类别"] = "3";
                if (suppliercount < 30)
                {
                    IEnumerable<供应商> gys = 用户管理.查询用户<供应商>(0, 30 - suppliercount, Query<供应商>.Where(
               o => o.审核数据.审核状态 == 审核状态.审核通过
               && o.供应商用户信息.供应商图片.Count > 0
               && o.所属地域.省份 != null
               && o.企业基本信息.所属行业 != null
                   && !o.供应商用户信息.协议供应商
                   && o.Id != 200000000462
                   && !o.供应商用户信息.应急供应商).And(Query<供应商>.NotIn(o => o.Id, ids)), includeDisabled: false);
                    if (gys != null && gys.Count() != 0)
                    {
                        foreach (var item in gys)
                        {
                            supplierList.Add(item);
                        }
                    }
                }
                if (suppliercount > 30)
                {
                    supplierList = supplierList.Take(30).ToList();
                }
            }

            ViewData["供应商信息"] = supplierList;

            /////////////////////////原网上竞价处修改
            var sp_add = new List<商品>();
            var w = 供应商服务记录管理.查询供应商服务记录(0, 0);//找出开通了A1广告位置的供应商增值服务记录
            var g = w.Where(o => o.已开通的服务.Exists(p => p.所申请项目名 == "企业推广服务A1位置" && p.签订时间 < now && p.结束时间 > now && p.是否通过 == 通过状态.通过));
            foreach (var k in g)//通过供应商服务记录的所属供应商ID找出相应供应商，并把该供应商选择的广告位置展示商品列出来
            {
                var 供应商 = 用户管理.查找用户<供应商>(k.所属供应商.用户ID);
                var 广告商品 = 供应商.供应商用户信息.广告商品.Where(o => o.Key.Contains("企业推广服务A1位置"));
                if (广告商品 != null && 广告商品.Count() > 0)
                {
                    foreach (var t in 广告商品.First().Value)
                    {
                        sp_add.Add(t.商品.商品);
                        gids.Add(t.商品.商品ID);
                        ids.Add(供应商.Id);
                    }
                }
                else
                {
                    IEnumerable<商品> gd = 商品管理.查询商品(0, 1, Query<商品>.Where(o => o.商品信息.所属供应商.用户ID == k.所属供应商.用户ID && o.审核数据.审核状态 == 审核状态.审核通过), false, SortBy.Descending("销售信息.点击量"), false);
                    if (gd != null && gd.Count() != 0)
                    {
                        sp_add.Add(gd.First());
                        gids.Add(gd.First().Id);
                        ids.Add(gd.First().商品信息.所属供应商.用户ID);
                    }
                }
            }
            var countsp = sp_add.Count();
            if (countsp < 6)
            {
                var 补充商品1 = 商品管理.查询商品(0, 0, p_q.And(Query<商品>.EQ(o => o.审核数据.审核状态, 审核状态.审核通过)).And(Query<商品>.NotIn(o => o.Id, gids)), false, SortBy.Descending("销售信息.点击量"), false);
                foreach (var sp1 in 补充商品1)
                {
                    if (ids.Contains(sp1.商品信息.所属供应商.用户ID)) continue;
                    sp_add.Add(sp1);
                    ids.Add(sp1.商品信息.所属供应商.用户ID);
                    ++countsp;
                    if (countsp == 6) break;
                }

            }
            ViewData["新增推荐商品"] = (IEnumerable<商品>)sp_add;



            return PartialView();
        }
        public ActionResult doApplayNewUser()
        {
            try
            {
                var contact_name = Request.Params["contact_name"];
                var contact_tel = Request.Params["contact_tel"];
                var contact_unit = Request.Params["contact_unit"];

                if (!string.IsNullOrWhiteSpace(contact_tel))
                {
                    //站内消息
                    站内消息 Msg = new 站内消息();
                    对话消息 Talk = new 对话消息();

                    Msg.重要程度 = 重要程度.特别重要;
                    Msg.消息类型 = 消息类型.普通;

                    Msg.发起者.用户ID = 100000000001;
                    Talk.发言人.用户ID = 100000000001;
                    站内消息管理.添加站内消息(Msg, 100000000001, 100000000001);
                    Talk.消息主体.标题 = "新用户申请";
                    Talk.消息主体.内容 = "有一个新用户申请未处理，联系人：" + contact_name + "，联系电话：" + contact_tel + "，单位名称或番号：" + contact_unit + "。请及时与联系该用户。";
                    对话消息管理.添加对话消息(Talk, Msg);
                    return Content("1");
                }
                else
                {
                    return Content("联系电话必须填写！");
                }
            }
            catch
            {
                return Content("提交时发生错误，请与管理员联系！");
            }

        }
        public void SealWithText()
        {
            try
            {
                string txt = Request.QueryString["txt"];
                string basestr = Request.QueryString["base"];
                string id = Request.QueryString["id"];
                Regex reg = new Regex("[0-9,a-z,A-Z]{7,}");
                var t = new 圆形公章()
                {
                    透明度 = 180,
                    颜色 = Color.Red
                };
                if (reg.IsMatch(txt))
                {
                    t.透明度 = 180;
                    t.半径 = 60;
                    t.边框宽度 = 1;
                    t.中央五角星 = false;
                    t.颜色 = Color.Blue;
                }
                else
                {
                    if (long.Parse(id) == 10005 || long.Parse(id) == 10009 || long.Parse(id) == 10008 || long.Parse(id) == 10013)
                    {
                        t.自动计算签名字体大小 = false;
                        t.业务签名.字体大小 = 15.6F;
                        t.八一五角星 = true;
                    }
                }
                t.旋转角度 = (int)(60 - ((long.Parse(id) % 10000) * 17) % 120);
                t.单位签名.文字 = txt;
                t.业务签名.文字 = basestr;
                Image bitmap = t.绘制结果;
                HttpContext.Response.ContentType = "image/png";
                bitmap.Save(HttpContext.Response.OutputStream, ImageFormat.Png);
            }
            catch
            {
            }
        }
        public static bool GetQRCode(string strContent, MemoryStream ms)
        {
            ErrorCorrectionLevel Ecl = ErrorCorrectionLevel.M; //误差校正水平   
            string Content = strContent;//待编码内容  
            QuietZoneModules QuietZones = QuietZoneModules.Two;  //空白区域   
            int ModuleSize = 3;//大小  
            var encoder = new QrEncoder(Ecl);
            QrCode qr;
            if (encoder.TryEncode(Content, out qr))//对内容进行编码，并保存生成的矩阵  
            {
                var render = new GraphicsRenderer(new FixedModuleSize(ModuleSize, QuietZones));
                render.WriteToStream(qr.Matrix, ImageFormat.Png, ms);
            }
            else
            {
                return false;
            }
            return true;
        }
        public void OutPutQrCode()
        {
            string context = Request.Params["str"];
            string strCon = "http://www.go81.cn/%E9%A6%96%E9%A1%B5/SearchAcceptance?id=" + context;
            using (var ms = new MemoryStream())
            {
                string stringtest = strCon;
                GetQRCode(stringtest, ms);
                HttpContext.Response.ContentType = "image/Png";
                HttpContext.Response.OutputStream.Write(ms.GetBuffer(), 0, (int)ms.Length);
            }
        }
        #endregion
        #region  注释代码
        //public ActionResult IndexProductClass()
        //{
        //    string requesrid = Request.Params["classid"].ToString();
        //    try
        //    {
        //        string retstr = "";

        //        var lowerclass = 商品分类管理.查找子分类(long.Parse(requesrid));
        //        if (lowerclass.Any())
        //        {
        //            var l = new List<商品分类>();

        //            foreach (var item in lowerclass)
        //            {

        //                if (item.分类名.StartsWith("其他"))//将以其他开头的商品分类移到最后显示
        //                {
        //                    l.Add(item);
        //                    continue;
        //                }

        //                retstr += "<dl class='procate'>";
        //                retstr += "<dt>" + item.分类名 + "</dt>";
        //                var lowerclasslist = 商品分类管理.查找子分类(item.Id);
        //                if (lowerclass.Any())
        //                {
        //                    var g = new List<商品分类>();
        //                    foreach (var itemlist in lowerclasslist)
        //                    {
        //                        if (itemlist.分类名.StartsWith("其他"))//将以其他开头的商品分类移到最后显示
        //                        {
        //                            g.Add(itemlist);
        //                            continue;
        //                        }

        //                        retstr += "<dd><a target='_blank' href='/商品陈列/ProductList/" + itemlist.Id + "?parId=" + requesrid + "'>|" + itemlist.分类名 + "</a></dd>";
        //                    }
        //                    foreach (var itemlist in g)
        //                    {
        //                        retstr += "<dd><a target='_blank' href='/商品陈列/ProductList/" + itemlist.Id + "?parId=" + requesrid + "'>|" + itemlist.分类名 + "</a></dd>";
        //                    }
        //                }
        //                retstr += "</dl>";
        //                //retstr += "<li id='" + item.Id + "' onclick='GetSonClass(this);'>" + item.分类名 + "</li>";
        //            }
        //            foreach (var item in l)
        //            {
        //                retstr += "<dl class='procate'>";
        //                retstr += "<dt>" + item.分类名 + "</dt>";
        //                var lowerclasslist = 商品分类管理.查找子分类(item.Id);
        //                if (lowerclass.Any())
        //                {
        //                    var g = new List<商品分类>();
        //                    foreach (var itemlist in lowerclasslist)
        //                    {
        //                        if (itemlist.分类名.StartsWith("其他"))//将以其他开头的商品分类移到最后显示
        //                        {
        //                            g.Add(itemlist);
        //                            continue;
        //                        }

        //                        retstr += "<dd><a target='_blank' href='/商品陈列/ProductList/" + itemlist.Id + "?parId=" + requesrid + "'>|" + itemlist.分类名 + "</a></dd>";
        //                    }
        //                    foreach (var itemlist in g)
        //                    {
        //                        retstr += "<dd><a target='_blank' href='/商品陈列/ProductList/" + itemlist.Id + "?parId=" + requesrid + "'>|" + itemlist.分类名 + "</a></dd>";
        //                    }
        //                }
        //                retstr += "</dl>";
        //            }

        //            //context.Response.Write("<div class='gys_ztbsearch_class'>选择类别：</div><div class='gys_ztbsearch_class_list'> <a href='#'>全部</a> <a href='#'>资质1</a> <a href='#'>资质1</a> <a href='#'>资质1</a> <a href='#'>资质1</a> <a href='#'>资质1</a> <a href='#'>资质1</a> </div>");
        //        }
        //        else
        //        {
        //            retstr = "分类完善中";
        //        }
        //        return Content(retstr);

        //    }
        //    catch
        //    {
        //        return Content("分类完善中");
        //    }
        //}
        #endregion
        /// <summary>
        /// 广告点击统计
        /// </summary>
        public void clickStatistics()
        {
            try
            {
                //广告位置
                var position = Request.Params["position"];
                //广告类型：1商品 2贵阳市
                var type = Request.Params["type"];
                //商品ID
                var id = Request.Params["id"];
                //供应商ID
                var gysid = Request.Params["gysid"];
                广告点击统计 m = new 广告点击统计();
                m.广告位置 = position;
                m.点击IP = FileHelper.StrHelp.GetIPAddress();

                var uid = HttpContext.检查登录();
                m.点击用户.用户ID = uid;
                m.游客点击 = false;
                if (uid == -1)
                {
                    m.游客点击 = true;
                }
                m.广告所属供应商.用户ID = long.Parse(gysid);
                if (type == "1")
                {
                    m.广告类型 = 广告类型.商品广告;
                    m.商品链接.商品ID = long.Parse(id);
                }
                else if (type == "2")
                {
                    m.广告类型 = 广告类型.供应商广告;
                }
#if INTRANET
                m.内网访问 = true;
#else
                m.内网访问 = false;
#endif
                广告点击统计管理.添加广告点击统计(m);
            }
            catch
            {

            }
        }
    }
}
