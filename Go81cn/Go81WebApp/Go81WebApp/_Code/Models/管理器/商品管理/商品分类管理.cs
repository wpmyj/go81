using System.Linq;
using Go81WebApp.Models.数据模型.商品数据模型;
using MongoDB;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using System.Collections.Generic;

namespace Go81WebApp.Models.管理器
{
    public static class 商品分类管理
    {
        public static 商品分类 添加分类(string catName, long parentCatID = -1, bool fn = false, int Num = 999999999, bool normal = true, bool agreement = false, bool emergency = false)
        {
            商品分类 pc = null;
            if (-1 != parentCatID)
            {
                pc = 查找分类(parentCatID);
                if (null == pc) return null;
            }
            var c = new 商品分类()
            {
                Id = -1,
                序号 = Num,
                分类名 = catName,
                普通采购 = normal,
                协议采购 = agreement,
                应急采购 = emergency,
            };
            if (fn)
            {
                c.分类性质 = 商品分类性质.专用物资;
            }
            else
            {
                c.分类性质 = 商品分类性质.通用物资;
            }
            if (null != pc)
            {
                c.父分类.商品分类ID = pc.Id;
                //c.父分类.商品分类名 = pc.分类名;
            }
            
            return Mongo.添加(c)
                ? c
                : null
                ;
        }
        public static bool 更新分类(商品分类 cat, bool updateModifiedTime = true)
        {
            return Mongo.更新(cat, updateModifiedTime);
        }
        public static bool 删除分类(long catID)
        {
            return Mongo.删除<商品分类>(catID);
        }
        public static bool 重命名分类(long catID, string newCatName)
        {
            return 1 == Mongo.更新<商品分类>(
                Query.EQ("_id", catID),
                Update.Set("分类名", newCatName)
                );
        }
        public static bool 重命名模板属性分类(long catID, string oldPropGroupName, string newPropGroupName, bool includeProducts = false)
        {
            if (includeProducts)
            {
                var c = 查找分类(catID).获取分类下商品列表(true);
                //TODO
            }

            return 1 == Mongo.更新<商品分类>(
                Query.EQ("_id", catID),
                Update.Rename("商品属性模板." + oldPropGroupName, "商品属性模板." + newPropGroupName)
                );
        }
        public static bool 重命名模板属性(long catID, string propGroupName, string oldPropGroupName, string newPropGroupName, bool includeProducts = false)
        {
            if (includeProducts)
            {
                var c = 查找分类(catID).获取分类下商品列表(true);
                //TODO
            }

            return 1 == Mongo.更新<商品分类>(
                Query.EQ("_id", catID),
                Update.Rename("商品属性模板." + propGroupName + '.' + oldPropGroupName, "商品属性模板." + propGroupName + '.' + newPropGroupName)
                );
        }

        public static 商品分类 查找分类(long catID)
        {
            return Mongo.查找<商品分类>(catID);
        }
        public static 商品分类 查找分类(string catName,bool num = true)
        {
            var l = Mongo.查询<商品分类>(0, 0, Query<商品分类>.EQ(o => o.分类名, catName));
            return l.Any() ? (num?l.First():l.Last()) : null;
        }

        public static IEnumerable<商品分类> 查找子分类(long parentCatID = -1)
        {
            return Mongo.查询<商品分类>(0, 0, Query.EQ("父分类.商品分类ID", parentCatID), false, SortBy.Ascending("序号"));
        }
        public static IEnumerable<商品分类> 查找三级分类(long parentCatID = -1)
        {
            var sonClassify = 查找子分类(parentCatID);
            List<商品分类> grandClassify = new List<商品分类>();
            foreach (var item in sonClassify)
            {
                var fn = 查找子分类(item.Id);
                if (fn!=null && fn.Count()>0)
                {
                    foreach (var k in fn)
                    {
                        grandClassify.Add(k);
                    }
                }
                else
                {
                    grandClassify.Add(item);
                }
            }
            return grandClassify as IEnumerable<商品分类>;
        }
        public static IEnumerable<商品分类> 查询商品分类(int skip, int limit, IMongoQuery conditions = null, bool modifiedDescending = true, IMongoSortBy sorting = null, bool includeDisabled = true, bool includeDeleted = false)
        {
            return Mongo.查询<商品分类>(skip, limit, conditions, modifiedDescending, sorting, includeDisabled, includeDeleted);
        }
    }
}