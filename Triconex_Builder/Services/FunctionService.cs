using TriconexMigration.Application.DTOS;
using TriconexMigration.Application.Models;
using TriconexMigration.Application.Utilities;

namespace TriconexMigration.Application.Services;
public class FunctionService(NetworkService networkService)
{
    private readonly NetworkService networkService = networkService;

    public List<ResultParserDTO> GetFinalParser(List<Function> Functions)
    {
        List<ResultParserDTO> ResultList = [];
        foreach (var function in Functions)
        {
            var result = networkService.GetFinalParser(function.Network, function.Name);
            ResultList.AddRange(result);
        }

        return ResultList;
    }
    public static List<Function> GetFunctionsDetails(List<string> data)
    {
        List<Function> FunctionList = [];
        foreach (var function in ParsingUtilities.GetFunctionsBoundaries(data))
        {
            Function Function = new()
            {
                Name = function[0],
                Version = function.Where(e => e.ToUpper().Contains("VERSION")).FirstOrDefault() ?? "",  
                Network = NetworkService.GetNetworksDetails(function),
            };

            FunctionList.Add(Function);
        }

        return FunctionList;
    }
}