using FileManager.Core.Interpreter.Syntax;
using HB.Code.Interpreter;
using HB.Code.Interpreter.Lexer.Default;
using HB.Code.Interpreter.Parser;
using HB.Code.Interpreter.Parser.Default;
using HB.Code.Interpreter.Syntax;
using System.Collections.Immutable;

namespace FileManager.Core.Interpreter.Parser;
public class FMParser : IParser<SyntaxTree, SyntaxToken, DefaultSyntaxError> {
    private readonly List<DefaultSyntaxError> syntaxErrors = [];
    private readonly DefaultTokenReader<SyntaxToken, SyntaxTokenKind> tokenReader = new();

    public SyntaxTree Parse(ImmutableArray<SyntaxToken> tokens) {
        syntaxErrors.Clear();
        tokenReader.Init(tokens);

        SyntaxNode root = new InterpreterUnitSyntax(SyntaxNodeKind.InterpreterUnit);
        root.AddChildToken(tokens.Last());
        root.Span = new TextSpan(tokens.First().FullSpan.Start, tokens.Last().FullSpan.End);

        while (tokenReader.CanMoveNext()) {
            SyntaxToken? startToken = tokenReader.CurrentToken;
            if (startToken is null)
                break;

            SyntaxNode? nextNode = BuildNextNode(tokenReader.CurrentToken);

            if (nextNode != null)
                root.AddChildNode(nextNode);
            else {
                SyntaxToken? endToken = tokenReader.CurrentToken;
                endToken ??= tokenReader.GetLastValidToken();
                syntaxErrors.Add(new DefaultSyntaxError(GetTextSpan(startToken.Value, endToken.Value)));
            }

            tokenReader.MoveNext();
        }

        return new SyntaxTree(root);
    }

    public ImmutableArray<DefaultSyntaxError> GetSyntaxErrors() => syntaxErrors.ToImmutableArray();

    #region Parsing
    // Build nodes that are direct childs from InterpreterUnitSyntax
    private SyntaxNode? BuildNextNode(SyntaxToken current) {
        switch (current.Kind) {
            case SyntaxTokenKind.BlockStart:
                return BuildCommandBlockSyntax();
            case SyntaxTokenKind.CopyKeyword:
                return BuildExpressionStatement();
            case SyntaxTokenKind.MoveKeyword:
                return BuildExpressionStatement();
            case SyntaxTokenKind.ReplaceKeyword:
                return BuildExpressionStatement();
            case SyntaxTokenKind.ToKeyword:
                return BuildExpressionStatement();
        }

        return null;
    }

    private CommandBlockSyntax? BuildCommandBlockSyntax() {
        CommandBlockSyntax commandBlock = new CommandBlockSyntax(SyntaxNodeKind.CommandBlock);
        commandBlock.AddChildToken(tokenReader.CurrentToken);
        TextSpan? start = tokenReader.GetCurrentFullSpan();
        if (!start.HasValue)
            return null;

        SyntaxToken? nextToken = tokenReader.GetNextToken();

        while (nextToken != null && nextToken.Value.Kind.IsCommandTokenKind()) {
            ExpressionStatementSyntax? expressionStatement = BuildExpressionStatement();
            if (expressionStatement is null)
                return null;

            commandBlock.AddChildNode(expressionStatement);
            nextToken = tokenReader.GetNextToken();
        }

        SyntaxToken endToken = nextToken ?? tokenReader.GetLastValidToken();

        if (!endToken.IsKind(SyntaxTokenKind.BlockEnd))
            return null;

        commandBlock.AddChildToken(tokenReader.CurrentToken);
        commandBlock.Span = GetTextSpan(start.Value, endToken.FullSpan);
        return commandBlock;
    }

    private ExpressionStatementSyntax? BuildExpressionStatement() {
        TextSpan? start = tokenReader.GetCurrentFullSpan();
        ExpressionStatementSyntax expressionStatement = new ExpressionStatementSyntax(SyntaxNodeKind.ExpressionStatement);

        if (tokenReader.CurrentToken.Kind.IsCommandTokenKind()) {
            CommandExpressionSyntax? commandExpression = BuildCommandExpression();
            if (commandExpression is null)
                return null;

            expressionStatement.AddChildNode(commandExpression);
        }

        switch (tokenReader.GetNextToken().Kind) {
            case SyntaxTokenKind.ToKeyword:
                CommandExpressionSyntax? commandExpression = BuildCommandExpression();
                if (commandExpression is null)
                    return null;

                expressionStatement.AddChildNode(commandExpression);
                break;
            case SyntaxTokenKind.Semicolon:
                expressionStatement.AddChildToken(tokenReader.CurrentToken);
                break;
            default:
                return null;
        }

        TextSpan? end = tokenReader.GetCurrentFullSpan();
        expressionStatement.Span = new TextSpan(start!.Value.Start, end!.Value.End);
        return expressionStatement;
    }

    private CommandExpressionSyntax? BuildCommandExpression() {
        TextSpan? start = tokenReader.GetCurrentFullSpan();

        CommandExpressionSyntax commandExpression;
        switch (tokenReader.CurrentToken.Kind) {
            case SyntaxTokenKind.ReplaceKeyword:
                commandExpression = new CommandExpressionSyntax(SyntaxNodeKind.ReplaceCommandExpression);
                break;
            case SyntaxTokenKind.CopyKeyword:
                commandExpression = new CommandExpressionSyntax(SyntaxNodeKind.CopyCommandExpression);
                break;
            case SyntaxTokenKind.MoveKeyword:
                commandExpression = new CommandExpressionSyntax(SyntaxNodeKind.MoveCommandExpression);
                break;
            case SyntaxTokenKind.ToKeyword:
                commandExpression = new CommandExecutorExpressionSyntax(SyntaxNodeKind.ToCommandExpression);
                break;
            default:
                return null;
        }

        commandExpression.AddChildToken(tokenReader.CurrentToken);
        SyntaxToken? nextToken = tokenReader.GetNextToken();
        if (nextToken is null)
            return null;

        switch (nextToken.Value.Kind) {
            // No command modifiers
            case SyntaxTokenKind.StringLiteral:
                LiteralExpressionSyntax? literal = BuildLiteralExpression();
                if (literal is null)
                    return null;

                commandExpression.AddChildNode(literal);
                break;

            // Check command modifiers, and neighbouring literal
            case SyntaxTokenKind.FileModifierKeyword:
            case SyntaxTokenKind.DirectoryModifierKeyword:
            case SyntaxTokenKind.ModifiedOnlyModifierKeyword:
                CommandModifierListSyntax commandModifierList = BuildCommandModifierList();
                commandExpression.AddChildNode(commandModifierList);

                // CurrentToken is no CommandModifier at this point
                // StringLiteral is expected
                switch (tokenReader.CurrentToken.Kind) {
                    case SyntaxTokenKind.StringLiteral:
                        literal = BuildLiteralExpression();
                        if (literal is null)
                            return null;

                        commandExpression.AddChildNode(literal);
                        break;
                }
                break;
        }

        TextSpan? end = tokenReader.GetCurrentFullSpan();
        commandExpression.Span = new TextSpan(start!.Value.Start, end!.Value.End);
        return commandExpression;
    }


    private CommandModifierListSyntax BuildCommandModifierList() {
        TextSpan? start = tokenReader.GetCurrentFullSpan();

        CommandModifierListSyntax commandModifierList = new CommandModifierListSyntax(SyntaxNodeKind.CommandModifierList);

        while (tokenReader.CanMoveNext() && tokenReader.CurrentToken.Kind.IsCommandModifierTokenKind()) {
            commandModifierList.AddChildToken(tokenReader.CurrentToken);
            tokenReader.MoveNext();
        }

        TextSpan? end = tokenReader.GetCurrentFullSpan();
        commandModifierList.Span = new TextSpan(start!.Value.Start, end!.Value.End);
        return commandModifierList;
    }

    private LiteralExpressionSyntax? BuildLiteralExpression() {
        TextSpan? start = tokenReader.GetCurrentFullSpan();

        LiteralExpressionSyntax literalExpression;
        switch (tokenReader.CurrentToken.Kind) {
            case SyntaxTokenKind.NumericLiteral:
                literalExpression = new LiteralExpressionSyntax(SyntaxNodeKind.NumericLiteral);
                break;
            case SyntaxTokenKind.StringLiteral:
                literalExpression = new LiteralExpressionSyntax(SyntaxNodeKind.StringLiteral);
                break;
            default:
                return null;
        }

        literalExpression.AddChildToken(tokenReader.CurrentToken);
        TextSpan? end = tokenReader.GetCurrentFullSpan();
        literalExpression.Span = new TextSpan(start!.Value.Start, end!.Value.End);

        return literalExpression;
    }
    #endregion



    #region Helper
    private static TextSpan GetTextSpan(SyntaxToken start, SyntaxToken end) => new TextSpan(start.FullSpan.Start, end.FullSpan.End - start.FullSpan.Start);
    private static TextSpan GetTextSpan(TextSpan start, TextSpan end) => new TextSpan(start.Start, end.End - start.Start);
    #endregion
}
