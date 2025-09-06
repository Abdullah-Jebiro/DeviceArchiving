using DeviceArchiving.Data.Dto.Devices;
using DeviceArchiving.Service.DeviceServices;
using Guna.UI2.WinForms;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace DeviceArchiving.WindowsForm.Forms
{
    public partial class DeviceForm : Form
    {
        private readonly IDeviceService _deviceService;
        private readonly GetAllDevicesDto _device;

        public DeviceForm(IDeviceService deviceService, GetAllDevicesDto device = null)
        {
            _deviceService = deviceService;
            _device = device;
            InitializeComponent();
            this.RightToLeft = RightToLeft.Yes;
            this.RightToLeftLayout = true;
            SetupUI();
        }

        private void SetupUI()
        {
            this.Text = _device == null ? "إضافة جهاز" : "تعديل جهاز";
            this.Size = new Size(470, 630);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.White;

            // Helper method to create label
            Guna2HtmlLabel CreateLabel(string text, Point location)
            {
                return new Guna2HtmlLabel
                {
                    Text = text,
                    Location = location,
                    AutoSize = true,
                    ForeColor = Color.Black,
                    BackColor = Color.Transparent,
                    TabStop = false,
                    Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point),
                    RightToLeft = RightToLeft.Yes 
                };
            }

            // Helper method to create textbox
            Guna2TextBox CreateTextBox(string name, Point location)
            {
                return new Guna2TextBox
                {
                    Name = name,
                    Location = location,
                    Size = new Size(260, 30),
                    BorderRadius = 5,
                    PlaceholderText = "",
                    RightToLeft = RightToLeft.Yes,
                };
            }

            // Create controls
            var lblSource = CreateLabel("الجهة", new Point(50, 30));
            var txtSource = CreateTextBox("txtSource", new Point(150, 25));

            var lblBrotherName = CreateLabel("الأخ اسم", new Point(50, 70));
            var txtBrotherName = CreateTextBox("txtBrotherName", new Point(150, 65));

            var lblLaptopName = CreateLabel("اللابتوب اسم", new Point(50, 110));
            var txtLaptopName = CreateTextBox("txtLaptopName", new Point(150, 105));

            var lblSystemPassword = CreateLabel("النظام مرور كلمة", new Point(50, 150));
            var txtSystemPassword = CreateTextBox("txtSystemPassword", new Point(150, 145));

            var lblWindowsPassword = CreateLabel("الويندوز  كلمة", new Point(50, 190));
            var txtWindowsPassword = CreateTextBox("txtWindowsPassword", new Point(150, 185));

            var lblHardDrivePassword = CreateLabel("الصلب قرص كلمة", new Point(50, 230));
            var txtHardDrivePassword = CreateTextBox("txtHardDrivePassword", new Point(150, 225));

            var lblFreezePassword = CreateLabel("التجميد كلمة", new Point(50, 270));
            var txtFreezePassword = CreateTextBox("txtFreezePassword", new Point(150, 265));

            var lblCode = CreateLabel("الكود", new Point(50, 310));
            var txtCode = CreateTextBox("txtCode", new Point(150, 305));

            var lblType = CreateLabel("النوع", new Point(50, 350));
            var txtType = CreateTextBox("txtType", new Point(150, 345));

            var lblSerialNumber = CreateLabel("التسلسل رقم", new Point(50, 390));
            var txtSerialNumber = CreateTextBox("txtSerialNumber", new Point(150, 385));

            var lblContactNumber = CreateLabel("التواصل رقم", new Point(50, 430));
            var txtContactNumber = CreateTextBox("txtContactNumber", new Point(150, 425));

            var lblCard = CreateLabel("الشاشة الكرت", new Point(50, 470));
            var txtCard = CreateTextBox("txtCard", new Point(150, 465));


            var lblComment = CreateLabel("ملاحظات", new Point(50, 510));
            var txtComment = CreateTextBox("txtComment", new Point(150, 505));

            var btnSave = new Guna2Button
            {
                Text = "حفظ",
                Location = new Point(180, 550),
                Size = new Size(120, 35),
                BorderRadius = 8,
                FillColor = Color.FromArgb(29, 206, 13), // main green color
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold, GraphicsUnit.Point)
            };
            btnSave.Click += BtnSave_Click;

            // Fill fields if editing
            if (_device != null)
            {
                txtSource.Text = _device.Source;
                txtBrotherName.Text = _device.BrotherName;
                txtLaptopName.Text = _device.LaptopName;
                txtSystemPassword.Text = _device.SystemPassword;
                txtWindowsPassword.Text = _device.WindowsPassword;
                txtHardDrivePassword.Text = _device.HardDrivePassword;
                txtFreezePassword.Text = _device.FreezePassword;
                txtCode.Text = _device.Code;
                txtType.Text = _device.Type;
                txtSerialNumber.Text = _device.SerialNumber;
                txtComment.Text = _device.Comment;
                txtContactNumber.Text = _device.ContactNumber;
                txtCard.Text = _device.Card;
            }

            // Add all controls
            this.Controls.AddRange(new Control[]
            {
                lblSource, txtSource,
                lblBrotherName, txtBrotherName,
                lblLaptopName, txtLaptopName,
                lblSystemPassword, txtSystemPassword,
                lblWindowsPassword, txtWindowsPassword,
                lblHardDrivePassword, txtHardDrivePassword,
                lblFreezePassword, txtFreezePassword,
                lblCode, txtCode,
                lblType, txtType,
                lblSerialNumber, txtSerialNumber,
                lblContactNumber, txtContactNumber,
                lblCard, txtCard,
                lblComment, txtComment,
                btnSave
            });
        }

        private async void BtnSave_Click(object sender, EventArgs e)
        {
            // Get controls
            var txtLaptopName = this.Controls["txtLaptopName"] as Guna2TextBox;
            var txtSerialNumber = this.Controls["txtSerialNumber"] as Guna2TextBox;
            var txtSource = this.Controls["txtSource"] as Guna2TextBox;
            var txtBrotherName = this.Controls["txtBrotherName"] as Guna2TextBox;
            var txtSystemPassword = this.Controls["txtSystemPassword"] as Guna2TextBox;
            var txtWindowsPassword = this.Controls["txtWindowsPassword"] as Guna2TextBox;
            var txtHardDrivePassword = this.Controls["txtHardDrivePassword"] as Guna2TextBox;
            var txtFreezePassword = this.Controls["txtFreezePassword"] as Guna2TextBox;
            var txtCode = this.Controls["txtCode"] as Guna2TextBox;
            var txtType = this.Controls["txtType"] as Guna2TextBox;
            var txtComment = this.Controls["txtComment"] as Guna2TextBox;
            var txtContactNumber = this.Controls["txtContactNumber"] as Guna2TextBox;
            var txtCard = this.Controls["txtCard"] as Guna2TextBox;

            // Validate required fields
            if (string.IsNullOrWhiteSpace(txtLaptopName.Text))
            {
                MessageBox.Show("اسم اللاب توب مطلوب", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (string.IsNullOrWhiteSpace(txtSerialNumber.Text))
            {
                MessageBox.Show("رقم التسلسل مطلوب", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                if (_device == null)
                {
                    var dto = new CreateDeviceDto
                    {
                        Source = txtSource.Text,
                        BrotherName = txtBrotherName.Text,
                        LaptopName = txtLaptopName.Text,
                        SystemPassword = txtSystemPassword.Text,
                        WindowsPassword = txtWindowsPassword.Text,
                        HardDrivePassword = txtHardDrivePassword.Text,
                        FreezePassword = txtFreezePassword.Text,
                        Code = txtCode.Text,
                        Type = txtType.Text,
                        SerialNumber = txtSerialNumber.Text,
                        Comment = txtComment.Text,
                        ContactNumber = txtContactNumber.Text,
                        Card = txtCard.Text
                    };
                    var response = await _deviceService.AddDeviceAsync(dto);
                    if (response.Success)
                    {
                        MessageBox.Show(response.Message, "نجاح", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        this.DialogResult = DialogResult.OK;
                        this.Close();
                    }
                    else
                    {
                        MessageBox.Show(response.Message, "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                else
                {
                    var dto = new UpdateDeviceDto
                    {
                        Source = txtSource.Text,
                        BrotherName = txtBrotherName.Text,
                        LaptopName = txtLaptopName.Text,
                        SystemPassword = txtSystemPassword.Text,
                        WindowsPassword = txtWindowsPassword.Text,
                        HardDrivePassword = txtHardDrivePassword.Text,
                        FreezePassword = txtFreezePassword.Text,
                        Code = txtCode.Text,
                        Type = txtType.Text,
                        SerialNumber = txtSerialNumber.Text,
                        Comment = txtComment.Text,
                        ContactNumber = txtContactNumber.Text,
                        Card = txtCard.Text
                    };
                    var response = await _deviceService.UpdateDeviceAsync(_device.Id, dto);
                    if (response.Success)
                    {
                        MessageBox.Show("تم تعديل الجهاز بنجاح", "نجاح", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        this.DialogResult = DialogResult.OK;
                        this.Close();
                    }
                    else
                    {
                        MessageBox.Show(response.Message, "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ أثناء حفظ الجهاز: {ex.Message}", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
