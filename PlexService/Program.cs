using System.Runtime.Versioning;
using PlexServiceCommon;

namespace PlexService
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [SupportedOSPlatform("windows")]
        private static void Main(string[] args)
        {
            LogWriter.Init();
            PlexMediaServerService.Create(args);

            //if (args.Length > 0 && args[0].ToUpper() == "DEBUG")
            //{
            //    System.Diagnostics.Debugger.Launch();
            //}

            //var servicesToRun = new ServiceBase[]
            //{
            //    new PlexMediaServerService()
            //};
            //ServiceBase.Run(servicesToRun);
        }
    }
}
