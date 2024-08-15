using FileManager.UI.Models;
using FileManager.UI.Models.JobModels.JobStepModels;
using HBLibrary.Wpf.Commands;
using HBLibrary.Wpf.ViewModels;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileManager.UI.ViewModels.JobViewModels.JobStepViewModels;
public class CopyStepViewModel : JobItemStepViewModel {
    public CopyStepModel TypedModel => (CopyStepModel)Model;

    private bool isInfoPopupOpen;
    public bool IsInfoPopupOpen {
        get => isInfoPopupOpen;
        set {
            isInfoPopupOpen = value;
            NotifyPropertyChanged();
        }
    }

    public string? Source {
        get => TypedModel.Source;
        set {
            TypedModel.Source = value;
            NotifyPropertyChanged();
        }
    }

    public TargetType? SourceType {
        get => TypedModel.SourceType;
        set {
            TypedModel.SourceType = value;
            NotifyPropertyChanged();
            UpdateAvailableDestinationTypes();

            Source = null;

            if (value == TargetType.Directory && DestinationType == TargetType.File) {
                DestinationType = null;
                Destination = null;
            }
        }
    }

    public string? Destination {
        get => TypedModel.Destination;
        set {
            TypedModel.Destination = value;
            NotifyPropertyChanged();
        }
    }

    public TargetType? DestinationType {
        get => TypedModel.DestinationType;
        set {
            TypedModel.DestinationType = value;
            NotifyPropertyChanged();

            Destination = null;
        }
    }

    public List<TargetType> availableDestinationTypes = [];
    public List<TargetType> AvailableDestinationTypes {
        get => availableDestinationTypes;
        set {
            availableDestinationTypes = value;
            NotifyPropertyChanged();
        }
    }

    public bool ModifiedOnly {
        get => TypedModel.ModifiedOnly;
        set {
            TypedModel.ModifiedOnly = value;
            NotifyPropertyChanged();
        }
    }

    public TimeSpan? TimeDifference {
        get => TypedModel.TimeDifference;
        set {
            TypedModel.TimeDifference = value;
            NotifyPropertyChanged();
        }
    }

    public RelayCommand BrowseSourceCommand { get; set; }
    public RelayCommand BrowseDestinationCommand { get; set; }
    public RelayCommand ToggleInfoPopupCommand { get; set; }

    public CopyStepViewModel(CopyStepModel model) : base(model) {
        BrowseSourceCommand = new RelayCommand(BrowseSource, SourceType != null);
        BrowseDestinationCommand = new RelayCommand(BrowseDestination, DestinationType != null);
        ToggleInfoPopupCommand = new RelayCommand(ToggleInfoPopup, true);

        UpdateAvailableDestinationTypes();
    }

    private void ToggleInfoPopup(object? obj) {
        IsInfoPopupOpen = !IsInfoPopupOpen;
    }

    private void BrowseSource(object? obj) {
        switch (SourceType) {
            case TargetType.File:
                OpenFileDialog fileDialog = new OpenFileDialog {
                    Title = "Select source file"
                };

                if (fileDialog.ShowDialog() is true) {
                    Source = fileDialog.FileName;
                }

                break;
            case TargetType.Directory:
                OpenFolderDialog folderDialog = new OpenFolderDialog {
                    Title = "Select source folder"
                };

                if (folderDialog.ShowDialog() is true) {
                    Source = folderDialog.FolderName;
                }

                break;
        }
    }

    private void BrowseDestination(object? obj) {
        switch (DestinationType) {
            case TargetType.File:
                OpenFileDialog fileDialog = new OpenFileDialog();
                fileDialog.Title = "Select destination file";

                if (fileDialog.ShowDialog() is true) {
                    Destination = fileDialog.FileName;
                }

                break;
            case TargetType.Directory:
                OpenFolderDialog folderDialog = new OpenFolderDialog();
                folderDialog.Title = "Select destination folder";

                if (folderDialog.ShowDialog() is true) {
                    Destination = folderDialog.FolderName;
                }

                break;
        }
    }

    private void UpdateAvailableDestinationTypes() {
        if (SourceType == TargetType.Directory) {
            AvailableDestinationTypes = [TargetType.Directory];
        }
        else {
            AvailableDestinationTypes = [TargetType.File, TargetType.Directory];
        }
    }
}
