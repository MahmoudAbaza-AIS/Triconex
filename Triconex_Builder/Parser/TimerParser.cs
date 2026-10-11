using System.Text.RegularExpressions;
using Triconex.Interfaces;
using TriconexMigration.Application.Models;

namespace TriconexMigration.Application.Parser;

public class TimerParser : IInstructionParser
{
    public string InstructionParser(List<IInstruction> instructions)
    {
        var filteredInstructions = instructions.Where(i => i.Operator != "NOP").ToList();

        var lhs = filteredInstructions.FirstOrDefault(i => i.Operator == "=");

        var timerInstruction = filteredInstructions.FirstOrDefault(i => i.Operator is "SP" or "SD" or "SF");

        string prefix = DetermineTimerPrefix(timerInstruction?.Operator);

        var loadInstruction = filteredInstructions.FirstOrDefault(i => i.Operator == "L");

        var gateConditions = filteredInstructions.Where(i =>
            i.Operator is "A" or "O" or "AN" or "ON" or "XOR")
            .ToList();

        string gate = DetermineGate(gateConditions);

        string startCondition = string.Join(", ", gateConditions.Select(i =>
            !string.IsNullOrWhiteSpace(i.Operator) && i.Operator.Contains('N')
            ? $"!{i.Address?.MemoryArea}{i.Address?.Value}"
            : $"{i.Address?.MemoryArea}{i.Address?.Value}"));

        if (gateConditions.Count > 1)
        {
            if (lhs != null) startCondition = Regex.Replace(startCondition, @"T\d+", string.Empty);
            startCondition = $"{gate}({startCondition})";
        }

        string timerExpression = $"{timerInstruction?.Address?.MemoryArea}{timerInstruction?.Address?.Value} = {prefix}({startCondition}, {loadInstruction?.Address?.MemoryArea?.Replace("S5", "")})";

        var finalAssignmentCondition = filteredInstructions.FirstOrDefault(i => i.Operator == "A" && i.Address?.MemoryArea == "T");

        if (finalAssignmentCondition != null && lhs != null)
        {
            var negated = instructions.Any(i => i.Operator == "NOT");
            var rhs = $"{(negated ? '!' : string.Empty)}{finalAssignmentCondition.Address?.MemoryArea}{finalAssignmentCondition.Address?.Value}";
            string finalExpression = $"{lhs?.Address?.MemoryArea}{lhs?.Address?.Value} = {rhs}";
            return $"{timerExpression};;{finalExpression}";
        }

        return $"{timerExpression}";
    }

    private static string DetermineGate(List<IInstruction> conditions)
    {
        if (conditions.Any(i => i.Operator == "O")) return "OR";

        if (conditions.Any(i => i.Operator == "XOR")) return "XOR";

        return "AND";
    }

    private static string DetermineTimerPrefix(string? instructionOperator)
    {
        return instructionOperator switch
        {
            "SP" => "TP",
            "SD" => "TON",
            "SF" => "TOF",
            //_ => throw new InvalidOperationException($"Unknown timer operator: {instructionOperator}")
            _ => string.Empty
        };
    }
}

