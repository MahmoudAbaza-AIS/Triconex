using System;
using System.Collections.Generic;
using System.Linq;
using Triconex.Interfaces;
using Triconex.Models;
using Triconex.Services;

namespace Triconex.Parser
{
    public class SampleLogicParser : IInstructionParser
    {
        public string InstructionParser(List<IInstruction> InstructionsList)
        {
            if (InstructionsList.Count() == 0)
                return string.Empty;

            var Instructions = InstructionsList.Where(e => e.Address.MemoryArea != "BLD").ToList();

            if (Instructions.Count() == 2)
            {
                return Instructions[1].Address.MemoryArea + Instructions[1].Address.Value + " = " +
                       (Instructions[0].Operator.Contains("N") ? "NOT " : "") +
                       Instructions[0].Address.MemoryArea + 
                       Instructions[0].Address.Value;
            }


            var endOfInstruction = Instructions.Count() - 1;
            var result = Instructions[endOfInstruction].Address.MemoryArea + Instructions[endOfInstruction].Address.Value + " " + Instructions[endOfInstruction].Operator + " ";
            var StarterOperator = ParserService.GetBitLogicOperator(Instructions[0].Operator);

            for (var i = 0; i < endOfInstruction; i++)
            {
                var haveNot = (Instructions[i].Operator.Contains("N")) ? "NOT " : "";
                var CurrentOpperator = ParserService.GetBitLogicOperator(Instructions[i].Operator);
                var PreviousOpperator = i > 0 ? ParserService.GetBitLogicOperator(Instructions[i - 1].Operator) : "";
                var IncomingOpperator = ParserService.GetBitLogicOperator(Instructions[i + 1].Operator);


                if (CurrentOpperator == PreviousOpperator)
                {

                    result += " , " + haveNot + Instructions[i].Address.MemoryArea + Instructions[i].Address.Value;

                    if (((i + 1) >= endOfInstruction) || (Instructions[i].Address.MemoryArea.Count() > 0 && CurrentOpperator != IncomingOpperator))
                        result += ")";
                }
                else
                {
                    if (i > 0 && StarterOperator == CurrentOpperator && Instructions[i - 1].Address.MemoryArea.Count() > 0)
                        CurrentOpperator = " , ";
                    
                    result += CurrentOpperator + haveNot + Instructions[i].Address.MemoryArea + Instructions[i].Address.Value;
                }
            }


            var numOfOpenningBracket = result.Split("(").Count();
            var numOfCloseBracket = result.Split(")").Count();

            for (var i = 0; i < numOfOpenningBracket - numOfCloseBracket; i++)
            {
                result += ")";
            }

            return result;
        }
    }
}
