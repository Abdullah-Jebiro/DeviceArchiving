using DeviceArchiving.Data.Entities;
using DeviceArchiving.Service.OperationTypeServices;
using Guna.UI2.WinForms;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace DeviceArchiving.WindowsForm.Forms
{
    public partial class OperationTypeForm : Form
    {
        private readonly IOperationTypeService _operationTypeService;
        private OperationType _operationType;
        private bool _isEditMode;

        private Guna2TextBox txtName;
        private Guna2Button btnSave;
        private Guna2Button btnCancel;
        private ErrorProvider errorProvider;

        public OperationTypeForm(IOperationTypeService operationTypeService, OperationType operationType = null)
        {
            _operationTypeService = operationTypeService;
            _operationType = operationType ?? new OperationType();
            _isEditMode = operationType != null;

            this.RightToLeft = RightToLeft.Yes;
            this.RightToLeftLayout = true;

            InitializeComponent();
            SetupUI();
        }

        private void SetupUI()
        {
            this.Text = _isEditMode ? "تعديل نوع العملية" : "إضافة نوع العملية";
            this.Size = new Size(420, 220);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.BackColor = Color.White;

            errorProvider = new ErrorProvider();

            // Label
            Label lblName = new Label
            {
                Text = "الاسم: *",
                Location = new Point(50, 40),
                AutoSize = true,
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.Black
            };

            // Guna2 TextBox
            txtName = new Guna2TextBox
            {
                Location = new Point(150, 35),
                Width = 200,
                Name = "txtName",
                Text = _operationType.Name ?? "",
                PlaceholderText = "أدخل اسم نوع العملية",
                BorderRadius = 5,
                Font = new Font("Segoe UI", 10),
                RightToLeft = RightToLeft.Yes
            };

            // Save Button
            btnSave = new Guna2Button
            {
                Text = "حفظ",
                Location = new Point(150, 100),
                Width = 100,
                Height = 40,
                FillColor = Color.FromArgb(45, 204, 112),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                BorderRadius = 8
            };
            btnSave.Click += BtnSave_Click;

            // Cancel Button
            btnCancel = new Guna2Button
            {
                Text = "إلغاء",
                Location = new Point(260, 100),
                Width = 100,
                Height = 40,
                FillColor = Color.Gray,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                BorderRadius = 8
            };
            btnCancel.Click += (s, e) => { this.DialogResult = DialogResult.Cancel; this.Close(); };

            this.AcceptButton = btnSave;
            this.CancelButton = btnCancel;

            this.Controls.AddRange(new Control[] { lblName, txtName, btnSave, btnCancel });

            // فوكس تلقائي
            this.Load += (s, e) => txtName.Focus();
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            errorProvider.Clear();

            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                errorProvider.SetError(txtName, "الاسم مطلوب.");
                return;
            }

            try
            {
                _operationType.Name = txtName.Text.Trim();

                if (_isEditMode)
                {
                    _operationTypeService.UpdateOperationType(_operationType);
                    MessageBox.Show("تم تحديث نوع العملية بنجاح.", "نجاح", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    _operationTypeService.AddOperationType(_operationType);
                    MessageBox.Show("تم إضافة نوع العملية بنجاح.", "نجاح", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"حدث خطأ أثناء الحفظ: {ex.Message}", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
