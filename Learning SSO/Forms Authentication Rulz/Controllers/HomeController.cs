using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Forms_Authentication_Rulz.Controllers
{

    [Authorize]
    public class HomeController : Controller
    {
        [Route("home")]
        [Route("")]
        public ActionResult Index()
        {
            return View();
        }

        [Route("about")]
        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }
    }
}