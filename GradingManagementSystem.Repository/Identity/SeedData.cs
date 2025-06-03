using GradingManagementSystem.Core;
using GradingManagementSystem.Core.Entities;
using GradingManagementSystem.Core.Entities.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Task = System.Threading.Tasks.Task;

namespace GradingManagementSystem.Repository.Identity
{
    public static class SeedData
    {
        public static async Task SeedRolesAndAdminUsersAsync(this IServiceProvider serviceProvider, UserManager<AppUser> userManager, IUnitOfWork unitOfWork)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var roles = new[] { "Admin", "Doctor", "Student" };

            foreach (var role in roles)
            {
                var roleExist = await roleManager.RoleExistsAsync(role);
                if (!roleExist)
                {
                    await roleManager.CreateAsync(new IdentityRole { Name = role });
                }
            }

            var adminEmails = new[] 
            {   "aehassan008@gmail.com", 
                "andrewekrame2@gmail.com", 
                "esmailarafat60@gmail.com", 
                "muhammedashraf22@gmail.com" 
            };

            foreach (var adminEmail in adminEmails)
            {
                var adminUser = await userManager.FindByEmailAsync(adminEmail);
                if (adminUser is null)
                {
                    adminUser = new AppUser
                    {
                        Email = adminEmail,
                        UserName = adminEmail.Split('@')[0],
                        FullName = adminEmail.Split('@')[0],
                    };
                    var result = await userManager.CreateAsync(adminUser, $"Admin@123");
                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(adminUser, "Admin");

                        var admin = new Admin
                        {
                            Email = adminEmail,
                            FullName = adminEmail.Split('@')[0],
                            AppUserId = adminUser.Id,
                        };

                        await unitOfWork.Repository<Admin>().AddAsync(admin);
                        await unitOfWork.CompleteAsync();
                    }
                }
            }
        }
    }
}
