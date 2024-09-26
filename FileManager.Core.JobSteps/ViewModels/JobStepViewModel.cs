using HBLibrary.Common.Plugins;
using HBLibrary.Wpf.ViewModels;
using System.Windows.Controls;

namespace FileManager.Core.JobSteps.ViewModels;
public class JobStepViewModel<TModel> : ViewModelBase<TModel>, IJobStepContext where TModel : JobStep {

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

    public bool CanExecute {
        get => Model.CanExecute;
        set { 
            Model.CanExecute = value;
            NotifyPropertyChanged();
        }
    }


    public event Action<int, JobStep>? ExecutionOrderChanged;
    public int ExecutionOrder {
        get => Model.ExecutionOrder;
        set {
            int oldValue = Model.ExecutionOrder;

            Model.ExecutionOrder = value;
            NotifyPropertyChanged();
            ExecutionOrderChanged?.Invoke(oldValue, Model);
        }
    }

    public PluginMetadata Metadata => PluginManager.GetPluginMetadata(Model.GetType());

    public JobStepViewModel(TModel model) : base(model) {

    }
}
