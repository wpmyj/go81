using System;
using MongoDB.Bson.Serialization.Attributes;

namespace Go81WebApp.Models.数据模型.用户数据模型
{
    public class 个人用户 : 用户基本数据
    {
        public _个人信息 个人信息 { get; set; }
        public _会员信息 会员信息 { get; set; }
        public class _个人信息
        {
            public string 姓名 { get; set; }
            public 性别 性别 { get; set; }
            public string 身份证号 { get; set; }
            [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
            public DateTime 出生日期 { get; set; }
        }
        public class _会员信息
        {
            public string 会员编号 { get; set; }
            public 会员等级 会员等级 { get; set; }
            public int 会员积分 { get; set; }
            public int 虚拟电子币 { get; set; }
        }
        public enum 会员等级
        {
            普通 = 0,
            VIP1 = 1,
            VIP2 = 2,
            VIP3 = 3,
            VIP4 = 4,
            VIP5 = 5,
            VIP6 = 6,
            VIP7 = 7,
            VIP8 = 8,
            VIP9 = 9,
        }
        public 个人用户()
        {
            个人信息 = new _个人信息();
            会员信息 = new _会员信息();
        }
        private static readonly int[] VIP积分等级 =
        {
            1000,
            3000,
            5000,
            7500,
            10000,
            50000,
            100000,
            200000,
            500000,
            1000000,
        };
    }

    public class 优惠码 : 基本数据模型
    {
        public string WeChatUser { get; set; }
        public string Code { get; set; }
    }
}
