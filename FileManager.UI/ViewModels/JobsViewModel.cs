﻿using FileManager.Core.Job;
using FileManager.Core.JobSteps.Views;
using FileManager.Core.Workspace;
using FileManager.UI.ViewModels.JobViewModels;
using FileManager.UI.ViewModels.JobViewModels.JobStepViewModels;
using FileManager.UI.Views.JobViews;
using HBLibrary.Common.DI.Unity;
using HBLibrary.Common.Workspace;
using HBLibrary.Wpf.Behaviors;
using HBLibrary.Wpf.Commands;
using HBLibrary.Wpf.Services;
using HBLibrary.Wpf.ViewModels;
using HBLibrary.Wpf.Views;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Data;
using Unity;

namespace FileManager.UI.ViewModels;
public sealed class JobsViewModel : InitializerViewModelBase, IDisposable, IDragDropTarget {
    private readonly IJobManager jobManager;
    private readonly IDialogService dialogService;
    private readonly ObservableCollection<JobItemViewModel> jobs = [];

    public RelayCommand AddJobCommand { get; set; }
    public RelayCommand<JobItemViewModel> DeleteJobCommand { get; set; }
    public AsyncRelayCommand<JobItemViewModel> ValidateJobCommand { get; set; }


    private JobItemViewModel? selectedJob;
    public JobItemViewModel? SelectedJob {
        get => selectedJob;
        set {
            selectedJob = value;
            NotifyPropertyChanged();
        }
    }


    private readonly ICollectionView jobsView;
    public ICollectionView JobsView => jobsView;

    private string? searchText;
    public string? SearchText {
        get => searchText;
        set {
            searchText = value;
            NotifyPropertyChanged();
            jobsView.Refresh();
        }
    }

    public JobsViewModel() {
        IUnityContainer container = UnityBase.Registry.Get(ApplicationHandler.FileManagerContainerGuid);
        this.dialogService = container.Resolve<IDialogService>();
        IApplicationWorkspaceManager<HBFileManagerWorkspace> workspaceManager = container.Resolve<IApplicationWorkspaceManager<HBFileManagerWorkspace>>();

        this.jobManager = workspaceManager.CurrentWorkspace!.JobManager!;

        AddJobCommand = new RelayCommand(AddJob, true);
        DeleteJobCommand = new RelayCommand<JobItemViewModel>(DeleteJob, true);
        ValidateJobCommand = new AsyncRelayCommand<JobItemViewModel>(ValidateJob, ValidationCanRun, OnValidationException);

        jobsView = CollectionViewSource.GetDefaultView(jobs);
        jobsView.Filter = FilterJobs;
        jobsView.CollectionChanged += JobsView_CollectionChanged;
    }

    private void OnValidationException(Exception exception) {
        HBDarkMessageBox.Show("Validation error",
            exception.Message,
            MessageBoxButton.OK,
            MessageBoxImage.Error);
    }

    private Task ValidateJob(JobItemViewModel model) {
        List<Task> validationTasks = [];
        return model.ValidateAllJobStepsAsync(model.Steps.Select(e => e.Model));
    }

    private bool ValidationCanRun(JobItemViewModel obj) {
        return obj.Steps.All(e => !(e.StepContext?.ValidationRunning ?? false) && !(e.StepContext?.AsyncValidationRunning ?? false));
    }

    protected override void InitializeViewModel() {
        LoadJobs();
        SelectedJob = jobs.FirstOrDefault();
    }

    private void JobsView_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) {
        SelectedJob = jobsView.Cast<JobItemViewModel>().FirstOrDefault();
    }

    private bool FilterJobs(object obj) {
        if (obj is JobItemViewModel job) {
            return string.IsNullOrEmpty(SearchText) || job.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase);
        }
        return false;
    }

    private void LoadJobs() {
        Job[] jobs = jobManager.GetAll();
        foreach (Job item in jobs) {
            JobItemViewModel jobItemViewModel = new JobItemViewModel(item);
            this.jobs.Add(jobItemViewModel);
        }
    }

    public void AddJob(object? o) {
        AddJobViewModel addJobViewModel = new AddJobViewModel();
        AddJobView addJobView = new AddJobView();

        bool result = dialogService.ShowCompactDialog(addJobView, addJobViewModel, "Add Job");
        if (result == true) {
            Job newJob = new Job() { Id = Guid.NewGuid(), Name = addJobViewModel.Name };

            JobItemViewModel jobItemViewModel = new JobItemViewModel(newJob);
            jobs.Add(jobItemViewModel);
            jobManager.AddOrUpdate(newJob);

            SelectedJob = jobItemViewModel;
        }
    }

    public void DeleteJob(JobItemViewModel jobItemViewModel) {
        MessageBoxResult result = HBDarkMessageBox.Show("Delete job",
            "Are you sure you want to delete this job?",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (result == MessageBoxResult.Yes) {
            jobs.Remove(jobItemViewModel);
            jobManager.Delete(jobItemViewModel.Model.Id);
            SelectedJob = jobs.FirstOrDefault();
        }
    }

    public void Dispose() {
        foreach (JobItemViewModel job in jobs) {
            job.Dispose();
        }

        JobsView.CollectionChanged -= JobsView_CollectionChanged;
    }

    public void MoveItem(object source, object target) {
        if (source is JobItemViewModel sourceItem && target is JobItemViewModel targetItem) {
            int oldIndex = jobs.IndexOf(sourceItem);
            int newIndex = jobs.IndexOf(targetItem);

            if (oldIndex != newIndex) {
                jobs.Move(oldIndex, newIndex);
                NotifyPropertyChanged(nameof(JobsView));
                SelectedJob = sourceItem;
            }
        }
    }
}
