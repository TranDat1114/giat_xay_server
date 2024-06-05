using System.ComponentModel.DataAnnotations.Schema;
using Swashbuckle.AspNetCore.Annotations;

namespace giat_xay_server;

public class LaundryServiceType : Entities
{
    public decimal Price { get; set; }
    public int UnitValue { get; set; }
    public int ConditionValue { get; set; }
    public ConditionTypes ConditionType { get; set; }
    public UnitTypes UnitType { get; set; }
    public string? Description { get; set; }
    public Guid LaundryServiceGuid { get; set; }
    [ForeignKey("LaundryServiceGuid")]
    [SwaggerIgnore]
    [NotMapped]
    public LaundryService LaundryService { get; set; } = null!;

}
