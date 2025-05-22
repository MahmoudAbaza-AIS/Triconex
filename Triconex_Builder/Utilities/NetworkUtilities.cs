using TriconexMigration.Application.Models;

namespace TriconexMigration.Application.Utilities;
public static class NetworkUtilities
{
    public static List<List<string>> GetNetworksBoundaries(List<string> data)
    {
        List<List<string>> nestedArrays = new();
        List<string> currentArray = null;

        var IsStandaloneNetwork = !data[0].ToUpper().Trim().StartsWith("FUNCTION");
        if (IsStandaloneNetwork) data.Add("END_NETWORK");

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

    public static List<IInstruction> GetInstructionsType(List<string> statements)
    {
        IInstruction instruction;
        List<IInstruction> InstructionList = new();
        foreach (var statement in statements)
        {
            if (ParsingUtilities.IsBitLogicType(statement))
            {
                instruction = new BitLogicInstruction();
                var ins = instruction.GetInstraction(statement);
                InstructionList.Add(ins);
            }
            else if (ParsingUtilities.IsLoadAndTransferType(statement))
            {
                instruction = new LoadAndTransferInstruction();
                var ins = instruction.GetInstraction(statement);
                InstructionList.Add(ins);
            }
            else if (ParsingUtilities.IsXInstructionType(statement))
            {
                instruction = new XInstruction();
                var ins = instruction.GetInstraction(statement);
                InstructionList.Add(ins);
            }
        }

        return InstructionList;
    }
}