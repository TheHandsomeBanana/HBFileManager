using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using HBLibrary.Common.Json;
using FileManager.Core.JobSteps.Converters;
using Unity;
using FileManager.Core.Job;
using System.Text.Json.Serialization;
using HBLibrary.Workspace;
using HBLibrary.Interface.IO.Storage;
using HBLibrary.Interface.Security.Account;
using HBLibrary.DI;
using HBLibrary.Interface.Plugins;
using HBLibrary.Interface.IO.Storage.Builder;
using HBLibrary.Interface.Security;
using HBLibrary.Interface.IO.Storage.Settings;
using HBLibrary.IO.Storage.Container;
using HBLibrary.DataStructures;
using HBLibrary.Interface.Security.Keys;
using HBLibrary.IO.Storage.Builder;
using HBLibrary.Interface.IO.Storage.Container;
using HBLibrary.IO.Storage;

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

    public override async Task OpenAsync(IAccount openedBy) {
        await base.OpenAsync(openedBy);

        containerPath = Path.Combine(Path.GetDirectoryName(FullPath!)!, Path.GetFileNameWithoutExtension(FullPath!));

        IUnityContainer container = UnityBase.Registry.Get(DIContainerGuids.FileManagerContainerGuid);
        IPluginManager pluginManager = container.Resolve<IPluginManager>();

        IStorageEntryContainerBuilder jobContainerBuilder = StorageEntryContainer.CreateBuilder(containerPath!);
        if (UsesEncryption) {
            jobContainerBuilder.EnableCryptography(new StorageContainerCryptography {
                CryptographyMode = CryptographyMode.AES,
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
        await base.SaveAsync();

        if (Storage is not null) {
            await Storage.SaveAllAsync();
        }
    }

    public override void Save() {
        base.Save();

        if (Storage is not null) {
            Storage.SaveAll();
        }
    }

    public override void Close() {
        Save();

        containerPath = null;

        base.Close();
    }

    public override async Task CloseAsync() {
        await SaveAsync();

        containerPath = null;

        await base.CloseAsync();
    }
}
