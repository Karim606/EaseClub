using EaseClub.Infrastructure.Auth.Entities;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Infrastructure.Data
{
    public static class AppDbContextIntialiser
    {
        public static async Task Init(this WebApplication app) {

            using var scope = app.Services.CreateScope();
            var intialiser = scope.ServiceProvider.GetRequiredService<DbIntialiser>();
            await  intialiser.initAsync();
            await intialiser.SeedAsync();   
        }
    }


    public class DbIntialiser(AppDbContext appDbContext,ILogger<DbIntialiser>Logger,UserManager<AuthUser>userManager,RoleManager<IdentityRole<Guid>>roleManager)
    {
        public async Task initAsync()
        {
            
            try {
               await  appDbContext.Database.EnsureCreatedAsync();
            
            }
            catch (Exception ex) { Logger.LogError(ex, "An error occurred while initialising database.");
                throw;
            }
        }

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
            await AddRole("Admin");
            

            // 2. Add users (IdentityAppUser)
            await AddAuthUser(
                Email: "admin@example.com",
                Password: "Admin@123456",
                phoneNumber: "1234567890",
                role: "Admin"
            );

            await AddAuthUser(
                Email: "user@example.com",
                Password:"User@123456",
                phoneNumber: "0987654321",
                role: "User"
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

        }


    }
}
