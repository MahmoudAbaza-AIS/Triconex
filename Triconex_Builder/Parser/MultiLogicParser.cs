using DocumentFormat.OpenXml.Drawing;
using System.Text.RegularExpressions;
using Triconex.Interfaces;
using TriconexMigration.Application.Models;

namespace TriconexMigration.Application.Parser;

public class MultiLogicParser(SimpleLogicParser slp, SetResetParser srp) : IInstructionParser
{
    private readonly SimpleLogicParser _simpleLogicParser = slp;
    private readonly SetResetParser _setResetParser = srp;

    public string InstructionParser(List<IInstruction> instructions)
    {
        List<string> instructionsBrokenDown = [];
        bool containsS = instructions.Any(i => !string.IsNullOrWhiteSpace(i.Operator) && i.Operator.Contains('S'));
        var retrun = DivideAndConquer(instructions, containsS);
        //foreach (var ret in retrun)
        //{
        //    string output = (containsS ? _setResetParser.InstructionParser(ret) : _simpleLogicParser.InstructionParser(ret));
        //    instructionsBrokenDown.Add(output);
        //}
        string output = (containsS ? _setResetParser.InstructionParser(instructions) : _simpleLogicParser.InstructionParser(instructions));
        return output;
    }

    public static List<List<IInstruction>> DivideAndConquer(List<IInstruction> instructions, bool hasSetReset = false)
    {
        var separatedInstructions = new List<List<IInstruction>>();
        List<IInstruction> currentGroup = [];
        bool collecting = false;

        foreach (var instruction in instructions)
        {
            if (IsStartOfGroup(instruction))
            {
                collecting = true;
                currentGroup = StartNewGroup(instruction);
            }
            else if (collecting)
            {
                currentGroup?.Add(instruction);

                if (IsEndOfGroup(instruction))
                {
                    if (hasSetReset) HandleSetResetLogic(instructions, currentGroup ?? [], instruction);
                    collecting = false;
                    separatedInstructions.Add(currentGroup ?? []);
                    currentGroup = [];
                }
            }
        }
        return separatedInstructions;
    }

    private static bool IsStartOfGroup(IInstruction instruction)
    {
        return !string.IsNullOrWhiteSpace(instruction.Operator) && instruction.Operator.Contains('(');
    }

    private static bool IsEndOfGroup(IInstruction instruction)
    {
        return instruction.Operator == ")";
    }

    private static List<IInstruction> StartNewGroup(IInstruction instruction)
    {
        var newGroup = new List<IInstruction>
        {
            instruction
        };
        return newGroup;
    }

    private static void HandleSetResetLogic(List<IInstruction> instructions, List<IInstruction> currentGroup, IInstruction endInstruction)
    {
        AddSetOrReset(instructions, currentGroup, endInstruction);
    }

    private static void AddSetOrReset(List<IInstruction> instructions, List<IInstruction> currentGroup, IInstruction endInstruction)
    {
        int indexOfR = instructions.FindIndex(i => !string.IsNullOrWhiteSpace(i.Operator) && i.Operator.Contains('R'));
        int indexOfS = instructions.FindIndex(i => !string.IsNullOrWhiteSpace(i.Operator) && i.Operator.Contains('S'));
        int currIndx = instructions.IndexOf(endInstruction);
        if (indexOfR > currIndx && indexOfS < currIndx)
        {
            currentGroup.Add(instructions[indexOfR]);
        }
        else if (indexOfS > currIndx)
        {
            currentGroup.Add(instructions[indexOfS]);
        }
        else if (indexOfS > currIndx && indexOfR > currIndx && indexOfR > indexOfS)
        {
            currentGroup.Add(instructions[indexOfS]);
        }
        else if (indexOfS > currIndx && indexOfR > currIndx && indexOfR < indexOfS)
        {
            currentGroup.Add(instructions[indexOfR]);
        }
    }

    private static void AddAssignmentOperator(List<IInstruction> instructions, List<IInstruction> currentGroup)
    {
        var assignmentOperator = instructions.FirstOrDefault(i => i.Operator == "=");
        if (assignmentOperator != null) currentGroup.Add(assignmentOperator);
    }

    public static string Simplify(string expression)
    {
        var mainOperatorMatch = Regex.Match(expression, @"(AND|OR|Exclusive)");
        if (!mainOperatorMatch.Success) return expression;

        string mainOperator = mainOperatorMatch.Value;

        expression = Regex.Replace(expression, $@"\b{mainOperator}\b", "").Trim();

        expression = Regex.Replace(expression, @"(\w+)\.(\w+)", "$1_$2");

        expression = expression.Replace(")OR (", "), ");
        expression = expression.Replace(")AND (", "), ");
        expression = expression.Replace(")Exclusive (", "), ");

        string finalExpression = $"{mainOperator} ({expression})";

        return finalExpression.Trim();
    }
}