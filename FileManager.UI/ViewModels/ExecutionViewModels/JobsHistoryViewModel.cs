using FileManager.Core.Jobs;
using FileManager.Core.Workspace;
using FileManager.Domain;
using FileManager.UI.ViewModels.ExecutionViewModels.JobsHistoryViewModels;
using HBLibrary.DI;
using HBLibrary.Interface.Workspace;
using HBLibrary.Wpf.Commands;
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

public sealed class JobsHistoryViewModel : AsyncInitializerViewModelBase, IDisposable {
    private JobRun[]? currentRunningJobs;

    private readonly JobExecutionManager jobExecutionManager;
    private readonly JobHistoryManager jobHistoryManager;
    public ObservableCollection<JobHistoryViewModel> CompletedJobs { get; set; } = [];

    private JobHistoryViewModel? selectedJobRun;
    public JobHistoryViewModel? SelectedJobRun {
        get { return selectedJobRun; }
        set {
            selectedJobRun = value;
            NotifyPropertyChanged();
        }
    }

    public AsyncRelayCommand ClearJobsHistoryCommand { get; }

    public JobsHistoryViewModel() {
        IUnityContainer mainContainer = UnityBase.Registry.Get(ApplicationHandler.FileManagerContainerGuid);

        IApplicationWorkspaceManager<HBFileManagerWorkspace> workspaceManager = mainContainer.Resolve<IApplicationWorkspaceManager<HBFileManagerWorkspace>>();
        jobExecutionManager = workspaceManager.CurrentWorkspace!.JobExecutionManager!;
        jobHistoryManager = workspaceManager.CurrentWorkspace!.JobHistoryManager!;

        ClearJobsHistoryCommand = new AsyncRelayCommand(ClearJobsHistory, _ => CompletedJobs.Any(), OnClearJobsHistoryException);
    }

    

    protected override async Task InitializeViewModelAsync() {
        JobRun[] jobRuns = await jobHistoryManager.GetCompletedJobsAsync();
        foreach (JobRun run in jobRuns) {
            JobHistoryViewModel jobHistoryViewModel = new JobHistoryViewModel(run);
            jobHistoryViewModel.OnJobDeleted += JobHistoryViewModel_OnJobDeleted;
            CompletedJobs.Insert(0, jobHistoryViewModel);
        }

        ClearJobsHistoryCommand.NotifyCanExecuteChanged();
        SelectedJobRun = CompletedJobs.FirstOrDefault();

        currentRunningJobs = jobExecutionManager.GetRunningJobs();

        foreach (JobRun activeRun in currentRunningJobs) {
            activeRun.OnJobFinished += () => ActiveRun_OnJobFinished(activeRun);
        }
    }

    private void JobHistoryViewModel_OnJobDeleted(JobHistoryViewModel obj) {
        CompletedJobs?.Remove(obj);
        SelectedJobRun = CompletedJobs?.FirstOrDefault();
        ClearJobsHistoryCommand.NotifyCanExecuteChanged();
    }

    protected override void OnInitializeException(Exception exception) {
        HBDarkMessageBox.Show("Initialize error", exception.Message, MessageBoxButton.OK, MessageBoxImage.Error);
    }

    private async Task ClearJobsHistory(object? arg) {
        await jobHistoryManager.ClearJobsAsync();
        Application.Current.Dispatcher.Invoke(CompletedJobs.Clear);
    }

    private void OnClearJobsHistoryException(Exception exception) {
        HBDarkMessageBox.Show("Clear failed", exception.Message, MessageBoxButton.OK, MessageBoxImage.Error);
    }

    private void ActiveRun_OnJobFinished(JobRun jobRun) {
        Application.Current.Dispatcher.Invoke(() => {
            JobHistoryViewModel jobHistoryViewModel = new JobHistoryViewModel(jobRun);
            jobHistoryViewModel.OnJobDeleted += JobHistoryViewModel_OnJobDeleted;

            CompletedJobs.Insert(0, jobHistoryViewModel);
            ClearJobsHistoryCommand.NotifyCanExecuteChanged();
        });
    }

    public void Dispose() {
        if (currentRunningJobs is not null) {
            foreach (JobRun activeRun in currentRunningJobs) {
                activeRun.OnJobFinished -= () => ActiveRun_OnJobFinished(activeRun);
            }
        }

        foreach(JobHistoryViewModel jobHistoryViewModel in CompletedJobs) {
            jobHistoryViewModel.OnJobDeleted -= JobHistoryViewModel_OnJobDeleted;
        }
    }
}
