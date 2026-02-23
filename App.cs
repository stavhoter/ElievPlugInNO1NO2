using Autodesk.Revit.UI;
using System;
using System.Reflection;

namespace ElievPlugInNO1NO2
{
    public class App : IExternalApplication
    {
        public Result OnStartup(UIControlledApplication app)
        {
            try
            {
                // ✅ Change these names if you want different tab/panel names
                string tabName = "STAVELIEVPLUGIN1";
                string panelName = "Eliev Tools";

                // Create ribbon tab (ignore if already exists)
                try { app.CreateRibbonTab(tabName); } catch { }

                // Create panel
                RibbonPanel panel = app.CreateRibbonPanel(tabName, panelName);

                // Path to this add-in DLL
                string assemblyPath = Assembly.GetExecutingAssembly().Location;

                // =========================
                // Existing buttons (keep yours here)
                // =========================

                // 1) Create Linear Scheme
                panel.AddItem(new PushButtonData(
                    "BTN_CreateScheme",
                    "סכמה קווית",
                    assemblyPath,
                    "ElievPlugInNO1NO2.CreateLinearSchemeCommand"));

                // 2) Components (open 2D families list)
                panel.AddItem(new PushButtonData(
                    "BTN_Components",
                    "רכיבים",
                    assemblyPath,
                    "ElievPlugInNO1NO2.ComponentsCommand"));

                // =========================
                // ✅ NEW buttons: 2D + 3D folders
                // =========================

                // 3) Schema (2D) folder
                panel.AddItem(new PushButtonData(
                    "BTN_Schema2DFolder",
                    "תיקיית סכמה\n(2D)",
                    assemblyPath,
                    "ElievPlugInNO1NO2.Schema2DFolderCommand"));

                // 4) Model (3D) folder
                panel.AddItem(new PushButtonData(
                    "BTN_Model3DFolder",
                    "תיקיית מודל\n(3D)",
                    assemblyPath,
                    "ElievPlugInNO1NO2.Model3DFolderCommand"));

                // If you still have the old "ComponentsFolderCommand" button,
                // you can remove it or keep it — it won't break anything.

                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                TaskDialog.Show("App Startup Error", ex.ToString());
                return Result.Failed;
            }
        }

        public Result OnShutdown(UIControlledApplication app)
        {
            return Result.Succeeded;
        }
    }
}