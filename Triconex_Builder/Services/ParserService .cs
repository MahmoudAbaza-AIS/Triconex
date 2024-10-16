using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Triconex.Models;

namespace Triconex.Services
{
    public static class ParserService
    {
        public static string GetBitLogicOperator(string statement)  
        {
            switch (statement)
            {
                case "A":
                case "AN":
                    return "AND (";
                case "O":
                case "ON":
                    return "OR (";
                case "X":
                case "XN":
                    return "Exclusive (";   
                case "(":
                case ")":
                    return "";
                case "A(":
                    return "AND (";
                default:
                    return " , ";
            }
        }

        public static List<List<IInstruction>> SplitInstructions(List<IInstruction> instructions)
        {
            List<List<IInstruction>> multiList = new List<List<IInstruction>>();

            for (int i = 0; i < instructions.Count(); i++)
            {
                if (instructions[i].Operator == "=")
                {
                    multiList.Add(instructions.GetRange(0, i + 1));
                    instructions.RemoveRange(0, i + 1);
                    i = 0;
                }
            }

            return multiList;
        }
    }
}
