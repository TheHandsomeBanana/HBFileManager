using HBLibrary.Wpf.Models;

namespace FileManager.UI.Models.SettingsModels;
public class SettingsEnvironmentModel : TrackableModel {
    public bool validateOnNavigation;
    public bool ValidateOnNavigation {
        get => validateOnNavigation;
        set {
            validateOnNavigation = value;
            NotifyTrackableChanged(value);
        }
    }

    private bool showTimestampInValidationLogs;
    public bool ShowTimestampInValidationLogs {
        get { return showTimestampInValidationLogs; }
        set {
            showTimestampInValidationLogs = value;
            NotifyTrackableChanged(value);
        }
    }

    private bool showTimestampInRunLogs;
    public bool ShowTimestampInRunLogs {
        get { return showTimestampInRunLogs; }
        set {
            showTimestampInRunLogs = value;
            NotifyTrackableChanged(value);
        }
    }

    private bool showTimestampInHistoryLogs;

    public bool ShowTimestampInHistoryLogs {
        get { return showTimestampInHistoryLogs; }
        set {
            showTimestampInHistoryLogs = value;
            NotifyTrackableChanged(value);
        }
    }


    private bool showCategoryInValidationLogs;
    public bool ShowCategoryInValidationLogs {
        get { return showCategoryInValidationLogs; }
        set {
            showCategoryInValidationLogs = value;
            NotifyTrackableChanged(value);
        }
    }

    private bool showCategoryInRunLogs;
    public bool ShowCategoryInRunLogs {
        get { return showCategoryInRunLogs; }
        set {
            showCategoryInRunLogs = value;
            NotifyTrackableChanged(value);
        }
    }

    private bool showCategoryInHistoryLogs;

    public bool ShowCategoryInHistoryLogs {
        get { return showCategoryInHistoryLogs; }
        set {
            showCategoryInHistoryLogs = value;
            NotifyTrackableChanged(value);
        }
    }



    private bool showLogLevelInValidationLogs;
    public bool ShowLogLevelInValidationLogs {
        get { return showLogLevelInValidationLogs; }
        set {
            showLogLevelInValidationLogs = value;
            NotifyTrackableChanged(value);
        }
    }

    private bool showLogLevelInRunLogs;
    public bool ShowLogLevelInRunLogs {
        get { return showLogLevelInRunLogs; }
        set {
            showLogLevelInRunLogs = value;
            NotifyTrackableChanged(value);
        }
    }

    private bool showLogLevelInHistoryLogs;
    public bool ShowLogLevelInHistoryLogs {
        get { return showLogLevelInHistoryLogs; }
        set {
            showLogLevelInHistoryLogs = value;
            NotifyTrackableChanged(value);
        }
    }

    private bool showJobProgressLog;
    public bool ShowJobProgressLog {
        get { return showJobProgressLog; }
        set {
            showJobProgressLog = value;
            NotifyTrackableChanged(value);
        }
    }

}
