using PlexServiceTray.ViewModel;

namespace PlexServiceTray.Mock
{
    public class MockSettingsViewModel:SettingsViewModel
    {
        public MockSettingsViewModel():base(new PlexServiceCommon.Settings(), "Dark Amber")
        {
            ServerPort = 8787;

            AutoRemount = true;
            AutoRemountCount = 2;
            AutoRestart = true;

            //add some mock data
            AuxiliaryApplications.Add(new AuxiliaryApplicationViewModel(new PlexServiceCommon.AuxiliaryApplication()
            {
                Name = "My Aux Application",
                FilePath = @"C:\Something\execute_me.exe",
                LogOutput = true,
                Argument = "-i someExtraInfo",
                KeepAlive = true,
                WorkingFolder = @"C:\Something",
                Url = "https://auxiliaryapps.com"
            }, this));

            AuxiliaryApplications.Add(new AuxiliaryApplicationViewModel(new PlexServiceCommon.AuxiliaryApplication()
            {
                Name = "Another Aux Application",
                FilePath = @"C:\Something\do_not_execute_me.exe",
                LogOutput = true,
                Argument = "--help",
                KeepAlive = false,
                WorkingFolder = @"C:\Something",
                Url = "https://bad.com"
            }, this));

            DriveMaps.Add(new DriveMapViewModel(new PlexServiceCommon.DriveMap(@"\\my_server\media", @"M")));
            DriveMaps.Add(new DriveMapViewModel(new PlexServiceCommon.DriveMap(@"\\my_server\photos", @"P")));
        }
    }
}
