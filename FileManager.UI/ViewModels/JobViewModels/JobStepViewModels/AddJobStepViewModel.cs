using FileManager.Core.JobSteps;
using FileManager.UI.Models.JobModels;
using HBLibrary.Wpf.Commands;
using HBLibrary.Wpf.Models;
using HBLibrary.Wpf.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace FileManager.UI.ViewModels.JobViewModels.JobStepViewModels;
public class AddJobStepViewModel : ViewModelBase {
    public RelayCommand<Window> AddJobCommand { get; set; }
    public RelayCommand<Window> CancelCommand { get; set; }

    private string name = "";
    public string Name {
        get { return name; }
        set {
            name = value;
            NotifyPropertyChanged();

            AddJobCommand.NotifyCanExecuteChanged();
        }
    }


    private JobStepInfo? selectedStepType;
    public JobStepInfo? SelectedStepType {
        get { return selectedStepType; }
        set {
            selectedStepType = value;
            NotifyPropertyChanged();

            AddJobCommand.NotifyCanExecuteChanged();
        }
    }

    private JobStepInfo[] availableJobSteps = [];
    public JobStepInfo[] AvailableStepTypes {
        get => availableJobSteps;
        set {
            availableJobSteps = value;
            NotifyPropertyChanged();
        }
    }

    public AddJobStepViewModel(IPluginJobStepManager jobStepManager) {
        AddJobCommand = new RelayCommand<Window>(AddAndFinish, _ => !string.IsNullOrWhiteSpace(Name) && selectedStepType is not null);
        CancelCommand = new RelayCommand<Window>(CancelAndFinish, true);

        AvailableStepTypes = jobStepManager.GetJobStepTypes().Select(e => {
            string typeName = PluginJobStepManager.GetJobStepTypeName(e);
            return new JobStepInfo() {
                TypeName = typeName,
                StepType = e
            };

        }).ToArray();
    }

    private void AddAndFinish(Window? obj) {
        if (obj is null) {
            return;
        }

        obj.DialogResult = true;
        obj.Close();
    }

    private void CancelAndFinish(Window? obj) {
        if (obj is null) {
            return;
        }

        obj.DialogResult = false;
        obj.Close();
    }
}

public class JobStepInfo {
    public required string TypeName { get; set; }
    public required Type StepType { get; set; }
}
