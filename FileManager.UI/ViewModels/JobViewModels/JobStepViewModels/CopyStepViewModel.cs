using FileManager.UI.Models.JobModels.JobStepModels;
using HBLibrary.Wpf.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileManager.UI.ViewModels.Jobs.JobStepViewModels;
public class CopyStepViewModel : JobItemStepViewModel {
    public CopyStepViewModel(CopyStepModel model) : base(model) {
    }
}
