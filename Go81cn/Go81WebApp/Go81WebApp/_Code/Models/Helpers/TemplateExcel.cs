using System;
using System.Collections.Generic;
using Go81WebApp.Models.管理器;
using Go81WebApp.Models.数据模型.商品数据模型;
using MongoDB.Driver.Linq;
using NPOI.HSSF.UserModel;
using NPOI.SS;
using NPOI.SS.UserModel;
using NPOI.SS.Util;
using NPOI.XSSF.UserModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Go81WebApp.Models.数据模型.需求计划模型;
using Go81WebApp.Models.管理器.需求计划管理;
using System.Web;
using Go81WebApp.Models.数据模型.用户数据模型;
using MongoDB.Driver.Builders;
using Go81WebApp.Models.数据模型;
using MongoDB.Driver;
using MongoDB;
using Go81WebApp.Models.数据模型.推广业务数据模型;

namespace Go81WebApp.Models.Helpers
{
    public static class TemplateExcel
    {
        public static List<List<Tuple<商品, string>>> 读取商品(string path, long 供应商ID)
        {
            var wb = LoadFromFile(path);
            if (null == wb || wb.NumberOfSheets < 1) return null;//加载文件失败，或没有可用的工作表
            var ret = new List<List<Tuple<商品, string>>>();
            for (var i = 0; i < wb.NumberOfSheets; ++i)
            {
                var wsName = wb.GetSheetName(i);
                if (string.IsNullOrWhiteSpace(wsName)) continue;//第一个工作表名为空
                var 商品分类名及ID = wsName.Split(".".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                long catId;
                if (商品分类名及ID.Length != 2 || !long.TryParse(商品分类名及ID[1], out catId)) continue;//工作表名格式不正确，或商品分类ID无效
                var cat = 商品分类管理.查找分类(catId);
                if (商品分类名及ID[0] != cat.分类名) continue;//商品分类名和ID不匹配
                var ws = wb.GetSheetAt(i);
                var ret2 = new List<Tuple<商品, string>>();
                //验证并解析格式
                for (var ci = 1; ci <
                    (ws is HSSFSheet ? SpreadsheetVersion.EXCEL97.MaxColumns : SpreadsheetVersion.EXCEL2007.MaxColumns);
                    ++ci)
                {
                    var hitColumn = false;
                    var p = new 商品();
                    string imgDir = null;
                    string lv1 = null, lv2 = null, lv3 = null;
                    for (var ri = 0; ri < ws.PhysicalNumberOfRows; ++ri)
                    {
                        var r = ws.GetRow(ri);
                        //检查第一列的属性名
                        var c = r.GetCell(0);
                        if (null == c) continue;
                        switch (c.CellStyle.Indention)
                        {
                            case 0:
                                try { lv1 = c.StringCellValue; }
                                catch { lv1 = null; }
                                continue;
                            case 1:
                                try { lv2 = c.StringCellValue; }
                                catch { lv2 = null; }
                                continue;
                            case 2:
                                try { lv3 = c.StringCellValue; }
                                catch { lv3 = null; }
                                break;//只有叶端节点继续进行值解析
                            default:
                                continue;//跳过缩进不正确的行
                        }
                        //取值
                        c = r.GetCell(ci);
                        if (null == c) continue;//值为空
                        c.SetCellType(CellType.String);
                        var val = c.StringCellValue.Trim();
                        if (string.IsNullOrWhiteSpace(val)) continue;//值为空
                        hitColumn = true;
                        //放置值
                        switch (lv1)
                        {
                            case "商品信息":
                                switch (lv3)
                                {
                                    case "图片目录":
                                        imgDir = val; break;
                                    case "商品名":
                                        p.商品信息.商品名 = val; break;
                                    case "品牌":
                                        p.商品信息.品牌 = val; break;
                                    case "型号":
                                        p.商品信息.型号 = val; break;
                                    case "计量单位":
                                        p.商品信息.计量单位 = val; break;
                                }
                                break;
                            case "商品数据":
                                switch (lv3)
                                {
                                    case "商品简介":
                                        p.商品数据.商品简介 = val; break;
                                    case "商品详情":
                                        p.商品数据.商品详情 = val; break;
                                    case "售后服务":
                                        p.商品数据.售后服务 = val; break;
                                }
                                break;
                            case "销售信息":
                                switch (lv3)
                                {
                                    case "价格":
                                        decimal price;
                                        if (decimal.TryParse(val, out price))
                                            p.销售信息.价格 = price;
                                        break;
                                }
                                break;
                            case "商品属性":
                                if (string.IsNullOrWhiteSpace(lv2) || string.IsNullOrWhiteSpace(lv3)) continue;
                                var tpl = cat.商品属性模板;
                                if (tpl.ContainsKey(lv2) && tpl[lv2].ContainsKey(lv3))
                                {
                                    if (!p.商品数据.商品属性.ContainsKey(lv2)) p.商品数据.商品属性[lv2] = new Dictionary<string, List<string>>();
                                    if (!p.商品数据.商品属性[lv2].ContainsKey(lv3)) p.商品数据.商品属性[lv2][lv3] = new List<string>();
                                    if (属性类型.复选 == tpl[lv2][lv3].属性类型)
                                        p.商品数据.商品属性[lv2][lv3].AddRange(val.Split(",，;；".ToCharArray(), StringSplitOptions.RemoveEmptyEntries));
                                    else
                                        p.商品数据.商品属性[lv2][lv3].Add(val);
                                }
                                break;
                            default:
                                continue;
                        }
                    }
                    if (!hitColumn) break;
                    p.商品信息.所属供应商.用户ID = 供应商ID;
                    p.商品信息.所属商品分类.商品分类ID = catId;
                    ret2.Add(Tuple.Create(p, imgDir));
                }
                ret.Add(ret2);
            }
            return ret;
        }

        public static void 导出需求计划(long id, HttpResponseBase rs)
        {
            var excel = new excel(SpreadsheetVersion.EXCEL97);
            excel.需求计划(id, rs);
        }
        public static void 导出专家信息(HttpResponseBase rs)
        {
            var excel = new excel(SpreadsheetVersion.EXCEL97);
            excel.专家信息(rs);
        }

        public static void 供应商手机号导出(Dictionary<string, string> dics, HttpResponseBase rs)
        {
            var excel = new excel(SpreadsheetVersion.EXCEL97);
            excel.导出供应商手机号(dics,rs);
        }


        private class excel
        {
            private readonly SpreadsheetVersion _excelVer;
            private readonly string _excelExt;
            public excel(SpreadsheetVersion excelVer)
            {
                _excelVer = excelVer;
                _excelExt = excelVer == SpreadsheetVersion.EXCEL97 ? "xls" : "xlsx";
            }
            private void PutNames(ISheet ws, ICellStyle cs1, ICellStyle cs2, ICellStyle cs3, params string[] names)
            {
                PutNamesWithValidations(ws, cs1, cs2, cs3, names.Select(s => new[] { s }).ToArray());
            }
            private void PutNamesWithValidations(ISheet ws, ICellStyle cs1, ICellStyle cs2, ICellStyle cs3, params string[][] names)
            {
                var rn0 = ws.PhysicalNumberOfRows;
                var rn = rn0;
                for (var i = 0; i < names.Length; ++i)
                {
                    var rnc = rn++;
                    var r = ws.CreateRow(rnc);
                    if (1 < names[i].Length)
                    {
                        r.RowStyle = cs3;
                        var vh = ws.GetDataValidationHelper();
                        var vl = names[i].Skip(1).ToList();
                        vl.Sort();
                        var vd = vh.CreateValidation(
                            vh.CreateExplicitListConstraint(vl.ToArray()),
                            new CellRangeAddressList(rnc, rnc, 1, _excelVer.LastColumnIndex)
                            );
                        vd.ShowErrorBox = false;
                        vd.ShowPromptBox = true;
                        ws.AddValidationData(vd);
                    }

                    var c = r.CreateCell(0);
                    c.SetCellValue(names[i][0]);
                    if (0 == i)
                    {
                        c.CellStyle = cs1;
                        r.RowStyle = cs1;
                    }
                    else
                    {
                        c.CellStyle = cs2;
                    }
                }
                if (1 < names.Length) ws.GroupRow(rn0 + 1, rn - 1);
                //ws.SetRowGroupCollapsed(rn0 + 1, true);
                ws.AddMergedRegion(new CellRangeAddress(rn0, rn0, 0, _excelVer.LastColumnIndex));
            }
            internal byte[] 商品分类(params long[] idList)
            {
                var wb = CreateWorkbook(_excelVer);

                //默认字体
                var ft = wb.GetFontAt(0);
                if (ft is XSSFFont) (ft as XSSFFont).SetScheme(FontScheme.NONE);
                ft.FontName = "微软雅黑";
                ft.FontHeightInPoints = 11;
                //默认样式
                var cs = wb.GetDefaultCellStyle();
                cs.IsLocked = false;
                cs.SetFont(ft);
                //wb.SetDefaultCellStyle(cs);

                //标题字体
                var f1 = wb.CreateFont();
                if (f1 is XSSFFont) (f1 as XSSFFont).SetScheme(FontScheme.NONE);
                f1.FontName = "微软雅黑";
                f1.Boldweight = (short)FontBoldWeight.Bold;
                f1.FontHeightInPoints = 11;
                //标题样式:一级
                var cs1 = wb.CreateCellStyle();
                cs1.SetFont(f1);
                cs1.BorderTop = BorderStyle.Thick;
                cs1.FillPattern = FillPattern.SolidForeground;
                cs1.FillForegroundColor = IndexedColors.Grey50Percent.Index;
                cs1.Alignment = HorizontalAlignment.Left;
                cs1.VerticalAlignment = VerticalAlignment.Center;
                cs1.IsLocked = true;
                //标题样式:二级
                var cs2 = wb.CreateCellStyle();
                cs2.SetFont(f1);
                cs2.BorderTop = BorderStyle.Medium;
                cs2.FillPattern = FillPattern.SolidForeground;
                cs2.FillForegroundColor = IndexedColors.Grey40Percent.Index;
                cs2.Alignment = HorizontalAlignment.Left;
                cs2.VerticalAlignment = VerticalAlignment.Center;
                cs2.IsLocked = true;
                cs2.Indention = 1;
                //标题样式:三级
                var cs3 = wb.CreateCellStyle();
                cs3.SetFont(f1);
                cs3.FillPattern = FillPattern.SolidForeground;
                cs3.FillForegroundColor = IndexedColors.Grey25Percent.Index;
                cs3.Alignment = HorizontalAlignment.Left;
                cs3.VerticalAlignment = VerticalAlignment.Center;
                cs3.IsLocked = true;
                cs3.Indention = 2;
                //复选值样式:
                var cs4 = wb.CreateCellStyle();
                cs4.SetFont(ft);
                cs4.FillPattern = FillPattern.SolidForeground;
                cs4.FillForegroundColor = IndexedColors.LightYellow.Index;
                cs4.IsLocked = false;

                foreach (var id in idList)
                {
                    var cat = 商品分类管理.查找分类(id);
                    var ws = wb.CreateSheet(cat.分类名 + "." + id);
                    ws.RowSumsBelow = false;
                    ws.RowSumsRight = false;
                    ws.CreateFreezePane(1, 6);

                    PutNames(ws, cs1, cs3, cs4, "商品信息", "图片目录", "商品名", "品牌", "型号", "计量单位");
                    PutNames(ws, cs1, cs3, cs4, "商品数据", "商品简介", "商品详情", "售后服务");
                    PutNames(ws, cs1, cs3, cs4, "销售信息", "价格");
                    var propTpl = cat.商品属性模板;
                    if (propTpl.Values.Sum(d => d.Count) > 0)
                    {
                        PutNames(ws, cs1, cs3, cs4, "商品属性");
                        var rn0 = ws.PhysicalNumberOfRows;
                        foreach (var kv in propTpl)
                        {
                            var pn = kv.Key;
                            var names = kv.Value.Keys.Select(s =>
                            {
                                var prop = cat.商品属性模板[pn][s];
                                if (prop.属性类型 != 属性类型.复选)
                                    return new[] { s };

                                var l = cat.商品属性模板[pn][s].值.ToList();
                                l.Insert(0, s);
                                return l.ToArray();
                            }).ToList();
                            names.Insert(0, new[] { kv.Key });
                            PutNamesWithValidations(ws, cs2, cs3, cs4, names.ToArray());
                        }
                        ws.GroupRow(rn0, ws.PhysicalNumberOfRows - 1);
                    }

                    ws.DefaultColumnWidth = 20;
                    ws.AutoSizeColumn(0);

                    ws.ProtectSheet("go81cn123`asd");
                }
                using (var ms = new MemoryStream())
                {
                    wb.Write(ms);
                    return ms.ToArray();
                }
                //var fn = Path.ChangeExtension(Path.GetTempFileName(), _excelExt);
                //using (var ms = File.OpenWrite(fn)) wb.Write(ms);
                //Process.Start(fn);
            }
            internal void 需求计划(long id, HttpResponseBase rs)
            {
                var 需求计划表 = 需求计划管理.查找需求计划(id);
                var wb = CreateWorkbook(_excelVer);

                #region 样式
                // 设置字体
                var font_10 = wb.CreateFont();
                font_10.FontName = "微软雅黑";
                font_10.FontHeightInPoints = 10;

                var font_12 = wb.CreateFont();
                font_12.FontName = "微软雅黑";
                font_12.FontHeightInPoints = 12;

                var font_15 = wb.CreateFont();
                font_15.FontHeightInPoints = 15;
                font_15.FontName = "微软雅黑";

                //设置样式-统一垂直居中
                //样式一：12号字体、右对齐
                ICellStyle style_1 = wb.CreateCellStyle();
                style_1.VerticalAlignment = VerticalAlignment.Center;
                style_1.SetFont(font_12);
                style_1.Alignment = HorizontalAlignment.Right;


                //样式二：12号字体、居中对齐
                ICellStyle style_2 = wb.CreateCellStyle();
                style_2.VerticalAlignment = VerticalAlignment.Center;
                style_2.SetFont(font_12);
                style_2.Alignment = HorizontalAlignment.Center;

                //样式三：12号字体、左对齐
                ICellStyle style_3 = wb.CreateCellStyle();
                style_3.VerticalAlignment = VerticalAlignment.Center;
                style_3.SetFont(font_12);
                style_3.Alignment = HorizontalAlignment.Left;

                //样式四：15号字体、右对齐
                ICellStyle style_4 = wb.CreateCellStyle();
                style_4.VerticalAlignment = VerticalAlignment.Center;
                style_4.SetFont(font_15);
                style_4.Alignment = HorizontalAlignment.Right;

                //样式五：15号字体、居中对齐
                ICellStyle style_5 = wb.CreateCellStyle();
                style_5.VerticalAlignment = VerticalAlignment.Center;
                style_5.SetFont(font_15);
                style_5.Alignment = HorizontalAlignment.Center;

                //样式六：15号字体、左对齐
                ICellStyle style_6 = wb.CreateCellStyle();
                style_6.VerticalAlignment = VerticalAlignment.Center;
                style_6.SetFont(font_15);
                style_6.Alignment = HorizontalAlignment.Left;

                //样式七：12号字体、居中对齐、有边框

                ICellStyle style_7 = wb.CreateCellStyle();
                style_7.VerticalAlignment = VerticalAlignment.Center;
                style_7.Alignment = HorizontalAlignment.Center;
                style_7.BorderBottom = BorderStyle.Thin;
                style_7.BorderLeft = BorderStyle.Thin;
                style_7.BorderRight = BorderStyle.Thin;
                style_7.BorderTop = BorderStyle.Thin;
                style_7.WrapText = true;
                #endregion

                for (int i = 0; i < 4; i++)
                {
                    switch (i)
                    {
                        case 0:
                            #region 封面
                            var ws = wb.CreateSheet("封面");
                            //秘密等级设置
                            ws.AddMergedRegion(new CellRangeAddress(2, 2, 0, 11));
                            var row = ws.CreateRow(2);
                            var cell = row.CreateCell(0);
                            cell.SetCellValue("秘密等级：");
                            ICellStyle style0 = wb.CreateCellStyle();
                            style0.SetFont(font_12);
                            style0.Alignment = HorizontalAlignment.Right;
                            cell.CellStyle = style0;
                            ws.AddMergedRegion(new CellRangeAddress(2, 2, 12, 13));
                            cell = row.CreateCell(12);
                            cell.SetCellValue(需求计划表.秘密等级.ToString());
                            ICellStyle style1 = wb.CreateCellStyle();
                            style1.BorderBottom = BorderStyle.Medium;
                            style1.Alignment = HorizontalAlignment.Left;
                            style1.SetFont(font_12);
                            cell.CellStyle = style1;
                            cell = row.CreateCell(13);
                            cell.CellStyle = style1;
                            //需求计划表标题设置
                            ws.AddMergedRegion(new CellRangeAddress(8, 8, 0, 13));
                            row = ws.CreateRow(8);
                            cell = row.CreateCell(0);
                            cell.SetCellValue("军 队 物 资 集 中 采 购 需 求 计 划");
                            var ft = wb.CreateFont();
                            ft.FontName = "微软雅黑";
                            ft.FontHeightInPoints = 30;
                            ICellStyle style2 = wb.CreateCellStyle();
                            style2.Alignment = HorizontalAlignment.Center;
                            style2.SetFont(ft);
                            cell.CellStyle = style2;
                            //编制单位
                            ws.AddMergedRegion(new CellRangeAddress(20, 20, 0, 4));
                            row = ws.CreateRow(20);
                            cell = row.CreateCell(0);
                            cell.SetCellValue("编制单位：");
                            ICellStyle style3 = wb.CreateCellStyle();
                            style3.SetFont(font_15);
                            style3.Alignment = HorizontalAlignment.Right;
                            cell.CellStyle = style3;
                            ws.AddMergedRegion(new CellRangeAddress(20, 20, 5, 9));
                            ICellStyle style4 = wb.CreateCellStyle();
                            style4.BorderBottom = BorderStyle.Medium;
                            style4.SetFont(font_12);
                            for (int j = 5; j < 10; j++)
                            {
                                cell = row.CreateCell(j);
                                if (j == 5)
                                {
                                    cell.SetCellValue(需求计划表.编制单位);
                                }
                                cell.CellStyle = style4;
                            }
                            //年月日
                            ws.AddMergedRegion(new CellRangeAddress(25, 25, 0, 13));
                            row = ws.CreateRow(25);
                            cell = row.CreateCell(0);
                            cell.SetCellValue("年                月                日");
                            ICellStyle style5 = wb.CreateCellStyle();
                            style5.SetFont(font_15);
                            style5.Alignment = HorizontalAlignment.Center;
                            cell.CellStyle = style5;

                            //打印设置
                            ws.PrintSetup.Landscape = true;//设置横向打印
                            ws.PrintSetup.PaperSize = 9; //设置打印纸张
                            break;
                            #endregion
                        case 1:
                            #region 物资列表
                            ws = wb.CreateSheet("军队物资集中采购需求计划表");

                            var 物资列表 = 需求计划表.物资列表;
                            var pageCount = 物资列表.Count % 14 == 0 ? 物资列表.Count / 14 : 物资列表.Count / 14 + 1;

                            for (int page = 0; page < pageCount; page++)
                            {
                                //设置表头
                                ws.AddMergedRegion(new CellRangeAddress(page * 20 + 1, page * 20 + 1, 0, 10));
                                row = ws.CreateRow(page * 20 + 1);
                                cell = row.CreateCell(0);
                                cell.SetCellValue("军队物资集中采购需求计划表");
                                cell.CellStyle = style_5;
                                //编制单位、采购年度
                                ws.AddMergedRegion(new CellRangeAddress(page * 20 + 2, page * 20 + 2, 0, 8));
                                row = ws.CreateRow(page * 20 + 2);
                                row.HeightInPoints = 25;
                                cell = row.CreateCell(0);
                                cell.SetCellValue("编制单位：" + 需求计划表.编制单位);
                                cell.CellStyle = style_3;
                                ws.AddMergedRegion(new CellRangeAddress(page * 20 + 2, page * 20 + 2, 9, 10));
                                cell = row.CreateCell(9);
                                cell.SetCellValue("采购年度：" + 需求计划表.采购年度.ToString("yyyy年MM月dd日"));
                                cell.CellStyle = style_1;

                                //内容表头
                                row = ws.CreateRow(page * 20 + 3);
                                for (int k = 0; k < 11; k++)
                                {
                                    cell = row.CreateCell(k);
                                    switch (k)
                                    {
                                        case 0: cell.SetCellValue("序号"); ws.SetColumnWidth(k, 1000); break;
                                        case 1: cell.SetCellValue("物资名称"); ws.SetColumnWidth(k, 7000); break;
                                        case 2: cell.SetCellValue("规格型号"); ws.SetColumnWidth(k, 3000); break;
                                        case 3: cell.SetCellValue("计量单位"); ws.SetColumnWidth(k, 1200); break;
                                        case 4: cell.SetCellValue("数量"); break;
                                        case 5: cell.SetCellValue("单价（元）"); break;
                                        case 6: cell.SetCellValue("预算金额（元）"); ws.SetColumnWidth(k, 3000); break;
                                        case 7: cell.SetCellValue("质量技术标准"); ws.SetColumnWidth(k, 2400); break;
                                        case 8: cell.SetCellValue("交货期限"); ws.SetColumnWidth(k, 3300); break;
                                        case 9: cell.SetCellValue("采购方式建议"); ws.SetColumnWidth(k, 2400); break;
                                        case 10: cell.SetCellValue("备注"); ws.SetColumnWidth(k, 8000); break;
                                    }
                                    style_7.SetFont(font_12);
                                    cell.CellStyle = style_7;
                                }

                                //填充内容
                                var list = 物资列表.Skip(page * 14).Take(14).ToList();
                                var length = list.Count();
                                for (int p = 0; p < length; p++)   //行
                                {
                                    row = ws.CreateRow(page * 20 + 4 + p);
                                    row.HeightInPoints = 25;
                                    for (int t = 0; t < 11; t++) //列
                                    {
                                        cell = row.CreateCell(t);
                                        switch (t)
                                        {
                                            case 0: cell.SetCellValue(p + 1); break;
                                            case 1: cell.SetCellValue(list[p].需求计划物资数据.物资名称); break;
                                            case 2: cell.SetCellValue(list[p].需求计划物资数据.规格型号); break;
                                            case 3: cell.SetCellValue(list[p].需求计划物资数据.计量单位); break;
                                            case 4: cell.SetCellValue(list[p].需求计划物资数据.数量.ToString()); break;
                                            case 5: cell.SetCellValue(list[p].需求计划物资数据.单价.ToString()); break;
                                            case 6: cell.SetCellValue(list[p].需求计划物资数据.预算金额.ToString()); break;
                                            case 7: cell.SetCellValue(list[p].需求计划物资数据.技术指标); break;
                                            case 8: cell.SetCellValue(list[p].需求计划物资数据.交货期限.ToString("yyyy-MM-dd")); break;
                                            case 9: cell.SetCellValue(list[p].需求计划物资数据.建议采购方式.ToString()); break;
                                            case 10: cell.SetCellValue(list[p].需求计划物资数据.所属需求计划.需求计划数据.需求发起单位链接.用户数据.单位信息.单位代号 != null ? list[p].需求计划物资数据.所属需求计划.需求计划数据.需求发起单位链接.用户数据.单位信息.单位代号 : list[p].需求计划物资数据.所属需求计划.需求计划数据.需求发起单位链接.用户数据.单位信息.单位名称); break;
                                        }
                                        style_7.SetFont(font_10);
                                        cell.CellStyle = style_7;
                                    }
                                }

                                //数据不够时用空白填充
                                for (int m = 0; m < 14 - length; m++)
                                {
                                    row = ws.CreateRow(page * 20 + 4 + length + m);
                                    row.HeightInPoints = 25;
                                    for (int r = 0; r < 11; r++) //列
                                    {
                                        cell = row.CreateCell(r);
                                        switch (r)
                                        {
                                            case 0: cell.SetCellValue(""); break;
                                            case 1: cell.SetCellValue(""); break;
                                            case 2: cell.SetCellValue(""); break;
                                            case 3: cell.SetCellValue(""); break;
                                            case 4: cell.SetCellValue(""); break;
                                            case 5: cell.SetCellValue(""); break;
                                            case 6: cell.SetCellValue(""); break;
                                            case 7: cell.SetCellValue(""); break;
                                            case 8: cell.SetCellValue(""); break;
                                            case 9: cell.SetCellValue(""); break;
                                            case 10: cell.SetCellValue(""); break;
                                        }
                                        cell.CellStyle = style_7;
                                    }
                                }

                                //表格底部
                                row = ws.CreateRow(page * 20 + 18);
                                row.HeightInPoints = 25;
                                ws.AddMergedRegion(new CellRangeAddress(page * 20 + 18, page * 20 + 18, 0, 5));
                                cell = row.CreateCell(0);
                                cell.SetCellValue("承办部门：" + 需求计划表.承办部门);
                                cell.CellStyle = style_3;
                                ws.AddMergedRegion(new CellRangeAddress(page * 20 + 18, page * 20 + 18, 6, 8));
                                cell = row.CreateCell(6);
                                cell.SetCellValue("联系人：" + 需求计划表.联系人);
                                cell.CellStyle = style_3;
                                ws.AddMergedRegion(new CellRangeAddress(page * 20 + 18, page * 20 + 18, 9, 10));
                                cell = row.CreateCell(9);
                                cell.SetCellValue("联系电话：" + 需求计划表.联系电话);
                                cell.CellStyle = style_3;

                                //打印设置
                                ws.PrintSetup.Landscape = true;//设置横向打印
                                ws.PrintSetup.PaperSize = 9; //设置打印纸张
                                ws.PrintSetup.Scale = 94;  //设置缩放
                                ws.FitToPage = false;
                                ws.SetRowBreak((page + 1) * 20);  //设置分页

                            }
                            break;
                            #endregion
                        case 2:
                            #region 分发列表
                            ws = wb.CreateSheet("军队物资集中采购需求计划分发表");

                            var 分发列表 = 需求计划表.分发列表;
                            pageCount = 分发列表.Count % 14 == 0 ? 分发列表.Count / 14 : 分发列表.Count / 14 + 1;

                            for (int page = 0; page < pageCount; page++)
                            {
                                //设置表头
                                ws.AddMergedRegion(new CellRangeAddress(page * 20 + 1, page * 20 + 1, 0, 9));
                                row = ws.CreateRow(page * 20 + 1);
                                cell = row.CreateCell(0);
                                cell.SetCellValue("军队物资集中采购需求计划表");
                                cell.CellStyle = style_5;
                                //编制单位、采购年度
                                ws.AddMergedRegion(new CellRangeAddress(page * 20 + 2, page * 20 + 2, 0, 9));
                                row = ws.CreateRow(page * 20 + 2);
                                row.HeightInPoints = 25;
                                cell = row.CreateCell(0);
                                cell.SetCellValue("编制单位：" + 需求计划表.编制单位);
                                cell.CellStyle = style_3;

                                //内容表头
                                row = ws.CreateRow(page * 20 + 3);
                                for (int k = 0; k < 10; k++)
                                {
                                    cell = row.CreateCell(k);
                                    switch (k)
                                    {
                                        case 0: cell.SetCellValue("序号"); ws.SetColumnWidth(k, 1000); break;
                                        case 1: cell.SetCellValue("物资名称"); ws.SetColumnWidth(k, 7000); break;
                                        case 2: cell.SetCellValue("规格型号"); ws.SetColumnWidth(k, 4000); break;
                                        case 3: cell.SetCellValue("计量单位"); ws.SetColumnWidth(k, 1200); break;
                                        case 4: cell.SetCellValue("收货单位名称"); ws.SetColumnWidth(k, 5000); break;
                                        case 5: cell.SetCellValue("分配数量"); ws.SetColumnWidth(k, 3000); break;
                                        case 6: cell.SetCellValue("提货方式"); ws.SetColumnWidth(k, 3000); break;
                                        case 7: cell.SetCellValue("运输方式"); ws.SetColumnWidth(k, 3000); break;
                                        case 8: cell.SetCellValue("到站"); ws.SetColumnWidth(k, 2000); break;
                                        case 9: cell.SetCellValue("备注"); ws.SetColumnWidth(k, 6500); break;
                                    }
                                    style_7.SetFont(font_12);
                                    cell.CellStyle = style_7;
                                }

                                //填充内容
                                var fflist = 分发列表.Skip(page * 14).Take(14).ToList();
                                var length = fflist.Count();
                                for (int p = 0; p < length; p++)   //行
                                {
                                    row = ws.CreateRow(page * 20 + 4 + p);
                                    row.HeightInPoints = 25;
                                    for (int t = 0; t < 10; t++) //列
                                    {
                                        cell = row.CreateCell(t);
                                        switch (t)
                                        {
                                            case 0: cell.SetCellValue(p + 1); break;
                                            case 1: cell.SetCellValue(fflist[p].需求计划分发数据.物资名称); break;
                                            case 2: cell.SetCellValue(fflist[p].需求计划分发数据.规格型号); break;
                                            case 3: cell.SetCellValue(fflist[p].需求计划分发数据.计量单位); break;
                                            case 4: cell.SetCellValue(fflist[p].需求计划分发数据.收货单位名称); break;
                                            case 5: cell.SetCellValue(fflist[p].需求计划分发数据.分配数量.ToString()); break;
                                            case 6: cell.SetCellValue(fflist[p].需求计划分发数据.提货方式.ToString()); break;
                                            case 7: cell.SetCellValue(fflist[p].需求计划分发数据.运输方式.ToString()); break;
                                            case 8: cell.SetCellValue(fflist[p].需求计划分发数据.到站); break;
                                            case 9: cell.SetCellValue(fflist[p].需求计划分发数据.备注); break;
                                        }
                                        style_7.SetFont(font_10);
                                        cell.CellStyle = style_7;
                                    }
                                }

                                //数据不够时用空白填充
                                for (int m = 0; m < 14 - length; m++)
                                {
                                    row = ws.CreateRow(page * 20 + 4 + length + m);
                                    row.HeightInPoints = 25;
                                    for (int r = 0; r < 10; r++) //列
                                    {
                                        cell = row.CreateCell(r);
                                        switch (r)
                                        {
                                            case 0: cell.SetCellValue(""); break;
                                            case 1: cell.SetCellValue(""); break;
                                            case 2: cell.SetCellValue(""); break;
                                            case 3: cell.SetCellValue(""); break;
                                            case 4: cell.SetCellValue(""); break;
                                            case 5: cell.SetCellValue(""); break;
                                            case 6: cell.SetCellValue(""); break;
                                            case 7: cell.SetCellValue(""); break;
                                            case 8: cell.SetCellValue(""); break;
                                            case 9: cell.SetCellValue(""); break;
                                            case 10: cell.SetCellValue(""); break;
                                        }
                                        cell.CellStyle = style_7;
                                    }
                                }

                                //表格底部
                                row = ws.CreateRow(page * 20 + 18);
                                row.HeightInPoints = 25;
                                ws.AddMergedRegion(new CellRangeAddress(page * 20 + 18, page * 20 + 18, 0, 3));
                                cell = row.CreateCell(0);
                                cell.SetCellValue("承办部门：" + 需求计划表.承办部门);
                                cell.CellStyle = style_3;
                                ws.AddMergedRegion(new CellRangeAddress(page * 20 + 18, page * 20 + 18, 4, 7));
                                cell = row.CreateCell(4);
                                cell.SetCellValue("联系人：" + 需求计划表.联系人);
                                cell.CellStyle = style_3;
                                ws.AddMergedRegion(new CellRangeAddress(page * 20 + 18, page * 20 + 18, 8, 9));
                                cell = row.CreateCell(8);
                                cell.SetCellValue("联系电话：" + 需求计划表.联系电话);
                                cell.CellStyle = style_3;

                                //打印设置
                                ws.PrintSetup.Landscape = true;//设置横向打印
                                ws.PrintSetup.PaperSize = 9; //设置打印纸张
                                ws.PrintSetup.Scale = 94;  //设置缩放
                                ws.FitToPage = false;
                                ws.SetRowBreak((page + 1) * 20);  //设置分页

                            }
                            break;
                            #endregion
                        case 3:
                            #region 编制说明
                            ws = wb.CreateSheet("《军队物资集中采购需求计划》填制说明");
                            ws.AddMergedRegion(new CellRangeAddress(0, 0, 0, 14));
                            ws.AddMergedRegion(new CellRangeAddress(1, 2, 0, 14));
                            ws.AddMergedRegion(new CellRangeAddress(3, 3, 0, 14));
                            ws.AddMergedRegion(new CellRangeAddress(4, 4, 0, 14));
                            ws.AddMergedRegion(new CellRangeAddress(5, 5, 0, 14));
                            ws.AddMergedRegion(new CellRangeAddress(6, 6, 0, 14));
                            ws.AddMergedRegion(new CellRangeAddress(7, 7, 0, 14));
                            ws.AddMergedRegion(new CellRangeAddress(8, 8, 0, 14));
                            ws.AddMergedRegion(new CellRangeAddress(9, 9, 0, 14));
                            row = ws.CreateRow(0);
                            row.HeightInPoints = 20;
                            cell = row.CreateCell(0);
                            cell.SetCellValue("《军队物资集中采购需求计划》填制说明");
                            cell.CellStyle = style_5;
                            row = ws.CreateRow(1);
                            row.HeightInPoints = 30;
                            cell = row.CreateCell(0);
                            cell.SetCellValue("一、《军队物资集中采购需求计划》包括《军队物资集中采购需求计划表》和《军队物资集中采购需求计划分发表》，由事业部门依据年度采购预算编制，预算批复后20个工作日内送军需物资油料部门。");
                            style_3.WrapText = true;
                            cell.CellStyle = style_3;
                            row = ws.CreateRow(3);
                            row.HeightInPoints = 20;
                            cell = row.CreateCell(0);
                            cell.SetCellValue("二、\"物资名称\"栏按照采购目录区分的物资类别填至具体品种名称。");
                            cell.CellStyle = style_3;
                            row = ws.CreateRow(4);
                            row.HeightInPoints = 20;
                            cell = row.CreateCell(0);
                            cell.SetCellValue("三、\"计量单位\"按照采购目录明确的计量单位填写。");
                            cell.CellStyle = style_3;
                            row = ws.CreateRow(5);
                            row.HeightInPoints = 20;
                            cell = row.CreateCell(0);
                            cell.SetCellValue("四、对采购机构、供应商的建议可在《军队物资集中采购需求计划表》\"备注\"栏内填写。");
                            cell.CellStyle = style_3;
                            row = ws.CreateRow(6);
                            row.HeightInPoints = 20;
                            cell = row.CreateCell(0);
                            cell.SetCellValue("五、非标准产品或者有特殊要求的产品应当另附详细说。");
                            cell.CellStyle = style_3;
                            row = ws.CreateRow(7);
                            row.HeightInPoints = 20;
                            cell = row.CreateCell(0);
                            cell.SetCellValue("六、\"提货方式\"栏填写：自提、代运。");
                            cell.CellStyle = style_3;
                            row = ws.CreateRow(8);
                            row.HeightInPoints = 20;
                            cell = row.CreateCell(0);
                            cell.SetCellValue("七、\"运输方式\"栏填写：铁路运输、公路运输、水路运输、航空运输。");
                            cell.CellStyle = style_3;
                            row = ws.CreateRow(9);
                            row.HeightInPoints = 20;
                            cell = row.CreateCell(0);
                            cell.SetCellValue("八、本表使用A4纸（210X297mm）。");
                            cell.CellStyle = style_3;

                            //打印设置
                            ws.PrintSetup.Landscape = true;//设置横向打印
                            ws.PrintSetup.PaperSize = 9; //设置打印纸张
                            break;
                            #endregion

                    }
                }


                rs.ContentType = "application/vnd.ms-excel";
                rs.AddHeader("Content-Disposition", string.Format("attachment;filename={0}", "军队物资集中采购需求计划表" + DateTime.Now.Second.ToString()));
                wb.Write(rs.OutputStream);
                //FileStream file = new FileStream(@"e:\军队物资集中采购需求计划表.xls", FileMode.Create);
                //wb.Write(file);
                //file.Close();
            }
            internal void 专家信息(HttpResponseBase rs)
            {
                var wb = CreateWorkbook(_excelVer);
                var ws = wb.CreateSheet("专家信息表");
                //设置样式
                var font = wb.CreateFont();
                font.FontName = "微软雅黑";
                font.FontHeightInPoints = 12;

                var font_1 = wb.CreateFont();
                font.FontName = "宋体";
                font.FontHeightInPoints = 12;

                ICellStyle style = wb.CreateCellStyle();
                style.VerticalAlignment = VerticalAlignment.Center;
                style.SetFont(font);
                style.Alignment = HorizontalAlignment.Center;
                style.BorderBottom = BorderStyle.Thin;
                style.BorderLeft = BorderStyle.Thin;
                style.BorderRight = BorderStyle.Thin;
                style.BorderTop = BorderStyle.Thin;

                ws.AddMergedRegion(new CellRangeAddress(0, 1, 0, 11));
                var rowp = ws.CreateRow(0);
                var cellp = rowp.CreateCell(0);
                cellp.SetCellValue("专家信息表");
                var ft = wb.CreateFont();
                ft.FontName = "微软雅黑";
                ft.FontHeightInPoints = 15;

                ICellStyle style0 = wb.CreateCellStyle();
                style0.SetFont(ft);
                style0.Alignment = HorizontalAlignment.Center;
                cellp.CellStyle = style0;
                //内容表头
                var row = ws.CreateRow(2);
                row.HeightInPoints = 25;
                for (int k = 0; k < 12; k++)
                {
                    var cell = row.CreateCell(k);
                    switch (k)
                    {
                        case 0: cell.SetCellValue("姓名"); ws.SetColumnWidth(k, 3000); break;
                        case 1: cell.SetCellValue("性别"); ws.SetColumnWidth(k, 1800); break;
                        case 2: cell.SetCellValue("专家证号"); ws.SetColumnWidth(k, 7000); break;
                        case 3: cell.SetCellValue("证件类型"); ws.SetColumnWidth(k, 3000); break;
                        case 4: cell.SetCellValue("证件号"); ws.SetColumnWidth(k, 5500); break;
                        case 5: cell.SetCellValue("专业技术职称"); ws.SetColumnWidth(k, 4000); break;
                        case 6: cell.SetCellValue("毕业院校"); ws.SetColumnWidth(k, 7500); break;
                        case 7: cell.SetCellValue("最高学历"); ws.SetColumnWidth(k, 3000); break;
                        case 8: cell.SetCellValue("工作单位"); ws.SetColumnWidth(k, 7000); break;
                        case 9: cell.SetCellValue("从事专业"); ws.SetColumnWidth(k, 4500); break;
                        case 10: cell.SetCellValue("联系电话"); ws.SetColumnWidth(k, 5000); break;
                        case 11: cell.SetCellValue("备注"); ws.SetColumnWidth(k, 5000); break;
                    }
                    cell.CellStyle = style;
                }

                //填充内容
                var _zjlist = 用户管理.查询用户<专家>(0, 0).ToList();
                var length = _zjlist.Count();

                for (int p = 0; p < length; p++)   //行
                {
                    row = ws.CreateRow(3 + p);
                    row.HeightInPoints = 20;
                    for (int t = 0; t < 12; t++) //列
                    {
                        var cell = row.CreateCell(t);
                        switch (t)
                        {
                            //case 0: cell.SetCellValue(p + 1); break;
                            case 0: cell.SetCellValue(_zjlist[p].身份信息.姓名); break;
                            case 1: cell.SetCellValue(_zjlist[p].身份信息.性别.ToString()); break;
                            case 2: cell.SetCellValue(_zjlist[p].身份信息.专家证号); break;
                            case 3: cell.SetCellValue(_zjlist[p].身份信息.证件类型.ToString()); break;
                            case 4: cell.SetCellValue(_zjlist[p].身份信息.证件号); break;
                            case 5: cell.SetCellValue(_zjlist[p].学历信息.专业技术职称.ToString()); break;
                            case 6: cell.SetCellValue(_zjlist[p].学历信息.毕业院校); break;
                            case 7: cell.SetCellValue(_zjlist[p].学历信息.最高学历.ToString()); break;
                            case 8: cell.SetCellValue(_zjlist[p].工作经历信息.工作单位); break;
                            case 9: cell.SetCellValue(_zjlist[p].工作经历信息.从事专业); break;
                            case 10: cell.SetCellValue(_zjlist[p].联系方式.手机); break;
                        }
                        style.SetFont(font_1);
                        cell.CellStyle = style;
                    }
                }
                ws.PrintSetup.Landscape = true;//设置横向打印
                ws.PrintSetup.PaperSize = 9; //设置打印纸张
                ws.PrintSetup.Scale = 94;  //设置缩放
                ws.FitToPage = false;
                //ws.SetRowBreak((page + 1) * 20);  //设置分页

                rs.ContentType = "application/vnd.ms-excel";
                rs.AddHeader("Content-Disposition", string.Format("attachment;filename={0}", "专家信息表" + DateTime.Now.Second.ToString()));
                wb.Write(rs.OutputStream);
            }

            internal void 导出供应商手机号(Dictionary<string,string> dics,HttpResponseBase rs)
            {
                var wb = CreateWorkbook(_excelVer);

                //设置分类样式
                var font = wb.CreateFont();
                font.FontName = "宋体";
                font.FontHeightInPoints = 16;
                font.Color = (short)FontColor.Red;
                ICellStyle style = wb.CreateCellStyle();
                style.VerticalAlignment = VerticalAlignment.Center;
                style.SetFont(font);
                style.Alignment = HorizontalAlignment.Center;

                IMongoQuery query = null;
                if (dics["审核状态"] != "-1")
                    query = query.And(Query<供应商>.Where(o => (int)o.审核数据.审核状态 == int.Parse(dics["审核状态"])));
                if (dics["认证级别"] != "0")
                    query = query.And(Query<供应商>.Where(o => (int)o.供应商用户信息.认证级别 == int.Parse(dics["认证级别"])));
                if (dics["所属行业"] != "请选择")
                    query = query.And(Query<供应商>.Where(o => o.企业基本信息.所属行业.Contains(dics["所属行业"])));
                if (dics["协议应急普通"] != "请选择")
                {
                    if (dics["协议应急普通"] == "普通供应商")
                        query = query.And(Query<供应商>.Where(o => o.供应商用户信息.普通供应商));
                    if (dics["协议应急普通"] == "协议供应商")
                        query = query.And(Query<供应商>.Where(o => o.供应商用户信息.协议供应商));
                    if (dics["协议应急普通"] == "应急供应商")
                        query = query.And(Query<供应商>.Where(o => o.供应商用户信息.应急供应商));
                }
                if (dics["省"] != "")
                {
                    query = query.And(Query<供应商>.Where(o => o.所属地域.省份 == dics["省"]));
                    if (dics["市"] != "")
                    {
                        query = query.And(Query<供应商>.Where(o => o.所属地域.城市 == dics["市"]));
                        if (dics["县"] != "")
                        {
                            query = query.And(Query<供应商>.Where(o => o.所属地域.区县 == dics["县"]));
                        }
                    }
                }
                //if (dics["短信服务"] != "0")
                //{
                //    var 供应商服务记录 = 供应商服务记录管理.查询供应商服务记录(0, 0, null, false, SortBy<供应商服务记录>.Ascending(o => o.基本数据.修改时间));
                //    var A2广告位置服务记录 = 供应商服务记录.Where(o => o.已开通的服务.Exists(p => p.所申请项目名 == "企业推广服务A2位置" && p.签订时间 < now && p.结束时间 > now));
                //}

                var ws = wb.CreateSheet("Sheet1");

                //数据准备
                var _gysCate = 用户管理.查询用户<供应商>(0, 0, query).ToList();
               
                //写入数据
                var rowsCount = 0;
                foreach (var its in _gysCate)
                {
                    rowsCount++;
                    var row = ws.CreateRow(rowsCount);
                    row.HeightInPoints = 20;
                    var cell = row.CreateCell(0);
                    cell.CellStyle = style;
                    for (int t = 0; t < 7; t++) //列
                    {
                        cell = row.CreateCell(t);
                        switch (t)
                        {
                            case 0: cell.SetCellValue(its.企业联系人信息.联系人手机); ws.SetColumnWidth(t, 5000); break;
                            case 1: cell.SetCellValue(its.企业基本信息.企业名称); ws.SetColumnWidth(t, 12000); break;
                            case 2: cell.SetCellValue(its.所属地域.地域); ws.SetColumnWidth(t, 5000); break;
                            case 3: cell.SetCellValue(its.审核数据.审核状态.ToString()); ws.SetColumnWidth(t, 3000); break;
                            case 4: cell.SetCellValue(its.供应商用户信息.认证级别.ToString()); ws.SetColumnWidth(t, 5000); break;
                            case 5: var ert = "";
                                if (its.供应商用户信息.协议供应商)
                                    ert = "协议供应商";
                                else if (its.供应商用户信息.应急供应商)
                                    ert = "应急供应商";
                                else if (its.供应商用户信息.普通供应商)
                                    ert = "普通供应商";
                                else
                                    ert = "未设置";
                                cell.SetCellValue(ert);
                                ws.SetColumnWidth(t, 4000);
                                break;
                            case 6: cell.SetCellValue(its.企业基本信息.所属行业); ws.SetColumnWidth(t, 5000); break;
                        }
                    }
                }

                rs.ContentType = "application/vnd.ms-excel";
                rs.AddHeader("Content-Disposition", string.Format("attachment;filename={0}", "供应商手机号信息表" + DateTime.Now.Second.ToString()));
                wb.Write(rs.OutputStream);
            }

            internal void Test()
            {
                var wb = CreateWorkbook(_excelVer);
                var s0 = wb.CreateCellStyle();
                s0.IsLocked = true;
                var s1 = wb.CreateCellStyle();
                s1.IsLocked = false;

                var ws = wb.CreateSheet();
                var r0 = ws.CreateRow(0);
                var c0 = r0.CreateCell(0);
                c0.SetCellValue("已锁定");
                c0.CellStyle = s0;
                var c1 = r0.CreateCell(1);
                c1.SetCellValue("未锁定");
                c1.CellStyle = s1;

                ws.ProtectSheet("123");

                var fn = Path.ChangeExtension(Path.GetTempFileName(), _excelExt);
                using (var ms = File.OpenWrite(fn)) wb.Write(ms);
                Process.Start(fn);
            }
        }
        public static class xls
        {
            private static readonly excel excel = new excel(SpreadsheetVersion.EXCEL97);
            public static byte[] 商品分类(params long[] idList) { return excel.商品分类(idList); }
            public static void Test() { excel.Test(); }
        }
        public static class xlsx
        {
            private static readonly excel excel = new excel(SpreadsheetVersion.EXCEL2007);
            public static byte[] 商品分类(params long[] idList) { return excel.商品分类(idList); }
            public static void Test() { excel.Test(); }
        }
        private static IWorkbook CreateWorkbook(SpreadsheetVersion excelVer)
        {
            return excelVer == SpreadsheetVersion.EXCEL97
                ? new HSSFWorkbook() as IWorkbook
                : new XSSFWorkbook()
                ;
        }
        private static IWorkbook LoadFromFile(string path)
        {
            using (var fs = File.OpenRead(path))
                return path.EndsWith(".xls") ? new HSSFWorkbook(fs)
                    : path.EndsWith(".xlsx") ? new XSSFWorkbook(fs)
                    : (IWorkbook)null
                    ;
        }
        private static ICellStyle GetDefaultCellStyle(this IWorkbook wb)
        {
            return wb.GetCellStyleAt(wb is HSSFWorkbook ? (short)15 : (short)0);
        }
        private static void SetDefaultCellStyle(this IWorkbook wb, ICellStyle cs)
        {
            wb.GetDefaultCellStyle().CloneStyleFrom(cs);
        }
    }
}