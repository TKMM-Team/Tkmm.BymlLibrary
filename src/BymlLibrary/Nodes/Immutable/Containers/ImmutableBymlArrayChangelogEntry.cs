namespace BymlLibrary.Nodes.Immutable.Containers;

public readonly ref struct ImmutableBymlArrayChangelogEntry(int index, Span<byte> data, int value, BymlNodeType type)
{
    public readonly int Index = index;
    public readonly ImmutableByml Node = new(data, value, type);

    public void Deconstruct(out int index, out ImmutableByml node)
    {
        index = Index;
        node = Node;
    }
}
