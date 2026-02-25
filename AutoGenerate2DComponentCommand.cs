using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;

namespace ElievPlugInNO1NO2
{
    [Transaction(TransactionMode.Manual)]
    public class AutoGenerate2DComponentCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;

            try
            {
                // 1. Select a 3D element from the model
                Reference selRef = uidoc.Selection.PickObject(ObjectType.Element, "Select a 3D element to create a 2D projection");
                Element elem = doc.GetElement(selRef);

                // 2. Extract geometry
                Options geomOptions = new Options();
                geomOptions.ComputeReferences = true;
                geomOptions.IncludeNonVisibleObjects = false;
                geomOptions.DetailLevel = ViewDetailLevel.Fine;

                GeometryElement geomElem = elem.get_Geometry(geomOptions);
                if (geomElem == null)
                {
                    TaskDialog.Show("Error", "Selected element has no geometry.");
                    return Result.Failed;
                }

                List<Curve> projectedCurves = ExtractAndProjectGeometry(geomElem);

                if (projectedCurves.Count == 0)
                {
                    TaskDialog.Show("Error", "Could not extract any valid lines for projection.");
                    return Result.Failed;
                }

                // 3. Create a new Detail Component Family Document
                // IMPORTANT: Adjust this path to match your Revit installation (e.g. RVT 2024, RVT 2023)
                string templatePath = @"C:\ProgramData\Autodesk\RVT 2024\Family Templates\English\Metric Detail Item.rft";

                if (!File.Exists(templatePath))
                {
                    TaskDialog.Show("Error", $"Family template not found at:\n{templatePath}\nPlease adjust the path in the code.");
                    return Result.Failed;
                }

                Document familyDoc = commandData.Application.Application.NewFamilyDocument(templatePath);

                if (familyDoc == null)
                {
                    TaskDialog.Show("Error", "Failed to create a new family document.");
                    return Result.Failed;
                }

                // 4. Draw the curves in the new Family Document
                using (Transaction t = new Transaction(familyDoc, "Create 2D Projection"))
                {
                    t.Start();

                    // Get the placement view (usually the active view in a new family)
                    View familyView = familyDoc.ActiveView ?? new FilteredElementCollector(familyDoc).OfClass(typeof(View)).Cast<View>().FirstOrDefault(v => v.ViewType == ViewType.FloorPlan || v.ViewType == ViewType.EngineeringPlan);

                    foreach (Curve curve in projectedCurves)
                    {
                        try
                        {
                            familyDoc.FamilyCreate.NewDetailCurve(familyView, curve);
                        }
                        catch
                        {
                            // Some extremely short curves might throw an exception, safely ignore
                        }
                    }

                    t.Commit();
                }

                // 5. Save the new Detail Component to the user's Desktop
                string saveFolder = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                string savePath = Path.Combine(saveFolder, $"{elem.Name}_2D_Projection.rfa");

                SaveAsOptions saveOptions = new SaveAsOptions { OverwriteExistingFile = true };
                familyDoc.SaveAs(savePath, saveOptions);
                familyDoc.Close(false);

                TaskDialog.Show("Success", $"2D Detail Component generated successfully!\nSaved to: {savePath}");

                return Result.Succeeded;
            }
            catch (Autodesk.Revit.Exceptions.OperationCanceledException)
            {
                return Result.Cancelled; // User pressed ESC
            }
            catch (Exception ex)
            {
                message = ex.Message;
                return Result.Failed;
            }
        }

        // Helper method to extract edges and project them to a 2D plane (Side Projection: Y,Z -> X,Y)
        private List<Curve> ExtractAndProjectGeometry(GeometryElement geomElem)
        {
            List<Curve> lines = new List<Curve>();

            Stack<GeometryObject> geomObjects = new Stack<GeometryObject>();
            foreach (GeometryObject go in geomElem) geomObjects.Push(go);

            while (geomObjects.Count > 0)
            {
                GeometryObject go = geomObjects.Pop();

                if (go is GeometryInstance geomInstance)
                {
                    GeometryElement instGeom = geomInstance.GetInstanceGeometry();
                    foreach (GeometryObject instObj in instGeom) geomObjects.Push(instObj);
                }
                else if (go is Solid solid && solid.Faces.Size > 0)
                {
                    foreach (Edge edge in solid.Edges)
                    {
                        Curve curve = edge.AsCurve();
                        Curve projected = ProjectCurveToSidePlane(curve);
                        if (projected != null)
                        {
                            lines.Add(projected);
                        }
                    }
                }
            }

            return lines;
        }

        // Projects a 3D curve onto the Side plane (YZ plane) and maps it to the XY plane for the Detail Component
        private Curve ProjectCurveToSidePlane(Curve curve)
        {
            if (curve is Line line)
            {
                XYZ p1 = ProjectPoint(line.GetEndPoint(0));
                XYZ p2 = ProjectPoint(line.GetEndPoint(1));

                // Don't create a line if points are too close (Revit limit is approx ~1mm)
                if (p1.DistanceTo(p2) > 0.005)
                {
                    return Line.CreateBound(p1, p2);
                }
            }
            // Note: Arcs/Splines are ignored in this simple POC to avoid complex tessellation.
            return null;
        }

        // Flattens the X axis. Y becomes X, Z becomes Y.
        private XYZ ProjectPoint(XYZ pt)
        {
            // Map 3D Y-axis to 2D X-axis, and 3D Z-axis to 2D Y-axis
            return new XYZ(pt.Y, pt.Z, 0);
        }
    }
}