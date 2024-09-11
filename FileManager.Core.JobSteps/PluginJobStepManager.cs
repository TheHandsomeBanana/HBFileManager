using FileManager.Core.JobSteps.Attributes;
using FileManager.Core.JobSteps.Models;
using System.IO;
using System.Reflection;

namespace FileManager.Core.JobSteps;
public class PluginJobStepManager : IPluginJobStepManager {
    private readonly string path;
    private readonly Dictionary<string, Type> jobStepTypes = new Dictionary<string, Type> {
        [GetJobStepTypeName(typeof(CopyStep))] = typeof(CopyStep),
        [GetJobStepTypeName(typeof(ArchiveStep))] = typeof(ArchiveStep),
    };

    public PluginJobStepManager(string path) {
        this.path = path;
        Directory.CreateDirectory(path);
    }

    public bool JobStepsLoaded { get; private set; } = false;
    public bool JobStepsLoading { get; private set; } = false;

    public event Action? PluginJobStepsLoaded;
    public event Action? PluginJobStepLoading;

    public void LoadPluginJobSteps() {
        JobStepsLoading = true;
        PluginJobStepLoading?.Invoke();

        IEnumerable<Assembly> assemblies = LoadPluginAssemblies();
        foreach (Assembly assembly in assemblies) {
            RegisterJobStepTypesFromAssembly(assembly);
        }

        JobStepsLoading = false;
        JobStepsLoaded = true;
        PluginJobStepsLoaded?.Invoke();
    }

    private IEnumerable<Assembly> LoadPluginAssemblies() {
        string[] plugins = Directory.GetFiles(path, "*.dll", SearchOption.AllDirectories);

        foreach (string plugin in plugins) {
            yield return Assembly.LoadFrom(plugin);
        }
    }

    private void RegisterJobStepTypesFromAssembly(Assembly assembly) {
        IEnumerable<Type> types = assembly.GetTypes()
            .Where(e => e.IsAssignableTo(typeof(IJobStep)) && e.IsClass && !e.IsAbstract);

        foreach (Type type in types) {
            string typeName = GetJobStepTypeName(type);
            jobStepTypes[typeName] = type;
        }
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

    public IEnumerable<Type> GetJobStepTypes() {
        return jobStepTypes.Values;
    }

    public Type GetJobStepType(string typeName) {
        return jobStepTypes[typeName];
    }
}
