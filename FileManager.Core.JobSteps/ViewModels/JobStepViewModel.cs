using HBLibrary.Wpf.ViewModels;
using System.Windows.Controls;

namespace FileManager.Core.JobSteps.ViewModels;
public class JobStepViewModel<TModel> : ViewModelBase<TModel> where TModel : IJobStep {

    public string Name {
        get => Model.Name;
        set {
            Model.Name = value;
            NotifyPropertyChanged();
        }
    }

    public bool IsAsync {
        get => Model.IsAsync;
        set {
            Model.IsAsync = value;
            NotifyPropertyChanged();
        }
    }

    public UserControl? StepView => Model.GetJobStepView();
    public string StepType => JobStepManager.GetJobStepTypeName(Model.GetType());

    public JobStepViewModel(TModel model) : base(model) {

    }
}
