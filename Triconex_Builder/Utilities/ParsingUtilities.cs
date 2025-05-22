using System.Text.RegularExpressions;
using TriconexMigration.Application.Models;

namespace TriconexMigration.Application.Utilities;

public static class ParsingUtilities
{
    public static List<List<string>> GetFunctionsBoundaries(List<string> data)
    {
        List<List<string>> nestedArrays = [];
        List<string> currentArray = [];

        foreach (var item in data)
        {
            if (item.Trim().Split(" ")?[0] == "FUNCTION")
            {
                currentArray = [item];
            }
            else if (item.Trim() == "END_FUNCTION")
            {
                currentArray?.Add(item);
                if (currentArray != null)
                {
                    nestedArrays.Add(currentArray);
                }
            }
            else
            {
                currentArray?.Add(item);
            }
        }

        return nestedArrays;
    }

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

    public static bool IsBitLogicType(string statement)
        => new Regex(@"^(AN|A|R|=)").IsMatch(statement.Trim());

    public static bool IsLoadAndTransferType(string statement)
        => new Regex(@"").IsMatch(statement.Trim());

    public static bool IsXInstructionType(string statement)
        => new Regex(@"").IsMatch(statement.Trim());
}