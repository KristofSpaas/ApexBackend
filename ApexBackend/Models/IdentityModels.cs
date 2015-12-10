using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;

namespace ApexBackend.Models
{
    // You can add profile data for the user by adding more properties to your ApplicationUser class, please visit http://go.microsoft.com/fwlink/?LinkID=317594 to learn more.
    public class ApplicationUser : IdentityUser
    {
        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager,
            string authenticationType)
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
            : base("ApexConnection", throwIfV1Schema: false)
        {
        }

        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }

        public virtual System.Data.Entity.DbSet<Doctor> Doctors { get; set; }
        public virtual System.Data.Entity.DbSet<Patient> Patients { get; set; }
        public virtual System.Data.Entity.DbSet<Temperature> Temperatures { get; set; }
        public virtual System.Data.Entity.DbSet<HeartRate> HeartRates { get; set; }
        public virtual System.Data.Entity.DbSet<StepsPerDay> StepsPerDays { get; set; }
        public virtual System.Data.Entity.DbSet<Appointment> Appointments { get; set; }
        public virtual System.Data.Entity.DbSet<EmergencyCall> EmergencyCalls { get; set; }
        public virtual System.Data.Entity.DbSet<Message> Messages { get; set; }
        public virtual System.Data.Entity.DbSet<Advice> Advices { get; set; }
    }
}