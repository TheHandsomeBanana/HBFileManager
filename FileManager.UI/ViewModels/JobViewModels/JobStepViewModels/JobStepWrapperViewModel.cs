using FileManager.Core.JobSteps;
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
            StepViewModel = stepView?.DataContext as ViewModelBase;
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
