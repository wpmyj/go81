using System;
using MongoDB.Bson.Serialization.Attributes;

namespace Go81WebApp.Models.数据模型.用户数据模型
{
    public class 运营团队 : 用户基本数据
    {
        public _运营团队工作人员信息 运营团队工作人员信息 { get; set; }
        public class _运营团队工作人员信息
        {
            public string 员工编号 { get; set; }
            public 运营职责 运营职责 { get; set; }
            public 岗位级别 岗位级别 { get; set; }
            public string 姓名 { get; set; }
            public 性别 性别 { get; set; }
            public string 身份证号 { get; set; }
            [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
            public DateTime 出生日期 { get; set; }
            public int 年龄 { get { return (int)((DateTime.Now - 出生日期).TotalDays / 365.25); } }
            [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
            public DateTime 入职日期 { get; set; }
            public int 工龄 { get { return (int)((DateTime.Now - 入职日期).TotalDays / 365.25); } }
        }
        [Flags]
        public enum 运营职责
        {
            未设置 = 0,
            客服 = 0x01,
            销售 = 0x02,
            管理员 = 0x04,
            超级管理员 = 0xFF,
        }
        public enum 岗位级别
        {
            未设置 = 0,
            经理 = 1,
            主管 = 2,
            副总 = 3,
            总经理 = 4,
        }
        public 运营团队()
        {
            this.运营团队工作人员信息 = new _运营团队工作人员信息();
        }
    }
}