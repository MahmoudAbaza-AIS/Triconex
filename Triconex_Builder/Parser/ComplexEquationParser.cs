using System.Text.RegularExpressions;
using TriconexMigration.Application.Utilities;

namespace TriconexMigration.Application.Parser;
public static class ComplexEquationParser
{
    private static readonly List<string> Gates = ["AND", "OR", "Exclusive"];

    public static string Execute(string logic)
    {
        if (IsComplex(logic) && !(logic.Contains("SR") || logic.Contains("RS")))
        {
            string dominantGate = DetectOutermostGate(logic);
            var logicSegments = logic.Split($"{dominantGate} (").ToList();
            var lhs = logicSegments[0].Split("=");
            logicSegments[0] = lhs[1].Trim();
            logicSegments = logicSegments
            .Select(s =>
            StandalonePeripheral(s)
            ? s.Replace("(", "").Replace(")", "")
            : Regex.Replace(s, @"\)+$", ")"))
            .ToList();
            logicSegments.RemoveAll(string.IsNullOrWhiteSpace);
            if (logicSegments.Count > 0) logic = $"{lhs[0]}= {dominantGate}({string.Join(", ", logicSegments)})";
        }
        return CleanLogic(logic);
    }

    private static bool IsComplex(string equation)
    {
        // Check for multiple levels of parentheses
        int openParenthesesCount = equation.Count(c => c == '(');
        int closeParenthesesCount = equation.Count(c => c == ')');

        // Check for nested logical operators inside parentheses
        bool hasNestedOperators = Regex.IsMatch(equation, @"\(([^()]*?(AND|OR|XOR|Exclusive)[^()]*?)\)");

        // Consider an equation complex if it has more than one level of parentheses or nested logical operators
        return openParenthesesCount >= 3 || hasNestedOperators;
    }

    // Helper method to detect the outermost logical gate (AND, OR, Exclusive)
    public static string DetectOutermostGate(string logic)
    {
        int currentDepth = 0;
        string? detectedGate = null;

        for (int i = 0; i < logic.Length; i++)
        {
            char c = logic[i];

            if (c == '(') currentDepth++;  // Entering deeper into nested parentheses
            else if (c == ')') currentDepth--;  // Exiting nested parentheses

            // Check for gates only when we are at depth 0 (i.e., outside any parentheses)
            if (currentDepth == 0)
                foreach (var gate in Gates)
                    if (logic[i..].StartsWith(gate)) detectedGate = gate;  // Track the latest gate found at depth 0
        }

        return detectedGate ?? "";  // Return the last gate found at depth 0
    }

    private static bool StandalonePeripheral(string part) => !part.Contains("AND") && !part.Contains("OR") && !part.Contains("Exclusive");

    public static string CleanLogic(string logic)
    {
        // Step 1: Remove empty parentheses followed by a comma and whitespace
        //logic = Regex.Replace(logic, @"\(\s*\),\s*", "");

        // Step 2: Add a comma before gates (AND, OR) if missing and ensure proper spacing
        //logic = Regex.Replace(logic, @"(?<=\))(?=\s*(AND|OR))", ", ");  // Add a comma before AND/OR if needed

        // Step 3: Remove misplaced commas before AND/OR and ensure proper formatting
        //logic = Regex.Replace(logic, @",\s*(AND|OR)", " $1");  // Remove misplaced commas before AND/OR

        // Step 4: Remove extra commas inside parentheses
        //logic = Regex.Replace(logic, @",+", ",");

        // Step 5: Remove spaces around parentheses
        logic = Regex.Replace(logic, @"\s*\(\s*", "(");
        logic = Regex.Replace(logic, @"\s*\)\s*", ")");
        logic = Regex.Replace(logic, @"\(\s*,", "(");
        logic = Regex.Replace(logic, @",\s*\)", ")");
        logic = Regex.Replace(logic, @"([A-Za-z0-9_]+)(?=OR|AND)", "$1,(");
        logic = logic.Trim().Replace(" ", "").Replace(",,", ",");

        return HelperUtilities.BalanceParentheses(logic);
    }
}
