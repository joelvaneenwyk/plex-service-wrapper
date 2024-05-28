using System;
using System.ServiceProcess;
using PlexServiceCommon;
using PlexServiceWCF;
using System.Threading;
using CoreWCF.Description;
using Serilog;
using EndpointAddress = System.ServiceModel.EndpointAddress;
using NetTcpBinding = System.ServiceModel.NetTcpBinding;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.Versioning;

namespace PlexService
{
    /// <summary>
    /// Service that runs an instance of PmsMonitor to maintain an instance of Plex Media Server in session 0
    /// </summary>
    public partial class PlexMediaServerService : ServiceBase
    {
        private const string BaseAddress = "net.tcp://localhost:{0}/PlexService";

        /// <summary>
        /// Default the address with port 8787
        /// </summary>
        private string _address = string.Format(CultureInfo.InvariantCulture, BaseAddress, 8787);

        private static readonly TimeSpan TimeOut = TimeSpan.FromSeconds(2);

        private TrayInteraction _host;

        private PlexServiceCommon.Interface.ITrayInteraction _plexService;

        private readonly AutoResetEvent _stopped = new(false);

        private PlexMediaServerService()
        {
            InitializeComponent();

            //This is a simple start stop service, no pause and resume.
            if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows))
            {
                CanPauseAndContinue = false;
            }
        }

        [Conditional("RELEASE")]
        [SupportedOSPlatform("windows")]
        public static void Create(string[] args)
        {
            PlexMediaServerService serviceCall = new();
            ServiceBase[] servicesToRun = [serviceCall];

            if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(
                    System.Runtime.InteropServices.OSPlatform.Windows))
            {
                Run(servicesToRun);
            }

            serviceCall.OnDebug(args);
        }

        [Conditional("DEBUG")]
        [SupportedOSPlatform("windows")]
        private void OnDebug(string[] args)
        {
            OnStart(args);
            Console.WriteLine("Press any key to exit...");
            Console.ReadLine();
            Console.WriteLine("Stopping Plex...");
            OnStop();
        }

        /// <summary>
        /// Fires when the service is started
        /// </summary>
        /// <param name="args"></param>
        [SupportedOSPlatform("windows")]
        protected override async void OnStart(string[] args)
        {
            try
            {
                if (_host != null) await _host.CloseAsync();

                int port = SettingsHandler.Load().ServerPort;

                // sanity check the port setting
                if (port == 0)
                    port = 8787;

                _address = string.Format(CultureInfo.InvariantCulture, BaseAddress, port);

                Uri[] addressBase = [new(_address)];
                _host = new(addressBase);

                ServiceMetadataBehavior behave = new();
                _host.Description.Behaviors.Add(behave);

                //Set up a TCP binding with appropriate timeouts.
                //use a reliable connection so the clients can be notified when the "receive" timeout has elapsed and the connection is torn down.
                NetTcpBinding netTcpB = new()
                {
                    OpenTimeout = TimeOut,
                    CloseTimeout = TimeOut,
                    ReceiveTimeout = TimeSpan.FromMinutes(10),
                    ReliableSession = {
                        Enabled = true,
                        InactivityTimeout = TimeSpan.FromMinutes(5)
                    }
                };
                //_host.AddServiceEndpoint(typeof(PlexServiceCommon.Interface.ITrayInteraction), netTcpB, _address);
                //_host.AddServiceEndpoint(typeof(IMetadataExchange),
                //MetadataExchangeBindings.CreateMexTcpBinding(), "mex");

                //once the host is opened, start plex
                //_host.Opened += (s, e) => System.Threading.Tasks.Task.Factory.StartNew(() => startPlex());
                // Open the ServiceHostBase to create listeners and start
                // listening for messages.
                _host.Open();
            }
            catch (Exception ex)
            {
                Log.Warning("Exception starting Plex Service: " + ex.Message + " at " + ex.StackTrace);
            }
            Log.Information("Plex Service Started.");

            if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(
                    System.Runtime.InteropServices.OSPlatform.Windows))
            {
                base.OnStart(args);
            }
        }

        /// <summary>
        /// Fires when the service is stopped
        /// </summary>
        protected override void OnStop()
        {
            if (_host != null)
            {
                //Try and connect to the WCF service and call its stop method
                try
                {
                    if (_plexService == null)
                    {
                        Log.Information("Connecting to plex service.");
                        Connect();
                    }

                    if (_plexService != null)
                    {
                        Log.Information("Stopping plex service.");
                        _plexService.Stop();
                        //wait for plex to stop for 10 seconds
                        if (!_stopped.WaitOne(10000))
                        {
                            Log.Warning("Timed out waiting for plex service to stop.");
                        }
                        Disconnect();
                    }
                }
                catch (Exception ex)
                {
                    Log.Warning("Exception in OnStop: " + ex.Message);
                }

                try
                {
                    _host.Close();
                }
                catch (Exception ex)
                {
                    Log.Warning("Exception closing host: " + ex.Message);
                }
                finally
                {
                    _host = null;
                }
            }
            Log.Information("Plex Service Stopped.");
            if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(
                    System.Runtime.InteropServices.OSPlatform.Windows))
            {
                base.OnStop();
            }
        }

        /// <summary>
        /// Connect to WCF service
        /// </summary>
        private void Connect()
        {
            //Create a NetTcp binding to the service and set some appropriate timeouts.
            //Use reliable connection so we know when we have been disconnected
            NetTcpBinding plexServiceBinding = new()
            {
                OpenTimeout = TimeOut,
                CloseTimeout = TimeOut,
                SendTimeout = TimeOut,
                ReliableSession = {
                    Enabled = true,
                    InactivityTimeout = TimeSpan.FromMinutes(1)
                }
            };
            //Generate the endpoint from the local settings
            EndpointAddress plexServiceEndpoint = new(_address);

            TrayCallback callback = new();
            callback.Stopped += (_, _) => _stopped.Set();
            TrayInteractionClient client = new(callback, plexServiceBinding, plexServiceEndpoint);

            //Make a channel factory, so we can create the link to the service
            //var plexServiceChannelFactory = new ChannelFactory<PlexServiceCommon.Interface.ITrayInteraction>(plexServiceBinding, plexServiceEndpoint);

            _plexService = null;

            try
            {
                _plexService = client.ChannelFactory.CreateChannel();

                _plexService.Subscribe();
                //If we lose connection to the service, set the object to null so we will know to reconnect the next time the tray icon is clicked
                _plexService.Faulted += (_, _) => _plexService = null;
                _plexService.Closed += (_, _) => _plexService = null;
            }
            catch (Exception ex)
            {
                Log.Warning("Exception connecting PMS/WCF: " + ex.Message);
                _plexService = null;
            }
        }

        /// <summary>
        /// Disconnect from WCF service
        /// </summary>
        private void Disconnect()
        {
            //try and be nice...
            if (_plexService != null)
            {
                try
                {
                    _plexService.Close();
                }
                catch (Exception ex)
                {
                    Log.Warning("Exception disconnecting PMS/WCF: " + ex.Message);
                }
            }
            _plexService = null;
        }
    }
}
