using Go81WebApp.Models.管理器;
using Go81WebApp.Models.管理器.抽选管理;
using Go81WebApp.Models.数据模型;
using Go81WebApp.Models.数据模型.商品数据模型;
using Go81WebApp.Models.数据模型.消息数据模型;
using Go81WebApp.Models.数据模型.用户数据模型;
using MongoDB;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using MongoDB.Driver.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using NPOI.SS.Formula.Functions;

namespace Go81WebApp.ApiControllers
{
    public class ExpertController : ApiController
    {
        public class typeclas
        {
            public string firsttype { get; set; }
            public List<string> secondtype { get; set; }
        }

        /// <summary>
        /// Api登录model
        /// </summary>
        public class Api登录信息 : 基本数据模型
        {
            public string tokenId { get; set; }
            public 用户链接<用户基本数据> 登录用户链接 { get; set; }
            [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
            public DateTime 登录时间 { get; set; }
            [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
            public DateTime 上次登录时间 { get; set; }
            public Api登录信息()
            {
                this.登录用户链接 = new 用户链接<用户基本数据>();
            }
        }
        
        /// <summary>
        /// 检查是否登录
        /// </summary>
        /// <param name="id">传入的guid</param>
        /// <returns>已登录返回用户ID，未登录返回-1</returns>
        private long 获取登录Id(string tokenId)
        {
            if (string.IsNullOrWhiteSpace(tokenId)) return -1;
            var login = Api登录管理.查询Api登录信息(0, 0,
                Query<Api登录信息>.Where(o => o.tokenId == tokenId && o.上次登录时间 > DateTime.Now.AddHours(-2)));
            if (!login.Any()) return -1;
            var loginInfo = login.First();
            loginInfo.上次登录时间 = DateTime.Now;
            Api登录管理.更新Api登录信息(loginInfo);
            return loginInfo.登录用户链接.用户ID;
        }
       
        /// <summary>
        /// 登陆
        /// </summary>
        /// <param name="LoginName">用户名</param>
        /// <param name="LoginPwd">密码</param>
        /// <returns>登陆结果</returns>
        [System.Web.Mvc.HttpPost]
        public string PostLogin()
        {
            var prs = HttpContext.Current.Request.Params;
            var LoginName = prs["LoginName"];
            var LoginPwd = prs["LoginPwd"];
            if (string.IsNullOrWhiteSpace(LoginName) || string.IsNullOrWhiteSpace(LoginPwd)) return "marchError";
            //HttpContextBase context = (HttpContextBase)Request.Properties["MS_HttpContext"];//获取传统context
            var u = Api登录管理.登录(LoginName, LoginPwd, false);
            if (null != u)
            {
                if (!(u is 单位用户))
                {
                    return "typeError";
                }
                //将登录信息写入数据库
                var dateNow = DateTime.Now;
                var loginMessage = new Api登录信息();
                loginMessage.tokenId = Guid.NewGuid().ToString();
                loginMessage.登录时间 = dateNow;
                loginMessage.上次登录时间 = dateNow;
                loginMessage.登录用户链接.用户ID = u.Id;
                Api登录管理.添加Api登录信息(loginMessage);
                //将登录信息写入数据库
                return loginMessage.tokenId;
            }
            return "marchError";
        }

        /// <summary>
        /// 申请抽取页面准备数据
        /// </summary>
        /// <returns>所需数据的json包</returns>
        /// Part_Expert_Applay
        [System.Web.Mvc.HttpPost]
        public object Part_Expert_Applay()
        {
            var prs = HttpContext.Current.Request.Params;
            var tokenId = prs["tokenId"];
            //先检查是否登录有效
            var userId = 获取登录Id(tokenId);
            if (userId == -1) return null;

            //HttpContextBase context = (HttpContextBase)Request.Properties["MS_HttpContext"];//获取传统context
            //var goodType = new Dictionary<string, List<string>>();
            //var goodTypelist = 商品分类管理.查找子分类();
            //foreach (var goodTypeitem in goodTypelist)
            //{
            //    goodType.Add(goodTypeitem.分类名,goodTypeitem.子分类.Select(o=>o.分类名).ToList());
            //}
            var goodType = new List<typeclas>();
            var goodTypelist = 商品分类管理.查找子分类();
            foreach (var goodTypeitem in goodTypelist)
            {
                typeclas tc = new typeclas();
                tc.firsttype = goodTypeitem.分类名;
                tc.secondtype = goodTypeitem.子分类.Select(o => o.分类名).ToList();
                goodType.Add(tc);
            }

            //var goodType = 商品分类管理.查找子分类();
            var 经办人 = 用户管理.查找用户<单位用户>(userId).联系方式.联系人;
                //(Api登录管理.查询Api登录信息(0, 0, Query<Api登录信息>.Where(o => o.tokenId == tokenId)).First().登录用户链接.用户数据 as 单位用户)
                //    .联系方式.联系人;
            //context.获取当前用户<单位用户>().联系方式.联系人;

            var t = typeof(专业技术职称);
            var vs = Enum.GetValues(t);
            var d = new List<string>();
            foreach (var v in vs)
            {
                d.Add(Enum.GetName(t, v));
            }
            var 专业技术职称 = d;

            t = typeof(学位);
            vs = Enum.GetValues(t);
            d = new List<string>();
            foreach (var v in vs)
            {
                d.Add(Enum.GetName(t, v));
            }
            var 最高学位 = d;

            t = typeof(学历);
            vs = Enum.GetValues(t);
            d = new List<string>();
            foreach (var v in vs)
            {
                d.Add(Enum.GetName(t, v));
            }
            var 最高学历 = d;

            t = typeof(专家类型);
            vs = Enum.GetValues(t);
            d = new List<string>();
            foreach (var v in vs)
            {
                d.Add(Enum.GetName(t, v));
            }
            var 专家类型 = d;

            t = typeof(专家类别);
            vs = Enum.GetValues(t);
            d = new List<string>();
            foreach (var v in vs)
            {
                d.Add(Enum.GetName(t, v));
            }
            var 专家类别 = d;

            t = typeof(专家级别);
            vs = Enum.GetValues(t);
            d = new List<string>();
            foreach (var v in vs)
            {
                d.Add(Enum.GetName(t, v));
            }
            var 专家级别 = d;

            var 专家特殊类别 = 专家可评标专业.非商品分类评审专业;
            var 所属单位 = 用户管理.查找用户<单位用户>(userId).单位信息.所属单位;
            所属单位 = 所属单位 == null ? "" : 所属单位;
            return new { goodType = goodType, 经办人 = 经办人, 专业技术职称 = 专业技术职称, 最高学位 = 最高学位, 最高学历 = 最高学历, 专家类型 = 专家类型, 专家类别 = 专家类别, 专家级别 = 专家级别, 专家特殊类别 = 专家特殊类别, 所属单位 = 所属单位 };
        }

        /// <summary>
        /// 获取回避名单
        /// </summary>
        /// <param name="page">页码</param>
        /// <param name="name">回避姓名</param>
        /// <returns>匹配的专家，当前页码，页码总数的Json数据</returns>
        [System.Web.Mvc.HttpPost]
        public object Part_Expert_ApplayOutList()
        {
            var prs = HttpContext.Current.Request.Params;
            var tokenId = prs["tokenId"];
            //先检查是否登录有效
            var userId = 获取登录Id(tokenId);
            if (userId == -1) return null;

            //var page = 1;
            //int.TryParse(prs["page"], out page);
            var name = prs["name"];

            var retlist = new List<outlistmodel>();
            var q = Query<专家>.EQ(o => o.审核数据.审核状态, 审核状态.审核通过);
            if (!string.IsNullOrWhiteSpace(name))
            {
                q = q.And(Query<专家>.EQ(o => o.身份信息.姓名, name));
            }
            //var listcount = 用户管理.列表用户<专家>(0, 0, Fields<专家>.Include(o => o.Id, o => o.身份信息.性别, o => o.工作经历信息.工作单位, o => o.身份信息.姓名, o => o.学历信息.取得现技术职称时间), q).Count();
            //int maxpage = Math.Max((listcount + pagesize - 1) / pagesize, 1);
            //try
            //{
            //    if (page > maxpage || page < 1)
            //    {
            //        page = 1;
            //    }
            //}
            //catch
            //{
            //    page = 1;
            //}
            var list = 用户管理.列表用户<专家>(0, 0, Fields<专家>.Include(o => o.Id, o => o.身份信息.性别, o => o.工作经历信息.工作单位, o => o.身份信息.姓名, o => o.学历信息.取得现技术职称时间), q);
            foreach (var l in list)
            {
                var om = new outlistmodel
                {
                    id = long.Parse(l["_id"].ToString()),
                    性别 = (性别)int.Parse(l["身份信息"]["性别"].ToString()),
                    工作单位 = l["工作经历信息"]["工作单位"].ToString(),
                    姓名 = l["身份信息"]["姓名"].ToString(),
                    取得现技术职称时间 = Convert.ToDateTime(l["学历信息"]["取得现技术职称时间"].ToString())
                };
                retlist.Add(om);
            }
            return new {data = retlist};
        }
#if INTRANET
        /// <summary>
        /// 计数分类下专家数
        /// </summary>
        /// <param name="classtype">分类类型1：一级分类  2：二级分类</param>
        /// <param name="classname">分类名称</param>
        /// <param name="所属单位"></param>
        /// <returns>该分类下的专家个数</returns>
        [System.Web.Mvc.HttpPost]
        public object 计数分类下专家数()
        {
            var prs = HttpContext.Current.Request.Params;
            var tokenId = prs["tokenId"];

            //先检查是否登录有效
            var userId = 获取登录Id(tokenId);
            if (userId == -1) return null;

            var classtype = int.Parse(prs["classtype"]);
            var classname = prs["classname"];
            var 所属单位 = prs["所属单位"];

            return new {data = 专家抽选管理.计数分类下专家数(classtype, classname, 所属单位)};
        }

        /// <summary>
        /// 查询可用评审专家个数
        /// </summary>
        /// <param name="value">提交的参数</param>
        /// <returns>符合条件的可用评审专家的个数</returns>
        [System.Web.Mvc.HttpPost]
        public object SearchByCondition_Temp()
        {
            //HttpContextBase context = (HttpContextBase)Request.Properties["MS_HttpContext"];//获取传统context
            var request = HttpContext.Current.Request;//定义传统request对象

            var tokenId = request.Params["tokenId"];
            //先检查是否登录有效
            var userId = 获取登录Id(tokenId);
            if (userId == -1) return null;

            try
            {
                var kpbwzlb = request.Params["kpbwzlb"];
                var province = request.Params["province"];
                var city = request.Params["city"];
                var area = request.Params["area"];
                var zyjszc = request.Params["zyjszc"];
                var leibie = request.Params["leibie"];
                var jibie = request.Params["jibie"];
                var leixing = request.Params["leixing"];
                var xueli = request.Params["xueli"];
                var xuewei = request.Params["xuewei"];

                var 所属单位 = 用户管理.查找用户<单位用户>(userId).单位信息.所属单位;

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
                var outlist = request.Params["outlist"].Split(new[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
                //var outlistarr = outlist.Split(new[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
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
                return new {data = listcount};
            }
            catch
            {
                return new {data = 0};
            }
        }

        /// <summary>
        /// 提交抽取申请信息
        /// </summary>
        /// <param name="value">提交的表单</param>
        /// <returns>提交数据状态</returns>
        [System.Web.Mvc.HttpPost]
        public object Expert_Applay([FromBody]string value)
        {
            HttpContextBase context = (HttpContextBase)Request.Properties["MS_HttpContext"];//获取传统context
            HttpRequestBase request = context.Request;//定义传统request对象

            var tokenId = request.Params["tokenId"];
            //先检查是否登录有效
            var userId = 获取登录Id(tokenId);
            if (userId == -1) return null;

            专家抽选记录 model = new 专家抽选记录
            {
                申请时间 = DateTime.Now,
                项目编号 = request.Form["pro_num"],
                项目名称 = request.Form["pro_name"],
                评标时间 = Convert.ToDateTime(request.Form["pro_time"]),
                总计预定抽选专家数 = int.Parse(request.Form["预定抽选总数"]),
                操作人姓名 = request.Form["prooperate_name"],
                操作人电话 = request.Form["prooperate_tel"]
            };
            model.经办人.用户ID = userId;
            model.申请抽选状态 = 申请抽选状态.已提交待批准;

            //屏蔽列表
            var outlist = request.Form["outlistcontent"];
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

            int count = int.Parse(request.Form["条件总数"]);
            var condition = new List<专家抽选记录._专家抽选条件>();
            for (int i = 1; i <= count; i++)
            {
                var t_condition = new 专家抽选记录._专家抽选条件();
                t_condition.需要专家数量 = int.Parse(request.Form["预定抽选个数__" + i]);

                //专家类别
                var tempparmlist = request.Form["专家类别__" + i];
                if (!string.IsNullOrWhiteSpace(tempparmlist))
                {
                    foreach (var temp in tempparmlist.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        t_condition.专家类别.Add((专家类别)(int.Parse(temp)));
                    }
                }

                //专家级别
                tempparmlist = request.Form["专家级别__" + i];
                if (!string.IsNullOrWhiteSpace(tempparmlist))
                {
                    foreach (var temp in tempparmlist.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        t_condition.专家级别.Add((专家级别)(int.Parse(temp)));
                    }
                }

                //专家类型
                tempparmlist = request.Form["专家类型__" + i];
                if (!string.IsNullOrWhiteSpace(tempparmlist))
                {
                    foreach (var temp in tempparmlist.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        t_condition.专家类型.Add((专家类型)(int.Parse(temp)));
                    }
                }
                //最高学历
                tempparmlist = request.Form["最高学历__" + i];
                if (!string.IsNullOrWhiteSpace(tempparmlist))
                {
                    foreach (var temp in tempparmlist.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        t_condition.学历要求.Add((学历)(int.Parse(temp)));
                    }
                }
                //最高学位
                tempparmlist = request.Form["最高学位__" + i];
                if (!string.IsNullOrWhiteSpace(tempparmlist))
                {
                    foreach (var temp in tempparmlist.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        t_condition.学位要求.Add((学位)(int.Parse(temp)));
                    }
                }
                //专业技术职称
                tempparmlist = request.Form["专业技术职称__" + i];
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
                tempparmlist = request.Form["可评标物质类别匹配模式__" + i];
                if (!string.IsNullOrWhiteSpace(tempparmlist) && tempparmlist == "1")
                {
                    t_condition.模糊查询 = request.Form["可评标物质类别__" + i];
                }
                else
                {
                    //可评标物质类别
                    tempparmlist = request.Form["可评标物质类别__" + i];
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
                t_condition.描述 = request.Form["抽选描述__" + i];
                //所在地域
                t_condition.所在地区.省份 = request.Form["省份__" + i];
                t_condition.所在地区.城市 = request.Form["城市__" + i];
                t_condition.所在地区.区县 = request.Form["区县__" + i];

                condition.Add(t_condition);
            }
            model.专家抽选条件 = condition;
            专家抽选管理.添加专家抽选历史(model);

            //站内消息
            站内消息 Msg = new 站内消息();
            对话消息 Talk = new 对话消息();

            Msg.重要程度 = 重要程度.特别重要;
            Msg.消息类型 = 消息类型.普通;
            var u_id = userId;

            Msg.发起者.用户ID = u_id;
            Talk.发言人.用户ID = u_id;
            站内消息管理.添加站内消息(Msg, u_id, 10002);
            Talk.消息主体.标题 = "待审核抽取评审专家";
            Talk.消息主体.内容 = "有一条待审核的申请抽取评审专家未处理，<a style='color:red;text-decoration:underline;' href='/专家抽选/Expert_ApplayAuditList'>点击这里进行审核</a>";
            对话消息管理.添加对话消息(Talk, Msg);


            return new {data = true};
        }

        /// <summary>
        /// 抽取专家（每次只抽取一个）
        /// </summary>
        /// <param name="value">传入的参数</param>
        /// <returns>抽取出的满足条件的专家</returns>
        [System.Web.Mvc.HttpPost]
        public object SearchByCondition()
        {
            //HttpContextBase context = (HttpContextBase)Request.Properties["MS_HttpContext"];//获取传统context
            var request = HttpContext.Current.Request;//定义传统request对象

            var tokenId = request.Params["tokenId"];
            //先检查是否登录有效
            var userId = 获取登录Id(tokenId);
            if (userId == -1) return null;

            try
            {
                var q = Query.Null;

                var id = long.Parse(request.Params["id"]);
                var model = 专家抽选管理.查找专家抽选记录(id);

                //当前抽取的条件数（第n个条件）
                var time = int.Parse(request.Params["time"]);
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
                var outlist = request.Params["outlist"].Split(new[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
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
                            qc1 = Query<专家>.Where(o => o.可参评物资类别列表.Any(oc => oc.二级分类.ContainsAny(condition.可评标产品类别[0].二级分类)));
                            list = 专家抽选管理.抽选专家(1, q_final.And(qc1), out_id_page, out_id, 用户管理.查找用户<单位用户>(userId).单位信息.所属单位);
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
                            list = 专家抽选管理.抽选专家(1, q_final.And(qc2), out_id_page, out_id, 用户管理.查找用户<单位用户>(userId).单位信息.所属单位);
                            //二级不够，再查一级
                            if (!list.Any() && condition.可评标产品类别[0].二级分类可用专家不足返回一级分类)
                            {
                                list = 专家抽选管理.抽选专家(1, q_final.And(qc1), out_id_page, out_id, 用户管理.查找用户<单位用户>(userId).单位信息.所属单位);
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

                    list = 专家抽选管理.抽选专家(1, q_final.And(q_march), out_id_page, out_id, 用户管理.查找用户<单位用户>(userId).单位信息.所属单位);
                }
                return new {data = list};
            }
            catch
            {
                return new { data = new List<专家>() };
            }
        }

        /// <summary>
        /// 存储抽取信息
        /// </summary>
        /// <param name="value">传入的参数</param>
        /// <returns>存储的状态</returns>
        [System.Web.Mvc.HttpPost]
        public object Expert_Choose_Select()
        {
            //HttpContextBase context = (HttpContextBase)Request.Properties["MS_HttpContext"];//获取传统context
            var request = HttpContext.Current.Request;//定义传统request对象

            var tokenId = request.Params["tokenId"];
            //先检查是否登录有效
            var userId = 获取登录Id(tokenId);
            if (userId == -1) return null;

            try
            {
                var time = request.Params["time"];
                var parm_str = request.Params["parm_str"];

                var model = 专家抽选管理.查找专家抽选记录(long.Parse(request.Params["id"]));
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
                        var 所属单位 = 用户管理.查找用户<单位用户>(userId).单位信息.所属单位;
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

                return new {data = true};
            }
            catch
            {
                return new {data = false};
            }

        }

        /// <summary>
        /// 抽取页面所需数据
        /// </summary>
        /// <param name="id">抽取ID</param>
        /// <returns>抽取页面所需json数据</returns>
        [System.Web.Mvc.HttpPost]
        public object Part_Expert_Choose()
        {
            var prs = HttpContext.Current.Request.Params;
            var tokenId = prs["tokenId"];

            //先检查是否登录有效
            var userId = 获取登录Id(tokenId);
            if (userId == -1) return null;

            专家抽选记录 model = new 专家抽选记录();
            var str = "";
            try
            {
                var id = long.Parse(prs["id"]);
                model = 专家抽选管理.查找专家抽选记录(id);
                foreach (var m in model.专家抽选条件)
                {
                    str += m.需要专家数量 + ",";
                }
                str = str.Substring(0, str.Length - 1);
            }
            catch
            {

            }
            return new {data = model, str = str};
        }

        /// <summary>
        /// 详情页面所需数据
        /// </summary>
        /// <param name="id">抽取ID</param>
        /// <returns>详情页面所需数据</returns>
        [System.Web.Mvc.HttpPost]
        public object Part_Expert_ApplayAudit()
        {
            var prs = HttpContext.Current.Request.Params;
            var tokenId = prs["tokenId"];

            //先检查是否登录有效
            var userId = 获取登录Id(tokenId);
            if (userId == -1) return null;

            专家抽选记录 model = new 专家抽选记录();
            try
            {
                var id = long.Parse(prs["id"]);
                model = 专家抽选管理.查找专家抽选记录(id);
            }
            catch
            {

            }
            return new {data = model};
        }

        /// <summary>
        /// 对评审专家进行评分
        /// </summary>
        /// <param name="id">抽取ID</param>
        /// <param name="parmstr">传入参数</param>
        /// <returns>存储评分状态</returns>
        [System.Web.Mvc.HttpPost]
        public object Expert_Scoring()
        {
            var prs = HttpContext.Current.Request.Params;
            var tokenId = prs["tokenId"];
            //先检查是否登录有效
            var userId = 获取登录Id(tokenId);
            if (userId == -1) return null;

            try
            {
                var id = long.Parse(prs["id"]);
                var parmstr = prs["parmstr"];
                专家抽选记录 model = 专家抽选管理.查找专家抽选记录(id);
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
                return new {data = false};
            }
            return new {data = true};
        }

#endif

        [System.Web.Mvc.HttpPost]
        public object getproduct()
        {
            var s = 商品管理.查询商品(5000, 5);
            return new {data=s};
        }

        public class outlistmodel
        {
            public long id { get; set; }
            public 性别 性别 { get; set; }
            public string 工作单位 { get; set; }
            public string 姓名 { get; set; }
            public DateTime 取得现技术职称时间 { get; set; }
        }














        [System.Web.Mvc.HttpPost]
        public IEnumerable<BsonDocument> getplist()
        {
            var sp = 商品管理.列表商品(0, 3, Fields<商品>.Include(o => o.Id, o => o.商品信息.商品名, o => o.销售信息.价格));

            //JsonResult json = new JsonResult() { Data = sp };

            return sp;
        }
        [System.Web.Mvc.HttpPost]
        public IList<thisclass> getaaa()
        {
            IList<thisclass> tl = new List<thisclass>();
            thisclass t = new thisclass()
            {
                id = 1,
                name = null,
                price = 1
            };
            tl.Add(t);
            return tl;
        }


        public class thisclass
        {
            public long id { get; set; }
            public string name { get; set; }
            public decimal price { get; set; }
        }
        [System.Web.Mvc.HttpGet]
        public IEnumerable<thisclass> getplistid(int id)
        {
            IList<thisclass> t = new List<thisclass>();
            var sp = 商品管理.列表商品(0, 3, Fields<商品>.Include(o => o.Id, o => o.商品信息.商品名, o => o.销售信息.价格));
            foreach (var s in sp)
            {
                thisclass ti = new thisclass();
                ti.id = long.Parse(s["_id"].ToString());
                ti.name = s["商品信息"]["商品名"].ToString();
                ti.price = decimal.Parse(s["销售信息"]["价格"].ToString());
                t.Add(ti);
            }
            return t;
        }
    }
}
