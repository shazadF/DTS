using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace DTS.Models
{
    public class UserGroup
    {
        public int Id { get; set; }
        [Required]
        public string UserId { get; set; }
        [Required]
        public int GroupId { get; set; }
    }
}