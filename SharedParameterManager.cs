using System;
using System.Collections.Generic;
using System.IO;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;

namespace ElievPlugInNO1NO2
{
    public static class SharedParameterManager
    {
        /// <summary>
        /// Ensures that STV_UserData shared parameter exists and is bound as an INSTANCE parameter
        /// to Detail Components and Generic Models.
        /// </summary>
        public static void EnsureStvUserData(Application app, Document doc)
        {
            string baseDir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                "STAVELIEVPLUGIN1");

            Directory.CreateDirectory(baseDir);

            string sharedParamsFilePath = Path.Combine(baseDir, "STV_SharedParameters.txt");
            const string groupName = "STV_Parameters";
            const string paramName = "STV_UserData";

            EnsureSharedParameterTextInstance(
                app,
                doc,
                sharedParamsFilePath,
                groupName,
                paramName,
                GroupTypeId.IdentityData, // ✅ instead of BuiltInParameterGroup.PG_IDENTITY_DATA
                new[]
                {
                    BuiltInCategory.OST_DetailComponents, // 2D schema
                    BuiltInCategory.OST_GenericModel      // 3D twins
                }
            );
        }

        /// <summary>
        /// Creates the shared parameter file (if missing), creates the definition (if missing),
        /// and binds it as INSTANCE parameter to the given categories.
        /// </summary>
        private static void EnsureSharedParameterTextInstance(
            Application app,
            Document doc,
            string sharedParamsFilePath,
            string groupName,
            string paramName,
            ForgeTypeId parameterGroupId,                 // ✅ GroupTypeId / ForgeTypeId
            IEnumerable<BuiltInCategory> targetCategories)
        {
            if (app == null) throw new ArgumentNullException(nameof(app));
            if (doc == null) throw new ArgumentNullException(nameof(doc));

            if (!File.Exists(sharedParamsFilePath))
            {
                using (File.Create(sharedParamsFilePath)) { }
            }

            string originalFile = app.SharedParametersFilename;
            app.SharedParametersFilename = sharedParamsFilePath;

            try
            {
                DefinitionFile defFile = app.OpenSharedParameterFile();
                if (defFile == null)
                    throw new InvalidOperationException("Failed to open shared parameter file: " + sharedParamsFilePath);

                DefinitionGroup group = defFile.Groups.get_Item(groupName) ?? defFile.Groups.Create(groupName);

                Definition definition = group.Definitions.get_Item(paramName);
                if (definition == null)
                {
                    // Text parameter
                    var opts = new ExternalDefinitionCreationOptions(paramName, SpecTypeId.String.Text)
                    {
                        Description = "User metadata for STV plugin",
                        Visible = true
                    };

                    definition = group.Definitions.Create(opts);
                }

                CategorySet catSet = app.Create.NewCategorySet();
                foreach (BuiltInCategory bic in targetCategories)
                {
                    Category cat = doc.Settings.Categories.get_Item(bic);
                    if (cat != null) catSet.Insert(cat);
                }

                InstanceBinding binding = app.Create.NewInstanceBinding(catSet);
                BindingMap map = doc.ParameterBindings;

                using (Transaction t = new Transaction(doc, "Ensure STV shared parameter"))
                {
                    t.Start();

                    // ✅ Use overload with ForgeTypeId (GroupTypeId)
                    bool inserted = map.Insert(definition, binding, parameterGroupId);
                    if (!inserted)
                    {
                        map.ReInsert(definition, binding, parameterGroupId);
                    }

                    t.Commit();
                }
            }
            finally
            {
                app.SharedParametersFilename = originalFile;
            }
        }
    }
}