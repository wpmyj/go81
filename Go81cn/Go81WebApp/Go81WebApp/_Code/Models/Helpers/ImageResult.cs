using System.Drawing;
using System.Drawing.Imaging;

namespace System.Web.Mvc
{
    public class ImageResult : ActionResult
    {
        // 图片
        public Image Image { get; set; }
        // 构造器
        public ImageResult(Image image)
        {
            Image = image;
        }
        // 主要需要重写的方法
        public override void ExecuteResult(ControllerContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }
            // 设置 HTTP Header
            context.HttpContext.Response.ContentType = "image/gif";
            // 将图片数据写入Response
            Image.Save(context.HttpContext.Response.OutputStream, ImageFormat.Gif);
        }
    }
}