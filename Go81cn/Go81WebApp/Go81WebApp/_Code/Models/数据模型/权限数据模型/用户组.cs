using Go81WebApp.Models.数据模型.用户数据模型;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Go81WebApp.Models.数据模型.权限数据模型
{
    public class 用户组 : 基本数据模型
    {
        [Required(ErrorMessage = "请填写用户组名称")]
        [System.Web.Mvc.Remote("CheckUserName", "运营团队后台", ErrorMessage = "该用户组名已存在")]
        public string 用户组名 { get; set; }
        public 用户链接<单位用户> 所属单位 { get; set; }
        public List<string> 权限列表 { get; set; }

        public 用户组()
        {
            权限列表 = new List<string>();
            所属单位 = new 用户链接<单位用户>();
        }
    }
}