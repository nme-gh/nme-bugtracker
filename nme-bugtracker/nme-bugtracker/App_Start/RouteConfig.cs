using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace nme_bugtracker
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            // Due to User's Role, if side navigation is limited  (e.g. submitter can't access/see Dashboard and Projects but can see Tickets only),
            // this Route should kick in and re-route the user to Controller:Tickets / Action: UserTickects.

            // root/Tickets/UserTickets
            //routes.MapRoute(
            //    name: "TicketsOnly",
            //    url: "Tickets/{action}/{id}",
            //    defaults: new { controller = "Tickets", action = "UserTickets", id = UrlParameter.Optional }
            //);

            // root/Home/Index ==> root/Home/Dashboard
            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}
