using DeviceArchiving.Data.Dto.Devices;
using DeviceArchiving.Service.DeviceServices;
using DeviceArchiving.WindowsForm.Dtos;
using Guna.UI2.WinForms;
using Microsoft.Extensions.DependencyInjection;
using Mono.TextTemplating;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;


namespace DeviceArchiving.WindowsForm.Forms;
public partial class ExcelPreviewForm : Form
{
    private readonly List<ExcelDevice> _devices;
    private readonly IDeviceService _deviceService;
    private Guna2DataGridView _grid;
    private Guna2Button _btnUpload;

    public ExcelPreviewForm(List<ExcelDevice> devices)
    {
        _devices = devices;
        _deviceService = Program.Services.GetService<IDeviceService>();
        SetupUI();
    }

    private void SetupUI()
    {
        this.StartPosition = FormStartPosition.CenterScreen;
        this.Width = Screen.PrimaryScreen.WorkingArea.Width;
        this.Height = 550;
        this.StartPosition = FormStartPosition.CenterScreen;
        this.RightToLeft = RightToLeft.Yes;
        this.Font = new Font("Segoe UI", 10);

        _grid = new Guna2DataGridView
        {
            Dock = DockStyle.Fill,
            RightToLeft = RightToLeft.Yes,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            MultiSelect = false,
            AutoGenerateColumns = false,
            EnableHeadersVisualStyles = false,
            BorderStyle = BorderStyle.None,
            BackgroundColor = Color.White,
            GridColor = Color.LightGray,
            AlternatingRowsDefaultCellStyle = new DataGridViewCellStyle { BackColor = Color.FromArgb(245, 245, 245) },
            ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = Color.FromArgb(94, 148, 255),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Alignment = DataGridViewContentAlignment.MiddleCenter
            },
            DefaultCellStyle = new DataGridViewCellStyle
            {
                Font = new Font("Segoe UI", 10),
                SelectionBackColor = Color.FromArgb(204, 228, 247),
                SelectionForeColor = Color.Black
            }
        };

        _grid.CellValueChanged += GridCellValueChanged;

        _grid.Columns.Add(new DataGridViewCheckBoxColumn { Name = "IsSelected", HeaderText = "تحديد", DataPropertyName = "IsSelected" });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Source", HeaderText = "الجهة", DataPropertyName = "Source" });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "BrotherName", HeaderText = "اسم الأخ", DataPropertyName = "BrotherName" });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "LaptopName", HeaderText = "اسم اللاب توب", DataPropertyName = "LaptopName" });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "SystemPassword", HeaderText = "كلمة مرور النظام", DataPropertyName = "SystemPassword" });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "WindowsPassword", HeaderText = "كلمة مرور ويندوز", DataPropertyName = "WindowsPassword" });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "HardDrivePassword", HeaderText = "كلمة التشفير", DataPropertyName = "HardDrivePassword" });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "FreezePassword", HeaderText = "كلمة التجميد", DataPropertyName = "FreezePassword" });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Code", HeaderText = "الكود", DataPropertyName = "Code" });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Type", HeaderText = "النوع", DataPropertyName = "Type" });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "SerialNumber", HeaderText = "الرقم التسلسلي", DataPropertyName = "SerialNumber" });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Card", HeaderText = "الكرت", DataPropertyName = "Card" });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "CreatedAt", HeaderText = "التاريخ", DataPropertyName = "CreatedAt" });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "ContactNumber", HeaderText = "رقم التواصل", DataPropertyName = "ContactNumber" });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Comment", HeaderText = "ملاحظات", DataPropertyName = "Comment" });


        _grid.DataSource = _devices;

        // Buttons
        var btnSelectAll = new Guna2Button
        {
            Text = "تحديد الكل",
            Width = 120,
            Margin = new Padding(10)
        };
        btnSelectAll.Click += (s, e) =>
        {
            foreach (var d in _devices) d.IsSelected = true;
            _grid.Refresh();
        };

        var btnDeselectAll = new Guna2Button
        {
            Text = "إلغاء تحديد الكل",
            Width = 150,
            Margin = new Padding(10)
        };
        btnDeselectAll.Click += (s, e) =>
        {
            foreach (var d in _devices) d.IsSelected = false;
            _grid.Refresh();
        };

        var btnCancel = new Guna2Button
        {
            Text = "إلغاء",
            Width = 100,
            Margin = new Padding(10),
            FillColor = Color.Gray
        };
        btnCancel.Click += (s, e) => this.Close();

        _btnUpload = new Guna2Button
        {
            Text = "رفع الأجهزة المحددة",
            Width = 180,
            Margin = new Padding(10),
            FillColor = Color.FromArgb(94, 148, 255)
        };
        _btnUpload.Click += BtnUpload_Click;

        var buttonPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Bottom,
            Height = 60,
            FlowDirection = FlowDirection.RightToLeft,
            Padding = new Padding(10)
        };

        buttonPanel.Controls.AddRange(new Control[] { _btnUpload, btnCancel, btnDeselectAll, btnSelectAll });

        // Add controls
        this.Controls.Add(_grid);
        this.Controls.Add(buttonPanel);
    }

    private void GridCellValueChanged(object sender, DataGridViewCellEventArgs e)
    {
        if (e.ColumnIndex == 0 && e.RowIndex >= 0)
        {
            _devices[e.RowIndex].IsSelected = Convert.ToBoolean(_grid.Rows[e.RowIndex].Cells[0].Value);
        }
    }
    
    private async void BtnUpload_Click(object sender, EventArgs e)
    {
        var selectedDevices = _devices
            .Where(d => d.IsSelected)
            .Select(d => new DeviceUploadDto
            {
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
                Card = d.Card,
                Comment = d.Comment,
                ContactNumber = d.ContactNumber,
                CreatedAt = d.CreatedAt,
            })
            .ToList();

        if (!selectedDevices.Any())
        {
            MessageBox.Show("لم يتم تحديد أي أجهزة للرفع", "تحذير", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        _btnUpload.Enabled = false;

        try
        {
            var response = await _deviceService.ProcessDevicesAsync(selectedDevices);
            if (response.Success)
            {
                MessageBox.Show(
                    response.Message ?? $"تم معالجة {response.Data} جهاز{(response.Data == 1 ? "" : "ات")} بنجاح",
                    "نجاح", MessageBoxButtons.OK, MessageBoxIcon.Information
                );

                Properties.Settings.Default.CanUploadExcel = false;
                Properties.Settings.Default.Save();
                _devices.Clear();
                this.Close();
            }
            else
            {
                MessageBox.Show(response.Message ?? "فشل معالجة الأجهزة", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message ?? "حدث خطأ أثناء معالجة الأجهزة", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            _btnUpload.Enabled = true;
        }
    }
}
