using Go81WebApp.Models.管理器;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver.Builders;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using NPOI.OpenXml4Net.OPC.Internal;

namespace Go81WebApp.Models.数据模型.用户数据模型
{
    public class 单位用户 : 用户基本数据
    {
        public static readonly Dictionary<string, 用户链接<单位用户>> 公告接收单位 = new Dictionary<string, 用户链接<单位用户>>
        {
            {"四川省", new 用户链接<单位用户> {用户ID = 11}},
            {"重庆市", new 用户链接<单位用户> {用户ID = 12}},
            {"云南省", new 用户链接<单位用户> {用户ID = 13}},
            {"贵州省", new 用户链接<单位用户> {用户ID = 14}},
            {"西藏自治区", new 用户链接<单位用户> {用户ID = 15}},
        };
        public string 验收单名称 { get; set; }
        public string 验收单审核单位名称 { get; set; }
        public string 印章底部文本 { get; set; }

        public _审核数据 审核数据 { get; set; }
        public 用户链接<单位用户> 管理单位 { get; set; }
        public 用户链接<单位用户> 所属单位 { get; set; }
        public _单位信息 单位信息 { get; set; }

        [Required(ErrorMessage = "请填写联系人职务")]
        public string 联系人职务 { get; set; }
        public class _审核数据
        {
            public 审核状态 审核状态 { get; set; }
            public 用户链接<用户基本数据> 审核者 { get; set; }
            [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
            public DateTime 审核时间 { get; set; }
            public string 审核不通过原因 { get; set; }
            public _审核数据() { this.审核状态 = 审核状态.未审核; this.审核者 = new 用户链接<用户基本数据>(); this.审核时间 = DateTime.MinValue; }
        }
        public class _单位信息
        {
            [Required(ErrorMessage = "请填写单位名称")]
            public string 单位名称 { get; set; }
            public string 单位编码 { get; set; }
            //[Required(ErrorMessage = "请填写单位代号")]
            public string 单位代号 { get; set; }
            //public string 单位对外名称 { get; set; }
            public 单位级别 单位级别 { get; set; }
            [Required(ErrorMessage = "请填写所属单位")]
            public string 所属单位 { get; set; }
        }
        public enum 单位级别
        {
            未设置 = 0,
            正军级 = 1,
            副军级 = 2,
            正师级 = 3,
            副师级 = 4,
            正团级 = 5,
            副团级 = 6,
            正营级 = 7,
            副营级 = 8,
            营级以下 = 99,
        }
        public 单位用户()
        {
            this.审核数据 = new _审核数据();
            this.管理单位 = new 用户链接<单位用户>();
            this.单位信息 = new _单位信息();
            this.联系人职务 = string.Empty;
            this.所属单位 = new 用户链接<单位用户>();
        }
        public long 计数管辖单位(int skip = 0, int limit = 0)
        {
            return 用户管理.计数用户<单位用户>(skip, limit, Query<单位用户>.EQ(o => o.管理单位.用户ID, Id));
        }
        public IEnumerable<单位用户> 获取管辖单位(int skip, int limit)
        {
            return 用户管理.查询用户<单位用户>(skip, limit, Query<单位用户>.EQ(o => o.管理单位.用户ID, Id));
        }
        public long 计数未审核管辖单位(int skip = 0, int limit = 0)
        {
            return 用户管理.计数用户<单位用户>(skip, limit,
                Query.And(Query<单位用户>.EQ(o => o.管理单位.用户ID, Id), Query<单位用户>.EQ(o => o.审核数据.审核状态, 审核状态.未审核)));
        }
        public IEnumerable<单位用户> 获取未审核管辖单位(int skip, int limit)
        {
            return 用户管理.查询用户<单位用户>(skip, limit,
                Query.And(Query<单位用户>.EQ(o => o.管理单位.用户ID, Id), Query<单位用户>.EQ(o => o.审核数据.审核状态, 审核状态.未审核)));
        }
        public long 计数已审核管辖单位(int skip = 0, int limit = 0)
        {
            return 用户管理.计数用户<单位用户>(skip, limit,
                Query.And(Query<单位用户>.EQ(o => o.管理单位.用户ID, Id), Query<单位用户>.Where(o => o.审核数据.审核状态 != 审核状态.未审核)));
        }
        public IEnumerable<单位用户> 获取已审核管辖单位(int skip, int limit)
        {
            return 用户管理.查询用户<单位用户>(skip, limit,
                Query.And(Query<单位用户>.EQ(o => o.管理单位.用户ID, Id), Query<单位用户>.Where(o => o.审核数据.审核状态 != 审核状态.未审核)));
        }
        public class 单位级别模型
        {
            public long id;
            public string name;
            public List<单位级别模型> 下级单位列表;

            public 单位级别模型()
            {
                this.下级单位列表 =new List<单位级别模型>();
            }
        }

        public static readonly List<单位级别模型> 单位级别列表 = new List<单位级别模型>
        {
            new 单位级别模型
            {
                id = 100,
                name = "成都军区司令部",
                下级单位列表 = new List<单位级别模型>
                {
                    new 单位级别模型
                    {
                        id=110,
                        name = "二级部",
                        下级单位列表 = new List<单位级别模型>
                        {
                            new 单位级别模型
                            {
                                id=111,
                                name="处",
                                下级单位列表 = new List<单位级别模型>()
                            }
                        }
                    },
                    new 单位级别模型
                    {
                        id=120,
                        name = "直属单位1",
                        下级单位列表 = new List<单位级别模型>
                        {
                            new 单位级别模型
                            {
                                id=1210,
                                name="具体部门1",
                                下级单位列表 = new List<单位级别模型>()
                            },
                            new 单位级别模型
                            {
                                id=1211,
                                name="具体部门2",
                                下级单位列表 = new List<单位级别模型>()
                            },
                            new 单位级别模型
                            {
                                id=1212,
                                name="具体部门3",
                                下级单位列表 = new List<单位级别模型>
                                {
                                    new 单位级别模型
                                    {
                                        id=1211,
                                        name="具体部门111111",
                                        下级单位列表 = new List<单位级别模型>()
                                    }
                                }
                            }
                        }
                    },
                    new 单位级别模型
                    {
                        id=130,
                        name = "直属单位2",
                        下级单位列表 = new List<单位级别模型>
                        {
                            new 单位级别模型
                            {
                                id=131,
                                name="具体部门",
                                下级单位列表 = new List<单位级别模型>()
                            }
                        }
                    },
                }
            },
            new 单位级别模型
            {
                id = 200,
                name = "成都军区政治部",
                下级单位列表 = new List<单位级别模型>
                {
                    new 单位级别模型
                    {
                        id=210,
                        name = "二级部2",
                        下级单位列表 = new List<单位级别模型>
                        {
                            new 单位级别模型
                            {
                                id=211,
                                name="处2",
                                下级单位列表 = new List<单位级别模型>()
                            }
                        }
                    },
                    new 单位级别模型
                    {
                        id=220,
                        name = "直属单位2",
                        下级单位列表 = new List<单位级别模型>
                        {
                            new 单位级别模型
                            {
                                id=221,
                                name="具体部门21",
                                下级单位列表 = new List<单位级别模型>()
                            },
                            new 单位级别模型
                            {
                                id=222,
                                name="具体部门22",
                                下级单位列表 = new List<单位级别模型>()
                            },
                            new 单位级别模型
                            {
                                id=223,
                                name="具体部门23",
                                下级单位列表 = new List<单位级别模型>()
                            }
                        }
                    },
                    new 单位级别模型
                    {
                        id=230,
                        name = "直属单位222",
                        下级单位列表 = new List<单位级别模型>
                        {
                            new 单位级别模型
                            {
                                id=231,
                                name="具体部门222",
                                下级单位列表 = new List<单位级别模型>()
                            }
                        }
                    },
                }
            }
        };

    }
}