using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.OAuth;
using WebApp.Models;
using System.Net;

namespace WebApp.Providers
{
    public class ApplicationOAuthProvider : OAuthAuthorizationServerProvider
    {
        private readonly string _publicClientId;

        //string UserId = "";
        //string RoleID = "";
        //string UserRoleID = null;
        private string Index = "";
        string ULHID = null;
        public ApplicationOAuthProvider(string publicClientId)
        {
            if (publicClientId == null)
            {
                throw new ArgumentNullException("publicClientId");
            }

            _publicClientId = publicClientId;
        }

        public override async Task GrantResourceOwnerCredentials(OAuthGrantResourceOwnerCredentialsContext context)
        {
            var userManager = context.OwinContext.GetUserManager<ApplicationUserManager>();

            ApplicationUser user = await userManager.FindAsync(context.UserName, context.Password);

            if (user == null)
            {
                context.SetError("invalid_grant", "The user name or password is incorrect.");
                return;
            }
            if (!user.EmailConfirmed)
            {
                context.SetError("invalid_grant", "Account pending approval.");
                return;
            }
            ClaimsIdentity oAuthIdentity = await user.GenerateUserIdentityAsync(userManager,
                OAuthDefaults.AuthenticationType);
            ClaimsIdentity cookiesIdentity = await user.GenerateUserIdentityAsync(userManager,
                CookieAuthenticationDefaults.AuthenticationType);

            AuthenticationProperties properties = CreateProperties(user.UserName);
            AuthenticationTicket ticket = new AuthenticationTicket(oAuthIdentity, properties);
            context.Validated(ticket);
            context.Request.Context.Authentication.SignIn(cookiesIdentity);

            using (WebAppEntities db = new WebAppEntities())
            {
                try
                {
                    Index = db.AspNetUserRoles.Where(x => x.UserId == user.Id).FirstOrDefault().AspNetRole.IndexPage;
                }
                catch (Exception)
                {
                    //Index = "/Admin/Role";
                }

                using (var dbContextTransaction = db.Database.BeginTransaction())
                {
                    try
                    {
                        ULHID = Guid.NewGuid().ToString();
                        string ip = "";
                        System.Web.HttpContext cont = System.Web.HttpContext.Current;
                        string ipAddress = cont.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
                        if (!string.IsNullOrEmpty(ipAddress))
                        {
                            string[] addresses = ipAddress.Split(',');
                            if (addresses.Length != 0)
                            {
                                ip = addresses[0];
                            }
                        }
                        ip = cont.Request.ServerVariables["REMOTE_ADDR"];
                        AspNetUsersLoginHistory anulh = new AspNetUsersLoginHistory();
                        anulh.vULHID = ULHID;
                        anulh.Id = user.Id;
                        anulh.dLogIn = DateTime.UtcNow;
                        anulh.nvIPAddress = ip;
                        db.AspNetUsersLoginHistories.Add(anulh);
                        db.SaveChanges();

                        dbContextTransaction.Commit();
                    }
                    catch
                    {
                        dbContextTransaction.Rollback();
                    }
                }
            }
        }

        public override Task TokenEndpoint(OAuthTokenEndpointContext context)
        {
            foreach (KeyValuePair<string, string> property in context.Properties.Dictionary)
            {
                context.AdditionalResponseParameters.Add(property.Key, property.Value);
            }
            context.AdditionalResponseParameters.Add("index", Index);
            context.AdditionalResponseParameters.Add("ULHID", ULHID);

            return Task.FromResult<object>(null);
        }

        public override Task ValidateClientAuthentication(OAuthValidateClientAuthenticationContext context)
        {
            // Resource owner password credentials does not provide a client ID.
            if (context.ClientId == null)
            {
                context.Validated();
            }

            return Task.FromResult<object>(null);
        }

        public override Task ValidateClientRedirectUri(OAuthValidateClientRedirectUriContext context)
        {
            if (context.ClientId == _publicClientId)
            {
                Uri expectedRootUri = new Uri(context.Request.Uri, "/");

                if (expectedRootUri.AbsoluteUri == context.RedirectUri)
                {
                    context.Validated();
                }
            }

            return Task.FromResult<object>(null);
        }

        public static AuthenticationProperties CreateProperties(string userName)
        {
            IDictionary<string, string> data = new Dictionary<string, string>
            {
                { "userName", userName }
            };
            return new AuthenticationProperties(data);
        }
    }
}