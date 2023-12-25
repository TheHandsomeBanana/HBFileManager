using HB.Code.Interpreter;
using HB.Code.Interpreter.Syntax;

namespace FileManager.Core.Interpreter.Syntax;
public class SyntaxNode : ISyntaxNode<SyntaxNode, SyntaxToken> {
    ISyntaxNode? ISyntaxNode.Parent => Parent;
    IReadOnlyList<ISyntaxNode> ISyntaxNode.ChildNodes => ChildNodes;
    IReadOnlyList<ISyntaxToken> ISyntaxNode.ChildTokens => ChildTokens.Cast<ISyntaxToken>().ToList();

    public TextSpan Span { get; }
    public SyntaxNode? Parent { get; }

    private readonly List<SyntaxNode> childNodes = [];
    public IReadOnlyList<SyntaxNode> ChildNodes => childNodes;

    private readonly List<SyntaxToken> childTokens = [];
    public IReadOnlyList<SyntaxToken> ChildTokens => childTokens;

    public SyntaxNode(TextSpan span, SyntaxNode? parent = null) {
        this.Span = span;
        this.Parent = parent;
    }

    public void AddChildNode(SyntaxNode node) {
        childNodes.Add(node);
    }

    public void AddChildNode(ISyntaxNode node) {
        if (node is not SyntaxNode fmSyntaxNode)
            throw new NotImplementedException();

        AddChildNode(fmSyntaxNode);
    }

    public void AddChildToken(SyntaxToken token) {
        childTokens.Add(token);
    }

    public void AddChildToken(ISyntaxToken token) {
        if (token is not SyntaxToken fmSyntaxToken)
            throw new NotImplementedException();

        AddChildToken(fmSyntaxToken);
    }

    public SyntaxNode GetRoot() {
        SyntaxNode temp = this;
        while(temp.Parent != null)
            temp = temp.Parent;

        return temp;
    }
}
