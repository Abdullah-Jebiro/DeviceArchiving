using System.ComponentModel.DataAnnotations.Schema;

namespace DeviceArchiving.Data.Dto.Devices;


public class CheckDuplicateDto
{
    public string? Source { get; set; }
    public string? BrotherName { get; set; }
    public string LaptopName { get; set; } = null!;
    public string? SystemPassword { get; set; } = null!;
    public string? WindowsPassword { get; set; } = null!;
    public string? HardDrivePassword { get; set; } = null!;
    public string? FreezePassword { get; set; }
    public string? Code { get; set; }
    public string? Type { get; set; }
    public string SerialNumber { get; set; } = null!;
    public string? Card { get; set; }
    public string? Comment { get; set; }
    public string? ContactNumber { get; set; }
    public DateTime? CreatedAt { get; set; }
    public bool IsSelected { get; set; }

}

public class DuplicateCheckResponse
{
    public List<string> DuplicateSerialNumbers { get; set; } = new List<string>();
    public List<string> DuplicateLaptopNames { get; set; } = new List<string>();
}

public class DeviceUploadDto : CreateDeviceDto
{
    public DateTime CreatedAt { get; set; }

}