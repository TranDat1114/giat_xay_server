using Swashbuckle.AspNetCore.Annotations;

namespace giat_xay_server;

public class LaundryService : Entities
{
    public string? Description { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? BlogUrl { get; set; }
    [SwaggerIgnore]
    public List<Price> Prices { get; set; } = [];

}
