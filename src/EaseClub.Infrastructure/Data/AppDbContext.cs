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
            .ToTable("Users")       // base table
            .HasKey(u => u.Id);

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
    }
}
