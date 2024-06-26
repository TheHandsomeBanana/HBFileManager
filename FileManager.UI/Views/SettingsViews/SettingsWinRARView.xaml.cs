using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace FileManager.UI.Views.SettingsViews;
/// <summary>
/// Interaction logic for SettingsWinRARView.xaml
/// </summary>
public partial class SettingsWinRARView : UserControl {
    public SettingsWinRARView() {
        InitializeComponent();

        this.DataContextChanged += (s, e) => {
            Debug.WriteLine($"SettingsWinRARView DataContext: {this.DataContext?.GetType().Name}");
        };
    }
}
