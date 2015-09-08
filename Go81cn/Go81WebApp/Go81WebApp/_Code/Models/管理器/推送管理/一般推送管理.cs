using Go81WebApp.Models.数据模型.推送数据模型;
using MongoDB;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using System.Collections.Generic;

namespace Go81WebApp.Models.管理器.推送管理
{
    public static class 一般推送管理
    {
        public static bool 添加一般推送(一般推送 content)
        {
            return Mongo.添加(content);
        }
        public static bool 更新一般推送(一般推送 content)
        {
            return Mongo.更新(content);
        }
        public static bool 屏蔽一般推送(long id)
        {
            return Mongo.屏蔽<一般推送>(id);
        }
        public static bool 屏蔽一般推送解除(long id)
        {
            return Mongo.屏蔽解除<一般推送>(id);
        }
        public static bool 删除一般推送(long id)
        {
            return Mongo.删除<一般推送>(id);
        }
        public static 一般推送 查找一般推送(long id)
        {
            return Mongo.查找<一般推送>(id);
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
        public static IEnumerable<一般推送> 查询一般推送(int skip, int limit, IMongoQuery conditions = null, bool modifiedDescending = true, IMongoSortBy sorting = null, bool includeDisabled = true, bool includeDeleted = false)
        {
            return Mongo.查询<一般推送>(skip, limit, conditions, modifiedDescending, sorting, includeDisabled, includeDeleted);
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
        public static long 计数一般推送(int skip, int limit, IMongoQuery conditions = null, bool includeDisabled = true, bool includeDeleted = false)
        {
            return Mongo.计数<一般推送>(skip, limit, conditions, includeDisabled, includeDeleted);
        }
        public static bool 审核一般推送(long contentID, long 审核人员ID, 推送信息状态 审核状态)
        {
            return 1 == Mongo.更新<一般推送>(Query.EQ("_id", contentID), Update.Combine(
                Update.Set("审核数据.审核人员.用户ID", 审核人员ID),
                Update.Set("审核数据.推送信息状态", 审核状态)
                )
            );
        }
    }
}