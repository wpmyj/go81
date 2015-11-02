using System.Collections.Generic;
using Go81WebApp.Models.数据模型.用户数据模型;
using Go81WebApp.Models.管理器;
using System.Web.Mvc;
using System;
using System.Linq;
using Go81WebApp.Models.管理器.推广业务管理;
using Go81WebApp.Models.数据模型.推广业务数据模型;
using MongoDB.Driver.Builders;

namespace Go81WebApp.Controllers.基本功能
{
    public class 布局Controller : Controller
    {
        public ActionResult TopBar()
        {
            return PartialView();
        }
        public JsonResult TopBarLoginInfo()
        {
            return new JsonResult
            {
                Data = -1 != HttpContext.检查登录()
                    ? new[]{
                        HttpContext.获取当前用户().登录信息.全名,
                        "注销登录",
                        "登录",
                        "LogOut",
                        GetBgLink(HttpContext.获取当前用户())
                    }
                    : new[]{
                        HttpContext.获取当前用户().登录信息.全名,
                        "登录",
                        "登录",
                        "Login"
                    }
            };
        }
        private static string GetBgLink(用户基本数据 u)
        {
            var t = u.GetType();
            return typeof(供应商) == t
                ? "供应商后台"
                : typeof(单位用户) == t
                    ? "单位用户后台"
                    : typeof(运营团队) == t
                        ? "运营团队后台"
                        : typeof(专家) == t
                        ? "专家后台"
                        : typeof(个人用户) == t
                            ? "个人用户后台"
                            : "首页"
                ;
        }
        public ActionResult Banner()
        {
            return PartialView();
        }
        public ActionResult NeedLogin(string type, long id)
        {
            ViewData["type"] = type;
            ViewData["id"] = id;
            return View();
        }
        public ActionResult SearchBar()
        {
            return PartialView();
        }
        public ActionResult NavigateMenu()
        {
            //if (-1 != HttpContext.检查登录())
            //{
            //    ViewData["已登录"] = "1";
            //}
            //else
            //{
            //    ViewData["已登录"] = "0";
            //}
            return PartialView();
        }
        public ActionResult NavigateMenuWithProductCategory()
        {
            //if (-1 != HttpContext.检查登录())
            //{
            //    ViewData["已登录"] = "1";
            //}
            //else
            //{
            //    ViewData["已登录"] = "0";
            //}
            return PartialView();
        }
        public ActionResult Footer()
        {
#if INTRANET
            return PartialView("Footer_Intranet");
#else
            return PartialView();
#endif
        }
        public ActionResult Footer_Intranet()
        {
            return PartialView();
        }
        public ActionResult LeftMenu()
        {
            ViewData["登录用户类型"] = "0";
            //供应商则弹出左边菜单
            var id = HttpContext.获取当前用户<用户基本数据>().Id;
            if (id > 200000000000 && id < 300000000000)
            {
                var 服务记录 = 供应商服务记录管理.查询供应商服务记录(0, 0, Query<供应商服务记录>.Where(o => o.所属供应商.用户ID == id));
                ViewData["企业模板"] = "G";
                if (服务记录.Count() > 0)
                {
                    var 已开通服务 = 服务记录.First().已开通的服务;
                    if (已开通服务.Any())
                    {
                        if (已开通服务.Where(o => o.所申请项目名.Contains("企业主页商品展示A类窗口") && o.结束时间 > DateTime.Now).Any())
                        {
                            ViewData["企业模板"] = "A";
                        }
                        else if (已开通服务.Where(o => o.所申请项目名.Contains("企业主页商品展示B类窗口") && o.结束时间 > DateTime.Now).Any())
                        {
                            ViewData["企业模板"] = "B";
                        }
                        else if (已开通服务.Where(o => o.所申请项目名.Contains("企业主页商品展示C类窗口") && o.结束时间 > DateTime.Now).Any())
                        {
                            ViewData["企业模板"] = "C";
                        }
                        else if (已开通服务.Where(o => o.所申请项目名.Contains("企业主页商品展示D类窗口") && o.结束时间 > DateTime.Now).Any())
                        {
                            ViewData["企业模板"] = "D";
                        }
                        else if (已开通服务.Where(o => o.所申请项目名.Contains("企业主页商品展示E类窗口") && o.结束时间 > DateTime.Now).Any())
                        {
                            ViewData["企业模板"] = "E";
                        }
                        else if (已开通服务.Where(o => o.所申请项目名.Contains("企业主页商品展示F类窗口") && o.结束时间 > DateTime.Now).Any())
                        {
                            ViewData["企业模板"] = "F";
                        }
                        else
                        {
                            ViewData["企业模板"] = "G";
                        }
                    }
                }

                ViewData["登录用户类型"] = "1";
            }

            var u = HttpContext.获取当前用户();
            var r = u as 供应商;
            var t = u.GetType();
            ViewData["用户类型"] = t.Name;
            return typeof(供应商) == t
                ? 供应商菜单(r)
                : typeof(单位用户) == t
                    ? 单位用户菜单()
                    : typeof(运营团队) == t
                        ? 运营团队菜单()
                        : typeof(个人用户) == t
                            ? 个人用户菜单()
                            : typeof(专家) == t
                            ? 专家菜单()
                            : new ContentResult();
        }


#if DEBUG
        private ActionResult 专家菜单()
        {
            var m = new Dictionary<string, object>
            {
                {
                    "后台首页", new Dictionary<string, object>
                    {
                        {
                            "评审专家注册须知","专家后台/Notice"
                        }
                    }
                },
                {
                "个人信息管理", new Dictionary<string, object>
                {
                    {"完善信息","专家后台/Notice"},
                    {"修改密码","专家后台/Modify_PWD"}
                  }
                },
                {
                "申请表下载与打印", new Dictionary<string, object>
                {
                    {"打印须知","专家后台/NoticeAboutApply"},
                    {"在线打印","专家后台/Print"},
                    {"下载表格","专家后台/Download"}
                  }
                },
            };
            return PartialView(m);
        }
#else
         private ActionResult 专家菜单()
        {
            var m = new Dictionary<string, object>
            {
                {
                    "后台首页", new Dictionary<string, object>
                    {
                        {"欢迎页面", "专家后台/Index"},
                        {"评审专家注册须知","专家后台/Notice"}
                    }
                },
                {
                    "个人信息管理", new Dictionary<string, object>
                    {
                        {"完善信息","专家后台/Notice"},
                        {"修改密码","专家后台/Modify_PWD"}
                    }
                },
                {
                    "申请表下载与打印", new Dictionary<string, object>
                    {
                        {"打印须知","专家后台/NoticeAboutApply"},
                        {"在线打印","专家后台/Print"},
                        {"下载表格","专家后台/Download"}
                    }
                },
            };
            return PartialView(m);
        }
#endif

        //        #region 新供应商后台
        //        private ActionResult 供应商菜单(供应商 u)
        //        {
        //            var p1 = new[] { 权限.新增消息 };            //权限.酒店
        //            var p2 = new[] { 权限.新增消息 };      //权限.历史采购结果查看, 权限.历史采购信息查看
        //            var p = new Dictionary<权限[], bool>
        //            {
        //                {p1, false}, //采用单一权限验证
        //                {p2, true}, //采用多重权限验证
        //            };
        //            //HttpContext.批量权限验证(ref p);
        //            var k = new Dictionary<string, object>();
        //            var m = new Dictionary<string, object>
        //            {
        //            };
        //            switch (u.供应商用户信息.供应商细分类型)
        //                {
        //                case 供应商.供应商细分类型.未填写:
        //                    m.Add("商品库维护", new Dictionary<string, object>
        //                    {
        //                        {"新增商品信息","供应商后台/AddGoods"},
        //                        {"新增中标商品","供应商后台/Gys_Product_AddStep1?id=1"},
        //                        {
        //                            "我的商品列表",new Dictionary<string,object>
        //                            {
        //                                {"未审核的商品信息","供应商后台/Gys_Product_List"},
        //                                {"已审核的商品信息" , "供应商后台/Gys_Product_Listed"},
        //                                {"当前上架的商品信息", "供应商后台/Gys_Product_Shelved"}
        //                    }
        //                },
        //#if DEBUG
        //                        {"新增促销商品信息","供应商后台/AddGoods"},
        //                {
        //                            "我的促销商品列表",new Dictionary<string,object>
        //                    {
        //                                {"未审核的促销(商品)信息","供应商后台/Gys_Product_List"},
        //                                {"已审核的促销(商品)信息" , "供应商后台/Gys_Product_AddStep1"},
        //                                {"当前上架的促销(商品)信息", "供应商后台/Gys_Product_Type"}
        //                            }
        //                        },
        //#endif
        //                    });
        //#if DEBUG
        //                    m.Add("采购商城店铺维护", new Dictionary<string, object>
        //                    {
        //                        {"店铺模板","供应商后台/AddAcceptanceForm"},
        //                        {"店铺推广", "供应商后台/Service_Evaluate"},
        //                        {"商城主题活动", "供应商后台/ProjectService_List"},
        //                    });
        //#endif


        //                    break;
        //                case 供应商.供应商细分类型.酒店:
        //                    m.Add("酒店管理", new Dictionary<string, object>(){
        //                                        { "酒店信息管理", "供应商后台/HotelEdit" },
        //                                        { "酒店房间管理", "供应商后台/Roomlist" },
        //                    });
        //                    break;
        //                case 供应商.供应商细分类型.机票代售点:
        //                    m.Add("机票验收单管理", new Dictionary<string, object>
        //                    {
        //                        {"新增机票验收单","供应商后台/AcceptanceTicketList"},
        //                        //{"上传验收单", "供应商后台/Service_Evaluate"},
        //                        //{"已上传验收单", "供应商后台/ProjectService_List"},
        //                        {"验收单管理流程", "通知/NoticeDetail?id=32"},
        //                    });


        //                    m.Add("机票代售点管理", new Dictionary<string, object>{
        //                                            {"添加代售点","供应商后台/TicketAdd"},
        //                                            {"代售点列表","供应商后台/Ticketlist"},
        //                    });
        //                    break;
        //                case 供应商.供应商细分类型.租车企业:
        //                    m.Add("租车信息管理", new Dictionary<string, object>{
        //                                            {"添加租车企业","供应商后台/CompanyWithCar"}
        //                    });
        //                    break;
        //            }

        //            m.Add("军采通服务管理", new Dictionary<string, object>
        //            {
        //                {"我订购的服务","供应商后台/AccountInfoManage"},
        //                //{"增值服务类订购合同","供应商后台/ValueAddedService"},
        //                //{"我的订购服务","供应商后台/MyServices"},
        //            });
        //#if DEBUG
        //            m.Add("协议供货（办公物资采购）", new Dictionary<string, object>
        //            {
        //                {"申请供货资格","供应商后台/AccountInfoManage"},
        //                {
        //                    "我供货的商品",new Dictionary<string,object>
        //                    {
        //                        {"办公设备","供应商后台/Gys_Product_List"},
        //                        {"办公耗材" , "供应商后台/Gys_Product_AddStep1"},
        //                        {"办公用品", "供应商后台/Gys_Product_Type"},
        //                        {"计算机及配件","供应商后台/Gys_Product_List"},
        //                        {"影音设备" , "供应商后台/Gys_Product_AddStep1"},
        //                        {"办公家具", "供应商后台/Gys_Product_Type"}
        //                    }
        //                },
        //            });
        //#endif

        //            if (u.供应商用户信息.供应商细分类型 == 供应商.供应商细分类型.未填写)
        //            {
        //                m.Add("验收单管理", new Dictionary<string, object>
        //                {
        //                    //{"新增及打印验收单","供应商后台/CheckSecurity?type=List"},
        //                    //{"上传验收单", "供应商后台/CheckSecurity?type=Upload"},
        //                    //{"已上传验收单", "供应商后台/CheckSecurity?type=UploadList"},
        //                    //{"验收单管理流程", "通知/NoticeDetail?id=32"},

        //                    {"验收单使用流程", "通知/NoticeDetail?id=32"},
        //                    {"新增验收单","供应商后台/AddAcceptForm"},
        //                    {"未审核验收单", "供应商后台/AddAcceptanceForm?comes=w"},
        //                    {"已审核验收单", "供应商后台/AddAcceptanceForm?comes=y"},
        //                    {"已作废验收单", "供应商后台/AddAcceptanceForm?comes=f"},
        //                    //{"打印验收单","供应商后台/AddAcceptanceForm"},
        //                    {"回传验收单存根联", "供应商后台/Service_Evaluate"},
        //                    {"全部验收单列表","供应商后台/AddAcceptanceForm?comes=a"},

        //                    //{"已上传验收单", "供应商后台/ProjectService_List"},
        //                });
        //            }
        //#if DEBUG
        //            m.Add("网上竞价管理", new Dictionary<string, object>
        //            {
        //                {"可参与的网上竞价项目","供应商后台/OnlineBiddingProject?comes=w"},
        //                {"我参与的网上竞价项目","供应商后台/OnlineBiddingProject?comes=y"},
        //                {"我已成交的网上竞价项目","供应商后台/OnlineBiddingManage"},
        //                {"我未成交的网上竞价项目","供应商后台/OnlineBiddingManage"},

        //                //{"历史报价项目","供应商后台/OnlineBiddingHistory"},
        //                //{"我的订购服务","供应商后台/MyServices"},
        //            });
        //            m.Add("批量集中采购管理", new Dictionary<string, object>
        //            {
        //                {"可参与的采购项目","供应商后台/OnlineBiddingProject"},
        //                {"我参与的采购项目","供应商后台/OnlineBiddingProject"},
        //                {"我已成交的采购项目","供应商后台/OnlineBiddingManage"},
        //                {"我未成交的采购项目","供应商后台/OnlineBiddingManage"},
        //            });
        //            m.Add("应急采购采购管理", new Dictionary<string, object>
        //            {
        //                {"网上协议","供应商后台/OnlineBiddingProject"},
        //                {"在线投标","供应商后台/OnlineBiddingProject"},
        //            });
        //#endif

        //            m.Add("企业信息管理", k = new Dictionary<string, object>
        //                    {
        //                        {"入库须知", "供应商后台/Notice"},
        //                        {"联系人信息", "供应商后台/Vip_Manage?comes=x"},
        //                        {"企业基本信息", "供应商后台/Gys_Manage"},
        //                        {"法定代表人信息", "供应商后台/Law_Person"},
        //                        {"营业信息", "供应商后台/Gys_Sales_Manage"},
        //                        {"资质证书信息", "供应商后台/Qualify_Management"},
        //                        {"售后服务机构信息", "供应商后台/Service_Management"},
        //                        {"出资人信息", "供应商后台/Investor_Management"},
        //                        {"财务信息", "供应商后台/Gys_Financial_Manage"},
        //                        {"税务信息", "供应商后台/Tax_Management"},
        //                        {"招投标经历", "供应商后台/Toubiao"},
        //                        //{
        //                          //  "地理位置", "供应商后台/Location"
        //                       // },
        //                        {"可提供商品类别", "供应商后台/Gys_Product_Type"},
        //                        {"提交预审", "供应商后台/SubmitAndCheck"},
        //                {"打印入库申请表","供应商后台/Print_Detail"},
        //                    });
        //            //m.Add("招标采购预报名", k = new Dictionary<string, object>
        //            //        {
        //            //            {"我的预报名", "供应商后台/gys_enroll?page=1"},
        //            //            {"可参加的预报名", "供应商后台/Gys_Manage"},
        //            //        });

        //            m.Add("本账号信息维护", new Dictionary<string, object>
        //                {
        //                {"修改登录密码", "供应商后台/Vip_Password_Manage"},
        //                {"修改注册人信息", "供应商后台/Vip_Manage?comes=z"},
        //                });
        //            m.Add("站内消息", new Dictionary<string, object>
        //            {
        //                {"新增消息" , "供应商后台/Gys_ZnxxAdd"},
        //                {"已发消息", "供应商后台/Msg_Sent"},
        //                {"已收消息", "供应商后台/Gys_Znxx"},
        //            });
        //            //m.Add("投诉建议", new Dictionary<string, object>
        //            //        {
        //            //            {
        //            //                "投诉", new Dictionary<string, object>
        //            //                {
        //            //                    {"发起投诉" , "供应商后台/Gys_ComplainAdd"},
        //            //                    {"我的投诉" , "供应商后台/Gys_ComplainList"},
        //            //                }
        //            //            },
        //            //            {
        //            //                "建议", new Dictionary<string, object>
        //            //{
        //            //                    {"发起建议" , "供应商后台/Gys_SuggestAdd"},
        //            //                    {"我的建议" , "供应商后台/Gys_SuggestList"},
        //            //}
        //            //            },
        //            //        });
        //            //if(u.供应商用户信息.供应商细分类型==供应商.供应商细分类型.未填写)
        //            //{
        //            //    k.Add("可提供商品类别", "供应商后台/Gys_Product_Type");
        //            //}
        //            //m.Add("公告管理", new Dictionary<string, object>
        //            //        {
        //            //            {
        //            //                "公告查询" , "供应商后台/Gys_Ztb_Search_Zb"
        //            //            },
        //            //             {
        //            //                "公告订阅", "供应商后台/Book_Msg"
        //            //            }
        //            //        });
        //            //m.Add("招标采购预报名管理", new Dictionary<string, object>
        //            //        {
        //            //               {"我的预报名" , "供应商后台/Gys_enroll?page=1"}
        //            //        });


        //            //m.Add("投诉建议", new Dictionary<string, object>
        //            //        {
        //            //            {
        //            //                "投诉", new Dictionary<string, object>
        //            //                {
        //            //                    //{"发起投诉" , "供应商后台/Gys_ComplainAdd"},
        //            //                    {"我的投诉" , "供应商后台/Gys_ComplainList"},
        //            //                }
        //            //            },
        //            //            {
        //            //                "建议", new Dictionary<string, object>
        //            //                {
        //            //                    //{"发起建议" , "供应商后台/Gys_SuggestAdd"},
        //            //                    {"我的建议" , "供应商后台/Gys_SuggestList"},
        //            //                }
        //            //            },
        //            //        });
        //            return PartialView(m);
        //        }
        //        #endregion
        private ActionResult 供应商菜单(供应商 u)
        {
            // var p1 = new[] { };
            // var p2 = new[] { };
            // var p = new Dictionary<权限[], bool>
            //{
            //    {p1, false}, //采用单一权限验证
            //    {p2, true}, //采用多重权限验证
            //};
            //HttpContext.批量权限验证(ref p);
            var k = new Dictionary<string, object>();
            var m = new Dictionary<string, object>
            {
                {
                    "后台首页",new Dictionary<string,object>
                    { 
                        {"欢迎页面", "供应商后台/Index"},
                        {"入库须知", "供应商后台/Notice"},
                        {"待办事项", "供应商后台/Completing"},
                    }
                },
                {
                    "消息管理", new Dictionary<string, object>
                    {
                        {"系统通知", "供应商后台/Gys_Xttz"},
                        {
                            "站内消息", new Dictionary<string, object>
                            {
                                {"发新消息" , "供应商后台/Gys_ZnxxAdd"},
                                {"已发消息", "供应商后台/Msg_Sent"},
                                {"已收消息", "供应商后台/Gys_Znxx"},
                            }
                        },

                    }
                },
            };

#if DEBUG
            m.Add("网上竞价管理", new Dictionary<string, object>
                    {
                        {"参与竞价项目","供应商后台/OnlineBiddingProject"},
                        {"中标结果查看","供应商后台/OnlineBiddingManage"},

                        //{"历史报价项目","供应商后台/OnlineBiddingHistory"},
                        //{"我的订购服务","供应商后台/MyServices"},
                    });
            m.Add("订单管理", new Dictionary<string, object> { { "我的订单", "供应商后台/PurchaseInfo" } });

#endif

            m.Add("服务管理", new Dictionary<string, object>
                    {
                        {"军采通服务管理","供应商后台/AccountInfoManage"},
                        //{"增值服务类订购合同","供应商后台/ValueAddedService"},
                        //{"我的订购服务","供应商后台/MyServices"},
                    });
            switch (u.供应商用户信息.供应商细分类型)
            {
                case 供应商.供应商细分类型.未填写:
                    m.Add("验收单管理", new Dictionary<string, object>
                    {
                        //{"新增及打印验收单","供应商后台/CheckSecurity?type=List"},
                        //{"上传验收单", "供应商后台/CheckSecurity?type=Upload"},
                        //{"已上传验收单", "供应商后台/CheckSecurity?type=UploadList"},
                        //{"验收单管理流程", "通知/NoticeDetail?id=32"},
                    
                        {"新增及打印验收单","供应商后台/AddAcceptanceForm"},
                        {"上传验收单", "供应商后台/Service_Evaluate"},
                        {"已上传验收单", "供应商后台/ProjectService_List"},
                        {"验收单管理流程", "通知/NoticeDetail?id=32"},
                    });
                    m.Add("商品管理", new Dictionary<string, object>
                    {
                        {
                            "我的商品库",new Dictionary<string,object>
                            {
                                {"商品列表","供应商后台/Gys_Product_List"},
                                //{"添加新商品" , "供应商后台/Gys_Product_AddStep1"},
                                //{"可提供商品类别", "供应商后台/Gys_Product_Type"}
                            }
                        },
                    });
                    break;
                case 供应商.供应商细分类型.酒店:
                    m.Add("酒店管理", new Dictionary<string, object>(){
                                        { "酒店信息管理", "供应商后台/HotelEdit" },
                                        { "酒店房间管理", "供应商后台/Roomlist" },
                    });
                    break;
                case 供应商.供应商细分类型.机票代售点:
                    m.Add("机票验收单管理", new Dictionary<string, object>
                    {
                        {"新增机票验收单","供应商后台/AcceptanceTicketList"},
                        //{"上传验收单", "供应商后台/Service_Evaluate"},
                        //{"已上传验收单", "供应商后台/ProjectService_List"},
                        {"验收单管理流程", "通知/NoticeDetail?id=32"},
                    });


                    m.Add("机票代售点管理", new Dictionary<string, object>{
                                            {"添加代售点","供应商后台/TicketAdd"},
                                            {"代售点列表","供应商后台/Ticketlist"},
                    });
                    break;
                case 供应商.供应商细分类型.租车企业:
                    m.Add("租车信息管理", new Dictionary<string, object>{
                                            {"添加租车企业","供应商后台/CompanyWithCar"}
                    });
                    break;
            }
            m.Add("企业信息管理", k = new Dictionary<string, object>
                    {
                        {"联系人信息", "供应商后台/Vip_Manage"},
                        {"企业基本信息", "供应商后台/Gys_Manage"},
                        {"法定代表人信息", "供应商后台/Law_Person"},
                        {"营业信息", "供应商后台/Gys_Sales_Manage"},
                        {"资质证书信息", "供应商后台/Qualify_Management"},
                        {"售后服务机构信息", "供应商后台/Service_Management"},
                        {"出资人信息", "供应商后台/Investor_Management"},
                        {"财务信息", "供应商后台/Gys_Financial_Manage"},
                        {"税务信息", "供应商后台/Tax_Management"},
                        {"招投标经历", "供应商后台/Toubiao"},
                        //{
                          //  "地理位置", "供应商后台/Location"
                       // },
                        {"可提供商品类别", "供应商后台/Gys_Product_Type"},
                        {"提交预审", "供应商后台/SubmitAndCheck"},
                        {"打印资料",new Dictionary<string,object> {
                        {"打印申请表","供应商后台/Print_Detail"},
                        {"打印须知","供应商后台/NoticeAboutApply"}
                        }},
                    });
            //m.Add("招标采购预报名", k = new Dictionary<string, object>
            //        {
            //            {"我的预报名", "供应商后台/gys_enroll?page=1"},
            //            {"可参加的预报名", "供应商后台/Gys_Manage"},
            //        });

            m.Add("密码管理", new Dictionary<string, object>
                {
                    {"修改密码", "供应商后台/Vip_Password_Manage"},
                });
            //m.Add("订单", new Dictionary<string, object>
            //    {
            //        {"订单列表", "供应商后台/PurchaseInfo"},
            //    });
            //m.Add("投诉建议", new Dictionary<string, object>
            //        {
            //            {
            //                "投诉", new Dictionary<string, object>
            //                {
            //                    {"发起投诉" , "供应商后台/Gys_ComplainAdd"},
            //                    {"我的投诉" , "供应商后台/Gys_ComplainList"},
            //                }
            //            },
            //            {
            //                "建议", new Dictionary<string, object>
            //{
            //                    {"发起建议" , "供应商后台/Gys_SuggestAdd"},
            //                    {"我的建议" , "供应商后台/Gys_SuggestList"},
            //}
            //            },
            //        });
            //if(u.供应商用户信息.供应商细分类型==供应商.供应商细分类型.未填写)
            //{
            //    k.Add("可提供商品类别", "供应商后台/Gys_Product_Type");
            //}
            m.Add("公告管理", new Dictionary<string, object>
                    {
                        {
                            "公告查询" , "供应商后台/Gys_Ztb_Search_Zb"
                        },
                         {
                            "公告订阅", "供应商后台/Book_Msg"
                        }
                    });
            m.Add("招标采购预报名管理", new Dictionary<string, object>
                    {
                           {"我的预报名" , "供应商后台/Gys_enroll?page=1"}
                    });


            m.Add("投诉建议", new Dictionary<string, object>
                    {
                        {
                            "投诉", new Dictionary<string, object>
                            {
                                //{"发起投诉" , "供应商后台/Gys_ComplainAdd"},
                                {"我的投诉" , "供应商后台/Gys_ComplainList"},
                            }
                        },
                        {
                            "建议", new Dictionary<string, object>
                            {
                                //{"发起建议" , "供应商后台/Gys_SuggestAdd"},
                                {"我的建议" , "供应商后台/Gys_SuggestList"},
                            }
                        },
                    });
            return PartialView(m);
        }

        #region 新单位用户后台
        private ActionResult 单位用户菜单()
        {
            var user = HttpContext.获取当前用户<单位用户>();
            if (user.审核数据.审核状态 == Models.数据模型.审核状态.审核通过)
            {
                #region 权限
                var p = new Dictionary<object, Tuple<权限[], bool>>
                {
                    {"询价采购历史", new Tuple<权限[], bool>(new[]{权限.欢迎页面}, false)}, 
                    {"欢迎页面", new Tuple<权限[], bool>(new[]{权限.欢迎页面}, false)}, 
                    {"我的订单", new Tuple<权限[], bool>(new[]{权限.欢迎页面}, false)},
                    {"待办任务", new Tuple<权限[], bool>(new[]{权限.待办任务}, false)}, 

                    {"新增采购公告", new Tuple<权限[], bool>(new[]{权限.新增采购公告}, false)}, 
                    {"我的采购公告", new Tuple<权限[], bool>(new[]{权限.我的采购公告}, false)}, 

                    {"验收单管理流程", new Tuple<权限[], bool>(new[]{权限.验收单管理流程}, false)}, 
                    {"未审核的验收单", new Tuple<权限[], bool>(new[]{权限.未审核的验收单}, false)}, 
                    {"已审核的验收单", new Tuple<权限[], bool>(new[]{权限.已审核的验收单}, false)}, 
                    {"已作废的验收单", new Tuple<权限[], bool>(new[]{权限.已作废的验收单}, false)}, 
                    {"我的验收单列表", new Tuple<权限[], bool>(new[]{权限.我的验收单列表}, false)}, 

                    {"编制需求计划", new Tuple<权限[], bool>(new[]{权限.编制需求计划}, false)}, 
                    {"审核需求计划", new Tuple<权限[], bool>(new[]{权限.审核需求计划}, false)}, 
                    {"我的需求计划列表", new Tuple<权限[], bool>(new[]{权限.我的需求计划列表}, false)}, 

                    {"编制采购任务", new Tuple<权限[], bool>(new[]{权限.编制采购任务}, false)},
                    {"审核采购任务", new Tuple<权限[], bool>(new[]{权限.审核采购任务}, false)},
                    {"受理采购任务", new Tuple<权限[], bool>(new[]{权限.受理采购任务}, false)},
                    {"我的采购任务列表", new Tuple<权限[], bool>(new[]{权限.我的采购任务列表}, false)},

                    {"新增网上竞价", new Tuple<权限[], bool>(new[]{权限.新增网上竞价}, false)}, 
                    {"未完成的网上竞价", new Tuple<权限[], bool>(new[]{权限.未完成的网上竞价}, false)}, 
                    {"已完成的网上竞价", new Tuple<权限[], bool>(new[]{权限.已完成的网上竞价}, false)}, 

                    {"新增电子标书", new Tuple<权限[], bool>(new[]{权限.新增电子标书}, false)}, 
                    {"可使用电子标书的项目列表", new Tuple<权限[], bool>(new[]{权限.可使用电子标书的项目列表}, false)},

                    {"中标商品查询系统", new Tuple<权限[], bool>(new[]{权限.中标商品查询系统}, false)},

                    {"应急采购在线投标系统", new Tuple<权限[], bool>(new[]{权限.应急采购在线投标系统}, false)}, 

                    {"新增抽取申请", new Tuple<权限[], bool>(new[]{权限.新增评审专家抽取申请,权限.新增供应商抽取申请}, false)},
                    {"我可进行的抽取申请", new Tuple<权限[], bool>(new[]{权限.我可进行的评审专家抽取申请,权限.我可进行的供应商抽取申请}, false)}, 
                    {"我已完成的抽取申请", new Tuple<权限[], bool>(new[]{权限.我已完成的评审专家抽取申请,权限.我已完成的供应商抽取申请}, false)}, 
                    {"我可审核的抽取申请", new Tuple<权限[], bool>(new[]{权限.我可审核的评审专家抽取申请,权限.我可审核的供应商抽取申请}, false)}, 
                    {"全部抽取记录列表", new Tuple<权限[], bool>(new[]{权限.评审专家全部抽取记录列表,权限.供应商全部抽取记录列表}, false)}, 

                    {"我可审核的入库申请", new Tuple<权限[], bool>(new[]{权限.我可审核的评审专家入库申请,权限.我可审核的供应商入库申请}, false)},
                    {"我已审核的入库申请", new Tuple<权限[], bool>(new[]{权限.我已审核的评审专家入库申请,权限.我已审核的供应商入库申请}, false)},
                    {"全部入库申请列表", new Tuple<权限[], bool>(new[]{权限.评审专家全部入库申请列表,权限.供应商全部入库申请列表}, false)},
                    {"打印初审/复审意见表", new Tuple<权限[], bool>(new[]{权限.打印评审专家初审复审意见表,权限.打印供应商初审复审意见表},false)},

                    {"推荐流程说明", new Tuple<权限[], bool>(new[]{权限.推荐流程说明}, false)}, 
                    {"下载入库申请表", new Tuple<权限[], bool>(new[]{权限.下载入库申请表}, false)},

                    {"新建附属账号", new Tuple<权限[], bool>(new[]{权限.新建附属账号}, false)}, 
                    {"未审核的附属账号", new Tuple<权限[], bool>(new[]{权限.未审核的附属账号}, false)}, 
                    {"已审核的附属账号", new Tuple<权限[], bool>(new[]{权限.已审核的附属账号}, false)}, 
                    {"全部附属账号列表", new Tuple<权限[], bool>(new[]{权限.全部附属账号列表}, false)}, 
                    {"全部附属账号的采购公告列表", new Tuple<权限[], bool>(new[]{权限.全部附属账号的采购公告列表}, false)}, 
                    {"全部附属账号的验收单列表", new Tuple<权限[], bool>(new[]{权限.全部附属账号的验收单列表}, false)}, 

                    {"修改登录密码", new Tuple<权限[], bool>(new[]{权限.修改登录密码}, false)}, 
                    {"修改联系人信息", new Tuple<权限[], bool>(new[]{权限.修改联系人信息}, false)}, 
                    
                    {"新增消息", new Tuple<权限[], bool>(new[]{权限.新增消息}, false)}, 
                    {"已收消息", new Tuple<权限[], bool>(new[]{权限.已收消息}, false)}, 
                    {"已发消息", new Tuple<权限[], bool>(new[]{权限.已发消息}, false)}, 

                    {"专家统计", new Tuple<权限[], bool>(new[]{权限.已发消息}, false)}, 
                    {"登陆统计", new Tuple<权限[], bool>(new[]{权限.已发消息}, false)}, 
                    {"广告点击统计", new Tuple<权限[], bool>(new[]{权限.已发消息}, false)}, 
                    {"商品统计", new Tuple<权限[], bool>(new[]{权限.已发消息}, false)}, 
                    {"供应商统计", new Tuple<权限[], bool>(new[]{权限.已发消息}, false)}, 
                    {"公告统计", new Tuple<权限[], bool>(new[]{权限.已发消息}, false)}, 
                    {"总体统计", new Tuple<权限[], bool>(new[]{权限.已发消息}, false)}, 
                };
                #endregion
                HttpContext.批量权限验证(ref p);
                Dictionary<string, object> dg;

                var m_nocontact = new Dictionary<string, object>
                {

                };

                //在内网，密码为123456或者联系人信息未填写，则不能使用网站其他功能
                if (WebApiApplication.IsIntranet && (
                    user.登录信息.密码 == "7C4A8D09CA3762AF61E59520943DC26494F8941B" ||
                    user.联系方式 == null || string.IsNullOrWhiteSpace(user.联系方式.联系人) || user.联系方式.联系人 == "联系人" || string.IsNullOrWhiteSpace(user.联系方式.手机)
                    ))
                {
                    m_nocontact.Add("本账号信息维护", new Dictionary<string, object>
                    {
                        {"修改密码", "单位用户后台/MessageKeyword"},
                        {"修改联系人信息", "单位用户后台/MessageModify"},
                    });
                    m_nocontact.Add("站内消息", new Dictionary<string, object>
                    {
                        {"发新消息", "单位用户后台/Procure_ZnxxAdd"},
                        {"已收消息", "单位用户后台/Procure_Znxx"},
                        {"已发消息", "单位用户后台/Procure_ZnxxSend"},
                    });
                    return PartialView(m_nocontact);
                }
                var m = new Dictionary<string, object>
                {
                    {
                        "采购公告管理", new Dictionary<string, object>
                        {
                            {"新增采购公告", "单位用户后台/Procure_AdAdd"},
                            {"我的采购公告", "单位用户后台/Procure_AdSendList_S"},
                            //{"审核采购公告", "单位用户后台/Procure_AdSendList"},
                            //{"全部采购公告列表", "单位用户后台/Procure_AdList"},
                        }
                    },
                    {
                       "订单管理", new Dictionary<string, object>
                       {
                       { "我的订单", "单位用户后台/PurchaseInfo" },
                       }
                    },
                    {
                        "验收单管理", new Dictionary<string, object>
                        {
                            {"验收单管理流程", "通知/NoticeDetail?id=32"},
                            {"未审核的验收单", "单位用户后台/AcceptanceList"},
                            {"已审核的验收单", "单位用户后台/Acceptance_checked"},
                            {"已作废的验收单", "单位用户后台/AcceptanceList_Avoid"},
                            {"我的验收单列表", "单位用户后台/Acceptanced_List"},
                        }
                    },
#if INTRANET
                      {
                        "采购需求管理", new Dictionary<string, object>
                        {
                            {"编制需求计划","单位用户后台/Add_Demand"},
                            {"审核需求计划","单位用户后台/DemandCheck?page=1"},
                            {"我的需求计划列表","单位用户后台/Demandlist?page=1"},
                        }
                    },
                    {
                        "采购任务管理", new Dictionary<string, object>
                        {
                             {"编制采购任务","单位用户后台/AssignmentTaskList"},
                             {"审核采购任务","单位用户后台/AssignmentTaskAuditList"},
                             {"受理采购任务","单位用户后台/AssignmentTaskList_p"},
                             //{"我的采购任务列表","单位用户后台/AssignmentTaskList"},
                        }
                    },
#endif
#if DUBUG
                    {
                        "网上竞价管理", new Dictionary<string, object>
                        {
                            {"新增网上竞价", "单位用户后台/OnlineBidding_Add"},
                            {"未完成的网上竞价", "单位用户后台/OnlineBidding_List"},
                            {"已完成的网上竞价", "单位用户后台/OnlineBidding_List_Ed"},
                        }
                    },
#endif

                    //{
                    //    "电子标书管理", new Dictionary<string, object>
                    //    {
                    //             {"新增电子标书", "单位用户后台/OnlineBidding_Add"},
                    //            {"可使用电子标书的项目列表", "单位用户后台/OnlineBidding_List"},
                    //    }
                    //},


                    //{
                    //    "中标商品查询系统, new Dictionary<string, object>
                    //    {

                    //    }
                    //},


                    //{
                    //    "应急采购在线投标系统,new Dictionary<string, object>
                    //    {
                                 
                    //    }
                    //},
                };
#if INTRANET
                m.Add("评审专家抽取系统", new Dictionary<string, object>
                        {
                            
                            //{"评审专家列表", "专家抽选/Expert_list"},
                            {"新增抽取申请", "专家抽选/Expert_Applay"},
                            {"我可进行的抽取申请", "专家抽选/Expert_Applay_S"},//包含历史我的历史抽取记录
                            {"我已完成的抽取申请", "专家抽选/Expert_Applay_ed"},//包含历史我的历史抽取记录
                            {"我可审核的抽取申请", "专家抽选/Expert_ApplayAuditList"},//包含历史我的历史抽取记录
                            {"全部抽取记录列表", "专家抽选/Expert_History"},
                        });
#endif
                //m.Add("在库供应商抽取系统", new Dictionary<string, object>
                //{
                //    {"新增抽取申请", "专家抽选/GysChoose_Applay"},
                //    {"我可进行的抽取申请", "专家抽选/GysChoose_Applay_S"},//包含历史我的历史抽取记录
                //    {"我已完成的抽取申请", "专家抽选/GysChoose_Applay_S"},//包含历史我的历史抽取记录
                //    {"我可审核的抽取申请", "专家抽选/GysChoose_ApplayAuditList"},
                //    {"全部抽取记录列表", "专家抽选/GysChoose_HistoryList"},
                //});

                //m.Add("评审专家入库管理", new Dictionary<string, object>
                //{
                //     {"我可审核的入库申请", "专家抽选/Expert_Add"},
                //     {"我已审核的入库申请", "专家抽选/Expert_Add"},
                //     {"全部入库申请列表", "专家抽选/Expert_Add"},
                //     {"打印初审/复审意见表", "专家抽选/Expert_Add"},
                //});

                //m.Add("供应商入库管理", new Dictionary<string, object>
                //{
                //      {"我可审核的入库申请", "单位用户后台/Gys_Examine"},
                //      {"我已审核的入库申请", "单位用户后台/Gys_Examined"},
                //      {"全部入库申请列表", "单位用户后台/Gys_Examined"},
                //      {"打印初审/复审意见表", "单位用户后台/Gys_Examined"},
                //});


                m.Add("推荐供应商与评审专家", new Dictionary<string, object>
                    {
                        {"推荐流程说明", "单位用户后台/Gys_Expert_Recomed"},
                        {"下载入库申请表", "下载/Download_List"},
                        //{"推荐供应商", "单位用户后台/Recommend_GysList"},
                        //{"审核推荐的供应商", "单位用户后台/Recommend_GysList_Audit"},
                        //{"打印已审核推荐的供应商", "单位用户后台/Print_Supplier"},
                        //{"推荐评审专家", "单位用户后台/Recommend_ExpertList"},
                        //{"审核推荐的评审专家", "单位用户后台/Recommend_ExpertList_Audit"},
                        //{"打印已审核推荐的评审专家", "单位用户后台/Print_Expert"},
                    });
#if DEBUG
                m.Add("附属帐号管理", new Dictionary<string, object>
                        {
                        {"新增附属账号",new Dictionary<string,object>
                         {
                                {"新建附属账号", "单位用户后台/DepartmentAdd"},
                                {"未审核的附属账号", "单位用户后台/DepartmentAuditList"},
                                {"已审核的附属账号", "单位用户后台/DepartmentList"},
                        }
                        },
                        {"全部附属账号列表", "单位用户后台/Print_UserList"}, 
                        {"全部附属账号的采购公告列表", "单位用户后台/SubUnit_Manage?comes=ad"}, 
                        {"全部附属账号的验收单列表", "单位用户后台/SubUnit_Manage?comes=ysd"},
                    });
#endif
                m.Add("本账号信息/维护", new Dictionary<string, object>
                    {
                        {"修改登录密码", "单位用户后台/MessageKeyword"},
                        {"修改联系人信息", "单位用户后台/MessageModify"},
                    });
                //m.Add("询价采购", new Dictionary<string, object>
                //    {
                //        {"询价采购历史", "单位用户后台/ConsultList"},
                //    });
                m.Add("站内消息", new Dictionary<string, object>
                    {
                        {"新增消息", "单位用户后台/Procure_ZnxxAdd"},
                        {"已收消息", "单位用户后台/Procure_Znxx"},
                        {"已发消息", "单位用户后台/Procure_ZnxxSend"},
                    });
                //m.Add("办事指南", new Dictionary<string, object>
                //    {
                //        {"添加办事指南", "单位用户后台/GuideAdd"},
                //        {"办事指南列表", "单位用户后台/GuideList"},
                //    });
                //m.Add("培训资料", new Dictionary<string, object>
                //    {
                //        {"添加培训资料", "单位用户后台/TrainingAdd"},
                //        {"培训资料列表", "单位用户后台/TrainingList"},
                //    });
#if DEBUG
                m.Add("统计管理", new Dictionary<string, object>
                {
                    {"专家统计", "单位用户后台/ExpertStatistic"},
                    {"登陆统计", "单位用户后台/LoginStatistic"}, 
                    {"广告点击统计", "单位用户后台/AdvertiseStatistic"}, 
                    {"商品统计","单位用户后台/GoodStatistic"}, 
                    {"供应商统计", "单位用户后台/GysStatistic"}, 
                    {"公告统计", "单位用户后台/AdStatistic"},
                    {"总体统计", "单位用户后台/OperationInfo"},
                });
#endif
                var l = m.Keys.ToArray();
                foreach (var sm in l)
                {
                    if (m[sm] is string && !p[sm].Item2) { m.Remove(sm); continue; }
                    var m1 = m[sm] as Dictionary<string, object>;
                    var l1 = m1.Keys.ToArray();
                    foreach (var sm1 in l1)
                    {
                        if (m1[sm1] is string && !p[sm1].Item2) { m1.Remove(sm1); continue; }
                        if (m1[sm1] is Dictionary<string, object>)
                        {
                            var m2 = m1[sm1] as Dictionary<string, object>;
                            var l2 = m2.Keys.ToArray();
                            foreach (var sm2 in l2)
                            {
                                if (m2[sm2] is string && !p[sm2].Item2) m2.Remove(sm2);
                            }
                            if (m2.Keys.Count <= 0) { m1.Remove(sm1); }
                        }
                    }
                    if (m1.Keys.Count <= 0) { m.Remove(sm); }
                }

                return PartialView(m);
            }
            else
            {
                ViewData["单位状态"] = HttpContext.获取当前用户<单位用户>().审核数据.审核状态.ToString();
                return PartialView();
            }
        }
        #endregion

        //        private ActionResult 单位用户菜单()
        //        {
        //            var user = HttpContext.获取当前用户<单位用户>();
        //            if (user.审核数据.审核状态 == Models.数据模型.审核状态.审核通过)
        //            {
        //                var p = new Dictionary<object, Tuple<权限[], bool>>
        //                {
        //                    {"欢迎页面", new Tuple<权限[], bool>(new[]{权限.欢迎页面}, false)}, 
        //                    {"修改密码", new Tuple<权限[], bool>(new[]{权限.修改密码}, false)}, 
        //                    {"修改联系人信息", new Tuple<权限[], bool>(new[]{权限.修改联系人信息}, false)}, 

        //                    {"发新消息", new Tuple<权限[], bool>(new[]{权限.发新消息}, false)}, 
        //                    {"已收消息", new Tuple<权限[], bool>(new[]{权限.已收消息}, false)}, 
        //                    {"已发消息", new Tuple<权限[], bool>(new[]{权限.已发消息}, false)}, 

        //                    {"编制需求计划", new Tuple<权限[], bool>(new[]{权限.添加需求提报}, false)}, 
        //                    {"审核需求计划", new Tuple<权限[], bool>(new[]{权限.审核需求提报}, false)}, 
        //                    {"编制采购任务", new Tuple<权限[], bool>(new[]{权限.审核需求提报}, false)},
        //                    {"审核采购任务", new Tuple<权限[], bool>(new[]{权限.审核需求提报}, false)},
        //                    {"受理采购任务", new Tuple<权限[], bool>(new[]{权限.审核需求提报}, false)},

        //                    {"发布公告", new Tuple<权限[], bool>(new[]{权限.发布公告}, false)}, 
        //                    {"我的公告列表", new Tuple<权限[], bool>(new[]{权限.查看我的公告}, false)}, 
        //                    {"审核公告", new Tuple<权限[], bool>(new[]{权限.审核公告}, false)}, 
        //                    {"公告列表", new Tuple<权限[], bool>(new[]{权限.查看全部公告}, false)},
        //                    {"待审核供应商", new Tuple<权限[], bool>(new[]{权限.查看待审核供应商}, false)}, 
        //                    {"已审核供应商", new Tuple<权限[], bool>(new[]{权限.查看已审核供应商}, false)}, 

        //                    {"发布新闻", new Tuple<权限[], bool>(new[]{权限.发布新闻}, false)}, 
        //                    {"我的新闻列表", new Tuple<权限[], bool>(new[]{权限.查看我的新闻}, false)}, 
        //                    {"审核新闻", new Tuple<权限[], bool>(new[]{权限.新闻审核}, false)}, 
        //                    {"新闻列表", new Tuple<权限[], bool>(new[]{权限.查看所有新闻}, false)}, 

        //                    {"发布通知", new Tuple<权限[], bool>(new[]{权限.发布通知}, false)}, 
        //                    {"我的通知列表", new Tuple<权限[], bool>(new[]{权限.查看我的通知}, false)}, 
        //                    {"审核通知", new Tuple<权限[], bool>(new[]{权限.通知审核}, false)}, 
        //                    {"通知列表", new Tuple<权限[], bool>(new[]{权限.查看所有通知}, false)}, 

        //                    {"发布政策法规", new Tuple<权限[], bool>(new[]{权限.发布政策法规}, false)}, 
        //                    {"我的政策法规列表", new Tuple<权限[], bool>(new[]{权限.查看我的政策法规}, false)}, 
        //                    {"审核政策法规", new Tuple<权限[], bool>(new[]{权限.政策法规审核}, false)}, 
        //                    {"政策法规列表", new Tuple<权限[], bool>(new[]{权限.查看所有政策法规}, false)}, 

        //                    {"添加单位用户", new Tuple<权限[], bool>(new[]{权限.单位用户添加}, false)}, 
        //                    {"单位用户列表", new Tuple<权限[], bool>(new[]{权限.查看所有单位用户,权限.查看我的单位用户}, false)}, 
        //                    {"我的单位用户列表", new Tuple<权限[], bool>(new[]{权限.查看我的单位用户}, false)}, 
        //                    {"审核单位用户", new Tuple<权限[], bool>(new[]{权限.单位用户审核}, false)}, 
        //                    {"打印用户名单", new Tuple<权限[], bool>(new[]{权限.打印单位用户名单}, false)}, 

        //                    {"添加用户组", new Tuple<权限[], bool>(new[]{权限.添加用户组}, false)}, 
        //                    {"我的用户组管理", new Tuple<权限[], bool>(new[]{权限.我的用户组管理}, false)}, 
        //                    {"用户组管理", new Tuple<权限[], bool>(new[]{权限.用户组管理}, false)}, 
        //                    {"添加角色", new Tuple<权限[], bool>(new[]{权限.添加角色}, false)}, 
        //                    {"角色管理", new Tuple<权限[], bool>(new[]{权限.角色管理}, false)}, 

        //                    {"添加评审专家", new Tuple<权限[], bool>(new[]{权限.添加评审专家}, false)}, 
        //                    {"评审专家列表", new Tuple<权限[], bool>(new[]{权限.查看评审专家, 权限.修改评审专家, 权限.删除评审专家, 权限.打印评审专家申请表}, false)}, 
        //                    {"评审专家抽取申请", new Tuple<权限[], bool>(new[]{权限.评审专家抽取申请}, false)}, 
        //                    {"我提交的评审专家抽取申请", new Tuple<权限[], bool>(new[]{权限.评审专家抽取申请,权限.评审专家抽取}, false)}, 
        //                    {"评审专家抽取审核", new Tuple<权限[], bool>(new[]{权限.评审专家抽取审核}, false)}, 
        //                    {"历史抽取记录", new Tuple<权限[], bool>(new[]{权限.所有评审专家历史抽取记录}, false)},

        //                    {"供应商抽取申请", new Tuple<权限[], bool>(new[]{权限.供应商抽取申请}, false)}, 
        //                    {"供应商抽取审核", new Tuple<权限[], bool>(new[]{权限.供应商抽取审核}, false)}, 
        //                    {"我提交的供应商抽取申请", new Tuple<权限[], bool>(new[]{权限.供应商抽取申请,权限.供应商抽取}, false)}, 
        //                    {"供应商历史抽取记录", new Tuple<权限[], bool>(new[]{权限.所有供应商历史抽取记录}, false)}, 

        //                    {"推荐供应商", new Tuple<权限[], bool>(new[]{权限.查看我的推荐供应商与专家}, false)}, 
        //                    {"打印已审核推荐的供应商", new Tuple<权限[], bool>(new[]{权限.打印已审核推荐的评审专家和供应商}, false)},
        //                    {"审核推荐的供应商", new Tuple<权限[], bool>(new[]{权限.审核推荐供应商与评审专家}, false)}, 
        //                    {"推荐评审专家", new Tuple<权限[], bool>(new[]{权限.查看我的推荐供应商与专家}, false)}, 
        //                    {"打印已审核推荐的评审专家", new Tuple<权限[], bool>(new[]{权限.打印已审核推荐的评审专家和供应商}, false)},
        //                    {"审核推荐的评审专家", new Tuple<权限[], bool>(new[]{权限.审核推荐供应商与评审专家}, false)}, 

        //                    {"新增竞价信息公告", new Tuple<权限[], bool>(new[]{权限.新增网上竞价项目}, false)}, 
        //                    {"未完成网上竞价项目", new Tuple<权限[], bool>(new[]{权限.所有网上竞价项目查看}, false)}, 
        //                    {"已完成网上竞价项目", new Tuple<权限[], bool>(new[]{权限.所有网上竞价项目查看}, false)}, 

        //                    {"审核验收单", new Tuple<权限[], bool>(new[]{权限.审核验收单}, false)},
        //                    {"已作废验收单", new Tuple<权限[], bool>(new[]{权限.审核验收单}, false)},
        //                    {"我的已审核验收单", new Tuple<权限[], bool>(new[]{权限.查看我审核的验收单},false)},
        //                    {"验收单列表", new Tuple<权限[], bool>(new[]{权限.验收单查看}, false)}, 

        //                    {"投诉列表", new Tuple<权限[], bool>(new[]{权限.已处理投诉查看}, false)}, 

        //                    {"项目需求申请", new Tuple<权限[], bool>(new[]{权限.提报采购需求}, false)}, 
        //                    {"我的需求列表", new Tuple<权限[], bool>(new[]{权限.提报采购需求}, false)}, 
        //                    {"审核需求申请", new Tuple<权限[], bool>(new[]{权限.审核项目需求申请}, false)}, 
        //                    {"历史需求列表", new Tuple<权限[], bool>(new[]{权限.所有历史需求查看}, false)}, 
        //                    {"招标采购项目列表", new Tuple<权限[], bool>(new[]{权限.查看招标采购预报名}, false)}, 

        //                    {"待评分项目服务列表", new Tuple<权限[], bool>(new[]{权限.项目服务评价}, false)}, 
        //                    {"已评分项目服务列表", new Tuple<权限[], bool>(new[]{权限.项目服务评价}, false)}, 

        //                    {"招标采购预报名列表", new Tuple<权限[], bool>(new[]{权限.查看招标采购预报名}, false)}, 

        //                    {"添加办事指南", new Tuple<权限[], bool>(new[]{权限.新增办事指南}, false)}, 
        //                    {"办事指南列表", new Tuple<权限[], bool>(new[]{权限.查看办事指南}, false)}, 

        //                    {"添加培训资料", new Tuple<权限[], bool>(new[]{权限.新增培训}, false)}, 
        //                    {"培训资料列表", new Tuple<权限[], bool>(new[]{权限.查看业务培训}, false)}, 
        //                     {"专家人数统计", new Tuple<权限[], bool>(new[]{权限.查看专家数量}, false)}, 

        //                };
        //                HttpContext.批量权限验证(ref p);
        //                Dictionary<string, object> dg;

        //                var m_nocontact = new Dictionary<string, object>
        //                {
        //                    {
        //                        "后台首页", new Dictionary<string, object>
        //                        {
        //                            {"欢迎页面", "单位用户后台/Index"},
        //                            {"修改密码", "单位用户后台/MessageKeyword"},
        //                            #if INTRANET
        //                            {"修改联系人信息", "单位用户后台/MessageModify"},
        //                            #endif
        //                        }
        //                    },
        //                    {
        //                        "消息管理", new Dictionary<string, object>
        //                        {
        //                            {
        //                                "站内消息", new Dictionary<string, object>
        //                                {
        //                                    {"发新消息", "单位用户后台/Procure_ZnxxAdd"},
        //                                    {"已收消息", "单位用户后台/Procure_Znxx"},
        //                                    {"已发消息", "单位用户后台/Procure_ZnxxSend"},
        //                                }
        //                            },
        //                        }
        //                    }
        //                };

        //                //在内网，密码为123456或者联系人信息未填写，则不能使用网站其他功能
        //                if (WebApiApplication.IsIntranet && (
        //                    user.登录信息.密码 == "7C4A8D09CA3762AF61E59520943DC26494F8941B" ||
        //                    user.联系方式 == null || string.IsNullOrWhiteSpace(user.联系方式.联系人) || user.联系方式.联系人 == "联系人" || string.IsNullOrWhiteSpace(user.联系方式.手机)
        //                    ))
        //                {
        //                    return PartialView(m_nocontact);
        //                }


        //                var m = new Dictionary<string, object>
        //                {
        //                    {
        //                        "后台首页", new Dictionary<string, object>
        //                        {
        //                            {"欢迎页面", "单位用户后台/Index"},
        //                            #if INTRANET
        //                            {"修改密码", "单位用户后台/MessageKeyword"},
        //                            {"修改联系人信息", "单位用户后台/MessageModify"},
        //                            #endif
        //                        }
        //                    },
        //                    {
        //                        "消息管理", new Dictionary<string, object>
        //                        {
        //                            {
        //                                "站内消息", new Dictionary<string, object>
        //                                {
        //                                    {"发新消息", "单位用户后台/Procure_ZnxxAdd"},
        //                                    {"已收消息", "单位用户后台/Procure_Znxx"},
        //                                    {"已发消息", "单位用户后台/Procure_ZnxxSend"},
        //                                }
        //                            },
        //                        }
        //                    },
        //                    #if INTRANET
        //                      {
        //                        "采购需求管理", new Dictionary<string, object>
        //                        {
        //                            {"编制需求计划","单位用户后台/Demandlist?page=1"},
        //                            {"审核需求计划","单位用户后台/DemandCheck?page=1"},
        //                             {"编制采购任务","单位用户后台/AssignmentTaskList"},
        //                             {"审核采购任务","单位用户后台/AssignmentTaskAuditList"},
        //                             {"受理采购任务","单位用户后台/AssignmentTaskList_p"},
        //                        }
        //                    },
        //                    {
        //                        "网上竞价管理", new Dictionary<string, object>
        //                        {
        //                            {"新增竞价信息公告", "单位用户后台/OnlineBidding_Add"},
        //                            {"未完成网上竞价项目", "单位用户后台/OnlineBidding_List"},
        //                            {"已完成网上竞价项目", "单位用户后台/OnlineBidding_List_Ed"},
        //                        }
        //                    },
        //                    #endif
        //                    {
        //                        "公告管理", dg = new Dictionary<string, object>
        //                        {
        //                            {"发布公告", "单位用户后台/Procure_AdAdd"},
        //                            {"我的公告列表", "单位用户后台/Procure_AdSendList_S"},
        //                        }
        //                    },
        //                };
        //#if INTRANET
        //                dg.Add("审核公告", "单位用户后台/Procure_AdSendList");
        //                dg.Add("公告列表", "单位用户后台/Procure_AdList");

        //                m.Add("供应商管理", new Dictionary<string, object>
        //                        {
        //                            {"待审核供应商", "单位用户后台/Gys_Examine"},
        //                            {"已审核供应商", "单位用户后台/Gys_Examined"},
        //                        });
        //                m.Add("新闻管理", new Dictionary<string, object>
        //                        {
        //                            {"发布新闻", "单位用户后台/Procure_NewsAdd"},
        //                            {"我的新闻列表", "单位用户后台/Procure_NewsList_S"},
        //                            {"审核新闻", "单位用户后台/Procure_NewsList_Audit"},
        //                            {"新闻列表", "单位用户后台/Procure_NewsList"},
        //                        });
        //                m.Add("通知管理", new Dictionary<string, object>
        //                        {
        //                            {"发布通知", "单位用户后台/Procure_TzAdd"},
        //                            {"我的通知列表", "单位用户后台/Procure_TzList_S"},
        //                            {"审核通知", "单位用户后台/Procure_TzList_Audit"},
        //                            {"通知列表", "单位用户后台/Procure_TzList"},
        //                        });
        //                m.Add("政策法规管理", new Dictionary<string, object>
        //                        {
        //                            {"发布政策法规", "单位用户后台/Procure_ZcfgAdd"},
        //                            {"我的政策法规列表", "单位用户后台/Procure_ZcfgList_S"},
        //                            {"审核政策法规", "单位用户后台/Procure_ZcfgList_Audit"},
        //                            {"政策法规列表", "单位用户后台/Procure_ZcfgList"},
        //                        });
        //                m.Add("单位用户管理", new Dictionary<string, object>
        //                        {
        //                            {"添加单位用户", "单位用户后台/DepartmentAdd"},
        //                            {"单位用户列表", "单位用户后台/DepartmentList"},
        //                            {"审核单位用户", "单位用户后台/DepartmentAuditList"},
        //                            {"打印用户名单", "单位用户后台/Print_UserList"},
        //                        });

        //                m.Add("权限管理", new Dictionary<string, object>
        //                    {
        //                        {"添加用户组", "单位用户后台/User_group_Add"},
        //                        {"用户组管理", "单位用户后台/Usergroup_Mannage"},
        //                        {"我的用户组管理", "单位用户后台/Usergroup_Mannage_My"},

        //                        //{"添加角色", "单位用户后台/Roles_Add"},
        //                        //{"角色管理", "单位用户后台/Role_Mannage"},
        //                    });
        //#endif

        //                m.Add("验收单管理", new Dictionary<string, object>
        //                        {
        //                            {"审核验收单", "单位用户后台/AcceptanceList"},
        //                            {"已作废验收单", "单位用户后台/AcceptanceList_Avoid"},
        //                            {"我的已审核验收单", "单位用户后台/Acceptance_checked"},
        //                            {"验收单列表", "单位用户后台/Acceptanced_List"},
        //                        });

        //#if INTRANET
        //                m.Add("评审专家管理", new Dictionary<string, object>
        //                    {
        //                        {"添加评审专家", "专家抽选/Expert_Add"},
        //                        //{"评审专家列表", "专家抽选/Expert_list"},
        //                        {"评审专家抽取申请", "专家抽选/Expert_Applay"},
        //                        {"我提交的评审专家抽取申请", "专家抽选/Expert_Applay_S"},//包含历史我的历史抽取记录
        //                        {"评审专家抽取审核", "专家抽选/Expert_ApplayAuditList"},
        //                        {"历史抽取记录", "专家抽选/Expert_History"},
        //                    });
        //                m.Add("供应商抽取", new Dictionary<string, object>
        //                    {
        //                        {"供应商抽取申请", "专家抽选/GysChoose_Applay"},
        //                        {"我提交的供应商抽取申请", "专家抽选/GysChoose_Applay_S"},//包含历史我的历史抽取记录
        //                        {"供应商抽取审核", "专家抽选/GysChoose_ApplayAuditList"},
        //                        {"供应商历史抽取记录", "专家抽选/GysChoose_HistoryList"},
        //                    });
        //                m.Add("推荐供应商与评审专家", new Dictionary<string, object>
        //                    {
        //                       {"推荐供应商", "单位用户后台/Recommend_GysList"},
        //                        {"审核推荐的供应商", "单位用户后台/Recommend_GysList_Audit"},
        //                        {"打印已审核推荐的供应商", "单位用户后台/Print_Supplier"},
        //                       {"推荐评审专家", "单位用户后台/Recommend_ExpertList"},
        //                        {"审核推荐的评审专家", "单位用户后台/Recommend_ExpertList_Audit"},
        //                        {"打印已审核推荐的评审专家", "单位用户后台/Print_Expert"},
        //                    });

        //                //m.Add("投诉管理", new Dictionary<string, object>
        //                //    {
        //                //       {"投诉列表", "单位用户后台/ComplainList"},
        //                //    });

        //                m.Add("办事指南", new Dictionary<string, object>
        //                    {
        //                        {"添加办事指南", "单位用户后台/GuideAdd"},
        //                        {"办事指南列表", "单位用户后台/GuideList"},
        //                    });
        //                m.Add("培训资料", new Dictionary<string, object>
        //                    {
        //                        {"添加培训资料", "单位用户后台/TrainingAdd"},
        //                        {"培训资料列表", "单位用户后台/TrainingList"},
        //                    });
        //                m.Add("数据统计", new Dictionary<string, object>
        //                        {
        //                            {"专家人数统计", "单位用户后台/ExpertStatistic"},
        //                        });
        //                // m.Add("采购需求管理", new Dictionary<string, object>
        //                //    {
        //                //        {"项目需求申请", "单位用户后台/Project_Applay"},
        //                //        {"我的需求列表", "单位用户后台/Project_List"},
        //                //        {"审核需求申请", "单位用户后台/Project_AuditList"},
        //                //        {"历史需求列表", "单位用户后台/Project_HistoryList"},
        //                //        {"招标采购项目列表", "单位用户后台/Project_ZbList"},
        //                //    });
        //                //m.Add("项目管理", new Dictionary<string, object>
        //                //    {
        //                //        {"待评分项目服务列表", "单位用户后台/Gys_PreScoreList"},
        //                //        {"已评分项目服务列表", "单位用户后台/Gys_AfScoreList"},
        //                //    });

        //                //m.Add("招标采购预报名管理", new Dictionary<string, object>
        //                //    {
        //                //        {"招标采购预报名列表", "单位用户后台/SignUp_List"},
        //                //    });

        //#endif


        //                var l = m.Keys.ToArray();
        //                foreach (var sm in l)
        //                {
        //                    if (m[sm] is string && !p[sm].Item2) { m.Remove(sm); continue; }
        //                    var m1 = m[sm] as Dictionary<string, object>;
        //                    var l1 = m1.Keys.ToArray();
        //                    foreach (var sm1 in l1)
        //                    {
        //                        if (m1[sm1] is string && !p[sm1].Item2) { m1.Remove(sm1); continue; }
        //                        if (m1[sm1] is Dictionary<string, object>)
        //                        {
        //                            var m2 = m1[sm1] as Dictionary<string, object>;
        //                            var l2 = m2.Keys.ToArray();
        //                            foreach (var sm2 in l2)
        //                            {
        //                                if (m2[sm2] is string && !p[sm2].Item2) m2.Remove(sm2);
        //                            }
        //                            if (m2.Keys.Count <= 0) { m1.Remove(sm1); }
        //                        }
        //                    }
        //                    if (m1.Keys.Count <= 0) { m.Remove(sm); }
        //                }

        //                return PartialView(m);
        //            }
        //            else
        //            {
        //                ViewData["单位状态"] = HttpContext.获取当前用户<单位用户>().审核数据.审核状态.ToString();
        //                return PartialView();
        //            }
        //        }

        private ActionResult 运营团队菜单()
        {
            //var p1 = new[] { 权限.商品库 };
            //var p2 = new[] { 权限.历史商品数据, 权限.历史采购结果 };
            //var p = new Dictionary<权限[], bool>
            //{
            //    {p1, false}, //采用单一权限验证
            //    {p2, true}, //采用多重权限验证
            //};
            //HttpContext.批量权限验证(ref p);
            var m = new Dictionary<string, object>
            {
                {
                    "后台首页",new Dictionary<string,object>
                    {
                        {"欢迎页面", "运营团队后台/Index"}
                    }
                },
                 {
                    "信息管理", new Dictionary<string, object>
                    {
                        {
                            "系统通知", "运营团队后台/SystemInfo"
                        },
                        {
                            "站内消息", new Dictionary<string, object>
                            {
                                {"发新消息" , "运营团队后台/ZNXX_Add"},
                                {"已收消息", "运营团队后台/ZNXX"},
                                {"已发消息", "运营团队后台/ZNXX_Sended"},
                            }
                        },
                         {
                            "投诉建议", new Dictionary<string, object>
                            {
                                {
                                    "投诉", new Dictionary<string, object>
                                    {
                                        {"已处理投诉" , "运营团队后台/Suggestion"},
                                        {"处理中投诉" , "运营团队后台/Suggestion_Dealing"},
                                        {"未处理投诉" , "运营团队后台/Suggestion_NoPss"},
                                    }
                                },
                                {
                                    "建议", new Dictionary<string, object>
                                    {
                                        {"已处理建议" , "运营团队后台/Requestion_Replied"},
                                        {"处理中建议" , "运营团队后台/Requestion_Dealing"},
                                        {"未处理建议" , "运营团队后台/Requestion_No_Replied"},
                                    }
                                },
                            }
                        },
                         {
                            "推送信息", new Dictionary<string, object>
                            {
                                {"推送信息管理", "运营团队后台/SendMessageList"},
                                {"推送信息审核", "运营团队后台/SendMessageAuditList"},
                            }
                        },
                        {
                            "公告管理", new Dictionary<string, object>
                            {
                                {"公告列表", "运营团队后台/Procure_AdList"},
                                {"发布公告", "运营团队后台/Procure_AdAdd"},
                                {"审核公告", "运营团队后台/Procure_AdAudit"},
                                {"上传公告", "运营团队后台/Procure_AdUpload"},
                            }
                        },
                        {
                            "新闻管理", new Dictionary<string, object>
                            {
                                {"新闻列表", "运营团队后台/Procure_NewsList"},
                                {"发布新闻", "运营团队后台/Procure_NewsAdd"},
                            }
                        },
                        {
                            "政策法规管理", new Dictionary<string, object>
                            {
                                {"政策法规列表", "运营团队后台/Procure_ZcfgList"},
                                {"发布政策法规", "运营团队后台/Procure_ZcfgAdd"},
                            }
                        },
                        {
                            "通知管理", new Dictionary<string, object>
                            {
                                {"通知列表", "运营团队后台/Procure_TzList"},
                                {"发布通知", "运营团队后台/Procure_TzAdd"},
                            }
                        },
                        {
                            "办事指南管理", new Dictionary<string, object>
                            {
                                {"办事指南列表", "运营团队后台/GuideList"},
                                {"发布办事指南", "运营团队后台/GuideAdd"},
                            }
                        },
                        {
                            "业务培训管理", new Dictionary<string, object>
                            {
                                {"业务培训列表", "运营团队后台/TrainingList"},
                                {"发布业务培训", "运营团队后台/TrainingAdd"},
                            }
                        },
                        {
                            "消息模板管理", new Dictionary<string, object>
                            {
                                {"消息模板", "运营团队后台/Msg_TemplateManage"},
                            }
                        },
                    }
                },
                {
                    "业务管理",new Dictionary<string,object>
                    {
                        {
                            "商品管理", new Dictionary<string, object>
                            {
                                {"商品目录管理", "运营团队后台/GoodClassify"},
                                {"商品列表", "运营团队后台/GoodExamine"},
                            }
                        },
                        {
                            "酒店管理",new Dictionary<string,object>
                            {
                                {"待审核酒店","运营团队后台/HotelExamine"},
                                {"已审核酒店","运营团队后台/HotelExamined"}
                            }
                        },
                        {
                            "机票代售点管理",new Dictionary<string,object>
                            {
                                {"待审核代售点","运营团队后台/TicketExamine"},
                                {"已审核代售点","运营团队后台/TicketExamined"},
                                {"机票验收单管理","运营团队后台/TicketList"},
                            }
                        },
                        {
                            "验收单管理",new Dictionary<string,object>
                            {
                                {"验收单列表","运营团队后台/Acceptanced_List"},
                            }
                        },
                        {
                            "网上报价管理",new Dictionary<string,object>
                            {
                                {"添加网上报价","运营团队后台/OnlineBidding_Add"},
                                {"未完成网上报价列表","运营团队后台/OnlineBidding_List"},
                                {"已完成网上报价列表","运营团队后台/OnlineBidding_List_Ed"},
                            }
                        },
                    }
                },
                {
                    "网站用户管理", new Dictionary<string, object>
                    {
                        {
                           "供应商信息管理", new Dictionary<string, object>
                            {
                                {"供应商列表", "运营团队后台/Supplier_PssInfo?page=1"},
                                {"添加供应商", "运营团队后台/Add_Supplier_Info"},
                            }
                        },
                        {
                            "单位管理", new Dictionary<string, object>
                            {
                                {"单位列表", "运营团队后台/DepartmentList"},
                                {"单位添加", "运营团队后台/DepartmentAdd"},
                                {"单位审核", "运营团队后台/DepartmentAuditList"},
                                {"打印用户名单", "运营团队后台/Print_UserList"},
                            }
                        },
                        {
                            "专家管理", new Dictionary<string, object>
                            {
                                {"专家列表", "运营团队后台/Expert_List?page=1"},
                                #if INTRANET
                                      {"专家批量修改", "运营团队后台/ExpertBatchmodify"},
                                #endif
                            }
                         
                        },
                        {
                            "个人用户管理", new Dictionary<string, object>
                            {
                                //{"专家列表", "运营团队后台/Expert_List?page=1"},
                            }
                         
                        },
                        {
                            "运营团队管理", new Dictionary<string, object>
                            {
                                //{"专家列表", "运营团队后台/Expert_List?page=1"},
                            }
                         
                        },
                    }
                },
                {
                    "广告管理", new Dictionary<string, object>
                    {
                        {"供应商推广列表", "运营团队后台/Recommendgys"},
                        {"商品推广列表", "运营团队后台/RecommendGood"},
#if !INTRANET
                        {"军采通申请列表", "运营团队后台/AccountInfoManage"},
#endif
                    }
                },
                {
                    "项目服务管理", new Dictionary<string, object>
                    {
                        {"项目服务列表", "运营团队后台/ProjectService_List"},
                    }
                },
                {
                    "招标采购预报名管理", new Dictionary<string, object>
                    {
                        {"招标采购预报名列表", "运营团队后台/SignUp_List"},
                    }
                },
                {
                    "下载管理", new Dictionary<string, object>
                    {
                        {"添加下载", "运营团队后台/DownloadAdd"},
                        {"下载管理", "运营团队后台/DownloadManage"},
                    }
                },
                {
                    "权限管理", new Dictionary<string, object>
                    {
                        {
                             "单位用户权限管理", new Dictionary<string, object>
                            {
                                {"添加用户组", "运营团队后台/Usergroup_Add"},
                                {"用户组管理", "运营团队后台/Usergroup_Mannage"},
                                {"添加角色", "运营团队后台/Role_Add"},
                                {"角色管理", "运营团队后台/Role_Mannage"},
                            }
                        },
                        {
                            "运营团队权限管理", new Dictionary<string, object>
                            {
                                //{"添加用户组", "运营团队后台/Usergroup_Add"},
                                //{"用户组管理", "运营团队后台/Usergroup_Mannage"},
                                //{"添加角色", "运营团队后台/Role_Add"},
                                //{"角色管理", "运营团队后台/Role_Mannage"},
                            }
                        },
                    }
                },
                {
                    "推荐管理", new Dictionary<string, object>
                    {
                        {
                            "推荐供应商管理", new Dictionary<string, object>
                            {
                                {"已联系推荐供应商", "运营团队后台/Recommend_GysList_Ed"},
                                {"待联系推荐供应商", "运营团队后台/Recommend_GysList_Pre"},
                            }
                        },
                        {
                            "推荐评审专家管理", new Dictionary<string, object>
                            {
                                {"已联系推荐评审专家", "运营团队后台/Recommend_ExpertList_Ed"},
                                {"待联系推荐评审专家", "运营团队后台/Recommend_ExpertList_Pre"},
                            }
                        },
                    }
                },
                {
                    "预报名管理",new Dictionary<string,object>
                    {
                        {"预报名管理","运营团队后台/RegistrationManange"},
                        {"预报名统计","运营团队后台/RegistrationCount"},
                    }
                },
                {
                    "企业信用查询", new Dictionary<string, object>
                    {
                        {"全国企业信用信息公示系统", "运营团队后台/redirect1"},
                        {"四川信用网", "运营团队后台/redirect2"},
                    }
                },
                {
                    "数据与统计管理",new Dictionary<string,object>
                    {
                        {
                            "数据统计",new Dictionary<string,object>
                            {
                                {"验收单统计","运营团队后台/AcceptanceStatistic"},
                                {"公告统计","运营团队后台/AdStatistic"},
                                //{"政策法规统计","运营团队后台/AdStatistic"},
                                //{"新闻统计","运营团队后台/AdStatistic"},
                                //{"通知统计","运营团队后台/AdStatistic"},
                                {"供应商统计","运营团队后台/GysStatistic"},
                                {"评审专家统计","运营团队后台/ExpertStatistic"},
                                {"商品属性统计","运营团队后台/GoodAttrStatistic"},
                                {"商品数量统计","运营团队后台/GoodStatistic"},
                                {"登录统计","运营团队后台/StatisticsLogin"},
                                {"广告点击统计","运营团队后台/StatisticsClick"},
                                {"网站运营情况统计","运营团队后台/OperationInfo"},
                            }
                        },
                         {
                            "数据处理",new Dictionary<string,object>
                            {
                                {"导出数据","运营团队后台/DbExport"},
                                {"导入数据","运营团队后台/DbInport"},
                                {"同步验收单","运营团队后台/SynchroYsd"},
                            }
                        },
                    }
                },
            };
            //var m = new Dictionary<string, object>
            //{
            //    {
            //        "一级菜单名1", new Dictionary<string, object>
            //        {
            //            {
            //                "二级菜单名1", new Dictionary<string, object>
            //                {
            //                    {p[p1] ? "三级菜单名1" : null, "controller/action"},
            //                    {"三级菜单名2", "controller/action"},
            //                    {"三级菜单名3", "controller/action"},
            //                    {"三级菜单名4", "controller/action"},
            //                }
            //            },
            //            {
            //                "二级菜单名2", "controller/action"
            //            },
            //        }
            //    },
            //};
            return PartialView(m);
        }
        private ActionResult 个人用户菜单()
        {
            //var p1 = new[] { 权限.商品库 };
            //var p2 = new[] { 权限.历史商品数据, 权限.历史采购结果 };
            //var p = new Dictionary<权限[], bool>
            //{
            //    {p1, false}, //采用单一权限验证
            //    {p2, true}, //采用多重权限验证
            //};
            //HttpContext.批量权限验证(ref p);
            var m = new Dictionary<string, object>
            {
                 {
                    "后台首页", new Dictionary<string, object>
                    {
                        {
                            "欢迎页面", "个人用户后台/Index"
                        },
                    }
                },
                {
                    "个人信息管理", new Dictionary<string, object>
                    {
                        {
                            "基本信息", "个人用户后台/MyBaseInfo"
                        },
                        {
                            "会员信息","个人用户后台/VipInfo"
                        },
                        
                    }
                },
                {
                    "订单管理", new Dictionary<string, object>
                    {
                        {
                            "我的购物车", "个人用户后台/ShopCar"
                        },
                        {
                            "我的订单", "个人用户后台/PurchaseInfo"
                        },
                    }
                },
            };
            //var m = new Dictionary<string, object>
            //{
            //    {
            //        "一级菜单名1", new Dictionary<string, object>
            //        {
            //            {
            //                "二级菜单名1", new Dictionary<string, object>
            //                {
            //                    {p[p1] ? "三级菜单名1" : null, "controller/action"},
            //                    {"三级菜单名2", "controller/action"},
            //                    {"三级菜单名3", "controller/action"},
            //                    {"三级菜单名4", "controller/action"},
            //                }
            //            },
            //            {
            //                "二级菜单名2", "controller/action"
            //            },
            //        }
            //    },
            //};
            return PartialView(m);
        }

    }
}
