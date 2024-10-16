using Triconex.DTOS;
using Triconex.Interfaces;
using Triconex.Models;
using Triconex.Parser;
using Triconex.Services;

namespace Triconex.Services
{
    public static class FunctionService
    {
        public static List<ResultParserDTO> GetFinalParser(List<Function> Functions)
        {
            List<ResultParserDTO> ResultList = new List<ResultParserDTO>();
            foreach (var function in Functions)
            {
                var result = NetworkService.GetFinalParser(function.Network, function.Name);
                ResultList.AddRange(result);
            }

            return ResultList;
        }
        public static List<Function> GetFunctionsDetails(List<string> data)
        {
            List<Function> FunctionList = new List<Function>();
            foreach (var function in GetFunctionsBoundaries(data)) 
            {
                Function Function = new Function()  
                {
                    Name = function[0],
                    Version = function.Where(e => e.ToUpper().Contains("VERSION")).FirstOrDefault(),
                    Network = NetworkService.GetNetworksDetails(function),
                };

                FunctionList.Add(Function); 
            }

            return FunctionList;    
        }


        #region Private Utilities
        private static List<List<string>> GetFunctionsBoundaries(List<string> data)
        {
            List<List<string>> nestedArrays = new List<List<string>>();
            List<string> currentArray = null;

            foreach (var item in data)
            {
                if (item.Trim().Split(" ")?[0] == "FUNCTION")
                {
                    currentArray = new List<string> { item };
                }
                else if (item.Trim() == "END_FUNCTION")
                {
                    currentArray?.Add(item);
                    if (currentArray != null)
                    {
                        nestedArrays.Add(currentArray);
                    }
                }
                else
                {
                    currentArray?.Add(item);
                }
            }

            return nestedArrays;
        }

        #endregion
    }
}