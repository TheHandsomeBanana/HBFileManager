using HBLibrary.Interface.Core.ChangeTracker;

namespace FileManager.UI.Services.SettingsService;
public interface ISettingsService {
    public TSetting? GetSetting<TSetting>() where TSetting : class, INotifyTrackableChanged;
    public void SetSetting<TSetting>(TSetting setting) where TSetting : class, INotifyTrackableChanged;
    public TSetting GetOrSetNew<TSetting>(Func<TSetting> createSettingFunc) where TSetting : class, INotifyTrackableChanged;
}
