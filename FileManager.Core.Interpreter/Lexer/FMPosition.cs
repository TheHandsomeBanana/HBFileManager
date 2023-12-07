using HB.Code.Interpreter.Location;
using System.Text;

namespace FileManager.Core.Interpreter.Lexer;
public class FMPosition : IPosition {
    public FMPosition? Previous { get; set; }
    public int Value { get; set; }
    public int Line { get; set; }
    public string? Tag { get; set; }

    public FMPosition(FMPosition? previous, int value, int line, string? tag = null) {
        if (previous is null && (value > -1 || line > 0))
            ArgumentNullException.ThrowIfNull(nameof(previous));

        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(previous!.Value, Value);

        Previous = previous;
        Value = value;
        Line = line;
        Tag = tag;
    }

    public TextSpan? GetSpanFromPrevious() => Previous is null ? null : new TextSpan(Previous.Value, Value);
    public char[] GetCharsFromPrevious(string fileContent) => Previous is null
        ? Array.Empty<char>()
        : ReadFromPrevious(fileContent).ToCharArray();


    private string ReadFromPrevious(string fileContent) {
        StringBuilder sb = new StringBuilder();
        for (int i = Previous!.Value; i <= Value; i++)
            sb.Append(fileContent[i]);

        return sb.ToString();
    }
}
