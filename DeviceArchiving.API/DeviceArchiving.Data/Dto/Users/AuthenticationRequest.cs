using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeviceArchiving.Data.Dto.Users;

public class AuthenticationRequest
{
    [Required(ErrorMessage = "UserName is required.")]
    public string UserName { get; set; } = null!;
    [Required(ErrorMessage = "Password is required.")]
    public string Password { get; set; } = null!;
    public string Admin { get; set; } = string.Empty;
    public string AdminPassword { get; set; } = string.Empty;





}



