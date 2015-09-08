using Go81WebApp.Models.数据模型;
using MongoDB;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Collections.Generic;

namespace Go81WebApp.Models.管理器
{
    public abstract class 管理器<T> where T : 基本数据模型
    {
        public bool 添加(T content)
        {
            return Mongo.添加(content);
        }
        public bool 更新(T content)
        {
            return Mongo.更新(content);
        }
        public bool 屏蔽(long id)
        {
            return Mongo.屏蔽<T>(id);
        }
        public bool 屏蔽解除(long id)
        {
            return Mongo.屏蔽解除<T>(id);
        }
        public bool 删除(long id)
        {
            return Mongo.删除<T>(id);
        }
        public T 查找(long id)
        {
            return Mongo.查找<T>(id);
        }
        public IEnumerable<T> 查询(int skip, int limit, IMongoQuery conditions = null, bool modifiedDescending = true, IMongoSortBy sorting = null, bool includeDisabled = true, bool includeDeleted = false)
        {
            return Mongo.查询<T>(skip, limit, conditions, modifiedDescending, sorting, includeDisabled, includeDeleted);
        }
        public IEnumerable<BsonDocument> 列表(int skip, int limit, IMongoFields fields, IMongoQuery conditions = null, bool modifiedDescending = true, IMongoSortBy sorting = null, bool includeDisabled = true, bool includeDeleted = false)
        {
            return Mongo.列表<T>(skip, limit, fields, conditions, modifiedDescending, sorting, includeDisabled, includeDeleted);
        }
        public long 计数(int skip, int limit, IMongoQuery conditions = null, bool includeDisabled = true, bool includeDeleted = false)
        {
            return Mongo.计数<T>(skip, limit, conditions, includeDisabled, includeDeleted);
        }
    }
}
