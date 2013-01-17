using System;
using System.Web.Mvc;

namespace Ektron.Contrib.Samples.Web.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
	        Session["UtcNow"] = DateTime.UtcNow;

            return View();
        }

	    public ActionResult Two()
	    {
			var dateTime = ((DateTime)Session["UtcNow"]);
			Session["UtcNow"] = dateTime.AddYears(1);

		    return View("Index");
	    }
    }
}
