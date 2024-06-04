using Swashbuckle.AspNetCore.Annotations;

namespace giat_xay_server;

public class Pagination<T> : Pagination// Add generic type parameter
{
    [SwaggerIgnore]
    public IEnumerable<T>? Data { get; set; } = default!;
    [SwaggerIgnore]
    public int Total { get; set; }
}

public class Pagination
{
    public int? PageIndex { get; set; } = 1;
    public int? PageSize { get; set; } = 10;
    public string? Keyword { get; set; }
}
