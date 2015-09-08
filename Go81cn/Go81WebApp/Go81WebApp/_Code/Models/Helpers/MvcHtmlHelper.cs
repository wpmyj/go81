namespace System.Web.Mvc
{
    public static class MvcHtmlHelper
    {
        public static MvcHtmlString ActionLinkWithImage(this HtmlHelper html, string imgSrc, string actionName)
        {
            var urlHelper = new UrlHelper(html.ViewContext.RequestContext);

            string imgUrl = urlHelper.Content(imgSrc);
            TagBuilder imgTagBuilder = new TagBuilder("img");
            imgTagBuilder.MergeAttribute("src", imgUrl);
            string img = imgTagBuilder.ToString(TagRenderMode.SelfClosing);

            string url = urlHelper.Action(actionName);

            TagBuilder tagBuilder = new TagBuilder("a") { InnerHtml = img };
            tagBuilder.MergeAttribute("href", url);

            return new MvcHtmlString(tagBuilder.ToString(TagRenderMode.Normal));
        }
        public static MvcHtmlString ActionLinkWithImage(this HtmlHelper html, string imgSrc, string actionName, string controllerName = null, object routeValue = null)
        {
            var urlHelper = new UrlHelper(html.ViewContext.RequestContext);

            string imgUrl = urlHelper.Content(imgSrc);
            TagBuilder imgTagBuilder = new TagBuilder("img");
            imgTagBuilder.MergeAttribute("src", imgUrl);
            string img = imgTagBuilder.ToString(TagRenderMode.SelfClosing);

            string url = urlHelper.Action(actionName, controllerName, routeValue);

            TagBuilder tagBuilder = new TagBuilder("a") { InnerHtml = img };
            tagBuilder.MergeAttribute("href", url);

            return new MvcHtmlString(tagBuilder.ToString(TagRenderMode.Normal));
        }
        public static MvcHtmlString ActionLinkWithImage(this AjaxHelper ajax, string imgSrc, string actionName)
        {
            var urlHelper = new UrlHelper(ajax.ViewContext.RequestContext);

            string imgUrl = urlHelper.Content(imgSrc);
            TagBuilder imgTagBuilder = new TagBuilder("img");
            imgTagBuilder.MergeAttribute("src", imgUrl);
            string img = imgTagBuilder.ToString(TagRenderMode.SelfClosing);

            string url = urlHelper.Action(actionName);

            TagBuilder tagBuilder = new TagBuilder("a") { InnerHtml = img };
            tagBuilder.MergeAttribute("href", url);

            return new MvcHtmlString(tagBuilder.ToString(TagRenderMode.Normal));
        }
        public static MvcHtmlString ActionLinkWithImage(this AjaxHelper ajax, string imgSrc, string actionName, string controllerName = null, object routeValue = null)
        {
            var urlHelper = new UrlHelper(ajax.ViewContext.RequestContext);

            string imgUrl = urlHelper.Content(imgSrc);
            TagBuilder imgTagBuilder = new TagBuilder("img");
            imgTagBuilder.MergeAttribute("src", imgUrl);
            string img = imgTagBuilder.ToString(TagRenderMode.SelfClosing);

            string url = urlHelper.Action(actionName, controllerName, routeValue);

            TagBuilder tagBuilder = new TagBuilder("a") { InnerHtml = img };
            tagBuilder.MergeAttribute("href", url);

            return new MvcHtmlString(tagBuilder.ToString(TagRenderMode.Normal));
        }
    }
}