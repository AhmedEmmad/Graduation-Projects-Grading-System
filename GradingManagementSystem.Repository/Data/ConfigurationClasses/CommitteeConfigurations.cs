using GradingManagementSystem.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GradingManagementSystem.Repository.Data.ConfigurationClasses
{
    public class CommitteeConfigurations : IEntityTypeConfiguration<Committee>
    {
        public void Configure(EntityTypeBuilder<Committee> builder)
        {
        }
    }
}
