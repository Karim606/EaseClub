using Azure.Core;
using EaseClub.Application.Common.Interfaces;
using EaseClub.Domain.ApplicationTemplates;
using EaseClub.Domain.Common;
using EaseClub.Domain.Common.Interfaces;
using EaseClub.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Infrastructure.Services
{


    public class ClubAuthorizationService : IClubAuthorizationService
    {
        private readonly AppDbContext _context;

        public ClubAuthorizationService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<bool> IsUserAdminOfClubAsync(Guid userId, Guid clubId)
        {
            return await _context.ClubAdminUsers
                .AnyAsync(x => x.Id == userId && x.ClubId == clubId);
        }

        public async Task<bool> DoesResourceBelongToClubAsync<TEntity>(
            Guid entityId,
            Guid clubId) where TEntity : Entity,IHaveClub
        {
            return await _context.Set<TEntity>()
                .AnyAsync(x => x.ClubId == clubId && x.Id == entityId);
        }

        public async Task<bool> DoesResourceBelongToUserAsync<TEntity>(
            Guid entityId,
            Guid userId) where TEntity : Entity, IBelongToUser
        {
            return await _context.Set<TEntity>()
                .AnyAsync(x => x.UserId == userId && x.Id == entityId);
        }

        public async Task<bool> CheckAppTemplateComponentsOwnership(Type resourceType,Guid resourceId,Guid clubId)
        {
            if(resourceType == typeof(ApplicationStepDefinition))
            {
                return await _context.ApplicationStepDefinitions
                    .AnyAsync(s => s.Id == resourceId &&
                                   s.Template.ClubId == clubId);
            }

            if (resourceType == typeof(ApplicationSectionDefinition))
            {
                return await _context.ApplicationSectionDefinitions
                    .AnyAsync(sec => sec.Id == resourceId &&
                                     sec.Step.Template.ClubId == clubId);
            }

            if (resourceType == typeof(ApplicationFieldDefinition))
            {
                return await _context.ApplicationFieldDefinitions
                    .AnyAsync(f => f.Id == resourceId &&
                                   f.Section.Step.Template.ClubId == clubId);
            }

            return false;
        }

        public Task<bool> IsUserMemberOfClubAsync(Guid userId, Guid clubId)
        {
            throw new NotImplementedException();
        }
    }

}
