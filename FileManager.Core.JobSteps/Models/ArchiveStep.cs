﻿using FileManager.Core.JobSteps.Attributes;
using HBLibrary.Common.Results;

namespace FileManager.Core.JobSteps.Models;
[JobStepType("Archive")]
public class ArchiveStep : IJobStep {
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = "";
    public bool IsAsync { get; set; }

    public void Execute(IServiceProvider serviceProvider) {
        throw new NotImplementedException();
    }

    public Task ExecuteAsync(IServiceProvider serviceProvider) {
        throw new NotImplementedException();
    }

    public System.Windows.Controls.UserControl? GetJobStepView() {
        return null;
    }

    public CollectionResult Validate(IServiceProvider serviceProvider) {
        throw new NotImplementedException();
    }

    public Task<CollectionResult> ValidateAsync(IServiceProvider serviceProvider) {
        throw new NotImplementedException();
    }
}
