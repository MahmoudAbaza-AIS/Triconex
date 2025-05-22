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
        bool containsR = network.Instructions.Any(e => e.Operator.Contains('R'));
        bool containsBracket = network.Instructions.Any(e => e.Operator.Contains('('));
        bool containsHash = network.Instructions.Any(e => e.Address.MemoryArea.Contains("#"));
        bool containsCall = network.Instructions.Any(e => e.Address.MemoryArea.ToUpper().Contains("CALL"));
        bool hasLogicInstruction = network.Instructions.Any(e => !containsCall && !containsR && !containsBracket && !containsHash);

        if (hasLogicInstruction) return _simpleLogicParser;
        if (containsR && !containsBracket) return _setResetParser;
        if (containsHash) return _timerParser;
        if (containsBracket || (containsR && containsBracket)) return _multiLogicParser;
        throw new ApplicationException("Network is not suitable to any of the Available Parsers");
    }
}
