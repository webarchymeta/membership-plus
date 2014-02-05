using System;
using System.Collections.Generic;
using System.Configuration;
using System.Web.Configuration;
using Microsoft.AspNet.Identity;
using Microsoft.Owin;
using Microsoft.Owin.Security.Cookies;
using Owin;
using CryptoGateway.RDB.Data.MembershipPlus;

namespace MemberAdminMvc5
{
    public partial class Startup
    {
        internal static CallContext ClientContext
        {
            get;
            set;
        }

        internal static Application_ App
        {
            get;
            set;
        }

        internal static bool MemberInitSuccess = false;
        
        // For more information on configuring authentication, please visit http://go.microsoft.com/fwlink/?LinkId=301864
        public void ConfigureAuth(IAppBuilder app)
        {
            // Enable the application to use a cookie to store information for the signed in user
            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = DefaultAuthenticationTypes.ApplicationCookie,
                LoginPath = new PathString("/Account/Login")
            });
            MembershipPlusServiceProxy svc = new MembershipPlusServiceProxy();
            if (ClientContext == null)
                ClientContext = svc.SignInService(new CallContext(), null);
            CallContext cctx = ClientContext.CreateCopy();
            // Get encryption and decryption key information from the configuration
            Configuration cfg = WebConfigurationManager.OpenWebConfiguration(System.Web.Hosting.HostingEnvironment.ApplicationVirtualPath);
            var machineKey = (MachineKeySection)cfg.GetSection("system.web/machineKey");
            if (machineKey.ValidationKey.Contains("AutoGenerate"))
            {
                throw new Exception("Hashed or Encrypted passwords " +
                                            "are not supported with auto-generated keys.");
            }
            string ApplicationName = ConfigurationManager.AppSettings["ApplicationName"];
            try
            {
                cctx.DirectDataAccess = true;
                Application_ServiceProxy apprepo = new Application_ServiceProxy();
                List<Application_> apps = apprepo.LoadEntityByNature(cctx, ApplicationName);
                if (apps == null || apps.Count == 0)
                {
                    cctx.OverrideExisting = true;
                    var tuple = apprepo.AddOrUpdateEntities(cctx, new Application_Set(), new Application_[] { new Application_ { Name = ApplicationName } });
                    App = tuple.ChangedEntities.Length == 1 && IsValidUpdate(tuple.ChangedEntities[0].OpStatus) ? tuple.ChangedEntities[0].UpdatedItem : null;
                    cctx.OverrideExisting = false;
                }
                else
                    App = apps[0];
                MemberInitSuccess = true;
            }
            catch
            {

            }
        }

        public static bool IsValidUpdate(int status)
        {
            return (status & (int)EntityOpStatus.Added) > 0 || (status & (int)EntityOpStatus.Updated) > 0 || (status & (int)EntityOpStatus.NoOperation) > 0;
        }
    }
}