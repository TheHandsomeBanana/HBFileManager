﻿using FileManager.Core.JobSteps.Attributes;
using HBLibrary.Common.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        throw new NotImplementedException();
    }

    public ValidationResult Validate(IServiceProvider serviceProvider) {
        throw new NotImplementedException();
    }

    public Task<ValidationResult> ValidateAsync(IServiceProvider serviceProvider) {
        throw new NotImplementedException();
    }
}
