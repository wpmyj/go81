using Go81WebApp.Models.数据模型;
using Go81WebApp.Models.数据模型.商品数据模型;
using MongoDB;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Go81WebApp.Models.管理器
{
    public static class 商品管理
    {
        public static bool 添加商品(商品 product, long productCatID, long userID)
        {
            //var 商品分类 = product.商品信息.所属商品分类.商品分类;
            //if (null != 商品分类.商品属性模板) 商品分类.商品属性模板 = new Dictionary<string, Dictionary<string, 商品属性数据>>();
            //foreach (var item in product.商品数据.商品属性)
            //{
            //    if (!商品分类.商品属性模板.ContainsKey(item.Key))
            //    {
            //        商品分类.商品属性模板.Add(item.Key, new Dictionary<string, 商品属性数据>(item.Value.Count));
            //    }
            //    var 商品属性分类模板 = 商品分类.商品属性模板[item.Key];
            //    foreach (var item1 in item.Value)
            //    {
            //        if (!商品属性分类模板.ContainsKey(item1.Key))
            //        {
            //            商品属性分类模板.Add(item1.Key, new 商品属性数据() { 频率 = 1, 必需 = false, 销售属性 = false, 值 = item1.Value });
            //        }
            //        else
            //        {
            //            商品属性分类模板[item1.Key].频率 += 1;
            //            商品属性分类模板[item1.Key].值.AddRange(item1.Value);
            //        }
            //    }
            //}
            product.商品信息.所属商品分类.商品分类ID = productCatID;
            product.商品信息.所属供应商.用户ID = userID;
            return Mongo.添加(product);
        }
        public static bool 更新商品(商品 product, bool updateModifiedTime = true, bool setUnverified = true)
        {
            var oldProduct = 查找商品(product.Id);
            if (product.销售信息.价格 != oldProduct.销售信息.价格
                || !EqualityComparer<商品._价格属性组合>.Default.Equals(product.销售信息.价格属性组合, oldProduct.销售信息.价格属性组合))
            {
                product.销售信息.价格修改日期 = DateTime.Now;
                更新商品价格历史(product);
            }
            if(setUnverified) product.审核数据.审核状态 = 审核状态.未审核;
            return Mongo.更新(product, updateModifiedTime);
        }
        public static void 更新商品价格(long 商品ID, decimal 价格,  商品._价格属性组合 价格组合)
        {
            更新商品价格(查找商品(商品ID), 价格, 价格组合);
        }
        public static void 更新商品价格(商品 product, decimal 价格, 商品._价格属性组合 价格组合)
        {
            if (product.销售信息.价格 == 价格
                && EqualityComparer<商品._价格属性组合>.Default.Equals(product.销售信息.价格属性组合, 价格组合))
                return;
            product.销售信息.价格 = 价格;
            product.销售信息.价格属性组合 = 价格组合;
            product.销售信息.价格修改日期 = DateTime.Now;
            更新商品价格历史(product);
            Mongo.更新(product);
        }
        private static bool 更新商品价格历史(商品 product)
        {
            var hsi = new 商品历史销售信息()
            {
                价格修改日期 = product.销售信息.价格修改日期,
                价格 = product.销售信息.价格,
                销量 = product.销售信息.销量,
                点击量 = product.销售信息.点击量,
                价格属性组合 = product.销售信息.价格属性组合,
            };
            hsi.所属商品.商品ID = product.Id;
            return Mongo.添加(hsi);
        }
        public static bool 增加浏览量(long id, int addnum=1)
        {
            var p = 查找商品(id);
#if !INTRANET
            p.销售信息.点击量 += addnum;
#else
            p.销售信息.内网点击量 += addnum;
#endif
            return Mongo.更新(p,false);
        }
        public static bool 屏蔽商品(long id)
        {
            return Mongo.屏蔽<商品>(id);
        }
        public static bool 屏蔽商品解除(long id)
        {
            return Mongo.屏蔽解除<商品>(id);
        }
        public static bool 删除商品(long id)
        {
            return Mongo.删除<商品>(id);
        }
        public static bool 删除商品(商品 product)
        {
            return 删除商品(product.Id);
        }
        public static 商品 查找商品(long id)
        {
            return Mongo.查找<商品>(id);
        }

        /// <summary>
        /// 查询指定类型的数据库表，返回结果列表，可以指定查询条件（指定某字段等于什么值）
        /// </summary>
        /// <param name="skip">略过前面 skip 条记录</param>
        /// <param name="limit">返回总数限制为最多 limit 条</param>
        /// <param name="conditions">查询条件：Query.EQ("登录信息.密码", "abc")搜寻所有密码设置为"abc"的用户</param>
        /// <param name="modifiedDescending">默认按照修改时间倒序排序，为 true 时忽略 sorting 参数</param>
        /// <param name="sorting">指定排序规则 new IMongoSortBy(){ { "登录信息.显示名", true }, { "登录信息.密码", false } }
        /// 按照【登录信息.显示名】升序排列为主排序规则，按照【登录信息.密码】降序为次要排序规则</param>
        /// <param name="includeDeleted">查找范围包括标记为【已删除】的记录。默认不包括。</param>
        /// <returns>特定类型的可枚举对象，用 foreach 遍历</returns>
        public static IEnumerable<商品> 查询商品(int skip, int limit, IMongoQuery conditions = null, bool modifiedDescending = true, IMongoSortBy sorting = null, bool includeDisabled = true, bool includeDeleted = false)
        {
            return Mongo.查询<商品>(skip, limit, conditions, modifiedDescending, sorting, includeDisabled, includeDeleted);
        }
        public static IEnumerable<BsonDocument> 列表商品(int skip, int limit, IMongoFields fields, IMongoQuery conditions = null, bool modifiedDescending = true, IMongoSortBy sorting = null, bool includeDisabled = true, bool includeDeleted = false)
        {
            return Mongo.列表<商品>(skip, limit, fields, conditions, modifiedDescending, sorting, includeDisabled, includeDeleted);
        }
        /// <summary>
        /// 查询指定类型的数据库表，返回总数，可以指定查询条件（指定某字段等于什么值）
        /// </summary>
        /// <typeparam name="T">指定要查找的类型</typeparam>
        /// <param name="skip">略过前面 skip 条记录</param>
        /// <param name="limit">返回总数限制为最多 limit 条</param>
        /// <param name="conditions">查询条件：Query.EQ("登录信息.密码", "abc")搜寻所有密码设置为"abc"的用户</param>
        /// <param name="includeDeleted">查找范围包括标记为【已删除】的记录。默认不包括。</param>
        /// <returns>特定类型的可枚举对象，用 foreach 遍历</returns>
        public static long 计数商品(int skip, int limit, IMongoQuery conditions = null, bool includeDisabled = true, bool includeDeleted = false)
        {
            return Mongo.计数<商品>(skip, limit, conditions, includeDisabled, includeDeleted);
        }
        public static IEnumerable<商品> 查询分类下商品(long catID, int skip = 0, int limit = 0, IMongoQuery conditions = null, bool modifiedDescending = true, IMongoSortBy sorting = null, bool includeDisabled = true, bool includeDeleted = false)
        {
            var q = Query.EQ("商品信息.所属商品分类.商品分类ID", catID);
            if (null != conditions) q = Query.And(conditions, q);
            return 商品管理.查询商品(skip, limit, q, modifiedDescending, sorting, includeDisabled, includeDeleted);
        }
        public static long 计数分类下商品(long catID, int skip = 0, int limit = 0, IMongoQuery conditions = null, bool includeDisabled = true, bool includeDeleted = false)
        {
            var q = Query.EQ("商品信息.所属商品分类.商品分类ID", catID);
            if (null != conditions) q = Query.And(conditions, q);
            return 商品管理.计数商品(skip, limit, q, includeDisabled, includeDeleted);
        }
        public static IEnumerable<商品> 查询供应商商品(long catID, int skip = 0, int limit = 0, IMongoQuery conditions = null, bool modifiedDescending = true, IMongoSortBy sorting = null, bool includeDisabled = true, bool includeDeleted = false)
        {
            var q = Query.EQ("商品信息.所属供应商.用户ID", catID);
            if (null != conditions) q = Query.And(conditions, q);
            return 商品管理.查询商品(skip, limit, q, modifiedDescending, sorting, includeDisabled, includeDeleted);
        }
        public static long 计数供应商商品(long catID, int skip = 0, int limit = 0, IMongoQuery conditions = null, bool includeDisabled = true, bool includeDeleted = false)
        {
            var q = Query.EQ("商品信息.所属供应商.用户ID", catID);
            if (null != conditions) q = Query.And(conditions, q);
            return 商品管理.计数商品(skip, limit, q, includeDisabled, includeDeleted);
        }
        public static Tuple<IReadOnlyDictionary<string, SortedDictionary<DateTime, decimal>>, IReadOnlyDictionary<string, SortedDictionary<DateTime, int>>> 查询历史价格数据(long productID, int skip = 0, int limit = 0, bool includeSales = false)
        {
            var p = 查找商品(productID);
            var he = Mongo.查询<商品历史销售信息>(skip, 20, Query.EQ("所属商品.商品ID", productID), includeDisabled: false);
            var hp = new Dictionary<string, SortedDictionary<DateTime, decimal>>();
            var hs = includeSales ? new Dictionary<string, SortedDictionary<DateTime, int>>() : null;
            foreach (var item in he)
            {
                if (!hp.ContainsKey(p.商品信息.商品名)) hp.Add(p.商品信息.商品名, new SortedDictionary<DateTime, decimal>());
                hp[p.商品信息.商品名][item.价格修改日期] = item.价格;
                if (includeSales)
                {
                    if (!hs.ContainsKey(p.商品信息.商品名)) hs.Add(p.商品信息.商品名, new SortedDictionary<DateTime, int>());
                    hs[p.商品信息.商品名][item.价格修改日期] = item.销量;
                }
                foreach (var item1 in item.价格属性组合.组合列表)
                {
                    var n = p.商品信息.商品名;
                    for (int i = 0; i < item.价格属性组合.属性列表.Count; ++i)
                    {
                        n += "，" + item.价格属性组合.属性列表[i].Split('.').Last() + "：" + item1.属性值表[i];
                    }
                    if (!hp.ContainsKey(n)) hp.Add(n, new SortedDictionary<DateTime, decimal>());
                    hp[n][item.价格修改日期] = item1.价格;
                    if (includeSales)
                    {
                        if (!hs.ContainsKey(n)) hs.Add(n, new SortedDictionary<DateTime, int>());
                        hs[n][item.价格修改日期] = item1.销量;
                    }
                }
            }
            return new Tuple<
                IReadOnlyDictionary<string, SortedDictionary<DateTime, decimal>>,
                IReadOnlyDictionary<string, SortedDictionary<DateTime, int>>
                >(hp, hs);
        }
        public static bool 审核商品(long 商品ID, long 审核者ID, 审核状态 审核状态,string 不通过原因=null)
        {
            var u = Update.Combine(
                Update.Set("审核数据.审核者.用户ID", 审核者ID),
                Update.Set("审核数据.审核状态", 审核状态),
                Update.Set("审核数据.审核时间", DateTime.Now)
                );
            if (null != 不通过原因) u.Combine(Update.Set("审核数据.审核不通过原因", 不通过原因));
            return 1 == Mongo.更新<商品>(Query.EQ("_id", 商品ID),u,updateModifiedTime:false
                
            );
        }
        public static int 获取内网浏览量(long id)
        {
            var u = Mongo.列表<商品>(0, 0,
                Fields<商品>.Include(o => o.销售信息.内网点击量),
                Query<商品>.EQ(o => o.Id, id))
                .FirstOrDefault();
            return null != u
                ? u["销售信息"]["内网点击量"].AsInt32
                : 0
                ;
        }
        public static int 获取浏览量(long id)
        {
            var u = Mongo.列表<商品>(0, 0,
               Fields<商品>.Include(o => o.销售信息.点击量),
               Query<商品>.EQ(o => o.Id, id))
               .FirstOrDefault();
            return null != u
                ? u["销售信息"]["点击量"].AsInt32
                : 0
                ;
        }
    }
}
