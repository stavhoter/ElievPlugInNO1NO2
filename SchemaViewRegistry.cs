using Autodesk.Revit.DB;
using Autodesk.Revit.DB.ExtensibleStorage;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ElievPlugInNO1NO2
{
    public static class SchemaViewRegistry
    {
        // DO NOT CHANGE after you start using in projects
        private static readonly Guid SchemaGuid = new Guid("8C8D5B7A-9B77-4F7A-9B3A-2B9E6E7E1B21");
        private const string SchemaName = "STV_SchemaViewRegistry";
        private const string StorageName = "STV_SchemaViewRegistry_DS";

        private const string FieldSystemKeys = "SystemKeys";
        private const string FieldViewIdInts = "ViewIdInts";

        private static Schema GetOrCreateSchema()
        {
            Schema s = Schema.Lookup(SchemaGuid);
            if (s != null) return s;

            var b = new SchemaBuilder(SchemaGuid);
            b.SetSchemaName(SchemaName);
            b.SetReadAccessLevel(AccessLevel.Public);
            b.SetWriteAccessLevel(AccessLevel.Public);

            b.AddArrayField(FieldSystemKeys, typeof(string));
            b.AddArrayField(FieldViewIdInts, typeof(int));

            return b.Finish();
        }

        private static DataStorage GetOrCreateStorage(Document doc)
        {
            var ds = new FilteredElementCollector(doc)
                .OfClass(typeof(DataStorage))
                .Cast<DataStorage>()
                .FirstOrDefault(x => x.Name == StorageName);

            if (ds != null) return ds;

            ds = DataStorage.Create(doc);
            ds.Name = StorageName;
            return ds;
        }

        public static void SetSchemaViewId(Document doc, string systemKey, ElementId viewId)
        {
            if (doc == null) throw new ArgumentNullException(nameof(doc));

            systemKey = (systemKey ?? "").Trim();
            if (systemKey.Length == 0) throw new ArgumentException("systemKey is empty.");

            if (viewId == null || viewId == ElementId.InvalidElementId)
                throw new ArgumentException("viewId is invalid.");

            var schema = GetOrCreateSchema();
            var ds = GetOrCreateStorage(doc);

            Entity ent = ds.GetEntity(schema);
            if (!ent.IsValid())
                ent = new Entity(schema);

            IList<string> keys = ent.Get<IList<string>>(schema.GetField(FieldSystemKeys)) ?? new List<string>();
            IList<int> ids = ent.Get<IList<int>>(schema.GetField(FieldViewIdInts)) ?? new List<int>();

            int n = Math.Min(keys.Count, ids.Count);
            int idx = -1;

            for (int i = 0; i < n; i++)
            {
                if (string.Equals((keys[i] ?? "").Trim(), systemKey, StringComparison.OrdinalIgnoreCase))
                {
                    idx = i;
                    break;
                }
            }

            if (idx >= 0)
                ids[idx] = viewId.IntegerValue;
            else
            {
                keys.Add(systemKey);
                ids.Add(viewId.IntegerValue);
            }

            ent.Set(schema.GetField(FieldSystemKeys), keys);
            ent.Set(schema.GetField(FieldViewIdInts), ids);

            ds.SetEntity(ent);
        }

        public static List<SchemaOption> GetAllSchemas(Document doc)
        {
            var list = new List<SchemaOption>();
            if (doc == null) return list;

            Schema schema = Schema.Lookup(SchemaGuid);
            if (schema == null) return list;

            var ds = new FilteredElementCollector(doc)
                .OfClass(typeof(DataStorage))
                .Cast<DataStorage>()
                .FirstOrDefault(x => x.Name == StorageName);

            if (ds == null) return list;

            Entity ent = ds.GetEntity(schema);
            if (!ent.IsValid()) return list;

            IList<string> keys = ent.Get<IList<string>>(schema.GetField(FieldSystemKeys)) ?? new List<string>();
            IList<int> ids = ent.Get<IList<int>>(schema.GetField(FieldViewIdInts)) ?? new List<int>();

            int n = Math.Min(keys.Count, ids.Count);
            for (int i = 0; i < n; i++)
            {
                string k = (keys[i] ?? "").Trim();
                if (k.Length == 0) continue;

                int idInt = ids[i];

                // validate view exists
                if (doc.GetElement(new ElementId(idInt)) is View)
                    list.Add(new SchemaOption(k, idInt));
            }

            list.Sort((a, b) => string.Compare(a.SystemKey, b.SystemKey, StringComparison.OrdinalIgnoreCase));
            return list;
        }
    }
}