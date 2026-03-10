using EaseClub.Application.Common.Interfaces;
using EaseClub.Domain.ClubAdmin;
using EaseClub.Domain.Common;
using EaseClub.Domain.Member;
using EaseClub.Infrastructure.Auth.Entities;
using EaseClub.Domain.Clubs;
using EaseClub.Domain.Branches;

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using EaseClub.Domain.MembershipTypes;
using EaseClub.Domain.MembershipPlans;
using EaseClub.Domain.ApplicationTemplates;
using EaseClub.Domain.MembershipApplications;
using EaseClub.Domain.PricingPolices;
using EaseClub.Domain.Memberships;

namespace EaseClub.Infrastructure.Data
{
    public class AppDbContext : IdentityDbContext<AuthUser, IdentityRole<Guid>, Guid>, IUnitOfWork
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

            builder.Entity<AuthUser>().ToTable("AuthUsers");
            builder.Entity<IdentityRole<Guid>>().ToTable("Roles");
            builder.Entity<IdentityUserRole<Guid>>().ToTable("UserRoles");
            builder.Entity<IdentityUserClaim<Guid>>().ToTable("UserClaims");
            builder.Entity<IdentityUserLogin<Guid>>().ToTable("UserLogins");
            builder.Entity<IdentityRoleClaim<Guid>>().ToTable("RoleClaims");
            builder.Entity<IdentityUserToken<Guid>>().ToTable("UserTokens");

            builder.Entity<UserBase>()
            .ToTable("Users");       // base table

            // MemberUser table
            builder.Entity<MemberUser>()
            .ToTable("MemberUsers")
            .HasBaseType<UserBase>();

            // ClubAdminUser table
            builder.Entity<ClubAdminUser>()
            .ToTable("ClubAdminUsers")
            .HasBaseType<UserBase>();
        }

        public async Task<IDbContextTransaction> BeginTransactionAsync(
        CancellationToken cancellationToken = default)
        {
            // Prevent accidental nested transactions
            if (Database.CurrentTransaction != null)
                return Database.CurrentTransaction;

            return await Database.BeginTransactionAsync(cancellationToken);
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return await base.SaveChangesAsync();
        }

        public DbSet<AuthUser> AuthUsers => Users;
        public DbSet<RefreshToken> RefreshTokens { get; set; }

        public DbSet<UserBase> UsersBase { get; set; }
        public DbSet<MemberUser> MemberUsers { get; set; }
        public DbSet<ClubAdminUser> ClubAdminUsers { get; set; }

        public DbSet<MembershipType> MembershipTypes { get; set; }
        public DbSet<MembershipTypeBranch> MembershipTypeBranches { get; set; }
        public DbSet<Club> Clubs { get; set; }
        public DbSet<Branch> Branches { get; set; }

        public DbSet<MembershipPlan> MembershipPlans { get; set; }
        public DbSet<InstallmentTemplate> InstallmentTemplates { get; set; }
        public DbSet<PlanInstallmentTemplate> PlanInstallmentTemplates { get; set; }
        public DbSet<MembershipInstallment> MembershipInstallments { get; set; }

        public DbSet<ApplicationTemplateDefinition>ApplicationTemplateDefinitions { get; set; }
        public DbSet<ApplicationStepDefinition> ApplicationStepDefinitions { get; set; }
        public DbSet<ApplicationSectionDefinition> ApplicationSectionDefinitions { get; set; }
        public DbSet<ApplicationFieldDefinition> ApplicationFieldDefinitions { get; set; }

        public DbSet<MembershipApplication> MembershipApplications { get; set; }
        public DbSet<ApplicationAnswer> ApplicationAnswers { get; set; }
        public DbSet<ApplicationReview> ApplicationReviews { get; set; }
        public DbSet<PricingPolicy> PricingPolicies { get; set; }

        public DbSet<Membership>Memberships { get; set; }
       

     }
}
