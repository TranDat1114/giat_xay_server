using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Swashbuckle.AspNetCore.Annotations;

namespace giat_xay_server;

public class Entities : IEntities
{
    [SwaggerIgnore]
    [Key]
    public Guid Guid { get; set; }
    [SwaggerIgnore]
    public DateTime CreatedAt { get; set; }
    [SwaggerIgnore]
    public DateTime UpdatedAt { get; set; }
    [SwaggerIgnore]
    public bool IsDeleted { get; set; }
    [SwaggerIgnore]
    public string? CreatedBy { get; set; }
    [SwaggerIgnore]
    public string? UpdatedBy { get; set; }

}
