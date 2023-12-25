using HB.Code.Interpreter;
using HB.Code.Interpreter.Syntax;

namespace FileManager.Core.Interpreter.Syntax;
public class FMSyntaxNode : ISyntaxNode<FMSyntaxNode, FMSyntaxToken> {
    ISyntaxNode? ISyntaxNode.Parent => Parent;
    IReadOnlyList<ISyntaxNode> ISyntaxNode.ChildNodes => ChildNodes;
    IReadOnlyList<ISyntaxToken> ISyntaxNode.ChildTokens => ChildTokens.Cast<ISyntaxToken>().ToList();

    public TextSpan Span { get; set; }
    public FMSyntaxNode? Parent { get; init; }

    private readonly List<FMSyntaxNode> childNodes = [];
    public IReadOnlyList<FMSyntaxNode> ChildNodes => childNodes;

    private readonly List<FMSyntaxToken> childTokens = [];
    public IReadOnlyList<FMSyntaxToken> ChildTokens => childTokens;



    public void AddChildNode(FMSyntaxNode node) {
        childNodes.Add(node);
    }

    public void AddChildNode(ISyntaxNode node) {
        if (node is not FMSyntaxNode fmSyntaxNode)
            throw new NotImplementedException();

        AddChildNode(fmSyntaxNode);
    }

    public void AddChildToken(FMSyntaxToken token) {
        childTokens.Add(token);
    }

    public void AddChildToken(ISyntaxToken token) {
        if (token is not FMSyntaxToken fmSyntaxToken)
            throw new NotImplementedException();

        AddChildToken(fmSyntaxToken);
    }
}
