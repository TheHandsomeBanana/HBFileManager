using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileManager.Core.Workspace;
public interface IWorkspaceLocationManager {
    public event Action<bool, WorkspaceLocation[]>? WorkspaceLocationsChanged;
    public WorkspaceLocationCache LocationCache { get; }
    public void AddWorkspaceLocations(string[] locations);
    public void RemoveWorkspaceLocations(string[] locations);
    public void SetLatestWorkspaceLocation(WorkspaceLocation location);
    public void Update(WorkspaceLocationCache locationCache);
}
