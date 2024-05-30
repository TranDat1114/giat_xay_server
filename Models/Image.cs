using System.ComponentModel.DataAnnotations;
using Swashbuckle.AspNetCore.Annotations;

namespace giat_xay_server;

public class Image
{
    [Key]
    [SwaggerIgnore]
    public Guid ImageGuid { get; set; }
    [SwaggerIgnore]
    public string Url { get; set; } = string.Empty;
    public string? Name { get; set; }
    public string GroupType { get; set; } = string.Empty;
}
