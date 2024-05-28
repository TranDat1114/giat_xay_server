namespace giat_xay_server;


public class PaginationService
{
    public (List<T> Items, int Total) GetPagedItems<T>(List<T> items, Pagination pagination)
    {
        // Apply pagination
        items = items.Skip((pagination.Page - 1) * pagination.PageSize).Take(pagination.PageSize).ToList();

        return (items, items.Count);
    }
}
