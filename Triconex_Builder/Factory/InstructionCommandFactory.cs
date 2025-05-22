using Triconex.Interfaces;
using TriconexMigration.Application.Commands;
using TriconexMigration.Application.Interfaces;
using TriconexMigration.Application.Parser;

namespace TriconexMigration.Application.Factory;

public class InstructionCommandFactory(
    SimpleLogicCommand simpleLogicCommand,
    GeneralInstructionCommand generalInstructionCommand) : IInstructionCommandFactory
{
    private readonly SimpleLogicCommand simpleLogicCommand = simpleLogicCommand;
    private readonly GeneralInstructionCommand generalInstructionCommand = generalInstructionCommand;

    public IInstructionCommand GetCommand(IInstructionParser parser) => parser is SimpleLogicParser ? simpleLogicCommand : generalInstructionCommand;
}