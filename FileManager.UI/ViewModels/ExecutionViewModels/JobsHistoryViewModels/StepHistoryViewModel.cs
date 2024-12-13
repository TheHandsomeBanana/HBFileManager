using FileManager.Domain;
using FileManager.Domain.JobSteps;
using FileManager.UI.Models.SettingsModels;
using FileManager.UI.Services.SettingsService;
using HBLibrary.DI;
using HBLibrary.Logging.FlowDocumentTarget;
using HBLibrary.Wpf.Logging;
using HBLibrary.Wpf.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity;

namespace FileManager.UI.ViewModels.ExecutionViewModels.JobsHistoryViewModels;
public class StepHistoryViewModel : ViewModelBase<StepRun> {

    public ListBoxLogTarget LogsTarget => Model.Logs;
    public TimeSpan Elapsed => Model.Duration.GetValueOrDefault();
    public string Name => Model.Name;
    public string StepType => Model.StepType;
    public RunState State => Model.State;
    public bool IsSuccess => Model.State == RunState.Success;
    public bool IsError => Model.State == RunState.Faulted;
    public bool IsWarning => Model.State == RunState.CompletedWithWarnings;
    public bool IsCanceled => Model.State == RunState.Canceled;

    public bool ShowTimestamp { get; }
    public bool ShowExplicitLevel { get; }
    public bool ShowCategory { get; }

    public StepHistoryViewModel(StepRun model) : base(model) {
        IUnityContainer container = UnityBase.Registry.Get(ApplicationHandler.FileManagerContainerGuid);

        ISettingsService settingsService = container.Resolve<ISettingsService>();
        SettingsEnvironmentModel? environmentSettings = settingsService.GetSetting<SettingsEnvironmentModel>();
        if (environmentSettings is not null) {
            ShowExplicitLevel = environmentSettings.ShowLogLevelInHistoryLogs;
            ShowTimestamp = environmentSettings.ShowTimestampInHistoryLogs;
            ShowCategory = environmentSettings.ShowCategoryInHistoryLogs;
        }

    }
}
