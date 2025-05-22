using Triconex.Interfaces;
using TriconexMigration.Application.DTOS;
using TriconexMigration.Application.Interfaces;
using TriconexMigration.Application.Models;
using TriconexMigration.Application.Services;
using TriconexMigration.Application.Utilities;

namespace TriconexMigration.Application.Commands;

public class SimpleLogicCommand : IInstructionCommand
{
    public void Execute(List<ResultParserDTO> resultList, Network network,IInstructionParser instruction, string functionName)
    {
        int numOfOutputs = network.Instructions.Count(e => e.Operator == "=");

        if (numOfOutputs > 1)
        {
            var instructionsList = ParserService.SplitInstructions(network.Instructions);
            foreach (var instructionSet in instructionsList)
            {
                var result = instruction.InstructionParser(instructionSet);
                HelperUtilities.AddResultToList(resultList, result, network.Number, functionName);
            }
        }
        else
        {
            var result = instruction.InstructionParser(network.Instructions);
            HelperUtilities.AddResultToList(resultList, result, network.Number, functionName);
        }
    }
}
