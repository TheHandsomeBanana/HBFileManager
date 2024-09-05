using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileManager.Core.JobSteps.Attributes;
[AttributeUsage(AttributeTargets.Class)]
public class JobStepNameAttribute : Attribute {
    public string Name { get; set; }

    public JobStepNameAttribute(string name) {
        Name = name;
    }
}
