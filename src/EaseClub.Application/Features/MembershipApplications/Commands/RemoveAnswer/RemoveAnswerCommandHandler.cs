//using EaseClub.Application.Common.Interfaces;
//using EaseClub.Domain.Common;
//using EaseClub.Domain.Common.Results;
//using EaseClub.Domain.MembershipApplications.Repositories;
//using MediatR;
//using Microsoft.Extensions.Logging;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace EaseClub.Application.Features.MembershipApplications.Commands.RemoveAnswer
//{
//    public class RemoveAnswerCommandHandler(
//    IMembershipApplicationRepository repository,
//    ILogger<RemoveAnswerCommandHandler> logger,
//    IUnitOfWork unitOfWork) : IRequestHandler<RemoveAnswerCommand, Result<Success>>
//    {
//        public async Task<Result<Success>> Handle(RemoveAnswerCommand request, CancellationToken ct)
//        {
//            // Load the Application with its Answers
//            var application = await repository.GetByIdWithAnswersAsync(request.ApplicationId, ct);

//            if (application == null)
//            {
//                logger.LogWarning("Application not found.");
//                return Error.NotFound("Application.NotFound", "Application not found.");
//            }


//            // The Domain method handles the logic and status checks
//            var result = application.RemoveAnswer(request.FieldDefinitionId, request.InstanceIndex);

//            if (result.IsError) {
//                logger.LogError("Failed to remove answer from application with id:{ApplicationId}, reason:{Error}",
//                    request.ApplicationId, result.TopError);
//                return result.TopError;
//             }

//            await unitOfWork.SaveChangesAsync(ct);
//            return Result.Success;
//        }
//    }
//}
