using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Application.Features.InstallmentTemplates.Queries.GetTemplateById
{
    public class InstallmentTemplateResponse
    {
        public InstallmentTemplateResponse(Guid id, string name, List<InstallmentDto> installments)
        {
            Id = id;
            Name = name;
            Installments = installments;
        }
        public Guid Id { get; init; }
        public string Name { get; init; }
        public List<InstallmentDto> Installments { get; init; } = new List<InstallmentDto>();
    }
    public class InstallmentDto
    {
        public InstallmentDto(int order, decimal percentage,int dueAfterDays)
        {
            Order = order;
            Percentage = percentage;
            DueAfterDays = dueAfterDays;
        }

        public int Order { get; set; }
        public decimal Percentage { get; set; }
        public int DueAfterDays { get; set; }

    }
}
