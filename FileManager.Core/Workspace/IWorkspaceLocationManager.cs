using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileManager.Core.Workspace;
public interface IWorkspaceLocationManager {
    public Task<WorkspaceLocationCache> GetWorkspaceLocationsAsync();
    public Task AddWorkspaceLocationAsync(string location);
    public Task RemoveWorkspaceLocation(string location);
}
