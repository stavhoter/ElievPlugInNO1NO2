using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace ElievPlugInNO1NO2
{
    public static class AutoGenerate2DHelper
    {
        public static string GenerateDetailComponentFrom3D(UIApplication uiapp, string family3DPath)
        {
            if (!File.Exists(family3DPath))
                throw new FileNotFoundException("3D Family not found: " + family3DPath);

            string family3DDir = Path.GetDirectoryName(family3DPath);
            string family3DName = Path.GetFileNameWithoutExtension(family3DPath);
            string output2DPath = Path.Combine(family3DDir, family3DName + "_2D.rfa");

            string templatePath = @"C:\ProgramData\Autodesk\RVT 2024\Family Templates\English\Metric Detail Item.rft";
            if (!File.Exists(templatePath))
                throw new FileNotFoundException("Detail Item template not found: " + templatePath);

            Document family3DDoc = uiapp.Application.OpenDocumentFile(family3DPath);

            try
            {
                Options geomOptions = new Options
                {
                    ComputeReferences = false,
                    IncludeNonVisibleObjects = false,
                    DetailLevel = ViewDetailLevel.Fine
                };

                var lines2D = new List<(XYZ start, XYZ end)>();

                // Collect geometry from all elements in the family document
                FilteredElementCollector collector = new FilteredElementCollector(family3DDoc);
                foreach (Element elem in collector.WhereElementIsNotElementType())
                {
                    GeometryElement geomElem = elem.get_Geometry(geomOptions);
                    if (geomElem == null)
                        continue;

                    ExtractLinesFromGeometry(geomElem, lines2D);
                }

                if (lines2D.Count == 0)
                    throw new InvalidOperationException("No line geometry found in the 3D family to project.");

                family3DDoc.Close(false);

                Document detailDoc = uiapp.Application.NewFamilyDocument(templatePath);

                using (Transaction t = new Transaction(detailDoc, "Create 2D Detail Lines"))
                {
                    t.Start();

                    // Find the first valid view in the family document
                    View activeView = new FilteredElementCollector(detailDoc)
                        .OfClass(typeof(View))
                        .Cast<View>()
                        .FirstOrDefault(v => v.ViewType == ViewType.FloorPlan || v.ViewType == ViewType.Detail);

                    if (activeView == null)
                        throw new InvalidOperationException("No valid view found in the Detail Item template.");

                    // Get or create a sketch plane
                    SketchPlane sp = null;
                    FilteredElementCollector spCollector = new FilteredElementCollector(detailDoc)
                        .OfClass(typeof(SketchPlane));

                    sp = spCollector.FirstElement() as SketchPlane;

                    if (sp == null)
                    {
                        Plane plane = Plane.CreateByNormalAndOrigin(XYZ.BasisZ, XYZ.Zero);
                        sp = SketchPlane.Create(detailDoc, plane);
                    }

                    activeView.SketchPlane = sp;

                    foreach (var (start, end) in lines2D)
                    {
                        // Skip degenerate lines (start == end)
                        if (start.DistanceTo(end) < 0.001)
                            continue;

                        Line line = Line.CreateBound(start, end);
                        detailDoc.FamilyCreate.NewDetailCurve(activeView, line);
                    }

                    t.Commit();
                }

                SaveAsOptions saveOpts = new SaveAsOptions { OverwriteExistingFile = true };
                detailDoc.SaveAs(output2DPath, saveOpts);
                detailDoc.Close(false);

                return output2DPath;
            }
            catch
            {
                if (family3DDoc != null && family3DDoc.IsValidObject)
                {
                    try { family3DDoc.Close(false); } catch { }
                }
                throw;
            }
        }

        private static void ExtractLinesFromGeometry(GeometryElement geomElem, List<(XYZ start, XYZ end)> lines2D)
        {
            foreach (GeometryObject gObj in geomElem)
            {
                // Direct line
                if (gObj is Line line)
                {
                    lines2D.Add((Project2D(line.GetEndPoint(0)), Project2D(line.GetEndPoint(1))));
                }
                // Solid - extract edges
                else if (gObj is Solid solid && solid.Faces.Size > 0)
                {
                    foreach (Edge edge in solid.Edges)
                    {
                        Curve curve = edge.AsCurve();
                        if (curve is Line edgeLine)
                        {
                            lines2D.Add((Project2D(edgeLine.GetEndPoint(0)), Project2D(edgeLine.GetEndPoint(1))));
                        }
                        else if (curve is Arc arc)
                        {
                            // Approximate arc with straight line (simple projection)
                            lines2D.Add((Project2D(arc.GetEndPoint(0)), Project2D(arc.GetEndPoint(1))));
                        }
                    }
                }
                // GeometryInstance - recurse into it
                else if (gObj is GeometryInstance gInst)
                {
                    GeometryElement instGeom = gInst.GetInstanceGeometry();
                    if (instGeom != null)
                    {
                        ExtractLinesFromGeometry(instGeom, lines2D);
                    }
                }
            }
        }

        private static XYZ Project2D(XYZ pt)
        {
            // Side view projection: Y → X, Z → Y
            return new XYZ(pt.Y, pt.Z, 0);
        }
    }
}