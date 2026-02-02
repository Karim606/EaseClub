using EaseClub.Domain.Common.Interfaces;
using EaseClub.Domain.Member;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Domain.Common
{
    public interface IUserBaseRepository:IRepository<UserBase>
    {
        Task<UserBase> GetByEmailAsync(string email);
        Task<bool> PhoneExistsAsync(string phoneNumber);
        Task<bool> EmailExistsAsync(string email);
    }
}
