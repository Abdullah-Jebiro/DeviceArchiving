using DeviceArchiving.Data.Dto.Operations;
using DeviceArchiving.Data.Entities;

namespace DeviceArchiving.Service.OperationServices;

public interface IOperationService
{
    Task AddOperations(CreateOperation operation);
    Task<List<Operation>> GetAllOperations(int driveId);

}
