using TriconexMigration.Application.Models;

namespace TriconexMigration.Application.Services;
public static class ParserService
{
    public static string GetBitLogicOperator(string statement)
    {
        return statement switch
        {
            "A" or "AN" => "AND (",
            "O" or "ON" => "OR (",
            "X" or "XN" => "Exclusive (",
            "(" or ")" => "",
            "A(" => "AND (",
            _ => " , ",
        };
    }

    public static List<List<IInstruction>> SplitInstructions(List<IInstruction> instructions)
    {
        List<List<IInstruction>> multiList = [];
        int indexOfLoad = -1;

        for (int i = 0; i < instructions.Count; i++)
        {
            if (instructions[i].Operator == "=")
            {
                multiList.Add(instructions.GetRange(0, i + 1));
                instructions.RemoveRange(0, i + 1);
                i = 0;
            }
        }

        for (int i = 0; i < multiList.Count; i++)
        {
            if (multiList[i].Any(e => e.Address?.MemoryArea == "L" && e.Operator == "="))
            {
                indexOfLoad = i;
                break;
            }
        }

        if (indexOfLoad >= 0)
        {
            var itemsToProcess = multiList.Select(list => list.ToList()).ToList();

            foreach (var item in itemsToProcess)
            {
                List<(int index, IInstruction instruction)> modificationsToApply = [];
                int originalCount = item.Count;

                for (int i = 0; i < originalCount; i++)
                {
                    if (item[i].Address?.MemoryArea == "L" && item[i].Operator != "=")
                    {
                        if (multiList[indexOfLoad].Count == 2)
                        {
                            item[i].Address.Content = multiList[indexOfLoad][0].Address?.Content;
                            item[i].Address.MemoryArea = multiList[indexOfLoad][0].Address?.MemoryArea;
                            item[i].Address.Value = multiList[indexOfLoad][0].Address?.Value;
                        }
                        else if (multiList[indexOfLoad].Count > 2)
                        {
                            var instructionsToAdd = multiList[indexOfLoad]
                                .Where(x => x.Address?.MemoryArea != "L" && x.Operator != "=")
                                .ToList();

                            for (int j = 0; j < instructionsToAdd.Count; j++)
                            {
                                modificationsToApply.Add((j, instructionsToAdd[j]));
                            }
                        }
                    }
                }

                foreach (var (index, instruction) in modificationsToApply.OrderBy(x => x.index))
                {
                    item.Insert(index, instruction);
                }

                for (int i = 0; i < itemsToProcess.Count; i++)
                {
                    item.RemoveAll(instruction => instruction.Address?.MemoryArea == "L" && instruction.Operator != "=");
                }
            }
            multiList = itemsToProcess;

            multiList.Remove(multiList[indexOfLoad]);
        }
        return multiList;
    }
}
