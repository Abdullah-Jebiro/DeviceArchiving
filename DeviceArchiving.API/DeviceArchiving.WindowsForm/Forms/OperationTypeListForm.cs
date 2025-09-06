using DeviceArchiving.Data.Entities;
using DeviceArchiving.Service.OperationTypeServices;
using Guna.UI2.WinForms;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DeviceArchiving.WindowsForm.Forms
{
    public partial class OperationTypeListForm : Form
    {
        private readonly IOperationTypeService _operationTypeService;
        private List<OperationType> _operationTypes = new List<OperationType>();
        private string _searchTerm = "";

        private Guna2DataGridView dataGridViewOperationTypes;
        private Guna2TextBox txtSearch;
        private Guna2Button btnClear;
        private Guna2Button btnAddOperationType;
        private ToolTip toolTip;

        public OperationTypeListForm(IOperationTypeService operationTypeService)
        {
            _operationTypeService = operationTypeService;
            this.RightToLeft = RightToLeft.Yes;
            this.RightToLeftLayout = true;
            SetupUI();
            LoadOperationTypes();
        }

        private void SetupUI()
        {
            this.Text = "قائمة أنواع العمليات";
            this.Size = new Size(800, 600);
            this.StartPosition = FormStartPosition.CenterScreen;

            // ToolTip
            toolTip = new ToolTip();

            // Header Label
            var lblHeader = new Guna2HtmlLabel
            {
                Text = "قائمة أنواع العمليات",
                Location = new Point(20, 20),
                AutoSize = true,
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = Color.FromArgb(45, 52, 54)
            };

            // Search Label
            var lblSearch = new Guna2HtmlLabel
            {
                Text = "البحث في أنواع العمليات",
                Location = new Point(20, 70),
                AutoSize = true,
                ForeColor = Color.FromArgb(45, 52, 54),
                Font = new Font("Segoe UI", 10, FontStyle.Regular)
            };

            // Search TextBox
            txtSearch = new Guna2TextBox
            {
                Location = new Point(170, 65),
                Width = 300,
                PlaceholderText = "ابحث هنا...",
                RightToLeft = RightToLeft.Yes,
                Font = new Font("Segoe UI", 10)
            };
            txtSearch.TextChanged += (s, e) =>
            {
                _searchTerm = txtSearch.Text;
                LoadOperationTypes();
            };
            toolTip.SetToolTip(txtSearch, "ابحث في أنواع العمليات");

            // Clear Button
            btnClear = new Guna2Button
            {
                Text = "مسح",
                Location = new Point(480, 63),
                Size = new Size(80, 30),
                FillColor = Color.FromArgb(214, 48, 49),
                ForeColor = Color.White,
                BorderRadius = 5,
                Font = new Font("Segoe UI", 9, FontStyle.Bold)
            };
            btnClear.Click += (s, e) =>
            {
                txtSearch.Text = "";
                _searchTerm = "";
                LoadOperationTypes();
            };
            toolTip.SetToolTip(btnClear, "مسح مربع البحث");

            // Add OperationType Button
            btnAddOperationType = new Guna2Button
            {
                Text = "إضافة نوع عملية",
                Size = new Size(140, 35),
                Location = new Point(this.ClientSize.Width - 200, 110), // مع حشو 20px
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                FillColor = Color.FromArgb(34, 177, 76), // أخضر هادئ
                ForeColor = Color.White,
                BorderRadius = 5,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };
            btnAddOperationType.Click += BtnAddOperationType_Click;
            toolTip.SetToolTip(btnAddOperationType, "إضافة نوع عملية جديد");

            // Guna2DataGridView for displaying operation types
            dataGridViewOperationTypes = new Guna2DataGridView
            {
                Location = new Point(20, 160),
                Size = new Size(this.ClientSize.Width - 40, this.ClientSize.Height - 200),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                RightToLeft = RightToLeft.Yes,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                RowHeadersVisible = false,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                BorderStyle = BorderStyle.FixedSingle,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                ColumnHeadersHeight = 40,
                Font = new Font("Segoe UI", 10),
            };

            // Columns
            dataGridViewOperationTypes.Columns.Clear();

            var colName = new DataGridViewTextBoxColumn
            {
                Name = "Name",
                HeaderText = "الاسم",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter }
            };
            dataGridViewOperationTypes.Columns.Add(colName);

            var editColumn = new DataGridViewButtonColumn
            {
                Name = "Edit",
                HeaderText = "تعديل",
                Text = "تعديل",
                UseColumnTextForButtonValue = true,
                Width = 80,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter }
            };
            dataGridViewOperationTypes.Columns.Add(editColumn);

            var deleteColumn = new DataGridViewButtonColumn
            {
                Name = "Delete",
                HeaderText = "حذف",
                Text = "حذف",
                UseColumnTextForButtonValue = true,
                Width = 80,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter }
            };
            dataGridViewOperationTypes.Columns.Add(deleteColumn);

            dataGridViewOperationTypes.CellClick += DataGridViewOperationTypes_CellClick;
            dataGridViewOperationTypes.CellPainting += DataGridViewOperationTypes_CellPainting;

            // Add controls
            this.Controls.AddRange(new Control[] {
                lblHeader,
                lblSearch,
                txtSearch,
                btnClear,
                btnAddOperationType,
                dataGridViewOperationTypes
            });

            this.Resize += (s, e) =>
            {
                dataGridViewOperationTypes.Size = new Size(this.ClientSize.Width - 40, this.ClientSize.Height - 200);
                btnAddOperationType.Location = new Point(this.ClientSize.Width - 200, 110);
            };
        }

        private async Task LoadOperationTypes()
        {
            try
            {
                _operationTypes = (await _operationTypeService.GetAllOperationsTypes(_searchTerm)).ToList();
                UpdateDataGridView();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ أثناء تحميل أنواع العمليات: {ex.Message}", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void UpdateDataGridView()
        {
            dataGridViewOperationTypes.Rows.Clear();
            if (_operationTypes.Count == 0)
            {
                dataGridViewOperationTypes.Rows.Add("لا توجد أنواع عمليات متاحة", "", "");
                var row = dataGridViewOperationTypes.Rows[0];
                row.DefaultCellStyle.ForeColor = Color.Gray;
                row.DefaultCellStyle.Font = new Font(dataGridViewOperationTypes.Font, FontStyle.Italic);
                row.ReadOnly = true;

                // تعطيل أزرار التعديل والحذف لهذا الصف
                row.Cells["Edit"].ReadOnly = true;
                row.Cells["Delete"].ReadOnly = true;
                return;
            }

            foreach (var type in _operationTypes)
            {
                dataGridViewOperationTypes.Rows.Add(type.Name, "تعديل", "حذف");
            }
        }

        private void BtnAddOperationType_Click(object sender, EventArgs e)
        {
            using (var form = new OperationTypeForm(_operationTypeService))
            {
                if (form.ShowDialog() == DialogResult.OK)
                {
                    LoadOperationTypes();
                }
            }
        }

        private void DataGridViewOperationTypes_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.RowIndex >= _operationTypes.Count) return;

            var selectedType = _operationTypes[e.RowIndex];

            if (dataGridViewOperationTypes.Columns[e.ColumnIndex].Name == "Edit")
            {
                using (var form = new OperationTypeForm(_operationTypeService, selectedType))
                {
                    if (form.ShowDialog() == DialogResult.OK)
                    {
                        LoadOperationTypes();
                    }
                }
            }
            else if (dataGridViewOperationTypes.Columns[e.ColumnIndex].Name == "Delete")
            {
                var confirm = MessageBox.Show("هل أنت متأكد من حذف نوع العملية؟", "تأكيد الحذف", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (confirm == DialogResult.Yes)
                {
                    try
                    {
                        _operationTypeService.DeleteOperationType(selectedType.Id);
                        MessageBox.Show("تم حذف نوع العملية بنجاح.", "نجاح", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        LoadOperationTypes();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"خطأ أثناء الحذف: {ex.Message}", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void DataGridViewOperationTypes_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex < 0) return;

            var dgv = sender as DataGridView;

            if (dgv.Columns[e.ColumnIndex].Name == "Edit" && e.RowIndex < _operationTypes.Count)
            {
                e.PaintBackground(e.CellBounds, true);
                TextRenderer.DrawText(e.Graphics, "✏️", e.CellStyle.Font, e.CellBounds, Color.Black, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                e.Handled = true;
            }
            else if (dgv.Columns[e.ColumnIndex].Name == "Delete" && e.RowIndex < _operationTypes.Count)
            {
                e.PaintBackground(e.CellBounds, true);
                TextRenderer.DrawText(e.Graphics, "🗑️", e.CellStyle.Font, e.CellBounds, Color.Black, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                e.Handled = true;
            }
        }

    }
}
