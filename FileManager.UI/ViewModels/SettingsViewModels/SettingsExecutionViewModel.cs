using FileManager.UI.Models.SettingsModels;
using HBLibrary.Wpf.ViewModels;

namespace FileManager.UI.ViewModels.SettingsViewModels; 
public class SettingsExecutionViewModel : ViewModelBase<SettingsExecutionModel> {

    public bool ContinueOnError {
        get => Model.ContinueOnError; 
        set {
            Model.ContinueOnError = value;
            NotifyPropertyChanged();
        }
    }

    public SettingsExecutionViewModel(SettingsExecutionModel model) : base(model) {
    }
}
