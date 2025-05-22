namespace TriconexMigration.Application.Models;

public class Network
{
    public int Number { get; set; }
    public List<IInstruction>? Instructions { get; set; }
}