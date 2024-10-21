using FileManager.UI.Models;
using FileManager.UI.Models.SettingsModels;
using HBLibrary.Wpf.ViewModels;

namespace FileManager.UI.ViewModels.SettingsViewModels {
    public class SettingsEnvironmentViewModel : ViewModelBase<SettingsEnvironmentModel> {
        public bool ValidateOnNavigation {
            get => Model.ValidateOnNavigation;
            set {
                Model.ValidateOnNavigation = value;
                NotifyPropertyChanged();
            }
        }
        
        public SettingsEnvironmentViewModel() : base(new SettingsEnvironmentModel()) {
        }

        public SettingsEnvironmentViewModel(SettingsEnvironmentModel model) : base(model) { }
    }
}
