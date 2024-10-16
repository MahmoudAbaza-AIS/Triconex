using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Triconex.DTOS;
using Triconex.Interfaces;
using Triconex.Models;
using Triconex.Parser;
using Triconex.Services;

namespace Triconex.Services
{
    public static class FunctionBlockService
    {
        public static List<ResultParserDTO> GetFinalParser(List<FunctionBlock> functionBlocks)
        {
            List<ResultParserDTO> ResultList = new List<ResultParserDTO>();
            foreach (var function in functionBlocks)
            {
                var result = NetworkService.GetFinalParser(function.Network, function.Name);
                ResultList.AddRange(result);
            }

            return ResultList;
        }
        
        public static List<FunctionBlock> GetFunctionBlockDetails(List<string> data)
        {
            List<FunctionBlock> functionBlockList = new List<FunctionBlock>();
            foreach (var function in GetFunctionBlocksBoundaries(data))
            {
                FunctionBlock functionBlock = new FunctionBlock()
                {
                    Name = function[0],
                    Version = function.Where(e => e.ToUpper().Contains("VERSION")).FirstOrDefault(),
                    Inputs = GetIOParams(function, @"^IN\d+.*$"),
                    Outputs = GetIOParams(function, @"^OUT\d+.*$"),
                    LocalVariables = GetIOParams(function, @"^STAT\d+.*$"),
                    Network = NetworkService.GetNetworksDetails(function),
                };

                functionBlockList.Add(functionBlock);
            }

            return functionBlockList;
        }

        #region Private Utilities
        private static List<List<string>> GetFunctionBlocksBoundaries(List<string> data)
        {
            List<List<string>> nestedArrays = new List<List<string>>();
            List<string> currentArray = null;

            foreach (var item in data)
            {
                if (item.Trim().StartsWith("FUNCTION_BLOCK"))
                {
                    currentArray = new List<string> { item };
                }
                else if (item.Trim().StartsWith("END_FUNCTION_BLOCK"))
                {
                    currentArray?.Add(item);
                    if (currentArray != null)
                    {
                        nestedArrays.Add(currentArray);
                    }
                }
                else
                {
                    if (item.Trim().Split(" ")?[0] == "FUNCTION") break;
                    currentArray?.Add(item);
                }
            }

            return nestedArrays;
        }

        private static List<FunctionBlockAttribute> GetIOParams(List<string> data, string regex)
        {
            List<FunctionBlockAttribute> FB = new List<FunctionBlockAttribute>();
            var DataList = data.Where(e => new Regex(regex).IsMatch(e.Trim())).ToList();
            foreach (var item in DataList)
            {
                FB.Add(new FunctionBlockAttribute
                {
                    Name = Regex.Match(item.Trim(), @"^([^:]+)").Value,
                    DataType = Regex.Match(item.Trim(), @"(?<=:)[^;]+").Value,
                    InitialValue = "N/A"
                });
            }

            return FB;
        }

        #endregion
    }
}


