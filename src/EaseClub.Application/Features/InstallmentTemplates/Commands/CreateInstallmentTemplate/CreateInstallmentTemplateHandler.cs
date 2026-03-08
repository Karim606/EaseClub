using EaseClub.Application.Common.Interfaces;
using EaseClub.Domain.Common.Results;
using EaseClub.Domain.MembershipPlans;
using EaseClub.Domain.MembershipPlans.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Application.Features.InstallmentTemplates.Commands.CreateInstallmentTemplate
{
    public class CreateInstallmentTemplateHandler(IInstallmentsTemplatesRepository repository,
        IUnitOfWork unitOfWork,ILogger<CreateInstallmentTemplateHandler>logger)
        : IRequestHandler<CreateInstallmentTemplateCommand, Result<Guid>>
    {
       

        public async Task<Result<Guid>> Handle(CreateInstallmentTemplateCommand request, CancellationToken cancellationToken)
        {
            List<Installment>? installments=null;

            if (request.Installments != null)
            {
                installments = new List<Installment>();
                foreach (var installment in request.Installments)
                {
                    var res = Installment.Create(installment.Percentage, installment.DueAfterDays, installment.Order);
                    if (res.IsError) return res.TopError;
                    installments.Add(res.Value);
                }
            }

                var templateResult = InstallmentTemplate.Create(Guid.NewGuid(), request.ClubId, request.Name, request.NumOfInstallments,
                    request.DurationInDays, installments);

            if (templateResult.IsError) return templateResult.TopError;

            await repository.AddAsync(templateResult.Value);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            return templateResult.Value.Id;
        }
    }
}
