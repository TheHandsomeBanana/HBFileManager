using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileManager.UI.Models.SettingsPageModels;
public class SettingsWinRARModel {
    public bool UseWinRAR { get; set; }
    public string Location { get; set; } = "";
    public string LicenseKeyLocation { get; set; } = "";
}
