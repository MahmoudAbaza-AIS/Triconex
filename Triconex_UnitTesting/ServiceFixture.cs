using TriconexMigration.Application.Commands;
using TriconexMigration.Application.Factory;
using TriconexMigration.Application.Parser;
using TriconexMigration.Application.Services;

namespace TriconexMigration.UnitTesting;

public class ServiceFixture
{
    public NetworkService NetworkService { get; private set; }

    public ServiceFixture()
    {
        var simpleLogicParser = new SimpleLogicParser();
        var setResetParser = new SetResetParser();
        var multiLogicParser = new MultiLogicParser(simpleLogicParser, setResetParser);
        var timerParser = new TimerParser();
        
        var simpleLogicCommand = new SimpleLogicCommand();
        var generalInstructionCommand = new GeneralInstructionCommand();
       
        var instructionCommandFactory = new InstructionCommandFactory(simpleLogicCommand, generalInstructionCommand);
        var instructionParserFactory = new InstructionParserFactory(
            simpleLogicParser,
            setResetParser,
            timerParser,
            multiLogicParser
        );

        NetworkService = new NetworkService(instructionParserFactory, instructionCommandFactory);
    }
}