using System.Text;
using TriconexMigration.Application.DTOS;
using TriconexMigration.Application.Services;
using Xunit;
using Xunit.Abstractions;

namespace TriconexMigration.UnitTesting;

public class AllAwlAudit(ServiceFixture fixture, ITestOutputHelper output) : TestFramework(fixture, output)
{
    [Fact]
    public void AuditEverySampleAwl()
    {
        var root = RepoRoot();
        var files = Directory.GetFiles(Path.Combine(root, "Triconex_Sample_Files"), "*.*", SearchOption.AllDirectories)
            .Where(f => f.EndsWith(".awl", StringComparison.OrdinalIgnoreCase) || f.EndsWith(".AWL", StringComparison.OrdinalIgnoreCase))
            .Where(f => !f.Contains(".ai"))
            .ToList();

        var report = new StringBuilder();
        int networks = 0;
        int rows = 0;
        int problems = 0;

        foreach (var file in files)
        {
            var lines = FileService.GetFileMock(File.ReadAllText(file));
            var blocks = FunctionBlockService.GetFunctionBlockDetails(lines);
            var functions = FunctionService.GetFunctionsDetails(lines);
            var results = new List<ResultParserDTO>();

            foreach (var block in blocks)
            {
                networks += block.Network?.Count ?? 0;
                results.AddRange(_networkService.GetFinalParser(block.Network ?? [], block.Name ?? ""));
            }
            foreach (var function in functions)
            {
                networks += function.Network.Count;
                results.AddRange(_networkService.GetFinalParser(function.Network, function.Name));
            }
            if (blocks.Count == 0 && functions.Count == 0)
            {
                var standalone = NetworkService.GetNetworksDetails(lines);
                networks += standalone.Count;
                results.AddRange(_networkService.GetFinalParser(standalone, Path.GetFileName(file)));
            }

            rows += results.Count;
            foreach (var row in results)
            {
                var issue = Inspect(row.Logic);
                if (issue == null) continue;
                problems++;
                report.AppendLine($"{Path.GetFileName(file)} net {row.NetworkNumber} [{row.FunctionBlock}] {issue}: {row.Logic}");
            }
        }

        var text = $"files={files.Count} networks={networks} rows={rows} problems={problems}{Environment.NewLine}{report}";
        _output.WriteLine(text);
        Xunit.Assert.Equal(0, problems);
    }

    private static string? Inspect(string logic)
    {
        if (string.IsNullOrWhiteSpace(logic)) return "empty";
        if (logic.Contains("{lhs}")) return "missing coil";
        if (logic.Contains("UNPARSED")) return "unparsed";
        int depth = 0;
        foreach (var c in logic)
        {
            if (c == '(') depth++;
            else if (c == ')') depth--;
            if (depth < 0) return "extra close";
        }
        if (depth != 0) return "unbalanced";
        if (!logic.Contains('=') && !logic.Contains("NOP")) return "no assignment";
        if (logic.Contains("()")) return "empty call";
        if (logic.Contains(",,") || logic.Contains("(,") || logic.Contains(",)")) return "empty argument";
        return null;
    }

    private static string RepoRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir != null && !File.Exists(Path.Combine(dir.FullName, "Triconex_Migration.sln")))
            dir = dir.Parent;
        return dir!.FullName;
    }
}
