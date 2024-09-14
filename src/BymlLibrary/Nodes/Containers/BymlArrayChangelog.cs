using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using BymlLibrary.Extensions;
using BymlLibrary.Writers;
using BymlLibrary.Yaml;
using VYaml.Emitter;

namespace BymlLibrary.Nodes.Containers;

public class BymlArrayChangelog : SortedDictionary<int, (BymlChangeType, Byml)>, IBymlNode
{
    public BymlArrayChangelog()
    {
    }

    public BymlArrayChangelog(IDictionary<int, (BymlChangeType, Byml)> values) : base(values)
    {
    }

    public void EmitYaml(ref Utf8YamlEmitter emitter)
    {
        emitter.Tag("!array_changelog");
        emitter.BeginMapping();

        foreach (var (hash, (change, node)) in this) {
            emitter.WriteInt32(hash);
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
        foreach (var (_, (_, node)) in this) {
            if (node.Type.IsContainerType()) {
                return true;
            }
        }

        return false;
    }

    public int GetValueHash()
    {
        HashCode hashCode = new();
        foreach (var (key, (change, node)) in this) {
            hashCode.Add(key);
            hashCode.Add(change);
            hashCode.Add(Byml.ValueEqualityComparer.Default.GetHashCode(node));
        }

        return hashCode.ToHashCode();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    int IBymlNode.Collect(in BymlWriter writer)
    {
        HashCode hashCode = new();
        foreach (var (key, (change, node)) in this) {
            hashCode.Add(key);
            hashCode.Add(change);
            hashCode.Add(writer.Collect(node));
        }

        return hashCode.ToHashCode();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    void IBymlNode.Write(BymlWriter context, Action<Byml> write)
    {
        context.WriteContainerHeader(BymlNodeType.ArrayChangelog, Count);
        foreach (var (key, (change, node)) in this) {
            context.Writer.Write(key);
            context.Writer.Write(change);
            write(node);
        }

        foreach ((_, Byml node) in Values) {
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

            if (x.Count != y.Count) {
                return false;
            }

            return x.Keys.SequenceEqual(y.Keys) && x.Values.SequenceEqual(y.Values, EntryValueEqualityComparer.Default);
        }

        public int GetHashCode([DisallowNull] BymlArrayChangelog obj)
        {
            throw new NotImplementedException();
        }
    }

    private class EntryValueEqualityComparer : IEqualityComparer<(BymlChangeType Change, Byml Node)>
    {
        public static readonly EntryValueEqualityComparer Default = new();

        public bool Equals((BymlChangeType Change, Byml Node) x, (BymlChangeType Change, Byml Node) y)
        {
            return x.Change == y.Change && Byml.ValueEqualityComparer.Default.Equals(x.Node, y.Node);
        }

        public int GetHashCode([DisallowNull] (BymlChangeType, Byml) obj)
        {
            throw new NotImplementedException();
        }
    }
}
