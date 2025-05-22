using Triconex.Interfaces;
using TriconexMigration.Application.DTOS;
using TriconexMigration.Application.Interfaces;
using TriconexMigration.Application.Models;
using TriconexMigration.Application.Utilities;

namespace TriconexMigration.Application.Commands;

public class GeneralInstructionCommand : IInstructionCommand
{
    public void Execute(List<ResultParserDTO> resultList, Network network, IInstructionParser instruction, string functionName)
    {
        var result = instruction.InstructionParser(network.Instructions);
        HelperUtilities.AddResultToList(resultList, result, network.Number, functionName);
    }
}
