using FileManager.Core.JobSteps;
using FileManager.Core.JobSteps.Models;
using FileManager.Core.JobSteps.ViewModels;
using FileManager.Core.JobSteps.Views;
using FileManager.UI.Models.JobModels;
using FileManager.UI.Services.JobService;
using FileManager.UI.ViewModels.JobViewModels.JobStepViewModels;
using FileManager.UI.Views.JobViews.JobStepViews;
using HBLibrary.Common.DI.Unity;
using HBLibrary.Common.Plugins;
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
using Unity;

namespace FileManager.UI.ViewModels.JobViewModels;
public class JobItemViewModel : ViewModelBase<JobItemModel>, IDragDropTarget {
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


    private JobStepWrapperViewModel? selectedStep;
    public JobStepWrapperViewModel? SelectedStep {
        get => selectedStep;
        set {

            // Remove the event handler for the event "ExecutionOrderChanged" using reflection
            // No way to access it otherwise -> bad architecture :(
            if (selectedStep?.StepViewModel is not null) {
                selectedStep.StepViewModel.GetType()
                    .GetEvent("ExecutionOrderChanged")?
                    .RemoveEventHandler(selectedStep.StepViewModel, JobStepViewModel_ExecutionOrderChanged);
            }

            selectedStep = value;
            NotifyPropertyChanged();

            // Add the event handler for the event "ExecutionOrderChanged" using reflection
            // No way to access it otherwise -> bad architecture :(
            value?.StepViewModel?.GetType()
               .GetEvent("ExecutionOrderChanged")?
               .AddEventHandler(value.StepViewModel, JobStepViewModel_ExecutionOrderChanged);
        }
    }


    private readonly ObservableCollection<JobStepWrapperViewModel> steps = [];
    private readonly ICollectionView stepsView;
    public ICollectionView StepsView => stepsView;



    public JobItemViewModel(JobItemModel model) : base(model) {
        IUnityContainer container = UnityBase.GetChildContainer(nameof(FileManager))!;
        this.dialogService = container.Resolve<IDialogService>();
        this.jobService = container.Resolve<IJobService>();
        this.pluginManager = container.Resolve<IPluginManager>();

        AddStepCommand = new RelayCommand(AddStep, true);
        DeleteStepCommand = new RelayCommand<JobStepWrapperViewModel>(DeleteStep, true);

        LoadJobSteps();

        stepsView = CollectionViewSource.GetDefaultView(steps);
        stepsView.Filter = FilterJobSteps;
        stepsView.CollectionChanged += StepsView_CollectionChanged;

        SelectedStep = steps.FirstOrDefault();
    }


    private void AddStep(object? obj) {
        AddJobStepViewModel addJobViewModel = new AddJobStepViewModel(pluginManager);
        AddJobStepView addJobView = new AddJobStepView();

        bool result = dialogService.ShowCompactDialog(addJobView, addJobViewModel, "Add Step");
        if (result == true) {
            JobStep? jobStep = Activator.CreateInstance(addJobViewModel.SelectedStepType!.StepType!) as JobStep;
            jobStep!.Name = addJobViewModel.Name;

            JobStepWrapperViewModel stepViewModel = new JobStepWrapperViewModel(jobStep);
            steps.Add(stepViewModel);
            jobService.AddOrUpdateStep(Model.Id, stepViewModel.Model);
            SelectedStep = steps[steps.IndexOf(stepViewModel)];
        }
    }


    public void DeleteStep(JobStepWrapperViewModel stepViewModel) {
        MessageBoxResult result = HBDarkMessageBox.Show("Delete step",
            "Are you sure you want to delete this step?",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (result == MessageBoxResult.Yes) {
            steps.Remove(stepViewModel);
            jobService.DeleteStep(Model.Id, stepViewModel.Model);
            SelectedStep = steps.FirstOrDefault();
        }
    }


    private void LoadJobSteps() {
        foreach (JobStep step in Model.Steps) {
            JobStepWrapperViewModel stepViewModel = new JobStepWrapperViewModel(step);
            this.steps.Add(stepViewModel);
        }
    }

    private void JobStepViewModel_ExecutionOrderChanged(int oldValue, JobStep target) {
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
        else if(target.ExecutionOrder < oldValue) {
            while(newIndex > 0 && target.ExecutionOrder < steps[newIndex - 1].Model.ExecutionOrder) {
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
}
