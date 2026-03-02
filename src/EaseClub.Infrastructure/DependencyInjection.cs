using EaseClub.Application.Common.interfaces;
using EaseClub.Application.Common.Interfaces;
using EaseClub.Application.Features.ApplicationTemplates.Queries;
using EaseClub.Application.Features.Auth.Common.Interfaces;
using EaseClub.Application.Features.MembershipPlans.Queries;
using EaseClub.Domain.ApplicationTemplates.Repositories;
using EaseClub.Domain.Branches;
using EaseClub.Domain.ClubAdmin;
using EaseClub.Domain.Clubs;
using EaseClub.Domain.Common;
using EaseClub.Domain.Common.Interfaces;
using EaseClub.Domain.Member;
using EaseClub.Domain.MembershipApplications.Repositories;
using EaseClub.Domain.MembershipPlans.Repositories;
using EaseClub.Domain.MembershipTypes;
using EaseClub.Domain.PricingPolices;
using EaseClub.Infrastructure.Auth.Entities;
using EaseClub.Infrastructure.Auth.interfaces;
using EaseClub.Infrastructure.Auth.Repositories;
using EaseClub.Infrastructure.Auth.Services;
using EaseClub.Infrastructure.Data;
using EaseClub.Infrastructure.Data.Repositories;
using EaseClub.Infrastructure.Services;
using EaseClub.Infrastructure.Services.QueryServices;
using EaseClub.Infrastructure.Settings;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services,IConfiguration configuration,
            IHostEnvironment env)
        {
            services.AddConfigurations(configuration)
                    .AddDatabase(configuration,env)
                    .AddJwtService(configuration)
                    .AddRepositories()
                    .AddQueryServices()
                    .AddServices();

            return services;
        }

        private static IServiceCollection AddDatabase(this IServiceCollection services,IConfiguration configuration,
            IHostEnvironment env)
        {
            if (env.IsEnvironment("testing"))
            {
                var uniqueDbName = Guid.NewGuid().ToString();
                services.AddDbContext<AppDbContext>(options =>
                options.UseInMemoryDatabase(uniqueDbName));
            }
            else
            {
                var typeOfDB = env.IsDevelopment() ? "Dev" : "Prod";
            var connectionString = configuration.GetConnectionString(typeOfDB);

            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(connectionString));
            }

            services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<AppDbContext>());
            services.AddIdentity<AuthUser, IdentityRole<Guid>>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequiredLength = 10;
                options.SignIn.RequireConfirmedEmail = false;
                options.SignIn.RequireConfirmedPhoneNumber = false;
                options.SignIn.RequireConfirmedAccount = false;
            }).AddEntityFrameworkStores<AppDbContext>().AddDefaultTokenProviders();

            services.AddScoped<DbInitializer>();

            return services;
        }
        private static IServiceCollection AddJwtService(this IServiceCollection services, IConfiguration configuration)
        {
            #region JwtSettings
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options =>
            {
                options.SaveToken = true;
                options.RequireHttpsMetadata = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = configuration["JwtSettings:Issuer"],
                    ValidAudience = configuration["JwtSettings:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(configuration["JwtSettings:SecretKey"])),
                    RoleClaimType = ClaimTypes.Role
                };
            });
            #endregion
            services.AddScoped<IJwtService, JwtService>();
            return services;
        }

        private static IServiceCollection AddServices(this IServiceCollection services)
        {
            //Auth Services
            services.AddScoped<IAuthIdentityService,AuthIdentityService>();
            services.AddScoped<IAuthSessionService,AuthSessionService>();
            services.AddScoped<IClubAuthorizationService, ClubAuthorizationService>();

            services.AddScoped<IEmailService, TurboEmailService>();
            return services;
        }

        private static IServiceCollection AddQueryServices(this IServiceCollection services)
        {
            services.AddScoped<IMembershipPlanQueryService, MembershipPlanQueryService>();
            services.AddScoped<IApplicationTemplateQueryService, ApplicationTemplateQueryService>();
            return services;
        }
        private static IServiceCollection AddRepositories(this IServiceCollection Services)
        {
            
            //Services.AddScoped<IUnitOfWork,AppDbContext>();
            Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
            Services.AddScoped<IMemberUserRepository, MemberUserRepository>();
            Services.AddScoped<IClubRepository, ClubRepository>();
            Services.AddScoped<IBranchRepository, BranchRepository>();
            Services.AddScoped<IClubAdminUserRepository,ClubAdminUserRepository>();
            Services.AddScoped<IUserBaseRepository,UserBaseRepository>();
            Services.AddScoped<IMembershipTypeRepository, MembershipTypeRepository>();
            Services.AddScoped<IMembershipPlanRepository, MembershipPlanRepository>();
            Services.AddScoped<IInstallmentsTemplatesRepository, InstallmentsRepository>();
            Services.AddScoped<IApplicationTemplateRepository, ApplicationTemplateRepository>();
            Services.AddScoped<IApplicationStepRepository, ApplicationStepRepository>();
            Services.AddScoped<IApplicationSectionRepository, ApplicationSectionRepository>();
            Services.AddScoped<IMembershipApplicationRepository,MembershipApplicationsRepository>();
            Services.AddScoped<IApplicationFieldRepository, ApplicationFieldRepository>();
            Services.AddScoped<IPricingPolicyRepository,PricingPolicyRepository>();

            return Services;
        }
        public static IServiceCollection AddConfigurations(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<EmailSettings>(configuration.GetSection("EmailSettings"));

            services.Configure<JwtSettings>(configuration.GetSection("JwtSettings"));

            return services;
        }
    }
}
