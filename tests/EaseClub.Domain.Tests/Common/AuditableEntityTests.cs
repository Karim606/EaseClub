using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using EaseClub.Domain.Common;
using FluentAssertions;

namespace EaseClub.Domain.Tests.Common
{
    public sealed class AuditableEntityTests
    {
        private class TestAuditableEntity : AuditableEntity
        {
            public TestAuditableEntity() : base() { }
            public TestAuditableEntity(Guid id) : base(id) { }
        }

        [Fact]
        public void SetCreated_ShouldSetCreatedAtAndCreatedByProperties()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var auditableEntity = new TestAuditableEntity();
            // Act
            auditableEntity.SetCreated(userId);
            // Assert
            auditableEntity.CreatedBy.Should().Be(userId);
            auditableEntity.CreatedAt.Should().NotBe(default(DateTime));

        }
    

    [Fact]
        public void SetUpdated_ShouldSetUpdatedAtAndUpdatedByProperties()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var auditableEntity = new TestAuditableEntity();
            // Act
            auditableEntity.SetUpdated(userId);
            // Assert
            auditableEntity.UpdatedAt.Should().NotBeNull();
            auditableEntity.UpdatedBy.Should().Be(userId);
            ;
        }
    }
 }
