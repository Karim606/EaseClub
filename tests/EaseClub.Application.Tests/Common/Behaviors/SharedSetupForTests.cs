using EaseClub.Domain.Common;
using EaseClub.Domain.Common.Interfaces;
using EaseClub.Domain.Common.Results;

using MediatR;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Application.Tests.Common.Behaviors.sharedSetupForTests
{
    public record TestRequest : IRequest<Result<TestResponse>>;
    public class TestResponse
    {
       public string Data { get; set; } = "Test Response";
    }
}
