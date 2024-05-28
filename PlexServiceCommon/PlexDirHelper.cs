using System;
using System.IO;
using System.Runtime.Versioning;

namespace PlexServiceCommon
{
    public static class PlexDirHelper
    {
        /// <summary>
        /// Returns the full path and filename of the plex media server executable
        /// </summary>
        /// <returns></returns>
        [SupportedOSPlatform("windows")]
        public static string GetPlexDataDir()
        {
            //set appDataFolder to the default user local app data folder
            string appDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

            //check if the user has a custom path specified in the registry, if so, update the path to return this instead
            string userDefinedPath = PlexRegistryHelper.ReadUserRegistryValue("LocalAppDataPath");
            appDataFolder = string.IsNullOrEmpty(userDefinedPath) ? appDataFolder : userDefinedPath;

            string path = Path.Combine(appDataFolder, "Plex Media Server");

            return Directory.Exists(path) ? path : string.Empty;
        }
    }
}