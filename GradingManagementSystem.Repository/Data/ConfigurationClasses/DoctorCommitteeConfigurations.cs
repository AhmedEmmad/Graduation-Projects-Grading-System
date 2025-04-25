using GradingManagementSystem.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GradingManagementSystem.Repository.Data.ConfigurationClasses
{
    public class DoctorCommitteeConfigurations : IEntityTypeConfiguration<DoctorCommittee>
    {
        public void Configure(EntityTypeBuilder<DoctorCommittee> builder)
        {
            builder.HasOne(dc => dc.Doctor)
                    .WithMany(d => d.DoctorCommittees)
                    .HasForeignKey(dc => dc.DoctorId);

            builder.HasOne(dc => dc.Committee)
                .WithMany(c => c.DoctorCommittees)
                .HasForeignKey(dc => dc.CommitteeId);
        }
    }
}
