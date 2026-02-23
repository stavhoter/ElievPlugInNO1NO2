using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace ElievPlugInNO1NO2
{
    public enum ComponentMetadataAction
    {
        None = 0,
        UpdateProperty = 1,
        RemoveProperty = 2,
        AddProperty = 3,
        AddToSchema = 4,
        AttachDigitalTwin = 5,
        Exit = 6
    }

    public class ComponentMetadataForm : System.Windows.Forms.Form
    {
        private readonly DataGridView _grid;
        private readonly Button _btnUpdate;
        private readonly Button _btnRemove;
        private readonly Button _btnAdd;
        private readonly Button _btnAddToSchema;
        private readonly Button _btnAttachTwin;
        private readonly Button _btnExit;

        private readonly Document _doc;
        private readonly string _familyKey;
        private readonly string _componentName;

        private const string DigitalTwinKey = "STV.Model3DPath";
        private const string DigitalTwinDisplay = "קיים תאום דיגיטלי";

        public ComponentMetadataAction SelectedAction { get; private set; } = ComponentMetadataAction.None;

        public ComponentMetadataForm(Document doc, string familyKey, string componentName)
        {
            _doc = doc;
            _familyKey = familyKey;
            _componentName = componentName ?? "רכיב";

            Text = _componentName;
            Width = 780;
            Height = 560;
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;

            RightToLeft = RightToLeft.Yes;
            RightToLeftLayout = true;

            var titleLabel = new Label
            {
                Text = _componentName,
                Dock = DockStyle.Top,
                Height = 44,
                TextAlign = System.Drawing.ContentAlignment.MiddleCenter,
                Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold)
            };

            _grid = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AllowUserToResizeRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };

            _grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Property",
                HeaderText = "תכונה"
            });

            _grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Value",
                HeaderText = "ערך"
            });

            // Buttons
            _btnUpdate = new Button { Text = "1) עדכן תכונה", Width = 140, Height = 34 };
            _btnRemove = new Button { Text = "2) הסר תכונה", Width = 140, Height = 34 };
            _btnAdd = new Button { Text = "3) הוסף תכונה", Width = 140, Height = 34 };
            _btnAddToSchema = new Button { Text = "4) הוסף רכיב לסכמה", Width = 170, Height = 34 };
            _btnAttachTwin = new Button { Text = "5) הצמד תאום דיגיטלי", Width = 180, Height = 34 };
            _btnExit = new Button { Text = "6) צא", Width = 120, Height = 34, DialogResult = DialogResult.Cancel };

            _btnUpdate.Click += (s, e) => CloseWith(ComponentMetadataAction.UpdateProperty);
            _btnRemove.Click += (s, e) => CloseWith(ComponentMetadataAction.RemoveProperty);
            _btnAdd.Click += (s, e) => CloseWith(ComponentMetadataAction.AddProperty);
            _btnAddToSchema.Click += (s, e) => CloseWith(ComponentMetadataAction.AddToSchema);
            _btnAttachTwin.Click += (s, e) => CloseWith(ComponentMetadataAction.AttachDigitalTwin);
            _btnExit.Click += (s, e) => CloseWith(ComponentMetadataAction.Exit);

            var buttonsPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Bottom,
                Height = 56,
                FlowDirection = FlowDirection.RightToLeft,
                Padding = new Padding(12, 10, 12, 10),
                WrapContents = false,
                AutoScroll = true
            };

            buttonsPanel.Controls.Add(_btnExit);
            buttonsPanel.Controls.Add(_btnAttachTwin);
            buttonsPanel.Controls.Add(_btnAddToSchema);
            buttonsPanel.Controls.Add(_btnAdd);
            buttonsPanel.Controls.Add(_btnRemove);
            buttonsPanel.Controls.Add(_btnUpdate);

            Controls.Add(_grid);
            Controls.Add(buttonsPanel);
            Controls.Add(titleLabel);

            CancelButton = _btnExit;

            LoadGrid();
        }

        public (string PropertyName, string PropertyValue)? GetSelectedRow()
        {
            if (_grid.SelectedRows.Count == 0)
                return null;

            var row = _grid.SelectedRows[0];
            string prop = row.Cells["Property"].Value?.ToString() ?? "";
            string val = row.Cells["Value"].Value?.ToString() ?? "";

            if (prop == DigitalTwinDisplay)
                return null; // prevent editing virtual row

            if (string.IsNullOrWhiteSpace(prop))
                return null;

            return (prop, val);
        }

        public void Reload()
        {
            LoadGrid();
        }

        private void LoadGrid()
        {
            var dict = StvDocumentMetadataStore.ReadProperties(_doc, _familyKey);

            _grid.Rows.Clear();

            // Show digital twin status
            string model3DPath = dict.ContainsKey(DigitalTwinKey) ? dict[DigitalTwinKey] : "";
            bool hasTwin = !string.IsNullOrWhiteSpace(model3DPath);

            _grid.Rows.Add(DigitalTwinDisplay, hasTwin ? "כן" : "לא");

            foreach (var kv in dict
                .Where(kv => kv.Key != DigitalTwinKey)
                .OrderBy(kv => kv.Key, StringComparer.OrdinalIgnoreCase))
            {
                _grid.Rows.Add(kv.Key, kv.Value);
            }

            if (_grid.Rows.Count > 0)
                _grid.Rows[0].Selected = true;
        }

        private void CloseWith(ComponentMetadataAction action)
        {
            SelectedAction = action;
            DialogResult = (action == ComponentMetadataAction.Exit) ? DialogResult.Cancel : DialogResult.OK;
            Close();
        }
    }
}