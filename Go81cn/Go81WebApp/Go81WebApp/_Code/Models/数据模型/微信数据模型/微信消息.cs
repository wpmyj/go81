using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Go81WebApp.Models.数据模型.微信数据模型
{
    public class 微信消息
    {
        /// <summary>
        /// 消息接收方微信号，一般为公众平台账号微信号
        /// </summary>
        public string ToUserName { get; set; }

        /// <summary>
        /// 消息发送方微信号
        /// </summary>
        public string FromUserName { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public string CreateTime { get; set; }

        /// <summary>
        /// 信息类型 地理位置:location,文本消息:text,消息类型:image，音频：voice，视频：video,取消关注：Action
        /// </summary>
        public string MsgType { get; set; }

        /// <summary>
        /// 信息内容
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// 消息ID（文本）
        /// </summary>
        public string MsgId { get; set; }

        /// <summary>
        /// 取消关注时的Event节点
        /// </summary>
        public string Wxevent { get; set; }

        /// <summary>
        /// 取消关注时的EventKey节点
        /// </summary>
        public string EventKey { get; set; }

        /// <summary>
        /// 地理位置纬度
        /// </summary>
        public string Location_X { get; set; }

        /// <summary>
        /// 地理位置经度
        /// </summary>
        public string Location_Y { get; set; }

        /// <summary>
        /// 地图缩放大小
        /// </summary>
        public string Scale { get; set; }

        /// <summary>
        /// 地理位置信息
        /// </summary>
        public string Label { get; set; }

        /// <summary>
        /// 图片链接，开发者可以用HTTP GET获取
        /// </summary>
        public string PicUrl { get; set; }
    }
}