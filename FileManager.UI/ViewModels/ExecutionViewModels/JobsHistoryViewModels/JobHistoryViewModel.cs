using FileManager.Core.Jobs;
using FileManager.Core.Workspace;
using FileManager.Domain;
using FileManager.Domain.JobSteps;
using FileManager.UI.ViewModels.ExecutionViewModels.RunningJobsViewModels;
using HBLibrary.DI;
using HBLibrary.Interface.Workspace;
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

    public JobHistoryViewModel(JobRun model) : base(model) {
        foreach (StepRun stepRun in model.StepRuns) {
            CompletedSteps.Add(new StepHistoryViewModel(stepRun));   
        }

        SelectedStepRun = CompletedSteps.FirstOrDefault();
    }
}
