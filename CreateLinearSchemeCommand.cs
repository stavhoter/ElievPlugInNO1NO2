using System;
using System.Windows.Forms;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;


namespace ElievPlugInNO1NO2
{
    [Transaction(TransactionMode.Manual)]
    public class CreateLinearSchemeCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            try
            {
                string defaultName = "סכמה קווית";
                string nameFromUser = PromptForName(defaultName);

                // If user pressed Cancel or left it empty, use default name
                if (string.IsNullOrWhiteSpace(nameFromUser))
                    nameFromUser = defaultName;

                CreatingLinerShceme.Create(commandData, nameFromUser);
                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                message = ex.Message;
                return Result.Failed;
            }
        }

        private static string PromptForName(string defaultValue)
        {
            // Simple WinForms dialog to ask for a name
            using (System.Windows.Forms.Form form = new System.Windows.Forms.Form())
            using (System.Windows.Forms.TextBox textBox = new System.Windows.Forms.TextBox())
            using (Button okButton = new Button())
            using (Button cancelButton = new Button())
            {
                form.Text = "שם הסכמה הקווית";
                form.FormBorderStyle = FormBorderStyle.FixedDialog;
                form.StartPosition = FormStartPosition.CenterScreen;
                form.MinimizeBox = false;
                form.MaximizeBox = false;
                form.ShowInTaskbar = false;
                form.Width = 420;
                form.Height = 150;

                textBox.Left = 15;
                textBox.Top = 15;
                textBox.Width = 370;
                textBox.Text = defaultValue;

                okButton.Text = "OK";
                okButton.Left = 230;
                okButton.Top = 55;
                okButton.Width = 75;
                okButton.DialogResult = DialogResult.OK;

                cancelButton.Text = "Cancel";
                cancelButton.Left = 310;
                cancelButton.Top = 55;
                cancelButton.Width = 75;
                cancelButton.DialogResult = DialogResult.Cancel;

                form.Controls.Add(textBox);
                form.Controls.Add(okButton);
                form.Controls.Add(cancelButton);

                form.AcceptButton = okButton;
                form.CancelButton = cancelButton;

                DialogResult result = form.ShowDialog();
                return result == DialogResult.OK ? textBox.Text : null;
            }
        }
    }
}
