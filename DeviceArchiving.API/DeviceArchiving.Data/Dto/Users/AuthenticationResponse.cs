using DeviceArchiving.Data.Entities;
using DeviceArchiving.Data.Enums;

namespace DeviceArchiving.Data.Dto.Users;

public class AuthenticationResponse
{
    public int Id { get; set; } 
    public string Token { get; set; } = null!;
    public string UserName { get; set; } = null!;
    public string Picture { get; set; } = string.Empty;
    public string Role { get; set; } = null!;

    public static AuthenticationResponse FromUser(User user , string token)
    {
        return new AuthenticationResponse
        {
            Id = user.Id,
            Token = token,
            UserName = user.UserName,
            Picture = Convert.ToBase64String(user.Picture ?? new byte[0]),
            Role = user.Role,
        };
    }
}

