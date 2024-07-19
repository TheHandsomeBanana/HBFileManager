using FileManager.UI.Models.Job;
using FileManager.UI.Models.JobModels.JobStepModels;
using FileManager.UI.ViewModels.Jobs;
using FileManager.UI.ViewModels.Jobs.JobStepViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileManager.UI.Models.JobModels;

public static class JobSteps {

    public static JobItemStepModel CreateStep(string name, StepType stepType) {
        switch(stepType) {
            case StepType.Archive:
                return new ArchiveStepModel { Name = name, Id = Guid.NewGuid() };
            case StepType.Copy:
                return new CopyStepModel { Name = name, Id = Guid.NewGuid() };
        }

        throw new NotSupportedException($"{stepType}");
    }

    public static JobItemStepViewModel CreateStepVM(string name, StepType stepType) {
        switch(stepType) {
            case StepType.Archive:
                ArchiveStepModel archiveStep = new ArchiveStepModel { Name = name, Id = Guid.NewGuid() };
                return new ArchiveStepViewModel(archiveStep);
            case StepType.Copy:
                CopyStepModel copyStep = new CopyStepModel { Name = name, Id = Guid.NewGuid() };
                return new CopyStepViewModel(copyStep);
        }

        throw new NotSupportedException($"{stepType}");

    }
}
