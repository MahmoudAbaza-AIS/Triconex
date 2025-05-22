using Triconex.Interfaces;
using TriconexMigration.Application.DTOS;
using TriconexMigration.Application.Models;

namespace TriconexMigration.Application.Interfaces;

public interface IInstructionCommand
{
    void Execute(List<ResultParserDTO> resultList, Network network, IInstructionParser instruction, string functionName);
}
