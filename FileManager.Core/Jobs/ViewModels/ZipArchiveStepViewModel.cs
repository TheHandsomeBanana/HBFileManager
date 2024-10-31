using FileManager.Core.Jobs.Models;
using FileManager.Core.JobSteps;

namespace FileManager.Core.Jobs.ViewModels;
public class ZipArchiveStepViewModel : JobStepViewModel<ZipArchiveStep> {
    public ZipArchiveStepViewModel(ZipArchiveStep model) : base(model) {
    }
}
