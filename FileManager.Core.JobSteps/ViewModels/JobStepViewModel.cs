﻿using HBLibrary.Wpf.ViewModels;
using System.Windows.Controls;

namespace FileManager.Core.JobSteps.ViewModels;
public class JobStepViewModel<TModel> : ViewModelBase<TModel> where TModel : IJobStep {

    public string Name {
        get => Model.Name;
        set {
            Model.Name = value;
            NotifyPropertyChanged();
        }
    }

    public UserControl? StepView => Model.GetJobStepView();
    public string StepType => PluginJobStepManager.GetJobStepTypeName(Model.GetType());

    public JobStepViewModel(TModel model) : base(model) {

    }
}
