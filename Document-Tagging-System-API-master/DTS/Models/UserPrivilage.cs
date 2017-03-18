using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace DTS.Models
{
    public class UserPrivilage
    {
        public int Id { set; get; }
        [Range(0, 1)]
        public int CanAddDocument { set; get; }
        [Range(0, 1)]
        public int CanDeleteDocument { set; get; }
        [Range(0, 1)]
        public int CanTagDocument { set; get; }
        [Required]
        public string UserId { set; get; }
    }
}