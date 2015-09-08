using Go81WebApp.Models.管理器;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Go81WebApp.Models.数据模型.用户数据模型
{
    public abstract class 用户基本数据 : 基本数据模型
    {
        public _登录信息 登录信息 { get; set; }
        public _联系方式 联系方式 { get; set; }
        //[Required(ErrorMessage = "请选择所在地区！")]
        public _地域 所属地域 { get; set; }
        public List<_地址信息> 地址信息 { get; set; }
        public List<string> 角色 { get; set; }
        public List<string> 用户组 { get; set; }
        public string 用户类型 { get { return this.GetType().Name; } }
        public 用户基本数据()
        {
            this.登录信息 = new _登录信息();
            this.联系方式 = new _联系方式();
            this.所属地域 = new _地域();
            this.地址信息 = new List<_地址信息>();
            this.角色 = new List<string>();
            this.用户组 = new List<string>();
        }
    }
    public class _登录信息
    {
        [Required(ErrorMessage = "*用户名必须填写")]
        [StringLength(30, MinimumLength = 6, ErrorMessage = "长度为6-30")]
        [RegularExpression(@"^[a-zA-Z]\w+$", ErrorMessage = "以字母开头(可用字母、数字或_)")]
        [System.Web.Mvc.Remote("CheckUserName", "注册", ErrorMessage = "该用户名已存在")]
        public string 登录名 { get; set; }
        [Required(ErrorMessage = "*请填写登录密码")]
        [RegularExpression("[a-zA-Z0-9]{6,}", ErrorMessage = "请输入6位以上的有效密码")]
        [DataType(DataType.Password)]
        public string 密码 { get; set; }
        public string 显示名 { get; set; }
        public string 全名 { get { return string.IsNullOrWhiteSpace(显示名) ? 登录名 : 显示名 + '(' + 登录名 + ')'; } }
        public string 头像 { get; set; }
        public string 主题 { get; set; }
        public _登录信息()
        {
            this.头像 = 头像管理.默认头像;
        }
    }
    public enum 性别
    {
        未填写 = 0,
        男 = 1,
        女 = 2,
        保密 = 7,
    }
    public class _联系方式
    {
        [Required(ErrorMessage = "请填写联系人姓名")]
        public string 联系人 { get; set; }
        public 性别 性别 { get; set; }
        [RegularExpression("^\\s*([A-Za-z0-9_-]+(\\.\\w+)*@(\\w+\\.)+\\w{2,5})\\s*$", ErrorMessage = "请输入正确的邮箱号")]
        //[Required(ErrorMessage = "请填写联系人邮箱")]
        public string 电子邮件 { get; set; }
        [Required(ErrorMessage = "请填写联系人手机")]
        [RegularExpression(@"(13|14|15|16|18|19)\d{9}$", ErrorMessage = "请输入正确的手机格式")]
        public string 手机 { get; set; }
        [Required(ErrorMessage = "请填写联系人固定电话")]
        //[RegularExpression(@"(\d{3}-\d{8})|(\d{4}-\d{7})",ErrorMessage="正确格式为：区号-电话号码")]
        public string 固定电话 { get; set; }
        //[Required(ErrorMessage = "*请填写联系人QQ")]
        public string QQ { get; set; }
        //[Required(ErrorMessage = "*请填写联系人微信")]
        public string 微信 { get; set; }
        //[Required(ErrorMessage = "*请填写联系人微博")]
        public string 微博 { get; set; }
        //[Required(ErrorMessage = "*请填写联系人传真")]
        public string 传真 { get; set; }
        public string 其他 { get; set; }
    }
    public class _地址信息
    {
        public 地址类型 地址类型 { get; set; }
        //[Required(ErrorMessage="请输入联系人姓名")]
        public string 联系人 { get; set; }
        public string 传真 { get; set; }
        //[Required(ErrorMessage="*请输入手机号")]
        [RegularExpression("^[1][345789]+\\d{9}", ErrorMessage = "*请输入正确的手机号码")]
        public string 手机 { get; set; }
        //[Required(ErrorMessage="请输入固定电话")]
        [RegularExpression(@"\d{2,5}-\d{7,8}", ErrorMessage = "*正确格式为：区号-电话号码")]
        public string 固定电话 { get; set; }
        public string 传真电话 { get; set; }
        //[Required(ErrorMessage="*请输入电子邮件")]
        [RegularExpression(@"^\w+([-+.]\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*$", ErrorMessage = "*请输入正确的电子邮箱")]
        public string 电子邮件 { get; set; }
        //[Required(ErrorMessage="*请输入省份")]
        public string 省份 { get; set; }
        //[Required(ErrorMessage = "*请输入城市")]
        public string 城市 { get; set; }
        //[Required(ErrorMessage = "*请输入区县")]
        public string 区县 { get; set; }
        //[Required(ErrorMessage = "*请输入街道")]
        public string 街道 { get; set; }
        //[Required(ErrorMessage = "*请输入邮政编码")]
        [RegularExpression("[0-9]{6}", ErrorMessage = "*请输入正确的邮政编码")]
        public string 邮政编码 { get; set; }
    }
    public enum 地址类型
    {
        未指定 = 0,
        联络地址 = 1,
        单位地址 = 2,
        经营地址 = 3,
        收货地址 = 4,
        发货地址 = 5,
        工作地址 = 6,
        居住地址 = 7,
        其他 = 99,
    }
    public class 用户链接<T> where T : 用户基本数据
    {
        public long 用户ID { get; set; }
        public T 用户数据 { get { return 用户管理.查找用户(用户ID) as T; } }
        public 用户链接() { 用户ID = -1; }
    }
}
