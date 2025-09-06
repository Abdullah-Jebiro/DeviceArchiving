using DeviceArchiving.Data.Contexts;
using DeviceArchiving.Data.Dto.Operations;
using DeviceArchiving.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace DeviceArchiving.Service.OperationServices;

public class OperationService : IOperationService
{
    private readonly DeviceArchivingContext _context;

    public OperationService(DeviceArchivingContext context)
    {
        _context = context;
    }

    public async Task AddOperations(CreateOperation createOperation)
    {
        if (createOperation == null)
            throw new ArgumentNullException(nameof(createOperation));

        var operation = new Operation
        {
            Comment = createOperation.Comment,
            NewValue = createOperation.NewValue,
            OldValue = createOperation.OldValue,
            OperationName = createOperation.OperationName,
            DeviceId = createOperation.DeviceId,
            CreatedAt = DateTime.Now
        };

        _context.Operations.Add(operation);
        await _context.SaveChangesAsync();
    }

    public Task<List<Operation>> GetAllOperations(int deviceId)
    {
        return _context.Operations
            .Where(o => o.DeviceId == deviceId)
            .Include(o=>o.User)
            .ToListAsync();
    }
}
