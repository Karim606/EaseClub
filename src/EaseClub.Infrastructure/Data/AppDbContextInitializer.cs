using EaseClub.Domain.Branches;
using EaseClub.Domain.ClubAdmin;
using EaseClub.Domain.Clubs;
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


    public class DbInitializer(AppDbContext appDbContext,ILogger<DbInitializer>logger
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
                logger.LogInformation("DEV: Dropping and recreating database...");
                await appDbContext.Database.EnsureDeletedAsync();
                await appDbContext.Database.EnsureCreatedAsync();
                logger.LogInformation("DEV: Database recreated successfully.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error during dev database initialization.");
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
                logger.LogInformation("PROD: Applying pending migrations...");
                await appDbContext.Database.MigrateAsync();
                logger.LogInformation("PROD: Migrations applied successfully.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error during production database migration.");
                throw;
            }
        }

        #endregion

        #region Seeding
        private static readonly Guid SeedClubId =
        Guid.Parse("9f3a8b6e-2a7d-4b5c-9d9c-1e8c4c2f7a31");

        public async Task SeedAsync()
        {
            using var transaction = await appDbContext.Database.BeginTransactionAsync();
            try
            {
                
                await TrySeeding();
                await appDbContext.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch (Exception ex) { 
                await transaction.RollbackAsync();
                logger.LogError(ex, "An error occurred while seeding database.");
                throw;
            }
        }

        private async Task TrySeeding()
        {
            await SeedClubsAndBranches();
            await SeedRolesAndUsers();
            
        }

        #region SeedUserAndRoles
        //--------------------------------Seed Roles and Domain-Specific Users------------------------------------------
        private async Task SeedRolesAndUsers()
        {
            // 1. Add roles
            await AddRole("ClubAdmin");
            await AddRole("Member");

            // 2. Add domain-specific users
            await AddClubAdminUser(
                id: Guid.NewGuid(),
                clubId:SeedClubId,
                firstName: "Alex",
                lastName: "ClubAdmin",
                phoneNumber: "01012345676",
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


        private async Task AddAuthUser(Guid id,string Email,string Password,string role)
        {
            var existing = await userManager.FindByEmailAsync(Email);

                if (existing != null)
                {
                    if (existing.Id != id)
                        throw new InvalidOperationException(
                            $"AuthUser with email {Email} exists with different Id.");

                    // Same user already exists → OK
                    return;
                }

            var user = new AuthUser
            {
                Id = id,
                Email = Email,
                UserName = Email,
                NormalizedEmail = Email.ToUpperInvariant(),
                NormalizedUserName = Email.ToUpperInvariant()
            };

            var result = await userManager.CreateAsync(user, Password);
            if (!result.Succeeded)
                throw new Exception(string.Join(", ", result.Errors.Select(e => e.Description)));

            var roleResult = await userManager.AddToRoleAsync(user, role);
            if (!roleResult.Succeeded)
                throw new Exception(string.Join(", ", roleResult.Errors.Select(e => e.Description)));
        }

        private async Task AddRole(string roleName)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                var role = new IdentityRole<Guid>()
                {
                    Id =  Guid.NewGuid(),
                    Name = roleName
                };
                var result = await roleManager.CreateAsync(role);
                if (result.Succeeded)
                {
                    logger.LogInformation("Role {RoleName} created successfully", roleName);
                }
                else
                {
                    logger.LogWarning("Failed to create role {RoleName}. Errors: {Errors}", roleName, string.Join(", ", result.Errors.Select(e => e.Description)));
                }
            }
            else
            {
                    logger.LogInformation("Role {RoleName} already exists", roleName);
            }

        }

        private async Task AddClubAdminUser(Guid id,Guid clubId,string firstName,string lastName,string phoneNumber,string email)
        {
            PhoneNumber phone = PhoneNumber.Create(phoneNumber).Value;
            Email userEmail = Email.Create(email).Value;

            var Exist =  appDbContext.ClubAdminUsers.Any(CA => CA.Email.Value == email );
            if (!Exist)
            {
                var clubAdminUser = ClubAdminUser.Create(id,clubId, firstName, lastName, phone, userEmail);
                
                await appDbContext.ClubAdminUsers.AddAsync(clubAdminUser);
                await AddAuthUser(id, email, "Admin123456", "ClubAdmin");

                logger.LogInformation("ClubAdminUser with Email {Email} added successfully", email);
            }
            else
            {
                logger.LogInformation("ClubAdminUser with Email {Email} already exists", email);
            }
        }

        private async Task AddMemberUser(Guid id, string firstName, string lastName, string phoneNumber, string email)
        {
            PhoneNumber phone = PhoneNumber.Create(phoneNumber).Value;
            Email userEmail = Email.Create(email).Value;

            var exist = appDbContext.MemberUsers.Any(CA => CA.Email.Value == email);
            if (!exist)
            {
                var memberUser = MemberUser.Create(id, firstName, lastName, phone, userEmail);
                await appDbContext.MemberUsers.AddAsync(memberUser);
                await AddAuthUser(id,email, "User123456", "Member");

                logger.LogInformation("memberUser with Email {Email} added successfully", email);
            }
            else
            {
                logger.LogInformation("memberUser with Email {Email} already exists", email);
            }
        }
        #endregion SeedUserAndRoles

        #region SeedClubsAndBranches
        private async Task SeedClubsAndBranches()
        {
            // Example club
            var club = await EnsureClubExists(
                clubId: SeedClubId,
                name: "Ease Club"
            );

            // Branches for that club
            await EnsureBranchExists(club.Id, "Main Branch");
            await EnsureBranchExists(club.Id, "Downtown Branch");
        }
        private async Task<Club> EnsureClubExists(Guid clubId,string name)
        {
            var existingClub = appDbContext.Clubs
                .FirstOrDefault(c => c.Id == clubId);

            if (existingClub != null)
            {
                logger.LogInformation("Club {ClubId} already exists", clubId);
                return existingClub;
            }

            var club = Club.Create(clubId,name).Value;

            await appDbContext.Clubs.AddAsync(club);
            logger.LogInformation("Club {ClubName} created successfully", name);

            return club;
        }

        private async Task EnsureBranchExists(Guid clubId, string branchName)
        {
            var exists = appDbContext.Branches
                .Any(b => b.ClubId == clubId && b.Name == branchName);

            if (exists)
            {
                logger.LogInformation(
                    "Branch {BranchName} already exists for Club {ClubId}",
                    branchName,
                    clubId
                );
                return;
            }

            var branch = Branch.Create(Guid.NewGuid(), clubId, branchName).Value;

            await appDbContext.Branches.AddAsync(branch);

            logger.LogInformation(
                "Branch {BranchName} added to Club {ClubId}",
                branchName,
                clubId
            );
        }
        #endregion SeedClubsAndBranches

        #endregion

    }
}
