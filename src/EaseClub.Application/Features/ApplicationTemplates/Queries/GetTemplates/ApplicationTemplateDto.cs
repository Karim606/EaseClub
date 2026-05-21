using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Application.Features.ApplicationTemplates.Queries.GetTemplates
{
    public sealed class ApplicationTemplateDto
    {
        public ApplicationTemplateDto(Guid id,string name,bool isActive,int numberOfSteps) {
            
            Id = id;
            Name = name;
            IsActive = isActive;
            NumberOfSteps = numberOfSteps;

        }
        public Guid Id { get; init; }
        public string Name { get; init; }
        public bool IsActive {  get; init; }

        public int NumberOfSteps { get; init; }

    }
}
