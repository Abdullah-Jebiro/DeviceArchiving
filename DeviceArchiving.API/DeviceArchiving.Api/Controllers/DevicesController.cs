using Azure;
using ClosedXML.Excel;
using DeviceArchiving.Data.Dto;
using DeviceArchiving.Data.Dto.Devices;
using DeviceArchiving.Data.Entities;
using DeviceArchiving.Data.Enums;
using DeviceArchiving.Service.DeviceServices;
using DocumentFormat.OpenXml.InkML;
using DocumentFormat.OpenXml.Office2010.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace DeviceArchiving.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DevicesController : ControllerBase
{
    private readonly IDeviceService _deviceService;

    public DevicesController(IDeviceService deviceService)
    {
        _deviceService = deviceService;
    }

    [HttpGet]
    public async Task<ActionResult<List<GetAllDevicesDto>>> GetAll()
    {
        var devices = await _deviceService.GetAllDevicesAsync();
        return Ok(devices);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<GetDeviceDto>> GetById(int id)
    {
        var device = await _deviceService.GetDeviceByIdAsync(id);
        if (device == null)
            return NotFound();

        return Ok(device);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateDeviceDto dto)
    {
        var response = await _deviceService.AddDeviceAsync(dto);
        return response.Success ? Ok(response) : BadRequest(response);

    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateDeviceDto dto)
    {
        var response = await _deviceService.UpdateDeviceAsync(id, dto);
        return response.Success ? Ok(response) : BadRequest(response);

    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            await _deviceService.DeleteDeviceAsync(id);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (Exception)
        {
            return StatusCode(500, "Internal server error");
        }
    }



    [HttpPost("upload-devices")]
    [Authorize(Roles = UserRole.Admin)]
    [ProducesResponseType(typeof(BaseResponse<int>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse<int>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UploadDevices([FromBody] List<DeviceUploadDto> devices)
    {
        // Check if the devices list is null or empty
        if (devices == null || !devices.Any())
        {
            return BadRequest(BaseResponse<int>.Failure("·„ Ì „ ≈—”«· √Ì √ÃÂ“…"));
        }

        // Optionally, check model state for validation issues
        if (!ModelState.IsValid)
        {
            return BadRequest(BaseResponse<int>.Failure("«·»Ì«‰«  «·„œŒ·… €Ì— ’ÕÌÕ…"));
        }

        // Process devices
        var response = await _deviceService.ProcessDevicesAsync(devices);
        return response.Success ? Ok(response) : BadRequest(response);
    }


    [HttpPost("check-duplicates")]
    [ProducesResponseType(typeof(BaseResponse<DuplicateCheckResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse<DuplicateCheckResponse>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CheckDuplicates([FromBody] List<CheckDuplicateDto> items)
    {
        if (items == null || !items.Any())
            return BadRequest(BaseResponse<DuplicateCheckResponse>.Failure("·„ Ì „ ≈—”«· »Ì«‰«  ·· Õﬁﬁ"));

        var response = await _deviceService.CheckDuplicatesInDatabaseAsync(items);
        return Ok(response);
    }

    [HttpPost("restore-device")]
    [ProducesResponseType(typeof(BaseResponse<DuplicateCheckResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse<DuplicateCheckResponse>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RestoreDevice(int id)
    {

        var response = await _deviceService.RestoreDeviceAsync(id);
        return Ok(response);
    }



}
