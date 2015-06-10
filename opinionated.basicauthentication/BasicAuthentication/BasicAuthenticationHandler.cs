﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Security.Claims;

using Microsoft.Owin.Security.Infrastructure;
using Microsoft.Owin.Security;

namespace opinionated.BasicAuthentication
{
    class BasicAuthenticationHandler : AuthenticationHandler<BasicAuthenticationOptions>
    {
        public BasicAuthenticationHandler(BasicAuthenticationOptions options)
        {
            _challenge = "Basic realm=" + options.Realm;
        }

        protected override async Task<AuthenticationTicket> AuthenticateCoreAsync()
        {
            var authzValue = Request.Headers.Get("Authorization");
            if (string.IsNullOrEmpty(authzValue) || !authzValue.StartsWith("Basic ", StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            var token = authzValue.Substring("Basic ".Length).Trim();
            var claims = await TryGetPrincipalFromBasicCredentials(token, Options.CredentialValidationFunction);

            if (claims == null)
            {
                return null;
            }
            else
            {
                var id = new ClaimsIdentity(claims, Options.AuthenticationType);
                return new AuthenticationTicket(id, new AuthenticationProperties());
            }
        }

        protected override Task ApplyResponseChallengeAsync()
        {
            if (Response.StatusCode == 401)
            {
                var challenge = Helper.LookupChallenge(Options.AuthenticationType, Options.AuthenticationMode);
                if (challenge != null)
                {
                    Response.Headers.AppendValues("WWW-Authenticate", _challenge);
                }
            }

            return Task.FromResult<object>(null);
        }

        async Task<IEnumerable<Claim>> TryGetPrincipalFromBasicCredentials(string credentials,
            BasicAuthenticationMiddleware.CredentialValidationFunction validate)
        {
            string pair;
            try
            {
                pair = Encoding.UTF8.GetString(
                    Convert.FromBase64String(credentials));
            }
            catch (FormatException)
            {
                return null;
            }
            catch (ArgumentException)
            {
                return null;
            }

            var ix = pair.IndexOf(':');
            if (ix == -1)
            {
                return null;
            }

            var username = pair.Substring(0, ix);
            var pw = pair.Substring(ix + 1);

            return await validate(username, pw);
        }

        readonly string _challenge;
    }

}
