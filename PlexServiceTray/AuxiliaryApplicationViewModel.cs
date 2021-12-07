﻿using System;
using PlexServiceCommon;
using System.Windows.Input;
using Microsoft.Win32;
using Ookii.Dialogs.Wpf;
using PlexServiceTray.Validation;
using System.ComponentModel.DataAnnotations;

namespace PlexServiceTray
{
    public class AuxiliaryApplicationViewModel:ObservableObject
    {
        #region Properties

        [UniqueAuxAppName]
        public string Name
        {
            get => _auxApplication.Name;
            set {
                if (_auxApplication.Name == value) {
                    return;
                }

                _auxApplication.Name = value;
                OnPropertyChanged("Name");
            }
        }

        [Required(ErrorMessage ="A path to execute must be specified")]
        public string FilePath
        {
            get => _auxApplication.FilePath;
            set {
                if (_auxApplication.FilePath == value) {
                    return;
                }

                _auxApplication.FilePath = value;
                OnPropertyChanged("FilePath");
            }
        }

        public string WorkingFolder
        {
            get => _auxApplication.WorkingFolder;
            set {
                if (_auxApplication.WorkingFolder == value) {
                    return;
                }

                _auxApplication.WorkingFolder = value;
                OnPropertyChanged("WorkingFolder");
            }
        }

        public string Argument
        {
            get => _auxApplication.Argument;
            set {
                if (_auxApplication.Argument == value) {
                    return;
                }

                _auxApplication.Argument = value;
                OnPropertyChanged("Argument");
            }
        }

        public bool KeepAlive
        {
            get => _auxApplication.KeepAlive;
            set
            {
                if (_auxApplication.KeepAlive != value)
                {
                    _auxApplication.KeepAlive = value;
                    OnPropertyChanged("KeepAlive");
                }
            }
        }
        
        public bool LogOutput
        {
            get => _auxApplication.LogOutput;
            set
            {
                if (_auxApplication.LogOutput != value)
                {
                    _auxApplication.LogOutput = value;
                    OnPropertyChanged("LogOutput");
                }
            }
        }

        private bool _running;

        public bool Running
        {
            get => _running;
            set {
                if (_running == value) {
                    return;
                }

                _running = value;
                OnPropertyChanged("Running");
            }
        }

        public string Url
        {
            get => _auxApplication.Url;
            set {
                if (_auxApplication.Url == value) {
                    return;
                }

                _auxApplication.Url = value;
                OnPropertyChanged("Url");
            }
        }

        #endregion Properties

        private readonly AuxiliaryApplication _auxApplication;

        public AuxiliaryApplicationViewModel(AuxiliaryApplication auxApplication, SettingsWindowViewModel context)
        {
            ValidationContext = context;
            _auxApplication = auxApplication;
            IsExpanded = false;
        }

        public AuxiliaryApplication GetAuxiliaryApplication()
        {
            return _auxApplication;
        }

        #region BrowseCommand
        RelayCommand _browseCommand;
        public ICommand BrowseCommand
        {
            get { return _browseCommand ??= new RelayCommand(_ => OnBrowse(), _ => CanBrowse()); }
        }

        private static bool CanBrowse()
        {
            return true;
        }

        private void OnBrowse()
        {
            var ofd = new OpenFileDialog {
                FileName = FilePath
            };
            if (ofd.ShowDialog() != true) {
                return;
            }

            FilePath = ofd.FileName;
            if(string.IsNullOrEmpty(WorkingFolder))
            {
                WorkingFolder = System.IO.Path.GetDirectoryName(FilePath);
            }
        }

        #endregion BrowseCommand

        #region BrowseFolderCommand
        RelayCommand _browseFolderCommand;
        public ICommand BrowseFolderCommand
        {
            get {
                return _browseFolderCommand ??= new RelayCommand(OnBrowseFolder, CanBrowseFolder);
            }
        }

        private static bool CanBrowseFolder(object parameter)
        {
            return true;
        }

        private void OnBrowseFolder(object parameter)
        {
            var fbd = new VistaFolderBrowserDialog {
                Description = "Please select working directory",
                UseDescriptionForTitle = true
            };
            if (!string.IsNullOrEmpty(WorkingFolder))
            {
                fbd.SelectedPath = WorkingFolder;
            }
            if (fbd.ShowDialog() == true)
            {
                WorkingFolder = fbd.SelectedPath;
            }
        }

        #endregion BrowseFolderCommand

        #region StartCommand
        RelayCommand _startCommand;
        public ICommand StartCommand
        {
            get { return _startCommand ??= new RelayCommand(OnStart, CanStart); }
        }

        private bool CanStart(object parameter)
        {
            return !Running;
        }

        private void OnStart(object parameter)
        {
            StartRequest?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler StartRequest;

        #endregion StartCommand

        #region StopCommand
        RelayCommand _stopCommand;
        public ICommand StopCommand
        {
            get { return _stopCommand ??= new RelayCommand(OnStop, CanStop); }
        }

        private bool CanStop(object parameter)
        {
            return Running;
        }

        private void OnStop(object parameter)
        {
            StopRequest?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler StopRequest;

        #endregion StopCommand

        #region GoToUrlCommand
        RelayCommand _goToUrlCommand;
        public ICommand GoToUrlCommand
        {
            get {
                return _goToUrlCommand ??= new RelayCommand(OnGoToUrl, CanGoToUrl);
            }
        }

        private bool CanGoToUrl(object parameter)
        {
            return !string.IsNullOrEmpty(Url);
        }

        private void OnGoToUrl(object parameter)
        {
            System.Diagnostics.Process.Start(Url);
        }

        #endregion GoToUrlCommand

        #region CheckRunningRequest

        public event EventHandler CheckRunningRequest;

        private void OnCheckRunningRequest()
        {
            CheckRunningRequest?.Invoke(this, EventArgs.Empty);
        }

        #endregion

        public override bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;
                    OnPropertyChanged("IsSelected");
                    if (value)
                        OnCheckRunningRequest();
                }
            }
        }
        
    }
}
