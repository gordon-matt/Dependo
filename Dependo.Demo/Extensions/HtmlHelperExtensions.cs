using Dependo.Demo.Services;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Dependo.Demo.Extensions;

public static class HtmlHelperExtensions
{
    extension(IHtmlHelper html)
    {
        public IHtmlContent HelloWorld()
        {
            var service = DependoResolver.Instance.Resolve<IHelloWorldService>();
            return new HtmlString(service.HelloWorld());
        }
    }
}