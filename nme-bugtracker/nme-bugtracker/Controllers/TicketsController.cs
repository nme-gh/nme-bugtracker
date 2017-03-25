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
using System.Threading.Tasks;

namespace nme_bugtracker.Controllers
{
    [Authorize]
    public class TicketsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        private ProjectsHelper ph = new ProjectsHelper();
        private TicketsHelper th = new TicketsHelper();
        private TicketNotificationsHelper tnh = new TicketNotificationsHelper();

        //[Authorize(Roles = "Admin")]
        // GET: Tickets
        public ActionResult Index()
        {
            // Originally scaffolded Index method.
            ////var tickets = db.Tickets.Include(t => t.AssignedToUser).Include(t => t.OwnerUser).Include(t => t.Project).Include(t => t.TicketPriority).Include(t => t.TicketStatus).Include(t => t.TicketType);
            ////return View(tickets.ToList());

/*
 * NME: Accessing Ticket Object Properties
            var myTicket = new Ticket { Id= 5, Title ="My Ticket"};
            foreach (var prop in typeof(Ticket).GetProperties())
            {
                Console.WriteLine(prop.Name); // Id, Title
                Console.WriteLine(prop.GetValue(myTicket)); //5, My Ticket
            }
*/

            var tickets = new List<Ticket>();

            if (User.IsInRole("Admin"))
            {
                //tickets = db.Tickets.ToList(); // Gets all tickets with Guids values.
                tickets = db.Tickets.Include(t => t.AssignedToUser).Include(t => t.OwnerUser).Include(t => t.Project).Include(t => t.TicketPriority).Include(t => t.TicketStatus).Include(t => t.TicketType).ToList(); // Gets all tickeets with values replacing Guid FKs which are navigation property Ids in our model (E.g. TicketType's Name vs TicketType's Id)

            }
            else if (User.IsInRole("Project Manager")) { // Project Manager
                foreach ( var project in ph.ListProjectsForUser(User.Identity.GetUserId()) )
                {
                    foreach (var ticket in project.Tickets)
                    {
                        // Or maybe tickets.AddRange()
                        tickets.Add(ticket);
                    }
                }

            }
            //tickets = db.Tickets.ToList();
            tickets = db.Tickets.Include(t => t.AssignedToUser).Include(t => t.OwnerUser).Include(t => t.Project).Include(t => t.TicketPriority).Include(t => t.TicketStatus).Include(t => t.TicketType).ToList();

            return View(tickets);
        }

        // GET: Tickets/UserTickets
        public ActionResult UserTickets()
        {
            var userId = User.Identity.GetUserId();
            var userTickets = new UserTicketsViewModel();
            userTickets.AssignedTickets = th.ListAssignedTicketsForUser(userId);
            userTickets.OwnedTickets = th.ListOwnedTicketsForUser(userId);
            userTickets.OtherProjectTickets = new List<Ticket>();
            if (!User.IsInRole("Submitter"))
            {
                userTickets.OtherProjectTickets = th.ListOtherTicketsForUser(userId);
            }
            return View(userTickets);
        }

        // GET: Tickets/ChooseProject to create the ticket on
        public ActionResult ChooseProject()
        {
            var projectList = new List<Project>();

            if (User.IsInRole("Admin"))
            {
                // TODO: List/Get ALL the active projects? Change below Ticket and TicketStatus to Project and ProjectStatus!
                //var temp2 = db.Tickets.Where(t => String.Compare(t.TicketStatus.ToString(), "Active")); // String.Compare returns int?
                //var temp3 = db.Tickets.Where(t => t.TicketStatus.ToString().Contains("Active")); // It can be "ActiveX"
                //var temp4 = db.Tickets.Where(t => t.TicketStatus == Enums.Status.Active); // Enums?

                // List/Get ALL the projects
                projectList = db.Projects.ToList();
            } else
            {
                // List/Get the projects of an user
                projectList = ph.ListProjectsForUser(User.Identity.GetUserId()).ToList();
            }
            ViewBag.ProjectId = new SelectList(projectList, "Id", "Name");
            return View();
        }

        // GET: Tickets/CreateTicket
        public ActionResult CreateTicket(string ProjectId)
        {
            // TODO: Multiple Submit Buttons. How to handle a Cancel button bw View and Controller, so we can redirect back to the previous page: //    Redirect(Request.UrlReferrer.ToString());

            if (String.IsNullOrEmpty(ProjectId))
                return RedirectToAction("ChooseProject");

            var project = db.Projects.Find(Int32.Parse(ProjectId));
            if (project != null)
            {
                ViewBag.TicketTypeId = new SelectList(db.TicketTypes, "Id", "Name");
                //ViewBag.TicketPriorityId = new SelectList(db.TicketPriorities, "Id", "Name");
                //ViewBag.AssignedToUserId = new SelectList(db.Users, "Id", "FullName");

                ViewData["ProjectId"] = ProjectId;
                ViewBag.ProjectTitle = project.Name;
                return View();
            }
            else
            {
                return RedirectToAction("ChooseProject");
            }
        }

        // POST: Tickets/CreateTicket
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateTicket([Bind(Include = "ProjectId,Title,Description,TicketTypeId,   TicketPriorityId")] Ticket ticket)
        {
            if (ModelState.IsValid)
            {
                ticket.OwnerUserId = User.Identity.GetUserId();
                ticket.Created = DateTimeOffset.Now;
                ticket.Updated = ticket.Created;
                ticket.TicketStatusId = db.TicketStatuses.AsNoTracking().SingleOrDefault(x => x.Name == "Unassigned").Id;
                ticket.TicketPriorityId = db.TicketPriorities.AsNoTracking().SingleOrDefault(x => x.Name == "Unprioritized").Id;
                // New Tickets are initially Unassigned, and ticket.AssignedToUserId is NULLable, so we'll leave it NULL. 
                // ticket.AssignedToUserId : Leaving NULL
                ticket.AssignedToUserId = db.Users.AsNoTracking().SingleOrDefault(x => x.UserName == "unassignedUser@bugtracker.com").Id;
                db.Tickets.Add(ticket);
                db.SaveChanges();
                if (User.IsInRole("Admin"))
                {
                    return RedirectToAction("Index", "Tickets");
                }
                else
                {
                    return RedirectToAction("UserTickets", "Tickets");
                }
            }

            ViewBag.TicketTypeId = new SelectList(db.TicketTypes, "Id", "Name", ticket.TicketTypeId);
            //ViewBag.TicketPriorityId = new SelectList(db.TicketPriorities, "Id", "Name", ticket.TicketPriorityId);
            //ViewBag.AssignedToUserId = new SelectList(db.Users, "Id", "FullName", ticket.AssignedToUserId);
            return View(ticket);
        }


        // GET: Tickets/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Ticket ticket = db.Tickets.Find(id);
            if (ticket == null)
            {
                return HttpNotFound();
            }
            return View(ticket);
        }

        // GET: Tickets/Create
        [NonAction]
        public ActionResult Create()
        {
            ViewBag.AssignedToUserId = new SelectList(db.Users, "Id", "FirstName");
            ViewBag.OwnerUserId = new SelectList(db.Users, "Id", "FirstName");
            ViewBag.ProjectId = new SelectList(db.Projects, "Id", "Name");
            ViewBag.TicketPriorityId = new SelectList(db.TicketPriorities, "Id", "Name");
            ViewBag.TicketStatusId = new SelectList(db.TicketStatuses, "Id", "Name");
            ViewBag.TicketTypeId = new SelectList(db.TicketTypes, "Id", "Name");
            return View();
        }

        // POST: Tickets/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [NonAction]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,Title,Description,Created,Updated,ProjectId,TicketTypeId,TicketPriorityId,TicketStatusId,OwnerUserId,AssignedToUserId")] Ticket ticket)
        {
            if (ModelState.IsValid)
            {
                db.Tickets.Add(ticket);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.AssignedToUserId = new SelectList(db.Users, "Id", "FirstName", ticket.AssignedToUserId);
            ViewBag.OwnerUserId = new SelectList(db.Users, "Id", "FirstName", ticket.OwnerUserId);
            ViewBag.ProjectId = new SelectList(db.Projects, "Id", "Name", ticket.ProjectId);
            ViewBag.TicketPriorityId = new SelectList(db.TicketPriorities, "Id", "Name", ticket.TicketPriorityId);
            ViewBag.TicketStatusId = new SelectList(db.TicketStatuses, "Id", "Name", ticket.TicketStatusId);
            ViewBag.TicketTypeId = new SelectList(db.TicketTypes, "Id", "Name", ticket.TicketTypeId);
            return View(ticket);
        }

        
        // GET: Tickets/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Ticket ticket = db.Tickets.Find(id);
            if (ticket == null)
            {
                return HttpNotFound();
            }
            ViewBag.AssignedToUserId = new SelectList(db.Users, "Id", "FirstName", ticket.AssignedToUserId);
            ViewBag.OwnerUserId = new SelectList(db.Users, "Id", "FirstName", ticket.OwnerUserId);
            ViewBag.ProjectId = new SelectList(db.Projects, "Id", "Name", ticket.ProjectId);
            ViewBag.TicketPriorityId = new SelectList(db.TicketPriorities, "Id", "Name", ticket.TicketPriorityId);
            ViewBag.TicketStatusId = new SelectList(db.TicketStatuses, "Id", "Name", ticket.TicketStatusId);
            ViewBag.TicketTypeId = new SelectList(db.TicketTypes, "Id", "Name", ticket.TicketTypeId);
            return View(ticket);
        }

        // POST: Tickets/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "Id,Title,Description,Created,Updated,ProjectId,TicketTypeId,TicketPriorityId,TicketStatusId,OwnerUserId,AssignedToUserId")] Ticket ticket)
        {
            if (ModelState.IsValid)
            {
                // original values of the ticket in DB before the update
                var oldTicket = db.Tickets.AsNoTracking().SingleOrDefault(t => t.Id == ticket.Id);
                db.Entry(ticket).State = EntityState.Modified;
                db.SaveChanges();
                //db.SaveChangesOfTickets(User.Identity.GetUserId());

                // new values of the ticket in DB after the update
                var newTicket = db.Tickets.AsNoTracking().SingleOrDefault(t => t.Id == ticket.Id);
                // Add an auditlog/changelog into TicketHistories for each Ticket property changed 
                await th.AddTicketHistory(oldTicket, newTicket, User.Identity.GetUserId(), User.Identity.GetUserName());

                string userName = User.Identity.GetUserName();
                // Add a new entry into TicketNotifications when a ticket is modified by a user different than the assignee or modified to assign/reassign
                //await tnh.HandleTicketNotification(ticket.Id, userName);

                return RedirectToAction("Index");
            }
            ViewBag.AssignedToUserId = new SelectList(db.Users, "Id", "FirstName", ticket.AssignedToUserId);
            ViewBag.OwnerUserId = new SelectList(db.Users, "Id", "FirstName", ticket.OwnerUserId);
            ViewBag.ProjectId = new SelectList(db.Projects, "Id", "Name", ticket.ProjectId);
            ViewBag.TicketPriorityId = new SelectList(db.TicketPriorities, "Id", "Name", ticket.TicketPriorityId);
            ViewBag.TicketStatusId = new SelectList(db.TicketStatuses, "Id", "Name", ticket.TicketStatusId);
            ViewBag.TicketTypeId = new SelectList(db.TicketTypes, "Id", "Name", ticket.TicketTypeId);
            return View(ticket);
        }

        // GET: Tickets/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Ticket ticket = db.Tickets.Find(id);
            if (ticket == null)
            {
                return HttpNotFound();
            }
            return View(ticket);
        }

        // POST: Tickets/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Ticket ticket = db.Tickets.Find(id);
            db.Tickets.Remove(ticket);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
