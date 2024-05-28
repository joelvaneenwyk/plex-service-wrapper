using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using Newtonsoft.Json;
using PlexServiceCommon;

namespace PlexServiceTray
{
    /// <summary>
    /// Local settings for the tray application to connect to the WCF service
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    internal class TrayApplicationSettings
    {
        public static readonly IList<string> Themes = new ReadOnlyCollection<string>
            (new List<string>
            {
                "Dark Amber",
                "Dark Blue",
                "Dark Brown",
                "Dark Cobalt",
                "Dark Crimson",
                "Dark Cyan",
                "Dark Emerald",
                "Dark Green",
                "Dark Indigo",
                "Dark Lime",
                "Dark Magenta",
                "Dark Mauve",
                "Dark Olive",
                "Dark Orange",
                "Dark Pink",
                "Dark Purple",
                "Dark Red",
                "Dark Sienna",
                "Dark Steel",
                "Dark Taupe",
                "Dark Teal",
                "Dark Violet",
                "Dark Yellow",
                "Light Amber",
                "Light Blue",
                "Light Brown",
                "Light Cobalt",
                "Light Crimson",
                "Light Cyan",
                "Light Emerald",
                "Light Green",
                "Light Indigo",
                "Light Lime",
                "Light Magenta",
                "Light Mauve",
                "Light Olive",
                "Light Orange",
                "Light Pink",
                "Light Purple",
                "Light Red",
                "Light Sienna",
                "Light Steel",
                "Light Taupe",
                "Light Teal",
                "Light Violet",
                "Light Yellow"
            });

        #region Properties

        /// <summary>
        /// Address of the server running the wcf service
        /// </summary>
        [JsonProperty]
        public string ServerAddress { get; set; } = Settings.LocalHost;

        /// <summary>
        /// port of the WCF service endpoint
        /// </summary>
        [JsonProperty]
        public int ServerPort { get; set; } = 8787;

        [JsonProperty]
        public bool IsLocalHost => ServerAddress is Settings.LocalHost;

        [JsonProperty]
        public bool IsLocal => IsLocalHost || ServerAddress is "127.0.0.1" or "0.0.0.0";

        [JsonProperty]
        public string Theme { get; set; } = "Dark.Amber";

        #endregion

        #region Constructor

        private TrayApplicationSettings() { }

        #endregion

        /// <summary>
        /// Turn the properties into a useful endpoint uri
        /// </summary>
        /// <returns></returns>
        public string GetServiceAddress()
        {
            return $"net.tcp://{ServerAddress}:{ServerPort}/PlexService/";
        }

        #region Load/Save

        /// <summary>
        /// get the settings file location
        /// </summary>
        /// <returns></returns>
        internal static string GetSettingsFile()
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"Plex Service\LocalSettings.json");
        }



        /// <summary>
        /// Save the settings file
        /// </summary>
        internal void Save()
        {
            string filePath = GetSettingsFile();

            if (!Directory.Exists(Path.GetDirectoryName(filePath))) {
                string? dir = Path.GetDirectoryName(filePath);
                if (!string.IsNullOrEmpty(dir)) {
                    Directory.CreateDirectory(dir);
                } else {
                    throw new DirectoryNotFoundException(dir);
                }
            }

            using StreamWriter sw = new(filePath, false);
            string rawSettings = JsonConvert.SerializeObject(this);
            sw.Write(rawSettings);
        }


        /// <summary>
        /// Load the settings from disk
        /// </summary>
        /// <returns></returns>
        internal static TrayApplicationSettings Load()
        {
            string filePath = GetSettingsFile();
            TrayApplicationSettings? settings = null;
            try
            {
                if (File.Exists(filePath)) {
                    using StreamReader sr = new(filePath);
                    string rawSettings = sr.ReadToEnd();
                    settings = JsonConvert.DeserializeObject<TrayApplicationSettings>(rawSettings);
                }
            }
            finally
            {
                settings ??= new TrayApplicationSettings();
                settings.Save();
            }
            return settings;
        }

        #endregion
    }
}
