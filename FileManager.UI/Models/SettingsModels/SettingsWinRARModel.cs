using HBLibrary.Wpf.Models;

namespace FileManager.UI.Models.SettingsModels;
public class SettingsWinRARModel : TrackableModel {
    private bool useWinRAR;
    public bool UseWinRAR { 
        get => useWinRAR; 
        set {
            useWinRAR = value;
            NotifyTrackableChanged(value);
        }
    }

    private string location = "";
    public string Location { 
        get => location; 
        set {
            location = value;
            NotifyTrackableChanged(value);
        } 
    }

    private string licenseKeyLocation = "";
    public string LicenseKeyLocation { 
        get => licenseKeyLocation; 
        set {
            licenseKeyLocation = value;
            NotifyTrackableChanged(value);
        } 
    }
}
