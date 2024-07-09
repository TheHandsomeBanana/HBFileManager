﻿using FileManager.UI.Models.SettingsPageModels;
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

namespace FileManager.UI.ViewModels.SettingsViewModels {
    public class SettingsWinRARViewModel : ViewModelBase<SettingsWinRARModel> {

        public RelayCommand DetectWinRARCommand { get; set; }
        public RelayCommand BrowseLocationCommand { get; set; }

        public bool UseWinRAR {
            get {
                return Model.UseWinRAR;
            }
            set {

                if(!value) {
                    Location = "";
                    LicenseKeyLocation = "";
                    ErrorText = "";
                    ErrorTextVisibility = Visibility.Collapsed;
                }

                Model.UseWinRAR = value;
                NotifyPropertyChanged();
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

        private Visibility errorTextVisibility = Visibility.Collapsed;
        public Visibility ErrorTextVisibility {
            get => errorTextVisibility;
            set {
                errorTextVisibility = value;
                NotifyPropertyChanged();
            }
        }

        public string LicenseKeyLocation {
            get => Model.LicenseKeyLocation;
            set {
                Model.LicenseKeyLocation = value;
                NotifyPropertyChanged();
            }
        }


        private string errorText = "";
        public string ErrorText {
            get => errorText;
            set {
                errorText = value;
                NotifyPropertyChanged();
            }
        }


        public SettingsWinRARViewModel() {
            Model = new SettingsWinRARModel();
            DetectWinRARCommand = new RelayCommand(DetectWinRARInstallation, true);
            BrowseLocationCommand = new RelayCommand(BrowseLocation, true);
        }

        public SettingsWinRARViewModel(SettingsWinRARModel model) : this() {
            this.Model = model;

            if (UseWinRAR && CheckProvidedLocation(model.Location)) {
                ValidateWinRARLicense();
            }
        }

        private void DetectWinRARInstallation(object? obj) {
            Location = WinRARManager.GetWinRARInstallationPath() ?? "";
        }

        private void BrowseLocation(object? obj) {
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
