using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace nme_bugtracker.Models
{
    public class Ticket
    {
        // Navigation properties 
        public virtual ICollection<TicketComment> TicketComments{ get; set; }
        public virtual ICollection<TicketAttachment> TicketAttachments { get; set; }
        public virtual ICollection<TicketNotification> TicketNotifications { get; set; }
        public virtual ICollection<TicketHistory> TicketHistories { get; set; }

        public Ticket()
        {
            this.TicketComments = new HashSet<TicketComment>();
            this.TicketAttachments = new HashSet<TicketAttachment>();
            this.TicketNotifications = new HashSet<TicketNotification>();
            this.TicketHistories = new HashSet<TicketHistory>();
        }

        // NME: Data Annotation required for Change/Audit Log while overriding db.SaveChanges()
        [Key]
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        [Display(Name = "Submitted")]
        public DateTimeOffset Created { get; set; }
        public DateTimeOffset Updated { get; set; }
        [Display(Name = "Project")]
        public int ProjectId { get; set; }
        [Display(Name = "Type")]
        public int TicketTypeId { get; set; }
        [Display(Name = "Priority")]
        public int TicketPriorityId { get; set; }
        [Display(Name = "Status")]
        public int TicketStatusId { get; set; }
        [Display(Name = "Submitted By")]
        public string OwnerUserId { get; set; }
        [Display(Name = "Assigned To")]
        public string AssignedToUserId { get; set; }

        public virtual Project Project { get; set; }
        public virtual TicketType TicketType { get; set; }
        public virtual TicketPriority TicketPriority { get; set; }
        public virtual TicketStatus TicketStatus { get; set; }
        public virtual ApplicationUser OwnerUser { get; set; }
        public virtual ApplicationUser AssignedToUser { get; set; }

        public string CreatedFormatted { get { return Created.ToString("ddd, dd MMMM yyyy h:mm tt"); } }
        public string UpdatedFormatted { get { return Updated.ToString("ddd, dd MMMM yyyy h:mm tt"); } }
        //public string UpdatedFormatted { get { return !Updated.HasValue ? "N/A" : Updated.Value.ToString("ddd, dd MMMM yyyy h:mm tt"); } }

    }
}