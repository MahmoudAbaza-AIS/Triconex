using Triconex.Interfaces;
using TriconexMigration.Application.Interfaces;
using TriconexMigration.Application.Models;
using TriconexMigration.Application.Parser;

namespace TriconexMigration.Application.Factory;

public class InstructionParserFactory(
    SimpleLogicParser simpleLogicParser,
    SetResetParser setResetParser,
    TimerParser timerParser,
    MultiLogicParser multiLogicParser) : IInstructionParserFactory
{
    private readonly SimpleLogicParser _simpleLogicParser = simpleLogicParser;
    private readonly SetResetParser _setResetParser = setResetParser;
    private readonly TimerParser _timerParser = timerParser;
    private readonly MultiLogicParser _multiLogicParser = multiLogicParser;

    public IInstructionParser Create(Network network)
    {
        var operators = network.Instructions.Select(e => e.Operator ?? "").ToList();
        bool containsReset = operators.Any(o => o == "R");
        bool containsBracket = operators.Any(o => o.Contains('('));
        bool containsTimer = operators.Any(o => o is "SP" or "SD" or "SF" or "SS" or "SE" or "SA" or "SI" or "SV");
        bool containsCall = operators.Any(o => o is "CALL" or "UC" or "CC");
        bool containsWord = operators.Any(o => o is "L" or "T" || IsMathOrCompare(o));
        bool hasLogicInstruction = network.Instructions.Any() && !containsReset && !containsBracket;

        if (containsTimer) return _timerParser;
        if (containsCall || containsWord) return _simpleLogicParser;
        if (hasLogicInstruction) return _simpleLogicParser;
        if (containsReset && !containsBracket) return _setResetParser;
        if (containsBracket || containsReset) return _multiLogicParser;
        throw new ApplicationException("Network is not suitable to any of the Available Parsers");
    }

    private static bool IsMathOrCompare(string op)
        => op is "*R" or "*I" or "*D" or "+R" or "+I" or "+D" or "-R" or "-I" or "-D" or "/R" or "/I" or "/D" or "MOD"
            or "==I" or "<>I" or ">=I" or "<=I" or ">I" or "<I"
            or "==R" or "<>R" or ">=R" or "<=R" or ">R" or "<R"
            or "==D" or "<>D" or ">=D" or "<=D" or ">D" or "<D";
}
