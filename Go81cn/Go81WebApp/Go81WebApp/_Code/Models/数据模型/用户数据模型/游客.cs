
namespace Go81WebApp.Models.数据模型.用户数据模型
{
    public class 游客 : 用户基本数据
    {
        public static readonly 游客 Default = new 游客();
        private 游客()
        {
            this.登录信息.登录名 = "未登录";
            this.登录信息.显示名 = "游客";
        }
    }
}