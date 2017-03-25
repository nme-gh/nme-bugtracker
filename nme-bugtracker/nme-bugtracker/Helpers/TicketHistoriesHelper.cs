using nme_bugtracker.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace nme_bugtracker.Helpers
{
    public class TicketHistoriesHelper
    {
        private static ApplicationDbContext db = new ApplicationDbContext();

        public static string GetProjectNameById(int projectId)
        {
            return db.Projects.FirstOrDefault(p => p.Id == projectId).Name;
        }

        public static string GetTicketTypeNameById(int ticketTypeId)
        {
            return db.TicketTypes.FirstOrDefault(tt => tt.Id == ticketTypeId).Name;
        }

        public static string GetTicketPriorityNameById(int ticketPriorityId)
        {
            return db.TicketPriorities.FirstOrDefault(tp => tp.Id == ticketPriorityId).Name;
        }

        public static string GetTicketStatusNameById(int ticketStatusId)
        {
            return db.TicketStatuses.FirstOrDefault(ts => ts.Id == ticketStatusId).Name;
        }

        public static string GetUserNameById(string userId)
        {
            if (String.IsNullOrEmpty(userId))
            {
                return "N/A";
            }
            return db.Users.FirstOrDefault(ts => ts.Id == userId).FullName;
        }

        public static string GetFormattedDate(DateTimeOffset? date)
        {
            return !date.HasValue ? "N/A" : date.Value.ToString("ddd, dd MMMM yyyy h:mm tt");
        }

        // index: 
        // 0: OldValue
        // 1: NewValue
        public static string ManageNavigationPropertiesData(TicketHistory change, int index)
        {
            var data = "";
            switch (change.Property)
            {
                case "ProjectId":
                    data = index == 0 ? GetProjectNameById(Convert.ToInt32(change.OldValue)) : GetProjectNameById(Convert.ToInt32(change.NewValue));
                    break;
                case "TicketTypeId":
                    data = index == 0 ? GetTicketTypeNameById(Convert.ToInt32(change.OldValue)) : GetTicketTypeNameById(Convert.ToInt32(change.NewValue));
                    break;
                case "TicketPriorityId":
                    data = index == 0 ? GetTicketPriorityNameById(Convert.ToInt32(change.OldValue)) : GetTicketPriorityNameById(Convert.ToInt32(change.NewValue));
                    break;
                case "TicketStatusId":
                    data = index == 0 ? GetTicketStatusNameById(Convert.ToInt32(change.OldValue)) : GetTicketStatusNameById(Convert.ToInt32(change.NewValue));
                    break;
                case "OwnerUserId":
                    data = index == 0 ? GetUserNameById(change.OldValue) : GetUserNameById(change.NewValue);
                    break;
                case "AssignedToUserId":
                    data = index == 0 ? GetUserNameById(change.OldValue) : GetUserNameById(change.NewValue);
                    break;
                case "Created":
                case "Updated":
                    DateTimeOffset.Parse(change.OldValue);
                    data = index == 0 ? GetFormattedDate(DateTimeOffset.Parse(change.OldValue)) : GetFormattedDate(DateTimeOffset.Parse(change.NewValue));
                    break;
                default:
                    data = index == 0 ? change.OldValue : change.NewValue;
                    break;

            }
            return data;
        }

    }
}