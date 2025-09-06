using DeviceArchiving.Data.Dto.Users;
using DeviceArchiving.Service;
using DeviceArchiving.Service.AccountServices;
using Guna.UI2.WinForms;
using Microsoft.Extensions.Configuration;
using System;
using System.Configuration;
using System.Drawing;
using System.Windows.Forms;

namespace DeviceArchiving.WindowsForm.Forms
{
    public partial class LoginForm : Form
    {
        private readonly IAccountService _accountService;
        private readonly IConfiguration _configuration;

        private Guna2TextBox txtUsername;
        private Guna2TextBox txtPassword;
        private Guna2Button btnLogin;
        private Guna2Button btnSignup;

        public AuthenticationResponse AuthResponse { get; private set; }
        public int LoggedInUserId => AuthResponse?.Id ?? 0;

        public LoginForm(IAccountService accountService  , IConfiguration configuration)
        {
            _accountService = accountService;
            _configuration = configuration;

            this.RightToLeft = RightToLeft.Yes;
            this.RightToLeftLayout = true;
         
            SetupUI();

        }

        private void SetupUI()
        {
            this.Text = "تسجيل الدخول";
            this.Size = new Size(420, 280);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.BackColor = Color.White;

            Label lblUsername = new Label
            {
                Text = "اسم المستخدم",
                Location = new Point(40, 50),
                AutoSize = true,
                Font = new Font("Segoe UI", 10)
            };

            txtUsername = new Guna2TextBox
            {
                Location = new Point(150, 45),
                Width = 200,
                PlaceholderText = "أدخل اسم المستخدم",
                BorderRadius = 6,
                Font = new Font("Segoe UI", 10)
            };

            Label lblPassword = new Label
            {
                Text = "كلمة المرور",
                Location = new Point(40, 100),
                AutoSize = true,
                Font = new Font("Segoe UI", 10)
            };

            txtPassword = new Guna2TextBox
            {
                Location = new Point(150, 95),
                Width = 200,
                PlaceholderText = "أدخل كلمة المرور",
                BorderRadius = 6,
                Font = new Font("Segoe UI", 10),
                UseSystemPasswordChar = true
            };

            btnLogin = new Guna2Button
            {
                Text = "تسجيل الدخول",
                Location = new Point(150, 150),
                Width = 100,
                Height = 40,
                FillColor = Color.FromArgb(0, 122, 204),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                BorderRadius = 8,
                ForeColor = Color.White
            };
            btnLogin.Click += BtnLogin_Click;

            btnSignup = new Guna2Button
            {
                Text = "إنشاء حساب",
                Location = new Point(260, 150),
                Width = 100,
                Height = 40,
                FillColor = Color.Gray,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                BorderRadius = 8,
                ForeColor = Color.White
            };
            btnSignup.Click += BtnSignup_Click;

            this.AcceptButton = btnLogin;
            this.CancelButton = btnSignup;

            this.Controls.AddRange(new Control[]
            {
                lblUsername, txtUsername,
                lblPassword, txtPassword,
                btnLogin, btnSignup
            });

            this.Load += (s, e) => txtUsername.Focus();
        }

        private async void BtnLogin_Click(object sender, EventArgs e)
        {
            try
            {
                var request = new AuthenticationRequest
                {
                    UserName = txtUsername.Text.Trim(),
                    Password = txtPassword.Text.Trim()
                };

                var response = await _accountService.AuthenticateAsync(request);

                if (response.Success)
                {
                    AppSession.CurrentUserId = response.Data.Id;
                    MainForm mainForm = new MainForm(_accountService, response.Data , _configuration);
                    mainForm.Show();
                    this.Hide();
                }
                else
                {
                    MessageBox.Show(response.Message, "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"حدث خطأ أثناء تسجيل الدخول: {ex.Message}", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnSignup_Click(object sender, EventArgs e)
        {
            using (var passwordForm = new PasswordPromptForm("أدخل كلمة السر لإنشاء حساب جديد:"))
            {
                if (passwordForm.ShowDialog() != DialogResult.OK)
                    return;

                if (passwordForm.EnteredPassword != AppSession.Password)
                {
                    MessageBox.Show("كلمة السر غير صحيحة", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                Guna2TextBox txtUsername = this.Controls["txtUsername"] as Guna2TextBox;
                Guna2TextBox txtPassword = this.Controls["txtPassword"] as Guna2TextBox;
                SignupForm signupForm = new SignupForm(_accountService);
                if (signupForm.ShowDialog() == DialogResult.OK)
                {
                    MessageBox.Show("تم إنشاء الحساب بنجاح، يرجى تسجيل الدخول.", "نجاح", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }
    }
}
