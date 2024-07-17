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
    public RelayCommand<Window> AddJobCommand { get; set; }
    public RelayCommand<Window> CancelCommand { get; set; }

    private string name = "";
    public string Name {
        get { return name; }
        set {
            name = value;
            NotifyPropertyChanged();

            AddJobCommand.NotifyCanExecuteChanged();
        }
    }

    public AddJobViewModel() {
        AddJobCommand = new RelayCommand<Window>(AddAndFinish, _ => !string.IsNullOrWhiteSpace(Name));
        CancelCommand = new RelayCommand<Window>(CancelAndFinish, true);
    }

    private void AddAndFinish(Window? obj) {
        if(obj is null) {
            return;
        }

        obj.DialogResult = true;
        obj.Close();
    }

    private void CancelAndFinish(Window? obj) {
        if (obj is null) {
            return;
        }

        obj.DialogResult = false;
        obj.Close();
    }
}
