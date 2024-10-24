﻿using FileManager.Core.JobSteps;
using FileManager.Core.JobSteps.Models;
using FileManager.Core.JobSteps.ViewModels;
using FileManager.Core.JobSteps.Views;
using FileManager.UI.ViewModels.JobViewModels.JobStepViewModels;
using FileManager.UI.Views.JobViews.JobStepViews;
using HBLibrary.Common;
using HBLibrary.Common.Plugins;
using HBLibrary.Wpf;
using HBLibrary.Wpf.Behaviors;
using HBLibrary.Wpf.Commands;
using HBLibrary.Wpf.Services;
using HBLibrary.Wpf.ViewModels;
using HBLibrary.Wpf.Views;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Reflection;
using System.Security.RightsManagement;
using System.Windows;
using System.Windows.Data;
using System.Windows.Documents;
using System.Xml.Serialization;
using Unity;
using FileManager.Core.Job;
using FileManager.Core.Workspace;
using FileManager.UI.Services.SettingsService;
using FileManager.UI.Models.SettingsModels;
using HBLibrary.Interface.Plugins;
using HBLibrary.Core.Extensions;
using HBLibrary.DI;
using HBLibrary.Interface.Workspace;
using HBLibrary.DataStructures;
using HBLibrary.Interface.Logging;
using HBLibrary.Interface.IO;
using HBLibrary.IO;
using HBLibrary.Interface.Core;

namespace FileManager.UI.ViewModels.JobViewModels;
public sealed class JobItemViewModel : AsyncInitializerViewModelBase<Job>, IDragDropTarget, IResetable, IDisposable {
    private readonly IUnityContainer container;
    private readonly IDialogService dialogService;
    private readonly JobManager jobManager;
    private readonly IPluginManager pluginManager;

    public RelayCommand AddStepCommand { get; set; }
    public RelayCommand<JobStepWrapperViewModel> DeleteStepCommand { get; set; }
    public AsyncRelayCommand<JobStepWrapperViewModel> ValidateStepCommand { get; set; }
    public RelayCommand ClearValidationLogCommand { get; set; }

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
                    .ContinueWith(e => CanRun = e.Result && IsEnabled, TaskContinuationOptions.OnlyOnRanToCompletion)
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


    public LogFlowDocument ValidationLog { get; } = new LogFlowDocument();


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


    public ObservableCollection<JobStepWrapperViewModel> Steps { get; } = [];
    private readonly ICollectionView stepsView;
    public ICollectionView StepsView => stepsView;


    public JobItemViewModel(Job model) : base(model) {
        container = UnityBase.Registry.Get(ApplicationHandler.FileManagerContainerGuid);
        this.dialogService = container.Resolve<IDialogService>();
        this.pluginManager = container.Resolve<IPluginManager>();

        IApplicationWorkspaceManager<HBFileManagerWorkspace> workspaceManager = container.Resolve<IApplicationWorkspaceManager<HBFileManagerWorkspace>>();
        this.jobManager = workspaceManager.CurrentWorkspace!.JobManager!;


        AddStepCommand = new RelayCommand(AddStep, CanEditStepsList);
        DeleteStepCommand = new RelayCommand<JobStepWrapperViewModel>(DeleteStep, CanEditStepsList);
        ValidateStepCommand = new AsyncRelayCommand<JobStepWrapperViewModel>(ValidateStep, CanExecuteValidation, OnValidationException);
        ClearValidationLogCommand = new RelayCommand(ClearValidationLog, true);

        stepsView = CollectionViewSource.GetDefaultView(Steps);
        stepsView.Filter = FilterJobSteps;
        stepsView.CollectionChanged += StepsView_CollectionChanged;
    }

    protected override async Task InitializeViewModelAsync() {
        LoadJobSteps();
        SelectedStep = Steps.FirstOrDefault();

        ISettingsService settingsService = container.Resolve<ISettingsService>();
        SettingsEnvironmentModel environmentSettings = settingsService.GetSetting<SettingsEnvironmentModel>()!;

        if (!environmentSettings.ValidateOnNavigation) {
            return;
        }

        bool canRun = true;
        List<Task<bool>> validationTasks = [];
        foreach (JobStepWrapperViewModel step in Steps) {
            validationTasks.Add(ValidateJobStepAsync(step.Model));
        }

        bool[] res = await Task.WhenAll(validationTasks);
        for (int i = 0; i < res.Length; i++) {
            canRun &= res[i];
        }

        CanRun = IsEnabled && canRun;
    }

    protected override void OnInitializeException(Exception exception) {
        HBDarkMessageBox.Show("Initialization error",
            exception.Message,
            MessageBoxButton.OK,
            MessageBoxImage.Error);
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
            Steps.Add(stepViewModel);
            jobManager.AddOrUpdateStep(Model.Id, stepViewModel.Model);
            SelectedStep = Steps[Steps.IndexOf(stepViewModel)];

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

            Steps.Remove(stepViewModel);
            jobManager.DeleteStep(Model.Id, stepViewModel.Model);
            SelectedStep = Steps.FirstOrDefault();

            CheckAllIsValid()
                .ContinueWith(e => CanRun = e.Result && IsEnabled, TaskContinuationOptions.OnlyOnRanToCompletion)
                .FireAndForget(OnInitializeException);
        }
    }

    private bool jobValidationRunning;
    private bool CanEditStepsList(object? obj) {
        return !jobValidationRunning;
    }



    private void LoadJobSteps() {
        foreach (JobStep step in Model.Steps) {
            JobStepWrapperViewModel stepViewModel = new JobStepWrapperViewModel(step);

            stepViewModel.StepContext!.ExecutionOrderChanged += JobItemViewModel_ExecutionOrderChanged;
            stepViewModel.StepContext!.ValidationRequired += JobItemViewModel_ValidationRequired;
            stepViewModel.StepContext!.AsyncValidationRequired += JobItemViewModel_AsyncValidationRequired;
            this.Steps.Add(stepViewModel);
        }
    }

    private void JobItemViewModel_ExecutionOrderChanged(int oldValue, JobStep target) {
        JobStepWrapperViewModel? foundVM = Steps.FirstOrDefault(e => e.Model == target);

        if (foundVM is null) {
            return;
        }

        int index = Steps.IndexOf(foundVM);
        int newIndex = index;

        if (target.ExecutionOrder > oldValue) {
            while ((newIndex + 1) < Steps.Count && target.ExecutionOrder > Steps[newIndex + 1].Model.ExecutionOrder) {
                newIndex++;
            }
        }
        else if (target.ExecutionOrder < oldValue) {
            while (newIndex > 0 && target.ExecutionOrder < Steps[newIndex - 1].Model.ExecutionOrder) {
                newIndex--;
            }
        }

        if (newIndex != index) {
            Steps.Move(index, newIndex);
            SelectedStep = Steps[newIndex];
            Model.Steps.Move(index, newIndex);
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
            int oldIndex = Steps.IndexOf(sourceItem);
            int newIndex = Steps.IndexOf(targetItem);

            if (oldIndex != newIndex) {
                Steps.Move(oldIndex, newIndex);
                NotifyPropertyChanged(nameof(StepsView));
                Model.Steps.Move(oldIndex, newIndex);

                SelectedStep = sourceItem;

                if (targetItem.Model.ExecutionOrder != sourceItem.Model.ExecutionOrder) {
                    sourceItem.Model.ExecutionOrder = targetItem.Model.ExecutionOrder;
                }
            }
        }
    }

    public void Reset() {
        IsInitialized = false;

        Unhook();
        Clear();
    }

    public void Dispose() {
        Unhook();
        Clear();
    }

    private void Unhook() {
        foreach (JobStepWrapperViewModel step in Steps) {
            step.StepContext!.ExecutionOrderChanged -= JobItemViewModel_ExecutionOrderChanged;
            step.StepContext!.ValidationRequired -= JobItemViewModel_ValidationRequired;
            step.StepContext!.AsyncValidationRequired -= JobItemViewModel_AsyncValidationRequired;
        }

        stepsView.CollectionChanged -= StepsView_CollectionChanged;
    }
    private void Clear() {
        this.Steps.Clear();
        ValidationLog.Clear();
    }

    #region Validation Logic
    private void ClearValidationLog(object? obj) {
        ValidationLog.Clear();
    }

    private Task ValidateStep(JobStepWrapperViewModel model) {
        return ValidateJobStepAsync(model.Model);
    }

    private bool CanExecuteValidation(JobStepWrapperViewModel obj) {
        return !(obj.StepContext?.ValidationRunning ?? true) && !(obj.StepContext?.AsyncValidationRunning ?? true);
    }

    private void OnValidationException(Exception exception) {
        HBDarkMessageBox.Show("Validation error",
                    exception.Message,
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
    }

    public bool ValidateJobStep(JobStep jobStep) {
        ValidationLogVisible = true;
        ValidationLog.AddInfoParagraph($"Validating {jobStep.Name}");

        ImmutableResultCollection res = Validate(jobStep);
        HandleLogs(res, jobStep);

        CanRun = res.IsSuccess && IsEnabled && Steps.All(e => e.StepContext!.IsValid);

        AddStepCommand.NotifyCanExecuteChanged();
        return res.IsSuccess;
    }

    public async Task<bool> ValidateJobStepAsync(JobStep jobStep) {
        Application.Current.Dispatcher.Invoke(() => ValidationLogVisible = true);

        ValidationLog.AddInfoParagraph($"Validating {jobStep.Name}");

        ImmutableResultCollection res = await ValidateAsync(jobStep);

        HandleLogs(res, jobStep);

        CanRun = res.IsSuccess && IsEnabled && Steps.All(e => e.StepContext!.IsValid);

        AddStepCommand.NotifyCanExecuteChanged();
        return res.IsSuccess;
    }

    public async Task ValidateAllJobStepsAsync(IEnumerable<JobStepWrapperViewModel> jobSteps) {
        jobValidationRunning = true;
        AddStepCommand.NotifyCanExecuteChanged();
        DeleteStepCommand.NotifyCanExecuteChanged();

        Application.Current.Dispatcher.Invoke(() => ValidationLogVisible = true);
        bool canRun = true;
        foreach (JobStepWrapperViewModel jobStep in jobSteps) {
            ValidationLog.AddInfoParagraph($"Validating {jobStep.Model.Name}");

            ImmutableResultCollection res = await ValidateAsync(jobStep.Model);
            HandleLogs(res, jobStep.Model);
            canRun &= res.IsSuccess;
        }

        CanRun = canRun && IsEnabled;

        jobValidationRunning = false;
        AddStepCommand.NotifyCanExecuteChanged();
        DeleteStepCommand.NotifyCanExecuteChanged();
    }

    private Task<bool> JobItemViewModel_AsyncValidationRequired(JobStep arg) {
        return ValidateJobStepAsync(arg);
    }

    private bool JobItemViewModel_ValidationRequired(JobStep jobStep) {
        return ValidateJobStep(jobStep);
    }

    private ImmutableResultCollection Validate(JobStep jobStep) {
        ILoggerFactory loggerFactory = container.Resolve<ILoggerFactory>();
        UnityContainer tempContainer = new UnityContainer();

        ILogger tempLogger = loggerFactory.CreateLogger(jobStep.Name);
        tempContainer.RegisterInstance(tempLogger);
        tempContainer.RegisterType<IFileEntryService, FileEntryService>();

        ImmutableResultCollection res = jobStep.Validate(tempContainer);

        tempContainer.Dispose();
        return res;
    }

    private async Task<ImmutableResultCollection> ValidateAsync(JobStep jobStep) {
        ILoggerFactory loggerFactory = container.Resolve<ILoggerFactory>();
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

        foreach (JobStepWrapperViewModel step in Steps) {
            while (step.StepContext!.AsyncValidationRunning || step.StepContext.ValidationRunning) {
                // Wait for Validation to finish
                await Task.Delay(100);
            }

            canRun = canRun && step.StepContext.IsValid;
        }

        return canRun;
    }

    private void HandleLogs(ImmutableResultCollection res, JobStep arg) {
        if (res.IsSuccess) {
            ValidationLog.AddSuccessParagraph($"{arg.Name} validated successfully");
        }
        else {
            ValidationLog.AddErrorParagraph($"{arg.Name} validation error:");

            foreach (Result result in res) {
                result.TapError(e => {
                    ValidationLog.AddParagraph(new Paragraph(new Run("- " + e.Message)) {
                        TextIndent = 10,
                        Margin = ValidationLog.ParagraphMargin,
                        Foreground = ValidationLog.ErrorBrush
                    });
                });
            }
        }
    }
    #endregion
}
