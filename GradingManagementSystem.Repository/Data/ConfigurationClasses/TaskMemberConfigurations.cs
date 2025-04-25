using GradingManagementSystem.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GradingManagementSystem.Repository.Data.ConfigurationClasses
{
    public class TaskMemberConfigurations : IEntityTypeConfiguration<TaskMember>
    {
        public void Configure(EntityTypeBuilder<TaskMember> builder)
        {
            builder.HasOne(tm => tm.Task)
                   .WithMany(t => t.TaskMembers)
                   .HasForeignKey(tm => tm.TaskId)
                   .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(tm => tm.Student)
                   .WithMany(s => s.TaskMembers)
                   .HasForeignKey(tm => tm.StudentId)
                   .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
