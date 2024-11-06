using FileManager.Domain.JobSteps;
using HBLibrary.Interface.Plugins;
using HBLibrary.Plugins;
using HBLibrary.Wpf.ViewModels;
using System.Windows.Controls;

namespace FileManager.Core.JobSteps;
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

            if (value && !AsyncValidationRunning && !ValidationRunning) {
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
    public event Action? ValidationStarted;
    public event Action? ValidationFinished;

    public bool ValidationRunning { get; private set; }
    public bool AsyncValidationRunning { get; private set; }
    public PluginMetadata Metadata => PluginManager.GetPluginMetadata(Model.GetType());

    public JobStepViewModel(TModel model) : base(model) {
    }

    public bool NotifyValidationRequired() {
        try {
            ValidationRunning = true;
            ValidationStarted?.Invoke();
            IsValid = ValidationRequired?.Invoke(Model) ?? false;
            return IsValid;
        }
        finally {
            ValidationRunning = false;
            ValidationFinished?.Invoke();
        }
    }

    public async Task<bool> NotifyAsyncValidationRequired() {
        IsValid = await InvokeAsyncValidation();
        return IsValid;
    }

    private async Task<bool> InvokeAsyncValidation() {
        try {
            AsyncValidationRunning = true;
            ValidationStarted?.Invoke();
            return await (AsyncValidationRequired?.Invoke(Model) ?? Task.FromResult(false));
        }
        finally {
            AsyncValidationRunning = false;
            ValidationFinished?.Invoke();
        }
    }
}
