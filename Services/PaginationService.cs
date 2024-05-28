using Microsoft.EntityFrameworkCore;

namespace giat_xay_server;

public class PaginationService(ApplicationDbContext context)
{
    private readonly ApplicationDbContext _context = context;

    public async Task<Pagination<T>> GetPaginatedList<T>(Pagination pagination, string[] searchFields) where T : class
    {
        // Áp dụng AsNoTracking() để không theo dõi các thực thể trả về
        IQueryable<T> query = _context.Set<T>().AsNoTracking();

        // Nếu có từ khóa tìm kiếm, áp dụng điều kiện Where
        if (!string.IsNullOrEmpty(pagination.Keyword))
        {
            query = query.Where(x => searchFields.Any(f => EF.Property<string>(x, f).Contains(pagination.Keyword)));
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
