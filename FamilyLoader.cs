using System;
using System.Linq;
using Autodesk.Revit.DB;

namespace ElievPlugInNO1NO2
{
    public static class FamilyLoader
    {
        /// <summary>
        /// Loads a family from an RFA path (if not already loaded) and returns the first FamilySymbol.
        /// </summary>
        public static FamilySymbol EnsureFirstSymbolLoaded(Document doc, string rfaPath)
        {
            if (doc == null) throw new ArgumentNullException(nameof(doc));
            if (string.IsNullOrWhiteSpace(rfaPath)) throw new ArgumentException("rfaPath is empty.");

            // Try find already loaded by family name (file name without extension)
            string famName = System.IO.Path.GetFileNameWithoutExtension(rfaPath);

            Family existing = new FilteredElementCollector(doc)
                .OfClass(typeof(Family))
                .Cast<Family>()
                .FirstOrDefault(f => string.Equals(f.Name, famName, StringComparison.OrdinalIgnoreCase));

            Family family = existing;

            if (family == null)
            {
                // Load the family into the project (requires Transaction)
                using (Transaction t = new Transaction(doc, "Load Family"))
                {
                    t.Start();
                    bool loaded = doc.LoadFamily(rfaPath, out family);
                    t.Commit();

                    if (!loaded || family == null)
                        return null;
                }
            }

            // Get first symbol (type)
            ElementId firstSymbolId = family.GetFamilySymbolIds().FirstOrDefault();
            if (firstSymbolId == null || firstSymbolId == ElementId.InvalidElementId)
                return null;

            return doc.GetElement(firstSymbolId) as FamilySymbol;
        }
    }
}