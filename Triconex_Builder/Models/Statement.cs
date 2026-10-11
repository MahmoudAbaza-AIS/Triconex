using System.Text.RegularExpressions;
using TriconexMigration.Application.Utilities;

namespace TriconexMigration.Application.Models;

public interface IInstruction
{
    public string? Operator { get; }
    public Address? Address { get; }
    public IInstruction GetInstraction(string statement);
}


public class Address
{
    public string? Content { get; set; }
    public string? MemoryArea { get; set; }
    public string? Value { get; set; }
}


public class BitLogicInstruction : IInstruction
{
    public string? Operator { get; private set; }
    public Address? Address { get; private set; }
    public IInstruction GetInstraction(string statement)
    {
        var instractionAddress = statement.Split(' ')
                                      .Where(x => !string.IsNullOrWhiteSpace(x) && x != ";")
                                      .ToList();
        var op = new BitLogicInstruction
        {
            Operator = ParsingUtilities.NormalizeAwlOperator(instractionAddress[0]),
            Address = new Address
            {
                Content = string.Join("     ", instractionAddress.Skip(1).Where(x => !string.IsNullOrWhiteSpace(x))).Replace(";", "").Replace("#", ""),
                MemoryArea = instractionAddress.Count > 1 && !string.IsNullOrWhiteSpace(instractionAddress[1]) ? instractionAddress[1].Replace(";", "").Replace("#", "") : "",
                Value = instractionAddress.Count > 2 && !string.IsNullOrWhiteSpace(instractionAddress[2]) ? instractionAddress[2].Replace(";", "").Replace("#", "") : "",
            }
        };
        return op;
    }
}

public class LoadAndTransferInstruction : IInstruction
{
    public string? Operator { get; private set; }
    public Address? Address { get; private set; }

    public IInstruction GetInstraction(string statement)
    {
        var instractionAddress = statement.Split(' ')
                                      .Where(x => !string.IsNullOrWhiteSpace(x) && x != ";")
                                      .ToList();
        if (instractionAddress.Count > 1 && instractionAddress[0].EndsWith(':'))
            instractionAddress.RemoveAt(0);
        var op = new LoadAndTransferInstruction
        {
            Operator = ParsingUtilities.NormalizeAwlOperator(instractionAddress[0]),
            Address = new Address
            {
                Content = string.Join("     ", instractionAddress.Skip(1).Where(x => !string.IsNullOrWhiteSpace(x))).Replace(";", ""),
                MemoryArea = instractionAddress.Count > 1 && !string.IsNullOrWhiteSpace(instractionAddress[1]) ? instractionAddress[1].Replace(";", "") : "",
                Value = instractionAddress.Count > 2 && !string.IsNullOrWhiteSpace(instractionAddress[2]) ? instractionAddress[2].Replace(";", "") : "",
            }
        };
        return op;
    }
}

public class XInstruction : IInstruction
{
    public string? Operator { get; private set; }
    public Address? Address { get; private set; }

    public IInstruction GetInstraction(string statement)
    {
        var InstractionAddress = Regex.Match(statement.Trim(), @"\b(?!A\b|AN\b|O\b|ON\b|X\b|XN\b|R\b)\w+\s*\d*\.*\d*").Value;

        return new XInstruction
        {
            Operator = ParsingUtilities.NormalizeAwlOperator(Regex.Match(statement.Trim(), @"\S+").Value),
            Address = new Address
            {
                Content = InstractionAddress,
                MemoryArea = Regex.Match(InstractionAddress.Trim(), @"^\w+").Value,
                Value = Regex.Match(InstractionAddress.Trim(), @"\d+\.\d+").Value,
            }
        };
    }
}

