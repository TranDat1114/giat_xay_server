using System.ComponentModel.DataAnnotations.Schema;
using Swashbuckle.AspNetCore.Annotations;

namespace giat_xay_server;

public class Price : Entities
{
    public decimal Value { get; set; }
    public string? Description { get; set; }
    public int? Weight { get; set; }
    public Guid LaundryServiceGuid { get; set; }
    [ForeignKey("LaundryServiceGuid")]
    [SwaggerIgnore]
    public LaundryService LaundryService { get; set; } = null!;

}
