using DeviceArchiving.Data.Entities;

namespace DeviceArchiving.Service.OperationTypeServices;
public interface IOperationTypeService
{
    Task AddOperationType(OperationType operationType);
    Task<IEnumerable<OperationType>> GetAllOperationsTypes(string? searchTerm);
    Task UpdateOperationType(OperationType operationType);
    Task DeleteOperationType(int id);

}
