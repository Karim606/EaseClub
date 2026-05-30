using EaseClub.Application.Common.Interfaces;
using EaseClub.Application.Features.MembershipPlans.Command.AddTemplateToPlan;
using EaseClub.Application.Features.MembershipPlans.Command.AddInstallmentTemplateToPlan;
using EaseClub.Domain.Common.Results;
using EaseClub.Domain.MembershipPlans;
using EaseClub.Domain.MembershipPlans.Repositories;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace EaseClub.Application.Tests.Features.MembershipPlans.Commands
{
    public class AddInstallmentTemplateToPlanHandlerTests
    {
        private readonly Mock<IMembershipPlanRepository> _planRepo = new();
        private readonly Mock<IInstallmentsTemplatesRepository> _templateRepo = new();
        private readonly Mock<IUnitOfWork> _unitOfWork = new();
        private readonly Mock<ILogger<AddInstallmentTemplateToPlanHandler>> _loggerMock = new();

        private readonly AddInstallmentTemplateToPlanHandler _handler;

        public AddInstallmentTemplateToPlanHandlerTests()
        {
            _handler = new AddInstallmentTemplateToPlanHandler(
                _planRepo.Object,
                _templateRepo.Object,
                _unitOfWork.Object,
                _loggerMock.Object);
        }

        [Fact]
        public async Task Handle_Should_Return_NotFound_When_Plan_Does_Not_Exist()
        {
            var command = new AddInstallmentTemplateToPlanCommand(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());

            _planRepo.Setup(x => x.GetPlanWithDetailsAsync(command.PlanId, It.IsAny<CancellationToken>()))
                     .ReturnsAsync((MembershipPlan?)null);

            var result = await _handler.Handle(command, default);

            result.IsError.Should().BeTrue();
            result.TopError.Should().Be(MembershipPlanErrors.NotFound);
        }

        [Fact]
        public async Task Handle_Should_Return_Error_When_Template_Does_Not_Exist()
        {
            var plan = MembershipPlan.Create(
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid(),
                EnrollmentMode.DirectPay,
                null,
                1,
                2,
                "abc",
                2000,
                60,
                1500,
                true,
                PaymentMode.Installments
            ).Value;

            var command = new AddInstallmentTemplateToPlanCommand(plan.ClubId, plan.Id, Guid.NewGuid());

            _planRepo.Setup(x => x.GetPlanWithDetailsAsync(command.PlanId, It.IsAny<CancellationToken>()))
                     .ReturnsAsync(plan);

            _templateRepo.Setup(x => x.GetByIdAsync(command.TemplateId, CancellationToken.None))
                          .ReturnsAsync((InstallmentTemplate?)null);

            var result = await _handler.Handle(command, default);

            result.IsError.Should().BeTrue();
            result.TopError.Should().Be(MembershipPlanErrors.InstallmentTemplateDoesntExist);
        }

        [Fact]
        public async Task Handle_Should_Save_When_Addition_Is_Successful()
        {
            var plan = MembershipPlan.Create(
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid(),
                EnrollmentMode.DirectPay,
                null,
                1,
                2,
                "abc",
                2000,
                60,
                1500,
                true,
                PaymentMode.Installments
            ).Value;

            var template = InstallmentTemplate.Create(Guid.NewGuid(), plan.ClubId, "def", 3, 60, null).Value;

            var command = new AddInstallmentTemplateToPlanCommand(plan.ClubId, plan.Id, template.Id);

            _planRepo.Setup(x => x.GetPlanWithDetailsAsync(command.PlanId, It.IsAny<CancellationToken>()))
                     .ReturnsAsync(plan);

            _templateRepo.Setup(x => x.GetByIdAsync(command.TemplateId, CancellationToken.None))
                          .ReturnsAsync(template);

            var result = await _handler.Handle(command, default);

            result.IsError.Should().BeFalse();
            _unitOfWork.Verify(x => x.SaveChangesAsync(default), Times.Once);
        }
    }
}
