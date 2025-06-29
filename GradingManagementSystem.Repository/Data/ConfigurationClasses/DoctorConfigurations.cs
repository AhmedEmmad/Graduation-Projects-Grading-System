﻿using GradingManagementSystem.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GradingManagementSystem.Repository.Data.ConfigurationClasses
{
    public class DoctorConfigurations : IEntityTypeConfiguration<Doctor>
    {
        public void Configure(EntityTypeBuilder<Doctor> builder)
        {   
            builder.HasIndex(D => D.Email).IsUnique(); // Make Student's Email Unique

            // One-To-One (AppUser <-> Doctors)
            builder.HasOne(D => D.AppUser)
                   .WithOne(AU => AU.Doctor)
                   .HasForeignKey<Doctor>(D => D.AppUserId)
                   .OnDelete(DeleteBehavior.NoAction); 
        }
    }
}
