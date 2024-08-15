using FileManager.UI.Models;
using FileManager.UI.Models.SettingsModels;
using HBLibrary.Wpf.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileManager.UI.ViewModels.SettingsViewModels {
    public class SettingsEnvironmentViewModel : ViewModelBase<SettingsEnvironmentModel> {


        public SettingsScope SelectedSettingsScope {
            get => Model.SettingsScope;
            set {
                Model.SettingsScope = value;
                NotifyPropertyChanged();
            }
        }
        
        public JobScope SelectedJobScope {
            get => Model.JobScope;
            set {
                Model.JobScope = value;
                NotifyPropertyChanged();
            }
        }

        public SettingsEnvironmentViewModel() {
            Model = new SettingsEnvironmentModel();
        }

        public SettingsEnvironmentViewModel(SettingsEnvironmentModel model) : base(model) { }
    }
}
