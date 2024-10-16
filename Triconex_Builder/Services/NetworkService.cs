using System;
using System.Text.RegularExpressions;
using Triconex.DTOS;
using Triconex.Interfaces;
using Triconex.Models;
using Triconex.Parser;


namespace Triconex.Services
{
    public static class NetworkService
    {
        public static List<ResultParserDTO> GetFinalParser(List<Network> Network, string FunctionName)
        {
            List<ResultParserDTO> ResultList = new List<ResultParserDTO>();
            foreach (var network in Network)
            {
                IInstructionParser instruction;
                var sampleInstruction = network.Instructions.Any(e => !(e.Address.MemoryArea.ToUpper().Contains("CALL")));

                if (sampleInstruction)
                {
                    instruction = new SampleLogicParser();
                    var NumOfOutput = network.Instructions.Count(e => e.Operator == "=");
                    if (NumOfOutput > 1)
                    {
                        var InstructionsList = ParserService.SplitInstructions(network.Instructions);
                        foreach (var Instruction in InstructionsList)
                        {
                            var result = instruction.InstructionParser(Instruction);
                            ResultList.Add(new ResultParserDTO
                            {
                                Logic = result,
                                NetworkNumber = network.Number,
                                FunctionBlock = FunctionName ?? ""
                            });
                        }
                    }
                    else
                    {
                        var result = instruction.InstructionParser(network.Instructions);
                        ResultList.Add(new ResultParserDTO
                        {
                            Logic = result,
                            NetworkNumber = network.Number,
                            FunctionBlock = FunctionName ?? ""
                        });
                    }
                }
                else { }
            }

            return ResultList;
        }
        public static List<Network> GetNetworksDetails(List<string> function)   
        {
            List<Network> NetworkList = new List<Network>();
            var networkBlocks = GetNetworksBoundaries(function);
            int networkNum = 1;

            foreach (var network in networkBlocks)
            {
                var InstructionList = GetInstructionsType(network);
                NetworkList.Add(new Network
                {
                    Number = networkNum++,
                    Instructions = InstructionList,
                });
            }

            return NetworkList;
        }


        #region Private Utilities

        private static List<List<string>> GetNetworksBoundaries(List<string> data)
        {
            List<List<string>> nestedArrays = new List<List<string>>();
            List<string> currentArray = null;

            #region -- Track Standalone Network --
            var IsStandaloneNetwork = !(data[0].ToUpper().Trim().StartsWith("FUNCTION"));
            if (IsStandaloneNetwork) data.Add("END_NETWORK");
            #endregion 

            foreach (var item in data)
            {
                if (item.Trim().StartsWith("TITLE"))
                {
                    currentArray = new List<string>();
                }
                else if (
                            item.Trim().StartsWith("NETWORK") ||
                            item.Trim().StartsWith("END_FUNCTION_BLOCK") ||
                            item.Trim().StartsWith("END_FUNCTION") ||
                            item.Trim().StartsWith("END_NETWORK")
                        )
                {
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

        private static List<IInstruction> GetInstructionsType(List<string> statements)
        {
            IInstruction instruction;
            List<IInstruction> InstructionList = new List<IInstruction>();
            foreach (var statement in statements)
            {
                if (IsBitLogicType(statement))
                {
                    instruction = new BitLogicInstruction();
                    var ins = instruction.GetInstraction(statement);
                    InstructionList.Add(ins);
                }
                else if (IsLoadAndTransferType(statement))
                {
                    instruction = new LoadAndTransferInstruction();
                    var ins = instruction.GetInstraction(statement);
                    InstructionList.Add(ins);
                }
                else if (IsXInstructionType(statement))
                {
                    instruction = new XInstruction();
                    var ins = instruction.GetInstraction(statement);
                    InstructionList.Add(ins);
                }
            }

            return InstructionList;
        }

        #endregion

        #region Network Valdation
        private static bool IsBitLogicType(string statement)
                => new Regex(@"^(AN|A|R|=)").IsMatch(statement.Trim());

        private static bool IsLoadAndTransferType(string statement)
                => new Regex(@"").IsMatch(statement.Trim());

        private static bool IsXInstructionType(string statement)
                => new Regex(@"").IsMatch(statement.Trim());
        #endregion
    }
}
