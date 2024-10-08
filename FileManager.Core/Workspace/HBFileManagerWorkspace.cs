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
    public IApplicationStorage? Storage { get; private set; }
    public JobManager? JobManager { get; set; }
    
    [JsonIgnore]
    public override string WorkspaceExtension => ".fmws";
    
    public HBFileManagerWorkspace() : base() {
    }

    public override async Task OpenAsync(Account openedBy) {
        await base.OpenAsync(openedBy);

        IUnityContainer container = UnityBase.GetChildContainer(nameof(FileManager))!;
        IPluginManager pluginManager = container.Resolve<IPluginManager>();

        IStorageEntryContainer jobContainer = StorageEntryContainer.CreateBuilder(FullPath!)
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
    }

    public override async Task CloseAsync() {
        if (Storage is not null) {
            await Storage.SaveAllAsync();
        }

        await base.CloseAsync();
    }
}
