using GradingManagementSystem.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GradingManagementSystem.Repository.Data.ConfigurationClasses
{
    public class CriteriaConfigurations : IEntityTypeConfiguration<Criteria>
    {
        public void Configure(EntityTypeBuilder<Criteria> builder)
        {
        }
    }
}
