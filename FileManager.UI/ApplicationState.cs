using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace FileManager.UI;
public class ApplicationState {
    public WindowState WindowState { get; set; }
    public double? Top { get; set; }
    public double? Left { get; set; }
}
