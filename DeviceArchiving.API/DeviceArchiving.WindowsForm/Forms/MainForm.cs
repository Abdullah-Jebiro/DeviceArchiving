using DeviceArchiving.Data.Dto.Devices;
using DeviceArchiving.Data.Dto.Users;
using DeviceArchiving.Data.Entities;
using DeviceArchiving.WindowsForm.Forms;
using Microsoft.Extensions.DependencyInjection;
using System.Windows.Forms;
using System.Drawing;
using System.Linq;
using System;
using System.Collections.Generic;
using ClosedXML.Excel;
using DeviceArchiving.WindowsForm.Dtos;
using Guna.UI2.WinForms;
using DeviceArchiving.Service.AccountServices;
using DeviceArchiving.Service.OperationServices;
using DeviceArchiving.Service.DeviceServices;
using DeviceArchiving.Service.OperationTypeServices;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using DeviceArchiving.Service;

namespace DeviceArchiving.WindowsForm.Forms;

public partial class MainForm : Form
{
    private readonly IDeviceService _deviceService;
    private readonly IOperationService _operationService;
    private readonly IOperationTypeService _operationTypeService;
    private readonly IAccountService _accountService;
    private readonly IConfiguration _configuration;
    private readonly ExcelReader _excelReader;
    private readonly AuthenticationResponse _user;
    private List<GetAllDevicesDto> _devices = new List<GetAllDevicesDto>();
    private List<GetAllDevicesDto> _filteredDevices = new List<GetAllDevicesDto>();
    private GetAllDevicesDto _selectedDevice;
    private string _globalSearchQuery = "";
    private string _laptopNameFilter = "";
    private string _serialNumberFilter = "";
    private string _typeFilter = "";
    private List<ExcelDevice> _excelData = new List<ExcelDevice>();
    private bool _isLoading = false;

    public MainForm(IAccountService accountService, AuthenticationResponse user , IConfiguration configuration)
    {
        _accountService = accountService;
        _configuration = configuration;
        _user = user;
        _excelReader = new ExcelReader(_configuration);
        _deviceService = Program.Services.GetService<IDeviceService>();
        _operationService = Program.Services.GetService<IOperationService>();
        _operationTypeService = Program.Services.GetService<IOperationTypeService>();

        this.RightToLeft = RightToLeft.Yes;
        this.RightToLeftLayout = true;

        InitializeComponent();

        SetupUI();
        LoadDevices();
    }
    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        base.OnFormClosing(e);
        Application.Exit();
    }
    private void SetupUI()
    {
        this.Text = "≈œ«—… «·√ÃÂ“…";
        this.Size = new Size(1000, 700);
        this.StartPosition = FormStartPosition.CenterScreen;
        this.WindowState = FormWindowState.Maximized;
        this.BackColor = Color.WhiteSmoke;
        this.RightToLeft = RightToLeft.Yes;
        this.RightToLeftLayout = true;

        // Header Label
        var lblHeader = new Guna2HtmlLabel
        {
            Text = $"„—Õ»«° {_user.UserName}",
            Location = new Point(20, 20),
            AutoSize = true,
            Font = new Font("Segoe UI", 16, FontStyle.Bold),
            ForeColor = Color.FromArgb(26, 115, 232),
            RightToLeft = RightToLeft.Yes,
            TabStop = false
        };

        // Search Panel
        var searchPanel = new Guna2Panel
        {
            Location = new Point(20, 60),
            Size = new Size(this.ClientSize.Width - 40, 110),
            Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
            BorderColor = Color.LightGray,
            BorderThickness = 1,
            FillColor = Color.White,
            ShadowDecoration = { Enabled = true, Shadow = new Padding(5) },
            RightToLeft = RightToLeft.Yes,
        };

        // Labels and TextBoxes
        var lblGlobalSearch = new Guna2HtmlLabel
        {
            Text = "«·⁄«„ «·»ÕÀ",
            Location = new Point(searchPanel.ClientSize.Width + 290, 20),
            AutoSize = true,
            RightToLeft = RightToLeft.Yes,
            TabStop = false
        };

        var txtGlobalSearch = new Guna2TextBox
        {
            Location = new Point(lblGlobalSearch.Location.X - 410, 15),
            Width = 400,
            Name = "txtGlobalSearch",
            PlaceholderText = "«»ÕÀ Â‰«...",
            BorderRadius = 8,
            RightToLeft = RightToLeft.Yes,
            TabIndex = 0
        };

        var lblLaptopName = new Guna2HtmlLabel
        {
            Text = " Ê» «··«» «”„",
            Location = new Point(searchPanel.ClientSize.Width + 290, 60),
            AutoSize = true,
            TabStop = false
        };

        var txtLaptopName = new Guna2TextBox
        {
            Location = new Point(lblGlobalSearch.Location.X - 210, 55),
            Width = 200,
            Name = "txtLaptopName",
            PlaceholderText = "«”„ «··«»  Ê»",
            BorderRadius = 8,
            RightToLeft = RightToLeft.Yes,
            TabIndex = 1
        };

        var lblSerialNumber = new Guna2HtmlLabel
        {
            Text = "«· ”·”·Ì «·—ﬁ„",
            Location = new Point(txtLaptopName.Location.X - 100, 60),
            AutoSize = true,
            RightToLeft = RightToLeft.Yes,
            TabStop = false
        };

        var txtSerialNumber = new Guna2TextBox
        {
            Location = new Point(lblSerialNumber.Location.X - 210, 55),
            Width = 200,
            Name = "txtSerialNumber",
            PlaceholderText = "«·—ﬁ„ «· ”·”·Ì",
            BorderRadius = 8,
            RightToLeft = RightToLeft.Yes,
            TabIndex = 2
        };

        var lblType = new Guna2HtmlLabel
        {
            Text = "«·‰Ê⁄",
            Location = new Point(txtSerialNumber.Location.X - 35, 60),
            AutoSize = true,
            RightToLeft = RightToLeft.Yes,
            TabStop = false
        };

        var cmbType = new Guna2ComboBox
        {
            Location = new Point(lblType.Location.X - 210, 55),
            Width = 200,
            Name = "cmbType",
            BorderRadius = 8,
            DropDownStyle = ComboBoxStyle.DropDownList,
            RightToLeft = RightToLeft.Yes,
            TabIndex = 3
        };

        // Clear Button
        var btnClear = new Guna2Button
        {
            Text = "„”Õ",
            Location = new Point(cmbType.Location.X - 210, 55),
            Width = 70,
            BorderRadius = 8,
            FillColor = Color.FromArgb(230, 57, 70),
            ForeColor = Color.White,
            HoverState = { FillColor = Color.FromArgb(200, 30, 50) },
            RightToLeft = RightToLeft.Yes,
            TabIndex = 4
        };

        // Event Handlers
        txtGlobalSearch.TextChanged += (s, e) => { _globalSearchQuery = txtGlobalSearch.Text; ApplyFilters(); };
        txtLaptopName.TextChanged += (s, e) => { _laptopNameFilter = txtLaptopName.Text; ApplyFilters(); };
        txtSerialNumber.TextChanged += (s, e) => { _serialNumberFilter = txtSerialNumber.Text; ApplyFilters(); };
        cmbType.SelectedIndexChanged += (s, e) => { _typeFilter = cmbType.SelectedItem?.ToString() ?? ""; ApplyFilters(); };
        btnClear.Click += BtnClearFilters_Click;

        // Adding Controls to Panel
        searchPanel.Controls.AddRange(new Control[] {
            lblGlobalSearch, txtGlobalSearch,
            lblLaptopName, txtLaptopName,
            lblSerialNumber, txtSerialNumber,
            lblType, cmbType,
            btnClear
        });



        // Buttons Panel
        var buttonsPanel = new Guna2Panel
        {
            Location = new Point(20, 180),
            Size = new Size(this.ClientSize.Width - 40, 60),
            Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
            BorderColor = Color.LightGray,
            BorderThickness = 1,
            FillColor = Color.White,
            ShadowDecoration = { Enabled = true, Shadow = new Padding(5) }
        };

        var buttonColor = Color.FromArgb(26, 115, 232);     // Main button color
        var secondaryColor = Color.FromArgb(29, 206, 13); // Secondary button fill color
        var grayColor = Color.FromArgb(128, 128, 128);    // Gray color
        var btnAddDevice = new Guna2Button
        {
            Text = "≈÷«›… ÃÂ«“",
            Location = new Point(970, 10),
            Width = 110,
            BorderRadius = 8,
            FillColor = secondaryColor,
            ForeColor = Color.White,
            TabIndex = 0
        };

        var btnEditDevice = new Guna2Button
        {
            Text = " ⁄œÌ· ÃÂ«“",
            Location = new Point(850, 10),
            Width = 110,
            BorderRadius = 8,
            FillColor = secondaryColor,
            ForeColor = Color.White,
            TabIndex = 1
        };

        var btnDeleteDevice = new Guna2Button
        {
            Text = "Õ–› ÃÂ«“",
            Location = new Point(730, 10),
            Width = 110,
            BorderRadius = 8,
            FillColor = Color.FromArgb(230, 57, 70),
            ForeColor = Color.White,
            TabIndex = 2
        };

        var btnAddOperation = new Guna2Button
        {
            Text = "≈÷«›… ⁄„·Ì…",
            Location = new Point(610, 10), // 730 - 110 - 10
            Width = 110,
            BorderRadius = 8,
            FillColor = buttonColor,
            ForeColor = Color.White,
            TabIndex = 3
        };

        var btnShowOperations = new Guna2Button
        {
            Text = "⁄—÷ «·⁄„·Ì« ",
            Location = new Point(490, 10), // 610 - 110 - 10
            Width = 110,
            BorderRadius = 8,
            FillColor = buttonColor,
            ForeColor = Color.White,
            TabIndex = 4
        };

        var btnShowOperationTypes = new Guna2Button
        {
            Text = "⁄—÷ √‰Ê«⁄ «·⁄„·Ì« ",
            Location = new Point(350, 10), // 490 - 130 - 10
            Width = 130,
            BorderRadius = 8,
            FillColor = buttonColor,
            ForeColor = Color.White,
            TabIndex = 5
        };

        var btnShowDeletedDevices = new Guna2Button
        {
            Text = "«·√ÃÂ“… «·„Õ–Ê›…",
            Location = new Point(200, 10),
            Width = 130,
            BorderRadius = 8,
            FillColor = buttonColor,
            ForeColor = Color.White,
            TabIndex = 6
        };


        var btnRefresh = new Guna2Button
        {
            Text = " ÕœÌÀ",
            Location = new Point(100, 10),
            Width = 80,
            BorderRadius = 8,
            FillColor = grayColor,
            ForeColor = Color.White,
            TabIndex = 7
        };

        var btnImportExcel = new Guna2Button
        {
            Text = "«Œ — „·› Excel",
            Location = new Point(-40, 10),
            Width = 130,
            BorderRadius = 8,
            FillColor = buttonColor,
            ForeColor = Color.White,
            //Visible = Properties.Settings.Default.CanUploadExcel == true,
            TabIndex = 8
        };

        var btnExportExcel = new Guna2Button
        {
            Text = " ’œÌ—",
            Location = new Point(-170, 10),
            Width = 110,
            BorderRadius = 8,
            FillColor = buttonColor,
            ForeColor = Color.White,
            Visible = false,
            TabIndex = 9
        };



        btnAddDevice.Click += BtnAddDevice_Click;
        btnEditDevice.Click += BtnEditDevice_Click;
        btnDeleteDevice.Click += BtnDeleteDevice_Click;
        btnExportExcel.Click += BtnExportExcel_Click;
        btnImportExcel.Click += BtnImportExcel_Click;
        btnAddOperation.Click += BtnAddOperation_Click;
        btnShowOperations.Click += BtnShowOperations_Click;
        btnRefresh.Click += BtnRefresh_Click;
        btnShowOperationTypes.Click += BtnShowOperationTypes_Click;
        btnShowDeletedDevices.Click += BtnShowDeletedDevices_Click;
        buttonsPanel.Controls.AddRange(new Control[] {
            btnAddDevice, btnEditDevice, btnDeleteDevice,
            btnExportExcel, btnImportExcel, btnAddOperation,
            btnShowOperations, btnRefresh, btnShowOperationTypes ,btnShowDeletedDevices
        });

        // Devices Table
        var dataGridViewDevices = new Guna2DataGridView
        {
            Location = new Point(20, 250),
            Size = new Size(this.ClientSize.Width - 40, this.ClientSize.Height - 300),
            Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom,
            Name = "dataGridViewDevices",
            RightToLeft = RightToLeft.Yes,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            MultiSelect = false,
            RowHeadersVisible = false,
            AllowUserToAddRows = false,
            ReadOnly = true,
            BackgroundColor = Color.White,
            BorderStyle = BorderStyle.None,
            CellBorderStyle = DataGridViewCellBorderStyle.SunkenVertical,
            ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None,
            EnableHeadersVisualStyles = false,
            ColumnHeadersHeight = 40,
            ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = Color.FromArgb(26, 115, 232),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Alignment = DataGridViewContentAlignment.MiddleCenter // „Õ«–«… «·‰’ ›Ì «·„‰ ’›

            },
            DefaultCellStyle = new DataGridViewCellStyle
            {
                Font = new Font("Segoe UI", 9),
                SelectionBackColor = Color.FromArgb(220, 230, 250),
                SelectionForeColor = Color.Black
            }
        };

        dataGridViewDevices.SelectionChanged += DataGridViewDevices_SelectionChanged;
        dataGridViewDevices.DoubleClick += (s, e) => BtnShowOperations_Click(s, e);

        this.Controls.AddRange(new Control[] { lblHeader, searchPanel, buttonsPanel, dataGridViewDevices });


        #region Resize
        this.Resize += (s, e) =>
        {
            searchPanel.Size = new Size(this.ClientSize.Width - 40, 110);
            searchPanel.Location = new Point(20, 60);

            buttonsPanel.Size = new Size(this.ClientSize.Width - 40, 60);
            buttonsPanel.Location = new Point(20, 180);

            dataGridViewDevices.Size = new Size(this.ClientSize.Width - 40, this.ClientSize.Height - 300);
            dataGridViewDevices.Location = new Point(20, 250);

            btnAddDevice.Location = new Point(buttonsPanel.ClientSize.Width - 130, 10);
            btnEditDevice.Location = new Point(btnAddDevice.Location.X - 120, 10);
            btnDeleteDevice.Location = new Point(btnEditDevice.Location.X - 120, 10);
            btnAddOperation.Location = new Point(btnDeleteDevice.Location.X - 120, 10);
            btnShowOperations.Location = new Point(btnAddOperation.Location.X - 120, 10);
            btnShowOperationTypes.Location = new Point(btnShowOperations.Location.X - 140, 10);
            btnShowDeletedDevices.Location = new Point(btnShowOperationTypes.Location.X - 140, 10);
            btnRefresh.Location = new Point(btnShowDeletedDevices.Location.X - 90, 10);
            btnImportExcel.Location = new Point(btnRefresh.Location.X - 140, 10);
            btnExportExcel.Location = new Point(btnImportExcel.Location.X - 120, 10);

            lblGlobalSearch.Location = new Point(searchPanel.ClientSize.Width - 110, 20);
            txtGlobalSearch.Location = new Point(lblGlobalSearch.Location.X - 410, 15);
            lblLaptopName.Location = new Point(searchPanel.ClientSize.Width - 110, 60);
            txtLaptopName.Location = new Point(lblLaptopName.Location.X - 210, 55);
            lblSerialNumber.Location = new Point(txtLaptopName.Location.X - 100, 60);
            txtSerialNumber.Location = new Point(lblSerialNumber.Location.X - 210, 55);
            lblType.Location = new Point(txtSerialNumber.Location.X - 50, 60);
            cmbType.Location = new Point(lblType.Location.X - 210, 55);
            btnClear.Location = new Point(20, 50);
        };
        #endregion Resize
    }
    private async void LoadDevices()
    {
        try
        {
            _isLoading = true;
            _devices = (await _deviceService.GetAllDevicesAsync()).Where(d => d.IsActive == true).ToList();
            _filteredDevices = _devices.ToList();

            UpdateDeviceTypesComboBox();
            UpdateDevicesGrid();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Œÿ√ √À‰«¡  Õ„Ì· «·√ÃÂ“…: {ex.Message}", "Œÿ√", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            _isLoading = false;
        }
    }

    private void BtnShowDeletedDevices_Click(object sender, EventArgs e)
    {
        var deletedDevicesForm = new DeletedDevicesForm(_deviceService);
        deletedDevicesForm.ShowDialog();
        LoadDevices();
    }

    private void UpdateDevicesGrid()
    {
        var dgv = Controls.Find("dataGridViewDevices", true).FirstOrDefault() as Guna2DataGridView;
        if (dgv == null) return;

        dgv.DataSource = null;
        dgv.DataSource = _filteredDevices;
        dgv.ScrollBars = ScrollBars.Both;
        var columnsRename = new Dictionary<string, string>
        {
            {"Id", "„"},
            {"Source", "«·ÃÂ…"},
            {"BrotherName", "«”„ «·√Œ"},
            {"LaptopName", "«”„ «··«»  Ê»"},
            {"SystemPassword", "ﬂ·„… „—Ê— «·‰Ÿ«„"},
            {"WindowsPassword", "ﬂ·„… „—Ê— ÊÌ‰œÊ“"},
            {"HardDrivePassword", "ﬂ·„… «· ‘›Ì—"},
            {"FreezePassword", "ﬂ·„… «· Ã„Ìœ"},
            {"Code", "«·ﬂÊœ"},
            {"Type", "«·‰Ê⁄"},
            {"SerialNumber", "«·—ﬁ„ «· ”·”·Ì"},
            {"Card", "«·ﬂ— "},
            {"Comment", "„·«ÕŸ« "},
            {"ContactNumber", "—ﬁ„ «· Ê«’·"},
            {"UserName", " „ »Ê«”ÿ…"},
            {"CreatedAt", " «—ÌŒ «·≈‰‘«¡"}
        };

        foreach (DataGridViewColumn column in dgv.Columns)
        {
            if (columnsRename.ContainsKey(column.Name))
            {
                column.HeaderText = columnsRename[column.Name];
                column.AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells; // ÷»ÿ «·⁄—÷ »‰«¡ ⁄·Ï «·„Õ ÊÏ
            }
            else
            {
                column.Visible = false;
            }
        }

        dgv.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.DisplayedCells);
    }

    private void UpdateDeviceTypesComboBox()
    {
        var cmbType = Controls.OfType<Panel>()
            .SelectMany(p => p.Controls.OfType<ComboBox>())
            .FirstOrDefault(cmb => cmb.Name == "cmbType");

        if (cmbType == null) return;

        var types = _devices.Select(d => d.Type).Distinct().OrderBy(t => t).ToList();
        cmbType.Items.Clear();
        cmbType.Items.Add(""); // «·ŒÌ«— «·«› —«÷Ì «·›«—€
        cmbType.Items.AddRange(types.ToArray());
        cmbType.SelectedIndex = 0;
    }

    private void ApplyFilters()
    {
        if (_isLoading) return;

        _filteredDevices = _devices.Where(d =>
            (string.IsNullOrEmpty(_globalSearchQuery) || ContainsAny(d, _globalSearchQuery)) &&
            (string.IsNullOrEmpty(_laptopNameFilter) || d.LaptopName?.Contains(_laptopNameFilter, StringComparison.OrdinalIgnoreCase) == true) &&
            (string.IsNullOrEmpty(_serialNumberFilter) || d.SerialNumber?.Contains(_serialNumberFilter, StringComparison.OrdinalIgnoreCase) == true) &&
            (string.IsNullOrEmpty(_typeFilter) || d.Type == _typeFilter)
        ).ToList();

        UpdateDevicesGrid();
    }

    private bool ContainsAny(GetAllDevicesDto device, string search)
    {
        search = search.ToLower();

        return
            (device.Source?.ToLower().Contains(search) == true) ||
            (device.BrotherName?.ToLower().Contains(search) == true) ||
            (device.LaptopName?.ToLower().Contains(search) == true) ||
            (device.SystemPassword?.ToLower().Contains(search) == true) ||
            (device.WindowsPassword?.ToLower().Contains(search) == true) ||
            (device.HardDrivePassword?.ToLower().Contains(search) == true) ||
            (device.FreezePassword?.ToLower().Contains(search) == true) ||
            (device.Code?.ToLower().Contains(search) == true) ||
            (device.Type?.ToLower().Contains(search) == true) ||
            (device.SerialNumber?.ToLower().Contains(search) == true) ||
            (device.Card?.ToLower().Contains(search) == true) ||
            (device.Comment?.ToLower().Contains(search) == true) ||
            (device.ContactNumber?.ToLower().Contains(search) == true) ||
            (device.UserName?.ToLower().Contains(search) == true);
    }

    private void BtnClearFilters_Click(object sender, EventArgs e)
    {
        _globalSearchQuery = "";
        _laptopNameFilter = "";
        _serialNumberFilter = "";
        var searchPanel = this.Controls.OfType<Panel>().First(p => p.Controls.ContainsKey("txtGlobalSearch"));
        (searchPanel.Controls["txtGlobalSearch"] as TextBox).Text = "";
        (searchPanel.Controls["txtLaptopName"] as TextBox).Text = "";
        (searchPanel.Controls["txtSerialNumber"] as TextBox).Text = "";
        (searchPanel.Controls["cmbType"] as ComboBox).SelectedIndex = 0;
        ApplyFilters();
    }

    private void BtnAddDevice_Click(object sender, EventArgs e)
    {
        DeviceForm deviceForm = new DeviceForm(_deviceService);
        if (deviceForm.ShowDialog() == DialogResult.OK)
        {
            LoadDevices();
        }
    }

    private void BtnRefresh_Click(object sender, EventArgs e)
    {
        LoadDevices();
    }

    private void BtnShowOperationTypes_Click(object sender, EventArgs e)
    {
        using (var form = new OperationTypeListForm(_operationTypeService))
        {
            form.ShowDialog();
        }
    }

    private void BtnEditDevice_Click(object sender, EventArgs e)
    {
        if (_selectedDevice == null)
        {
            MessageBox.Show("Ì—ÃÏ  ÕœÌœ ÃÂ«“ ·· ⁄œÌ·.", " Õ–Ì—", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        DeviceForm deviceForm = new DeviceForm(_deviceService, _selectedDevice);
        if (deviceForm.ShowDialog() == DialogResult.OK)
        {
            LoadDevices();
        }
    }

    private async void BtnDeleteDevice_Click(object sender, EventArgs e)
    {
        if (_selectedDevice == null)
        {
            MessageBox.Show("Ì—ÃÏ  ÕœÌœ ÃÂ«“ ··Õ–›.", " Õ–Ì—", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        if (MessageBox.Show($"Â· √‰  „ √ﬂœ „‰ Õ–› «·ÃÂ«“ '{_selectedDevice.LaptopName}'ø", " √ﬂÌœ «·Õ–›", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
        {
            try
            {
                await _deviceService.DeleteDeviceAsync(_selectedDevice.Id);
                _selectedDevice = null;
                LoadDevices();
                MessageBox.Show(" „ Õ–› «·ÃÂ«“ »‰Ã«Õ.", "‰Ã«Õ", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Œÿ√ √À‰«¡ Õ–› «·ÃÂ«“: {ex.Message}", "Œÿ√", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

    private void BtnExportExcel_Click(object sender, EventArgs e)
    {
        try
        {
   

                using (var workbook = new XLWorkbook())
                {
                    var worksheet = workbook.Worksheets.Add("«·√ÃÂ“…");
                    var headers = new[] { "«·ÃÂ…", "«”„ «·√Œ", "«”„ «··«»  Ê»", "ﬂ·„… „—Ê— «·‰Ÿ«„", "ﬂ·„… „—Ê— ÊÌ‰œÊ“", "ﬂ·„… «· ‘›Ì—", "ﬂ·„… «· Ã„Ìœ", "«·ﬂÊœ", "«·‰Ê⁄", "«·—ﬁ„ «· ”·”·Ì", "«·ﬂ— ", "„·«ÕŸ…", "—ﬁ„ «· Ê«’·", " „ »Ê«”ÿ…", " «—ÌŒ «·≈‰‘«¡" };
                    for (int i = 0; i < headers.Length; i++)
                        worksheet.Cell(1, i + 1).Value = headers[i];

                    for (int i = 0; i < _filteredDevices.Count; i++)
                    {
                        var device = _filteredDevices[i];
                        worksheet.Cell(i + 2, 1).Value = device.Source;
                        worksheet.Cell(i + 2, 2).Value = device.BrotherName;
                        worksheet.Cell(i + 2, 3).Value = device.LaptopName;
                        worksheet.Cell(i + 2, 4).Value = device.SystemPassword;
                        worksheet.Cell(i + 2, 5).Value = device.WindowsPassword;
                        worksheet.Cell(i + 2, 6).Value = device.HardDrivePassword;
                        worksheet.Cell(i + 2, 7).Value = device.FreezePassword;
                        worksheet.Cell(i + 2, 8).Value = device.Code;
                        worksheet.Cell(i + 2, 9).Value = device.Type;
                        worksheet.Cell(i + 2, 10).Value = device.SerialNumber;
                        worksheet.Cell(i + 2, 11).Value = device.Card;
                        worksheet.Cell(i + 2, 12).Value = device.Comment;
                        worksheet.Cell(i + 2, 13).Value = device.ContactNumber;
                        worksheet.Cell(i + 2, 14).Value = device.UserName;
                        worksheet.Cell(i + 2, 15).Value = device.CreatedAt.ToString("g");
                    }

                    using (var saveFileDialog = new SaveFileDialog { Filter = "Excel files (*.xlsx)|*.xlsx", FileName = "devices.xlsx" })
                    {
                        if (saveFileDialog.ShowDialog() == DialogResult.OK)
                        {
                            workbook.SaveAs(saveFileDialog.FileName);
                            MessageBox.Show(" „  ’œÌ— «·»Ì«‰«  »‰Ã«Õ.", "‰Ã«Õ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Œÿ√ √À‰«¡  ’œÌ— «·»Ì«‰« : {ex.Message}", "Œÿ√", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async void BtnImportExcel_Click(object sender, EventArgs e)
    {
        using (var passwordForm = new PasswordPromptForm("√œŒ· ﬂ·„… «·”— · ’œÌ— «·»Ì«‰« "))
        {
            if (passwordForm.ShowDialog() != DialogResult.OK)
                return;

            // «” »œ· "admin123" »ﬂ·„… «·”— «·›⁄·Ì… √Ê ﬁ„ »«· Õﬁﬁ „‰ ﬁ«⁄œ… »Ì«‰« /≈⁄œ«œ« 
            if (passwordForm.EnteredPassword != AppSession.Password)
            {
                MessageBox.Show("ﬂ·„… «·”— €Ì— ’ÕÌÕ…", "Œÿ√", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            using (var openFileDialog = new OpenFileDialog { Filter = "Excel files (*.xlsx;*.xls)|*.xlsx;*.xls" })
            {
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        _isLoading = true;
                        _excelData = _excelReader.ReadExcelFile(openFileDialog.FileName);
                        if (_excelData.Count == 0) return;

                        if (!_excelReader.CheckDuplicatesInFile(_excelData))
                        {
                            _excelData.Clear();
                            return;
                        }



                        if (!await CheckDuplicatesInDatabaseAsync(_excelData))
                        {
                            _excelData.Clear();
                            return;
                        }

                        ShowExcelPreviewDialog();
                        LoadDevices();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Œÿ√ √À‰«¡ ﬁ—«¡… «·„·›: {ex.Message}", "Œÿ√", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    finally
                    {
                        _isLoading = false;
                    }
                }
            }
        }

    }



    private async Task<bool> CheckDuplicatesInDatabaseAsync(List<ExcelDevice> devices)
    {
        try
        {
            //  ÕÊÌ· √ÃÂ“… ExcelDevice ≈·Ï CheckDuplicateDto „⁄ «·ÕﬁÊ· «·„ÿ·Ê»… ›ﬁÿ
            var checkItems = devices.Select(d => new CheckDuplicateDto
            {
                SerialNumber = d.SerialNumber,
                LaptopName = d.LaptopName
            }).ToList();

            var response =await  _deviceService.CheckDuplicatesInDatabaseAsync(checkItems);

            if (!response.Success)
            {
                MessageBox.Show($"ÕœÀ Œÿ√ √À‰«¡ «· Õﬁﬁ „‰ «· ﬂ—«—« : {response.Message}",
                                "Œÿ√", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            var duplicates = response.Data;

            bool hasDuplicates = false;
            string message = "";

            if (duplicates.DuplicateSerialNumbers != null && duplicates.DuplicateSerialNumbers.Any())
            {
                hasDuplicates = true;
                message += "ÌÊÃœ √—ﬁ«„  ”·”· „ﬂ——…:\n" + string.Join("\n", duplicates.DuplicateSerialNumbers) + "\n\n";
            }

            if (duplicates.DuplicateLaptopNames != null && duplicates.DuplicateLaptopNames.Any())
            {
                hasDuplicates = true;
                message += "ÌÊÃœ √”„«¡ ·«»  Ê» „ﬂ——…:\n" + string.Join("\n", duplicates.DuplicateLaptopNames) + "\n\n";
            }

            if (hasDuplicates)
            {
                MessageBox.Show(message, " ﬂ—«—«  „ÊÃÊœ…", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Œÿ√ √À‰«¡ «· Õﬁﬁ „‰ «· ﬂ—«—« : {ex.Message}",
                            "Œÿ√", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return false;
        }
    }



    private void ShowExcelPreviewDialog()
    {
        var previewForm = new ExcelPreviewForm(_excelData);
        previewForm.ShowDialog();
    }

    private void DataGridViewDevices_SelectionChanged(object sender, EventArgs e)
    {
        var dataGridView = sender as DataGridView;
        if (dataGridView.SelectedRows.Count > 0)
            _selectedDevice = dataGridView.SelectedRows[0].DataBoundItem as GetAllDevicesDto;
        else
            _selectedDevice = null;
    }

    private void BtnAddOperation_Click(object sender, EventArgs e)
    {
        if (_selectedDevice == null)
        {
            MessageBox.Show("Ì—ÃÏ  ÕœÌœ ÃÂ«“ ·≈÷«›… ⁄„·Ì….", " Õ–Ì—", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        AddOperationDialog addOperationDialog = new AddOperationDialog(_operationTypeService, _operationService, _selectedDevice.Id, _selectedDevice.LaptopName);
        addOperationDialog.ShowDialog();
    }

    private async void BtnShowOperations_Click(object sender, EventArgs e)
    {
        if (_selectedDevice == null)
        {
            MessageBox.Show("Ì—ÃÏ  ÕœÌœ ÃÂ«“ ·⁄—÷ «·⁄„·Ì« .", " Õ–Ì—", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        try
        {
            var operations = await _operationService.GetAllOperations(_selectedDevice.Id);
            using (var operationListForm = new OperationListForm(_operationService, _selectedDevice.Id))
            {
                operationListForm.ShowDialog();
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Œÿ√ √À‰«¡ Ã·» «·⁄„·Ì« : {ex.Message}", "Œÿ√", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}

