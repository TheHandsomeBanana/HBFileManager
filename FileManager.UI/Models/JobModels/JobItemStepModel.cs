using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileManager.UI.Models.Job;
public abstract class JobItemStepModel {
    public abstract StepType StepType { get; }
    public Guid Id { get; set; }
    public string Name { get; set; } = "";
}

public enum StepType {
    Archive,
    Copy,
    Move,
    Replace,
}
