using System;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace ElievPlugInNO1NO2
{
    public static class SchemaPlacer
    {
        private const double GridSizeMeters = 0.10; // 10 cm snap

        public static ElementId Place2DInSchemaView(
            UIApplication uiapp,
            ElementId schemaViewId,
            string schema2DRfaPath,
            string schemaName)
        {
            if (uiapp == null) throw new ArgumentNullException(nameof(uiapp));
            if (schemaViewId == null || schemaViewId == ElementId.InvalidElementId)
                throw new ArgumentException("schemaViewId is invalid.");
            if (string.IsNullOrWhiteSpace(schema2DRfaPath))
                throw new ArgumentException("schema2DRfaPath is empty.");

            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document doc = uidoc.Document;

            View schemaView = doc.GetElement(schemaViewId) as View;
            if (schemaView == null)
                throw new InvalidOperationException("Schema view not found.");

            // 1) Switch to schema view
            uidoc.ActiveView = schemaView;

            // 1.5) Ensure the view has a Work Plane before picking a point
            using (Transaction tWP = new Transaction(doc, "Set Work Plane"))
            {
                tWP.Start();
                EnsureWorkPlaneInView(doc, schemaView);
                tWP.Commit();
            }

            // 2) Load family symbol (must be Detail Component)
            FamilySymbol symbol = FamilyLoader.EnsureFirstSymbolLoaded(doc, schema2DRfaPath);
            if (symbol == null)
                throw new InvalidOperationException("Could not load FamilySymbol from: " + schema2DRfaPath);

            if (symbol.Category == null || symbol.Category.Id.IntegerValue != (int)BuiltInCategory.OST_DetailComponents)
                throw new InvalidOperationException("Selected 2D family must be a Detail Component (OST_DetailComponents).");

            // 3) Pick point (Now it won't crash because we have a Work Plane)
            XYZ picked = uidoc.Selection.PickPoint("בחר נקודה להנחת רכיב בסכמה.");

            // 4) Snap point in view plane (Right/Up)
            XYZ snapped = SnapToGridInViewPlane(schemaView, picked, GridSizeMeters, out double uSnap, out double vSnap);

            // 5) Place + write metadata
            using (Transaction t = new Transaction(doc, "STV - Place 2D in Schema"))
            {
                t.Start();

                if (!symbol.IsActive)
                    symbol.Activate();

                // Place as detail component in this view
                FamilyInstance inst = doc.Create.NewFamilyInstance(snapped, symbol, schemaView);

                // Store instance metadata
                ElementId levelId = GetViewLevelId(schemaView);
                Guid instanceGuid = Guid.NewGuid();

                StvSchemaInstanceStorage.SetInstanceData(
                    inst,
                    instanceGuid,
                    schemaName ?? "",
                    schema2DRfaPath,
                    levelId,
                    uSnap,
                    vSnap
                );

                t.Commit();
                return inst.Id;
            }
        }

        // --- Helper: Ensures the view has a valid SketchPlane ---
        private static void EnsureWorkPlaneInView(Document doc, View view)
        {
            if (view.SketchPlane == null)
            {
                Plane plane = Plane.CreateByNormalAndOrigin(view.ViewDirection, view.Origin);
                SketchPlane sp = SketchPlane.Create(doc, plane);
                view.SketchPlane = sp;
            }
        }

        private static XYZ SnapToGridInViewPlane(View view, XYZ p, double gridSizeMeters, out double uSnap, out double vSnap)
        {
            double grid = UnitUtils.ConvertToInternalUnits(gridSizeMeters, UnitTypeId.Meters);

            XYZ d = p - view.Origin;
            double u = d.DotProduct(view.RightDirection);
            double v = d.DotProduct(view.UpDirection);

            uSnap = Math.Round(u / grid) * grid;
            vSnap = Math.Round(v / grid) * grid;

            return view.Origin + view.RightDirection * uSnap + view.UpDirection * vSnap;
        }

        private static ElementId GetViewLevelId(View view)
        {
            try
            {
                if (view is ViewSection vs && vs.GenLevel != null)
                    return vs.GenLevel.Id;
            }
            catch
            {
                // ignored
            }

            return ElementId.InvalidElementId;
        }
    }
}