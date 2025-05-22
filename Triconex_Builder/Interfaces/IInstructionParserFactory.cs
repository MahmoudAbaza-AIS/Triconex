using Triconex.Interfaces;
using TriconexMigration.Application.Models;

namespace TriconexMigration.Application.Interfaces;

public interface IInstructionParserFactory
{
    IInstructionParser Create(Network network);
}