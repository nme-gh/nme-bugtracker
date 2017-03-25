using nme_bugtracker.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using System.Web;


namespace nme_bugtracker.Helpers
{
    public class TicketNotificationsHelper
    {
        private static ApplicationDbContext db = new ApplicationDbContext();

        public async Task HandleTicketNotification(int ticketId, string userName, string updatedProperties, bool reassignment)
        {
            Ticket ticket = db.Tickets.AsNoTracking().Include(t => t.Project).Include(t => t.OwnerUser).Include(t => t.TicketPriority).Include(t => t.TicketType).SingleOrDefault(t => t.Id == ticketId);

            var detail = ComposeEmailBody(ticket, userName, updatedProperties, reassignment);
            await SendNotificationEmail(ticket, userName, detail);
            InsertTicketNotification(ticket, updatedProperties, detail);
        }

        public void InsertTicketNotification(Ticket ticket, string updatedProperties, string notificationDetail)
        {
            var ticketNotification = new TicketNotification {
                TicketId = ticket.Id,
                Property = updatedProperties,
                NotificationDetail = notificationDetail,
                RecipientUserId = ticket.AssignedToUserId,
                Created = DateTimeOffset.Now
            };

            // Create a new TicketNotification entry
            db.TicketNotifications.Add(ticketNotification);
            db.SaveChanges();
        }

        public async Task SendNotificationEmail(Ticket ticket, string userName, string notificationDetail)
        {
            // NME: Implement the SMTP Email service in here!
            try
            {
                var emailTo = db.Users.Find(ticket.AssignedToUserId).Email;
                // public MailMessage(string from, string to);
                var email = new MailMessage(ConfigurationManager.AppSettings["emailfromto"], emailTo)
                {
                    Subject = "BugTracker Ticket Notification",
                    Body = notificationDetail,
                    IsBodyHtml = true
                };

                var svc = new MyEmailService();
                await svc.SendAsync(email);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                await Task.FromResult(0);
            }
        }

        private string ComposeEmailBody(Ticket ticket, string userName, string updatedProperties, bool reassignment)
        {
            var eMailBody = new StringBuilder();
            if (reassignment)
            {
                eMailBody.AppendFormat("One of your tickets has been updated by {0}. Details are below. || ", userName).AppendLine();
            } else
            {
                eMailBody.AppendFormat("A new ticket has been assigned to you by {0}. Details are below. || ", userName).AppendLine();
            }

            if (!string.IsNullOrEmpty(updatedProperties))
            {
                eMailBody.AppendFormat("Please check out the ticket history for following details as they have been updated: {0} || ", updatedProperties).AppendLine();
            }

            //eMailBody.AppendFormat("A new ticket has been assigned to you by {0}. Details are below. || ", userName).AppendLine();
            eMailBody.AppendFormat("<b>Project:</b> {0} | <b>Ticket:</b> {1} | <b>TicketDesc:</b> {2} | <b>Submitter:</b> {3} | <b>TicketPriority:</b> {4} | <b>TicketType:</b> {5} ", ticket.Project.Name, ticket.Title, ticket.Description, ticket.OwnerUser.UserName, ticket.TicketPriority.Name, ticket.TicketType.Name);
            return eMailBody.ToString();
        }
    }
}