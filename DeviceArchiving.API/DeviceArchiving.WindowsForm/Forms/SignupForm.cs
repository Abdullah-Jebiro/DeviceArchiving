using DeviceArchiving.Data.Dto.Users;
using DeviceArchiving.Service.AccountServices;
using Guna.UI2.WinForms;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace DeviceArchiving.WindowsForm.Forms
{
    public partial class SignupForm : Form
    {
        private readonly IAccountService _accountService;

        public SignupForm(IAccountService accountService)
        {
            _accountService = accountService;
            InitializeComponentSignupForm();
            this.RightToLeft = RightToLeft.Yes;
            this.RightToLeftLayout = true;
        }

        private void InitializeComponentSignupForm()
        {
            this.Text = "إنشاء حساب جديد";
            this.Size = new Size(400, 300);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;

            // Guna2 Label for Admin
            Guna2HtmlLabel lblUsername = new Guna2HtmlLabel
            {
                Text = "اسم المستخدم",
                Location = new Point(50, 50),
                AutoSize = true,
                Font = new Font("Segoe UI", 10)
            };

            // Guna2 TextBox for Admin
            Guna2TextBox txtUsername = new Guna2TextBox
            {
                Location = new Point(150, 45),
                Width = 200,
                Name = "txtUsername",
                BorderRadius = 8,
                PlaceholderText = "أدخل اسم المستخدم",
                Font = new Font("Segoe UI", 10)
            };

            // Guna2 Label for Password
            Guna2HtmlLabel lblPassword = new Guna2HtmlLabel
            {
                Text = "كلمة المرور",
                Location = new Point(50, 100),
                AutoSize = true,
                Font = new Font("Segoe UI", 10)
            };

            // Guna2 TextBox for Password
            Guna2TextBox txtPassword = new Guna2TextBox
            {
                Location = new Point(150, 95),
                Width = 200,
                Name = "txtPassword",
                UseSystemPasswordChar = true,
                BorderRadius = 8,
                PlaceholderText = "أدخل كلمة المرور",
                Font = new Font("Segoe UI", 10)
            };

            // Guna2 Button for Save
            Guna2Button btnSave = new Guna2Button
            {
                Text = "إنشاء",
                Location = new Point(150, 150),
                Width = 100,
                Height = 40,
                FillColor = Color.FromArgb(0, 122, 204),
                BorderRadius = 8,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.White
            };

            btnSave.Click += BtnSave_Click;

            // Add Guna2 Shadow effect
            Guna2ShadowForm shadowForm = new Guna2ShadowForm();
            shadowForm.SetShadowForm(this);

            // Add Guna2 controls to form
            this.Controls.AddRange(new Control[] { lblUsername, txtUsername, lblPassword, txtPassword, btnSave });
        }

        private async void BtnSave_Click(object sender, EventArgs e)
        {
            Guna2TextBox txtUsername = this.Controls["txtUsername"] as Guna2TextBox;
            Guna2TextBox txtPassword = this.Controls["txtPassword"] as Guna2TextBox;

            try
            {
                if (string.IsNullOrWhiteSpace(txtUsername.Text) || string.IsNullOrWhiteSpace(txtPassword.Text))
                {
                    MessageBox.Show("يرجى ملء جميع الحقول", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var request = new AuthenticationRequest { UserName = txtUsername.Text, Password = txtPassword.Text };
                var response = await _accountService.AddUserAsync(request);
                if (response.Success)
                {
                    MessageBox.Show("تم إنشاء الحساب بنجاح!", "نجاح", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                else
                {
                    MessageBox.Show(response.Message, "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ أثناء إنشاء الحساب: {ex.Message}", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}