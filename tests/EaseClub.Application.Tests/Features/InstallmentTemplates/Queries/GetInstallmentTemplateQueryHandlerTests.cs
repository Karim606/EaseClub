using EaseClub.Application.Features.InstallmentTemplates.Queries.GetTemplateById;
using EaseClub.Domain.Common;
using EaseClub.Domain.MembershipPlans;
using EaseClub.Domain.MembershipPlans.Repositories;
using FluentAssertions;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Application.Tests.Features.InstallmentTemplates.Queries
{
    public class GetInstallmentTemplateQueryHandlerTests
    {
        private readonly Mock<IInstallmentsTemplatesRepository> _repo = new();
        private readonly GetInstallmentTemplateQueryHandler _handler;

        public GetInstallmentTemplateQueryHandlerTests()
        {
            _handler = new GetInstallmentTemplateQueryHandler(_repo.Object);
        }

        [Fact]
        public async Task Handle_Should_Return_NotFound_When_Template_Does_Not_Exist()
        {
            var query = new GetInstallmentTemplateQuery(Guid.NewGuid());

            _repo.Setup(x => x.GetByIdAsync(query.Id, CancellationToken.None))
                 .ReturnsAsync((InstallmentTemplate?)null);

            var result = await _handler.Handle(query, CancellationToken.None);

            result.IsError.Should().BeTrue();
            result.TopError.Type.Should().Be(ErrorKind.NotFound);
        }

        [Fact]
        public async Task Handle_Should_Return_Template_When_Found()
        {
            var template = InstallmentTemplate.Create(
                Guid.NewGuid(),
                Guid.NewGuid(),
                "Monthly",
                null,
                null,
                new List<Installment>
                {
                    Installment.Create(50, 0, 1).Value,
                    Installment.Create(50, 30, 2).Value
                }
            ).Value;

            _repo.Setup(x => x.GetByIdAsync(template.Id, CancellationToken.None))
                 .ReturnsAsync(template);

            var result = await _handler.Handle(
                new GetInstallmentTemplateQuery(template.Id),
                CancellationToken.None);

            result.IsError.Should().BeFalse();
            result.Value.Name.Should().Be("Monthly");
            result.Value.Installments.Should().HaveCount(2);
            result.Value.Installments[0].Order.Should().Be(1);
        }
    }
}
