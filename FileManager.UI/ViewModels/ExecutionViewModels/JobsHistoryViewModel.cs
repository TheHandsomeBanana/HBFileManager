using FileManager.Core.Jobs;
using FileManager.Core.Workspace;
using FileManager.Domain;
using FileManager.UI.ViewModels.ExecutionViewModels.JobsHistoryViewModels;
using HBLibrary.DI;
using HBLibrary.Interface.Workspace;
using HBLibrary.Wpf.ViewModels;
using HBLibrary.Wpf.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Unity;

namespace FileManager.UI.ViewModels.ExecutionViewModels;

public class JobsHistoryViewModel : AsyncInitializerViewModelBase {
    private readonly JobExecutionManager jobExecutionManager;
    public ObservableCollection<JobHistoryViewModel> CompletedJobs { get; set; } = [];

    public JobsHistoryViewModel() {
        IUnityContainer mainContainer = UnityBase.Registry.Get(ApplicationHandler.FileManagerContainerGuid);

        IApplicationWorkspaceManager<HBFileManagerWorkspace> workspaceManager = mainContainer.Resolve<IApplicationWorkspaceManager<HBFileManagerWorkspace>>();
        jobExecutionManager = workspaceManager.CurrentWorkspace!.JobExecutionManager!;
    }

    protected override async Task InitializeViewModelAsync() {
        JobRun[] jobRuns = await jobExecutionManager.GetCompletedJobsAsync();
        foreach (JobRun run in jobRuns) {
            CompletedJobs.Add(new JobHistoryViewModel(run));
        }
    }

    protected override void OnInitializeException(Exception exception) {
        HBDarkMessageBox.Show("Initialize error", exception.Message, MessageBoxButton.OK, MessageBoxImage.Error);
    }
}
