using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace DTS.Models
{
    public class Restricted
    {
        public int Id { get; set; }
        [Required]
        public int DocAclId { get; set; }
        [Required]
        public string UserId { get; set; }
        
        public string List { get; set; }
    }
}