using FileManager.UI.Models;
using FileManager.UI.Models.JobModels.JobStepModels;
using HBLibrary.Wpf.Commands;
using HBLibrary.Wpf.Models;
using HBLibrary.Wpf.ViewModels;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

    private string? source;
    public string? Source {
        get => source;
        set {
            source = value;
            NotifyPropertyChanged();

            AddSourceCommand.NotifyCanExecuteChanged();
        }
    }

    private FileEntryType? sourceType;
    public FileEntryType? SourceType {
        get => sourceType;
        set {
            sourceType = value;
            NotifyPropertyChanged();

            Source = null;
            BrowseSourceCommand.NotifyCanExecuteChanged();
        }
    }

    private string? destination;
    public string? Destination {
        get => destination;
        set {
            destination = value;
            NotifyPropertyChanged();

            AddDestinationCommand.NotifyCanExecuteChanged();
        }
    }

    private FileEntryType? destinationType;
    public FileEntryType? DestinationType {
        get => destinationType;
        set {
            destinationType = value;
            NotifyPropertyChanged();

            Destination = null;
            BrowseDestinationCommand.NotifyCanExecuteChanged();
        }
    }


    public FileEntryType[] AvailableSourceTypes => [FileEntryType.Directory, FileEntryType.File];
    public FileEntryType[] AvailableDestinationTypes => [FileEntryType.Directory];


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

    public string? TimeDifferenceText {
        get => TypedModel.TimeDifferenceText;
        set {
            TypedModel.TimeDifferenceText = value;
            NotifyPropertyChanged();
        }
    }
    
    public TimeUnit? TimeDifferenceUnit {
        get => TypedModel.TimeDifferenceUnit;
        set {
            TypedModel.TimeDifferenceUnit = value;
            NotifyPropertyChanged();
        }
    }

    public ObservableCollection<FileEntryWrapper> SourceItems { get; set; }
    public ObservableCollection<FileEntryWrapper> DestinationItems { get; set; }

    private FileEntryWrapper? selectedSource;
    public FileEntryWrapper? SelectedSource {
        get { return selectedSource; }
        set {
            selectedSource = value;
            NotifyPropertyChanged();
        }
    }

    private FileEntryWrapper? selectedDestination;
    public FileEntryWrapper? SelectedDestination {
        get { return selectedDestination; }
        set {
            selectedDestination = value;
            NotifyPropertyChanged();
        }
    }

    public RelayCommand BrowseSourceCommand { get; set; }
    public RelayCommand BrowseDestinationCommand { get; set; }
    public RelayCommand ToggleInfoPopupCommand { get; set; }

    public RelayCommand AddSourceCommand { get; set; }
    public RelayCommand AddDestinationCommand { get; set; }
    public RelayCommand<FileEntryWrapper> DeleteSourceCommand { get; set; }
    public RelayCommand<FileEntryWrapper> DeleteDestinationCommand { get; set; }
    public CopyStepViewModel(CopyStepModel model) : base(model) {
        SourceItems = new ObservableCollection<FileEntryWrapper>(TypedModel.SourceItems);

        // Refresh Model collection on change
        SourceItems.CollectionChanged += (_, _) => {
            TypedModel.SourceItems.Clear();
            TypedModel.SourceItems.AddRange(SourceItems);
        };
        
        DestinationItems = new ObservableCollection<FileEntryWrapper>(TypedModel.DestinationItems);

        // Refresh Model collection on change
        DestinationItems.CollectionChanged += (_, _) => {
            TypedModel.DestinationItems.Clear();
            TypedModel.DestinationItems.AddRange(DestinationItems);
        };

        BrowseSourceCommand = new RelayCommand(BrowseSource, _ => SourceType is not null);
        BrowseDestinationCommand = new RelayCommand(BrowseDestination, _ => DestinationType is not null);
        ToggleInfoPopupCommand = new RelayCommand(ToggleInfoPopup, true);

        AddSourceCommand = new RelayCommand(AddSource, _ => SourceType is not null && Source is not null);
        AddDestinationCommand = new RelayCommand(AddDestination, _ => DestinationType is not null && Destination is not null);

        DeleteSourceCommand = new RelayCommand<FileEntryWrapper>(DeleteSource, true);
        DeleteDestinationCommand = new RelayCommand<FileEntryWrapper>(DeleteDestination, true);

        TimeDifference = model.TimeDifference;
        TimeDifferenceUnit = model.TimeDifferenceUnit;
    }

    private void DeleteDestination(FileEntryWrapper obj) {
        DestinationItems.Remove(obj);
    }

    private void DeleteSource(FileEntryWrapper obj) {
        SourceItems.Remove(obj);
    }

    private void AddDestination(object? obj) {
        DestinationItems.Add(new FileEntryWrapper {
            Type = DestinationType!.Value,
            Path = Destination!
        });

        Destination = null;
        DestinationType = null;
    }

    private void AddSource(object? obj) {
        SourceItems.Add(new FileEntryWrapper {
            Type = SourceType!.Value,
            Path = Source!
        });

        Source = null;
        SourceType = null;
    }

    private void ToggleInfoPopup(object? obj) {
        IsInfoPopupOpen = !IsInfoPopupOpen;
    }

    private void BrowseSource(object? obj) {
        switch (SourceType) {
            case FileEntryType.File:
                OpenFileDialog fileDialog = new OpenFileDialog {
                    Title = "Select source file"
                };

                if (fileDialog.ShowDialog() is true) {
                    Source = fileDialog.FileName;
                }

                break;
            case FileEntryType.Directory:
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
            case FileEntryType.File:
                OpenFileDialog fileDialog = new OpenFileDialog();
                fileDialog.Title = "Select destination file";

                if (fileDialog.ShowDialog() is true) {
                    Destination = fileDialog.FileName;
                }

                break;
            case FileEntryType.Directory:
                OpenFolderDialog folderDialog = new OpenFolderDialog();
                folderDialog.Title = "Select destination folder";

                if (folderDialog.ShowDialog() is true) {
                    Destination = folderDialog.FolderName;
                }

                break;
        }
    }
}
