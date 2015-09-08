using Go81WebApp.Models.数据模型;
using Go81WebApp.Models.数据模型.内容数据模型;
using MongoDB;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using System;
using System.Collections.Generic;

namespace Go81WebApp.Models.管理器
{
    public static class 公告管理
    {
        public static bool 添加公告(公告 content)
        {
            return Mongo.添加(content);
        }
        public static bool 更新公告(公告 content,bool s = true,bool f = true)
        {
            if(s){content.审核数据.审核状态 = 审核状态.未审核;}
            return Mongo.更新(content,f);
        }
        public static bool 屏蔽公告(long id)
        {
            return Mongo.屏蔽<公告>(id);
        }
        public static bool 屏蔽公告解除(long id)
        {
            return Mongo.屏蔽解除<公告>(id);
        }
        public static bool 删除公告(long id)
        {
            return Mongo.删除<公告>(id);
        }
        public static 公告 查找公告(long id)
        {
            return Mongo.查找<公告>(id);
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
        public static IEnumerable<公告> 查询公告(int skip, int limit, IMongoQuery conditions = null, bool modifiedDescending = true, IMongoSortBy sorting = null, bool includeDisabled = true, bool includeDeleted = false)
        {
            return Mongo.查询<公告>(skip, limit, conditions, modifiedDescending, sorting, includeDisabled, includeDeleted);
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
        public static long 计数公告(int skip, int limit, IMongoQuery conditions = null, bool includeDisabled = true, bool includeDeleted = false)
        {
            return Mongo.计数<公告>(skip, limit, conditions, includeDisabled, includeDeleted);
        }
        public static bool 审核公告(long contentID, long 审核者ID, 审核状态 审核状态)
        {
            return 1 == Mongo.更新<公告>(Query.EQ("_id", contentID), Update.Combine(
                Update.Set("审核数据.审核者.用户ID", 审核者ID),
                Update.Set("审核数据.审核状态", 审核状态)
                )
            );
        }
        public static IEnumerable<公告> 筛选公告(int skip, int limit, 公告.公告类别 公告类别, 公告.公告性质 公告性质, DateTime 起始日期, DateTime 结束日期, string 关键字 = null)
        {
            var q = new List<IMongoQuery>();
            if (公告类别 != 公告.公告类别.未设置) q.Add(Query.EQ("公告信息.公告类别", 公告类别));
            if (公告性质 != 公告.公告性质.未设置) q.Add(Query.EQ("公告信息.公告性质", 公告性质));
            if (DateTime.MinValue != 起始日期 && DateTime.MinValue != 结束日期)
                q.Add(Query.And(
                    Query.GTE("基本数据.修改时间", 起始日期),
                    Query.LTE("基本数据.修改时间", 结束日期)
                ));
            if (string.IsNullOrWhiteSpace(关键字))
                q.Add(Query.Matches("内容主体.标题", 关键字));
            return Mongo.查询<公告>(skip, limit, Query.And(q.ToArray()), includeDisabled: false);
        }
    }
}
