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
    public class Authentication {

        private IPrincipal PreAuthenticate()
        {
            var ci = new ClaimsIdentity(AuthenticationTypes.Federation);
            ci.AddClaim(new Claim(ClaimTypes.Name, "test"));
            var claimsPrincipal = new ClaimsPrincipal(ci);

            var s = new SessionSecurityToken(claimsPrincipal, TimeSpan.FromDays(365));
            FederatedAuthentication.SessionAuthenticationModule.WriteSessionTokenToCookie(s);
            return claimsPrincipal;
        }

        private IPrincipal PreAuthenticateIsAuthenticated()
        {
            var s = System.Web.HttpContext.Current.User;
            return s;
        }

        public void Authenticate()
        {
            var user = System.Web.HttpContext.Current.User.Identity.IsAuthenticated ? PreAuthenticateIsAuthenticated() : PreAuthenticate();
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
            var s = new Authentication();

            s.Authenticate();
        }
	}
}