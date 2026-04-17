using EaseClub.Domain.Common;
using EaseClub.Domain.Common.Results;
using EaseClub.Domain.MembershipApplications.Repositories;
using EaseClub.Domain.PricingPolices;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Application.Features.MembershipApplications.Queries.GetPricingForApplication
{
    public class GetApplicationPricingHandler(
    IMembershipApplicationRepository repo
) : IRequestHandler<GetApplicationPricingQuery, Result<Pricing>>
    {
        public async Task<Result<Pricing>> Handle(GetApplicationPricingQuery request, CancellationToken ct)
        {
            var app = await repo.GetByIdAsync(request.ApplicationId, ct);
            if (app == null)
                return Error.NotFound("Application.NotFound");

            var pricingResult = app.GetPricePreview();
            if (pricingResult.IsError) return pricingResult.TopError;
            
            var installments = app.GetPaymentSchedule();
            if(installments.IsError) return installments.TopError;

            return new Pricing(pricingResult.Value, installments.Value);

        }
    }
}
