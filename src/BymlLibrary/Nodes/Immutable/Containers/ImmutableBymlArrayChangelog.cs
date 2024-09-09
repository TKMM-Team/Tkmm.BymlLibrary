using BymlLibrary.Extensions;
using BymlLibrary.Nodes.Containers;
using BymlLibrary.Structures;
using BymlLibrary.Yaml;
using Revrs;
using Revrs.Extensions;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using LiteYaml.Emitter;

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
    private readonly Span<BymlNodeType> _types = count == 0 ? []
        : data[(offset + BymlContainer.SIZE + (Entry.SIZE * count))..]
            .ReadSpan<BymlNodeType>(count);

    public readonly ImmutableBymlArrayChangelogEntry this[int index] {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get {
            Entry entry = _entries[index];
            return new(entry.Index, entry.Change, _data, entry.Value, _types[index]);
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 4, Size = SIZE)]
    private readonly struct Entry
    {
        public const int SIZE = 12;

        public readonly int Index;
        public readonly BymlChangeType Change;
        public readonly int Value;

        public class Reverser : IStructReverser
        {
            public static void Reverse(in Span<byte> slice)
            {
                slice[0..4].Reverse();
                slice[4..8].Reverse();
                slice[8..12].Reverse();
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly Enumerator GetEnumerator()
        => new(this);

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
            if (++_index >= _container.Count) {
                return false;
            }

            return true;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public BymlArrayChangelog ToMutable(in ImmutableByml root)
    {
        BymlArrayChangelog arrayChangelog = [];
        foreach (var (key, change, value) in this) {
            arrayChangelog[key] = (change, Byml.FromImmutable(value, root));
        }

        return arrayChangelog;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal unsafe void EmitYaml(ref Utf8YamlEmitter emitter, in ImmutableByml root)
    {
        emitter.SetTag("!array-changelog");
        emitter.BeginMapping();

        foreach (var (index, change, node) in this) {
            emitter.WriteInt32(index);
            emitter.BeginMapping();
            {
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
                offset + BymlContainer.SIZE + (Entry.SIZE * i)
            );

            ImmutableByml.ReverseNode(ref reader, entry.Value,
                reader.Read<BymlNodeType>(offset + BymlContainer.SIZE + (Entry.SIZE * count) + i),
                reversedOffsets
            );
        }
    }
}
