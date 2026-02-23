using Autodesk.Revit.DB;
using Autodesk.Revit.DB.ExtensibleStorage;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ElievPlugInNO1NO2
{
    public static class StvExtensibleStorage
    {
        // IMPORTANT: Keep this GUID stable forever once you ship.
        private static readonly Guid SchemaGuid = new Guid("A7D2A1E1-5B76-4D83-9E4D-9E1A9E6A0B21");

        private const string SchemaName = "STV_Metadata";
        private const string FieldKeys = "Keys";
        private const string FieldValues = "Values";

        private static Schema GetOrCreateSchema()
        {
            Schema schema = Schema.Lookup(SchemaGuid);
            if (schema != null)
                return schema;

            var builder = new SchemaBuilder(SchemaGuid);
            builder.SetSchemaName(SchemaName);
            builder.SetReadAccessLevel(AccessLevel.Public);
            builder.SetWriteAccessLevel(AccessLevel.Public);

            // Two parallel arrays: Keys[i] corresponds to Values[i]
            builder.AddArrayField(FieldKeys, typeof(string));
            builder.AddArrayField(FieldValues, typeof(string));

            return builder.Finish();
        }

        public static Dictionary<string, string> ReadAll(Element element)
        {
            if (element == null) return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            Schema schema = GetOrCreateSchema();
            Entity ent = element.GetEntity(schema);

            if (!ent.IsValid())
                return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            IList<string> keys = ent.Get<IList<string>>(schema.GetField(FieldKeys)) ?? new List<string>();
            IList<string> values = ent.Get<IList<string>>(schema.GetField(FieldValues)) ?? new List<string>();

            var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            int n = Math.Min(keys.Count, values.Count);
            for (int i = 0; i < n; i++)
            {
                string k = (keys[i] ?? "").Trim();
                if (k.Length == 0) continue;

                dict[k] = values[i] ?? "";
            }

            return dict;
        }

        public static string ReadValue(Element element, string key)
        {
            if (string.IsNullOrWhiteSpace(key)) return null;
            var dict = ReadAll(element);
            return dict.TryGetValue(key.Trim(), out var v) ? v : null;
        }

        public static void SetValue(Element element, string key, string value)
        {
            if (element == null) throw new ArgumentNullException(nameof(element));
            if (string.IsNullOrWhiteSpace(key)) throw new ArgumentException("Key is empty.", nameof(key));

            key = key.Trim();
            value = value ?? "";

            Schema schema = GetOrCreateSchema();
            var dict = ReadAll(element);

            dict[key] = value;

            WriteAll(element, dict, schema);
        }

        public static bool RemoveKey(Element element, string key)
        {
            if (element == null) return false;
            if (string.IsNullOrWhiteSpace(key)) return false;

            key = key.Trim();
            Schema schema = GetOrCreateSchema();
            var dict = ReadAll(element);

            bool removed = dict.Remove(key);
            if (!removed) return false;

            WriteAll(element, dict, schema);
            return true;
        }

        private static void WriteAll(Element element, Dictionary<string, string> dict, Schema schema)
        {
            // Keep stable ordering for repeatability
            var keys = dict.Keys.OrderBy(k => k, StringComparer.OrdinalIgnoreCase).ToList();
            var values = keys.Select(k => dict[k] ?? "").ToList();

            Entity ent = new Entity(schema);
            ent.Set(schema.GetField(FieldKeys), (IList<string>)keys);
            ent.Set(schema.GetField(FieldValues), (IList<string>)values);

            element.SetEntity(ent);
        }
    }
}