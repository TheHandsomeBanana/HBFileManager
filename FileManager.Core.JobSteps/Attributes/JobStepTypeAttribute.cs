using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileManager.Core.JobSteps.Attributes;
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class JobStepTypeAttribute : Attribute {
    public string Name { get; set; }

    public JobStepTypeAttribute(string name) {
        Name = name;
    }
}
