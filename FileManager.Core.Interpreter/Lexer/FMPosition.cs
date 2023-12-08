using HB.Code.Interpreter.Exceptions;
using HB.Code.Interpreter.Lexer;
using HB.Code.Interpreter.Location;
using System.Numerics;
using System.Text;

namespace FileManager.Core.Interpreter.Lexer;
public class FMPosition : IPosition {
    public FMPosition? Previous { get; set; }
    public int Index { get; set; }
    public int Line { get; set; }
    public int LineIndex { get; set; }


    private FMPosition(FMPosition? previous, int index, int line, int lineIndex) {
        if (previous is null && (index > -1 || line > 0))
            ArgumentNullException.ThrowIfNull(nameof(previous));

        ArgumentOutOfRangeException.ThrowIfGreaterThan(previous!.Index, index);

        Previous = previous;
        Index = index;
        Line = line;
        LineIndex = lineIndex;
    }

    private FMPosition(int index, int line, int lineIndex) {
        Previous = null;
        Index = index;
        Line = line;
        LineIndex = lineIndex;
    }

    public static FMPosition CreateNull() => new FMPosition(-1, 1, -1);
    public static FMPosition CreateStart() => new FMPosition(-1, 1, -1);
    public static FMPosition CreateFrom(FMPosition position, int index, int line, int lineIndex)
        => new FMPosition(position, index, line, lineIndex);

    public TextSpan GetSpanFromPrevious() => new TextSpan(Previous?.Index ?? Index, Index - (Previous?.Index ?? Index));
    public char[] GetCharsFromPrevious(string content) => GetStringFromPrevious(content).ToCharArray();

    public string GetStringFromPrevious(string content) {
        StringBuilder sb = new StringBuilder();
        for (int i = Previous!.Index; i < Index; i++)
            sb.Append(content[i]);

        return sb.ToString();
    }

    public char GetValue(string content) {
        if (Index == -1 || Index >= content.Length)
            return CommonCharCollection.NULL;

        return content[Index];
    }
}
