using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IdentityModel;
using System.IdentityModel.Services;
using System.Diagnostics;
using System.IdentityModel.Protocols.WSFederation;
using System.Security.Claims;
using System.IdentityModel.Configuration;
using System.IdentityModel.Tokens;
using System.Text;
using System.IdentityModel.Protocols.WSTrust;
using System.Security.Cryptography.X509Certificates;
using System.IdentityModel.Selectors;
using STS.Util;
using STS.Core;
using System.Security.Principal;


namespace STS.Controllers
{
    public class Authenticator {
        /// <summary>
        /// User hadn't logged in before, so set an authentication cookie in the WSFed response for this domain
        /// </summary>
        /// 
        private IPrincipal AuthenticateAndCreateCookie()
        {
            var claimsIdentity = new ClaimsIdentity(AuthenticationTypes.Federation);
            claimsIdentity.AddClaim(new Claim(ClaimTypes.Name, "test"));
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

            var sessionSecurityToken = new SessionSecurityToken(claimsPrincipal, TimeSpan.FromDays(365));
            FederatedAuthentication.SessionAuthenticationModule.WriteSessionTokenToCookie(sessionSecurityToken);
            return claimsPrincipal;
        }

        /// <summary>
        /// The user had already signed in from one domain, sign the user in from the previously set cookie
        /// </summary>
        private IPrincipal PreviouslyAuthenticated()
        {
            var user = System.Web.HttpContext.Current.User;
            return user;
        }

        public void Authenticate()
        {
            var user = System.Web.HttpContext.Current.User.Identity.IsAuthenticated ? PreviouslyAuthenticated() : AuthenticateAndCreateCookie();
            var config = new SecurityTokenServiceConfiguration(true);
            config.SigningCredentials = new X509SigningCredentials(CertificateUtil.GetCertificate(StoreName.My, StoreLocation.LocalMachine, "CN=learnSSO"));
            config.TokenIssuerName = "http://sts.local/";
            FederatedPassiveSecurityTokenServiceOperations.ProcessRequest(System.Web.HttpContext.Current.Request, (ClaimsPrincipal)user, new CustomTokenService(config), System.Web.HttpContext.Current.Response); 
        }
    }

    public class AuthenticationController : Controller
    {
        [Route("")]
        public void Index()
        {
            var authenticator = new Authenticator();

            authenticator.Authenticate();
        }
	}
}