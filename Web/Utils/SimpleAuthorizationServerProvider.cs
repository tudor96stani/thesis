using Microsoft.Owin.Security;
using Microsoft.Owin.Security.OAuth;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity.EntityFramework;

namespace Web.Utils
{
    public class SimpleAuthorizationServerProvider : OAuthAuthorizationServerProvider
    {
        public override async Task ValidateClientAuthentication(OAuthValidateClientAuthenticationContext context)
        {
            context.Validated();
        }

        public override async Task GrantResourceOwnerCredentials(OAuthGrantResourceOwnerCredentialsContext context)
        {
            context.OwinContext.Response.Headers.Add("Access-Control-Allow-Origin", new[] { "*" });

            using (AuthRepository _repo = new AuthRepository())
            {
                //var logger = NLog.LogManager.GetCurrentClassLogger();
                //logger.Debug($"GrantResourceOwnerCredentials username={context.UserName}");
                IdentityUser user = await _repo.FindUser(context.UserName, context.Password);

                if (user == null)
                {
                    context.SetError("invalid_grant", "The user name or password is incorrect.");
                    return;
                }
                
                   
                 var properties = new Dictionary<string, string>()
                {
                    { ClaimTypes.NameIdentifier, user.Id},
                    { ClaimTypes.Name, context.UserName }
                };
                var identity = new ClaimsIdentity(context.Options.AuthenticationType);
                //properties.ToList().ForEach(c => identity.AddClaim(new Claim(c.Key, c.Value)));
                foreach(var kvp in properties)
                {
                    identity.AddClaim(new Claim(kvp.Key, kvp.Value));
                }


                //AuthenticationProperties properties = CreateProperties(user.UserName, user.Id.ToString(), context.ClientId);
                //AuthenticationTicket ticket = new AuthenticationTicket(identity, properties);
                var ticket = new AuthenticationTicket(identity, new AuthenticationProperties(properties));
                context.Validated(ticket);
                context.Request.Context.Authentication.SignIn(identity);
            }
        }

      


        public static AuthenticationProperties CreateProperties(string userName, string id, string clientid)
        {
            IDictionary<string, string> data = new Dictionary<string, string>
            {
                { "Username", userName },
                
                { "Id", id }
            };
            return new AuthenticationProperties(data);
        }

        public override Task TokenEndpoint(OAuthTokenEndpointContext context)
        {
            foreach (KeyValuePair<string, string> property in context.Properties.Dictionary)
            {
                context.AdditionalResponseParameters.Add(property.Key, property.Value);
            }
            return Task.FromResult<object>(null);
        }
    }
}