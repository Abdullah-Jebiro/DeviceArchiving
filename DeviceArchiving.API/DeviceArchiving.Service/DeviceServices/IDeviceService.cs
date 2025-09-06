using DeviceArchiving.Data.Dto;
using DeviceArchiving.Data.Dto.Devices;
using System.Threading.Tasks;

namespace DeviceArchiving.Service.DeviceServices;

public interface IDeviceService
{
    Task<BaseResponse<string>> AddDeviceAsync(CreateDeviceDto dto);
    Task<List<GetAllDevicesDto>> GetAllDevicesAsync();
    Task<GetDeviceDto?> GetDeviceByIdAsync(int id);
    Task<BaseResponse<string>> UpdateDeviceAsync(int id, UpdateDeviceDto dto);
    Task DeleteDeviceAsync(int id);
    Task<BaseResponse<DuplicateCheckResponse>> CheckDuplicatesInDatabaseAsync(List<CheckDuplicateDto> items);
    Task<BaseResponse<int>> ProcessDevicesAsync(List<DeviceUploadDto> dtos);
    Task<GetDeviceDto?> GetInactiveDeviceBySerialOrLaptopAsync(string? serialNumber, string? laptopName);
    Task<BaseResponse<string>> RestoreDeviceAsync(int id); 
}


