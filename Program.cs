namespace Chomsky_CYK
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=== Context-Free Grammar to Chomsky Normal Form Converter ===\n");

            Grammar grammar = new Grammar();

            Console.WriteLine("Enter grammar rules (format: A -> BCD | a | ε)");
            Console.WriteLine("Enter empty line to finish.\n");
            Console.WriteLine("Example:\nS -> AB | a\nA -> a\nB -> b | ε\n");

            string startSymbol = null;

            while (true)
            {
                Console.Write("Rule: ");
                string? line = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(line))
                {
                    break;
                }

                try
                {
                    string[] parts = line.Split(["->"], StringSplitOptions.None);
                    if (parts.Length != 2)
                    {
                        Console.WriteLine("Invalid format. Use: A -> BCD | a");
                        continue;
                    }

                    char nonTerminal = parts[0].Trim()[0];
                    startSymbol ??= nonTerminal.ToString();

                    string[] productions = parts[1].Split('|');
                    foreach (string prod in productions)
                    {
                        grammar.AddRule(nonTerminal, prod.Trim());
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                }
            }

            if (grammar.Rules.Count == 0)
            {
                Console.WriteLine("No rules entered. Using example grammar:");
                grammar.AddRule('S', "AB");
                grammar.AddRule('S', "BC");
                grammar.AddRule('A', "BA");
                grammar.AddRule('A', "a");
                grammar.AddRule('B', "CC");
                grammar.AddRule('B', "b");
                grammar.AddRule('C', "AB");
                grammar.AddRule('C', "a");
                startSymbol = "S";
            }

            Console.WriteLine("\n=== Original Grammar ===");
            grammar.Print();

            CNFConverter cnfConverter = new CNFConverter(grammar, startSymbol![0]);
            cnfConverter.ConvertToCNF();

            Console.WriteLine("\n=== Grammar in Chomsky Normal Form ===");
            cnfConverter.CNFGrammar.Print();

            Console.WriteLine("\n=== CYK Algorithm - Word Recognition ===");

            while (true)
            {
                Console.Write("\nEnter word to check (or 'quit' to exit): ");
                string? word = Console.ReadLine();

                if (word?.ToLower() == "quit")
                {
                    break;
                }

                CYKParser cyk = new CYKParser(cnfConverter.CNFGrammar, cnfConverter.StartSymbol);
                bool accepted = cyk.Parse(word!);

                Console.WriteLine($"\nWord '{word}' is {(accepted ? "ACCEPTED" : "REJECTED")}");
                Console.WriteLine("\nParse Table:");
                cyk.PrintTable();
            }
        }
    }
}