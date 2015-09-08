using Go81WebApp.Models.数据模型;
using Go81WebApp.Models.数据模型.用户数据模型;
using Microsoft.Ajax.Utilities;
using MongoDB;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Go81WebApp.Models.管理器.抽选管理
{
#if INTRANET
    public class 专家抽选管理
    {
        public static List<专家> 抽选专家(int count, IMongoQuery conditions, IEnumerable<long> selected, IEnumerable<long> avoid, string 所属单位)
        {
            var ret = new List<专家>(count);
            var rnd = new Random();

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
            if (null != conditions)
                conditions = Query.And(conditions, q);
            else
                conditions = q;

            if (null != selected && 0 != selected.Count())
            {
                q = Query.NotIn("_id", new BsonArray(selected));
                conditions = Query.And(conditions, q);
            }
            if (null != avoid && 0 != avoid.Count())
            {
                q = Query.NotIn("_id", new BsonArray(avoid));
                conditions = Query.And(conditions, q);
            }
            int total = (int)Mongo.计数<专家>(0, 0, conditions);
            if (total < count) return new List<专家>();
            var r = Mongo.查询<专家>(0, 0, conditions);
            var ns = new HashSet<int>();
            for (int i = 0; i < count; i++)
            {
                int n;
                do
                {
                    n = rnd.Next(total);
                } while (ns.Contains(n));
                ret.Add(r.ElementAt(n));
                ns.Add(n);
            }
            return ret;
        }

        public static IEnumerable<BsonDocument> 列表专家(int count, IMongoFields fields, IMongoQuery conditions, IEnumerable<long> selected, IEnumerable<long> avoid)
        {
            var ret = new List<BsonDocument>(count);
            var rnd = new Random();
            //var q = Query<专家>.LT(o => o.上次出席评标时间, DateTime.Now.Date.AddDays(-122));
            var q = Query.Null;
            if (null != conditions)
                conditions = Query.And(conditions, q);
            else
                conditions = q;

            if (null != selected && 0 != selected.Count())
            {
                q = Query.NotIn("_id", new BsonArray(selected));
                conditions = Query.And(conditions, q);
            }
            if (null != avoid && 0 != avoid.Count())
            {
                q = Query.NotIn("_id", new BsonArray(avoid));
                conditions = Query.And(conditions, q);
            }
            int total = (int)Mongo.计数<专家>(0, 0, conditions);
            if (total < count) return new List<BsonDocument>();
            var r = Mongo.列表<专家>(0, 0, fields, conditions);
            var ns = new HashSet<int>();
            for (int i = 0; i < count; i++)
            {
                int n;
                do
                {
                    n = rnd.Next(total);
                } while (ns.Contains(n));
                ret.Add(r.ElementAt(n));
                ns.Add(n);
            }
            return ret;
        }

        public static bool 添加专家抽选历史(专家抽选记录 record)
        {
            return Mongo.添加(record);
        }
        public static IEnumerable<专家抽选记录> 查询专家抽选记录(int skip, int limit, IMongoQuery conditions = null)
        {
            return Mongo.查询<专家抽选记录>(skip, limit, conditions);
        }
        public static 专家抽选记录 查找专家抽选记录(long id)
        {
            return Mongo.查找<专家抽选记录>(id);
        }
        public static void 批准专家抽选申请(long contentID, long 批准人员ID, 申请抽选状态 申请状态)
        {
            Mongo.更新<专家抽选记录>(Query.EQ("_id", contentID), Update.Combine(
                Update<专家抽选记录>.Set(o => o.批准人.用户ID, 批准人员ID),
                Update<专家抽选记录>.Set(o => o.批准时间, DateTime.Now),
                Update<专家抽选记录>.Set(o => o.申请抽选状态, 申请状态)
                )
            );
        }
        public static bool 更新专家抽选记录(专家抽选记录 p, bool updateModifiedTime = true)
        {
            return Mongo.更新(p, updateModifiedTime);
        }

        public static long 计数分类下专家数(int classtype, string classname,string 所属单位)
        {
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
            var q1 = classtype == 1
                ? Query<专家>.Where(o => o.可参评物资类别列表.Any(oc => oc.一级分类 == classname))
                : classtype == 2 ? Query<专家>.Where(o => o.可参评物资类别列表.Any(oc => oc.二级分类.Contains(classname)))
                : Query<专家>.Where(o => o.可参评物资类别列表.Any(oc => oc.一级分类 == "（地方政府专家库评审专业目录）"));

            return Mongo.计数<专家>(0, 0, q.And(q1));
        }
        public static long 计数分类下供应商数(int classtype, string classname)
        {
            var q = Query<供应商>.EQ(o => o.审核数据.审核状态, 审核状态.审核通过);
            var q1 = classtype == 1
                ? Query<供应商>.Where(o => o.可提供产品类别列表.Any(oc => oc.一级分类 == classname))
                : Query<供应商>.Where(o => o.可提供产品类别列表.Any(oc => oc.二级分类.Contains(classname)));

            return Mongo.计数<供应商>(0, 0, q.And(q1));
        }
    }
#endif
}
