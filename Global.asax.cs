using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace Wikimedia
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            //var culture = new CultureInfo("fr-FR");
            //CultureInfo.DefaultThreadCurrentCulture = culture;
            //CultureInfo.DefaultThreadCurrentUICulture = culture;

            //Clean_Data();
        }
        protected void Session_End()
        {
            var connectedUser = Models.User.ConnectedUser;
            if (connectedUser != null)
                connectedUser.Online = false;
        }
    }
}
