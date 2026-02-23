using System;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System.Windows.Forms;

namespace ElievPlugInNO1NO2
{
    [Transaction(TransactionMode.Manual)]
    public class ComponentsFolderCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            try
            {
                using (var dialog = new FolderBrowserDialog())
                {
                    dialog.Description = "בחר תיקיית רכיבים";
                    dialog.ShowNewFolderButton = true;

                    if (dialog.ShowDialog() != DialogResult.OK)
                        return Result.Cancelled;

                    string selectedFolder = dialog.SelectedPath;

                    // ✅ כאן שומרים את הנתיב
                    PluginSettings.SaveComponentsFolder(selectedFolder);

                    TaskDialog.Show("Components Folder", $"Saved:\n{selectedFolder}");
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
