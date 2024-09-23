using FileManager.Core.JobSteps;
using FileManager.UI.Models.JobModels;
using FileManager.UI.Services.JobService;
using FileManager.UI.ViewModels.JobViewModels.JobStepViewModels;
using FileManager.UI.Views.JobViews.JobStepViews;
using HBLibrary.Common.DI.Unity;
using HBLibrary.Common.Plugins;
using HBLibrary.Wpf.Commands;
using HBLibrary.Wpf.Services;
using HBLibrary.Wpf.ViewModels;
using HBLibrary.Wpf.Views;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Data;
using Unity;

namespace FileManager.UI.ViewModels.JobViewModels;
public class JobItemViewModel : ViewModelBase<JobItemModel> {
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
            selectedStep = value;
            NotifyPropertyChanged();
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

        SelectedStep = steps.FirstOrDefault();
    }

    public JobItemViewModel() : this(new JobItemModel()) { }


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
            jobService.DeleteStep(Model.Id, stepViewModel.Model.Id);
            SelectedStep = steps.FirstOrDefault();
        }
    }


    private void LoadJobSteps() {
        foreach (JobStep step in Model.Steps.Values) {
            JobStepWrapperViewModel stepViewModel = new JobStepWrapperViewModel(step);
            this.steps.Add(stepViewModel);
        }
    }


    private bool FilterJobSteps(object obj) {
        if (obj is JobStepWrapperViewModel step) {
            return string.IsNullOrEmpty(SearchText) || step.Model.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase);
        }
        return false;
    }
}
