using HBLibrary.Common.Extensions;
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

            if (value && !AsyncValidationRunning) {
                InvokeAsyncValidation()
                    .ContinueWith(e => {
                        IsValid = e.Result;
                    }, TaskContinuationOptions.OnlyOnRanToCompletion);
            }
        }
    }

    public bool IsValid {
        get => Model.IsValid;
        set {
            Model.IsValid = value;
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

    public event Func<JobStep, bool>? ValidationRequired;
    public event Func<JobStep, Task<bool>>? AsyncValidationRequired;

    public bool ValidationRunning { get; private set; }
    public bool AsyncValidationRunning { get; private set; }

    public PluginMetadata Metadata => PluginManager.GetPluginMetadata(Model.GetType());

    public JobStepViewModel(TModel model) : base(model) {
    }

    public void NotifyValidationRequired() {
        try {
            ValidationRunning = true;
            IsValid = ValidationRequired?.Invoke(Model) ?? false;
        }
        finally {
            ValidationRunning = false;
        }
    }

    public async Task NotifyAsyncValidationRequired() {
        IsValid = await InvokeAsyncValidation();
    }

    private async Task<bool> InvokeAsyncValidation() {
        try {
            AsyncValidationRunning = true;
            return await (AsyncValidationRequired?.Invoke(Model) ?? Task.FromResult(false));
        }
        finally {
            AsyncValidationRunning = false;
        }
    }
}
