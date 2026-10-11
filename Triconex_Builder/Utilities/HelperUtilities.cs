using System.Text.RegularExpressions;
using TriconexMigration.Application.DTOS;

namespace TriconexMigration.Application.Utilities;

public static class HelperUtilities
{
    public static void AddResultToList(List<ResultParserDTO> resultList, string logic, int networkNumber, string functionBlock)
    {
        var splits = logic.Contains(";;") ? logic.Split(";;") : [logic];

        foreach (var split in splits)
        {
            resultList.Add(new ResultParserDTO
            {
                Logic = split.Trim(),
                NetworkNumber = networkNumber,
                FunctionBlock = functionBlock ?? ""
            });
        }
    }

    public static string BalanceParentheses(string input)
    {
        Stack<char> stack = new();
        int unmatchedOpen = 0;
        int unmatchedClose = 0;

        foreach (char c in input)
        {
            if (c == '(')
                stack.Push(c);
            else if (c == ')')
            {
                if (stack.Count > 0 && stack.Peek() == '(') stack.Pop();
                else unmatchedClose++;
            }
        }

        unmatchedOpen = stack.Count;


        return SimplifyParameters(new string('(', unmatchedClose) + input + new string(')', unmatchedOpen));
    }

    public static string SimplifyParameters(string input)
    {
        if (string.IsNullOrWhiteSpace(input)) return input;

        Regex regex = new(@"\b(AND|OR)\s*\(([^()]+)\)");

        string result = regex.Replace(input, match =>
        {
            string functionName = match.Groups[1].Value;
            string parameters = match.Groups[2].Value;

            string[] paramList = parameters.Split(',', StringSplitOptions.RemoveEmptyEntries);

            if (paramList.Length == 1)
            {
                bool keepsGate = match.Index > 0 && input[match.Index - 1] == ')';
                return keepsGate
                    ? $"{functionName}({paramList[0].Trim()})"
                    : paramList[0].Trim();
            }

            string joinedParams = string.Join(", ", paramList);
            return $"{functionName}({joinedParams})";
        });

        result = Regex.Replace(result, @"\)\s*(AND|OR)\s*\((?!\s*[A-Za-z0-9_!]+\s*\))", "), $1 (");

        result = Regex.Replace(result, @"(?<=\))\s*!", ", !");

        return result.Trim();
    }
}
