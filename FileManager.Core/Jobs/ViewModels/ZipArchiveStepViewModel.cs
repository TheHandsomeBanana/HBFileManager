using FileManager.Core.Jobs.Models;
using FileManager.Core.JobSteps;
using HBLibrary.Wpf.Commands;
using HBLibrary.Wpf.Models;
using Microsoft.Win32;
using System.Collections.ObjectModel;

namespace FileManager.Core.Jobs.ViewModels;
public class ZipArchiveStepViewModel : JobStepViewModel<ZipArchiveStep> {
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

    private EntryBrowseType? sourceType;
    public EntryBrowseType? SourceBrowseType {
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

    private EntryBrowseType? destinationType;
    public EntryBrowseType? DestinationBrowseType {
        get => destinationType;
        set {
            destinationType = value;
            NotifyPropertyChanged();

            Destination = null;
            BrowseDestinationCommand.NotifyCanExecuteChanged();
        }
    }


    public EntryBrowseType[] AvailableSourceTypes => [EntryBrowseType.Directory, EntryBrowseType.File];
    public EntryBrowseType[] AvailableDestinationTypes => [EntryBrowseType.Directory];

    public bool ModifiedOnly {
        get => Model.ModifiedOnly;
        set {
            Model.ModifiedOnly = value;
            NotifyPropertyChanged();
        }
    }

    public TimeSpan? TimeDifference {
        get => Model.TimeDifference;
        set {
            Model.TimeDifference = value;
            NotifyPropertyChanged();
        }
    }

    public string? TimeDifferenceText {
        get => Model.TimeDifferenceText;
        set {
            Model.TimeDifferenceText = value;
            NotifyPropertyChanged();
        }
    }

    public TimeUnit? TimeDifferenceUnit {
        get => Model.TimeDifferenceUnit;
        set {
            Model.TimeDifferenceUnit = value;
            NotifyPropertyChanged();
        }
    }

    public ObservableCollection<Entry> SourceItems { get; set; }
    public ObservableCollection<Entry> DestinationItems { get; set; }

    private Entry? selectedSource;
    public Entry? SelectedSource {
        get { return selectedSource; }
        set {
            selectedSource = value;
            NotifyPropertyChanged();
        }
    }

    private Entry? selectedDestination;
    public Entry? SelectedDestination {
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
    public RelayCommand<Entry> DeleteSourceCommand { get; set; }
    public RelayCommand<Entry> DeleteDestinationCommand { get; set; }

    public ZipArchiveStepViewModel(ZipArchiveStep model) : base(model) {
        SourceItems = new ObservableCollection<Entry>(Model.SourceItems);

        // Refresh Model collection on change
        SourceItems.CollectionChanged += (_, _) => {
            Model.SourceItems.Clear();
            Model.SourceItems.AddRange(SourceItems);
            NotifyValidationRequired();
        };

        DestinationItems = new ObservableCollection<Entry>(Model.DestinationItems);

        // Refresh Model collection on change
        DestinationItems.CollectionChanged += (_, _) => {
            Model.DestinationItems.Clear();
            Model.DestinationItems.AddRange(DestinationItems);
            NotifyValidationRequired();
        };

        BrowseSourceCommand = new RelayCommand(BrowseSource, _ => SourceBrowseType is not null);
        BrowseDestinationCommand = new RelayCommand(BrowseDestination, _ => DestinationBrowseType is not null);
        ToggleInfoPopupCommand = new RelayCommand(ToggleInfoPopup);

        AddSourceCommand = new RelayCommand(AddSource, _ => SourceBrowseType is not null && Source is not null);
        AddDestinationCommand = new RelayCommand(AddDestination, _ => DestinationBrowseType is not null && Destination is not null);

        DeleteSourceCommand = new RelayCommand<Entry>(DeleteSource);
        DeleteDestinationCommand = new RelayCommand<Entry>(DeleteDestination);

        TimeDifference = model.TimeDifference;
        TimeDifferenceUnit = model.TimeDifferenceUnit;
    }

    private void DeleteDestination(Entry obj) {
        DestinationItems.Remove(obj);
    }

    private void DeleteSource(Entry obj) {
        SourceItems.Remove(obj);
    }

    private void AddDestination(object? obj) {
        DestinationItems.Add(new Entry {
            Type = DestinationBrowseType!.Value,
            Path = Destination!
        });

        Destination = null;
        DestinationBrowseType = null;
    }

    private void AddSource(object? obj) {
        SourceItems.Add(new Entry {
            Type = SourceBrowseType!.Value,
            Path = Source!
        });

        Source = null;
        SourceBrowseType = null;
    }

    private void ToggleInfoPopup(object? obj) {
        IsInfoPopupOpen = !IsInfoPopupOpen;
    }

    private void BrowseSource(object? obj) {
        switch (SourceBrowseType) {
            case EntryBrowseType.File:
                OpenFileDialog fileDialog = new OpenFileDialog {
                    Title = "Select source file"
                };

                if (fileDialog.ShowDialog() is true) {
                    Source = fileDialog.FileName;
                }

                break;
            case EntryBrowseType.Directory:
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
        switch (DestinationBrowseType) {
            case EntryBrowseType.Directory:
                OpenFolderDialog folderDialog = new OpenFolderDialog();
                folderDialog.Title = "Select destination folder";

                if (folderDialog.ShowDialog() is true) {
                    Destination = folderDialog.FolderName;
                }

                break;
        }
    }
}
