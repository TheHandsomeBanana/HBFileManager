using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileManager.Core.JobSteps;
public class JobStepMetadata {
    public required string TypeName { get; init; }
    public string? Description { get; init; }
}
