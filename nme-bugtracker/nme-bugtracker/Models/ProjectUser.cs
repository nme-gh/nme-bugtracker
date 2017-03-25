using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace nme_bugtracker.Models
{
    public class ProjectUser
    {
        public ProjectUser()
        {
        }

        public ProjectUser(int x, string y)
        {
            ProjectId = x;
            UserId = y;
        }

        [Required(ErrorMessage = "Please select a project")]
        [Display(Name = "Project")]
        [Key, Column(Order = 0)]
        public int ProjectId { get; set; }

        [Required]
        [Display(Name = "User")]
        [Key, Column(Order = 1)]
        public string UserId { get; set; }

        public virtual Project Project { get; set; }
        public virtual ApplicationUser User { get; set; }
    }

    //public class ProjectUserEntities : DbContext
    //{
    //    //public DbSet<ProjectUser> ProjectUsers { get; set; }
    //    protected override void OnModelCreating(DbModelBuilder modelBuilder)
    //    {
    //        // NME: EF Fluent API
    //        // Configure Code First to create composite PK on ProjectUser
    //        modelBuilder.Entity<ProjectUser>().HasKey(a => new { a.ProjectId, a.UserId });
    //    }

    //}

}