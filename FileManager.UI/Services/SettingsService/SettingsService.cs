﻿
using HBLibrary.Core.Extensions;
using HBLibrary.Interface.Core.ChangeTracker;
using HBLibrary.Interface.IO.Storage;
using HBLibrary.Interface.IO.Storage.Container;
using HBLibrary.Interface.IO.Storage.Entries;

namespace FileManager.UI.Services.SettingsService;
public class SettingsService : ISettingsService {
    private readonly IStorageEntryContainer container;
    public SettingsService(IApplicationStorage applicationStorage) {
        this.container = applicationStorage.GetContainer(typeof(SettingsService));
    }

    public TSetting? GetSetting<TSetting>() where TSetting : class, INotifyTrackableChanged {
        return GetSetting(typeof(TSetting)) as TSetting;
    }

    public void SetSetting<TSetting>(TSetting setting) where TSetting : class, INotifyTrackableChanged {
        SetSetting(typeof(TSetting), setting);
    }

    public TSetting GetOrSetNew<TSetting>(Func<TSetting> createSettingFunc) where TSetting : class, INotifyTrackableChanged {
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

    private object? GetSetting(Type type) {
        if (container.TryGet(type.GuidString(), out IStorageEntry? entry)) {
            return entry!.Get(type);
        }

        return null;
    }

    private void SetSetting(Type type, object setting) {
        container.AddOrUpdate(type.GuidString(), setting, StorageEntryContentType.Json);
    }
}
