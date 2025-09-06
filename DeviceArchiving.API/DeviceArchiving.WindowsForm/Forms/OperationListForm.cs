using DeviceArchiving.Data.Dto.Devices;
using Guna.UI2.WinForms;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using DeviceArchiving.Service.OperationServices;

namespace DeviceArchiving.WindowsForm.Forms
{
    public partial class OperationListForm : Form
    {
        private readonly IOperationService _operationService;
        private List<OperationDto> _operations = new List<OperationDto>();
        private readonly int _deviceId;

        private Guna2DataGridView dataGridViewOperations;
        private Guna2Button btnClose;

        public OperationListForm(IOperationService operationService, int deviceId)
        {
            _operationService = operationService;
            _deviceId = deviceId;

            this.RightToLeft = RightToLeft.Yes;
            this.RightToLeftLayout = true;

            SetupUI();
            LoadOperations();
        }

        private void SetupUI()
        {
            this.Text = "العمليات";
            this.Size = new Size(900, 600);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.BackColor = Color.White;

            dataGridViewOperations = new Guna2DataGridView
            {
                Location = new Point(20, 20),
                Size = new Size(this.ClientSize.Width - 40, this.ClientSize.Height - 100),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom,
                RightToLeft = RightToLeft.Yes,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                RowHeadersVisible = false,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                Font = new Font("Segoe UI", 10),
                BorderStyle = BorderStyle.None,
                BackgroundColor = Color.White,
                ColumnHeadersHeight = 40,
                ColumnHeadersDefaultCellStyle = { BackColor = Color.FromArgb(45, 204, 112), ForeColor = Color.White, Font = new Font("Segoe UI", 10, FontStyle.Bold) },
                EnableHeadersVisualStyles = false,
            };

            dataGridViewOperations.Columns.Add("OperationName", "اسم العملية");
            dataGridViewOperations.Columns.Add("OldValue", "القيمة القديمة");
            dataGridViewOperations.Columns.Add("NewValue", "القيمة الجديدة");
            dataGridViewOperations.Columns.Add("Comment", "ملاحظة");
            dataGridViewOperations.Columns.Add("UserName", "تم بواسطة");
            dataGridViewOperations.Columns.Add("CreatedAt", "تاريخ الإنشاء");

            btnClose = new Guna2Button
            {
                Text = "إغلاق",
                Location = new Point(this.ClientSize.Width - 120, this.ClientSize.Height - 60),
                Size = new Size(100, 35),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right,
                FillColor = Color.FromArgb(220, 53, 69),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                BorderRadius = 8,
                ForeColor = Color.White,
            };
            btnClose.Click += BtnClose_Click;

            this.Controls.Add(dataGridViewOperations);
            this.Controls.Add(btnClose);

            // عند تغيير حجم النافذة نحدث حجم DataGridView وموضع زر الإغلاق
            this.Resize += (s, e) =>
            {
                dataGridViewOperations.Size = new Size(this.ClientSize.Width - 40, this.ClientSize.Height - 100);
                btnClose.Location = new Point(this.ClientSize.Width - 120, this.ClientSize.Height - 60);
            };
        }

        private async void LoadOperations()
        {
            try
            {
                var operations = await _operationService.GetAllOperations(_deviceId);

                if (operations == null || !operations.Any())
                {
                    return;
                }

                _operations = operations.Select(d => new OperationDto
                {
                    Comment = d.Comment,
                    UserName = d.User?.UserName ?? "غير معروف",
                    NewValue = d.NewValue,
                    OldValue = d.OldValue,
                    CreatedAt = d.CreatedAt,
                    OperationName = d.OperationName,
                }).ToList();

                UpdateDataGridView();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ أثناء جلب العمليات: {ex.Message}", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void UpdateDataGridView()
        {
            dataGridViewOperations.Rows.Clear();
            foreach (var operation in _operations)
            {
                dataGridViewOperations.Rows.Add(
                    operation.OperationName ?? "-",
                    operation.OldValue ?? "-",
                    operation.NewValue ?? "-",
                    operation.Comment ?? "-",
                    operation.UserName ?? "-",
                    operation.CreatedAt.ToString("g") ?? "-"
                );
            }
            if (_operations.Count == 0)
            {
                dataGridViewOperations.Rows.Add("لا توجد عمليات متاحة", "-", "-", "-", "-", "-");
            }
        }

        private void BtnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
