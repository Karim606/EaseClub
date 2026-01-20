using EaseClub.Domain.Common;
using EaseClub.Domain.Common.Results;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Domain.Tests.Common
{
    public  class ResultTests
    {
        [Fact]
        public void SuccessResult_ShouldHaveValueAndBeSuccessful()
        {
            //Arrange & Act
            var value = 42;
            Result<int> result = value;

            //Assert
            result.IsSuccess.Should().BeTrue();
            result.IsError.Should().BeFalse();
            result.Value.Should().Be(42);
            result.Errors.Should().BeEmpty();
        }

        [Fact]
        public void ErrorResult_ShouldHaveErrorsAndBeFailed()
        {
            // Arrange
            var error = Error.Validation("CODE", "desc");
            Result<int> result = error;
            //Act & Assert
            Action act = () => { var val = result.Value; };

            result.IsSuccess.Should().BeFalse();
            result.IsError.Should().BeTrue();
            result.Errors.Should().ContainSingle().Which.Should().Be(error);
            result.TopError.Should().Be(error);
            act.Should().Throw<InvalidOperationException>().WithMessage("Cannot access Value on an error result.");

        }


        [Fact]
        public void Match_ShouldExecuteOnSuccess()
        {
            //Arrange
            Result<int> result = 42;
            
            //Act
            var output = result.Match(
                success => success * 2,
                errors => 0);

            //Assert
            output.Should().Be(84);
        }

        [Fact]
        public void Match_ShouldExecuteOnError()
        {
            //Arrange
            var error = Error.Validation();
            Result<int> result = error;

            //Act
            var output = result.Match(
                success => 1,
                errors => errors.Count);

            //Assert
            output.Should().Be(1); // Actually errors.Count is 1
        }

        [Fact]
        public void ImplicitOperators_ShouldWork()
        {
            //Arrange & Act
            Result<int> fromValue = 10;
            Result<int> fromError = Error.Conflict();
            Result<int> fromList = new List<Error> { Error.NotFound() };

            //Assert
            fromValue.IsSuccess.Should().BeTrue();
            fromError.IsSuccess.Should().BeFalse();
            fromList.IsSuccess.Should().BeFalse();
        }
    }
}
