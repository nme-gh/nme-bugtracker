using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace nme_bugtracker.Models
{
    public class UserTicketsViewModel
    {
        public IEnumerable<Ticket> AssignedTickets { get; set; }
        public IEnumerable<Ticket> OwnedTickets { get; set; }
        public IEnumerable<Ticket> OtherProjectTickets { get; set; }
    }

}