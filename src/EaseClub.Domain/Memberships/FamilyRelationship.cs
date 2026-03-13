using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Domain.Memberships
{
    public enum FamilyRelationship
    {
        Wife = 1,
        Husband = 2,
        Son = 3,
        Daughter = 4,
        Father = 5,
        Mother = 6,
        Brother = 7,  // New
        Sister = 8,   // New
        Other = 99
    }
}
