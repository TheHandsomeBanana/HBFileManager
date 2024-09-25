using FileManager.Core.JobSteps;
using FileManager.UI.Models.SettingsModels;
using FileManager.UI.Services.JobService;
using HBLibrary.Common;
using HBLibrary.Common.DI.Unity;
using HBLibrary.Common.Plugins;
using HBLibrary.Common.Plugins.Loader;
using HBLibrary.Wpf.Commands;
using HBLibrary.Wpf.Services;
using HBLibrary.Wpf.ViewModels;
using HBLibrary.Wpf.Views;
using Microsoft.Win32;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Data;
using Unity;

namespace FileManager.UI.ViewModels.SettingsViewModels;

public class SettingsPluginsViewModel : ViewModelBase {
    private readonly IPluginManager pluginManager;
    private readonly IJobService jobService;

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
    public RelayCommand<AssemblyName> DeleteAssemblyCommand { get; set; }

    private AssemblyName? selectedAssembly;
    public AssemblyName? SelectedAssembly {
        get { return selectedAssembly; }
        set {
            selectedAssembly = value;
            NotifyPropertyChanged();

            if (value is not null) {
                if (!pluginManager.IsPluginAssemblyLoaded(value)) {
                    Result res = pluginManager.LoadAssembly(value);
                }

                FindPlugins(value.Name!);
            }
        }
    }

    private readonly ICollectionView assemblyView;
    public ICollectionView AssembliesView => assemblyView;
    public readonly ObservableCollection<AssemblyName> assemblies;


    private PluginType[] foundPlugins = [];
    public PluginType[] FoundPlugins {
        get => foundPlugins;
        set {
            foundPlugins = value;
            NotifyPropertyChanged();
        }
    }

    public SettingsPluginsViewModel() : base() {
        IUnityContainer container = UnityBase.GetChildContainer(nameof(FileManager))!;
        pluginManager = container.Resolve<IPluginManager>();
        jobService = container.Resolve<IJobService>();

        assemblies = [.. pluginManager.GetLoadedAssemblies().Select(e => e.GetFirst().GetName())];

        assemblyView = CollectionViewSource.GetDefaultView(assemblies);
        assemblyView.Filter = FilterPlugins;

        AddAssemblyCommand = new RelayCommand(AddAssembly, true);
        DeleteAssemblyCommand = new RelayCommand<AssemblyName>(DeleteAssembly, true);

        SelectedAssembly = assemblies.FirstOrDefault();
    }

    private bool FilterPlugins(object obj) {
        if (obj is AssemblyName name) {
            return string.IsNullOrEmpty(SearchText) || name.FullName.Contains(SearchText, StringComparison.OrdinalIgnoreCase);
        }
        return false;
    }

    private void DeleteAssembly(AssemblyName obj) {
        PluginType[] types = pluginManager.TypeProvider.GetByAttribute<JobStep>([pluginManager.GetLoadedAssembly(obj.Name!)!]);

        List<Tuple<JobStep, PluginType>> foundSteps = [];
        foreach (JobStep jobStep in jobService.GetAll().SelectMany(e => e.Steps)) {
            foreach (PluginType type in types) {
                if (jobStep.GetType() == type.ConcreteType) {
                    foundSteps.Add(new(jobStep, type));
                }
            }
        }

        string message;
        if (foundSteps.Count != 0) {
            message = $"If you delete this plugin, the following Job-Steps will be deleted as well:\n- " +
            $"{string.Join("\n- ", foundSteps.Select(e => $"[Name: {e.Item1.Name} | Type: {e.Item2.ConcreteType}]"))}";
        }
        else {
            message = "No Job-Step is using this plugin, you can remove it safely.";
        }



        MessageBoxResult result = HBDarkMessageBox.Show("Remove plugin", message, MessageBoxButton.YesNo, MessageBoxImage.Warning);

        if (result == MessageBoxResult.Yes) {
            foreach (JobStep jobStep in foundSteps.Select(e => e.Item1)) {
                jobService.DeleteStep(jobStep);
            }

            assemblies.Remove(obj);

            // TODO: handle result
            Result res = pluginManager.RemovePluginAssembly(obj);
            SelectedAssembly = assemblies.FirstOrDefault();
        }
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
                AssemblyName assemblyName = AssemblyName.GetAssemblyName(file);
                if (assemblies.All(e => e.FullName != assemblyName.FullName)) {
                    assemblies.Add(assemblyName);
                }

                // TODO: handle result
                Result res = pluginManager.AddOrUpdatePluginAssembly(file);

                SelectedAssembly = assemblies.LastOrDefault();
            }
        }
    }

    private void FindPlugins(string assemblyFileName) {
        AssemblyContext? loadedContext = pluginManager.GetLoadedAssembly(assemblyFileName);
        if (loadedContext is not null) {
            FoundPlugins = pluginManager.TypeProvider.GetCachedByAttribute<JobStep>([loadedContext]);
        }
    }
}
