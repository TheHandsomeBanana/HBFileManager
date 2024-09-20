using FileManager.Core.JobSteps.Attributes;
using FileManager.Core.JobSteps.Models;
using HBLibrary.Common.Plugins;
using System.Collections.Immutable;
using System.IO;
using System.Reflection;

namespace FileManager.Core.JobSteps;
public class JobStepManager : IJobStepManager {
    private readonly IPluginManager pluginManager;

    private readonly Dictionary<string, Type> jobStepTypes = new() {
        [GetJobStepMetadata(typeof(CopyStep)).TypeName] = typeof(CopyStep),
        [GetJobStepMetadata(typeof(ArchiveStep)).TypeName] = typeof(ArchiveStep),
    };

    public JobStepManager(IPluginManager pluginManager) {
        this.pluginManager = pluginManager;    
    }

    public void LoadJobSteps() {
        pluginManager.LoadAssemblies();
        ImmutableArray<Type> jobStepType = pluginManager.GetPluginTypes<IJobStep>();
        foreach(Type type in jobStepType) {
            string typeName = GetJobStepMetadata(type).TypeName;
            jobStepTypes[typeName] = type;
        }
    }

    public IEnumerable<Type> GetJobStepTypes() {
        return jobStepTypes.Values;
    }

    private static readonly Dictionary<Type, JobStepMetadata> knownJobStepMetadata = [];
    public static JobStepMetadata GetJobStepMetadata(Type stepType) {
        if (knownJobStepMetadata.TryGetValue(stepType, out JobStepMetadata? jobStepMetadata)) {
            return jobStepMetadata;
        }

        JobStepTypeAttribute? stepTypeAttribute = stepType.GetCustomAttribute<JobStepTypeAttribute>(false);
        JobStepDescriptionAttribute? stepDescriptionAttribute = stepType.GetCustomAttribute<JobStepDescriptionAttribute>(false);

        jobStepMetadata = new JobStepMetadata {
            TypeName = stepTypeAttribute is not null ? stepTypeAttribute.Name : stepType.FullName!,
            Description = stepDescriptionAttribute?.Description
        };

        knownJobStepMetadata[stepType] = jobStepMetadata;
        return jobStepMetadata;
    }
}
