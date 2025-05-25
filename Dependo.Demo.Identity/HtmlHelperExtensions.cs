using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Dependo.Demo.Identity;

public static class HtmlHelperExtensions
{
    public static IHtmlContent Test(this IHtmlHelper html)
    {
        var test = DependoResolver.Instance.Resolve<UserManager<IdentityUser>>();
        var users = test.Users.ToList();

        return new HtmlString("Hello");
    }
}
