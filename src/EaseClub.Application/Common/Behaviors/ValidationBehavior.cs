using EaseClub.Domain.Common.Interfaces;
using FluentValidation;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EaseClub.Domain.Common;

namespace EaseClub.Application.Common.Behaviors
{
    internal sealed class ValidationBehavior<TRequest,TResponse>(IValidator<TRequest>? validator=null):IPipelineBehavior<TRequest,TResponse>
        where TRequest : IRequest<TResponse>
        where TResponse: IResult
    {
        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            if (validator is null)
            {
                return await next();
            }
            
            var validationResult = await validator.ValidateAsync(request, cancellationToken);
            if (validationResult.IsValid)
            {
                return await next();
            }

            var errors = validationResult.Errors
                .Where(failure => failure != null)
                .Select(failure => Error.Validation(failure.PropertyName, failure.ErrorMessage))
                .ToList();

            return (dynamic)errors;
        }
    }
    
}
