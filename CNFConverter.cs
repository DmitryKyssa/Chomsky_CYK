namespace Chomsky_CYK
{
    internal class CNFConverter
    {
        public Grammar CNFGrammar { get; private set; }
        public char StartSymbol { get; private set; }
        private int newNonTerminalCounter = 0;

        public CNFConverter(Grammar grammar, char startSymbol)
        {
            CNFGrammar = grammar.Clone();
            StartSymbol = startSymbol;
        }

        public void ConvertToCNF()
        {
            Console.WriteLine("\n--- Step 1: Eliminate ε-productions ---");
            EliminateEpsilonProductions();
            CNFGrammar.Print();

            Console.WriteLine("\n--- Step 2: Eliminate unit productions ---");
            EliminateUnitProductions();
            CNFGrammar.Print();

            Console.WriteLine("\n--- Step 3: Replace nonsolitary terminals ---");
            ReplaceNonsolitaryTerminals();
            CNFGrammar.Print();

            Console.WriteLine("\n--- Step 4: Break long productions ---");
            BreakLongProductions();
            CNFGrammar.Print();
        }

        private void EliminateEpsilonProductions()
        {
            HashSet<char> nullable = FindNullable();
            List<(char, string)> rulesToAdd = new List<(char, string)>();
            List<(char, string)> rulesToRemove = new List<(char, string)>();

            foreach (KeyValuePair<char, List<string>> kvp in CNFGrammar.Rules.ToList())
            {
                foreach (string? production in kvp.Value.ToList())
                {
                    if (production == "ε" || production == "")
                    {
                        rulesToRemove.Add((kvp.Key, production));
                    }
                    else
                    {
                        List<string> combinations = GenerateCombinations(production, nullable);
                        foreach (string combo in combinations)
                        {
                            if (!string.IsNullOrEmpty(combo) && combo != production)
                            {
                                rulesToAdd.Add((kvp.Key, combo));
                            }
                        }
                    }
                }
            }

            foreach ((char, string) rule in rulesToRemove)
            {
                CNFGrammar.RemoveRule(rule.Item1, rule.Item2);
            }

            foreach ((char, string) rule in rulesToAdd)
            {
                if (!CNFGrammar.Rules[rule.Item1].Contains(rule.Item2))
                {
                    CNFGrammar.AddRule(rule.Item1, rule.Item2);
                }
            }
        }

        private HashSet<char> FindNullable()
        {
            HashSet<char> nullable = [];
            bool changed = true;

            while (changed)
            {
                changed = false;
                foreach (KeyValuePair<char, List<string>> kvp in CNFGrammar.Rules)
                {
                    if (nullable.Contains(kvp.Key))
                    {
                        continue;
                    }

                    foreach (string production in kvp.Value)
                    {
                        if (production == "ε" || production == "" || production.All(nullable.Contains))
                        {
                            nullable.Add(kvp.Key);
                            changed = true;
                            break;
                        }
                    }
                }
            }

            return nullable;
        }

        private List<string> GenerateCombinations(string production, HashSet<char> nullable)
        {
            List<string> result = [production];

            for (int i = 0; i < production.Length; i++)
            {
                if (nullable.Contains(production[i]))
                {
                    List<string> newResults = new List<string>();
                    foreach (string str in result)
                    {
                        int count = 0;
                        for (int j = 0; j < str.Length; j++)
                        {
                            if (str[j] == production[i])
                            {
                                if (count == result.IndexOf(str))
                                {
                                    string newStr = str.Remove(j, 1);
                                    if (!newResults.Contains(newStr))
                                    {
                                        newResults.Add(newStr);
                                    }

                                    break;
                                }
                                count++;
                            }
                        }

                        for (int j = 0; j < str.Length; j++)
                        {
                            string newStr = str.Remove(j, 1);
                            if (str[j] == production[i] && !newResults.Contains(newStr))
                            {
                                newResults.Add(newStr);
                            }
                        }
                    }
                    result.AddRange(newResults);
                }
            }

            return [.. result.Distinct()];
        }

        private void EliminateUnitProductions()
        {
            bool changed = true;

            while (changed)
            {
                changed = false;
                List<(char, string)> rulesToAdd = [];
                List<(char, string)> rulesToRemove = [];

                foreach (KeyValuePair<char, List<string>> kvp in CNFGrammar.Rules.ToList())
                {
                    foreach (string? production in kvp.Value.ToList())
                    {
                        if (production.Length == 1 && char.IsUpper(production[0]))
                        {
                            char unitNonTerminal = production[0];

                            if (CNFGrammar.Rules.TryGetValue(unitNonTerminal, out List<string>? value))
                            {
                                foreach (string prod in value)
                                {
                                    if (!CNFGrammar.Rules[kvp.Key].Contains(prod))
                                    {
                                        rulesToAdd.Add((kvp.Key, prod));
                                        changed = true;
                                    }
                                }
                            }

                            rulesToRemove.Add((kvp.Key, production));
                        }
                    }
                }

                foreach ((char, string) rule in rulesToRemove)
                {
                    CNFGrammar.RemoveRule(rule.Item1, rule.Item2);
                }

                foreach ((char, string) rule in rulesToAdd)
                {
                    CNFGrammar.AddRule(rule.Item1, rule.Item2);
                }
            }
        }

        private void ReplaceNonsolitaryTerminals()
        {
            Dictionary<char, char> terminalMap = [];
            List<(char, string)> rulesToAdd = [];
            List<(char, string, string)> rulesToModify = [];

            foreach (KeyValuePair<char, List<string>> kvp in CNFGrammar.Rules.ToList())
            {
                foreach (string? production in kvp.Value.ToList())
                {
                    if (production.Length > 1)
                    {
                        string newProduction = production;

                        for (int i = 0; i < production.Length; i++)
                        {
                            char c = production[i];
                            if (!char.IsUpper(c))
                            {
                                if (!terminalMap.TryGetValue(c, out char value))
                                {
                                    char newNonTerminal = GetNewNonTerminal();
                                    value = newNonTerminal;
                                    terminalMap[c] = value;
                                    rulesToAdd.Add((newNonTerminal, c.ToString()));
                                }

                                newProduction = newProduction.Replace(c.ToString(), value.ToString());
                            }
                        }

                        if (newProduction != production)
                        {
                            rulesToModify.Add((kvp.Key, production, newProduction));
                        }
                    }
                }
            }

            foreach ((char, string) rule in rulesToAdd)
            {
                CNFGrammar.AddRule(rule.Item1, rule.Item2);
            }

            foreach ((char, string, string) rule in rulesToModify)
            {
                CNFGrammar.RemoveRule(rule.Item1, rule.Item2);
                CNFGrammar.AddRule(rule.Item1, rule.Item3);
            }
        }

        private void BreakLongProductions()
        {
            List<(char, string)> rulesToAdd = [];
            List<(char, string)> rulesToRemove = [];

            foreach (KeyValuePair<char, List<string>> kvp in CNFGrammar.Rules.ToList())
            {
                foreach (string? production in kvp.Value.ToList())
                {
                    if (production.Length > 2)
                    {
                        string current = production;
                        char lastNonTerminal = kvp.Key;

                        rulesToRemove.Add((kvp.Key, production));

                        while (current.Length > 2)
                        {
                            char newNonTerminal = GetNewNonTerminal();
                            string firstTwo = current[..2];

                            rulesToAdd.Add((lastNonTerminal, firstTwo[0].ToString() + newNonTerminal));
                            lastNonTerminal = newNonTerminal;
                            current = firstTwo[1] + current.Substring(2);
                        }

                        rulesToAdd.Add((lastNonTerminal, current));
                    }
                }
            }

            foreach ((char, string) rule in rulesToRemove)
            {
                CNFGrammar.RemoveRule(rule.Item1, rule.Item2);
            }

            foreach ((char, string) rule in rulesToAdd)
            {
                CNFGrammar.AddRule(rule.Item1, rule.Item2);
            }
        }

        private char GetNewNonTerminal()
        {
            string available = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray()
                .Where(c => !CNFGrammar.Rules.ContainsKey(c))
                .Aggregate("", (current, c) => current + c);

            foreach (char c in available)
            {
                if (!CNFGrammar.Rules.ContainsKey(c))
                {
                    return c;
                }
            }

            while (true)
            {
                char candidate = (char)('D' + newNonTerminalCounter);
                newNonTerminalCounter++;

                if (!CNFGrammar.Rules.ContainsKey(candidate) && candidate <= 'Z')
                {
                    return candidate;
                }
            }
        }
    }
}