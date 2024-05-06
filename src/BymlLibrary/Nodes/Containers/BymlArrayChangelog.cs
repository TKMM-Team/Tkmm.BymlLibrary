using BymlLibrary.Extensions;
using BymlLibrary.Writers;
using BymlLibrary.Yaml;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using VYaml.Emitter;

namespace BymlLibrary.Nodes.Containers;

public class BymlArrayChangelog : SortedDictionary<int, Byml>, IBymlNode
{
    public BymlArrayChangelog()
    {
    }

    public BymlArrayChangelog(IDictionary<int, Byml> values) : base(values)
    {
    }

    public void EmitYaml(ref Utf8YamlEmitter emitter)
    {
        emitter.Tag("!array_changelog");
        emitter.BeginMapping((Count < Byml.YamlConfig.InlineContainerMaxCount && !HasContainerNodes()) switch {
            true => MappingStyle.Flow,
            false => MappingStyle.Block,
        });

        foreach (var (hash, node) in this) {
            emitter.WriteInt32(hash);
            BymlYamlWriter.Write(ref emitter, node);
        }

        emitter.EndMapping();
    }

    public bool HasContainerNodes()
    {
        foreach (var (_, node) in this) {
            if (node.Type.IsContainerType()) {
                return true;
            }
        }

        return false;
    }

    public int GetValueHash()
    {
        HashCode hashCode = new();
        foreach (var (key, node) in this) {
            hashCode.Add(key);
            hashCode.Add(Byml.ValueEqualityComparer.Default.GetHashCode(node));
        }

        return hashCode.ToHashCode();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    int IBymlNode.Collect(in BymlWriter writer)
    {
        HashCode hashCode = new();
        foreach (var (key, node) in this) {
            hashCode.Add(key);
            hashCode.Add(writer.Collect(node));
        }

        return hashCode.ToHashCode();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    void IBymlNode.Write(BymlWriter context, Action<Byml> write)
    {
        context.WriteContainerHeader(BymlNodeType.ArrayChangelog, Count);
        foreach (var (key, node) in this) {
            context.Writer.Write(key);
            write(node);
        }

        foreach (Byml node in Values) {
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

            return x.Keys.SequenceEqual(y.Keys) && x.Values.SequenceEqual(y.Values, Byml.ValueEqualityComparer.Default);
        }

        public int GetHashCode([DisallowNull] BymlArrayChangelog obj)
        {
            throw new NotImplementedException();
        }
    }
}
