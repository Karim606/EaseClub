using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Domain.Memberships
{
    public enum FamilyRelationship
    {
        Spouse = 1,
        Son = 2,
        Daughter = 3,
        Father = 4,
        Mother = 5,
        Brother = 6,  // New
        Sister = 7,   // New
        Other = 99
    }
}
