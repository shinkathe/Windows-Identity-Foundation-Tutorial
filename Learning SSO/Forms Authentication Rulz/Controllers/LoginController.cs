using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace Forms_Authentication_Rulz.Controllers
{
    public class LoginController : Controller
    {
        [Route("login")]
        public ActionResult Index()
        {
            return View();
        }

        [Route("submit")]
        public ActionResult Login(string username, string password)
        {
            if (password == "test")
            {
                FormsAuthentication.SetAuthCookie(username, true);
            }
            return Redirect("/home");
        }
	}
}