using System;
using System.Collections.Generic;
using System.Text;

namespace PMS.Domain.Entities
{
    public class Status
    {
        public Guid StatusID { get; set; }
        public string Name { get; set; } // Active, Inactive, Pending
    }
}
