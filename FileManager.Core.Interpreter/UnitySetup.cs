
using HBLibrary.Interface.DI;
using HBLibrary.Interface.Interpreter;
using Unity;

namespace FileManager.Core.Interpreter;
internal class UnitySetup : IUnitySetup {
    public void Build(IUnityContainer container) {
        container.RegisterType<IInterpreter, FMInterpreter>();
    }
}
