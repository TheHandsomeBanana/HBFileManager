using FileManager.Core.Jobs;
using FileManager.Core.Workspace;
using FileManager.Domain;
using HBLibrary.Core.Extensions;
using HBLibrary.DI;
using HBLibrary.Interface.Workspace;
using HBLibrary.Wpf.Commands;
using HBLibrary.Wpf.ViewModels;
using HBLibrary.Wpf.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Unity;

namespace FileManager.UI.ViewModels.ExecutionViewModels;
public class ExecutableJobViewModel : ViewModelBase<Job> {
    private readonly IApplicationWorkspaceManager<HBFileManagerWorkspace> workspaceManager;
    private readonly IUnityContainer mainContainer;
    public string Name {
        get => Model.Name;
        set {
            Model.Name = value;
            NotifyPropertyChanged();
        }
    }

    public AsyncRelayCommand<ExecutableJobViewModel> RunJobCommand { get; set; }
    public AsyncRelayCommand<ExecutableJobViewModel> ScheduleJobCommand { get; set; }

    public ExecutableJobViewModel(Job model) : base(model) {
        mainContainer = UnityBase.Registry.Get(ApplicationHandler.FileManagerContainerGuid);
        workspaceManager = mainContainer.Resolve<IApplicationWorkspaceManager<HBFileManagerWorkspace>>();

        RunJobCommand = new AsyncRelayCommand<ExecutableJobViewModel>(RunJobAsync, e => e!.Model.OnDemand, e => OnException("Run error", e));
        ScheduleJobCommand = new AsyncRelayCommand<ExecutableJobViewModel>(ScheduleJob, e => e!.Model.Scheduled, e => OnException("Scheduling error", e));

    }

    private void OnException(string title, Exception exception) {
        HBDarkMessageBox.Show(title, exception.Message, MessageBoxButton.OK, MessageBoxImage.Error);
    }

    private Task ScheduleJob(ExecutableJobViewModel job) {
        throw new NotImplementedException();
    }

    private async Task RunJobAsync(ExecutableJobViewModel job) {
        JobExecutionManager jobRunner = workspaceManager.CurrentWorkspace!.JobRunner!;
        await Task.Run(() => jobRunner.RunAsync(job.Model, mainContainer));
    }
}
