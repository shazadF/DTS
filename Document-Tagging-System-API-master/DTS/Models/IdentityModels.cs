using System.Data.Entity;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;

namespace DTS.Models
{
    // You can add profile data for the user by adding more properties to your ApplicationUser class, please visit http://go.microsoft.com/fwlink/?LinkID=317594 to learn more.
    public class ApplicationUser : IdentityUser
    {
        public int CompanyId { get; set; }
        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager, string authenticationType)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, authenticationType);
            // Add custom user claims here
            return userIdentity;
        }
    }

    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
    
    
        public ApplicationDbContext()
            : base("DefaultConnection", throwIfV1Schema: false)
        {
        }
        
        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }

        public System.Data.Entity.DbSet<DTS.Models.UserProfile> UserProfiles { get; set; }
        public System.Data.Entity.DbSet<DTS.Models.Company> Companies { get; set; }
        public System.Data.Entity.DbSet<DTS.Models.Document> Documents { get; set; }
        public System.Data.Entity.DbSet<DTS.Models.DocumentACL> DocumentAcls { get; set; }
        public System.Data.Entity.DbSet<DTS.Models.File> Files { get; set; }
        public System.Data.Entity.DbSet<DTS.Models.History> Histories { get; set; }
        public System.Data.Entity.DbSet<DTS.Models.HistorySequence> HistorySequences { get; set; }
        public System.Data.Entity.DbSet<DTS.Models.Notification> Notifications { get; set; }
        public System.Data.Entity.DbSet<DTS.Models.Restricted> Restricteds { get; set; }
        public System.Data.Entity.DbSet<DTS.Models.SequenceACL> SequenceAcls { get; set; }
        public System.Data.Entity.DbSet<DTS.Models.Tag>  Tags { get; set; }
        public System.Data.Entity.DbSet<DTS.Models.UserACL> UserAcls { get; set; }
        public System.Data.Entity.DbSet<DTS.Models.Group> Groups { get; set; }
        public System.Data.Entity.DbSet<DTS.Models.UserGroup> UserGroups { get; set; }
        public System.Data.Entity.DbSet<DTS.Models.UserPrivilage> UserPrivilages { get; set; }
        public System.Data.Entity.DbSet<DTS.Models.DocumentUser> DocumentUsers { get; set; }


    }
}