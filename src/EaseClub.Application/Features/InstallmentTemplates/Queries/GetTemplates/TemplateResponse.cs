using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Application.Features.InstallmentTemplates.Queries.GetTemplates
{
   public class TemplatesResponse
    {
        public TemplatesResponse(Guid id, string name, int numOfInstallments, int duration) {
            Name = name;
            Id = id;
            NumOfInstallments = numOfInstallments;
            Duration = duration;
        }
        public string Name { get; set; }
        public Guid Id { get; set; }
        public int NumOfInstallments{ get; set; }
        public int Duration { get; set; }
    }
}
