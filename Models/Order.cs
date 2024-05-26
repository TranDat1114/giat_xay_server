﻿using System.ComponentModel.DataAnnotations.Schema;
using Swashbuckle.AspNetCore.Annotations;

namespace giat_xay_server;

public class Order : Entities
{
    public int OrderId { get; set; }
    public string UserAddress { get; set; } = string.Empty;
    public string? Note { get; set; }
    public string? Status { get; set; }
    public Guid LaundryServiceGuid { get; set; }
    [ForeignKey("LaundryServiceGuid")]
    [SwaggerIgnore]
    public LaundryService LaundryService { get; set; } = null!;

}
