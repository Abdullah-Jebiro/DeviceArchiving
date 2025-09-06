using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using DeviceArchiving.Data.Dto.Devices;
using DeviceArchiving.Service.DeviceServices;
using Guna.UI2.WinForms;

namespace DeviceArchiving.WindowsForm.Forms;

public partial class DeletedDevicesForm : Form
{
    private readonly IDeviceService _deviceService;
    private List<GetAllDevicesDto> _deletedDevices = new List<GetAllDevicesDto>();
    private string _globalSearchQuery = "";
    private string _laptopNameFilter = "";
    private string _serialNumberFilter = "";
    private string _typeFilter = "";

    public DeletedDevicesForm(IDeviceService deviceService)
    {
        _deviceService = deviceService;
        SetupUI();
        LoadDeletedDevices();
    }

    private void SetupUI()
    {
        this.Text = "الأجهزة المحذوفة";
        this.Size = new Size(800, 600);
        this.StartPosition = FormStartPosition.CenterScreen;
        this.BackColor = Color.WhiteSmoke;
        this.RightToLeft = RightToLeft.Yes;
        this.RightToLeftLayout = true;

        // Header Label
        var lblHeader = new Guna2HtmlLabel
        {
            Text = "قائمة الأجهزة المحذوفة",
            Location = new Point(20, 20),
            AutoSize = true,
            Font = new Font("Segoe UI", 14, FontStyle.Bold),
            ForeColor = Color.FromArgb(26, 115, 232),
            RightToLeft = RightToLeft.Yes
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
            Text = "العام البحث",
            Location = new Point(searchPanel.ClientSize.Width - 110, 20),
            AutoSize = true,
            RightToLeft = RightToLeft.Yes,
            TabStop = false
        };

        var txtGlobalSearch = new Guna2TextBox
        {
            Location = new Point(lblGlobalSearch.Left - 410, 15),
            Width = 400,
            Name = "txtGlobalSearch",
            PlaceholderText = "ابحث هنا...",
            BorderRadius = 8,
            RightToLeft = RightToLeft.Yes,
            TabIndex = 0
        };

        var lblLaptopName = new Guna2HtmlLabel
        {
            Text = "توب اللاب اسم",
            Location = new Point(searchPanel.ClientSize.Width - 110, 60),
            AutoSize = true,
            TabStop = false
        };

        var txtLaptopName = new Guna2TextBox
        {
            Location = new Point(lblLaptopName.Left - 210, 55),
            Width = 200,
            Name = "txtLaptopName",
            PlaceholderText = "اسم اللاب توب",
            BorderRadius = 8,
            RightToLeft = RightToLeft.Yes,
            TabIndex = 1
        };

        var lblSerialNumber = new Guna2HtmlLabel
        {
            Text = "التسلسلي الرقم",
            Location = new Point(txtLaptopName.Left - 100, 60),
            AutoSize = true,
            RightToLeft = RightToLeft.Yes,
            TabStop = false
        };

        var txtSerialNumber = new Guna2TextBox
        {
            Location = new Point(lblSerialNumber.Left - 210, 55),
            Width = 200,
            Name = "txtSerialNumber",
            PlaceholderText = "الرقم التسلسلي",
            BorderRadius = 8,
            RightToLeft = RightToLeft.Yes,
            TabIndex = 2
        };

        var lblType = new Guna2HtmlLabel
        {
            Text = "النوع",
            Location = new Point(txtSerialNumber.Left - 50, 60),
            AutoSize = true,
            RightToLeft = RightToLeft.Yes,
            TabStop = false
        };

        var cmbType = new Guna2ComboBox
        {
            Location = new Point(lblType.Left - 210, 55),
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
            Text = "مسح",
            Location = new Point(20, 50),
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

        // Devices Table
        var dataGridViewDeletedDevices = new Guna2DataGridView
        {
            Location = new Point(20, 180),
            Size = new Size(this.ClientSize.Width - 40, this.ClientSize.Height - 270),
            Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom,
            Name = "dataGridViewDeletedDevices",
            RightToLeft = RightToLeft.Yes,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            MultiSelect = false,
            RowHeadersVisible = false,
            AllowUserToAddRows = false,
            ReadOnly = true,
            BackgroundColor = Color.White,
            BorderStyle = BorderStyle.None,
            CellBorderStyle = DataGridViewCellBorderStyle.SingleVertical,
            ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None,
            EnableHeadersVisualStyles = false,
            ColumnHeadersHeight = 40,
            ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = Color.FromArgb(26, 115, 232),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Alignment = DataGridViewContentAlignment.MiddleCenter
            },
            DefaultCellStyle = new DataGridViewCellStyle
            {
                Font = new Font("Segoe UI", 9),
                SelectionBackColor = Color.FromArgb(220, 230, 250),
                SelectionForeColor = Color.Black
            }
        };

        // Restore Button
        var btnRestore = new Guna2Button
        {
            Text = "استعادة الجهاز",
            Size = new Size(150, 40),
            Location = new Point(this.ClientSize.Width - 170, this.ClientSize.Height - 80),
            Anchor = AnchorStyles.Bottom | AnchorStyles.Right,
            FillColor = Color.FromArgb(26, 115, 232),
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            RightToLeft = RightToLeft.Yes,
            BorderRadius = 5,
            Enabled = false
        };
        btnRestore.Click += async (s, e) => await RestoreDeviceAsync(dataGridViewDeletedDevices);

        // Enable/Disable Restore Button based on selection
        dataGridViewDeletedDevices.SelectionChanged += (s, e) =>
        {
            btnRestore.Enabled = dataGridViewDeletedDevices.SelectedRows.Count > 0;
        };

        this.Controls.AddRange(new Control[] { lblHeader, searchPanel, dataGridViewDeletedDevices, btnRestore });

        // Resize event handler
        this.Resize += (s, e) =>
        {
            searchPanel.Size = new Size(this.ClientSize.Width - 40, 110);
            searchPanel.Location = new Point(20, 60);
            dataGridViewDeletedDevices.Size = new Size(this.ClientSize.Width - 40, this.ClientSize.Height - 300);
            dataGridViewDeletedDevices.Location = new Point(20, 180);
            btnRestore.Location = new Point(this.ClientSize.Width - 170, this.ClientSize.Height - 80);

            // Reposition search panel controls
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
    }

    private async void LoadDeletedDevices()
    {
        try
        {
            _deletedDevices = (await _deviceService.GetAllDevicesAsync()).Where(d => d.IsActive == false).ToList();
            UpdateDeletedDevicesGrid();
            // Populate ComboBox with unique types
            var cmbType = this.Controls.Find("cmbType", true).FirstOrDefault() as Guna2ComboBox;
            if (cmbType != null)
            {
                cmbType.Items.Clear();
                cmbType.Items.AddRange(_deletedDevices.Select(d => d.Type).Distinct().Where(t => !string.IsNullOrEmpty(t)).ToArray());
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"خطأ أثناء تحميل الأجهزة المحذوفة: {ex.Message}", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async Task RestoreDeviceAsync(Guna2DataGridView dgv)
    {
        if (dgv.SelectedRows.Count == 0) return;

        var selectedDevice = dgv.SelectedRows[0].DataBoundItem as GetAllDevicesDto;
        if (selectedDevice == null) return;

        var confirmResult = MessageBox.Show(
            $"هل أنت متأكد من استعادة الجهاز '{selectedDevice.LaptopName}'؟",
            "تأكيد الاستعادة",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question,
            MessageBoxDefaultButton.Button2,
            MessageBoxOptions.RightAlign | MessageBoxOptions.RtlReading);

        if (confirmResult != DialogResult.Yes) return;

        try
        {
            await _deviceService.RestoreDeviceAsync(selectedDevice.Id);
            _deletedDevices.Remove(selectedDevice);
            UpdateDeletedDevicesGrid();
            MessageBox.Show("تم استعادة الجهاز بنجاح", "نجاح", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"خطأ أثناء استعادة الجهاز: {ex.Message}", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void UpdateDeletedDevicesGrid()
    {
        var dgv = this.Controls.Find("dataGridViewDeletedDevices", true).FirstOrDefault() as Guna2DataGridView;
        if (dgv == null) return;

        var filteredDevices = _deletedDevices;

        if (!string.IsNullOrEmpty(_globalSearchQuery))
        {
            filteredDevices = filteredDevices.Where(d =>
                d.LaptopName?.Contains(_globalSearchQuery, StringComparison.OrdinalIgnoreCase) == true ||
                d.SerialNumber?.Contains(_globalSearchQuery, StringComparison.OrdinalIgnoreCase) == true ||
                d.Type?.Contains(_globalSearchQuery, StringComparison.OrdinalIgnoreCase) == true ||
                d.Source?.Contains(_globalSearchQuery, StringComparison.OrdinalIgnoreCase) == true ||
                d.BrotherName?.Contains(_globalSearchQuery, StringComparison.OrdinalIgnoreCase) == true)
                .ToList();
        }

        if (!string.IsNullOrEmpty(_laptopNameFilter))
        {
            filteredDevices = filteredDevices.Where(d =>
                d.LaptopName?.Contains(_laptopNameFilter, StringComparison.OrdinalIgnoreCase) == true)
                .ToList();
        }

        if (!string.IsNullOrEmpty(_serialNumberFilter))
        {
            filteredDevices = filteredDevices.Where(d =>
                d.SerialNumber?.Contains(_serialNumberFilter, StringComparison.OrdinalIgnoreCase) == true)
                .ToList();
        }

        if (!string.IsNullOrEmpty(_typeFilter))
        {
            filteredDevices = filteredDevices.Where(d =>
                d.Type == _typeFilter)
                .ToList();
        }

        dgv.DataSource = null;
        dgv.DataSource = filteredDevices;
        dgv.ScrollBars = ScrollBars.Both;

        var columnsRename = new Dictionary<string, string>
        {
            {"Id", "م"},
            {"Source", "الجهة"},
            {"BrotherName", "اسم الأخ"},
            {"LaptopName", "اسم اللاب توب"},
            {"SystemPassword", "كلمة مرور النظام"},
            {"WindowsPassword", "كلمة مرور ويندوز"},
            {"HardDrivePassword", "كلمة التشفير"},
            {"FreezePassword", "كلمة التجميد"},
            {"Code", "الكود"},
            {"Type", "النوع"},
            {"SerialNumber", "الرقم التسلسلي"},
            {"Card", "الكرت"},
            {"Comment", "ملاحظات"},
            {"ContactNumber", "رقم التواصل"},
            {"UserName", "تم بواسطة"},
            {"CreatedAt", "تاريخ الإنشاء"}
        };

        foreach (DataGridViewColumn column in dgv.Columns)
        {
            if (columnsRename.ContainsKey(column.Name))
            {
                column.HeaderText = columnsRename[column.Name];
                column.AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells;
            }
            else
            {
                column.Visible = false;
            }
        }
    }

    private void ApplyFilters()
    {
        UpdateDeletedDevicesGrid();
    }

    private void BtnClearFilters_Click(object sender, EventArgs e)
    {
        var txtGlobalSearch = this.Controls.Find("txtGlobalSearch", true).FirstOrDefault() as Guna2TextBox;
        var txtLaptopName = this.Controls.Find("txtLaptopName", true).FirstOrDefault() as Guna2TextBox;
        var txtSerialNumber = this.Controls.Find("txtSerialNumber", true).FirstOrDefault() as Guna2TextBox;
        var cmbType = this.Controls.Find("cmbType", true).FirstOrDefault() as Guna2ComboBox;

        if (txtGlobalSearch != null) txtGlobalSearch.Text = "";
        if (txtLaptopName != null) txtLaptopName.Text = "";
        if (txtSerialNumber != null) txtSerialNumber.Text = "";
        if (cmbType != null) cmbType.SelectedIndex = -1;

        _globalSearchQuery = "";
        _laptopNameFilter = "";
        _serialNumberFilter = "";
        _typeFilter = "";

        ApplyFilters();
    }
}