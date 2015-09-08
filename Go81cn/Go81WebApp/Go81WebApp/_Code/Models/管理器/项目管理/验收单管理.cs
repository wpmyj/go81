using Go81WebApp.Models.数据模型.项目数据模型;
using MongoDB;
using MongoDB.Driver;
using System.Collections.Generic;

namespace Go81WebApp.Models.管理器
{
    public static class 验收单管理
    {
        public static bool 添加验收单(验收单 content)
        {
            return Mongo.添加(content);
        }
        public static bool 更新验收单(验收单 content)
        {
            return Mongo.更新(content);
        }
        public static bool 屏蔽验收单(long id)
        {
            return Mongo.屏蔽<验收单>(id);
        }
        public static bool 屏蔽验收单解除(long id)
        {
            return Mongo.屏蔽解除<验收单>(id);
        }
        public static bool 删除验收单(long id)
        {
            return Mongo.删除<验收单>(id);
        }
        public static 验收单 查找验收单(long id)
        {
            return Mongo.查找<验收单>(id);
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
        public static IEnumerable<验收单> 查询验收单(int skip, int limit, IMongoQuery conditions = null, bool modifiedDescending = true, IMongoSortBy sorting = null, bool includeDisabled = true, bool includeDeleted = false)
        {
            return Mongo.查询<验收单>(skip, limit, conditions, modifiedDescending, sorting, includeDisabled, includeDeleted);
        }
        /// <summary>
        /// 查询指定类型的数据库表，返回总数，可以指定查询条件（指定某字段等于什么值）
        /// </summary>
        /// <param name="skip">略过前面 skip 条记录</param>
        /// <param name="limit">返回总数限制为最多 limit 条</param>
        /// <param name="conditions">查询条件：Query.EQ("登录信息.密码", "abc")搜寻所有密码设置为"abc"的用户</param>
        /// <param name="includeDeleted">查找范围包括标记为【已删除】的记录。默认不包括。</param>
        /// <returns>特定类型的可枚举对象，用 foreach 遍历</returns>
        public static long 计数验收单(int skip, int limit, IMongoQuery conditions = null, bool includeDisabled = true, bool includeDeleted = false)
        {
            return Mongo.计数<验收单>(skip, limit, conditions, includeDisabled, includeDeleted);
        }
    }
}
