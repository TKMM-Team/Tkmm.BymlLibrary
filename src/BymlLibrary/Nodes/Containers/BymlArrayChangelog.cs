using System.Runtime.CompilerServices;
using BymlLibrary.Extensions;
using BymlLibrary.Writers;
using BymlLibrary.Yaml;
using VYaml.Emitter;

namespace BymlLibrary.Nodes.Containers;

public class BymlArrayChangelog : List<(int, BymlChangeType, Byml)>, IBymlNode
{
    public BymlArrayChangelog()
    {
    }

    public BymlArrayChangelog(IEnumerable<(int, BymlChangeType, Byml)> values) : base(values)
    {
    }

    public void EmitYaml(ref Utf8YamlEmitter emitter)
    {
        emitter.Tag("!array_changelog");
        emitter.BeginMapping();

        foreach ((int index, BymlChangeType change, Byml node) in this) {
            emitter.WriteInt32(index);
            emitter.BeginMapping(MappingStyle.Flow);
            {
                emitter.WriteString(change.ToString());
                BymlYamlWriter.Write(ref emitter, node);
            }
            emitter.EndMapping();
        }

        emitter.EndMapping();
    }

    public bool HasContainerNodes()
    {
        foreach ((_, _, Byml node) in this) {
            if (node.Type.IsContainerType()) {
                return true;
            }
        }

        return false;
    }

    public int GetValueHash()
    {
        HashCode hashCode = new();
        foreach ((int index, BymlChangeType change, Byml node) in this) {
            hashCode.Add(index);
            hashCode.Add(change);
            hashCode.Add(Byml.ValueEqualityComparer.Default.GetHashCode(node));
        }

        return hashCode.ToHashCode();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    int IBymlNode.Collect(in BymlWriter writer)
    {
        HashCode hashCode = new();
        foreach ((int index, BymlChangeType change, Byml node) in this) {
            hashCode.Add(index);
            hashCode.Add(change);
            hashCode.Add(writer.Collect(node));
        }

        return hashCode.ToHashCode();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    void IBymlNode.Write(BymlWriter context, Action<Byml> write)
    {
        context.WriteContainerHeader(BymlNodeType.ArrayChangelog, Count);
        foreach ((int index, BymlChangeType change, Byml node) in this) {
            context.Writer.Write(index);
            context.Writer.Write(change);
            write(node);
        }

        foreach ((_, _, Byml node) in this) {
            context.Writer.Write(node.Type);
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

    private class EntryValueEqualityComparer : IEqualityComparer<(int Index, BymlChangeType Change, Byml Node)>
    {
        public static readonly EntryValueEqualityComparer Default = new();

        public bool Equals((int Index, BymlChangeType Change, Byml Node) x, (int Index, BymlChangeType Change, Byml Node) y)
        {
            return x.Change == y.Change && Byml.ValueEqualityComparer.Default.Equals(x.Node, y.Node);
        }

        public int GetHashCode((int, BymlChangeType, Byml) obj)
        {
            throw new NotImplementedException();
        }
    }
}
