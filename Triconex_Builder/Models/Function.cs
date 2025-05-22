namespace TriconexMigration.Application.Models;

public class Function
{
    public required string Name { get; set; }
    public required string Version { get; set; }
    public string? Reference { get; set; }
    public List<Network> Network { get; set; } = [];
}
