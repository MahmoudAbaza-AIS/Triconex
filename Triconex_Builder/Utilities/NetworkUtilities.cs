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
            var trimmed = item.Trim();
            if (trimmed.StartsWith("TITLE"))
            {
                currentArray = new List<string>();
            }
            else if (
                        trimmed.StartsWith("NETWORK") ||
                        trimmed.StartsWith("NETZWERK") ||
                        trimmed.StartsWith("END_FUNCTION_BLOCK") ||
                        trimmed.StartsWith("END_FUNCTION") ||
                        trimmed.StartsWith("END_ORGANIZATION_BLOCK") ||
                        trimmed.StartsWith("END_ORGANISATION_BLOCK") ||
                        trimmed.StartsWith("END_NETWORK")
                    )
            {
                if (currentArray != null && currentArray.Count > 0)
                    nestedArrays.Add(currentArray);

                if (trimmed.StartsWith("NETWORK") || trimmed.StartsWith("NETZWERK"))
                    currentArray = [];
                else
                    currentArray = null;
            }
            else
            {
                currentArray?.Add(item);
            }
        }

        if (nestedArrays.Count == 0)
        {
            var body = data.Where(line =>
            {
                var token = line.Trim();
                return token.Length > 0
                    && !token.StartsWith("END_")
                    && !token.StartsWith("FUNCTION")
                    && !token.StartsWith("ORGANIZATION_BLOCK")
                    && !token.StartsWith("ORGANISATION_BLOCK")
                    && !token.StartsWith("DATA_BLOCK")
                    && !token.StartsWith("VERSION")
                    && !token.StartsWith("VAR")
                    && !token.StartsWith("BEGIN")
                    && !token.Contains(" : ");
            }).ToList();
            if (body.Count > 0)
                nestedArrays.Add(body);
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