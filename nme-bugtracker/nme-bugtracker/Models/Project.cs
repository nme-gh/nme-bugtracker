using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace nme_bugtracker.Models
{
    public class Project
    {
        // Navigation properties 
        public virtual ICollection<ApplicationUser> Users { get; set; }
        public virtual ICollection<Ticket> Tickets { get; set; }


        public Project()
        {
            this.Users = new HashSet<ApplicationUser>();
            this.Tickets = new HashSet<Ticket>();
        }

        //[Required(ErrorMessage = "Please select a project")]
        public int Id { get; set; }

        [Required]
        [StringLength(150, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [Display(Name = "Project")]
        public string Name { get; set; }

        [Required]
        [DataType(DataType.DateTime)]
        [Display(Name = "Creation Date")]
        public DateTimeOffset Created { get; set; }

        [DataType(DataType.DateTime)]
        [Display(Name = "Update Date")]
        public DateTimeOffset Updated { get; set; }


        public string AuthorName { get; set; }

        ////[Required]
        //[Display(Name = "Created by")]
        ////public string CreatedUserId { get; set; }
        //public string CreatedById { get; set; }

        //public virtual ApplicationUser CreatedBy { get; set; }

    }
}