﻿using FileManager.Core.Workspace;
using FileManager.UI.ViewModels.ExecutionViewModels;
using FileManager.UI.ViewModels.JobViewModels;
using FileManager.UI.Views;
using HBLibrary.DI;
using HBLibrary.Interface.Workspace;
using HBLibrary.Wpf.Services.NavigationService;
using HBLibrary.Wpf.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using Unity;

namespace FileManager.UI.ViewModels;
public class ExecutionViewModel : ViewModelBase, IDisposable {
    private readonly IApplicationWorkspaceManager<HBFileManagerWorkspace> workspaceManager;
    private readonly INavigationService navigationService;
    private readonly INavigationStore navigationStore;

    public ObservableCollection<ExecutableJobViewModel> ExecutableJobs { get; set; }
    private readonly ICollectionView executableJobsView;
    public ICollectionView ExecutableJobsView => executableJobsView;

    private string? searchText;
    public string? SearchText {
        get { return searchText; }
        set {
            searchText = value;
            NotifyPropertyChanged();

            executableJobsView.Refresh();
        }
    }

    private bool runningJobsChecked = true;
    public bool RunningJobsChecked {
        get { return runningJobsChecked; }
        set { 
            runningJobsChecked = value;
            NotifyPropertyChanged();

            if(value) {
                ScheduledJobsChecked = false;
                JobsHistoryChecked = false;
            }
        }
    }
    
    private bool scheduledJobsChecked;
    public bool ScheduledJobsChecked {
        get { return scheduledJobsChecked; }
        set {
            scheduledJobsChecked = value;
            NotifyPropertyChanged();

            if(value) {
                RunningJobsChecked = false;
                JobsHistoryChecked = false;
            }
        }
    }
    
    private bool jobsHistoryChecked;
    public bool JobsHistoryChecked {
        get { return jobsHistoryChecked; }
        set {
            jobsHistoryChecked = value;
            NotifyPropertyChanged();

            if (value) {
                RunningJobsChecked = false;
                ScheduledJobsChecked = false;
            }
        }
    }


    public ViewModelBase? CurrentViewModel => navigationStore[NavigateCommandParameter].ViewModel;

    public string NavigateCommandParameter { get; } = nameof(ExecutionViewModel);
    public NavigateCommand<RunningJobsViewModel> NavigateRunningJobsCommand { get; set; }
    public NavigateCommand<ScheduledJobsViewModel> NavigateScheduledJobsCommand { get; set; }
    public NavigateCommand<JobsHistoryViewModel> NavigateJobsHistoryCommand { get; set; }


    public ExecutionViewModel() {
        IUnityContainer container = UnityBase.Registry.Get(ApplicationHandler.FileManagerContainerGuid);
        workspaceManager = container.Resolve<IApplicationWorkspaceManager<HBFileManagerWorkspace>>();
        navigationService = container.Resolve<INavigationService>();
        navigationStore = container.Resolve<INavigationStore>();

        ExecutableJobs = new ObservableCollection<ExecutableJobViewModel>(workspaceManager.CurrentWorkspace!.JobManager!.GetExecutableJobs()
            .Select(e => new ExecutableJobViewModel(e)));

        executableJobsView = CollectionViewSource.GetDefaultView(ExecutableJobs);
        executableJobsView.Filter = FilterJobs;

        NavigateRunningJobsCommand = new NavigateCommand<RunningJobsViewModel>(navigationService, () => new RunningJobsViewModel());
        NavigateScheduledJobsCommand = new NavigateCommand<ScheduledJobsViewModel>(navigationService, () => new ScheduledJobsViewModel());
        NavigateJobsHistoryCommand = new NavigateCommand<JobsHistoryViewModel>(navigationService, () => new JobsHistoryViewModel());

        navigationStore[NavigateCommandParameter].CurrentViewModelChanged += ExecutionViewModel_CurrentViewModelChanged;

        workspaceManager.CurrentWorkspace!.JobExecutionManager!.OnJobStarting += ExecutionViewModel_OnJobStarting;

        NavigateRunningJobsCommand.Execute(NavigateCommandParameter);
    }

    private void ExecutionViewModel_OnJobStarting(Domain.JobRun _) {
        NavigateRunningJobsCommand.Execute(NavigateCommandParameter);
    }

    private void ExecutionViewModel_CurrentViewModelChanged() {
        NotifyPropertyChanged(nameof(CurrentViewModel));
    }

    private bool FilterJobs(object obj) {
        if (obj is ExecutableJobViewModel job) {
            return string.IsNullOrEmpty(SearchText) || job.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase);
        }
        return false;
    }

    public void Dispose() {
        navigationStore[NavigateCommandParameter].CurrentViewModelChanged -= ExecutionViewModel_CurrentViewModelChanged;
        workspaceManager.CurrentWorkspace!.JobExecutionManager!.OnJobStarting -= ExecutionViewModel_OnJobStarting;
        
        navigationStore.DisposeByParentTypename(NavigateCommandParameter);
    }
}
