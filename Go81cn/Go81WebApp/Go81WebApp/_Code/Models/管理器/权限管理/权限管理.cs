using Go81WebApp.Models.数据模型.权限数据模型;
using Go81WebApp.Models.数据模型.用户数据模型;
using MongoDB.Driver.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Go81WebApp.Models.管理器
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = true)]
    public sealed class 用户类型验证 : ActionFilterAttribute
    {
        private Type[] 用户类型 { get; set; }
        public 用户类型验证(params Type[] t)
        {
            Order = 200;
            用户类型 = t;
        }
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var u = filterContext.HttpContext.获取当前用户();
#if INTRANET
            if (!(u is 单位用户 || u is 运营团队))
            {
                filterContext.HttpContext.登出();
                filterContext.Result = new RedirectToRouteResult(new System.Web.Routing.RouteValueDictionary
                {
                    {"action", "CanNotLoginIntranet"},
                    {"controller", "错误页面"},
                });
            }
#endif
            if (!用户类型.Contains(u.GetType()))
            {
                filterContext.Result = new RedirectToRouteResult(new System.Web.Routing.RouteValueDictionary
                {
                    {"action", "WrongUserType"},
                    {"controller", "错误页面"},
                });
            }
        }
    }
    //[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = true)]
    //public sealed class 用户审核验证 : ActionFilterAttribute
    //{
    //    public 用户审核验证()
    //    {
    //        Order = 210;
    //    }
    //    public override void OnActionExecuting(ActionExecutingContext filterContext)
    //    {
    //        var u = filterContext.HttpContext.获取当前用户();
    //        //if (!用户类型.Contains(u.GetType()))
    //        {
    //            filterContext.Result = new RedirectToRouteResult(new System.Web.Routing.RouteValueDictionary
    //            {
    //                {"action", "WrongUserType"},
    //                {"controller", "错误页面"},
    //            });
    //        }
    //    }
    //}
    /// <summary>
    /// 单一权限验证当具有传入权限中的至少一种时返回true
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public sealed class 单一权限验证 : ActionFilterAttribute
    {
        private IEnumerable<string> 权限列表 { get; set; }
        public 单一权限验证(params 权限[] 权限列表)
        {
            Order = 300;
            this.权限列表 = 权限列表.Select(o => o.ToString());
        }
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (!filterContext.HttpContext.单一权限验证(权限列表))
            {
                filterContext.Result = new RedirectToRouteResult(new System.Web.Routing.RouteValueDictionary
                {
                    {"action", "NoPrivilege"},
                    {"controller", "错误页面"},
                });
            }
        }

    }
    /// <summary>
    /// 多重权限验证当具有传入的所有权限时返回true
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public sealed class 多重权限验证 : ActionFilterAttribute
    {
        private IEnumerable<string> 权限列表 { get; set; }
        public 多重权限验证(params 权限[] 权限列表)
        {
            Order = 300;
            this.权限列表 = 权限列表.Select(o => o.ToString());
        }
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (!filterContext.HttpContext.多重权限验证(权限列表))
            {
                filterContext.Result = new RedirectToRouteResult(new System.Web.Routing.RouteValueDictionary
                {
                    {"action", "NoPrivilege"},
                    {"controller", "错误页面"},
                });
            }
        }
    }
    public static class 权限管理
    {
        private static IEnumerable<角色> 全部角色 { get; set; }
        private static IEnumerable<用户组> 全部用户组 { get; set; }
        public static IEnumerable<string> 全部权限列表 { get { return Enum.GetNames(typeof(权限)); } }
        static 权限管理()
        {
            重新加载全部用户组();
        }
        public static void 重新加载全部用户组()
        {
            全部角色 = 角色管理.查询角色(0, 0, includeDisabled: false);
            全部用户组 = 用户组管理.查询用户组(0, 0, includeDisabled: false);
        }
        public static IEnumerable<string> 计算权限<T>(T u) where T : 用户基本数据
        {
            return 全部用户组.Where(
                    g => 全部角色.Where(r => u.角色.Contains(r.角色名) && 0 != r.包含用户组.Count)
                    .Aggregate(new List<string>() as IEnumerable<string>, (current, r) => current.Union(r.包含用户组))
                    .Union(u.用户组).Contains(g.用户组名))
                .Aggregate(new List<string>() as IEnumerable<string>, (current, g) => current.Union(g.权限列表));
        }
        public static bool 单一权限验证(this HttpContextBase hc, IEnumerable<string> 权限列表)
        {
            return hc.获取当前用户权限列表().ContainsAny(权限列表);
        }
        public static bool 单一权限验证(this HttpContextBase hc, params string[] 权限列表)
        {
            return 单一权限验证(hc, 权限列表 as IEnumerable<string>);
        }
        public static bool 单一权限验证(this HttpContextBase hc, params 权限[] 权限列表)
        {
            return 单一权限验证(hc, 权限列表.Select(g => g.ToString()));
        }
        public static bool 多重权限验证(this HttpContextBase hc, IEnumerable<string> 权限列表)
        {
            return hc.获取当前用户权限列表().ContainsAll(权限列表);
        }
        public static bool 多重权限验证(this HttpContextBase hc, params string[] 权限列表)
        {
            return 多重权限验证(hc, 权限列表 as IEnumerable<string>);
        }
        public static bool 多重权限验证(this HttpContextBase hc, params 权限[] 权限列表)
        {
            return 多重权限验证(hc, 权限列表.Select(g => g.ToString()));
        }
        public static void 批量单一权限验证(this HttpContextBase hc, ref Dictionary<object, Tuple<权限[], bool>> 权限列表)
        {
            foreach (var g in 权限列表.Keys)
            {
                权限列表[g] = new Tuple<权限[], bool>(权限列表[g].Item1, 单一权限验证(hc, 权限列表[g].Item1));
            }
        }
        public static void 批量多重权限验证(this HttpContextBase hc, ref Dictionary<object, Tuple<权限[], bool>> 权限列表)
        {
            foreach (var g in 权限列表.Keys)
            {
                权限列表[g] = new Tuple<权限[], bool>(权限列表[g].Item1, 多重权限验证(hc, 权限列表[g].Item1));
            }
        }
        public static void 批量权限验证(this HttpContextBase hc, ref Dictionary<object, Tuple<权限[], bool>> 权限列表)
        {
            var k = 权限列表.Keys.ToArray();
            foreach (var g in k)
            {
                权限列表[g] = new Tuple<权限[], bool>(权限列表[g].Item1, 权限列表[g].Item2
                    ? 多重权限验证(hc, 权限列表[g].Item1)
                    : 单一权限验证(hc, 权限列表[g].Item1));
            }
        }
        public static bool 角色已存在(string 角色名)
        {
            return 全部角色.Any(g => g.角色名 == 角色名);
        }
        public static bool 用户组已存在(string 用户组名)
        {
            return 全部用户组.Any(g => g.用户组名 == 用户组名);
        }
    }
    public enum 权限
    {
        欢迎=11000,
        欢迎页面,

        待办=12000,
        待办任务,

        本账号信息维护 = 13000,
        修改登录密码,
        修改联系人信息,

        站内消息 = 14000,
        新增消息,
        已收消息,
        已发消息,

        采购公告管理=15000,
        新增采购公告,
        我的采购公告,

        验收单管理=16000,
        验收单管理流程,
        未审核的验收单,
        已审核的验收单,
        已作废的验收单,
        我的验收单列表,

        采购需求管理=17000,
        编制需求计划,
        审核需求计划,
        我的需求计划列表,

        采购任务管理=18000,
        编制采购任务,
        审核采购任务,
        受理采购任务,
        我的采购任务列表,

        网上竞价管理=19000,
        新增网上竞价,
        未完成的网上竞价,
        已完成的网上竞价,

        电子标书管理=20000,
        新增电子标书,
        可使用电子标书的项目列表,

        中标商品=21000,
        中标商品查询系统,

        应急采购系统=22000,
        应急采购在线投标系统,

        评审专家抽取系统=23000,
        新增评审专家抽取申请,
        我可进行的评审专家抽取申请,
        我已完成的评审专家抽取申请,
        我可审核的评审专家抽取申请,
        评审专家全部抽取记录列表,

        在库供应商抽取系统=24000,
        新增供应商抽取申请,
        我可进行的供应商抽取申请,
        我已完成的供应商抽取申请,
        我可审核的供应商抽取申请,
        供应商全部抽取记录列表,

        评审专家入库管理=25000,
        我可审核的评审专家入库申请,
        我已审核的评审专家入库申请,
        评审专家全部入库申请列表,
        打印评审专家初审复审意见表,
        
        供应商入库管理=26000,
        我可审核的供应商入库申请,
        我已审核的供应商入库申请,
        供应商全部入库申请列表,
        打印供应商初审复审意见表,

        推荐供应商与评审专家=27000,
        推荐流程说明,
        下载入库申请表,

        附属账号管理=28000,
        新建附属账号,
        未审核的附属账号,
        已审核的附属账号,
        全部附属账号列表,
        全部附属账号的采购公告列表,
        全部附属账号的验收单列表,
    }
}
