using Go81WebApp.Models.数据模型;
using Go81WebApp.Models.数据模型.用户数据模型;
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
    public class 供应商抽选管理
    {
        public static List<供应商> 抽选供应商(int count, IMongoQuery conditions, IEnumerable<long> selected, IEnumerable<long> avoid)
        {
            var ret = new List<供应商>(count);
            var rnd = new Random();

            conditions = conditions.And(Query<供应商>.EQ(o => o.审核数据.审核状态, 审核状态.审核通过));
            if (null != selected && 0 != selected.Count())
            {
                var q = Query.NotIn("_id", new BsonArray(selected));
                conditions = Query.And(conditions, q);
            }
            if (null != avoid && 0 != avoid.Count())
            {
                var q = Query.NotIn("_id", new BsonArray(avoid));
                conditions = Query.And(conditions, q);
            }
            int total = (int)Mongo.计数<供应商>(0, 0, conditions);
            if(total < count) return new List<供应商>();
            var r = Mongo.查询<供应商>(0, 0, conditions);
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
        public static bool 添加供应商抽选历史(供应商抽选记录 record)
        {
            return Mongo.添加(record);
        }
        public static IEnumerable<供应商抽选记录> 查询供应商抽选历史记录(int skip, int limit, IMongoQuery conditions = null)
        {
            return Mongo.查询<供应商抽选记录>(skip, limit, conditions);
        }
        public static 供应商抽选记录 查找供应商抽选历史记录(long id)
        {
            return Mongo.查找<供应商抽选记录>(id);
        }
        public static void 批准供应商抽选申请(long contentID, long 批准人员ID, 申请抽选状态 申请状态)
        {
            Mongo.更新<供应商抽选记录>(Query.EQ("_id", contentID), Update.Combine(
                Update<供应商抽选记录>.Set(o => o.批准时间, DateTime.Now),
                Update<供应商抽选记录>.Set(o=>o.批准人.用户ID, 批准人员ID),
                Update<供应商抽选记录>.Set(o=>o.申请抽选状态, 申请状态)
                )
            );
        }
        public static bool 更新供应商抽选历史记录(供应商抽选记录 p)
        {
            return Mongo.更新(p);
        }
    }
#endif
}
