using Azure;
using ClosedXML.Excel;
using DeviceArchiving.Data.Dto;
using DeviceArchiving.Data.Dto.Devices;
using DeviceArchiving.Data.Entities;
using DeviceArchiving.Service;
using DocumentFormat.OpenXml.InkML;
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
    [ProducesResponseType(typeof(BaseResponse<int>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse<int>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UploadDevices([FromBody] List<DeviceUploadDto> devices)
    {
        if (devices == null || !devices.Any())
            return BadRequest(BaseResponse<int>.Failure("�� ��� ����� �� �����"));

        var response = await _deviceService.ProcessDevicesAsync(devices);
        return response.Success ? Ok(response) : BadRequest(response);
    }



    [HttpPost("check-duplicates")]
    [ProducesResponseType(typeof(BaseResponse<DuplicateCheckResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse<DuplicateCheckResponse>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CheckDuplicates([FromBody] List<CheckDuplicateDto> items)
    {
        if (items == null || !items.Any())
            return BadRequest(BaseResponse<DuplicateCheckResponse>.Failure("�� ��� ����� ������ ������"));

        var response = await _deviceService.CheckDuplicatesAsync(items);
        return Ok(response);
    }


}
