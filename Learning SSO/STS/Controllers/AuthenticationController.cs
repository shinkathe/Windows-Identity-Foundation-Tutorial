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

        // Load certificate
        X509Certificate2 LoadCertificate()
        {
            return new X509Certificate2(
                string.Format(@"{0}\bin\Certificate.pfx", AppDomain.CurrentDomain.BaseDirectory), "");
        }

        /// <summary>
        /// User hadn't logged in before, so set an authentication cookie in the WSFed response for this domain
        /// </summary>
        private IPrincipal AuthenticateAndCreateCookie(string relyingPartyUrl)
        {
            var user = new ClaimsIdentity(AuthenticationTypes.Federation);
            user.AddClaim(new Claim(ClaimTypes.Name, "test"));
            user.AddClaim(new Claim(ClaimTypes.Uri, relyingPartyUrl));
            return SaveToCookie(user);
        }

        /// <summary>
        /// The user had already signed in from one domain, sign the user in from the previously set cookie
        /// </summary>
        private IPrincipal PreviouslyAuthenticated(string relyingPartyUrl)
        {
            var user = (ClaimsIdentity)HttpContext.Current.User.Identity;
            user.AddClaim(new Claim(ClaimTypes.Uri, relyingPartyUrl));
            return SaveToCookie(user);
        }

        /// <summary>
        /// Save user state to cookie
        /// </summary>
        /// <param name="user">User to save to cookie</param>
        /// <returns></returns>
        private static ClaimsPrincipal SaveToCookie(ClaimsIdentity user)
        {
            var claimsPrincipal = new ClaimsPrincipal(user);
            var sessionSecurityToken = new SessionSecurityToken(claimsPrincipal, TimeSpan.FromDays(365));
            FederatedAuthentication.SessionAuthenticationModule.WriteSessionTokenToCookie(sessionSecurityToken);
            return claimsPrincipal;
        }

        /// <summary>
        /// Figure out, if the user wants to sign in or sign out, and do the correct path based on that
        /// </summary>
        /// <returns></returns>
        public ActionResult ProcessRequest()
        {
            // Pull the request apart
            var message = WSFederationMessage.CreateFromUri(HttpContext.Current.Request.Url);
            // Get the relying party url
            var relyingPartyUrl = HttpContext.Current.Request.UrlReferrer.ToString();
            // Get reply to address
            var reply = message.GetParameter(ReplyLiteral);
            // Sign out, if the action or wa-parameter is "wsignout1.0", and sign in otherwise
            return message.Action == SignOutLiteral ? SignOut(reply, relyingPartyUrl) : SignIn(relyingPartyUrl);
        }

        /// <summary>
        /// Find out which realms the user is signed in - sign them out from all of them, and return to @replyTo
        /// </summary>
        /// <param name="replyTo">Redirect to this address after signout is done</param>
        /// <param name="relyingPartyUrl"></param>
        /// <returns>A bit of html, which renders images with signout urls for all domains.</returns>
        private ActionResult SignOut(string replyTo, string relyingPartyUrl)
        {
            // First, remove the session authentication cookie for the STS
            FederatedAuthentication.SessionAuthenticationModule.SignOut();
            var ci = (ClaimsIdentity)HttpContext.Current.User.Identity;
            // Get all urls where the user has signed in previously, and make them into a list of strings with the format "{url}?wa=wsignoutcleanup1.0"
            var logoutUrls = ci.FindAll(i => i.Type == ClaimTypes.Uri).Select(i => string.Format("{0}?wa={1}", i.Value, SignOutCleanupLiteral)).ToList();
            // Construct a viewmodel from the logout urls and replyto address
            var model = new LogoutViewModel { LogoutUrls = logoutUrls, ReplyTo = replyTo };
            // Add relying party url if it isn't in there, because in some cases, a client might call signout even though the local STS cookie has expired 
            var relyingPartyUrlCleanup = string.Format("{0}?wa={1}", relyingPartyUrl, SignOutCleanupLiteral);
            if (!logoutUrls.Contains(relyingPartyUrlCleanup)) logoutUrls.Add(relyingPartyUrlCleanup);
            // Build a viewresult object and return that
            var viewResult = new ViewResult
            {
                ViewName = "~/Views/Shared/Logout.cshtml",
                ViewData = new ViewDataDictionary(model)
            };
            return viewResult;
        }

        private ActionResult SignIn(string replyToAddress)
        {
            var user = HttpContext.Current.User.Identity.IsAuthenticated ? PreviouslyAuthenticated(replyToAddress) : AuthenticateAndCreateCookie(replyToAddress);
            var config = new SecurityTokenServiceConfiguration("http://sts.local", new X509SigningCredentials(LoadCertificate()));

            FederatedPassiveSecurityTokenServiceOperations.ProcessRequest(HttpContext.Current.Request, (ClaimsPrincipal) user, new CustomTokenService(config), HttpContext.Current.Response);
            return new HttpStatusCodeResult(HttpStatusCode.OK);
        }
    }

    /// <summary>
    /// Entry point into our STS
    /// </summary>
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