using HBLibrary.Code.Interpreter;
using HBLibrary.Common.DI.Unity;
using Unity;

namespace FileManager.Core.Interpreter;
internal class UnitySetup : IUnitySetup {
    public void Build(IUnityContainer container) {
        container.RegisterType<IInterpreter, FMInterpreter>();
    }
}
