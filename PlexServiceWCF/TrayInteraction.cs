﻿using System;
using System.Collections.Generic;
using System.IO;
using System.ServiceModel;
using System.Threading.Tasks;
using PlexServiceCommon;
using PlexServiceCommon.Interface;
using Serilog;
using Serilog.Events;
using static System.Net.WebRequestMethods;
using File = System.IO.File;

namespace PlexServiceWCF
{
    /// <summary>
    /// WCF service implementation
    /// </summary>
    [ServiceBehavior(ConfigurationName = "PlexServiceWCF:PlexServiceWCF.TrayInteraction", InstanceContextMode = InstanceContextMode.Single)]
    public class TrayInteraction : ITrayInteraction
    {
        public static readonly string AppDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"Plex Service\");

        private readonly PmsMonitor _pms;

        private static readonly List<ITrayCallback> CallbackChannels = [];
        private readonly ITrayInteraction _trayInteractionImplementation;

        public TrayInteraction() {
            _trayInteractionImplementation = this;
            _pms = new PmsMonitor();
            _pms.StateChange += PlexStateChange;
            _pms.PlexStop += PlexStopped;
        }

        private void PlexStopped(object sender, EventArgs e)
        {
            if (_pms != null)
            {
                CallbackChannels.ForEach(callback => {
                    if (callback == null) {
                        return;
                    }

                    try
                    {
                        callback.OnPlexStopped();
                    } catch (Exception ex) {
                        Log.Warning("Exception running callback: " + ex.Message);
                    }
                });
            }
        }

        private void PlexStateChange(object sender, EventArgs e)
        {
            if (_pms != null)
            {
                CallbackChannels.ForEach(callback => {
                    if (callback == null) {
                        return;
                    }

                    try
                    {
                        callback.OnPlexStateChange(_pms.State);
                    } catch (Exception ex) {
                        Log.Warning("Exception on plex state change callback: " + ex.Message);
                    }
                });
            }
        }

        /// <summary>
        /// Start Plex
        /// </summary>
        public async Task Start()
        {
            //do this in another thread to return immediately so we don't hold up the service starting
            await _pms.Start();
        }

        /// <summary>
        /// Stop Plex
        /// </summary>
        public void Stop()
        {
            Task.Factory.StartNew(() => _pms.Stop());
        }

        /// <summary>
        /// Restart Plex
        /// </summary>
        public void Restart()
        {
            //stop and restart plex and the auxiliary apps
            Task.Factory.StartNew(() =>
                {
                    _pms.Restart(5000);
                });
        }

        /// <summary>
        /// Write the settings to the server
        /// </summary>
        /// <param name="settings">Json serialised Settings instance</param>
        public void SetSettings(Settings settings)
        {
            SettingsHandler.Save(settings);
        }

        public void LogMessage(string message, LogEventLevel level=LogEventLevel.Debug) {
            Log.Write(level,message);
        }

        /// <summary>
        /// Returns the log file as a string
        /// </summary>
        /// <returns></returns>
        public string GetLog()
        {
            var res = LogWriter.Read().Result;
            Log.Debug("Res is " + res.Length);
            return res;
        }

        public string GetLogPath() {
            return LogWriter.GetLatestLog();
        }

        public string GetPmsDataPath() {
            return PlexDirHelper.GetPlexDataDir();
        }

        /// <summary>
        /// Returns the settings file from the server as a json string
        /// </summary>
        /// <returns></returns>
        public Settings GetSettings()
        {
            return SettingsHandler.Load();
        }

        /// <summary>
        /// Returns Running or Stopped
        /// </summary>
        /// <returns></returns>
        public PlexState GetStatus()
        {
            if (_pms != null)
                return _pms.State;
            return PlexState.Stopped;
        }


        /// <summary>
        /// A request from the client for the running status of a specific auxiliary application
        /// </summary>
        /// <param name="name">the name of the auxiliary application to check</param>
        /// <returns></returns>
        public bool IsAuxAppRunning(string name)
        {
            return _pms.IsAuxAppRunning(name);
        }

        public async void StartAuxApp(string name)
        {
            await _pms.StartAuxApp(name);
        }

        public void StopAuxApp(string name)
        {
            _pms.StopAuxApp(name);
        }

        public string GetWebLink()
        {
            Log.Write(LogEventLevel.Information, "WebLink requested, plex version is: " + _pms.PlexVersion);
            var address = $"http://{PlexServiceCommon.Settings.LocalHost}:32400/web";

            if (_pms.PlexVersion > new Version("1.32.0.0"))
            {
                Log.Write(LogEventLevel.Information, "Plex version is greater than 1.32, checking for token and server claim status");
                //try to read the token
                var token = PlexRegistryHelper.ReadUserRegistryValue("PlexOnlineToken");
                if (string.IsNullOrEmpty(token))
                {
                    Log.Write(LogEventLevel.Information, "Plex online token is empty or cannot be read, checking for claim url...");
                    //empty token means the server is unclaimed and we should try and hit the claim url
                    var dataDir = PlexDirHelper.GetPlexDataDir();
                    var claimUrlFile = Path.Combine(dataDir, ".claimURL");
                    var setupPlex = Path.Combine(dataDir, "Setup Plex.html");
                    if (File.Exists(claimUrlFile))
                    {
                        var claimUrl = File.ReadAllText(claimUrlFile);
                        //return the claim url or if for some reason its empty, return the setup plex html
                        if (string.IsNullOrEmpty(claimUrl))
                        {
                            Log.Write(LogEventLevel.Information, "No claim url found, returning the setup address");
                            address = setupPlex;
                        }
                        else
                        {
                            Log.Write(LogEventLevel.Information, "Claim url found: " + claimUrl);
                            address = claimUrl;
                        }
                    }
                    else
                    {
                        Log.Write(LogEventLevel.Information, "No claim file found, returning the setup address");
                        address = setupPlex;
                    }

                }
            }
            return address;
        }

        public void Subscribe()
        {
            var channel = OperationContext.Current.GetCallbackChannel<ITrayCallback>();
            if (!CallbackChannels.Contains(channel)) //if CallbackChannels not contain current one.
            {
                CallbackChannels.Add(channel);
            }
        }

        public void UnSubscribe()
        {
            var channel = OperationContext.Current.GetCallbackChannel<ITrayCallback>();
            if (CallbackChannels.Contains(channel)) //if CallbackChannels not contain current one.
            {
                CallbackChannels.Remove(channel);
            }
        }

        public void Abort() {
            _trayInteractionImplementation.Abort();
        }

        public void Close() {
            _trayInteractionImplementation.Close();
        }

        public void Close(TimeSpan timeout) {
            _trayInteractionImplementation.Close(timeout);
        }

        public IAsyncResult BeginClose(AsyncCallback callback, object state) {
            return _trayInteractionImplementation.BeginClose(callback, state);
        }

        public IAsyncResult BeginClose(TimeSpan timeout, AsyncCallback callback, object state) {
            return _trayInteractionImplementation.BeginClose(timeout, callback, state);
        }

        public void EndClose(IAsyncResult result) {
            _trayInteractionImplementation.EndClose(result);
        }

        public void Open() {
            _trayInteractionImplementation.Open();
        }

        public void Open(TimeSpan timeout) {
            _trayInteractionImplementation.Open(timeout);
        }

        public IAsyncResult BeginOpen(AsyncCallback callback, object state) {
            return _trayInteractionImplementation.BeginOpen(callback, state);
        }

        public IAsyncResult BeginOpen(TimeSpan timeout, AsyncCallback callback, object state) {
            return _trayInteractionImplementation.BeginOpen(timeout, callback, state);
        }

        public void EndOpen(IAsyncResult result) {
            _trayInteractionImplementation.EndOpen(result);
        }

        public CommunicationState State => _trayInteractionImplementation.State;

        public event EventHandler Closed {
            add => _trayInteractionImplementation.Closed += value;
            remove => _trayInteractionImplementation.Closed -= value;
        }

        public event EventHandler Closing {
            add => _trayInteractionImplementation.Closing += value;
            remove => _trayInteractionImplementation.Closing -= value;
        }

        public event EventHandler Faulted {
            add => _trayInteractionImplementation.Faulted += value;
            remove => _trayInteractionImplementation.Faulted -= value;
        }

        public event EventHandler Opened {
            add => _trayInteractionImplementation.Opened += value;
            remove => _trayInteractionImplementation.Opened -= value;
        }

        public event EventHandler Opening {
            add => _trayInteractionImplementation.Opening += value;
            remove => _trayInteractionImplementation.Opening -= value;
        }
    }
}
