using System.Diagnostics.CodeAnalysis;

namespace BymlLibrary.Nodes.Containers;

[method: SetsRequiredMembers]
public readonly struct BymlArrayChangelogEntry(int index, BymlChangeType change, Byml node, Byml? keyPrimary = null, Byml? keySecondary = null)
{
    public int Index { get; init; } = index;

    public BymlChangeType Change { get; init; } = change;

    public required Byml Node { get; init; } = node;

    public Byml? KeyPrimary { get; init; } = keyPrimary;

    public Byml? KeySecondary { get; init; } = keySecondary;

    public static implicit operator BymlArrayChangelogEntry((int index, BymlChangeType change, Byml node, Byml? keyPrimary, Byml? keySecondary) src) => new() {
        Index = src.index,
        Change = src.change,
        Node = src.node,
        KeyPrimary = src.keyPrimary,
        KeySecondary = src.keySecondary,
    };

    public static implicit operator BymlArrayChangelogEntry((int index, BymlChangeType change, Byml node) src) => new() {
        Index = src.index,
        Change = src.change,
        Node = src.node
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