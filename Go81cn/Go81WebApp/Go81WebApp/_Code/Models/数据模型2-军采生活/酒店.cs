using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Go81WebApp.Models.数据模型;
using Go81WebApp.Models.数据模型.用户数据模型;
using System.ComponentModel.DataAnnotations;
using Microsoft.Ajax.Utilities;
using MongoDB.Bson.Serialization.Attributes;
using WebGrease.Css.Extensions;

namespace Go81WebApp.Models.数据模型
{
    public class 酒店 : 基本数据模型
    {
        public 用户链接<供应商> 所属供应商 { get; set; }
        public _酒店基本信息 酒店基本信息 { get; set; }
        public _酒店设施 酒店设施 { get; set; }
        public _酒店服务 酒店服务 { get; set; }
        public List<_房间信息> 房间信息 { get; set; }
        public _审核数据 审核数据 { get; set; }
        public class _酒店基本信息
        {
            [Required(ErrorMessage = "请填写酒店名称")]
            public string 酒店名 { get; set; }
            [Required(ErrorMessage = "请填写酒店所在地址")]
            public string 地址 { get; set; }
            public string 所属商圈 { get; set; }
            public string 简介 { get; set; }
            [Required(ErrorMessage = "请填写酒店联系电话")]
            [RegularExpression(@"^(13|14|15|16|18|19)\d{9}$", ErrorMessage = "*请输入正确的手机格式")]
            public string 联系电话 { get; set; }
            public string 交通信息 { get; set; }
            [Required(ErrorMessage="请填写入住和离店时间")]
            public string 入住和离店时间 { get; set; }
            public List<string> 照片 { get; set; }
            public bool Wifi { get; set; }
            public bool 免费停车场 { get; set; }
            public _地域 所属地域 { get; set; }
            public double[] 地理位置 { get; set; }
            public _酒店基本信息()
            {
                this.照片 = new List<string>();
                this.所属地域 = new _地域();
            }
        }
        public class _酒店设施
        {
            public bool 西式餐厅 { get; set; }
            public bool 中式餐厅 { get; set; }
            public bool 残疾人设施 { get; set; }
            public bool 室外游泳池 { get; set; }
            public bool 室内游泳池 { get; set; }
            public bool 会议室 { get; set; }
            public bool 健身房 { get; set; }
            public bool SPA { get; set; }
            public bool 无烟房 { get; set; }
            public bool 商务中心 { get; set; }
            public bool 桑拿 { get; set; }
            public bool 温泉 { get; set; }
            public bool 棋牌 { get; set; }
        }
        public class _酒店服务 
        {
            public bool 接站服务 { get; set; }
            public bool 接机服务 { get; set; }
            public bool 接待外宾 { get; set; }
            public bool 洗衣服务 { get; set; }
            public bool 行李寄存 { get; set; }
            public bool 租车 { get; set; }
            public bool 携带宠物 { get; set; }
            public bool 叫醒服务 { get; set; }
        }
        public class _房间设施
        {
            public bool 宽带上网 { get; set; }
            public bool 免费市内电话 { get; set; }
            public bool 空调 { get; set; }
            public bool 国际长途通话 { get; set; }
            public bool 免费国内长途通话 { get; set; }
            public bool 吹风机 { get; set; }
            public bool 暖气 { get; set; }
            public bool 二十四小时热水 { get; set; }        
        }
        public class _房间信息
        {
            public string 房型 { get; set; }
            public string 价格 { get; set; }
            public string 床型 { get; set; }
            public string 早餐{get;set;}
            public string 简介 { get; set; }
            public List<string> 图片 { get; set; }
            public _房间设施 房间设施 { get; set; }
            public _房间信息()
            {
                this.图片 = new List<string>();
                this.房间设施 = new _房间设施();
            }
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
       
        public 酒店()
        {
            this.房间信息 = new List<_房间信息>();
            this.所属供应商 = new 用户链接<供应商>();
            this.审核数据 = new _审核数据();
            this.酒店基本信息 = new _酒店基本信息();
            this.酒店设施 = new _酒店设施();
            this.酒店服务 = new _酒店服务();
        }
    }

    public static class 地理位置数据
    {
        public static readonly Dictionary<string, Dictionary<string, Dictionary<string, List<string>>>> 商圈 =
            new Dictionary<string, Dictionary<string, Dictionary<string, List<string>>>>();
        private static void 添加商圈数据(string 省, string 市, string 区县, params string[] 商圈名列表)
        {
            if (string.IsNullOrWhiteSpace(省)) return;
            if (!商圈.ContainsKey(省))
                商圈[省] = new Dictionary<string, Dictionary<string, List<string>>>();
            if (string.IsNullOrWhiteSpace(市)) return;
            if (!商圈[省].ContainsKey(市))
                商圈[省][市] = new Dictionary<string, List<string>>();
            if (string.IsNullOrWhiteSpace(区县)) return;
            if (!商圈[省][市].ContainsKey(区县))
                商圈[省][市][区县] = new List<string>();
            if (0 == 商圈名列表.Length) return;
            商圈[省][市][区县].AddRange(商圈名列表.Where(s => !商圈[省][市][区县].Contains(s)));
        }

        static 地理位置数据()
        {
            添加商圈数据("四川", "成都", "双流县", new[]
            {
                "双流国际机场",
                "黄龙溪"
            });
            添加商圈数据("四川", "成都", "都江堰市", new[]
            {
                "青城山",
                "青城后山"
            });
            添加商圈数据("四川", "成都", "邛崃市", new[]
            {
                "平乐镇",
                "天台山景区"
            });
            添加商圈数据("四川", "成都", "大邑县", new[]
            {
                "花水湾",
                "西岭雪山",
                "安仁古镇",
            });
            添加商圈数据("四川", "成都", "金牛区", new[]
            {
                "成都北站",
                "营门口",
                "茶店子",
            });
            添加商圈数据("四川", "成都", "锦江区", new[]
            {
                "九眼桥",
                "喜年广场",
                "万达广场",
            });
            添加商圈数据("四川", "成都", "青羊区", new[]
            {
                "人民公园",
                "宽窄巷子",
            });
            添加商圈数据("四川", "成都", "成华区", new[]
            {
                "电子科技大学",
                "磨子桥",
                "万年场",
            });
            添加商圈数据("四川", "成都", "市中心区", new[]
            {
                "武侯祠",
                "锦里古街",
                "华西医院",
            });
            添加商圈数据("四川", "绵阳", "渭城区", new[]
            {
                "咸阳国际机场",
                "含光路",
            });
            添加商圈数据("四川", "绵阳", "新城区", new[]
            {
                "西安站",
                "北大街",
            });
        }
    }
}
