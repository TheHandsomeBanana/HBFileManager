using HBLibrary.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace FileManager.Core.JobSteps;

public abstract class JobStep : IJobStep {
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = "";
    public bool IsAsync { get; set; }
    public bool IsEnabled { get; set; }

    public abstract void Execute(IServiceProvider serviceProvider);
    public abstract Task ExecuteAsync(IServiceProvider serviceProvider);
    public abstract UserControl? GetJobStepView();
    public abstract ImmutableResultCollection Validate(IServiceProvider serviceProvider);
    public abstract Task<ImmutableResultCollection> ValidateAsync(IServiceProvider serviceProvider);
}
