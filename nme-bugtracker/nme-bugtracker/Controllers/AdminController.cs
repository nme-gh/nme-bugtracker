using nme_bugtracker.Helpers;
using nme_bugtracker.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace nme_bugtracker.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        private UserRolesHelper uh = new UserRolesHelper();

        // GET: Admin
        public ActionResult Index()
        {
            return View();
        }

        // GET: AssignRoleOnUsers
        public ActionResult AddUserToRole()
        {
            ViewBag.RoleName = new SelectList(db.Roles, "Name", "Name");
            ViewBag.Users = new MultiSelectList(db.Users, "Id", "FullName");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        // POST: AssignRoleOnUsers
        // NME - Optional UrlParameters might be ViewBag types as in here and usually start with a capital as opposed to common C# naming convention.
        public ActionResult AddUserToRole(string RoleName, List<string> Users)
        {
            if (RoleName == null ||  Users == null)
                return RedirectToAction("Index");

            var errors = ModelState.Values.SelectMany(v => v.Errors); // NME: To remove
            if (ModelState.IsValid)
            {
                foreach (var userId in Users)
                {
                    if (!uh.IsUserInRole(userId, RoleName))
                        uh.AddUserToRole(userId, RoleName);
                }
                return RedirectToAction("Index");
            }

            ViewBag.Users = new SelectList(db.Roles, "Id", "Name", RoleName); // return the value before (where I left off)
            ViewBag.Users = new MultiSelectList(db.Users, "Id", "FullName", Users); // return the value before (where I left off)
            return View();
        }

        // GET: UnassignRoleOnUsers
        public ActionResult RemoveUserFromRole()
        {
            ViewBag.RoleName = new SelectList(db.Roles, "Name", "Name");
            ViewBag.Users = new MultiSelectList(db.Users, "Id", "FullName");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        // POST: UnassignRoleOnUsers
        // NME - Optional UrlParameters might be ViewBag types as in here and usually start with a capital as opposed to common C# naming convention.
        public ActionResult RemoveUserFromRole(string RoleName, List<string> Users)
        {
            if (RoleName == null || Users == null)
                return RedirectToAction("Index");

            var errors = ModelState.Values.SelectMany(v => v.Errors); // NME: To remove
            if (ModelState.IsValid)
            {
                foreach (var userId in Users)
                {
                    if (uh.IsUserInRole(userId, RoleName))
                        uh.RemoveUserFromRole(userId, RoleName);
                }
                return RedirectToAction("Index");
            }

            ViewBag.Users = new SelectList(db.Roles, "Id", "Name", RoleName); // return the value before (where I left off)
            ViewBag.Users = new MultiSelectList(db.Users, "Id", "FullName", Users); // return the value before (where I left off)
            return View();
        }

    }
}
