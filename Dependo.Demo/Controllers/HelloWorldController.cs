using Microsoft.AspNetCore.Mvc;

namespace Dependo.Demo.Controllers
{
    public class HelloWorldController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
