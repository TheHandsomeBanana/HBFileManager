using FileManager.Core.Jobs;
using FileManager.Core.JobSteps;
using FileManager.Core.Workspace;
using FileManager.Domain;
using FileManager.Domain.JobSteps;
using FileManager.UI.Models.SettingsModels;
using HBLibrary.Common;
using HBLibrary.Common.Plugins;
using HBLibrary.DataStructures;
using HBLibrary.DI;
using HBLibrary.Interface.Plugins;
using HBLibrary.Interface.Workspace;
using HBLibrary.Wpf.Commands;
using HBLibrary.Wpf.Services;
using HBLibrary.Wpf.ViewModels;
using HBLibrary.Wpf.Views;
using Microsoft.Win32;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics.Eventing.Reader;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Data;
using Unity;

namespace FileManager.UI.ViewModels.SettingsViewModels;

public class SettingsPluginsViewModel : ViewModelBase {
    private readonly IPluginManager pluginManager;
    private readonly JobManager jobManager;

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
        IUnityContainer container = UnityBase.Registry.Get(ApplicationHandler.FileManagerContainerGuid);
        pluginManager = container.Resolve<IPluginManager>();

        IApplicationWorkspaceManager<HBFileManagerWorkspace> workspaceManager = container.Resolve<IApplicationWorkspaceManager<HBFileManagerWorkspace>>();
        jobManager = workspaceManager.CurrentWorkspace!.JobManager!;

        assemblies = [.. pluginManager.GetLoadedAssemblies().Select(e => e.GetFirst().GetName())];

        assemblyView = CollectionViewSource.GetDefaultView(assemblies);
        assemblyView.Filter = FilterPlugins;

        AddAssemblyCommand = new RelayCommand(AddAssembly);
        DeleteAssemblyCommand = new RelayCommand<AssemblyName>(DeleteAssembly);

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

        Dictionary<Job, Tuple<JobStep, PluginType>> found = [];

        foreach (Job job in jobManager.GetAll()) {
            foreach (JobStep jobStep in job.Steps) {
                foreach (PluginType type in types) {
                    if (jobStep.GetType() == type.ConcreteType) {
                        found.Add(job, new(jobStep, type));
                    }
                }
            }
        }

        string message;
        if (found.Count != 0) {
            message = $"If you delete this plugin, the following Job-Steps will be deleted as well:\n- " +
            $"{string.Join("\n- ", found.Select(e => $"[Name: {e.Value.Item1.Name} | Type: {e.Value.Item2.ConcreteType}]"))}";
        }
        else {
            message = "No Job-Step is using this plugin, you can remove it safely.";
        }


        MessageBoxResult result = HBDarkMessageBox.Show("Remove plugin", message, MessageBoxButton.YesNo, MessageBoxImage.Warning);

        if (result == MessageBoxResult.Yes) {
            foreach (KeyValuePair<Job, Tuple<JobStep, PluginType>> item in found) {
                jobManager.DeleteStep(item.Key, item.Value.Item1);
            }

            foreach (Job job in found.Select(e => e.Key)) {
                bool canRun = true;

                foreach (JobStep step in job.Steps) {
                    canRun &= (step.IsValid);
                }

                job.CanRun = canRun;
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
        IAssemblyContext? loadedContext = pluginManager.GetLoadedAssembly(assemblyFileName);
        if (loadedContext is not null) {
            try {
                FoundPlugins = pluginManager.TypeProvider.GetCachedByAttribute<JobStep>([loadedContext]);
            }
            catch (TypeLoadException ex) {
                ApplicationHandler.ShowError("Type load error", "Could not load plugin types from assembly - try updating the assembly file");
            }
            catch (Exception ex) {
                ApplicationHandler.ShowError("Plugin load error", ex.Message);
            }
        }
    }
}
