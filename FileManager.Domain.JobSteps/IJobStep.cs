﻿using HBLibrary.Common;
using HBLibrary.DataStructures;
using HBLibrary.Wpf.ViewModels;
using System.Windows.Controls;
using Unity;

namespace FileManager.Domain.JobSteps;

/// <summary>
/// Classes that implement this interface are responsible for executing a job step. They MUST have a parameterless constructor.
/// <br></br>
/// They should also serve as model for the corresponding view if used.
/// </summary>
public interface IJobStep {
    public Guid Id { get; }
    public string Name { get; set; }
    public bool IsAsync { get; set; }
    public bool IsEnabled { get; set; }
    public int ExecutionOrder { get; set; }
    public void Execute(IUnityContainer container, CancellationToken stepCancellationToken = default, CancellationToken jobCancellationToken = default);
    public ImmutableResultCollection Validate(IUnityContainer container);

    public Task ExecuteAsync(IUnityContainer container, CancellationToken stepCancellationToken = default, CancellationToken jobCancellationToken = default);
    public Task<ImmutableResultCollection> ValidateAsync(IUnityContainer container);

    public UserControl? GetJobStepView();
    public IJobStepContext? GetJobStepDataContext();
}


