using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace ElievPlugInNO1NO2
{
    public static class CreatingLinerShceme
    {
        public static void Create(ExternalCommandData commandData, string viewNameFromUser)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc?.Document;

            if (uidoc == null || doc == null)
                throw new Exception("No active document.");

            View activeView = uidoc.ActiveView;

            // Pick source side view:
            // - If current view is a side view, use it.
            // - Else pick a random existing side view from the project.
            View sourceSideView = PickSourceSideView(doc, activeView);
            if (sourceSideView == null)
                throw new Exception("No existing side view (Elevation/Section) found in the project.");

            View newView;

            using (Transaction t = new Transaction(doc, "Create Linear Scheme (Duplicate Random Side View)"))
            {
                t.Start();

                // Step 1: Duplicate
                ElementId dupId = sourceSideView.Duplicate(ViewDuplicateOption.Duplicate);
                newView = doc.GetElement(dupId) as View;

                if (newView == null)
                    throw new Exception("Failed to duplicate the selected side view.");

                // Name
                string baseName = string.IsNullOrWhiteSpace(viewNameFromUser) ? "סכמה קווית" : viewNameFromUser.Trim();
                newView.Name = MakeUniqueViewName(doc, baseName);

                // Step 2: Hide everything except Levels + Level Heads
                HideAllCategoriesExcept(doc, newView, new[]
                {
                    BuiltInCategory.OST_Levels,
                    BuiltInCategory.OST_LevelHeads
                });

                // Optional: keep crop active but hide crop rectangle
                TrySetCrop(newView, cropActive: true, cropVisible: false);

                t.Commit();
            }

            // Switch AFTER commit
            uidoc.RequestViewChange(newView);
        }

        private static View PickSourceSideView(Document doc, View activeView)
        {
            // If active is already Elevation/Section, use it
            if (activeView != null &&
                !activeView.IsTemplate &&
                (activeView.ViewType == ViewType.Elevation || activeView.ViewType == ViewType.Section))
            {
                return activeView;
            }

            // Else pick random Elevation/Section from project
            List<View> sideViews = new FilteredElementCollector(doc)
                .OfClass(typeof(View))
                .Cast<View>()
                .Where(v =>
                    !v.IsTemplate &&
                    (v.ViewType == ViewType.Elevation || v.ViewType == ViewType.Section))
                .ToList();

            if (sideViews.Count == 0)
                return null;

            // Random index (new each run)
            int seed = Guid.NewGuid().GetHashCode();
            Random rnd = new Random(seed);
            int idx = rnd.Next(sideViews.Count);

            return sideViews[idx];
        }

        private static void HideAllCategoriesExcept(Document doc, View view, IEnumerable<BuiltInCategory> keepCats)
        {
            HashSet<ElementId> keep = keepCats.Select(c => new ElementId(c)).ToHashSet();

            foreach (Category cat in doc.Settings.Categories)
            {
                if (cat == null) continue;

                try
                {
                    if (!view.CanCategoryBeHidden(cat.Id)) continue;
                    view.SetCategoryHidden(cat.Id, !keep.Contains(cat.Id));
                }
                catch
                {
                    // ignore
                }
            }
        }

        private static void TrySetCrop(View view, bool cropActive, bool cropVisible)
        {
            try { view.CropBoxActive = cropActive; } catch { }
            try { view.CropBoxVisible = cropVisible; } catch { }
        }

        private static string MakeUniqueViewName(Document doc, string baseName)
        {
            var existingNames = new FilteredElementCollector(doc)
                .OfClass(typeof(View))
                .Cast<View>()
                .Select(v => v.Name)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            if (!existingNames.Contains(baseName))
                return baseName;

            int i = 2;
            while (existingNames.Contains($"{baseName} ({i})"))
                i++;

            return $"{baseName} ({i})";
        }
    }
}
