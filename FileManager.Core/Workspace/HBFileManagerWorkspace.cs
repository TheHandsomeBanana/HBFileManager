using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using HBLibrary.Common.Json;
using Unity;
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
using HBLibrary.Core.ChangeTracker;
using HBLibrary.Interface.Core.ChangeTracker;
using FileManager.Core.Jobs;
using FileManager.Core.Converters;
using System.IO;
using System.Security.Cryptography.Pkcs;
using HBLibrary.Logging.FlowDocumentTarget;

namespace FileManager.Core.Workspace;
public sealed class HBFileManagerWorkspace : ApplicationWorkspace {
    public const string WorkspaceExtension = ".fmws";
    private string? containerPath;

    [JsonIgnore]
    public IApplicationStorage? Storage { get; private set; }
    [JsonIgnore]
    public IChangeTracker ChangeTracker { get; set; }
    [JsonIgnore]
    public JobManager? JobManager { get; set; }
    [JsonIgnore]
    public JobExecutionManager? JobExecutionManager { get; set; }
    [JsonIgnore]
    public JobHistoryManager? JobHistoryManager { get; set; }


    public HBFileManagerWorkspace() : base() {
        ChangeTracker = new ChangeTracker();
    }

    public override void OnCreated() {
        base.OnCreated();

        string basePath = Path.Combine(Path.GetDirectoryName(FullPath!)!, Path.GetFileNameWithoutExtension(FullPath!));
        string jobsDirectory = Path.Combine(basePath, "jobs");
        Directory.CreateDirectory(jobsDirectory);
    }

    public override async Task OpenAsync(IAccount openedBy) {
        await base.OpenAsync(openedBy);

        if (ChangeTracker is null) {
            throw new InvalidOperationException("ChangeTracker is not set");
        }

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

        jobContainerBuilder.EnableChangeTracker(ChangeTracker);

        jobContainerBuilder.SetContainerPath("jobs")
            .ConfigureFileServices(fs => fs.UseJsonFileService(jfs =>
                jfs.SetGlobalOptions(new JsonSerializerOptions {
                    Converters = {
                        new TimeOnlyConverter(),
                        new JobStepConverter(pluginManager)
                    },
                    WriteIndented = true
                }))
            );

        IStorageEntryContainer jobContainer = jobContainerBuilder.Build();

        IStorageEntryContainerBuilder jobRunnerContainerBuilder = StorageEntryContainer.CreateBuilder(containerPath!);
        jobRunnerContainerBuilder.SetContainerPath("jobruns")
            .ConfigureFileServices(fs => fs.UseJsonFileService(jfs => {
                jfs.SetGlobalOptions(new JsonSerializerOptions {
                    WriteIndented = true
                });
            }));

        if (UsesEncryption) {
            jobRunnerContainerBuilder.EnableCryptography(new StorageContainerCryptography {
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

        IStorageEntryContainer jobRunContainer = jobRunnerContainerBuilder.Build();

        Storage = ApplicationStorage.CreateBuilder(Path.GetDirectoryName(FullPath)!)
           .AddContainer(typeof(JobManager), _ => jobContainer)
           .AddContainer(typeof(JobExecutionManager), _ => jobRunContainer)
           .Build();


        IUnityContainer mainContainer = UnityBase.Registry.Get(DIContainerGuids.FileManagerContainerGuid);

        JobManager = new JobManager(jobContainer);
        JobExecutionManager = new JobExecutionManager(jobRunContainer, pluginManager);
        JobHistoryManager = new JobHistoryManager(jobRunContainer);

        foreach (IStorageEntryContainer entryContainer in Storage.GetContainers()) {
            entryContainer.ChangeTracker?.HookStateChanged();
        }

        NotifyOpened();
    }

    public override async Task SaveAsync() {

        if (Storage is not null) {
            await Storage.SaveAllAsync();
        }

        await base.SaveAsync();
    }

    public override void Save() {
        Storage?.SaveAll();
        base.Save();
    }

    public override void Close() {
        Save();

        Storage?.Dispose();

        containerPath = null;

        base.Close();
    }

    public override async Task CloseAsync() {
        await SaveAsync();

        Storage?.Dispose();

        containerPath = null;

        await base.CloseAsync();
    }
}
