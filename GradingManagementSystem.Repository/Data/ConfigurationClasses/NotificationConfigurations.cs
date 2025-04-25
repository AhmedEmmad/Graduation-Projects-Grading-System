using GradingManagementSystem.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GradingManagementSystem.Repository.Data.ConfigurationClasses
{
    public class NotificationConfigurations : IEntityTypeConfiguration<Notification>
    {
        public void Configure(EntityTypeBuilder<Notification> builder)
        {
            builder.ToTable("Notifications");

            builder.HasIndex(N => N.Title).IsUnique();

            builder.Property(N => N.Title)
                   .HasMaxLength(255)
                   .IsRequired();

            builder.Property(N => N.Description)
                   .IsRequired();

            
            builder.Property(N => N.Role)
                   .IsRequired()
                   .HasConversion<string>();

           
            builder.HasOne(N => N.Admin)
                   .WithMany(A => A.Notifications)
                   .HasForeignKey(N => N.AdminId)
                   .OnDelete(DeleteBehavior.SetNull);
        }
    }
}



