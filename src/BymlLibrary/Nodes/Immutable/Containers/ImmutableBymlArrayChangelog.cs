using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using BymlLibrary.Nodes.Containers;
using BymlLibrary.Structures;
using BymlLibrary.Yaml;
using Revrs;
using Revrs.Extensions;
using VYaml.Emitter;

namespace BymlLibrary.Nodes.Immutable.Containers;

public readonly ref struct ImmutableBymlArrayChangelog(Span<byte> data, int offset, int count)
{
    /// <summary>
    /// Span of the BYMl data
    /// </summary>
    private readonly Span<byte> _data = data;

    /// <summary>
    /// The container item count
    /// </summary>
    public readonly int Count = count;

    /// <summary>
    /// Container offset entries
    /// </summary>
    private readonly Span<Entry> _entries = count == 0 ? []
        : data[(offset + BymlContainer.SIZE)..]
            .ReadSpan<Entry>(count);

    /// <summary>
    /// Container entry types
    /// </summary>
    private readonly Span<TypeEntry> _types = count == 0 ? []
        : data[(offset + BymlContainer.SIZE + Entry.SIZE * count)..]
            .ReadSpan<TypeEntry>(count);

    public readonly ImmutableBymlArrayChangelogEntry this[int index] {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get {
            Entry entry = _entries[index];
            TypeEntry typeEntry = _types[index];
            return new ImmutableBymlArrayChangelogEntry(
                entry.Index, entry.Change, _data,
                entry.Node, typeEntry.Main,
                entry.KeyPrimary, typeEntry.KeyPrimary,
                entry.KeySecondary, typeEntry.KeySecondary
            );
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 4, Size = SIZE)]
    private readonly struct Entry
    {
        public const int SIZE = 20;

        public readonly int Index;
        public readonly BymlChangeType Change;
        public readonly int Node;
        public readonly int KeyPrimary;
        public readonly int KeySecondary;

        public class Reverser : IStructReverser
        {
            public static void Reverse(in Span<byte> slice)
            {
                slice[0..4].Reverse();
                slice[4..8].Reverse();
                slice[8..12].Reverse();
                slice[12..16].Reverse();
                slice[16..20].Reverse();
            }
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1, Size = SIZE)]
    private readonly struct TypeEntry
    {
        public const int SIZE = 4;

        public readonly BymlNodeType Main;
        public readonly BymlNodeType KeyPrimary;
        public readonly BymlNodeType KeySecondary;
        public readonly byte Unused;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Enumerator GetEnumerator() => new(this);

    [method: MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ref struct Enumerator(ImmutableBymlArrayChangelog container)
    {
        private readonly ImmutableBymlArrayChangelog _container = container;
        private int _index = -1;

        public readonly ImmutableBymlArrayChangelogEntry Current {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _container[_index];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool MoveNext()
        {
            return ++_index < _container.Count;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public BymlArrayChangelog ToMutable(in ImmutableByml root)
    {
        BymlArrayChangelog arrayChangelog = [];
        foreach ((int index, BymlChangeType change, ImmutableByml value, ImmutableByml keyPrimary, ImmutableByml keySecondary) in this) {
            arrayChangelog.Add(
                (index, change,
                    node: Byml.FromImmutable(value, root),
                    keyPrimary: Byml.FromImmutable(keyPrimary, root),
                    keySecondary: Byml.FromImmutable(keySecondary, root)
                )
            );
        }

        return arrayChangelog;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void EmitYaml(ref Utf8YamlEmitter emitter, in ImmutableByml root)
    {
        emitter.Tag("!array-changelog");
        emitter.BeginMapping();

        foreach ((int index, BymlChangeType change, ImmutableByml node, ImmutableByml keyPrimary, ImmutableByml keySecondary) in this) {
            emitter.WriteInt32(index);
            emitter.BeginMapping();
            {
                if (keyPrimary.Type is not BymlNodeType.None) {
                    emitter.WriteString("Key");
                    emitter.BeginSequence(SequenceStyle.Flow);
                    {
                        BymlYamlWriter.Write(ref emitter, keyPrimary, root);

                        if (keySecondary.Type is not BymlNodeType.None) {
                            BymlYamlWriter.Write(ref emitter, keySecondary, root);
                        }
                    }
                    emitter.EndSequence();
                }
                
                emitter.WriteString(change.ToString());
                BymlYamlWriter.Write(ref emitter, node, root);
            }
            emitter.EndMapping();
        }

        emitter.EndMapping();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Reverse(ref RevrsReader reader, int offset, int count, in HashSet<int> reversedOffsets)
    {
        for (int i = 0; i < count; i++) {
            Entry entry = reader.Read<Entry, Entry.Reverser>(
                offset + BymlContainer.SIZE + Entry.SIZE * i
            );
            
            var typeEntry = reader.Read<TypeEntry>(
                offset + BymlContainer.SIZE + Entry.SIZE * count + TypeEntry.SIZE * i
            );

            ImmutableByml.ReverseNode(ref reader,
                entry.Node, typeEntry.Main,
                reversedOffsets
            );
            
            ImmutableByml.ReverseNode(ref reader,
                entry.KeyPrimary, typeEntry.KeyPrimary,
                reversedOffsets
            );
            
            ImmutableByml.ReverseNode(ref reader,
                entry.KeySecondary, typeEntry.KeySecondary,
                reversedOffsets
            );
        }
    }
}