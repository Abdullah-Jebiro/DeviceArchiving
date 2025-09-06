using System;
using System.Drawing;
using System.Windows.Forms;
using Guna.UI2.WinForms;

namespace DeviceArchiving.WindowsForm.Forms
{
    public partial class PasswordPromptForm : Form
    {
        private  Guna2TextBox _txtPassword;
        private  Guna2Button _btnConfirm;
        private  Guna2Button _btnCancel;
        private  string _message;
        public string EnteredPassword { get; private set; }

        public PasswordPromptForm(string message)
        {
            _message = message;
            Initialize();
            SetupUI();
          
        }

        private void Initialize()
        {
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.CenterParent;
            this.Size = new Size(350, 200);
            this.Text = "إدخال كلمة السر";
            this.RightToLeft = RightToLeft.Yes;
            this.Font = new Font("Segoe UI", 10);
        }

        private void SetupUI()
        {
            var lblPrompt = new Label
            {
                Text = _message,
                Dock = DockStyle.Top,
                Height = 30,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 11, FontStyle.Regular)
            };

            _txtPassword = new Guna2TextBox
            {
                PlaceholderText = "كلمة السر",
                UseSystemPasswordChar = true,
                Width = 250,
                Location = new Point((this.ClientSize.Width - 250) / 2, 60),
                Font = new Font("Segoe UI", 10),
                TextAlign = HorizontalAlignment.Center,
                BorderRadius = 5
            };
            _txtPassword.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    ConfirmPassword();
                }
            };

            _btnConfirm = new Guna2Button
            {
                Text = "تأكيد",
                Width = 100,
                Location = new Point((this.ClientSize.Width - 210) / 2, 110),
                FillColor = Color.FromArgb(94, 148, 255),
                ForeColor = Color.White,
                BorderRadius = 5
            };
            _btnConfirm.Click += (s, e) => ConfirmPassword();

            _btnCancel = new Guna2Button
            {
                Text = "إلغاء",
                Width = 100,
                Location = new Point((this.ClientSize.Width + 10) / 2, 110),
                FillColor = Color.Gray,
                ForeColor = Color.White,
                BorderRadius = 5
            };
            _btnCancel.Click += (s, e) => this.DialogResult = DialogResult.Cancel;

            this.Controls.AddRange(new Control[] { lblPrompt, _txtPassword, _btnConfirm, _btnCancel });
        }

        private void ConfirmPassword()
        {
            EnteredPassword = _txtPassword.Text;
            if (string.IsNullOrWhiteSpace(EnteredPassword))
            {
                MessageBox.Show("يرجى إدخال كلمة السر", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            this.DialogResult = DialogResult.OK;
        }
    }
}