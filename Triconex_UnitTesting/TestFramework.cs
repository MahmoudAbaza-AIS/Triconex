using TriconexMigration.Application.Models;
using TriconexMigration.Application.Services;
using Xunit;
using Xunit.Abstractions;

namespace TriconexMigration.UnitTesting;

public class TestFramework(ServiceFixture fixture, ITestOutputHelper output) : IClassFixture<ServiceFixture>
{
    protected readonly NetworkService _networkService = fixture.NetworkService;
    protected readonly ITestOutputHelper _output = output;

    protected void ValidateSingleNetworkLogic(string networkTitle, string expectedOutput)
    {
        var fileData = FileService.GetFileMock(networkTitle);

        ProgramBuilder programBuilder = ProgramBuilder.GetProgramBuilder(fileData, "filePath");
        var standaloneNetworkResult = _networkService.GetFinalParser(programBuilder.StandaloneNetwork ?? [], "");

        var testcase = standaloneNetworkResult[0].Logic.ToString().Trim();
        _output.WriteLine(testcase);
        Xunit.Assert.True(expectedOutput.Replace(" ", "") == testcase.Replace(" ", ""));
    }
    
    protected void ValidateMultipleNetworkLogics(string networkTitle, string[] expectedOutputs)
    {
        var fileData = FileService.GetFileMock(networkTitle);

        ProgramBuilder programBuilder = ProgramBuilder.GetProgramBuilder(fileData, "filePath");
        var standaloneNetworkResult = _networkService.GetFinalParser(programBuilder.StandaloneNetwork ?? [], "");

        for (int i = 0; i < expectedOutputs.Length; i++)
        {
            var testcase = standaloneNetworkResult[i].Logic.ToString().Trim();
            _output.WriteLine(testcase);
            Xunit.Assert.True(expectedOutputs[i].Replace(" ", "") == testcase.Replace(" ", ""));
        }
    }

}
