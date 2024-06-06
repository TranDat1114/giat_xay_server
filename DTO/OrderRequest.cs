using Swashbuckle.AspNetCore.Annotations;

namespace giat_xay_server;

public class OrderRequest
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
    public string? Status { get; set; }

}
