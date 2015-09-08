using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Go81WebApp.Models.数据模型;
using Go81WebApp.Models.数据模型.用户数据模型;
using System.ComponentModel.DataAnnotations;
using MongoDB.Bson.Serialization.Attributes;

namespace Go81WebApp.Models.数据模型
{
    public class 租车企业 : 基本数据模型
    {
        public 用户链接<供应商> 所属供应商 { get; set; }
        public _企业信息 企业信息 { get; set; }
        public List<_车辆信息> 车辆信息 { get; set; }
        public _审核数据 审核数据 { get; set; }
        public class _企业信息
        {
            public string 企业名称 { get; set; }
            public string 企业简介 { get; set; }
            public string 企业地址 { get; set; }
            public string 联系电话 { get; set; }
            public string 企业图片 { get; set; }
            public double[] 地理位置 { get; set; }
        }
        public class _车辆信息
        {
            public 品牌 品牌 { get; set; }
            public 级别 级别 { get; set; }
            public int 座位数 { get; set; }
            public int 车门数 { get; set; }
            public 燃料类型 燃料类型 { get; set; }
            public 变速箱类型 变速箱类型 { get; set; }
            public 驱动方式 驱动方式 { get; set; }
            public bool 天窗 { get; set; }
            public string 油箱容量 { get; set; }
            public int 音箱数量 { get; set; }
            public string 座椅 { get; set; }
            public bool GPS导航 { get; set; }
            public bool 倒车雷达 { get; set; }
            public int 气囊数量 { get; set; }
            public decimal 日租金 { get; set; }
            public List<string> 图片 { get; set; }
            public _车辆信息()
            {
                this.图片=new List<string>();
            }
        }
        public enum DVD或CD
        {
            DVD,
            CD
        }
        public enum 发动机进气形式
        {
            自然吸气,
            涡轮增压,
            机械增压
        }
        public enum 驱动方式
        {
            前驱,
            后驱
        }
        public enum 燃料类型
        {
            天然气,
            石油
        }
        public enum 变速箱类型
        {
            手动变速箱,
            普通自动变速箱,
            CVT无级变速箱,
            CVT带挡位的变速箱,
            双离合变速箱,
            AMT,
            序列变速箱
        }
        public enum 级别
        {
            不限,
            手动紧凑型轿车,
            经济型轿车,
            商务型轿车,
            豪华型轿车,
            SUV,
            个性车
        }
        public enum 品牌
        {
            不限,
            奥迪,
            宝马,
            奔驰,
            本田,
            标致,
            别克,
            大众,
            丰田,
            福特,
            金杯,
            凯迪拉克,
            铃木,
            马自达,
            奇瑞,
            起亚,
            斯柯达,
            现代,
            雪佛兰,
            雪铁龙,
            中华,
            猎豹,
            日产,
            沃尔沃,
            依维柯,
            华泰,
            野马,
            三菱,
            MINI,
            雪铁龙DS,
            北京汽车,
            菲亚特,
            江淮
        }
        public class _审核数据
        {
            public 审核状态 审核状态 { get; set; }
            [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
            public DateTime 审核时间 { get; set; }
            public string 审核不通过原因 { get; set; }
            public 用户链接<运营团队> 审核者 { get; set; }
            public _审核数据() { this.审核者 = new 用户链接<运营团队>(); }
        }
        public 租车企业()
        {
            this.车辆信息 = new List<_车辆信息>();
            this.企业信息 = new _企业信息();
            this.审核数据 = new _审核数据();
            this.所属供应商 = new 用户链接<供应商>();
        }
    }
}
