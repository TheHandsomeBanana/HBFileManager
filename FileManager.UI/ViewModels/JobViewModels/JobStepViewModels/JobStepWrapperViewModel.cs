using FileManager.Core.JobSteps;
using FileManager.Core.JobSteps.ViewModels;
using FileManager.Core.JobSteps.Views;
using HBLibrary.Wpf.ViewModels;
using System.Windows.Controls;

namespace FileManager.UI.ViewModels.JobViewModels.JobStepViewModels;

public class JobStepWrapperViewModel : ViewModelBase<JobStep> {

    public UserControl? StepView {
        get {
            UserControl? stepView = Model.GetJobStepView();
            if (stepView is not null && StepContext is not FallbackStepViewModel) {
                stepView.DataContext = StepContext;
            }
            else {
                stepView = new FallbackStepView();
                stepView.DataContext = StepContext;
            }

            return stepView;
        }
    }

    
    public IJobStepContext? StepContext { get; }

    public JobStepWrapperViewModel(JobStep model) : base(model) {
        StepContext = Model.GetJobStepDataContext()
            ?? new FallbackStepViewModel(Model);
    }
}
