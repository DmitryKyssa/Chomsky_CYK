namespace Chomsky_CYK
{
    class CYKParser
    {
        private Grammar grammar;
        private char startSymbol;
        private HashSet<char>[,] table;
        private int n;

        public CYKParser(Grammar grammar, char startSymbol)
        {
            this.grammar = grammar;
            this.startSymbol = startSymbol;
        }

        public bool Parse(string word)
        {
            n = word.Length;

            if (n == 0)
            {
                return false;
            }

            table = new HashSet<char>[n, n];

            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    table[i, j] = new HashSet<char>();
                }
            }

            for (int i = 0; i < n; i++)
            {
                char terminal = word[i];

                foreach (KeyValuePair<char, List<string>> kvp in grammar.Rules)
                {
                    foreach (string production in kvp.Value)
                    {
                        if (production.Length == 1 && production[0] == terminal)
                        {
                            table[0, i].Add(kvp.Key);
                        }
                    }
                }
            }

            for (int length = 2; length <= n; length++)
            {
                for (int i = 0; i <= n - length; i++)
                {
                    for (int k = 1; k < length; k++)
                    {
                        HashSet<char> leftSet = table[k - 1, i];
                        HashSet<char> rightSet = table[length - k - 1, i + k];

                        foreach (char leftNT in leftSet)
                        {
                            foreach (char rightNT in rightSet)
                            {
                                foreach (KeyValuePair<char, List<string>> kvp in grammar.Rules)
                                {
                                    foreach (string production in kvp.Value)
                                    {
                                        if (production.Length == 2 && production[0] == leftNT && production[1] == rightNT)
                                        {
                                            table[length - 1, i].Add(kvp.Key);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return table[n - 1, 0].Contains(startSymbol);
        }

        public void PrintTable()
        {
            for (int length = n; length >= 1; length--)
            {
                Console.Write($"Length {length}: ");
                for (int i = 0; i <= n - length; i++)
                {
                    HashSet<char> symbols = table[length - 1, i];
                    Console.Write($"[{string.Join(",", symbols)}] ");
                }
                Console.WriteLine();
            }
        }
    }
}