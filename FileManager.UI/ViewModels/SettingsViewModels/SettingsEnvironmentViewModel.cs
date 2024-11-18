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

        public bool ShowTimestampInValidationLogs {
            get { return Model.ShowTimestampInValidationLogs; }
            set {
                Model.ShowTimestampInValidationLogs = value;
                NotifyPropertyChanged();
            }
        }

        public bool ShowTimestampInRunLogs {
            get { return Model.ShowTimestampInRunLogs; }
            set {
                Model.ShowTimestampInRunLogs = value;
                NotifyPropertyChanged();
            }
        }
        
        public bool ShowTimestampInHistoryLogs {
            get { return Model.ShowTimestampInHistoryLogs; }
            set {
                Model.ShowTimestampInHistoryLogs = value;
                NotifyPropertyChanged();
            }
        }

        public bool ShowCategoryInValidationLogs {
            get => Model.ShowCategoryInValidationLogs;
            set {
                Model.ShowCategoryInValidationLogs = value;
                NotifyPropertyChanged();
            }
        }

        public bool ShowCategoryInRunLogs {
            get => Model.ShowCategoryInRunLogs;
            set {
                Model.ShowCategoryInRunLogs = value;
                NotifyPropertyChanged();
            }
        }
        
        public bool ShowCategoryInHistoryLogs {
            get => Model.ShowCategoryInHistoryLogs;
            set {
                Model.ShowCategoryInHistoryLogs = value;
                NotifyPropertyChanged();
            }
        }

        public bool ShowLogLevelInValidationLogs {
            get { return Model.ShowLogLevelInValidationLogs; }
            set {
                Model.ShowLogLevelInValidationLogs = value;
                NotifyPropertyChanged();
            }
        }

        public bool ShowLogLevelInRunLogs {
            get { return Model.ShowLogLevelInRunLogs; }
            set {
                Model.ShowLogLevelInRunLogs = value;
                NotifyPropertyChanged();
            }
        }

        public bool ShowLogLevelInHistoryLogs {
            get { return Model.ShowLogLevelInHistoryLogs; }
            set {
                Model.ShowLogLevelInHistoryLogs = value;
                NotifyPropertyChanged();
            }
        }

        public bool ShowJobProgressLog {
            get { return Model.ShowJobProgressLog; }
            set {
                Model.ShowJobProgressLog = value;
                NotifyPropertyChanged();
            }
        }


        public SettingsEnvironmentViewModel() : base(new SettingsEnvironmentModel()) {
        }

        public SettingsEnvironmentViewModel(SettingsEnvironmentModel model) : base(model) { }
    }
}
