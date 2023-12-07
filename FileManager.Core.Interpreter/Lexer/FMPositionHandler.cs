using HB.Code.Interpreter.Location;

namespace FileManager.Core.Interpreter.Lexer;
public class FMPositionHandler : IPositionHandler<FMPosition> {
    public FMPosition CurrentPosition { get; private set; }

    public FMPositionHandler() {
        CurrentPosition = new FMPosition(null, -1, 0, "Start");
    }

    public void MoveNext(int steps) {
        FMPosition old = CurrentPosition;
        CurrentPosition = new FMPosition(old, CurrentPosition.Value + steps, CurrentPosition.Line);
    }

    public void NewLine() => CurrentPosition.Line++;
}
