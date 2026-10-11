using TriconexMigration.Application.DTOS;
using TriconexMigration.Application.Models;
using TriconexMigration.Application.Services;
using Xunit;
using Xunit.Abstractions;

namespace TriconexMigration.UnitTesting;

public class SampleAwlFileTests(ServiceFixture fixture, ITestOutputHelper output) : TestFramework(fixture, output)
{
    private static string SampleFile(string fileName)
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir != null && !File.Exists(Path.Combine(dir.FullName, "Triconex_Migration.sln")))
            dir = dir.Parent;

        Xunit.Assert.NotNull(dir);
        var path = Path.Combine(dir.FullName, "Triconex_Sample_Files", fileName);
        Xunit.Assert.True(File.Exists(path), path);
        return path;
    }

    private List<ResultParserDTO> ParseFile(string fileName)
    {
        var path = SampleFile(fileName);
        var lines = FileService.GetFileMock(File.ReadAllText(path));
        var blocks = FunctionBlockService.GetFunctionBlockDetails(lines);
        var functions = FunctionService.GetFunctionsDetails(lines);

        var results = new List<ResultParserDTO>();
        foreach (var block in blocks)
            results.AddRange(_networkService.GetFinalParser(block.Network ?? [], block.Name ?? ""));
        foreach (var function in functions)
            results.AddRange(_networkService.GetFinalParser(function.Network, function.Name));

        if (blocks.Count == 0 && functions.Count == 0)
        {
            var standalone = NetworkService.GetNetworksDetails(lines);
            results.AddRange(_networkService.GetFinalParser(standalone, ""));
        }

        foreach (var row in results)
            _output.WriteLine($"{row.NetworkNumber}: {row.Logic}");

        return results;
    }

    private static void ContainsEquation(List<ResultParserDTO> results, string expected)
    {
        var needle = expected.Replace(" ", "");
        Xunit.Assert.Contains(results, row => row.Logic.Replace(" ", "").Equals(needle, StringComparison.Ordinal));
    }

    [Fact]
    public void Logic1_FunctionBlockNetworks_MatchKnownEquations()
    {
        var results = ParseFile("Logic-1.AWL");
        Xunit.Assert.True(results.Count > 20);

        ContainsEquation(results, "STAT5=SR(IN2,AND(!IN2,!STAT6,IN1))");
        ContainsEquation(results, "STAT8=AND(STAT5,!STAT7)");
        ContainsEquation(results, "STAT7=STAT5");
        ContainsEquation(results, "STAT6=SR(STAT8,IN0)");
        ContainsEquation(results, "OUT4=OR(M1_0,AND(STAT5,!STAT6),AND(STAT6,M1_1))");
    }

    [Fact]
    public void Logic1_LaterNetworks_MatchBitTimerAndLatchPatterns()
    {
        var results = ParseFile("Logic-1.AWL");

        ContainsEquation(results, "M13_7=AND(M14_0,OR(M13_1,M13_3,M13_5))");
        ContainsEquation(results, "M14_0=SR(M13_6,M13_7)");
        ContainsEquation(results, "M14_2=OR(AND(M14_0,!I2_5,!I2_6),I2_5)");
        ContainsEquation(results, "M14_3=AND(M11_3,M14_2,!M15_0)");
        ContainsEquation(results, "T11=TP(M14_3,T#2S)");
        ContainsEquation(results, "Q3_0=T11");
        ContainsEquation(results, "Q3_1=!T12");
        ContainsEquation(results, "T15=TON(AND(!I3_3,M14_3),T#3S)");
        ContainsEquation(results, "Q1_2=OR(M1_0,I3_3)");
        ContainsEquation(results, "Q132_0=AND(OR(Q1_2,Q1_3),M4_1)");
        ContainsEquation(results, "M900_4=AND(>=I(MW902,8),<=I(MW902,11))");
        ContainsEquation(results, "M900_5=AND(>=I(MW904,8),<=I(MW904,11))");
        ContainsEquation(results, "DB103_DBX0_2=OR(<>I(DB103_DBB13,0),<>I(DB103_DBB14,0),DB103_DBX6_0)");
        ContainsEquation(results, "QD200=DB50_DBD0");
        ContainsEquation(results, "QD204=DB50_DBD4");
        ContainsEquation(results, "M3_7=!M3_7");
    }

    [Fact]
    public void Logic1_SpacedFileNameCopy_ParsesTheSameBlockEquations()
    {
        var results = ParseFile("Logic-1 1.AWL");
        ContainsEquation(results, "OUT4=OR(M1_0,AND(STAT5,!STAT6),AND(STAT6,M1_1))");
        ContainsEquation(results, "T11=TP(M14_3,T#2S)");
    }

    [Fact]
    public void Logic1_DuplicateExport_ProducesTheSameEquations()
    {
        var original = ParseFile("Logic-1.AWL").Select(r => r.Logic).ToList();
        var copy = ParseFile(Path.Combine("Siemens Exported Backups", "Logic-1.AWL")).Select(r => r.Logic).ToList();
        Xunit.Assert.Equal(original, copy);
    }

    [Fact]
    public void LogicTest_IsTheFunctionBlockSliceOfLogic1()
    {
        var results = ParseFile(Path.Combine("Siemens Exported Backups", "Logic-test.AWL"));
        Xunit.Assert.Equal(6, results.Count);
        ContainsEquation(results, "OUT4=OR(M1_0,AND(STAT5,!STAT6),AND(STAT6,M1_1))");
    }

    [Fact]
    public void WordAndDataBlockExports_DoNotThrow()
    {
        var fc2034 = ParseFile("FC2034.awl");
        var fc2035 = ParseFile("FC2035.awl");
        var fc2038 = ParseFile("FC2038.awl");
        var dataBlocks = ParseFile(Path.Combine("Siemens Exported Backups", "Logic-2.AWL"));
        var logic3 = ParseFile(Path.Combine("Siemens Exported Backups", "Logic-3.AWL"));

        Xunit.Assert.NotEmpty(fc2034);
        Xunit.Assert.NotEmpty(fc2035);
        Xunit.Assert.NotEmpty(fc2038);
        Xunit.Assert.Contains(fc2034, row => row.Logic.Contains('='));
        Xunit.Assert.Contains(fc2035, row => row.Logic.Contains('='));
        Xunit.Assert.Contains(fc2038, row => row.Logic.Contains('='));
        Xunit.Assert.Contains(fc2034, row => row.Logic.Replace(" ", "").Contains("DID6=DB443_DBD122"));
        Xunit.Assert.Contains(fc2035, row => row.Logic.Replace(" ", "").Contains("FC303(P#L=2_0,P#L=0_2)"));
        Xunit.Assert.Contains(fc2038, row => row.Logic.Replace(" ", "").Contains("Q64_1=DBX120_0"));
        Xunit.Assert.NotNull(dataBlocks);
        Xunit.Assert.NotNull(logic3);
    }

    [Fact]
    public void GermanMnemonicEquivalent_OfLogic1FirstOrNetwork() =>
        ValidateSingleNetworkLogic("""
            NETWORK
            TITLE =
                  O     M      1.0;
                  O     ;
                  U     #STAT5;
                  UN    #STAT6;
                  O     ;
                  U     #STAT6;
                  U     M      1.1;
                  =     #OUT4;
            """,
            "OUT4=OR(M1_0,AND(STAT5,!STAT6),AND(STAT6,M1_1))");

    [Fact]
    public void Pattern_FromLogic1_OrOfContactAndInput() =>
        ValidateSingleNetworkLogic("""
            NETWORK
            TITLE =
                  O     M      1.0;
                  O     I      3.3;
                  =     Q      1.2;
            """,
            "Q1_2=OR(M1_0,I3_3)");

    [Fact]
    public void Pattern_FromLogic1_AndOfMarkersWithNegation() =>
        ValidateSingleNetworkLogic("""
            NETWORK
            TITLE =
                  A     M     11.3;
                  A     M     14.2;
                  AN    M     15.0;
                  =     M     14.3;
            """,
            "M14_3=AND(M11_3,M14_2,!M15_0)");

    [Fact]
    public void Pattern_FromLogic1_PulseTimerInsideBracketThenNot() =>
        ValidateMultipleNetworkLogics("""
            NETWORK
            TITLE =
                  A(    ;
                  AN    M     14.3;
                  L     S5T#2S;
                  SP    T     12;
                  NOP   0;
                  NOP   0;
                  NOP   0;
                  A     T     12;
                  )     ;
                  NOT   ;
                  =     Q      3.1;
            """,
            ["T12=TP(!M14_3,T#2S)", "Q3_1=!T12"]);
}
