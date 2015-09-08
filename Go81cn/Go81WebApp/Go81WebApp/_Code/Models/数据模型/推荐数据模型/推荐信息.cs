using System;
using System.ComponentModel.DataAnnotations;
using Go81WebApp.Models.数据模型.用户数据模型;
using MongoDB.Bson.Serialization.Attributes;

namespace Go81WebApp.Models.数据模型.推荐数据模型
{
    public class 推荐信息 : 基本数据模型
    {
        public 推荐类型 推荐类型 { get; set; }
        [Required(ErrorMessage = "必须填写")]
        public string 名称 { get; set; }
        [Required(ErrorMessage = "必须填写")]
        public string 推荐理由 { get; set; }
        public _联系方式 联系方式 { get; set; }
        public 用户链接<单位用户> 推荐人 { get; set; }
        public _推荐审核数据 推荐审核数据 { get; set; }
        public _推荐审核数据 推荐审核数据2 { get; set; }
        public _推荐审核数据 推荐审核数据3 { get; set; }
        public 推荐信息()
        {
            this.联系方式 = new _联系方式();
            this.推荐人 = new 用户链接<单位用户>();
            this.推荐审核数据 = new _推荐审核数据();
            this.推荐审核数据2= new _推荐审核数据();
            this.推荐审核数据3= new _推荐审核数据();
        }
    }
    public enum 推荐类型
    {
        未设置 = 0,
        供应商 = 1,
        专家 = 2,
        酒店 = 3,
        机票代售点 = 4
    }
    public enum 推荐状态
    {
        待联系 = 0,
        推荐通过 = 1,
        推荐未通过 = 2
    }
    public class _推荐审核数据
    {
        public 推荐状态 推荐状态 { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime 审核时间 { get; set; }
        public 用户链接<用户基本数据> 审核者 { get; set; }
        public _推荐审核数据() { this.审核者 = new 用户链接<用户基本数据>(); }
    }
}