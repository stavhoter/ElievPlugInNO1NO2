using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace ElievPlugInNO1NO2
{
    [Transaction(TransactionMode.Manual)]
    public class ComponentsCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            try
            {
                var uiapp = commandData.Application;
                var uidoc = uiapp.ActiveUIDocument;
                var doc = uidoc.Document;

                // 1) Load 3D model folder (changed from 2D components folder)
                string folderPath = PluginSettings.LoadModel3DFolder();
                if (string.IsNullOrWhiteSpace(folderPath) || !Directory.Exists(folderPath))
                {
                    TaskDialog.Show("רכיבים",
                        "לא הוגדרה תיקיית מודל 3D או שהתיקייה לא קיימת.\n\nגש ל'תיקיית מודל (3D)' ובחר תיקייה תקינה.");
                    return Result.Cancelled;
                }

                // 2) Collect all .rfa files
                List<string> familyFullPaths = Directory
                    .GetFiles(folderPath, "*.rfa", SearchOption.TopDirectoryOnly)
                    .OrderBy(p => p, StringComparer.OrdinalIgnoreCase)
                    .ToList();

                if (familyFullPaths.Count == 0)
                {
                    TaskDialog.Show("רכיבים", "לא נמצאו קבצי .rfa בתיקייה שנבחרה.");
                    return Result.Cancelled;
                }

                // 3) Pick a family (this defines the target for all actions)
                string selectedFamilyPath;
                using (var form = new FamilyListForm(folderPath, familyFullPaths))
                {
                    if (form.ShowDialog() != DialogResult.OK)
                        return Result.Cancelled;

                    selectedFamilyPath = form.SelectedFamilyPath;
                }

                if (string.IsNullOrWhiteSpace(selectedFamilyPath))
                    return Result.Cancelled;

                // Target key = FULL PATH (as you chose)
                string familyKey = selectedFamilyPath;
                string componentName = Path.GetFileNameWithoutExtension(selectedFamilyPath);

                // 4) Open the metadata management window (table + 5 buttons)
                return RunComponentMetadataUI(uiapp, doc, familyKey, componentName);
            }
            catch (Exception ex)
            {
                TaskDialog.Show("Error", ex.ToString());
                return Result.Failed;
            }
        }

        private static Result RunComponentMetadataUI(UIApplication uiapp, Document doc, string familyKey, string componentName)
        {
            while (true)
            {
                using (var metaForm = new ComponentMetadataForm(doc, familyKey, componentName))
                {
                    var dr = metaForm.ShowDialog();

                    // Exit window
                    if (dr != DialogResult.OK || metaForm.SelectedAction == ComponentMetadataAction.Exit)
                        return Result.Succeeded;

                    switch (metaForm.SelectedAction)
                    {
                        case ComponentMetadataAction.AddProperty:
                            {
                                string propName;
                                using (var nameForm = new AddPropertyForm())
                                {
                                    if (nameForm.ShowDialog() != DialogResult.OK)
                                        break;

                                    propName = (nameForm.PropertyName ?? "").Trim();
                                    if (string.IsNullOrWhiteSpace(propName))
                                        break;
                                }

                                string propValue;
                                using (var valForm = new AddPropertyValueForm(propName))
                                {
                                    if (valForm.ShowDialog() != DialogResult.OK)
                                        break;

                                    propValue = valForm.PropertyValue ?? "";
                                }

                                using (Transaction t = new Transaction(doc, "STV - Add Property"))
                                {
                                    t.Start();
                                    StvDocumentMetadataStore.SetProperty(doc, familyKey, propName, propValue);
                                    t.Commit();
                                }

                                break;
                            }

                        case ComponentMetadataAction.UpdateProperty:
                            {
                                var row = metaForm.GetSelectedRow();
                                if (row == null)
                                {
                                    TaskDialog.Show("תכונה", "בחר תכונה מהרשימה.");
                                    break;
                                }

                                string propName = row.Value.PropertyName;
                                string currentValue = row.Value.PropertyValue;

                                string newValue;
                                using (var valForm = new AddPropertyValueForm(propName, currentValue))
                                {
                                    if (valForm.ShowDialog() != DialogResult.OK)
                                        break;

                                    newValue = valForm.PropertyValue ?? "";
                                }

                                using (Transaction t = new Transaction(doc, "STV - Update Property"))
                                {
                                    t.Start();
                                    StvDocumentMetadataStore.SetProperty(doc, familyKey, propName, newValue);
                                    t.Commit();
                                }

                                break;
                            }

                        case ComponentMetadataAction.RemoveProperty:
                            {
                                var row = metaForm.GetSelectedRow();
                                if (row == null)
                                {
                                    TaskDialog.Show("תכונה", "בחר תכונה מהרשימה.");
                                    break;
                                }

                                string propName = row.Value.PropertyName;

                                using (Transaction t = new Transaction(doc, "STV - Remove Property"))
                                {
                                    t.Start();
                                    StvDocumentMetadataStore.RemoveProperty(doc, familyKey, propName);
                                    t.Commit();
                                }

                                break;
                            }

                        case ComponentMetadataAction.AddToSchema:
                            {
                                try
                                {
                                    var schemas = SchemaViewRegistry.GetAllSchemas(doc);
                                    if (schemas.Count == 0)
                                    {
                                        TaskDialog.Show("סכמה", "לא נמצאו סכמות.\nצור קודם סכמה קווית.");
                                        break;
                                    }

                                    string schemaName;
                                    ElementId schemaViewId;

                                    using (var pick = new SchemaSelectForm(schemas))
                                    {
                                        if (pick.ShowDialog() != DialogResult.OK)
                                            break;

                                        schemaName = pick.SelectedSystemKey;
                                        schemaViewId = new ElementId(pick.SelectedViewIdInt);
                                    }

                                    if (schemaViewId == ElementId.InvalidElementId)
                                        break;

                                    // ✅ Real placement (PickPoint + Snap + Place)
                                    // Use uiapp variable, not commandData inside the case
                                    SchemaPlacer.Place2DInSchemaView(
                                        uiapp,
                                        schemaViewId,
                                        familyKey,
                                        schemaName
                                    );
                                }
                                catch (Autodesk.Revit.Exceptions.OperationCanceledException)
                                {
                                    // user canceled PickPoint
                                }
                                catch (Exception ex)
                                {
                                    TaskDialog.Show("שגיאה", ex.Message);
                                }

                                break;
                            }

                        default:
                            return Result.Cancelled;
                    }
                }
            }
        }
    }
}