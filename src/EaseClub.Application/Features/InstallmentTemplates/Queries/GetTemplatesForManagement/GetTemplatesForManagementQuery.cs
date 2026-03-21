using EaseClub.Domain.Common.Results;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Application.Features.InstallmentTemplates.Queries.GetTemplatesForManagement
{
    public record GetTemplatesForManagementQuery(Guid clubId, Guid? planId, bool? isActive):IRequest<Result<List<InstallmentTemplateAdminsDto>>>;

    public class InstallmentTemplateAdminsDto
    {
        public Guid Id { get; set; }
        public Guid ClubId { get; set; }
        public string Name { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public bool isActive { get; set; }
        public int DurationOfPaymentInDays { get; set; }
        public int numOfInstallments { get; set; }
        


    }
}
