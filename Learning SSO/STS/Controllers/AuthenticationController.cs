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


namespace STS.Controllers
{
    public class AuthenticationController : Controller
    {
        [Route("")]
        public void Index()
        {
                // Let's create a mock identity - in the real scenario we'd build this identity against some auth store
                var ci = new ClaimsIdentity(AuthenticationTypes.Federation);
                ci.AddClaim(new Claim(ClaimTypes.Name, "test"));
                var claimsPrincipal = new ClaimsPrincipal(ci);

                var config = new SecurityTokenServiceConfiguration(true);
                config.SigningCredentials = new X509SigningCredentials(CertificateUtil.GetCertificate(StoreName.My, StoreLocation.LocalMachine, "CN=learnSSO"));
                config.TokenIssuerName = "http://localhost:59171/";
                FederatedPassiveSecurityTokenServiceOperations.ProcessRequest(System.Web.HttpContext.Current.Request, claimsPrincipal, new CustomTokenService(config), System.Web.HttpContext.Current.Response);
        }
	}
}