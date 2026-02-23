using System;
using System.Windows.Forms;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace ElievPlugInNO1NO2
{
    [Transaction(TransactionMode.Manual)]
    public class Model3DFolderCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            try
            {
                using (var dialog = new FolderBrowserDialog())
                {
                    dialog.Description = "בחר תיקייה עבור רכיבי מודל (3D)";
                    dialog.ShowNewFolderButton = true;

                    if (dialog.ShowDialog() != DialogResult.OK)
                        return Result.Cancelled;

                    PluginSettings.SaveModel3DFolder(dialog.SelectedPath);
                    TaskDialog.Show("תיקיית מודל (3D)", dialog.SelectedPath);
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