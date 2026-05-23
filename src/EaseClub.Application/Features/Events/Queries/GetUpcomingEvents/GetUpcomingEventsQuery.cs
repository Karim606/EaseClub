using EaseClub.Application.Features.Events.Dtos;
using EaseClub.Domain.Common.Results;
using MediatR;
using System;
using System.Collections.Generic;

namespace EaseClub.Application.Features.Events.Queries.GetUpcomingEvents;

public record GetUpcomingEventsQuery(Guid ClubId) : IRequest<Result<List<EventSummaryDto>>>;
