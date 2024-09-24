using HBLibrary.Common.Plugins;
using HBLibrary.Wpf.ViewModels;
using System.Windows.Controls;

namespace FileManager.Core.JobSteps.ViewModels;
public class JobStepViewModel<TModel> : ViewModelBase<TModel> where TModel : JobStep {

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

    public bool IsEnabled {
        get => Model.IsEnabled;
        set {
            Model.IsEnabled = value;
            NotifyPropertyChanged();
        }
    }

    public UserControl? StepView => Model.GetJobStepView();
    public PluginMetadata Metadata => PluginManager.GetPluginMetadata(Model.GetType());

    public JobStepViewModel(TModel model) : base(model) {

    }
}
