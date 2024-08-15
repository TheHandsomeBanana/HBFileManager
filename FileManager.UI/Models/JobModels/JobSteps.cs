using FileManager.UI.Models.Job;
using FileManager.UI.Models.JobModels.JobStepModels;
using FileManager.UI.ViewModels.JobViewModels;
using FileManager.UI.ViewModels.JobViewModels.JobStepViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileManager.UI.Models.JobModels;

public static class JobSteps {
    public static JobItemStepViewModel CreateStepVM(string name, StepType stepType) {
        switch(stepType) {
            case StepType.Archive:
                ArchiveStepModel archiveStep = new ArchiveStepModel { Name = name, Id = Guid.NewGuid() };
                return new ArchiveStepViewModel(archiveStep);
            case StepType.Copy:
                CopyStepModel copyStep = new CopyStepModel { Name = name, Id = Guid.NewGuid() };
                return new CopyStepViewModel(copyStep);
            case StepType.Move:
                MoveStepModel moveStep = new MoveStepModel { Name = name, Id = Guid.NewGuid() };
                return new MoveStepViewModel(moveStep);
            case StepType.Replace:
                ReplaceStepModel replaceStep = new ReplaceStepModel { Name = name, Id = Guid.NewGuid() };
                return new ReplaceStepViewModel(replaceStep);

        }

        throw new NotSupportedException($"{stepType}");
    }
}
