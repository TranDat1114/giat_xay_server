using System.ComponentModel.DataAnnotations.Schema;
using Swashbuckle.AspNetCore.Annotations;

namespace giat_xay_server;

public class Order : Entities
{
    public string? UserName { get; set; }
    public string? Email { get; set; }
    public string Address { get; set; } = string.Empty;
    public DateTime DeliveryDate { get; set; }
    public string PhoneNumber { get; set; } = string.Empty;
    public int? Value { get; set; }
    public string? Unit { get; set; }
    [SwaggerIgnore]
    public decimal TotalPrice { get; set; }
    public string? Note { get; set; }
    [SwaggerIgnore]
    public string? Status { get; set; }
    public Guid LaundryServiceTypeGuid { get; set; }
    public Guid LaundryServiceGuid { get; set; }
    [NotMapped]
    [SwaggerIgnore]
    public string? LaundryServiceTypeDescription { get; set; } = string.Empty;
    [NotMapped]
    [SwaggerIgnore]
    public string? LaundryServiceName { get; set; } = string.Empty;
    // public Guid? PaymentMethodGuid { get; set; }

}
