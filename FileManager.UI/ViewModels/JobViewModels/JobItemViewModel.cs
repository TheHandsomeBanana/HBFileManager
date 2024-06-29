using FileManager.UI.Models.Job;
using HBLibrary.Wpf.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileManager.UI.ViewModels.Jobs;
public class JobItemViewModel : ViewModelBase<JobItemModel> {
    public string Name {
        get => Model.Name;
        set {
            Model.Name = value;
            NotifyPropertyChanged();
        }
    }
    public DateTime? ScheduledAt {
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

    private ObservableCollection<JobItemStepViewModel> steps;
    public ObservableCollection<JobItemStepViewModel> Steps => steps;


    public JobItemViewModel() {
        Model = new JobItemModel();
        steps = GetJobSteps();
    }

    public JobItemViewModel(JobItemModel model) : base(model) {
        steps = GetJobSteps();
    }

    private ObservableCollection<JobItemStepViewModel> GetJobSteps() {
        ObservableCollection<JobItemStepViewModel> steps = [];
        foreach (JobItemStepModel step in Model.Steps) {
            steps.Add(new JobItemStepViewModel(step));
        }
        return steps;
    }
}
