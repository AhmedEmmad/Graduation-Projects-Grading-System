using GradingManagementSystem.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GradingManagementSystem.Repository.Data.ConfigurationClasses
{
    public class StudentConfigurations : IEntityTypeConfiguration<Student>
    {
        public void Configure(EntityTypeBuilder<Student> builder)
        {
            builder.HasIndex(S => S.Email).IsUnique(); // Student's Email Unique

            // One-To-One (AppUser <-> Doctors)
            builder.HasOne(S => S.AppUser)
                   .WithOne(AU => AU.Student)
                   .HasForeignKey<Student>(S => S.AppUserId)
                   .OnDelete(DeleteBehavior.NoAction);

            // One-To-Many (Teams <-> Students)
            builder.HasOne(S => S.Team)
                   .WithMany(T => T.Students)
                   .HasForeignKey(S => S.TeamId)
                   .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
