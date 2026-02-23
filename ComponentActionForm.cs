using System;
using System.Windows.Forms;

namespace ElievPlugInNO1NO2
{
    public enum ComponentAction
    {
        None = 0,
        AddToSchema = 1,
        AddProperty = 2,
        Attach3DTwin = 3,
        Exit = 4
    }

    public class ComponentActionForm : Form
    {
        private readonly Button _btnAddToSchema;
        private readonly Button _btnAddProperty;
        private readonly Button _btnAttach3D;
        private readonly Button _btnExit;

        public ComponentAction SelectedAction { get; private set; } = ComponentAction.None;

        public ComponentActionForm(string componentDisplayName)
        {
            // Window setup
            Text = componentDisplayName; // title = "שם הרכיב"
            Width = 520;
            Height = 280;
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;

            RightToLeft = RightToLeft.Yes;
            RightToLeftLayout = true;

            var titleLabel = new Label
            {
                Dock = DockStyle.Top,
                Height = 45,
                TextAlign = System.Drawing.ContentAlignment.MiddleCenter,
                Text = $"רכיב: {componentDisplayName}"
            };

            _btnAddToSchema = new Button
            {
                Text = "1) הוסף רכיב לסכמה",
                Dock = DockStyle.Top,
                Height = 40
            };
            _btnAddToSchema.Click += (s, e) => Choose(ComponentAction.AddToSchema);

            _btnAddProperty = new Button
            {
                Text = "2) הוסף תכונה לרכיב",
                Dock = DockStyle.Top,
                Height = 40
            };
            _btnAddProperty.Click += (s, e) => Choose(ComponentAction.AddProperty);

            _btnAttach3D = new Button
            {
                Text = "3) הצמד אלמנט תלת מימדי לרכיב",
                Dock = DockStyle.Top,
                Height = 40
            };
            _btnAttach3D.Click += (s, e) => Choose(ComponentAction.Attach3DTwin);

            _btnExit = new Button
            {
                Text = "4) צא",
                Dock = DockStyle.Top,
                Height = 40,
                DialogResult = DialogResult.Cancel
            };
            _btnExit.Click += (s, e) => Choose(ComponentAction.Exit);

            var panel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(12)
            };

            // Add buttons in reverse order because DockStyle.Top stacks last-added at top
            panel.Controls.Add(_btnExit);
            panel.Controls.Add(_btnAttach3D);
            panel.Controls.Add(_btnAddProperty);
            panel.Controls.Add(_btnAddToSchema);

            Controls.Add(panel);
            Controls.Add(titleLabel);

            CancelButton = _btnExit; // ESC = Exit
        }

        private void Choose(ComponentAction action)
        {
            SelectedAction = action;
            DialogResult = (action == ComponentAction.Exit) ? DialogResult.Cancel : DialogResult.OK;
            Close();
        }
    }
}