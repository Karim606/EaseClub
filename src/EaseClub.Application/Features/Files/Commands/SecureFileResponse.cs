using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Application.Features.Files.Commands
{
    public record SecureFileResponse(
        Guid FileId,
        string FileName
    );
}
