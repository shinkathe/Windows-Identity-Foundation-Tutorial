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
using AutoMapper;
using System.Security.Cryptography.X509Certificates;
using System.IdentityModel.Selectors;


namespace STS.Controllers
{
    public class CertificateUtil
    {
        public static X509Certificate2 GetCertificate(StoreName name, StoreLocation location, string subjectName)
        {
            X509Store store = new X509Store(name, location);
            X509Certificate2Collection certificates = null;
            store.Open(OpenFlags.ReadOnly);

            try
            {
                X509Certificate2 result = null;

                //
                // Every time we call store.Certificates property, a new collection will be returned.
                //
                certificates = store.Certificates;

                for (int i = 0; i < certificates.Count; i++)
                {
                    X509Certificate2 cert = certificates[i];

                    if (cert.SubjectName.Name.ToLower() == subjectName.ToLower())
                    {
                        if (result != null)
                        {
                            throw new ApplicationException(string.Format("There are multiple certificates for subject Name {0}", subjectName));
                        }

                        result = new X509Certificate2(cert);
                    }
                }

                if (result == null)
                {
                    throw new ApplicationException(string.Format("No certificate was found for subject Name {0}", subjectName));
                }

                return result;
            }
            finally
            {
                if (certificates != null)
                {
                    for (int i = 0; i < certificates.Count; i++)
                    {
                        X509Certificate2 cert = certificates[i];
                        cert.Reset();
                    }
                }

                store.Close();
            }
        }
    }

    public class CustomTokenService : SecurityTokenService
    {
        public CustomTokenService(SecurityTokenServiceConfiguration config) : base(config)
        {
        }

        protected override ClaimsIdentity GetOutputClaimsIdentity(ClaimsPrincipal principal, System.IdentityModel.Protocols.WSTrust.RequestSecurityToken request, Scope scope)
        {
            return principal.Identity as ClaimsIdentity;
        }

        protected override Scope GetScope(ClaimsPrincipal principal, System.IdentityModel.Protocols.WSTrust.RequestSecurityToken request)
        {
            var s = new Scope();
            s.SigningCredentials = SecurityTokenServiceConfiguration.SigningCredentials; 
            s.TokenEncryptionRequired = false;
            s.SymmetricKeyEncryptionRequired = false;
            s.ReplyToAddress = request.ReplyTo;
            s.AppliesToAddress = request.AppliesTo.Uri.ToString();
            return s;
        }
    }

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