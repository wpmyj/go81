using Go81WebApp.Models.管理器;
using Go81WebApp.Models.数据模型;
using Go81WebApp.Models.数据模型.用户数据模型;
using MongoDB;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using System.Web.WebPages;
using MailApiPostClass;
using System;
using System.Configuration;
using System.Linq;
using System.Collections.Generic;
using MongoDB.Driver.Builders;
namespace Go81WebApp.Controllers.基本功能
{
    public class 注册Controller : Controller
    {
        public ActionResult Register_Gys()
        {
            string p = Request.Params["p"];
            string a = "成都军区物资采购机构（成都）";
            string b = "成都军区物资采购机构（昆明）";
            string c = "成都军区物资采购机构（贵阳）";
            string d = "成都军区物资采购机构（重庆）";
            string e = "西藏军区物资采购中心";
            if (!p.IsEmpty() && (p.Contains(a) || p.Contains(b) || p.Contains(c) || p.Contains(d) || p.Contains(e)))
            {
                ViewData["行业列表"] = 商品分类管理.查找子分类();
                ViewData["申请采购机构"] = p;
                //ViewData["gystype"] = GysType(供应商.供应商类型.物资供应商.ToString());
                //ViewData["gystypename"] = GysTypeName(供应商.供应商细分类型.未填写.ToString());
                return View();
            }
            else
            {
                return Redirect("/首页/Index");
            }
        }
        public ActionResult Register_Person()
        {
            return View();
        }
        public ActionResult Register_Unit()
        {
            IEnumerable<单位用户> user = 用户管理.查询用户<单位用户>(0, 0);
            ViewData["user"] = user;
            ViewData["用户组列表"] = 用户组管理.查询用户组(0, 0);
#if INTRANET
            单位用户 u = new 单位用户();
            //ViewData["一级单位"] = 单位用户.公告接收单位;
            return View("Register_Unit_Intranet");
#else
            return View("OutUnitRegisterTips");
#endif
        }
        public ActionResult OutUnitRegisterTips()
        {
            return View();
        }
        public JsonResult CheckUserName(string loginname)
        {
            var result = -1 != 用户管理.检查用户是否存在(loginname, true, true);
            return Json(!result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Part_Agree_Regist()
        {
            return PartialView("Part_Agree_Regist");
        }
        public ActionResult Successe_Regist()
        {
            ViewData["账号申请类型"] = Request.QueryString["id"];
            return View();
        }

        public ActionResult Register_Gys_Agree()
        {
#if INTRANET
            return View("OutGysRegisterTips");
#else
            return View();
#endif
        }
        public ActionResult OutGysRegisterTips()
        {
            return View();
        }
        public ActionResult Register_Expert_Agree()
        {
            return View();
        }
        public ActionResult Part_Register_Gys_Organization()
        {
            return PartialView("Part_Register_Gys_Organization");
        }
        public ActionResult Part_Register_Expert_Organization()
        {
            return PartialView("Part_Register_Expert_Organization");
        }
        public ActionResult Expert_Apply()
        {
            return View();
        }
        public class User
        {
            public long Id { get; set; }
            public string Name { get; set; }
        }
        public ActionResult SearchUser()
        {
            try
            {
                long id = long.Parse(Request.QueryString["id"]);
                IEnumerable<单位用户> model = 用户管理.查询用户<单位用户>(0, 0, Query<单位用户>.Where(m => m.所属单位.用户ID == id));
                List<User> us=new List<User>();
                foreach(var item in model)
                {
                    User u=new User();
                    u.Id=item.Id;
                    if(!string.IsNullOrWhiteSpace(item.单位信息.单位代号))
                    {
                        u.Name=item.单位信息.单位代号;
                    }
                    else
                    {
                        u.Name=item.单位信息.单位名称;
                    }
                    us.Add(u);
                }
                JsonResult json = new JsonResult() { Data=us};
                return Json(json, JsonRequestBehavior.AllowGet);
            }
            catch
            {
                return Redirect("/注册/Register_Unit");
            }
        }
        //public static SelectList GysType(String option)
        //{
        //    List<SelectListItem> items = new List<SelectListItem>()
        //     {
        //        new SelectListItem(){Text=供应商.供应商类型.物资供应商.ToString(),Value=供应商.供应商类型.物资供应商.ToString()},
        //        new SelectListItem(){Text=供应商.供应商类型.服务供应商.ToString(),Value=供应商.供应商类型.服务供应商.ToString()},
        //        new SelectListItem(){Text=供应商.供应商类型.工程供应商.ToString(),Value=供应商.供应商类型.工程供应商.ToString()},
        //     };
        //    SelectList selectlist = new SelectList(items, "Text", "Value", option);
        //    return selectlist;
        //}
        //public static SelectList GysTypeName(String option)
        //{
        //    List<SelectListItem> items = new List<SelectListItem>()
        //     {
        //        new SelectListItem(){Text=供应商.供应商细分类型.未填写.ToString(),Value=供应商.供应商细分类型.未填写.ToString()},
        //        new SelectListItem(){Text=供应商.供应商细分类型.机票代售点.ToString(),Value=供应商.供应商细分类型.机票代售点.ToString()},
        //        new SelectListItem(){Text=供应商.供应商细分类型.酒店.ToString(),Value=供应商.供应商细分类型.酒店.ToString()},
        //        new SelectListItem(){Text=供应商.供应商细分类型.租车企业.ToString(),Value=供应商.供应商细分类型.租车企业.ToString()},
        //     };
        //    SelectList selectlist = new SelectList(items, "Text", "Value", option);
        //    return selectlist;
        //}
        public class RegInfo_Gys
        {
            [Required(ErrorMessage = "密码必须填写")]
            [StringLength(30, MinimumLength = 6, ErrorMessage = "长度为6-30")]
            //[RegularExpression(@"^\w+$", ErrorMessage = "密码含有非法字符")]
            [DataType(DataType.Password)]
            [Display(Name = "密码")]
            public string Pwd { get; set; }

            [System.ComponentModel.DataAnnotations.Compare("Pwd", ErrorMessage = "两次密码不一致")]
            [Display(Name = "确认密码")]
            public string PwdConfirm { get; set; }

            [Required(ErrorMessage = "用户名必须填写")]
            [StringLength(30, MinimumLength = 6, ErrorMessage = "长度为6-30")]
            [RegularExpression(@"^[a-zA-Z]\w+$", ErrorMessage = "以字母开头(可用字母、数字或_)")]
            [System.Web.Mvc.Remote("CheckUserName", "注册", ErrorMessage = "该用户名已注册")]
            public string loginName { get; set; }

            [Required(ErrorMessage = "验证码必须填写")]
            public string verifyCode { get; set; }

            [Required(ErrorMessage = "固定电话必须填写")]
            //[RegularExpression(@"(\d{3}-\d{8})|(\d{4}-\d{7,8})", ErrorMessage = "*正确格式为：区号-电话号码")]
            public string Phone { get; set; }

            [Required(ErrorMessage = "手机号必须填写")]
            [RegularExpression(@"(13|14|15|16|18|19)\d{9}$", ErrorMessage = "*请输入正确的手机格式")]
            public string Tel_Phone { get; set; }

            [Required(ErrorMessage = "邮箱必须填写")]
            [RegularExpression(@"^\w+([-+.]\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*$", ErrorMessage = "*请输入正确的邮箱")]

            public string Mail { get; set; }

            public 性别 Sex { get; set; }

            [Required(ErrorMessage = "请填写企业名称")]
            //[RegularExpression("\u4e00-\u9fa5", ErrorMessage = "*企业名称不能包含特殊字符")]
            public string company_Name { get; set; }

            [Required(ErrorMessage = "* 请选择供应商类型")]
            public 供应商.供应商类型 gystype { get; set; }

            [Required(ErrorMessage = "* 请选择供应商二级类型")]
            public 供应商.供应商细分类型 gystypename { get; set; }

            [Required(ErrorMessage = "* 姓名必须填写")]
            [RegularExpression("[\u4E00-\u9FA5]{2,5}(?:·[\u4E00-\u9FA5]{2,5})*", ErrorMessage = "* 姓名为中文且不包括特殊字符")]
            public string Name { get; set; }
            //public string Industry { get; set; }
            //public string SecondHy { get; set; }
            public string Province { get; set; }
            public string City { get; set; }
            public string Area { get; set; }

        }
        public int CheckCompany()
        {
            try
            {
                string name = Request.QueryString["n"].ToString().Replace(" ","");
                int count= 用户管理.查询用户<供应商>(0, 0, Query<供应商>.Where(m => m.企业基本信息.企业名称 == name)).Count();
                if (count !=0)
                {
                    return 1;
                }
                return 0;
            }
            catch
            {
                return 0;
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register_Gys(RegInfo_Gys GysRegisterModel)
        {
            var reqplace = Request.Form["hid_input"];

            var vc = this.HttpContext.Session["vcode"] as 验证码;
            if (null == vc || GysRegisterModel.verifyCode == null || GysRegisterModel.verifyCode.ToUpper() != vc.Code)
            {
                ViewBag.VCodeError = "验证码错误";
                ViewData["行业列表"] = 商品分类管理.查找子分类();
                return View();
            }
            if (ModelState.IsValid)
            {
                var u = new 供应商();
                u.登录信息.登录名 = GysRegisterModel.loginName;
                u.登录信息.密码 = GysRegisterModel.Pwd;
                u.企业基本信息.企业名称 = GysRegisterModel.company_Name;
                u.企业联系人信息.联系人姓名 = GysRegisterModel.Name;
                u.企业联系人信息.联系人手机 = GysRegisterModel.Tel_Phone;
                u.企业联系人信息.联系人固定电话 = GysRegisterModel.Phone;
                u.企业联系人信息.联系人邮箱 = GysRegisterModel.Mail;
                u.企业联系人信息.联系人性别 = GysRegisterModel.Sex;
                u.供应商用户信息.供应商类型 = GysRegisterModel.gystype;
                u.供应商用户信息.供应商细分类型 = GysRegisterModel.gystypename;
                //u.企业基本信息.所属行业 = GysRegisterModel.Industry;
                //u.企业基本信息.所属行业二级分类 = GysRegisterModel.SecondHy;
                if (!string.IsNullOrEmpty(GysRegisterModel.Province))
                {
                    u.所属地域.省份 = GysRegisterModel.Province;
                }
                if (!string.IsNullOrEmpty(GysRegisterModel.City))
                {
                    u.所属地域.城市 = GysRegisterModel.City;
                }
                if (!string.IsNullOrEmpty(GysRegisterModel.Area))
                {
                    u.所属地域.区县 = GysRegisterModel.Area;
                }

                if (!string.IsNullOrEmpty(reqplace))
                {
                    switch (reqplace)
                    {
                        case "成都军区物资采购机构（成都）":
                            u.供应商用户信息.所属管理单位 = 供应商.采购管理单位.成都军区物资采购机构_成都;
                            break;

                        case "成都军区物资采购机构（昆明）":
                            u.供应商用户信息.所属管理单位 = 供应商.采购管理单位.成都军区物资采购机构_昆明;
                            break;

                        case "成都军区物资采购机构（贵阳）":
                            u.供应商用户信息.所属管理单位 = 供应商.采购管理单位.成都军区物资采购机构_贵阳;
                            break;

                        case "成都军区物资采购机构（重庆）":
                            u.供应商用户信息.所属管理单位 = 供应商.采购管理单位.成都军区物资采购机构_重庆;
                            break;

                        case "西藏军区物资采购中心":
                            u.供应商用户信息.所属管理单位 = 供应商.采购管理单位.西藏军区物资采购中心;
                            break;

                        default:
                            u.供应商用户信息.所属管理单位 = 供应商.采购管理单位.成都军区物资采购机构_成都;
                            break;
                    }
                }
                用户管理.添加用户(u);
                string UserNumber = u.企业联系人信息.联系人手机;//收信人列表
                string MessageContent = "欢迎加入《西南物资采购网》，请登录后台上传企业资质材料备审，如果资质优秀，则有机会推荐加入中国人民解放军总后供应商库，请确保上传材料的真实有效。详情请查看《西南物资采购网》供应商注册及须知，或致电客服热线028-69908681!";//短信内容

                string retstr = MailApiPost.PostDataGetHtml(UserNumber, MessageContent);
                //TempData["RegisterMessage"] = "注册成功";

                return Content("<script>window.location='/注册/Successe_Regist';</script>");
            }
            return View();
        }

        public class RegInfo_Person
        {
            [Required(ErrorMessage = "密码必须填写")]
            [StringLength(30, MinimumLength = 6, ErrorMessage = "长度为6-30")]
            //[RegularExpression(@"^\w+$", ErrorMessage = "密码含有非法字符")]
            [Display(Name = "密码")]
            [DataType(DataType.Password)]
            public string Pwd { get; set; }

            [System.ComponentModel.DataAnnotations.Compare("Pwd", ErrorMessage = "两次密码不一致")]
            [Display(Name = "确认密码")]
            public string PwdConfirm { get; set; }

            [Required(ErrorMessage = "用户名必须填写")]
            [StringLength(30, MinimumLength = 6, ErrorMessage = "长度为6-30")]
            [RegularExpression(@"^[a-zA-Z]\w+$", ErrorMessage = "以字母开头(可用字母、数字或_)")]
            [System.Web.Mvc.Remote("CheckUserName", "注册", ErrorMessage = "该用户名已注册")]
            public string loginName { get; set; }
            public string Name { get; set; }
            [Required(ErrorMessage = "身份证号码必须填写")]
            [RegularExpression("[0-9]{17}[0-9,X]{1}", ErrorMessage = "*请正确输入您的18位有效身份证号码")]
            public string IdCard { get; set; }
            [Required(ErrorMessage = "手机号必须填写")]
            [RegularExpression(@"(13|14|15|16|18|19)\d{9}$", ErrorMessage = "*请输入正确的手机格式")]
            public string Phone { get; set; }
            [Required(ErrorMessage = "邮箱必须填写")]
            [RegularExpression(@"^\w+([-+.]\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*$", ErrorMessage = "*请输入正确的邮箱")]
            public string Emial { get; set; }
            public string verifyCode { get; set; }

            public 个人用户 u { get; set; }
            public RegInfo_Person() { u = new 个人用户(); }
        }


        public class RegInfo_Unit
        {
            [Required(ErrorMessage = "密码必须填写")]
            [StringLength(30, MinimumLength = 6, ErrorMessage = "长度为6-30")]
            //[RegularExpression(@"^\w+$", ErrorMessage = "密码含有非法字符")]
            [Display(Name = "密码")]
            [DataType(DataType.Password)]
            public string Pwd { get; set; }

            [System.ComponentModel.DataAnnotations.Compare("Pwd", ErrorMessage = "两次密码不一致")]
            [Display(Name = "确认密码")]
            public string PwdConfirm { get; set; }

            [Required(ErrorMessage = "用户名必须填写")]
            [StringLength(30, MinimumLength = 6, ErrorMessage = "长度为6-30")]
            [RegularExpression(@"^[a-zA-Z]\w+$", ErrorMessage = "以字母开头(可用字母、数字或_)")]
            [System.Web.Mvc.Remote("CheckUserName", "注册", ErrorMessage = "该用户名已注册")]
            public string LoginName { get; set; }

            [Required(ErrorMessage = "联系人必须填写")]
            public string ContactName { get; set; }

            [Required(ErrorMessage = "电话必须填写")]
            public string ContactPhone { get; set; }
            [Required(ErrorMessage = "手机号必须填写")]
            public string ContactTel { get; set; }

            [Required(ErrorMessage = "单位名称必须填写")]
            public string Unitname { get; set; }
            //[Required(ErrorMessage = "单位代号必须填写")]
            public string Codename { get; set; }
            [Required(ErrorMessage = "联系人职务必须填写")]
            public string UnitDuty { get; set; }

            public string VerifyCode { get; set; }

            public 单位用户 U { get; set; }
            public RegInfo_Unit() { U = new 单位用户(); }
        }
        public JsonResult CheckUnitCode(string unitCode)
        {
            var result = -1 != 用户管理.检查单位编码是否存在(unitCode, true, true);
            return Json(!result, JsonRequestBehavior.AllowGet);
        }
        // POST: /Movie/Create
        // 为了防止“过多发布”攻击，请启用要绑定到的特定属性，有关 
        // 详细信息，请参阅 http://go.microsoft.com/fwlink/?LinkId=317598。
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register_Person(RegInfo_Person PersonRegisterModel)
        {
            var vc = this.HttpContext.Session["vcode"] as 验证码;
            if (null == vc || PersonRegisterModel.verifyCode == null || PersonRegisterModel.verifyCode.ToUpper() != vc.Code)
            {
                ViewBag.VCodeError = "验证码错误";
                return View();
            }

            if (ModelState.IsValid)
            {
                PersonRegisterModel.u.登录信息.登录名 = PersonRegisterModel.loginName;
                PersonRegisterModel.u.登录信息.密码 = PersonRegisterModel.Pwd;
                PersonRegisterModel.u.个人信息.姓名 = PersonRegisterModel.Name;
                PersonRegisterModel.u.个人信息.身份证号 = PersonRegisterModel.IdCard;
                PersonRegisterModel.u.联系方式.电子邮件 = PersonRegisterModel.Emial;
                PersonRegisterModel.u.联系方式.手机 = PersonRegisterModel.Phone;
                用户管理.添加用户(PersonRegisterModel.u);
                //TempData["RegisterMessage"] = "注册成功";
                return RedirectToAction("Login", "登录");
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register_Unit(RegInfo_Unit unitRegisterModel)
        {
            var vc = this.HttpContext.Session["vcode"] as 验证码;
            if (null == vc || unitRegisterModel.VerifyCode == null || unitRegisterModel.VerifyCode.ToUpper() != vc.Code)
            {
                ViewBag.VCodeError = "验证码错误";
                return View();
            }

            if (ModelState.IsValid)
            {
                unitRegisterModel.U.登录信息.登录名 = unitRegisterModel.LoginName;
                unitRegisterModel.U.登录信息.密码 = unitRegisterModel.Pwd;
                unitRegisterModel.U.联系方式.联系人 = unitRegisterModel.ContactName;
                unitRegisterModel.U.单位信息.单位代号 = unitRegisterModel.Unitname;
                unitRegisterModel.U.联系方式.手机 = unitRegisterModel.ContactPhone;
                unitRegisterModel.U.审核数据.审核状态 = 审核状态.未审核;
                unitRegisterModel.U.单位信息.单位编码 = string.Empty;
                用户管理.添加用户(unitRegisterModel.U);
                //TempData["RegisterMessage"] = "注册成功";
                return Content("<script>window.location='/注册/Successe_Regist';</script>");
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register_Unit_Intranet(RegInfo_Unit unitRegisterModel)
        {
            var vc = this.HttpContext.Session["vcode"] as 验证码;
            if (null == vc || unitRegisterModel.VerifyCode == null || unitRegisterModel.VerifyCode.ToUpper() != vc.Code)
            {
                单位用户 u = new 单位用户();
                //ViewData["一级单位"] = 单位用户.公告接收单位;
                ViewBag.VCodeError = "验证码错误";
                return View();
            }
            if (ModelState.IsValid)
            {
                var p = Request.Form["deliverprovince"];
                var c = Request.Form["delivercity"];
                var a = Request.Form["deliverarea"];
                //long admin_id =long.Parse(Request.Form["admin"]);
                switch (p)
                {
                    case "重庆市":
                        unitRegisterModel.U.管理单位.用户ID = 12;
                        break;
                    case "云南省":
                        unitRegisterModel.U.管理单位.用户ID = 13;
                        break;
                    case "贵州省":
                        unitRegisterModel.U.管理单位.用户ID = 14;
                        break;
                    case "西藏自治区":
                        unitRegisterModel.U.管理单位.用户ID = 15;
                        break;
                    default:
                        unitRegisterModel.U.管理单位.用户ID = 16;
                        break;
                }

                unitRegisterModel.U.所属地域.省份 = p;
                unitRegisterModel.U.所属地域.城市 = c;
                unitRegisterModel.U.所属地域.区县 = a;

                unitRegisterModel.U.登录信息.登录名 = unitRegisterModel.LoginName;
                unitRegisterModel.U.登录信息.密码 = unitRegisterModel.Pwd;

                unitRegisterModel.U.联系方式.联系人 = unitRegisterModel.ContactName;
                unitRegisterModel.U.联系方式.手机 = unitRegisterModel.ContactTel;
                unitRegisterModel.U.联系方式.固定电话 = unitRegisterModel.ContactPhone;
                unitRegisterModel.U.联系人职务 = unitRegisterModel.UnitDuty;

                unitRegisterModel.U.单位信息.单位名称 = unitRegisterModel.Unitname;
                unitRegisterModel.U.单位信息.单位代号 = unitRegisterModel.Codename;

                unitRegisterModel.U.审核数据.审核状态 = 审核状态.未审核;
                //TD:单位编码待处理
                unitRegisterModel.U.单位信息.单位编码 = string.Empty;

                var usergroup = Request.Form["usergroup"];
                if (!string.IsNullOrWhiteSpace(usergroup))
                {
                    var _f = usergroup.Split(',');
                    for (int i = 0; i < _f.Length - 1; i++)
                    {
                        unitRegisterModel.U.用户组.Add(_f[i]);
                    }
                }
                //unitRegisterModel.U.所属单位.用户ID = admin_id;
                用户管理.添加用户(unitRegisterModel.U);
                //TempData["RegisterMessage"] = "注册成功";
                return Content("<script>window.location='/注册/Successe_Regist?id=1';</script>");
            }

            return View();
        }

        public class ExpertApply
        {
            [Required(ErrorMessage = "密码必须填写")]
            [StringLength(30, MinimumLength = 6, ErrorMessage = "长度为6-30")]
            //[RegularExpression(@"^\w+$", ErrorMessage = "密码含有非法字符")]
            [Display(Name = "密码")]
            [DataType(DataType.Password)]
            public string Pwd { get; set; }

            [Required(ErrorMessage = "用户名必须填写")]
            [StringLength(30, MinimumLength = 6, ErrorMessage = "长度为6-30")]
            [RegularExpression(@"^[a-zA-Z]\w+$", ErrorMessage = "以字母开头(可用字母、数字或_)")]
            [System.Web.Mvc.Remote("CheckUserName", "注册", ErrorMessage = "该用户名已注册")]
            public string LoginName { get; set; }

            //[Required(ErrorMessage = "姓名必须填写")]
            //public string Name { get; set; }

            [Required(ErrorMessage = "电话必须填写")]
            [RegularExpression(@"^(13|14|15|16|18|19)\d{9}$", ErrorMessage = "格式不正确,应为：13598763567")]
            public string Phone { get; set; }

            //[Required(ErrorMessage = "单位必须填写")]
            //public string Unitname { get; set; }
            [Required(ErrorMessage = "验证码必须填写")]
            public string VerifyCode { get; set; }

            public 专家 U { get; set; }
            public ExpertApply() { U = new 专家(); }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Expert_Apply(ExpertApply expertapply)
        {
            var vc = this.HttpContext.Session["vcode"] as 验证码;
            if (null == vc || expertapply.VerifyCode == null || expertapply.VerifyCode.ToUpper() != vc.Code)
            {
                ViewBag.VCodeError = "验证码错误";
                return View();
            }

            if (ModelState.IsValid)
            {
                expertapply.U.登录信息.登录名 = expertapply.LoginName;
                expertapply.U.登录信息.密码 = expertapply.Pwd;
                expertapply.U.联系方式.手机 = expertapply.Phone;
                //expertapply.U.身份信息.姓名 = expertapply.Name;
                //expertapply.U.工作经历信息.工作单位 = expertapply.Unitname;
                用户管理.添加用户(expertapply.U);
                //TempData["RegisterMessage"] = "注册成功";
                return Content("<script>alert('注册成功！');window.location='/登录/Login/评审专家';</script>");
            }

            return View();
        }

        public ActionResult ProductClass()
        {
            string requesrid = Request.Params["classid"].ToString();
            try
            {
                string retstr = "";

                var lowerclass = 商品分类管理.查找子分类(long.Parse(requesrid));

                if (lowerclass.Any())
                {
                    foreach (var item in lowerclass)
                    {
                        retstr += "<option class=\"yjfn\" value=\"" + item.分类名 + "\" id=\"" + item.Id + "\">" + item.分类名 + "</option>";
                    }
                }
                return Content(retstr);

            }
            catch
            {
                return Content("加载失败");
            }
        }
    }
}
