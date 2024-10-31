using FileManager.UI.ViewModels.ExecutionViewModels.RunningJobsViewModels;
using HBLibrary.Wpf.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileManager.UI.ViewModels.ExecutionViewModels;

public class RunningJobsViewModel : ViewModelBase {
    public ObservableCollection<RunningJobViewModel> RunningJobs { get; set; }

    private RunningJobViewModel? selectedJobRun;
    public RunningJobViewModel? SelectedJobRun {
        get { return selectedJobRun; }
        set {
            selectedJobRun = value;
            NotifyPropertyChanged();
        }
    }


    public RunningJobsViewModel() {
        RunningJobs = [];
    }
}
