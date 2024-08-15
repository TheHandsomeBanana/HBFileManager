using FileManager.UI.Models.Job;
using FileManager.UI.Models.JobModels;
using FileManager.UI.Models.JobModels.JobStepModels;
using FileManager.UI.Services.JobService;
using FileManager.UI.ViewModels.JobViewModels;
using FileManager.UI.ViewModels.JobViewModels.JobStepViewModels;
using FileManager.UI.Views;
using FileManager.UI.Views.JobViews;
using FileManager.UI.Views.JobViews.JobStepViews;
using HBLibrary.Common.DI.Unity;
using HBLibrary.Wpf.Commands;
using HBLibrary.Wpf.Services;
using HBLibrary.Wpf.ViewModels;
using HBLibrary.Wpf.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using Unity;

namespace FileManager.UI.ViewModels.JobViewModels;
public class JobItemViewModel : ViewModelBase<JobItemModel> {
    private readonly IDialogService dialogService;
    private readonly IJobService jobService;

    public RelayCommand AddStepCommand { get; set; }
    public RelayCommand<JobItemStepViewModel> DeleteStepCommand { get; set; }

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


    private JobItemStepViewModel? selectedStep;
    public JobItemStepViewModel? SelectedStep {
        get => selectedStep;
        set {
            selectedStep = value;
            NotifyPropertyChanged();
        }
    }


    private readonly ObservableCollection<JobItemStepViewModel> steps = [];
    private readonly ICollectionView stepsView;
    public ICollectionView StepsView => stepsView;

    

    public JobItemViewModel(JobItemModel model) : base(model) {
        IUnityContainer container = UnityBase.GetChildContainer(nameof(FileManager))!;
        this.dialogService = container.Resolve<IDialogService>();
        this.jobService = container.Resolve<IJobService>();

        AddStepCommand = new RelayCommand(AddStep, true);
        DeleteStepCommand = new RelayCommand<JobItemStepViewModel>(DeleteStep, true);

        LoadJobSteps();

        stepsView = CollectionViewSource.GetDefaultView(steps);
        stepsView.Filter = FilterJobSteps;

        if (steps.Any()) {
            SelectedStep = steps[0];
        }
    }

    public JobItemViewModel() : this(new JobItemModel()) { }


    private void AddStep(object? obj) {
        AddJobStepViewModel addJobViewModel = new AddJobStepViewModel();
        AddJobStepView addJobView = new AddJobStepView();

        bool result = dialogService.ShowCompactDialog(addJobView, addJobViewModel, "Add Step");
        if (result == true) {
            JobItemStepViewModel stepViewModel = JobSteps.CreateStepVM(addJobViewModel.Name, addJobViewModel.SelectedStepType);
            steps.Add(stepViewModel);
            jobService.AddOrUpdateStep(Model.Id, stepViewModel.Model);
        }
    }

    public void DeleteStep(JobItemStepViewModel stepViewModel) {
        MessageBoxResult result = HBDarkMessageBox.Show("Delete step",
            "Are you sure you want to delete this step?",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (result == MessageBoxResult.Yes) {
            steps.Remove(stepViewModel);
            jobService.DeleteStep(Model.Id, stepViewModel.Model.Id);
        }
    }


    private void LoadJobSteps() {
        foreach (JobItemStepModel step in Model.Steps.Values) {
            switch (step.StepType) {
                case StepType.Archive:
                    this.steps.Add(new ArchiveStepViewModel((ArchiveStepModel)step));
                    break;
                case StepType.Copy:
                    this.steps.Add(new CopyStepViewModel((CopyStepModel)step));
                    break;
            }
        }
    }


    private bool FilterJobSteps(object obj) {
        if (obj is JobItemStepViewModel step) {
            return string.IsNullOrEmpty(SearchText) || step.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase);
        }
        return false;
    }
}
