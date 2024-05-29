using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore;

namespace giat_xay_server;

public class PaginationService(ApplicationDbContext context)
{
    private readonly ApplicationDbContext _context = context;

    public async Task<Pagination<T>> GetPaginatedList<T>(Pagination pagination, string[] searchFields) where T : class
    {
        // Apply AsNoTracking() to avoid tracking the returned entities
    IQueryable<T> query = _context.Set<T>().AsNoTracking();

    // If there is a search keyword, apply the Where condition
    if (!string.IsNullOrEmpty(pagination.Keyword))
    {
        var keyword = pagination.Keyword.ToLower();
        var keywordExpression = Expression.Constant($"%{keyword}%");

        // Prepare the parameter expression for the entity type
        var parameter = Expression.Parameter(typeof(T), "x");

        MethodInfo toLowerMethod = typeof(string).GetMethod("ToLower", Type.EmptyTypes);
        MethodInfo likeMethod = typeof(DbFunctionsExtensions).GetMethod("Like", new[] { typeof(DbFunctions), typeof(string), typeof(string) });

        if (toLowerMethod == null || likeMethod == null)
        {
            throw new InvalidOperationException("Required method not found.");
        }

        Expression predicate = null;

        foreach (var field in searchFields)
        {
            // Get the property expression
            var property = Expression.Property(parameter, field);

            // Ensure the property is of type string
            if (property.Type != typeof(string))
            {
                continue; // Skip non-string properties
            }

            // Call ToLower on the property
            var toLowerCall = Expression.Call(property, toLowerMethod);

            // Call EF.Functions.Like
            var likeCall = Expression.Call(
                likeMethod,
                Expression.Constant(EF.Functions),
                toLowerCall,
                keywordExpression);

            if (predicate == null)
            {
                predicate = likeCall;
            }
            else
            {
                predicate = Expression.OrElse(predicate, likeCall);
            }
        }

        // If no valid search fields, throw an exception
        if (predicate == null)
        {
            throw new InvalidOperationException("No valid search fields found.");
        }

        var lambda = Expression.Lambda<Func<T, bool>>(predicate, parameter);
        query = query.Where(lambda);
        }

        // Áp dụng phân trang
        var data = await query
            .Skip((pagination.Page ?? 1 - 1) * (pagination.PageSize ?? 10))
            .Take(pagination.PageSize ?? 10)
            .ToListAsync();
        return new Pagination<T>
        {
            Result = data,
            Total = await query.CountAsync()
        };
    }
}
