using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace DTS.Models
{
    public class Tag
    {
       
        public int Id { get; set; }
        
        public int DocumentId { get; set; }
        
        public string TagContent { get; set; }
        
        public string TagColor { get; set; }
        
        public string UserId { get; set; }
        
       
    }
}