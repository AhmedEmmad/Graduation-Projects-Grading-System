using GradingManagementSystem.APIs.Extensions;
using GradingManagementSystem.APIs.Hubs;
using GradingManagementSystem.APIs.Middlewares;
using GradingManagementSystem.Core;
using GradingManagementSystem.Core.Entities.Identity;
using GradingManagementSystem.Repository.Data.DbContexts;
using GradingManagementSystem.Repository.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace GradingManagementSystem.APIs
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            #region Configure Services - (Add Services To The Container)

                builder.Services.AddApplicationServices(builder.Configuration);

                builder.Services.AddIdentityServices(builder.Configuration);

                #region OLD - CORS Configuation
                //builder.Services.AddCors(options =>
                //{
                //    options.AddPolicy("CorsPolicy", policy => policy.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin());
                //});
                #endregion

                #region CORS Configuation
                    builder.Services.AddCors(options =>
                    {
                        options.AddPolicy("CorsPolicy", policy =>
                        {
                            policy.WithOrigins("http://localhost:4200", "https://graduation-project-angular.vercel.app")
                                  .AllowAnyHeader()
                                  .AllowAnyMethod()
                                  .AllowCredentials();
                        });
                    });
                #endregion

            #endregion

            var app = builder.Build();

            #region Update Database Automatically
                using var scope = app.Services.CreateScope();
                var services = scope.ServiceProvider;
                // To Handle Errors In Console Screen In An Organized Manner => Ask CLR For Object Of Class That Implement ILoggerFactory Interface To Handle This
                var loggerFactory = services.GetRequiredService<ILoggerFactory>();

                try
                {
                    var _dbContext = services.GetRequiredService<GradingManagementSystemDbContext>(); // Ask CLR For Creating An Object DbContext Explicitly
                    var userManager = services.GetRequiredService<UserManager<AppUser>>();
                    var unitOfWork = services.GetRequiredService<IUnitOfWork>();   
                    await _dbContext.Database.MigrateAsync(); // Update Database Automatically
                    await SeedData.SeedRolesAndAdminUsersAsync(services, userManager, unitOfWork);    
                }
                catch (Exception ex)
                {
                    var logger = loggerFactory.CreateLogger<Program>();
                    logger.LogError(ex, "An Error Has Been Occurred During Applying The Migration");
                }
            #endregion

            #region Configuration - (Configure The HTTP Request Pipeline)
            
            app.MapHub<NotificationHub>("/api/notificationHub");
            app.UseMiddleware<ExceptionMiddleware>();

            //if (app.Environment.IsDevelopment())
            //{
                app.UseSwagger();
                app.UseSwaggerUI();
            //}
            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCors("CorsPolicy");
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllers();

            #endregion

            app.Run();
        }
    }
}
