using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Data.Entity;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;

namespace MVCManukauTech.Models
{
    // You can add profile data for the user by adding more properties to your ApplicationUser class, please visit http://go.microsoft.com/fwlink/?LinkID=317594 to learn more.
    public class ApplicationUser : IdentityUser
    {
        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
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
            //140903 JPC change needed here - see notes below
            // -- MS Comments -- Set the database initializer which is run once during application start
            // -- MS Comments -- This seeds the database with admin user credentials and admin role

            // Interesting that the above comments say that it runs one time only to automatically create a test user.   
            // It appears to do more, much more, as we saw this morning (140826).
            // We need code "new ApplicationDbInitializer()"  early on in our process because it seems to be 
            // this code that adds the identity membership tables into our database.
            // BUT after it has done that good work, we need to change it to "null" because we do not need 
            // or want any more automatic database changes.


                Database.SetInitializer<ApplicationDbContext>(null);

            return new ApplicationDbContext();
        }
    }
}