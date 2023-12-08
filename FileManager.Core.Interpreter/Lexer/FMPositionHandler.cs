using HB.Code.Interpreter.Exceptions;
using HB.Code.Interpreter.Lexer;
using HB.Code.Interpreter.Location;

namespace FileManager.Core.Interpreter.Lexer;
public class FMPositionHandler : IPositionHandler<FMPosition> {
    public FMPosition CurrentPosition { get; private set; } = FMPosition.CreateNull();
    public string Content { get; private set; } = "";


    public void Init(string content) {
        Content = content;
        CurrentPosition = FMPosition.CreateStart();
    }

    public void MoveNext(int steps) {
        FMPosition old = CurrentPosition;
        CurrentPosition = FMPosition.CreateFrom(old, CurrentPosition.Index + steps, CurrentPosition.Line, CurrentPosition.LineIndex + steps);
    }

    public void NewLine() {
        CurrentPosition.Line += 1;
        CurrentPosition.LineIndex = -1;
    }

    public void MoveNextWhile(int steps, Predicate<FMPosition> predicate) {
        FMPosition old = CurrentPosition;
        CurrentPosition = FMPosition.CreateFrom(old, CurrentPosition.Index, CurrentPosition.Line, CurrentPosition.LineIndex);

        while (predicate.Invoke(CurrentPosition)) {
            if (CurrentPosition.GetValue(Content) == CommonCharCollection.NULL)
                return;

            CurrentPosition.Index += steps;
            CurrentPosition.LineIndex += steps;
        }
    }

    public void Skip(int steps) {
        CurrentPosition.Index += steps;
        CurrentPosition.LineIndex += steps;
    }

    public void Reset() {
        CurrentPosition = FMPosition.CreateNull();
    }
}
