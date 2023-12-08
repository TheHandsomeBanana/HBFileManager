using FileManager.Core.Interpreter.Lexer;
using FileManager.Core.Interpreter.Syntax;
using HB.Code.Interpreter.Lexer;
using HB.Code.Interpreter.Location;
using HB.DependencyInjection.Unity;
using Unity;

namespace FileManager.TestBase;

[TestClass]
public abstract class TestBase {
    protected IUnityContainer UnityContainer { get; }

    public TestBase() {
        UnityContainer = UnityBase.CreateOrGetChildContainer("TestContainer");
        UnityContainer.RegisterType(typeof(ILexer<FMSyntaxToken>), typeof(FMLexer))
            .RegisterType(typeof(IPositionHandler<FMPosition>), typeof(FMPositionHandler));
    }
}