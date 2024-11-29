using FileManager.Core.Jobs.Models;
using FileManager.Core.Jobs.Models.Copy;
using FileManager.Core.JobSteps;
using FileManager.Domain.JobSteps;
using HBLibrary.Common.Plugins;
using HBLibrary.Interface.Plugins;
using HBLibrary.Plugins;
using HBLibrary.Wpf.Commands;
using HBLibrary.Wpf.ViewModels;
using System.Collections.Generic;
using System.Windows;

namespace FileManager.UI.ViewModels.JobViewModels.JobStepViewModels;
public class AddJobStepViewModel : ViewModelBase {
    private readonly static JobStepInfo[] fixedTypes = [
       new JobStepInfo {
            StepType = typeof(CopyStep),
            Metadata = PluginManager.GetPluginMetadata(typeof(CopyStep))
        },
        new JobStepInfo {
            StepType = typeof(ZipArchiveStep),
            Metadata = PluginManager.GetPluginMetadata(typeof(ZipArchiveStep))
        }
    ];

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

    public AddJobStepViewModel(IPluginManager pluginManager) {
        AddJobCommand = new RelayCommand<Window>(AddAndFinish, _ => !string.IsNullOrWhiteSpace(Name) && selectedStepType is not null);
        CancelCommand = new RelayCommand<Window>(CancelAndFinish);


        AvailableStepTypes =
        [
            .. fixedTypes,
            .. pluginManager.TypeProvider
                .QueryByAttribute<JobStep>(pluginManager.GetLoadedAssemblies())
                .Select(e => {
                    return new JobStepInfo {
                        StepType = e.ConcreteType,
                        Metadata = e.Metadata,
                    };
                }),
        ];
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
    public required PluginMetadata Metadata { get; set; }
    public required Type StepType { get; set; }
}
