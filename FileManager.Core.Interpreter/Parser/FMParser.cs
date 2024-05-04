using FileManager.Core.Interpreter.Syntax;
using FileManager.Core.Interpreter.Syntax.Arguments;
using FileManager.Core.Interpreter.Syntax.Commands;
using FileManager.Core.Interpreter.Syntax.Expressions;
using HBLibrary.Code.Interpreter;
using HBLibrary.Code.Interpreter.Parser;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;

namespace FileManager.Core.Interpreter.Parser;
public class FMParser : IParser<SyntaxTree, SyntaxToken> {
    private readonly List<SimpleError> syntaxErrors = [];
    private readonly TokenReader<SyntaxToken, SyntaxTokenKind> tokenReader = new();

    public SyntaxTree Parse(ImmutableArray<SyntaxToken> tokens) {
        syntaxErrors.Clear();
        tokenReader.Init(tokens);

        SyntaxNode root = new InterpreterUnitSyntax(SyntaxNodeKind.InterpreterUnit);
        root.AddChildToken(tokens.Last());
        SyntaxToken firstToken = tokens.First();
        SyntaxToken lastToken = tokens.Last();

        SetSpans(root, firstToken.FullSpan, lastToken.FullSpan, firstToken.LineSpan, lastToken.LineSpan);

        while (tokenReader.CanMoveNext()) {
            SyntaxNode? nextNode = BuildNextNode();

            if (nextNode != null)
                root.AddChildNode(nextNode);

            tokenReader.MoveNext();
        }

        return new SyntaxTree(root);
    }

    public ImmutableArray<SimpleError> GetSyntaxErrors(string content) => syntaxErrors.Select(e => {
        e.SetAffected(content);
        return e;
    }).ToImmutableArray();

    #region Parsing
    // Build nodes that are direct childs from InterpreterUnitSyntax
    private SyntaxNode? BuildNextNode() {
        return tokenReader.CurrentToken.Kind switch {
            SyntaxTokenKind.CopyKeyword or
            SyntaxTokenKind.MoveKeyword or
            SyntaxTokenKind.ReplaceKeyword => BuildCommandStatement(),
            _ => null,
        };
    }

    private CommandStatementSyntax? BuildCommandStatement() {
        CommandStatementSyntax commandStatement = new CommandStatementSyntax(SyntaxNodeKind.CommandStatement);
        TextSpan start = tokenReader.GetCurrentFullSpan();
        LineSpan startLine = tokenReader.GetCurrentLineSpan();


        CommandSyntax? command = BuildCommandSyntax();
        if (command is null)
            return null;


        commandStatement.AddChildNode(command);
        TextSpan end = tokenReader.GetCurrentFullSpan();
        LineSpan endLine = tokenReader.GetCurrentLineSpan();

        // BuildCommandSyntax advances tokenReader 1 step after last command parameter
        // --> CurrentToken should be ';' at this point
        if (!tokenReader.CurrentToken.IsKind(SyntaxTokenKind.Semicolon)) {
            syntaxErrors.Add(new SimpleError(
                GetTextSpan(start, end),
                GetLineSpan(startLine, endLine),
                "';' expected"
            ));
            return null;
        }

        commandStatement.AddChildToken(tokenReader.CurrentToken);

        SetSpans(commandStatement, start, end, startLine, endLine);
        return commandStatement;
    }

    private CommandSyntax? BuildCommandSyntax() {
        CommandSyntax? command = tokenReader.CurrentToken.Kind switch {
            SyntaxTokenKind.MoveKeyword or
            SyntaxTokenKind.CopyKeyword or
            SyntaxTokenKind.ReplaceKeyword or
            SyntaxTokenKind.ArchiveKeyword => new CommandSyntax(tokenReader.CurrentToken.GetNodeKind()),
            _ => null
        };

        TextSpan start = tokenReader.GetCurrentFullSpan();
        LineSpan startLine = tokenReader.GetCurrentLineSpan();

        if (command is null) {
            AddSyntaxErrorWithCurrentSpan("Command [COPY, MOVE, ..] expected");
            return null;
        }

        command.AddChildToken(tokenReader.CurrentToken);
        SyntaxToken? nextToken = tokenReader.GetNextToken();

        if (!nextToken?.Kind.IsCommandParameterTokenKind() ?? true) {
            AddSyntaxErrorWithCurrentSpan("Command parameter [-source, -target, ..] expected");
            return null;
        }

        CommandParameterListSyntax? commandParameterList = BuildCommandParameterList();
        if (commandParameterList is null)
            return null;

        command.AddChildNode(commandParameterList);
        TextSpan end = tokenReader.GetCurrentFullSpan();
        LineSpan endLine = tokenReader.GetCurrentLineSpan();

        SetSpans(command, start, end, startLine, endLine);
        return command;
    }

    private CommandParameterListSyntax? BuildCommandParameterList() {
        TextSpan start = tokenReader.GetCurrentFullSpan();
        LineSpan startLine = tokenReader.GetCurrentLineSpan();

        CommandParameterListSyntax commandArgumentList = new CommandParameterListSyntax(SyntaxNodeKind.CommandParameterList);

        while (tokenReader.CanMoveNext() && tokenReader.CurrentToken.Kind.IsCommandParameterTokenKind()) {
            CommandParameterSyntax? commandArgument = BuildCommandParameter();
            if (commandArgument is null)
                return null;

            commandArgumentList.AddChildNode(commandArgument);
            tokenReader.MoveNext();
        }

        TextSpan end = tokenReader.GetCurrentFullSpan();
        LineSpan endLine = tokenReader.GetCurrentLineSpan();
        SetSpans(commandArgumentList, start, end, startLine, endLine);

        return commandArgumentList;
    }

    private CommandParameterSyntax? BuildCommandParameter() {
        TextSpan start = tokenReader.GetCurrentFullSpan();
        LineSpan startLine = tokenReader.GetCurrentLineSpan();

        CommandParameterSyntax commandParameter = new CommandParameterSyntax(tokenReader.CurrentToken.GetNodeKind());
        commandParameter.AddChildToken(tokenReader.CurrentToken);
        switch (tokenReader.CurrentToken.Kind) {
            case SyntaxTokenKind.SourceParameter:
            case SyntaxTokenKind.TargetParameter:
            case SyntaxTokenKind.TypeParameter:
                CommandParameterAssignmentSyntax? commandParameterAssignment = BuildCommandParameterAssignment();
                if (commandParameterAssignment is null)
                    return null;

                commandParameter.AddChildNode(commandParameterAssignment);
                break;

            case SyntaxTokenKind.ModifiedOnlyParameter:
                break;
            default:
                return null;
        }

        TextSpan end = tokenReader.GetCurrentFullSpan();
        LineSpan endLine = tokenReader.GetCurrentLineSpan();
        SetSpans(commandParameter, start, end, startLine, endLine);

        return commandParameter;
    }

    private CommandParameterAssignmentSyntax? BuildCommandParameterAssignment() {
        CommandParameterAssignmentSyntax commandParameterAssignment = new CommandParameterAssignmentSyntax(SyntaxNodeKind.CommandParameterAssignment);
        TextSpan start = tokenReader.GetCurrentFullSpan();
        LineSpan startLine = tokenReader.GetCurrentLineSpan();

        SyntaxToken? nextToken = tokenReader.GetNextToken();

        if (!nextToken?.IsKind(SyntaxTokenKind.Equals) ?? true) {
            AddSyntaxErrorWithCurrentSpan("'=' expected");
            return null;
        }

        ArgumentListSyntax? argumentList = BuildArgumentList();
        if (argumentList is null)
            return null;

        commandParameterAssignment.AddChildNode(argumentList);
        TextSpan end = tokenReader.GetCurrentFullSpan();
        LineSpan endLine = tokenReader.GetCurrentLineSpan();
        SetSpans(commandParameterAssignment, start, end, startLine, endLine);

        return commandParameterAssignment;
    }

    private ArgumentListSyntax? BuildArgumentList() {
        SyntaxToken? nextToken = tokenReader.GetNextToken();
        if (nextToken is null)
            return null;

        TextSpan start = tokenReader.GetCurrentFullSpan();
        LineSpan startLine = tokenReader.GetCurrentLineSpan();
        
        ArgumentListSyntax argumentList = new ArgumentListSyntax(SyntaxNodeKind.ArgumentList);

        if (!nextToken.Value.IsKind(SyntaxTokenKind.OpenParenthesis)) {
            ArgumentSyntax? argument = BuildArgument();
            if (argument is null)
                return null;

            argumentList.AddChildNode(argument);
            return argumentList;
        }

        argumentList.AddChildToken(tokenReader.CurrentToken);
        tokenReader.MoveNext();
        while (tokenReader.CanMoveNext() && tokenReader.CurrentToken.Kind.IsArgumentTokenKind()) {

            ArgumentSyntax? argument = BuildArgument();
            if (argument is null)
                return null;

            argumentList.AddChildNode(argument);
            nextToken = tokenReader.GetNextToken();

            if (tokenReader.PeekNextToken().Kind.IsArgumentTokenKind()) {

                if (!nextToken?.IsKind(SyntaxTokenKind.Comma) ?? true) {
                    AddSyntaxErrorWithCurrentSpan("',' expected");
                    return null;
                }

                argumentList.AddChildToken(tokenReader.CurrentToken);
                tokenReader.MoveNext();
            }
            else if (!nextToken?.IsKind(SyntaxTokenKind.CloseParenthesis) ?? true) {
                AddSyntaxErrorWithCurrentSpan("')' expected");
                return null;
            }
        }

        argumentList.AddChildToken(nextToken!.Value);

        SetSpans(argumentList, start, nextToken.Value.FullSpan, startLine, nextToken.Value.LineSpan);
        return argumentList;
    }

    private ArgumentSyntax? BuildArgument() {
        TextSpan start = tokenReader.GetCurrentFullSpan();
        LineSpan startLine = tokenReader.GetCurrentLineSpan();

        ArgumentSyntax argument = new ArgumentSyntax(SyntaxNodeKind.Argument);
        LiteralExpressionSyntax? literalExpression = BuildLiteralExpression();
        if (literalExpression is null)
            return null;

        TextSpan end = tokenReader.GetCurrentFullSpan();
        LineSpan endLine = tokenReader.GetCurrentLineSpan();

        argument.AddChildNode(literalExpression);
        SetSpans(argument, start, end, startLine, endLine);
        return argument;
    }

    private LiteralExpressionSyntax? BuildLiteralExpression() {
        TextSpan start = tokenReader.GetCurrentFullSpan();
        LineSpan startLine = tokenReader.GetCurrentLineSpan();

        LiteralExpressionSyntax? literalExpression = tokenReader.CurrentToken.Kind switch {
            SyntaxTokenKind.NumericLiteral or
            SyntaxTokenKind.StringLiteral => new LiteralExpressionSyntax(tokenReader.CurrentToken.GetNodeKind()),
            _ => null
        };

        if (literalExpression is null) {
            AddSyntaxErrorWithCurrentSpan("Literal expression [strings, numbers, ..] expected");
            return null;
        }

        literalExpression.AddChildToken(tokenReader.CurrentToken);
        TextSpan end = tokenReader.GetCurrentFullSpan();
        LineSpan endLine = tokenReader.GetCurrentLineSpan();
        SetSpans(literalExpression, start, end, startLine, endLine);

        return literalExpression;
    }
    #endregion

    #region Helper
    private static TextSpan GetTextSpan(TextSpan start, TextSpan end) => new TextSpan(start.Start, end.End - start.Start);
    private static LineSpan GetLineSpan(LineSpan start, LineSpan end) 
        => new LineSpan(start.StartLine, end.EndLine - start.StartLine, GetTextSpan(start.Span, end.Span));

    private void AddSyntaxErrorWithCurrentSpan(string message) {
        syntaxErrors.Add(new SimpleError(
                tokenReader.CurrentToken.FullSpan,
                tokenReader.CurrentToken.LineSpan,
                message
            ));
    }

    private static void SetSpans(SyntaxNode node, TextSpan start, TextSpan end, LineSpan startLine, LineSpan endLine) {
        node.Span = GetTextSpan(start, end);
        node.LineSpan = GetLineSpan(startLine, endLine);
    }
    #endregion
}
