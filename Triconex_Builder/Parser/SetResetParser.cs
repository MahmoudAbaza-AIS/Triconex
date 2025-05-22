using Triconex.Interfaces;
using TriconexMigration.Application.Models;

namespace TriconexMigration.Application.Parser;

public class SetResetParser : IInstructionParser
{
    public string InstructionParser(List<IInstruction> instructions)
    {
        if (instructions == null || instructions.Count == 0)
            throw new ArgumentException("Instructions list cannot be null or empty.");

        var lhs = instructions.FirstOrDefault(i => !string.IsNullOrWhiteSpace(i.Operator) && (i.Operator.Contains('=') || i.Operator.Contains("NOP")));
        var rhs = instructions.Where(i => !string.IsNullOrWhiteSpace(i.Operator) && !i.Operator.Contains('=')).ToList();

        int iR = rhs.FindIndex(i => i.Operator == "R");
        int iS = rhs.FindIndex(i => i.Operator == "S");

        var resolvedS = ConvertInstructionsToExpression(GetInstructionsBeforeOperand(rhs, "S", iS));
        var resolvedR = ConvertInstructionsToExpression(GetInstructionsBeforeOperand(rhs, "R", iR));

        var rOperator = rhs.FirstOrDefault(i => !string.IsNullOrWhiteSpace(i.Operator) && i.Operator.Contains("R"));
        //string finalOutput = $"_{rOperator?.Address?.MemoryArea}{rOperator?.Address?.Value}({resolvedS}, {resolvedR})";
        string finalOutput = $"({resolvedS}, {resolvedR})";

        string equation = $"{rOperator?.Address?.MemoryArea}{rOperator?.Address?.Value} = " + (iR < iS ? $"SR{finalOutput}" : $"RS{finalOutput}");

        if (lhs != null && lhs.Operator != "NOP")
        {
            equation +=
                $";;{lhs.Address?.MemoryArea} = {rOperator?.Address?.MemoryArea}";
        }

        return equation;
    }

    private static List<IInstruction> GetInstructionsBeforeOperand(List<IInstruction> instructionList, string operand, int operandIndex)
    {
        if (operandIndex == -1)
            return [];

        var instructions = instructionList.Take(operandIndex).ToList();
        int filterIndex = instructions.FindIndex(i => i.Operator is "R" or "S");

        return filterIndex != -1 ? instructions.Skip(filterIndex + 1).ToList() : instructions;
    }

    private static string ConvertInstructionsToExpression(List<IInstruction> instructions)
    {
        if (instructions == null || instructions.Count == 0)
            return "";

        var operatorMap = new Dictionary<string, (string transformation, string op)>
        {
            { "AN", ("! {0}", "AND") },
            { "ON", ("! {0}", "OR") },
            { "XN", ("! {0}", "Exclusive") },
            { "A", ("{0}", "AND") },
            { "O", ("{0}", "OR") },
            { "X", ("{0}", "Exclusive") }
        };

        var result = new List<string>();
        string mainOperator = "";
        bool canBeDominated = false;
        string dominantOperator = "";

        foreach (var instruction in instructions)
        {
            if (string.IsNullOrWhiteSpace(instruction.Address?.Content) && !string.IsNullOrWhiteSpace(instruction.Operator) && !instruction.Operator.Contains('(') && !instruction.Operator.Contains(')'))
            {
                canBeDominated = true;
                dominantOperator = operatorMap[instruction.Operator].op;

                continue;
            }
            if (operatorMap.TryGetValue(instruction.Operator ?? "", out var mapping))
            {
                var address = instruction.Address?.MemoryArea;
                var value = instruction.Address?.Value;
                var concatenatedAddressValue = $"{address}{value}";
                result.Add(string.Format(mapping.transformation, concatenatedAddressValue));
                mainOperator = mapping.op;
            }
        }

        if (string.IsNullOrWhiteSpace(mainOperator))
            throw new InvalidOperationException("No valid operator found in the instructions.");

        if (result.Count > 1 && canBeDominated)
        {
            var eqn = $"{dominantOperator}(";
            var final = instructions.Where(i => i.Operator is not "A(" and not ")").ToList();
            int indexWithEmptyContent = final.FindIndex(i => string.IsNullOrWhiteSpace(i.Address?.Content));

            var beforeEmpty = final.Take(indexWithEmptyContent).ToList();
            var afterEmpty = final.Skip(indexWithEmptyContent + 1).ToList();
            Func<List<IInstruction>, List<string>> extractValues = instructions =>
                instructions.Select(i => $"{i.Address?.MemoryArea}{i.Address?.Value}".Replace(" ", "")).ToList();

            var beforeValues = extractValues(beforeEmpty);
            var afterValues = extractValues(afterEmpty);

            var beforeCommonOperator = beforeEmpty.GroupBy(i => i.Operator)
                           .OrderByDescending(g => g.Count())
                           .First()
                           .Key;
            var afterCommonOperator = beforeEmpty.GroupBy(i => i.Operator)
                           .OrderByDescending(g => g.Count())
                           .First()
                           .Key;

            var beforeEquation = $"{operatorMap[beforeCommonOperator ?? ""].op} ({string.Join(" , ", beforeValues)})";
            var afterEquation = $"{operatorMap[afterCommonOperator ?? ""].op} ({string.Join(" , ", afterValues)})";
            return $"{dominantOperator}({beforeEquation}, {afterEquation})";
        }

        return result.Count == 1 ? result[0] : $"{mainOperator} ({string.Join(", ", result)})";
    }
}