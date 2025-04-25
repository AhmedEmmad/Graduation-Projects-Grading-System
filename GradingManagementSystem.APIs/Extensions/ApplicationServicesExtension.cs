using GradingManagementSystem.Core;
using GradingManagementSystem.Core.Repositories.Contact;
using GradingManagementSystem.Core.Services.Contact;
using GradingManagementSystem.Repository;
using GradingManagementSystem.Service;
using Microsoft.OpenApi.Models;
using GradingManagementSystem.Repository.Data.DbContexts;
using Microsoft.EntityFrameworkCore;
using GradingManagementSystem.Core.CustomResponses;
using Microsoft.AspNetCore.Mvc;

namespace GradingManagementSystem.APIs.Extensions
{
    public static class ApplicationServicesExtension
    {
        public static void AddApplicationServices(this IServiceCollection Services, IConfiguration configuration)
        {
            Services.AddDbContext<GradingManagementSystemDbContext>(options =>
            {
                options.UseSqlServer(configuration.GetConnectionString("MonsterConnection"));
                //options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"));
            });

            Services.AddControllers()
                    .ConfigureApiBehaviorOptions(options =>
                    {
                        options.InvalidModelStateResponseFactory = actionContext =>
                        {
                            var errors = actionContext.ModelState
                                                      .Where(model => model.Value?.Errors.Count > 0)
                                                      .SelectMany(model => model.Value.Errors)
                                                      .Select(error => error.ErrorMessage)
                                                      .ToList();
                                var errorResponse = new ApiResponse
                                {
                                    StatusCode = 400,
                                    Message = "Validation failed.",
                                    Data = new
                                    {
                                        IsSuccess = false,
                                        Errors = errors
                                    }
                                };      
                            return new BadRequestObjectResult(errorResponse);
                        };
                    });

            Services.AddEndpointsApiExplorer();
            Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
            Services.AddScoped<INotificationRepository, NotificationRepository>();
            Services.AddScoped<IProjectRepository, ProjectRepository>();
            Services.AddScoped<IStudentRepository, StudentRepository>();
            Services.AddScoped<ITaskRepository, TaskRepository>();
            Services.AddScoped<ITeamRepository, TeamRepository>();
            Services.AddScoped<IUserProfileRepository, UserProfileRepository>();
            Services.AddScoped<IUnitOfWork, UnitOfWork>();
            Services.AddScoped<IAuthenticationService, AuthenticationService>();
            Services.AddScoped<IEmailService, EmailService>();
            Services.AddScoped<ITaskService, TaskService>();
            Services.AddScoped<ITokenService, TokenService>();
            Services.AddScoped<IUserProfileService, UserProfileService>();
            Services.AddSignalR();

            Services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc(
                    "v1",
                    new OpenApiInfo
                    {
                        Title = "Graduation-Projects-Grading-System",
                        Version = "v1",
                    });
                var securityScheme = new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Description = "Enter JWT Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "bearer",
                    Reference = new OpenApiReference
                    {
                        Id = "bearer",
                        Type = ReferenceType.SecurityScheme
                    }
                };
                options.AddSecurityDefinition("bearer", securityScheme);

                var securityRequirements = new OpenApiSecurityRequirement
                {
                    {securityScheme,new[]{ "bearer"} }
                };

                options.AddSecurityRequirement(securityRequirements);
            });
        }
    }
}
