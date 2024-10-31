using FileManager.Domain;
using HBLibrary.Logging.FlowDocumentTarget;
using HBLibrary.Wpf.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileManager.UI.ViewModels.ExecutionViewModels.RunningJobsViewModels;
public class RunningJobViewModel : ViewModelBase<JobRun> {

    public RunState State { 
        get => Model.State;
        set {
            Model.State = value;
            NotifyPropertyChanged(nameof(IsRunning));
            NotifyPropertyChanged(nameof(IsSuccess));
            NotifyPropertyChanged(nameof(IsError));
        } 
    }

    public ObservableCollection<RunningStepViewModel> RunningSteps { get; set; } = [];

    public bool IsRunning => State == RunState.Running;
    public bool IsSuccess => State == RunState.Success;
    public bool IsError => State == RunState.Faulted;

    public RunningJobViewModel(JobRun model) : base(model) {
    }

}
