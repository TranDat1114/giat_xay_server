using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore;

namespace giat_xay_server;

public class PaginationService()
{
    public async Task<Pagination<T>> GetPaginatedList<T>(Pagination pagination, IQueryable<T> query) where T : Entities 
    {
        if (pagination.PageIndex < 1 || pagination.PageSize < 1)
        {
            throw new ArgumentException("PageIndex and PageSize must be greater than 0");
        }
        if (pagination.PageIndex == 0)
        {
            pagination.PageIndex = 1;
        }
        if (pagination.PageSize == 0)
        {
            pagination.PageSize = 10;
        }

        int PageIndex = pagination.PageIndex ?? 1;
        int PageSize = pagination.PageSize ?? 10;

        // Apply AsNoTracking() to avoid tracking the returned entities
        query = query.AsNoTracking();

        // Áp dụng phân trang
        var data = await query
            .Skip((PageIndex - 1) * PageSize)
            .Take(PageSize)
            .OrderBy(x => x.CreatedAt)
            .ToArrayAsync();
        return new Pagination<T>
        {
            Data = data,
            Total = await query.CountAsync(),
            PageIndex = PageIndex,
            PageSize = PageSize
        };
    }


}
