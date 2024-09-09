using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace FileManager.Core.JobSteps.Converters;
public class JobStepConverter : JsonConverter<IJobStep> {
    public override IJobStep? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
        using (JsonDocument doc = JsonDocument.ParseValue(ref reader)) {
            JsonElement root = doc.RootElement;
            string? typeName = root.GetProperty("TypeName").GetString()
                ?? throw new JsonException("Missing type information for step");

            Type? stepType = Type.GetType(typeName)
                ?? throw new JsonException($"Unknown type {typeName}");

            return (IJobStep)JsonSerializer.Deserialize(root.GetProperty("StepData").GetRawText(), stepType, options)!;
        }
    }

    public override void Write(Utf8JsonWriter writer, IJobStep value, JsonSerializerOptions options) {
        writer.WriteStartObject();

        // Write the type name
        writer.WriteString("TypeName", value.GetType().AssemblyQualifiedName);

        // Write the step data
        writer.WritePropertyName("StepData");
        JsonSerializer.Serialize(writer, value, value.GetType(), options);

        writer.WriteEndObject();
    }
}
