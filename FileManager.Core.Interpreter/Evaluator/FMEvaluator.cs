using FileManager.Core.Interpreter.Syntax;
using HBLibrary.Code.Interpreter;
using HBLibrary.Code.Interpreter.Evaluator;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileManager.Core.Interpreter.Evaluator;
public class FMEvaluator : ISemanticEvaluator<SyntaxTree> {
    public ImmutableArray<SimpleError> Evaluate(SyntaxTree syntaxTree, string content) {
        throw new NotImplementedException();
    }
}
