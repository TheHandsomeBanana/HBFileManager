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
using HBLibrary.Services.IO.Storage.Settings;
using HBLibrary.Common.Security.Keys;
using HBLibrary.Common;
using HBLibrary.Services.IO.Storage.Builder;

namespace FileManager.Core.Workspace;
public sealed class HBFileManagerWorkspace : ApplicationWorkspace {
    public const string WorkspaceExtension = ".fmws";
    private string? containerPath;

    [JsonIgnore]
    public IApplicationStorage? Storage { get; private set; }
    [JsonIgnore]
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

        IUnityContainer container = UnityBase.Registry.Get(DIContainerGuids.FileManagerContainerGuid);
        IPluginManager pluginManager = container.Resolve<IPluginManager>();

        IStorageEntryContainerBuilder jobContainerBuilder = StorageEntryContainer.CreateBuilder(containerPath!);
        if (UsesEncryption) {
            jobContainerBuilder.EnableCryptography(new StorageContainerCryptography {
                CryptographyMode = HBLibrary.Common.Security.CryptographyMode.AES,
                GetEntryKeyAsync = async () => {
                    Result<AesKey> keyResult = await GetKeyAsync();
                    return keyResult.Map<IKey>(e => e);
                },
                GetEntryKey = () => {
                    Result<AesKey> keyResult = GetKey();
                    return keyResult.Map<IKey>(e => e);
                }
            });
        }

        jobContainerBuilder.SetContainerPath("jobs")
            .ConfigureFileServices(fs => fs.UseJsonFileService(jfs =>
                jfs.SetGlobalOptions(new JsonSerializerOptions {
                    Converters = {
                        new TimeOnlyConverter(),
                        new JobStepConverter(pluginManager)
                    },
                    WriteIndented = true
                }))
            ).Build();

        IStorageEntryContainer jobContainer = jobContainerBuilder.Build();
            
        Storage = ApplicationStorage.CreateBuilder(Path.GetDirectoryName(FullPath)!)
           .AddContainer(typeof(JobManager), _ => jobContainer)
           .Build();

        JobManager = new JobManager(jobContainer);

        NotifyOpened();
    }

    public override async Task SaveAsync() {
        if (Storage is not null) {
            await Storage.SaveAllAsync();
        }
    }

    public override void Save() {
        if (Storage is not null) {
            Storage.SaveAll();
        }
    }

    public override void Close() {
        if (Storage is not null) {
            Storage.SaveAll();
        }

        containerPath = null;

        base.Close();
    }

    public override async Task CloseAsync() {
        if (Storage is not null) {
            await Storage.SaveAllAsync();
        }

        containerPath = null;

        await base.CloseAsync();
    }
}
