using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace ElievPlugInNO1NO2
{
    public class SchemaSelectForm : Form
    {
        private readonly ListBox _listBox;
        private readonly Button _okButton;
        private readonly Button _cancelButton;

        public string SelectedSystemKey { get; private set; }
        public int SelectedViewIdInt { get; private set; } = -1;

        public SchemaSelectForm(List<SchemaOption> options)
        {
            Text = "בחר סכמה";
            Width = 520;
            Height = 520;
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;

            RightToLeft = RightToLeft.Yes;
            RightToLeftLayout = true;

            var title = new Label
            {
                Text = "בחר סכמה שנוצרה",
                Dock = DockStyle.Top,
                Height = 44,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 11F, FontStyle.Bold)
            };

            _listBox = new ListBox
            {
                Dock = DockStyle.Fill
            };

            options = options ?? new List<SchemaOption>();
            _listBox.DataSource = options;
            _listBox.DisplayMember = nameof(SchemaOption.DisplayName);
            _listBox.DoubleClick += (s, e) => Confirm();

            _okButton = new Button { Text = "OK", Width = 120, Height = 34 };
            _okButton.Click += (s, e) => Confirm();

            _cancelButton = new Button { Text = "ביטול", Width = 120, Height = 34, DialogResult = DialogResult.Cancel };

            var bottom = new FlowLayoutPanel
            {
                Dock = DockStyle.Bottom,
                Height = 56,
                FlowDirection = FlowDirection.RightToLeft,
                Padding = new Padding(12, 10, 12, 10)
            };

            bottom.Controls.Add(_cancelButton);
            bottom.Controls.Add(_okButton);

            Controls.Add(_listBox);
            Controls.Add(bottom);
            Controls.Add(title);

            AcceptButton = _okButton;
            CancelButton = _cancelButton;
        }

        private void Confirm()
        {
            var selected = _listBox.SelectedItem as SchemaOption;
            if (selected == null)
            {
                MessageBox.Show("בחר סכמה מהרשימה.");
                return;
            }

            SelectedSystemKey = selected.SystemKey;
            SelectedViewIdInt = selected.ViewIdInt;

            DialogResult = DialogResult.OK;
            Close();
        }
    }

    public class SchemaOption
    {
        public string DisplayName { get; set; }
        public string SystemKey { get; set; }
        public int ViewIdInt { get; set; }

        public SchemaOption() { }

        public SchemaOption(string systemKey, int viewIdInt)
        {
            SystemKey = systemKey ?? "";
            DisplayName = SystemKey;
            ViewIdInt = viewIdInt;
        }
    }
}