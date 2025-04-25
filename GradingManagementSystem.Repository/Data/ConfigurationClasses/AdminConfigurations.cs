using GradingManagementSystem.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GradingManagementSystem.Repository.Data.ConfigurationClasses
{
    public class AdminConfigurations : IEntityTypeConfiguration<Admin>
    {
        public void Configure(EntityTypeBuilder<Admin> builder)
        {   
            builder.HasIndex(A => A.Email).IsUnique(); // Admin's Email Unique

            // One-To-One (AppUser <-> Admin)
            builder.HasOne(A => A.AppUser)
                   .WithOne(AU => AU.Admin)
                   .HasForeignKey<Admin>(A => A.AppUserId)
                   .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
