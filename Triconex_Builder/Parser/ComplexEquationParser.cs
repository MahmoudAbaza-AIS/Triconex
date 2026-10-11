using System.Text.RegularExpressions;
using TriconexMigration.Application.Utilities;

namespace TriconexMigration.Application.Parser;
public static class ComplexEquationParser
{
    private static readonly List<string> Gates = ["AND", "OR", "Exclusive"];

    public static string Execute(string logic)
    {
        if (IsComplex(logic) && !(logic.Contains("SR") || logic.Contains("RS")) && !HasSiblingGates(logic))
        {
            string dominantGate = DetectOutermostGate(logic);
            if (!string.IsNullOrEmpty(dominantGate))
                logic = ApplyDominantGate(logic, dominantGate);
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

    private static string ApplyDominantGate(string logic, string dominantGate)
    {
        var logicSegments = SplitOnGate(logic, dominantGate);
        if (logicSegments.Count <= 1) return logic;

        var lhs = logicSegments[0].Split('=');
        if (lhs.Length < 2) return logic;

        logicSegments[0] = lhs[1].Trim();
        logicSegments = logicSegments
            .Select(s => StandalonePeripheral(s) && !Regex.IsMatch(s, @"[A-Za-z0-9_!]+\(")
                ? s.Replace("(", "").Replace(")", "")
                : TrimUnmatchedClosing(s))
            .ToList();
        logicSegments.RemoveAll(string.IsNullOrWhiteSpace);
        if (logicSegments.Count == 0) return logic;

        return $"{lhs[0]}= {dominantGate}({string.Join(", ", logicSegments)})";
    }

    private static List<string> SplitOnGate(string logic, string gate)
    {
        var parts = new List<string>();
        int depth = 0;
        int last = 0;

        for (int i = 0; i < logic.Length; i++)
        {
            char c = logic[i];
            if (c == '(')
            {
                depth++;
                continue;
            }
            if (c == ')')
            {
                depth--;
                continue;
            }
            if (!logic.AsSpan(i).StartsWith(gate)) continue;

            int after = i + gate.Length;
            if (after < logic.Length && char.IsLetter(logic[after])) continue;

            int paren = after;
            while (paren < logic.Length && logic[paren] == ' ') paren++;
            if (paren >= logic.Length || logic[paren] != '(') continue;

            bool atDepthZero = depth == 0;
            bool continuesSameGate = i > 0 && logic[i - 1] == ')';
            if (!atDepthZero && !continuesSameGate) continue;

            parts.Add(logic[last..i]);
            last = paren + 1;
            depth++;
            i = paren;
        }

        parts.Add(logic[last..]);
        return parts;
    }

    private static string TrimUnmatchedClosing(string segment)
    {
        int depth = 0;
        for (int i = 0; i < segment.Length; i++)
        {
            if (segment[i] == '(') depth++;
            else if (segment[i] == ')')
            {
                if (depth == 0) return segment[..i];
                depth--;
            }
        }
        return segment;
    }

    private static bool HasSiblingGates(string logic)
    {
        int eq = logic.IndexOf('=');
        if (eq < 0 || eq == logic.Length - 1) return false;

        int i = eq + 1;
        if (!TryConsumeExpression(logic, ref i)) return false;
        while (i < logic.Length && char.IsWhiteSpace(logic[i])) i++;
        return i < logic.Length && logic[i] == ',';
    }

    private static bool TryConsumeExpression(string logic, ref int i)
    {
        while (i < logic.Length && char.IsWhiteSpace(logic[i])) i++;
        if (i < logic.Length && logic[i] == '!') i++;
        while (i < logic.Length && char.IsWhiteSpace(logic[i])) i++;
        foreach (var gate in Gates)
        {
            if (!logic.AsSpan(i).StartsWith(gate)) continue;
            int after = i + gate.Length;
            if (after < logic.Length && char.IsLetter(logic[after])) continue;
            i = after;
            while (i < logic.Length && char.IsWhiteSpace(logic[i])) i++;
            if (i >= logic.Length || logic[i] != '(') return false;
            int depth = 0;
            for (; i < logic.Length; i++)
            {
                if (logic[i] == '(') depth++;
                else if (logic[i] == ')')
                {
                    depth--;
                    if (depth == 0) { i++; return true; }
                }
            }
            return false;
        }
        return false;
    }

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
