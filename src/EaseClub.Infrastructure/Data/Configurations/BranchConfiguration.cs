using EaseClub.Domain.Branches;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Infrastructure.Data.Configurations
{
    public class BranchConfiguration : IEntityTypeConfiguration<Branch>
    {
        public void Configure(EntityTypeBuilder<Branch> builder)
        {
            builder.HasKey(b => b.Id);

            //Composite Index on Name and ClubId to ensure uniqueness of Branch names within a Club
            builder.HasIndex(b => new { b.Name, b.ClubId })
            .HasDatabaseName("IX_Branch_Name_ClubId") 
            .IsUnique()
            .IsClustered(false);
        }
    }
}
