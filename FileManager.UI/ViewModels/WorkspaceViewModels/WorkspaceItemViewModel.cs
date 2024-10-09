using FileManager.Core.Workspace;
using HBLibrary.Wpf.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileManager.UI.ViewModels.WorkspaceViewModels;

public class WorkspaceItemViewModel : ViewModelBase<HBFileManagerWorkspace> {
    
    public string Owner { get => Model.Owner!.Username; }
    public string Name { get => Model.Name!; }
    public bool UsesEncryption { get => Model.UsesEncryption; }
    public string FullPath { get => Model.FullPath!; }

    public WorkspaceItemViewModel(HBFileManagerWorkspace model) : base(model) {

    }
}
