using Go81WebApp.Models.数据模型.推荐数据模型;
using Go81WebApp.Models.管理器;
using Go81WebApp.Models.管理器.推荐管理;
using Go81WebApp.Models.管理器.推送管理;
using Go81WebApp.Models.数据模型;
using Go81WebApp.Models.数据模型.内容数据模型;
using Go81WebApp.Models.数据模型.权限数据模型;
using Go81WebApp.Models.数据模型.商品数据模型;
using Go81WebApp.Models.数据模型.推送数据模型;
using Go81WebApp.Models.数据模型.消息数据模型;
using Go81WebApp.Models.数据模型.用户数据模型;
using Helpers;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.PanGu;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using MailApiPostClass;
using MongoDB;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json;
using PanGu.Framework;

namespace Go81WebApp.ModuleTester
{
    public static partial class 工具
    {
        public static void 导入Lucene()
        {
            //临时商品lucene(包含商品精确型号)，可能含有重复值  
            List<string> catlog = new List<string>();
            //商品lucene，去除重复值后的集合
            List<string> catlogdata = new List<string>();

            //已审核通过的商品集合
            IEnumerable<商品> sp = 商品管理.查询商品(0, 0, Query<商品>.Where(o => o.审核数据.审核状态 == 审核状态.审核通过),includeDisabled:false,includeDeleted:false);

            //已审核通过的商品写入lucene
            foreach (var s in sp)
            {
                CreateIndex(商品管理.查找商品(s.Id), "Lucene.Net\\IndexDic\\Product");

                //提取商品精确型号
                if (s.商品信息 != null && !string.IsNullOrWhiteSpace(s.商品信息.精确型号))
                {
                    catlog.Add(s.商品信息.精确型号);
                }
            }

            //已审核供应商集合
            IEnumerable<供应商> gys = 用户管理.查询用户<供应商>(0, 0, Query<供应商>.Where(o => o.审核数据.审核状态 == 审核状态.审核通过),includeDisabled:false,includeDeleted:false);
            //已审核供应商写入lucene        
            foreach (var g in gys)
            {
                CreateIndex_gys(用户管理.查找用户<供应商>(g.Id), "Lucene.Net\\IndexDic\\Gys");
            }
            //去除商品精确型号的重复值
            foreach (var g in catlog)
            {
                if (!catlogdata.Contains(g))
                {
                    catlogdata.Add(g); ;
                }
            }

            //商品精确型号写入lucene
            foreach (var g in catlogdata)
            {
                CreateIndex_ProductCatalog(g, "Lucene.Net\\IndexDic\\ProductCatalog");
            }

            //商品分类写入lucene
            IEnumerable<商品分类> ProductCatalog = 商品分类管理.查询商品分类(0, 0);
            foreach (var g in ProductCatalog)
            {
                CreateIndex_ProductCatalog(g.分类名, "Lucene.Net\\IndexDic\\ProductCatalog");
            }


            //公告写入lucene
            var 公告 = 公告管理.查询公告(0, 0, Query<公告>.EQ(o => o.审核数据.审核状态, 审核状态.审核通过), includeDisabled: false, includeDeleted: false);
            foreach (var item in 公告)
            {
                GG_CreateIndex(item, "/Lucene.Net/IndexDic/GongGao");
            }

            //新闻写入lucene
            var 新闻 = 新闻管理.查询新闻(0, 0, includeDisabled: false, includeDeleted: false);
            foreach (var item in 新闻)
            {
                CreateIndex(item, "/Lucene.Net/IndexDic/News");
            }

            //通知写入lucene
            var 通知 = 通知管理.查询通知(0, 0, includeDisabled: false, includeDeleted: false);
            foreach (var item in 通知)
            {
                CreateIndex(item, "/Lucene.Net/IndexDic/Tongzhi");
            }

            //新闻写入lucene
            var 政策法规 = 政策法规管理.查询政策法规(0, 0, includeDisabled: false, includeDeleted: false);
            foreach (var item in 政策法规)
            {
                CreateIndex(item, "/Lucene.Net/IndexDic/Zcfg");
            }
        }

        /// <summary>
        /// 索引存放目录
        /// </summary>
        public static string IndexDic(string IndexPath)
        {
            return @"E:\Go81\Go81WebApp\Go81WebApp\" + IndexPath;
        }

        /// <summary>
        /// 盘古分词器
        /// </summary>
        public static Analyzer PanGuAnalyzer
        {
            get { return new PanGuAnalyzer(); }
        }

        /// <summary>
        /// 盘古分词的配置文件
        /// </summary>
        public static string PanGuXmlPath
        {
            get
            {
                return @"E:\Go81\Go81WebApp\Go81WebApp\Lucene.Net\PanGu\PanGu.xml";
            }
        }






        /// <summary>
        /// 创建商品索引
        /// </summary>
        public static void CreateIndex(商品 model, string indexdic)
        {
            string Indexdic_Server = IndexDic(indexdic);

            PanGu.Segment.Init(PanGuXmlPath);
            //创建索引目录
            if (!Directory.Exists(Indexdic_Server))
            {
                Directory.CreateDirectory(Indexdic_Server);
            }
            Lucene.Net.Store.Directory dir = new Lucene.Net.Store.SimpleFSDirectory(new DirectoryInfo(Indexdic_Server));
            //IndexWriter第三个参数:true指重新创建索引,false指从当前索引追加....此处为追加索引所以为false
            try
            {
                //IndexWriter writer = new IndexWriter(Indexdic_Server, PanGuAnalyzer, false, Lucene.Net.Index.IndexWriter.MaxFieldLength.LIMITED);
                IndexWriter writer = new IndexWriter(dir, PanGuAnalyzer, false, IndexWriter.MaxFieldLength.UNLIMITED);
                AddIndex(writer, model);

                writer.Optimize();
                writer.Close();
            }
            catch
            {
                //IndexWriter writer = new IndexWriter(Indexdic_Server, PanGuAnalyzer, true, Lucene.Net.Index.IndexWriter.MaxFieldLength.LIMITED);
                IndexWriter writer = new IndexWriter(dir, PanGuAnalyzer, true, IndexWriter.MaxFieldLength.UNLIMITED);
                AddIndex(writer, model);

                writer.Optimize();
                writer.Close();
            }

        }

        /// <summary>
        /// 创建商品索引
        /// </summary>
        /// <param name="analyzer"></param>
        /// <param name="title"></param>
        /// <param name="content"></param>
        public static void AddIndex(IndexWriter writer, 商品 model)
        {
            try
            {
                Document doc = new Document();
                doc.Add(new Field("NumId", model.Id.ToString(), Field.Store.YES, Field.Index.NOT_ANALYZED));//存储且索引

                Field f = new Field("Name", model.商品信息.商品名, Field.Store.YES, Field.Index.ANALYZED);
                f.SetBoost(5F);//为标题增加权重
                doc.Add(f);

                f = new Field("ExactModel", model.商品信息.精确型号, Field.Store.YES, Field.Index.ANALYZED);
                f.SetBoost(4F);//为精确型号增加权重
                doc.Add(f);

                doc.Add(new Field("Description", model.商品数据.商品简介, Field.Store.YES, Field.Index.NOT_ANALYZED));//存储且索引

                if (model.商品信息.商品图片.Count > 0)
                {
                    doc.Add(new Field("Pic", model.商品信息.商品图片[0], Field.Store.YES, Field.Index.NOT_ANALYZED));//存储且索引
                }
                else
                {
                    doc.Add(new Field("Pic", "/images/noimage.jpg", Field.Store.YES, Field.Index.NOT_ANALYZED));//存储且索引
                }
                doc.Add(new Field("Price", model.销售信息.价格.ToString(), Field.Store.YES, Field.Index.NOT_ANALYZED));//存储且索引
                doc.Add(new Field("Company", model.商品信息.所属供应商.用户ID.ToString(), Field.Store.YES, Field.Index.NOT_ANALYZED));//存储且索引
                doc.Add(new Field("Attribute", JsonConvert.SerializeObject(model.商品数据.商品属性), Field.Store.YES, Field.Index.NOT_ANALYZED));//存储且索引
                doc.Add(new Field("AddTime", model.基本数据.修改时间.ToString("yyyy-MM-dd"), Field.Store.YES, Field.Index.NOT_ANALYZED));//存储且索引
                writer.AddDocument(doc);
            }
            catch
            {
                return;
            }
        }


        /// <summary>
        /// 创建供应商索引
        /// </summary>
        /// <param name="model"></param>
        /// <param name="indexdic"></param>
        public static void CreateIndex_gys(供应商 model, string indexdic)
        {
            string Indexdic_Server = IndexDic(indexdic);

            PanGu.Segment.Init(PanGuXmlPath);
            //创建索引目录
            if (!Directory.Exists(Indexdic_Server))
            {
                Directory.CreateDirectory(Indexdic_Server);
            }
            Lucene.Net.Store.Directory dir = new Lucene.Net.Store.SimpleFSDirectory(new DirectoryInfo(Indexdic_Server));
            //IndexWriter第三个参数:true指重新创建索引,false指从当前索引追加....此处为追加索引所以为false
            try
            {
                //Lucene.Net.Store.FSDirectory directory = Lucene.Net.Store.FSDirectory.Open(new DirectoryInfo("/Lucene.Net/IndexDic/Gys"), new Lucene.Net.Store.NativeFSLockFactory());
                //bool isUpdate = IndexReader.IndexExists(directory);


                //IndexWriter writer = new IndexWriter(directory, PanGuAnalyzer, isUpdate, Lucene.Net.Index.IndexWriter.MaxFieldLength.LIMITED);
                //AddIndex_gys(writer, model);
                //IndexWriter writer = new IndexWriter(Indexdic_Server, PanGuAnalyzer, false, Lucene.Net.Index.IndexWriter.MaxFieldLength.LIMITED);

                IndexWriter writer = new IndexWriter(dir, PanGuAnalyzer, false, IndexWriter.MaxFieldLength.UNLIMITED);
                AddIndex_gys(writer, model);

                writer.Optimize();
                writer.Close();
            }
            catch
            {
                //IndexWriter writer = new IndexWriter(Indexdic_Server, PanGuAnalyzer, true, Lucene.Net.Index.IndexWriter.MaxFieldLength.LIMITED);
                IndexWriter writer = new IndexWriter(dir, PanGuAnalyzer, true, IndexWriter.MaxFieldLength.UNLIMITED);
                AddIndex_gys(writer, model);

                writer.Optimize();
                writer.Close();
            }


            //string Indexdic_Server = IndexDic(indexdic);

            //PanGu.Segment.Init(PanGuXmlPath);
            ////创建索引目录
            //Lucene.Net.Store.FSDirectory directory = Lucene.Net.Store.FSDirectory.Open(new DirectoryInfo(Indexdic_Server), new Lucene.Net.Store.NativeFSLockFactory());
            //bool isUpdate = IndexReader.IndexExists(directory);

            //IndexWriter writer = new IndexWriter(directory, PanGuAnalyzer, isUpdate, Lucene.Net.Index.IndexWriter.MaxFieldLength.LIMITED);
            //AddIndex_gys(writer, model);

            //writer.Optimize();
            //writer.Close();

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="model"></param>
        public static void AddIndex_gys(IndexWriter writer, 供应商 model)
        {
            try
            {
                Document doc = new Document();
                doc.Add(new Field("NumId", model.Id.ToString(), Field.Store.YES, Field.Index.NOT_ANALYZED));//存储不索引

                if (model.所属地域.省份 != null)
                {
                    Field f = new Field("Province", model.所属地域.省份, Field.Store.YES, Field.Index.ANALYZED);//所属省份
                    f.SetBoost(3F);
                    doc.Add(f);
                }

                if (model.所属地域.城市 != null)
                {
                    Field f = new Field("City", model.所属地域.城市, Field.Store.YES, Field.Index.ANALYZED);//所属城市
                    f.SetBoost(3F);
                    doc.Add(f);
                }

                if (model.所属地域.区县 != null)
                {
                    Field f = new Field("Area", model.所属地域.区县, Field.Store.YES, Field.Index.ANALYZED);//所属区县
                    f.SetBoost(3F);
                    doc.Add(f);
                }

                if (model.企业联系人信息 != null && model.企业联系人信息.联系人固定电话 != null)
                {
                    doc.Add(new Field("Telephone", model.企业联系人信息.联系人固定电话, Field.Store.YES, Field.Index.NOT_ANALYZED));//注册地址    存储不索引
                }

                if (model.企业联系人信息 != null && model.企业联系人信息.联系人姓名 != null)
                {
                    doc.Add(new Field("P_Name", model.企业联系人信息.联系人姓名, Field.Store.YES, Field.Index.NOT_ANALYZED));//存储不索引
                }
                if (model.企业基本信息 != null && model.企业基本信息.所属行业 != null)
                {
                    Field f = new Field("Industry", model.企业基本信息.所属行业, Field.Store.YES, Field.Index.ANALYZED);//所属行业
                    f.SetBoost(3F);
                    doc.Add(f);
                }
                //if (model.可提供产品类别列表 != null && model.可提供产品类别列表.Count > 0 && !string.IsNullOrEmpty(model.可提供产品类别列表[0].一级分类))
                //{
                //    Field f = new Field("Industry", model.可提供产品类别列表[0].一级分类, Field.Store.YES, Field.Index.ANALYZED);//所属行业
                //    f.SetBoost(3F);
                //    doc.Add(f);
                //}

                if (model.企业基本信息.企业名称 != null)
                {
                    Field f = new Field("Name", model.企业基本信息.企业名称, Field.Store.YES, Field.Index.ANALYZED);//企业名称
                    f.SetBoost(5F);
                    doc.Add(f);
                }

                if (model.供应商用户信息.年检列表 != null && model.供应商用户信息.年检列表.Any() &&
                    model.供应商用户信息.年检列表.ContainsKey(DateTime.Now.Year.ToString()))
                {
                    Field f = new Field("YearCheck", "1", Field.Store.YES, Field.Index.NOT_ANALYZED);
                    doc.Add(f);
                }
                else
                {
                    Field f = new Field("YearCheck", "0", Field.Store.YES, Field.Index.NOT_ANALYZED);
                    doc.Add(f);
                }

                //if (model.供应商用户信息.已入库)
                //{
                    Field f1 = new Field("Storage", "1", Field.Store.YES, Field.Index.NOT_ANALYZED);
                    doc.Add(f1);
                //}
                //else
                //{
                //    Field f = new Field("Storage", "0", Field.Store.YES, Field.Index.NOT_ANALYZED);
                //    doc.Add(f);
                //}

                writer.AddDocument(doc);
            }
            catch
            {
                return;
            }
        }

        /// <summary>
        /// 创建商品分类索引--用于搜索框智能提示
        /// </summary>
        public static void CreateIndex_ProductCatalog(string catalogName, string indexdic)
        {
            string Indexdic_Server = IndexDic(indexdic);

            PanGu.Segment.Init(PanGuXmlPath);
            //创建索引目录
            if (!Directory.Exists(Indexdic_Server))
            {
                Directory.CreateDirectory(Indexdic_Server);
            }
            Lucene.Net.Store.Directory dir = new Lucene.Net.Store.SimpleFSDirectory(new DirectoryInfo(Indexdic_Server));
            //IndexWriter第三个参数:true指重新创建索引,false指从当前索引追加....此处为追加索引所以为false
            try
            {
                //IndexWriter writer = new IndexWriter(Indexdic_Server, PanGuAnalyzer, false, Lucene.Net.Index.IndexWriter.MaxFieldLength.LIMITED);
                IndexWriter writer = new IndexWriter(dir, PanGuAnalyzer, false, IndexWriter.MaxFieldLength.UNLIMITED);
                AddIndex_ProductCatalog(writer, catalogName);

                writer.Optimize();
                writer.Close();
            }
            catch
            {
                //IndexWriter writer = new IndexWriter(Indexdic_Server, PanGuAnalyzer, true, Lucene.Net.Index.IndexWriter.MaxFieldLength.LIMITED);
                IndexWriter writer = new IndexWriter(dir, PanGuAnalyzer, true, IndexWriter.MaxFieldLength.UNLIMITED);
                AddIndex_ProductCatalog(writer, catalogName);

                writer.Optimize();
                writer.Close();
            }

        }

        /// <summary>
        /// 创建商品分类索引--用于搜索框智能提示
        /// </summary>
        /// <param name="analyzer"></param>
        /// <param name="content"></param>
        public static void AddIndex_ProductCatalog(IndexWriter writer, string catalogName)
        {
            try
            {
                Document doc = new Document();
                doc.Add(new Field("Name", catalogName.Replace(" ", ""), Field.Store.YES, Field.Index.NOT_ANALYZED));//存储且索引
                writer.AddDocument(doc);
            }
            catch
            {
                return;
            }
        }

        /// <summary>
        /// 创建公告索引
        /// </summary>
        public static void GG_CreateIndex(公告 g, string indexdic)
        {
            string Indexdic_Server = IndexDic(indexdic);

            PanGu.Segment.Init(PanGuXmlPath);
            //创建索引目录
            if (!Directory.Exists(Indexdic_Server))
            {
                Directory.CreateDirectory(Indexdic_Server);
            }
            Lucene.Net.Store.Directory dir = new Lucene.Net.Store.SimpleFSDirectory(new DirectoryInfo(Indexdic_Server));
            //IndexWriter第三个参数:true指重新创建索引,false指从当前索引追加....此处为追加索引所以为false
            try
            {
                //IndexWriter writer = new IndexWriter(Indexdic_Server, PanGuAnalyzer, false, Lucene.Net.Index.IndexWriter.MaxFieldLength.LIMITED);
                IndexWriter writer = new IndexWriter(dir, PanGuAnalyzer, false, IndexWriter.MaxFieldLength.UNLIMITED);
                GG_AddIndex(writer, g);

                writer.Optimize();
                writer.Close();
            }
            catch
            {
                //IndexWriter writer = new IndexWriter(Indexdic_Server, PanGuAnalyzer, true, Lucene.Net.Index.IndexWriter.MaxFieldLength.LIMITED);
                IndexWriter writer = new IndexWriter(dir, PanGuAnalyzer, true, IndexWriter.MaxFieldLength.UNLIMITED);
                GG_AddIndex(writer, g);

                writer.Optimize();
                writer.Close();
            }
        }

        /// <summary>
        /// 创建公告索引
        /// </summary>
        /// <param name="analyzer"></param>
        /// <param name="title"></param>
        /// <param name="content"></param>
        public static void GG_AddIndex(IndexWriter writer, 公告 model)
        {
            try
            {
                Document doc = new Document();
                doc.Add(new Field("NumId", model.Id.ToString(), Field.Store.YES, Field.Index.NOT_ANALYZED));//存储且索引
                Field f = new Field("Title", model.内容主体.标题, Field.Store.YES, Field.Index.ANALYZED);
                f.SetBoost(5F);//为标题增加权重
                doc.Add(f);//存储且索引
                doc.Add(new Field("Content", model.内容主体.内容, Field.Store.YES, Field.Index.ANALYZED));//存储且索引
                doc.Add(new Field("AdClass", model.公告信息.公告类别.ToString(), Field.Store.YES, Field.Index.ANALYZED));//存储且索引
                doc.Add(new Field("AdFeature", model.公告信息.公告性质.ToString(), Field.Store.YES, Field.Index.NOT_ANALYZED));//存储且索引
                doc.Add(new Field("AdPro", model.公告信息.所属地域.省份, Field.Store.YES, Field.Index.ANALYZED));//存储且索引
                doc.Add(new Field("AdCity", model.公告信息.所属地域.城市, Field.Store.YES, Field.Index.ANALYZED));//存储且索引
                doc.Add(new Field("AdArea", model.公告信息.所属地域.区县, Field.Store.YES, Field.Index.ANALYZED));//存储且索引
                doc.Add(new Field("AdHy", model.公告信息.一级分类, Field.Store.YES, Field.Index.ANALYZED));//存储且索引
                doc.Add(new Field("AddTime", model.内容主体.发布时间.ToString(), Field.Store.YES, Field.Index.NOT_ANALYZED));//存储且索引
                writer.AddDocument(doc);
            }
            catch
            {
                return;
            }
        }

        /// <summary>
        /// 创建索引
        /// </summary>
        public static void CreateIndex(内容基本数据 model, string indexdic)
        {
            string Indexdic_Server = IndexDic(indexdic);

            PanGu.Segment.Init(PanGuXmlPath);
            //var g = model as Go81WebApp.Models.数据模型.内容数据模型.公告;
            //创建索引目录
            if (!Directory.Exists(Indexdic_Server))
            {
                Directory.CreateDirectory(Indexdic_Server);
            }
            Lucene.Net.Store.Directory dir = new Lucene.Net.Store.SimpleFSDirectory(new DirectoryInfo(Indexdic_Server));
            //IndexWriter第三个参数:true指重新创建索引,false指从当前索引追加....此处为追加索引所以为false
            try
            {
                //IndexWriter writer = new IndexWriter(Indexdic_Server, PanGuAnalyzer, false, Lucene.Net.Index.IndexWriter.MaxFieldLength.LIMITED);
                IndexWriter writer = new IndexWriter(dir, PanGuAnalyzer, false, IndexWriter.MaxFieldLength.UNLIMITED);
                AddIndex(writer, model.Id.ToString(), model.内容主体.标题, model.内容主体.内容, model.内容主体.发布时间.ToString());

                writer.Optimize();
                writer.Close();
            }
            catch
            {
                //IndexWriter writer = new IndexWriter(Indexdic_Server, PanGuAnalyzer, true, Lucene.Net.Index.IndexWriter.MaxFieldLength.LIMITED);
                IndexWriter writer = new IndexWriter(dir, PanGuAnalyzer, true, IndexWriter.MaxFieldLength.UNLIMITED);
                AddIndex(writer, model.Id.ToString(), model.内容主体.标题, model.内容主体.内容, model.内容主体.发布时间.ToString());

                writer.Optimize();
                writer.Close();
            }
        }

        /// <summary>
        /// 创建索引
        /// </summary>
        /// <param name="analyzer"></param>
        /// <param name="title"></param>
        /// <param name="content"></param>
        public static void AddIndex(IndexWriter writer, string id, string title, string content, string date)
        {
            try
            {
                Document doc = new Document();
                doc.Add(new Field("NumId", id, Field.Store.YES, Field.Index.NOT_ANALYZED));//存储且索引
                //doc.Add(new Field("Title", title, Field.Store.YES, Field.Index.ANALYZED));//存储且索引
                Field f = new Field("Title", title, Field.Store.YES, Field.Index.ANALYZED);
                f.SetBoost(5F);//为标题增加权重
                doc.Add(f);
                doc.Add(new Field("Content", content, Field.Store.YES, Field.Index.ANALYZED));//存储且索引
                doc.Add(new Field("AddTime", date, Field.Store.YES, Field.Index.NOT_ANALYZED));//存储且索引
                writer.AddDocument(doc);
            }
            catch
            {
                return;
            }
        }
    }
}
