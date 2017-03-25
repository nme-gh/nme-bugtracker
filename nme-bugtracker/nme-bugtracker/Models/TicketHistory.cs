using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace nme_bugtracker.Models
{
    public class TicketHistory
    {
        public int Id { get; set; }
        public string Property { get; set; }
        public string OldValue { get; set; }
        public string NewValue { get; set; }
        public int TicketId { get; set; }
        public string ChangedByUserId { get; set; }
        public DateTimeOffset ChangedDate { get; set; }

        public virtual Ticket Ticket { get; set; }
        public virtual ApplicationUser ChangedByUser { get; set; }

        public string ChangedDateFormatted { get { return ChangedDate.ToString("ddd, dd MMMM yyyy h:mm tt"); } }

    }
}