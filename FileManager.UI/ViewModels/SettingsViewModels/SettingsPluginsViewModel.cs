using FileManager.Core.JobSteps;
using FileManager.UI.Models.SettingsModels;
using HBLibrary.Common.DI.Unity;
using HBLibrary.Common.Plugins;
using HBLibrary.Wpf.Commands;
using HBLibrary.Wpf.Services;
using HBLibrary.Wpf.ViewModels;
using Microsoft.Win32;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Windows.Data;
using Unity;

namespace FileManager.UI.ViewModels.SettingsViewModels;

public class SettingsPluginsViewModel : ViewModelBase<SettingsPluginsModel> {
    private readonly IDialogService dialogService;
    private readonly IPluginManager pluginManager;

    private string? searchText;
    public string? SearchText {
        get => searchText;
        set {
            searchText = value;
            NotifyPropertyChanged();
            assemblyView.Refresh();
        }
    }

    public RelayCommand AddAssemblyCommand { get; set; }
    public RelayCommand<string> DeleteAssemblyCommand { get; set; }

    private string? selectedAssembly;
    public string? SelectedAssembly {
        get { return selectedAssembly; }
        set {
            selectedAssembly = value;
            NotifyPropertyChanged();

            if (!string.IsNullOrEmpty(value)) {
                if (!pluginManager.IsPluginAssemblyLoaded(value)) {
                    pluginManager.LoadAssembly(value);
                }

                FindPlugins(value);
            }
        }
    }

    private readonly ICollectionView assemblyView;
    public ICollectionView AssembliesView => assemblyView;
    public readonly ObservableCollection<string> assemblies;


    private PluginType[] foundPlugins = [];
    public PluginType[] FoundPlugins {
        get => foundPlugins;
        set {
            foundPlugins = value;
            NotifyPropertyChanged();
        }
    }

    public SettingsPluginsViewModel(SettingsPluginsModel model) : base(model) {
        IUnityContainer container = UnityBase.GetChildContainer(nameof(FileManager))!;
        dialogService = container.Resolve<IDialogService>();
        pluginManager = container.Resolve<IPluginManager>();

        assemblies = [.. Model.Assemblies];

        assemblies.CollectionChanged += (_, s) => {
            Model.Assemblies.Clear();
            Model.Assemblies.AddRange(assemblies);
        };

        assemblyView = CollectionViewSource.GetDefaultView(assemblies);
        assemblyView.Filter = FilterPlugins;

        AddAssemblyCommand = new RelayCommand(AddAssembly, true);
        DeleteAssemblyCommand = new RelayCommand<string>(DeleteAssembly, true);
    }

    private bool FilterPlugins(object obj) {
        if (obj is string name) {
            return string.IsNullOrEmpty(SearchText) || name.Contains(SearchText, StringComparison.OrdinalIgnoreCase);
        }
        return false;
    }

    private void DeleteAssembly(string obj) {
        assemblies.Remove(obj);
        pluginManager.RemovePluginAssembly(obj);
        SelectedAssembly = assemblies.FirstOrDefault();
    }

    private void AddAssembly(object? obj) {
        OpenFileDialog ofd = new OpenFileDialog();
        ofd.Filter = "DLL Files (*.dll)|*.dll";
        ofd.Title = "Select Plugins";
        ofd.CheckFileExists = true;
        ofd.DefaultExt = ".dll";
        ofd.Multiselect = true;

        if (ofd.ShowDialog().GetValueOrDefault()) {
            foreach (string file in ofd.FileNames) {
                string fileName = Path.GetFileName(file);
                assemblies.Add(fileName);
                pluginManager.AddPluginAssembly(file);
            }
        }
    }

    private void FindPlugins(string assemblyFileName) {
        FoundPlugins = pluginManager.TypeProvider
            .GetCachedByAttribute<IJobStep>(pluginManager.GetLoadedAssemblies());
    }
}
