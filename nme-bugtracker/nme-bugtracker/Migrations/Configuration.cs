namespace nme_bugtracker.Migrations
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;
    using Microsoft.AspNet.Identity;
    using Microsoft.AspNet.Identity.EntityFramework;
    using Models;

    internal sealed class Configuration : DbMigrationsConfiguration<nme_bugtracker.Models.ApplicationDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = true;
        }

        protected override void Seed(nme_bugtracker.Models.ApplicationDbContext context)
        {
            //  This method will be called after migrating to the latest version.

            //  You can use the DbSet<T>.AddOrUpdate() helper extension method 
            //  to avoid creating duplicate seed data. E.g.
            //
            //    context.People.AddOrUpdate(
            //      p => p.FullName,
            //      new Person { FullName = "Andrew Peters" },
            //      new Person { FullName = "Brice Lambson" },
            //      new Person { FullName = "Rowan Miller" }
            //    );
            //

            // Create Roles for BugTracker Project
            var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(context));

            if (!context.Roles.Any(r => r.Name == "Admin"))
            {
                roleManager.Create(new IdentityRole { Name = "Admin" });
            }

            if (!context.Roles.Any(r => r.Name == "Project Manager"))
            {
                roleManager.Create(new IdentityRole { Name = "Project Manager" });
            }

            context.Roles.AddOrUpdate(
              r => r.Name,
              new IdentityRole { Name = "Developer" },
              new IdentityRole { Name = "Submitter" }
            );

            // Create Users for BugTracker Project
            var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(context));

            // *************************************************************
            //          nuranme@gmail.com / Admin
            // *************************************************************
            // Creating User for Nuran
            if (!context.Users.Any(u => u.Email == "nuranme@gmail.com"))
            {
                userManager.Create(new ApplicationUser
                {
                    UserName = "nuranme@gmail.com",
                    Email = "nuranme@gmail.com",
                    EmailConfirmed = true,
                    FirstName = "Nuran",
                    LastName = "Esen",
                    DisplayName = "NME"
                }, "metin716458");
            }

            // Assigning User to a Role (User for Nuran, Role of Admin) 
            var userId = userManager.FindByEmail("nuranme@gmail.com").Id;
            userManager.AddToRole(userId, "Admin");

            // *************************************************************
            //          jtwichell@coderfoundry.com / Project Manager
            // *************************************************************
            // Creating User for Jason
            if (!context.Users.Any(u => u.Email == "jtwichell@coderfoundry.com"))
            {
                userManager.Create(new ApplicationUser
                {
                    UserName = "jtwichell@coderfoundry.com",
                    Email = "jtwichell@coderfoundry.com",
                    EmailConfirmed = true,
                    FirstName = "Jason",
                    LastName = "Twichell",
                    DisplayName = "JTwichell"
                }, "Abcd&1234");
            }

            // Assigning User to a Role (User for Jason, Role of Project Manager) 
            userId = userManager.FindByEmail("jtwichell@coderfoundry.com").Id;
            userManager.AddToRole(userId, "Project Manager");

            // *************************************************************
            //          nme1@mailinator.com / Developer
            // *************************************************************
            // Creating User for User1
            if (!context.Users.Any(u => u.Email == "nme1@mailinator.com"))
            {
                userManager.Create(new ApplicationUser
                {
                    UserName = "nme1@mailinator.com",
                    Email = "nme1@mailinator.com",
                    EmailConfirmed = false,
                    FirstName = "User1_FN",
                    LastName = "User1_LN",
                    DisplayName = "User1"
                }, "Abcd&1234");
            }

            // Assigning User to a Role (User for User1, Role of Developer) 
            userId = userManager.FindByEmail("nme1@mailinator.com").Id;
            userManager.AddToRole(userId, "Developer");

            // *************************************************************
            //          nme2@mailinator.com / Developer
            // *************************************************************
            // Creating User for User2
            if (!context.Users.Any(u => u.Email == "nme2@mailinator.com"))
            {
                userManager.Create(new ApplicationUser
                {
                    UserName = "nme2@mailinator.com",
                    Email = "nme2@mailinator.com",
                    EmailConfirmed = false,
                    FirstName = "User2_FN",
                    LastName = "User2_LN",
                    DisplayName = "User2"
                }, "Abcd&1234");
            }

            // Assigning User to a Role (User for User2, Role of Developer) 
            userId = userManager.FindByEmail("nme2@mailinator.com").Id;
            userManager.AddToRole(userId, "Developer");

            // *************************************************************
            //          nuranme.coderfoundry2@gmail.com / Submitter
            // *************************************************************
            // Creating User for User with Email
            if (!context.Users.Any(u => u.Email == "nuranme.coderfoundry2@gmail.com"))
            {
                userManager.Create(new ApplicationUser
                {
                    UserName = "nuranme.coderfoundry2@gmail.com",
                    Email = "nuranme.coderfoundry2@gmail.com",
                    EmailConfirmed = true,
                    FirstName = "Nuranme",
                    LastName = "Coderfoundry2",
                    DisplayName = "NME_CF2"
                }, "Abcd&1234");
            }

            // Assigning User to a Role (User for User3, Role of Submitter) 
            userId = userManager.FindByEmail("nuranme.coderfoundry2@gmail.com").Id;
            userManager.AddToRole(userId, "Submitter");

            // *************************************************************
            //          Unassigned User for each new Ticket
            // *************************************************************
            // *************************************************************
            //          unassignedUser@bugtracker.com / Developer
            // *************************************************************
            // Creating User for unassignedUser
            if (!context.Users.Any(u => u.Email == "unassignedUser@bugtracker.com"))
            {
                userManager.Create(new ApplicationUser
                {
                    UserName = "unassignedUser@bugtracker.com",
                    Email = "unassignedUser@bugtracker.com",
                    EmailConfirmed = false,
                    FirstName = "unassignedUser",
                    LastName = "Developer",
                    DisplayName = "unassignedUser"
                }, "Abcd&1234");
            }

            // Assigning User to a Role (User for guestAdmin, Role of Admin) 
            userId = userManager.FindByEmail("unassignedUser@bugtracker.com").Id;
            userManager.AddToRole(userId, "Developer");

            // *************************************************************
            //          DemoLogin Users for each Role
            // *************************************************************
            // *************************************************************
            //          guestAdmin@bugtracker.com / Admin
            // *************************************************************
            // Creating User for guestAdmin
            if (!context.Users.Any(u => u.Email == "guestAdmin@bugtracker.com"))
            {
                userManager.Create(new ApplicationUser
                {
                    UserName = "guestAdmin@bugtracker.com",
                    Email = "guestAdmin@bugtracker.com",
                    EmailConfirmed = false,
                    FirstName = "guestAdmin",
                    LastName = "Admin",
                    DisplayName = "guestAdmin"
                }, "Abcd&1234");
            }

            // Assigning User to a Role (User for guestAdmin, Role of Admin) 
            userId = userManager.FindByEmail("guestAdmin@bugtracker.com").Id;
            userManager.AddToRole(userId, "Admin");

            // *************************************************************
            //          guestPM@bugtracker.com / Project Manager
            // *************************************************************
            // Creating User for guestPM
            if (!context.Users.Any(u => u.Email == "guestPM@bugtracker.com"))
            {
                userManager.Create(new ApplicationUser
                {
                    UserName = "guestPM@bugtracker.com",
                    Email = "guestPM@bugtracker.com",
                    EmailConfirmed = false,
                    FirstName = "guestPM",
                    LastName = "ProjectManager",
                    DisplayName = "guestPM"
                }, "Abcd&1234");
            }

            // Assigning User to a Role (User for guestPM, Role of Project Manager) 
            userId = userManager.FindByEmail("guestPM@bugtracker.com").Id;
            userManager.AddToRole(userId, "Project Manager");

            // *************************************************************
            //          guestDeveloper@bugtracker.com / Developer
            // *************************************************************
            // Creating User for guestDeveloper
            if (!context.Users.Any(u => u.Email == "guestDeveloper@bugtracker.com"))
            {
                userManager.Create(new ApplicationUser
                {
                    UserName = "guestDeveloper@bugtracker.com",
                    Email = "guestDeveloper@bugtracker.com",
                    EmailConfirmed = false,
                    FirstName = "guestDeveloper",
                    LastName = "Developer",
                    DisplayName = "guestDeveloper"
                }, "Abcd&1234");
            }

            // Assigning User to a Role (User for guestDeveloper, Role of Developer) 
            userId = userManager.FindByEmail("guestDeveloper@bugtracker.com").Id;
            userManager.AddToRole(userId, "Developer");

            // *************************************************************
            //          guestSubmitter@bugtracker.com / Submitter
            // *************************************************************
            // Creating User for guestSubmitter
            if (!context.Users.Any(u => u.Email == "guestSubmitter@bugtracker.com"))
            {
                userManager.Create(new ApplicationUser
                {
                    UserName = "guestSubmitter@bugtracker.com",
                    Email = "guestSubmitter@bugtracker.com",
                    EmailConfirmed = false,
                    FirstName = "guestSubmitter",
                    LastName = "Submitter",
                    DisplayName = "guestSubmitter"
                }, "Abcd&1234");
            }

            // Assigning User to a Role (User for guestSubmitter, Role of Submitter) 
            userId = userManager.FindByEmail("guestSubmitter@bugtracker.com").Id;
            userManager.AddToRole(userId, "Submitter");


            //// Remove Ticket Priorities
            //context.TicketPriorities.RemoveRange(
            //    context.TicketPriorities.Where(tp => tp.Name == "Urgent" || 
            //                                    tp.Name == "High" || 
            //                                    tp.Name == "Normal" || 
            //                                    tp.Name == "Moderate" || 
            //                                    tp.Name == "Low")
            //                                );

            //Create Ticket Priorities
            context.TicketPriorities.AddOrUpdate(
              tp => tp.Name,
              new TicketPriority { Name = "Urgent" },
              new TicketPriority { Name = "High" },
              new TicketPriority { Name = "Normal" },
              new TicketPriority { Name = "Low" },
              new TicketPriority { Name = "Unprioritized" }
            );

            //Create Ticket Types
            context.TicketTypes.AddOrUpdate(
              tt => tt.Name,
              new TicketType { Name = "Build New" },
              new TicketType { Name = "Add On" },
              new TicketType { Name = "Refactor" },
              new TicketType { Name = "Prototype" },
              new TicketType { Name = "Bug" }
            );

            //Create Ticket Statuses
            context.TicketStatuses.AddOrUpdate(
              ts => ts.Name,
              new TicketStatus { Name = "Resolved" },
              new TicketStatus { Name = "Closed" },
              new TicketStatus { Name = "In Review" },
              new TicketStatus { Name = "In Progress" },
              new TicketStatus { Name = "Assigned" },
              new TicketStatus { Name = "Unassigned" }
            );

            ////Create Ticket Stages/Phases/Environments
            //context.TicketStages.AddOrUpdate(
            //  tsp => tsp.Name,
            //  new TicketStage { Name = "Security" },    // Certifications
            //  new TicketStage { Name = "Network" },     // Certifications
            //  new TicketStage { Name = "Database" },
            //  new TicketStage { Name = "Integration" }, // 3rd party integration, Data Center, etc
            //  new TicketStage { Name = "UAT" },         // User Aceeptance Testing env. issues or improvements
            //  new TicketStage { Name = "QA" },          // Qa env. issues or improvements
            //  new TicketStage { Name = "Dev" },         // Dev env. issues or improvements
            //  new TicketStage { Name = "Testing" },
            //  new TicketStage { Name = "User Interface" },
            //  new TicketStage { Name = "Design" }
            //);
        }
    }
}
