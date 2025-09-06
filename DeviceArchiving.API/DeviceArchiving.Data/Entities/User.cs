using DeviceArchiving.Data.Enums;
using System.ComponentModel.DataAnnotations;
namespace DeviceArchiving.Data.Entities;


public class User
{
    public int Id { get; set; }
    public string UserName { get; set; } = null!;
    public string Password { get; set; } = null!;
    public byte[]? Picture { get; set; } = null;
    public string Role { get; set; } = UserRole.User; // Change to string
}
