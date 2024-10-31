using FileManager.Core.Jobs.ViewModels;
using FileManager.Core.Jobs.Views;
using FileManager.Core.JobSteps;
using FileManager.Domain.JobSteps;
using FileManager.UI.Views.JobViews.JobStepViews;
using HBLibrary.Wpf.ViewModels;
using System.Windows.Controls;
using Unity;

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
