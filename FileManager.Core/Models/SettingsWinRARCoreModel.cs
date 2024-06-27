using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileManager.Core.Models;
public class SettingsWinRARCoreModel {
    public bool UseWinRAR { get; set; }
    public string Location { get; set; } = "";
    public string LicenseKeyLocation { get; set; } = "";
}
