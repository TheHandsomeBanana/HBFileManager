using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileManager.Core.JobSteps.Attributes;
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class JobStepDescriptionAttribute : Attribute {
    public string Description { get; set; }

    public JobStepDescriptionAttribute(string description) {
        Description = description;
    }
}
