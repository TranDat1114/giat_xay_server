using Swashbuckle.AspNetCore.Annotations;

namespace giat_xay_server;

public class LaundryService : Entities
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
}

