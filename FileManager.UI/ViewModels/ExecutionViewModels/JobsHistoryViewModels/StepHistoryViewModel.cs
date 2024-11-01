﻿using FileManager.Domain;
using HBLibrary.Logging.FlowDocumentTarget;
using HBLibrary.Wpf.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileManager.UI.ViewModels.ExecutionViewModels.JobsHistoryViewModels;
public class StepHistoryViewModel : ViewModelBase<StepRun> {
    public FlowDocumentTarget DocumentTarget => Model.Logs;

    public StepHistoryViewModel(StepRun model) : base(model) {
    }
}
