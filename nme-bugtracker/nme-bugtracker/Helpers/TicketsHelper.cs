using nme_bugtracker.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace nme_bugtracker.Helpers
{
    public class TicketsHelper
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        private ProjectsHelper ph = new ProjectsHelper();
        private TicketNotificationsHelper tnh = new TicketNotificationsHelper();


        // Lists/Gets the tickets a user is assigned to
        public ICollection<Ticket> ListAssignedTicketsForUser(string userId)
        {
            //return db.Users.Find(userId).Tickets;
            var tickets = db.Tickets.Where(t => t.AssignedToUserId == userId).ToList();
            return tickets;
        }

        // Lists/Gets the tickets a user is submitted
        public ICollection<Ticket> ListOwnedTicketsForUser(string userId)
        {
            //var tickets = db.Tickets.Where(t => t.OwnerUserId == userId && t.AssignedToUserId != userId).ToList();
            var tickets = db.Tickets.Where(t => t.OwnerUserId == userId && t.OwnerUserId != t.AssignedToUserId).ToList();
            return tickets;
        }

        // Lists/Gets all other tickets belonging to the projects to which the user is assigned to (Ticket is NOT directly related with the user by either Assignee or Submitter)
        // Lists/Gets all other Project tickets
        public ICollection<Ticket> ListOtherTicketsForUser(string userId)
        {
            var tickets = new List<Ticket>();

            // Different approach: Using Linq, and avoiding ANY foreach() loop
            tickets = db.Users.Find(userId).Projects.SelectMany(p => p.Tickets).Where(t => t.AssignedToUserId != userId && t.OwnerUserId != userId).ToList();

            //// Different approach: Using Linq but NOT avoiding 1st foreach() loop
            //foreach (var project in ph.ListProjectsForUser(userId))
            //{
            //    // Different approach: Using Linq but avoiding 2nd foreach() loop
            //    var unrelatedProjectTickets = project.Tickets.Where(t => t.AssignedToUserId != userId && t.OwnerUserId != userId);
            //    tickets.AddRange(unrelatedProjectTickets.Select(t => t));

            //    // Different approach: Using Linq but NOT avoiding 2nd foreach() loop
            //    //foreach (var ticket in unrelatedProjectTickets) {   tickets.Add(ticket);   }
            //}


            return tickets;

        }

        //public void AddTicketHistory(Ticket oldTicket, Ticket newTicket, string userId)
        public async Task AddTicketHistory(Ticket oldTicket, Ticket newTicket, string userId, string userName)
        {
            StringBuilder propertyCollection = new StringBuilder();
            // true: Reassignment of the ticket
            // false: New Assignment of the ticket
            bool reassignment = false;
            var unassignedUserId = db.Users.SingleOrDefault(u => u.UserName == "unassignedUser@bugtracker.com").Id;


            if (oldTicket.AssignedToUserId == unassignedUserId && newTicket.AssignedToUserId != unassignedUserId)
            {
                // New Assignment: from Noone to A by ?Z
                reassignment = false;
            }
            if (oldTicket.AssignedToUserId != unassignedUserId && newTicket.AssignedToUserId != unassignedUserId && oldTicket.AssignedToUserId != newTicket.AssignedToUserId)
            {
                // Reassignment: from A to B by ?Z?
                reassignment = true;
            }

            // Cycle over the Ticket properties and compare their values ...
            foreach (var prop in typeof(Ticket).GetProperties())
            {
                if (prop.GetValue(oldTicket) == null)
                {
                    prop.SetValue(oldTicket, "", null);
                    //prop.SetValue(oldTicket, Convert.ChangeType("", Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType), null);
                    //prop.SetValue(oldTicket, System.Convert.ChangeType("", Nullable.GetUnderlyingType(prop.PropertyType)), null);
                    /*PropertyDescriptorCollection props = TypeDescriptor.GetProperties(oldTicket);
                    PropertyDescriptor property = props[prop.Name]; */
                    /*
                    PropertyDescriptor property = TypeDescriptor.GetProperties(oldTicket)[prop.Name];
                    property.SetValue(oldTicket, property.Converter.ConvertFromInvariantString(""));
                    */
                    /*Type t = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;
                    object safeValue = ("" == null) ? null : Convert.ChangeType("", t);
                    prop.SetValue(oldTicket, safeValue, null);
                    */
                }
                if (prop.GetValue(newTicket) == null)
                {
                    prop.SetValue(newTicket, "", null);
                }

                // If the value of a Ticket property hasn't changed continue to the next property. If changed, create the change log entry ...
                //if (object.ReferenceEquals(prop.GetValue(oldTicket), prop.GetValue(newTicket))) { continue; }
                //if (prop.GetValue(oldTicket).Equals(prop.GetValue(newTicket))) { continue; }

                //Object of type 'System.String' cannot be converted to type 'nme_bugtracker.Models.ApplicationUser'.
                if ( prop.GetValue(oldTicket).ToString() != prop.GetValue(newTicket).ToString() )
                {
                    // Create a TicketHistory object to populate a new record
                    var ticketHistory = new TicketHistory
                    {
                        TicketId = oldTicket.Id,
                        Property = prop.Name,
                        OldValue = prop.GetValue(oldTicket) == null ? null : prop.GetValue(oldTicket).ToString(),
                        NewValue = prop.GetValue(newTicket) == null ? null : prop.GetValue(newTicket).ToString(),
                        ChangedByUserId = userId,
                        ChangedDate = DateTimeOffset.Now
                    };

                    // Skip accounting for "Updated" property as this date changes everytime there is a change on the ticket
                    if (ticketHistory.Property != "Updated")
                    {
                        // Build TicketNotification details on the properties changed
                        propertyCollection.AppendFormat("{0},", ticketHistory.Property);
                    }

                    // Create a new TicketHistory entry
                    db.TicketHistories.Add(ticketHistory);
                }
            }
            db.SaveChanges();
            /*
            if (newTicket.AssignedToUserId == unassignedUserId)
            {
                // Ticket still unassigned, but getting updated. Noone to send the notifications yet.
                // Skip the entire ticket notification.
            }
            if (newTicket.AssignedToUserId != unassignedUserId && userId == newTicket.AssignedToUserId)
            {
                // Assigned ticket being updated by its own assignee, no notification is needed.
                // Skip the entire ticket notification.
            }
            */
            // Assigned ticket being updated by someone else (not by its own assignee), so a notification is needed to be sent to the assignee.
            if (newTicket.AssignedToUserId != unassignedUserId && userId != newTicket.AssignedToUserId)
            {
                await tnh.HandleTicketNotification(newTicket.Id, userName, propertyCollection.ToString(), reassignment);
            }
        }

    }
}