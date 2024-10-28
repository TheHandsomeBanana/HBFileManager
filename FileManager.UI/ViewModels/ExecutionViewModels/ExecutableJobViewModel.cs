using FileManager.Core.Job;
using HBLibrary.Wpf.Commands;
using HBLibrary.Wpf.ViewModels;
using HBLibrary.Wpf.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace FileManager.UI.ViewModels.ExecutionViewModels;
public class ExecutableJobViewModel : ViewModelBase<Job> {

    public string Name {
        get => Model.Name;
        set {
            Model.Name = value;
            NotifyPropertyChanged();
        }
    }

    public RelayCommand<ExecutableJobViewModel> RunJobCommand { get; set; }
    public AsyncRelayCommand<ExecutableJobViewModel> ScheduleJobCommand { get; set; }

    public ExecutableJobViewModel(Job model) : base(model) {
        RunJobCommand = new RelayCommand<ExecutableJobViewModel>(RunJob, e => e!.Model.OnDemand);
        ScheduleJobCommand = new AsyncRelayCommand<ExecutableJobViewModel>(ScheduleJob, e => e!.Model.Scheduled, OnSchedulingException);

    }

    private void OnSchedulingException(Exception exception) {
        HBDarkMessageBox.Show("Scheduling error", exception.Message, MessageBoxButton.OK, MessageBoxImage.Error);
    }

    private async Task ScheduleJob(ExecutableJobViewModel job) {
        throw new NotImplementedException();
    }

    private void RunJob(ExecutableJobViewModel job) {
        throw new NotImplementedException();
    }
}
