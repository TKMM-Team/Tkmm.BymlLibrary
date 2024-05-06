namespace BymlLibrary.Nodes.Immutable.Containers;

public readonly ref struct ImmutableBymlArrayChangelogEntry(int index, BymlChangeType change, Span<byte> data, int value, BymlNodeType type)
{
    public readonly int Index = index;
    public readonly BymlChangeType Change = change;
    public readonly ImmutableByml Node = new(data, value, type);

    public void Deconstruct(out int index, out BymlChangeType change, out ImmutableByml node)
    {
        index = Index;
        change = Change;
        node = Node;
    }
}
