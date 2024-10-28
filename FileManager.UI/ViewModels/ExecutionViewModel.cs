using FileManager.Core.Job;
using FileManager.Core.Workspace;
using FileManager.UI.ViewModels.ExecutionViewModels;
using FileManager.UI.ViewModels.JobViewModels;
using FileManager.UI.Views;
using HBLibrary.DI;
using HBLibrary.Interface.Workspace;
using HBLibrary.Wpf.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using Unity;

namespace FileManager.UI.ViewModels;
public class ExecutionViewModel : ViewModelBase {
    private readonly IApplicationWorkspaceManager<HBFileManagerWorkspace> workspaceManager;
    public ObservableCollection<ExecutableJobViewModel> ExecutableJobs { get; set; }
    private readonly ICollectionView executableJobsView;
    public ICollectionView ExecutableJobsView => executableJobsView;

    private string? searchText;
    public string? SearchText {
        get { return searchText; }
        set {
            searchText = value;
            NotifyPropertyChanged();

            executableJobsView.Refresh();
        }
    }


    public ExecutionViewModel() {
        IUnityContainer container = UnityBase.Registry.Get(ApplicationHandler.FileManagerContainerGuid);
        workspaceManager = container.Resolve<IApplicationWorkspaceManager<HBFileManagerWorkspace>>();

        ExecutableJobs = new ObservableCollection<ExecutableJobViewModel>(workspaceManager.CurrentWorkspace!.JobManager!.GetExecutableJobs()
            .Select(e => new ExecutableJobViewModel(e)));

        executableJobsView = CollectionViewSource.GetDefaultView(ExecutableJobs);
        executableJobsView.Filter = FilterJobs;

        

    }

    private bool FilterJobs(object obj) {
        if (obj is ExecutableJobViewModel job) {
            return string.IsNullOrEmpty(SearchText) || job.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase);
        }
        return false;
    }
}
