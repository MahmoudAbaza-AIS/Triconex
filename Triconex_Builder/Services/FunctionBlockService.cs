using System.Text.RegularExpressions;
using TriconexMigration.Application.DTOS;
using TriconexMigration.Application.Models;
using TriconexMigration.Application.Utilities;

namespace TriconexMigration.Application.Services;

public class FunctionBlockService(NetworkService networkService)
{
    private readonly NetworkService networkService = networkService;

    public List<ResultParserDTO> GetFinalParser(List<FunctionBlock> functionBlocks)
    {
        List<ResultParserDTO> ResultList = new();
        foreach (var function in functionBlocks)
        {
            var result = networkService.GetFinalParser(function.Network, function.Name);
            ResultList.AddRange(result);
        }

        return ResultList;
    }

    public static List<FunctionBlock> GetFunctionBlockDetails(List<string> data)
    {
        List<FunctionBlock> functionBlockList = new();
        foreach (var function in ParsingUtilities.GetFunctionBlocksBoundaries(data))
        {
            FunctionBlock functionBlock = new()
            {
                Name = function[0],
                Version = function.Where(e => e.ToUpper().Contains("VERSION")).FirstOrDefault(),
                Inputs = ParsingUtilities.GetIOParams(function, @"^IN\d+.*$"),
                Outputs = ParsingUtilities.GetIOParams(function, @"^OUT\d+.*$"),
                LocalVariables = ParsingUtilities.GetIOParams(function, @"^STAT\d+.*$"),
                Network = NetworkService.GetNetworksDetails(function),
            };

            functionBlockList.Add(functionBlock);
        }

        return functionBlockList;
    }
}


