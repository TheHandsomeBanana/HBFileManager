namespace FileManager.Core.JobSteps.Attributes;
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class JobStepTypeAttribute : Attribute {
    public string Name { get; set; }

    public JobStepTypeAttribute(string name) {
        Name = name;
    }
}
