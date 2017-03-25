using Microsoft.AspNet.Identity.EntityFramework;
using nme_bugtracker.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace nme_bugtracker.Helpers
{
    #region ProjectsHelper
    public class ProjectsHelper
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        public ICollection<Project> ListProjectsForUser(string userId)
        {
            return db.Users.Find(userId).Projects;
        }

        public bool IsUserOnProject(string userId, int projectId)
        {
            // NME: Find() vs SingleOrDefault()
            // Find() avoids a round trip to DB and searches in the DB context first.
            // SingleOrDefault() will force a query to DB everytime. If used with AsNoTracking(), the entities returned from DB will not be cached in the DB context.
            // var project = db.Projects.AsNoTracking().SingleOrDefault(x => x.Id == projectid);    // Do Not Use! No need for a round trip to DB.
            var project = db.Projects.Find(projectId);
            var flag = project.Users.Any(u => u.Id == userId);
            return flag;

            /*
            if (db.Projects.Find(projectId).Users.Contains(db.Users.Find(userId)))
            {
                return true;
            }
            return false;
            */
        }

        public void AddUserToProject(string userId, int projectId)
        {
            if (!IsUserOnProject(userId, projectId))
            {
                Project project = db.Projects.Find(projectId);
                //project.Users.Add(db.Users.Find(userId));
                var newUser = db.Users.Find(userId);
                project.Users.Add(newUser);
                //db.Entry(project).State = EntityState.Modified; // just saves this obj instance.
                db.SaveChanges();
            }
        }

        public void RemoveUserFromProject(string userId, int projectId)
        {
            if (IsUserOnProject(userId, projectId))
            {
                Project project = db.Projects.Find(projectId);
                project.Users.Remove(db.Users.Find(userId));
                //db.Entry(project).State = EntityState.Modified; // just saves this obj instance.
                db.SaveChanges();
            }
        }

        public ICollection<ApplicationUser> UsersNotOnProject(int projectId)
        {
            return db.Users.Where(u => u.Projects.All(p => p.Id != projectId)).ToList();
        }

        public ICollection<ApplicationUser> UsersOnProject(int projectId)
        {
            return db.Projects.Find(projectId).Users;
        }

        public List<string> UserIdsOnProject(int projectId)
        {
            // users on project
            var usrsOnProject = UsersOnProject(projectId);
            // Out of these users, get only the user id property and have a list of them to return
            var userIds = new List<string>();
            userIds.AddRange(usrsOnProject.Select(u => u.Id));
            // user ids on project
            return userIds;
        }

        // Returns same result with the other method above: public List<string> UserIdsOnProject(int projectId)  
        public List<string> UserIdsOnProject_2(int projectId)
        {
            // users on project
            var usrsOnProject = UsersOnProject(projectId);
            // Out of these users, get only the user id property and have a list of them to return
            var userIds = new List<string>();
            foreach (var usr in usrsOnProject)
            {
                userIds.Add(usr.Id);
            }
            // user ids on project
            return userIds;
        }



        //#region [ProjectUsers]Helper
        //public void AddUserToPUsers(ProjectUser pu)
        //{
        //    if (!IsUserOnPUsers(pu))
        //    {
        //        db.ProjectUsers.Add(pu);
        //        //db.Entry(pu).State = EntityState.Modified; // just saves this obj instance.
        //        db.SaveChanges();
        //    }
        //}

        //public bool IsUserOnPUsers(ProjectUser pu)
        //{
        //    if (db.ProjectUsers.AsNoTracking().SingleOrDefault(x => x.ProjectId == pu.ProjectId && x.UserId == pu.UserId) != null)
        //    {
        //        return true;
        //    }
        //    return false;
        //}
        //#endregion
    }
    #endregion ProjectsHelper

    #region UsersHelper
    public class UsersHelper
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        public bool IsUserOnRole(string userId, string roleId)
        {
            var role = db.Roles.Find(roleId);
            var flag = role.Users.Any(u => u.UserId == userId);
            return flag;
        }

        public void AssignUserToRole(string userId, string roleId)
        {
            if (!IsUserOnRole(userId, roleId))
            {
                IdentityRole role = db.Roles.Find(roleId);
                var newUserRole = new IdentityUserRole();
                newUserRole.RoleId = roleId;
                newUserRole.UserId = userId;
                role.Users.Add(newUserRole);
                db.SaveChanges();
            }
        }

        public void UnassignUserFromRole(string userId, string roleId)
        {
            if (IsUserOnRole(userId, roleId))
            {
                IdentityRole role = db.Roles.Find(roleId);
                var delUserRole = new IdentityUserRole();
                delUserRole.RoleId = roleId;
                delUserRole.UserId = userId;
                role.Users.Remove(delUserRole);
                //db.Entry(role).State = EntityState.Modified; // just saves this obj instance.
                db.SaveChanges();
            }
        }

    }
    #endregion UsersHelper
}