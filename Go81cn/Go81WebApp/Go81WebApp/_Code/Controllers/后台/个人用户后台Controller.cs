using Go81WebApp.Models.管理器;
using Lucene.Net.Search;
using MongoDB.Bson;
using System.Web;
using System.Web.Mvc;
using MongoDB.Driver.Builders;
using Query = MongoDB.Driver.Builders.Query;
using Go81WebApp.Models.数据模型.用户数据模型;
using System.Collections.Generic;
using Go81WebApp.Models.数据模型.消息数据模型;
using System;
using System.IO;
using FileHelper;
using Go81WebApp.Models.数据模型.商品数据模型;
using MongoDB;

namespace Go81WebApp.Controllers.后台
{
    [登录验证]
    public class 个人用户后台Controller : Controller
    {
        private 个人用户 currentUser
        {
            get
            {
                return HttpContext.获取当前用户<个人用户>();
            }
        }
        
        //[单一权限验证(权限.专家库, 权限.合同范本)]
        public ActionResult Index()
        {
            IEnumerable<站内消息> Msg = 站内消息管理.查询收信人的站内消息(0, 0, currentUser.Id);
            int count = 0;
            foreach (var item in Msg)
            {
                if (item.基本数据.添加时间 > item.收信人.上次阅读时间)
                {
                    count++;
                }
            }
            ViewData["msg_count"] = count;
            ViewData["user"] = currentUser.登录信息.登录名;
            return View();
        }
        public ActionResult MyBaseInfo()
        {
            个人用户 per = 用户管理.查找用户<个人用户>(currentUser.Id);
            return View(per);
        }
        public ActionResult VipInfo()
        {
            个人用户 per = 用户管理.查找用户<个人用户>(currentUser.Id);
            return View(per);
        }
        public ActionResult Part_BackHead()
        {
            var m = currentUser;
            return PartialView("Person_Part/Part_BackHead", m);
        }
        public ActionResult PurchaseInfo()
        {
            long pgCount = 0;
            int cpg = 0;
            if (!string.IsNullOrWhiteSpace(Request.QueryString["page"]))
            {
                cpg = int.Parse(Request.QueryString["page"]);
            }
            if (cpg <= 0)
            {
                cpg = 1;
            }
            long pc = 购物车管理.计数购物车(0, 0, Query<购物车>.Where(m => m.所属用户.用户ID == currentUser.Id));
            pgCount = pc / 10;
            if (pc % 10 > 0)
            {
                pgCount++;
            }
            ViewData["Pagecount"] = pgCount;
            ViewData["CurrentPage"] = cpg;
            IEnumerable<购物车> cars = 购物车管理.查询购物车(10 * (cpg - 1), 10, Query<购物车>.Where(m => m.所属用户.用户ID == currentUser.Id));
            return View(cars);
        }

        public ActionResult PersonInfoManage(个人用户 per)
        {
            var step = Request.Form["step"];
            switch (step)
            {
                case "identity":
                    currentUser.个人信息 = per.个人信息;
                    break;
                case "attach":
                    if (!string.IsNullOrWhiteSpace(Request.Form["attachtext"]))
                    {
                        currentUser.登录信息.头像 = Request.Form["attachtext"];
                    }
                    break;
                case "contact":
                    currentUser.联系方式 = per.联系方式;
                    break;
                case "password":
                    var oldpwd = Request.Form["oldpwd"];
                    var newpwd = Request.Form["newpwd"];
                    var confirmpwd = Request.Form["confirmpwd"];

                    if (currentUser.登录信息.密码 != Hash.Compute(oldpwd.Trim()))
                    {
                        return Content("<script>alert('原密码错误！');window.location='/个人用户后台/MyBaseInfo'</script>");
                    }
                    else if (newpwd.Trim() != confirmpwd.Trim())
                    {
                        return Content("<script>alert('新密码两次输入不一致！');window.location='/个人用户后台/MyBaseInfo'</script>");
                    }
                    else
                    {
                        currentUser.登录信息.密码 = Hash.Compute(newpwd.Trim());
                    }
                    break;
            }

            
            用户管理.更新用户<个人用户>(currentUser);
            return Content("<script>alert('修改成功');window.location='/个人用户后台/MyBaseInfo'</script>");
        }

        [HttpPost]
        public ActionResult UploadPhoto(HttpPostedFileBase fileData)
        {
            if (fileData != null)
            {
                string fileName = fileData.FileName;
                string filePath = 上传管理.获取上传路径<个人用户>(媒体类型.头像, 路径类型.服务器本地路径);

                //string extension = fileData.FileName.Substring(fileData.FileName.LastIndexOf("."), (fileData.FileName.Length - fileData.FileName.LastIndexOf(".")));

                fileName = 上传管理.获取文件名(fileName).Replace("{", "").Replace("}", "");
                if (!Directory.Exists(filePath))
                {
                    Directory.CreateDirectory(filePath);
                }
                fileData.SaveAs(string.Format("{0}\\{1}", filePath, fileName));
                return Content(string.Format("{0}{1}", 上传管理.获取上传路径<个人用户>(媒体类型.头像, 路径类型.不带域名根路径), fileName));
            }
            else
            {
                return Content("0");
            }
        }
        [HttpPost]
        /// <summary>
        /// 删除头像
        /// </summary>
        /// <returns></returns>
        public ActionResult DeleteAttachment()
        {
            string qid = Request.Params["q"] ?? "";
            string name = Request.Params["n"] ?? "";
            try
            {
                if (!string.IsNullOrEmpty(qid) && !string.IsNullOrEmpty(name))
                {
                    UploadPic.DelPic(string.Format("{0}", name));
                    return Content(qid);
                }
                else
                {
                    return Content("0");
                }
            }
            catch { return Content("0"); }
        }
        //public string UploadAttachment(HttpPostedFileBase fileData)
        //{
        //    string extension = string.Empty;
        //    string fileName = string.Empty;
        //    if (fileData != null)
        //    {
        //        if (fileData.FileName != "" && fileData.FileName != null)
        //        {
        //            string filePath = 上传管理.获取上传路径<个人用户>(媒体类型.头像, 路径类型.服务器本地路径);

        //            extension = fileData.FileName.Substring(fileData.FileName.LastIndexOf("."), (fileData.FileName.Length - fileData.FileName.LastIndexOf(".")));

        //            fileName = 上传管理.获取文件名() + extension.ToLower();
        //            if (!Directory.Exists(filePath))
        //            {
        //                Directory.CreateDirectory(filePath);
        //            }
        //            fileData.SaveAs(string.Format("{0}\\{1}", filePath, fileName));
        //        }
        //        return fileName;
        //    }
        //    else
        //    {
        //        return null;
        //    }
        //}

       
    }
}
