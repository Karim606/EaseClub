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
    private readonly ILogger _logger;

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
        var pagedQuery = query.ApplyCursorPaging(parameters, orderSelector);
        var entities = await pagedQuery.ToListAsync(cancellationToken);

        string? nextCursor = null;
        bool hasMore = entities.Count > parameters.Limit;

        if (hasMore)
        {
            var lastEntity = entities[parameters.Limit];
            entities.RemoveAt(parameters.Limit);

            // Using Compile() here for a single object is safe
            //we use it to turn  expression into delegate in memory then use its key k=> k.CreatedAt
            var cursorValue = orderSelector.Compile()(lastEntity);
            nextCursor = CursorHelper.Encode(cursorValue);
        }

        return new CursorPaginatedResult<TDto>
        {
            Items = entities.AsQueryable().Select(selector).ToList(),
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
            TotalPages = (int)Math.Ceiling(totalCount / (double)parameters.Limit),
            HasMore = parameters.Page * parameters.Limit < totalCount
        };
    }
}