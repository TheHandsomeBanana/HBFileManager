using FileManager.Core.Interpreter.Syntax;
using FileManager.Core.Interpreter.Syntax.Commands;
using HBLibrary.Code.Interpreter;
using HBLibrary.Code.Interpreter.Evaluator;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileManager.Core.Interpreter;
public class FMEvaluator : ISemanticEvaluator<SyntaxTree>
{
    private string content;
    public ImmutableArray<SimpleError> Evaluate(SyntaxTree syntaxTree, string content)
    {
        this.content = content;

        return syntaxTree.Root
            .GetDescendantNodes()
            .SelectMany(e => CheckNode(e))
            .ToImmutableArray();
    }

    private ImmutableArray<SimpleError> CheckNode(SyntaxNode? node)
    {
        ImmutableArray<SimpleError>.Builder errorBuilder = ImmutableArray.CreateBuilder<SimpleError>();
        switch (node)
        {
            case CommandStatementSyntax commandStatement:
                errorBuilder.AddRange(CheckNode(commandStatement.Command));
                break;
            case CommandSyntax command:
                errorBuilder.AddRange(CheckNode(command.ParameterList));
                break;
            case CommandParameterListSyntax commandParameterList:
                foreach (CommandParameterSyntax commandParameter in commandParameterList.Parameters)
                {
                    errorBuilder.AddRange(CheckParameter(commandParameter));
                }
                break;

        }

        return errorBuilder.ToImmutableArray();
    }

    private List<SimpleError> CheckParameter(CommandParameterSyntax commandParameter)
    {
        CommandSyntax parentCommand = (CommandSyntax)commandParameter.Parent!;
        if (FMSemanticRuleset.CheckValidCommandParameter(parentCommand.Kind, commandParameter.Kind))
        {
            return [new SimpleError(
                commandParameter.Span,
                commandParameter.LineSpan,
                $"{commandParameter.Kind} is not valid for {parentCommand.Kind}",
                SimpleError.GetAffectedString(commandParameter, content)
            )];
        }

        List<SimpleError> list = [];


        return list;
    }
}
