using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using nme_bugtracker.Models;
using Microsoft.AspNet.Identity;

namespace nme_bugtracker.Controllers
{
    [Authorize]
    public class TicketNotificationsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: TicketNotifications
        [Authorize(Roles = "Admin, Project Manager")]
        // TODO: ActionName to be in NavigationPartial is "AllTicketNotifications"
        public ActionResult Index()
        {
            var ticketNotifications = db.TicketNotifications.Include(t => t.RecipientUser).Include(t => t.Ticket).Where(tn => tn.IsRead == false);
            return View(ticketNotifications.ToList());
        }

        // GET: TicketNotifications
        [Authorize(Roles = "Developer")]
        // TODO: Create a View
        public ActionResult TicketNotifications()
        {
            string userId = User.Identity.GetUserId();
            var ticketNotifications = db.TicketNotifications.Include(t => t.RecipientUser).Include(t => t.Ticket).Where(tn => tn.RecipientUserId == userId && tn.IsRead == false);
            return View(ticketNotifications.ToList());
        }

        // GET: MarkRead
        public ActionResult MarkRead(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            TicketNotification ticketNotification = db.TicketNotifications.Find(id);
            if (ticketNotification == null)
            {
                return HttpNotFound();
            }
            ticketNotification.IsRead = true;
            db.Entry(ticketNotification).State = EntityState.Modified;
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        // GET: MarkAllRead
        public ActionResult MarkAllRead()
        {
            string userId = User.Identity.GetUserId();
            var ticketNotifications = db.TicketNotifications.Include(t => t.RecipientUser).Include(t => t.Ticket).Where(tn => tn.RecipientUserId == userId && tn.IsRead == false);

            //ticketNotifications.Select(c => { c.IsRead = true; return c; }).ToList();
            ticketNotifications.ToList().ForEach(c => c.IsRead = true);

            //db.Entry(ticketNotifications).State = EntityState.Modified; // Runtime Error: The entity type DbQuery`1 is not part of the model for the current context.
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        // GET: TicketNotifications/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            TicketNotification ticketNotification = db.TicketNotifications.Find(id);
            if (ticketNotification == null)
            {
                return HttpNotFound();
            }
            return View(ticketNotification);
        }

        // GET: TicketNotifications/Create
        public ActionResult Create()
        {
            ViewBag.RecipientUserId = new SelectList(db.Users, "Id", "FirstName");
            ViewBag.TicketId = new SelectList(db.Tickets, "Id", "Title");
            return View();
        }

        // POST: TicketNotifications/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,TicketId,Property,NotificationDetail,RecipientUserId,Created")] TicketNotification ticketNotification)
        {
            if (ModelState.IsValid)
            {
                db.TicketNotifications.Add(ticketNotification);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.RecipientUserId = new SelectList(db.Users, "Id", "FirstName", ticketNotification.RecipientUserId);
            ViewBag.TicketId = new SelectList(db.Tickets, "Id", "Title", ticketNotification.TicketId);
            return View(ticketNotification);
        }

        // GET: TicketNotifications/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            TicketNotification ticketNotification = db.TicketNotifications.Find(id);
            if (ticketNotification == null)
            {
                return HttpNotFound();
            }
            ViewBag.RecipientUserId = new SelectList(db.Users, "Id", "FirstName", ticketNotification.RecipientUserId);
            ViewBag.TicketId = new SelectList(db.Tickets, "Id", "Title", ticketNotification.TicketId);
            return View(ticketNotification);
        }

        // POST: TicketNotifications/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,TicketId,Property,NotificationDetail,RecipientUserId,Created")] TicketNotification ticketNotification)
        {
            if (ModelState.IsValid)
            {
                db.Entry(ticketNotification).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.RecipientUserId = new SelectList(db.Users, "Id", "FirstName", ticketNotification.RecipientUserId);
            ViewBag.TicketId = new SelectList(db.Tickets, "Id", "Title", ticketNotification.TicketId);
            return View(ticketNotification);
        }

        // GET: TicketNotifications/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            TicketNotification ticketNotification = db.TicketNotifications.Find(id);
            if (ticketNotification == null)
            {
                return HttpNotFound();
            }
            return View(ticketNotification);
        }

        // POST: TicketNotifications/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            TicketNotification ticketNotification = db.TicketNotifications.Find(id);
            db.TicketNotifications.Remove(ticketNotification);
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
