using FileManager.UI.Models.SettingsModels;
using HBLibrary.IO.Archiving.WinRAR;
using HBLibrary.Wpf.Commands;
using HBLibrary.Wpf.ViewModels;
using Microsoft.Win32;

namespace FileManager.UI.ViewModels.SettingsViewModels {
    public class SettingsWinRARViewModel : ViewModelBase<SettingsWinRARModel> {

        public RelayCommand DetectWinRARCommand { get; set; }
        public RelayCommand BrowseLocationCommand { get; set; }

        public bool UseWinRAR {
            get {
                return Model.UseWinRAR;
            }
            set {
                if (!value) {
                    Location = "";
                    LicenseKeyLocation = "";
                    LocationErrorText = "";
                    Model.UseWinRAR = value;
                }
                else {
                    DetectWinRARInstallation(null);

                    if (CheckProvidedLocation(Location)) {
                        Model.UseWinRAR = value;
                        NotifyPropertyChanged();

                        ValidateWinRARLicense();
                    }
                }
            }
        }

        public string Location {
            get => Model.Location;
            set {
                Model.Location = value;
                NotifyPropertyChanged();

                if (!UseWinRAR) {
                    return;
                }

                if (CheckProvidedLocation(value)) {
                    ValidateWinRARLicense();
                }
                else {
                    LicenseKeyLocation = "";
                }
            }
        }

        public string LicenseKeyLocation {
            get => Model.LicenseKeyLocation;
            set {
                Model.LicenseKeyLocation = value;
                NotifyPropertyChanged();
            }
        }


        private string locationErrorText = "";
        public string LocationErrorText {
            get => locationErrorText;
            set {
                locationErrorText = value;
                NotifyPropertyChanged();
            }
        }

        private string licenseKeyLocationErrorText = "";
        public string LicenseKeyLocationErrorText {
            get => licenseKeyLocationErrorText;
            set {
                licenseKeyLocationErrorText = value;
                NotifyPropertyChanged();
            }
        }


        public SettingsWinRARViewModel(SettingsWinRARModel model) : base(model) {
            DetectWinRARCommand = new RelayCommand(DetectWinRARInstallation, true);
            BrowseLocationCommand = new RelayCommand(BrowseLocation, true);
        }

        private void DetectWinRARInstallation(object? obj) {
            Location = WinRARManager.GetWinRARInstallationPath() ?? "";
        }

        private void BrowseLocation(object? obj) {
            OpenFolderDialog openFolderDialog = new OpenFolderDialog();
            openFolderDialog.Multiselect = false;
            if (openFolderDialog.ShowDialog() ?? false) {
                Location = openFolderDialog.FolderName;
            }
        }

        private bool CheckProvidedLocation(string value) {
            if (!WinRARManager.CheckWinRARInstallationForPath(value)) {
                LocationErrorText = "WinRAR not found";
                return false;
            }

            LocationErrorText = "";
            return true;
        }

        private void ValidateWinRARLicense() {
            if (WinRARManager.CheckWinRARLicense(out string licenseKey)) {
                LicenseKeyLocation = licenseKey;
                LicenseKeyLocationErrorText = "";
            }
            else {
                LicenseKeyLocationErrorText = "WinRAR license is invalid.";
            }
        }
    }
}
