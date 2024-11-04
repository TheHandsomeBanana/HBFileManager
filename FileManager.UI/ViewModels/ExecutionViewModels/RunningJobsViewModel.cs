using FileManager.Core.Jobs;
using FileManager.Core.Workspace;
using FileManager.Domain;
using FileManager.UI.ViewModels.ExecutionViewModels.RunningJobsViewModels;
using HBLibrary.DI;
using HBLibrary.Interface.Core;
using HBLibrary.Interface.Workspace;
using HBLibrary.Wpf.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Unity;

namespace FileManager.UI.ViewModels.ExecutionViewModels;

public class RunningJobsViewModel : InitializerViewModelBase, IDisposable {
    private readonly JobExecutionManager jobRunner;

    public ObservableCollection<RunningJobViewModel> RunningJobs { get; set; }

    private RunningJobViewModel? selectedJobRun;
    public RunningJobViewModel? SelectedJobRun {
        get { return selectedJobRun; }
        set {
            selectedJobRun = value;
            NotifyPropertyChanged();
        }
    }

    public bool AnyJobsRunning => RunningJobs.Count > 0;

    public RunningJobsViewModel() {
        IUnityContainer mainContainer = UnityBase.Registry.Get(ApplicationHandler.FileManagerContainerGuid);
        IApplicationWorkspaceManager<HBFileManagerWorkspace> workspaceManager = mainContainer.Resolve<IApplicationWorkspaceManager<HBFileManagerWorkspace>>();
        jobRunner = workspaceManager.CurrentWorkspace!.JobExecutionManager!;

        RunningJobs = [];
    }

    protected override void InitializeViewModel() {
        foreach (JobRun jobRun in jobRunner.GetRunningJobs()) {
            RunningJobs.Add(new RunningJobViewModel(jobRun));
        }

        jobRunner.OnJobStarting += JobRunner_OnJobStarting;

        NotifyPropertyChanged(nameof(AnyJobsRunning));
    }

    private void JobRunner_OnJobStarting(JobRun obj) {
        Application.Current.Dispatcher.Invoke(() => {
            RunningJobViewModel runningJobVM = new RunningJobViewModel(obj);
            SelectedJobRun = runningJobVM;
            RunningJobs.Add(runningJobVM);
        });

        NotifyPropertyChanged(nameof(AnyJobsRunning));
    }

    public void Dispose() {
        jobRunner.OnJobStarting -= JobRunner_OnJobStarting;

        foreach (RunningJobViewModel runningJob in RunningJobs) {
            runningJob.Dispose();
        }
    }
}
