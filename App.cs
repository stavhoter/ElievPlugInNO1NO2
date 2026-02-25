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
                string tabName = "STAVELIEVPLUGIN1";
                string panelName = "Eliev Tools";

                try { app.CreateRibbonTab(tabName); } catch { }

                RibbonPanel panel = app.CreateRibbonPanel(tabName, panelName);

                string assemblyPath = Assembly.GetExecutingAssembly().Location;

                // 1) Create Linear Scheme
                panel.AddItem(new PushButtonData(
                    "BTN_CreateScheme",
                    "סכמה קווית",
                    assemblyPath,
                    "ElievPlugInNO1NO2.CreateLinearSchemeCommand"));

                // 2) Components (now shows 3D families from Model folder)
                panel.AddItem(new PushButtonData(
                    "BTN_Components",
                    "רכיבים",
                    assemblyPath,
                    "ElievPlugInNO1NO2.ComponentsCommand"));

                // 3) Model (3D) folder - configure where 3D families are stored
                panel.AddItem(new PushButtonData(
                    "BTN_Model3DFolder",
                    "תיקיית מודל\n(3D)",
                    assemblyPath,
                    "ElievPlugInNO1NO2.Model3DFolderCommand"));

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