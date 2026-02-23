using Autodesk.Revit.DB;
using Autodesk.Revit.DB.ExtensibleStorage;
using System;

namespace ElievPlugInNO1NO2
{
    public static class StvSchemaInstanceStorage
    {
        // Do not change once used in real projects
        private static readonly Guid SchemaGuid = new Guid("C2D8C3B2-5E6A-4A12-9F8B-5B9B5A2C4F10");
        private const string SchemaName = "STV_SchemaInstance";

        private const string F_InstanceGuid = "InstanceGuid";
        private const string F_SchemaName = "SchemaName";
        private const string F_Family2DPath = "Family2DPath";
        private const string F_LevelIdInt = "LevelIdInt";
        private const string F_GridU = "GridU";
        private const string F_GridV = "GridV";

        private static Schema GetOrCreateSchema()
        {
            Schema s = Schema.Lookup(SchemaGuid);
            if (s != null) return s;

            var b = new SchemaBuilder(SchemaGuid);
            b.SetSchemaName(SchemaName);
            b.SetReadAccessLevel(AccessLevel.Public);
            b.SetWriteAccessLevel(AccessLevel.Public);

            b.AddSimpleField(F_InstanceGuid, typeof(string));
            b.AddSimpleField(F_SchemaName, typeof(string));
            b.AddSimpleField(F_Family2DPath, typeof(string));
            b.AddSimpleField(F_LevelIdInt, typeof(int));
            b.AddSimpleField(F_GridU, typeof(double));
            b.AddSimpleField(F_GridV, typeof(double));

            return b.Finish();
        }

        public static void SetInstanceData(
            Element element,
            Guid instanceGuid,
            string schemaName,
            string family2DPath,
            ElementId levelId,
            double gridU,
            double gridV)
        {
            if (element == null) throw new ArgumentNullException(nameof(element));

            Schema s = GetOrCreateSchema();

            var e = new Entity(s);
            e.Set(s.GetField(F_InstanceGuid), instanceGuid.ToString());
            e.Set(s.GetField(F_SchemaName), schemaName ?? "");
            e.Set(s.GetField(F_Family2DPath), family2DPath ?? "");
            e.Set(s.GetField(F_LevelIdInt), (levelId != null && levelId != ElementId.InvalidElementId) ? levelId.IntegerValue : -1);
            e.Set(s.GetField(F_GridU), gridU);
            e.Set(s.GetField(F_GridV), gridV);

            element.SetEntity(e);
        }

        public static bool TryGetInstanceGuid(Element element, out Guid guid)
        {
            guid = Guid.Empty;
            if (element == null) return false;

            Schema s = Schema.Lookup(SchemaGuid);
            if (s == null) return false;

            Entity ent = element.GetEntity(s);
            if (!ent.IsValid()) return false;

            string g = ent.Get<string>(s.GetField(F_InstanceGuid));
            return Guid.TryParse(g, out guid);
        }
    }
}