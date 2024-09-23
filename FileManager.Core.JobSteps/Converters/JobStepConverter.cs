using FileManager.Core.JobSteps.Models;
using HBLibrary.Common.Plugins;
using HBLibrary.Common.Plugins.Provider.Registry;
using HBLibrary.Common.Plugins.Provider.Resolver;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace FileManager.Core.JobSteps.Converters;
public class JobStepConverter : JsonConverter<JobStep> {
    private readonly IPluginManager pluginManager;

    public JobStepConverter(IPluginManager pluginManager) {
        this.pluginManager = pluginManager;
    }


    public override JobStep? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
        using (JsonDocument doc = JsonDocument.ParseValue(ref reader)) {
            JsonElement root = doc.RootElement;
            string? typeName = root.GetProperty("TypeName").GetString()
                ?? throw new JsonException("Missing type information for step");

            Type? stepType = pluginManager.TypeRegistry.GetType(typeName);
            if(stepType is null) {
                stepType = pluginManager.TypeResolver.ResolveType(typeName, pluginManager.GetLoadedAssemblies())
                    ?? throw new JsonException($"Unknown type {typeName}");

                pluginManager.TypeRegistry.RegisterType(stepType);
            }

            return (JobStep)JsonSerializer.Deserialize(root.GetProperty("StepData").GetRawText(), stepType, options)!;
        }
    }

    public override void Write(Utf8JsonWriter writer, JobStep value, JsonSerializerOptions options) {
        writer.WriteStartObject();

        Type valueType = value.GetType();

        string fullName = valueType.Assembly == Assembly.GetExecutingAssembly()
            ? valueType.AssemblyQualifiedName!
            : valueType.FullName!;


        // Write the type name
        writer.WriteString("TypeName", fullName);

        // Write the step data
        writer.WritePropertyName("StepData");
        JsonSerializer.Serialize(writer, value, value.GetType(), options);

        writer.WriteEndObject();
    }
}
