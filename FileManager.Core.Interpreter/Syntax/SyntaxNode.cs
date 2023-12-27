using HB.Code.Interpreter;
using HB.Code.Interpreter.Syntax;

namespace FileManager.Core.Interpreter.Syntax;
public class SyntaxNode : ISyntaxNode<SyntaxNode, SyntaxToken> {
    

    public TextSpan Span { get; }
    public SyntaxNode? Parent { get; private set; }
    public SyntaxNodeKind Kind { get; }

    private readonly List<SyntaxNode> childNodes = [];
    public IReadOnlyList<SyntaxNode> ChildNodes => childNodes;

    private readonly List<SyntaxToken> childTokens = [];
    public IReadOnlyList<SyntaxToken> ChildTokens => childTokens;

    public SyntaxNode(TextSpan span, SyntaxNodeKind kind) {
        this.Span = span;
        this.Kind = kind;
    }

    public void AddChildNode(SyntaxNode node) {
        node.Parent = this;
        childNodes.Add(node);
    }

    public void AddChildToken(SyntaxToken token) {
        childTokens.Add(token);
    }

    public SyntaxNode GetRoot() {
        SyntaxNode temp = this;
        while(temp.Parent != null)
            temp = temp.Parent;

        return temp;
    }

    #region Untyped
    ISyntaxNode? ISyntaxNode.Parent => Parent;
    IReadOnlyList<ISyntaxNode> ISyntaxNode.ChildNodes => ChildNodes;
    IReadOnlyList<ISyntaxToken> ISyntaxNode.ChildTokens => ChildTokens.Cast<ISyntaxToken>().ToList();
    public void AddChildToken(ISyntaxToken token) {
        if (token is not SyntaxToken fmSyntaxToken)
            throw new NotImplementedException();

        AddChildToken(fmSyntaxToken);
    }

    public void AddChildNode(ISyntaxNode node) {
        if (node is not SyntaxNode fmSyntaxNode)
            throw new NotImplementedException();

        AddChildNode(fmSyntaxNode);
    }
    #endregion
}
