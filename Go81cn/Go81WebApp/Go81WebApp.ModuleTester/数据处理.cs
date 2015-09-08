using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using Go81WebApp.Models.Helpers;
using Go81WebApp.Models.数据模型;
using Go81WebApp.Models.数据模型.内容数据模型;
using Go81WebApp.Models.数据模型.商品数据模型;
using Go81WebApp.Models.数据模型.权限数据模型;
using Go81WebApp.Models.数据模型.用户数据模型;
using Go81WebApp.Models.数据模型.项目数据模型;
using Go81WebApp.Models.管理器;
using Ionic.Zip;
using Ivony.Html;
using Ivony.Html.Parser;
using MongoDB;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Driver.Builders;
using MongoDB.Driver.Linq;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Windows;
using System.Collections.Generic;
using PanGu.Framework;
using Go81WebApp.Models.数据模型.推广业务数据模型;
using Go81WebApp.Models.管理器.推广业务管理;

namespace Go81WebApp.ModuleTester
{
    public partial class MainWindow : Window
    {
        private static string 商品品牌统计()
        {
            var 返回字符串文本 = "";
            var firstclass = 商品分类管理.查找子分类();
            foreach (var spclass in firstclass)
            {
                if (spclass.子分类.Any())
                {
                    foreach (var 商品分类 in spclass.子分类)
                    {
                        if (商品分类.子分类.Any())
                        {
                            返回字符串文本 += spclass.分类名 + ">" + 商品分类.分类名 + "\r\n";

                            foreach (var 商品分类1 in 商品分类.子分类)
                            {
                                var 分类下商品 = 商品管理.查询分类下商品(商品分类1.Id, 0, 0,
                   Query<商品>.Where(o => o.审核数据.审核状态 == 审核状态.审核通过));
                                Dictionary<string, int> model_str = new Dictionary<string, int>();
                                if (分类下商品 != null && 分类下商品.Any())
                                {
                                    foreach (var item in 分类下商品)
                                    {
                                        if (!string.IsNullOrWhiteSpace(item.商品信息.品牌))
                                        {
                                            if (model_str.ContainsKey(item.商品信息.品牌))
                                            {
                                                model_str[item.商品信息.品牌] += 1;
                                            }
                                            else
                                            {
                                                model_str.Add(item.商品信息.品牌, 1);
                                            }
                                        }
                                        else
                                        {
                                            if (model_str.ContainsKey("未填写品牌"))
                                            {
                                                model_str["未填写品牌"] += 1;
                                            }
                                            else
                                            {
                                                model_str.Add("未填写品牌", 1);
                                            }
                                        }
                                    }

                                    返回字符串文本 += "      " + 商品分类1.分类名 + "       该分类下审核通过商品总数：" + 分类下商品.Count().ToString() + "\r\n" + "           ";
                                    if (model_str.Any())
                                    {
                                        foreach (var it in model_str)
                                        {
                                            返回字符串文本 += it.Key + "：" + it.Value + "个" + "      ";
                                        }
                                    }
                                    返回字符串文本 += "\r\n";
                                }

                            }
                        }
                        else
                        {

                            var 商品分类1 = 商品分类;
                            var 分类下商品 = 商品管理.查询分类下商品(商品分类1.Id, 0, 0,
                   Query<商品>.Where(o => o.审核数据.审核状态 == 审核状态.审核通过));
                            Dictionary<string, int> model_str = new Dictionary<string, int>();
                            if (分类下商品 != null && 分类下商品.Any())
                            {
                                返回字符串文本 += "      专用物资>" + spclass.分类名 + "\r\n" + "   ";
                                foreach (var item in 分类下商品)
                                {
                                    if (!string.IsNullOrWhiteSpace(item.商品信息.品牌))
                                    {
                                        if (model_str.ContainsKey(item.商品信息.品牌))
                                        {
                                            model_str[item.商品信息.品牌] += 1;
                                        }
                                        else
                                        {
                                            model_str.Add(item.商品信息.品牌, 1);
                                        }
                                    }
                                    else
                                    {
                                        if (model_str.ContainsKey("未填写品牌"))
                                        {
                                            model_str["未填写品牌"] += 1;
                                        }
                                        else
                                        {
                                            model_str.Add("未填写品牌", 1);
                                        }
                                    }
                                }

                                返回字符串文本 += 商品分类1.分类名 + "       该分类下审核通过商品总数：" + 分类下商品.Count().ToString() + "\r\n" + "           ";
                                if (model_str.Any())
                                {
                                    foreach (var it in model_str)
                                    {
                                        返回字符串文本 += it.Key + "：" + it.Value + "个" + "      ";
                                    }
                                }
                                返回字符串文本 += "\r\n\r\n";
                            }
                        }

                    }
                }
            }
            return 返回字符串文本;
        }
        private static string 合并账号(long 保留账号ID,string 企业名称)
        {
            var retstr = "";
            var gys1 = 用户管理.查找用户<供应商>(保留账号ID);

            var gyslist = 用户管理.查询用户<供应商>(0, 0, Query<供应商>.Where(o => o.企业基本信息.企业名称 == 企业名称 && o.Id != 保留账号ID),includeDeleted:true);
            foreach (var gys in gyslist)
            {
                var classlist = gys.可提供产品类别列表;
                foreach (var cla in classlist)
                {
                    gys1.可提供产品类别列表.Add(cla);
                }
                var sp = 商品管理.查询供应商商品(gys.Id, 0, 0);
                foreach (var s in sp)
                {
                    s.商品信息.所属供应商.用户ID = 保留账号ID;
                    商品管理.更新商品(s, false, false);
                }
                var ysd = 验收单管理.查询验收单(0, 0, Query<验收单>.Where(o => o.供应商链接.用户ID == gys.Id));
                foreach (var k in ysd)
                {
                    k.供应商链接.用户ID = 保留账号ID;
                    验收单管理.更新验收单(k);
                }

                用户管理.删除用户<供应商>(gys.Id);
            }
            用户管理.更新用户<供应商>(gys1);
            retstr += 商品管理.计数供应商商品(保留账号ID, 0, 0).ToString();
            return retstr;
        }
        private static string 同步验收单()
        {
            var retstr = "";
            var retstr1 = "已改变状态的验收单如下：\r\n";
            var retstr2 = "状态不一致的验收单如下，请查证后再设置状态：\r\n";

            //处理图片资源
            //textBox1.Text = string.Join("\r\n", 工具Data.列表资源().Where(r => r.存在).Select(r => r.路径));
            var fp = 工具.ChooseOpenFile("导出Coll-验收单.js");
            if (string.IsNullOrWhiteSpace(fp)) return "内容为空";
            using (var sr = new System.IO.StreamReader(fp, Encoding.UTF8))
            {
                try
                {
                    //内网导出的验收单列表
                    var ln_ysdlist = new List<验收单>();
                    for (; ; )
                    {
                        var json = sr.ReadLine();
                        if (null == json)
                        {
                            break;
                        }
                        var doc = BsonSerializer.Deserialize<验收单>(json);
                        ln_ysdlist.Add(doc);
                    }
                    retstr += "内网导出验收单总数：" + ln_ysdlist.Count + "\r\n";

                    if (ln_ysdlist.Any())
                    {
                        //外网的验收单列表
                        var ysdlist = 验收单管理.查询验收单(0, 0, includeDeleted: false);
                        foreach (var ysd in ysdlist)
                        {
                            //得出ID相同的内网验收单
                            var in_ysd = ln_ysdlist.Where(o => o.Id == ysd.Id);
                            if (in_ysd.Any())
                            {
                                var in_y = in_ysd.First();
                                //内网已审核，而外网未审核，改变外网数据状态
                                if (in_y.审核数据.审核状态 != 审核状态.未审核 && ysd.审核数据.审核状态 == 审核状态.未审核)
                                {
                                    ysd.审核数据 = in_y.审核数据;
                                    ysd.验收单核对码 = in_y.验收单核对码;
                                    验收单管理.更新验收单(ysd);
                                    retstr1 += "          验收单号：" + ysd.验收单号 + "\r\n";
                                }
                                else if (in_y.审核数据.审核状态 != 审核状态.未审核 && ysd.审核数据.审核状态 != 审核状态.未审核 && ysd.审核数据.审核状态 != in_y.审核数据.审核状态)
                                {
                                    //内外网均审核，但状态不一致，不进行修改，写入文本，待查证
                                    retstr2 += "          验收单号：" + ysd.验收单号 + "，内网状态：" + in_y.审核数据.审核状态 + "，外网状态：" + ysd.审核数据.审核状态 + "\r\n";
                                }
                            }
                        }
                    }
                    
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
                finally
                {
                    sr.Close();
                }
            }
            return retstr + retstr1 + retstr2;
        }
    }
}
