using Microsoft.Data.SqlClient;
using System.Data;

namespace DeviceArchiving.Service;

public partial class DataAccessLayer
{
    private readonly string _connectionString;
    private readonly string _pathDb;

    public DataAccessLayer(string connectionString, string pathDb)
    {
        _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        _pathDb = pathDb ?? throw new ArgumentNullException(nameof(pathDb));
    }


    public string GetConnectionString()
    {
        CheckDatabaseFile();
        return _connectionString;
    }
    public async Task ExecuteNonQueryAsync(string storedProcedureName, SqlParameter[] parameters)
    {
        CheckDatabaseFile();
        using (var connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            using (var command = new SqlCommand(storedProcedureName, connection))
            {
                command.CommandType = CommandType.StoredProcedure;
                if (parameters != null)
                {
                    command.Parameters.AddRange(parameters);
                }

                await command.ExecuteNonQueryAsync();
                connection.Close();
            }
        }
    }

    public async Task<List<T>> ExecuteQueryAsync<T>(string storedProcedureName, SqlParameter[] parameters, Func<SqlDataReader, T> mapFunction)
    {
        CheckDatabaseFile();
        var results = new List<T>();

        using (var connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            using (var command = new SqlCommand(storedProcedureName, connection))
            {
                command.CommandType = CommandType.StoredProcedure;
                if (parameters != null)
                {
                    command.Parameters.AddRange(parameters);
                }

                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        results.Add(mapFunction(reader));
                    }
                }
            }
            connection.Close();
        }

        return results;
    }

    private void CheckDatabaseFile()
    {
        if (!File.Exists(_pathDb))
        {
            throw new FileNotFoundException("ملف قاعدة البيانات غير موجود.", _pathDb);
        }
    }


}
