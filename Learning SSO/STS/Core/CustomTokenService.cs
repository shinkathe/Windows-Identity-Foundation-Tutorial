using System;
using System.Collections.Generic;
using System.IdentityModel;
using System.IdentityModel.Configuration;
using System.Linq;
using System.Security.Claims;
using System.Web;

namespace STS.Core
{
    public class CustomTokenService : SecurityTokenService
    {
        public CustomTokenService(SecurityTokenServiceConfiguration config)
            : base(config)
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
}