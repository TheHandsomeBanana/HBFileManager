using FileManager.Core.JobSteps;
using FileManager.Core.JobSteps.ViewModels;
using FileManager.Core.JobSteps.Views;
using HBLibrary.Wpf.ViewModels;
using System.Windows.Controls;

namespace FileManager.UI.ViewModels.JobViewModels.JobStepViewModels;

public class JobStepWrapperViewModel : ViewModelBase<JobStep> {

    public UserControl? StepView {
        get {
            bool createNewVm = stepViewModel == null;

            UserControl? stepView = Model.GetJobStepView(createNewVm);
            if (stepView is not null) {
                if (createNewVm) {
                    StepViewModel = stepView.DataContext as ViewModelBase;
                }
                else {
                    stepView.DataContext = stepViewModel;
                }
            }
            else {
                stepView = new FallbackStepView();
                StepViewModel = new FallbackStepViewModel(Model);
                stepView.DataContext = stepViewModel;
            }

            return stepView;
        }
    }

    // Cache the VM but not the view
    private ViewModelBase? stepViewModel;
    public ViewModelBase? StepViewModel {
        get {
            return stepViewModel;
        }
        set {
            stepViewModel = value;
            NotifyPropertyChanged();
        }
    }

    public JobStepWrapperViewModel(JobStep model) : base(model) {
    }
}
