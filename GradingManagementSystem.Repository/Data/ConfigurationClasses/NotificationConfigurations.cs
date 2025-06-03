using GradingManagementSystem.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GradingManagementSystem.Repository.Data.ConfigurationClasses
{
    public class NotificationConfigurations : IEntityTypeConfiguration<Notification>
    {
        public void Configure(EntityTypeBuilder<Notification> builder)
        {
            //builder.HasIndex(N => N.Title).IsUnique();
           
            builder.HasOne(N => N.Admin)
                   .WithMany(A => A.Notifications)
                   .HasForeignKey(N => N.AdminId)
                   .OnDelete(DeleteBehavior.NoAction);
        }
    }
}



