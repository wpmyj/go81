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
            ViewData["mysex"] = MySex(性别.男.ToString());
            个人用户 per = 用户管理.查找用户<个人用户>(currentUser.Id);
            return View(per);
        }
        public ActionResult VipInfo()
        {
            个人用户 per = 用户管理.查找用户<个人用户>(currentUser.Id);
            return View(per);
        }

        public ActionResult PersonInfoManage(个人用户 per)
        {
            if (!string.IsNullOrEmpty(Request.Form["attachtext"]))
            {
                currentUser.登录信息.头像 = Request.Form["attachtext"];
            }
            
            currentUser.个人信息 = per.个人信息;
            currentUser.联系方式 = per.联系方式;
            用户管理.更新用户<个人用户>(currentUser);
            return RedirectToAction("MyBaseInfo");
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

        public static SelectList MySex(String option)
        {
            List<SelectListItem> items = new List<SelectListItem>()
            {
                new SelectListItem(){Text=性别.男.ToString(),Value=性别.男.ToString()},
                new SelectListItem(){Text=性别.女.ToString(),Value=性别.女.ToString()},
            };
            SelectList selectlist = new SelectList(items, "Text", "Value", option);
            return selectlist;
        }
    }
}
