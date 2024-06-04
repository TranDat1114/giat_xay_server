using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace giat_xay_server;

public class User : IdentityUser
{
    public string Address { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? Avatar { get; set; } = "https://fastly.picsum.photos/id/228/600/600.jpg?hmac=TDkN4LVBjPRvjQqMs-TT63NvrvlB-FhcHIilfj8U4xg";
    [NotMapped]
    public string Role { get; set; } = string.Empty;
}
