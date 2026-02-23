using System;
using System.Windows.Forms;

namespace ElievPlugInNO1NO2
{
    public class MyForm : System.Windows.Forms.Form
    {
        private TextBox textBox;
        private Button okButton;
        private Button cancelButton;

        public string ViewName { get; private set; }

        public MyForm()
        {
            this.Text = "Create New Section View";
            this.Width = 400;
            this.Height = 160;
            this.StartPosition = FormStartPosition.CenterScreen;

            textBox = new TextBox();
            textBox.Left = 20;
            textBox.Top = 20;
            textBox.Width = 340;

            okButton = new Button();
            okButton.Text = "OK";
            okButton.Left = 100;
            okButton.Top = 60;
            okButton.Click += OkButton_Click;

            cancelButton = new Button();
            cancelButton.Text = "Cancel";
            cancelButton.Left = 200;
            cancelButton.Top = 60;
            cancelButton.Click += (s, e) =>
            {
                this.DialogResult = DialogResult.Cancel;
                this.Close();
            };

            this.Controls.Add(textBox);
            this.Controls.Add(okButton);
            this.Controls.Add(cancelButton);
        }

        private void OkButton_Click(object sender, EventArgs e)
        {
            ViewName = textBox.Text.Trim();

            if (string.IsNullOrWhiteSpace(ViewName))
            {
                MessageBox.Show("Please enter a view name.");
                return;
            }

            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}
