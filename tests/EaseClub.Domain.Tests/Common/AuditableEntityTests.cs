using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using EaseClub.Domain.Common;

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
            Assert.NotEqual(default(DateTime), auditableEntity.CreatedAt);
            Assert.Equal(userId, auditableEntity.CreatedBy);
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
            Assert.NotNull(auditableEntity.UpdatedAt);
            Assert.Equal(userId, auditableEntity.UpdatedBy);
        }
    }
 }
