using Dependable.Demo.Services;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Dependable.Demo.Extensions
{
    public static class HtmlHelperExtensions
    {
        public static IHtmlContent HelloWorld(this IHtmlHelper htmlHelper)
        {
            var service = EngineContext.Current.Resolve<IHelloWorldService>();
            return new HtmlString(service.HelloWorld());
        }
    }
}