using System.IO;
using Newtonsoft.Json;
using PlexServiceCommon;

namespace PlexServiceWCF
{
    /// <summary>
    /// Class for loading and saving settings on the server
    /// Code is here rather than in the settings class as it should only ever be save on the server.
    /// settings are retrieved remotely by calling the wcf service GetSettings and SetSettings methods
    /// </summary>
    public static class SettingsHandler
    {
        #region Load/Save

        private static string GetSettingsFile()
        {
            return Path.Combine(TrayInteraction.AppDataPath, "Settings.json");
        }

        /// <summary>
        /// Save the settings file
        /// </summary>
        internal static void Save(Settings? settings)
        {
            string filePath = GetSettingsFile();

            if (!Directory.Exists(Path.GetDirectoryName(filePath))) {
                string? dir = Path.GetDirectoryName(filePath);
                if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);
            }

            using StreamWriter sw = new(filePath, false);
            sw.Write(JsonConvert.SerializeObject(settings, Formatting.Indented));
            TrayCallback tc = new();
            tc.OnSettingChange(settings);
        }     

        /// <summary>
        /// Load the settings from disk
        /// </summary>
        /// <returns></returns>
        public static Settings Load()
        {
            string filePath = GetSettingsFile();
            Settings? settings = null;
            try
            {
                if (File.Exists(filePath)) {
                    using StreamReader sr = new(filePath);
                    string rawSettings = sr.ReadToEnd();
                    settings = JsonConvert.DeserializeObject<Settings>(rawSettings);
                }     
            }
            finally
            {
                settings ??= new Settings();
                Save(settings);
            }
            return settings;
        }

        #endregion
    }
}
