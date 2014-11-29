using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.IdentityModel.Services;
using System.Security.Claims;
using System.IdentityModel.Configuration;
using System.IdentityModel.Tokens;
using System.Security.Cryptography.X509Certificates;
using STS.Util;
using STS.Core;
using System.Security.Principal;

namespace STS.Controllers
{
    public class LogoutViewModel
    {
        public List<string> LogoutUrls;
        public string ReplyTo;
    }

    public class AuthenticationService {
        public const string SignOutLiteral = "wsignout1.0";
        public const string SignOutCleanupLiteral = "wsignoutcleanup1.0";
        public const string SignInLiteral = "wsignin1.0";
        public const string RealmLiteral = "wtrealm";
        public const string ReplyLiteral = "wreply";
        /// <summary>
        /// User hadn't logged in before, so set an authentication cookie in the WSFed response for this domain
        /// </summary>
        /// 
        private IPrincipal AuthenticateAndCreateCookie(string realm)
        {
            var user = new ClaimsIdentity(AuthenticationTypes.Federation);
            user.AddClaim(new Claim(ClaimTypes.Name, "test"));
            user.AddClaim(new Claim(ClaimTypes.Uri, realm));

            var claimsPrincipal = new ClaimsPrincipal(user);
            var sessionSecurityToken = new SessionSecurityToken(claimsPrincipal, TimeSpan.FromDays(365));
            FederatedAuthentication.SessionAuthenticationModule.WriteSessionTokenToCookie(sessionSecurityToken);
            return claimsPrincipal;
        }

        /// <summary>
        /// The user had already signed in from one domain, sign the user in from the previously set cookie
        /// </summary>
        private IPrincipal PreviouslyAuthenticated(string realm)
        {
            var user = new ClaimsIdentity(AuthenticationTypes.Federation);
            user.AddClaim(new Claim(ClaimTypes.Uri, realm));
            return new ClaimsPrincipal(user);;
        }

        public ActionResult ProcessRequest()
        {
            var message = WSFederationMessage.CreateFromUri(HttpContext.Current.Request.Url);
            var realm = message.GetParameter(RealmLiteral);
            var reply = message.GetParameter(ReplyLiteral);
            return message.Action == SignOutLiteral ? SignOut(reply) : SignIn(realm);
        }
        
        private ActionResult SignOut(string replyTo)
        {
            FederatedAuthentication.SessionAuthenticationModule.SignOut();
            var ci = (ClaimsIdentity)HttpContext.Current.User.Identity;
            var logoutUrls = ci.FindAll(i => i.Type == ClaimTypes.Uri).Select(i => string.Format("{0}?wa={1}", i.Value, SignOutCleanupLiteral)).ToList();
            var model = new LogoutViewModel { LogoutUrls = logoutUrls, ReplyTo = replyTo };
            var viewResult = new ViewResult
            {
                ViewName = "~/Views/Shared/Logout.cshtml",
                ViewData = new ViewDataDictionary(model)
            };
            return viewResult;
        }

        private ActionResult SignIn(string realm)
        {
            var user = HttpContext.Current.User.Identity.IsAuthenticated ? PreviouslyAuthenticated(realm) : AuthenticateAndCreateCookie(realm);
            var config = new SecurityTokenServiceConfiguration(true)
            {
                SigningCredentials = new X509SigningCredentials(CertificateUtil.GetCertificate(StoreName.My, StoreLocation.LocalMachine, "CN=learningSSO")),
                TokenIssuerName = "http://sts.local/"
            };

            FederatedPassiveSecurityTokenServiceOperations.ProcessRequest(HttpContext.Current.Request, (ClaimsPrincipal) user, new CustomTokenService(config), HttpContext.Current.Response);
            return new HttpStatusCodeResult(HttpStatusCode.OK);
        }
    }

    public class AuthenticationController : Controller
    {
        [Route("")]
        public ActionResult Index()
        {
            var authenticator = new AuthenticationService();
            return authenticator.ProcessRequest();
        }
	}
}