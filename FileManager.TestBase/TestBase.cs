using FileManager.Core.Interpreter.Lexer;
using FileManager.Core.Interpreter.Syntax;
using HB.Code.Interpreter.Lexer;
using HB.Code.Interpreter.Lexer.Default;
using HB.DependencyInjection.Unity;
using Unity;

namespace FileManager.TestBase;

[TestClass]
public abstract class TestBase {
    protected readonly string LexerScriptNoError = File.ReadAllText("../../../Assets/LexerScript_NoError.txt");
    protected readonly string LexerScriptError = File.ReadAllText("../../../Assets/LexerScript_Error.txt");
    protected readonly string ParserScriptNoError = File.ReadAllText("../../../Assets/ParserScript_NoError.txt");
    protected readonly string ParserScriptError = File.ReadAllText("../../../Assets/ParserScript_Error.txt");
    protected IUnityContainer UnityContainer { get; }

    public TestBase() {
        UnityContainer = UnityBase.CreateOrGetChildContainer("TestContainer");
        UnityContainer.RegisterType(typeof(ILexer<SyntaxToken, DefaultSyntaxError>), typeof(FMLexer));
    }
}