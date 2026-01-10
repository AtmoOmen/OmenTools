namespace OmenTools.Infos;

public class AhoCorasick
{
    private class Node
    {
        public readonly Dictionary<char, Node> Children = [];
        public readonly HashSet<string>        Outputs  = [];
        public Node FailureLink { get; set; } = null!;
    }

    private readonly Node root = new();

    public AhoCorasick(IEnumerable<string> patterns)
    {
        foreach (var pattern in patterns) 
            Insert(pattern);
        BuildFailureFunction();
    }

    private void Insert(string pattern)
    {
        var current = root;
        foreach (var c in pattern)
        {
            if (!current.Children.ContainsKey(c)) 
                current.Children[c] = new();
            current = current.Children[c];
        }

        current.Outputs.Add(pattern.ToLowerInvariant());
    }

    private void BuildFailureFunction()
    {
        var queue = new Queue<Node>();
        foreach (var child in root.Children.Values)
        {
            child.FailureLink = root;
            queue.Enqueue(child);
        }

        while (queue.Count > 0)
        {
            var node = queue.Dequeue();
            foreach (var pair in node.Children)
            {
                var target = node.FailureLink;
                while (target != null && !target.Children.ContainsKey(pair.Key)) 
                    target = target.FailureLink;
                
                pair.Value.FailureLink = target != null ? target.Children[pair.Key] : root;

                pair.Value.Outputs.UnionWith(pair.Value.FailureLink.Outputs);
                queue.Enqueue(pair.Value);
            }
        }
    }

    public bool ContainsAny(string text)
    {
        var currentState = root;
        foreach (var c in text)
        {
            while (currentState != null && !currentState.Children.ContainsKey(c))
                currentState = currentState.FailureLink;
            currentState = currentState == null ? root : currentState.Children[c];

            if (currentState.Outputs.Count > 0) return true;
        }

        return false;
    }

    public IEnumerable<string> FindAllMatches(string text)
    {
        var currentState  = root;
        var foundPatterns = new List<string>();
        foreach (var c in text)
        {
            while (currentState != null && !currentState.Children.ContainsKey(c))
                currentState = currentState.FailureLink;
            
            currentState = currentState == null ? root : currentState.Children[c];

            foreach (var output in currentState.Outputs)
            {
                if (!foundPatterns.Contains(output))
                    foundPatterns.Add(output);
            }
        }

        return foundPatterns;
    }
}
