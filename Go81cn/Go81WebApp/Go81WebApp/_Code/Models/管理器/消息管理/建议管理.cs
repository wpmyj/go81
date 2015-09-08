using Go81WebApp.Models.数据模型.消息数据模型;
using MongoDB;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using System;
using System.Collections.Generic;

namespace Go81WebApp.Models.管理器
{
    public static class 建议管理
    {
        public static bool 添加建议(建议 message, long 发起者ID, long 受理单位ID)
        {
            message.发起者.用户ID = 发起者ID;
            message.受理单位.用户ID = 受理单位ID;
            return Mongo.添加(message);
        }
        public static bool 更新已读时间(long 建议ID, bool 发起者, DateTime time)
        {
            return 1 == Mongo.更新<建议>(
                Query.EQ("_id", 建议ID),
                发起者
                    ? Update.Set("发起者.上次阅读时间", time)
                    : Update.Set("受理单位.上次阅读时间", time)
                );
        }
        public static bool 更新建议(建议 message)
        {
            return Mongo.更新(message);
        }
        public static bool 屏蔽建议(long id)
        {
            return Mongo.屏蔽<建议>(id);
        }
        public static bool 屏蔽建议解除(long id)
        {
            return Mongo.屏蔽解除<建议>(id);
        }
        public static bool 删除建议(long id)
        {
            return Mongo.删除<建议>(id);
        }
        public static 建议 查找建议(long id)
        {
            return Mongo.查找<建议>(id);
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
        public static IEnumerable<建议> 查询建议(int skip, int limit, IMongoQuery conditions = null, bool modifiedDescending = true, IMongoSortBy sorting = null, bool includeDisabled = true, bool includeDeleted = false)
        {
            return Mongo.查询<建议>(skip, limit, conditions, modifiedDescending, sorting, includeDisabled, includeDeleted);
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
        public static long 计数建议(int skip, int limit, IMongoQuery conditions = null, bool includeDisabled = true, bool includeDeleted = false)
        {
            return Mongo.计数<建议>(skip, limit, conditions, includeDisabled, includeDeleted);
        }
        public static void 处理建议(long messageID, 处理状态 state)
        {
            Mongo.更新<建议>(Query.EQ("_id", messageID), Update.Set("处理状态", state));
        }
        public static IEnumerable<建议> 查询发起者的建议(int skip, int limit, long userID, 处理状态 state)
        {
            return 查询建议(skip, limit, state == 处理状态.全部
                ? Query.EQ("发起者.用户ID", userID)
                : Query.And(Query.EQ("发起者.用户ID", userID), Query.EQ("处理状态", state)))
                ;
        }
        public static IEnumerable<建议> 查询受理单位收到的建议(int skip, int limit, long userID, 处理状态 state)
        {
            return 查询建议(skip, limit, state == 处理状态.全部
                ? Query.EQ("受理单位.用户ID", userID)
                : Query.And(Query.EQ("受理单位.用户ID", userID), Query.EQ("处理状态", state)))
                ;
        }
    }
}