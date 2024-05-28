using System;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace PlexServiceCommon
{
    /// <summary>
    /// Event arguments for a plex status change
    /// </summary>
    [Serializable]
    [DataContract]
    public class StatusChangeEventArgs: EventArgs
    {
        [DataMember] public readonly EventLogEntryType? EventType;

        [DataMember] public readonly string Description;

        public StatusChangeEventArgs(string description, EventLogEntryType? eventType = null)
        {
            if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(
                    System.Runtime.InteropServices.OSPlatform.Windows))
            {
                EventType = eventType ?? EventLogEntryType.Information;
            }
            Description = description;
        }
    }
}
