using FileManager.UI.Models.SettingsModels;
using HBLibrary.Common.DI.Unity;
using HBLibrary.Wpf.Commands;
using HBLibrary.Wpf.Services;
using HBLibrary.Wpf.ViewModels;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Windows.Data;
using Unity;

namespace FileManager.UI.ViewModels.SettingsViewModels;

public class SettingsPluginsViewModel : ViewModelBase<SettingsPluginsModel> {
    private readonly IDialogService dialogService;

    private string? searchText;
    public string? SearchText {
        get => searchText;
        set {
            searchText = value;
            NotifyPropertyChanged();
            pluginsView.Refresh();
        }
    }

    public RelayCommand AddPluginCommand { get; set; }
    public RelayCommand<string> DeletePluginCommand { get; set; }

    private string? selectedPlugin;
    public string? SelectedPlugin {
        get { return selectedPlugin; }
        set {
            selectedPlugin = value;
            NotifyPropertyChanged();
        }
    }

    private readonly ICollectionView pluginsView;
    public ICollectionView PluginsView => pluginsView;
    public ObservableCollection<string> plugins { get; set; }

    public SettingsPluginsViewModel(SettingsPluginsModel model) : base(model) {
        IUnityContainer container = UnityBase.GetChildContainer(nameof(FileManager))!;
        dialogService = container.Resolve<IDialogService>();

        plugins = [.. Model.Plugins];

        plugins.CollectionChanged += (_, s) => {
            Model.Plugins.Clear();
            Model.Plugins.AddRange(plugins);
        };

        pluginsView = CollectionViewSource.GetDefaultView(plugins);
        pluginsView.Filter = FilterPlugins;


        AddPluginCommand = new RelayCommand(AddPlugin, true);
        DeletePluginCommand = new RelayCommand<string>(DeletePlugin, true);
    }

    private bool FilterPlugins(object obj) {
        if (obj is string name) {
            return string.IsNullOrEmpty(SearchText) || name.Contains(SearchText, StringComparison.OrdinalIgnoreCase);
        }
        return false;
    }

    private void DeletePlugin(string obj) {
        plugins.Remove(obj);
        SelectedPlugin = plugins.FirstOrDefault();
    }

    private void AddPlugin(object? obj) {
        OpenFileDialog ofd = new OpenFileDialog();
        ofd.Filter = "DLL Files (*.dll)|*.dll";
        ofd.Title = "Select Plugins";
        ofd.CheckFileExists = true;
        ofd.DefaultExt = ".dll";
        ofd.Multiselect = true;

        if (ofd.ShowDialog().GetValueOrDefault()) {
            foreach (string file in ofd.FileNames) {
                plugins.Add(Path.GetFileName(file));
            }
        }
    }
}
