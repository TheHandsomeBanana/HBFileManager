using FileManager.Core.JobSteps;
using FileManager.Core.JobSteps.ViewModels;
using FileManager.Core.JobSteps.Views;
using HBLibrary.Wpf.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace FileManager.UI.ViewModels.JobViewModels.JobStepViewModels;

public class JobStepWrapperViewModel : ViewModelBase<IJobStep> {
    public UserControl? StepView {
        get {
            UserControl? stepView = Model.GetJobStepView();
            if (stepView is not null) {
                StepViewModel = stepView.DataContext as ViewModelBase;
            }
            else {
                stepView = new FallbackStepView();
                StepViewModel = new FallbackStepViewModel(Model);
                stepView.DataContext = StepViewModel;
            }

            return stepView;
        }
    }

    // Cache the VM but not the view
    private ViewModelBase? stepViewModel;
    public ViewModelBase? StepViewModel {
        get {
            stepViewModel ??= StepView?.DataContext as ViewModelBase;
            return stepViewModel;
        }
        set {
            stepViewModel = value;
            NotifyPropertyChanged();
        }
    }

    public JobStepWrapperViewModel(IJobStep model) : base(model) {
    }
}
