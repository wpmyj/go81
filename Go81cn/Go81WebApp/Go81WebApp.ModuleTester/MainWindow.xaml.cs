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
using System.IO;
using Go81WebApp.Models.数据模型.需求计划模型;
using Go81WebApp.Models.数据模型.统计数据模型;

namespace Go81WebApp.ModuleTester
{
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();
            ConfigurationManager.AppSettings["Mongo账号"] = "go81";
            ConfigurationManager.AppSettings["Mongo密码"] = "123456";
            ConfigurationManager.AppSettings["Mongo服务器IP"] =
                Dns.GetHostAddresses(Dns.GetHostName())
                    .Where(ip => ip.AddressFamily == AddressFamily.InterNetwork)
                    .Select(ip => ip.ToString())
                    .Any(ip => ip.EndsWith("101.101") || new[] { "222.214.218.92" }.Contains(ip))
                    ? "127.0.0.1"
                    : "127.0.0.1"
                ;
            ConfigurationManager.AppSettings["Mongo服务器端口"] =
                Dns.GetHostAddresses(Dns.GetHostName())
                    .Where(ip => ip.AddressFamily == AddressFamily.InterNetwork)
                    .Select(ip => ip.ToString())
                    .Any(ip => ip.EndsWith("101.101"))
                    ? "17707"
                    : "27017"
                ;
            ConfigurationManager.AppSettings["Mongo数据库名"] = "go81";
        }
        private void Button_Click1(object sender, RoutedEventArgs e)
        {
            IEnumerable<验收单> ysd=验收单管理.查询验收单(0,0);
            foreach(var item in ysd)
            {
                foreach(var k in 验收单单位列表信息.验收单单位列表)
                {
                    if (item.审核数据.审核者.用户ID == k.Id && item.审核数据.审核者.用户ID == 10013 && item.审核数据.审核时间 >= new DateTime(2015, 7, 9) && item.审核数据.审核时间<=new DateTime(2015,9,18))
                    {
                        item.管理单位审核人签名 = "/Images/seal/77200签名.png";
                    }
                    else if (item.审核数据.审核者.用户ID == k.Id && item.审核数据.审核者.用户ID == 10013 && item.审核数据.审核时间 > new DateTime(2015, 9, 18))
                    {
                        item.管理单位审核人签名 = "/Images/seal/14集团军签名.png";
                    }
                    else if (item.审核数据.审核者.用户ID == k.Id)
                    {
                        item.管理单位审核人签名 = k.签名图片链接;
                    }
                }
                验收单管理.更新验收单(item);

            }



            

            //List<long> idlist = new List<long> { 10001, 10003,20317, 20306, 10004, 10005, 10007, 10008, 10009, 10011, 10012, 10013, 10014, 10015, 20057, 20151, 20150, 20145, 20146, 20137, 20138, 20139, 20140, 20141, 20142, 20143, 20144, 20152, 20149, 20314, 20148, 20261, 20292, 20254 };
            //foreach (var item in idlist)
            //{
            //    单位用户 m = 用户管理.查找用户<单位用户>(item);
            //    if (m != null)
            //    {
            //        switch (item)
            //        {
            //            case 10001:
            //                m.验收单名称 = "成都军区联勤部军需物资油料部";
            //                m.验收单审核单位名称 = "成都军区联勤部军需物资油料部(中标)";
            //                m.印章底部文本 = "物资采购专用章";
            //                break;
            //            case 10003:
            //                m.验收单名称 = "成都军区联勤部军需物资油料部";
            //                m.验收单审核单位名称 = "成都军区联勤部军需物资油料部";
            //                m.印章底部文本 = "物资采购专用章";
            //                break;
            //            case 10004:
            //                m.验收单名称 = "中国人民解放军78006部队";
            //                m.验收单审核单位名称 = "中国人民解放军78006部队";
            //                m.印章底部文本 = "物资采购专用章";
            //                break;
            //            case 10005:
            //                m.验收单名称 = "中国人民解放军四川省军区机关";
            //                m.验收单审核单位名称 = "中国人民解放军四川省军区机关";
            //                m.印章底部文本 = "物资集中采购办公室";
            //                break;
            //            case 10007:
            //                m.验收单名称 = "成都总医院物资采购中心";
            //                m.验收单审核单位名称 = "成都总医院物资采购中心";
            //                m.印章底部文本 = "物资采购专用章";
            //                break;
            //            case 10008:
            //                m.验收单名称 = "中国人民解放军77100部队";
            //                m.验收单审核单位名称 = "中国人民解放军77100部队";
            //                m.印章底部文本 = "物资采购办公室";
            //                break;
            //            case 10009:
            //                m.验收单名称 = "中国人民解放军重庆警备区";
            //                m.验收单审核单位名称 = "中国人民解放军重庆警备区";
            //                m.印章底部文本 = "物资采购办公室";
            //                break;
            //            case 10011:
            //                m.验收单名称 = "中国人民解放军后勤工程学院物资采购";
            //                m.验收单审核单位名称 = "中国人民解放军后勤工程学院";
            //                m.印章底部文本 = "管理办公室";
            //                break;
            //            case 10012:
            //                m.验收单名称 = "中国人民解放军第三二四医院";
            //                m.验收单审核单位名称 = "中国人民解放军第三二四医院";
            //                m.印章底部文本 = "物资采购专用章";
            //                break;
            //            case 10013:
            //                m.验收单名称 = "中国人民解放军77200部队";
            //                m.验收单审核单位名称 = "中国人民解放军77200部队";
            //                m.印章底部文本 = "物资采购办公室";
            //                break;
            //            case 10014:
            //                m.验收单名称 = "云南省军区物资集中采购办公室";
            //                m.验收单审核单位名称 = "中国人民解放军云南省军区";
            //                m.印章底部文本 = "";
            //                break;
            //            case 10015:
            //                m.验收单名称 = "昆明民族干部学院";
            //                m.验收单审核单位名称 = "昆明民族干部学院";
            //                m.印章底部文本 = "";
            //                break;
            //            case 20057:
            //                m.验收单名称 = "贵州省军区后勤部";
            //                m.验收单审核单位名称 = "中国人民解放军贵州省军区";
            //                m.印章底部文本 = "";
            //                break;
            //            case 20151:
            //                m.验收单名称 = "中国人民解放军77215部队";
            //                m.验收单审核单位名称 = "中国人民解放军77215部队";
            //                m.印章底部文本 = "物资采购专用章";
            //                break;
            //            case 20150:
            //                m.验收单名称 = "中国人民解放军77221部队";
            //                m.验收单审核单位名称 = "中国人民解放军77221部队";
            //                m.印章底部文本 = "物资采购专用章";
            //                break;
            //            case 20145:
            //                m.验收单名称 = "中国人民解放军77225部队";
            //                m.验收单审核单位名称 = "中国人民解放军77225部队";
            //                m.印章底部文本 = "物资采购专用章";
            //                break;
            //            case 20146:
            //                m.验收单名称 = "中国人民解放军77226部队";
            //                m.验收单审核单位名称 = "中国人民解放军77226部队";
            //                m.印章底部文本 = "物资采购专用章";
            //                break;
            //            case 20137:
            //                m.验收单名称 = "中国人民解放军77228部队";
            //                m.验收单审核单位名称 = "中国人民解放军77228部队";
            //                m.印章底部文本 = "物资采购专用章";
            //                break;
            //            case 20138:
            //                m.验收单名称 = "中国人民解放军77229部队";
            //                m.验收单审核单位名称 = "中国人民解放军77229部队";
            //                m.印章底部文本 = "物资采购专用章";
            //                break;
            //            case 20139:
            //                m.验收单名称 = "中国人民解放军77231部队";
            //                m.验收单审核单位名称 = "中国人民解放军77231部队";
            //                m.印章底部文本 = "物资采购专用章";
            //                break;
            //            case 20140:
            //                m.验收单名称 = "中国人民解放军77235部队";
            //                m.验收单审核单位名称 = "中国人民解放军77235部队";
            //                m.印章底部文本 = "物资采购专用章";
            //                break;
            //            case 20141:
            //                m.验收单名称 = "中国人民解放军77251部队";
            //                m.验收单审核单位名称 = "中国人民解放军77251部队";
            //                m.印章底部文本 = "物资采购专用章";
            //                break;
            //            case 20142:
            //                m.验收单名称 = "中国人民解放军77256部队";
            //                m.验收单审核单位名称 = "中国人民解放军77256部队";
            //                m.印章底部文本 = "物资采购专用章";
            //                break;
            //            case 20143:
            //                m.验收单名称 = "中国人民解放军77298部队";
            //                m.验收单审核单位名称 = "中国人民解放军77298部队";
            //                m.印章底部文本 = "物资采购专用章";
            //                break;
            //            case 20144:
            //                m.验收单名称 = "中国人民解放军77223部队";
            //                m.验收单审核单位名称 = "中国人民解放军77223部队";
            //                m.印章底部文本 = "物资采购专用章";
            //                break;
            //            case 20152:
            //                m.验收单名称 = "中国人民解放军77211部队";
            //                m.验收单审核单位名称 = "中国人民解放军77211部队";
            //                m.印章底部文本 = "物资采购专用章";
            //                break;
            //            case 20149:
            //                m.验收单名称 = "中国人民解放军77216部队";
            //                m.验收单审核单位名称 = "中国人民解放军77216部队";
            //                m.印章底部文本 = "物资采购专用章";
            //                break;
            //            case 20314:
            //                m.验收单名称 = "中国人民解放军77206部队";
            //                m.验收单审核单位名称 = "中国人民解放军77206部队";
            //                m.印章底部文本 = "物资采购专用章";
            //                break;
            //            case 20148:
            //                m.验收单名称 = "中国人民解放军77208部队";
            //                m.验收单审核单位名称 = "中国人民解放军77208部队";
            //                m.印章底部文本 = "物资采购专用章";
            //                break;
            //            case 20261: m.验收单名称 = "中国人民解放军西藏军区物资采购中心";
            //                m.验收单审核单位名称 = "中国人民解放军西藏军区物资采购中心";
            //                m.印章底部文本 = ""; break;
            //            case 20292: m.验收单名称 = "织金县人武部";
            //                m.验收单审核单位名称 = "织金县人武部";
            //                m.印章底部文本 = "物资采购专用章";
            //                m.单位信息.单位代号 = "织金县人武部";
            //                break;
            //            case 20254: m.验收单名称 = "贵州省贵阳警备区后勤部";
            //                m.验收单审核单位名称 = "贵州省贵阳警备区后勤部";
            //                m.印章底部文本 = "物资采购专用章";
            //                m.单位信息.单位代号 = "贵州省贵阳警备区后勤部";
            //                break;
            //            case 20306:
            //                m.验收单名称 = "中国人民解放军贵州省贵阳市观山湖区人武部";
            //                m.验收单审核单位名称 = "贵州省贵阳市观山湖区人武部";
            //                m.印章底部文本 = "";
            //                break;
            //            case 20317:
            //                m.验收单名称 = "中国人民解放军78655部队";
            //                m.验收单审核单位名称 = "中国人民解放军78655部队";
            //                m.印章底部文本 = "物资采购专用章";
            //                break;
            //        }
            //        用户管理.更新用户<单位用户>(m);
               // }
           // }
            //textBox1.Text = 合并账号(200000000626, "成都欣德隆商贸有限公司");
            //var x = 商品管理.查询商品(0, 0, Query<商品>.EQ(o => o.中标商品, false));

            //var u = 用户管理.查找用户<单位用户>(10002);
            //u.用户组.RemoveAll(o => o == "需求合并");
            //用户管理.更新用户(u, false);

            //u = 用户管理.查找用户<单位用户>(11);
            //u.用户组.RemoveAll(o => o == "需求合并");
            //用户管理.更新用户(u, false);

            //u = 用户管理.查找用户<单位用户>(14);
            //u.用户组.RemoveAll(o => o == "需求合并");
            //用户管理.更新用户(u, false);

            //u = 用户管理.查找用户<单位用户>(15);
            //u.用户组.Add("需求合并");
            //用户管理.更新用户(u, false);
            //textBox1.Text = 商品品牌统计();
        }
        private void Button_Click2(object sender, RoutedEventArgs e)
        {
            var u = new 单位用户();
            u.登录信息.登录名 = "CJCDCGZ007";
            u.登录信息.密码 = "123456";
            u.所属地域.省份 = "四川省";
            u.所属地域.城市 = "成都市";
            u.所属地域.区县 = "";
            u.单位信息.单位名称 = "成都军区成都物资采购站";
            u.审核数据.审核状态 = 审核状态.审核通过;
            u.审核数据.审核时间 = DateTime.Now;
            u.审核数据.审核者.用户ID = 16;
            u.管理单位.用户ID = 16;
            u.用户组.Add("二级管理员账号");
            用户管理.添加用户(u);

            u = new 单位用户();
            u.登录信息.登录名 = "CJCDCGZ008";
            u.登录信息.密码 = "123456";
            u.所属地域.省份 = "四川省";
            u.所属地域.城市 = "成都市";
            u.所属地域.区县 = "";
            u.单位信息.单位名称 = "成都军区成都物资采购站";
            u.审核数据.审核状态 = 审核状态.审核通过;
            u.审核数据.审核时间 = DateTime.Now;
            u.审核数据.审核者.用户ID = 16;
            u.管理单位.用户ID = 16;
            u.用户组.Add("二级管理员账号");
            用户管理.添加用户(u);

            var d1 = 用户管理.查找用户<单位用户>(16);
            d1.登录信息.密码 = "7C4A8D09CA3762AF61E59520943DC26494F8941B";
            用户管理.更新用户<单位用户>(d1, false);


            var unitlist = 用户管理.查询用户<单位用户>(0, 0);
            foreach (var unit in unitlist)
            {
                if (unit.用户组.Contains("操作员"))
                {
                    unit.用户组 = new List<string> { "二级管理员账号" };
                    用户管理.更新用户<单位用户>(unit, false);
                }
            }
            MessageBox.Show("success");
            //var retstr = "";
            //Dictionary<string, List<string>> xx = new Dictionary<string, List<string>>();
            //var ysdlist = 验收单管理.查询验收单(0, 0,
            //    Query<验收单>.Where(o => !o.是否作废 && o.是否已经打印 && o.审核数据.审核状态 == 审核状态.审核通过 && !o.验收单扫描件.Any()), includeDisabled: false);
            //ysdlist = ysdlist.Where(o => o.打印信息.Any() && o.打印信息.Last().打印时间 < new DateTime(2015, 7, 1));
            //foreach (var ysd in ysdlist)
            //{
            //    if (ysd.供应商链接.用户数据 != null)
            //    {
            //        if (xx.ContainsKey(ysd.供应商链接.用户数据.企业基本信息.企业名称))
            //        {
            //            xx[ysd.供应商链接.用户数据.企业基本信息.企业名称].Add(ysd.验收单号);
            //        }
            //        else
            //        {
            //            xx.Add(ysd.供应商链接.用户数据.企业基本信息.企业名称, new List<string> { ysd.验收单号 });
            //        }
            //    }
            //}

            //foreach (var x in xx)
            //{
            //    retstr += x.Key + "\r\n";
            //    foreach (var n in x.Value)
            //    {
            //        retstr += "     " + n + "\r\n";
            //    }
            //}
            //textBox1.Text = retstr;
            //添加专家数据(150);
        }
        private void Button_Click3(object sender, RoutedEventArgs e)
        {
            ///////////////////////////////////////批量更新专家数据--------可参评物资类别列表
            var hylist = 商品分类管理.查找子分类().Select(o => o.分类名);

            var zjlist = 用户管理.查询用户<专家>(0, 0,
                Query<专家>.Where(o => o.可参评物资类别列表.Any(p => p.一级分类 != "（地方政府专家库评审专业目录）" && !hylist.Contains(p.一级分类) && !p.二级分类.Any())));
            var count = zjlist.Count();

            var etext = "";
            var excount = 0;

            foreach (var zj in zjlist)
            {
                var zylist = zj.可参评物资类别列表;
                var newlist = new List<string>();
                foreach (var zy in zylist)
                {
                    if (zy.一级分类 != "（地方政府专家库评审专业目录）" && !hylist.Contains(zy.一级分类) && !zy.二级分类.Any())
                    {
                        newlist.Add(zy.一级分类);
                    }
                }
                if (newlist.Any())
                {
                    var zy_x = zylist.Where(o => o.一级分类 == "（地方政府专家库评审专业目录）");
                    if (zy_x.Any())
                    {
                        if (zy_x.Count() > 1)
                        {
                            etext += zj.Id.ToString() + "\r\n";
                        }
                        var zy_y = zy_x.First();
                        foreach (var newl in newlist)
                        {
                            if (!zy_y.二级分类.Contains(newl))
                            {
                                zy_y.二级分类.Add(newl);

                            }
                            zj.可参评物资类别列表.RemoveAll(o => o.一级分类 == newl);
                        }

                        zj.可参评物资类别列表.RemoveAll(o => o.一级分类 == "（地方政府专家库评审专业目录）");
                        zj.可参评物资类别列表.Add(zy_y);
                        用户管理.更新用户<专家>(zj, false);
                        excount++;

                    }
                    else
                    {
                        供应商._产品类别 lb = new 供应商._产品类别();
                        lb.一级分类 = "（地方政府专家库评审专业目录）";
                        lb.二级分类 = newlist;
                        zj.可参评物资类别列表.Add(lb);
                        foreach (var newl in newlist)
                        {
                            zj.可参评物资类别列表.RemoveAll(o => o.一级分类 == newl);
                        }
                        用户管理.更新用户<专家>(zj, false);
                        excount++;
                    }
                }
            }
            textBox1.Text = etext + "---------------------------------\r\n" + excount;
        }

        public class ysdclass
        {
            public int count;
            public decimal price;
        }

        private void Button_Click4(object sender, RoutedEventArgs e)
        {
            var ysdlist = 验收单管理.查询验收单(0, 0);
            var str = "";
            var d = 验收单单位列表信息.验收单单位列表;

            foreach (var item in d)
            {
                str += item.Id + "," + item.验收单名称 + "----------------------   \r\n";
                var list = 验收单管理.查询验收单(0, 0, Query<验收单>.Where(o => o.审核数据.审核者.用户ID == item.Id));
                foreach (var k in list)
                {
                    str += k.Id + "  " + k.管理单位审核人签名 + "\r\n";
                }
            }
            textBox1.Text = str;
            MessageBox.Show("OK");

            //var x = 单位用户.单位级别列表;
            //var str = "";
            //var user = 用户管理.查询用户<单位用户>(0, 0, null, false, SortBy<单位用户>.Ascending(o => o.Id), includeDeleted: true);
            //foreach (var u in user)
            //{
            //    str += u.Id + "\r\n";
            //}
            //textBox1.Text = str;

            //var count = 0;
            //count = 商品管理.查询商品(0, 0, Query<商品>.Where(o => o.采购信息.参与协议采购 && o.审核数据.审核状态 == 审核状态.审核通过)).Count();
            //var gyslist = 用户管理.查询用户<供应商>(0, 0, Query<供应商>.Where(o => o.供应商用户信息.协议供应商 && o.审核数据.审核状态 == 审核状态.审核通过 && o.所属地域.省份 != "四川省"));
            //foreach (var gys in gyslist)
            //{

            //    var splist = 商品管理.查询供应商商品(gys.Id, 0, 0, Query<商品>.Where(o => o.审核数据.审核状态 == 审核状态.审核通过));
            //    foreach (var sp in splist)
            //    {
            //        sp.采购信息.参与协议采购 = true;
            //        商品管理.更新商品(sp, false, false);
            //        count++;
            //    }
            //}


            //textBox1.Text = count.ToString()+"             "+gyslist.Count().ToString();
            //var p = 商品管理.计数商品(0, 0);
            //var prolist = 商品管理.查询商品(0, 0);
            //var x = 1;


            //int allcount = 0;
            //decimal allprice = 0;
            //var date1 = new DateTime(2015, 1, 1);
            //var date2 = new DateTime(2015, 6, 1);

            //var ysdlist = 验收单管理.查询验收单(0, 0, Query<验收单>.Where(o => o.是否作废 == false && o.基本数据.添加时间 >= date1 && o.基本数据.添加时间 < date2), includeDisabled: false);
            //Dictionary<string, ysdclass> dlist = new Dictionary<string, ysdclass>();
            //foreach (var ysd in ysdlist)
            //{
            //    allcount++;
            //    allprice += ysd.总金额;
            //    var unitname = ysd.管理单位审核人.用户数据.单位信息.单位名称;
            //    if (dlist.ContainsKey(unitname))
            //    {
            //        var d = dlist[unitname];
            //        d.count++;
            //        d.price += ysd.总金额;
            //    }
            //    else
            //    {
            //        ysdclass y = new ysdclass();
            //        y.count = 1;
            //        y.price = ysd.总金额;
            //        dlist.Add(unitname, y);
            //    }
            //    if (allcount == 101)
            //    {
            //        var m = true;
            //    }
            //}
            //var retstr = "单位名称--------------验收单数----------------总金额";
            //foreach (var dl in dlist)
            //{
            //    retstr += dl.Key + "--------------" + dl.Value.count + "----------------" + dl.Value.price + "\r\n";
            //}
            //retstr += "\r\n验收单总数：" + allcount + "--------------------总金额：" + allprice;
            //textBox1.Text = retstr;

        }
        private void Button_Click5(object sender, RoutedEventArgs e)
        {
            textBox1.Text = 工具.ImpColl<单位用户>(true).ToString();
            MessageBox.Show("导入完成!开始删除敏感字段！");
            var userlist = 用户管理.查询用户<单位用户>(0, 0, includeDeleted: false);
            foreach (var user in userlist)
            {
                user.所属地域 = new _地域();
                user.联系人职务 = string.Empty;
                user.联系方式 = new _联系方式();
                if (!string.IsNullOrWhiteSpace(user.单位信息.单位代号))
                {
                    user.单位信息.单位名称 = string.Empty;
                }
                用户管理.更新用户<单位用户>(user, false);
            }
            MessageBox.Show("删除敏感文字成功！");
            /*var ysd = Mongo.Coll<专家>().FindAs<BsonDocument>(Query.Exists("历史抽取信息"));
            foreach (var y in ysd)
            {
                var p = y["_id"].AsInt64;
                Mongo.Coll<专家>().Update(
                   Query.EQ("_id", y["_id"]),
                   Update.Unset("历史抽取信息"));
            }*/

            /*IEnumerable<公告> model = 公告管理.查询公告(0,0);公告添加撤回字段后
            foreach(var item in  model)
            {
                item.公告信息.是否撤回 = false;
                公告管理.更新公告(item);
            }*/
            //工具Data.列表资源(true);
        }
        private void Button_Click6(object sender, RoutedEventArgs e)
        {
            #region 修改验收单回传字段类型
            var ysd = Mongo.Coll<验收单>().FindAs<BsonDocument>(Query.Exists("验收单扫描件"));
            foreach (var y in ysd)
            {
                var kp = new List<BsonDocument>();
                var p = y["验收单扫描件"].AsBsonArray;
                foreach (var item in p)
                {
                    var po = new _回传信息();

                    if (!(item is BsonDocument))
                    {
                        po.回传单路径 = item.AsString;
                        kp.Add(po.ToBsonDocument());
                    }

                }
                Mongo.Coll<验收单>().Update(
                   Query.EQ("_id", y["_id"]),
                   Update.Set("验收单扫描件", new BsonArray(kp)));
            }
            #endregion

            #region 验收单添加回传单审核状态字段
            var ysdd = Mongo.Coll<验收单>().FindAs<BsonDocument>(Query.Exists("扫描件审核状态"));
            foreach (var h in ysdd)
            {
                Mongo.Coll<验收单>().Update(Query.EQ("_id", h["_id"]), Update.Unset("扫描件审核状态"));
            }
            #endregion


            //var str = "";
            //var splist = 商品分类管理.查找子分类();
            //foreach (var sp in splist)
            //{
            //    str += sp.分类名 + "\r\n";
            //    var sp_list = 商品分类管理.查找子分类(sp.Id);
            //    foreach (var s in sp_list)
            //    {
            //        str += "                 " + s.分类名 + "\r\n";
            //        var sp_list_t = 商品分类管理.查找子分类(s.Id);
            //        foreach (var s_t in sp_list_t)
            //        {
            //            str += "                                  " + s_t.分类名 + "\r\n";
            //        }
            //    }
            //}
            //textBox1.Text = str;


            //var str = "";
            //var userlist = 用户管理.查询用户<单位用户>(0, 0, null, includeDeleted: true);
            //userlist = userlist.OrderBy(o => o.管理单位.用户ID);
            //foreach (var user in userlist)
            //{
            //    str += user.Id + "                 " + user.单位信息.单位名称 + "                 " + user.单位信息.单位代号 + "              " + user.管理单位.用户ID + "\r\n";
            //}
            //textBox1.Text = str;
            //var ysd = 供应商增值服务申请记录管理.查询供应商增值服务申请记录(0, 0);
            //foreach (var y in ysd)
            //{
            //    y.开通个数 = 1;
            //    供应商增值服务申请记录管理.更新供应商增值服务申请记录(y, false);
            //}
        }
        private void Button_Click7(object sender, RoutedEventArgs e)
        {
            //验收单商品名折行计算
            var 物资列表 = new List<商品>();

            const int 每张验收单允许总行数 = 10;
            const int 每行允许的半角字符数 = 72;
            var pl1 = new List<商品[]>();
            var pl2 = new List<商品>();
            var 当前累计行数 = 每张验收单允许总行数;
            foreach (var p in 物资列表)
            {
                var pn = p.商品信息.商品名;
                var pnl = pn.Select(c => ((int)c) < 256).Count() + pn.Select(c => (int)c > 256).Count() * 2;
                var n = (pnl - pnl % 每行允许的半角字符数) / 每行允许的半角字符数;
                if (pnl % 每行允许的半角字符数 > 0) ++n;
                if (0 == n) n = 1;
                if (当前累计行数 + n > 每张验收单允许总行数)
                {
                    pl1.Add(pl2.ToArray());
                    pl2 = new List<商品> { p };
                    当前累计行数 = n;
                }
                else
                {
                    pl2.Add(p);
                    当前累计行数 += n;
                }
            }

            foreach (var pl in pl1)
            {
                //输出表头()
                foreach (var p in pl)
                {
                    //输出商品(p)
                }
                //输出表尾()
            }
        }

        private void Button_Click8(object sender, RoutedEventArgs e)
        {
            #region 修改商品中标字段类型
            //var sp = Mongo.Coll<商品>().FindAs<BsonDocument>(Query.Exists("中标项目编号"));
            //foreach (var item in sp)
            //{
            //    var KP = new List<BsonDocument>();
            //    var p = item["中标项目编号"].AsBsonArray;
            //    foreach (var im in p)
            //    {
            //        var jk = new 商品._中标信息();
            //        jk.中标项目编号 = im.AsString;
            //        KP.Add(jk.ToBsonDocument());
            //    }

            //    Mongo.Coll<商品>().Update(
            //      Query.EQ("_id", item["_id"]),
            //      Update.Combine(
            //      Update.Unset("中标项目编号"))
            //      );

            //    Mongo.Coll<商品>().Update(
            //       Query.EQ("_id", item["_id"]),
            //       Update.Set("中标信息", new BsonArray(KP)));
            //}
            #endregion

            #region 修改验收单回传字段类型
            //var ysd = Mongo.Coll<验收单>().FindAs<BsonDocument>(Query.Exists("验收单扫描件"));
            //foreach (var y in ysd)
            //{
            //    var kp = new List<BsonDocument>();
            //    var p = y["验收单扫描件"].AsBsonArray;
            //    foreach (var item in p)
            //    {
            //        var po = new _回传信息();

            //        if (!(item is BsonDocument))
            //        {
            //            po.回传单路径 = item.AsString;
            //            kp.Add(po.ToBsonDocument());
            //        }

            //    }
            //    Mongo.Coll<验收单>().Update(
            //       Query.EQ("_id", y["_id"]),
            //       Update.Set("验收单扫描件", new BsonArray(kp)));
            //}
            #endregion

            #region 验收单添加回传单审核状态字段
            //var ysdd = Mongo.Coll<验收单>().FindAs<BsonDocument>(Query.Exists("扫描件审核状态"));
            //foreach (var h in ysdd)
            //{
            //    Mongo.Coll<验收单>().Update(Query.EQ("_id", h["_id"]), Update.Unset("扫描件审核状态"));
            //}
            #endregion

            #region 修改专家证件字段类型
            var ysd = Mongo.Coll<专家>().FindAs<BsonDocument>(Query.Exists("学历信息.职称证书电子扫描件"));
            foreach (var y in ysd)
            {
                var p = y["学历信息"]["职称证书电子扫描件"];

                if (!(p is BsonNull))
                {
                    if (!(p is BsonArray))
                    {
                        Mongo.Coll<专家>().Update(
                       Query.EQ("_id", y["_id"]),
                       Update.Combine(
                       Update.Unset("学历信息.职称证书电子扫描件"))
                       );

                        Mongo.Coll<专家>().Update(
                       Query.EQ("_id", y["_id"]),
                       Update.Combine(
                       Update.Set("学历信息.职称证书电子扫描件", new BsonArray(new List<string> { p.AsString })))
                       );
                    }
                }
                else
                {
                    Mongo.Coll<专家>().Update(
                  Query.EQ("_id", y["_id"]),
                  Update.Combine(
                  Update.Unset("学历信息.职称证书电子扫描件"))
                  );
                    Mongo.Coll<专家>().Update(
                 Query.EQ("_id", y["_id"]),
                 Update.Combine(
                 Update.Set("学历信息.职称证书电子扫描件", new BsonArray(new List<string>())))
                 );
                }

            }

            ysd = Mongo.Coll<专家>().FindAs<BsonDocument>(Query.Exists("身份信息.证件电子扫描件"));
            foreach (var y in ysd)
            {
                var p = y["身份信息"]["证件电子扫描件"];

                if (!(p is BsonNull))
                {
                    Mongo.Coll<专家>().Update(
                   Query.EQ("_id", y["_id"]),
                   Update.Combine(
                   Update.Unset("身份信息.证件电子扫描件"))
                   );

                    Mongo.Coll<专家>().Update(
                   Query.EQ("_id", y["_id"]),
                   Update.Combine(
                   Update.Set("身份信息.专家证电子扫描件", new BsonArray(new List<string> { p.AsString })))
                   );
                }
                else
                {
                    Mongo.Coll<专家>().Update(
                  Query.EQ("_id", y["_id"]),
                  Update.Combine(
                  Update.Unset("身份信息.证件电子扫描件"))
                  );
                    Mongo.Coll<专家>().Update(
                 Query.EQ("_id", y["_id"]),
                 Update.Combine(
                 Update.Set("身份信息.专家证电子扫描件", new BsonArray(new List<string>())))
                 );
                }

            }
            #endregion

            #region 修改专家单位地址字段类型
            var ysd1 = Mongo.Coll<专家>().FindAs<BsonDocument>(Query.Exists("工作经历信息.单位地址"));
            foreach (var y in ysd1)
            {
                Mongo.Coll<专家>().Update(
                Query.EQ("_id", y["_id"]),
                Update.Combine(
                Update.Unset("工作经历信息.单位地址"))
                );

                Mongo.Coll<专家>().Update(
                Query.EQ("_id", y["_id"]),
                Update.Combine(
                Update.Set("工作经历信息.单位地址", new BsonString("")))
                );

            }
            #endregion
            #region 修改专家单位地址字段类型
            //var ysd = Mongo.Coll<商品>().FindAs<BsonDocument>(Query.Exists("商品信息.单位重量"));
            //foreach (var y in ysd)
            //{
            //    Mongo.Coll<商品>().Update(
            //    Query.EQ("_id", y["_id"]),
            //    Update.Combine(
            //    Update.Unset("商品信息.单位重量"))
            //    );

            //    Mongo.Coll<商品>().Update(
            //    Query.EQ("_id", y["_id"]),
            //    Update.Combine(
            //    Update.Set("商品信息.单位重量", new BsonDouble(0))
            //    ));

            //}
            #endregion

            #region 专家抽选数据准备
            //var ZJ = 用户管理.查询用户<专家>(0, 0);
            //foreach (var item in ZJ)
            //{
            //    var d = new 供应商._产品类别();
            //    d.一级分类 = "后勤装备";
            //    d.二级分类.Add("整车改装类装备");
            //    d.二级分类.Add("方舱制造类装备");
            //    d.二级分类.Add("机电设备制造类装备");
            //    d.二级分类.Add("工程机械制造类装备");
            //    d.二级分类.Add("管线制造类装备");
            //    d.二级分类.Add("箱组类装备");
            //    d.二级分类.Add("装具类装备");
            //    d.二级分类.Add("指挥控制类装备");
            //    d.二级分类.Add("营具类装备");
            //    d.二级分类.Add("医疗器械类装备");
            //    item.审核数据.审核状态 = 审核状态.审核通过;
            //    item.可参评物资类别列表.Add(d);
            //    用户管理.更新用户<专家>(item);
            //}
            #endregion
            MessageBox.Show("OK");
        }
        private void ButtonExpColl_Click(object sender, RoutedEventArgs e)
        {
            //textBox1.Text = 工具.ExpColl<办事指南>().ToString();
            textBox1.Text = 工具.ExpColl<单位用户>().ToString();
            //textBox1.Text = 工具.ExpColl<登录统计>().ToString();
            //textBox1.Text = 工具.ExpColl<广告点击统计>().ToString();
            //textBox1.Text = 工具.ExpColl<培训资料>().ToString();
            //textBox1.Text = 工具.ExpColl<通知>().ToString();
            //textBox1.Text = 工具.ExpColl<下载>().ToString();
            //textBox1.Text = 工具.ExpColl<政策法规>().ToString();
            //textBox1.Text = 工具.ExpColl<专家>().ToString();
            ////textBox1.Text = 工具.ExpColl<专家抽选记录>().ToString();
            //textBox1.Text = 工具.ExpColl<专家可评标专业>().ToString();
        }
        private void ButtonImpColl_Click(object sender, RoutedEventArgs e)
        {
            //textBox1.Text = 工具.ImpColl<办事指南>(true).ToString();
            textBox1.Text = 工具.ImpColl<单位用户>(true).ToString();
            //textBox1.Text = 工具.ImpColl<供应商>(true).ToString();
            //textBox1.Text = 工具.ImpColl<商品>(true).ToString();
            //textBox1.Text = 工具.ImpColl<登录统计>(true).ToString();
            //textBox1.Text = 工具.ImpColl<广告点击统计>(true).ToString();
            //textBox1.Text = 工具.ImpColl<培训资料>(true).ToString();
            //textBox1.Text = 工具.ImpColl<通知>(true).ToString();
            //textBox1.Text = 工具.ImpColl<下载>(true).ToString();
            //textBox1.Text = 工具.ImpColl<政策法规>(true).ToString();
            //textBox1.Text = 工具.ImpColl<专家>(true).ToString();
            //textBox1.Text = 工具.ImpColl<专家抽选记录>(true).ToString();
            //textBox1.Text = 工具.ImpColl<专家可评标专业>(true).ToString();
        }
        private void Button31_OnClick(object sender, RoutedEventArgs e)
        {
            var zjnamestr = "";
            var zjlist = 用户管理.查询用户<专家>(0, 0, includeDisabled: false);
            //foreach (var zj in zjlist)
            //{
            //    if (!string.IsNullOrWhiteSpace(zj.身份信息.姓名))
            //    {
            //        zj.身份信息.姓名 = zj.身份信息.姓名.Replace(" ", "").Replace(" ", "");
            //        用户管理.更新用户<专家>(zj, false);
            //        zjnamestr += zj.身份信息.姓名 + "          " + zj.身份信息.性别 + "          " + zj.身份信息.出生年月 + "          " +
            //                     zj.Id + "\r\n";
            //    }
            //    else
            //    {
            //        zjnamestr += "删除专家ID：" + zj.Id + "\r\n";
            //    }
            //}
            foreach(var item in zjlist)
            {
                string temp = "";
                foreach(var item1 in item.可参评物资类别列表)
                {
                    temp +=item1.一级分类+"|";
                }
                zjnamestr += item.身份信息.姓名 + "       " + item.身份信息.出生年月.ToString("yyyy-MM-dd") + "      " + item.学历信息.专业技术职称 + "      " + item.身份信息.专家级别 + "      " + item.联系方式.手机 + "      " + item.学历信息.毕业院校 + "      " +"可评标类别："+ temp +"\r\n";
            }
            textBox1.Text = zjnamestr;
        }
        private void Button32_OnClick(object sender, RoutedEventArgs e)
        {
        }
        private void Button33_OnClick(object sender, RoutedEventArgs e)
        {
        }
        private void Button34_OnClick(object sender, RoutedEventArgs e)
        {
            var p1 = new 专家可评标专业分类();
            p1.Id = -1;
            p1.分类名 = "后勤装备";
            p1.子分类.Add("整车改装类装备");
            p1.子分类.Add("方舱制造类装备");
            p1.子分类.Add("机电设备制造类装备");
            p1.子分类.Add("工程机械制造类装备");
            p1.子分类.Add("管线制造类装备");
            p1.子分类.Add("箱组类装备");
            p1.子分类.Add("装具类装备");
            p1.子分类.Add("指挥控制类装备");
            p1.子分类.Add("营具类装备");
            p1.子分类.Add("医疗器械类装备");
            if (-1 == p1.Id) p1.Id = Mongo.NextId<专家可评标专业分类>();
            Mongo.Coll<专家可评标专业分类>().Insert(p1);

            p1 = new 专家可评标专业分类();
            p1.Id = -1;
            p1.分类名 = "被装";
            p1.子分类.Add("大（卷）檐冒类");
            p1.子分类.Add("贝雷帽类");
            p1.子分类.Add("栽绒冒类");
            p1.子分类.Add("皮帽类");
            p1.子分类.Add("礼服类");
            p1.子分类.Add("毛料（仿毛）常服类");
            p1.子分类.Add("布料常服类");
            p1.子分类.Add("作训（雨衣）类");
            p1.子分类.Add("工作服装类");
            p1.子分类.Add("针织服装类");
            p1.子分类.Add("毛衣类");
            p1.子分类.Add("皮服类");
            p1.子分类.Add("领带类");
            p1.子分类.Add("皮鞋（靴）类");
            p1.子分类.Add("模压鞋（靴）类");
            p1.子分类.Add("毛料（仿毛）常服类");
            p1.子分类.Add("胶布鞋类");
            p1.子分类.Add("硫化鞋靴类");
            p1.子分类.Add("双密度鞋靴类");
            p1.子分类.Add("仪仗队马靴类");
            p1.子分类.Add("袜类");
            p1.子分类.Add("线手套类");
            p1.子分类.Add("皮革手套类");
            p1.子分类.Add("缝制手套类");
            p1.子分类.Add("被褥类");
            p1.子分类.Add("凉席类");
            p1.子分类.Add("毛毯类");
            p1.子分类.Add("毛巾被类");
            p1.子分类.Add("眼镜类");
            p1.子分类.Add("注塑装具类");
            p1.子分类.Add("拉伸装具类");
            p1.子分类.Add("编织（缝制、盖布）装具类");
            p1.子分类.Add("马装具类");
            p1.子分类.Add("军旗类");
            p1.子分类.Add("指挥刀类");
            p1.子分类.Add("金属服饰类");
            p1.子分类.Add("织唛服饰类");
            p1.子分类.Add("硬肩章类");
            p1.子分类.Add("资历章架类");
            p1.子分类.Add("洗涤设备类");
            p1.子分类.Add("机具类");
            p1.子分类.Add("对二氯苯类");
            if (-1 == p1.Id) p1.Id = Mongo.NextId<专家可评标专业分类>();
            Mongo.Coll<专家可评标专业分类>().Insert(p1);

            p1 = new 专家可评标专业分类();
            p1.Id = -1;
            p1.分类名 = "被装材料";
            p1.子分类.Add("精纺呢绒类");
            p1.子分类.Add("粗纺呢绒类");
            p1.子分类.Add("化纤仿毛类");
            p1.子分类.Add("混纺（里料）类");
            p1.子分类.Add("迷彩作训类");
            p1.子分类.Add("轧光涂层类");
            p1.子分类.Add("针织绒线类");
            p1.子分类.Add("针织布类");
            p1.子分类.Add("长毛绒类");
            p1.子分类.Add("服饰类");
            p1.子分类.Add("复合纱线类");
            p1.子分类.Add("鞋用材料类");
            p1.子分类.Add("保暖絮片类");
            p1.子分类.Add("衬布类");
            p1.子分类.Add("平剪绒类");
            p1.子分类.Add("拉链类");
            p1.子分类.Add("纽扣类");
            p1.子分类.Add("冒用材料类");
            p1.子分类.Add("其他配件类");
            if (-1 == p1.Id) p1.Id = Mongo.NextId<专家可评标专业分类>();
            Mongo.Coll<专家可评标专业分类>().Insert(p1);


            p1 = new 专家可评标专业分类();
            p1.Id = -1;
            p1.分类名 = "军用食品";
            p1.子分类.Add("野战食品");
            p1.子分类.Add("专用食品");
            p1.子分类.Add("罐头食品");
            p1.子分类.Add("奶粉");
            //p1.子分类.Add("其他");
            p1.子分类.Add("其他军用食品");
            if (-1 == p1.Id) p1.Id = Mongo.NextId<专家可评标专业分类>();
            Mongo.Coll<专家可评标专业分类>().Insert(p1);

            p1 = new 专家可评标专业分类();
            p1.Id = -1;
            p1.分类名 = "文体器材";
            p1.子分类.Add("乐器");
            p1.子分类.Add("球类");
            p1.子分类.Add("体育器材及配件");
            p1.子分类.Add("训练健身器材");
            p1.子分类.Add("运动防护用具");
            p1.子分类.Add("其他文体器材");
            if (-1 == p1.Id) p1.Id = Mongo.NextId<专家可评标专业分类>();
            Mongo.Coll<专家可评标专业分类>().Insert(p1);

            p1 = new 专家可评标专业分类();
            p1.Id = -1;
            p1.分类名 = "家具";
            p1.子分类.Add("木制家具");
            p1.子分类.Add("金属家具");
            p1.子分类.Add("其他家具");
            if (-1 == p1.Id) p1.Id = Mongo.NextId<专家可评标专业分类>();
            Mongo.Coll<专家可评标专业分类>().Insert(p1);

            p1 = new 专家可评标专业分类();
            p1.Id = -1;
            p1.分类名 = "办公设备";
            p1.子分类.Add("扫描、复印、打印、传真设备");
            p1.子分类.Add("销毁设备");
            p1.子分类.Add("幻灯及投影设备");
            p1.子分类.Add("照相、摄像机及器材");
            p1.子分类.Add("图书档案设备");
            p1.子分类.Add("制图设备");
            p1.子分类.Add("办公耗材");
            p1.子分类.Add("其他办公设备");
            if (-1 == p1.Id) p1.Id = Mongo.NextId<专家可评标专业分类>();
            Mongo.Coll<专家可评标专业分类>().Insert(p1);

            p1 = new 专家可评标专业分类();
            p1.Id = -1;
            p1.分类名 = "基建营房工程器材";
            p1.子分类.Add("野营住房器材");
            p1.子分类.Add("供电器材");
            p1.子分类.Add("供水器材");
            p1.子分类.Add("通风除湿设备");
            p1.子分类.Add("卫浴器材");
            p1.子分类.Add("供暖及制冷器材");
            p1.子分类.Add("机械零备件");
            p1.子分类.Add("营具");
            p1.子分类.Add("大型工程防护抢修设备");
            p1.子分类.Add("小型工程防护抢修机具器材");
            p1.子分类.Add("战场防护伪装器材");
            p1.子分类.Add("成套营房器材和国外引进器材");
            if (-1 == p1.Id) p1.Id = Mongo.NextId<专家可评标专业分类>();
            Mongo.Coll<专家可评标专业分类>().Insert(p1);


            p1 = new 专家可评标专业分类();
            p1.Id = -1;
            p1.分类名 = "军事交通器材";
            p1.子分类.Add("装卸搬运设备");
            p1.子分类.Add("装卸加固器材");
            p1.子分类.Add("运输保障器材");
            p1.子分类.Add("交通保障器材");
            p1.子分类.Add("指挥监控器材");
            p1.子分类.Add("检修检测器材");
            p1.子分类.Add("教学训练器材");
            if (-1 == p1.Id) p1.Id = Mongo.NextId<专家可评标专业分类>();
            Mongo.Coll<专家可评标专业分类>().Insert(p1);


            p1 = new 专家可评标专业分类();
            p1.Id = -1;
            p1.分类名 = "药品";
            p1.子分类.Add("抗微生物用药");
            p1.子分类.Add("抗寄生虫用药");
            p1.子分类.Add("中枢神经系统药物");
            p1.子分类.Add("神经系统药物");
            p1.子分类.Add("循环系统用药");
            p1.子分类.Add("呼吸系统用药");
            p1.子分类.Add("消化系统用药");
            p1.子分类.Add("泌尿系统用药");
            p1.子分类.Add("生殖系统及泌乳功能药物");
            p1.子分类.Add("影响血液及造血系统药物");
            p1.子分类.Add("激素及其有关药物");
            p1.子分类.Add("酶类及其他生化制剂");
            p1.子分类.Add("调节水、电解质及酸碱平衡药物");
            p1.子分类.Add("营养药");
            p1.子分类.Add("抗肿瘤药");
            p1.子分类.Add("影响机体免疫功能药物");
            p1.子分类.Add("减肥药");
            p1.子分类.Add("延缓衰老及相关老年病用药");
            p1.子分类.Add("其他外科、消毒防腐收敛、皮肤科、眼科、耳鼻喉科及口腔用药");
            p1.子分类.Add("解毒药、防治矽肺、防治放射病、药用附加剂、诊断用药、放射性药物、杀虫驱蚊液");
            p1.子分类.Add("生物制品");
            p1.子分类.Add("中成药");
            p1.子分类.Add("中药材");
            p1.子分类.Add("军队特需药品");
            if (-1 == p1.Id) p1.Id = Mongo.NextId<专家可评标专业分类>();
            Mongo.Coll<专家可评标专业分类>().Insert(p1);


            p1 = new 专家可评标专业分类();
            p1.Id = -1;
            p1.分类名 = "医用耗材";
            p1.子分类.Add("体外诊断试剂类");
            p1.子分类.Add("电生理类");
            p1.子分类.Add("射频消融类");
            p1.子分类.Add("口腔科耗材类");
            p1.子分类.Add("麻醉类耗材");
            p1.子分类.Add("手术室通用耗材类");
            p1.子分类.Add("消毒类耗材类");
            p1.子分类.Add("透析材料类");
            p1.子分类.Add("医学影像类");
            p1.子分类.Add("医用高分子材料类");
            p1.子分类.Add("卫生材料及敷料类");
            p1.子分类.Add("眼科耗材类");
            p1.子分类.Add("外科材料类");
            p1.子分类.Add("血管介入耗材类");
            p1.子分类.Add("非血管介入类");
            p1.子分类.Add("骨科耗材类");
            p1.子分类.Add("心脏起搏器类");
            if (-1 == p1.Id) p1.Id = Mongo.NextId<专家可评标专业分类>();
            Mongo.Coll<专家可评标专业分类>().Insert(p1);


            p1 = new 专家可评标专业分类();
            p1.Id = -1;
            p1.分类名 = "给养器材";
            p1.子分类.Add("主食加工机械");
            p1.子分类.Add("副食加工机械");
            p1.子分类.Add("包装机械");
            p1.子分类.Add("冷藏设备");
            p1.子分类.Add("分餐设备");
            p1.子分类.Add("炊事炉灶");
            p1.子分类.Add("食品安全检测设备");
            p1.子分类.Add("洗消设备");
            p1.子分类.Add("饮料设备");
            p1.子分类.Add("餐厨垃圾处理设备");
            p1.子分类.Add("普通给养器材");
            p1.子分类.Add("野战给养器材");
            p1.子分类.Add("特种给养器材");
            if (-1 == p1.Id) p1.Id = Mongo.NextId<专家可评标专业分类>();
            Mongo.Coll<专家可评标专业分类>().Insert(p1);

            p1 = new 专家可评标专业分类();
            p1.Id = -1;
            p1.分类名 = "军用粮油";
            p1.子分类.Add("花生油");
            p1.子分类.Add("菜籽油");
            p1.子分类.Add("大豆油");
            p1.子分类.Add("其他成品油脂");
            p1.子分类.Add("小麦粉");
            p1.子分类.Add("大米");
            p1.子分类.Add("其他成品粮");
            p1.子分类.Add("大豆");
            p1.子分类.Add("绿豆");
            p1.子分类.Add("马料");
            p1.子分类.Add("其他原粮");
            if (-1 == p1.Id) p1.Id = Mongo.NextId<专家可评标专业分类>();
            Mongo.Coll<专家可评标专业分类>().Insert(p1);

            p1 = new 专家可评标专业分类();
            p1.Id = -1;
            p1.分类名 = "仪器仪表";
            p1.子分类.Add("自动化仪表");
            p1.子分类.Add("电工仪器仪表");
            p1.子分类.Add("电子和通信测量仪器");
            p1.子分类.Add("光学仪器");
            p1.子分类.Add("分析仪器");
            if (-1 == p1.Id) p1.Id = Mongo.NextId<专家可评标专业分类>();
            Mongo.Coll<专家可评标专业分类>().Insert(p1);

            p1 = new 专家可评标专业分类();
            p1.Id = -1;
            p1.分类名 = "计算机及通信";
            p1.子分类.Add("计算机");
            p1.子分类.Add("计算机外网设备");
            p1.子分类.Add("服务器设备");
            p1.子分类.Add("网络设备");
            p1.子分类.Add("机房综合设备");
            p1.子分类.Add("存储容灾设备");
            p1.子分类.Add("网络安全防护设备");
            p1.子分类.Add("密码加密设备");
            p1.子分类.Add("基础通用软件");
            p1.子分类.Add("有线通信系统设备");
            p1.子分类.Add("无线通信系统设备");
            p1.子分类.Add("音视频相关设备");
            p1.子分类.Add("自动感知控制设备");
            p1.子分类.Add("安全防护检测设备");
            p1.子分类.Add("电视电话会议系统设备");
            p1.子分类.Add("其他设备");
            p1.子分类.Add("系统集成");
            if (-1 == p1.Id) p1.Id = Mongo.NextId<专家可评标专业分类>();
            Mongo.Coll<专家可评标专业分类>().Insert(p1);

            p1 = new 专家可评标专业分类();
            p1.Id = -1;
            p1.分类名 = "电气设备";
            p1.子分类.Add("汽、柴油发电设备");
            p1.子分类.Add("光伏风能发电设备");
            p1.子分类.Add("电源设备");
            p1.子分类.Add("输变电设备");
            p1.子分类.Add("配电设备");
            p1.子分类.Add("电线电缆");
            p1.子分类.Add("生活电器");
            p1.子分类.Add("照明器材");
            if (-1 == p1.Id) p1.Id = Mongo.NextId<专家可评标专业分类>();
            Mongo.Coll<专家可评标专业分类>().Insert(p1);


            p1 = new 专家可评标专业分类();
            p1.Id = -1;
            p1.分类名 = "车辆";
            p1.子分类.Add("乘用车辆");
            p1.子分类.Add("载货车辆");
            p1.子分类.Add("牵引车辆");
            p1.子分类.Add("挂车");
            p1.子分类.Add("特种车辆");
            p1.子分类.Add("车辆底盘");
            p1.子分类.Add("车辆附属设备、零部件及机工具");
            p1.子分类.Add("电动车辆");
            if (-1 == p1.Id) p1.Id = Mongo.NextId<专家可评标专业分类>();
            Mongo.Coll<专家可评标专业分类>().Insert(p1);


            p1 = new 专家可评标专业分类();
            p1.Id = -1;
            p1.分类名 = "机械设备";
            p1.子分类.Add("锅炉、辅机及附件");
            p1.子分类.Add("原动机");
            p1.子分类.Add("电梯");
            p1.子分类.Add("起重设备");
            p1.子分类.Add("输送设备");
            p1.子分类.Add("装卸设备");
            p1.子分类.Add("泵");
            p1.子分类.Add("阀门");
            p1.子分类.Add("风机");
            p1.子分类.Add("气体压缩机");
            p1.子分类.Add("制冷空调设备");
            p1.子分类.Add("空气净化设备");
            p1.子分类.Add("水净化设备");
            p1.子分类.Add("消防设备");
            p1.子分类.Add("工程机械");
            p1.子分类.Add("农业机械");
            p1.子分类.Add("印刷机械");
            p1.子分类.Add("仓储器材");
            if (-1 == p1.Id) p1.Id = Mongo.NextId<专家可评标专业分类>();
            Mongo.Coll<专家可评标专业分类>().Insert(p1);

            p1 = new 专家可评标专业分类();
            p1.Id = -1;
            p1.分类名 = "建筑装修材料";
            p1.子分类.Add("卫生洁具");
            p1.子分类.Add("磁砖");
            p1.子分类.Add("地板");
            p1.子分类.Add("涂料");
            //p1.子分类.Add("其他");
            p1.子分类.Add("其他建筑装修材料");
            if (-1 == p1.Id) p1.Id = Mongo.NextId<专家可评标专业分类>();
            Mongo.Coll<专家可评标专业分类>().Insert(p1);

            p1 = new 专家可评标专业分类();
            p1.Id = -1;
            p1.分类名 = "原材料";
            p1.子分类.Add("钢材");
            p1.子分类.Add("木材");
            p1.子分类.Add("水泥");
            p1.子分类.Add("煤炭");
            p1.子分类.Add("沥青及制品");
            p1.子分类.Add("橡胶制品");
            p1.子分类.Add("塑料制品");
            p1.子分类.Add("酸、碱");
            p1.子分类.Add("铜");
            p1.子分类.Add("铝");
            p1.子分类.Add("铅");
            p1.子分类.Add("锌");
            p1.子分类.Add("铜材");
            p1.子分类.Add("铝材");
            if (-1 == p1.Id) p1.Id = Mongo.NextId<专家可评标专业分类>();
            Mongo.Coll<专家可评标专业分类>().Insert(p1);

            p1 = new 专家可评标专业分类();
            p1.Id = -1;
            p1.分类名 = "医疗设备";
            p1.子分类.Add("眼科诊疗设备");
            p1.子分类.Add("耳鼻喉诊疗设备");
            p1.子分类.Add("中医诊疗设备");
            p1.子分类.Add("医用电子设备");
            p1.子分类.Add("康复理疗设备");
            p1.子分类.Add("病理设备");
            p1.子分类.Add("消毒供应设备");
            p1.子分类.Add("医疗设备维修及质控设备");
            p1.子分类.Add("护理设备");
            p1.子分类.Add("其他理疗设备");
            p1.子分类.Add("手术急救设备");
            p1.子分类.Add("医用内窥镜设备");
            p1.子分类.Add("口腔诊疗设备");
            p1.子分类.Add("检验设备");
            p1.子分类.Add("血液透析过滤设备");
            p1.子分类.Add("超声诊断设备");
            p1.子分类.Add("放射影像设备（含MRI）");
            p1.子分类.Add("放射治疗设备");
            p1.子分类.Add("核医学设备");
            if (-1 == p1.Id) p1.Id = Mongo.NextId<专家可评标专业分类>();
            Mongo.Coll<专家可评标专业分类>().Insert(p1);

            p1 = new 专家可评标专业分类();
            p1.Id = -1;
            p1.分类名 = "油料设备器材";
            p1.子分类.Add("油料储存设备器材");
            p1.子分类.Add("油料输转设备器材");
            p1.子分类.Add("油料收发设备器材");
            p1.子分类.Add("油料安全防护设备器材");
            p1.子分类.Add("油料化验仪器");
            p1.子分类.Add("油料检测设备器材");
            p1.子分类.Add("其他保障器材（含阀门、管件、线缆、胶管、等维修配件，油泵、便携式加油器）");
            if (-1 == p1.Id) p1.Id = Mongo.NextId<专家可评标专业分类>();
            Mongo.Coll<专家可评标专业分类>().Insert(p1);

            MessageBox.Show("成功！");

        }
        private void DataGrid1_OnAutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
        }
        private void DataGrid1_OnAutoGeneratedColumns(object sender, EventArgs e)
        {
            //var count1 = 0;
            //var count2 = 0;
            //var ysd = Mongo.Coll<验收单>().FindAs<BsonDocument>(Query.Exists("打印时间"));
            //foreach (var y in ysd)
            //{
            //    var plist = new List<BsonDocument>();
            //    var other = y["打印时间"];

            //    _打印信息 p = new _打印信息();
            //    p.打印时间 = (DateTime)other;
            //    plist.Add(p.ToBsonDocument());

            //    Mongo.Coll<验收单>().Update(
            //       Query.EQ("_id", y["_id"]),
            //        Update.Combine(
            //        Update.Set("打印信息", new BsonArray(plist)),
            //        Update.Unset("打印时间")
            //        )
            //       );
            //    count1++;
            //}
        }
    }
}



//////////////////////////////////////////////////////////////////////更新其他费用
//var ysd = Mongo.Coll<验收单>().FindAs<BsonDocument>(Query.Exists("其他费用"));
//            foreach (var y in ysd)
//            {
//                var p = decimal.Parse(y["其他费用"].AsString); 
//                 Mongo.Coll<验收单>().Update(
//                    Query.EQ("_id", y["_id"]),
//                    Update.Set("其他费用", new BsonDocument(new Dictionary<string,decimal>{{"其他费用",p}})));
//            }


//////////////////////////////////////////////////////////////设置通知的审核数据
//var l = Mongo.列表<通知>(0, 0, Fields.Include("_id"), Query.NotExists("审核数据"));
//           var count = 0;
//           foreach (var l1 in l)
//           {
//               Mongo.更新<通知>(
//                   Query<通知>.EQ(o => o.Id, l1["_id"].AsInt64),
//                   Update<通知>.Set(o => o.审核数据.审核状态, 审核状态.审核通过),
//                           updateModifiedTime: false
//                       );
//               ++count;
//           }
//           textBox1.Text = "通知：" + count.ToString();



///////////////////////////////////////////处理商品按地域查找字符串
//var retstr = "";
//var i = 0;
//var str = "js中的字符串";
//var province = str.Split(new char[] { '#' },StringSplitOptions.RemoveEmptyEntries);
//foreach(var pro in province){
//    var city = pro.Split(new char[]{'$'},StringSplitOptions.RemoveEmptyEntries);
//    retstr += "cityArr[" + i + "] = new Array(\"" + city[0] + "\",\"";
//    var city2= city[1];
//    var city3 = city2.Split(new char[]{'|'},StringSplitOptions.RemoveEmptyEntries);
//    foreach(var c in city3){
//        var city4 = c.Split(new char[]{','},StringSplitOptions.RemoveEmptyEntries);
//        if (city4[0] != "不限城市")
//        {
//            retstr += city4[0] + "|";
//        }
//    }
//    retstr = retstr.Substring(0, retstr.Length - 1);
//    retstr+="\");\r\n";
//    i++;
//}
//textBox1.Text=retstr;
///////////////////////////////////////////处理商品按地域查找字符串

//////////////////////////////////////////////////处理通知、新闻、政策法规审核状态
//var l = Mongo.列表<通知>(0, 0, Fields.Include("_id"), Query.NotExists("审核数据"));
//            var count = 0;
//            foreach (var l1 in l)
//            {
//                Mongo.更新<通知>(
//                    Query<通知>.EQ(o => o.Id, l1["_id"].AsInt64),
//                        Update<通知>.Set(
//                            o => o.审核数据.审核状态, 审核状态.审核通过),
//                            updateModifiedTime: false
//                        );
//                ++count;
//            }
//            textBox1.Text = count.ToString();
//////////////////////////////////////////////////处理通知、新闻、政策法规审核状态

////入库
//var l = Mongo.列表<供应商>(0, 0, Fields.Include("_id", "供应商用户信息.已入库"), Query.Exists("供应商用户信息.已入库"));
//var count = 0;
//foreach (var l1 in l)
//{
//    Mongo.更新<供应商>(
//        Query<供应商>.EQ(o => o.Id, l1["_id"].AsInt64),
//        Update.Combine(
//            Update<供应商>.Set(
//                o => o.供应商用户信息.入库级别,
//                l1["供应商用户信息"]["已入库"].AsBoolean
//                    ? 供应商.入库级别.入网供应商
//                    : 供应商.入库级别.未设置),
//            Update.Unset("供应商用户信息.已入库")
//            ), updateModifiedTime: false
//            );
//    ++count;
//}
//textBox1.Text = count.ToString();
//count = 0;
////点击率
//var rd = new Random();
//var gys = 用户管理.查询用户<供应商>(0, 0, Query<供应商>.EQ(o => o.审核数据.审核状态, 审核状态.审核通过));
//foreach (var g in gys)
//{
//    var hit = false;
//    if (g.供应商用户信息.内网点击量 <=50)
//    {
//        g.供应商用户信息.内网点击量 = rd.Next(30,55);
//        hit = true;
//    }
//    if (g.供应商用户信息.点击量  <=50)
//    {
//        g.供应商用户信息.点击量 = rd.Next(30, 55);
//        hit = true;
//    }
//    if (hit)
//    {
//    Mongo.更新<供应商>(
//        Query<供应商>.EQ(o => o.Id, g.Id),
//        Update.Combine(
//                Update<供应商>.Set(o => o.供应商用户信息.内网点击量, g.供应商用户信息.内网点击量),
//        Update<供应商>.Set(o => o.供应商用户信息.点击量, g.供应商用户信息.点击量)
//                ), updateModifiedTime: false);
//        ++count;
//}
//}
//textBox1.Text += "\r\n"+count;
//count = 0;

//var sp =商品管理.查询商品(0, 0, Query<商品>.EQ(o => o.审核数据.审核状态, 审核状态.审核通过));
//foreach (var s in sp)
//{
//    var hit = false;
//    if (s.销售信息.内网点击量 <= 50)
//    {
//        s.销售信息.内网点击量 = rd.Next(30, 55);
//        hit = true;
//    }
//    if (s.销售信息.点击量 <= 50)
//    {
//        s.销售信息.点击量 = rd.Next(30, 55);
//        hit = true;
//    }
//    if (hit)
//    {
//    Mongo.更新<商品>(
//        Query<商品>.EQ(o => o.Id, s.Id),
//        Update.Combine(
//        Update<商品>.Set(o => o.销售信息.内网点击量, s.销售信息.内网点击量),
//        Update<商品>.Set(o => o.销售信息.点击量, s.销售信息.点击量)
//                ), updateModifiedTime: false);
//        ++count;
//    }
//}
//textBox1.Text += "\r\n" + count;
//textBox1.Text += "\r\nold id: "+添加固定单位用户();


//添加测试数据();
//运营团队 u1 = new 运营团队();
//u1.登录信息.登录名 = "rootadmin";
//u1.登录信息.密码 = "123456";
//用户管理.添加用户(u1);

//单位用户 u2 = new 单位用户();
//u2.登录信息.登录名 = "cdjqlqb";
//u2.登录信息.密码 = "123456";
//u2.单位信息.单位级别 = 单位用户.单位级别.军区级;
//u2.单位信息.单位职能 = 单位用户.单位职能.管理 | 单位用户.单位职能.监审 | 单位用户.单位职能.采购 | 单位用户.单位职能.需求;
//用户管理.添加用户(u2);

//Mongo.Coll<单位用户>().Drop();
//添加固定单位用户();
//添加分类数据();
//运营团队 u1 = new 运营团队();
//u1.登录信息.登录名 = "rootadmin";
//u1.登录信息.密码 = "123456";
//用户管理.添加用户(u1);

//单位用户 u2 = new 单位用户();
//u2.登录信息.登录名 = "cdjqlqb";
//u2.登录信息.密码 = "123456";
//u2.单位信息.单位级别 = 单位用户.单位级别.军区级;
//u2.单位信息.单位职能 = 单位用户.单位职能.管理 | 单位用户.单位职能.监审 | 单位用户.单位职能.采购 | 单位用户.单位职能.需求;
//用户管理.添加用户(u2);
//添加专家数据(100);
//添加固定单位用户();
//var k = 用户管理.查找用户<单位用户>(3);
//Mongo.物理删除<单位用户>(3);
//k.Id = 11;
//Mongo.物理删除<单位用户>(11);
//用户管理.添加用户(k);
//foreach (var p in 商品管理.查询商品(0,0))
//{
//    if (null == p.销售信息.价格属性组合)
//    {
//        p.销售信息.价格属性组合 = new 商品._价格属性组合();
//        商品管理.更新商品(p);
//    }
//}
//foreach (var p in Mongo.查询<商品历史销售信息>(0, 0))
//{
//    if (null == p.价格属性组合)
//    {
//        p.价格属性组合 = new 商品._价格属性组合();
//        Mongo.更新<商品历史销售信息>(p);
//    }
//}
//var c = 商品分类管理.查找分类("专用物资");
//foreach (var c1 in c.子分类)
//{
//    c1.分类性质 = 商品分类性质.专用物资;
//    c1.父分类.商品分类ID = -1;
//    商品分类管理.更新分类(c1);
//    foreach (var c2 in c1.子分类)
//    {
//        c2.分类性质 = 商品分类性质.专用物资;
//        商品分类管理.更新分类(c2);
//    }
//}
//商品分类管理.删除分类(c.Id);
//var l = 商品管理.查询商品(0, 0,
//    Query.And(
//    Query<商品>.EQ(o => o.审核数据.审核状态, 审核状态.审核通过),
//    Query<商品>.NE(o=>o.销售信息.价格属性组合, null)
//    ), includeDisabled: false);
//foreach (var p in l)
//{
//    添加商品价格历史数据(p.Id);
//}
//添加固定单位用户();

//Mongo.Coll<公告>().Update(null, Update.Unset("行业"), UpdateFlags.Multi);
//Mongo.Coll<公告>().Update(null, Update.Unset("二级细分类别"), UpdateFlags.Multi);
//Mongo.Coll<公告>().Update(null, Update.Unset("三级细分类别"), UpdateFlags.Multi);

//Mongo.Coll<专家>().Update(null, Update.Rename("单位信息.单位显示名", "单位信息.单位代号"), UpdateFlags.Multi);

//var q = Query.SizeGreaterThan("出资人或股东信息", 0);
//var u = Mongo.Coll<供应商>().FindAs<BsonDocument>(q).ToArray();
//foreach (var item in u)
//{
//    var k = item["出资人或股东信息"].AsBsonArray;
//    for (int i = 0; i < k.Count; ++i)
//    {
//        k[i].AsBsonDocument.Remove("所属供应商");
//    }
//    Mongo.Coll<供应商>().Update(Query.EQ("_id", item["_id"]), Update.Replace(item));
//}
//textBox1.Text = u.Length.ToString();


//添加专家数据(100);
//var u = new 供应商();
//u.登录信息.登录名 = "test1";
//u.登录信息.密码 = "123456";
//u.供应商用户信息.认证数据[供应商.认证级别.待审核用户.ToString()] = new 操作数据();
//用户管理.添加用户(u);
//用户管理.认证供应商(u.Id, -1, 供应商.认证级别.待审核用户);
//用户管理.认证供应商(u.Id, -1, 供应商.认证级别.认证服务用户);
//var u = Mongo.查询<专家>(0, 0).ToArray();
//Mongo.Coll<专家>().RemoveAll();
//foreach (var item in u)
//{
//    item.Id += 300000000000;
//    item.登录信息.登录名 = item.Id.ToString();
//    用户管理.添加用户<专家>(item);
//}
//Mongo.Coll<专家>().Update(null, Update.Rename("所在地区", "所属地域"), UpdateFlags.Multi);
//Mongo.Coll<专家>().Update(null, Update.Rename("单位信息.单位显示名", "单位信息.单位代号"), UpdateFlags.Multi);
//Mongo.Coll<供应商>().Update(null, Update.Rename("所属地域", "企业基本信息"), UpdateFlags.Multi);
//textBox1.Text = Mongo.Coll<供应商>().Update(Query.Exists("供应商用户信息.年检列表"), Update.Unset("供应商用户信息.年检列表"), UpdateFlags.Multi).DocumentsAffected.ToString();
//Mongo.Coll<专家>().Update(null, Update.Unset("联系方式"), UpdateFlags.Multi);
//Mongo.Coll<专家>().Update(null, Update.Unset("地址信息"), UpdateFlags.Multi);
//添加基本用户();
//添加专家数据(136);
//Mongo.Coll<用户基本数据>().Update(Query.Null, Update.Unset("地址信息.邮编"),UpdateFlags.Multi);
//for (_iCount = 40;_iCount>=0; _iCount+=10)
//{
//    textBox1.Text = _iCount.ToString();
//    SetSeal1(_iCount);
//    Dispatcher.CurrentDispatcher.Invoke(DispatcherPriority.Background, new Action(() => { }));
//    SetSeal2(_iCount);
//    Dispatcher.CurrentDispatcher.Invoke(DispatcherPriority.Background, new Action(() => { }));
//}

//添加商品价格历史数据(296);
//添加测试机票();
//添加商品价格历史数据(296);
//SetSeal2();
//var l = 用户管理.查询用户<供应商>(0, 0);
//foreach (var g in l)
//{
//    switch (g.供应商用户信息.认证级别)
//    {
//        case 供应商.认证级别.免费用户://0
//            g.审核数据.审核状态 = 审核状态.未审核;
//            Mongo.更新(g, false);
//            break;

//        case 供应商.认证级别.普通收费用户://2
//        case 供应商.认证级别.CA证书用户:
//        case 供应商.认证级别.认证服务用户:
//        case 供应商.认证级别.金融服务用户:
//            g.审核数据.审核状态 = 审核状态.审核通过;
//            g.审核数据.审核时间 = DateTime.Now;
//            g.审核数据.审核者.用户ID = 100000000001;
//            Mongo.更新(g, false);
//            break;
//    }
//}
//var tst = "计算机"; //只有2个

//var q1 = Query<专家>.Where(o => o.可参评物资类别列表.Any(oc => oc.一级分类.Contains(tst)));
//var q2 = Query.Where(
//    "function(){" +
//    " for (var k1 in obj.可参评物资类别列表)" +
//    " for (var k2 in obj.可参评物资类别列表[k1].二级分类)" +
//    " if (obj.可参评物资类别列表[k1].二级分类[k2].indexOf('" + tst + "') != -1) return true;" +
//    " return false;" +
//    " }");
//var q = q1.Or(q2);

//var count = 用户管理.计数用户<专家>(0, 0, q);



//var infolist = 用户管理.列表用户<专家>(0, 0, Fields<专家>.Include(
//    o => o.身份信息.姓名,
//    o => o.身份信息.性别,
//    o => o.工作经历信息.工作单位,
//    //o => o.可参评物资类别列表,
//    o => o.联系方式.手机, o => o.联系方式.固定电话
//        //...
//        //...
//    ), q);

//var sl = 商品管理.查询商品(0, 0);
//var nc = 0;
//foreach (var sp in sl)
//{
//    var up = 0;
//    var g = sp.商品信息.所属供应商.用户数据;
//    if(null == g){++nc; continue;}
//    if (g.供应商用户信息.协议供应商)
//    {
//        sp.采购信息.参与协议采购 = true;
//        ++up;
//    }
//    if (g.供应商用户信息.应急供应商)
//    {
//        sp.采购信息.参与应急采购 = true;
//        ++up;
//    }
//    if (0 != up) 商品管理.更新商品(sp, false, false);
//}
//textBox1.Text = nc.ToString();
//添加固定单位用户();
//var fn = 工具.ChooseSaveFile();
//if(null==fn)return;
//File.WriteAllText(fn, 用户组管理.查询用户组(0, 0).ToArray().ToJson(), Encoding.UTF8);
//textBox1.Text += 工具.ExpColl<用户组>();
//textBox1.Text += 工具.ExpColl<专家可评标专业>();
//textBox1.Text += 工具.ExpColl<专家>();
//textBox1.Text += 工具.ExpColl<用户组>();

//textBox1.Text += 工具.ImpColl<用户组>() + "\r\n"; Mongo.NextIdSetTo<用户组>(5);
//textBox1.Text += 工具.ImpColl<单位用户>() + "\r\n"; Mongo.NextIdSetTo<单位用户>(10003);
//textBox1.Text += 工具.ImpColl<专家>() + "\r\n"; Mongo.NextIdSetTo<专家>(用户管理.专家Id基数+697);
//textBox1.Text += 工具.ImpColl<专家可评标专业>() + "\r\n";
//textBox1.Text += 工具.ImpColl<用户组>();
//textBox1.Text += 工具.ImpColl<单位用户>();
//(sender as Button).Content = "导入";
////工具.添加专家可评标类别();
//Mongo.Coll<专家>().Drop();
//var zjc = (工具.导入1(true) +
//           工具.导入2(true) +
//           工具.导入3(true));
//textBox1.Text = "导入专家 " + zjc;
//if (!WebApiApplication.IsIntranet)
//textBox1.Text += 工具.模糊专家信息();

//var pcas = 工具.省市区县表.读省市区县表();
//textBox1.Text = 工具.省市区县表.JS数据(pcas);

//textBox1.Text = 工具.省市区县JSON(pcas);
//textBox1.Text = Helpers.省市区县代码表.pcas.ToJson();

//var l = 用户组管理.查询用户组(0, 0).ToArray().ToJson();
//textBox1.Text = l;
//return;
//var tst = "设备";
//var q1 = Query<专家>.Where(o => o.可参评物资类别列表.Any(oc => oc.一级分类.Contains(tst)));
////var q2 = Query<专家>.Where(o => o.可参评物资类别列表.Any(oc => oc.二级分类.Contains(tst)));
////q2.And();
//var q2 = Query.Where("function(){ for (var k1 in obj.可参评物资类别列表) for (var k2 in obj.可参评物资类别列表[k1]) if (obj.可参评物资类别列表[k1][k2].indexOf('设备') != -1) return true; return false; }");
//var q = q1.Or(q2);
//var c = 用户管理.计数用户<专家>(0, 0, q);
//textBox1.Text = c.ToString();

//var q = Query<专家>.ElemMatch(oc=>oc.可参评物资类别列表, builder => builder.Matches())

//using (var zip = new Ionic.Zip.ZipFile
//{
//    AlternateEncoding = Encoding.UTF8,
//    AlternateEncodingUsage = ZipOption.Always
//})
//{
//    zip.AddFile(@"E:\1.txt", "招标公告1").FileName = "招标公告2\\2.txt";
//    zip.AddFile(@"E:\1.txt", "招标公告1\\附件").FileName = "招标公告2\\附件\\3.txt";
//    zip.Save(@"E:\1.zip");
//}

//var l = Enum.GetValues(typeof (AQ))
//    .Cast<AQ>()
//    .GroupBy(g => (int) g - (int) g%1000)
//    .Select(g => g.ToArray())
//    .ToDictionary(g => g[0]/*.ToString()*/, g =>
//        100 != (int) g[1] - (int) g[0]
//            ? g.Skip(1).ToArray()
//            : (object) g.Skip(1)
//                .GroupBy(gg => (int) gg - (int) gg%100)
//                .Select(gg => gg.ToArray())
//                .ToDictionary(gg => gg[0]/*.ToString()*/, gg => gg.Skip(1).ToArray()));
//textBox1.Text = JsonConvert.SerializeObject(l);


//using (var zip = new Ionic.Zip.ZipFile
//{
//    AlternateEncoding = Encoding.UTF8,
//    AlternateEncodingUsage = ZipOption.Always
//})
//{
//    zip.AddFile(@"E:\1.txt", "招标公告1").FileName = "招标公告2\\2.txt";
//    zip.AddFile(@"E:\1.txt", "招标公告1\\附件").FileName = "招标公告2\\附件\\3.txt";
//    zip.Save(@"E:\1.zip");
//}


//工具.添加专家可评标类别();
//var p = 商品管理.查找商品(254).商品数据.商品属性;
//var p = new Dictionary<string, Dictionary<string, List<string>>>();
//p["{"] = new Dictionary<string, List<string>> {{"@##<>\"~|", new List<string> {"@##<>\"~|"}}};
//var j = JsonConvert.SerializeObject(p);
//var n = p.ToJson();
//var k = JsonConvert.DeserializeObject<Dictionary<string,Dictionary<string, List<string>>>>(j);
//var l = BsonSerializer.Deserialize<Dictionary<string, Dictionary<string, List<string>>>>(BsonDocument.Parse(n));

//textBox1.Text =
//    (k.ToJson() == l.ToJson()) +
//    "\r\n----------------------------------------\r\n" +
//    "\r\n----------------------------------------\r\n" +
//    k.ToJson() +
//    "\r\n----------------------------------------\r\n" +
//    l.ToJson()
//    ;
//添加测试商品(2729, 8);

////////////////////////////////////////////////////decimal c#排序
//var gys = 用户管理.查询用户<供应商>(0, 0).ToList();
//gys.Sort((_1, _2) =>
//    _1.固定可订阅数 > _2.固定可订阅数 ? 1
//    : _1.固定可订阅数 < _2.固定可订阅数 ? -1
//    : 0
//    );
////////////////////////////////////////////////////decimal c#排序

////////////////////////////////////////////////////////////更新供应商所属行业--可提供商品类别的一级加;构成
//var t1 = 0;
//            var t2 = 0;
//            var gys = 用户管理.查询用户<供应商>(0, 0);
//            foreach (var g in gys)
//            {
//                if (g.可提供产品类别列表.Any())
//                {
//                    var cla = "";
//                    foreach (var c in g.可提供产品类别列表)
//                    {
//                        cla += c.一级分类 + ";";
//                    }
//                    g.企业基本信息.所属行业 = cla;
//                    t1++;
//                }
//                else
//                {
//                    g.企业基本信息.所属行业 = null;
//                    t2++;
//                }
//                用户管理.更新用户<供应商>(g, false);
//            }
//            this.textBox1.Text = t1 + "\r\n" + t2;
////////////////////////////////////////////////////////////更新供应商所属行业--可提供商品类别的一级加;构成

///////解压ZIP
//using (ZipFile zip = ZipFile.Read(@"E:\\20141211-114411.zip"))
//{
//    foreach (ZipEntry entry in zip)
//    {
//        //Extract解压zip文件包的方法，参数是保存解压后文件的路基
//        entry.Extract(@"E:\\Test");
//    }
//}
///////解压ZIP

////////////////////////////////////////////////////////////////////////////////////////更新商品分类的价格区间
//var 商品分类 = 商品分类管理.查询商品分类(0, 0).Where(o => o.子分类.Count() == 0);
//            var 返回字符串文本 = "";
//            foreach (var 商品分类1 in 商品分类)
//            {
//                var 分类下商品集合 = 商品管理.查询分类下商品(商品分类1.Id, 0, 0, Query<商品>.EQ(o => o.审核数据.审核状态, 审核状态.审核通过)).OrderBy(m => m.销售信息.价格);
//                var 分类下商品总数 = 分类下商品集合.Count();

//                if (分类下商品总数 >= 30)
//                {
//                    var 价格分段 = new List<decimal>();
//                    var tempprice = 0M;
//                    var nextprice = 0M;

//                    var max = 分类下商品集合.Max(o => o.销售信息.价格);
//                    var min = 分类下商品集合.Min(o => o.销售信息.价格);

//                    var 价格 = "Min = " + min.ToString();
//                    价格 += "       Max = " + max.ToString() + "       分段价格：";

//                    var avgnum = 分类下商品总数 / 6;
//                    for (var i = avgnum; i < 分类下商品总数; i += avgnum)
//                    {
//                        var tempnextprice = 0M;
//                        var thisprice_p = 分类下商品集合.ToList()[i].销售信息.价格;
//                        if (thisprice_p > 100)
//                        {
//                            var x = thisprice_p % 100;
//                            if (x > 50)
//                            {
//                                thisprice_p = thisprice_p - x - 1 + 100;
//                            }
//                            else
//                            {
//                                thisprice_p = thisprice_p - x - 1;
//                            }
//                            tempnextprice = thisprice_p + 1;
//                        }
//                        else if (thisprice_p > 10)
//                        {
//                            var x = thisprice_p % 10;
//                            if (x > 5)
//                            {
//                                thisprice_p = thisprice_p - x - 1 + 10;
//                            }
//                            else
//                            {
//                                thisprice_p = thisprice_p - x - 1;
//                            }
//                            tempnextprice = thisprice_p + 1;
//                        }
//                        else if (thisprice_p > 1)
//                        {
//                            thisprice_p = thisprice_p - thisprice_p % 1 - 0.1M;
//                            tempnextprice = thisprice_p + 0.1M;
//                        }
//                        else
//                        {
//                            thisprice_p = 0.9M;
//                            tempnextprice = 1;
//                        }
//                        if (tempprice != thisprice_p)
//                        {
//                            tempprice = thisprice_p;
//                            价格 += i.ToString() + "：" + nextprice.ToString("g0") + "--" + thisprice_p.ToString("g0") + "  ，";
//                            价格分段.Add(nextprice);
//                            价格分段.Add(thisprice_p);
//                            nextprice = tempnextprice;
//                        }
//                    }
//                    价格 += "最后一段：" + nextprice.ToString("g0") + "以上";
//                    返回字符串文本 += 商品分类1.分类名 + "：---------------" + 分类下商品总数 + "-------------" + 价格 + "\r\n";
//                    价格分段.Add(nextprice);
//                    价格分段.Add(decimal.MaxValue);
//                    Mongo.更新<商品分类>(
//                                Query<商品分类>.EQ(o => o.Id, 商品分类1.Id),
//                                Update<商品分类>.Set(o => o.价格分段, 价格分段),
//                                        updateModifiedTime: false
//                                    );
//                }
//            }
//            textBox1.Text = 返回字符串文本;
////////////////////////////////////////////////////////////////////////////////////////更新商品分类的价格区间

//转换打印时间为类
//var count1 = 0;
//var count2 = 0;
//var ysd = Mongo.Coll<验收单>().FindAs<BsonDocument>(Query.Exists("打印时间"));
//foreach (var y in ysd)
//{
//    var plist = new List<BsonDocument>();
//    var other = y["打印时间"];

//    _打印信息 p = new _打印信息();
//    p.打印时间 = (DateTime)other;
//    plist.Add(p.ToBsonDocument());

//    Mongo.Coll<验收单>().Update(
//       Query.EQ("_id", y["_id"]),
//        Update.Combine(
//        Update.Set("打印信息", new BsonArray(plist)),
//        Update.Unset("打印时间")
//        )
//       );
//    count1++;
//}

//ysd = Mongo.Coll<验收单>().FindAs<BsonDocument>(Query.NotExists("打印信息"));
//foreach (var y in ysd)
//{
//    Mongo.Coll<验收单>().Update(
//       Query.EQ("_id", y["_id"]),
//        Update.Combine(
//        Update.Set("是否已经打印",false)
//        )
//       );
//    count2++;
//}
//this.textBox1.Text = count1.ToString() + "\r\n" + count2.ToString();
//转换打印时间为类

//商品价格分段查询--------------decimal条件语句
//var m1 = 2399M;
//var m2 = 2999M;
//var m1s = m1.ToString("#.##").Split(".".ToCharArray());
//var m2s = m2.ToString("#.##").Split(".".ToCharArray());
//var q = Query<商品>.EQ(o => o.审核数据.审核状态, 审核状态.审核通过)
//    .And(Query.Where(
//    "function(){" +
//    "var p = parseInt(obj.销售信息.价格.split('.')[0]);" +
//    "return p >= " + m1s[0] + " && p <= " + m2s[0] + ";" +
//    "}"))
//    ;
//var p0 = 商品管理.查询分类下商品(666, 0, 0, q).Select(o => o.销售信息.价格).ToArray();
//var p1 = Mongo.Coll<商品>().Find(q).Select(o => o.销售信息.价格).ToArray();
//商品价格分段查询--------------decimal条件语句

//批量导入商品测试
//var binData = TemplateExcel.xls.商品分类(1120L,666L);
//var fn = System.IO.Path.ChangeExtension(System.IO.Path.GetTempFileName(), "xls");
//System.IO.File.WriteAllBytes(fn, binData);
//Process.Start(fn);

//var z = new ZipFile();
//var ImgExts = new[] {"jpg", "jpeg", "png", "gif"};
//var count = z.Entries.Count(zf =>
//    !zf.IsDirectory
//    && zf.FileName.StartsWith("dirName" + "\\")
//    && ImgExts.Contains(System.IO.Path.GetExtension(zf.FileName))
//    );
////var
//    fn = 工具.ChooseOpenFile();
//if(null == fn) return;
//var data = TemplateExcel.读取商品(fn, 789);
//textBox1.Text = data.Count.ToString();

////转换其他费用为类
//var ysd = Mongo.Coll<验收单>().FindAs<BsonDocument>(Query.Exists("其他费用"));
//foreach (var y in ysd)
//{
//    var otherlist = new List<BsonDocument>();
//    if (y["其他费用"].IsBsonArray) continue;
//    var other = y["其他费用"].AsBsonDocument;
//    foreach (var o in other)
//    {
//        _其他费用 q = new _其他费用();
//        q.费用名称 = o.Name;
//        q.金额 = decimal.Parse(o.Value.AsString);
//        otherlist.Add(q.ToBsonDocument());
//    }

//    Mongo.Coll<验收单>().Update(
//       Query.EQ("_id", y["_id"]),
//       Update.Set("其他费用", new BsonArray(otherlist)));
//}

//var spclasslist = 商品分类管理.查询商品分类(0, 0);
//foreach (var spclass in spclasslist)
//{
//    if (spclass.分类名.Contains("其他"))
//    {
//        spclass.序号 = 999999999;
//    }
//    else
//    {
//        spclass.序号 = 989999999;
//        if (spclass.分类名 == "办公设备")
//        {
//            spclass.序号 = 799999999;
//        }
//        if (spclass.分类名 == "文体器材")
//        {
//            spclass.序号 = 809999999;
//        }
//        if (spclass.分类名 == "计算机及通信")
//        {
//            spclass.序号 = 819999999;
//        }
//        if (spclass.分类名 == "电气设备")
//        {
//            spclass.序号 = 829999999;
//        }
//        if (spclass.分类名 == "机械设备")
//        {
//            spclass.序号 = 839999999;
//        }
//        if (spclass.分类名 == "家具")
//        {
//            spclass.序号 = 849999999;
//        }
//        if (spclass.分类名 == "仪器仪表")
//        {
//            spclass.序号 = 859999999;
//        }
//        if (spclass.分类名 == "原材料")
//        {
//            spclass.序号 = 869999999;
//        }
//        if (spclass.分类名 == "建筑装修材料")
//        {
//            spclass.序号 = 879999999;
//        }
//        if (spclass.分类名 == "车辆")
//        {
//            spclass.序号 = 889999999;
//        }
//        if (spclass.分类名 == "专用物质")
//        {
//            spclass.序号 = 899999999;
//        }

//    }
//    商品分类管理.更新分类(spclass,false);
//}

//var str = "";
//var gys = 商品管理.查询商品(0,0,Query<商品>.Where(o=>o.基本数据.已屏蔽==true && o.基本数据.已删除 == false),includeDisabled:true, includeDeleted:true);
//str += gys.Count()+"\r\n";
//foreach (var g in gys)
//{
//    str+=g.Id+"-----"+g.商品信息.商品名+"----"+g.商品信息.所属供应商.用户数据==null? "已删除":g.商品信息.所属供应商.用户数据.企业基本信息.企业名称+"\r\n";
//}
//textBox1.Text=str;
//var gys = 用户管理.查询用户<供应商>(0, 0, Query<供应商>.EQ(o => o.审核数据.审核状态, 审核状态.审核通过));
//foreach (var g in gys)
//{
//    g.供应商用户信息.符合入库标准 = true;
//    用户管理.更新用户<供应商>(g, false);
//}
//var u = 用户管理.查找用户(10003);
//u.联系方式.手机 = "123";
//用户管理.更新用户(u,false);


//var gyslist1 = 用户管理.查询用户<供应商>(0, 0, Query<供应商>.Where(o => o.可提供产品类别列表.Any(oc => oc.一级分类 == "其他")));
//var gyslist2 = 用户管理.查询用户<供应商>(0, 0, Query<供应商>.Where(o => o.可提供产品类别列表.Any(oc => oc.二级分类.Contains("其他"))));
//var retstr = "一级分类包含其他的供应商：" + gyslist1 .Count().ToString()+ "个\r\n";
//foreach (var gys in gyslist1)
//{
//    retstr += gys.Id.ToString() + "--------" + gys.企业基本信息.企业名称 + "--------" + gys.Id + "--------" + gys.审核数据.审核状态 + "\r\n";
//}
//retstr += "\r\n\r\n\r\n";
//retstr += "二级分类包含其他的供应商：" + gyslist2.Count().ToString() + "个\r\n";
//foreach (var gys in gyslist2)
//{
//    retstr += gys.Id.ToString() + "--------" + gys.企业基本信息.企业名称 + "--------" + gys.Id + "--------" + gys.审核数据.审核状态 + "\r\n";
//}

//textBox1.Text = retstr;

//var count1 = 0;
//var zjlist = Mongo.Coll<专家>().FindAs<BsonDocument>(Query.Exists("上次出席评标时间"));
//foreach (var zj in zjlist)
//{
//    var h = new Dictionary<string, List<DateTime>>();

//    Mongo.Coll<专家>().Update(
//       Query.EQ("_id", zj["_id"]),
//        Update.Combine(
//        Update.Set("历史抽取信息", new BsonDocument(h)),
//        Update.Unset("上次出席评标时间")
//        )
//       );
//    count1++;
//}
//this.textBox1.Text = zjlist.Count().ToString() + "------------" + count1.ToString();

/////////////////////////////转移验收单
//var arr = new long[] { 200000000675 ,200000000714,200000000646};
//            foreach (var kk in arr)
//            {
//                var ysd = 验收单管理.查询验收单(0, 0, Query<验收单>.Where(o => o.供应商链接.用户ID == kk), includeDeleted: true);
//                var gys = 用户管理.查找用户<供应商>(kk, includeDeleted: true); ;
//                var lc = 用户管理.查询用户<供应商>(0, 0, Query<供应商>.Where(o => o.企业基本信息.企业名称 == gys.企业基本信息.企业名称));
//                foreach (var k in ysd)
//                {
//                    k.供应商链接.用户ID = lc.First().Id;
//                    验收单管理.更新验收单(k);
//                }
//            }
//            MessageBox.Show("修改成功");


/////////////////////////////////////统计验收单
//public class ysdclass
//{
//    public int count;
//    public decimal price;
//}
//int allcount = 0;
//decimal allprice = 0;
//var date1 = new DateTime(2015, 1, 1);
//var date2 = new DateTime(2015, 6, 1);

//var ysdlist = 验收单管理.查询验收单(0, 0, Query<验收单>.Where(o => o.是否作废 == false && o.基本数据.添加时间>=date1 && o.基本数据.添加时间<date2 && o.审核数据.审核状态 == 审核状态.审核通过), includeDisabled: false);
//Dictionary<long,ysdclass> dlist = new Dictionary<long, ysdclass>();
//foreach (var ysd in ysdlist)
//{
//    allcount++;
//    allprice+=ysd.总金额;
//    var unitname = ysd.管理单位审核人.用户ID;
//    if (dlist.ContainsKey(unitname))
//    {
//        var d = dlist[unitname];
//        d.count++;
//        d.price += ysd.总金额;
//    }
//    else
//    {
//        ysdclass y = new ysdclass();
//        y.count = 1;
//        y.price = ysd.总金额;
//        dlist.Add(unitname,y);
//    }
//}
//var retstr = "";
//foreach (var dl in dlist)
//{
//    retstr += 用户管理.查找用户<单位用户>(dl.Key).单位信息.单位名称  + "--------------" + dl.Value.count + "----------------" + dl.Value.price +"\r\n";
//    Dictionary<string, ysdclass> dlistgys = new Dictionary<string, ysdclass>();
//    var ysdlistgys = ysdlist.Where(o => o.管理单位审核人.用户ID == dl.Key);
//    foreach (var ysdgys in ysdlistgys)
//    {
//        var gysname = ysdgys.供应商链接.用户数据.企业基本信息.企业名称;
//        if (dlistgys.ContainsKey(gysname))
//        {
//            dlistgys[gysname].count++;
//            dlistgys[gysname].price += ysdgys.总金额;
//        }
//        else
//        {
//            ysdclass y = new ysdclass();
//            y.count = 1;
//            y.price = ysdgys.总金额;
//            dlistgys.Add(gysname, y);
//        }
//    }

//    foreach (var dlgys in dlistgys)
//    {
//        retstr += "                               "+dlgys.Key + "--------------" + dlgys.Value.count + "----------------" + dlgys.Value.price + "\r\n";
//    }

//    retstr += "\r\n\r\n";
//}
//retstr += "\r\n验收单总数：" + allcount + "--------------------总金额：" + allprice;
//textBox1.Text = retstr;
/////////////////////////////////////统计验收单
/// 
/// 
/// 
/// 
/// IEnumerable<公告> model=公告管理.查询公告(0, 0);
//IEnumerable<新闻> news = 新闻管理.查询新闻(0,0);
//IEnumerable<通知> tz = 通知管理.查询通知(0,0);
//IEnumerable<政策法规> policy = 政策法规管理.查询政策法规(0, 0);
//string str = "公告里面\r\n\r\n";
//foreach(var item in model)
//{
//    if(item.内容主体.标题.Contains("徐")||item.内容主体.内容.Contains("徐"))
//    {
//        str += "徐\r\n" + item.Id.ToString() + "\r\n" + item.内容主体.标题 + "\r\n\r\n";
//    }
//    else if(item.内容主体.标题.Contains("郭")|| item.内容主体.内容.Contains("郭"))
//    {
//        str += "郭\r\n" + item.Id.ToString() + "\r\n" + item.内容主体.标题 + "\r\n\r\n";
//    }
//    else if(item.内容主体.标题.Contains("周")|| item.内容主体.内容.Contains("周"))
//    {
//        str += "周\r\n" + item.Id.ToString() + "\r\n" + item.内容主体.标题 + "\r\n\r\n";
//    }
//    else if(item.内容主体.标题.Contains("谷")|| item.内容主体.内容.Contains("谷"))
//    {
//        str += "谷\r\n" + item.Id.ToString() + "\r\n" + item.内容主体.标题 + "\r\n\r\n";
//    }
//    else if(item.内容主体.标题.Contains("杨")|| item.内容主体.内容.Contains("杨"))
//    {
//        str += "杨\r\n" + item.Id.ToString() + "\r\n" + item.内容主体.标题 + "\r\n\r\n";
//    }
//    else if(item.内容主体.标题.Contains("朱")|| item.内容主体.内容.Contains("朱"))
//    {
//        str += "朱\r\n" + item.Id.ToString() + "\r\n" + item.内容主体.标题 + "\r\n\r\n";
//    }
//}
//str+= "新闻里面\r\n\r\n";
//foreach (var item in news)
//{
//    if (item.内容主体.标题.Contains("徐") || item.内容主体.内容.Contains("徐"))
//    {
//        str += "徐\r\n" + item.Id.ToString() + "\r\n" + item.内容主体.标题 + "\r\n\r\n";
//    }
//    else if (item.内容主体.标题.Contains("郭") || item.内容主体.内容.Contains("郭"))
//    {
//        str += "郭\r\n" + item.Id.ToString() + "\r\n" + item.内容主体.标题 + "\r\n\r\n";
//    }
//    else if (item.内容主体.标题.Contains("周") || item.内容主体.内容.Contains("周"))
//    {
//        str += "周\r\n" + item.Id.ToString() + "\r\n" + item.内容主体.标题 + "\r\n\r\n";
//    }
//    else if (item.内容主体.标题.Contains("谷") || item.内容主体.内容.Contains("谷"))
//    {
//        str += "谷\r\n" + item.Id.ToString() + "\r\n" + item.内容主体.标题 + "\r\n\r\n";
//    }
//    else if (item.内容主体.标题.Contains("杨") || item.内容主体.内容.Contains("杨"))
//    {
//        str += "杨\r\n" + item.Id.ToString() + "\r\n" + item.内容主体.标题 + "\r\n\r\n";
//    }
//    else if (item.内容主体.标题.Contains("朱") || item.内容主体.内容.Contains("朱"))
//    {
//        str += "朱\r\n" + item.Id.ToString() + "\r\n" + item.内容主体.标题 + "\r\n\r\n";
//    }
//}
//str += "通知里面\r\n\r\n";
//foreach (var item in tz)
//{
//    if (item.内容主体.标题.Contains("徐") || item.内容主体.内容.Contains("徐"))
//    {
//        str += "徐\r\n" + item.Id.ToString() + "\r\n" + item.内容主体.标题 + "\r\n\r\n";
//    }
//    else if (item.内容主体.标题.Contains("郭") || item.内容主体.内容.Contains("郭"))
//    {
//        str += "郭\r\n" + item.Id.ToString() + "\r\n" + item.内容主体.标题 + "\r\n\r\n";
//    }
//    else if (item.内容主体.标题.Contains("周") || item.内容主体.内容.Contains("周"))
//    {
//        str += "周\r\n" + item.Id.ToString() + "\r\n" + item.内容主体.标题 + "\r\n\r\n";
//    }
//    else if (item.内容主体.标题.Contains("谷") || item.内容主体.内容.Contains("谷"))
//    {
//        str += "谷\r\n" + item.Id.ToString() + "\r\n" + item.内容主体.标题 + "\r\n\r\n";
//    }
//    else if (item.内容主体.标题.Contains("杨") || item.内容主体.内容.Contains("杨"))
//    {
//        str += "杨\r\n" + item.Id.ToString() + "\r\n" + item.内容主体.标题 + "\r\n\r\n";
//    }
//    else if (item.内容主体.标题.Contains("朱") || item.内容主体.内容.Contains("朱"))
//    {
//        str += "朱\r\n" + item.Id.ToString() + "\r\n" + item.内容主体.标题 + "\r\n\r\n";
//    }
//}
//str += "政策法规里面\r\n\r\n";
//foreach (var item in policy)
//{
//    if (item.内容主体.标题.Contains("徐") || item.内容主体.内容.Contains("徐"))
//    {
//        str += "徐\r\n" + item.Id.ToString() + "\r\n" + item.内容主体.标题 + "\r\n\r\n";
//    }
//    else if (item.内容主体.标题.Contains("郭") || item.内容主体.内容.Contains("郭"))
//    {
//        str += "郭\r\n" + item.Id.ToString() + "\r\n" + item.内容主体.标题 + "\r\n\r\n";
//    }
//    else if (item.内容主体.标题.Contains("周") || item.内容主体.内容.Contains("周"))
//    {
//        str += "周\r\n" + item.Id.ToString() + "\r\n" + item.内容主体.标题 + "\r\n\r\n";
//    }
//    else if (item.内容主体.标题.Contains("谷") || item.内容主体.内容.Contains("谷"))
//    {
//        str += "谷\r\n" + item.Id.ToString() + "\r\n" + item.内容主体.标题 + "\r\n\r\n";
//    }
//    else if (item.内容主体.标题.Contains("杨") || item.内容主体.内容.Contains("杨"))
//    {
//        str += "杨\r\n" + item.Id.ToString() + "\r\n" + item.内容主体.标题 + "\r\n\r\n";
//    }
//    else if (item.内容主体.标题.Contains("朱") || item.内容主体.内容.Contains("朱"))
//    {
//        str += "朱\r\n" + item.Id.ToString() + "\r\n" + item.内容主体.标题 + "\r\n\r\n";
//    }
//}
//textBox1.Text = str;