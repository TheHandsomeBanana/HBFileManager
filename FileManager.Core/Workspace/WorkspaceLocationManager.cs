using HBLibrary.Common.Account;
using HBLibrary.Common.Extensions;
using HBLibrary.Services.IO.Storage;
using HBLibrary.Services.IO.Storage.Container;
using HBLibrary.Services.IO.Storage.Entries;
using Microsoft.Graph.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileManager.Core.Workspace;
public class WorkspaceLocationManager : IWorkspaceLocationManager {
    private readonly IStorageEntryContainer container;
    public WorkspaceLocationCache LocationCache { get; private set; }

    public event Action<bool, WorkspaceLocation[]>? WorkspaceLocationsChanged;

    public WorkspaceLocationManager(IApplicationStorage applicationStorage) {
        this.container = applicationStorage.DefaultContainer;

        if (container.TryGet("workspacelocations", out IStorageEntry? entry)) {
            WorkspaceLocationCache? locationCache = entry.Get<WorkspaceLocationCache>();
            if (locationCache is not null) {
                this.LocationCache = locationCache;
            }
            else {
                this.LocationCache = new WorkspaceLocationCache();
            }
        }
        else {
            LocationCache = new WorkspaceLocationCache();
        }
    }

    public void AddWorkspaceLocations(string[] locations) {

        List<WorkspaceLocation> added = [];
        foreach (string location in locations) {
            if (LocationCache.WorkspaceLocations.All(e => e.FullPath != location)) {
                WorkspaceLocation wsLocation = new WorkspaceLocation {
                    FullPath = location,
                    Name = Path.GetFileName(location)
                };

                added.Add(wsLocation);
                LocationCache.WorkspaceLocations.Add(wsLocation);
            }
        }

        if (added.Count > 0) {
            container.AddOrUpdate("workspacelocations", LocationCache, StorageEntryContentType.Json);
            WorkspaceLocationsChanged?.Invoke(true, added.ToArray());
        }
    }

    public void RemoveWorkspaceLocations(string[] locations) {

        List<WorkspaceLocation> removed = [];

        foreach (string location in locations) {
            WorkspaceLocation? workspaceLocation = LocationCache.WorkspaceLocations.FirstOrDefault(e => e.FullPath == location);
            if (workspaceLocation is not null) {
                LocationCache.WorkspaceLocations.Remove(workspaceLocation);
                removed.Add(workspaceLocation);

                if (workspaceLocation.FullPath == LocationCache.LastWorkspace?.FullPath) {
                    LocationCache.LastWorkspace = null;
                }
            }
        }

        if (removed.Count > 0) {
            container.AddOrUpdate("workspacelocations", LocationCache, StorageEntryContentType.Json);
            WorkspaceLocationsChanged?.Invoke(false, removed.ToArray());
        }
    }

    public void SetLatestWorkspaceLocation(WorkspaceLocation location) {
        LocationCache.LastWorkspace = location;
        container.AddOrUpdate("workspacelocations", LocationCache, StorageEntryContentType.Json);
    }

    public void Update(WorkspaceLocationCache locationCache) {
        if (!ReferenceEquals(LocationCache, locationCache)) {
            throw new InvalidOperationException("The new caches reference does not match the existing reference");
        }

        this.LocationCache = locationCache;
    }
}
