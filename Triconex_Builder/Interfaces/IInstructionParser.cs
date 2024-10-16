using Triconex.Models;

namespace Triconex.Interfaces
{
    public interface IInstructionParser
    {   
        string InstructionParser(List<IInstruction> instructions);            
    }
}
