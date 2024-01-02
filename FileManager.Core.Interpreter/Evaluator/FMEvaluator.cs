using FileManager.Core.Interpreter.Syntax;
using HB.Code.Interpreter.Analyser;
using HB.Code.Interpreter.Evaluator.Default;
using HB.Code.Interpreter.Lexer.Default;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileManager.Core.Interpreter.Evaluator;
public class FMEvaluator : ISemanticEvaluator<SyntaxTree, DefaultSemanticError> {
    public ImmutableArray<DefaultSemanticError> Evaluate(SyntaxTree syntaxTree, string content) {
        throw new NotImplementedException();
    }
}
