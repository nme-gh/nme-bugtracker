using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace nme_bugtracker.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return View();
            }
            else
            {
                if (User.IsInRole("Submitter"))
                {
                    return RedirectToAction("UserTickets", "Tickets");
                }
                else if (User.IsInRole("Admin") || User.IsInRole("Project Manager") || User.IsInRole("Developer"))
                {
                    // Admin: Dashboard[Current Tickets] or SideNav[Tickets] goes to "All Tickets". On "All Tickets", the user will have a separate button reads 'View MyTickets' and goes to "UserTickets".
                    // Non-Admin (ProjMngr and Developer): Dashboard[Current Tickets] or SideNav[Tickets] goes to "UserTickets"
                    return RedirectToAction("Dashboard", "Home");
                }
            }
            return View();
        }

        [Authorize(Roles = "Admin, Project Manager, Developer")]
        // GET: Gets the Dashboard with Morris Charts
        public ActionResult Dashboard()
        {
            ViewBag.ActionName = "UserTickets";
            if (User.IsInRole("Admin"))
            {
                ViewBag.ActionName = "Index";
            }
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}