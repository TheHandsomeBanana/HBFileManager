using HBLibrary.Wpf.Models;

namespace FileManager.UI.Models.SettingsModels;
public class SettingsEnvironmentModel : TrackableModel {
    public bool validateOnNavigation;
    public bool ValidateOnNavigation { 
        get => validateOnNavigation; 
        set {
            validateOnNavigation = value;
            NotifyTrackableChanged(value);
        }
    }
}
