using HBLibrary.Common.Account;
using HBLibrary.Common.Extensions;
using HBLibrary.Services.IO.Storage;
using HBLibrary.Services.IO.Storage.Container;
using HBLibrary.Services.IO.Storage.Entries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileManager.Core.Workspace;
public class WorkspaceLocationManager : IWorkspaceLocationManager {
    private readonly IStorageEntryContainer container;
    public WorkspaceLocationManager(IApplicationStorage applicationStorage) {
        this.container = applicationStorage.GetContainer(typeof(WorkspaceLocationManager));
    }

    public async Task<WorkspaceLocationCache> GetWorkspaceLocationsAsync() {
        if (container.TryGet("workspacelocations", out IStorageEntry? entry)) {
            WorkspaceLocationCache? locationCache = await entry.GetAsync<WorkspaceLocationCache>();
            if(locationCache is not null) {
                return locationCache;
            }
        }

        WorkspaceLocationCache newLocationCache = new WorkspaceLocationCache();
        await SaveWorkspaceLocationsAsync(newLocationCache);
        return newLocationCache;
    }

    public async Task AddWorkspaceLocationAsync(string location) {
        WorkspaceLocationCache? workspaceLocations = await GetWorkspaceLocationsAsync();

        if (workspaceLocations is not null) {
            workspaceLocations.WorkspaceLocations.Add(location);
            await SaveWorkspaceLocationsAsync(workspaceLocations);
        }
    }

    public async Task RemoveWorkspaceLocation(string location) {
        WorkspaceLocationCache? workspaceLocations = await GetWorkspaceLocationsAsync();

        if (workspaceLocations is not null) {
            workspaceLocations.WorkspaceLocations.Remove(location);
            await SaveWorkspaceLocationsAsync(workspaceLocations);
        }
    }

    private Task SaveWorkspaceLocationsAsync(WorkspaceLocationCache locations) {
        container.AddOrUpdate("workspacelocations", locations, StorageEntryContentType.Json);
        return container.SaveAsync();
    }
}
