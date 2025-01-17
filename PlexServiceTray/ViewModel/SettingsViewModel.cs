﻿using System;
using System.Linq;
using System.Collections.ObjectModel;
using PlexServiceCommon;
using Microsoft.Win32;
using System.IO;

namespace PlexServiceTray.ViewModel
{
    public class SettingsViewModel : ObservableObject
    {
        /// <summary>
        /// The server endpoint port
        /// </summary>
        public int ServerPort
        {
            get => WorkingSettings.ServerPort;
            set
            {
                if (WorkingSettings.ServerPort == value) return;

                WorkingSettings.ServerPort = value;
                OnPropertyChanged(nameof(ServerPort));
            }
        }

        /// <summary>
        /// Plex restart delay
        /// </summary>
        public int RestartDelay
        {
            get => WorkingSettings.RestartDelay;
            set
            {
                if (WorkingSettings.RestartDelay == value) return;

                WorkingSettings.RestartDelay = value;
                OnPropertyChanged(nameof(RestartDelay));
            }
        }

        public bool AutoRestart
        {
            get => WorkingSettings.AutoRestart;
            set
            {
                if (WorkingSettings.AutoRestart == value) return;

                WorkingSettings.AutoRestart = value;
                OnPropertyChanged(nameof(AutoRestart));
            }
        }

        public bool AutoRemount
        {
            get => WorkingSettings.AutoRemount;
            set
            {
                if (WorkingSettings.AutoRemount == value) return;

                WorkingSettings.AutoRemount = value;
                OnPropertyChanged(nameof(AutoRemount));
            }
        }

        public int AutoRemountCount
        {
            get => WorkingSettings.AutoRemountCount;
            set
            {
                if (WorkingSettings.AutoRemountCount == value) return;

                WorkingSettings.AutoRemountCount = value;
                OnPropertyChanged(nameof(AutoRemountCount));
            }
        }

        public int AutoRemountDelay
        {
            get => WorkingSettings.AutoRemountDelay;
            set
            {
                if (WorkingSettings.AutoRemountDelay == value) return;

                WorkingSettings.AutoRemountDelay = value;
                OnPropertyChanged(nameof(AutoRemountDelay));
            }
        }

        public bool StartPlexOnMountFail
        {
            get => WorkingSettings.StartPlexOnMountFail;
            set
            {
                if (WorkingSettings.StartPlexOnMountFail == value) return;

                WorkingSettings.StartPlexOnMountFail = value;
                OnPropertyChanged(nameof(StartPlexOnMountFail));
            }
        }

        public string? UserDefinedInstallLocation
        {
            get => WorkingSettings.UserDefinedInstallLocation;
            set
            {
                if (WorkingSettings.UserDefinedInstallLocation == value) return;
                WorkingSettings.UserDefinedInstallLocation = value;
                OnPropertyChanged(nameof(UserDefinedInstallLocation));
            }
        }


        private int _selectedTab;

        public int SelectedTab
        {
            get => _selectedTab;
            set
            {
                if (_selectedTab == value) return;

                _selectedTab = value;
                OnPropertyChanged(nameof(SelectedTab));
                OnPropertyChanged(nameof(RemoveToolTip));
                OnPropertyChanged(nameof(AddToolTip));
            }
        }

        /// <summary>
        /// Collection of Auxiliary applications to run alongside plex
        /// </summary>
        public ObservableCollection<AuxiliaryApplicationViewModel> AuxiliaryApplications { get; } = new();

        private AuxiliaryApplicationViewModel? _selectedAuxApplication;

        public AuxiliaryApplicationViewModel? SelectedAuxApplication
        {
            get => _selectedAuxApplication;
            set
            {
                if (_selectedAuxApplication != value)
                {
                    _selectedAuxApplication = value;
                    OnPropertyChanged(nameof(SelectedAuxApplication));
                    OnPropertyChanged(nameof(RemoveToolTip));
                }
            }
        }


        public ObservableCollection<DriveMapViewModel> DriveMaps { get; } = new();

        private DriveMapViewModel? _selectedDriveMap;

        public DriveMapViewModel? SelectedDriveMap
        {
            get => _selectedDriveMap;
            set
            {
                if (_selectedDriveMap == value)
                {
                    return;
                }

                _selectedDriveMap = value;
                OnPropertyChanged(nameof(SelectedDriveMap));
                OnPropertyChanged(nameof(RemoveToolTip));
            }
        }

        private string _theme;

        public string Theme
        {
            get => _theme;
            set
            {
                if (_theme == value) return;
                _theme = value;
                OnPropertyChanged(nameof(Theme));
            }
        }

        public ObservableCollection<string> Themes { get; } = new(TrayApplicationSettings.Themes);

        public string RemoveToolTip
        {
            get
            {
                switch (SelectedTab)
                {
                    case 0:
                        if (SelectedAuxApplication != null)
                        {
                            return "Remove " + SelectedAuxApplication.Name;
                        }
                        break;
                    case 1:
                        if (SelectedDriveMap != null)
                        {
                            return "Remove Drive Map " + SelectedDriveMap.DriveLetter + " -> " + SelectedDriveMap.ShareName;
                        }
                        break;
                }
                return "Nothing selected!";
            }
        }

        public string? AddToolTip
        {
            get
            {
                return SelectedTab switch
                {
                    0 => "Add Auxiliary Application",
                    1 => "Add Drive Map",
                    _ => null
                };
            }
        }

        private bool? _dialogResult;

        public bool? DialogResult
        {
            get => _dialogResult;
            set
            {
                if (_dialogResult == value) return;
                _dialogResult = value;
                OnPropertyChanged(nameof(DialogResult));
            }
        }

        /// <summary>
        /// Use one settings instance for the life of the window.
        /// </summary>
        public readonly Settings WorkingSettings;

        public SettingsViewModel(Settings? settings, string theme)
        {
            WorkingSettings = settings ?? new Settings();
            _theme = theme;

            WorkingSettings.AuxiliaryApplications.ForEach(x =>
            {
                AuxiliaryApplicationViewModel auxApp = new(x, this);
                auxApp.StartRequest += OnAuxAppStartRequest;
                auxApp.StopRequest += OnAuxAppStopRequest;
                auxApp.CheckRunningRequest += OnAuxAppCheckRunRequest;
                AuxiliaryApplications.Add(auxApp);
            });

            WorkingSettings.DriveMaps.ForEach(x => DriveMaps.Add(new DriveMapViewModel(x)));

            if (AuxiliaryApplications.Count > 0)
            {
                AuxiliaryApplications[0].IsExpanded = true;
            }
        }

        /// <summary>
        /// Allow the user to add a new Auxiliary application
        /// </summary>
        #region AddCommand

        private RelayCommand? _addCommand;
        public RelayCommand AddCommand => _addCommand ??= new RelayCommand(OnAdd);

        private void OnAdd(object? parameter)
        {
            switch (SelectedTab)
            {
                case 0:
                    AuxiliaryApplication newAuxApp = new()
                    {
                        Name = "New Auxiliary Application"
                    };
                    AuxiliaryApplicationViewModel newAuxAppViewModel = new(newAuxApp, this);
                    newAuxAppViewModel.StartRequest += OnAuxAppStartRequest;
                    newAuxAppViewModel.StopRequest += OnAuxAppStopRequest;
                    newAuxAppViewModel.CheckRunningRequest += OnAuxAppCheckRunRequest;
                    newAuxAppViewModel.IsExpanded = true;
                    AuxiliaryApplications.Add(newAuxAppViewModel);
                    break;
                case 1:
                    DriveMap newDriveMap = new(@"\\computer\share", "Z");
                    DriveMapViewModel newDriveMapViewModel = new(newDriveMap);
                    DriveMaps.Add(newDriveMapViewModel);
                    break;
            }

        }

        #endregion AddCommand

        /// <summary>
        /// Allow the user brose to the plex executable
        /// </summary>
        #region BrowseForPlexCommand

        private RelayCommand? _browseForPlexCommand;
        public RelayCommand BrowseForPlexCommand => _browseForPlexCommand ??= new RelayCommand(OnBrowseForPlex);

        private void OnBrowseForPlex(object? parameter)
        {
            string? initialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
            if (!string.IsNullOrEmpty(UserDefinedInstallLocation))
            {
                initialDirectory = Path.GetDirectoryName(UserDefinedInstallLocation);
            }
            OpenFileDialog ofd = new()
            {
                FileName = "Plex Media Server.exe",
                Filter = "Executable Files *.exe|*.exe",
                InitialDirectory = initialDirectory,
                CheckPathExists = true,
                CheckFileExists = true,
                Title = "Locate Plex Media Server"
            };
            if (ofd.ShowDialog() != true)
            {
                return;
            }

            UserDefinedInstallLocation = ofd.FileName;
        }

        #endregion BrowseForPlexCommand

        /// <summary>
        /// Remove the selected auxiliary application
        /// </summary>
        #region RemoveCommand

        private RelayCommand? _removeCommand;
        public RelayCommand RemoveCommand => _removeCommand ??= new RelayCommand(OnRemove, CanRemove);

        private bool CanRemove(object? parameter)
        {
            return SelectedTab switch
            {
                0 => parameter is AuxiliaryApplicationViewModel,
                1 => parameter is DriveMapViewModel,
                _ => false,
            };
        }

        private void OnRemove(object? parameter)
        {
            switch (SelectedTab)
            {
                case 0:
                    if (parameter is AuxiliaryApplicationViewModel auxApp)
                    {
                        auxApp.StartRequest -= OnAuxAppStartRequest;
                        auxApp.StopRequest -= OnAuxAppStopRequest;
                        AuxiliaryApplications.Remove(auxApp);
                    }
                    break;
                case 1:
                    if (parameter is DriveMapViewModel map)
                        DriveMaps.Remove(map);
                    break;
            }

        }

        #endregion RemoveCommand

        /// <summary>
        /// Save the settings file
        /// </summary>
        #region SaveCommand

        private RelayCommand? _saveCommand;
        public RelayCommand SaveCommand => _saveCommand ??= new RelayCommand(OnSave, CanSave);

        private bool CanSave(object? parameter)
        {
            return ServerPort > 0 && string.IsNullOrEmpty(Error) && !AuxiliaryApplications.Any(a => !string.IsNullOrEmpty(a.Error) || string.IsNullOrEmpty(a.Name)) && !DriveMaps.Any(dm => !string.IsNullOrEmpty(dm.Error) || string.IsNullOrEmpty(dm.ShareName) || string.IsNullOrEmpty(dm.DriveLetter));
        }

        private void OnSave(object? parameter)
        {
            WorkingSettings.AuxiliaryApplications.Clear();
            foreach (AuxiliaryApplicationViewModel aux in AuxiliaryApplications)
            {
                WorkingSettings.AuxiliaryApplications.Add(aux.GetAuxiliaryApplication());
            }
            WorkingSettings.DriveMaps.Clear();
            foreach (DriveMapViewModel dMap in DriveMaps)
            {
                WorkingSettings.DriveMaps.Add(dMap.GetDriveMap());
            }
            DialogResult = true;
        }

        #endregion SaveCommand

        /// <summary>
        /// Close the dialogue without saving changes
        /// </summary>
        #region CancelCommand

        private RelayCommand? _cancelCommand;
        public RelayCommand CancelCommand => _cancelCommand ??= new RelayCommand(OnCancel);

        private void OnCancel(object? parameter)
        {
            DialogResult = false;
        }

        #endregion CancelCommand

        #region Aux app start/stop request handling

        private void OnAuxAppStopRequest(object? sender, EventArgs e)
        {
            AuxAppStopRequest?.Invoke(sender, e);
        }

        public event EventHandler? AuxAppStopRequest;

        private void OnAuxAppStartRequest(object? sender, EventArgs e)
        {
            AuxAppStartRequest?.Invoke(sender, e);
        }

        public event EventHandler? AuxAppStartRequest;

        private void OnAuxAppCheckRunRequest(object? sender, EventArgs e)
        {
            AuxAppCheckRunRequest?.Invoke(sender, e);
        }

        public event EventHandler? AuxAppCheckRunRequest;

        #endregion
    }
}
