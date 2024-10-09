using HBLibrary.Common.Account;
using HBLibrary.Common.Extensions;
using HBLibrary.Common.Workspace;
using HBLibrary.Services.IO.Json;
using HBLibrary.Services.IO.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using HBLibrary.Common.Json;
using FileManager.Core.JobSteps.Converters;
using HBLibrary.Common.Plugins;
using Unity;
using HBLibrary.Common.DI.Unity;
using HBLibrary.Services.IO.Storage.Container;
using FileManager.Core.Job;
using System.Text.Json.Serialization;

namespace FileManager.Core.Workspace;
public sealed class HBFileManagerWorkspace : ApplicationWorkspace {
    public const string WorkspaceExtension = ".fmws";
    private string? containerPath;

    public IApplicationStorage? Storage { get; private set; }
    public JobManager? JobManager { get; set; }
    
    
    public HBFileManagerWorkspace() : base() {
    }

    public override void OnCreated() {
        base.OnCreated();

        string basePath = Path.Combine(Path.GetDirectoryName(FullPath!)!, Path.GetFileNameWithoutExtension(FullPath!));
        string jobsDirectory = Path.Combine(basePath, "jobs");
        Directory.CreateDirectory(jobsDirectory);
    }

    public override async Task OpenAsync(Account openedBy) {
        await base.OpenAsync(openedBy);

        containerPath = Path.Combine(Path.GetDirectoryName(FullPath!)!, Path.GetFileNameWithoutExtension(FullPath!));

        IUnityContainer container = UnityBase.GetChildContainer(nameof(FileManager))!;
        IPluginManager pluginManager = container.Resolve<IPluginManager>();


        IStorageEntryContainer jobContainer = StorageEntryContainer.CreateBuilder(containerPath!)
            .SetContainerPath("jobs")
            .ConfigureFileServices(fs => fs.UseJsonFileService(jfs =>
                jfs.SetGlobalOptions(new JsonSerializerOptions {
                    Converters = {
                        new TimeOnlyConverter(),
                        new JobStepConverter(pluginManager)
                    },
                    WriteIndented = true
                }))
            ).Build();

        Storage = ApplicationStorage.CreateBuilder(Path.GetDirectoryName(FullPath)!)
           .AddContainer(typeof(JobManager), _ => jobContainer)
           .Build();

        JobManager = new JobManager(jobContainer);

        NotifyOpened();
    }

    public override async Task CloseAsync() {
        if (Storage is not null) {
            await Storage.SaveAllAsync();
        }

        containerPath = null;

        await base.CloseAsync();
    }
}
