using Go81WebApp.Models.数据模型;
using Go81WebApp.Models.数据模型.推广业务数据模型;
using MongoDB;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using System;
using System.Collections.Generic;

namespace Go81WebApp.Models.管理器.推广业务管理
{
    public class 供应商充值余额管理
    {
        public static bool 添加供应商充值余额(供应商充值余额 content)
        {
            return Mongo.添加(content);
        }
        public static bool 更新供应商充值余额(供应商充值余额 content, bool f = true)
        {
            return Mongo.更新(content, f);
        }
        public static bool 屏蔽供应商充值余额(long id)
        {
            return Mongo.屏蔽<供应商充值余额>(id);
        }
        public static bool 屏蔽供应商充值余额解除(long id)
        {
            return Mongo.屏蔽解除<供应商充值余额>(id);
        }
        public static bool 删除供应商充值余额(long id)
        {
            return Mongo.删除<供应商充值余额>(id);
        }
        public static 供应商充值余额 查找供应商充值余额(long id)
        {
            return Mongo.查找<供应商充值余额>(id);
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
        public static IEnumerable<供应商充值余额> 查询供应商充值余额(int skip, int limit, IMongoQuery conditions = null, bool modifiedDescending = true, IMongoSortBy sorting = null, bool includeDisabled = true, bool includeDeleted = false)
        {
            return Mongo.查询<供应商充值余额>(skip, limit, conditions, modifiedDescending, sorting, includeDisabled, includeDeleted);
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
        public static long 计数供应商充值余额(int skip, int limit, IMongoQuery conditions = null, bool includeDisabled = true, bool includeDeleted = false)
        {
            return Mongo.计数<供应商充值余额>(skip, limit, conditions, includeDisabled, includeDeleted);
        }
    }
}