﻿using HBLibrary.Wpf.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileManager.UI.ViewModels.SettingsPageViewModels {
    public class SettingsEnvironmentPageViewModel : ViewModelBase {
        public string TestEnvironmentSetting { get; set; } = "Testvalue";

        public SettingsEnvironmentPageViewModel() {

        }
    }
}
