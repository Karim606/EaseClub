using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EaseClub.Domain.Common;


namespace EaseClub.Domain.Tests.Common
{
    public  class EntityTests
    {
        private class TestEntity : Entity
        {
            public TestEntity() : base(Guid.Empty) { }
            public TestEntity(Guid id) : base(id) { }
        }

        [Fact]
        public void DefaultConstructor_ShouldGenerateNewGuidId_WhenIdIsEmpty()
        {
            // Arrange & Act
            var entity = new TestEntity();
            // Assert
            Assert.NotEqual(Guid.Empty, entity.Id);
        }

        [Fact]
        public void Constructor_WithGuidParameter_ShouldAssignProvidedId_WhenIdIsNotEmpty()
        {
            // Arrange
            var expectedId = Guid.NewGuid();
            // Act
            var entity = new TestEntity(expectedId);
            // Assert
            Assert.Equal(expectedId, entity.Id);
        }
    }
}
