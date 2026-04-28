using EaseClub.Domain.Branches;
using EaseClub.Domain.ClubAdmin;
using EaseClub.Domain.Clubs;
using EaseClub.Domain.Common.ValueObjects;
using EaseClub.Domain.Member;
using EaseClub.Domain.MembershipPlans;
using EaseClub.Domain.MembershipTypes;
using EaseClub.Infrastructure.Auth.Entities;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EaseClub.Domain.Clubs.ValueObjects;
using EaseClub.Domain.ApplicationTemplates;
using EaseClub.Domain.ApplicationTemplates.SystemSections;
using EaseClub.Domain.ApplicationTemplates.ValueObjects.ValidationRulesSet;
using EaseClub.Domain.MembershipApplications.ValueObjects;
using EaseClub.Domain.PricingPolices;
using EaseClub.Domain.MembershipApplications;

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

        private static readonly Guid SeedClubAdminId =
        Guid.Parse("9f3a7b4e-2a7d-4b5c-9d9c-1e8c4c2f7a31");

        private static readonly Guid SeedMemberId = Guid.Parse("d865c243-2d37-4feb-9c01-77d39c6c7912");

        private static readonly Guid SeedMembershipTypeId = Guid.Parse("a1e8b6f2-4f6c-4c4a-9d0f-2a8b7e3c1d94");

        private static readonly Guid SeedMembershipPlanId = Guid.Parse("7c2d4a8e-1b9f-4e2a-8f3c-5b6d9a1e0c47");

        private static readonly Guid SeedInstallmentTemplateId = Guid.Parse("7c2d4a8e-1b9f-4e2a-8f3c-5b6d9a1e0c48");

        private static readonly Guid SeedApplicationTemplateId = Guid.Parse("5a1e8b6f-4f6c-4c4a-9d0f-2a8b7e3c1d95");
        private static readonly Guid SeedMembershipType2Id = Guid.Parse("b2e8b6f2-4f6c-4c4a-9d0f-2a8b7e3c1d96");
        private static readonly Guid SeedMembershipPlan2Id = Guid.Parse("c3e8b6f2-4f6c-4c4a-9d0f-2a8b7e3c1d97");
        private static readonly Guid SeedMembershipApplicationId = Guid.Parse("d4e8b6f2-4f6c-4c4a-9d0f-2a8b7e3c1d98");
        private static readonly Guid SeedPricingPolicy1Id = Guid.Parse("e5e8b6f2-4f6c-4c4a-9d0f-2a8b7e3c1d99");
        private static readonly Guid SeedPricingPolicy2Id = Guid.Parse("f6e8b6f2-4f6c-4c4a-9d0f-2a8b7e3c1d00");

        public async Task SeedAsync()
        {
            if (appDbContext.Database.IsInMemory())
            {
                await TrySeeding();
                await appDbContext.SaveChangesAsync();
                return;
            }
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
            await SeedPricingPolicies();
            await SeedMembershipType_Plan_InstallmentTemplate();
            await SeedMembershipApplications();
        }

        #region SeedUserAndRoles
        //--------------------------------Seed Roles and Domain-Specific Users------------------------------------------
        private async Task SeedRolesAndUsers()
        {
            // 1. Add roles
            await AddRole("ClubAdmin");
            await AddRole("Member");
            await AddRole("SuperAdmin");

            // 2. Add domain-specific users
            await AddClubAdminUser(
                id: SeedClubAdminId,
                clubId:SeedClubId,
                firstName: "Alex",
                lastName: "ClubAdmin",
                phoneNumber: "01012345676",
                email: "admin@example.com"
                );

            await AddMemberUser(
                    id:SeedMemberId,
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
            PhoneNumber defaultPhone = PhoneNumber.Create("01000000000").Value;
            Email defaultEmail = Email.Create("Def@default.com").Value;

            var club = Club.Create(clubId,name,"CLUB", new ContactInfo(defaultPhone,defaultEmail), new List<WorkSchedule>(), new List<Amenity>(),"CLU").Value;

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

            var branch = Branch.Create(Guid.NewGuid(), clubId, branchName, "Default Address").Value;

            await appDbContext.Branches.AddAsync(branch);

            logger.LogInformation(
                "Branch {BranchName} added to Club {ClubId}",
                branchName,
                clubId
            );
        }
        #endregion SeedClubsAndBranches

        #endregion

        public async Task SeedMembershipType_Plan_InstallmentTemplate()
        {
            // 1. Check MembershipType (pro)
            if (!await appDbContext.MembershipTypes.AnyAsync(x => x.Id == SeedMembershipTypeId))
            {
                var membershipType = MembershipType.Create(SeedMembershipTypeId, SeedClubId, "pro").Value;
                await appDbContext.MembershipTypes.AddAsync(membershipType);
            }

            // 2. Check ApplicationTemplate (Standard Form)
            if (!await appDbContext.ApplicationTemplateDefinitions.AnyAsync(x => x.Id == SeedApplicationTemplateId))
            {
                var template = ApplicationTemplateDefinition.Create(SeedApplicationTemplateId, SeedClubId, "Standard Membership Form").Value;
                
                // Seed a simple step with Member Identity
                var stepId = Guid.NewGuid();
                var sectionId = Guid.NewGuid();
                var fieldId = Guid.NewGuid();

                var steps = new List<StepSnapshot>
                {
                    new StepSnapshot(stepId, "Personal Information", 1, new List<SectionSnapshot>
                    {
                        new SectionSnapshot(sectionId, "Identity", 1, null, SectionIntent.General, new List<FieldSnapshot>
                        {
                            new FieldSnapshot(fieldId, "sys_full_name", "Full Name", FieldType.Text, 
                                ValidationRuleSet.Create(true, 3, 100).Value.ToSnapshot(), null, null, 1, true)
                        })
                    })
                };
                template.UpdateSteps(steps);
                await appDbContext.ApplicationTemplateDefinitions.AddAsync(template);
            }

            // 3. Check MembershipType (Premium)
            if (!await appDbContext.MembershipTypes.AnyAsync(x => x.Id == SeedMembershipType2Id))
            {
                var membershipType = MembershipType.Create(SeedMembershipType2Id, SeedClubId, "Premium").Value;
                await appDbContext.MembershipTypes.AddAsync(membershipType);
            }

            // 4. Check MembershipPlan (Direct Pay)
            if (!await appDbContext.MembershipPlans.AnyAsync(x => x.Id == SeedMembershipPlanId))
            {
                var membershipPlan = MembershipPlan.Create(
                    SeedMembershipPlanId, SeedClubId, SeedMembershipTypeId, EnrollmentMode.DirectPay, null, 1, 3, "Elite Monthly", 2000, 60, 1000, false, PaymentMode.Cash
                ).Value;
                await appDbContext.MembershipPlans.AddAsync(membershipPlan);
            }

            // 5. Check InstallmentTemplate — must exist BEFORE MembershipPlan2 is created
            //    so we can immediately link them via AddInstallmentTemplate.
            InstallmentTemplate? installmentTemplate = await appDbContext.InstallmentTemplates
                .FirstOrDefaultAsync(x => x.Id == SeedInstallmentTemplateId);

            if (installmentTemplate == null)
            {
                installmentTemplate = InstallmentTemplate.Create(
                    SeedInstallmentTemplateId, SeedClubId, "Quarterly Plan", 4, 365, null
                ).Value;
                await appDbContext.InstallmentTemplates.AddAsync(installmentTemplate);
                // Flush so the installment template gets its Id persisted before the plan references it
                await appDbContext.SaveChangesAsync();
            }

            // 6. Check MembershipPlan2 (Application Form) — created AFTER InstallmentTemplate exists
            if (!await appDbContext.MembershipPlans.AnyAsync(x => x.Id == SeedMembershipPlan2Id))
            {
                var membershipPlan = MembershipPlan.Create(
                    SeedMembershipPlan2Id, SeedClubId, SeedMembershipType2Id, EnrollmentMode.ApplicationForm, SeedApplicationTemplateId, 1, 5, "Premium Annual (Application)", 5000, 365, 4500, true, PaymentMode.Mixed
                ).Value;

                // Link the installment template — it now definitely exists
                membershipPlan.AddInstallmentTemplate(installmentTemplate);

                await appDbContext.MembershipPlans.AddAsync(membershipPlan);
            }

            await appDbContext.SaveChangesAsync();
        }

        private async Task SeedPricingPolicies()
        {
            if (await appDbContext.PricingPolicies.AnyAsync(x => x.Id == SeedPricingPolicy1Id))
                return;

            var policy1 = PricingPolicy.Create(
                SeedPricingPolicy1Id,
                SeedClubId,
                "Late Fee",
                true, // Increase
                100m, // Fixed Amount
                null,
                null
            ).Value;

            var policy2 = PricingPolicy.Create(
                SeedPricingPolicy2Id,
                SeedClubId,
                "Early Bird Discount",
                false, // Decrease
                null,
                10m, // 10%
                null
            ).Value;

            await appDbContext.PricingPolicies.AddRangeAsync(policy1, policy2);
            logger.LogInformation("Seed PricingPolicies created successfully.");
        }

        private async Task SeedMembershipApplications()
        {
            if (await appDbContext.MembershipApplications.AnyAsync(x => x.Id == SeedMembershipApplicationId))
            {
                logger.LogInformation("Seed MembershipApplication already exists.");
                return;
            }

            var plan = await appDbContext.MembershipPlans
                .Include(p => p.MembershipType)
                .Include(p => p.InstallmentTemplates)
                .FirstOrDefaultAsync(p => p.Id == SeedMembershipPlan2Id);

            var template = await appDbContext.ApplicationTemplateDefinitions
                .FirstOrDefaultAsync(t => t.Id == SeedApplicationTemplateId);

            var installmentTemplate = await appDbContext.InstallmentTemplates
                .FirstOrDefaultAsync(x => x.Id == SeedInstallmentTemplateId);

            if (plan == null || template == null || installmentTemplate == null)
            {
                logger.LogWarning("Seeding Application failed: Required data missing (Plan, Template, or InstallmentTemplate).");
                return;
            }

            logger.LogInformation("Creating Seed MembershipApplication...");
            
            // 1. Create Application
            var instRules = installmentTemplate.Installments.Select(i => i.ToSnapshot()).ToList();
            var snapshot = template.ToSnapshot(0, new(), plan.ToSnapshot(), instRules);
            
            var application = MembershipApplication.Create(
                SeedMembershipApplicationId,
                "APP-2024-001",
                snapshot,
                SeedMemberId,
                SeedClubId,
                plan,
                installmentTemplate, 
                plan.MembershipType,
                template.Id
            ).Value;

            // 2. Add some sample answers to complete the first step
            var step = snapshot.Steps.First();
            var field = step.Sections.First().Fields.First();
            
            var answers = new List<UserAnswer>
            {
                new UserAnswer(field.Id, field.Key, "Seed User Name", null, field.Type)
            };

            application.CompleteStep(step.Order, answers);

            await appDbContext.MembershipApplications.AddAsync(application);
            logger.LogInformation("Seed MembershipApplication created successfully with ID: {AppId}", SeedMembershipApplicationId);
        }
    }
}
