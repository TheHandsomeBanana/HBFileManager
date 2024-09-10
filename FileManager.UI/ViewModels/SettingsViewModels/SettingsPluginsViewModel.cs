using FileManager.UI.Models.SettingsModels;
using HBLibrary.Wpf.Commands;
using HBLibrary.Wpf.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileManager.UI.ViewModels.SettingsViewModels;

public class SettingsPluginsViewModel : ViewModelBase<SettingsPluginsModel> {
    private string newPluginText;

    public string NewPluginText {
        get {
            return newPluginText;
        }
        set {
            newPluginText = value;
            NotifyPropertyChanged();
            AddPluginCommand.NotifyCanExecuteChanged();
        }
    }


    public RelayCommand BrowseCommand { get; set; }
    public RelayCommand AddPluginCommand { get; set; }
    public RelayCommand<string> DeletePluginCommand { get; set; }

    public ObservableCollection<string> Plugins { get; set; }
    public SettingsPluginsViewModel(SettingsPluginsModel model) : base(model) {
        Plugins = new ObservableCollection<string>(Model.Plugins);
        Plugins.CollectionChanged += (_, s) => {
            Model.Plugins.Clear();
            Model.Plugins.AddRange(Plugins);
        };

        AddPluginCommand = new RelayCommand(AddPlugin, o => IsPotentialNewPlugin(NewPluginText));
        DeletePluginCommand = new RelayCommand<string>(DeletePlugin, true);
        BrowseCommand = new RelayCommand(Browse, true);
    }

    private void Browse(object? obj) {
    }

    private void DeletePlugin(string obj) {
        Plugins.Remove(obj);
    }

    private void AddPlugin(object? obj) {
        Plugins.Add(NewPluginText);
        NewPluginText = "";
    }

    private bool IsPotentialNewPlugin(string text) {
        return text.EndsWith(".dll", StringComparison.OrdinalIgnoreCase)
            && !Plugins.Contains(text);
    }
}
