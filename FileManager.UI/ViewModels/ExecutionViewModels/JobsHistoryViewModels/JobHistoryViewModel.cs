using FileManager.Core.Jobs;
using FileManager.Core.Workspace;
using FileManager.Domain;
using FileManager.Domain.JobSteps;
using FileManager.UI.ViewModels.ExecutionViewModels.RunningJobsViewModels;
using HBLibrary.DI;
using HBLibrary.Interface.Workspace;
using HBLibrary.Wpf.Commands;
using HBLibrary.Wpf.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity;

namespace FileManager.UI.ViewModels.ExecutionViewModels.JobsHistoryViewModels;
public class JobHistoryViewModel : ViewModelBase<JobRun> {
    private readonly JobHistoryManager jobHistoryManager;

    public TimeSpan Elapsed => Model.Duration.GetValueOrDefault();
    public string Name => Model.Name;
    public bool IsSuccess => Model.State == RunState.Success;
    public bool IsError => Model.State == RunState.Faulted;
    public bool IsWarning => Model.State == RunState.CompletedWithWarnings;
    public DateTime? StartedAt => Model.StartedAt;

    public ObservableCollection<StepHistoryViewModel> CompletedSteps { get; set; } = [];
    private StepHistoryViewModel? selectedStepRun;
    public StepHistoryViewModel? SelectedStepRun {
        get { return selectedStepRun; }
        set {
            selectedStepRun = value;
            NotifyPropertyChanged();
        }
    }

    public RelayCommand<JobHistoryViewModel> DeleteJobCommand { get; set; }
    public event Action<JobHistoryViewModel>? OnJobDeleted;

    public JobHistoryViewModel(JobRun model) : base(model) {
        IUnityContainer mainContainer = UnityBase.Registry.Get(ApplicationHandler.FileManagerContainerGuid);

        IApplicationWorkspaceManager<HBFileManagerWorkspace> workspaceManager = mainContainer.Resolve<IApplicationWorkspaceManager<HBFileManagerWorkspace>>();
        jobHistoryManager = workspaceManager.CurrentWorkspace!.JobHistoryManager!;


        DeleteJobCommand = new RelayCommand<JobHistoryViewModel>(DeleteJob);

        foreach (StepRun stepRun in model.StepRuns) {
            CompletedSteps.Add(new StepHistoryViewModel(stepRun));   
        }

        SelectedStepRun = CompletedSteps.FirstOrDefault();
    }

    private void DeleteJob(JobHistoryViewModel obj) {
        jobHistoryManager.DeleteJob(obj.Model.Id);
        OnJobDeleted?.Invoke(obj);
    }
}
