using FileManager.Core.Workspace;
using HBLibrary.Wpf.Commands;
using HBLibrary.Wpf.ViewModels;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace FileManager.UI.ViewModels.WorkspaceViewModels; 
public class AddWorkspaceViewModel : ViewModelBase {
    public RelayCommand<Window> AddWorkspaceCommand { get; set; }
    public RelayCommand<Window> CancelCommand { get; set; }
    public RelayCommand BrowseDirectoryCommand { get; set; }

    private string directory = "";

    public string Directory {
        get { return directory; }
        set { 
            directory = value;
            NotifyPropertyChanged();

            AddWorkspaceCommand.NotifyCanExecuteChanged();
        }
    }


    private string name = "";
    public string Name {
        get { return name; }
        set {
            name = value;
            NotifyPropertyChanged();

            AddWorkspaceCommand.NotifyCanExecuteChanged();
        }
    }

    private bool usesEncryption;
    public bool UsesEncryption {
        get { return usesEncryption; }
        set { usesEncryption = value; }
    }


    public AddWorkspaceViewModel() {
        AddWorkspaceCommand = new RelayCommand<Window>(AddAndFinish, _ => CanAddWorkspace());
        CancelCommand = new RelayCommand<Window>(CancelAndFinish);
        BrowseDirectoryCommand = new RelayCommand(BrowseDirectory);
    }

    private void BrowseDirectory(object? obj) {
        OpenFolderDialog openFolderDialog = new OpenFolderDialog();
        openFolderDialog.Multiselect = false;

        if (openFolderDialog.ShowDialog().GetValueOrDefault()) {
            Directory = openFolderDialog.FolderName;
        }
    }

    private void AddAndFinish(Window? obj) {
        if (obj is null) {
            return;
        }

        obj.DialogResult = true;
        obj.Close();
    }

    private void CancelAndFinish(Window? obj) {
        if (obj is null) {
            return;
        }

        obj.DialogResult = false;
        obj.Close();
    }

    private bool CanAddWorkspace() {
        return !string.IsNullOrWhiteSpace(Name) && 
            System.IO.Directory.Exists(Directory) && 
            !System.IO.Directory.GetFiles(Directory).Any(e => Path.GetExtension(e) == HBFileManagerWorkspace.WorkspaceExtension);
    }
}
