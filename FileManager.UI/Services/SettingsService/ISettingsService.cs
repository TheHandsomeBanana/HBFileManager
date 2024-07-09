using HBLibrary.Services.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace FileManager.UI.Services.SettingsService;
public interface ISettingsService {
    public TSetting? GetSetting<TSetting>() where TSetting : class;
    public object? GetSetting(Type type);
    public void SetSetting<TSetting>(TSetting setting) where TSetting : class;
    public void SetSetting(Type type, object setting);

    public TSetting GetOrSetNew<TSetting>(Func<TSetting> createSettingFunc) where TSetting : class;

    public void SaveSettings();
}
