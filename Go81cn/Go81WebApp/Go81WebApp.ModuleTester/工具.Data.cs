using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Windows;
using Go81WebApp.Models.数据模型;
using Go81WebApp.Models.数据模型.商品数据模型;
using Ivony.Html;
using Ivony.Html.Parser;
using Lucene.Net.Analysis;
using Lucene.Net.Messages;
using Lucene.Net.Search;
using MongoDB;
using MongoDB.Bson;
using MongoDB.Driver.Builders;
using MongoDB.Driver.Linq;
using Regex = System.Text.RegularExpressions.Regex;
using System.IO;

namespace Go81WebApp.ModuleTester
{
    public static partial class 工具Data
    {
        public static bool CheckCreateDirectory(string path, bool withFilename = true)
        {
            if (!Path.IsPathRooted(path)) return false;
            var dirs = path.Split(@"\".ToCharArray());
            var targetDir = dirs[0];
            var dirDepthEnd = dirs.Length;
            if (withFilename) --dirDepthEnd;
            for (var i = 1; i < dirDepthEnd; ++i)
            {
                targetDir += @"\" + dirs[i];
                if (!Directory.Exists(targetDir))
                {
                    Directory.CreateDirectory(targetDir);
                }
            }
            return true;
        }
        public static bool FileCopy(string srcFile, string dstFile)
        {
            srcFile = Path.GetFullPath(srcFile);
            dstFile = Path.GetFullPath(dstFile);
            if (!CheckCreateDirectory(dstFile)) return false;
            File.Copy(srcFile, dstFile, true);
            return true;
        }
        public static IEnumerable<资源引用> 列表资源(bool copy = false)
        {
            //列表
            var reclist = 提取在用图片(copy).ToArray();
            //详情
            var listTxt = string.Join("\r\n", reclist
                .Where(r => !r.存在)
                .Select((r, i) => string.Format("{0,8}:{1}\t{2}\t{3}\r\n\t\t\t\t{4}\r\n",
                    i + 1,
                    r.存在 ? "OK" : "--",
                    r.记录.Count,
                    r.路径,
                    string.Join("\r\n\t\t\t\t", r.记录.Select(t => "[" + t.Item1 + ", " + t.Item2 + ", " + t.Item3 + "]"))
                    ))) +
            "\r\n--------\r\n" +
            string.Join("\r\n", reclist
                .Where(r => r.存在)
                .Select((r, i) => string.Format("{0,8}:{1}\t{2}\t{3}\r\n\t\t\t\t{4}\r\n",
                    i + 1,
                    r.存在 ? "OK" : "--",
                    r.记录.Count,
                    r.路径,
                    string.Join("\r\n\t\t\t\t", r.记录.Select(t => "[" + t.Item1 + ", " + t.Item2 + ", " + t.Item3 + "]"))
                    )));
            //选择保存文件
            var fp = 工具.ChooseSaveFile("List.txt");
            if (string.IsNullOrWhiteSpace(fp)) return reclist;
            //写列表
            if (File.Exists(fp)) File.Delete(fp);
            File.WriteAllLines(fp, reclist.Where(r => r.存在).Select(r => r.路径).ToArray());
            //写详情
            fp = Path.ChangeExtension(fp, ".detail.txt");
            if (File.Exists(fp)) File.Delete(fp);
            File.WriteAllText(fp, listTxt);
            return reclist;
        }
        public static IEnumerable<资源引用> 提取在用图片(bool copy = false)
        {
            var dir = App_Uploads_Dir;
            const string dirName = @"\App_Uploads";
            var dstDir = dir + dirName + ".1";
            if (copy)
            {
                if (Directory.Exists(dstDir)) Directory.Delete(dstDir, true);
                copy = CheckCreateDirectory(dstDir, false);
            }
            var dic = new SortedDictionary<string, 资源引用>();
            foreach (var l0 in 引用图片列表())
            {
                var src = l0.Item4.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
                var fp = dir + src;
                var exist = File.Exists(fp);
                if (copy && exist)
                {
                    var dst = (dstDir + src).Replace(dirName + @"\", @"\");
                    FileCopy(fp, dst);
                    //复制小图片
                    var srcDirs = fp.Split(@"\".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                    var dstDirs = dst.Split(@"\".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                    if ("original" == srcDirs[srcDirs.Length - 2] && "original" == dstDirs[dstDirs.Length - 2])
                    {
                        foreach (var fbl in new[] {"50X50","150X150","350X350"})
                        {
                            srcDirs[srcDirs.Length - 2] = dstDirs[dstDirs.Length - 2] = fbl;
                            var srcFn = string.Join(@"\", srcDirs);
                            var dstFn = string.Join(@"\", dstDirs);
                            FileCopy(srcFn, dstFn);
                        }
                    }
                }
                var rref = dic.ContainsKey(src) ? dic[src] : dic[src] = new 资源引用{路径 = src,存在 = exist};
                rref.记录.Add(new Tuple<string, long, string>(l0.Item1, l0.Item2, l0.Item3));
            }
            foreach (var rr in dic.Values)
                rr.记录 = rr.记录.OrderBy(o => o.Item1).ThenBy(o => o.Item2).ThenBy(o => o.Item3).ToList();
            return dic.Values
                .OrderBy(o=>o.存在)
                .ThenBy(o=>o.记录.Count)
                .ThenBy(o=>o.路径)
                ;
        }
        public static IEnumerable<Tuple<string, long, string, string>> 引用图片列表()
        {
            var db = Mongo.Coll<商品>().Database;
            var cns = db.GetCollectionNames();
            return from cn in cns
                   let docs =
                        db.GetCollection<BsonDocument>(cn)
                        .Find(Query<基本数据模型>.EQ(o => o.基本数据.已删除, false))
                   from d in docs
                   from s in GetAllPathString(cn, d["_id"].AsInt64, "", d)
                   select s;
        }
        private static IEnumerable<Tuple<string, long, string, string>> GetAllPathString(string typeName, long id, string path, BsonValue v)
        {
            switch (v.BsonType)
            {
                case BsonType.Array:
                    foreach (var s in v.AsBsonArray.SelectMany((el, i) => GetAllPathString(typeName, id, path + "[" + i+"]", el)))
                        yield return s;
                    break;
                case BsonType.Document:
                    foreach (var s in v.AsBsonDocument.SelectMany(el => GetAllPathString(typeName, id, path + "." + el.Name, el.Value)))
                        yield return s;
                    break;
                case BsonType.String:
                    var p = v.AsString;
                    if (p.StartsWith("/App_Uploads/"))
                        yield return new Tuple<string, long, string, string>(typeName, id, path, p);
                    else
                    {
                        var jp = new JumonyParser();
                        foreach (var s in jp.Parse(p).Find("img[src]")
                            .Select(img =>img.Attribute("src").AttributeValue)
                            .Where(p0 => !p0.StartsWith("data:image", StringComparison.OrdinalIgnoreCase)
                                && !p0.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
                                && !p0.StartsWith("file://", StringComparison.OrdinalIgnoreCase)
                                )
                            )
                            yield return new Tuple<string, long, string, string>(typeName, id, path, s);
                        //foreach (var c in re.Matches(p).Cast<Match>().SelectMany(m => m.Captures.Cast<Capture>()))
                        //    yield return new Tuple<string, long, string, string>(typeName, id, path, c.Value);
                    }
                    break;
            }
        }

        public class 资源引用
        {
            public string 路径;
            public bool 存在;
            public List<Tuple<string, long, string>> 记录 = new List<Tuple<string, long, string>>();
        }
        //private static readonly Regex re = new Regex(@"(/App_Uploads/[^""]+/[^""]+\.[^""]+)");
        private static readonly Dictionary<string, string> App_Uploads_Dirs =
            new Dictionary<string, string>
            {
                /*开发机*/{"DEV0", @"E:\Go81\Go81WebApp\Go81WebApp"},
                /*外网*/{"WIN-GAMD2DD7FVN", @"D:\Go81"},
            };

        public static string App_Uploads_Dir { get { string dir; return App_Uploads_Dirs.TryGetValue(Dns.GetHostName(), out dir) ? dir : null; } }
    }
    public static partial class 工具Data
    {
        public static _服务器身份 服务器身份
        {
            get
            {
                switch (Dns.GetHostName())
                {
                    case "DEV0": return _服务器身份.开发机DEV0;
                    case "WIN-GAMD2DD7FVN": return _服务器身份.外网服务器;
                }
                return _服务器身份.未知;
            }
        }

        public enum _服务器身份
        {
            未知,
            开发机DEV0,
            外网服务器,
            内网服务器,
        }
    }
    public static partial class 工具Data
    {
        /*
        public static Dictionary<string, string> 导入2修正 = new Dictionary<string, string>
        {
            {"中央空调机电", "中央空调"},
            {"临床医学技术类", "临床医学"},
            {"仪器仪表电气自动化技术及管理", "仪器仪表、电气自动化、仪器设备管理"},
            {"信息化建设", "信息化工程"},
            {"信息技术、教育技术学", "信息化工程、教育技术学"},
            {"区间、道路、市政、园林工程造价", "公路桥梁、市政工程、园林景观、工程造价"},
            {"卫星遥感、大地测量、GPS\\GIS、计算机软硬件", "卫星遥感、大地测量、GPS、GIS、计算机软件、计算机硬件"},
            {"后勤建设、教学、体育设备采购", "后勤建设、后勤建设教学、体育设备采购"},
            {"土建", "土建工程"},
            {"土建、造价", "土建工程、工程造价"},
            {"土建市政", "土建工程、市政工程"},
            {"土木", "土木工程"},
            {"土木工程及造价", "土木工程、工程造价"},
            {"土木工程施工技术及经济管理", "土木工程、建筑经济、工程管理"},
            {"地质勘查、测绘与信息化设备等管理", "地质勘查、地质测绘、地质工程、地质信息化设备"},
            {"城市排水、计算机应用、招投标及合同管理", "城市排水、计算机应用、招投标管理、合同管理"},
            {"城市规划、市政设计、环境工程设计", "城市规划、市政工程、环境工程"},
            {"安全技术防范", "安防监控"},
            {"安防监控消防工程", "安防监控、消防工程"},
            {"岩土工工程", "岩土工程"},
            {"工民建", "工业与民用建筑"},
            {"工民建、机电设备、招投标管理等", "工业与民用建筑、机电设备、招投标管理"},
            {"工民建及招投标代理", "工业与民用建筑、招投标管理"},
            {"工程", "工程管理"},
            {"工程、设备采购", "工程管理、设备采购"},
            {"工程咨询、监理、造价", "工程咨询、工程监理、工程造价"},
            {"工程咨询、管理", "工程咨询、工程管理"},
            {"工程咨询管理", "工程咨询、工程管理"},
            {"工程师造价", "工程造价"},
            {"工程师造价、审计", "工程造价、工程审计"},
            {"工程师造价、招标管理", "工程造价、招标管理"},
            {"工程师造价咨询", "工程造价"},
            {"工程师造价等", "工程造价"},
            {"工程建筑管理、造价", "工程建筑管理、工程造价"},
            {"工程建设、管理", "工程建设、工程管理"},
            {"工程技术、造价", "工程技术、工程造价"},
            {"工程技术管理及咨询", "工程技术管理、工程咨询"},
            {"工程招投标、造价、管理", "工程招投标、工程造价、工程管理"},
            {"工程招投标代理及造价咨询", "工程招投标、工程造价"},
            {"工程招标、造价", "工程招投标、工程造价"},
            {"工程招标代理", "工程招投标"},
            {"工程招标代理及造价咨询", "工程招投标、工程造价"},
            {"工程机械管理", "工程机械"},
            {"工程机械设备销售、维修", "工程机械设备销售、工程机械设备维修"},
            {"工程水电气管网园林及建筑结构", "工程水电气管网、园林景观、建筑结构"},
            {"工程监理、施工", "工程监理、工程施工"},
            {"工程监理工程造价", "工程监理、工程造价"},
            {"工程监理项目管理", "工程监理、项目管理"},
            {"工程管理、造价、审计、体育设备采购", "工程管理、工程造价、工程审计、体育设备采购"},
            {"工程管理监理", "工程管理、工程监理"},
            {"工程管理造价咨询招标代理", "工程管理、造价咨询、工程招投标"},
            {"工程经济技术", "工程经济、工程技术"},
            {"工程经济技术、造价咨询", "工程经济、工程技术、造价咨询"},
            {"工程造价、咨询", "工程造价、工程咨询"},
            {"工程造价、咨询、审计、过控", "工程造价、工程咨询、工程审计、工程过控"},
            {"工程造价、审核", "工程造价、工程审核"},
            {"工程造价、审计", "工程造价、工程审计"},
            {"工程造价、审计、建筑、市政", "工程造价、工程审计、工程建筑、市政工程"},
            {"工程造价、投资咨询、房地估价等", "工程造价、投资咨询、房地产评估"},
            {"工程造价、招投标、评标", "工程造价、工程招投标、工程评标"},
            {"工程造价、招投标代理", "工程造价、工程招投标"},
            {"工程造价、施工", "工程造价、工程施工"},
            {"工程造价、监理", "工程造价、工程监理"},
            {"工程造价、监理、项目管理", "工程造价、工程监理、项目管理"},
            {"工程造价、管理", "工程造价、工程管理"},
            {"工程造价、管理、监理", "工程造价、工程管理、工程监理"},
            {"工程造价、编制、审核", "工程造价、工程编制、工程审核"},
            {"工程造价、设备安装等", "工程造价、设备安装"},
            {"工程造价及管理", "工程造价、工程管理"},
            {"工程造价及项目管理", "工程造价、项目管理"},
            {"工程造价和技术", "工程造价、工程技术"},
            {"工程造价咨询", "工程造价"},
            {"工程造价审核", "工程造价"},
            {"工程造价等", "工程造价"},
            {"工程造价管理", "工程造价"},
            {"工程造价结算", "工程造价"},
            {"工程采购与管理", "工程采购、工程管理"},
            {"工程项目管理", "工程管理"},
            {"工程项目管理、造价", "工程管理、工程造价"},
            {"工程预结算", "工程预决算"},
            {"市政工程及公路桥梁", "市政工程、公路桥梁"},
            {"建筑", "建筑工程"},
            {"建筑、公路、市政", "建筑工程、公路桥梁、市政工程"},
            {"建筑、公路施工", "建筑工程、公路桥梁"},
            {"建筑、市政、公路工程施工技术、造价、招标代理", "建筑工程、市政工程、公路桥梁、工程造价、工程招投标"},
            {"建筑、电气、市政公路", "建筑工程、电气工程、市政工程、公路桥梁"},
            {"建筑、项目经理", "建筑工程、项目管理"},
            {"建筑安装工程", "建筑安装"},
            {"建筑安装机电专业", "建筑安装、机电安装"},
            {"建筑工程技术工程造价装饰工程建筑", "建筑工程、工程技术、工程造价、装饰"},
            {"建筑工程技术管理", "建筑工程、工程技术"},
            {"建筑工程监理", "工程监理"},
            {"建筑工程管理", "工程管理"},
            {"建筑工程造价", "工程造价"},
            {"建筑工程造价管理", "工程造价"},
            {"建筑技术、工程造价、建筑、市政公用监理", "建筑工程、工程造价、工程监理、市政工程"},
            {"建筑施工", "建筑工程"},
            {"建筑施工、公路施工", "建筑工程、公路桥梁"},
            {"建筑施工、投标", "建筑工程、工程招投标"},
            {"建筑施工、管理", "建筑工程、工程管理"},
            {"建筑施工及监理", "建筑工程、工程监理"},
            {"建筑施工技术", "建筑工程"},
            {"建筑施工技术、工程造价管理", "建筑工程、工程造价"},
            {"建筑施工管理", "建筑工程"},
            {"建筑施工管理监理工作", "建筑工程、工程监理"},
            {"建筑经济、工程造价管理", "建筑经济、工程造价"},
            {"建筑经济、技术", "建筑经济、建筑技术"},
            {"建筑经济、管理、咨询", "建筑经济、建筑工程管理、建筑工程咨询"},
            {"建筑结构、经济、市政工程", "建筑结构、建筑经济、市政工程"},
            {"建筑设计、施工、管理、绿化、装修", "建筑设计、建筑工程施工、建筑工程管理、装修"},
            {"建设工程及设备工程管理", "建筑工程、建筑工程设备、工程管理"},
            {"建设工程监理、结构加固", "建筑工程监理、结构加固"},
            {"建设工程管理", "建筑工程管理"},
            {"建设工程造价", "建筑工程造价"},
            {"房地产开发、建筑造价", "房地产开发、建筑工程造价"},
            {"房地产开发技术管理", "房地产开发"},
            {"房屋建筑", "房屋建筑工程"},
            {"房屋建筑施工管理", "房屋建筑工程"},
            {"技术", "工程技术"},
            {"技术、经济、结构", "建筑技术、建筑经济、建筑结构"},
            {"投资计划管理、设备管理、采购", "投资计划管理、设备管理、设备采购"},
            {"招投标", "工程招投标"},
            {"招投标代理、监理", "工程招投标、工程监理"},
            {"招投标及合同管理", "工程招投标、合同管理"},
            {"招标", "招投标管理"},
            {"施工管理、监理", "施工管理、施工监理"},
            {"无线电电子", "无线电通讯"},
            {"暖通、给排水及机电设备设计及工程管理", "暖通工程、给排水工程、机电设备设计、工程管理"},
            {"暖通设计、中央空调设计", "暖通工程、中央空调"},
            {"机械、电气、铁路、建筑", "机械设备制造、电气自动化、铁路工程、建筑工程"},
            {"机械、电气自动化、计算机", "机械设备制造、电气自动化、计算机应用"},
            {"机械制造、科研管理", "机械设备制造、科研管理"},
            {"机电一体化自动控制、计算机信息网络", "机电一体化自动控制、计算机网络"},
            {"机电专业", "机电一体化自动控制"},
            {"机电设备安装", "机电设备、机电安装"},
            {"机电设备安装工程", "机电设备、机电安装"},
            {"机电设备成套及招投标代理", "机电设备、招投标管理"},
            {"机电设备贸易、技术服务", "机电设备贸易、机电设备技术服务"},
            {"水利", "水利工程"},
            {"水利、电力等工程设计、科研、监理、规划", "水利工程、电力工程"},
            {"水利水电", "水利工程、电力工程"},
            {"水利水电、工程机械", "水利工程、电力工程、工程机械"},
            {"水利水电工程", "水利工程、电力工程"},
            {"水利水电工程、设计、监理", "水利工程、电力工程"},
            {"水利水电工程建筑", "水利工程、电力工程"},
            {"水利水电工程设计监理等", "水利工程、电力工程"},
            {"水处理、安防管理等", "水处理、安防管理"},
            {"水工", "水利工程"},
            {"汽车、拖拉机制造与维修", "汽车制造与维修、拖拉机制造与维修"},
            {"环境保护、给排水、消防", "环境保护、给排水工程、消防工程"},
            {"电力安装机电安装造价", "电力工程、机电安装、工程造价"},
            {"电力工程、土建工程设计施工", "电力工程、土建工程"},
            {"电力工程机电安装房屋建筑", "电力工程、机电安装、房屋建筑工程"},
            {"电子、技术医疗器械", "电子器械、医疗器械"},
            {"电子信息技术、智能交通技术开发", "电子信息技术、交通运输工程"},
            {"电子计算机", "计算机应用"},
            {"监理", "工程监理"},
            {"筑路总监", "建筑工程、公路桥梁"},
            {"管理", "项目管理"},
            {"精密机械、仪器", "精密机械、仪器仪表"},
            {"精密机械制造", "精密机械"},
            {"经济及管理", "经济管理"},
            {"经济管理、统计、档案、法学", "经济管理、统计、档案管理、法律"},
            {"计算机、网络、软件", "计算机应用、计算机网络、计算机软件"},
            {"网络及安全系统集成", "计算机网络、计算机安全、系统集成"},
            {"装饰、幕墙施工、设计", "装饰、幕墙工程"},
            {"装饰、装修、房屋建筑", "装饰、装修、房屋建筑工程"},
            {"装饰土建安装", "装饰、土建工程"},
            {"装饰工程及园林景观工程设计与施工", "装饰、园林景观"},
            {"计算机管理、信息化应用", "计算机应用、信息化工程"},
            {"计算机网络管理", "计算机网络"},
            {"计算机软硬件、光通信、信息化建设", "计算机软件、计算机硬件、光通信、信息化工程"},
            {"计算机软硬件、网络、广电设备", "计算机软件、计算机硬件、计算机网络、广电设备"},
            {"设备使用、维修、管理", "设备使用、设备维修、设备管理"},
            {"设备物资管理", "设备管理"},
            {"设备管理及会计", "设备管理、会计"},
            {"设计、监理、工程造价", "建筑工程、工程监理、工程造价"},
            {"评标及招投标监督工程技术", "招投标管理、工程评标"},
            {"财务、审计管理等", "财务管理、审计"},
            {"财务会计审计", "财务管理、会计、审计"},
            {"货物、设备及工程管理", "货物管理、设备管理、工程管理"},
            {"轻工、纺织检验监管", "轻工纺织检验监管"},
            {"通信", "通信工程"},
            {"通信工程、智能建筑工程等", "通信工程、智能建筑工程"},
            {"通信工程系统集成", "通信工程、系统集成"},
            {"通讯、遥控、自动控制", "无线电通讯、遥控、自动控制"},
            {"造价", "工程造价"},
            {"造价、招标、监理", "工程造价、工程招投标、工程监理"},
            {"造价及土木工程、建筑工程管理", "工程造价、土木工程、建筑工程管理"},
            {"造价及工程技术", "工程造价、建筑工程"},
            {"道桥、市政、房屋建筑", "公路桥梁、市政工程、房屋建筑工程"},
            {"道桥设计", "公路桥梁"},
            {"铁路建筑市政", "铁路工程、建筑工程、市政工程"},
            {"项目工程管理、房地产评估", "工程管理、房地产评估"},
            {"预决算", "工程预决算"},
            {"预结算", "工程预决算"},
        };
        */
    }
}
