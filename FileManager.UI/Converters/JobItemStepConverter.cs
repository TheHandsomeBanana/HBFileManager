using FileManager.UI.Models.JobModels;
using FileManager.UI.Models.JobModels.JobStepModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace FileManager.UI.Converters;
public class JobItemStepConverter : JsonConverter<JobItemStepModel> {
    public override JobItemStepModel? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
        using JsonDocument document = JsonDocument.ParseValue(ref reader);
        JsonElement rootElement = document.RootElement;

        if (!rootElement.TryGetProperty("StepType", out JsonElement stepTypeElement)) {
            throw new JsonException("StepType property not found");
        }

        StepType stepType = (StepType)stepTypeElement.GetInt32();
        Type targetType = stepType switch {
            StepType.Archive => typeof(ArchiveStepModel),
            StepType.Copy => typeof(CopyStepModel),
            StepType.Move => typeof(MoveStepModel),
            StepType.Replace => typeof(ReplaceStepModel),
            _ => throw new NotSupportedException($"StepType {stepType} is not supported")
        };

        return (JobItemStepModel)JsonSerializer.Deserialize(rootElement.GetRawText(), targetType, options)!;

    }

    public override void Write(Utf8JsonWriter writer, JobItemStepModel value, JsonSerializerOptions options) {
        JsonSerializer.Serialize(writer, value, value.GetType(), options);
    }
}
