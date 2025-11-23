namespace Chomsky_CYK
{
    class CNFConverter
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
            Console.WriteLine("\n--- Step 1: Add new start symbol ---");
            AddNewStartSymbol();
            CNFGrammar.Print();

            Console.WriteLine("\n--- Step 2: Eliminate ε-productions ---");
            EliminateEpsilonProductions();
            CNFGrammar.Print();

            Console.WriteLine("\n--- Step 3: Eliminate unit productions ---");
            EliminateUnitProductions();
            CNFGrammar.Print();

            Console.WriteLine("\n--- Step 4: Replace nonsolitary terminals ---");
            ReplaceNonsolitaryTerminals();
            CNFGrammar.Print();

            Console.WriteLine("\n--- Step 5: Break long productions ---");
            BreakLongProductions();
            CNFGrammar.Print();
        }

        private void AddNewStartSymbol()
        {
            char newStart = GetNewNonTerminal();
            CNFGrammar.AddRule(newStart, StartSymbol.ToString());
            StartSymbol = newStart;
        }

        private void EliminateEpsilonProductions()
        {
            // Find nullable nonterminals
            var nullable = FindNullable();

            // Remove epsilon productions and add alternatives
            var rulesToAdd = new List<(char, string)>();
            var rulesToRemove = new List<(char, string)>();

            foreach (var kvp in CNFGrammar.Rules.ToList())
            {
                foreach (var production in kvp.Value.ToList())
                {
                    if (production == "ε" || production == "")
                    {
                        rulesToRemove.Add((kvp.Key, production));
                    }
                    else
                    {
                        // Generate all combinations by removing nullable symbols
                        var combinations = GenerateCombinations(production, nullable);
                        foreach (var combo in combinations)
                        {
                            if (!string.IsNullOrEmpty(combo) && combo != production)
                            {
                                rulesToAdd.Add((kvp.Key, combo));
                            }
                        }
                    }
                }
            }

            foreach (var rule in rulesToRemove)
                CNFGrammar.RemoveRule(rule.Item1, rule.Item2);

            foreach (var rule in rulesToAdd)
            {
                if (!CNFGrammar.Rules[rule.Item1].Contains(rule.Item2))
                    CNFGrammar.AddRule(rule.Item1, rule.Item2);
            }
        }

        private HashSet<char> FindNullable()
        {
            var nullable = new HashSet<char>();
            bool changed = true;

            while (changed)
            {
                changed = false;
                foreach (var kvp in CNFGrammar.Rules)
                {
                    if (nullable.Contains(kvp.Key))
                        continue;

                    foreach (var production in kvp.Value)
                    {
                        if (production == "ε" || production == "" ||
                            production.All(c => nullable.Contains(c)))
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
            var result = new List<string> { production };

            for (int i = 0; i < production.Length; i++)
            {
                if (nullable.Contains(production[i]))
                {
                    var newResults = new List<string>();
                    foreach (var str in result)
                    {
                        // Find the character at position i in original and remove it
                        int count = 0;
                        for (int j = 0; j < str.Length; j++)
                        {
                            if (str[j] == production[i])
                            {
                                if (count == result.IndexOf(str))
                                {
                                    string newStr = str.Remove(j, 1);
                                    if (!newResults.Contains(newStr))
                                        newResults.Add(newStr);
                                    break;
                                }
                                count++;
                            }
                        }
                        // Simple removal
                        for (int j = 0; j < str.Length; j++)
                        {
                            string newStr = str.Remove(j, 1);
                            if (str[j] == production[i] && !newResults.Contains(newStr))
                                newResults.Add(newStr);
                        }
                    }
                    result.AddRange(newResults);
                }
            }

            return result.Distinct().ToList();
        }

        private void EliminateUnitProductions()
        {
            bool changed = true;

            while (changed)
            {
                changed = false;
                var rulesToAdd = new List<(char, string)>();
                var rulesToRemove = new List<(char, string)>();

                foreach (var kvp in CNFGrammar.Rules.ToList())
                {
                    foreach (var production in kvp.Value.ToList())
                    {
                        // Check if it's a unit production (single nonterminal)
                        if (production.Length == 1 && char.IsUpper(production[0]))
                        {
                            char unitNonTerminal = production[0];

                            // Add all productions of the unit nonterminal
                            if (CNFGrammar.Rules.ContainsKey(unitNonTerminal))
                            {
                                foreach (var prod in CNFGrammar.Rules[unitNonTerminal])
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

                foreach (var rule in rulesToRemove)
                    CNFGrammar.RemoveRule(rule.Item1, rule.Item2);

                foreach (var rule in rulesToAdd)
                    CNFGrammar.AddRule(rule.Item1, rule.Item2);
            }
        }

        private void ReplaceNonsolitaryTerminals()
        {
            var terminalMap = new Dictionary<char, char>();
            var rulesToAdd = new List<(char, string)>();
            var rulesToModify = new List<(char, string, string)>();

            foreach (var kvp in CNFGrammar.Rules.ToList())
            {
                foreach (var production in kvp.Value.ToList())
                {
                    if (production.Length > 1)
                    {
                        string newProduction = production;

                        for (int i = 0; i < production.Length; i++)
                        {
                            char c = production[i];
                            if (!char.IsUpper(c)) // Terminal
                            {
                                if (!terminalMap.ContainsKey(c))
                                {
                                    char newNonTerminal = GetNewNonTerminal();
                                    terminalMap[c] = newNonTerminal;
                                    rulesToAdd.Add((newNonTerminal, c.ToString()));
                                }

                                newProduction = newProduction.Replace(c.ToString(),
                                    terminalMap[c].ToString());
                            }
                        }

                        if (newProduction != production)
                        {
                            rulesToModify.Add((kvp.Key, production, newProduction));
                        }
                    }
                }
            }

            foreach (var rule in rulesToAdd)
                CNFGrammar.AddRule(rule.Item1, rule.Item2);

            foreach (var rule in rulesToModify)
            {
                CNFGrammar.RemoveRule(rule.Item1, rule.Item2);
                CNFGrammar.AddRule(rule.Item1, rule.Item3);
            }
        }

        private void BreakLongProductions()
        {
            var rulesToAdd = new List<(char, string)>();
            var rulesToRemove = new List<(char, string)>();

            foreach (var kvp in CNFGrammar.Rules.ToList())
            {
                foreach (var production in kvp.Value.ToList())
                {
                    if (production.Length > 2)
                    {
                        string current = production;
                        char lastNonTerminal = kvp.Key;

                        rulesToRemove.Add((kvp.Key, production));

                        // Break into binary productions from left to right
                        while (current.Length > 2)
                        {
                            char newNonTerminal = GetNewNonTerminal();
                            string firstTwo = current.Substring(0, 2);

                            rulesToAdd.Add((lastNonTerminal, firstTwo[0].ToString() + newNonTerminal));
                            lastNonTerminal = newNonTerminal;
                            current = firstTwo[1] + current.Substring(2);
                        }

                        rulesToAdd.Add((lastNonTerminal, current));
                    }
                }
            }

            foreach (var rule in rulesToRemove)
                CNFGrammar.RemoveRule(rule.Item1, rule.Item2);

            foreach (var rule in rulesToAdd)
                CNFGrammar.AddRule(rule.Item1, rule.Item2);
        }

        private char GetNewNonTerminal()
        {
            // First try X, Y, Z, then X0, X1, etc.
            char[] available = "XYZ".ToCharArray();

            foreach (char c in available)
            {
                if (!CNFGrammar.Rules.ContainsKey(c))
                    return c;
            }

            // Use numbered nonterminals
            while (true)
            {
                char candidate = (char)('D' + newNonTerminalCounter);
                newNonTerminalCounter++;

                if (!CNFGrammar.Rules.ContainsKey(candidate) && candidate <= 'Z')
                    return candidate;
            }
        }
    }
}