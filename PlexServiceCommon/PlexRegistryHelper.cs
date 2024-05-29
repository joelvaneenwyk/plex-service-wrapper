using Microsoft.Win32;
using System;
using System.Runtime.Versioning;

namespace PlexServiceCommon
{
    public static class PlexRegistryHelper
    {
        [SupportedOSPlatform("windows")]
        public static string ReadUserRegistryValue(string name)
        {
            bool is64Bit = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("PROCESSOR_ARCHITEW6432"));
            const string subKey = @"Software\Plex, Inc.\Plex Media Server";
            string? result = null;
            try
            {
                using RegistryKey? pmsDataKey = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, is64Bit ? RegistryView.Registry64 : RegistryView.Registry32).OpenSubKey(subKey);
                result = pmsDataKey?.GetValue(name)?.ToString();
            }
            catch
            {
                // ignored
            }

            return result ?? string.Empty;
        }

        [SupportedOSPlatform("windows")]
        public static string? ReadLocalMachineRegistryValue(string name)
        {
            var is64Bit = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("PROCESSOR_ARCHITEW6432"));
            string subKey = @"Software\Plex, Inc.\Plex Media Server";
            try
            {
                using var pmsDataKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, is64Bit ? RegistryView.Registry64 : RegistryView.Registry32).OpenSubKey(subKey);

                if (pmsDataKey is not null)
                {
                    return pmsDataKey.GetValue(name)?.ToString();
                }
            }
            catch { }
            return string.Empty;
        }
    }
}
