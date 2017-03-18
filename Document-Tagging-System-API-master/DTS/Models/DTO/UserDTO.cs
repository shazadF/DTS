using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace DTS.Models.DTO
{
    public class UserDTO
    {
        [Required]
        [UniqueUserName(ErrorMessage = "This UserName is already registered")]
        public string UserName { get; set; }
        [Required]
        [UniqueEmail(ErrorMessage = "This e-mail is already registered")]
        public string Email { get; set; }
        
        public string Password { get; set; }
        
        public int CompanyId { get; set; }
        [Required]
        public string FirstName { get; set; }
        [Required]
        public string LastName { get; set; }
        public string Id { set; get; }
    }

    public class UniqueEmailAttribute : ValidationAttribute
    {
        public override bool IsValid(object value)
        {
            ApplicationDbContext db = new ApplicationDbContext();
            var userWithTheSameEmail = db.Users.SingleOrDefault(
                u => u.Email == (string)value);
            return userWithTheSameEmail == null;
        }
    }
    public class UniqueUserNameAttribute : ValidationAttribute
    {
        public override bool IsValid(object value)
        {
            ApplicationDbContext db = new ApplicationDbContext();
            var userWithTheSameUserName = db.Users.SingleOrDefault(
                u => u.UserName == (string)value);
            return userWithTheSameUserName == null;
        }
    }


}