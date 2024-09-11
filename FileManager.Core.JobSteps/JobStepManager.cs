using FileManager.Core.JobSteps.Attributes;
using FileManager.Core.JobSteps.Models;
using HBLibrary.Common.Plugins;
using System.IO;
using System.Reflection;

namespace FileManager.Core.JobSteps;
public class JobStepManager : IJobStepManager {
    private readonly IPluginManager pluginManager;

    private readonly Dictionary<string, Type> jobStepTypes = new() {
        [GetJobStepTypeName(typeof(CopyStep))] = typeof(CopyStep),
        [GetJobStepTypeName(typeof(ArchiveStep))] = typeof(ArchiveStep),
    };

    public JobStepManager(IPluginManager pluginManager) {
        this.pluginManager = pluginManager;    
    }

    public void LoadPluginJobSteps() {
        pluginManager.LoadAssemblies();
        Type[] jobStepType = pluginManager.GetPluginTypes<IJobStep>();
        foreach(Type type in jobStepType) {
            string typeName = GetJobStepTypeName(type);
            jobStepTypes[typeName] = type;
        }
    }

    public IEnumerable<Type> GetJobStepTypes() {
        return jobStepTypes.Values;
    }

    private static readonly Dictionary<Type, string> knownDisplayNames = [];
    public static string GetJobStepTypeName(Type stepType) {
        if (knownDisplayNames.TryGetValue(stepType, out string? displayName)) {
            return displayName;
        }

        JobStepTypeAttribute? nameAttribute = stepType.GetCustomAttribute<JobStepTypeAttribute>(false);

        displayName = nameAttribute is not null ? nameAttribute.Name : stepType.FullName!;
        knownDisplayNames[stepType] = displayName;
        return displayName;
    }
}
