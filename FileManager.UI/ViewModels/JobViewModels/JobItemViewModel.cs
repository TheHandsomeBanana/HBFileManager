using FileManager.Core.JobSteps;
using FileManager.Core.JobSteps.Models;
using FileManager.Core.JobSteps.ViewModels;
using FileManager.Core.JobSteps.Views;
using FileManager.UI.Models.JobModels;
using FileManager.UI.Services.JobService;
using FileManager.UI.ViewModels.JobViewModels.JobStepViewModels;
using FileManager.UI.Views.JobViews.JobStepViews;
using HBLibrary.Common;
using HBLibrary.Common.DI.Unity;
using HBLibrary.Common.Extensions;
using HBLibrary.Common.Plugins;
using HBLibrary.Services.IO;
using HBLibrary.Services.Logging;
using HBLibrary.Services.Logging.Loggers;
using HBLibrary.Wpf.Behaviors;
using HBLibrary.Wpf.Commands;
using HBLibrary.Wpf.Services;
using HBLibrary.Wpf.ViewModels;
using HBLibrary.Wpf.Views;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Reflection;
using System.Windows;
using System.Windows.Data;
using System.Windows.Documents;
using System.Xml.Serialization;
using Unity;

namespace FileManager.UI.ViewModels.JobViewModels;
public sealed class JobItemViewModel : AsyncInitializerViewModelBase<JobItemModel>, IDragDropTarget, IDisposable {
    private readonly IUnityContainer fileManagerContainer;
    private readonly IDialogService dialogService;
    private readonly IJobService jobService;
    private readonly IPluginManager pluginManager;

    public RelayCommand AddStepCommand { get; set; }
    public RelayCommand<JobStepWrapperViewModel> DeleteStepCommand { get; set; }

    public string Name {
        get => Model.Name;
        set {
            Model.Name = value;
            NotifyPropertyChanged();
        }
    }
    public bool IsEnabled {
        get => Model.IsEnabled;
        set {
            Model.IsEnabled = value;
            NotifyPropertyChanged();

            if (!value) {
                CanRun = false;
            }
            else {
                CheckAllIsValid()
                    .ContinueWith(e => CanRun = e.Result, TaskContinuationOptions.OnlyOnRanToCompletion)
                    .FireAndForget(OnInitializeException);
            }
        }
    }
    public bool CanRun {
        get => Model.CanRun;
        set {
            Model.CanRun = value;
            NotifyPropertyChanged();
        }
    }
    public TimeOnly? ScheduledAt {
        get => Model.ScheduledAt;
        set {
            Model.ScheduledAt = value;
            NotifyPropertyChanged();
        }
    }
    public bool OnDemand {
        get => Model.OnDemand;
        set {
            Model.OnDemand = value;
            NotifyPropertyChanged();
        }
    }
    public bool Scheduled {
        get => Model.Scheduled;
        set {
            Model.Scheduled = value;
            if (!value) {
                ScheduledAt = null;
            }

            NotifyPropertyChanged();
        }
    }
    public string? Description {
        get => Model.Description;
        set {
            Model.Description = value;
            NotifyPropertyChanged();
        }
    }

    private string? searchText;
    public string? SearchText {
        get => searchText;
        set {
            searchText = value;
            NotifyPropertyChanged();
            stepsView.Refresh();
        }
    }


    public FlowDocument? ValidationLog { get; }
    private bool validationLogVisible;

    public bool ValidationLogVisible {
        get { return validationLogVisible; }
        set { 
            validationLogVisible = value;
            NotifyPropertyChanged();
        }
    }



    private JobStepWrapperViewModel? selectedStep;
    public JobStepWrapperViewModel? SelectedStep {
        get => selectedStep;
        set {
            selectedStep = value;
            NotifyPropertyChanged();
        }
    }


    private readonly ObservableCollection<JobStepWrapperViewModel> steps = [];
    private readonly ICollectionView stepsView;
    public ICollectionView StepsView => stepsView;


    public JobItemViewModel(JobItemModel model) : base(model) {
        fileManagerContainer = UnityBase.GetChildContainer(nameof(FileManager))!;
        this.dialogService = fileManagerContainer.Resolve<IDialogService>();
        this.jobService = fileManagerContainer.Resolve<IJobService>();
        this.pluginManager = fileManagerContainer.Resolve<IPluginManager>();

        AddStepCommand = new RelayCommand(AddStep, true);
        DeleteStepCommand = new RelayCommand<JobStepWrapperViewModel>(DeleteStep, true);


        stepsView = CollectionViewSource.GetDefaultView(steps);
        stepsView.Filter = FilterJobSteps;
        stepsView.CollectionChanged += StepsView_CollectionChanged;
    }

    protected override async Task InitializeViewModelAsync() {
        LoadJobSteps();
        SelectedStep = steps.FirstOrDefault();

        bool canRun = true;
        foreach (JobStepWrapperViewModel step in steps) {
            ImmutableResultCollection res = await ValidateAsync(step.Model);

            // TODO: Handle faulted result
            canRun = canRun && res.IsSuccess;
        }

        CanRun = IsEnabled && canRun;
    }

    protected override void OnInitializeException(Exception exception) {
        // TODO: IMPLEMENT EXCEPTION HANDLING
    }

    private void AddStep(object? obj) {
        AddJobStepViewModel addJobViewModel = new AddJobStepViewModel(pluginManager);
        AddJobStepView addJobView = new AddJobStepView();

        bool result = dialogService.ShowCompactDialog(addJobView, addJobViewModel, "Add Step");
        if (result == true) {
            JobStep jobStep = (JobStep)Activator.CreateInstance(addJobViewModel.SelectedStepType!.StepType!)!;
            jobStep.IsValid = true;
            jobStep.Name = addJobViewModel.Name;

            JobStepWrapperViewModel stepViewModel = new JobStepWrapperViewModel(jobStep);
            steps.Add(stepViewModel);
            jobService.AddOrUpdateStep(Model.Id, stepViewModel.Model);
            SelectedStep = steps[steps.IndexOf(stepViewModel)];

            stepViewModel.StepContext!.ExecutionOrderChanged += JobItemViewModel_ExecutionOrderChanged;
            stepViewModel.StepContext!.ValidationRequired += JobItemViewModel_ValidationRequired;
            stepViewModel.StepContext!.AsyncValidationRequired += JobItemViewModel_AsyncValidationRequired;
        }
    }

    public void DeleteStep(JobStepWrapperViewModel stepViewModel) {
        MessageBoxResult result = HBDarkMessageBox.Show("Delete step",
            "Are you sure you want to delete this step?",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (result == MessageBoxResult.Yes) {
            stepViewModel.StepContext!.ExecutionOrderChanged -= JobItemViewModel_ExecutionOrderChanged;
            stepViewModel.StepContext!.ValidationRequired -= JobItemViewModel_ValidationRequired;
            stepViewModel.StepContext!.AsyncValidationRequired -= JobItemViewModel_AsyncValidationRequired;

            steps.Remove(stepViewModel);
            jobService.DeleteStep(Model.Id, stepViewModel.Model);
            SelectedStep = steps.FirstOrDefault();

            CheckAllIsValid()
                .ContinueWith(e => CanRun = e.Result, TaskContinuationOptions.OnlyOnRanToCompletion)
                .FireAndForget(OnInitializeException);
        }
    }

    private void LoadJobSteps() {
        foreach (JobStep step in Model.Steps) {
            JobStepWrapperViewModel stepViewModel = new JobStepWrapperViewModel(step);

            stepViewModel.StepContext!.ExecutionOrderChanged += JobItemViewModel_ExecutionOrderChanged;
            stepViewModel.StepContext!.ValidationRequired += JobItemViewModel_ValidationRequired;
            stepViewModel.StepContext!.AsyncValidationRequired += JobItemViewModel_AsyncValidationRequired;
            this.steps.Add(stepViewModel);
        }
    }

    private void JobItemViewModel_ExecutionOrderChanged(int oldValue, JobStep target) {
        JobStepWrapperViewModel? foundVM = steps.FirstOrDefault(e => e.Model == target);

        if (foundVM is null) {
            return;
        }

        int index = steps.IndexOf(foundVM);
        int newIndex = index;

        if (target.ExecutionOrder > oldValue) {
            while ((newIndex + 1) < steps.Count && target.ExecutionOrder > steps[newIndex + 1].Model.ExecutionOrder) {
                newIndex++;
            }
        }
        else if (target.ExecutionOrder < oldValue) {
            while (newIndex > 0 && target.ExecutionOrder < steps[newIndex - 1].Model.ExecutionOrder) {
                newIndex--;
            }
        }

        if (newIndex != index) {
            steps.Move(index, newIndex);
            SelectedStep = steps[newIndex];

            // Apply new indexed list
            Model.Steps.Clear();
            Model.Steps.AddRange(steps.Select(e => e.Model));
        }
    }

    private bool FilterJobSteps(object obj) {
        if (obj is JobStepWrapperViewModel step) {
            return string.IsNullOrEmpty(SearchText) || step.Model.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase);
        }
        return false;
    }

    private void StepsView_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) {
        SelectedStep = stepsView.Cast<JobStepWrapperViewModel>().FirstOrDefault();
    }

    public void MoveItem(object source, object target) {
        if (source is JobStepWrapperViewModel sourceItem && target is JobStepWrapperViewModel targetItem) {
            int oldIndex = steps.IndexOf(sourceItem);
            int newIndex = steps.IndexOf(targetItem);

            if (oldIndex != newIndex) {
                steps.Move(oldIndex, newIndex);
                NotifyPropertyChanged(nameof(StepsView));
                SelectedStep = sourceItem;

                // Apply new indexed list
                Model.Steps.Clear();
                Model.Steps.AddRange(steps.Select(e => e.Model));

                if (targetItem.Model.ExecutionOrder != sourceItem.Model.ExecutionOrder) {
                    sourceItem.Model.ExecutionOrder = targetItem.Model.ExecutionOrder;
                }
            }
        }
    }

    public void Dispose() {
        foreach (JobStepWrapperViewModel step in steps) {
            step.StepContext!.ExecutionOrderChanged -= JobItemViewModel_ExecutionOrderChanged;
            step.StepContext!.ValidationRequired -= JobItemViewModel_ValidationRequired;
            step.StepContext!.AsyncValidationRequired -= JobItemViewModel_AsyncValidationRequired;
        }

        stepsView.CollectionChanged -= StepsView_CollectionChanged;
    }

    #region Validation Logic
    private async Task<bool> JobItemViewModel_AsyncValidationRequired(JobStep arg) {
        ImmutableResultCollection res = await ValidateAsync(arg);
        // TODO: Handle faulted case

        CanRun = res.IsSuccess;
        

        return res.IsSuccess;
    }

    private bool JobItemViewModel_ValidationRequired(JobStep jobStep) {
        ImmutableResultCollection res = Validate(jobStep);

        CanRun = res.IsSuccess;

        // TODO: Handle faulted case
        return res.IsSuccess;
    }

    private ImmutableResultCollection Validate(JobStep jobStep) {
        ILoggerFactory loggerFactory = fileManagerContainer.Resolve<ILoggerFactory>();
        UnityContainer tempContainer = new UnityContainer();

        ILogger tempLogger = loggerFactory.CreateLogger(jobStep.Name);
        tempContainer.RegisterInstance(tempLogger);
        tempContainer.RegisterType<IFileEntryService, FileEntryService>();

        ImmutableResultCollection res = jobStep.Validate(tempContainer);

        tempContainer.Dispose();
        return res;
    }

    private async Task<ImmutableResultCollection> ValidateAsync(JobStep jobStep) {
        ILoggerFactory loggerFactory = fileManagerContainer.Resolve<ILoggerFactory>();
        UnityContainer tempContainer = new UnityContainer();

        IAsyncLogger tempLogger = loggerFactory.CreateAsyncLogger(jobStep.Name);
        tempContainer.RegisterInstance(tempLogger);
        tempContainer.RegisterType<IFileEntryService, FileEntryService>();

        ImmutableResultCollection res = await jobStep.ValidateAsync(tempContainer);

        tempContainer.Dispose();
        return res;
    }

    private async Task<bool> CheckAllIsValid() {
        bool canRun = true;

        foreach (JobStepWrapperViewModel step in steps) {
            while (step.StepContext!.AsyncValidationRunning || step.StepContext.ValidationRunning) {
                // Wait for Validation to finish
                await Task.Delay(100);
            }

            canRun = canRun && step.StepContext.IsValid;
        }

        return canRun;
    }
    #endregion
}
