using FileManager.Domain;
using HBLibrary.Logging.FlowDocumentTarget;
using HBLibrary.Wpf.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;

namespace FileManager.UI.ViewModels.ExecutionViewModels.RunningJobsViewModels;
public class RunningStepViewModel : ViewModelBase<StepRun> {
    public FlowDocumentTarget DocumentTarget => Model.Logs;

    public RunningStepViewModel(StepRun model) : base(model) {
    }
}
