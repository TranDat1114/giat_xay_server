using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore;

namespace giat_xay_server;

public class PaginationService()
{
    public async Task<Pagination<T>> GetPaginatedList<T>(Pagination pagination, IQueryable<T> query) where T : class
    {
        // Apply AsNoTracking() to avoid tracking the returned entities
        query = query.AsNoTracking();

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
