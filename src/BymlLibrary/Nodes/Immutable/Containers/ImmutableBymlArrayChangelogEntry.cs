namespace BymlLibrary.Nodes.Immutable.Containers;

public readonly ref struct ImmutableBymlArrayChangelogEntry(
    int index, BymlChangeType change, Span<byte> data,
    int node, BymlNodeType type,
    int keyPrimaryNode, BymlNodeType keyPrimaryType,
    int keySecondaryNode, BymlNodeType keySecondaryType)
{
    public readonly int Index = index;
    public readonly BymlChangeType Change = change;
    public readonly ImmutableByml Node = new(data, node, type);
    public readonly ImmutableByml KeyPrimary = new(data, keyPrimaryNode, keyPrimaryType);
    public readonly ImmutableByml KeySecondary = new(data, keySecondaryNode, keySecondaryType);

    public void Deconstruct(out int index, out BymlChangeType change, out ImmutableByml node, out ImmutableByml keyPrimary, out ImmutableByml keySecondary)
    {
        index = Index;
        change = Change;
        node = Node;
        keyPrimary = KeyPrimary;
        keySecondary = KeySecondary;
    }
}
