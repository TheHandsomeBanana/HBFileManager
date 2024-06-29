using FileManager.UI.Models.Job;
using FileManager.UI.Services;
using FileManager.UI.ViewModels.JobViewModels;
using FileManager.UI.Views.JobViews;
using HBLibrary.Common.DI.Unity;
using HBLibrary.Wpf.Commands;
using HBLibrary.Wpf.Services;
using HBLibrary.Wpf.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using Unity;

namespace FileManager.UI.ViewModels.Jobs;
public class JobListViewModel : ViewModelBase {
    private readonly IJobService jobService;
    private readonly IDialogService dialogService;
    private readonly ObservableCollection<JobItemViewModel> jobs = [];

    public RelayCommand AddJobCommand { get; set; }
    public RelayCommand<JobItemViewModel> DeleteJobCommand { get; set; }

    public ObservableCollection<JobItemViewModel> Jobs => jobs;

    private JobItemViewModel selectedJob;
    public JobItemViewModel SelectedJob {
        get => selectedJob;
        set {
            selectedJob = value;
            NotifyPropertyChanged();
        }
    }

    private ICollectionView jobsView;
    public ICollectionView JobsView => jobsView;

    private string searchText;
    public string SearchText {
        get => searchText;
        set {
            searchText = value;
            NotifyPropertyChanged();
            jobsView.Refresh();
        }
    }

    public JobListViewModel() {
        IUnityContainer container = UnityBase.GetChildContainer(nameof(FileManager))!;
        this.jobService = container.Resolve<IJobService>();
        this.dialogService = container.Resolve<IDialogService>();

        AddJobCommand = new RelayCommand(AddJob, true);
        DeleteJobCommand = new RelayCommand<JobItemViewModel>(DeleteJob, true);

        jobs = [
            new JobItemViewModel(new JobItemModel() { Name = "Test" }),
            new JobItemViewModel(new JobItemModel() { Name = "Test" }),
            new JobItemViewModel(new JobItemModel() { Name = "Test" }),
            new JobItemViewModel(new JobItemModel() { Name = "Test" }),
            new JobItemViewModel(new JobItemModel() { Name = "Test" }),
            new JobItemViewModel(new JobItemModel() { Name = "Test" }),
            new JobItemViewModel(new JobItemModel() { Name = "Test" }),
            new JobItemViewModel(new JobItemModel() { Name = "Test" }),
            new JobItemViewModel(new JobItemModel() { Name = "Test" }),
            new JobItemViewModel(new JobItemModel() { Name = "Test" }),
            new JobItemViewModel(new JobItemModel() { Name = "Test" }),
            new JobItemViewModel(new JobItemModel() { Name = "Test" }),
            new JobItemViewModel(new JobItemModel() { Name = "Test" }),
            new JobItemViewModel(new JobItemModel() { Name = "Test" }),
            new JobItemViewModel(new JobItemModel() { Name = "Test" }),
            new JobItemViewModel(new JobItemModel() { Name = "Test" }),
            new JobItemViewModel(new JobItemModel() { Name = "Test" }),
            new JobItemViewModel(new JobItemModel() { Name = "Test" }),
            new JobItemViewModel(new JobItemModel() { Name = "Test" }),
            new JobItemViewModel(new JobItemModel() { Name = "Test" }),
            new JobItemViewModel(new JobItemModel() { Name = "Test" }),
            new JobItemViewModel(new JobItemModel() { Name = "Test" }),
            new JobItemViewModel(new JobItemModel() { Name = "Test" }),
            new JobItemViewModel(new JobItemModel() { Name = "Test" }),
            new JobItemViewModel(new JobItemModel() { Name = "Test" }),
            new JobItemViewModel(new JobItemModel() { Name = "Test" }),
            new JobItemViewModel(new JobItemModel() { Name = "Test" }),
            new JobItemViewModel(new JobItemModel() { Name = "Test" }),
            new JobItemViewModel(new JobItemModel() { Name = "Test" }),
            new JobItemViewModel(new JobItemModel() { Name = "Test" }),
            new JobItemViewModel(new JobItemModel() { Name = "Test" }),
            new JobItemViewModel(new JobItemModel() { Name = "Test" }),
            new JobItemViewModel(new JobItemModel() { Name = "Test" }),
            new JobItemViewModel(new JobItemModel() { Name = "Test" }),
            new JobItemViewModel(new JobItemModel() { Name = "Test" }),
            new JobItemViewModel(new JobItemModel() { Name = "Test" }),
            new JobItemViewModel(new JobItemModel() { Name = "Test" }),
            new JobItemViewModel(new JobItemModel() { Name = "Test" }),
            new JobItemViewModel(new JobItemModel() { Name = "Test" }),
            new JobItemViewModel(new JobItemModel() { Name = "Test" }),
            new JobItemViewModel(new JobItemModel() { Name = "Test" }),
            new JobItemViewModel(new JobItemModel() { Name = "Test" }),
            new JobItemViewModel(new JobItemModel() { Name = "Test" }),
            new JobItemViewModel(new JobItemModel() { Name = "Test" }),
            new JobItemViewModel(new JobItemModel() { Name = "Test" }),
            new JobItemViewModel(new JobItemModel() { Name = "Test" }),
            new JobItemViewModel(new JobItemModel() { Name = "Test" }),
            new JobItemViewModel(new JobItemModel() { Name = "Test" }),
            new JobItemViewModel(new JobItemModel() { Name = "Test" }),
            new JobItemViewModel(new JobItemModel() { Name = "Test" }),
            new JobItemViewModel(new JobItemModel() { Name = "Test" }),
            new JobItemViewModel(new JobItemModel() { Name = "Test" }),
        ];
        jobsView = CollectionViewSource.GetDefaultView(jobs);
        jobsView.Filter = FilterJobs;
    }

    private bool FilterJobs(object obj) {
        if (obj is JobItemViewModel job) {
            return string.IsNullOrEmpty(SearchText) || job.Name.IndexOf(SearchText, StringComparison.OrdinalIgnoreCase) >= 0;
        }
        return false;
    }

    private void LoadJobs() {
        JobItemModel[] jobs = jobService.GetAllJobs();
        foreach (JobItemModel item in jobs) {
            this.jobs.Add(new JobItemViewModel(item));
        }
    }

    public void AddJob(object? o) {
        AddJobViewModel addJobViewModel = new AddJobViewModel();
        AddJobView addJobView = new AddJobView();

        bool result = dialogService.ShowCompactDialog(addJobView, addJobViewModel, "Add Job");
        if(result == true) {
            JobItemViewModel jobItemViewModel = new JobItemViewModel(new JobItemModel() { Name = addJobViewModel.Name });
            jobs.Add(jobItemViewModel);
            SelectedJob = jobItemViewModel;
        }
    }

    public void DeleteJob(JobItemViewModel jobItemViewModel) {
        jobs.Remove(jobItemViewModel);
    }
}
