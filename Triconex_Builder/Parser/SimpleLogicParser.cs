using System.Text.RegularExpressions;
using Triconex.Interfaces;
using TriconexMigration.Application.Models;
using TriconexMigration.Application.Services;
using TriconexMigration.Application.Utilities;

namespace TriconexMigration.Application.Parser;
public class SimpleLogicParser : IInstructionParser
{
    public string InstructionParser(List<IInstruction> InstructionsList)
    {
        if (InstructionsList.Count == 0) return string.Empty;

        var instructions = InstructionsList.Where(e => e.Address?.MemoryArea != "BLD" && e.Operator != "NOP" && e.Operator != "BLD").ToList();

        if (instructions == null) return "EMPTY!";

        var lhs = "";

        if (instructions.Count == 2)
        {
            lhs = instructions[1].Address?.MemoryArea + instructions[1].Address?.Value;
            return (!string.IsNullOrWhiteSpace(lhs) ? lhs : "{lhs}") + " = " +
                   (!string.IsNullOrWhiteSpace(instructions[0].Operator) && instructions[0].Operator.Contains("N") ? "! " : "") +
                   instructions[0].Address?.MemoryArea + instructions[0].Address?.Value;
        }

        var endOfInstruction = instructions.Any(i => i.Operator == "T") ? instructions.FindIndex(i => i.Operator == "T") : instructions.Count - 1;
        lhs = instructions[endOfInstruction].Address?.MemoryArea + instructions[endOfInstruction].Address?.Value;
        var result = (!string.IsNullOrWhiteSpace(lhs) ? lhs : "{lhs}") + " "
            + (instructions[endOfInstruction].Operator != "=" ? "=" : instructions[endOfInstruction].Operator) + " ";
        var StarterOperator = ParserService.GetBitLogicOperator(instructions[0].Operator ?? string.Empty);

        for (var i = 0; i < endOfInstruction; i++)
        {
            if (instructions[i].Operator == "NOT") continue;
            var haveNot = !string.IsNullOrWhiteSpace(instructions[i].Operator) && instructions[i].Operator.Contains("N") ? "! " : "";
            var CurrentOperator = ParserService.GetBitLogicOperator(instructions[i].Operator);
            var PreviousOperator = i > 0 ? ParserService.GetBitLogicOperator(instructions[i - 1].Operator) : "";
            var IncomingOperator = ParserService.GetBitLogicOperator(instructions[i + 1].Operator);

            if (CurrentOperator == PreviousOperator && !string.IsNullOrWhiteSpace(instructions[i].Address?.MemoryArea))
            {
                result += (string.IsNullOrWhiteSpace(instructions[i - 1].Address?.MemoryArea) ? "" : " , ") + haveNot + instructions[i].Address?.MemoryArea + instructions[i].Address?.Value;

                if (i + 1 >= endOfInstruction || instructions[i].Address?.MemoryArea?.Length > 0 && CurrentOperator != IncomingOperator)
                    result += ")";
            }
            else
            {
                if (i > 0 && StarterOperator == CurrentOperator && instructions[i - 1].Address?.MemoryArea?.Length > 0)
                    CurrentOperator = " , ";

                result += CurrentOperator + haveNot + instructions[i].Address?.MemoryArea + instructions[i].Address?.Value;
            }
        }

        var numOfOpenningBracket = result.Split("(").Length;
        var numOfCloseBracket = result.Split(")").Length;

        for (var i = 0; i < numOfOpenningBracket - numOfCloseBracket; i++)
        {
            result += ")";
        }

        if (result.Contains("{lhs}") && instructions[endOfInstruction].Operator == ")")
        {
            var parentGate = ParserService.GetBitLogicOperator(instructions[0].Operator ?? string.Empty);
            var rhs = result.Split("=")[0];
            result = $"{rhs} = {parentGate}{rhs[1]}{instructions[endOfInstruction].Operator}";
        }

        if (instructions.Any(i => i.Operator == "NOT"))
        {
            result = HelperUtilities.SimplifyParameters(result);
            var splits = result.Split("=");
            splits[1] = $"!{splits[1].Replace(" ", "")}";
            result = string.Join("=", splits);
            //result += $";;{splits[0]}=!{splits[0]}";
        }

        return result;
    }
}
