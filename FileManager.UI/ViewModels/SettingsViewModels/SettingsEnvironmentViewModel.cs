using FileManager.UI.Models;
using FileManager.UI.Models.SettingsModels;
using HBLibrary.Wpf.ViewModels;

namespace FileManager.UI.ViewModels.SettingsViewModels {
    public class SettingsEnvironmentViewModel : ViewModelBase<SettingsEnvironmentModel> {
        public bool EncryptJobs {
            get => Model.EncryptJobs;
            set {
                Model.EncryptJobs = value;
                NotifyPropertyChanged();
            }
        }
        
        public SettingsEnvironmentViewModel() : base(new SettingsEnvironmentModel()) {
        }

        public SettingsEnvironmentViewModel(SettingsEnvironmentModel model) : base(model) { }
    }
}
