using Go81WebApp.Models.数据模型.用户数据模型;
using MongoDB;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Go81WebApp.Models.管理器
{
    public class Api登录管理
    {
        public static 用户基本数据 登录(string loginName, string password, bool noExpire)
        {
            用户基本数据 u;
            if (!用户管理.验证登录名和密码(loginName, password, out u)) return null;
            return u;
        }

        public static bool 添加Api登录信息(ApiControllers.ExpertController.Api登录信息 content)
        {
            return Mongo.添加(content);
        }
        public static bool 更新Api登录信息(ApiControllers.ExpertController.Api登录信息 content)
        {
            return Mongo.更新(content);
        }
        public static bool 屏蔽Api登录信息(long id)
        {
            return Mongo.屏蔽<ApiControllers.ExpertController.Api登录信息>(id);
        }
        public static bool 屏蔽Api登录信息解除(long id)
        {
            return Mongo.屏蔽解除<ApiControllers.ExpertController.Api登录信息>(id);
        }
        public static bool 删除Api登录信息(long id)
        {
            return Mongo.删除<ApiControllers.ExpertController.Api登录信息>(id);
        }
        public static ApiControllers.ExpertController.Api登录信息 查找Api登录信息(long id)
        {
            return Mongo.查找<ApiControllers.ExpertController.Api登录信息>(id);
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
        public static IEnumerable<ApiControllers.ExpertController.Api登录信息> 查询Api登录信息(int skip, int limit, IMongoQuery conditions = null, bool modifiedDescending = true, IMongoSortBy sorting = null, bool includeDisabled = true, bool includeDeleted = false)
        {
            return Mongo.查询<ApiControllers.ExpertController.Api登录信息>(skip, limit, conditions, modifiedDescending, sorting, includeDisabled, includeDeleted);
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
        public static long 计数Api登录信息(int skip, int limit, IMongoQuery conditions = null, bool includeDisabled = true, bool includeDeleted = false)
        {
            return Mongo.计数<ApiControllers.ExpertController.Api登录信息>(skip, limit, conditions, includeDisabled, includeDeleted);
        }
    }
}