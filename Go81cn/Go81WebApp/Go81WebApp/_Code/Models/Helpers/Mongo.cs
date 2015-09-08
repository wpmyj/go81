using Go81WebApp.Models.数据模型;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using System;
using System.Collections.Generic;
using System.Configuration;

namespace MongoDB
{
    public static class MongoHelpers
    {
        public static IMongoQuery And(this IMongoQuery q0, IMongoQuery q)
        {
            return null != q0
                ? Query.And(q0, q)
                : q
                ;
        }
        public static IMongoQuery Or(this IMongoQuery q0, IMongoQuery q)
        {
            return null != q0
                ? Query.Or(q0, q)
                : q
                ;
        }
        public static IMongoUpdate Combine(this IMongoUpdate u0, IMongoUpdate u)
        {
            return null != u0
                ? Update.Combine(u0, u)
                : u
                ;
        }
    }
    public static class Mongo
    {
        private static MongoClient mc { get; set; }
        private static MongoServer ms { get; set; }
        private static MongoDatabase db { get; set; }
        private static MongoCollection<BsonDocument> aic { get; set; }
        private static Dictionary<string, MongoCollection> colls { get; set; }
        static Mongo()
        {
            mc = new MongoClient(string.Format("mongodb://{0}:{1}@{2}:{3}/{4}",
                ConfigurationManager.AppSettings["Mongo账号"], ConfigurationManager.AppSettings["Mongo密码"],
                ConfigurationManager.AppSettings["Mongo服务器IP"], ConfigurationManager.AppSettings["Mongo服务器端口"],
                ConfigurationManager.AppSettings["Mongo数据库名"]));
            ms = mc.GetServer();
            db = ms.GetDatabase(ConfigurationManager.AppSettings["Mongo数据库名"]);
            aic = db.GetCollection("_autoInc");
            colls = new Dictionary<string, MongoCollection>();
        }
        public static void CollectionInitialize<T>(long idBase = 0, params string[] uniqueIndexNames)
        {
            var tn = typeof(T).Name;
            if (1 != aic.Count(new CountArgs { Query = Query.EQ("_id", tn) }))
            {
                aic.Insert(new { _id = tn, current_id = idBase });
            }
            var c = Coll<T>();
            foreach (var uin in uniqueIndexNames)
            {
                if (!c.IndexExists(uin))
                {
                    c.CreateIndex(IndexKeys.Ascending(uin), IndexOptions.SetUnique(true));
                }
            }
        }
        public static long NextId<T>(bool updateDB = true)
        {
            var r = aic.FindAndModify(new FindAndModifyArgs
            {
                Query = Query.EQ("_id", typeof(T).Name),
                Update = Update.Inc("current_id", 1L),
                Upsert = updateDB,
                VersionReturned = FindAndModifyDocumentVersion.Modified,
            });
            return null != r.ModifiedDocument ? r.ModifiedDocument["current_id"].AsInt64 : 0;
        }
        public static long NextId<T>(string prefix, bool updateDB = true)
        {
            var r = aic.FindAndModify(new FindAndModifyArgs
            {
                Query = Query.EQ("_id", typeof(T).Name+"."+prefix),
                Update = Update.Inc("current_id", 1L),
                Upsert = updateDB,
                VersionReturned = FindAndModifyDocumentVersion.Modified,
            });
            return null != r.ModifiedDocument ? r.ModifiedDocument["current_id"].AsInt64 : 0;
        }
        public static long NextIdSetTo<T>(long id)
        {
            var r = aic.FindAndModify(new FindAndModifyArgs
            {
                Query = Query.EQ("_id", typeof(T).Name),
                Update = Update.Set("current_id", id),
                Upsert = true,
                VersionReturned = FindAndModifyDocumentVersion.Original,
            });
            return null != r.ModifiedDocument ? r.ModifiedDocument["current_id"].AsInt64 : -1;
        }
        public static MongoCollection Coll(string collName)
        {
            lock (colls)
            {
                return colls.ContainsKey(collName)
                    ? colls[collName]
                    : null
                    ;
            }
        }
        public static MongoCollection<T> Coll<T>()
        {
            lock (colls)
            {
                var tn = typeof(T).Name;
                if (colls.ContainsKey(tn)) return (MongoCollection<T>)colls[tn];
                var c = db.GetCollection<T>(tn);
                colls.Add(tn, c);
                return c;
            }
        }
        public static bool 添加<T>(T item) where T : 基本数据模型
        {
            if (-1 == item.Id) item.Id = NextId<T>();
            item.基本数据.添加时间 = item.基本数据.修改时间 = DateTime.Now;
            return Coll<T>().Insert(item).Ok;
        }
        public static bool 屏蔽<T>(long Id)
        {
            return Coll<T>().Update(Query.EQ("_id", Id), Update.Set("基本数据.已屏蔽", true).Set("基本数据.修改时间", DateTime.Now)).UpdatedExisting;
        }
        public static bool 屏蔽解除<T>(long Id)
        {
            return Coll<T>().Update(Query.EQ("_id", Id), Update.Set("基本数据.已屏蔽", false).Set("基本数据.修改时间", DateTime.Now)).UpdatedExisting;
        }
        public static bool 删除<T>(long Id)
        {
            return Coll<T>().Update(Query.EQ("_id", Id), Update.Set("基本数据.已删除", true).Set("基本数据.修改时间", DateTime.Now)).UpdatedExisting;
        }
        public static bool 删除恢复<T>(long Id)
        {
            return Coll<T>().Update(Query.EQ("_id", Id), Update.Set("基本数据.已删除", false).Set("基本数据.修改时间", DateTime.Now)).UpdatedExisting;
        }
        public static bool 物理删除<T>(long id)
        {
            return 1 == Coll<T>().Remove(Query.EQ("_id", id)).DocumentsAffected;
        }
        public static T 查找<T>(long id, bool includeDisabled = true, bool includeDeleted = false)
        {
            //GC.Collect();
            var fa = new FindOneArgs
            {
                Query = PrepareQuery(Query.EQ("_id", id), includeDisabled, includeDeleted)
            };
            return Coll<T>().FindOneAs<T>(fa);
        }
        public static bool 更新<T>(T item, bool updateModifiedTime = true) where T : 基本数据模型
        {
            if (updateModifiedTime) item.基本数据.修改时间 = DateTime.Now;
            return Coll<T>().Update(Query.EQ("_id", item.Id), Update.Replace(item)).UpdatedExisting;
        }
        public static long 更新<T>(IMongoQuery conditions, IMongoUpdate update, bool includeDisabled = true, bool includeDeleted = false, bool updateModifiedTime = true)
        {
            return Coll<T>().Update(
                PrepareQuery(conditions, includeDisabled, includeDeleted),
                PrepareUpdate(update, updateModifiedTime),
                UpdateFlags.Multi
                ).DocumentsAffected;
        }

        /// <summary>
        /// 查询指定类型的数据库表，返回结果列表，可以指定查询条件（指定某字段等于什么值）
        /// </summary>
        /// <typeparam name="T">指定要查找的类型</typeparam>
        /// <param name="skip">略过前面 skip 条记录</param>
        /// <param name="limit">返回总数限制为最多 limit 条</param>
        /// <param name="conditions">查询条件，MongoDB驱动格式</param>
        /// <param name="modifiedDescending">默认按照修改时间倒序排序，为 true 时忽略 sorting 参数</param>
        /// <param name="sorting">排序条件，MongoDB驱动格式</param>
        /// <param name="includeDisabled">查找范围包括标记为【已屏蔽】的记录。默认包括。</param>
        /// <param name="includeDeleted">查找范围包括标记为【已删除】的记录。默认不包括。</param>
        /// <returns>特定类型的可枚举对象，用 foreach 遍历</returns>
        public static IEnumerable<T> 查询<T>(int skip, int limit, IMongoQuery conditions = null, bool modifiedDescending = true, IMongoSortBy sorting = null, bool includeDisabled = true, bool includeDeleted = false)
        {
            //GC.Collect();
            var rc = Coll<T>().FindAs<T>(PrepareQuery(conditions, includeDisabled, includeDeleted));
            var s = PrepareSortBy(modifiedDescending, sorting);
            if (null != s) rc.SetSortOrder(s);
            if (skip > 0) rc.SetSkip(skip);
            if (limit > 0) rc.SetLimit(limit);
            return rc;
        }

        /// <summary>
        /// 查询指定类型的数据库表，返回 BsonDocument 结果列表，指定返回的结果仅包含哪些字段，可以指定查询条件（指定某字段等于什么值）
        /// </summary>
        /// <typeparam name="T">指定要查找的类型</typeparam>
        /// <param name="skip">略过前面 skip 条记录，大于0时有效</param>
        /// <param name="limit">返回总数限制为最多 limit 条，大于0时有效</param>
        /// <param name="fields">指定要包含的字段名：new string[]{ "登录信息.密码" }返回结果中仅包含“登录信息.密码”字段，用["登录信息"]["密码"]的形式访问</param>
        /// <param name="conditions">查询条件，MongoDB驱动格式</param>
        /// <param name="modifiedDescending">默认按照修改时间倒序排序，为 true 时忽略 sorting 参数</param>
        /// <param name="sorting">排序条件，MongoDB驱动格式</param>
        /// <param name="includeDisabled">查找范围包括标记为【已屏蔽】的记录。默认包括。</param>
        /// <param name="includeDeleted">查找范围包括标记为【已删除】的记录。默认不包括。</param>
        /// <returns>特定类型的可枚举对象，用 foreach 遍历</returns>
        public static IEnumerable<BsonDocument> 列表<T>(int skip, int limit, IMongoFields fields, IMongoQuery conditions = null, bool modifiedDescending = true, IMongoSortBy sorting = null, bool includeDisabled = true, bool includeDeleted = false)
        {
            //GC.Collect();
            var rc = Coll<T>().FindAs<BsonDocument>(PrepareQuery(conditions, includeDisabled, includeDeleted));
            if (null != fields) rc.SetFields(fields);
            var s = PrepareSortBy(modifiedDescending, sorting);
            if (null != s) rc.SetSortOrder(s);
            if (skip > 0) rc.SetSkip(skip);
            if (limit > 0) rc.SetLimit(limit);
            return rc;
        }
        /// <summary>
        /// 查询指定类型的数据库表，返回总数，可以指定查询条件（指定某字段等于什么值）
        /// </summary>
        /// <typeparam name="T">指定要查找的类型</typeparam>
        /// <param name="skip">略过前面 skip 条记录</param>
        /// <param name="limit">返回总数限制为最多 limit 条</param>
        /// <param name="conditions">查询条件，MongoDB驱动格式</param>
        /// <param name="includeDisabled">查找范围包括标记为【已屏蔽】的记录。默认包括。</param>
        /// <param name="includeDeleted">查找范围包括标记为【已删除】的记录。默认不包括。</param>
        /// <returns></returns>
        public static long 计数<T>(int skip, int limit, IMongoQuery conditions = null, bool includeDisabled = true, bool includeDeleted = false)
        {
            var ca = new CountArgs
            {
                Query = PrepareQuery(conditions, includeDisabled, includeDeleted)
            };
            if (skip > 0) ca.Skip = skip;
            if (limit > 0) ca.Limit = limit;
            return Coll<T>().Count(ca);
        }
        private static IMongoQuery PrepareQuery(IMongoQuery query, bool includeDisabled, bool includeDeleted)
        {
            if (!includeDisabled)
            {
                var q = Query.EQ("基本数据.已屏蔽", false);
                query = null == query
                    ? q
                    : Query.And(q, query);
            }
            if (!includeDeleted)
            {
                var q = Query.EQ("基本数据.已删除", false);
                query = null == query
                    ? q
                    : Query.And(q, query);
            }
            return query;
        }
        private static IMongoUpdate PrepareUpdate(IMongoUpdate update, bool updateModifiedTime = true)
        {
            return updateModifiedTime
                ? Update.Combine(Update.Set("基本数据.修改时间", DateTime.Now), update)
                : update
                ;
        }
        private static IMongoSortBy PrepareSortBy(bool modifiedDescending, IMongoSortBy sorting)
        {
            return modifiedDescending
                ? SortBy.Descending("基本数据.修改时间")
                : sorting
                ;
        }
    }
}
