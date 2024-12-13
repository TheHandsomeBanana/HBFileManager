using FileManager.Core.Jobs.Models;
using FileManager.Core.Jobs.Models.Copy;
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

    private EntryBrowseType sourceType;
    public EntryBrowseType SourceType {
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

    public EntryBrowseType[] AvailableSourceTypes => [EntryBrowseType.Directory, EntryBrowseType.File];

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
    public ObservableCollection<string> DestinationItems { get; set; }

    private Entry? selectedSource;
    public Entry? SelectedSource {
        get { return selectedSource; }
        set {
            selectedSource = value;
            NotifyPropertyChanged();
        }
    }

    private string? selectedDestination;
    public string? SelectedDestination {
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
    public RelayCommand<string> DeleteDestinationCommand { get; set; }
    public CopyStepViewModel(CopyStep model) : base(model) {
        SourceItems = new ObservableCollection<Entry>(Model.SourceItems);

        // Refresh Model collection on change
        SourceItems.CollectionChanged += (_, _) => {
            Model.SourceItems.Clear();
            Model.SourceItems.AddRange(SourceItems);
            NotifyValidationRequired();
        };

        DestinationItems = new ObservableCollection<string>(Model.DestinationItems);

        // Refresh Model collection on change
        DestinationItems.CollectionChanged += (_, _) => {
            Model.DestinationItems.Clear();
            Model.DestinationItems.AddRange(DestinationItems);
            NotifyValidationRequired();
        };

        BrowseSourceCommand = new RelayCommand(BrowseSource);
        BrowseDestinationCommand = new RelayCommand(BrowseDestination);
        ToggleInfoPopupCommand = new RelayCommand(ToggleInfoPopup);

        AddSourceCommand = new RelayCommand(AddSource, _ => Source is not null);
        AddDestinationCommand = new RelayCommand(AddDestination, _ => Destination is not null);

        DeleteSourceCommand = new RelayCommand<Entry>(DeleteSource);
        DeleteDestinationCommand = new RelayCommand<string>(DeleteDestination);

        TimeDifference = model.TimeDifference;
        TimeDifferenceUnit = model.TimeDifferenceUnit;
    }

    private void DeleteDestination(string obj) {
        DestinationItems.Remove(obj);
    }

    private void DeleteSource(Entry obj) {
        SourceItems.Remove(obj);
    }

    private void AddDestination(object? obj) {
        string[] destinations = Destination?.Split("; ") ?? [];

        foreach (string dest in destinations) {
            DestinationItems.Add(dest);
        }

        Destination = null;
    }

    private void AddSource(object? obj) {
        string[] sourceItems = Source?.Split("; ") ?? [];

        foreach (string source in sourceItems) {
            SourceItems.Add(new Entry {
                Type = SourceType,
                Path = source
            });
        }

        Source = null;
    }

    private void ToggleInfoPopup(object? obj) {
        IsInfoPopupOpen = !IsInfoPopupOpen;
    }

    private void BrowseSource(object? obj) {
        switch (SourceType) {
            case EntryBrowseType.File:
                OpenFileDialog fileDialog = new OpenFileDialog {
                    Title = "Select source file",
                    Multiselect = true,
                };

                if (fileDialog.ShowDialog() is true) {
                    Source = string.Join("; ", fileDialog.FileNames);
                }

                break;
            case EntryBrowseType.Directory:
                OpenFolderDialog folderDialog = new OpenFolderDialog {
                    Title = "Select source folder",
                    Multiselect = true,
                };

                if (folderDialog.ShowDialog() is true) {
                    Source = string.Join("; ", folderDialog.FolderNames);
                }

                break;
        }
    }

    private void BrowseDestination(object? obj) {
        OpenFolderDialog folderDialog = new OpenFolderDialog();
        folderDialog.Title = "Select destination folder";
        folderDialog.Multiselect = true;

        if (folderDialog.ShowDialog() is true) {
            Destination = string.Join("; ", folderDialog.FolderNames);
        }
    }
}
