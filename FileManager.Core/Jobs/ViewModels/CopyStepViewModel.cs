using FileManager.Core.Jobs.Models;
using FileManager.Core.JobSteps;
using FileManager.Domain;
using FileManager.Domain.JobSteps;
using HBLibrary.Wpf.Commands;
using HBLibrary.Wpf.Models;
using Microsoft.Win32;
using System.Collections.ObjectModel;

namespace FileManager.Core.Jobs.ViewModels;
public class CopyStepViewModel : JobStepViewModel<CopyStep> {


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

    private EntryType? sourceType;
    public EntryType? SourceType {
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

    private EntryType? destinationType;
    public EntryType? DestinationType {
        get => destinationType;
        set {
            destinationType = value;
            NotifyPropertyChanged();

            Destination = null;
            BrowseDestinationCommand.NotifyCanExecuteChanged();
        }
    }


    public EntryType[] AvailableSourceTypes => [EntryType.Directory, EntryType.File];
    public EntryType[] AvailableDestinationTypes => [EntryType.Directory];

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
    public CopyStepViewModel(CopyStep model) : base(model) {
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

        BrowseSourceCommand = new RelayCommand(BrowseSource, _ => SourceType is not null);
        BrowseDestinationCommand = new RelayCommand(BrowseDestination, _ => DestinationType is not null);
        ToggleInfoPopupCommand = new RelayCommand(ToggleInfoPopup, true);

        AddSourceCommand = new RelayCommand(AddSource, _ => SourceType is not null && Source is not null);
        AddDestinationCommand = new RelayCommand(AddDestination, _ => DestinationType is not null && Destination is not null);

        DeleteSourceCommand = new RelayCommand<Entry>(DeleteSource, true);
        DeleteDestinationCommand = new RelayCommand<Entry>(DeleteDestination, true);

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
            Type = DestinationType!.Value,
            Path = Destination!
        });

        Destination = null;
        DestinationType = null;
    }

    private void AddSource(object? obj) {
        SourceItems.Add(new Entry {
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
            case EntryType.File:
                OpenFileDialog fileDialog = new OpenFileDialog {
                    Title = "Select source file"
                };

                if (fileDialog.ShowDialog() is true) {
                    Source = fileDialog.FileName;
                }

                break;
            case EntryType.Directory:
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
            case EntryType.File:
                OpenFileDialog fileDialog = new OpenFileDialog();
                fileDialog.Title = "Select destination file";

                if (fileDialog.ShowDialog() is true) {
                    Destination = fileDialog.FileName;
                }

                break;
            case EntryType.Directory:
                OpenFolderDialog folderDialog = new OpenFolderDialog();
                folderDialog.Title = "Select destination folder";

                if (folderDialog.ShowDialog() is true) {
                    Destination = folderDialog.FolderName;
                }

                break;
        }
    }
}
