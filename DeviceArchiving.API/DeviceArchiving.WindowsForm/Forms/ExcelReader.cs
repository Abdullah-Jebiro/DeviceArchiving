using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Microsoft.Extensions.Configuration;
using ClosedXML.Excel;
using System.Globalization;
using System.IO;
using DeviceArchiving.WindowsForm.Dtos;

public class ExcelReader
{
    private readonly IConfiguration _configuration;
    private readonly string[] _headers = { "�����", "�����", "��� ����", "��� ��������", "���� �� ������",
        "���� �� ��������", "���� �� ������", "���� �������", "�����", "�����",
        "��� ��������", "�����", "�������", "�������", "��� �������" };

    public ExcelReader(IConfiguration configuration)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
    }

    public List<ExcelDevice> ReadExcelFile(string filePath)
    {
        if (!File.Exists(filePath))
        {
            ShowErrorMessage("����� ��� �����.");
            return new List<ExcelDevice>();
        }

        try
        {
            using var workbook = new XLWorkbook(filePath);
            var worksheet = workbook.Worksheet(1);
            var devices = new List<ExcelDevice>();
            var errorMessages = new List<string>();

            if (!ValidateHeaders(worksheet))
            {
                ShowErrorMessage($"��� ������ ��� ����. ���� ������� ������ ������: {string.Join(", ", _headers)}");
                return new List<ExcelDevice>();
            }

            var allowedFormats = _configuration.GetSection("DateSettings:AllowedFormats").Get<string[]>();
            var datePattern = _configuration.GetSection("DateSettings:DatePattern").Get<string>();

            foreach (var row in worksheet.RowsUsed().Skip(1))
            {
                if (!TryParseRow(row, allowedFormats, datePattern, out var device, out var error))
                {
                    errorMessages.Add(error);
                    continue;
                }
                devices.Add(device);
            }

            return HandleResults(devices, errorMessages);
        }
        catch (Exception ex)
        {
            ShowErrorMessage($"��� ����� ����� �����: {ex.Message}");
            return new List<ExcelDevice>();
        }
    }

    private bool ValidateHeaders(IXLWorksheet worksheet)
    {
        for (int i = 0; i < _headers.Length; i++)
        {
            if (worksheet.Cell(1, i + 1).GetString() != _headers[i])
            {
                return false;
            }
        }
        return true;
    }

    private bool TryParseRow(IXLRow row, string[] allowedFormats, string datePattern,
        out ExcelDevice device, out string error)
    {
        device = null;
        error = string.Empty;

        string dateString = row.Cell(14).GetString().Trim().Split(' ')[0];
        if (string.IsNullOrWhiteSpace(dateString))
        {
            error = $"���� ������� �� ���� {row.RowNumber()} ����. ���� ����� ����� ������� {string.Join(" �� ", allowedFormats)}.";
            return false;
        }

        if (!Regex.IsMatch(dateString, datePattern) ||
            !DateTime.TryParseExact(dateString, allowedFormats, CultureInfo.InvariantCulture,
                DateTimeStyles.None, out var parsedDate))
        {
            error = $"���� ������� �� ���� {row.RowNumber()} ��� ����� �� ������� ��� ����. ��� �� ���� {string.Join(" �� ", allowedFormats)}.";
            return false;
        }

        var laptopName = row.Cell(4).GetString().Trim();
        var systemPassword = row.Cell(5).GetString().Trim();
        var windowsPassword = row.Cell(6).GetString().Trim();
        var hardDrivePassword = row.Cell(7).GetString().Trim();
        var serialNumber = row.Cell(11).GetString().Trim();

        var requiredFields = new (string Field, string Label)[]
        {
            (laptopName, "��� ��������"),
            (systemPassword, "���� �� ������"),
            (windowsPassword, "���� �� ��������"),
            (hardDrivePassword, "���� �� ������"),
            (serialNumber, "��� ��������")
        };

        var missingFields = requiredFields
            .Where(field => string.IsNullOrWhiteSpace(field.Field))
            .Select(field => field.Label)
            .ToList();

        if (missingFields.Any())
        {
            error = $"���� ������ ����� �� ���� {row.RowNumber()}: {string.Join("� ", missingFields)}";
            return false;
        }

        device = new ExcelDevice
        {
            Source = row.Cell(2).GetString().Trim(),
            BrotherName = row.Cell(3).GetString().Trim(),
            LaptopName = laptopName,
            SystemPassword = systemPassword,
            WindowsPassword = windowsPassword,
            HardDrivePassword = hardDrivePassword,
            FreezePassword = row.Cell(8).GetString().Trim(),
            Code = row.Cell(9).GetString().Trim(),
            Type = row.Cell(10).GetString().Trim(),
            SerialNumber = serialNumber,
            Card = row.Cell(12).GetString().Trim(),
            Comment = string.IsNullOrWhiteSpace(row.Cell(13).GetString()) ? null : row.Cell(13).GetString().Trim(),
            ContactNumber = row.Cell(15).GetString().Trim(),
            CreatedAt = new DateTime(parsedDate.Year, parsedDate.Month, parsedDate.Day, 0, 0, 0),
            IsSelected = true,
            IsDuplicateSerial = false,
            IsDuplicateLaptopName = false
        };

        return true;
    }

    private List<ExcelDevice> HandleResults(List<ExcelDevice> devices, List<string> errorMessages)
    {
        if (devices.Count == 0 && errorMessages.Any())
        {
            ShowWarningMessage(string.Join("\n", errorMessages));
            return new List<ExcelDevice>();
        }

        if (errorMessages.Any())
        {
            ShowWarningMessage("���� ����� �� ������ �������:\n" + string.Join("\n", errorMessages));
            return new List<ExcelDevice>();
        }

        if (devices.Count == 0)
        {
            ShowWarningMessage("�� ��� ������ ��� ������ ����� �� �����.");
            return new List<ExcelDevice>();
        }

        return devices;
    }

    public bool CheckDuplicatesInFile(List<ExcelDevice> devices)
    {
        var serialNumbers = new HashSet<string>();
        var laptopNames = new HashSet<string>();
        var duplicateSerials = new HashSet<string>();
        var duplicateLaptopNames = new HashSet<string>();

        for (int i = 0; i < devices.Count; i++)
        {
            var device = devices[i];

            if (!string.IsNullOrEmpty(device.SerialNumber))
            {
                if (serialNumbers.Contains(device.SerialNumber))
                    duplicateSerials.Add(device.SerialNumber);
                else
                    serialNumbers.Add(device.SerialNumber);
            }

            if (!string.IsNullOrEmpty(device.LaptopName))
            {
                if (laptopNames.Contains(device.LaptopName))
                    duplicateLaptopNames.Add(device.LaptopName);
                else
                    laptopNames.Add(device.LaptopName);
            }
        }

        var errorMessages = new List<string>();
        if (duplicateSerials.Any())
            errorMessages.Add($"����� ������ �����: {string.Join(", ", duplicateSerials)}");
        if (duplicateLaptopNames.Any())
            errorMessages.Add($"����� ������ �����: {string.Join(", ", duplicateLaptopNames)}");

        if (errorMessages.Any())
        {
            ShowErrorMessage($"����� ����� ��� �����: {string.Join("� ", errorMessages)}\n���� �� �� ����� �������� ������ �������� �����.");
            return false;
        }

        return true;
    }

    private void ShowErrorMessage(string message)
    {
        MessageBox.Show(message, "���", MessageBoxButtons.OK, MessageBoxIcon.Error);
    }

    private void ShowWarningMessage(string message)
    {
        MessageBox.Show(message, "�����", MessageBoxButtons.OK, MessageBoxIcon.Warning);
    }
}

