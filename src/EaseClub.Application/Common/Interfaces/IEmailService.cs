using EaseClub.Domain.Common.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Application.Common.Interfaces
{
    public interface IEmailService
    {
        public Task<Result<Success>> SendAsync(string toEmail, string Subject, string Message);
    }
}
