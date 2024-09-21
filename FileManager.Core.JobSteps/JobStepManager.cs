using FileManager.Core.JobSteps.Attributes;
using FileManager.Core.JobSteps.Models;
using HBLibrary.Common.Plugins;
using HBLibrary.Common.Plugins.Attributes;
using System.Collections.Immutable;
using System.IO;
using System.Reflection;

namespace FileManager.Core.JobSteps;
public class JobStepManager : IJobStepManager {
    private readonly IPluginManager pluginManager;

    private readonly Dictionary<string, Type> jobStepTypes = new() {
        [PluginManager.GetPluginMetadata(typeof(CopyStep)).TypeName] = typeof(CopyStep),
        [PluginManager.GetPluginMetadata(typeof(ArchiveStep)).TypeName] = typeof(ArchiveStep),
    };

    public JobStepManager(IPluginManager pluginManager) {
        this.pluginManager = pluginManager;    
    }

    public void LoadJobSteps() {
        pluginManager.LoadAssemblies();
        IEnumerable<PluginType> jobStepType = pluginManager.TypeProvider
            .QueryByAttribute<IJobStep>(pluginManager.GetLoadedAssemblies());

        foreach(PluginType type in jobStepType) {
            jobStepTypes[type.Metadata.TypeName] = type.ConcreteType;
        }
    }

    public IEnumerable<Type> GetJobStepTypes() {
        return jobStepTypes.Values;
    }
}
