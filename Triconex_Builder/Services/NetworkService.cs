using Microsoft.Extensions.Logging;
using Triconex.Interfaces;
using TriconexMigration.Application.DTOS;
using TriconexMigration.Application.Interfaces;
using TriconexMigration.Application.Models;
using TriconexMigration.Application.Parser;
using TriconexMigration.Application.Utilities;

namespace TriconexMigration.Application.Services;

public class NetworkService(IInstructionParserFactory instructionParserFactory, IInstructionCommandFactory instructionCommandFactory)
{
    private readonly IInstructionParserFactory _instructionParserFactory = instructionParserFactory;
    private readonly IInstructionCommandFactory _instructionCommandFactory = instructionCommandFactory;

    public List<ResultParserDTO> GetFinalParser(List<Network> networks, string functionName)
    {
        var resultList = new List<ResultParserDTO>();

        foreach (var network in networks)
        {
            try
            {
                IInstructionParser instruction = _instructionParserFactory.Create(network);
                IInstructionCommand command = _instructionCommandFactory.GetCommand(instruction);
                command.Execute(resultList, network, instruction, functionName);
            }
            catch (Exception)
            {
                var raw = string.Join(" | ", network.Instructions?.Select(i => $"{i.Operator} {i.Address?.MemoryArea} {i.Address?.Value}".Trim()) ?? []);
                resultList.Add(new ResultParserDTO
                {
                    Logic = string.IsNullOrWhiteSpace(raw) ? "UNPARSED" : raw,
                    NetworkNumber = network.Number,
                    FunctionBlock = functionName ?? ""
                });
            }
        }

        resultList.ForEach(result =>
        {
            if (result.Logic.Contains(".")) result.Logic = result.Logic.Replace(".", "_");
            result.Logic = ComplexEquationParser.Execute(result.Logic);
        });

        return resultList;
    }

    public static List<Network> GetNetworksDetails(List<string> function)
    {
        List<Network> NetworkList = [];
        var networkBlocks = NetworkUtilities.GetNetworksBoundaries(function);
        int networkNum = 1;

        foreach (var network in networkBlocks)
        {
            var InstructionList = NetworkUtilities.GetInstructionsType(network);
            NetworkList.Add(new Network
            {
                Number = networkNum++,
                Instructions = InstructionList,
            });
        }
        return NetworkList;
    }
}
