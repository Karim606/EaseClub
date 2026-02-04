using EaseClub.Domain.MembershipTypes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Infrastructure.Data.Configurations
{
    public class MembershipTypeBranchConfiguration: IEntityTypeConfiguration<MembershipTypeBranch>
    {
        public void Configure(EntityTypeBuilder<MembershipTypeBranch> builder)
        {

            builder.HasKey(x => new { x.MembershipTypeId, x.BranchId });

            builder.HasIndex(x => x.BranchId);


            builder.HasOne(x => x.Branch)
                   .WithMany(x => x.MembershipTypeBranchesList)            
                   .HasForeignKey(x => x.BranchId)
                   .OnDelete(DeleteBehavior.Restrict); 

        }
    }
}
