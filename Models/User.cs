using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace giat_xay_server;

public class User : IdentityUser
{
    [NotMapped]
    public string Role { get; set; } = string.Empty;

}
