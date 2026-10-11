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
        if (InstructionsList.Any(i => i.Operator is "CALL" or "UC" or "CC"))
            return FormatCall(InstructionsList);

        bool hasWord = InstructionsList.Any(i => i.Operator is "L" or "T" || IsMathOrCompare(i.Operator));
        if (!hasWord)
            return ParseBoolean(InstructionsList);

        var (remaining, wordEquations) = ExtractTransfers(InstructionsList);
        string boolean = "";
        if (remaining.Any(i => i.Operator is "S" or "R") && !remaining.Any(i => i.Operator == "="))
            boolean = FormatLatches(remaining);
        else if (remaining.Any(i => i.Operator is "A" or "AN" or "O" or "ON" or "X" or "XN" or "A(" or "O(" or "X(" or "=" or "NOT"))
            boolean = ParseBoolean(remaining);

        if (string.IsNullOrWhiteSpace(boolean) || boolean.Contains("()") || boolean.Contains("{lhs}"))
            boolean = "";

        var parts = new[] { boolean }.Concat(wordEquations).Where(p => !string.IsNullOrWhiteSpace(p));
        var joined = string.Join(";;", parts);
        return string.IsNullOrWhiteSpace(joined) ? ParseBoolean(InstructionsList) : joined;
    }

    private string ParseBoolean(List<IInstruction> InstructionsList)
    {
        if (InstructionsList.Count == 0) return string.Empty;

        var instructions = InstructionsList.Where(e => e.Address?.MemoryArea != "BLD" && e.Operator != "NOP" && e.Operator != "BLD").ToList();

        if (instructions == null) return "EMPTY!";

        if (instructions.Any(i => (i.Operator ?? "").Contains('(')))
            return ParseBracketedNetwork(instructions);

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

    private static string ParseBracketedNetwork(List<IInstruction> instructions)
    {
        var end = instructions.FindIndex(i => i.Operator == "=");
        if (end < 0) end = instructions.Count;
        var lhsIns = end < instructions.Count ? instructions[end] : null;
        var lhs = (lhsIns?.Address?.MemoryArea ?? "") + (lhsIns?.Address?.Value ?? "");
        if (string.IsNullOrWhiteSpace(lhs) || lhsIns?.Operator != "=")
        {
            var jump = instructions.FirstOrDefault(i => i.Operator is "JC" or "JNB");
            lhs = jump == null ? "{lhs}" : Operand(jump);
        }

        var cursor = new Cursor { Seq = instructions, Limit = end };
        var node = ParseSequence(cursor, stopOnClose: false);
        var body = Format(node);
        if (instructions.Any(i => i.Operator == "NOT"))
            body = "!" + body.Replace(" ", "");
        return $"{lhs} = {body}";
    }

    private sealed class Cursor
    {
        public required List<IInstruction> Seq { get; init; }
        public int Pos { get; set; }
        public int Limit { get; init; }
    }

    private sealed class Node
    {
        public string? Atom { get; init; }
        public string? Gate { get; init; }
        public List<Node> Args { get; init; } = [];
    }

    private static Node ParseSequence(Cursor cursor, bool stopOnClose)
    {
        var orTerms = new List<Node>();
        var andTerms = new List<Node>();

        void FlushAnd()
        {
            if (andTerms.Count == 0) return;
            orTerms.Add(andTerms.Count == 1 ? andTerms[0] : new Node { Gate = "AND", Args = [.. andTerms] });
            andTerms.Clear();
        }

        while (cursor.Pos < cursor.Limit)
        {
            var ins = cursor.Seq[cursor.Pos];
            var op = ins.Operator ?? "";
            if (op == ")")
            {
                if (stopOnClose)
                {
                    cursor.Pos++;
                    break;
                }
                cursor.Pos++;
                continue;
            }

            if (op is "A(" or "O(" or "X(")
            {
                cursor.Pos++;
                var inner = ParseSequence(cursor, true);
                if (IsBlank(inner)) continue;

                if (op is "O(" or "X(")
                {
                    FlushAnd();
                    var gate = op == "X(" ? "Exclusive" : "OR";
                    if (inner.Gate == gate)
                        orTerms.AddRange(inner.Args.Where(a => !IsBlank(a)));
                    else
                        orTerms.Add(inner.Gate == null ? inner : ApplyBracket(op, inner));
                    continue;
                }

                var bracket = ApplyBracket(op, inner);
                bool combining = andTerms.Count > 0 || HasMoreAndTerms(cursor);
                if (bracket.Gate == "AND" && combining)
                {
                    if (bracket.Args.Count == 1)
                        andTerms.Add(bracket.Args[0]);
                    else
                        andTerms.AddRange(bracket.Args);
                }
                else
                    andTerms.Add(bracket);
                continue;
            }

            if (op == "L"
                && cursor.Pos + 2 < cursor.Limit
                && cursor.Seq[cursor.Pos + 1].Operator == "L"
                && IsCompare(cursor.Seq[cursor.Pos + 2].Operator))
            {
                var cmp = cursor.Seq[cursor.Pos + 2].Operator ?? "";
                andTerms.Add(new Node
                {
                    Atom = $"{cmp}({Operand(cursor.Seq[cursor.Pos])}, {Operand(cursor.Seq[cursor.Pos + 1])})"
                });
                cursor.Pos += 3;
                continue;
            }

            if (string.IsNullOrWhiteSpace(ins.Address?.MemoryArea) && op is "O" or "ON" or "A" or "AN" or "X" or "XN")
            {
                FlushAnd();
                cursor.Pos++;
                continue;
            }

            if (op is "O" or "ON" or "X" or "XN")
            {
                FlushAnd();
                orTerms.Add(Atom(ins));
                cursor.Pos++;
                continue;
            }

            if (op is "A" or "AN")
            {
                andTerms.Add(Atom(ins));
                cursor.Pos++;
                continue;
            }

            cursor.Pos++;
        }

        FlushAnd();
        if (orTerms.Count == 0) return new Node { Atom = "" };
        if (orTerms.Count == 1) return orTerms[0];
        return new Node { Gate = "OR", Args = orTerms };
    }

    private static bool HasMoreAndTerms(Cursor cursor)
    {
        for (int i = cursor.Pos; i < cursor.Limit; i++)
        {
            var op = cursor.Seq[i].Operator ?? "";
            if (op is "A" or "AN" or "A(") return true;
            if (op is "O" or "ON" or "O(" or "X" or "XN" or "X(" or ")" or "=") return false;
        }
        return false;
    }

    private static Node ApplyBracket(string op, Node inner) => op switch
    {
        "A(" when inner.Gate == "OR" && inner.Args.All(a => a.Gate == null) => new Node { Gate = "AND", Args = [inner] },
        "O(" when inner.Gate != "OR" => new Node { Gate = "OR", Args = [inner] },
        "X(" when inner.Gate != "Exclusive" => new Node { Gate = "Exclusive", Args = [inner] },
        _ => inner
    };

    private static Node Atom(IInstruction instruction)
    {
        var op = instruction.Operator ?? "";
        var neg = op is "AN" or "ON" or "XN";
        return new Node { Atom = (neg ? "!" : "") + instruction.Address?.MemoryArea + instruction.Address?.Value };
    }

    private static string Format(Node node)
    {
        if (node.Gate == null) return node.Atom ?? "";
        if (node.Gate == "AND"
            && node.Args.Count == 2
            && node.Args[0].Gate == "OR"
            && node.Args[0].Args.Any(a => a.Gate == "AND")
            && node.Args[1].Gate == "OR")
            return $"{Format(node.Args[0])}, AND ({Format(node.Args[1])})";

        return $"{node.Gate} ({string.Join(" , ", node.Args.Select(Format))})";
    }

    private static bool IsBlank(Node node)
        => node.Gate == null ? string.IsNullOrWhiteSpace(node.Atom) : node.Args.Count == 0 || node.Args.All(IsBlank);

    private static string FormatCall(List<IInstruction> instructions)
    {
        var callIndex = instructions.FindIndex(i => i.Operator is "CALL" or "UC" or "CC");
        var call = instructions[callIndex];
        var before = instructions.Take(callIndex).ToList();
        var after = instructions.Skip(callIndex + 1).ToList();
        var (_, beforeEquations) = ExtractTransfers(before);
        var (_, afterEquations) = ExtractTransfers(after);
        var prefix = before;
        var enable = prefix.Where(i => i.Operator is "A" or "AN" or "O" or "ON").ToList();
        bool invertEnable = prefix.Any(i => i.Operator is "JC" or "JCB");
        string enableText = "";
        if (enable.Count == 1)
            enableText = (enable[0].Operator is "AN" or "ON" ? "!" : "") + Operand(enable[0]);
        else if (enable.Count > 1)
        {
            var gate = enable.Any(i => i.Operator is "O" or "ON") ? "OR" : "AND";
            enableText = $"{gate}({string.Join(", ", enable.Select(i => (i.Operator is "AN" or "ON" ? "!" : "") + Operand(i)))})";
        }
        if (invertEnable && enableText.Length > 0)
            enableText = "!" + enableText;

        var parameters = new List<string>();
        foreach (var ins in after)
        {
            var op = ins.Operator ?? "";
            if (op is "NOP" or "BLD" or ")" or "(" || op.EndsWith(':')) continue;
            if (op is "A" or "AN" or "O" or "ON" or "=" or "S" or "R" or "L" or "T" or "OPN" or "CDB" or "LAR1" or "LAR2" or "SET" or "SAVE" or "CLR") continue;
            var raw = ins.Address?.Content ?? "";
            var value = raw.Replace(":=", " ").Replace("(", " ").Replace(")", " ");
            value = string.Join("", value.Split(' ', StringSplitOptions.RemoveEmptyEntries));
            if (string.IsNullOrWhiteSpace(op) || string.IsNullOrWhiteSpace(value) || op == value) continue;
            parameters.Add($"{op}={value}");
        }

        var (block, instance) = CallTarget(call);
        string callText = parameters.Count == 0
            ? (enableText.Length > 0 ? $"{block}(EN={enableText})" : $"{block} = CALL")
            : $"{block}({string.Join(", ", parameters)})";
        if (!string.IsNullOrWhiteSpace(instance))
            callText = $"{instance} = {callText}";

        var pieces = new List<string>();
        pieces.AddRange(beforeEquations);
        if (prefix.Any(i => i.Operator == "="))
        {
            var assigned = new SimpleLogicParser().ParseBoolean(prefix.Where(i => i.Operator is not "CALL").ToList());
            if (!string.IsNullOrWhiteSpace(assigned) && !assigned.Contains("{lhs}") && !assigned.Contains("()"))
                pieces.Add(assigned);
        }
        pieces.Add(callText);
        pieces.AddRange(afterEquations);
        var coilSource = after.Where(i => i.Operator is "A" or "AN" or "O" or "ON" or "X" or "XN" or "=" or "NOT").ToList();
        if (coilSource.Any(i => i.Operator == "="))
        {
            var coil = new SimpleLogicParser().ParseBoolean(coilSource);
            if (!string.IsNullOrWhiteSpace(coil) && !coil.Contains("{lhs}") && !coil.Contains("()"))
                pieces.Add(coil);
        }
        return string.Join(";;", pieces.Where(p => !string.IsNullOrWhiteSpace(p)));
    }

    private static (string Block, string Instance) CallTarget(IInstruction call)
    {
        var parts = (call.Address?.Content ?? "")
            .Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Select(p => p.Trim('"', ',', '(', ')'))
            .Where(p => p.Length > 0 && p != ",")
            .ToList();
        if (parts.Count >= 4 && parts[0] is "FB" or "FC" or "SFB" or "SFC" && parts[2] is "DB" or "DI")
            return (parts[0] + parts[1], parts[2] + parts[3]);
        if (parts.Count >= 2 && parts[0] is "FB" or "FC" or "SFB" or "SFC")
            return (parts[0] + parts[1], "");
        var name = parts.FirstOrDefault() ?? call.Address?.MemoryArea ?? "CALL";
        return (name.Trim('"'), "");
    }

    private static (List<IInstruction> Remaining, List<string> Equations) ExtractTransfers(List<IInstruction> instructions)
    {
        var rest = new List<IInstruction>();
        var equations = new List<string>();
        for (int i = 0; i < instructions.Count;)
        {
            if (IsStatus(instructions[i]))
            {
                i++;
                continue;
            }
            if (TryTakeTransfer(instructions, i, out int count, out var eqs))
            {
                equations.AddRange(eqs);
                i += count;
                continue;
            }
            rest.Add(instructions[i]);
            i++;
        }
        return (rest, equations);
    }

    private static bool TryTakeTransfer(List<IInstruction> list, int i, out int count, out List<string> equations)
    {
        equations = [];
        count = 0;
        if (list[i].Operator != "L") return false;
        if (i + 2 < list.Count && list[i + 1].Operator == "L" && IsCompare(list[i + 2].Operator))
            return false;
        if (i + 2 < list.Count && list[i + 1].Operator == "L" && IsMath(list[i + 2].Operator) && i + 3 < list.Count && list[i + 3].Operator == "T")
        {
            equations.Add($"{Operand(list[i + 3])} = {list[i + 2].Operator}({Operand(list[i])}, {Operand(list[i + 1])})");
            count = 4;
            return true;
        }
        if (i + 2 < list.Count && IsUnary(list[i + 1].Operator) && list[i + 2].Operator == "T")
        {
            equations.Add($"{Operand(list[i + 2])} = {list[i + 1].Operator}({Operand(list[i])})");
            count = 3;
            return true;
        }
        int next = SkipBlockOperations(list, i + 1);
        if (next < list.Count && list[next].Operator == "T")
        {
            var source = Operand(list[i]);
            int n = next;
            while (n < list.Count && list[n].Operator == "T")
            {
                equations.Add($"{Operand(list[n])} = {source}");
                n++;
            }
            count = n - i;
            return true;
        }
        return false;
    }

    private static int SkipBlockOperations(List<IInstruction> list, int index)
    {
        while (index < list.Count && list[index].Operator is "OPN" or "CDB" or "LAR1" or "LAR2")
            index++;
        return index;
    }

    private static string FormatLatches(List<IInstruction> instructions)
    {
        bool invert = instructions.Any(i => i.Operator is "JC" or "JCB");
        var contacts = instructions.Where(i => i.Operator is "A" or "AN" or "O" or "ON" or "X" or "XN").ToList();
        var latches = instructions.Where(i => i.Operator is "S" or "R").ToList();
        string cond = "";
        if (contacts.Count == 1)
            cond = (contacts[0].Operator is "AN" or "ON" or "XN" ? "!" : "") + Operand(contacts[0]);
        else if (contacts.Count > 1)
        {
            var gate = contacts.Any(c => c.Operator is "O" or "ON") ? "OR" : "AND";
            cond = $"{gate}({string.Join(", ", contacts.Select(c => (c.Operator is "AN" or "ON" or "XN" ? "!" : "") + Operand(c)))})";
        }
        if (invert && cond.Length > 0)
            cond = cond.StartsWith('!') ? cond[1..] : "!" + cond;

        return string.Join(";;", latches.Select(l => $"{Operand(l)} = {l.Operator}({cond})"));
    }

    private static bool IsStatus(IInstruction instruction)
    {
        var op = instruction.Operator ?? "";
        if (op is "SET" or "SAVE" or "CLR" or "NOP" or "BLD" or "LAR1" or "LAR2" or "CAR" or "TAK" or "BE" or "BEU")
            return true;
        var bit = (instruction.Address?.MemoryArea ?? "") + (instruction.Address?.Value ?? "");
        if (op == "A" && bit == "BR") return true;
        if (op == "AN" && bit == "OV") return true;
        return false;
    }

    private static string Operand(IInstruction instruction)
    {
        var area = (instruction.Address?.MemoryArea ?? "").Trim().TrimStart('#');
        var value = (instruction.Address?.Value ?? "").Trim();
        return area + value;
    }

    private static bool IsCompare(string? op)
        => op is "==I" or "<>I" or ">=I" or "<=I" or ">I" or "<I"
            or "==R" or "<>R" or ">=R" or "<=R" or ">R" or "<R"
            or "==D" or "<>D" or ">=D" or "<=D" or ">D" or "<D";

    private static bool IsMath(string? op)
        => op is "*R" or "*I" or "*D" or "+R" or "+I" or "+D" or "-R" or "-I" or "-D" or "/R" or "/I" or "/D" or "MOD";

    private static bool IsMathOrCompare(string? op) => IsMath(op) || IsCompare(op);

    private static bool IsUnary(string? op)
        => op is "RND" or "RND+" or "RND-" or "ABS" or "NEG" or "NEGI" or "NEGR" or "DTR" or "ITD" or "BTD" or "DTB" or "BTI" or "ITB";
}
