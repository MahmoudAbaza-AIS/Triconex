using System.Text.RegularExpressions;
using TriconexMigration.Application.Models;

namespace TriconexMigration.Application.Utilities;

public static class ParsingUtilities
{
    public static List<List<string>> GetFunctionsBoundaries(List<string> data)
    {
        List<List<string>> nestedArrays = [];
        List<string> currentArray = [];
        bool open = false;

        foreach (var item in data)
        {
            if (IsLogicBlockStart(item))
            {
                if (open && currentArray.Count > 0)
                    nestedArrays.Add(currentArray);
                currentArray = [item];
                open = true;
            }
            else if (IsLogicBlockEnd(item))
            {
                if (!open) continue;
                currentArray.Add(item);
                nestedArrays.Add(currentArray);
                currentArray = [];
                open = false;
            }
            else if (open)
            {
                currentArray.Add(item);
            }
        }

        if (open && currentArray.Count > 0)
            nestedArrays.Add(currentArray);

        return nestedArrays;
    }

    private static string FirstToken(string item)
        => item.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? "";

    private static bool IsLogicBlockStart(string item)
        => FirstToken(item) is "FUNCTION" or "ORGANIZATION_BLOCK" or "ORGANISATION_BLOCK";

    private static bool IsLogicBlockEnd(string item)
        => FirstToken(item) is "END_FUNCTION" or "END_ORGANIZATION_BLOCK" or "END_ORGANISATION_BLOCK";

    public static List<List<string>> GetFunctionBlocksBoundaries(List<string> data)
    {
        List<List<string>> nestedArrays = [];
        List<string> currentArray = [];

        foreach (var item in data)
        {
            if (item.Trim().StartsWith("FUNCTION_BLOCK"))
            {
                currentArray = new List<string> { item };
            }
            else if (item.Trim().StartsWith("END_FUNCTION_BLOCK"))
            {
                currentArray?.Add(item);
                if (currentArray != null)
                {
                    nestedArrays.Add(currentArray);
                }
            }
            else
            {
                if (item.Trim().Split(" ")?[0] == "FUNCTION") break;
                currentArray?.Add(item);
            }
        }

        return nestedArrays;
    }

    public static List<FunctionBlockAttribute> GetIOParams(List<string> data, string regex)
    {
        List<FunctionBlockAttribute> FB = new();
        var DataList = data.Where(e => new Regex(regex).IsMatch(e.Trim())).ToList();
        foreach (var item in DataList)
        {
            FB.Add(new FunctionBlockAttribute
            {
                Name = Regex.Match(item.Trim(), @"^([^:]+)").Value,
                DataType = Regex.Match(item.Trim(), @"(?<=:)[^;]+").Value,
                InitialValue = "N/A"
            });
        }

        return FB;
    }

    public static string NormalizeAwlOperator(string op)
    {
        var token = op.Trim().ToUpperInvariant();
        return token switch
        {
            "U" => "A",
            "UN" => "AN",
            "U(" => "A(",
            "SI" => "SP",
            "SE" => "SD",
            "SA" => "SF",
            _ => token
        };
    }

    public static bool IsBitLogicType(string statement)
        => Regex.IsMatch(statement.Trim(), @"^(UN|U\(|U|XN|X\(|X|ON|O\(|O|AN|A\(|A|NOT|R|S|=|\)|\()(?=\s|$)", RegexOptions.IgnoreCase);

    public static bool IsLoadAndTransferType(string statement)
        => new Regex(@"").IsMatch(statement.Trim());

    public static bool IsXInstructionType(string statement)
        => new Regex(@"").IsMatch(statement.Trim());
}