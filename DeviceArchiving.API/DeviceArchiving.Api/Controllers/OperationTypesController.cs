using DeviceArchiving.Data.Entities;
using DeviceArchiving.Service.OperationTypeServices;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace DeviceArchiving.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class OperationTypesController : ControllerBase
{
    private readonly IOperationTypeService operationTypeService;

    public OperationTypesController(IOperationTypeService operationTypeService)
    {
        this.operationTypeService = operationTypeService;
    }

    [HttpGet]
    public async Task<ActionResult<List<OperationType>>> GetOperationTypes([FromQuery] string? searchTerm = null)
    {
        var operationTypes = await operationTypeService.GetAllOperationsTypes(searchTerm);
        return Ok(operationTypes);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<OperationType>> GetOperationType(int id)
    {
        var operationTypes = await operationTypeService.GetAllOperationsTypes(null);
        var operationType = operationTypes.FirstOrDefault(ot => ot.Id == id);
        if (operationType == null) return NotFound();
        return Ok(operationType);
    }

    [HttpPost]
    public ActionResult<OperationType> PostOperationType(OperationType operationType)
    {
        operationTypeService.AddOperationType(operationType);
        return CreatedAtAction(nameof(GetOperationType), new { id = operationType.Id }, operationType);
    }

    [HttpPut("{id}")]
    public IActionResult PutOperationType(int id, OperationType operationType)
    {
        if (id != operationType.Id) return BadRequest();

        try
        {
            operationTypeService.UpdateOperationType(operationType);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        return NoContent();
    }

    [HttpDelete("{id}")]
    public IActionResult DeleteOperationType(int id)
    {
        try
        {
            operationTypeService.DeleteOperationType(id);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        return NoContent();
    }
}