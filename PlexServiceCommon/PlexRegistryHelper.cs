using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Versioning;
using System.Text;
using System.Threading.Tasks;

namespace PlexServiceCommon
{
    public static class PlexRegistryHelper
    {

        [SupportedOSPlatform("windows")]
        public static string ReadUserRegistryValue(string name)
        {
            bool is64Bit = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("PROCESSOR_ARCHITEW6432"));
            const string subKey = @"Software\Plex, Inc.\Plex Media Server";
            try
            {
                using var pmsDataKey = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, is64Bit ? RegistryView.Registry64 : RegistryView.Registry32).OpenSubKey(subKey);

                if (pmsDataKey is not null)
                {
                    return pmsDataKey.GetValue(name)?.ToString();
                }
            }
            catch
            {
                // ignored
            }

            return string.Empty;
        }
    }
}
