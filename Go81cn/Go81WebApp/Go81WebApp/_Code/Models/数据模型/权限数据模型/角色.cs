using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Go81WebApp.Models.数据模型.权限数据模型
{
    public class 角色 : 基本数据模型
    {
        [Required(ErrorMessage = "请填写角色名称")]
        [System.Web.Mvc.Remote("CheckRoleName", "运营团队后台", ErrorMessage = "该用角色名已存在")]
        public string 角色名 { get; set; }
        public List<string> 包含用户组 { get; set; }
        public 角色(){ this.包含用户组 = new List<string>(); }
    }
}