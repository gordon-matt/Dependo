using Microsoft.AspNetCore.Mvc;

namespace Dependable.Demo.Controllers
{
    public class HelloWorldController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
