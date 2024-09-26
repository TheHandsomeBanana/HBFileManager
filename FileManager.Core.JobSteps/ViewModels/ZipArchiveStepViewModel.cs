using FileManager.Core.JobSteps.Models;

namespace FileManager.Core.JobSteps.ViewModels;
public class ZipArchiveStepViewModel : JobStepViewModel<ZipArchiveStep> {
    public ZipArchiveStepViewModel(ZipArchiveStep model) : base(model) {
    }
}
