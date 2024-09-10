using FileManager.Core.JobSteps.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileManager.Core.JobSteps.ViewModels;
public class ArchiveStepViewModel : JobStepViewModel<ArchiveStep> {
    public ArchiveStepViewModel(ArchiveStep model) : base(model) {
    }
}
