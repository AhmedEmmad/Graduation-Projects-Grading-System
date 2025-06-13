using GradingManagementSystem.Core.Entities;
using GradingManagementSystem.Core.Entities.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace GradingManagementSystem.Repository.Data.DbContexts
{
    public class GradingManagementSystemDbContext : IdentityDbContext<AppUser,IdentityRole,string>
    {
        // This Class has Some Properties And Methods. I Will Inherit These From DbContext Class.
        public GradingManagementSystemDbContext(DbContextOptions<GradingManagementSystemDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Instead Of Applying Each Configuration Like modelBuilder.ApplyConfiguration(new AdminConfigurations()), Use This : 
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

            base.OnModelCreating(modelBuilder);
        }

        // Represent Tables In SQL Server
        public DbSet<Admin> Admins { get; set; }
        public DbSet<Doctor> Doctors { get; set; }
        public DbSet<DoctorProjectIdea> DoctorProjectIdeas { get; set; }
        public DbSet<Team> Teams { get; set; }
        public DbSet<Student> Students { get; set; }
        public DbSet<TeamProjectIdea> TeamProjectIdeas { get; set; }
        public DbSet<TemporaryUser> TemporaryUsers { get; set; }
        public DbSet<UserOtp> UserOtps { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<TeamRequestDoctorProjectIdea> TeamsRequestDoctorProjectIdeas { get; set; }
        public DbSet<FinalProjectIdea> FinalProjectIdeas { get; set; }
        public DbSet<TaskItem> Tasks { get; set; }
        public DbSet<TaskMember> TaskMembers { get; set; }
        public DbSet<Invitation> Invitations { get; set; }
        public DbSet<Criteria> Criterias { get; set; }
        public DbSet<Schedule> Schedules { get; set; }
        public DbSet<CommitteeDoctorSchedule> CommitteeDoctorSchedules { get; set; }
        public DbSet<AcademicAppointment> AcademicAppointments { get; set; }
        public DbSet<Evaluation> Evaluations { get; set; }
        public DbSet<CriteriaSchedule> CriteriaSchedules { get; set; }
        public DbSet<StudentTotalGrade> StudentTotalGrades { get; set; }
    }
}
