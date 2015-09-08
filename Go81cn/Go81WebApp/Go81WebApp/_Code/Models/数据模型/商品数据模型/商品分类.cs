using Go81WebApp.Models.管理器;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Options;

namespace Go81WebApp.Models.数据模型.商品数据模型
{
    public class 商品分类 : 基本数据模型
    {
        public int 序号 { get; set; }
        public string 分类名 { get; set; }
        public 商品分类性质 分类性质 { get; set; }
        //public string 前置分类 { get { var c = 父分类.商品分类; return c.前置分类 + '.' + c.分类名; } }
        public bool 普通采购 { get; set; }
        public bool 协议采购 { get; set; }
        public bool 应急采购 { get; set; }
        public 商品分类链接 父分类 { get; set; }
        public IEnumerable<商品分类> 子分类 { get { return 商品分类管理.查找子分类(Id); } }
        public IEnumerable<商品> 商品列表 { get { return 获取分类下商品列表(false); } }
        [BsonDictionaryOptions(DictionaryRepresentation.Document)]
        public Dictionary<string, Dictionary<string, 商品属性数据>> 商品属性模板 { get; set; }
        public List<decimal> 价格分段 { get; set; }
        public 商品分类()
        {
            this.序号 = 999999999;
            this.父分类 = new 商品分类链接();
            this.普通采购 = true;
            this.商品属性模板 = new Dictionary<string, Dictionary<string, 商品属性数据>>();
            this.价格分段 = new List<decimal>();
        }
        public List<商品分类> 添加子分类(商品分类性质 attr, params string[] cats)
        {
            var cl = new List<商品分类>(cats.Length);
            foreach (var c in cats)
            {
                var c0 = 商品分类管理.添加分类(c, Id);
                c0.分类性质 = attr;
                cl.Add(c0);
            }
            return cl;
        }
        public List<商品分类> 添加子分类(params string[] cats)
        {
            return 添加子分类(商品分类性质.通用物资, cats);
        }
        public IEnumerable<商品> 获取分类下商品列表(bool includeDisabled)
        {
            return 商品管理.查询分类下商品(Id, includeDisabled: includeDisabled);
        }
    }
    public enum 商品分类性质
    {
        通用物资 = 0,
        专用物资 = 1,
    }
    public class 商品属性数据
    {
        public int 频率 { get; set; }
        public bool 必需 { get; set; }
        public bool 销售属性 { get; set; }
        public 属性类型 属性类型 { get; set; }
        public List<string> 值 { get; set; }
        public 商品属性数据() { this.值 = new List<string>(); }
    }
    public enum 属性类型
    {
        复选 = 0,
        输入 = 1,
    }
    public class 商品分类链接
    {
        public long 商品分类ID { get; set; }
        public 商品分类 商品分类 { get { return 商品分类管理.查找分类(商品分类ID); } }
        public 商品分类链接() { this.商品分类ID = -1; }
    }
}
