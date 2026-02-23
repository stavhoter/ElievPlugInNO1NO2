using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace ElievPlugInNO1NO2
{
    [Transaction(TransactionMode.Manual)]
    public class Main : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            MyForm form = new MyForm();
            var result = form.ShowDialog();

            if (result != System.Windows.Forms.DialogResult.OK)
                return Result.Cancelled;

            string viewName = form.ViewName;

            try
            {
                CreatingLinerShceme.Create(commandData, viewName);
                return Result.Succeeded;
            }
            catch (System.Exception ex)
            {
                TaskDialog.Show("Error", ex.Message);
                return Result.Failed;
            }
        }
    }
}
