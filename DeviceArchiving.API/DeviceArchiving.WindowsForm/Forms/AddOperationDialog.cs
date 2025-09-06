using DeviceArchiving.Data.Dto;
using DeviceArchiving.Data.Dto.Operations;
using DeviceArchiving.Data.Entities;
using DeviceArchiving.Service.OperationServices;
using DeviceArchiving.Service.OperationTypeServices;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DeviceArchiving.WindowsForm.Forms
{
    public partial class AddOperationDialog : Form
    {
        private readonly IOperationTypeService _operationTypeService;
        private readonly IOperationService _operationService;
        private readonly int _deviceId;
        private readonly string _laptopName;
        private Operation _operation = new Operation { OperationName = "", OldValue = null, NewValue = null, Comment = null };
        private List<OperationType> _operationTypes = new List<OperationType>();

        public AddOperationDialog(IOperationTypeService operationTypeService, IOperationService operationService, int deviceId, string laptopName)
        {
            _operationTypeService = operationTypeService;
            _operationService = operationService;
            _deviceId = deviceId;
            _laptopName = laptopName;
            this.RightToLeft = RightToLeft.Yes;
            this.RightToLeftLayout = true;
            SetupUI();
            LoadOperationTypes();
        }

        private void SetupUI()
        {
            this.Text = $"إضافة عملية لجهاز: {_laptopName}";
            this.Size = new Size(450, 450);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            // Create labels and controls
            var lblOperationType = CreateLabel("نوع العملية", true, new Point(40, 30));
            var cmbOperationType = CreateComboBox(new Point(160, 30), "cmbOperationType");

            var lblOldValue = CreateLabel("القيمة القديمة", false, new Point(40, 80));
            var txtOldValue = CreateTextBox(new Point(160, 80), "txtOldValue");

            var lblNewValue = CreateLabel("القيمة الجديدة", false, new Point(40, 130));
            var txtNewValue = CreateTextBox(new Point(160, 130), "txtNewValue");

            var lblComment = CreateLabel("التعليق", false, new Point(40, 180));
            var txtComment = CreateMultilineTextBox(new Point(160, 180), "txtComment");

            var btnSave = CreateButton("حفظ", new Point(160, 270));
            var btnCancel = CreateButton("إلغاء", new Point(265, 270));

            // Set event handlers
            btnSave.Click += BtnSave_Click;
            btnCancel.Click += BtnCancel_Click;

            // Add controls to the form
            this.Controls.AddRange(new Control[]
            {
                lblOperationType, cmbOperationType,
                lblOldValue, txtOldValue,
                lblNewValue, txtNewValue,
                lblComment, txtComment,
                btnSave,
                btnCancel
            });
        }

        private Guna.UI2.WinForms.Guna2HtmlLabel CreateLabel(string text, bool isRequired, Point location)
        {
            string labelText = text + (isRequired == false ? " - اختياري" : "");

            return new Guna.UI2.WinForms.Guna2HtmlLabel
            {
                Text = labelText,
                Location = location,
                AutoSize = true
            };
        }

        private Guna.UI2.WinForms.Guna2ComboBox CreateComboBox(Point location, string name)
        {

            return new Guna.UI2.WinForms.Guna2ComboBox
            {
                Location = location,
                Width = 200,
                Name = name,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
        }

        private Guna.UI2.WinForms.Guna2TextBox CreateTextBox(Point location, string name)
        {
            return new Guna.UI2.WinForms.Guna2TextBox
            {
                Location = location,
                Width = 200,
                Name = name
            };
        }

        private Guna.UI2.WinForms.Guna2TextBox CreateMultilineTextBox(Point location, string name)
        {
            return new Guna.UI2.WinForms.Guna2TextBox
            {
                Location = location,
                Width = 200,
                Height = 60,
                Name = name,
                Multiline = true
            };
        }

        private Guna.UI2.WinForms.Guna2Button CreateButton(string text, Point location)
        {
            return new Guna.UI2.WinForms.Guna2Button
            {
                Text = text,
                Location = location,
                Width = 100
            };
        }


        private async Task LoadOperationTypes()
        {
            try
            {
                var operationTypes = await _operationTypeService.GetAllOperationsTypes(null);
                _operationTypes = operationTypes.ToList();

                var cmbOperationType = this.Controls["cmbOperationType"] as ComboBox;
                if (cmbOperationType != null)
                {
                    cmbOperationType.Items.Clear();
                    cmbOperationType.Items.AddRange(_operationTypes.Select(t => t.Name).ToArray());
                    if (_operationTypes.Any())
                    {
                        cmbOperationType.SelectedIndex = 0;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ أثناء تحميل أنواع العمليات: {ex.Message}", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            var cmbOperationType = this.Controls["cmbOperationType"] as Guna.UI2.WinForms.Guna2ComboBox;
            var txtOldValue = this.Controls["txtOldValue"] as Guna.UI2.WinForms.Guna2TextBox;
            var txtNewValue = this.Controls["txtNewValue"] as Guna.UI2.WinForms.Guna2TextBox;
            var txtComment = this.Controls["txtComment"] as Guna.UI2.WinForms.Guna2TextBox;

            if (cmbOperationType?.SelectedItem == null)
            {
                MessageBox.Show("يرجى اختيار نوع العملية", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (_operation == null)
            {
                _operation = new Operation();
            }

            _operation.OperationName = cmbOperationType.SelectedItem.ToString();
            _operation.OldValue = string.IsNullOrWhiteSpace(txtOldValue?.Text) ? string.Empty : txtOldValue.Text;
            _operation.NewValue = string.IsNullOrWhiteSpace(txtNewValue?.Text) ? string.Empty : txtNewValue.Text;
            _operation.Comment = string.IsNullOrWhiteSpace(txtComment?.Text) ? string.Empty : txtComment.Text;

            var createOperation = new CreateOperation()
            {
                DeviceId = _deviceId,
                NewValue = _operation.NewValue,
                OldValue = _operation.OldValue,
                OperationName = _operation.OperationName,
                Comment = _operation.Comment,

            };

            _operationService.AddOperations(createOperation);

            this.DialogResult = DialogResult.OK;
            this.Close();
        }


        private void BtnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        // خاصية لاسترجاع العملية بعد الحفظ
        public Operation Operation => _operation;
    }
}