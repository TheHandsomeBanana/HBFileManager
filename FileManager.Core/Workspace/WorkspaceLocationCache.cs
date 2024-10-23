using HBLibrary.Wpf.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileManager.Core.Workspace;
public class WorkspaceLocationCache {
    public WorkspaceLocation? LastWorkspace { get; set; }
    public List<WorkspaceLocation> WorkspaceLocations { get; set; } = [];
}

public class WorkspaceLocationViewModel : ViewModelBase<WorkspaceLocation> {
    public string Name { 
        get => Model.Name;
        set { 
            Model.Name = value;
            NotifyPropertyChanged();
        }
    }

    public WorkspaceLocationViewModel(WorkspaceLocation model) : base(model) {
    }
}

public class WorkspaceLocation : IEquatable<WorkspaceLocation> {
    public required string FullPath { get; set; }
    public required string Name { get; set; }

    public bool Equals(WorkspaceLocation? other) {
        return other?.FullPath == FullPath;
    }

    public override bool Equals(object? obj) {
        return Equals(obj as WorkspaceLocation);
    }

    public override int GetHashCode() {
        return FullPath.GetHashCode();
    }

    public static bool operator ==(WorkspaceLocation left, WorkspaceLocation right) {
        return left.Equals(right);
    }

    public static bool operator !=(WorkspaceLocation left, WorkspaceLocation right) {
        return !(left == right);
    }
}
