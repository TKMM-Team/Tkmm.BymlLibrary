﻿using BymlLibrary.Extensions;
using BymlLibrary.Writers;
using BymlLibrary.Yaml;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using LiteYaml.Emitter;

namespace BymlLibrary.Nodes.Containers;

public class BymlArray : List<Byml>, IBymlNode
{
    public BymlArray()
    {
    }

    public BymlArray(IEnumerable<Byml> array) : base(array)
    {
    }

    public BymlArray(int capacity) : base(capacity)
    {
    }

    public void EmitYaml(ref Utf8YamlEmitter emitter)
    {
        emitter.BeginSequence((Count < Byml.YamlConfig.InlineContainerMaxCount && !HasContainerNodes()) switch {
            true => SequenceStyle.Flow,
            false => SequenceStyle.Block,
        });

        foreach (Byml node in this) {
            BymlYamlWriter.Write(ref emitter, node);
        }

        emitter.EndSequence();
    }

    public int GetValueHash()
    {
        HashCode hashCode = new();
        foreach (var node in this) {
            hashCode.Add(Byml.ValueEqualityComparer.Default.GetHashCode(node));
        }

        return hashCode.ToHashCode();
    }

    public bool HasContainerNodes()
    {
        foreach (var node in this) {
            if (node.Type.IsContainerType()) {
                return true;
            }
        }

        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    int IBymlNode.Collect(in BymlWriter writer)
    {
        HashCode hashCode = new();
        foreach (var node in this) {
            hashCode.Add(writer.Collect(node));
        }

        return hashCode.ToHashCode();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    void IBymlNode.Write(BymlWriter context, Action<Byml> write)
    {
        context.WriteContainerHeader(BymlNodeType.Array, Count);
        foreach (var node in this) {
            context.Writer.Write(node.Type);
        }

        context.Writer.Align(4);

        foreach (var node in this) {
            write(node);
        }
    }

    public class ValueEqualityComparer : IEqualityComparer<BymlArray>
    {
        public bool Equals(BymlArray? x, BymlArray? y)
        {
            if (x is null || y is null) {
                return y == x;
            }

            if (x.Count != y.Count) {
                return false;
            }

            return x.SequenceEqual(y, Byml.ValueEqualityComparer.Default);
        }

        public int GetHashCode([DisallowNull] BymlArray obj)
        {
            throw new NotImplementedException();
        }
    }
}
