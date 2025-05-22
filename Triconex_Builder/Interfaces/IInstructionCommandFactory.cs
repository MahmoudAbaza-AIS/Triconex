using Triconex.Interfaces;

namespace TriconexMigration.Application.Interfaces;

public interface IInstructionCommandFactory
{
    IInstructionCommand GetCommand(IInstructionParser parser);
}
