using HBLibrary.Wpf.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileManager.Core.JobSteps.ViewModels;
public class FallbackStepViewModel : JobStepViewModel<IJobStep> {
    public string FallbackText { get; set; }


    public FallbackStepViewModel(IJobStep jobStep) : base(jobStep) {
        FallbackText = $"No UI available for the step type '{StepType}'";
    }
}
