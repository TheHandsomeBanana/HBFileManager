using FileManager.UI.Models.Job;
using FileManager.UI.Services.JobService;
using FileManager.UI.ViewModels.Jobs;
using FileManager.UI.ViewModels.JobViewModels;
using FileManager.UI.Views.JobViews;
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

namespace FileManager.UI.ViewModels;
public class JobsViewModel : ViewModelBase {
    private readonly IJobService jobService;
    private readonly IDialogService dialogService;
    private readonly ObservableCollection<JobItemViewModel> jobs = [];

    public RelayCommand AddJobCommand { get; set; }
    public RelayCommand<JobItemViewModel> DeleteJobCommand { get; set; }


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
        IUnityContainer container = UnityBase.GetChildContainer(nameof(FileManager))!;
        this.jobService = container.Resolve<IJobService>();
        this.dialogService = container.Resolve<IDialogService>();

        AddJobCommand = new RelayCommand(AddJob, true);
        DeleteJobCommand = new RelayCommand<JobItemViewModel>(DeleteJob, true);

        LoadJobs();

        jobsView = CollectionViewSource.GetDefaultView(jobs);
        jobsView.Filter = FilterJobs;

        if (jobs.Any()) {
            SelectedJob = jobs[0];
        }
    }

    private bool FilterJobs(object obj) {
        if (obj is JobItemViewModel job) {
            return string.IsNullOrEmpty(SearchText) || job.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase);
        }
        return false;
    }

    private void LoadJobs() {
        JobItemModel[] jobs = jobService.GetAll();
        foreach (JobItemModel item in jobs) {
            this.jobs.Add(new JobItemViewModel(item));
        }
    }

    public void AddJob(object? o) {
        AddJobViewModel addJobViewModel = new AddJobViewModel();
        AddJobView addJobView = new AddJobView();

        bool result = dialogService.ShowCompactDialog(addJobView, addJobViewModel, "Add Job");
        if (result == true) {
            JobItemModel newJob = new JobItemModel() { Id = Guid.NewGuid(), Name = addJobViewModel.Name };

            JobItemViewModel jobItemViewModel = new JobItemViewModel(newJob);
            jobs.Add(jobItemViewModel);
            SelectedJob = jobItemViewModel;

            jobService.AddOrUpdate(newJob);
        }
    }

    public void DeleteJob(JobItemViewModel jobItemViewModel) {
        MessageBoxResult result = HBDarkMessageBox.Show("Delete job",
            "Are you sure you want to delete this job?",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (result == MessageBoxResult.Yes) {
            jobs.Remove(jobItemViewModel);
            jobService.Delete(jobItemViewModel.Model.Id);
        }
    }
}
