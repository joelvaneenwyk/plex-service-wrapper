﻿using Newtonsoft.Json;

namespace PlexServiceCommon
{
    /// <summary>
    /// Auxiliary application class.
    /// This class represents an application that should be run when plex runs
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class AuxiliaryApplication
    {
        /// <summary>
        /// Friendly name of the application
        /// </summary>
        [JsonProperty]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Path of the executable to run
        /// </summary>
        [JsonProperty]
        public string FilePath { get; set; } = string.Empty;

        //The working folder to set
        [JsonProperty]
        public string WorkingFolder { get; set; } = string.Empty;

        /// <summary>
        /// any arguments to pass to the executable
        /// </summary>
        [JsonProperty]
        public string Argument { get; set; } = string.Empty;

        /// <summary>
        /// A flag to determine if the application should be kept running (service like) or if it can run and stop (like a script)
        /// </summary>
        [JsonProperty]
        public bool KeepAlive { get; set; } = true;

        /// <summary>
        /// A flag to determine if the application's std. output should be redirected to the Plex Service log.
        /// </summary>
        [JsonProperty]
        public bool LogOutput { get; set; } = true;

        /// <summary>
        /// Url link to the auxiliary application interface.
        /// </summary>
        [JsonProperty]
        public string? Url { get; set; }
    }
}
