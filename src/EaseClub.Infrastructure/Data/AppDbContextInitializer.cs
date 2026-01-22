using EaseClub.Domain.ClubAdmin;
using EaseClub.Domain.Common.ValueObjects;
using EaseClub.Domain.Member;
using EaseClub.Infrastructure.Auth.Entities;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Infrastructure.Data
{
    public static class AppDbContextInitializer
    {
        public static async Task Init(this WebApplication app) {

            using var scope = app.Services.CreateScope();
            var initializer = scope.ServiceProvider.GetRequiredService<DbInitializer>();
            var env = app.Environment;

            if (env.IsDevelopment())
            {
                // DEV: Drop + Create database for fast iteration
                await initializer.InitDevAsync();
            }
            else
            {
                // PROD / STAGING: Apply migrations safely
                await initializer.InitProdAsync();
            }

            // Seeding runs in all environments
            await initializer.SeedAsync();
        }
    }


    public class DbInitializer(AppDbContext appDbContext,ILogger<DbInitializer>Logger
        ,UserManager<AuthUser>userManager,RoleManager<IdentityRole<Guid>>roleManager)
    {

       #region Initialization
        /// <summary>
        /// DEV: Drop + create DB every run
        /// </summary>
        public async Task InitDevAsync()
        {
            try
            {
                Logger.LogInformation("DEV: Dropping and recreating database...");
                await appDbContext.Database.EnsureDeletedAsync();
                await appDbContext.Database.EnsureCreatedAsync();
                Logger.LogInformation("DEV: Database recreated successfully.");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error during dev database initialization.");
                throw;
            }
        }

        /// <summary>
        /// PROD / STAGING: Apply migrations safely
        /// </summary>
        public async Task InitProdAsync()
        {
            try
            {
                Logger.LogInformation("PROD: Applying pending migrations...");
                await appDbContext.Database.MigrateAsync();
                Logger.LogInformation("PROD: Migrations applied successfully.");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error during production database migration.");
                throw;
            }
        }

        #endregion

        #region Seeding

        public async Task SeedAsync()
        {
            using var transaction = await appDbContext.Database.BeginTransactionAsync();
            try
            {
                
                await TrySeeding();
                await transaction.CommitAsync();
            }
            catch (Exception ex) { 
                await transaction.RollbackAsync();
                Logger.LogError(ex, "An error occurred while seeding database.");
                throw;
            }
        }

        private async Task TrySeeding()
        {
            await SeedRolesAndUsers();
        }

       
        private async Task SeedRolesAndUsers()
        {
            // 1. Add roles
            await AddRole("ClubAdmin");
            await AddRole("Member");

            // 2. Add users (IdentityAppUser)
            await AddAuthUser(
                Email: "admin@example.com",
                Password: "Admin@123456",
                phoneNumber: "1234567890",
                role: "ClubAdmin"
            );

            await AddAuthUser(
                Email: "user@example.com",
                Password:"User@123456",
                phoneNumber: "0987654321",
                role: "Member"
            );
            // 3. Add domain-specific users
            await AddClubAdminUser(
                id: Guid.NewGuid(),
                firstName: "Alex",
                lastName: "ClubAdmin",
                phoneNumber: "1234567890",
                email: "admin@example.com"
                );

            await AddMemberUser(
                    id: Guid.NewGuid(),
                    firstName: "Ali",
                    lastName: "Asad",
                    phoneNumber: "01012345678",
                    email: "user@example.com"
                );


        }


        private async Task AddAuthUser(string Email,string Password,string phoneNumber,string role)
        {
            bool NotExist = userManager.Users.All(u => u.Email != Email)&&Email!=null;
            if (NotExist) {
                var User = new AuthUser()
                {
                    Id = new Guid(),
                    Email=Email,
                    UserName=Email,
                    NormalizedEmail=Email,
                    PhoneNumber=phoneNumber,
                    
                };
                var result  = await userManager.CreateAsync(User,Password);
                if (result.Succeeded) {
                    Logger.LogInformation("User with Email {Email} created successfully", Email);
                   var added = await userManager.AddToRoleAsync(User, role);
                    if(added.Succeeded)
                    {
                        Logger.LogInformation("User with Email {Email} added to role {Role} successfully", Email,role);
                    }
                    else
                    {
                        Logger.LogWarning("Failed to add user with Email {Email} to role {Role}. Errors: {Errors}", Email, role, string.Join(", ", added.Errors.Select(e => e.Description)));
                    }
                }
            }
            else 
            {
                Logger.LogInformation("User with Email {Email} already exists", Email);
            }

        }

        private async Task AddRole(string roleName)
        {
            if (roleManager.Roles.All(r => r.Name != roleName) && roleName != null)
            {
                var role = new IdentityRole<Guid>()
                {
                    Id = new Guid(),
                    Name = roleName
                };
                var result = await roleManager.CreateAsync(role);
                if (result.Succeeded)
                {
                    Logger.LogInformation("Role {RoleName} created successfully", roleName);
                }
                else
                {
                    Logger.LogWarning("Failed to create role {RoleName}. Errors: {Errors}", roleName, string.Join(", ", result.Errors.Select(e => e.Description)));
                }
            }
            else
            {
                               Logger.LogInformation("Role {RoleName} already exists", roleName);
            }

        }

        private async Task AddClubAdminUser(Guid id,string firstName,string lastName,string phoneNumber,string email)
        {
            PhoneNumber phone = PhoneNumber.Create(phoneNumber).Value;
            Email userEmail = Email.Create(email).Value;
            ClubAdminUser.Create(id, firstName, lastName, phone, userEmail);

          var exist =  appDbContext.ClubAdminUsers.All(CA => CA.Email.Value != email );
            if (exist)
            {
                var clubAdminUser = ClubAdminUser.Create(id, firstName, lastName, phone, userEmail);
                await appDbContext.ClubAdminUsers.AddAsync(clubAdminUser);
                Logger.LogInformation("ClubAdminUser with Email {Email} added successfully", email);
            }
            else
            {
                Logger.LogInformation("ClubAdminUser with Email {Email} already exists", email);
            }
        }

        private async Task AddMemberUser(Guid id, string firstName, string lastName, string phoneNumber, string email)
        {
            PhoneNumber phone = PhoneNumber.Create(phoneNumber).Value;
            Email userEmail = Email.Create(email).Value;
            MemberUser.Create(id, firstName, lastName, phone, userEmail);

            var exist = appDbContext.MemberUsers.All(CA => CA.Email.Value != email);
            if (exist)
            {
                var memberUser = MemberUser.Create(id, firstName, lastName, phone, userEmail);
                await appDbContext.MemberUsers.AddAsync(memberUser);
                Logger.LogInformation("memberUser with Email {Email} added successfully", email);
            }
            else
            {
                Logger.LogInformation("memberUser with Email {Email} already exists", email);
            }
        }

        #endregion

    }
}
