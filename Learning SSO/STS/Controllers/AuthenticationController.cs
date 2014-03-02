using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IdentityModel;
using System.IdentityModel.Claims;
using System.IdentityModel.Services;
using System.Diagnostics;

namespace STS.Controllers
{
    public class AuthenticationController : Controller
    {
        //
        // GET: /Authentication/
        [Route("")]
        public void Index()
        {
            var wsFederationMessage = WSFederationMessage.CreateFromUri(Request.Url);
            Trace.Write(String.Join(",", wsFederationMessage.Parameters.Select(i => i.Key + " - " + i.Value)));
        }
	}
}