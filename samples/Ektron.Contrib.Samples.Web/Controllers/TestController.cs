using System.Web.Mvc;

namespace Ektron.Contrib.Samples.Web.Controllers
{
    public class TestController : Controller
    {
        [OutputCache(Duration = 3600)]
        public ActionResult Index()
        {
            return View();
        }
    }
}
