namespace BymlLibrary.Nodes.Containers;

public readonly struct BymlArrayChangelogEntry
{
    public int Index { get; init; }

    public BymlChangeType Change { get; init; }

    public required Byml Node { get; init; }

    public Byml? KeyPrimary { get; init; }

    public Byml? KeySecondary { get; init; }

    public static implicit operator BymlArrayChangelogEntry((int index, BymlChangeType change, Byml node, Byml? keyPrimary, Byml? keySecondary) x) => new() {
        Index = x.index,
        Change = x.change,
        Node = x.node,
        KeyPrimary = x.keyPrimary,
        KeySecondary = x.keySecondary,
    };

    public void Deconstruct(out int index, out BymlChangeType change, out Byml node, out Byml? keyPrimary, out Byml? keySecondary)
    {
        index = Index;
        change = Change;
        node = Node;
        keyPrimary = KeyPrimary;
        keySecondary = KeySecondary;
    }
}