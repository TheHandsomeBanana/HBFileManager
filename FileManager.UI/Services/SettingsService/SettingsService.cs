using HBLibrary.Common.Account;
using HBLibrary.Common.Extensions;
using HBLibrary.Services.IO.Storage;
using HBLibrary.Services.IO.Storage.Container;
using HBLibrary.Services.IO.Storage.Entries;

namespace FileManager.UI.Services.SettingsService;
public class SettingsService : ISettingsService {
    private readonly IStorageEntryContainer container;
    public SettingsService(IApplicationStorage applicationStorage, IAccountService accountService) {
        string containerString = accountService.Account!.AccountId + nameof(SettingsService);

        this.container = applicationStorage.GetContainer(containerString.ToGuid());
    }

    public TSetting? GetSetting<TSetting>() where TSetting : class {
        return GetSetting(typeof(TSetting)) as TSetting;
    }

    public void SetSetting<TSetting>(TSetting setting) where TSetting : class {
        SetSetting(typeof(TSetting), setting);
    }

    public object? GetSetting(Type type) {
        if (container.TryGet(type.GuidString(), out IStorageEntry? entry)) {
            return entry!.Get(type);
        }

        return null;
    }

    public void SetSetting(Type type, object setting) {
        container.AddOrUpdate(type.GuidString(), setting, StorageEntryContentType.Json);
    }

    public TSetting GetOrSetNew<TSetting>(Func<TSetting> createSettingFunc) where TSetting : class {
        if (container.TryGet(typeof(TSetting).GuidString(), out IStorageEntry? entry)) {
            TSetting? setting = entry!.Get(typeof(TSetting)) as TSetting;

            if (setting is null) {
                setting = createSettingFunc();
                SetSetting(setting);
            }

            return setting;
        }

        TSetting newSetting = createSettingFunc();
        SetSetting(newSetting);
        return newSetting;
    }
}
