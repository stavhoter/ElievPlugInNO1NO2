using System;
using System.Windows.Forms;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace ElievPlugInNO1NO2
{
    [Transaction(TransactionMode.Manual)]
    public class Schema2DFolderCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            try
            {
                using (var dialog = new FolderBrowserDialog())
                {
                    dialog.Description = "בחר תיקייה עבור רכיבי סכמה (2D)";
                    dialog.ShowNewFolderButton = true;

                    if (dialog.ShowDialog() != DialogResult.OK)
                        return Result.Cancelled;

                    PluginSettings.SaveSchema2DFolder(dialog.SelectedPath);
                    TaskDialog.Show("תיקיית סכמה (2D)", dialog.SelectedPath);
                    return Result.Succeeded;
                }
            }
            catch (Exception ex)
            {
                TaskDialog.Show("Error", ex.ToString());
                return Result.Failed;
            }
        }
    }
}