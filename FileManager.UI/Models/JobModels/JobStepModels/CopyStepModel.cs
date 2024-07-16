using FileManager.UI.Models.Job;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileManager.UI.Models.Jobs.JobSteps;
public class CopyStepModel : JobItemStepModel {
    public override StepType StepType => StepType.Copy;
}
