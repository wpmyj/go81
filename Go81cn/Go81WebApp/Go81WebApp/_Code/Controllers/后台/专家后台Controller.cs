using Go81WebApp.Models.数据模型.用户数据模型;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Go81WebApp.Models.管理器;
using System.ComponentModel.DataAnnotations;
using Go81WebApp.Models.数据模型;
using System.IO;
using NPOI.SS.Formula.Functions;

namespace Go81WebApp._Code.Controllers.后台
{
    public class 专家后台Controller : Controller
    {
        // GET: Expert
        private 专家 currentUser
        {
            get
            {
                return HttpContext.获取当前用户<专家>();
            }
        }
        public ActionResult Download()
        {
            return View();
        }
        public ActionResult NoticeAboutApply()
        {
            return View();
        }
        public ActionResult Print()
        {
            return View();
        }
        public ActionResult Index()
        {
            return View(currentUser);
        }

        public ActionResult Part_BackHead()
        {
            var m = currentUser;
            return PartialView("Part_Expert/Part_BackHead",m);
        }

        public class password
        {
            [Required(ErrorMessage = "密码必须填写")]
            [StringLength(30, MinimumLength = 6, ErrorMessage = "长度为6-30")]
            [DataType(DataType.Password)]
            [Display(Name = "密码")]
            public string Pwd { get; set; }

            [System.ComponentModel.DataAnnotations.Compare("Pwd", ErrorMessage = "两次密码不一致")]
            [Display(Name = "确认密码")]
            public string PwdConfirm { get; set; }
        }
        public ActionResult Modify_Info()
        {
            return View(currentUser);
        }
        [HttpPost]
        public ActionResult ModifyPwd(password model)
        {
            if(ModelState.IsValid)
            {
                用户管理.修改登录密码<专家>(currentUser.Id,model.Pwd);
                return Content("<script>alert('修改密码成功');window.location='/登录/LogOut';</script>");
            }
            else
            {
                return Content("<script>alert('两次密码不一致');window.location='/专家后台/Modify_PWD';</script>");
            }
        }
        public ActionResult Modify_PWD()
        {
            return View();
        }
        public ActionResult Notice()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Modify(专家 model)
        {
            currentUser.联系方式.手机 = model.联系方式.手机;
            用户管理.更新用户<专家>(currentUser);
            return Content("<script>alert('修改成功！');window.location='/专家后台/'</script>");
        }

        public ActionResult ApplyOnline()
        {
            ViewData["goodType"] = 商品分类管理.查找子分类();

            ViewData["专家可评标类别"] = 专家可评标专业分类.评审专业;

            ViewData["专家特殊类别"] = 专家可评标专业.非商品分类评审专业;
            var model = 用户管理.查找用户<专家>(currentUser.Id);
            return View(model);
        }

        [HttpPost]
        public ActionResult AddExpert(专家 model)
        {
            var _zj = 用户管理.查找用户<专家>(currentUser.Id);
            model.身份信息.专家证电子扫描件 = _zj.身份信息.专家证电子扫描件;
            model.身份信息.证件电子扫描件 = _zj.身份信息.证件电子扫描件;
            model.工作经历信息.退休证书 = _zj.工作经历信息.退休证书;
            model.学历信息.最高学位证书 = _zj.学历信息.最高学位证书;
            model.学历信息.职称证书电子扫描件 = _zj.学历信息.职称证书电子扫描件;
            if (model.所属地域.地域 == "不限省份不限城市不限区县")
            {
                model.所属地域.省份 = "";
                model.所属地域.城市 = "";
                model.所属地域.区县 = "";
            }
            if (model.所属地域.城市 == "不限城市")
            {
                model.所属地域.城市 = "";
                model.所属地域.区县 = "";
            }
            if (model.所属地域.区县 == "不限区县")
            {
                model.所属地域.区县 = "";
            }

            var kpbtype = Request.Form["可参评物资类别列表"];
            var az = kpbtype.Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
            var cplb = new List<供应商._产品类别>();
            foreach (var y in az)
            {
                var yj = y.Split(':')[0];
                var ej = y.Split(':')[1].Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                var cp = new 供应商._产品类别();
                var listr = new List<string>();
                foreach (var j in ej)
                {
                    listr.Add(j);
                }
                cp.一级分类 = yj;
                cp.二级分类 = listr;
                cplb.Add(cp);
            }
           
            //List<供应商._产品类别> list = new List<供应商._产品类别>();
            //if (model.可参评物资类别列表 != null && model.可参评物资类别列表.Count() != 0)
            //{
            //    if (model.可参评物资类别列表.Count() == 1)
            //    {
            //        if (model.可参评物资类别列表[0].二级分类[0] == "-1")
            //        {
            //            供应商._产品类别 mm = new 供应商._产品类别();
            //            mm.一级分类 = model.可参评物资类别列表[0].一级分类;
            //            mm.二级分类 = new List<string>();
            //            list.Add(mm);
            //        }
            //        else
            //        {
            //            model.可参评物资类别列表[0].二级分类 = model.可参评物资类别列表[0].二级分类.Where(s => !string.IsNullOrEmpty(s)).ToList();
            //            if (model.可参评物资类别列表[0].二级分类.Count > 0)
            //            {
            //                供应商._产品类别 mm = new 供应商._产品类别();
            //                mm.一级分类 = model.可参评物资类别列表[0].一级分类;
            //                mm.二级分类 = model.可参评物资类别列表[0].二级分类;
            //                list.Add(mm);
            //            }
            //        }
            //    }
            //    else
            //    {
            //        for (int i = 0; i < model.可参评物资类别列表.Count; i++)
            //        {
            //            model.可参评物资类别列表[i].二级分类 = model.可参评物资类别列表[i].二级分类.Where(s => !string.IsNullOrEmpty(s)).ToList();
            //            if (model.可参评物资类别列表[i].二级分类.Count > 0)
            //            {
            //                供应商._产品类别 mm = new 供应商._产品类别();
            //                mm.一级分类 = model.可参评物资类别列表[i].一级分类;
            //                mm.二级分类 = model.可参评物资类别列表[i].二级分类;
            //                list.Add(mm);
            //            }
            //        }
            //    }
          //  }

            _zj.可参评物资类别列表 = cplb;
            _zj.所属管理单位 = model.所属管理单位;
            _zj.学历信息 = model.学历信息;
            _zj.工作经历信息 = model.工作经历信息;
            _zj.所属地域 = model.所属地域;
            _zj.身份信息 = model.身份信息;
            _zj.联系方式 = model.联系方式;
            用户管理.更新用户<专家>(_zj);
            return Content("<script>alert('申请成功！');window.location='ApplyOnline';</script>");
            //return RedirectToAction("Expert_Add");
        }

        public string UploadAttachment(HttpPostedFileBase fileData)
        {
            if (fileData != null)
            {
                string fileName = fileData.FileName;
                if (fileName != "" && fileName != null)
                {
                    string filePath = 上传管理.获取上传路径<专家>(媒体类型.图片, 路径类型.服务器本地路径);

                    //extension = fileData.FileName.Substring(fileData.FileName.LastIndexOf("."), (fileData.FileName.Length - fileData.FileName.LastIndexOf(".")));
                    if (fileName.LastIndexOf("\\") > -1)
                    {
                        fileName = fileName.Substring(fileName.LastIndexOf("\\") + 1);
                    }
                    fileName = 上传管理.获取文件名(fileName).Replace("{", "").Replace("}", "");
                    if (!Directory.Exists(filePath))
                    {
                        Directory.CreateDirectory(filePath);
                    }
                    fileData.SaveAs(string.Format("{0}\\{1}", filePath, fileName));
                }
                return fileName;
            }
            else
            {
                return null;
            }
        }

        public int DeletePic()
        {
            try
            {
                string uri = Request.QueryString["uri"];
                var type = Request.Params["type"];
                var zj = 用户管理.查找用户<专家>(currentUser.Id);
                if (type == "zjzgz"){zj.身份信息.专家证电子扫描件.Remove(uri);}
                if (type == "zczs"){zj.学历信息.职称证书电子扫描件.Remove(uri);}
                if (type == "zjsmj") { zj.身份信息.证件电子扫描件=""; }
                if (type == "txz") { zj.工作经历信息.退休证书=""; }
                if (type == "zgxwz"){  zj.学历信息.最高学位证书 = "";}
                用户管理.更新用户<专家>(zj);
                if (System.IO.File.Exists(Server.MapPath(@uri)))
                {
                    System.IO.File.Delete(Server.MapPath(@uri));
                    return 1;
                }
                else
                {
                    return 0;
                }
            }
            catch
            {
                return 0;
            }
        }

        [HttpPost]
        public ActionResult SavePicture()
        {
            try
            {
                string path = "";
                string zj_type = "";
                var model = 用户管理.查找用户<专家>(currentUser.Id);
                string name = Request.Form["pic1"];
                for (int i = 0; i < Request.Files.Count; i++)
                {
                    HttpPostedFileBase file = Request.Files[i];
                    if (file != null && (file.ContentType == "image/jpeg" || file.ContentType == "image/pjpeg" || file.ContentType == "image/x-png" || file.ContentType == "image/png") && (((file.ContentLength / 1024) / 1024) < 2))
                    {
                            string filePath = 上传管理.获取上传路径<专家>(媒体类型.图片, 路径类型.不带域名根路径);
                            string fname = UploadAttachment(file);
                            //path += filePath + fname + "|";
                            if (name == "zjzgz")//专家资格证
                            {
                                zj_type = "zjzgz";
                                model.身份信息.专家证电子扫描件.Add(filePath + fname);
                            }
                            else if (name == "zczs")//职称证书
                            {
                                zj_type = "zczs";
                                model.学历信息.职称证书电子扫描件.Add(filePath + fname);
                            }
                            else if (name == "zjsmj")//身份证或军官证
                            {
                                if (System.IO.File.Exists(Server.MapPath(@model.身份信息.证件电子扫描件)))
                                {
                                    System.IO.File.Delete(Server.MapPath(@model.身份信息.证件电子扫描件));
                                }
                                zj_type = "zjsmj";
                                model.身份信息.证件电子扫描件 = filePath + fname;
                            }
                            else if (name == "txz") //退休证
                            {
                                if (System.IO.File.Exists(Server.MapPath(@model.工作经历信息.退休证书)))
                                {
                                    System.IO.File.Delete(Server.MapPath(@model.工作经历信息.退休证书));
                                }
                                zj_type = "txz";
                                model.工作经历信息.退休证书 = filePath + fname;
                            }
                            else //最高学位证
                            {
                                if (System.IO.File.Exists(Server.MapPath(@model.学历信息.最高学位证书)))
                                {
                                    System.IO.File.Delete(Server.MapPath(@model.学历信息.最高学位证书));
                                }
                                zj_type = "zgxwz";
                                model.学历信息.最高学位证书 = filePath + fname;
                            }
                    }
                    else
                    {
                        ViewData["path"] = "上传失败";
                        return View();
                    }
                }
                switch (zj_type)
                {
                    case "zjzgz":
                        foreach (var item in model.身份信息.专家证电子扫描件)
                        {
                            path += item + "|";
                        }
                        break;
                    case "zczs":
                        foreach (var item in model.学历信息.职称证书电子扫描件)
                        {
                            path += item + "|";
                        }
                        break;
                    case "zjsmj":
                        path = model.身份信息.证件电子扫描件;
                        break;
                    case "txz":
                        path = model.工作经历信息.退休证书;
                        break;
                    case "zgxwz":
                        path = model.学历信息.最高学位证书;
                        break;

                }
                用户管理.更新用户<专家>(model);
               ViewBag.type = zj_type;
                ViewData["path"] = path;
               return View();
            }
            catch
            {
                ViewData["path"] = "上传出错!|";
                return View();
            }
        }


    }
}