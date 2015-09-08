using System.Web.Mvc;

namespace Go81WebApp.Controllers.基本功能
{
    public class 错误页面Controller : Controller
    {
        public ActionResult CanNotLoginIntranet()
        {
            ViewBag.Title = "错误";
            ViewBag.Msg = "尊敬的用户您好，您的账户不能在内网登录。";
            return View("Error");
        }
        public ActionResult WrongUserType()
        {
            ViewBag.Title = "错误";
            ViewBag.Msg = "尊敬的用户您好，非常抱歉此功能并非为您设计。";
            return View("Error");
        }
        public ActionResult NoPrivilege()
        {
            ViewBag.Title = "错误";
            ViewBag.Msg = "尊敬的用户您好，您的权限不足，不能使用此功能。";
            return View("Error");
        }
        public ActionResult UnderConstruction()
        {
            ViewBag.Title = "建设中";
            ViewBag.Msg = "施工中";
            return View();
        }
    }
}
