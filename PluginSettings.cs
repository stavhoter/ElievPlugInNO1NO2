using System;
using System.IO;

namespace ElievPlugInNO1NO2
{
    internal static class PluginSettings
    {
        private static string SettingsDir =>
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ElievPlugInNO1NO2");

        private static string Schema2DFilePath => Path.Combine(SettingsDir, "schema_2d_folder.txt");
        private static string Model3DFilePath => Path.Combine(SettingsDir, "model_3d_folder.txt");

        // Optional: old single-folder setting (won't hurt)
        private static string ComponentsFilePath => Path.Combine(SettingsDir, "components_folder.txt");

        public static void SaveSchema2DFolder(string folderPath)
        {
            Directory.CreateDirectory(SettingsDir);
            File.WriteAllText(Schema2DFilePath, (folderPath ?? "").Trim());
        }

        public static string LoadSchema2DFolder()
        {
            if (!File.Exists(Schema2DFilePath)) return null;
            var path = File.ReadAllText(Schema2DFilePath)?.Trim();
            return string.IsNullOrWhiteSpace(path) ? null : path;
        }

        public static void SaveModel3DFolder(string folderPath)
        {
            Directory.CreateDirectory(SettingsDir);
            File.WriteAllText(Model3DFilePath, (folderPath ?? "").Trim());
        }

        public static string LoadModel3DFolder()
        {
            if (!File.Exists(Model3DFilePath)) return null;
            var path = File.ReadAllText(Model3DFilePath)?.Trim();
            return string.IsNullOrWhiteSpace(path) ? null : path;
        }

        // Backward compatibility (optional)
        public static void SaveComponentsFolder(string folderPath)
        {
            Directory.CreateDirectory(SettingsDir);
            File.WriteAllText(ComponentsFilePath, (folderPath ?? "").Trim());
        }

        public static string LoadComponentsFolder()
        {
            if (!File.Exists(ComponentsFilePath)) return null;
            var path = File.ReadAllText(ComponentsFilePath)?.Trim();
            return string.IsNullOrWhiteSpace(path) ? null : path;
        }
    }
}