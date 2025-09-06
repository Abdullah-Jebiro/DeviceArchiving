using DeviceArchiving.Data.Entities;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace DeviceArchiving.Service.OperationTypeServices;

public class OperationTypeProcedureService : IOperationTypeService
{
    private readonly DataAccessLayer _dataAccessLayer;

    public OperationTypeProcedureService(IConfiguration configuration)
    {
        string connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new ArgumentNullException(nameof(configuration), "سلسلة الاتصال 'DefaultConnection' غير موجودة.");
        string pathDb = configuration["PathDb"]
            ?? throw new ArgumentNullException(nameof(configuration), "المفتاح 'PathDb' غير موجود في الإعدادات.");

        _dataAccessLayer = new DataAccessLayer(connectionString, pathDb);
    }

    public async Task AddOperationType(OperationType operationType)
    {
        var parameters = new[]
        {
            new SqlParameter("@Name", (object)operationType.Name ?? DBNull.Value),
            new SqlParameter("@Description", (object)operationType.Description ?? DBNull.Value)
        };

        await _dataAccessLayer.ExecuteNonQueryAsync("sp_AddOperationType", parameters);
    }

    public async Task<IEnumerable<OperationType>> GetAllOperationsTypes(string? searchTerm)
    {
        var parameters = new[]
        {
            new SqlParameter("@SearchTerm", (object)searchTerm ?? DBNull.Value)
        };

        return await _dataAccessLayer.ExecuteQueryAsync("sp_GetAllOperationTypes", parameters, reader => new OperationType
        {
            Id = reader.GetInt32(reader.GetOrdinal("Id")),
            Name = reader.IsDBNull(reader.GetOrdinal("Name")) ? null : reader.GetString(reader.GetOrdinal("Name")),
            Description = reader.IsDBNull(reader.GetOrdinal("Description")) ? null : reader.GetString(reader.GetOrdinal("Description"))
        });
    }

    public async Task UpdateOperationType(OperationType operationType)
    {
        var parameters = new[]
        {
            new SqlParameter("@Id", operationType.Id),
            new SqlParameter("@Name", (object)operationType.Name ?? DBNull.Value),
            new SqlParameter("@Description", (object)operationType.Description ?? DBNull.Value)
        };

        try
        {
            await _dataAccessLayer.ExecuteNonQueryAsync("sp_UpdateOperationType", parameters);
        }
        catch (SqlException ex) when (ex.Number == 50000 && ex.Message.Contains("نوع العملية غير موجود"))
        {
            throw new Exception("نوع العملية غير موجود");
        }
    }

    public async Task DeleteOperationType(int id)
    {
        var parameters = new[]
        {
            new SqlParameter("@Id", id)
        };

        await _dataAccessLayer.ExecuteNonQueryAsync("sp_DeleteOperationType", parameters);
    }
}