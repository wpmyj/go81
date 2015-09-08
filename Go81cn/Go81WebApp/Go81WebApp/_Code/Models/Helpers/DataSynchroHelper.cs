using MongoDB;
using MongoDB.Bson;
using System;
using System.IO;
using System.Text;

namespace Go81WebApp.Models.Helpers
{
    public class DataSynchroHelper
    {
        public class ImpExpInfo
        {
            public readonly int Count;
            public readonly long CurId;

            public ImpExpInfo(int count, long curId)
            {
                Count = count;
                CurId = curId;
            }

            public override string ToString()
            {
                return string.Format("记录数：{0}\r\n当前ID：{1}", Count, CurId);
            }
        }
        public static string ExpColl<T>()
        {
            var time = DateTime.Now.ToLocalTime().ToString("yyyyMMdd-hhmmss");
            var collName = typeof(T).Name;

            var dirstr = "/App_Uploads/导出数据/" + time + "/";
            var dir = System.Web.HttpContext.Current.Server.MapPath(dirstr);
            var  filename = "导出Coll-" + collName + ".js";
            var file = dir + filename;
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            if (!System.IO.File.Exists(file))
            {
                FileStream fs = System.IO.File.Create(file);
                fs.Close();
            }

            var exp = 0;
            var cur_id = -1L;
            using (var sw = new StreamWriter(file, false, Encoding.UTF8))
            {
                foreach (var doc in Mongo.Coll<T>().FindAllAs<BsonDocument>())
                {
                    if (BsonType.Int64 == doc["_id"].BsonType)
                        cur_id = Math.Max(cur_id, doc["_id"].AsInt64);
                    var json = doc.ToJson();
                    sw.WriteLine(json);
                    ++exp;
                }
                sw.Close();
            }
            return dirstr + filename;
        }
        //public static ImpExpInfo ImpColl<T>(bool setNextId = false)
        //{
        //    var collName = typeof(T).Name;
        //    var fp = ChooseOpenFile("导出Coll-" + collName + ".js");
        //    if (string.IsNullOrWhiteSpace(fp)) return null;

        //    MongoCollection mc = null;

        //    var imp = 0;
        //    var cur_id = -1L;
        //    using (var sr = new StreamReader(fp, Encoding.UTF8))
        //    {
        //        try
        //        {
        //            const int batchCount = 2;
        //            var l = new List<BsonDocument>(batchCount);
        //            for (; ; )
        //            {
        //                var json = sr.ReadLine();
        //                if (null == json)
        //                {
        //                    if (0 != l.Count)
        //                    {
        //                        if (null == mc)
        //                        {
        //                            mc = Mongo.Coll<T>();
        //                            mc.Drop();
        //                        }
        //                        mc.InsertBatch(l);
        //                        imp += l.Count;
        //                        l.Clear();
        //                    }
        //                    break;
        //                }

        //                var doc = BsonSerializer.Deserialize<BsonDocument>(json);
        //                l.Add(doc);
        //                Debug.Print(json);

        //                if (BsonType.Int64 == doc["_id"].BsonType)
        //                    cur_id = Math.Max(cur_id, doc["_id"].AsInt64);

        //                if (l.Count != batchCount) continue;
        //                if (null == mc)
        //                {
        //                    mc = Mongo.Coll<T>();
        //                    mc.Drop();
        //                }
        //                mc.InsertBatch(l);
        //                imp += l.Count;
        //                l.Clear();
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            MessageBox.Show(ex.Message);
        //            Debug.Print(ex.Message);
        //            if (null != mc) mc.Drop();
        //            return null;
        //        }
        //        sr.Close();
        //    }
        //    if (setNextId && -1 != cur_id) Mongo.NextIdSetTo<T>(cur_id);
        //    return new ImpExpInfo(imp, cur_id);
        //}
    }
}