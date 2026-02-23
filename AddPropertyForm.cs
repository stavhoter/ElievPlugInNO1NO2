using System;
using System.Windows.Forms;

namespace ElievPlugInNO1NO2
{
    public class AddPropertyForm : Form
    {
        private readonly TextBox _textBox;
        private readonly Button _okButton;
        private readonly Button _cancelButton;

        public string PropertyName { get; private set; }

        public AddPropertyForm()
        {
            Text = "שם התכונה";
            Width = 520;
            Height = 200;
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;

            RightToLeft = RightToLeft.Yes;
            RightToLeftLayout = true;

            var label = new Label
            {
                Text = "הכנס שם תכונה:",
                Dock = DockStyle.Top,
                Height = 30,
                Padding = new Padding(12, 10, 12, 0)
            };

            _textBox = new TextBox
            {
                Dock = DockStyle.Top,
                Height = 28,
                Margin = new Padding(12),
            };

            _okButton = new Button
            {
                Text = "אישור",
                Width = 110
            };
            _okButton.Click += (s, e) => Confirm();

            _cancelButton = new Button
            {
                Text = "ביטול",
                Width = 110,
                DialogResult = DialogResult.Cancel
            };

            var buttonsPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Bottom,
                Height = 48,
                FlowDirection = FlowDirection.RightToLeft,
                Padding = new Padding(12, 8, 12, 8)
            };

            buttonsPanel.Controls.Add(_cancelButton);
            buttonsPanel.Controls.Add(_okButton);

            var bodyPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(12)
            };

            bodyPanel.Controls.Add(_textBox);
            bodyPanel.Controls.Add(label);

            Controls.Add(bodyPanel);
            Controls.Add(buttonsPanel);

            AcceptButton = _okButton;
            CancelButton = _cancelButton;
        }

        private void Confirm()
        {
            string value = (_textBox.Text ?? string.Empty).Trim();

            if (string.IsNullOrWhiteSpace(value))
            {
                MessageBox.Show("אנא הזן שם תכונה.", "שגיאה", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            PropertyName = value;
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}