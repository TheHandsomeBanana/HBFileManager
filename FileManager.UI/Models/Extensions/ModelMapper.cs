using FileManager.Core.Models;
using FileManager.UI.Models.SettingsPageModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileManager.UI.Models.Extensions;
public static class ModelMapper {
    public static SettingsWinRARModel Map(this SettingsWinRARCoreModel model) {
        return new SettingsWinRARModel {
            UseWinRAR = model.UseWinRAR,
            Location = model.Location,
            LicenseKeyLocation = model.LicenseKeyLocation,
        };
    }

    public static SettingsWinRARCoreModel Map(this SettingsWinRARModel model) {
        return new SettingsWinRARCoreModel {
            UseWinRAR = model.UseWinRAR,
            Location = model.Location,
            LicenseKeyLocation = model.LicenseKeyLocation,
        };
    }
}
