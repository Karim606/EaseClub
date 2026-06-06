using EaseClub.Application.Common.Pagination;
using EaseClub.Application.Common.Pagination.Parameters;
using EaseClub.Application.Common.Pagination.Results;
using EaseClub.Domain.Common;
using EaseClub.Domain.Common.Results;
using EaseClub.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;

namespace EaseClub.Infrastructure.Common.QueryServices;

public abstract class BaseQueryService<TEntity> where TEntity : class
{
    protected readonly AppDbContext _context;
    protected readonly ILogger _logger;

    protected BaseQueryService(AppDbContext context, ILogger logger)
    {
        _context = context;
        _logger  = logger;
    }

    protected IQueryable<TEntity> Query() => _context.Set<TEntity>().AsNoTracking();

    protected async Task<Result<TPaginatedResult>> GetPaginatedAsync<TDto, TKey, TPaginatedResult>(
        IQueryable<TEntity> query,
        PaginationParameters parameters,
        Expression<Func<TEntity, TDto>> selector,
        Expression<Func<TEntity, TKey>> orderSelector,
        CancellationToken cancellationToken)
        where TKey : IComparable<TKey>
        where TPaginatedResult : PaginatedResult<TDto>, new()
        //The new() constraint tells the compiler:
        //"The type passed into this generic must have a public,
        //parameterless constructor."
        //you guarantee that you can instantiate that type on the fly.
    {
        try
        {
            PaginatedResult<TDto> result = parameters switch
            {
                CursorPaginationParameters cursor => await ExecuteCursorQuery(query, cursor, selector, orderSelector, cancellationToken),
                OffsetPaginationParameters offset => await ExecuteOffsetQuery(query, offset, selector, cancellationToken),
                _ => throw new InvalidOperationException("Unsupported pagination type")
            };

            // Using implicit operator: TPaginatedResult -> Result<TPaginatedResult>
            return (TPaginatedResult)result;
        }
        catch (FormatException) // Handle invalid Base64 cursor strings
        {
            _logger.LogError("Pagination.InvalidCursor,The provided cursor is not in a valid format.");
            return Error.Validation("Pagination.InvalidCursor", "The provided cursor is not in a valid format.");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Pagination.Error, Error: {ex.Message}");
            return Error.Unexpected("Pagination.Error", $"An unexpected error occurred during pagination: {ex.Message}");
        }
    }

    private async Task<CursorPaginatedResult<TDto>> ExecuteCursorQuery<TDto, TKey>(
        IQueryable<TEntity> query,
        CursorPaginationParameters parameters,
        Expression<Func<TEntity, TDto>> selector,
        Expression<Func<TEntity, TKey>> orderSelector,
        CancellationToken cancellationToken)
        where TKey : IComparable<TKey>
    {
        // 1. Combine selector and orderSelector for projection in DB
        var parameter = Expression.Parameter(typeof(TEntity), "e");
        var selectorBody = new ParameterReplacer(selector.Parameters[0], parameter).Visit(selector.Body);
        var orderBody = new ParameterReplacer(orderSelector.Parameters[0], parameter).Visit(orderSelector.Body);

        var memberInit = Expression.MemberInit(
            Expression.New(typeof(ProjectedItem<TDto, TKey>)),
            Expression.Bind(typeof(ProjectedItem<TDto, TKey>).GetProperty(nameof(ProjectedItem<TDto, TKey>.Dto))!, selectorBody),
            Expression.Bind(typeof(ProjectedItem<TDto, TKey>).GetProperty(nameof(ProjectedItem<TDto, TKey>.Key))!, orderBody)
        );

        var combinedSelector = Expression.Lambda<Func<TEntity, ProjectedItem<TDto, TKey>>>(memberInit, parameter);

        // 2. Execute query with projection using existing cursor extension
        var results = await query.ApplyCursorPaging(parameters, orderSelector)
            .Select(combinedSelector)
            .ToListAsync(cancellationToken);

        string? nextCursor = null;
        bool hasMore = results.Count > parameters.Limit;

        if (hasMore)
        {
            var lastItem = results[parameters.Limit];
            results.RemoveAt(parameters.Limit);
            nextCursor = CursorHelper.Encode(lastItem.Key);
        }

        return new CursorPaginatedResult<TDto>
        {
            Items = results.Select(r => r.Dto).ToList(),
            NextCursor = nextCursor,
            HasMore = hasMore
        };
    }

    private async Task<OffsetPaginatedResult<TDto>> ExecuteOffsetQuery<TDto>(
        IQueryable<TEntity> query,
        OffsetPaginationParameters parameters,
        Expression<Func<TEntity, TDto>> selector,
        CancellationToken cancellationToken)
    {
        var totalCount = await query.CountAsync(cancellationToken);

        var sortBy = parameters.SortBy ?? "Id";
        query = parameters.SortDesc
            ? query.OrderByDescending(e => EF.Property<object>(e, sortBy))
            : query.OrderBy(e => EF.Property<object>(e, sortBy));

        var items = await query
            .Skip(parameters.CalculatedOffset())
            .Take(parameters.Limit)
            .Select(selector)
            .ToListAsync(cancellationToken);

        return new OffsetPaginatedResult<TDto>
        {
            Items = items,
            Page = parameters.Page,
            TotalCount = totalCount,
           // TotalPages = (int)Math.Ceiling(totalCount / (double)parameters.Limit),
            HasMore = parameters.Page * parameters.Limit < totalCount
        };
    }


    protected async Task<Result<UnifiedPaginatedResponse<TDto>>> GetUnifiedPaginatedAsync<TDto, TKey>(
       IQueryable<TEntity> query,
       PaginationRequest paginationRequest,
       Expression<Func<TEntity, TDto>> selector,
       Expression<Func<TEntity, TKey>> orderSelector,
       CancellationToken cancellationToken)
       where TKey : IComparable<TKey>

    {
        try
        {
            var parameters = paginationRequest.ToParameters();
            PaginatedResult<TDto> result = parameters switch
            {
                CursorPaginationParameters cursor => await ExecuteCursorQuery(query, cursor, selector, orderSelector, cancellationToken),
                OffsetPaginationParameters offset => await ExecuteOffsetQuery(query, offset, selector, cancellationToken),
                _ => throw new InvalidOperationException("Unsupported pagination type")
            };

            return MapToUnified(result);
        }
        catch (FormatException) // Handle invalid Base64 cursor strings
        {
            _logger.LogError("Pagination.InvalidCursor,The provided cursor is not in a valid format.");
            return Error.Validation("Pagination.InvalidCursor", "The provided cursor is not in a valid format.");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Pagination.Error, Error: {ex.Message}");
            return Error.Unexpected("Pagination.Error", $"An unexpected error occurred during pagination: {ex.Message}");
        }
    }

    private UnifiedPaginatedResponse<TDto> MapToUnified<TDto>(PaginatedResult<TDto> result)
    {
        // Pattern matching (C# 9.0+) is the cleanest way to do this
        return result switch
        {
            OffsetPaginatedResult<TDto> offset => new UnifiedPaginatedResponse<TDto>(
                offset.Items,
                offset.HasMore,
                Page: offset.Page,
                TotalCount: offset.TotalCount,
                NextCursor: null),

            CursorPaginatedResult<TDto> cursor => new UnifiedPaginatedResponse<TDto>(
                cursor.Items,
                cursor.HasMore,
                Page: null,
                TotalCount: null,
                NextCursor: cursor.NextCursor),

            _ => new UnifiedPaginatedResponse<TDto>(result.Items, result.HasMore)
        };
    }

    // Helper classes for database-side projection
    private class ProjectedItem<TDto, TKey>
    {
        public TDto Dto { get; set; } = default!;
        public TKey Key { get; set; } = default!;
    }

    private class ParameterReplacer(ParameterExpression oldParameter, ParameterExpression newParameter) : ExpressionVisitor
    {
        protected override Expression VisitParameter(ParameterExpression node)
            => node == oldParameter ? newParameter : base.VisitParameter(node);
    }
}