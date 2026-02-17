using EaseClub.Application.Common.Pagination.Parameters;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace EaseClub.Infrastructure.Common.QueryServices
{
    public static class CursorPaginationExtensions
    {
        // Apply cursor-based paging
        public static IQueryable<TEntity> ApplyCursorPaging<TEntity, TKey>(
            this IQueryable<TEntity> query,
            CursorPaginationParameters parameters,
            Expression<Func<TEntity, TKey>> orderSelector)
            where TKey : IComparable<TKey>
        {
            query = parameters.SortDesc
                ? query.OrderByDescending(orderSelector)
                : query.OrderBy(orderSelector);

            if (!string.IsNullOrEmpty(parameters.Cursor))
            {
                var decoded = CursorHelper.Decode<TKey>(parameters.Cursor);

                // Instead of CompareTo, use dynamic expression building or simple checks
                // This is more likely to translate to "WHERE CreatedAt < '2026-01-01'"
                query = parameters.SortDesc
                    ? query.Where(CreateCompareExpression(orderSelector, decoded, true))
                    : query.Where(CreateCompareExpression(orderSelector, decoded, false));
            }

            return query.Take(parameters.Limit + 1); // fetch extra to check for next cursor
        }

        private static Expression<Func<TEntity, bool>> CreateCompareExpression<TEntity, TKey>(
            Expression<Func<TEntity, TKey>> orderSelector,
            TKey decodedValue,
            bool sortDesc)
        {
            // The 'x' in 'x => x.CreatedAt'
            var parameter = orderSelector.Parameters[0];

            // The property 'x.CreatedAt'
            var property = orderSelector.Body;

            // The constant value we are comparing against (the cursor value)
            var constant = Expression.Constant(decodedValue, typeof(TKey));

            // Logic: If sorting descending, we want items LESS THAN the cursor.
            // If sorting ascending, we want items GREATER THAN the cursor.
            BinaryExpression comparison = sortDesc
                ? Expression.LessThan(property, constant)
                : Expression.GreaterThan(property, constant);

            // Returns: x => x.CreatedAt > [Value]
            return Expression.Lambda<Func<TEntity, bool>>(comparison, parameter);
        }

    }
}
