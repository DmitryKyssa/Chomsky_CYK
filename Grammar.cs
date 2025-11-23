namespace Chomsky_CYK
{
    internal class Grammar
    {
        public Dictionary<char, List<string>> Rules { get; private set; }

        public Grammar()
        {
            Rules = new Dictionary<char, List<string>>();
        }

        public void AddRule(char nonTerminal, string production)
        {
            if (!Rules.TryGetValue(nonTerminal, out List<string>? value))
            {
                value = new List<string>();
                Rules[nonTerminal] = value;
            }

            value.Add(production);
        }

        public void RemoveRule(char nonTerminal, string production)
        {
            if (Rules.TryGetValue(nonTerminal, out List<string>? value))
            {
                value.Remove(production);
            }
        }

        public Grammar Clone()
        {
            Grammar clone = new Grammar();
            foreach (KeyValuePair<char, List<string>> kvp in Rules)
            {
                clone.Rules[kvp.Key] = [.. kvp.Value];
            }
            return clone;
        }

        public void Print()
        {
            foreach (KeyValuePair<char, List<string>> kvp in Rules.OrderBy(r => r.Key))
            {
                Console.WriteLine($"{kvp.Key} -> {string.Join(" | ", kvp.Value)}");
            }
        }
    }
}