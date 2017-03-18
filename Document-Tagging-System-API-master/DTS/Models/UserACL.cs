using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DTS.Models
{
    public class UserACL
    {
        public int Id { get; set; }
        public int UserType { get; set; } //0 for admin , 1 for normal
        public string UserId { get; set; }
    }
}