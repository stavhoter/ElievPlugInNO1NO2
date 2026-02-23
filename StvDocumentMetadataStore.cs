using Autodesk.Revit.DB;
using Autodesk.Revit.DB.ExtensibleStorage;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ElievPlugInNO1NO2
{
    public static class StvDocumentMetadataStore
    {
        private static readonly Guid SchemaGuid = new Guid("5C0D6D53-6B56-4E10-B3D2-0C3B2A18A4A9");
        private const string SchemaName = "STV_Document_MetadataStore";
        private const string StorageName = "STV_MetadataStore";

        private const string FieldFamilyKeys = "FamilyKeys";
        private const string FieldPropKeys = "PropKeys";
        private const string FieldPropValues = "PropValues";

        private static Schema GetOrCreateSchema()
        {
            var schema = Schema.Lookup(SchemaGuid);
            if (schema != null) return schema;

            var b = new SchemaBuilder(SchemaGuid);
            b.SetSchemaName(SchemaName);
            b.SetReadAccessLevel(AccessLevel.Public);
            b.SetWriteAccessLevel(AccessLevel.Public);

            // Parallel arrays:
            // FamilyKeys[i], PropKeys[i], PropValues[i]
            b.AddArrayField(FieldFamilyKeys, typeof(string));
            b.AddArrayField(FieldPropKeys, typeof(string));
            b.AddArrayField(FieldPropValues, typeof(string));

            return b.Finish();
        }

        private static DataStorage GetOrCreateStorage(Document doc)
        {
            var existing = new FilteredElementCollector(doc)
                .OfClass(typeof(DataStorage))
                .Cast<DataStorage>()
                .FirstOrDefault(ds => ds.Name == StorageName);

            if (existing != null) return existing;

            var dsNew = DataStorage.Create(doc);
            dsNew.Name = StorageName;
            return dsNew;
        }

        public static Dictionary<string, string> ReadProperties(Document doc, string familyKey)
        {
            familyKey = (familyKey ?? "").Trim();
            if (familyKey.Length == 0)
                return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            var schema = GetOrCreateSchema();
            var ds = FindStorage(doc);
            if (ds == null)
                return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            var ent = ds.GetEntity(schema);
            if (!ent.IsValid())
                return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            var fArr = ent.Get<IList<string>>(schema.GetField(FieldFamilyKeys)) ?? new List<string>();
            var kArr = ent.Get<IList<string>>(schema.GetField(FieldPropKeys)) ?? new List<string>();
            var vArr = ent.Get<IList<string>>(schema.GetField(FieldPropValues)) ?? new List<string>();

            var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            int n = Math.Min(fArr.Count, Math.Min(kArr.Count, vArr.Count));
            for (int i = 0; i < n; i++)
            {
                if (!string.Equals((fArr[i] ?? "").Trim(), familyKey, StringComparison.OrdinalIgnoreCase))
                    continue;

                string prop = (kArr[i] ?? "").Trim();
                if (prop.Length == 0) continue;

                dict[prop] = vArr[i] ?? "";
            }

            return dict;
        }

        public static void SetProperty(Document doc, string familyKey, string propName, string propValue)
        {
            familyKey = (familyKey ?? "").Trim();
            propName = (propName ?? "").Trim();
            propValue = propValue ?? "";

            if (familyKey.Length == 0) throw new ArgumentException("familyKey is empty.");
            if (propName.Length == 0) throw new ArgumentException("propName is empty.");

            var schema = GetOrCreateSchema();
            var ds = GetOrCreateStorage(doc);

            var ent = ds.GetEntity(schema);
            if (!ent.IsValid())
                ent = new Entity(schema);

            var fArr = ent.Get<IList<string>>(schema.GetField(FieldFamilyKeys)) ?? new List<string>();
            var kArr = ent.Get<IList<string>>(schema.GetField(FieldPropKeys)) ?? new List<string>();
            var vArr = ent.Get<IList<string>>(schema.GetField(FieldPropValues)) ?? new List<string>();

            // Find existing (familyKey+propName) row
            int n = Math.Min(fArr.Count, Math.Min(kArr.Count, vArr.Count));
            int idx = -1;
            for (int i = 0; i < n; i++)
            {
                if (string.Equals((fArr[i] ?? "").Trim(), familyKey, StringComparison.OrdinalIgnoreCase) &&
                    string.Equals((kArr[i] ?? "").Trim(), propName, StringComparison.OrdinalIgnoreCase))
                {
                    idx = i;
                    break;
                }
            }

            if (idx >= 0)
            {
                vArr[idx] = propValue;
            }
            else
            {
                fArr.Add(familyKey);
                kArr.Add(propName);
                vArr.Add(propValue);
            }

            ent.Set(schema.GetField(FieldFamilyKeys), fArr);
            ent.Set(schema.GetField(FieldPropKeys), kArr);
            ent.Set(schema.GetField(FieldPropValues), vArr);

            ds.SetEntity(ent);
        }

        public static bool RemoveProperty(Document doc, string familyKey, string propName)
        {
            familyKey = (familyKey ?? "").Trim();
            propName = (propName ?? "").Trim();

            if (familyKey.Length == 0 || propName.Length == 0) return false;

            var schema = GetOrCreateSchema();
            var ds = FindStorage(doc);
            if (ds == null) return false;

            var ent = ds.GetEntity(schema);
            if (!ent.IsValid()) return false;

            var fArr = ent.Get<IList<string>>(schema.GetField(FieldFamilyKeys)) ?? new List<string>();
            var kArr = ent.Get<IList<string>>(schema.GetField(FieldPropKeys)) ?? new List<string>();
            var vArr = ent.Get<IList<string>>(schema.GetField(FieldPropValues)) ?? new List<string>();

            int n = Math.Min(fArr.Count, Math.Min(kArr.Count, vArr.Count));
            int idx = -1;
            for (int i = 0; i < n; i++)
            {
                if (string.Equals((fArr[i] ?? "").Trim(), familyKey, StringComparison.OrdinalIgnoreCase) &&
                    string.Equals((kArr[i] ?? "").Trim(), propName, StringComparison.OrdinalIgnoreCase))
                {
                    idx = i;
                    break;
                }
            }

            if (idx < 0) return false;

            fArr.RemoveAt(idx);
            kArr.RemoveAt(idx);
            vArr.RemoveAt(idx);

            ent.Set(schema.GetField(FieldFamilyKeys), fArr);
            ent.Set(schema.GetField(FieldPropKeys), kArr);
            ent.Set(schema.GetField(FieldPropValues), vArr);

            ds.SetEntity(ent);
            return true;
        }

        private static DataStorage FindStorage(Document doc)
        {
            return new FilteredElementCollector(doc)
                .OfClass(typeof(DataStorage))
                .Cast<DataStorage>()
                .FirstOrDefault(ds => ds.Name == StorageName);
        }
    }
}