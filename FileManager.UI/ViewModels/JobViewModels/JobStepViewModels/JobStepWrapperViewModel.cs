using FileManager.Core.Jobs.ViewModels;
using FileManager.Core.Jobs.Views;
using FileManager.Core.JobSteps;
using FileManager.Domain.JobSteps;
using FileManager.UI.Views.JobViews.JobStepViews;
using HBLibrary.Wpf.ViewModels;
using System.Windows;
using System.Windows.Controls;
using Unity;

namespace FileManager.UI.ViewModels.JobViewModels.JobStepViewModels;

public class JobStepWrapperViewModel : ViewModelBase<JobStep>, IDisposable {

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


    public IJobStepContext StepContext { get; }

    public bool IsValidationRunning => StepContext.AsyncValidationRunning || StepContext.ValidationRunning;
    public bool IsValidationError => !StepContext.IsValid && !IsValidationRunning;
    public bool IsValidationSuccess => StepContext.IsValid && !IsValidationRunning;

    public JobStepWrapperViewModel(JobStep model) : base(model) {
        StepContext = Model.GetJobStepDataContext()
            ?? new FallbackStepViewModel(Model);

        StepContext.ValidationStarted += StepContext_ValidationStarted;
        StepContext.ValidationFinished += StepContext_ValidationFinished;
    }

   

    private void StepContext_ValidationFinished() {
        Application.Current.Dispatcher.Invoke(() => {
            NotifyPropertyChanged(nameof(IsValidationError));
            NotifyPropertyChanged(nameof(IsValidationSuccess));
            NotifyPropertyChanged(nameof(IsValidationRunning));
        });
    }

    private void StepContext_ValidationStarted() {
        Application.Current.Dispatcher.Invoke(() => {
            NotifyPropertyChanged(nameof(IsValidationError));
            NotifyPropertyChanged(nameof(IsValidationSuccess));
            NotifyPropertyChanged(nameof(IsValidationRunning));
        });
    }

    public void Dispose() {
        StepContext.ValidationStarted -= StepContext_ValidationStarted;
        StepContext.ValidationFinished -= StepContext_ValidationFinished;
    }
}
