using FileManager.Domain;
using FileManager.Domain.JobSteps;
using FileManager.UI.ViewModels.ExecutionViewModels.RunningJobsViewModels;
using HBLibrary.Wpf.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileManager.UI.ViewModels.ExecutionViewModels.JobsHistoryViewModels;
public class JobHistoryViewModel : ViewModelBase<JobRun> {
    public TimeSpan Elapsed => Model.Duration.GetValueOrDefault();
    public string Name => Model.Name;
    public bool IsSuccess => Model.State == RunState.Success;
    public bool IsError => Model.State == RunState.Faulted;
    public bool IsWarning => Model.State == RunState.CompletedWithWarnings;

    public ObservableCollection<StepHistoryViewModel> CompletedSteps { get; set; } = [];
    public JobHistoryViewModel(JobRun model) : base(model) {
    }
}
