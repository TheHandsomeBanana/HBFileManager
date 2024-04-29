using FileManager.Core.Interpreter.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileManager.Core.Interpreter.Exceptions;
public class SyntaxBuilderException : Exception {
    public SyntaxBuilderException(string? message) : base(message) {
    }

    public SyntaxBuilderException(string? message, Exception? innerException) : base(message, innerException) {
    }

    public static SyntaxBuilderException NodeNotTypeOf(SyntaxNode node, string nodeType)
        => new SyntaxBuilderException($"{node.GetType()} is not of type {nodeType}");

    public static void ThrowMaxTokenLength(string nodeType)
        => throw new SyntaxBuilderException($"Max child node count reached for {nodeType}");
    public static void ThrowMaxNodeLength(string nodeType)
        => throw new SyntaxBuilderException($"Max child token count reached for {nodeType}");
}
