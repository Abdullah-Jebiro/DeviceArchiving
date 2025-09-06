using DeviceArchiving.Data.Dto;
using DeviceArchiving.Data.Dto.Devices;
using DeviceArchiving.Data.Entities;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;

namespace DeviceArchiving.Service.DeviceServices;

public class DeviceProcedureService : IDeviceService
{
    private readonly DataAccessLayer _dataAccessLayer;
    public DeviceProcedureService(IConfiguration configuration)
    {
        string connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new ArgumentNullException(nameof(configuration), "سلسلة الاتصال 'DefaultConnection' غير موجودة.");
        string pathDb = configuration["PathDb"]
         ?? throw new ArgumentNullException(nameof(configuration), "المفتاح 'PathDb' غير موجود في الإعدادات.");

        _dataAccessLayer = new DataAccessLayer(connectionString, pathDb);
    }
    public async Task<BaseResponse<string>> UpdateDeviceAsync(int id, UpdateDeviceDto dto)
    {
        try
        {
            var userId = AppSession.CurrentUserId;
            // Check for existing device
            var existingDevice = await GetDeviceByIdAsync(id);
            // Update the device
            var updateParameters = new[]
            {
                new SqlParameter("@Id", id),
                new SqlParameter("@Source", (object)dto.Source ?? DBNull.Value),
                new SqlParameter("@BrotherName", (object)dto.BrotherName ?? DBNull.Value),
                new SqlParameter("@LaptopName", (object)dto.LaptopName ?? DBNull.Value),
                new SqlParameter("@SystemPassword", (object)dto.SystemPassword ?? DBNull.Value),
                new SqlParameter("@WindowsPassword", (object)dto.WindowsPassword ?? DBNull.Value),
                new SqlParameter("@HardDrivePassword", (object)dto.HardDrivePassword ?? DBNull.Value),
                new SqlParameter("@FreezePassword", (object)dto.FreezePassword ?? DBNull.Value),
                new SqlParameter("@Code", (object)dto.Code ?? DBNull.Value),
                new SqlParameter("@Type", (object)dto.Type ?? DBNull.Value),
                new SqlParameter("@SerialNumber", (object)dto.SerialNumber ?? DBNull.Value),
                new SqlParameter("@Card", (object)dto.Card ?? DBNull.Value),
                new SqlParameter("@Comment", (object)dto.Comment ?? DBNull.Value),
                new SqlParameter("@ContactNumber", (object)dto.ContactNumber ?? DBNull.Value),
                new SqlParameter("@UpdatedAt", DateTime.Now),
                new SqlParameter("@UserId",userId ),
            };
            await _dataAccessLayer.ExecuteNonQueryAsync("sp_UpdateDevice", updateParameters);
            var operations = new List<Operation>();
            // Retrieve the current device to track changes
            TrackDeviceChanges(id, dto, existingDevice!, operations);
            foreach (var operation in operations)
            {
                var operationParameters = new[]
                {
                    new SqlParameter("@DeviceId", operation.DeviceId),
                    new SqlParameter("@OperationName", operation.OperationName),
                    new SqlParameter("@OldValue", (object)operation.OldValue ?? DBNull.Value),
                    new SqlParameter("@NewValue", (object)operation.NewValue ?? DBNull.Value),
                    new SqlParameter("@Comment", (object)operation.Comment ?? DBNull.Value),
                    new SqlParameter("@CreatedAt", operation.CreatedAt),
                    new SqlParameter("@UserId",userId )

                };
                await _dataAccessLayer.ExecuteNonQueryAsync("sp_AddOperation", operationParameters);
            }
            return BaseResponse<string>.SuccessResult("تم تحديث الجهاز بنجاح.");
        }
    
        catch (Exception ex) 
        {
            return BaseResponse<string>.Failure(ex.Message);
        }
    }
 
    public async Task<BaseResponse<string>> AddDeviceAsync(CreateDeviceDto dto)
    {
        try
        {
            // Validate input
            if (dto == null)
                return BaseResponse<string>.Failure("بيانات الجهاز غير صالحة");

            var userId = AppSession.CurrentUserId;
            if (userId == 0)
                return BaseResponse<string>.Failure("معرف المستخدم غير صالح");

            // Check for existing device
            var existingDevice = await GetInactiveDeviceBySerialOrLaptopAsync(dto.SerialNumber, dto.LaptopName);

            if (existingDevice != null)
            {
                string baseMessage = existingDevice.IsActive
                    ? "الجهاز موجود بالنظام حالياً"
                    : "الجهاز موجود بالنظام بس محذوف سابقاً";

                if (existingDevice.LaptopName == dto.LaptopName && existingDevice.SerialNumber != dto.SerialNumber)
                {
                    return BaseResponse<string>.SuccessResult($"{baseMessage}، و رقم التسلسل مختلف");
                }

                if (existingDevice.SerialNumber == dto.SerialNumber && existingDevice.LaptopName != dto.LaptopName)
                {
                    return BaseResponse<string>.SuccessResult($"{baseMessage}، و اسم اللاب توب مختلف");
                }

                return BaseResponse<string>.SuccessResult($"{baseMessage}.");
            }

       



            // Create new device
            var parameters = CreateSqlParameters(dto, userId);
            await _dataAccessLayer.ExecuteNonQueryAsync("sp_AddDevice", parameters);

            return BaseResponse<string>.SuccessResult("تم إضافة الجهاز بنجاح");
        }
        catch (SqlException ex) when (ex.Message.Contains("الجهاز موجود بالفعل في النظام"))
        {
            return BaseResponse<string>.Failure("رقم التسلسل أو اسم اللاب توب موجود بالفعل");
        }
        catch (Exception ex)
        {
            // Consider logging the exception here for debugging
            return BaseResponse<string>.Failure($"حدث خطأ: {ex.Message}");
        }
    }

    public async Task<BaseResponse<DuplicateCheckResponse>> CheckDuplicatesInDatabaseAsync(List<CheckDuplicateDto> items)
    {
        if (items == null || items.Count == 0)
            return BaseResponse<DuplicateCheckResponse>.SuccessResult(new DuplicateCheckResponse());

        try
        {
            var serialNumbers = items
                .Select(i => i.SerialNumber)
                .Where(sn => !string.IsNullOrWhiteSpace(sn))
                .Distinct()
                .ToList();

            var laptopNames = items
                .Select(i => i.LaptopName)
                .Where(ln => !string.IsNullOrWhiteSpace(ln))
                .Distinct()
                .ToList();

            var parameters = new[]
            {
                new SqlParameter("@SerialNumbers", SqlDbType.NVarChar) { Value = string.Join(",", serialNumbers) },
                new SqlParameter("@LaptopNames", SqlDbType.NVarChar) { Value = string.Join(",", laptopNames) }
            };

            var result = await _dataAccessLayer.ExecuteQueryAsync("sp_CheckDuplicates", parameters, reader =>
                new GetDeviceDto
                {
                    SerialNumber = reader.IsDBNull(reader.GetOrdinal("SerialNumber")) ? null : reader.GetString(reader.GetOrdinal("SerialNumber")),
                    LaptopName = reader.IsDBNull(reader.GetOrdinal("LaptopName")) ? null : reader.GetString(reader.GetOrdinal("LaptopName")),
                }
            );

            var duplicateSerials = result
                .Select(r => r.SerialNumber)
                .Where(sn => !string.IsNullOrEmpty(sn))
                .Distinct()
                .ToList();

            var duplicateNames = result
                .Select(r => r.LaptopName)
                .Where(ln => !string.IsNullOrEmpty(ln))
                .Distinct()
                .ToList();

            return BaseResponse<DuplicateCheckResponse>.SuccessResult(new DuplicateCheckResponse
            {
                DuplicateSerialNumbers = duplicateSerials,
                DuplicateLaptopNames = duplicateNames
            });
        }
        catch (Exception ex)
        {
            return BaseResponse<DuplicateCheckResponse>.Failure($"خطأ أثناء التحقق من التكرارات: {ex.Message}");
        }
    }







    private SqlParameter[] CreateSqlParameters(CreateDeviceDto dto, int userId)
    {
        return new[]
        {
        new SqlParameter("@Source", (object)dto.Source ?? DBNull.Value),
        new SqlParameter("@BrotherName", (object)dto.BrotherName ?? DBNull.Value),
        new SqlParameter("@LaptopName", (object)dto.LaptopName ?? DBNull.Value),
        new SqlParameter("@SystemPassword", (object)dto.SystemPassword ?? DBNull.Value),
        new SqlParameter("@WindowsPassword", (object)dto.WindowsPassword ?? DBNull.Value),
        new SqlParameter("@HardDrivePassword", (object)dto.HardDrivePassword ?? DBNull.Value),
        new SqlParameter("@FreezePassword", (object)dto.FreezePassword ?? DBNull.Value),
        new SqlParameter("@Code", (object)dto.Code ?? DBNull.Value),
        new SqlParameter("@Type", (object)dto.Type ?? DBNull.Value),
        new SqlParameter("@SerialNumber", (object)dto.SerialNumber ?? DBNull.Value),
        new SqlParameter("@Card", (object)dto.Card ?? DBNull.Value),
        new SqlParameter("@Comment", (object)dto.Comment ?? DBNull.Value),
        new SqlParameter("@ContactNumber", (object)dto.ContactNumber ?? DBNull.Value),
        new SqlParameter("@CreatedAt", DateTime.Now),
        new SqlParameter("@UserId", userId)
    };
    }

    public async Task<List<GetAllDevicesDto>> GetAllDevicesAsync()
    {
        return await _dataAccessLayer.ExecuteQueryAsync("sp_GetAllDevices", null, reader => new GetAllDevicesDto
        {
            Id = reader.GetInt32(reader.GetOrdinal("Id")),
            Source = reader.IsDBNull(reader.GetOrdinal("Source")) ? null : reader.GetString(reader.GetOrdinal("Source")),
            BrotherName = reader.IsDBNull(reader.GetOrdinal("BrotherName")) ? null : reader.GetString(reader.GetOrdinal("BrotherName")),
            LaptopName = reader.IsDBNull(reader.GetOrdinal("LaptopName")) ? null : reader.GetString(reader.GetOrdinal("LaptopName")),
            SystemPassword = reader.IsDBNull(reader.GetOrdinal("SystemPassword")) ? null : reader.GetString(reader.GetOrdinal("SystemPassword")),
            WindowsPassword = reader.IsDBNull(reader.GetOrdinal("WindowsPassword")) ? null : reader.GetString(reader.GetOrdinal("WindowsPassword")),
            HardDrivePassword = reader.IsDBNull(reader.GetOrdinal("HardDrivePassword")) ? null : reader.GetString(reader.GetOrdinal("HardDrivePassword")),
            FreezePassword = reader.IsDBNull(reader.GetOrdinal("FreezePassword")) ? null : reader.GetString(reader.GetOrdinal("FreezePassword")),
            Code = reader.IsDBNull(reader.GetOrdinal("Code")) ? null : reader.GetString(reader.GetOrdinal("Code")),
            Type = reader.IsDBNull(reader.GetOrdinal("Type")) ? null : reader.GetString(reader.GetOrdinal("Type")),
            SerialNumber = reader.IsDBNull(reader.GetOrdinal("SerialNumber")) ? null : reader.GetString(reader.GetOrdinal("SerialNumber")),
            Card = reader.IsDBNull(reader.GetOrdinal("Card")) ? null : reader.GetString(reader.GetOrdinal("Card")),
            Comment = reader.IsDBNull(reader.GetOrdinal("Comment")) ? null : reader.GetString(reader.GetOrdinal("Comment")),
            ContactNumber = reader.IsDBNull(reader.GetOrdinal("ContactNumber")) ? null : reader.GetString(reader.GetOrdinal("ContactNumber")),
            UserName = reader.IsDBNull(reader.GetOrdinal("UserName")) ? null : reader.GetString(reader.GetOrdinal("UserName")),
            CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
            IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive"))
        });
    }
    public async Task<GetDeviceDto?> GetDeviceByIdAsync(int id)
    {
        try
        {
            // Create a new SqlParameter instance for the first call
            var deviceParams = new[] { new SqlParameter("@Id", id) };

            // Fetch device info
            var devices = await _dataAccessLayer.ExecuteQueryAsync(
                "sp_GetDeviceById_Device",
                deviceParams,
                reader => new
                {
                    Device = new GetDeviceDto
                    {
                        Id = reader.GetInt32(reader.GetOrdinal("Id")),
                        Source = reader.IsDBNull(reader.GetOrdinal("Source")) ? null : reader.GetString(reader.GetOrdinal("Source")),
                        BrotherName = reader.IsDBNull(reader.GetOrdinal("BrotherName")) ? null : reader.GetString(reader.GetOrdinal("BrotherName")),
                        LaptopName = reader.IsDBNull(reader.GetOrdinal("LaptopName")) ? null : reader.GetString(reader.GetOrdinal("LaptopName")),
                        SystemPassword = reader.IsDBNull(reader.GetOrdinal("SystemPassword")) ? null : reader.GetString(reader.GetOrdinal("SystemPassword")),
                        WindowsPassword = reader.IsDBNull(reader.GetOrdinal("WindowsPassword")) ? null : reader.GetString(reader.GetOrdinal("WindowsPassword")),
                        HardDrivePassword = reader.IsDBNull(reader.GetOrdinal("HardDrivePassword")) ? null : reader.GetString(reader.GetOrdinal("HardDrivePassword")),
                        FreezePassword = reader.IsDBNull(reader.GetOrdinal("FreezePassword")) ? null : reader.GetString(reader.GetOrdinal("FreezePassword")),
                        Code = reader.IsDBNull(reader.GetOrdinal("Code")) ? null : reader.GetString(reader.GetOrdinal("Code")),
                        Type = reader.IsDBNull(reader.GetOrdinal("Type")) ? null : reader.GetString(reader.GetOrdinal("Type")),
                        SerialNumber = reader.IsDBNull(reader.GetOrdinal("SerialNumber")) ? null : reader.GetString(reader.GetOrdinal("SerialNumber")),
                        Card = reader.IsDBNull(reader.GetOrdinal("Card")) ? null : reader.GetString(reader.GetOrdinal("Card")),
                        Comment = reader.IsDBNull(reader.GetOrdinal("Comment")) ? null : reader.GetString(reader.GetOrdinal("Comment")),
                        ContactNumber = reader.IsDBNull(reader.GetOrdinal("ContactNumber")) ? null : reader.GetString(reader.GetOrdinal("ContactNumber")),
                        UserName = reader.IsDBNull(reader.GetOrdinal("UserName")) ? null : reader.GetString(reader.GetOrdinal("UserName"))
                    }
                });

            if (!devices.Any())
                return null;

            var device = devices.First().Device;

            // ❌ Do NOT reuse the same SqlParameter instance.
            // ✅ Create a new one for the second call
            var operationParams = new[] { new SqlParameter("@Id", id) };

            // Fetch operations
            var operations = await _dataAccessLayer.ExecuteQueryAsync(
                "sp_GetDeviceById_Operations",
                operationParams,
                reader => new OperationDto
                {
                    OperationName = reader.IsDBNull(reader.GetOrdinal("OperationName")) ? null : reader.GetString(reader.GetOrdinal("OperationName")),
                    OldValue = reader.IsDBNull(reader.GetOrdinal("OldValue")) ? null : reader.GetString(reader.GetOrdinal("OldValue")),
                    NewValue = reader.IsDBNull(reader.GetOrdinal("NewValue")) ? null : reader.GetString(reader.GetOrdinal("NewValue")),
                    Comment = reader.IsDBNull(reader.GetOrdinal("Comment")) ? null : reader.GetString(reader.GetOrdinal("Comment")),
                    CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
                    UserName = reader.IsDBNull(reader.GetOrdinal("UserName")) ? null : reader.GetString(reader.GetOrdinal("UserName"))
                });

            device.OperationsDtos = operations.ToList();

            return device;
        }
        catch (Exception e)
        {
            throw new Exception(e.Message);
        }
    }
    public async Task DeleteDeviceAsync(int id)
    {
        var userId = AppSession.CurrentUserId;

        try
        {
            var parameters = new[]
            {
              new SqlParameter("@Id", id),
              new SqlParameter("@UpdatedAt", DateTime.Now),
              new SqlParameter("@UserId",userId )
            };

            await _dataAccessLayer.ExecuteNonQueryAsync("sp_DeleteDevice", parameters);
        }
        catch (SqlException ex) when (ex.Message.Contains("الجهاز بالمعرف غير موجود"))
        {
            throw new KeyNotFoundException($"الجهاز بالمعرف {id} غير موجود.");
        }
    }

    public async Task<GetDeviceDto?> GetInactiveDeviceBySerialOrLaptopAsync(string? serialNumber, string? laptopName)
    {
        var parameters = new[]
        {
            new SqlParameter("@SerialNumber", (object)serialNumber ?? DBNull.Value),
            new SqlParameter("@LaptopName", (object)laptopName ?? DBNull.Value)
        };

        var devices = await _dataAccessLayer.ExecuteQueryAsync("sp_GetInactiveDeviceBySerialOrLaptop", parameters, reader => new GetDeviceDto
        {
            Id = reader.GetInt32(reader.GetOrdinal("Id")),
            Source = reader.IsDBNull(reader.GetOrdinal("Source")) ? null : reader.GetString(reader.GetOrdinal("Source")),
            BrotherName = reader.IsDBNull(reader.GetOrdinal("BrotherName")) ? null : reader.GetString(reader.GetOrdinal("BrotherName")),
            LaptopName = reader.IsDBNull(reader.GetOrdinal("LaptopName")) ? null : reader.GetString(reader.GetOrdinal("LaptopName")),
            SystemPassword = reader.IsDBNull(reader.GetOrdinal("SystemPassword")) ? null : reader.GetString(reader.GetOrdinal("SystemPassword")),
            WindowsPassword = reader.IsDBNull(reader.GetOrdinal("WindowsPassword")) ? null : reader.GetString(reader.GetOrdinal("WindowsPassword")),
            HardDrivePassword = reader.IsDBNull(reader.GetOrdinal("HardDrivePassword")) ? null : reader.GetString(reader.GetOrdinal("HardDrivePassword")),
            FreezePassword = reader.IsDBNull(reader.GetOrdinal("FreezePassword")) ? null : reader.GetString(reader.GetOrdinal("FreezePassword")),
            Code = reader.IsDBNull(reader.GetOrdinal("Code")) ? null : reader.GetString(reader.GetOrdinal("Code")),
            Type = reader.IsDBNull(reader.GetOrdinal("Type")) ? null : reader.GetString(reader.GetOrdinal("Type")),
            SerialNumber = reader.IsDBNull(reader.GetOrdinal("SerialNumber")) ? null : reader.GetString(reader.GetOrdinal("SerialNumber")),
            Card = reader.IsDBNull(reader.GetOrdinal("Card")) ? null : reader.GetString(reader.GetOrdinal("Card")),
            Comment = reader.IsDBNull(reader.GetOrdinal("Comment")) ? null : reader.GetString(reader.GetOrdinal("Comment")),
            ContactNumber = reader.IsDBNull(reader.GetOrdinal("ContactNumber")) ? null : reader.GetString(reader.GetOrdinal("ContactNumber")),
            UserName = reader.IsDBNull(reader.GetOrdinal("UserName")) ? null : reader.GetString(reader.GetOrdinal("UserName")),
            IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive"))
        });

        return devices.FirstOrDefault();
    }
    public async Task<BaseResponse<string>> RestoreDeviceAsync(int id)
    {
        try
        {
            var device = await GetDeviceByIdAsync(id);
            if (device == null)
                return BaseResponse<string>.Failure("الجهاز غير موجود");

            if (device.IsActive)
                return BaseResponse<string>.Failure("الجهاز نشط بالفعل");

            var updateDto = new UpdateDeviceDto
            {
                Source = device.Source,
                BrotherName = device.BrotherName,
                LaptopName = device.LaptopName,
                SystemPassword = device.SystemPassword,
                WindowsPassword = device.WindowsPassword,
                HardDrivePassword = device.HardDrivePassword,
                FreezePassword = device.FreezePassword,
                Code = device.Code,
                Type = device.Type,
                SerialNumber = device.SerialNumber,
                Card = device.Card,
                Comment = device.Comment,
                ContactNumber = device.ContactNumber,
                IsActive = true 
            };

            var updateResponse = await UpdateDeviceAsync(id, updateDto);

            return updateResponse.Success
                ? BaseResponse<string>.SuccessResult("تم استعادة الجهاز بنجاح")
                : BaseResponse<string>.Failure(updateResponse.Message);
        }
        catch (Exception ex)
        {
            return BaseResponse<string>.Failure($"خطأ أثناء استعادة الجهاز: {ex.Message}");
        }
    }

    public async Task<BaseResponse<int>> ProcessDevicesAsync(List<DeviceUploadDto> deviceUploadDto)
    {


        var userId = AppSession.CurrentUserId;
        if (userId == 0)
            return BaseResponse<int>.Failure("معرف المستخدم غير صالح");

        #region This part is not useful
        if (deviceUploadDto.Any(dto => string.IsNullOrEmpty(dto.SerialNumber) || string.IsNullOrEmpty(dto.LaptopName)))
            return BaseResponse<int>.Failure("رقم التسلسل واسم اللاب توب مطلوبان لجميع الأجهزة");
        #endregion

        int processedCount = 0;
        const int batchSize = 100;

        if (deviceUploadDto.Any())
        {
            processedCount += await ProcessCreateDevicesAsync(deviceUploadDto, userId, batchSize);
        }


        return BaseResponse<int>.SuccessResult(processedCount, $"تم معالجة {processedCount} جهاز بنجاح");
    }

    private async Task<int> ProcessCreateDevicesAsync(List<DeviceUploadDto> createDevices, int userId, int batchSize)
    {
        int processedCount = 0;

        foreach (var batch in createDevices.Chunk(batchSize))
        {
            foreach (var dto in batch)
            {
                var parameters = new[]
                {
                     new SqlParameter("@Source", (object)dto.Source ?? DBNull.Value),
                     new SqlParameter("@BrotherName", (object)dto.BrotherName ?? DBNull.Value),
                     new SqlParameter("@LaptopName", (object)dto.LaptopName ?? DBNull.Value),
                     new SqlParameter("@SystemPassword", (object)dto.SystemPassword ?? DBNull.Value),
                     new SqlParameter("@WindowsPassword", (object)dto.WindowsPassword ?? DBNull.Value),
                     new SqlParameter("@HardDrivePassword", (object)dto.HardDrivePassword ?? DBNull.Value),
                     new SqlParameter("@FreezePassword", (object)dto.FreezePassword ?? DBNull.Value),
                     new SqlParameter("@Code", (object)dto.Code ?? DBNull.Value),
                     new SqlParameter("@Type", (object)dto.Type ?? DBNull.Value),
                     new SqlParameter("@SerialNumber", (object)dto.SerialNumber ?? DBNull.Value),
                     new SqlParameter("@Card", (object)dto.Card ?? DBNull.Value),
                     new SqlParameter("@Comment", (object)dto.Comment ?? DBNull.Value),
                     new SqlParameter("@ContactNumber", (object)dto.ContactNumber ?? DBNull.Value),
                     new SqlParameter("@CreatedAt", (object)dto.CreatedAt ?? DateTime.Now),
                     new SqlParameter("@UserId", userId)
                };

                await _dataAccessLayer.ExecuteNonQueryAsync("sp_AddDevice", parameters);
                processedCount++;
            }
        }

        return processedCount;
    }

    private void AddChange(List<Operation> operations, int deviceId, string operationName, string? oldValue, string? newValue, bool skipValues = false)
    {
        if (oldValue != newValue)
        {
            operations.Add(new Operation
            {
                DeviceId = deviceId,
                OperationName = operationName,
                OldValue = skipValues ? null : oldValue,
                NewValue = skipValues ? null : newValue,
                CreatedAt = DateTime.Now,
                UserId = AppSession.CurrentUserId
            });
        }
    }

    private void TrackDeviceChanges(int id, UpdateDeviceDto dto, GetDeviceDto device, List<Operation> operations)
    {
        AddChange(operations, id, "تمت استعادة الجهاز", device.IsActive.ToString(), dto.IsActive.ToString(), skipValues: true);
        AddChange(operations, id, "تحديث الجهة", device.Source, dto.Source);
        AddChange(operations, id, "تحديث اسم الأخ", device.BrotherName, dto.BrotherName);
        AddChange(operations, id, "تحديث اسم اللاب توب", device.LaptopName, dto.LaptopName);
        AddChange(operations, id, "تحديث كلمة سر النظام", device.SystemPassword, dto.SystemPassword);
        AddChange(operations, id, "تحديث كلمة سر الويندوز", device.WindowsPassword, dto.WindowsPassword);
        AddChange(operations, id, "تحديث كلمة سر الهارد", device.HardDrivePassword, dto.HardDrivePassword);
        AddChange(operations, id, "تحديث كلمة التجميد", device.FreezePassword, dto.FreezePassword);
        AddChange(operations, id, "تحديث الكود", device.Code, dto.Code);
        AddChange(operations, id, "تحديث النوع", device.Type, dto.Type);
        AddChange(operations, id, "تحديث رقم السيريال", device.SerialNumber, dto.SerialNumber);
        AddChange(operations, id, "تحديث الكرت", device.Card, dto.Card);
        AddChange(operations, id, "تحديث الملاحظة", device.Comment, dto.Comment);
        AddChange(operations, id, "تحديث رقم التواصل", device.ContactNumber, dto.ContactNumber);
    }


}

