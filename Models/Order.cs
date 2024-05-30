using System.ComponentModel.DataAnnotations.Schema;
using Swashbuckle.AspNetCore.Annotations;

namespace giat_xay_server;

public class Order : Entities
{
    [SwaggerIgnore]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int OrderId { get; set; }
    public string? UserName { get; set; }
    public string? Email { get; set; }
    public string PickupAddress { get; set; } = string.Empty;
    [SwaggerIgnore]
    public DateTime? PickupDate { get; set; }
    public string DeliveryAddress { get; set; } = string.Empty;
    [SwaggerIgnore]
    public DateTime? DeliveryDate { get; set; }
    public string PhoneNumber { get; set; } = string.Empty;
    public int? Weight { get; set; }
    public string? Unit { get; set; }
    public decimal? TotalPrice { get; set; }
    public string? Note { get; set; }
    [SwaggerIgnore]
    public string? Status { get; set; }
    public string? Description { get; set; }
    public Guid LaundryServiceTypeGuid { get; set; }
    public Guid LaundryServiceGuid { get; set; }
    [SwaggerIgnore]
    [ForeignKey("LaundryServiceGuid")]
    public LaundryServiceType LaundryServiceType { get; set; } = null!;
}
