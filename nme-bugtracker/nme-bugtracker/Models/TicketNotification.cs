using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace nme_bugtracker.Models
{
    public class TicketNotification
    {
        public int Id { get; set; }
        [Display(Name = "Ticket")]
        public int TicketId { get; set; }
        public string Property { get; set; }
        [Display(Name = "Message")]
        public string NotificationDetail { get; set; }
        [Display(Name = "Recipient")]
        public string RecipientUserId { get; set; }
        [Display(Name = "Sent")]
        public DateTimeOffset Created { get; set; }
        public bool IsRead { get; set; }

        public virtual Ticket Ticket { get; set; }
        public virtual ApplicationUser RecipientUser { get; set; }

        public string CreatedFormatted { get { return Created.ToString("ddd, dd MMMM yyyy h:mm tt"); } }
    }
}