﻿using DeviceArchiving.Data.Entities;
using DeviceArchiving.Service;
using Microsoft.AspNetCore.Mvc;

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
    public ActionResult<IEnumerable<OperationType>> GetOperationTypes([FromQuery] string? searchTerm = null)
    {
        var operationTypes = operationTypeService.GetAllOperationsTypes(searchTerm);
        return Ok(operationTypes);
    }

    [HttpGet("{id}")]
    public ActionResult<OperationType> GetOperationType(int id)
    {
        var operationType = operationTypeService.GetAllOperationsTypes(null).FirstOrDefault(ot => ot.Id == id);
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