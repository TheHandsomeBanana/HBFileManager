using HBLibrary.Wpf.Commands;
using HBLibrary.Wpf.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace FileManager.UI.ViewModels.JobViewModels;
public class AddJobViewModel : ViewModelBase {
    public RelayCommand AddJobCommand { get; set; }
    public RelayCommand CancelCommand { get; set; }


    private string name = "";
    public string Name {
        get { return name; }
        set {
            name = value;
            NotifyPropertyChanged();
        }
    }

    public AddJobViewModel() {
        AddJobCommand = new RelayCommand(AddAndFinish, true);
        CancelCommand = new RelayCommand(CancelAndFinish, true);
    }

    private void AddAndFinish(object? obj) {
        if(obj is Window window) {
            window.DialogResult = true;
            window.Close();
        }
    }

    private void CancelAndFinish(object? obj) {
        if (obj is Window window) {
            window.DialogResult = false;
            window.Close();
        }
    }
}
