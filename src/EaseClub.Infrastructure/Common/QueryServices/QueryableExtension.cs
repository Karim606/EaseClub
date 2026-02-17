using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Infrastructure.Common.QueryServices
{


    public static class QueryableExtensions
    {
        public static IQueryable<TEntity> ApplySearch<TEntity>(
            this IQueryable<TEntity> query,
            string? search,
            Expression<Func<TEntity, string>> searchSelector)
        {
            if (string.IsNullOrWhiteSpace(search)) return query;

            // 1. Prepare the search term
            var searchTerm = $"%{search}%";

            // 2. Get the 'Like' method info from EF Core
            var likeMethod = typeof(DbFunctionsExtensions).GetMethod("Like",
                new[] { typeof(DbFunctions), typeof(string), typeof(string) })
                ?? throw new InvalidOperationException("EF.Functions.Like not found.");

            // 3. Build the call: EF.Functions.Like(searchSelector.Body, searchTerm)
            var methodCall = Expression.Call(
                null,
                likeMethod,
                Expression.Constant(EF.Functions),
                searchSelector.Body,
                Expression.Constant(searchTerm));

            // 4. Create the final lambda: e => EF.Functions.Like(e.Property, "%search%")
            var lambda = Expression.Lambda<Func<TEntity, bool>>(methodCall, searchSelector.Parameters);

            return query.Where(lambda);
        }
    }
}
