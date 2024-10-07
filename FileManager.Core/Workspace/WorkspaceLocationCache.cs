using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileManager.Core.Workspace;
public class WorkspaceLocationCache {
    public string? LastWorkspace {  get; set; }
    public List<string> WorkspaceLocations { get; set; } = [];
}
