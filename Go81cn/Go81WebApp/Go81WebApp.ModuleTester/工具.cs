using Go81WebApp.Models.管理器;
using Go81WebApp.Models.数据模型.商品数据模型;
using Go81WebApp.Models.数据模型.用户数据模型;
using Microsoft.Win32;
using MongoDB;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using Newtonsoft.Json;
using NPOI.SS;
using NPOI.SS.UserModel;
using NPOI.SS.Util;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;

namespace Go81WebApp.ModuleTester
{
    public static partial class 工具
    {
        public static string ChooseOpenFile(string fn = null, string dir = null)
        {
            var ofd = new OpenFileDialog
            {
                InitialDirectory = dir,
                CheckFileExists = true,
                FileName = fn,
            };

            if (!(ofd.ShowDialog() ?? false)) return null;
            return ofd.FileName;
        }
        public static string ChooseSaveFile(string fn = null, string dir = null)
        {
            var sfd = new SaveFileDialog
            {
                InitialDirectory = dir,
                FileName = fn,
                OverwritePrompt = true,
            };

            if (!(sfd.ShowDialog() ?? false)) return null;
            return sfd.FileName;
        }

        public static class 省市区县表
        {
            private static void pcasInit(IDictionary<string, Dictionary<string, Dictionary<string, string[][]>>> pcas, string[] codes1, string[] codes2, params string[] pca)
            {
                var p = pca[0];
                var c = pca[1];
                var a = pca[2];
                if (!pcas.ContainsKey(p)) pcas[p] = new Dictionary<string, Dictionary<string, string[][]>>();
                if (!pcas[p].ContainsKey(c)) pcas[p][c] = new Dictionary<string, string[][]>();
                pcas[p][c][a] = new[] { codes1, codes2 };
            }
            public static Dictionary<string, Dictionary<string, Dictionary<string, string[][]>>> 读省市区县表()
            {
                var fp = ChooseOpenFile();
                var pcas = new Dictionary<string, Dictionary<string, Dictionary<string, string[][]>>>();
                if (string.IsNullOrWhiteSpace(fp)) return pcas;
                var al = File.ReadAllLines(fp);
                var lastPCA = new[] {"不限省份", "不限城市", "不限区县"};

                #region 编码转换表
                var pCodeTable1 = new Dictionary<char, char>
                {
                    {'0', '0'},
                    {'1', '5'},
                    {'2', '6'},
                    {'3', '7'},
                    {'4', '8'},
                    {'5', '1'},
                    {'6', '2'},
                    {'7', '3'},
                    {'8', '4'},
                    {'9', '9'},
                };
                var pCodeTable2 = new Dictionary<char, char>
                {
                    {'0', '0'},
                    {'1', '1'},
                    {'2', '3'},
                    {'3', '5'},
                    {'4', '6'},
                    {'5', '2'},
                    {'6', '4'},
                    {'7', '8'},
                    {'8', '7'},
                    {'9', '9'},
                };
                var cCodeTable1 = new Dictionary<char, char>
                {
                    {'0', '0'},
                    {'1', '1'},
                    {'2', '3'},
                    {'3', '5'},
                    {'4', '7'},
                    {'5', '8'},
                    {'6', '6'},
                    {'7', '4'},
                    {'8', '2'},
                    {'9', '9'},
                };
                var cCodeTable2 = new Dictionary<char, char>
                {
                    {'0', '0'},
                    {'1', '1'},
                    {'2', '4'},
                    {'3', '7'},
                    {'4', '6'},
                    {'5', '3'},
                    {'6', '8'},
                    {'7', '5'},
                    {'8', '2'},
                    {'9', '9'},
                };
                var aCodeTable1 = new Dictionary<char, char>
                {
                    {'0', '0'},
                    {'1', '8'},
                    {'2', '7'},
                    {'3', '6'},
                    {'4', '5'},
                    {'5', '1'},
                    {'6', '2'},
                    {'7', '3'},
                    {'8', '4'},
                    {'9', '9'},
                };
                var aCodeTable2 = new Dictionary<char, char>
                {
                    {'0', '0'},
                    {'1', '2'},
                    {'2', '4'},
                    {'3', '6'},
                    {'4', '8'},
                    {'5', '7'},
                    {'6', '5'},
                    {'7', '3'},
                    {'8', '1'},
                    {'9', '9'},
                };
                #endregion

                foreach (var l in al.Where(o => !string.IsNullOrWhiteSpace(o)))
                {
                    var codes = new[] {l.Substring(0, 2), l.Substring(2, 2), l.Substring(4, 2)};
                    var codes2 = new[]
                    {
                        string.Empty + pCodeTable1[codes[0][0]] + pCodeTable2[codes[0][1]],
                        string.Empty + cCodeTable1[codes[1][0]] + cCodeTable2[codes[1][1]],
                        string.Empty + aCodeTable1[codes[2][0]] + aCodeTable2[codes[2][1]],
                    };
                    var pca = new string[3];
                    if (l[10] != ' ') //省
                    {
                        pca[0] = l.Substring(10);
                        pca[1] = "不限城市";
                        pca[2] = "不限区县";
                    }
                    else if (l[12] != ' ') //市
                    {
                        pca[0] = lastPCA[0];
                        pca[1] = l.Substring(12);
                        pca[2] = "不限区县";
                    }
                    else if (l[14] != ' ') //区县
                    {
                        pca[0] = lastPCA[0];
                        pca[1] = lastPCA[1];
                        pca[2] = l.Substring(14);
                    }
                    pcasInit(pcas, codes, codes2, pca);
                    lastPCA = pca;
                }
                pcasInit(pcas, new[] {"99", "99", "99"}, new[] {"99", "99", "99"}, new[] {"其他", "其他", "其他"});
                File.WriteAllText(fp + ".json", JSON数据(pcas), Encoding.UTF8);
                File.WriteAllText(fp + ".js", JS数据(pcas), Encoding.UTF8);
                导出EXCEL(fp + ".xls", pcas);
                return pcas;
            }
            public static string JSON数据(Dictionary<string, Dictionary<string, Dictionary<string, string[][]>>> pcas)
            {
                return JsonConvert.SerializeObject(pcas).Replace('"', '\'');
            }
            public static string JS数据(Dictionary<string, Dictionary<string, Dictionary<string, string[][]>>> pcas)
            {
                return string.Format("\"{0}\"",
                    string.Join("#",
                        pcas.Keys.Select(p => p + '$' + string.Join("|",
                            pcas[p].Keys.Select(c => c + ',' + string.Join(",",
                                pcas[p][c].Keys))))));
            }
            public static void 导出EXCEL(string fp, Dictionary<string, Dictionary<string, Dictionary<string, string[][]>>> pcas)
            {
                var x = new org.in2bits.MyXls.XlsDocument();
                var s = x.Workbook.Worksheets.Add("省市区县表");

                //标题
                var
                cell = s.Cells.Add(1, 1, "省份"); cell.Font.Bold = true; cell.BottomLineStyle = (ushort)org.in2bits.MyXls.LineStyle.Medium;
                cell = s.Cells.Add(1, 2, "城市"); cell.Font.Bold = true; cell.BottomLineStyle = (ushort)org.in2bits.MyXls.LineStyle.Medium;
                cell = s.Cells.Add(1, 3, "区县"); cell.Font.Bold = true; cell.BottomLineStyle = (ushort)org.in2bits.MyXls.LineStyle.Medium;
                cell = s.Cells.Add(1, 4, "省码"); cell.Font.Bold = true; cell.BottomLineStyle = (ushort)org.in2bits.MyXls.LineStyle.Medium;
                cell = s.Cells.Add(1, 5, "市码"); cell.Font.Bold = true; cell.BottomLineStyle = (ushort)org.in2bits.MyXls.LineStyle.Medium;
                cell = s.Cells.Add(1, 6, "区码"); cell.Font.Bold = true; cell.BottomLineStyle = (ushort)org.in2bits.MyXls.LineStyle.Medium;
                cell = s.Cells.Add(1, 7, "省码2"); cell.Font.Bold = true; cell.BottomLineStyle = (ushort)org.in2bits.MyXls.LineStyle.Medium;
                cell = s.Cells.Add(1, 8, "市码2"); cell.Font.Bold = true; cell.BottomLineStyle = (ushort)org.in2bits.MyXls.LineStyle.Medium;
                cell = s.Cells.Add(1, 9, "区码2"); cell.Font.Bold = true; cell.BottomLineStyle = (ushort)org.in2bits.MyXls.LineStyle.Medium;

                var i = 2;
                foreach (var p in pcas.Keys)
                {
                    foreach (var c in pcas[p].Keys)
                    {
                        foreach (var a in pcas[p][c].Keys)
                        {
                            s.Cells.Add(i, 1, p);
                            s.Cells.Add(i, 2, c);
                            s.Cells.Add(i, 3, a);
                            var codes = pcas[p][c][a];
                            s.Cells.Add(i, 4, codes[0][0]);
                            s.Cells.Add(i, 5, codes[0][1]);
                            s.Cells.Add(i, 6, codes[0][2]);
                            s.Cells.Add(i, 7, codes[1][0]);
                            s.Cells.Add(i, 8, codes[1][1]);
                            s.Cells.Add(i, 9, codes[1][2]);
                            ++i;
                        }
                    }
                }
                x.Save(File.OpenWrite(fp));
            }
        }

        #region 第一次导入专家
#if Imp1stZJ
        [Obsolete]
        public static int 导入1(bool add2db = false)
        {
            var fp = ChooseOpenFile("复件 【2013.12】发证专家统计表.xlsx");
            if (string.IsNullOrWhiteSpace(fp)) return 0;
            var zjc = 0;
            var pzl = new List<string>();
            using (var fs = File.Open(fp, FileMode.Open))
            {
                var wb = new XWorkbook(fs);
                var ws = wb[0];
                for (var i = 4; i <= ws.XS.LastRowNum; ++i)
                {
                    var u = new 专家();
                    u.Id = Mongo.NextId<专家>();
                    u.登录信息.登录名 = "pszj02" + (u.Id - 用户管理.专家Id基数).ToString("D6");
                    u.登录信息.密码 = "00000000";
                    u.所属管理单位 = 采购管理单位.成都军区物资采购机构_成都;
                    u.身份信息.专家类别 = 专家类别.军内;
                    u.身份信息.专家级别 = 专家级别.全军库专家;
                    u.所属地域.省份 = "四川省";
                    u.所属地域.城市 = "成都市";
                    u.审核数据.审核状态 = 审核状态.审核通过;

                    //姓名
                    u.登录信息.显示名 = u.身份信息.姓名 = u.联系方式.联系人 = ws[i, 1].ReadCellAsString().Trim();

                    //性别
                    var sxb = ws[i, 2].ReadCellAsString().Trim();
                    性别 xb;
                    if (Enum.TryParse(sxb, out xb)) u.身份信息.性别 = u.联系方式.性别 = xb;
                    else switch (sxb)
                    {
                        default:
                            break;
                    }
                    
                    //出生年月
                    var d = ws[i, 3].ToString().Trim()
                        .Split(new []{'.'}, StringSplitOptions.RemoveEmptyEntries);
                    u.身份信息.出生年月 = DateTime.ParseExact(d[0] + "." + d[1], "yyyy.M", null);
                    
                    //工作单位
                    u.工作经历信息.工作单位 = ws[i, 4].ReadCellAsString().Trim();
                    
                    //专业技术职称
                    var szc = ws[i, 5].ReadCellAsString().Trim();
                    专业技术职称 zc;
                    if (Enum.TryParse(szc, out zc)) u.学历信息.专业技术职称 = zc;
                    else switch (szc)
                    {
                        case "正高（教授）级": u.学历信息.专业技术职称 = 专业技术职称.教授; break;
                        default:
                            break;
                    }
                    
                    //毕业院校
                    u.学历信息.毕业院校 = ws[i, 6].ReadCellAsString().Trim();
                    
                    //从事专业
                    u.工作经历信息.从事专业 = ws[i, 7].ReadCellAsString().Trim();

                    //评审专业
                    var pzs = ws[i, 8].ReadCellAsString().Trim()
                        .Split("、, \r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                    for (var pzn =0;pzn<pzs.Length;++pzn)
                    {
                        var pz = pzs[pzn];
                        if (1 == Mongo.计数<商品分类>(0, 0, Query<商品分类>.Where(o => o.分类名 == pz && o.父分类.商品分类ID == -1))
                            || 专家可评标专业.非商品分类评审专业.Contains(pz)
                            )
                        {
                            u.可参评物资类别列表.Add(new 供应商._产品类别{一级分类 = pz});
                            //u.可参评物资类别列表.Add(pz);
                            pzs[pzn] = null;
                            continue;
                        }
                        if (pz.EndsWith("类"))
                        {
                            var pzj = pz.Remove(pz.Length - 1);
                            if (1 == Mongo.计数<商品分类>(0, 0, Query<商品分类>.Where(o => o.分类名 == pzj && o.父分类.商品分类ID == -1))
                                || 专家可评标专业.非商品分类评审专业.Contains(pz)
                                )
                            {
                                u.可参评物资类别列表.Add(new 供应商._产品类别 {一级分类 = pzj});
                                //u.可参评物资类别列表.Add(pzj);
                                pzs[pzn] = null;
                                continue;
                            }
                        }
                        else
                        {
                            var pzpl = pz + "类";
                            if (1 == Mongo.计数<商品分类>(0, 0, Query<商品分类>.Where(o => o.分类名 == pzpl && o.父分类.商品分类ID == -1))
                                || 专家可评标专业.非商品分类评审专业.Contains(pz)
                                )
                            {
                                u.可参评物资类别列表.Add(new 供应商._产品类别 { 一级分类 = pzpl });
                                //u.可参评物资类别列表.Add(pzpl);
                                pzs[pzn] = null;
                                continue;
                            }
                        }
                    }
                    pzs = pzs.Where(s => !string.IsNullOrWhiteSpace(s)).ToArray();
                    if (0!=pzs.Length)
                    {
                        var pzo = string.Join("、", pzs);
                        u.可参评物资类别列表.Add(new 供应商._产品类别 { 一级分类 = pzo });
                        //u.可参评物资类别列表.Add(pzo);
                        if(!pzl.Contains(pzo))pzl.Add(pzo);
                    }

                    //联系电话
                    var tels = ws[i, 9].ReadCellAsString().Trim()
                        .Split("；，、; ,\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                    foreach (var tel in tels)
                    {
                        var t = tel.Trim();
                        if ('1' == tel[0] && tel.Length == 11)
                        {
                            if (string.IsNullOrWhiteSpace(u.联系方式.手机))
                                u.联系方式.手机 = t;
                            else
                            {
                                if (string.IsNullOrWhiteSpace(u.联系方式.其他))
                                    u.联系方式.其他 = t;
                                else
                                    u.联系方式.其他 += "；"+t;
                            }

                        }
                        else if ('0' == tel[0]) u.联系方式.固定电话 = t;
                        else continue;
                    }

                    //学历
                    var sxl = ws[i, 11].ReadCellAsString().Trim();
                    学历 xl;
                    if (Enum.TryParse(sxl, out xl)) u.学历信息.最高学历 = xl;
                    else switch (sxl)
                    {
                        case "大学本科": u.学历信息.最高学历 = 学历.本科; break;
                        default:
                            break;
                    }

                    Debug.Print("{11} - {0}/{1}/{2}/{3}/{4}/{5}/{6}/{7}/{8},{9}/{10}",
                        u.身份信息.姓名, u.身份信息.性别, u.身份信息.出生年月.ToString("yyyy.MM"),
                        u.工作经历信息.工作单位, u.学历信息.专业技术职称, u.学历信息.毕业院校,
                        u.工作经历信息.从事专业, string.Join("，",u.可参评物资类别列表/*.Select(o=>o.一级分类)*/),
                        u.联系方式.固定电话, u.联系方式.手机, u.学历信息.最高学历, i);
                    ++zjc;
                    if(add2db) Mongo.添加(u);
                }
                ws = wb[1];
                for (var i = 4; i <= ws.XS.LastRowNum; ++i)
                {
                    var u = new 专家();
                    u.Id = Mongo.NextId<专家>();
                    u.登录信息.登录名 = "pszj02" + (u.Id - 用户管理.专家Id基数).ToString("D6");
                    u.登录信息.密码 = "00000000";
                    u.所属管理单位 = 采购管理单位.成都军区物资采购机构_成都;
                    u.身份信息.专家类别 = 专家类别.军内;
                    u.身份信息.专家级别 = 专家级别.军区库专家;
                    u.所属地域.省份 = "四川省";
                    u.所属地域.城市 = "成都市";
                    u.审核数据.审核状态 = 审核状态.审核通过;

                    //姓名
                    u.登录信息.显示名 = u.身份信息.姓名 = u.联系方式.联系人 = ws[i, 1].ReadCellAsString().Trim();

                    //性别
                    var sxb = ws[i, 2].ReadCellAsString().Trim();
                    性别 xb;
                    if (Enum.TryParse(sxb, out xb)) u.身份信息.性别 = u.联系方式.性别 = xb;
                    else switch (sxb)
                        {
                            default:
                                break;
                        }

                    //出生年月
                    var d = ws[i, 3].ToString().Trim()
                        .Split(new[] { '.',',' }, StringSplitOptions.RemoveEmptyEntries);
                    u.身份信息.出生年月 = DateTime.ParseExact(d[0] + "." + d[1], "yyyy.M", null);

                    //工作单位
                    u.工作经历信息.工作单位 = ws[i, 4].ReadCellAsString().Trim();

                    //专业技术职称
                    var szc = ws[i, 5].ReadCellAsString().Trim().Replace(" ", string.Empty);
                    专业技术职称 zc;
                    if (Enum.TryParse(szc, out zc)) u.学历信息.专业技术职称 = zc;
                    else switch (szc)
                        {
                            case "正高（教授）级": u.学历信息.专业技术职称 = 专业技术职称.教授; break;
                            default:
                                break;
                        }

                    //毕业院校
                    u.学历信息.毕业院校 = ws[i, 6].ReadCellAsString().Trim();

                    //从事专业
                    u.工作经历信息.从事专业 = ws[i, 7].ReadCellAsString().Trim();

                    //评审专业
                    var pzs = ws[i, 8].ReadCellAsString().Trim()
                        .Split("、, \r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                    for (var pzn = 0; pzn < pzs.Length; ++pzn)
                    {
                        var pz = pzs[pzn];
                        if (1 == Mongo.计数<商品分类>(0, 0, Query<商品分类>.Where(o => o.分类名 == pz && o.父分类.商品分类ID == -1))
                            || 专家可评标专业.非商品分类评审专业.Contains(pz)
                            )
                        {
                            u.可参评物资类别列表.Add(new 供应商._产品类别 { 一级分类 = pz });
                            //u.可参评物资类别列表.Add(pz);
                            pzs[pzn] = null;
                            continue;
                        }
                        if (pz.EndsWith("类"))
                        {
                            var pzj = pz.Remove(pz.Length - 1);
                            if (1 == Mongo.计数<商品分类>(0, 0, Query<商品分类>.Where(o => o.分类名 == pzj && o.父分类.商品分类ID == -1))
                                || 专家可评标专业.非商品分类评审专业.Contains(pz)
                                )
                            {
                                u.可参评物资类别列表.Add(new 供应商._产品类别 { 一级分类 = pzj });
                                //u.可参评物资类别列表.Add(pzj);
                                pzs[pzn] = null;
                                continue;
                            }
                        }
                        else
                        {
                            var pzpl = pz + "类";
                            if (1 == Mongo.计数<商品分类>(0, 0, Query<商品分类>.Where(o => o.分类名 == pzpl && o.父分类.商品分类ID == -1))
                                || 专家可评标专业.非商品分类评审专业.Contains(pz)
                                )
                            {
                                u.可参评物资类别列表.Add(new 供应商._产品类别 { 一级分类 = pzpl });
                                //u.可参评物资类别列表.Add(pzpl);
                                pzs[pzn] = null;
                                continue;
                            }
                        }
                    }
                    pzs = pzs.Where(s => !string.IsNullOrWhiteSpace(s)).ToArray();
                    if (0 != pzs.Length)
                    {
                        var pzo = string.Join("、", pzs);
                        switch (pzo)
                        {
                            case "建筑、装修材料类":
                                u.可参评物资类别列表.Add(new 供应商._产品类别 { 一级分类 = "建筑装修材料"});
                                break;
                            default:
                                u.可参评物资类别列表.Add(new 供应商._产品类别 { 一级分类 = pzo });
                                //u.可参评物资类别列表.Add(pzo);
                                if (!pzl.Contains(pzo)) pzl.Add(pzo);
                                break;
                        }
                    }

                    //联系电话
                    var tels = ws[i, 9].ReadCellAsString().Trim()
                        .Split("；，、; ,\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                    foreach (var tel in tels)
                    {
                        var t = tel.Trim();
                        if ('1' == tel[0] && tel.Length == 11)
                        {
                            if (string.IsNullOrWhiteSpace(u.联系方式.手机))
                                u.联系方式.手机 = t;
                            else
                            {
                                if (string.IsNullOrWhiteSpace(u.联系方式.其他))
                                    u.联系方式.其他 = t;
                                else
                                    u.联系方式.其他 += "；" + t;
                            }

                        }
                        else if ('0' == tel[0]) u.联系方式.固定电话 = t;
                        else continue;
                    }

                    //学历
                    var sxl = ws[i, 10].ReadCellAsString().Trim();
                    学历 xl;
                    if (Enum.TryParse(sxl, out xl)) u.学历信息.最高学历 = xl;
                    else switch (sxl)
                        {
                            case "大学本科": u.学历信息.最高学历 = 学历.本科; break;
                            case "大学专科及以下": u.学历信息.最高学历 = 学历.大专及以下; break;
                            default:
                                break;
                        }

                    Debug.Print("{11} - {0}/{1}/{2}/{3}/{4}/{5}/{6}/{7}/{8},{9}/{10}",
                        u.身份信息.姓名, u.身份信息.性别, u.身份信息.出生年月.ToString("yyyy.MM"),
                        u.工作经历信息.工作单位, u.学历信息.专业技术职称, u.学历信息.毕业院校,
                        u.工作经历信息.从事专业, string.Join("，", u.可参评物资类别列表/*.Select(o=>o.一级分类)*/),
                        u.联系方式.固定电话, u.联系方式.手机, u.学历信息.最高学历, i);
                    ++zjc;
                    if (add2db) Mongo.添加(u);
                }
            }
            if (0 != pzl.Count) File.WriteAllLines(fp + ".pz.txt", pzl);
            return zjc;
        }
        [Obsolete]
        public static int 导入2(bool add2db = false)
        {
            var fp = ChooseOpenFile("复件 成套设备招标局  随机取数据.xlsx");
            if (string.IsNullOrWhiteSpace(fp)) return 0;
            var pd = 工具Data.导入2修正;
            var zjc = 0;
            var pzl = new List<string>();
            using (var fs = File.Open(fp, FileMode.Open))
            {
                var wb = new XWorkbook(fs);
                var ws = wb[0];
                for (var i = 2; i <= ws.XS.LastRowNum; ++i)
                {
                    var u = new 专家();
                    u.Id = Mongo.NextId<专家>();
                    u.登录信息.登录名 = "pszj02" + (u.Id - 用户管理.专家Id基数).ToString("D6");
                    u.登录信息.密码 = "00000000";
                    u.所属管理单位 = 采购管理单位.成都军区物资采购机构_成都;
                    u.身份信息.专家类别 = 专家类别.地方;
                    u.身份信息.专家级别 = 专家级别.地方库专家;
                    u.所属地域.省份 = "四川省";
                    u.所属地域.城市 = "成都市";
                    u.审核数据.审核状态 = 审核状态.审核通过;

                    //姓名
                    u.登录信息.显示名 = u.身份信息.姓名 = u.联系方式.联系人 = ws[i, 0].ReadCellAsString().Trim();

                    //毕业院校
                    u.学历信息.毕业院校 = ws[i, 1].ReadCellAsString().Trim();

                    //从事专业
                    u.工作经历信息.从事专业 = ws[i, 2].ReadCellAsString().Trim();

                    //评审专业
                    var pzs = ws[i, 3].ReadCellAsString().Trim()
                        .Split("、, \r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                    for (var pzn = 0; pzn < pzs.Length; ++pzn)
                    {
                        var pz = pzs[pzn];
                        if (1 == Mongo.计数<商品分类>(0, 0, Query<商品分类>.Where(o => o.分类名 == pz && o.父分类.商品分类ID == -1))
                            //|| 专家可评标专业.非商品分类评审专业.Contains(pz)
                            )
                        {
                            u.可参评物资类别列表.Add(new 供应商._产品类别 { 一级分类 = pz });
                            //u.可参评物资类别列表.Add(pz);
                            pzs[pzn] = null;
                            continue;
                        }
                        if (pz.EndsWith("类"))
                        {
                            var pzj = pz.Remove(pz.Length - 1);
                            if (1 == Mongo.计数<商品分类>(0, 0, Query<商品分类>.Where(o => o.分类名 == pzj && o.父分类.商品分类ID == -1))
                                //|| 专家可评标专业.非商品分类评审专业.Contains(pz)
                                )
                            {
                                u.可参评物资类别列表.Add(new 供应商._产品类别 {一级分类 = pzj});
                                //u.可参评物资类别列表.Add(pzj);
                                pzs[pzn] = null;
                                continue;
                            }
                        }
                        else
                        {
                            var pzpl = pz + "类";
                            if (1 == Mongo.计数<商品分类>(0, 0, Query<商品分类>.Where(o => o.分类名 == pzpl && o.父分类.商品分类ID == -1))
                                //|| 专家可评标专业.非商品分类评审专业.Contains(pz)
                                )
                            {
                                u.可参评物资类别列表.Add(new 供应商._产品类别 { 一级分类 = pzpl });
                                //u.可参评物资类别列表.Add(pzpl);
                                pzs[pzn] = null;
                                continue;
                            }
                        }
                    }
                    pzs = pzs.Where(s => !string.IsNullOrWhiteSpace(s)).ToArray();
                    if (0 != pzs.Length)
                    {
                        var pzo = string.Join("、", pzs);
                        
                        pzo = pd.ContainsKey(pzo) ? pd[pzo] : pzo;
                        var pzos = pzo.Split("、".ToCharArray());

                        foreach (var ppz in pzos)
                        {
                            u.可参评物资类别列表.Add(new 供应商._产品类别 { 一级分类 = ppz });
                            //u.可参评物资类别列表.Add(ppz);
                            if (!专家可评标专业.非商品分类评审专业.Contains(ppz) && !pzl.Contains(ppz))
                                pzl.Add(ppz);
                        }
                    }

                    //工作单位
                    u.工作经历信息.工作单位 = ws[i, 4].ReadCellAsString().Trim();

                    //联系电话
                    var tels = ws[i, 5].ReadCellAsString().Trim()
                        .Split("；，、; ,.。 /\\\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                    foreach (var tel in tels)
                    {
                        var t = tel.Trim();
                        if ('1' == tel[0] && tel.Length == 11)
                        {
                            if (string.IsNullOrWhiteSpace(u.联系方式.手机))
                                u.联系方式.手机 = t;
                            else
                            {
                                if (string.IsNullOrWhiteSpace(u.联系方式.其他))
                                    u.联系方式.其他 = t;
                                else
                                    u.联系方式.其他 += "；" + t;
                            }
                        }
                        else if ('0' == tel[0] || ('2' <= tel[0] && tel[0] <= '9'))
                        {
                            if (string.IsNullOrWhiteSpace(u.联系方式.固定电话))
                                u.联系方式.固定电话 = t;
                            else
                            {
                                if (string.IsNullOrWhiteSpace(u.联系方式.其他))
                                    u.联系方式.其他 = t;
                                else
                                    u.联系方式.其他 += "；" + t;
                            }
                        }
                        else
                        {
                            if (string.IsNullOrWhiteSpace(u.联系方式.其他))
                                u.联系方式.其他 = t;
                            else
                                u.联系方式.其他 += "；" + t;
                        }
                    }

                    Debug.Print("{0} - {1}/{2}/{3}/{4}/{5}/{6}", i,
                        u.身份信息.姓名, u.学历信息.毕业院校, u.工作经历信息.从事专业,
                        string.Join("，", u.可参评物资类别列表/*.Select(o => o.一级分类)*/),
                        u.联系方式.手机, u.联系方式.其他);
                    ++zjc;
                    if(add2db) Mongo.添加(u);
                }
            }
            if (0 != pzl.Count) File.WriteAllLines(fp + ".pz.txt", pzl);
            return zjc;
        }
        [Obsolete]
        public static int 导入3(bool add2db = false)
        {
            var fp = ChooseOpenFile("复件 四川省军区上报专家、供应商.xlsx");
            if (string.IsNullOrWhiteSpace(fp)) return 0;
            var zjc = 0;
            var pzl = new List<string>();
            using (var fs = File.Open(fp, FileMode.Open))
            {
                var wb = new XWorkbook(fs);
                var ws = wb[0];
                for (var i = 3; i <= 18; ++i)
                {
                    var u = new 专家();
                    u.Id = Mongo.NextId<专家>();
                    u.登录信息.登录名 = "pszj01" + (u.Id - 用户管理.专家Id基数).ToString("D6");
                    u.登录信息.密码 = "00000000";
                    u.所属管理单位 = 采购管理单位.成都军区物资采购机构_成都;
                    u.身份信息.专家类别 = 专家类别.军内;
                    u.身份信息.专家级别 = 专家级别.军区库专家;
                    u.所属地域.省份 = "四川省";
                    u.所属地域.城市 = "成都市";
                    u.审核数据.审核状态 = 审核状态.审核通过;

                    //姓名
                    u.登录信息.显示名 = u.身份信息.姓名 = u.联系方式.联系人 = ws[i, 1].ReadCellAsString().Trim();

                    //性别
                    var sxb = ws[i, 2].ReadCellAsString().Trim();
                    性别 xb;
                    if (Enum.TryParse(sxb, out xb)) u.身份信息.性别 = u.联系方式.性别 = xb;
                    else switch (sxb)
                        {
                            default:
                                break;
                        }

                    //工作单位
                    u.工作经历信息.工作单位 = ws[i, 3].ReadCellAsString().Trim();

                    //专业技术职称
                    var szc = ws[i, 4].ReadCellAsString().Trim();
                    专业技术职称 zc;
                    if (Enum.TryParse(szc, out zc)) u.学历信息.专业技术职称 = zc;
                    else switch (szc)
                        {
                            case "正高（教授）级": u.学历信息.专业技术职称 = 专业技术职称.教授; break;
                            case "高工": u.学历信息.专业技术职称 = 专业技术职称.高级工程师; break;
                            default:
                                break;
                        }

                    //评审专业
                    var pzs = ws[i, 5].ReadCellAsString().Trim()
                        .Split("、, \r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                    for (var pzn = 0; pzn < pzs.Length; ++pzn)
                    {
                        var pz = pzs[pzn];
                        if (1 == Mongo.计数<商品分类>(0, 0, Query<商品分类>.Where(o => o.分类名 == pz && o.父分类.商品分类ID == -1))
                            || 专家可评标专业.非商品分类评审专业.Contains(pz)
                            )
                        {
                            u.可参评物资类别列表.Add(new 供应商._产品类别 { 一级分类 = pz });
                            //u.可参评物资类别列表.Add(pz);
                            pzs[pzn] = null;
                            continue;
                        }
                        if (pz.EndsWith("类"))
                        {
                            var pzj = pz.Remove(pz.Length - 1);
                            if (1 == Mongo.计数<商品分类>(0, 0, Query<商品分类>.Where(o => o.分类名 == pzj && o.父分类.商品分类ID == -1))
                                || 专家可评标专业.非商品分类评审专业.Contains(pz)
                                )
                            {
                                u.可参评物资类别列表.Add(new 供应商._产品类别 { 一级分类 = pzj });
                                //u.可参评物资类别列表.Add(pzj);
                                pzs[pzn] = null;
                                continue;
                            }
                        }
                        else
                        {
                            var pzpl = pz + "类";
                            if (1 == Mongo.计数<商品分类>(0, 0, Query<商品分类>.Where(o => o.分类名 == pzpl && o.父分类.商品分类ID == -1))
                                || 专家可评标专业.非商品分类评审专业.Contains(pz)
                                )
                            {
                                u.可参评物资类别列表.Add(new 供应商._产品类别 { 一级分类 = pzpl });
                                //u.可参评物资类别列表.Add(pzpl);
                                pzs[pzn] = null;
                                continue;
                            }
                        }
                    }
                    pzs = pzs.Where(s => !string.IsNullOrWhiteSpace(s)).ToArray();
                    if (0 != pzs.Length)
                    {
                        var pzo = string.Join("、", pzs);
                        u.可参评物资类别列表.Add(new 供应商._产品类别 { 一级分类 = pzo });
                        //u.可参评物资类别列表.Add(pzo);
                        if (!pzl.Contains(pzo)) pzl.Add(pzo);
                    }

                    //联系电话
                    var tels = ws[i, 6].ReadCellAsString().Trim()
                        .Split("；，、; ,\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                    foreach (var tel in tels)
                    {
                        var t = tel.Trim();
                        if ('1' == tel[0] && tel.Length == 11)
                        {
                            if (string.IsNullOrWhiteSpace(u.联系方式.手机))
                                u.联系方式.手机 = t;
                            else
                            {
                                if (string.IsNullOrWhiteSpace(u.联系方式.其他))
                                    u.联系方式.其他 = t;
                                else
                                    u.联系方式.其他 += "；" + t;
                            }

                        }
                        else if ('0' == tel[0]) u.联系方式.固定电话 = t;
                        else continue;
                    }

                    Debug.Print("{0} - {1}/{2}/{3}/{4}/{5}/{6}/{7}/{8}", i,
                        u.身份信息.姓名, u.身份信息.性别, u.工作经历信息.工作单位,
                        u.学历信息.专业技术职称, string.Join("，", u.可参评物资类别列表/*.Select(o => o.一级分类)*/),
                        u.联系方式.固定电话, u.联系方式.手机, u.联系方式.其他);
                    ++zjc;
                    if(add2db) Mongo.添加(u);
                }
            }
            if (0 != pzl.Count) File.WriteAllLines(fp + ".pz.txt", pzl);
            return zjc;
        }
        [Obsolete]
        public static void 添加专家可评标类别()
        {
            专家可评标专业[] l =
            {
                new 专家可评标专业 {专业名 = "野营工程器材"}, 
                new 专家可评标专业 {专业名 = "医疗卫生装备"},
                new 专家可评标专业 {专业名 = "油库器材"},
                new 专家可评标专业 {专业名 = "油料器材"},
                new 专家可评标专业 {专业名 = "机电"},
                new 专家可评标专业 {专业名 = "绿化工程"},
                new 专家可评标专业 {专业名 = "计算机"},
                new 专家可评标专业 {专业名 = "绿化"},
                new 专家可评标专业 {专业名 = "办公家具"},
                new 专家可评标专业 {专业名 = "GIS"},
                new 专家可评标专业 {专业名 = "GPS"},
                new 专家可评标专业 {专业名 = "中央空调"},
                new 专家可评标专业 {专业名 = "临床内科教学"},
                new 专家可评标专业 {专业名 = "临床医学"},
                new 专家可评标专业 {专业名 = "交通运输工程"},
                new 专家可评标专业 {专业名 = "产品质量检验"},
                new 专家可评标专业 {专业名 = "人事管理"},
                new 专家可评标专业 {专业名 = "仪器仪表"},
                new 专家可评标专业 {专业名 = "仪器设备管理"},
                new 专家可评标专业 {专业名 = "会计"},
                new 专家可评标专业 {专业名 = "会计教学"},
                new 专家可评标专业 {专业名 = "体育设备采购"},
                new 专家可评标专业 {专业名 = "供配电及自动化设计施工"},
                new 专家可评标专业 {专业名 = "保险培训"},
                new 专家可评标专业 {专业名 = "信息化工程"},
                new 专家可评标专业 {专业名 = "光通信"},
                new 专家可评标专业 {专业名 = "公路工程"},
                new 专家可评标专业 {专业名 = "公路桥梁"},
                new 专家可评标专业 {专业名 = "内部审计"},
                new 专家可评标专业 {专业名 = "办公管理"},
                new 专家可评标专业 {专业名 = "化学工程"},
                new 专家可评标专业 {专业名 = "化工机械教育"},
                new 专家可评标专业 {专业名 = "化工能源"},
                new 专家可评标专业 {专业名 = "医学检验"},
                new 专家可评标专业 {专业名 = "医疗器械"},
                new 专家可评标专业 {专业名 = "医疗设备管理"},
                new 专家可评标专业 {专业名 = "医院管理"},
                new 专家可评标专业 {专业名 = "卫星遥感"},
                new 专家可评标专业 {专业名 = "卫生检验"},
                new 专家可评标专业 {专业名 = "合同管理"},
                new 专家可评标专业 {专业名 = "后勤建设"},
                new 专家可评标专业 {专业名 = "后勤建设教学"},
                new 专家可评标专业 {专业名 = "园林土建装饰"},
                new 专家可评标专业 {专业名 = "园林景观"},
                new 专家可评标专业 {专业名 = "园林绿化工程"},
                new 专家可评标专业 {专业名 = "土地评估"},
                new 专家可评标专业 {专业名 = "土建工程"},
                new 专家可评标专业 {专业名 = "土木工程"},
                new 专家可评标专业 {专业名 = "地理信息系统"},
                new 专家可评标专业 {专业名 = "地质信息化设备"},
                new 专家可评标专业 {专业名 = "地质勘查"},
                new 专家可评标专业 {专业名 = "地质工程"},
                new 专家可评标专业 {专业名 = "地质测绘"},
                new 专家可评标专业 {专业名 = "城市建设配套"},
                new 专家可评标专业 {专业名 = "城市排水"},
                new 专家可评标专业 {专业名 = "城市规划"},
                new 专家可评标专业 {专业名 = "大地测量"},
                new 专家可评标专业 {专业名 = "安防监控"},
                new 专家可评标专业 {专业名 = "安防管理"},
                new 专家可评标专业 {专业名 = "审计"},
                new 专家可评标专业 {专业名 = "岩土工程"},
                new 专家可评标专业 {专业名 = "工业与民用建筑"},
                new 专家可评标专业 {专业名 = "工业炉设计"},
                new 专家可评标专业 {专业名 = "工业电气自动化"},
                new 专家可评标专业 {专业名 = "工商管理"},
                new 专家可评标专业 {专业名 = "工商管理教育"},
                new 专家可评标专业 {专业名 = "工程咨询"},
                new 专家可评标专业 {专业名 = "工程审核"},
                new 专家可评标专业 {专业名 = "工程审计"},
                new 专家可评标专业 {专业名 = "工程建筑"},
                new 专家可评标专业 {专业名 = "工程建筑建设质量监督及设计"},
                new 专家可评标专业 {专业名 = "工程建筑管理"},
                new 专家可评标专业 {专业名 = "工程建设"},
                new 专家可评标专业 {专业名 = "工程技术"},
                new 专家可评标专业 {专业名 = "工程技术管理"},
                new 专家可评标专业 {专业名 = "工程招投标"},
                new 专家可评标专业 {专业名 = "工程施工"},
                new 专家可评标专业 {专业名 = "工程机械"},
                new 专家可评标专业 {专业名 = "工程机械设备维修"},
                new 专家可评标专业 {专业名 = "工程机械设备销售"},
                new 专家可评标专业 {专业名 = "工程水电气管网"},
                new 专家可评标专业 {专业名 = "工程监理"},
                new 专家可评标专业 {专业名 = "工程管理"},
                new 专家可评标专业 {专业名 = "工程经济"},
                new 专家可评标专业 {专业名 = "工程编制"},
                new 专家可评标专业 {专业名 = "工程设计"},
                new 专家可评标专业 {专业名 = "工程评标"},
                new 专家可评标专业 {专业名 = "工程过控"},
                new 专家可评标专业 {专业名 = "工程造价"},
                new 专家可评标专业 {专业名 = "工程采购"},
                new 专家可评标专业 {专业名 = "工程预决算"},
                new 专家可评标专业 {专业名 = "工艺设计"},
                new 专家可评标专业 {专业名 = "市场营销"},
                new 专家可评标专业 {专业名 = "市政工程"},
                new 专家可评标专业 {专业名 = "市政建设管理"},
                new 专家可评标专业 {专业名 = "市政道路"},
                new 专家可评标专业 {专业名 = "幕墙工程"},
                new 专家可评标专业 {专业名 = "广播电视"},
                new 专家可评标专业 {专业名 = "广电设备"},
                new 专家可评标专业 {专业名 = "建筑业及交通业"},
                new 专家可评标专业 {专业名 = "建筑安装"},
                new 专家可评标专业 {专业名 = "建筑工程"},
                new 专家可评标专业 {专业名 = "建筑工程咨询"},
                new 专家可评标专业 {专业名 = "建筑工程施工"},
                new 专家可评标专业 {专业名 = "建筑工程监理"},
                new 专家可评标专业 {专业名 = "建筑工程管理"},
                new 专家可评标专业 {专业名 = "建筑工程结构"},
                new 专家可评标专业 {专业名 = "建筑工程设备"},
                new 专家可评标专业 {专业名 = "建筑工程造价"},
                new 专家可评标专业 {专业名 = "建筑技术"},
                new 专家可评标专业 {专业名 = "建筑智能化"},
                new 专家可评标专业 {专业名 = "建筑材料"},
                new 专家可评标专业 {专业名 = "建筑环境与设备工程"},
                new 专家可评标专业 {专业名 = "建筑管理"},
                new 专家可评标专业 {专业名 = "建筑经济"},
                new 专家可评标专业 {专业名 = "建筑结构"},
                new 专家可评标专业 {专业名 = "建筑装饰"},
                new 专家可评标专业 {专业名 = "建筑设备安装"},
                new 专家可评标专业 {专业名 = "建筑设计"},
                new 专家可评标专业 {专业名 = "建筑造价"},
                new 专家可评标专业 {专业名 = "强弱电"},
                new 专家可评标专业 {专业名 = "微波技术"},
                new 专家可评标专业 {专业名 = "成本核算"},
                new 专家可评标专业 {专业名 = "房地产开发"},
                new 专家可评标专业 {专业名 = "房地产评估"},
                new 专家可评标专业 {专业名 = "房屋建筑工程"},
                new 专家可评标专业 {专业名 = "房建及市政公路"},
                new 专家可评标专业 {专业名 = "技术开发与管理"},
                new 专家可评标专业 {专业名 = "投资咨询"},
                new 专家可评标专业 {专业名 = "投资计划管理"},
                new 专家可评标专业 {专业名 = "拖拉机制造与维修"},
                new 专家可评标专业 {专业名 = "招投标管理"},
                new 专家可评标专业 {专业名 = "招标代理"},
                new 专家可评标专业 {专业名 = "招标管理"},
                new 专家可评标专业 {专业名 = "政府采购"},
                new 专家可评标专业 {专业名 = "教学"},
                new 专家可评标专业 {专业名 = "教学设备管理"},
                new 专家可评标专业 {专业名 = "教学设备采购与管理"},
                new 专家可评标专业 {专业名 = "教育"},
                new 专家可评标专业 {专业名 = "教育技术学"},
                new 专家可评标专业 {专业名 = "教育技术装备教育"},
                new 专家可评标专业 {专业名 = "数学教学"},
                new 专家可评标专业 {专业名 = "新闻"},
                new 专家可评标专业 {专业名 = "施工技术"},
                new 专家可评标专业 {专业名 = "施工技术及造价"},
                new 专家可评标专业 {专业名 = "施工技术工程质量安全管理"},
                new 专家可评标专业 {专业名 = "施工监理"},
                new 专家可评标专业 {专业名 = "施工管理"},
                new 专家可评标专业 {专业名 = "无线电通讯"},
                new 专家可评标专业 {专业名 = "景观园林"},
                new 专家可评标专业 {专业名 = "智能建筑工程"},
                new 专家可评标专业 {专业名 = "暖通工程"},
                new 专家可评标专业 {专业名 = "机械制造工艺及设备"},
                new 专家可评标专业 {专业名 = "机械制造自动化及控制"},
                new 专家可评标专业 {专业名 = "机械工程"},
                new 专家可评标专业 {专业名 = "机械工程设备安装维护"},
                new 专家可评标专业 {专业名 = "机械工艺设计"},
                new 专家可评标专业 {专业名 = "机械设备制造"},
                new 专家可评标专业 {专业名 = "机械设备管理"},
                new 专家可评标专业 {专业名 = "机械设计与制造"},
                new 专家可评标专业 {专业名 = "机械设计及自动化"},
                new 专家可评标专业 {专业名 = "机械锻压"},
                new 专家可评标专业 {专业名 = "机电一体化自动控制"},
                new 专家可评标专业 {专业名 = "机电产品销售"},
                new 专家可评标专业 {专业名 = "机电及水电站电器自动化设计"},
                new 专家可评标专业 {专业名 = "机电安装"},
                new 专家可评标专业 {专业名 = "机电工程"},
                new 专家可评标专业 {专业名 = "机电设备"},
                new 专家可评标专业 {专业名 = "机电设备技术服务"},
                new 专家可评标专业 {专业名 = "机电设备设计"},
                new 专家可评标专业 {专业名 = "机电设备贸易"},
                new 专家可评标专业 {专业名 = "材料采购"},
                new 专家可评标专业 {专业名 = "林业及木材产品利用研究"},
                new 专家可评标专业 {专业名 = "林业科研与推广"},
                new 专家可评标专业 {专业名 = "林业管理"},
                new 专家可评标专业 {专业名 = "档案管理"},
                new 专家可评标专业 {专业名 = "森林消防"},
                new 专家可评标专业 {专业名 = "水利工程"},
                new 专家可评标专业 {专业名 = "水处理"},
                new 专家可评标专业 {专业名 = "汽车制造与维修"},
                new 专家可评标专业 {专业名 = "油气储运设计及总承包管理"},
                new 专家可评标专业 {专业名 = "法律"},
                new 专家可评标专业 {专业名 = "测绘科学与技术"},
                new 专家可评标专业 {专业名 = "消防工程"},
                new 专家可评标专业 {专业名 = "烹饪及食品加工"},
                new 专家可评标专业 {专业名 = "物业管理"},
                new 专家可评标专业 {专业名 = "物资设备管理"},
                new 专家可评标专业 {专业名 = "环境保护"},
                new 专家可评标专业 {专业名 = "环境工程"},
                new 专家可评标专业 {专业名 = "电力工程"},
                new 专家可评标专业 {专业名 = "电子信息工程教学及科研"},
                new 专家可评标专业 {专业名 = "电子信息技术"},
                new 专家可评标专业 {专业名 = "电子器械"},
                new 专家可评标专业 {专业名 = "电机组装教学"},
                new 专家可评标专业 {专业名 = "电梯安全技术检验"},
                new 专家可评标专业 {专业名 = "电梯检测"},
                new 专家可评标专业 {专业名 = "电气工程"},
                new 专家可评标专业 {专业名 = "电气工程设计"},
                new 专家可评标专业 {专业名 = "电气施工技术"},
                new 专家可评标专业 {专业名 = "电气自动化"},
                new 专家可评标专业 {专业名 = "电气设计"},
                new 专家可评标专业 {专业名 = "科技管理"},
                new 专家可评标专业 {专业名 = "科研管理"},
                new 专家可评标专业 {专业名 = "精密机械"},
                new 专家可评标专业 {专业名 = "系统集成"},
                new 专家可评标专业 {专业名 = "纺织化学与染整"},
                new 专家可评标专业 {专业名 = "经济管理"},
                new 专家可评标专业 {专业名 = "结构加固"},
                new 专家可评标专业 {专业名 = "给排水工程"},
                new 专家可评标专业 {专业名 = "统计"},
                new 专家可评标专业 {专业名 = "绿化园林"},
                new 专家可评标专业 {专业名 = "绿化工程技术管理"},
                new 专家可评标专业 {专业名 = "网络应用管理"},
                new 专家可评标专业 {专业名 = "自动化"},
                new 专家可评标专业 {专业名 = "自动控制"},
                new 专家可评标专业 {专业名 = "航空工程"},
                new 专家可评标专业 {专业名 = "行政管理"},
                new 专家可评标专业 {专业名 = "装修"},
                new 专家可评标专业 {专业名 = "装饰"},
                new 专家可评标专业 {专业名 = "计算机安全"},
                new 专家可评标专业 {专业名 = "计算机应用"},
                new 专家可评标专业 {专业名 = "计算机硬件"},
                new 专家可评标专业 {专业名 = "计算机网络"},
                new 专家可评标专业 {专业名 = "计算机软件"},
                new 专家可评标专业 {专业名 = "计算机通信"},
                new 专家可评标专业 {专业名 = "设备使用"},
                new 专家可评标专业 {专业名 = "设备安装"},
                new 专家可评标专业 {专业名 = "设备管理"},
                new 专家可评标专业 {专业名 = "设备管理医学工程技术评审"},
                new 专家可评标专业 {专业名 = "设备维修"},
                new 专家可评标专业 {专业名 = "设备采购"},
                new 专家可评标专业 {专业名 = "试验针灸学"},
                new 专家可评标专业 {专业名 = "财务管理"},
                new 专家可评标专业 {专业名 = "货物管理"},
                new 专家可评标专业 {专业名 = "质量控制"},
                new 专家可评标专业 {专业名 = "资产评估"},
                new 专家可评标专业 {专业名 = "资源环境与城乡规划"},
                new 专家可评标专业 {专业名 = "起重运输"},
                new 专家可评标专业 {专业名 = "软件开发"},
                new 专家可评标专业 {专业名 = "轻工纺织检验监管"},
                new 专家可评标专业 {专业名 = "进出口机电产品检验"},
                new 专家可评标专业 {专业名 = "通信工程"},
                new 专家可评标专业 {专业名 = "造价咨询"},
                new 专家可评标专业 {专业名 = "造价管理"},
                new 专家可评标专业 {专业名 = "遥控"},
                new 专家可评标专业 {专业名 = "金属热处理铸造工艺及设备"},
                new 专家可评标专业 {专业名 = "针灸学"},
                new 专家可评标专业 {专业名 = "铁路工程"},
                new 专家可评标专业 {专业名 = "项目审核"},
                new 专家可评标专业 {专业名 = "项目管理"},
                new 专家可评标专业 {专业名 = "高中信息技术教学"},
            };

            Mongo.Coll<专家可评标专业>().InsertBatch(l);
        }
#endif
        #endregion

        public static int 模糊专家信息()
        {
            var l = 用户管理.查询用户<专家>(0, 0);
            var i = 0;
            foreach (var zj in l)
            {
                ++i;
                zj.登录信息.显示名 = zj.身份信息.姓名 = "赵钱孙" + i;
                if (null != zj.工作经历信息.工作单位)
                zj.工作经历信息.工作单位 = zj.工作经历信息.工作单位
                    .Replace("四川", "天朝")
                    .Replace("成都", "帝都")
                    .Replace("企业","集团")
                    ;
                zj.联系方式 = new _联系方式
                {
                    QQ = "10086",
                    固定电话 = "028-88776655",
                    传真 = "028-88774433",
                    手机 = "18987654321",
                    其他 = "13912345678",
                    微信 = "noweixin"+i,
                    微博 = "noweibo"+i,
                    电子邮件 = "no"+i+"@email.com",
                    联系人 = "赵钱孙"+i,
                    性别 = 性别.保密
                };
                用户管理.更新用户(zj);
            }
            return i;
        }

        public static List<String> 去掉商品分类名的空格()
        {
            //去掉商品分类名的空格
            var logs = new List<String>();

            var cats = Mongo.查询<商品分类>(0, 0, sorting: SortBy<商品分类>.Ascending(o => o.Id));
            foreach (var cat in cats)
            {
                var hit = false;
                var 新属性模板 = new Dictionary<string, Dictionary<string, 商品属性数据>>();
                foreach (var kv0 in cat.商品属性模板)
                {
                    var 属性组名 = kv0.Key;
                    var 属性组名OK = 属性组名.Trim();
                    if (属性组名OK != 属性组名)
                    {
                        hit = true;
                        logs.Add(string.Format("Trim属性组名:{0}({1}).{2}({3})",
                            cat.分类名, cat.Id, 属性组名OK, 属性组名));
                    }
                    if (string.IsNullOrWhiteSpace(属性组名OK))
                    {
                        hit = true;
                        logs.Add(string.Format("去掉空白属性组:{0}({1})",
                            cat.分类名, cat.Id));
                        continue;
                    }
                    新属性模板[属性组名OK] = new Dictionary<string, 商品属性数据>();

                    foreach (var kv1 in kv0.Value)
                    {
                        var 属性名 = kv1.Key;
                        var 属性名OK = 属性名.Trim();
                        if (属性名OK != 属性名)
                        {
                            hit = true;
                            logs.Add(string.Format("Trim属性名:{0}({1}).{2}.{3}({4})",
                                cat.分类名, cat.Id, 属性组名OK, 属性名OK, 属性名));
                        }
                        if (string.IsNullOrWhiteSpace(属性名OK))
                        {
                            hit = true;
                            logs.Add(string.Format("去掉空白属性:{0}({1}).{2}",
                                cat.分类名, cat.Id, 属性组名OK));
                            continue;
                        }
                        新属性模板[属性组名OK][属性名OK] = new 商品属性数据
                        {
                            频率 = kv1.Value.频率,
                            必需 = kv1.Value.必需,
                            销售属性 = kv1.Value.销售属性,
                            属性类型 = kv1.Value.属性类型,
                            值 = new List<string>()
                        };
                        if (cat.商品属性模板[属性组名][属性名].属性类型 != 属性类型.复选) continue;
                        foreach (var val in kv1.Value.值)
                        {
                            var valOK = val.Trim();
                            if (valOK != val)
                            {
                                hit = true;
                                logs.Add(string.Format("Trim属性值:{0}({1}).{2}.{3}.{4}({5})",
                                    cat.分类名, cat.Id, 属性组名OK, 属性名OK, valOK, val));
                            }
                            if (string.IsNullOrWhiteSpace(valOK))
                            {
                                hit = true;
                                logs.Add(string.Format("去掉空白属性值:{0}({1}).{2}.{3}",
                                    cat.分类名, cat.Id, 属性组名OK, 属性名OK));
                                continue;
                            }
                            新属性模板[属性组名OK][属性名OK].值.Add(valOK);
                        }
                    }
                }
                if (!hit) continue;
                cat.商品属性模板 = 新属性模板;
                logs.Add("------------------------------");
                Mongo.更新(cat, false);
            }
            logs.Add("==============================");

            var prods = Mongo.查询<商品>(0, 0, sorting: SortBy<商品>.Ascending(o => o.Id));
            foreach (var prod in prods)
            {
                var hit = false;
                var 新商品属性 = new Dictionary<string, Dictionary<string, List<string>>>();
                foreach (var kv0 in prod.商品数据.商品属性)
                {
                    var 属性组名 = kv0.Key;
                    var 属性组名OK = 属性组名.Trim();
                    if (属性组名OK != 属性组名)
                    {
                        hit = true;
                        logs.Add(string.Format("Trim商品属性组名:{0}({1}).{2}({3})",
                            prod.商品信息.商品名, prod.Id, 属性组名OK, 属性组名));
                    }
                    if (string.IsNullOrWhiteSpace(属性组名OK))
                    {
                        hit = true;
                        logs.Add(string.Format("去掉商品空白属性组:{0}({1})",
                            prod.商品信息.商品名, prod.Id));
                        continue;
                    }
                    新商品属性[属性组名OK] = new Dictionary<string, List<string>>();

                    foreach (var kv1 in kv0.Value)
                    {
                        var 属性名 = kv1.Key;
                        var 属性名OK = 属性名.Trim();
                        if (属性名OK != 属性名)
                        {
                            hit = true;
                            logs.Add(string.Format("Trim商品属性名:{0}({1}).{2}.{3}({4})",
                                prod.商品信息.商品名, prod.Id, 属性组名OK, 属性名OK, 属性名));
                        }
                        if (string.IsNullOrWhiteSpace(属性名OK))
                        {
                            hit = true;
                            logs.Add(string.Format("去掉商品空白属性:{0}({1}).{2}",
                                prod.商品信息.商品名, prod.Id, 属性组名OK));
                            continue;
                        }
                        新商品属性[属性组名OK][属性名OK] = new List<string>();

                        foreach (var val in kv1.Value)
                        {
                            var valOK = val.Trim();
                            if (valOK != val)
                            {
                                hit = true;
                                logs.Add(string.Format("Trim商品属性值:{0}({1}).{2}.{3}.{4}({5})",
                                    prod.商品信息.商品名, prod.Id, 属性组名OK, 属性名OK, valOK, val));
                            }
                            if (string.IsNullOrWhiteSpace(valOK))
                            {
                                hit = true;
                                logs.Add(string.Format("去掉商品空白属性值:{0}({1}).{2}.{3}",
                                    prod.商品信息.商品名, prod.Id, 属性组名OK, 属性名OK));
                                continue;
                            }
                            新商品属性[属性组名OK][属性名OK].Add(valOK);
                        }
                    }
                }
                if (hit) prod.商品数据.商品属性 = 新商品属性;

                var 新价格属性组合 = new 商品._价格属性组合();
                foreach (var val in prod.销售信息.价格属性组合.属性列表)
                {
                    var valOK = val.Trim();
                    if (valOK != val)
                    {
                        hit = true;
                        logs.Add(string.Format("Trim价格属性组合.属性列表:{0}({1}).{2}({3})",
                            prod.商品信息.商品名, prod.Id, valOK, val));
                    }
                    if (string.IsNullOrWhiteSpace(valOK))
                    {
                        hit = true;
                        logs.Add(string.Format("去掉商品价格属性组合.属性列表.空白属性值:{0}({1})",
                            prod.商品信息.商品名, prod.Id));
                        continue;
                    }
                    新价格属性组合.属性列表.Add(valOK);
                }

                foreach (var cb in prod.销售信息.价格属性组合.组合列表)
                {
                    var 新价格属性搭配 = new 商品._价格属性组合._价格属性搭配
                    {
                        价格 = cb.价格,
                        库存状态 = cb.库存状态,
                        销量 = cb.销量,
                        属性值表 = new List<string>()
                    };
                    foreach (var val in cb.属性值表)
                    {
                        var valOK = val.Trim();
                        if (valOK != val)
                        {
                            hit = true;
                            logs.Add(string.Format("Trim价格属性组合.组合列表.属性值表:{0}({1}).{2}({3})",
                                prod.商品信息.商品名, prod.Id, valOK, val));
                        }
                        if (string.IsNullOrWhiteSpace(valOK))
                        {
                            hit = true;
                            logs.Add(string.Format("去掉商品价格属性组合.组合列表.属性值表.空白属性值:{0}({1})",
                                prod.商品信息.商品名, prod.Id));
                            continue;
                        }
                        新价格属性搭配.属性值表.Add(valOK);
                    }
                }
                if (!hit) continue;
                prod.销售信息.价格属性组合 = 新价格属性组合;
                logs.Add("------------------------------");
                Mongo.更新(prod, false);

            }

            //foreach (var log in logs) Debug.Print(log);
            if (logs.Count > 1) File.WriteAllLines(@"D:\trimlog.txt", logs, Encoding.Unicode);
            
            return logs;
        }

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
        public static ImpExpInfo ExpColl<T>()
        {
            var collName = typeof(T).Name;
            var fp = ChooseSaveFile("导出Coll-" + collName + ".js");
            if(string.IsNullOrWhiteSpace(fp)) return null;

            var exp = 0;
            var cur_id = -1L;
            using (var sw = new StreamWriter(fp, false, Encoding.UTF8))
            {
                foreach (var doc in Mongo.Coll<T>().FindAllAs<BsonDocument>())
                {
                    if (BsonType.Int64 == doc["_id"].BsonType)
                        cur_id = Math.Max(cur_id, doc["_id"].AsInt64);
                    var json = doc.ToJson();
                    sw.WriteLine(json);
                    Debug.Print(json);
                    ++exp;
                }
                sw.Close();
            }
            return new ImpExpInfo(exp, cur_id);
        }
        public static ImpExpInfo ImpColl<T>(bool setNextId = false)
        {
            var collName = typeof (T).Name;
            var fp = ChooseOpenFile("导出Coll-" + collName + ".js");
            if (string.IsNullOrWhiteSpace(fp)) return null;

            MongoCollection mc = null;

            var imp = 0;
            var cur_id = -1L;
            using (var sr = new StreamReader(fp, Encoding.UTF8))
            {
                try
                {
                    const int batchCount = 2;
                    var l = new List<BsonDocument>(batchCount);
                    for (;;)
                    {
                        var json = sr.ReadLine();
                        if (null == json)
                        {
                            if (0 != l.Count)
                            {
                                if (null == mc)
                                {
                                    mc = Mongo.Coll<T>();
                                    mc.Drop();
                                }
                                mc.InsertBatch(l);
                                imp += l.Count;
                                l.Clear();
                            }
                            break;
                        }

                        var doc = BsonSerializer.Deserialize<BsonDocument>(json);
                        l.Add(doc);
                        Debug.Print(json);

                        if (BsonType.Int64 == doc["_id"].BsonType)
                            cur_id = Math.Max(cur_id, doc["_id"].AsInt64);

                        if (l.Count != batchCount) continue;
                        if (null == mc)
                        {
                            mc = Mongo.Coll<T>();
                            mc.Drop();
                        }
                        mc.InsertBatch(l);
                        imp += l.Count;
                        l.Clear();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                    Debug.Print(ex.Message);
                    if (null != mc) mc.Drop();
                    return null;
                }
                sr.Close();
            }
            if (setNextId && -1 != cur_id) Mongo.NextIdSetTo<T>(cur_id);
            return new ImpExpInfo(imp, cur_id);
        }

        public static string 导出到XLSX<T>(Action<ISheet, IEnumerable<T>, int> xlsxProcess, int skip = 0, int limit = 0, IMongoQuery q = null)
        {
            if (null == xlsxProcess) return null;

            var docsCount = (int)Mongo.计数<T>(skip, limit, q);
            if (docsCount <= 0) return null;

            var fn = ChooseSaveFile();
            if(string.IsNullOrWhiteSpace(fn)) return null;
            if (!fn.EndsWith(".xlsx")) fn += ".xlsx";

            var wb = new XSSFWorkbook();

            for (var wsn = 1; 0 < docsCount; ++wsn)
            {
                var dataPageCount = Math.Min(SpreadsheetVersion.EXCEL2007.MaxRows-1, docsCount);
                var dataPage = Mongo.查询<T>(skip, dataPageCount, q);

                var ws = wb.CreateSheet("页" + wsn);
                xlsxProcess(ws, dataPage, dataPageCount);

                skip += dataPageCount;
                docsCount -= dataPageCount;
            }

            using(var fs = File.OpenWrite(fn)) wb.Write(fs);
            return fn;
        }
        private static List<string> GetEnumNames<T>() { return Enum.GetNames(typeof(T)).ToList(); }
        public static void 导出专家列表到XLSX(bool open = true)
        {
            var fn = 导出到XLSX<专家>((ws, zjl, count) =>
            {
                var headers = new Dictionary<string, List<string>>
                {
                    {"Id", null},
                    {"姓名", null},
                    {"性别", GetEnumNames<性别>()},
                    {"评审专家证号", null},
                    {"证件类型", GetEnumNames<证件类型>()},
                    {"证件号", null},
                    {"职称", null},//GetEnumNames<专业技术职称>()},
                    {"毕业院校", null},
                    {"学历", GetEnumNames<学历>()},
                    {"工作单位", null},
                    {"从事专业", null},
                    {"评标专业", null},
                    {"评标经历", new List<string>{"有", "无"}},
                };

                var rh = ws.CreateRow(0);
                var coln = 0;
                foreach (var kv in headers) rh.CreateCell(coln++).SetCellValue(kv.Key);

                var rn = 1;
                foreach (var zj in zjl)
                {
                    var r = ws.CreateRow(rn++);
                    coln = 0;
                    foreach (var field in new[]
                    {
                        zj.Id.ToString(),
                        zj.身份信息.姓名,
                        zj.身份信息.性别.ToString(),
                        zj.身份信息.专家证号,
                        zj.身份信息.证件类型.ToString(),
                        zj.身份信息.证件号,
                        zj.学历信息.专业技术职称.ToString(),
                        zj.学历信息.毕业院校,
                        zj.学历信息.最高学历.ToString(),
                        zj.工作经历信息.工作单位,
                        zj.工作经历信息.从事专业,
                        string.Join(",",zj.可参评物资类别列表.Select(o=>o.一级分类)),
                        zj.身份信息.评标经历 ? "有" : "无",
                    }) r.CreateCell(coln++).SetCellValue(field);
                }
                //加验证
                var vh = ws.GetDataValidationHelper();
                coln = 0;
                foreach (var kv in headers)
                {
                    if (null != kv.Value && 0 != kv.Value.Count)
                    {
                        var vl = kv.Value;
                        var vd = vh.CreateValidation(
                            vh.CreateExplicitListConstraint(vl.ToArray()),
                            new CellRangeAddressList(1, count, coln, coln)
                            );
                        vd.ShowErrorBox = false;
                        vd.ShowPromptBox = true;
                        ws.AddValidationData(vd);
                    }
                    ++coln;
                }
                coln = 0;
                foreach (var kv in headers)
                    ws.AutoSizeColumn(coln++);
            });
            if (open)
            {
                Process.Start(fn);
            }
        }
    }
}
