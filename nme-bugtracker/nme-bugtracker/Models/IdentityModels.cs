using System.Data.Entity;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Collections.Generic;
using System.Collections;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System;

using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace nme_bugtracker.Models
{
    // You can add profile data for the user by adding more properties to your ApplicationUser class, please visit http://go.microsoft.com/fwlink/?LinkID=317594 to learn more.
    public class ApplicationUser : IdentityUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string DisplayName { get; set; }

        public string UserNameDisplayName { get { return string.Format("[{0}][{1}]", DisplayName, UserName); } }
        public string FullName { get { return string.Format("[{0}][{1}]", FirstName, LastName); } }


        // Navigation properties 
        public virtual ICollection<TicketComment> TicketComments { get; set; }
        public virtual ICollection<TicketAttachment> TicketAttachments { get; set; }
        public virtual ICollection<TicketNotification> TicketNotifications { get; set; }
        public virtual ICollection<TicketHistory> TicketHistories { get; set; }
        public virtual ICollection<Project> Projects { get; set; }
        //// NME: Nav.
        //public virtual ICollection<Ticket> OwnerUser { get; set; }
        //public virtual ICollection<Ticket> AssignedToUser { get; set; }

        //// Get User's Tickets
        //public virtual ICollection<Ticket> Tickets { get; set; }

        public ApplicationUser()
        {
            this.TicketComments = new HashSet<TicketComment>();
            this.TicketAttachments = new HashSet<TicketAttachment>();
            this.TicketNotifications = new HashSet<TicketNotification>();
            this.TicketHistories = new HashSet<TicketHistory>();
            this.Projects = new HashSet<Project>();
            //// NME: Nav.
            //this.OwnerUser = new HashSet<Ticket>();
            //this.AssignedToUser = new HashSet<Ticket>();

            //// Get User's Tickets
            //this.Tickets = new HashSet<Ticket>();
        }


        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            // Add custom user claims here
            return userIdentity;
        }
    }

    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext()
            : base("DefaultConnection", throwIfV1Schema: false)
        {
        }

        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }

        public DbSet<Ticket> Tickets { get; set; }
        public DbSet<Project> Projects { get; set; }
        public DbSet<TicketAttachment> TicketAttachments { get; set; }
        public DbSet<TicketComment> TicketComments { get; set; }
        public DbSet<TicketNotification> TicketNotifications { get; set; }
        public DbSet<TicketHistory> TicketHistories { get; set; }
        public DbSet<TicketPriority> TicketPriorities { get; set; }
        public DbSet<TicketStatus> TicketStatuses { get; set; }
        public DbSet<TicketType> TicketTypes { get; set; }

        // NME: DO NOT Create the Table, as EF will create it as ApplicationUsersProjects due to Many to Many relations setup in our Models.
        public DbSet<ProjectUser> ProjectUsers { get; set; }

        public override int SaveChanges()
        {
            return base.SaveChanges();
        }

        // TicketHistories
        public int SaveChangesOfTickets(string userId)
        {
            // Have a change log first for each property change made on a Ticket.
            int objectsCount;

            List<DbEntityEntry> newEntities = new List<DbEntityEntry>();

            // Get all Added/Deleted/Modified entities (not Unmodified or Detached)
            foreach (var entry in this.ChangeTracker.Entries().Where
                (x => (x.State == System.Data.Entity.EntityState.Added) ||
                    (x.State == System.Data.Entity.EntityState.Deleted) ||
                    (x.State == System.Data.Entity.EntityState.Modified)))
            {
                if (entry.State == System.Data.Entity.EntityState.Modified)
                {
                    // For each changed record, get the audit record entries and add them
                    foreach (TicketHistory changeDescription in GetAuditRecordsForEntity(entry, userId))
                    {
                        this.TicketHistories.Add(changeDescription);
                    }
                }
            }

            // Default save changes call to actually save changes to the database
            objectsCount = base.SaveChanges();

            return objectsCount;
        }


        #region Helper Methods
        /// <summary>
        /// Helper method to create record description for Audit table based on operation done on dbEntity
        /// [Delete] ONLY - Insert, Delete, Update
        /// </summary>
        /// <param name="dbEntity"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        private List<TicketHistory> GetAuditRecordsForEntity(DbEntityEntry dbEntity, string userId, bool insertSpecial = false)
        {
            List<TicketHistory> changesCollection = new List<TicketHistory>();

            DateTime changeTime = DateTime.Now;

            /*
            // Get Entity Type Name.
            string tableName = ((IObjectContextAdapter)dbEntity).ObjectContext.GetType().Name;
            // Runtime Error: Unable to cast object of type 'System.Data.Entity.Infrastructure.DbEntityEntry' to type 'System.Data.Entity.Infrastructure.IObjectContextAdapter'.
             */

            // Get the Table() attribute, if one exists
            TableAttribute tableAttr = dbEntity.Entity.GetType().GetCustomAttributes(typeof(TableAttribute), false).SingleOrDefault() as TableAttribute;

            // Get table name (if it has a Table attribute, use that, otherwise get the pluralized name)
            var tableName = tableAttr != null ? tableAttr.Name : dbEntity.Entity.GetType().Name;

            // Get primary key value (If you have more than one key column, this will need to be adjusted)
            var primaryKeyName = dbEntity.Entity.GetType().GetProperties().Single(p => p.GetCustomAttributes(typeof(KeyAttribute), false).Count() > 0).Name;

            // http://stackoverflow.com/questions/17904631/how-can-i-log-all-entities-change-during-savechanges-using-ef-code-first
            // https://jmdority.wordpress.com/2011/07/20/using-entity-framework-4-1-dbcontext-change-tracking-for-audit-logging/

            int primaryKeyId = 0;

            if (dbEntity.State == System.Data.Entity.EntityState.Modified)
            {
                var primaryKeyValue = dbEntity.OriginalValues.GetValue<object>(primaryKeyName).ToString();

                if (primaryKeyValue != null)
                {
                    Int32.TryParse(primaryKeyValue, out primaryKeyId);
                }

                //foreach (var prop in typeof(Ticket).GetProperties())
                //{
                //    var tTemp = new Ticket();
                //    var temp = prop.GetValue(tTemp);

                //}

                //foreach (var propertyName in dbEntity.OriginalValues.PropertyNames)
                //{
                //        var temp = dbEntity.OriginalValues.GetValue<object>(propertyName);
                //}

                /*
                foreach (var propertyName in dbEntity.OriginalValues.PropertyNames)
                {
                    // For updates, we only want to capture the columns that actually changed
                    var original = dbEntity.Property(propertyName).OriginalValue.ToString();
                    var current = dbEntity.Property(propertyName).CurrentValue.ToString();


                }
                */

                /*
                foreach (var propertyName in dbEntity.GetModifiedProperties())
                VS.
                foreach (var propertyName in dbEntity.OriginalValues.PropertyNames)
                 */
                foreach (var propertyName in dbEntity.OriginalValues.PropertyNames)
                {
                    /*
                    dbEntry.OriginalValues.GetValue<object>(propertyName);
                    VS.
                    dbEntry.GetDatabaseValues().GetValue<object>(propertyName);
                    */

                    // For updates, we only want to capture the columns that actually changed

                    //if (dbEntity.OriginalValues.GetValue<object>(propertyName) == null)
                    //    dbEntity.Property(propertyName).OriginalValue = "";

                    //if (dbEntity.CurrentValues.GetValue<object>(propertyName) == null)
                    //    dbEntity.Property(propertyName).CurrentValue = "";

                    //if (!object.Equals(dbEntity.OriginalValues.GetValue<object>(propertyName), 
                    //        dbEntity.CurrentValues.GetValue<object>(propertyName)))
                    if (!object.Equals(dbEntity.GetDatabaseValues().GetValue<object>(propertyName), 
                            dbEntity.CurrentValues.GetValue<object>(propertyName)))
                    //if (dbEntity.OriginalValues.GetValue<object>(propertyName).ToString() !=
                    //        dbEntity.CurrentValues.GetValue<object>(propertyName).ToString())
                    {
                        changesCollection.Add(new TicketHistory()
                        {
                            ChangedByUserId = userId,
                            ChangedDate = changeTime,
                            //EventType = ModelConstants.UPDATE_TYPE_MODIFY,
                            //TableName = tableName,
                            TicketId = primaryKeyId,
                            Property = propertyName,
                            OldValue = dbEntity.GetDatabaseValues().GetValue<object>(propertyName) == null ? null : dbEntity.GetDatabaseValues().GetValue<object>(propertyName).ToString(),
                            NewValue = dbEntity.CurrentValues.GetValue<object>(propertyName) == null ? null : dbEntity.CurrentValues.GetValue<object>(propertyName).ToString()
                        }
                        );
                    }
                }
            }


            // Otherwise, don't do anything, we don't care about Unchanged or Detached entities
            return changesCollection;
        }
#endregion

    }
}