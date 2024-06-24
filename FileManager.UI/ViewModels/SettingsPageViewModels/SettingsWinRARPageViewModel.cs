using FileManager.UI.Models.SettingsPageModels;
using HBLibrary.Services.IO.Archiving.WinRAR;
using HBLibrary.Wpf.Commands;
using HBLibrary.Wpf.ViewModels;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Navigation;

namespace FileManager.UI.ViewModels.SettingsPageViewModels {
    public class SettingsWinRARPageViewModel : ViewModelBase {
        private readonly SettingsWinRARPageModel model;

        public RelayCommand DetectWinRARCommand { get; set; }
        public RelayCommand BrowseLocationCommand { get; set; }

        public bool UseWinRAR {
            get {
                return model.UseWinRAR;
            }
            set {
                model.UseWinRAR = value;
                NotifyPropertyChanged();

                if(!value) {
                    Location = "";
                    LicenseKeyLocation = "";
                }
            }
        }

        public string Location {
            get => model.Location;
            set {
                model.Location = value;
                NotifyPropertyChanged();

                if (CheckProvidedLocation(value)) {
                    ValidateWinRARLicense();
                }
                else {
                    LicenseKeyLocation = "";
                }
            }
        }

        private Visibility errorTextVisibility = Visibility.Collapsed;
        public Visibility ErrorTextVisibility {
            get => errorTextVisibility;
            set {
                if (!UseWinRAR)
                    return;

                errorTextVisibility = value;
                NotifyPropertyChanged();
            }
        }

        public string LicenseKeyLocation {
            get => model.LicenseKeyLocation;
            set {
                model.LicenseKeyLocation = value;
                NotifyPropertyChanged();
            }
        }


        private string errorText = "";
        public string ErrorText {
            get => errorText;
            set {
                if (!UseWinRAR)
                    return;

                errorText = value;
                NotifyPropertyChanged();
            }
        }


        public SettingsWinRARPageViewModel() {
            model = new SettingsWinRARPageModel();
            DetectWinRARCommand = new RelayCommand(DetectWinRARInstallation, true);
            BrowseLocationCommand = new RelayCommand(BrowseLocation, true);
        }

        public SettingsWinRARPageViewModel(SettingsWinRARPageModel model) : this() {
            this.model = model;
        }

        private void DetectWinRARInstallation(object obj) {
            Location = WinRARManager.GetWinRARInstallationPath() ?? "";
        }

        private void BrowseLocation(object obj) {
            OpenFolderDialog openFolderDialog = new OpenFolderDialog();
            openFolderDialog.Multiselect = false;
            if(openFolderDialog.ShowDialog() ?? false) {
                Location = openFolderDialog.FolderName;
            }
        }

        private bool CheckProvidedLocation(string value) {
            if (!WinRARManager.CheckWinRARInstallationForPath(value)) {
                ErrorText = "WinRAR not found in this directory";
                ErrorTextVisibility = Visibility.Visible;
                return false;
            }

            ErrorTextVisibility = Visibility.Collapsed;
            ErrorText = "";
            return true;
        }

        private void ValidateWinRARLicense() {
            if(WinRARManager.CheckWinRARLicense(out string licenseKey)) {
                LicenseKeyLocation = licenseKey;
                ErrorTextVisibility = Visibility.Collapsed;
                ErrorText = "";
            }
            else {
                ErrorText = "WinRAR license is invalid.";
                ErrorTextVisibility = Visibility.Visible;
            }
        }
    }
}
