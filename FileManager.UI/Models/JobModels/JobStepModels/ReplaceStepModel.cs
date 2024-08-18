using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileManager.UI.Models.JobModels.JobStepModels;
public class ReplaceStepModel : JobItemStepModel {
    public override StepType StepType => StepType.Replace;
}
