using System.Runtime.CompilerServices;
using BymlLibrary.Writers;
using BymlLibrary.Yaml;
using VYaml.Emitter;

namespace BymlLibrary.Nodes.Containers;

public class BymlArrayChangelog : List<BymlArrayChangelogEntry>, IBymlNode
{
    public BymlArrayChangelog()
    {
    }

    public BymlArrayChangelog(IEnumerable<BymlArrayChangelogEntry> values) : base(values)
    {
    }

    public void EmitYaml(ref Utf8YamlEmitter emitter)
    {
        emitter.Tag("!array-changelog");
        emitter.BeginMapping();

        foreach ((int index, BymlChangeType change, Byml node, Byml? keyPrimary, Byml? keySecondary) in this) {
            emitter.WriteInt32(index);
            emitter.BeginMapping();
            {
                if (keyPrimary is not null) {
                    emitter.WriteString("Key");
                    emitter.BeginSequence(SequenceStyle.Flow);
                    {
                        BymlYamlWriter.Write(ref emitter, keyPrimary);

                        if (keySecondary is not null) {
                            BymlYamlWriter.Write(ref emitter, keySecondary);
                        }
                    }
                    emitter.EndSequence();
                }
                
                emitter.WriteString(change.ToString());
                BymlYamlWriter.Write(ref emitter, node);
            }
            emitter.EndMapping();
        }

        emitter.EndMapping();
    }

    public int GetValueHash()
    {
        HashCode hashCode = new();
        foreach ((int index, BymlChangeType change, Byml node, Byml? keyPrimary, Byml? keySecondary) in this) {
            hashCode.Add(index);
            hashCode.Add(change);
            hashCode.Add(Byml.ValueEqualityComparer.Default.GetHashCode(node));
            hashCode.Add(keyPrimary is not null
                ? Byml.ValueEqualityComparer.Default.GetHashCode(keyPrimary) : 0);
            hashCode.Add(keySecondary is not null
                ? Byml.ValueEqualityComparer.Default.GetHashCode(keySecondary) : 0);
        }

        return hashCode.ToHashCode();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    int IBymlNode.Collect(in BymlWriter writer)
    {
        HashCode hashCode = new();
        foreach ((int index, BymlChangeType change, Byml node, Byml? keyPrimary, Byml? keySecondary) in this) {
            hashCode.Add(index);
            hashCode.Add(change);
            hashCode.Add(writer.Collect(node));
            hashCode.Add(keyPrimary is not null
                ? writer.Collect(keyPrimary) : 0);
            hashCode.Add(keySecondary is not null
                ? writer.Collect(keySecondary) : 0);
        }

        return hashCode.ToHashCode();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    void IBymlNode.Write(BymlWriter context, Action<Byml> write)
    {
        Byml emptyNode = new();
        
        context.WriteContainerHeader(BymlNodeType.ArrayChangelog, Count);
        foreach ((int index, BymlChangeType change, Byml node, Byml? keyPrimary, Byml? keySecondary) in this) {
            context.Writer.Write(index);
            context.Writer.Write(change);
            write(node);
            write(keyPrimary ?? emptyNode);
            write(keySecondary ?? emptyNode);
        }

        foreach ((_, _, Byml node, Byml? keyPrimary, Byml? keySecondary) in this) {
            context.Writer.Write(node.Type);
            context.Writer.Write(keyPrimary?.Type ?? BymlNodeType.None);
            context.Writer.Write(keySecondary?.Type ?? BymlNodeType.None);
            context.Writer.Write(BymlNodeType.None); // Unused
        }

        context.Writer.Align(4);
    }

    public class ValueEqualityComparer : IEqualityComparer<BymlArrayChangelog>
    {
        public bool Equals(BymlArrayChangelog? x, BymlArrayChangelog? y)
        {
            if (x is null || y is null) {
                return y == x;
            }

            return x.Count == y.Count && x.SequenceEqual(y, EntryValueEqualityComparer.Default);
        }

        public int GetHashCode(BymlArrayChangelog obj)
        {
            throw new NotImplementedException();
        }
    }

    private class EntryValueEqualityComparer : IEqualityComparer<BymlArrayChangelogEntry>
    {
        public static readonly EntryValueEqualityComparer Default = new();

        public bool Equals(BymlArrayChangelogEntry x, BymlArrayChangelogEntry y)
        {
            return x.Change == y.Change && Byml.ValueEqualityComparer.Default.Equals(x.Node, y.Node);
        }

        public int GetHashCode(BymlArrayChangelogEntry obj)
        {
            throw new NotImplementedException();
        }
    }
}
