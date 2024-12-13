using FileManager.Core.Jobs.Models;
using FileManager.Core.Jobs.Models.Copy;
using FileManager.Core.Jobs.Models.Zip;
using FileManager.Core.JobSteps;
using HBLibrary.Wpf.Commands;
using HBLibrary.Wpf.Models;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.IO;

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

    private EntryBrowseType sourceBrowseType;
    public EntryBrowseType SourceBrowseType {
        get => sourceBrowseType;
        set {
            sourceBrowseType = value;
            NotifyPropertyChanged();

            Source = null;
            BrowseSourceCommand.NotifyCanExecuteChanged();
        }
    }

    private string archiveName = "archive";
    public string ArchiveName {
        get { return archiveName; }
        set {
            archiveName = value;
            NotifyPropertyChanged();


            Destination = Destination?.Substring(0, Destination.IndexOf(Path.GetFileName(Destination))) + value + ".zip";
        }
    }


    public string? Destination {
        get => Model.Destination;
        set {
            Model.Destination = value;
            NotifyPropertyChanged();
        }
    }

    public EntryBrowseType[] AvailableSourceTypes => [EntryBrowseType.Directory, EntryBrowseType.File];


    public ObservableCollection<Entry> SourceItems { get; set; }

    private Entry? selectedSource;
    public Entry? SelectedSource {
        get { return selectedSource; }
        set {
            selectedSource = value;
            NotifyPropertyChanged();
        }
    }

    public RelayCommand BrowseSourceCommand { get; set; }
    public RelayCommand BrowseDestinationCommand { get; set; }
    public RelayCommand ToggleInfoPopupCommand { get; set; }

    public RelayCommand AddSourceCommand { get; set; }
    public RelayCommand<Entry> DeleteSourceCommand { get; set; }

    public ZipArchiveStepViewModel(ZipArchiveStep model) : base(model) {
        SourceItems = new ObservableCollection<Entry>(Model.SourceItems);

        // Refresh Model collection on change
        SourceItems.CollectionChanged += (_, _) => {
            Model.SourceItems.Clear();
            Model.SourceItems.AddRange(SourceItems);
            NotifyValidationRequired();
        };


        BrowseSourceCommand = new RelayCommand(BrowseSource);
        BrowseDestinationCommand = new RelayCommand(BrowseDestination);
        ToggleInfoPopupCommand = new RelayCommand(ToggleInfoPopup);
        AddSourceCommand = new RelayCommand(AddSource, _ => Source is not null);
        DeleteSourceCommand = new RelayCommand<Entry>(DeleteSource);

        if (!string.IsNullOrEmpty(Destination)) {
            ArchiveName = Path.GetFileNameWithoutExtension(Destination);
        }
    }

    private void DeleteSource(Entry obj) {
        SourceItems.Remove(obj);
    }

    private void AddSource(object? obj) {
        string[] sourceItems = Source?.Split("; ") ?? [];

        foreach (string item in sourceItems) {
            SourceItems.Add(new Entry {
                Type = SourceBrowseType,
                Path = item!
            });
        }

        Source = null;
    }

    private void ToggleInfoPopup(object? obj) {
        IsInfoPopupOpen = !IsInfoPopupOpen;
    }

    private void BrowseSource(object? obj) {
        switch (SourceBrowseType) {
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
                    Source = string.Join(";", folderDialog.FolderNames);
                }

                break;
        }
    }

    private void BrowseDestination(object? obj) {
        OpenFolderDialog folderDialog = new OpenFolderDialog();
        folderDialog.Title = "Select destination folder";

        if (folderDialog.ShowDialog() is true) {
            Destination = System.IO.Path.Combine(folderDialog.FolderName, archiveName + ".zip");
        }
    }
}
