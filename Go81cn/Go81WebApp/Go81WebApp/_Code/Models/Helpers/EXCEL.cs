using Go81WebApp.Models.管理器;
using Go81WebApp.Models.管理器.需求计划管理;
using Go81WebApp.Models.数据模型.需求计划模型;
using Go81WebApp.Models.数据模型.用户数据模型;
using NPOI.HSSF.UserModel;
using NPOI.HSSF.Util;
using NPOI.SS;
using NPOI.SS.UserModel;
using NPOI.SS.Util;
using NPOI.XSSF.UserModel;
using org.in2bits.MyXls;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
namespace Helpers
{
    public static class ExcelHelper
    {
        public static void OutBidGysExcel(string strid, HttpResponseBase rs)
        {
            string idstr = strid;
            //生成Excel开始
            XlsDocument xls = new XlsDocument();

            xls.FileName = "报价供应商信息表" + DateTime.Now.Second.ToString();

            Worksheet sheet = xls.Workbook.Worksheets.Add("报价供应商");

            //设置文档列属性 
            ColumnInfo cinfo = new ColumnInfo(xls, sheet);
            cinfo.Collapsed = true;
            //设置列的范围 如 0列-10列
            cinfo.ColumnIndexStart = 0;//列开始
            cinfo.ColumnIndexEnd = 7;//列结束
            //cinfo.Collapsed = true;
            cinfo.Width = 100 * 60;//列宽度
            sheet.AddColumnInfo(cinfo);
            //设置文档列属性结束

            //创建列样式创建标题列时引用
            XF cellXF = xls.NewXF();
            cellXF.VerticalAlignment = VerticalAlignments.Centered;
            cellXF.HorizontalAlignment = HorizontalAlignments.Centered;
            cellXF.ShrinkToCell = true;
            cellXF.TextWrapRight = true;
            cellXF.UseBorder = true;
            cellXF.Font.Height = 24 * 12;
            cellXF.Font.Bold = true;
            cellXF.Pattern = 0;//设定单元格填充风格。如果设定为0，则是纯色填充
            cellXF.PatternColor = Colors.Red;//设定填充线条的颜色
            //创建列样式创建内容列时引用
            XF cellXF1 = xls.NewXF();
            cellXF1.VerticalAlignment = VerticalAlignments.Centered;
            cellXF1.HorizontalAlignment = HorizontalAlignments.Left;
            cellXF1.ShrinkToCell = true;
            cellXF1.TextWrapRight = true;
            cellXF1.UseBorder = true;
            cellXF1.Pattern = 0;//设定单元格填充风格。如果设定为0，则是纯色填充
            cellXF1.PatternBackgroundColor = Colors.Red;//填充的背景底色
            cellXF1.PatternColor = Colors.Red;//设定填充线条的颜色
            //创建列样式结束
            Cells cells = sheet.Cells; //获得指定工作页列集合
            for (int i = 1; i <= 4; i++)
            {            //列操作基本
                Cell cell = null;
                switch (i)
                {
                    case 1: cell = cells.Add(1, i, "序号", cellXF); break;//添加标题列返回一个列  参数：行 列 名称 样式对象
                    case 2: cell = cells.Add(1, i, "供应商名称", cellXF); break;
                    case 3: cell = cells.Add(1, i, "报价（包括费用）", cellXF); break;
                    case 4: cell = cells.Add(1, i, "备注", cellXF); break;
                }
                cell.HorizontalAlignment = HorizontalAlignments.Centered;
                cell.VerticalAlignment = VerticalAlignments.Centered;
                cell.Font.FontFamily = FontFamilies.Modern;//设置字体 默认为宋体               
                //创建列结束  
            }

            var bid = 网上竞标管理.查找网上竞标(long.Parse(strid));
            var bjgys = bid.报价供应商列表;
            for (int m = 0; m < bjgys.Count(); m++)
            {
                for (int n = 1; n <= 4; n++)
                {
                    Cell cell = null;
                    switch (n)
                    {
                        case 1: cell = cells.Add(m + 2, n, m + 1, cellXF1); break;
                        case 2: cell = cells.Add(m + 2, n, bjgys[m].报价供应商.用户数据.企业基本信息.企业名称, cellXF1); break;
                        case 3: cell = cells.Add(m + 2, n, bjgys[m].总价, cellXF1); break;
                        case 4: cell = cells.Add(m + 2, n, bjgys[m].备注, cellXF1); break;
                    }
                    //设置XY居中
                    cell.HorizontalAlignment = HorizontalAlignments.Centered;
                    cell.VerticalAlignment = VerticalAlignments.Centered;
                    //设置字体
                    cell.Font.Bold = false;//设置粗体
                    cell.Font.ColorIndex = 0;//设置颜色码           
                    cell.Font.FontFamily = FontFamilies.Default;//设置字体 默认为宋体               
                    //创建列结束  
                }
            }
            for (int i = 1; i <= 4; i++)
            {
                Cell cell = null;
                switch (i)
                {
                    case 1: cell = cells.Add(bjgys.Count + 2, i, "", cellXF1); break;
                    case 2: cell = cells.Add(bjgys.Count + 2, i, "", cellXF1); break;
                    case 3: cell = cells.Add(bjgys.Count + 2, i, "参与人员签字：", cellXF1); break;
                    case 4: cell = cells.Add(bjgys.Count + 2, i, "", cellXF1); break;
                }
                //设置XY居中
                cell.HorizontalAlignment = HorizontalAlignments.Centered;
                cell.VerticalAlignment = VerticalAlignments.Centered;
                //设置字体
                cell.Font.Bold = false;//设置粗体
                cell.Font.ColorIndex = 0;//设置颜色码           
                cell.Font.FontFamily = FontFamilies.Default;//设置字体 默认为宋体               
                //创建列结束  
            }


            rs.ContentType = "application/vnd.ms-excel";
            rs.AddHeader("Content-Disposition", string.Format("attachment;filename={0}", xls.FileName));
            xls.Save(rs.OutputStream);
        }
        public static void PutExcelAll(HttpResponseBase rs)//导出供单位用户所有信息
        {
            //生成Excel开始
            XlsDocument xls = new XlsDocument();
            
                xls.FileName = "单位用户列表" + DateTime.Now.Second.ToString();

                Worksheet sheet = xls.Workbook.Worksheets.Add("单位用户");

                //设置文档列属性 
                ColumnInfo cinfo = new ColumnInfo(xls, sheet);
                cinfo.Collapsed = true;
                //设置列的范围 如 0列-10列
                cinfo.ColumnIndexStart = 0;//列开始
                cinfo.ColumnIndexEnd = 9;//列结束
                //cinfo.Collapsed = true;
                cinfo.Width = 100 * 60;//列宽度
                sheet.AddColumnInfo(cinfo);
                //设置文档列属性结束

                //创建列样式创建标题列时引用
                XF cellXF = xls.NewXF();
                cellXF.VerticalAlignment = VerticalAlignments.Centered;
                cellXF.HorizontalAlignment = HorizontalAlignments.Centered;
                cellXF.ShrinkToCell = true;
                cellXF.TextWrapRight = true;
                cellXF.UseBorder = true;
                cellXF.TopLineStyle = 1;
                cellXF.TopLineColor = Colors.Black;
                cellXF.BottomLineStyle = 1;
                cellXF.BottomLineColor = Colors.Black;
                cellXF.TopLineStyle = 1;
                cellXF.TopLineColor = Colors.Black;
                cellXF.LeftLineStyle = 1;
                cellXF.LeftLineColor = Colors.Black;
                cellXF.RightLineStyle = 1;
                cellXF.RightLineColor = Colors.Black;
                cellXF.Font.Height = 24 * 16;
                cellXF.Font.Bold = true;
                cellXF.Pattern = 0;//设定单元格填充风格。如果设定为0，则是纯色填充
                cellXF.PatternColor = Colors.Red;//设定填充线条的颜色
                //创建列样式创建内容列时引用
                XF cellXF1 = xls.NewXF();
                cellXF1.VerticalAlignment = VerticalAlignments.Centered;
                cellXF1.HorizontalAlignment = HorizontalAlignments.Left;
                cellXF1.ShrinkToCell = true;
                cellXF1.TextWrapRight = true;
                cellXF1.Font.Height = 24 * 14;
                cellXF1.UseBorder = true;
                cellXF1.BottomLineStyle = 1;
                cellXF1.BottomLineColor = Colors.Black;
                cellXF1.TopLineStyle = 1;
                cellXF1.TopLineColor = Colors.Black;
                cellXF1.LeftLineStyle = 1;
                cellXF1.LeftLineColor = Colors.Black;
                cellXF1.RightLineStyle = 1;
                cellXF1.RightLineColor = Colors.Black;
                cellXF1.Pattern = 0;//设定单元格填充风格。如果设定为0，则是纯色填充
                cellXF1.PatternBackgroundColor = Colors.Red;//填充的背景底色
                cellXF1.PatternColor = Colors.Red;//设定填充线条的颜色
                //创建列样式结束
                Cells cells = sheet.Cells; //获得指定工作页列集合
                for (int i = 1; i <= 10; i++)
                {            //列操作基本
                    Cell cell = null;
                    switch (i)
                    {
                        case 1: cell = cells.Add(1, i, "登陆账号", cellXF); break;//添加标题列返回一个列  参数：行 列 名称 样式对象
                        case 2: cell = cells.Add(1, i, "单位名称", cellXF); break;
                        case 3: cell = cells.Add(1, i, "单位代号", cellXF); break;
                        case 4: cell = cells.Add(1, i, "所属管理单位", cellXF); break;
                        case 5: cell = cells.Add(1, i, "级别", cellXF); break;
                        case 6: cell = cells.Add(1, i, "联系人", cellXF); break;
                        case 7: cell = cells.Add(1, i, "联系人职务", cellXF); break;
                        case 8: cell = cells.Add(1, i, "联系电话", cellXF); break;
                        case 9: cell = cells.Add(1, i, "手机", cellXF); break;
                        case 10: cell = cells.Add(1, i, "联系地址", cellXF); break;
                    }
                    cell.HorizontalAlignment = HorizontalAlignments.Centered;
                    cell.VerticalAlignment = VerticalAlignments.Centered;
                    cell.Font.FontFamily = FontFamilies.Modern;//设置字体 默认为宋体               
                    //创建列结束  
                }
                IEnumerable<单位用户> model = 用户管理.查询用户<单位用户>(0,0);
                for (int m = 0; m < model.Count(); m++)
                {
                    for (int n = 1; n <= 10; n++)
                    {
                        Cell cell = null;
                        switch (n)
                        {
                            case 1: cell = cells.Add(m + 2, n, model.ElementAt(m).登录信息.登录名, cellXF1); break;
                            case 2: cell = cells.Add(m + 2, n, model.ElementAt(m).单位信息.单位名称, cellXF1); break;
                            case 3: cell = cells.Add(m + 2, n, model.ElementAt(m).单位信息.单位代号, cellXF1); break;
                            case 4: cell = cells.Add(m + 2, n, model.ElementAt(m).单位信息.所属单位, cellXF1); break;
                            case 5: cell = cells.Add(m + 2, n, model.ElementAt(m).单位信息.单位级别.ToString(), cellXF1); break;
                            case 6: cell = cells.Add(m + 2, n, model.ElementAt(m).联系方式.联系人, cellXF1); break;
                            case 7: cell = cells.Add(m + 2, n, model.ElementAt(m).联系人职务, cellXF1); break;
                            case 8: cell = cells.Add(m + 2, n, model.ElementAt(m).联系方式.固定电话, cellXF1); break;
                            case 9: cell = cells.Add(m + 2, n, model.ElementAt(m).联系方式.手机, cellXF1); break;
                            case 10: cell = cells.Add(m + 2, n, model.ElementAt(m).所属地域.地域, cellXF1); break;
                        }
                        //设置XY居中
                        cell.HorizontalAlignment = HorizontalAlignments.Centered;
                        cell.VerticalAlignment = VerticalAlignments.Centered;
                        //设置字体
                        cell.Font.Bold = false;//设置粗体
                        cell.Font.ColorIndex = 0;//设置颜色码           
                        cell.Font.FontFamily = FontFamilies.Default;//设置字体 默认为宋体               
                        //创建列结束  
                    }
                }
            rs.ContentType = "application/vnd.ms-excel";
            rs.AddHeader("Content-Disposition", string.Format("attachment;filename={0}", xls.FileName));
            xls.Save(rs.OutputStream);
        }
        public static void PutExcel(string type, string strid, HttpResponseBase rs)//导出供应商所有信息
        {
            string idstr = strid;
            //生成Excel开始
            XlsDocument xls = new XlsDocument();
            if (type == "supplier")
            {

                xls.FileName = "供应商信息表" + DateTime.Now.Second.ToString();

                Worksheet sheet = xls.Workbook.Worksheets.Add("供应商");

                //设置文档列属性 
                ColumnInfo cinfo = new ColumnInfo(xls, sheet);
                cinfo.Collapsed = true;
                //设置列的范围 如 0列-10列
                cinfo.ColumnIndexStart = 0;//列开始
                cinfo.ColumnIndexEnd = 7;//列结束
                //cinfo.Collapsed = true;
                cinfo.Width = 100 * 60;//列宽度
                sheet.AddColumnInfo(cinfo);
                //设置文档列属性结束

                //创建列样式创建标题列时引用
                XF cellXF = xls.NewXF();
                cellXF.VerticalAlignment = VerticalAlignments.Centered;
                cellXF.HorizontalAlignment = HorizontalAlignments.Centered;
                cellXF.ShrinkToCell = true;
                cellXF.TextWrapRight = true;
                cellXF.UseBorder = true;
                cellXF.Font.Height = 24 * 12;
                cellXF.Font.Bold = true;
                cellXF.Pattern = 0;//设定单元格填充风格。如果设定为0，则是纯色填充
                cellXF.PatternColor = Colors.Red;//设定填充线条的颜色
                //创建列样式创建内容列时引用
                XF cellXF1 = xls.NewXF();
                cellXF1.VerticalAlignment = VerticalAlignments.Centered;
                cellXF1.HorizontalAlignment = HorizontalAlignments.Left;
                cellXF1.ShrinkToCell = true;
                cellXF1.TextWrapRight = true;
                cellXF1.UseBorder = true;
                cellXF1.Pattern = 0;//设定单元格填充风格。如果设定为0，则是纯色填充
                cellXF1.PatternBackgroundColor = Colors.Red;//填充的背景底色
                cellXF1.PatternColor = Colors.Red;//设定填充线条的颜色
                //创建列样式结束
                Cells cells = sheet.Cells; //获得指定工作页列集合
                for (int i = 1; i <= 8; i++)
                {            //列操作基本
                    Cell cell = null;
                    switch (i)
                    {
                        case 1: cell = cells.Add(1, i, "供应商名称", cellXF); break;//添加标题列返回一个列  参数：行 列 名称 样式对象
                        case 2: cell = cells.Add(1, i, "认证级别", cellXF); break;
                        case 3: cell = cells.Add(1, i, "联系人", cellXF); break;
                        case 4: cell = cells.Add(1, i, "联系电话", cellXF); break;
                        case 5: cell = cells.Add(1, i, "主要经营范围", cellXF); break;
                        case 6: cell = cells.Add(1, i, "是否是全军物资采购供应商", cellXF); break;
                        case 7: cell = cells.Add(1, i, "供应商类别", cellXF); break;
                        case 8: cell = cells.Add(1, i, "备注", cellXF); break;
                    }
                    cell.HorizontalAlignment = HorizontalAlignments.Centered;
                    cell.VerticalAlignment = VerticalAlignments.Centered;
                    cell.Font.FontFamily = FontFamilies.Modern;//设置字体 默认为宋体               
                    //创建列结束  
                }
                List<供应商> model = new List<供应商>();
                if (!string.IsNullOrWhiteSpace(idstr))
                {
                    string[] sid = strid.Split(',');
                    for (int j = 0; j < sid.Length; j++)
                    {
                        if (!string.IsNullOrWhiteSpace(sid[j]))
                        {
                            model.Add(用户管理.查找用户<供应商>(long.Parse(sid[j])));
                        }
                    }
                }
                for (int m = 0; m < model.Count(); m++)
                {
                    for (int n = 1; n <= 8; n++)
                    {
                        Cell cell = null;
                        switch (n)
                        {
                            case 1: cell = cells.Add(m + 2, n, model[m].企业基本信息.企业名称, cellXF1); break;
                            case 2: cell = cells.Add(m + 2, n, model[m].供应商用户信息.认证级别.ToString(), cellXF1); break;
                            case 3: cell = cells.Add(m + 2, n, model[m].企业联系人信息.联系人姓名, cellXF1); break;
                            case 4: cell = cells.Add(m + 2, n, model[m].企业联系人信息.联系人手机, cellXF1); break;
                            case 5: cell = cells.Add(m + 2, n, model[m].营业执照信息.经营范围, cellXF1); break;
                            case 6: cell = cells.Add(m + 2, n, model[m].供应商用户信息.用户来源.ToString(), cellXF1); break;
                            case 7:
                                if (model[m].供应商用户信息.协议供应商 && model[m].供应商用户信息.应急供应商)
                                {
                                    cell = cells.Add(m + 2, n, "应急协议供应商", cellXF1);
                                }
                                else if (model[m].供应商用户信息.应急供应商 && !model[m].供应商用户信息.协议供应商)
                                {
                                    cell = cells.Add(m + 2, n, "应急供应商", cellXF1);
                                }
                                else if (model[m].供应商用户信息.协议供应商 && !model[m].供应商用户信息.应急供应商)
                                {
                                    cell = cells.Add(m + 2, n, "协议供应商", cellXF1);
                                }
                                else
                                {
                                    cell = cells.Add(m + 2, n, "普通供应商", cellXF1);
                                }
                                break;
                            case 8: cell = cells.Add(m + 2, n, "", cellXF1); break;
                        }
                        //设置XY居中
                        cell.HorizontalAlignment = HorizontalAlignments.Centered;
                        cell.VerticalAlignment = VerticalAlignments.Centered;
                        //设置字体
                        cell.Font.Bold = false;//设置粗体
                        cell.Font.ColorIndex = 0;//设置颜色码           
                        cell.Font.FontFamily = FontFamilies.Default;//设置字体 默认为宋体               
                        //创建列结束  
                    }
                }
            }
            else
            {
                xls.FileName = "单位用户列表" + DateTime.Now.Second.ToString();

                Worksheet sheet = xls.Workbook.Worksheets.Add("单位用户");

                //设置文档列属性 
                ColumnInfo cinfo = new ColumnInfo(xls, sheet);
                cinfo.Collapsed = true;
                //设置列的范围 如 0列-10列
                cinfo.ColumnIndexStart = 0;//列开始
                cinfo.ColumnIndexEnd = 9;//列结束
                //cinfo.Collapsed = true;
                cinfo.Width = 100 * 60;//列宽度
                sheet.AddColumnInfo(cinfo);
                //设置文档列属性结束

                //创建列样式创建标题列时引用
                XF cellXF = xls.NewXF();
                cellXF.VerticalAlignment = VerticalAlignments.Centered;
                cellXF.HorizontalAlignment = HorizontalAlignments.Centered;
                cellXF.ShrinkToCell = true;
                cellXF.TextWrapRight = true;
                cellXF.UseBorder = true;
                cellXF.Font.Height = 24 * 12;
                cellXF.Font.Bold = true;
                cellXF.Pattern = 0;//设定单元格填充风格。如果设定为0，则是纯色填充
                cellXF.PatternColor = Colors.Red;//设定填充线条的颜色
                //创建列样式创建内容列时引用
                XF cellXF1 = xls.NewXF();
                cellXF1.VerticalAlignment = VerticalAlignments.Centered;
                cellXF1.HorizontalAlignment = HorizontalAlignments.Left;
                cellXF1.ShrinkToCell = true;
                cellXF1.TextWrapRight = true;
                cellXF1.UseBorder = true;
                cellXF1.Pattern = 0;//设定单元格填充风格。如果设定为0，则是纯色填充
                cellXF1.PatternBackgroundColor = Colors.Red;//填充的背景底色
                cellXF1.PatternColor = Colors.Red;//设定填充线条的颜色
                //创建列样式结束
                Cells cells = sheet.Cells; //获得指定工作页列集合
                for (int i = 1; i <= 10; i++)
                {            //列操作基本
                    Cell cell = null;
                    switch (i)
                    {
                        case 1: cell = cells.Add(1, i, "登陆账号", cellXF); break;//添加标题列返回一个列  参数：行 列 名称 样式对象
                        case 2: cell = cells.Add(1, i, "单位名称", cellXF); break;
                        case 3: cell = cells.Add(1, i, "单位代号", cellXF); break;
                        case 4: cell = cells.Add(1, i, "所属管理单位", cellXF); break;
                        case 5: cell = cells.Add(1, i, "级别", cellXF); break;
                        case 6: cell = cells.Add(1, i, "联系人", cellXF); break;
                        case 7: cell = cells.Add(1, i, "联系人职务", cellXF); break;
                        case 8: cell = cells.Add(1, i, "联系电话", cellXF); break;
                        case 9: cell = cells.Add(1, i, "手机", cellXF); break;
                        case 10: cell = cells.Add(1, i, "联系地址", cellXF); break;
                    }
                    cell.HorizontalAlignment = HorizontalAlignments.Centered;
                    cell.VerticalAlignment = VerticalAlignments.Centered;
                    cell.Font.FontFamily = FontFamilies.Modern;//设置字体 默认为宋体               
                    //创建列结束  
                }
                List<单位用户> model = new List<单位用户>();
                if (!string.IsNullOrWhiteSpace(idstr))
                {
                    string[] sid = strid.Split(',');
                    for (int j = 0; j < sid.Length; j++)
                    {
                        if (!string.IsNullOrWhiteSpace(sid[j]))
                        {
                            model.Add(用户管理.查找用户<单位用户>(long.Parse(sid[j])));
                        }
                    }
                }
                for (int m = 0; m < model.Count(); m++)
                {
                    for (int n = 1; n <= 8; n++)
                    {
                        Cell cell = null;
                        switch (n)
                        {
                            case 1: cell = cells.Add(m + 2, n, model[m].登录信息.登录名, cellXF1); break;
                            case 2: cell = cells.Add(m + 2, n, model[m].单位信息.单位名称, cellXF1); break;
                            case 3: cell = cells.Add(m + 2, n, model[m].单位信息.单位代号, cellXF1); break;
                            case 4: cell = cells.Add(m + 2, n, model[m].单位信息.所属单位, cellXF1); break;
                            case 5: cell = cells.Add(m + 2, n, model[m].单位信息.单位级别.ToString(), cellXF1); break;
                            case 6: cell = cells.Add(m + 2, n, model[m].联系方式.联系人, cellXF1); break;
                            case 7: cell = cells.Add(m + 2, n, model[m].联系人职务, cellXF1); break;
                            case 8:cell = cells.Add(m + 2, n, model[m].联系方式.固定电话, cellXF1);break;
                            case 9: cell = cells.Add(m + 2, n, model[m].联系方式.手机, cellXF1); break;
                            case 10: cell = cells.Add(m + 2, n, model[m].所属地域.地域, cellXF1); break;
                        }
                        //设置XY居中
                        cell.HorizontalAlignment = HorizontalAlignments.Centered;
                        cell.VerticalAlignment = VerticalAlignments.Centered;
                        //设置字体
                        cell.Font.Bold = false;//设置粗体
                        cell.Font.ColorIndex = 0;//设置颜色码           
                        cell.Font.FontFamily = FontFamilies.Default;//设置字体 默认为宋体               
                        //创建列结束  
                    }
                }
            }
            rs.ContentType = "application/vnd.ms-excel";
            rs.AddHeader("Content-Disposition", string.Format("attachment;filename={0}", xls.FileName));
            xls.Save(rs.OutputStream);
        }
        public static void PutOutExcel(string strid, HttpResponseBase rs)//导出电话号码
        {
            string idstr = strid;
            //生成Excel开始
            XlsDocument xls = new XlsDocument();

            xls.FileName = "供应商信息电话号码" + DateTime.Now.Second.ToString();

            Worksheet sheet = xls.Workbook.Worksheets.Add("供应商信息电话号码");

            //设置文档列属性 
            ColumnInfo cinfo = new ColumnInfo(xls, sheet);
            cinfo.Collapsed = true;
            //设置列的范围 如 0列-10列
            cinfo.ColumnIndexStart = 0;//列开始
            cinfo.ColumnIndexEnd = 0;//列结束
            //cinfo.Collapsed = true;
            cinfo.Width = 100 * 60;//列宽度
            sheet.AddColumnInfo(cinfo);
            //设置文档列属性结束

            //创建列样式创建标题列时引用
            XF cellXF = xls.NewXF();
            cellXF.VerticalAlignment = VerticalAlignments.Centered;
            cellXF.HorizontalAlignment = HorizontalAlignments.Centered;
            cellXF.ShrinkToCell = true;
            cellXF.TextWrapRight = true;
            cellXF.UseBorder = true;
            cellXF.Font.Height = 24 * 12;
            cellXF.Font.Bold = true;
            cellXF.Pattern = 0;//设定单元格填充风格。如果设定为0，则是纯色填充
            cellXF.PatternColor = Colors.Red;//设定填充线条的颜色
            //创建列样式创建内容列时引用
            XF cellXF1 = xls.NewXF();
            cellXF1.VerticalAlignment = VerticalAlignments.Centered;
            cellXF1.HorizontalAlignment = HorizontalAlignments.Left;
            cellXF1.ShrinkToCell = true;
            cellXF1.TextWrapRight = true;
            cellXF1.UseBorder = true;
            cellXF1.Pattern = 0;//设定单元格填充风格。如果设定为0，则是纯色填充
            cellXF1.PatternBackgroundColor = Colors.Red;//填充的背景底色
            cellXF1.PatternColor = Colors.Red;//设定填充线条的颜色
            //创建列样式结束
            Cells cells = sheet.Cells; //获得指定工作页列集合
            for (int i = 1; i <= 2; i++)
            {            //列操作基本
                Cell cell = null;
                switch (i)
                {
                    case 1: cell = cells.Add(1, i, "供应商名称", cellXF); break;//添加标题列返回一个列  参数：行 列 名称 样式对象
                    case 2: cell = cells.Add(1, 2, "联系号码", cellXF); break;
                }
                cell.HorizontalAlignment = HorizontalAlignments.Centered;
                cell.VerticalAlignment = VerticalAlignments.Centered;
                cell.Font.FontFamily = FontFamilies.Modern;//设置字体 默认为宋体               
                //创建列结束  
            }
            List<供应商> model = new List<供应商>();
            if (!string.IsNullOrWhiteSpace(idstr))
            {
                string[] sid = strid.Split(',');
                for (int j = 0; j < sid.Length; j++)
                {
                    if (!string.IsNullOrWhiteSpace(sid[j]))
                    {
                        model.Add(用户管理.查找用户<供应商>(long.Parse(sid[j])));
                    }
                }
            }
            for (int m = 0; m < model.Count(); m++)
            {
                for (int n = 1; n <= 2; n++)
                {
                    Cell cell = null;
                    switch (n)
                    {
                        case 1: cell = cells.Add(m + 2, n, model[m].企业基本信息.企业名称, cellXF1); break;
                        case 2: cell = cells.Add(m + 2, n, model[m].企业联系人信息.联系人手机, cellXF1); break;
                    }
                    //设置XY居中
                    cell.HorizontalAlignment = HorizontalAlignments.Centered;
                    cell.VerticalAlignment = VerticalAlignments.Centered;
                    //设置字体
                    cell.Font.Bold = false;//设置粗体
                    cell.Font.ColorIndex = 0;//设置颜色码           
                    cell.Font.FontFamily = FontFamilies.Default;//设置字体 默认为宋体               
                    //创建列结束  
                }
            }
            rs.ContentType = "application/vnd.ms-excel";
            rs.AddHeader("Content-Disposition", string.Format("attachment;filename={0}", xls.FileName));
            xls.Save(rs.OutputStream);
        }
        public static void PutDemandInfo(string strid, HttpResponseBase rs)
        {
            string idstr = strid;
            //生成Excel开始
            XlsDocument xls = new XlsDocument();

            xls.FileName = "军队物资集中采购需求计划表" + DateTime.Now.Second.ToString();

            Worksheet sheet = xls.Workbook.Worksheets.Add("需求采购任务表");
            Worksheet sheet1 = xls.Workbook.Worksheets.Add("需求采购任务分发物资列表");
            Worksheet sheet2 = xls.Workbook.Worksheets.Add("需求采购任务分发列表");
            //设置文档列属性 
            ColumnInfo cinfo = new ColumnInfo(xls, sheet);
            ColumnInfo cinfo1 = new ColumnInfo(xls, sheet1);
            ColumnInfo cinfo2 = new ColumnInfo(xls, sheet2);
            ColumnInfo cinfo3 = new ColumnInfo(xls, sheet1);
            ColumnInfo cinfo4 = new ColumnInfo(xls, sheet1);
            ColumnInfo cinfo5 = new ColumnInfo(xls, sheet1);

            //设置列的范围 如 0列-10列
            cinfo.ColumnIndexStart = 0;//列开始
            cinfo.ColumnIndexEnd = 10;//列结束
            cinfo.Width = 100 * 40;//列宽度
            sheet.AddColumnInfo(cinfo);

            cinfo1.ColumnIndexStart = 0;//列开始
            cinfo1.ColumnIndexEnd = 10;//列结束
            cinfo1.Width = 100 * 40;//列宽度
            sheet1.AddColumnInfo(cinfo1);

            cinfo2.ColumnIndexStart = 0;//列开始
            cinfo2.ColumnIndexEnd = 10;//列结束
            cinfo2.Width = 100 * 40;//列宽度
            sheet2.AddColumnInfo(cinfo2);

            cinfo3.ColumnIndexStart = 0;//列开始
            cinfo3.ColumnIndexEnd = 0;//列结束
            cinfo3.Width = 10000;//列宽度

            cinfo4.ColumnIndexStart = 6;//列开始
            cinfo4.ColumnIndexEnd = 6;//列结束
            cinfo4.Width = 2800;//列宽度

            cinfo5.ColumnIndexStart = 8;//列开始
            cinfo5.ColumnIndexEnd = 8;//列结束
            cinfo5.Width = 2800;//列宽度

            XF cellXF = xls.NewXF();
            cellXF.VerticalAlignment = VerticalAlignments.Centered;
            cellXF.HorizontalAlignment = HorizontalAlignments.Centered;
            cellXF.ShrinkToCell = true;
            cellXF.TextWrapRight = true;
            cellXF.Font.Height = 20 * 12;
            cellXF.Font.Weight = FontWeight.SemiBold;
            //创建列样式创建内容列时引用

            XF cellXF1 = xls.NewXF();
            cellXF1.VerticalAlignment = VerticalAlignments.Centered;
            cellXF1.HorizontalAlignment = HorizontalAlignments.Left;
            cellXF1.ShrinkToCell = true;
            cellXF1.TextWrapRight = true;
            cellXF1.UseBorder = true;
            cellXF1.Font.Height = 20 * 10;
            cellXF1.Font.Weight = FontWeight.Normal;
            //创建列样式结束

            Cells cells = sheet.Cells; //获得指定工作页列集合
            cells.Merge(1, 1, 1, 10);
            Cell header = cells.Add(1, 1, "需求采购任务");
            header.Font.Height = 250;
            header.Font.Weight = FontWeight.Bold;
            header.HorizontalAlignment = HorizontalAlignments.Centered;
            for (int i = 1; i <= 10; i++)
            {            //列操作基本
                Cell cell = null;
                switch (i)
                {
                    case 1: cell = cells.Add(2, i, "管理部门", cellXF); break;//添加标题列返回一个列  参数：行 列 名称 样式对象
                    case 2: cell = cells.Add(2, i, "包含物资项", cellXF); break;
                    case 3: cell = cells.Add(2, i, "包含分发项", cellXF); break;
                    case 4: cell = cells.Add(2, i, "采购机构", cellXF); break;
                    case 5: cell = cells.Add(2, i, "采购方式建议", cellXF); break;
                    case 6: cell = cells.Add(2, i, "下达时间", cellXF); break;
                    case 7: cell = cells.Add(2, i, "完成时间", cellXF); break;
                    case 8: cell = cells.Add(2, i, "联系人", cellXF); break;
                    case 9: cell = cells.Add(2, i, "联系电话", cellXF); break;
                    case 10: cell = cells.Add(2, i, "描述", cellXF); break;
                }
                cell.UseBorder = true;

                cell.TopLineStyle = 1;
                cell.TopLineColor = Colors.Black;

                cell.LeftLineStyle = 1;
                cell.LeftLineColor = Colors.Black;

                cell.RightLineStyle = 1;
                cell.RightLineColor = Colors.Black;

                cell.BottomLineStyle = 1;
                cell.BottomLineColor = Colors.Black;
                cell.HorizontalAlignment = HorizontalAlignments.Centered;
                cell.VerticalAlignment = VerticalAlignments.Centered;
                cell.Font.FontFamily = FontFamilies.Modern;//设置字体 默认为宋体               
                //创建列结束  
            }
            需求采购任务 plan = 需求采购任务管理.查找需求采购任务(long.Parse(strid));
            for (int n = 1; n <= 10; n++)
            {
                Cell cell = null;
                switch (n)
                {
                    case 1: cell = cells.Add(3, n, plan.需求发起单位链接.用户数据.单位信息.单位名称, cellXF1); break;
                    case 2: cell = cells.Add(3, n, plan.物资列表.Count, cellXF1); break;
                    case 3: cell = cells.Add(3, n, plan.分发列表.Count, cellXF1); break;
                    case 4: cell = cells.Add(3, n, plan.当前处理单位链接.用户数据.单位信息.单位名称, cellXF1); break;
                    case 5: cell = cells.Add(3, n, plan.采购方式.ToString(), cellXF1); break;
                    case 6: cell = cells.Add(3, n, plan.基本数据.添加时间.ToString("yyyy/MM/dd"), cellXF1); break;
                    case 7: cell = cells.Add(3, n, plan.建议完成时间.ToString("yyyy/MM/dd"), cellXF1); break;
                    case 8: cell = cells.Add(3, n, plan.联系人, cellXF1); break;
                    case 9: cell = cells.Add(3, n, plan.联系电话, cellXF1); break;
                    case 10: cell = cells.Add(3, n, plan.描述, cellXF1); break;
                }
                cell.UseBorder = true;

                cell.TopLineStyle = 1;
                cell.TopLineColor = Colors.Black;

                cell.LeftLineStyle = 1;
                cell.LeftLineColor = Colors.Black;

                cell.RightLineStyle = 1;
                cell.RightLineColor = Colors.Black;

                cell.BottomLineStyle = 1;
                cell.BottomLineColor = Colors.Black;
                //设置XY居中
                cell.HorizontalAlignment = HorizontalAlignments.Centered;
                cell.VerticalAlignment = VerticalAlignments.Centered;
                //设置字体
                cell.Font.Bold = false;//设置粗体
                cell.Font.ColorIndex = 0;//设置颜色码           
                cell.Font.FontFamily = FontFamilies.Default;//设置字体 默认为宋体               
                //创建列结束  
            }

            Cells cells1 = sheet1.Cells; //获得指定工作页列集合
            cells1.Merge(1, 1, 1, 10);
            Cell header1 = cells1.Add(1, 1, "需求采购任务分发物资");
            header1.Font.Height = 250;
            header1.Font.Weight = FontWeight.Bold;
            header1.HorizontalAlignment = HorizontalAlignments.Centered;
            for (int i = 1; i <= 10; i++)
            {            //列操作基本
                Cell cell = null;
                switch (i)
                {
                    case 1: cell = cells1.Add(2, i, "物资名称", cellXF); sheet1.AddColumnInfo(cinfo3); break;//添加标题列返回一个列  参数：行 列 名称 样式对象
                    case 2: cell = cells1.Add(2, i, "规格型号", cellXF); break;
                    case 3: cell = cells1.Add(2, i, "计量单位", cellXF); break;
                    case 4: cell = cells1.Add(2, i, "数量", cellXF); break;
                    case 5: cell = cells1.Add(2, i, "单价", cellXF); break;
                    case 6: cell = cells1.Add(2, i, "预算金额", cellXF); break;
                    case 7: cell = cells1.Add(2, i, "质量技术标准", cellXF); sheet1.AddColumnInfo(cinfo4); break;
                    case 8: cell = cells1.Add(2, i, "交货期限", cellXF); break;
                    case 9: cell = cells1.Add(2, i, "采购方式建议", cellXF); sheet1.AddColumnInfo(cinfo5); break;
                    case 10: cell = cells1.Add(2, i, "备注", cellXF); break;
                }
                cell.UseBorder = true;

                cell.TopLineStyle = 1;
                cell.TopLineColor = Colors.Black;

                cell.LeftLineStyle = 1;
                cell.LeftLineColor = Colors.Black;

                cell.RightLineStyle = 1;
                cell.RightLineColor = Colors.Black;

                cell.BottomLineStyle = 1;
                cell.BottomLineColor = Colors.Black;
                cell.HorizontalAlignment = HorizontalAlignments.Centered;
                cell.VerticalAlignment = VerticalAlignments.Centered;
                cell.Font.FontFamily = FontFamilies.Modern;//设置字体 默认为宋体               
                //创建列结束  
            }
            for (int i = 0; i < plan.物资列表.Count; i++)
            {
                for (int n = 1; n <= 10; n++)
                {
                    Cell cell = null;
                    switch (n)
                    {
                        case 1: cell = cells1.Add(i + 3, n, plan.物资列表[i].需求计划物资数据.物资名称, cellXF1); break;
                        case 2: cell = cells1.Add(i + 3, n, plan.物资列表[i].需求计划物资数据.规格型号, cellXF1); break;
                        case 3: cell = cells1.Add(i + 3, n, plan.物资列表[i].需求计划物资数据.计量单位, cellXF1); break;
                        case 4: cell = cells1.Add(i + 3, n, plan.物资列表[i].需求计划物资数据.数量, cellXF1); break;
                        case 5: cell = cells1.Add(i + 3, n, plan.物资列表[i].需求计划物资数据.单价, cellXF1); break;
                        case 6: cell = cells1.Add(i + 3, n, plan.物资列表[i].需求计划物资数据.预算金额, cellXF1); break;
                        case 7: cell = cells1.Add(i + 3, n, plan.物资列表[i].需求计划物资数据.技术指标, cellXF1); break;
                        case 8: cell = cells1.Add(i + 3, n, plan.物资列表[i].需求计划物资数据.交货期限.ToString("yyyy/MM/dd"), cellXF1); break;
                        case 9: cell = cells1.Add(i + 3, n, plan.物资列表[i].需求计划物资数据.建议采购方式, cellXF1); break;
                        case 10: cell = cells1.Add(i + 3, n, plan.物资列表[i].需求计划物资数据.备注, cellXF1); break;
                    }
                    cell.UseBorder = true;

                    cell.TopLineStyle = 1;
                    cell.TopLineColor = Colors.Black;

                    cell.LeftLineStyle = 1;
                    cell.LeftLineColor = Colors.Black;

                    cell.RightLineStyle = 1;
                    cell.RightLineColor = Colors.Black;

                    cell.BottomLineStyle = 1;
                    cell.BottomLineColor = Colors.Black;
                    //设置XY居中
                    cell.HorizontalAlignment = HorizontalAlignments.Centered;
                    cell.VerticalAlignment = VerticalAlignments.Centered;
                    //设置字体
                    cell.Font.Bold = false;//设置粗体
                    cell.Font.ColorIndex = 0;//设置颜色码           
                    cell.Font.FontFamily = FontFamilies.Default;//设置字体 默认为宋体               
                    //创建列结束  
                }
            }
            Cells cells2 = sheet2.Cells; //获得指定工作页列集合
            cells2.Merge(1, 1, 1, 9);
            Cell header2 = cells2.Add(1, 1, "需求采购任务分发");
            header2.Font.Height = 250;
            header2.Font.Weight = FontWeight.Bold;
            header2.HorizontalAlignment = HorizontalAlignments.Centered;
            for (int i = 1; i < 10; i++)
            {            //列操作基本
                Cell cell = null;
                switch (i)
                {
                    case 1: cell = cells2.Add(2, i, "物资名称", cellXF); break;//添加标题列返回一个列  参数：行 列 名称 样式对象
                    case 2: cell = cells2.Add(2, i, "规格型号", cellXF); break;
                    case 3: cell = cells2.Add(2, i, "计量单位", cellXF); break;
                    case 4: cell = cells2.Add(2, i, "收货单位名称", cellXF); break;
                    case 5: cell = cells2.Add(2, i, "分配数量", cellXF); break;
                    case 6: cell = cells2.Add(2, i, "提货方式", cellXF); break;
                    case 7: cell = cells2.Add(2, i, "运输方式", cellXF); break;
                    case 8: cell = cells2.Add(2, i, "到站", cellXF); break;
                    case 9: cell = cells2.Add(2, i, "备注", cellXF); break;
                }
                cell.UseBorder = true;

                cell.TopLineStyle = 1;
                cell.TopLineColor = Colors.Black;

                cell.LeftLineStyle = 1;
                cell.LeftLineColor = Colors.Black;

                cell.RightLineStyle = 1;
                cell.RightLineColor = Colors.Black;

                cell.BottomLineStyle = 1;
                cell.BottomLineColor = Colors.Black;
                cell.HorizontalAlignment = HorizontalAlignments.Centered;
                cell.VerticalAlignment = VerticalAlignments.Centered;
                cell.Font.FontFamily = FontFamilies.Modern;//设置字体 默认为宋体               
                //创建列结束  
            }
            for (int i = 0; i < plan.分发列表.Count; i++)
            {
                for (int n = 1; n <= 9; n++)
                {
                    Cell cell = null;
                    switch (n)
                    {
                        case 1: cell = cells2.Add(i + 3, n, plan.分发列表[i].需求计划分发数据.物资名称, cellXF1); break;
                        case 2: cell = cells2.Add(i + 3, n, plan.分发列表[i].需求计划分发数据.规格型号, cellXF1); break;
                        case 3: cell = cells2.Add(i + 3, n, plan.分发列表[i].需求计划分发数据.计量单位, cellXF1); break;
                        case 4: cell = cells2.Add(i + 3, n, plan.分发列表[i].需求计划分发数据.收货单位名称, cellXF1); break;
                        case 5: cell = cells2.Add(i + 3, n, plan.分发列表[i].需求计划分发数据.分配数量, cellXF1); break;
                        case 6: cell = cells2.Add(i + 3, n, plan.分发列表[i].需求计划分发数据.提货方式.ToString(), cellXF1); break;
                        case 7: cell = cells2.Add(i + 3, n, plan.分发列表[i].需求计划分发数据.运输方式.ToString(), cellXF1); break;
                        case 8: cell = cells2.Add(i + 3, n, plan.分发列表[i].需求计划分发数据.到站, cellXF1); break;
                        case 9: cell = cells2.Add(i + 3, n, plan.分发列表[i].需求计划分发数据.备注, cellXF1); break;
                    }
                    cell.UseBorder = true;

                    cell.TopLineStyle = 1;
                    cell.TopLineColor = Colors.Black;

                    cell.LeftLineStyle = 1;
                    cell.LeftLineColor = Colors.Black;

                    cell.RightLineStyle = 1;
                    cell.RightLineColor = Colors.Black;

                    cell.BottomLineStyle = 1;
                    cell.BottomLineColor = Colors.Black;
                    //设置XY居中
                    cell.HorizontalAlignment = HorizontalAlignments.Centered;
                    cell.VerticalAlignment = VerticalAlignments.Centered;
                    //设置字体
                    cell.Font.Bold = false;//设置粗体
                    cell.Font.ColorIndex = 0;//设置颜色码           
                    cell.Font.FontFamily = FontFamilies.Default;//设置字体 默认为宋体               
                    //创建列结束  
                }
            }
            rs.ContentType = "application/vnd.ms-excel";
            rs.AddHeader("Content-Disposition", string.Format("attachment;filename={0}", xls.FileName));
            xls.Save(rs.OutputStream);
        }
        //public static void PutMaterialInfo(string strid, HttpResponseBase rs)
        //{

        //    string idstr = strid;
        //    需求计划 plan = 需求计划管理.查找需求计划(long.Parse(strid));
        //    //生成Excel开始
        //    XlsDocument xls = new XlsDocument();

        //    xls.FileName = "军队物资集中采购需求计划表" + DateTime.Now.Second.ToString();

        //    XF left = xls.NewXF();//表头样式
        //    left.HorizontalAlignment = HorizontalAlignments.Left;
        //    left.Font.Height = 240;

        //    XF right = xls.NewXF();//表头样式
        //    right.HorizontalAlignment = HorizontalAlignments.Right;
        //    right.Pattern = 0;
        //    right.Font.Height = 240;

        //    XF number = xls.NewXF();//表头样式
        //    number.HorizontalAlignment = HorizontalAlignments.Centered;
        //    number.Font.Height = 240;

        //    XF center = xls.NewXF();//表头样式
        //    center.HorizontalAlignment = HorizontalAlignments.CenteredAcrossSelection;
        //    center.Font.Height = 300;

        //    var shet = xls.Workbook.Worksheets.Add("封面");
        //    #region 封面
        //    Cells cells0 = shet.Cells; //获得指定工作页列集合
        //    cells0.Merge(3, 3, 1, 12);
        //    cells0.Add(3, 1, "秘密等级：", right);
        //    cells0.Merge(3, 3, 13, 14);
        //    var cels = cells0.Add(3, 13, plan.秘密等级.ToString(), left);
        //    cels.BottomLineColor = Colors.Black;
        //    cels.BottomLineStyle = 2;
        //    cels = cells0.Add(3, 14, "", left);
        //    cels.BottomLineStyle = 2;
        //    cels.BottomLineColor = Colors.Black;


        //    cells0.Merge(9, 9, 1, 14);
        //    cels = cells0.Add(9, 1, "军 队 物 资 集 中 采 购 需 求 计 划", center);
        //    cels.Font.Height = 600;

        //    cells0.Merge(21, 21, 1, 5);
        //    cels = cells0.Add(21, 1, "编制单位：", right);
        //    cels.Font.Height = 300;
        //    cells0.Merge(21, 21, 6, 10);
        //    cels = cells0.Add(21, 6, plan.编制单位.ToString(), left);
        //    cels.BottomLineColor = Colors.Black;
        //    cels.BottomLineStyle = 2;
        //    cels = cells0.Add(21, 7, "", left);
        //    cels.BottomLineColor = Colors.Black;
        //    cels.BottomLineStyle = 2;
        //    cels = cells0.Add(21, 8, "", left);
        //    cels.BottomLineColor = Colors.Black;
        //    cels.BottomLineStyle = 2;
        //    cels = cells0.Add(21, 9, "", left);
        //    cels.BottomLineColor = Colors.Black;
        //    cels.BottomLineStyle = 2;
        //    cels = cells0.Add(21, 10, "", left);
        //    cels.BottomLineColor = Colors.Black;
        //    cels.BottomLineStyle = 2;

        //    cells0.Merge(26, 26, 1, 14);
        //    cells0.Add(26, 1, "年                月                日", center);
        //    #endregion

        //    var sheet = xls.Workbook.Worksheets.Add("军队物资集中采购需求计划表");
        //    #region 创建需求计划表
        //    //设置文档列属性 
        //    ColumnInfo cinfo = new ColumnInfo(xls, sheet);//规格型号列宽
        //    ColumnInfo cinfo1 = new ColumnInfo(xls, sheet);//序号列宽
        //    ColumnInfo cinfo2 = new ColumnInfo(xls, sheet);//计量单位列宽
        //    ColumnInfo cinfo3 = new ColumnInfo(xls, sheet);//预算金额、质量技术标准、交货期限、采购方式建议列宽
        //    ColumnInfo cinfo4 = new ColumnInfo(xls, sheet);//物资名称列宽
        //    ColumnInfo cinfo5 = new ColumnInfo(xls, sheet);//备注列宽
        //    //设置列的范围 如 0列-10列
        //    cinfo1.ColumnIndexStart = 0;//列开始
        //    cinfo1.ColumnIndexEnd = 0;//列结束
        //    cinfo1.Width = 1000;//列宽度
        //    sheet.AddColumnInfo(cinfo1);

        //    //设置列的范围 如 0列-10列
        //    cinfo2.ColumnIndexStart = 3;//列开始
        //    cinfo2.ColumnIndexEnd = 3;//列结束
        //    cinfo2.Width = 1600;//列宽度
        //    sheet.AddColumnInfo(cinfo2);

        //    //设置列的范围 如 0列-10列
        //    cinfo3.ColumnIndexStart = 6;//列开始
        //    cinfo3.ColumnIndexEnd = 9;//列结束
        //    cinfo3.Width = 3000;//列宽度
        //    sheet.AddColumnInfo(cinfo3);

        //    cinfo4.ColumnIndexStart = 1;//列开始
        //    cinfo4.ColumnIndexEnd = 1;//列结束
        //    cinfo4.Width = 7000;//列宽度
        //    sheet.AddColumnInfo(cinfo4);

        //    cinfo5.ColumnIndexStart = 10;//列开始
        //    cinfo5.ColumnIndexEnd = 10;//列结束
        //    cinfo5.Width = 6500;//列宽度
        //    sheet.AddColumnInfo(cinfo5);

        //    //设置列的范围 如 0列-10列
        //    cinfo.ColumnIndexStart = 2;//列开始
        //    cinfo.ColumnIndexEnd = 2;//列结束
        //    //cinfo.Collapsed = true;
        //    cinfo.Width = 100 * 30;//列宽度
        //    sheet.AddColumnInfo(cinfo);

        //    //设置文档列属性结束

        //    //创建列样式创建标题列时引用
        //    XF cellXF = xls.NewXF();
        //    cellXF.VerticalAlignment = VerticalAlignments.Centered;
        //    cellXF.HorizontalAlignment = HorizontalAlignments.Centered;
        //    cellXF.ShrinkToCell = true;
        //    cellXF.TextWrapRight = true;
        //    cellXF.UseBorder = true;
        //    cellXF.Font.Height = 20 * 12;
        //    cellXF.Font.Weight = FontWeight.Medium;
        //    //cellXF.Pattern =200;//设定单元格填充风格。如果设定为0，则是纯色填充
        //    //cellXF.PatternColor = Colors.Red;//设定填充线条的颜色
        //    //创建列样式创建内容列时引用
        //    XF cellXF1 = xls.NewXF();
        //    cellXF1.VerticalAlignment = VerticalAlignments.Centered;
        //    cellXF1.HorizontalAlignment = HorizontalAlignments.Left;
        //    cellXF1.ShrinkToCell = true;
        //    cellXF1.TextWrapRight = true;
        //    cellXF1.UseBorder = true;
        //    cellXF1.Font.Height = 20 * 10;
        //    cellXF1.Font.Weight = FontWeight.Normal;
        //    //cellXF1.Pattern = 0;//设定单元格填充风格。如果设定为0，则是纯色填充
        //    //cellXF1.PatternBackgroundColor = Colors.Red;//填充的背景底色
        //    //cellXF1.PatternColor = Colors.Red;//设定填充线条的颜色

        //    //创建列样式结束
        //    Cells cells = sheet.Cells; //获得指定工作页列集合
        //    cells.Merge(1, 1, 1, 11);
        //    cells.Add(1, 1, "军队物资集中采购需求计划表", center);
        //    cells.Merge(2, 3, 1, 9);
        //    cels = cells.Add(2, 1, "编制单位：" + plan.编制单位, left);
        //    cels.VerticalAlignment = VerticalAlignments.Centered;
        //    cells.Merge(2, 3, 10, 11);
        //    cels = cells.Add(2, 10, "采购年度：" + plan.采购年度.ToString("yyyy年MM月dd日"), right);
        //    cels.VerticalAlignment = VerticalAlignments.Centered;

        //    #region 生成表头
        //    for (int i = 1; i <= 11; i++)
        //    {            //列操作基本
        //        Cell cell = null;
        //        switch (i)
        //        {
        //            case 1: cell = cells.Add(4, i, "序号", cellXF); break;//添加标题列返回一个列  参数：行 列 名称 样式对象
        //            case 2: cell = cells.Add(4, i, "物资名称", cellXF); break;
        //            case 3: cell = cells.Add(4, i, "规格型号", cellXF); break;
        //            case 4: cell = cells.Add(4, i, "计量单位", cellXF); break;
        //            case 5: cell = cells.Add(4, i, "数量", cellXF); break;
        //            case 6: cell = cells.Add(4, i, "单价（元）", cellXF); break;
        //            case 7: cell = cells.Add(4, i, "预算金额（元）", cellXF); break;
        //            case 8: cell = cells.Add(4, i, "质量技术标准", cellXF); break;
        //            case 9: cell = cells.Add(4, i, "交货期限", cellXF); break;
        //            case 10: cell = cells.Add(4, i, "采购方式建议", cellXF); break;
        //            case 11: cell = cells.Add(4, i, "备注", cellXF); break;
        //        }
        //        cell.HorizontalAlignment = HorizontalAlignments.Centered;
        //        cell.VerticalAlignment = VerticalAlignments.Centered;
        //        cell.Font.FontFamily = FontFamilies.Modern;//设置字体 默认为宋体 
        //        //设置单元格边框样式
        //        cell.LeftLineColor = Colors.Black;
        //        cell.LeftLineStyle = 1;
        //        cell.RightLineColor = Colors.Black;
        //        cell.RightLineStyle = 1;
        //        cell.TopLineColor = Colors.Black;
        //        cell.TopLineStyle = 1;
        //        cell.BottomLineColor = Colors.Black;
        //        cell.BottomLineStyle = 1;

        //        //创建列结束  
        //    }
        //    #endregion

        //    #region 生成表中内容 
        //    for (int m = 0; m < plan.物资列表.Count; m++)
        //    {
        //        for (int n = 1; n <= 11; n++)
        //        {
        //            cells.Merge(m * 2 + 5, m * 2 + 6, n, n);
        //            Cell cell = null;
        //            switch (n)
        //            {
        //                case 1: cell = cells.Add(m * 2 + 5, n, m + 1, cellXF1); break;
        //                case 2: cell = cells.Add(m * 2 + 5, n, plan.物资列表[m].需求计划物资数据.物资名称, cellXF1); break;
        //                case 3: cell = cells.Add(m * 2 + 5, n, plan.物资列表[m].需求计划物资数据.规格型号, cellXF1); break;
        //                case 4: cell = cells.Add(m * 2 + 5, n, plan.物资列表[m].需求计划物资数据.计量单位, cellXF1); break;
        //                case 5: cell = cells.Add(m * 2 + 5, n, plan.物资列表[m].需求计划物资数据.数量, cellXF1); break;
        //                case 6: cell = cells.Add(m * 2 + 5, n, plan.物资列表[m].需求计划物资数据.单价, cellXF1); break;
        //                case 7: cell = cells.Add(m * 2 + 5, n, plan.物资列表[m].需求计划物资数据.预算金额, cellXF1); break;
        //                case 8: cell = cells.Add(m * 2 + 5, n, plan.物资列表[m].需求计划物资数据.技术指标, cellXF1); break;
        //                case 9: cell = cells.Add(m * 2 + 5, n, plan.物资列表[m].需求计划物资数据.交货期限.ToString("yyyy-MM-dd"), cellXF1); break;
        //                case 10: cell = cells.Add(m * 2 + 5, n, plan.物资列表[m].需求计划物资数据.建议采购方式.ToString(), cellXF1); break;
        //                case 11: cell = cells.Add(m * 2 + 5, n, (plan.物资列表[m].需求计划物资数据.所属需求计划.需求计划数据.需求发起单位链接.用户数据.单位信息.单位代号 != null ? plan.物资列表[m].需求计划物资数据.所属需求计划.需求计划数据.需求发起单位链接.用户数据.单位信息.单位代号 : plan.物资列表[m].需求计划物资数据.所属需求计划.需求计划数据.需求发起单位链接.用户数据.单位信息.单位名称), cellXF1); break;
        //            }
        //            //设置XY居中
        //            cell.HorizontalAlignment = HorizontalAlignments.Centered;
        //            cell.VerticalAlignment = VerticalAlignments.Centered;
        //            //设置字体
        //            cell.Font.Height = 230;
        //            cell.Font.Bold = false;//设置粗体
        //            cell.Font.ColorIndex = 0;//设置颜色码           
        //            cell.Font.FontFamily = FontFamilies.Default;//设置字体 默认为宋体   

        //            //设置单元格边框样式
        //            cell.LeftLineColor = Colors.Black;
        //            cell.LeftLineStyle = 1;
        //            cell.RightLineColor = Colors.Black;
        //            cell.RightLineStyle = 1;
        //            cell.TopLineColor = Colors.Black;
        //            cell.TopLineStyle = 1;
        //            cell.BottomLineColor = Colors.Black;
        //            cell.BottomLineStyle = 1;
        //            //创建列结束  

        //            for (int i = 1; i <= 11; i++)
        //            {
        //                cels = cells.Add(m*2 + 6, i, "", cellXF1);
        //                if (i == 1)
        //                {
        //                    cels.LeftLineColor = Colors.Black;
        //                    cels.LeftLineStyle = 1;
        //                }
        //                if (m == plan.物资列表.Count * 2 - 1)
        //                {
        //                    cels.BottomLineColor = Colors.Black;
        //                    cels.BottomLineStyle = 1;
        //                }
        //                cels.RightLineColor = Colors.Black;
        //                cels.RightLineStyle = 1;
        //            }
        //        }
        //    }
        //    #endregion

        //    #region 内容不够时用空的填充
        //    for (int m=plan.物资列表.Count; m < 14; m++)
        //    {
        //        for (int n = 1; n <= 11; n++)
        //        {
        //            cells.Merge(m*2 + 5, m*2 + 6, n, n);
        //            Cell cell = null;
        //            switch (n)
        //            {
        //                case 1: cell = cells.Add(m*2 + 5, n, "", cellXF1); break;
        //                case 2: cell = cells.Add(m*2 + 5, n, "", cellXF1); break;
        //                case 3: cell = cells.Add(m*2 + 5, n, "", cellXF1); break;
        //                case 4: cell = cells.Add(m*2 + 5, n, "", cellXF1); break;
        //                case 5: cell = cells.Add(m*2 + 5, n, "", cellXF1); break;
        //                case 6: cell = cells.Add(m*2 + 5, n, "", cellXF1); break;
        //                case 7: cell = cells.Add(m*2 + 5, n, "", cellXF1); break;
        //                case 8: cell = cells.Add(m*2 + 5, n, "", cellXF1); break;
        //                case 9: cell = cells.Add(m*2 + 5, n, "", cellXF1); break;
        //                case 10: cell = cells.Add(m*2 + 5, n, "", cellXF1); break;
        //                case 11: cell = cells.Add(m*2 + 5, n, "", cellXF1); break;
        //            }
        //            //设置XY居中
        //            cell.HorizontalAlignment = HorizontalAlignments.Centered;
        //            cell.VerticalAlignment = VerticalAlignments.Centered;
        //            //设置字体
        //            cell.Font.Height = 230;
        //            cell.Font.Bold = false;//设置粗体
        //            cell.Font.ColorIndex = 0;//设置颜色码           
        //            cell.Font.FontFamily = FontFamilies.Default;//设置字体 默认为宋体   

        //            //设置单元格边框样式
        //            cell.LeftLineColor = Colors.Black;
        //            cell.LeftLineStyle = 1;
        //            cell.RightLineColor = Colors.Black;
        //            cell.RightLineStyle = 1;
        //            cell.TopLineColor = Colors.Black;
        //            cell.TopLineStyle = 1;
        //            cell.BottomLineColor = Colors.Black;
        //            cell.BottomLineStyle = 1;
        //            //创建列结束  
        //        }

        //        for (int i = 1; i <= 11; i++)
        //        {
        //            cels = cells.Add(m*2 + 6, i, "", cellXF1);
        //            if (i == 1)
        //            {
        //                cels.LeftLineColor = Colors.Black;
        //                cels.LeftLineStyle = 1;
        //            }
        //            if (m == 13)
        //            {
        //                cels.BottomLineColor = Colors.Black;
        //                cels.BottomLineStyle = 1;
        //            }
        //            cels.RightLineColor = Colors.Black;
        //            cels.RightLineStyle = 1;
        //        }
        //    }
        //    #endregion

        //    left.VerticalAlignment = VerticalAlignments.Centered;
        //    cells.Merge(33, 34, 1, 6);
        //    cels = cells.Add(33, 1, "承办部门：" + plan.承办部门, left);
        //    cels.VerticalAlignment = VerticalAlignments.Centered;
        //    cells.Merge(33, 34, 7, 9);
        //    cels = cells.Add(33, 7, "联系人：" + plan.联系人, left);
        //    cels.VerticalAlignment = VerticalAlignments.Centered;
        //    cells.Merge(33, 34, 10, 11);
        //    cels = cells.Add(33, 10, "联系电话：" + plan.联系电话, left);
        //    cels.VerticalAlignment = VerticalAlignments.Centered;

        //    #endregion

        //    var sheet1 = xls.Workbook.Worksheets.Add("军队物资集中采购需求计划分发表");
        //    #region 创建需求分发表
        //    //设置文档列属性 
        //    ColumnInfo colinfo1 = new ColumnInfo(xls, sheet1);//序号列宽
        //    ColumnInfo colinfo2 = new ColumnInfo(xls, sheet1);//计量单位列宽
        //    ColumnInfo colinfo3 = new ColumnInfo(xls, sheet1);//预算金额、质量技术标准、交货期限、采购方式建议列宽
        //    ColumnInfo colinfo4 = new ColumnInfo(xls, sheet1);//物资名称列宽
        //    ColumnInfo colinfo5 = new ColumnInfo(xls, sheet1);//备注列宽
        //    ColumnInfo colinfo6 = new ColumnInfo(xls, sheet1);//收货单位名称列宽
        //    ColumnInfo colinfo7 = new ColumnInfo(xls, sheet1);//规格型号列宽
        //    //设置列的范围 如 0列-10列
        //    colinfo1.ColumnIndexStart = 0;//列开始
        //    colinfo1.ColumnIndexEnd = 0;//列结束
        //    colinfo1.Width = 1000;//列宽度
        //    sheet1.AddColumnInfo(colinfo1);

        //    //设置列的范围 如 0列-10列
        //    colinfo2.ColumnIndexStart = 3;//列开始
        //    colinfo2.ColumnIndexEnd = 3;//列结束
        //    colinfo2.Width = 1600;//列宽度
        //    sheet1.AddColumnInfo(colinfo2);

        //    //设置列的范围 如 0列-10列
        //    colinfo3.ColumnIndexStart = 5;//列开始
        //    colinfo3.ColumnIndexEnd = 8;//列结束
        //    colinfo3.Width = 3000;//列宽度
        //    sheet1.AddColumnInfo(colinfo3);

        //    colinfo4.ColumnIndexStart = 1;//列开始
        //    colinfo4.ColumnIndexEnd = 1;//列结束
        //    colinfo4.Width = 7000;//列宽度
        //    sheet1.AddColumnInfo(colinfo4);

        //    colinfo5.ColumnIndexStart = 9;//列开始
        //    colinfo5.ColumnIndexEnd = 9;//列结束
        //    colinfo5.Width = 6500;//列宽度
        //    sheet1.AddColumnInfo(colinfo5);

        //    colinfo6.ColumnIndexStart = 4;//列开始
        //    colinfo6.ColumnIndexEnd = 4;//列结束
        //    colinfo6.Width = 5000;//列宽度
        //    sheet1.AddColumnInfo(colinfo6);

        //    colinfo7.ColumnIndexStart = 2;//列开始
        //    colinfo7.ColumnIndexEnd = 2;//列结束
        //    colinfo7.Width = 4000;//列宽度
        //    sheet1.AddColumnInfo(colinfo7);
        //    //设置文档列属性结束

        //    //创建列样式创建标题列时引用
        //    XF cellXF2 = xls.NewXF();
        //    cellXF2.VerticalAlignment = VerticalAlignments.Centered;
        //    cellXF2.HorizontalAlignment = HorizontalAlignments.Centered;
        //    cellXF2.ShrinkToCell = true;
        //    cellXF2.TextWrapRight = true;
        //    cellXF2.UseBorder = true;
        //    cellXF2.Font.Height = 20 * 12;
        //    cellXF2.Font.Weight = FontWeight.Medium;
        //    cellXF2.Pattern = 0;//设定单元格填充风格。如果设定为0，则是纯色填充
        //    cellXF2.PatternColor = Colors.Red;//设定填充线条的颜色
        //    //创建列样式创建内容列时引用
        //    XF cellXF3 = xls.NewXF();
        //    cellXF3.VerticalAlignment = VerticalAlignments.Centered;
        //    cellXF3.HorizontalAlignment = HorizontalAlignments.Left;
        //    cellXF3.ShrinkToCell = true;
        //    cellXF3.TextWrapRight = true;
        //    cellXF3.UseBorder = true;
        //    cellXF3.Font.Height = 20 * 10;
        //    cellXF3.Font.Weight = FontWeight.Normal;
        //    cellXF3.Pattern = 0;//设定单元格填充风格。如果设定为0，则是纯色填充
        //    cellXF3.PatternBackgroundColor = Colors.Red;//填充的背景底色
        //    cellXF3.PatternColor = Colors.Red;//设定填充线条的颜色

        //    //创建列样式结束
        //    Cells cells1 = sheet1.Cells; //获得指定工作页列集合
        //    cells1.Merge(1, 1, 1, 10);
        //    cells1.Add(1, 1, "军队物资集中采购需求计划分发表", center);
        //    cells1.Merge(2, 3, 1, 10);
        //    cels = cells1.Add(2, 1, "编制单位：" + plan.编制单位, left);
        //    cels.VerticalAlignment = VerticalAlignments.Centered;
        //    //cells.Add(2, 10, "采购年度：", right);
        //    #region 生成表头
        //    for (int i = 1; i <= 10; i++)
        //    {            //列操作基本
        //        Cell cell = null;
        //        switch (i)
        //        {
        //            case 1: cell = cells1.Add(4, i, "序号", cellXF2); break;//添加标题列返回一个列  参数：行 列 名称 样式对象
        //            case 2: cell = cells1.Add(4, i, "物资名称", cellXF2); break;
        //            case 3: cell = cells1.Add(4, i, "规格型号", cellXF2); break;
        //            case 4: cell = cells1.Add(4, i, "计量单位", cellXF2); break;
        //            case 5: cell = cells1.Add(4, i, "收货单位名称", cellXF2); break;
        //            case 6: cell = cells1.Add(4, i, "分配数量", cellXF2); break;
        //            case 7: cell = cells1.Add(4, i, "提货方式", cellXF2); break;
        //            case 8: cell = cells1.Add(4, i, "运输方式", cellXF2); break;
        //            case 9: cell = cells1.Add(4, i, "到站", cellXF2); break;
        //            case 10: cell = cells1.Add(4, i, "备注", cellXF2); break;
        //        }
        //        cell.HorizontalAlignment = HorizontalAlignments.Centered;
        //        cell.VerticalAlignment = VerticalAlignments.Centered;
        //        cell.Font.FontFamily = FontFamilies.Modern;//设置字体 默认为宋体   

        //        //设置单元格边框样式
        //        cell.LeftLineColor = Colors.Black;
        //        cell.LeftLineStyle = 1;
        //        cell.RightLineColor = Colors.Black;
        //        cell.RightLineStyle = 1;
        //        cell.TopLineColor = Colors.Black;
        //        cell.TopLineStyle = 1;
        //        cell.BottomLineColor = Colors.Black;
        //        cell.BottomLineStyle = 1;
        //        //创建列结束  
        //    }
        //    #endregion

        //    #region 生成内容
        //    for (int m = 0; m < plan.分发列表.Count; m++)
        //    {
        //        for (int n = 1; n <= 10; n++)
        //        {
        //            cells1.Merge(m * 2 + 5, m * 2 + 6, n, n);
        //            Cell cell = null;
        //            switch (n)
        //            {
        //                case 1: cell = cells1.Add(m*2 + 5, n, m + 1, cellXF3); break;
        //                case 2: cell = cells1.Add(m*2 + 5, n, plan.分发列表[m].需求计划分发数据.物资名称, cellXF3); break;
        //                case 3: cell = cells1.Add(m*2 + 5, n, plan.分发列表[m].需求计划分发数据.规格型号, cellXF3); break;
        //                case 4: cell = cells1.Add(m*2 + 5, n, plan.分发列表[m].需求计划分发数据.计量单位, cellXF3); break;
        //                case 5: cell = cells1.Add(m*2 + 5, n, plan.分发列表[m].需求计划分发数据.收货单位名称, cellXF3); break;
        //                case 6: cell = cells1.Add(m*2 + 5, n, plan.分发列表[m].需求计划分发数据.分配数量, cellXF3); break;
        //                case 7: cell = cells1.Add(m*2 + 5, n, plan.分发列表[m].需求计划分发数据.提货方式.ToString(), cellXF3); break;
        //                case 8: cell = cells1.Add(m*2 + 5, n, plan.分发列表[m].需求计划分发数据.运输方式.ToString(), cellXF3); break;
        //                case 9: cell = cells1.Add(m*2 + 5, n, plan.分发列表[m].需求计划分发数据.到站, cellXF3); break;
        //                case 10: cell = cells1.Add(m*2 + 5, n, plan.分发列表[m].需求计划分发数据.备注, cellXF3); break;
        //            }
        //            //设置XY居中
        //            cell.HorizontalAlignment = HorizontalAlignments.Centered;
        //            cell.VerticalAlignment = VerticalAlignments.Centered;
        //            //设置字体
        //            cell.Font.Height = 230;
        //            cell.Font.Bold = false;//设置粗体
        //            cell.Font.ColorIndex = 0;//设置颜色码           
        //            cell.Font.FontFamily = FontFamilies.Default;//设置字体 默认为宋体     

        //            //设置单元格边框样式
        //            cell.LeftLineColor = Colors.Black;
        //            cell.LeftLineStyle = 1;
        //            cell.RightLineColor = Colors.Black;
        //            cell.RightLineStyle = 1;
        //            cell.TopLineColor = Colors.Black;
        //            cell.TopLineStyle = 1;
        //            cell.BottomLineColor = Colors.Black;
        //            cell.BottomLineStyle = 1;
        //            //创建列结束  
        //            for (int i = 1; i <= 10; i++)
        //            {
        //                cels = cells1.Add(m*2 + 6, i, "", cellXF3);
        //                if (i == 1)
        //                {
        //                    cels.LeftLineColor = Colors.Black;
        //                    cels.LeftLineStyle = 1;
        //                }
        //                if (m == plan.分发列表.Count * 2 - 1)
        //                {
        //                    cels.BottomLineColor = Colors.Black;
        //                    cels.BottomLineStyle = 1;
        //                }
        //                cels.RightLineColor = Colors.Black;
        //                cels.RightLineStyle = 1;
        //            }
        //        }
        //    }
        //    #endregion

        //    #region 内容不够时用空的填充
        //    for (int m = plan.分发列表.Count; m < 14; m++)
        //    {
        //        for (int n = 1; n <= 10; n++)
        //        {
        //            cells1.Merge(m * 2 + 5, m * 2 + 6, n, n);
        //            Cell cell = null;
        //            switch (n)
        //            {
        //                case 1: cell = cells1.Add(m * 2 + 5, n, "", cellXF1); break;
        //                case 2: cell = cells1.Add(m * 2 + 5, n, "", cellXF1); break;
        //                case 3: cell = cells1.Add(m * 2 + 5, n, "", cellXF1); break;
        //                case 4: cell = cells1.Add(m * 2 + 5, n, "", cellXF1); break;
        //                case 5: cell = cells1.Add(m * 2 + 5, n, "", cellXF1); break;
        //                case 6: cell = cells1.Add(m * 2 + 5, n, "", cellXF1); break;
        //                case 7: cell = cells1.Add(m * 2 + 5, n, "", cellXF1); break;
        //                case 8: cell = cells1.Add(m * 2 + 5, n, "", cellXF1); break;
        //                case 9: cell = cells1.Add(m * 2 + 5, n, "", cellXF1); break;
        //                case 10: cell = cells1.Add(m * 2 + 5, n, "", cellXF1); break;
        //                case 11: cell = cells1.Add(m * 2 + 5, n, "", cellXF1); break;
        //            }
        //            //设置XY居中
        //            cell.HorizontalAlignment = HorizontalAlignments.Centered;
        //            cell.VerticalAlignment = VerticalAlignments.Centered;
        //            //设置字体
        //            cell.Font.Height = 230;
        //            cell.Font.Bold = false;//设置粗体
        //            cell.Font.ColorIndex = 0;//设置颜色码           
        //            cell.Font.FontFamily = FontFamilies.Default;//设置字体 默认为宋体   

        //            //设置单元格边框样式
        //            cell.LeftLineColor = Colors.Black;
        //            cell.LeftLineStyle = 1;
        //            cell.RightLineColor = Colors.Black;
        //            cell.RightLineStyle = 1;
        //            cell.TopLineColor = Colors.Black;
        //            cell.TopLineStyle = 1;
        //            cell.BottomLineColor = Colors.Black;
        //            cell.BottomLineStyle = 1;
        //            //创建列结束  
        //        }

        //        for (int i = 1; i <= 10; i++)
        //        {
        //            cels = cells1.Add(m * 2 + 6, i, "", cellXF1);
        //            if (i == 1)
        //            {
        //                cels.LeftLineColor = Colors.Black;
        //                cels.LeftLineStyle = 1;
        //            }
        //            if (m == 13)
        //            {
        //                cels.BottomLineColor = Colors.Black;
        //                cels.BottomLineStyle = 1;
        //            }
        //            cels.RightLineColor = Colors.Black;
        //            cels.RightLineStyle = 1;
        //        }

        //    }
        //    #endregion
        //    left.VerticalAlignment = VerticalAlignments.Centered;
        //    cells1.Merge(33, 34, 1, 4);
        //    cells1.Add(33, 1, "承办部门：" + plan.承办部门, left);
        //    cells1.Merge(33, 34, 5, 8);
        //    cells1.Add(33, 5, "联系人：" + plan.联系人, left);
        //    cells1.Merge(33, 34, 9, 10);
        //    cells1.Add(33, 9, "联系电话：" + plan.联系电话, left);

        //    #endregion

        //    var sheet2 = xls.Workbook.Worksheets.Add("《军队物资集中采购需求计划》填制说明");
        //    #region 填制说明
        //    Cells cells2 = sheet2.Cells; //获得指定工作页列集合
        //    cells2.Merge(1, 1, 1, 14);
        //    cells2.Add(1, 1, "《军队物资集中采购需求计划》填制说明", center);
        //    cells2.Merge(2, 4, 1, 14);
        //    var cellWrap = cells2.Add(2, 1, "一、《军队物资集中采购需求计划》包括《军队物资集中采购需求计划表》和《军队物资集中采购需求计划分发表》，由事业部门依据年度采购预算编制，预算批复后20个工作日内送军需物资油料部门。", left);
        //    cellWrap.TextWrapRight = true;
        //    cells2.Merge(5, 5, 1, 14);
        //    cells2.Add(3, 1, "二、\"物资名称\"栏按照采购目录区分的物资类别填至具体品种名称。", left);
        //    cells2.Merge(6, 6, 1, 14);
        //    cells2.Add(4, 1, "三、\"计量单位\"按照采购目录明确的计量单位填写。", left);
        //    cells2.Merge(7, 7, 1, 14);
        //    cells2.Add(5, 1, "四、对采购机构、供应商的建议可在《军队物资集中采购需求计划表》\"备注\"栏内填写。", left);
        //    cells2.Merge(8, 8, 1, 14);
        //    cells2.Add(6, 1, "五、非标准产品或者有特殊要求的产品应当另附详细说。", left);
        //    cells2.Merge(9, 9, 1, 14);
        //    cells2.Add(7, 1, "六、\"提货方式\"栏填写：自提、代运。", left);
        //    cells2.Merge(10, 10, 1, 14);
        //    cells2.Add(8, 1, "七、\"运输方式\"栏填写：铁路运输、公路运输、水路运输、航空运输。", left);
        //    cells2.Merge(11, 11, 1, 14);
        //    cells2.Add(9, 1, "八、本表使用A4纸（210X297mm）。", left);
        //    #endregion

        //    rs.ContentType = "application/vnd.ms-excel";
        //    rs.AddHeader("Content-Disposition", string.Format("attachment;filename={0}", xls.FileName));
        //    xls.Save(rs.OutputStream);

        //}
        //public static void PutDistributeInfo(string strid, HttpResponseBase rs)
        //{
        //    string idstr = strid;
        //    //生成Excel开始
        //    XlsDocument xls = new XlsDocument();

        //    xls.FileName = "军队物资集中采购需求计划分发表" + DateTime.Now.Second.ToString();

        //    Worksheet sheet = xls.Workbook.Worksheets.Add("军队物资集中采购需求计划分发表");


        //}

    }
}