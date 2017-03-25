using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using nme_bugtracker.Models;
using nme_bugtracker.Helpers;
using Microsoft.AspNet.Identity;

namespace nme_bugtracker.Controllers
{
    [Authorize]
    public class ProjectsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        private ProjectsHelper ph = new ProjectsHelper();
        private UsersHelper uh = new UsersHelper(); // NME


        // GET: MyProjects
        public ActionResult Index()
        {
            //var projects = db.Projects.Include(p => p.CreatedBy); // NME: Lists/Gets what? 
            //var projects = db.Projects; // Lists/Gets all the projects
            //return View(projects.ToList());

            var user = db.Users.Find(User.Identity.GetUserId());
            var projects = ph.ListProjectsForUser(user.Id); // Lists/Gets the projects of an user
            return View(projects.ToList());
        }

        [Authorize(Roles = "Admin, Project Manager")]
        // GET: AllProjects
        public ActionResult IndexAll()
        {
            var projects = db.Projects; // Lists/Gets all the projects
            return View(projects.ToList());
        }

        // GET: Projects/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Project project = db.Projects.Find(id);
            if (project == null)
            {
                return HttpNotFound();
            }
            return View(project);
        }

        [Authorize(Roles = "Admin, Project Manager")]
        // GET: Projects/Create
        public ActionResult Create()
        {
            //ViewBag.CreatedById = new SelectList(db.Users, "Id", "DisplayName");
            ViewBag.CreatedById = new SelectList(db.Users, "Id", "UserNameDisplayName"); // returns a users list with their UserNameDisplayName, so during runtime, one can be selected on the view and be passed to HttpPost.
            return View();
        }

        // POST: Projects/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        //public ActionResult Create([Bind(Include = "Id,Name,Created,CreatedById")] Project project)
        public ActionResult Create([Bind(Include = "Name")] Project project)
        {
            if (ModelState.IsValid)
            {
                var user = db.Users.Find(User.Identity.GetUserId());
                //project.CreatedById = user.Id; // NME: Add new property AuthorName
                project.Created = DateTimeOffset.Now;
                db.Projects.Add(project);
                db.SaveChanges();
                return RedirectToAction("IndexAll");
            }

            //ViewBag.CreatedById = new SelectList(db.Users, "Id", "DisplayName", project.CreatedById);
            //ViewBag.CreatedById = new SelectList(db.Users, "Id", "UserNameDisplayName", project.CreatedById); // returns only one user in the users list with the UserNameDisplayName, which was the user selected during runtime, but due to an error HttpGet View will be called again and previously selected user will be already displayed as selected.
            ViewBag.CreatedById = new SelectList(db.Users, "Id", "UserNameDisplayName", project.AuthorName); // returns only one user in the users list with the UserNameDisplayName, which was the user selected during runtime, but due to an error HttpGet View will be called again and previously selected user will be already displayed as selected.
            return View(project);
        }

        [Authorize(Roles = "Admin, Project Manager")]
        // GET: Projects/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Project project = db.Projects.Find(id);
            if (project == null)
            {
                return HttpNotFound();
            }
            return View(project);
        }

        // POST: Projects/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id, Created, Updated, CreatedById,   Name")] Project project)
        {
            if (ModelState.IsValid)
            {
                project.Updated = DateTimeOffset.Now;
                db.Entry(project).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("IndexAll");
            }
            return View(project);
        }

        [Authorize(Roles = "Admin, Project Manager")]
        // GET: Projects/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Project project = db.Projects.Find(id);
            if (project == null)
            {
                return HttpNotFound();
            }
            return View(project);
        }

        // POST: Projects/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Project project = db.Projects.Find(id);
            db.Projects.Remove(project);
            db.SaveChanges();
            return RedirectToAction("IndexAll");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        [Authorize(Roles = "Admin, Project Manager")]
        // GET: AssignUsers
        public ActionResult AddUserToProject(int? projectId)
        {
            if (projectId == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            // TODO: What difference below 2 lines have? Find() vs FirstOrDefault()?
            Project project = db.Projects.Find(projectId);
            //project = db.Projects.FirstOrDefault(p => p.Id == projectId);
            if (project == null)
            {
                return HttpNotFound();
            }

            // user ids of those currently on the project
            var assignedUserIds = ph.UserIdsOnProject((int)projectId);
            // NME - Remember ViewBag is not strongly typed!
            // MultiSelectList: List of all users with the assigned ones highlighted
            ViewBag.Users = new MultiSelectList(db.Users, "Id", "FullName", assignedUserIds);
            return View(project);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        // POST: AssignUsers
        // NME - Optional UrlParameters might be ViewBag types as in here and usually start with a capital as opposed to common C# naming convention.
        public ActionResult AddUserToProject(int projectId, List<string> Users)
        {
            if (Users == null)
                return RedirectToAction("IndexAll");

            var errors = ModelState.Values.SelectMany(v => v.Errors); // NME: To remove
            if (ModelState.IsValid)
            {
                foreach (var userId in Users)
                {
                    ph.AddUserToProject(userId, projectId);
                }
                return RedirectToAction("IndexAll");
            }

            ViewBag.Users = new MultiSelectList(db.Users, "Id", "FullName", Users); // return the value before (where I left off)
            return View();
        }

        [Authorize(Roles = "Admin, Project Manager")]
        // GET: UnassignUsers
        public ActionResult RemoveUserFromProject(int? projectId)
        {
            if (projectId == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Project project = db.Projects.Find(projectId);
            if (project == null)
            {
                return HttpNotFound();
            }

            int projId = (int)projectId;
            // users currently on the project
            var userList = ph.UsersOnProject(projId);

            // user ids of those currently on the project
            var assignedUserIds = ph.UserIdsOnProject(projId);
            // NME - Remember ViewBag is not strongly typed!
            // MultiSelectList: List of currently assigned users while all of those highlighted
            ViewBag.AssignedUsers = new MultiSelectList(userList, "Id", "FullName", assignedUserIds);
            return View(project);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        // POST: UnassignUsers
        // NME - Optional UrlParameters might be ViewBag types as in here and usually start with a capital as opposed to common C# naming convention.
        public ActionResult RemoveUserFromProject(int projectId, List<string> AssignedUsers)
        {
            if (AssignedUsers == null)
                return RedirectToAction("IndexAll", "Projects");

            var errors = ModelState.Values.SelectMany(v => v.Errors); // NME: To remove
            if (ModelState.IsValid)
            {
                foreach (var userId in AssignedUsers)
                {
                    ph.RemoveUserFromProject(userId, projectId);
                }
                return RedirectToAction("IndexAll");
            }

            // users currently on the project
            var userList = ph.UsersOnProject(projectId);

            ViewBag.AssignedUsers = new MultiSelectList(userList, "Id", "FullName", AssignedUsers);  // return the value before (where I left off)
            return View();
        }

#region UserRoles Methods: Need to be moved to Admin controller
        // UserRoles Methods: Need to be moved to Admin controller

        [Authorize(Roles = "Admin")]
        // GET: Projects/AssignUsersRole
        public ActionResult AssignUsersRole()
        {
            ViewBag.RoleId = new SelectList(db.Roles, "Id", "Name");
            ViewBag.Users = new MultiSelectList(db.Users, "Id", "FullName");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AssignUsersRole(string RoleId, List<string> Users)
        {

            var errors = ModelState.Values.SelectMany(v => v.Errors);
            if (ModelState.IsValid)
            {
                foreach (var usr in Users)
                {
                    uh.AssignUserToRole(usr, RoleId); 
                }
                return RedirectToAction("Index");
            }

            ViewBag.RoleId = new SelectList(db.Roles, "Id", "Name", RoleId); // return the value before (where I left off)
            ViewBag.Users = new MultiSelectList(db.Users, "Id", "FullName", Users);  // return the value before (where I left off)
            return View();
        }

        [Authorize(Roles = "Admin")]
        // GET: Projects/UnassignUsersRole
        public ActionResult UnassignUsersRole()
        {
            ViewBag.RoleId = new SelectList(db.Roles, "Id", "Name");
            ViewBag.Users = new MultiSelectList(db.Users, "Id", "FullName");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UnassignUsersRole(string RoleId, List<string> Users)
        {

            var errors = ModelState.Values.SelectMany(v => v.Errors);
            if (ModelState.IsValid)
            {
                foreach (var usr in Users)
                {
                    uh.UnassignUserFromRole(usr, RoleId);
                }
                return RedirectToAction("Index");
            }

            ViewBag.RoleId = new SelectList(db.Roles, "Id", "Name", RoleId); // return the value before (where I left off)
            ViewBag.Users = new MultiSelectList(db.Users, "Id", "FullName", Users);  // return the value before (where I left off)
            return View();
        }
#endregion


#region (Un)Assign users to project - Old/Primitive Way of Implementation
        // NME: [ProjectUsers] is being utilized. Remove this action later.
        [Authorize(Roles = "Admin, Project Manager")]
        //[NonAction]
        // GET: Projects/AssignProjects
        public ActionResult AssignProjects()
        {
            //ViewBag.ProjectId = new SelectList(db.Projects, "Id", "Name");
            ViewBag.Projects = new SelectList(db.Projects, "Id", "Name");
            ViewBag.Users = new MultiSelectList(db.Users, "Id", "FullName");
            return View();
        }

        // POST: Projects/AssignProjects
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        //[NonAction]
        [HttpPost]
        [ValidateAntiForgeryToken]
        // NME: ViewBag usage for SelectList() or MultiSelectList():
        // ViewBag names used for SelectList() in the HttpGet method above should match the parameter names in the HttpPost method below.
        ////public ActionResult AssignProjects([Bind(Include = "ProjectId,UserId")] ProjectUser pu) // This syntax is not valid for SelectList() usage!!!
        //public ActionResult AssignProjects([Bind(Include = "ProjectId")] ProjectUser pu, List<string> Users) // Further on ViewBag: If ViewBag name is for Bind, then it must match the property name of the object used to bind to. In this case ViewBag.ProjectId is binding to object ProjectUser.[ProjectId] on property ProjectId.
        public ActionResult AssignProjects(string Projects, List<string> Users)
        {
            if (ModelState.IsValid)
            {
                //var localPU = new ProjectUser();
                ////localPU.ProjectId = pu.ProjectId;
                //localPU.ProjectId = Int32.Parse(Projects);
                //foreach (var usr in Users)
                //{
                //    localPU.UserId = usr;

                //    // TODO: Possible to have a Duplicate check for an entity without accessing DB?
                //    var dupPU = db.ProjectUsers.AsNoTracking().SingleOrDefault(x => x.ProjectId == localPU.ProjectId && x.UserId == localPU.UserId);
                //    if (dupPU == null)
                //    {
                //        db.ProjectUsers.Add(localPU);
                //        db.SaveChanges();
                //    }
                //}

                foreach (var usr in Users)
                {
                    var projectId = Int32.Parse(Projects);
                    ProjectUser projuser = db.ProjectUsers.Find(projectId, usr);
                    if (projuser == null)
                    {
                        db.ProjectUsers.Add(new ProjectUser(projectId, usr));
                        //var temp_pu = new ProjectUser { ProjectId = projectId , UserId = usr};
                        //db.ProjectUsers.Add(temp_pu);
                        db.SaveChanges();
                    }
                }
                return RedirectToAction("Index");
            }

            //ViewBag.ProjectId = new SelectList(db.Projects, "Id", "Name", pu.ProjectId); // return the value before (where I left off)
            ViewBag.Projects = new SelectList(db.Projects, "Id", "Name", Projects); // return the value before (where I left off)
            ViewBag.Users = new MultiSelectList(db.Users, "Id", "FullName", Users);  // return the value before (where I left off)
            return View();
        }


        [Authorize(Roles = "Admin, Project Manager")]
        // GET: Projects/AddUsers
        public ActionResult AddUsers()
        {
            ViewBag.Id = new SelectList(db.Projects, "Id", "Name");
            ViewBag.Users = new MultiSelectList(db.Users, "Id", "FullName");
            return View();
        }

        // POST: Projects/AddUsers
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        //public ActionResult AddUsers([Bind(Include = "Id")] Project proj, List<string> Users) // ViewBag.Id used for binding is matching to Project.[Id].
        public ActionResult AddUsers(string Id, List<string> Users)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors);
            if (ModelState.IsValid)
            {
                //var localPU = new ProjectUser();
                ////localPU.ProjectId = proj.Id;
                //localPU.ProjectId = Int32.Parse(Id);
                foreach (var usr in Users)
                {
                    //localPU.UserId = usr;
                    ////ph.AddUserToPUsers(localPU);
                    ph.AddUserToProject(usr, Int32.Parse(Id)); // NME: Many2Many EF usage with [ApplicationUsersProjects] table
                }
                return RedirectToAction("Index");
            }

            //ViewBag.Id = new SelectList(db.Projects, "Id", "Name", proj.Id); // return the value before (where I left off)
            ViewBag.Id = new SelectList(db.Projects, "Id", "Name", Id); // return the value before (where I left off)
            ViewBag.Users = new MultiSelectList(db.Users, "Id", "FullName", Users);  // return the value before (where I left off)
            return View();
        }

        [Authorize(Roles = "Admin, Project Manager")]
        // GET: Projects/RemoveUsers
        public ActionResult RemoveUsers(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var users = ph.UsersOnProject((int)id);
            List<ApplicationUser> usersList = users.ToList();
            return View(usersList); // Pass the model to the view.

            //ViewBag.Id = new SelectList(db.Projects, "Id", "Name");
            //ViewBag.Users = new MultiSelectList(db.Users, "Id", "FullName");
        }
#endregion

    }
}
