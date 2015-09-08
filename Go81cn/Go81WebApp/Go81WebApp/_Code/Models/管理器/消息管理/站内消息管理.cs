using Go81WebApp.Models.数据模型.消息数据模型;
using MongoDB;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using System;
using System.Collections.Generic;

namespace Go81WebApp.Models.管理器
{
    public static class 站内消息管理
    {
        public static bool 添加站内消息(站内消息 message, long 发起者ID, long 收信人ID)
        {
            message.发起者.用户ID = 发起者ID;
            message.收信人.用户ID = 收信人ID;
            return Mongo.添加(message);
        }
        public static bool 更新已读时间(long 站内消息ID, bool 发起者, DateTime time)
        {
            return 1 == Mongo.更新<站内消息>(
                Query.EQ("_id", 站内消息ID),
                发起者
                    ? Update.Set("发起者.上次阅读时间", time)
                    : Update.Set("收信人.上次阅读时间", time)
                );
        }
        public static bool 更新站内消息(站内消息 message)
        {
            return Mongo.更新(message);
        }
        public static bool 屏蔽站内消息(long id)
        {
            return Mongo.屏蔽<站内消息>(id);
        }
        public static bool 屏蔽站内消息解除(long id)
        {
            return Mongo.屏蔽解除<站内消息>(id);
        }
        public static bool 删除站内消息(long id)
        {
            return Mongo.删除<站内消息>(id);
        }
        public static 站内消息 查找站内消息(long id)
        {
            return Mongo.查找<站内消息>(id);
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
        public static IEnumerable<站内消息> 查询站内消息(int skip, int limit, IMongoQuery conditions = null, bool modifiedDescending = true, IMongoSortBy sorting = null, bool includeDisabled = true, bool includeDeleted = false)
        {
            return Mongo.查询<站内消息>(skip, limit, conditions, modifiedDescending, sorting, includeDisabled, includeDeleted);
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
        public static long 计数站内消息(int skip, int limit, IMongoQuery conditions = null, bool includeDisabled = true, bool includeDeleted = false)
        {
            return Mongo.计数<站内消息>(skip, limit, conditions, includeDisabled, includeDeleted);
        }
        public static IEnumerable<站内消息> 查询发起者的站内消息(int skip, int limit, long userID)
        {
            return 查询站内消息(skip, limit, Query.EQ("发起者.用户ID", userID));
        }
        public static IEnumerable<站内消息> 查询收信人的站内消息(int skip, int limit, long userID)
        {
            return 查询站内消息(skip, limit, Query.EQ("收信人.用户ID", userID));
        }
    }
}