using HBLibrary.Wpf.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileManager.UI.Models.SettingsModels;
public class SettingsExecutionModel : TrackableModel {
    private bool continueOnError;
    public bool ContinueOnError { 
        get => continueOnError; 
        set {
            continueOnError = value;
            NotifyTrackableChanged(value);
        } 
    }
}
