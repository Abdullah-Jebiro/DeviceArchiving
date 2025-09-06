namespace DeviceArchiving.Data.Dto.Devices;

public class GetAllDevicesDto
{
    public int Id { get; set; }
    public string Source { get; set; } = null!;
    public string BrotherName { get; set; } = null!;
    public string LaptopName { get; set; } = null!;
    public string SystemPassword { get; set; } = null!;
    public string WindowsPassword { get; set; } = null!;
    public string HardDrivePassword { get; set; } = null!;
    public string FreezePassword { get; set; } = null!;
    public string Code { get; set; } = null!;
    public string Type { get; set; } = null!;
    public string SerialNumber { get; set; } = null!;
    public string? Comment { get; set; }
    public string? ContactNumber { get; set; }
    public string Card { get; set; } = null!;
    public string UserName { get; set; } = null!;
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public bool IsActive { get; set; } 
}
