using EaseClub.Domain.Common.Results;
using EaseClub.Domain.Files;
using EaseClub.Domain.Files.Enums;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Application.Features.Files.Queries.GetFileQuery
{
    public record GetFileQuery(Guid FileId) : IRequest<Result<FileDto>>;
    public record FileDto(
        Guid Id,
        string FileName,
        string Url,
        string ContentType,
        long Size,
        FilePurpose Category
    );

}
