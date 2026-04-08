using System.Buffers;
using System.Collections.Frozen;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace OmenTools.Info.Algorithms;

public class AhoCorasick
{
    private static readonly FrozenDictionary<char, int> EmptyTransitions =
        new Dictionary<char, int>().ToFrozenDictionary();

    private static readonly int[] EmptyOutputIDs = [];

    private readonly Node[]   states;
    private readonly string[] patterns;

    private readonly record struct Node
    (
        FrozenDictionary<char, int> Transitions,
        int                         FailureState,
        int[]                       OutputIDs
    );

    private sealed class BuilderNode
    {
        public Dictionary<char, int> Transitions { get; } = [];

        public int FailureState { get; set; }

        public int[] OutputIDs { get; set; } = EmptyOutputIDs;
    }

    public AhoCorasick(IEnumerable<string> patterns)
    {
        ArgumentNullException.ThrowIfNull(patterns);

        this.patterns = NormalizePatterns(patterns);
        states        = BuildStates(this.patterns);
    }

    public bool ContainsAny(string text) =>
        ContainsAny(text.AsSpan());

    public bool ContainsAny(ReadOnlySpan<char> text)
    {
        if (text.IsEmpty || patterns.Length == 0)
            return false;

        var state = 0;

        foreach (var c in text)
        {
            state = Advance(state, char.ToLowerInvariant(c));

            if (states[state].OutputIDs.Length != 0)
                return true;
        }

        return false;
    }

    public IEnumerable<string> FindAllMatches(string text) =>
        FindAllMatchesCore(text.AsSpan());

    private string[] FindAllMatchesCore(ReadOnlySpan<char> text)
    {
        if (text.IsEmpty || patterns.Length == 0)
            return [];

        byte[]? rentedBuffer = null;
        var seen = patterns.Length <= 256
                       ? stackalloc byte[patterns.Length]
                       : (rentedBuffer = ArrayPool<byte>.Shared.Rent(patterns.Length)).AsSpan(0, patterns.Length);

        seen.Clear();

        var matches = new List<string>(Math.Min(patterns.Length, 8));
        var state   = 0;

        try
        {
            foreach (var c in text)
            {
                state = Advance(state, char.ToLowerInvariant(c));

                var outputIDs = states[state].OutputIDs;
                if (outputIDs.Length == 0)
                    continue;

                foreach (var outputID in outputIDs)
                {
                    if (seen[outputID] != 0)
                        continue;

                    seen[outputID] = 1;
                    matches.Add(patterns[outputID]);
                }
            }

            return matches.Count == 0 ? [] : [.. matches];
        }
        finally
        {
            if (rentedBuffer != null)
                ArrayPool<byte>.Shared.Return(rentedBuffer, true);
        }
    }

    private static string[] NormalizePatterns(IEnumerable<string> source)
    {
        List<string>    normalizedPatterns = [];
        HashSet<string> deduped            = new(StringComparer.Ordinal);

        foreach (var pattern in source)
        {
            if (string.IsNullOrEmpty(pattern))
                continue;

            var normalizedPattern = pattern.ToLowerInvariant();
            if (normalizedPattern.Length == 0 || !deduped.Add(normalizedPattern))
                continue;

            normalizedPatterns.Add(normalizedPattern);
        }

        return [.. normalizedPatterns];
    }

    private static Node[] BuildStates(string[] patterns)
    {
        List<BuilderNode> builderStates = [new()];

        for (var patternID = 0; patternID < patterns.Length; patternID++)
            Insert(builderStates, patterns[patternID], patternID);

        BuildFailureLinks(builderStates);

        var frozenStates = new Node[builderStates.Count];

        for (var state = 0; state < builderStates.Count; state++)
        {
            var builderState = builderStates[state];
            var transitions  = builderState.Transitions.Count == 0 ? EmptyTransitions : builderState.Transitions.ToFrozenDictionary();

            frozenStates[state] = new(transitions, builderState.FailureState, builderState.OutputIDs);
        }

        return frozenStates;
    }

    private static void Insert(List<BuilderNode> states, string pattern, int patternID)
    {
        var state = 0;

        foreach (var c in pattern)
        {
            ref var nextState = ref CollectionsMarshal.GetValueRefOrAddDefault(states[state].Transitions, c, out var exists);

            if (!exists)
            {
                nextState = states.Count;
                states.Add(new());
            }

            state = nextState;
        }

        states[state].OutputIDs = [patternID];
    }

    private static void BuildFailureLinks(List<BuilderNode> states)
    {
        if (states.Count == 1)
            return;

        Queue<int> queue = new(states.Count - 1);

        foreach (var childState in states[0].Transitions.Values)
        {
            states[childState].FailureState = 0;
            queue.Enqueue(childState);
        }

        while (queue.Count > 0)
        {
            var state       = queue.Dequeue();
            var current     = states[state];
            var transitions = current.Transitions;

            foreach (var (c, nextState) in transitions)
            {
                var failureState = current.FailureState;

                while (failureState != 0 && !states[failureState].Transitions.TryGetValue(c, out _))
                    failureState = states[failureState].FailureState;

                states[nextState].FailureState =
                    states[failureState].Transitions.GetValueOrDefault(c, 0);

                var fallbackOutputs = states[states[nextState].FailureState].OutputIDs;
                if (fallbackOutputs.Length != 0)
                    states[nextState].OutputIDs = MergeSortedUnique(states[nextState].OutputIDs, fallbackOutputs);

                queue.Enqueue(nextState);
            }
        }
    }

    private static int[] MergeSortedUnique(int[] current, int[] fallback)
    {
        if (current.Length == 0)
            return fallback;

        if (fallback.Length == 0)
            return current;

        var merged  = new int[current.Length + fallback.Length];
        var i       = 0;
        var j       = 0;
        var count   = 0;
        var hasLast = false;
        var last    = 0;

        while (i < current.Length || j < fallback.Length)
        {
            int candidate;

            if (j >= fallback.Length || i < current.Length && current[i] < fallback[j])
            {
                candidate = current[i];
                i++;
            }
            else if (i >= current.Length || fallback[j] < current[i])
            {
                candidate = fallback[j];
                j++;
            }
            else
            {
                candidate = current[i];
                i++;
                j++;
            }

            if (hasLast && candidate == last)
                continue;

            merged[count] = candidate;
            last          = candidate;
            hasLast       = true;
            count++;
        }

        return count == merged.Length ? merged : merged[..count];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int Advance(int state, char c)
    {
        while (true)
        {
            if (states[state].Transitions.TryGetValue(c, out var nextState))
                return nextState;

            if (state == 0)
                return 0;

            state = states[state].FailureState;
        }
    }
}
