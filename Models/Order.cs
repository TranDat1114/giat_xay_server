using System.ComponentModel.DataAnnotations.Schema;
using Swashbuckle.AspNetCore.Annotations;

namespace giat_xay_server;

public class Order : Entities
{
    [SwaggerIgnore]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int OrderId { get; set; }
    public string? Name { get; set; }
    public string PickupAddress { get; set; } = string.Empty;
    public string DeliveryAddress { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string? Note { get; set; }
    [SwaggerIgnore]
    public string? Status { get; set; }
    public Guid LaundryServiceGuid { get; set; }
    [ForeignKey("LaundryServiceGuid")]
    [SwaggerIgnore]
    public LaundryService LaundryService { get; set; } = null!;

}
