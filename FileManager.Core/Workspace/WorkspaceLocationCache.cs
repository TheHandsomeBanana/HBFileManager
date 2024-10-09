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
