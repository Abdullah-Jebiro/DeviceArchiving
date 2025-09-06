using DeviceArchiving.Data.Dto.Operations;
using DeviceArchiving.Data.Entities;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace DeviceArchiving.Service.OperationServices;

public class OperationProcedureService : IOperationService
{
    private readonly DataAccessLayer _dataAccessLayer;

    public OperationProcedureService(IConfiguration configuration)
    {
        string connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new ArgumentNullException(nameof(configuration), "سلسلة الاتصال 'DefaultConnection' غير موجودة.");
        string pathDb = configuration["PathDb"]
        ?? throw new ArgumentNullException(nameof(configuration), "المفتاح 'PathDb' غير موجود في الإعدادات.");

        _dataAccessLayer = new DataAccessLayer(connectionString, pathDb);
    }

    public async Task AddOperations(CreateOperation createOperation)
    {

        var userId = AppSession.CurrentUserId;

        if (createOperation == null)
            throw new ArgumentNullException(nameof(createOperation));

        var parameters = new[]
        {
            new SqlParameter("@Comment", (object?)createOperation.Comment ?? DBNull.Value),
            new SqlParameter("@NewValue", (object?)createOperation.NewValue ?? DBNull.Value),
            new SqlParameter("@OldValue", (object?)createOperation.OldValue ?? DBNull.Value),
            new SqlParameter("@OperationName", (object?)createOperation.OperationName ?? DBNull.Value),
            new SqlParameter("@DeviceId", createOperation.DeviceId),
            new SqlParameter("@CreatedAt", DateTime.Now),
            new SqlParameter("@UserId",userId )
        };

        await _dataAccessLayer.ExecuteNonQueryAsync("sp_AddOperation", parameters);
    }

    public async Task<List<Operation>> GetAllOperations(int deviceId)
    {
        var parameters = new[]
        {
            new SqlParameter("@DeviceId", deviceId)
        };

        return await _dataAccessLayer.ExecuteQueryAsync("sp_GetAllOperations", parameters, reader => new Operation
        {
            Id = reader.GetInt32(reader.GetOrdinal("Id")),
            Comment = reader.IsDBNull(reader.GetOrdinal("Comment")) ? null : reader.GetString(reader.GetOrdinal("Comment")),
            NewValue = reader.IsDBNull(reader.GetOrdinal("NewValue")) ? null : reader.GetString(reader.GetOrdinal("NewValue")),
            OldValue = reader.IsDBNull(reader.GetOrdinal("OldValue")) ? null : reader.GetString(reader.GetOrdinal("OldValue")),
            OperationName = reader.IsDBNull(reader.GetOrdinal("OperationName")) ? null : reader.GetString(reader.GetOrdinal("OperationName")),
            DeviceId = reader.GetInt32(reader.GetOrdinal("DeviceId")),
            CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
            User = new User
            {
                Id = reader.IsDBNull(reader.GetOrdinal("UserId")) ? 0 : reader.GetInt32(reader.GetOrdinal("UserId")),
                UserName = reader.IsDBNull(reader.GetOrdinal("UserName")) ? null : reader.GetString(reader.GetOrdinal("UserName"))
            }

        });
    }
}
