using Go81WebApp.Models.管理器;
using Go81WebApp.Models.数据模型.用户数据模型;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Go81WebApp.Models.数据模型.项目数据模型
{
    public class 需求申请 : 基本数据模型
    {
        public 用户链接<单位用户> 需求提报单位 { get; set; }

        [Required(ErrorMessage = "请填写标题")]
        public string 标题 { get; set; }
        public string 正文 { get; set; }
        //public List<string> 需求物资列表 { get; set; }
        public 需求申请()
        {
            需求提报单位 = new 用户链接<单位用户>();
            //需求物资列表 = new List<string>();
        }
    }
    public class 需求申请链接
    {
        public long 需求申请ID { get; set; }
        public 需求申请 需求申请 { get { return 需求申请管理.查找需求申请(需求申请ID); } }
        public 需求申请链接() { this.需求申请ID = -1; }
    }
}