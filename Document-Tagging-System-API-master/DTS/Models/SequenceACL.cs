using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DTS.Models
{
    public class SequenceACL
    {
        public int Id { get; set; }
        public int DocAclId { get; set; }
        public string Sequence { get; set; }
        public int PresentAccess { get; set; }
        public string UserId { get; set; }
    }
}