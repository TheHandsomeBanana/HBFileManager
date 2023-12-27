using FileManager.Core.Interpreter.Lexer;
using HB.Code.Interpreter;
using HB.DependencyInjection.Unity;
using Unity;

namespace FileManager.Core.Interpreter;
internal class UnitySetup : IUnitySetup {
    public void Build(IUnityContainer container) {
        container.RegisterType<IInterpreter, FMInterpreter>();
    }
}
