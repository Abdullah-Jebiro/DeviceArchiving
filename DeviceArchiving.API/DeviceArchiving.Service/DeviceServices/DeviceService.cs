using DeviceArchiving.Data.Contexts;
using DeviceArchiving.Data.Dto;
using DeviceArchiving.Data.Dto.Devices;
using DeviceArchiving.Data.Entities;
using Humanizer;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DeviceArchiving.Service.DeviceServices;

public class DeviceService : IDeviceService
{

    private readonly IDbContextFactory<DeviceArchivingContext> _contextFactory;

    public DeviceService(IDbContextFactory<DeviceArchivingContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<BaseResponse<string>> AddDeviceAsync(CreateDeviceDto dto)
    {

        await using var context = await _contextFactory.CreateDbContextAsync();
        var existingDevice = await context.Devices
            .AnyAsync(d => d.SerialNumber == dto.SerialNumber || d.LaptopName == dto.LaptopName);

        if (existingDevice)
        {
            return BaseResponse<string>.Failure("رقم التسلسل أو اسم اللاب توب موجود بالفعل");
        }

        var device = new Device
        {
            Source = dto.Source,
            BrotherName = dto.BrotherName,
            LaptopName = dto.LaptopName,
            SystemPassword = dto.SystemPassword,
            WindowsPassword = dto.WindowsPassword,
            HardDrivePassword = dto.HardDrivePassword,
            FreezePassword = dto.FreezePassword,
            Code = dto.Code,
            Type = dto.Type,
            SerialNumber = dto.SerialNumber,
            Card = dto.Card,
            Comment = dto.Comment,
            ContactNumber = dto.ContactNumber,
            IsActive = true,
            CreatedAt = DateTime.Now
        };

        context.Devices.Add(device);
        await context.SaveChangesAsync();

        return BaseResponse<string>.SuccessResult("Device added successfully.");
    }

    public async Task<BaseResponse<string>> UpdateDeviceAsync(int id, UpdateDeviceDto dto)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        var device = await context.Devices.Include(d => d.Operations).FirstOrDefaultAsync(d => d.Id == id);
        if (device == null)
            return BaseResponse<string>.Failure("الجهاز غير موجود");

        // Check for existing devices with the same SerialNumber || LaptopName, excluding the current device
        var existingDevice = await context.Devices.AnyAsync(d => (d.SerialNumber == dto.SerialNumber || d.LaptopName == dto.LaptopName) && d.Id != id);
        if (existingDevice)
            return BaseResponse<string>.Failure("رقم التسلسل أو اسم اللاب توب موجود بالفعل");


        var operations = new List<Operation>();
        TrackChange(operations, device, d => d.Source, dto.Source, id, "تحديث الجهة");
        TrackChange(operations, device, d => d.BrotherName, dto.BrotherName, id, "تحديث اسم الأخ");
        TrackChange(operations, device, d => d.LaptopName, dto.LaptopName, id, "تحديث اسم اللاب توب");
        TrackChange(operations, device, d => d.SystemPassword, dto.SystemPassword, id, "تحديث كلمة سر النظام");
        TrackChange(operations, device, d => d.WindowsPassword, dto.WindowsPassword, id, "تحديث كلمة سر الويندوز");
        TrackChange(operations, device, d => d.HardDrivePassword, dto.HardDrivePassword, id, "تحديث كلمة سر الهارد");
        TrackChange(operations, device, d => d.FreezePassword, dto.FreezePassword, id, "تحديث كلمة التجميد");
        TrackChange(operations, device, d => d.Code, dto.Code, id, "تحديث الكود");
        TrackChange(operations, device, d => d.Type, dto.Type, id, "تحديث النوع");
        TrackChange(operations, device, d => d.SerialNumber, dto.SerialNumber, id, "تحديث رقم السيريال");
        TrackChange(operations, device, d => d.Card, dto.Card, id, "تحديث الكرت");
        TrackChange(operations, device, d => d.Comment, dto.Comment, id, "تحديث الملاحظة");
        TrackChange(operations, device, d => d.ContactNumber, dto.ContactNumber, id, "تحديث رقم التواصل");


        device.UpdatedAt = DateTime.Now;
        context.Devices.Update(device);
        if (operations.Any())
            context.Operations.AddRange(operations);

        await context.SaveChangesAsync();
        return BaseResponse<string>.SuccessResult("تم تحديث الجهاز بنجاح.");

    }

    public async Task<BaseResponse<DuplicateCheckResponse>> CheckDuplicatesInDatabaseAsync(List<CheckDuplicateDto> items)
    {
        // Extract serial numbers and laptop names from the input list
        var serialNumbers = items
            .Select(i => i.SerialNumber)
            .Where(sn => !string.IsNullOrEmpty(sn))
            .ToList();

        var laptopNames = items
            .Select(i => i.LaptopName)
            .Where(ln => !string.IsNullOrEmpty(ln))
            .ToList();

        // Create database context
        await using var context = await _contextFactory.CreateDbContextAsync();

        // Fetch existing serial numbers and laptop names from the database
        var existingSerials = await context.Devices
            .Where(d => serialNumbers.Contains(d.SerialNumber))
            .Select(d => d.SerialNumber)
            .ToListAsync();

        var existingLaptopNames = await context.Devices
            .Where(d => laptopNames.Contains(d.LaptopName))
            .Select(d => d.LaptopName)
            .ToListAsync();

        // Prepare the response
        var response = new DuplicateCheckResponse
        {
            DuplicateSerialNumbers = existingSerials,
            DuplicateLaptopNames = existingLaptopNames
        };

        // Check for duplicates and return the appropriate response
        if (!existingSerials.Any() && !existingLaptopNames.Any())
        {
            return BaseResponse<DuplicateCheckResponse>.SuccessResult(
                response,
                "لا توجد أرقام تسلسلية أو أسماء لاب توب مكررة في قاعدة البيانات"
            );
        }
        else
        {
            // Create a message listing the duplicates
            var duplicateMessage = "الأسماء لاب توب المكررة: " + string.Join(", \n", existingLaptopNames) +
                                   " | الأرقام التسلسلية المكررة: " + string.Join(", \n", existingSerials);

            return BaseResponse<DuplicateCheckResponse>.Failure(duplicateMessage);
        }
    }

    public async Task<BaseResponse<int>> ProcessDevicesAsync(List<DeviceUploadDto> dtos)
    {
        if (dtos == null || !dtos.Any())
            return BaseResponse<int>.Failure("لا توجد أجهزة للمعالجة");




        await using var context = await _contextFactory.CreateDbContextAsync();
        int processedCount = 0;
        const int batchSize = 100;

        var checkDuplicates = await CheckDuplicatesInDatabaseAsync(dtos.Select(d => new CheckDuplicateDto
        {
            SerialNumber = d.SerialNumber,
            LaptopName = d.LaptopName
        }).ToList());

        if (checkDuplicates.Data != null && (checkDuplicates.Data.DuplicateSerialNumbers.Any() || checkDuplicates.Data.DuplicateLaptopNames.Any()))
            return BaseResponse<int>.Failure("تم العثور على أرقام تسلسلية أو أسماء لاب توب مكررة");

        foreach (var batch in dtos.Chunk(batchSize))
        {
            var devices = batch.Select(dto => new Device
            {
                Source = dto.Source,
                BrotherName = dto.BrotherName,
                LaptopName = dto.LaptopName,
                SystemPassword = dto.SystemPassword,
                WindowsPassword = dto.WindowsPassword,
                HardDrivePassword = dto.HardDrivePassword,
                FreezePassword = dto.FreezePassword,
                Code = dto.Code,
                Type = dto.Type,
                SerialNumber = dto.SerialNumber,
                Card = dto.Card,
                Comment = dto.Comment,
                ContactNumber = dto.ContactNumber,
                IsActive = true,
                CreatedAt = dto.CreatedAt!, 
            }).ToList();

            context.Devices.AddRange(devices);
            await context.SaveChangesAsync();
            processedCount += devices.Count;
        }

        return BaseResponse<int>.SuccessResult(processedCount, $"تم معالجة {processedCount} جهاز بنجاح");
    }


    public async Task<List<GetAllDevicesDto>> GetAllDevicesAsync()
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.Devices
            .Include(d => d.User)
            .Select(d => new GetAllDevicesDto
            {
                Id = d.Id,
                Source = d.Source,
                BrotherName = d.BrotherName,
                LaptopName = d.LaptopName,
                SystemPassword = d.SystemPassword,
                WindowsPassword = d.WindowsPassword,
                HardDrivePassword = d.HardDrivePassword,
                FreezePassword = d.FreezePassword,
                Code = d.Code,
                Type = d.Type,
                SerialNumber = d.SerialNumber,
                Comment = d.Comment,
                ContactNumber = d.ContactNumber,
                Card = d.Card,
                UserName = d.User.UserName,
                CreatedAt = d.CreatedAt,
                IsActive = d.IsActive,

            })
            .ToListAsync();
    }

    public async Task<GetDeviceDto?> GetDeviceByIdAsync(int id)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        var device = await context.Devices
            .Include(d => d.User)
            .FirstOrDefaultAsync(d => d.Id == id);

        if (device == null)
            return null;

        var operations = await context.Operations
            .Where(o => o.DeviceId == id)
            .Select(o => new OperationDto
            {
                OperationName = o.OperationName,
                OldValue = o.OldValue,
                NewValue = o.NewValue,
                Comment = o.Comment,
                CreatedAt = o.CreatedAt,
                UserName = o.User.UserName,
            })
            .ToListAsync();

        return new GetDeviceDto
        {
            Id = device.Id,
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
            Comment = device.Comment,
            ContactNumber = device.ContactNumber,
            Card = device.Card,
            UserName = device.User.UserName,
            OperationsDtos = operations
        };
    }

    public async Task DeleteDeviceAsync(int id)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        var device = await context.Devices
            .Include(d => d.Operations)
            .FirstOrDefaultAsync(d => d.Id == id);
        if (device == null)
            throw new KeyNotFoundException($"الجهاز بالمعرف {id} غير موجود.");

        device.IsActive = false;
        device.UpdatedAt = DateTime.Now;

        device.Operations.Add(new Operation
        {
            DeviceId = id,
            OperationName = "حذف جهاز",
            CreatedAt = DateTime.Now
        });

        context.Devices.Update(device);
        await context.SaveChangesAsync();
    }

    private void TrackChange(List<Operation> operations, Device device, Func<Device, string?> selector, string? newValue, int deviceId, string operationName)
    {
        var oldValue = selector(device);
        if (oldValue != newValue)
        {
            operations.Add(new Operation
            {
                DeviceId = deviceId,
                OperationName = operationName,
                OldValue = oldValue,
                NewValue = newValue,
                CreatedAt = DateTime.Now
            });

            // Update the device property directly
            var property = typeof(Device).GetProperties()
             .FirstOrDefault(p => p.GetMethod?.Invoke(device, null)?.ToString() == oldValue);
            if (property != null && property.CanWrite)
                property.SetValue(device, newValue);
        }
    }

    public async Task<GetDeviceDto?> GetInactiveDeviceBySerialOrLaptopAsync(string? serialNumber, string? laptopName)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        var device = await context.Devices
            .Include(d => d.User)
            .FirstOrDefaultAsync(d => !d.IsActive &&
                ((serialNumber != null && d.SerialNumber == serialNumber) ||
                 (laptopName != null && d.LaptopName == laptopName)));

        if (device == null)
            return null;

        return new GetDeviceDto
        {
            Id = device.Id,
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
            UserName = device.User?.UserName,
            IsActive = device.IsActive
        };
    }

    public async Task<BaseResponse<string>> RestoreDeviceAsync(int id)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        var device = await context.Devices
            .Include(d => d.Operations)
            .Include(d => d.User)
            .FirstOrDefaultAsync(d => d.Id == id);

        if (device == null)
            return BaseResponse<string>.Failure("الجهاز غير موجود");

        if (device.IsActive)
            return BaseResponse<string>.Failure("الجهاز نشط بالفعل");



        // Restore device
        device.IsActive = true;
        device.UpdatedAt = DateTime.Now;

        // Log restore operation
        device.Operations.Add(new Operation
        {
            DeviceId = id,
            OperationName = "تمت استعادة الجهاز",
            CreatedAt = DateTime.Now,
        });

        context.Devices.Update(device);
        await context.SaveChangesAsync();

        return BaseResponse<string>.SuccessResult("تم استعادة الجهاز بنجاح");
    }
}