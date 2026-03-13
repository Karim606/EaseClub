using EaseClub.Application.Common.Interfaces;
using EaseClub.Application.Features.MembershipPlans.Command.RemoveInstallmentTemplateFromPlan;
using EaseClub.Domain.Common;
using EaseClub.Domain.MembershipPlans;
using EaseClub.Domain.MembershipPlans.Repositories;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Application.Tests.Features.MembershipPlans.Commands
{
    public class RemoveInstallmentTemplateFromPlanCommandHandlerTests
    {
        private readonly Mock<IMembershipPlanRepository> _planRepo = new();
        private readonly Mock<IInstallmentsTemplatesRepository> _templateRepo = new();
        private readonly Mock<IUnitOfWork> _unitOfWork = new();
        private readonly Mock<ILogger<RemoveInstallmentTemplateFromPlanCommandHandler>> _logger = new();

        private readonly RemoveInstallmentTemplateFromPlanCommandHandler _handler;

        public RemoveInstallmentTemplateFromPlanCommandHandlerTests()
        {
            _handler = new RemoveInstallmentTemplateFromPlanCommandHandler(
                _planRepo.Object,
                _templateRepo.Object,
                _logger.Object,
                _unitOfWork.Object);
        }

        [Fact]
        public async Task Handle_Should_Return_NotFound_When_Template_Does_Not_Exist()
        {
            var command = new RemoveInstallmentTemplateFromPlanCommand(
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid());

            _templateRepo
                .Setup(x => x.GetByIdAsync(command.TemplateId, CancellationToken.None))
                .ReturnsAsync((InstallmentTemplate?)null);

            var result = await _handler.Handle(command, default);

            result.IsError.Should().BeTrue();
            result.TopError.Type.Should().Be(ErrorKind.NotFound);
        }

        [Fact]
        public async Task Handle_Should_Return_NotFound_When_Plan_Does_Not_Exist()
        {
            var template = InstallmentTemplate.Create(Guid.NewGuid(), Guid.NewGuid(), "Temp", 3, 60, null).Value;

            var command = new RemoveInstallmentTemplateFromPlanCommand(
                template.ClubId,
                Guid.NewGuid(),
                template.Id);

            _templateRepo
                .Setup(x => x.GetByIdAsync(command.TemplateId, CancellationToken.None))
                .ReturnsAsync(template);

            _planRepo
                .Setup(x => x.GetByIdAsync(command.PlanId, CancellationToken.None))
                .ReturnsAsync((MembershipPlan?)null);

            var result = await _handler.Handle(command, default);

            result.IsError.Should().BeTrue();
            result.TopError.Type.Should().Be(ErrorKind.NotFound);
        }

        [Fact]
        public async Task Handle_Should_Save_When_Removal_Is_Successful()
        {
            var plan = MembershipPlan.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),1,2, "Plan", 2000, 60).Value;
            var template = InstallmentTemplate.Create(Guid.NewGuid(), plan.ClubId, "Temp", 3, 60, null).Value;

            plan.AddInstallmentTemplate(template);

            var command = new RemoveInstallmentTemplateFromPlanCommand(
                plan.ClubId,
                plan.Id,
                template.Id);

            _templateRepo
                .Setup(x => x.GetByIdAsync(command.TemplateId,CancellationToken.None))
                .ReturnsAsync(template);

            _planRepo
                .Setup(x => x.GetByIdAsync(command.PlanId, CancellationToken.None))
                .ReturnsAsync(plan);

            var result = await _handler.Handle(command, default);

            result.IsError.Should().BeFalse();
            _unitOfWork.Verify(x => x.SaveChangesAsync(default), Times.Once);
        }
    }
}
