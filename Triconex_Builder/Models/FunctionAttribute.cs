namespace TriconexMigration.Application.Models;

public class FunctionBlockAttribute
{
    public required string Name { get; set; }
    public required string DataType { get; set; }
    public required string InitialValue { get; set; }
}
