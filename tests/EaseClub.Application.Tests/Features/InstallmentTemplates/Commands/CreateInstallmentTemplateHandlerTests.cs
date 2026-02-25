using EaseClub.Application.Common.Interfaces;
using EaseClub.Application.Features.InstallmentTemplates.Commands.CreateInstallmentTemplate;
using EaseClub.Application.Features.InstallmentTemplates.Queries.GetTemplateById;
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

namespace EaseClub.Application.Tests.Features.InstallmentTemplates.Commands
{
    public class CreateInstallmentTemplateHandlerTests
    {
        private readonly Mock<IInstallmentsTemplatesRepository> _repo = new();
        private readonly Mock<IUnitOfWork> _unitOfWork = new();
        private readonly Mock<ILogger<CreateInstallmentTemplateHandler>> _logger = new();

        private readonly CreateInstallmentTemplateHandler _handler;

        public CreateInstallmentTemplateHandlerTests()
        {
            _handler = new CreateInstallmentTemplateHandler(
                _repo.Object,
                _unitOfWork.Object,
                _logger.Object);
        }

        [Fact]
        public async Task Handle_Should_Return_Error_When_Domain_Creation_Fails()
        {
            var command = new CreateInstallmentTemplateCommand(
                "",                     // invalid name → domain error
                0,                      // invalid installments count
                0,
                null
            );

            var result = await _handler.Handle(command, CancellationToken.None);

            result.IsError.Should().BeTrue();
            _repo.Verify(x => x.AddAsync(It.IsAny<InstallmentTemplate>(), CancellationToken.None), Times.Never);
            _unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_Should_Save_And_Return_Id_When_Creation_Is_Successful()
        {
            var command = new CreateInstallmentTemplateCommand(
                "Standard Plan",
                3,
                60,
                null
            );

            var result = await _handler.Handle(command, CancellationToken.None);

            result.IsError.Should().BeFalse();
            result.Value.Should().NotBe(Guid.Empty);

            _repo.Verify(x => x.AddAsync(It.IsAny<InstallmentTemplate>(), CancellationToken.None), Times.Once);
            _unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
