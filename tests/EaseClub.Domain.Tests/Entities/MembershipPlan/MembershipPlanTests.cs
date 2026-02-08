using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EaseClub.Domain.MembershipPlans;
using FluentAssertions;
namespace EaseClub.Domain.Tests.Entities.MembershipPlan
{
    public class MembershipPlanTests
    {
        private MembershipPlans.MembershipPlan CreatePlan(int durationInDays = 60,decimal totalPrice = 1000m)
        {
            return MembershipPlans.MembershipPlan.Create(
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid(),
                "Gold Plan",
                totalPrice,
                durationInDays
            ).Value;
        }

        [Fact]
        public void AddInstallmentTemplate_Should_Fail_When_Template_Is_Null()
        {
            var plan = CreatePlan();

            var result = plan.AddInstallmentTemplate(null);

            result.IsError.Should().BeTrue();
            result.TopError.Should().Be(
                MembershipPlanErrors.InstallmenttTemplateMustBeProvided
            );
        }
        [Fact]
        public void AddInstallmentTemplate_Should_Add_Template_To_Plan()
        {
            // Arrange
            var plan =  CreatePlan();

            var template = InstallmentTemplateBuilder.CreateValidTemplate();

            // Act
            var result = plan.AddInstallmentTemplate(template);

            // Assert
            result.IsSuccess.Should().BeTrue();
            plan.InstallmentTemplates.Should().HaveCount(1);
            plan.InstallmentTemplates.First().InstallmentTemplateId.Should().Be(template.Id);
        }

        [Fact]
        public void AddInstallmentTemplate_Should_Fail_When_Template_Already_Added()
        {
            var plan = CreatePlan();

            var template = InstallmentTemplateBuilder.CreateValidTemplate();

            plan.AddInstallmentTemplate(template);

            var result = plan.AddInstallmentTemplate(template);

            result.IsError.Should().BeTrue();
            result.TopError.Should().Be(MembershipPlanErrors.InstallmenttTemplateAlreadyAddedForThisPlan);
        }

        [Fact]
        public void GenerateMembershipInstallments_Should_Generate_Correct_Installments()
        {
            // Arrange
            var plan = CreatePlan();
            var template = InstallmentTemplateBuilder.CreateValidTemplate();

            plan.AddInstallmentTemplate(template);

            var membershipId = Guid.NewGuid();

            // Act
            var result = plan.GenerateMembershipInstallments(membershipId, template);

            // Assert
            result.IsSuccess.Should().BeTrue();

            var installments = result.Value;

            installments.Should().HaveCount(2);

            installments[0].Amount.Should().Be(500m);
            installments[1].Amount.Should().Be(500m);

            installments[0].Status.Should().Be(InstallmentStatus.Pending);
            installments[1].Status.Should().Be(InstallmentStatus.Pending);

            installments[1].DueDate.Should().BeAfter(installments[0].DueDate);
        }

        [Fact]
        public void GenerateMembershipInstallments_Total_Amount_Should_Equal_Plan_TotalPrice()
        {
            var plan = CreatePlan(totalPrice: 999m);
            var template = InstallmentTemplateBuilder.CreateValidTemplate();

            plan.AddInstallmentTemplate(template);

            var result = plan.GenerateMembershipInstallments(Guid.NewGuid(), template);

            var total = result.Value.Sum(x => x.Amount);

            total.Should().Be(plan.TotalPrice);
        }

        [Fact]
        public void GenerateMembershipInstallments_Should_Fail_When_Template_Not_Assigned()
        {
            var plan = CreatePlan();
            var template = InstallmentTemplateBuilder.CreateValidTemplate();

            var result = plan.GenerateMembershipInstallments(Guid.NewGuid(), template);

            result.IsError.Should().BeTrue();
            result.TopError.Should().Be(MembershipPlanErrors.InstallmentTemplateDoesntExist);
        }

        [Fact]
        public void AddInstallmentTemplate_Should_Fail_When_DueAfter_Exceeds_Plan_Duration()
        {
            var plan = CreatePlan(durationInDays: 30);

            var template = InstallmentTemplate.Create(
                Guid.NewGuid(),
                Guid.NewGuid(),
                "abc",
                null,
                null,
                new List<Installment>
                {
            Installment.Create(50m, 0, 0).Value,
            Installment.Create(50m, 60, 1).Value
                }
            ).Value;

            var result = plan.AddInstallmentTemplate(template);

            result.IsError.Should().BeTrue();
            result.TopError.Should().Be(
                MembershipPlanErrors.InstallmentTemplateExceedsPlanDuration
            );
        }

        [Fact]
        public void GenerateMembershipInstallments_Should_Preserve_OrderIndex()
        {
            var plan = CreatePlan();
            var template = InstallmentTemplateBuilder.CreateValidTemplate();

            plan.AddInstallmentTemplate(template);

            var result = plan.GenerateMembershipInstallments(Guid.NewGuid(), template);

            result.Value.Select(x => x.Order)
                .Should()
                .BeInAscendingOrder()
                .And.HaveCount(result.Value.Count);
        }

        [Fact]
        public void RemoveInstallmentTemplate_ShouldSucceed_WhenTemplateExistsInPlan()
        {
            // Arrange
            var plan = CreatePlan();
            var template = InstallmentTemplateBuilder.CreateValidTemplate(); // Assume this helper exists
            plan.AddInstallmentTemplate(template);

            // Act
            var result = plan.RemoveInstallmentTemplate(template.Id);

            // Assert
            result.IsSuccess.Should().BeTrue();
            plan.InstallmentTemplates.Should().BeEmpty();
            plan.InstallmentTemplates.Should().NotContain(x => x.InstallmentTemplateId == template.Id);
        }

        [Fact]
        public void RemoveInstallmentTemplate_ShouldFail_WhenTemplateIsNotInPlan()
        {
            // Arrange
            var plan = CreatePlan();
            var randomId = Guid.NewGuid();

            // Act
            var result = plan.RemoveInstallmentTemplate(randomId);

            // Assert
            result.IsError.Should().BeTrue();
            result.TopError.Should().Be(MembershipPlanErrors.InstallmentTemplateNotFoundInThisPlan);
        }

        [Fact]
        public void RemoveInstallmentTemplate_ShouldOnlyRemoveTargetTemplate_WhenMultipleExist()
        {
            // Arrange
            var plan = CreatePlan();
            var template1 = InstallmentTemplateBuilder.CreateValidTemplate();
            var template2 = InstallmentTemplateBuilder.CreateValidTemplate();

            plan.AddInstallmentTemplate(template1);
            plan.AddInstallmentTemplate(template2);

            // Act
            plan.RemoveInstallmentTemplate(template1.Id);

            // Assert
            plan.InstallmentTemplates.Should().HaveCount(1);
            plan.InstallmentTemplates.Should().Contain(x => x.InstallmentTemplateId == template2.Id);
            plan.InstallmentTemplates.Should().NotContain(x => x.InstallmentTemplateId == template1.Id);
        }

    }
}
