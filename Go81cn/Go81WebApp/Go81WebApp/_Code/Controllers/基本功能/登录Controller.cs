using Go81WebApp.Models.管理器;
using Go81WebApp.Models.管理器.统计管理;
using Go81WebApp.Models.数据模型.统计数据模型;
using Go81WebApp.Models.数据模型.用户数据模型;
using MongoDB;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace Go81WebApp.Controllers.基本功能
{
    public class 登录Controller : Controller
    {
        public ActionResult Login(string id)
        {
            ViewData["type"] = id;
            if (-1 != this.HttpContext.检查登录()) return Jump(this.HttpContext.获取当前用户());
            return View();
        }
        public ActionResult NewVCodeImage()
        {
            var vc = this.HttpContext.Session["vcode"] as 验证码;
            if (null == vc)
            {
                vc = new 验证码();
                this.HttpContext.Session["vcode"] = vc;
            }
            vc.Generate();
            return new ImageResult(vc.Image);
        }
        public class LoginInfo
        {
            [Required(ErrorMessage = "用户名必须填写")]
            [StringLength(30, MinimumLength = 6, ErrorMessage = "长度为6-30")]
            [RegularExpression(@"^[a-zA-Z]\w+$", ErrorMessage = "以字母开头(可用字母、数字或_)")]
            public string LoginName { get; set; }
            [Required(ErrorMessage = "密码必须填写")]
            [StringLength(30, MinimumLength = 6, ErrorMessage = "长度为6-30")]
            //[RegularExpression(@"^\w+$", ErrorMessage = "密码含有非法字符")]
            [Display(Name = "密码")]
            public string LoginPwd { get; set; }
            public string VerifyCode { get; set; }
            public bool noExpire { get; set; }
        }
        [HttpPost]
        [AllowAnonymous]
        //[ValidateAntiForgeryToken]
        public ActionResult Login(LoginInfo li)
        {
            string temp_session="";
            if (Session["Ginfo"] != null)
            {
                temp_session = Session["Ginfo"].ToString();
            }
            var vc = this.HttpContext.Session["vcode"] as 验证码;
            if (null == vc || string.IsNullOrWhiteSpace(li.VerifyCode))
            {
                ViewBag.WrongLoginMessage = "请填写验证码";
                return View();
            }
            if (li.VerifyCode.ToUpper() != vc.Code)
            {
                ViewBag.WrongLoginMessage = "验证码错误";
                return View();
            }
            var u = this.HttpContext.登录(li.LoginName, li.LoginPwd, li.noExpire);
            Session["Ginfo"] = temp_session;
            //记录登录统计
            登录统计 logindate = new 登录统计();
            logindate.登录结果 = 登录结果.用户名或密码错误;
            logindate.用户数据.用户ID = -1;
            logindate.登录IP = FileHelper.StrHelp.GetIPAddress();
            logindate.内网访问 = false;

            if (null != u)
            {
#if INTRANET
                logindate.内网访问 = true;
                if (!(u is 单位用户 || u is 运营团队))
                {
                    HttpContext.登出();
                    return RedirectToAction("CanNotLoginIntranet", "错误页面");
                }
#endif
                logindate.登录结果 = 登录结果.登录成功;
                logindate.用户数据.用户ID = u.Id;
                登录统计管理.添加登录统计(logindate);
                return Jump(u);
            }

            登录统计管理.添加登录统计(logindate);

            ViewBag.WrongLoginMessage = "用户名或密码错误";
            return View();
        }
        private ActionResult Jump(用户基本数据 u)
        {
            var url = this.Request.Params["ReturnUrl"];
            if (!string.IsNullOrWhiteSpace(url)) return Redirect(url) as ActionResult;
            var t = u.GetType();
            if (typeof(供应商) == t) return RedirectToAction("Index", "供应商后台") as ActionResult;
            if (typeof(单位用户) == t) return RedirectToAction("Index", "单位用户后台") as ActionResult;
            if (typeof(运营团队) == t) return RedirectToAction("Index", "运营团队后台") as ActionResult;
            if (typeof(个人用户) == t) return RedirectToAction("Index", "个人用户后台") as ActionResult;
            if (typeof(专家) == t) return RedirectToAction("Index", "专家后台") as ActionResult;
            return RedirectToAction("Index", "首页") as ActionResult;
        }
        public ActionResult Logout()
        {
            this.HttpContext.登出();
            return RedirectToAction("Index", "首页");
        }
    }
}
