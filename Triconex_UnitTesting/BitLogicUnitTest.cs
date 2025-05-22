using TriconexMigration.Application.Models;
using TriconexMigration.Application.Services;
using Xunit;
using Xunit.Abstractions;

namespace TriconexMigration.UnitTesting;

public class BitLogicUnitTest(ServiceFixture fixture, ITestOutputHelper output) : TestFramework(fixture, output)
{
    [Fact]
    public void Pattern_1() =>
        ValidateSingleNetworkLogic(networkTitle: """
            NETWORK
            TITLE = 
                  AN    #IN2; 
                  AN    #STAT6; 
                  A     #IN1; 
                  A     #IN2; 
                  A     #STAT5; 
                  =     #OUT3; 
            """,
            expectedOutput: "OUT3 = AND (! IN2 , ! STAT6 , IN1 , IN2 , STAT5)".Replace(" ", ""));

    [Fact]
    public void Pattern_2() =>
        ValidateSingleNetworkLogic(networkTitle: """
            NETWORK
            TITLE =
                  O     M      1.0; 
                  O     ; 
                  A     #STAT5; 
                  AN    #STAT6; 
                  O     ; 
                  A     #STAT6; 
                  A     M      1.1; 
                  =     #OUT4; 
            """,
    expectedOutput: "OUT4 = OR (M1_0 , AND (STAT5 , ! STAT6) , AND (STAT6 , M1_1))".Replace(" ", ""));

    [Fact]
    public void Pattern_3()
    {
        var FileData = FileService.GetFileMock("""
            NETWORK
            TITLE =
                  A     M     14.0; 
                  A(    ; 
                  O     M     13.1; 
                  O     M     13.3; 
                  O     M     13.5; 
                  )     ; 
                  =     M     13.7; 
            """);


        ProgramBuilder programBuilder = ProgramBuilder.GetProgramBuilder(FileData, "filePath");
        var StandaloneNetworkResult = _networkService.GetFinalParser(programBuilder.StandaloneNetwork ?? [], "");

        var testCase = StandaloneNetworkResult[0].Logic.ToString().Trim();
        Xunit.Assert.True("M13_7 = AND (M14_0 , OR (M13_1 , M13_3 , M13_5))".Replace(" ", "") == testCase.Replace(" ", ""));
    }

    [Fact]
    public void Pattern_4()
    {
        var FileData = FileService.GetFileMock("""
            NETWORK
            TITLE =
                  A     #STAT5; 
                  =     L      0.0; 
                  A     L      0.0; 
                  AN    #STAT7; 
                  =     #STAT8; 
                  A     L      0.0; 
                  BLD   102; 
                  =     #STAT7; 
                  A     L      0.0; 
                  =     #STAT11;
            """);


        ProgramBuilder programBuilder = ProgramBuilder.GetProgramBuilder(FileData, "filePath");
        var StandaloneNetworkResult = _networkService.GetFinalParser(programBuilder.StandaloneNetwork ?? [], "");
        var testCase_2 = StandaloneNetworkResult[0].Logic.ToString().Trim();
        var testCase_3 = StandaloneNetworkResult[1].Logic.ToString().Trim();
        var testCase_4 = StandaloneNetworkResult[2].Logic.ToString().Trim();
        _output.WriteLine(testCase_3);
        Xunit.Assert.True("STAT8 = AND (STAT5 , ! STAT7)".Replace(" ", "") == testCase_2.Replace(" ", ""));
        Xunit.Assert.True("STAT7 = STAT5".Replace(" ", "") == testCase_3.Replace(" ", ""));

        Xunit.Assert.True("STAT11 = STAT5".Replace(" ", "") == testCase_4.Replace(" ", ""));
    }

    [Fact]
    public void Pattern_5()
    {
        var FileData = FileService.GetFileMock("""
            NETWORK
            TITLE =
                  AN     I      6.4; 
                  =     Q    160.0; 
            """);


        ProgramBuilder programBuilder = ProgramBuilder.GetProgramBuilder(FileData, "filePath");
        var StandaloneNetworkResult = _networkService.GetFinalParser(programBuilder.StandaloneNetwork ?? [], "");
        var testCase = StandaloneNetworkResult[0].Logic.ToString().Trim();
        Xunit.Assert.True("Q160_0 = ! I6_4".Replace(" ", "") == testCase.Replace(" ", ""));
    }

    [Fact]
    public void Pattern_5_1() =>
        ValidateSingleNetworkLogic("""
            NETWORK
            TITLE =

                  A     T      32; 
                  A     M      1.1; 
                  O     M      1.0; 
                  =     Q      1.0; 
            """, "Q1_0=AND(T32, M1_1)OR(M1_0)".Replace(" ", ""));

    [Fact]
    public void Pattern_6()
    {
        var FileData = FileService.GetFileMock("""
            NETWORK
            TITLE =
                  A     I      6.4; 
                  =     Q    160.0; 
            """);


        ProgramBuilder programBuilder = ProgramBuilder.GetProgramBuilder(FileData, "filePath");
        var StandaloneNetworkResult = _networkService.GetFinalParser(programBuilder.StandaloneNetwork ?? [], "");
        var testCase = StandaloneNetworkResult[0].Logic.ToString().Trim();
        Xunit.Assert.True("Q160_0 = I6_4".Replace(" ", "") == testCase.Replace(" ", ""));
    }

    [Fact]
    public void Pattern_7_0()
    {
        var fileData = FileService.GetFileMock("""
            NETWORK
            TITLE =
                  AN    #IN2; 
                  AN    #STAT6; 
                  A     #IN1; 
                  R     #STAT5; 
                  A     #IN2; 
                  S     #STAT5; 
                  A     #STAT5; 
                  =     #OUT3; 
            """);

        ProgramBuilder programBuilder = ProgramBuilder.GetProgramBuilder(fileData, "filePath");
        var standaloneNetworkResult = _networkService.GetFinalParser(programBuilder.StandaloneNetwork ?? [], "");

        var testCase = standaloneNetworkResult[0].Logic.ToString().Trim();
        //Xunit.Assert.True("STAT5 = SR_STAT5(IN2, AND (! IN2, ! STAT6, IN1))".Replace(" ", "")== testCase.Replace(" ", ""));
        Xunit.Assert.True("STAT5 = SR(IN2, AND (! IN2, ! STAT6, IN1))".Replace(" ", "") == testCase.Replace(" ", ""));
    }

    [Fact]
    public void Pattern_7_1()
    {
        var fileData = FileService.GetFileMock("""
            NETWORK
            TITLE =
                  AN    #IN1; 
                  AN    #STAT6; 
                  A     #IN3; 
                  S     #STAT8; 
                  A     #IN7; 
                  R     #STAT8; 
                  A     #STAT8; 
                  =     #OUT2; 
            """);

        ProgramBuilder programBuilder = ProgramBuilder.GetProgramBuilder(fileData, "filePath");
        var standaloneNetworkResult = _networkService.GetFinalParser(programBuilder.StandaloneNetwork ?? [], "");

        var testCase = standaloneNetworkResult[0].Logic.ToString().Trim();
        //Xunit.Assert.True("STAT8 = RS_STAT8(AND (! IN1, ! STAT6, IN3), IN7)".Replace(" ", "")== testCase.Replace(" ", ""));
        Xunit.Assert.True("STAT8 = RS(AND (! IN1, ! STAT6, IN3), IN7)".Replace(" ", "") == testCase.Replace(" ", ""));
    }

    [Fact]
    public void Pattern_8_0()
    {
        var fileData = FileService.GetFileMock("""
            NETWORK
            TITLE =

                  A     #IN0; 
                  R     #STAT6; 
                  A     #STAT8; 
                  S     #STAT6; 
                  NOP   0; 
            """);

        ProgramBuilder programBuilder = ProgramBuilder.GetProgramBuilder(fileData, "filePath");
        var standaloneNetworkResult = _networkService.GetFinalParser(programBuilder.StandaloneNetwork ?? [], "");
        var testCase = standaloneNetworkResult[0].Logic.ToString().Trim();
        //Xunit.Assert.True("STAT6 = SR_STAT6(STAT8, IN0)".Replace(" ", "")== testCase.Replace(" ", ""));
        Xunit.Assert.True("STAT6 = SR(STAT8, IN0)".Replace(" ", "") == testCase.Replace(" ", ""));
    }

    [Fact]
    public void Pattern_8_1()
    {
        var fileData = FileService.GetFileMock("""
            NETWORK
            TITLE =

                  A     #IN0; 
                  S     #STAT6; 
                  A     #STAT8; 
                  R     #STAT6; 
                  NOP   0; 
            """);

        ProgramBuilder programBuilder = ProgramBuilder.GetProgramBuilder(fileData, "filePath");
        var standaloneNetworkResult = _networkService.GetFinalParser(programBuilder.StandaloneNetwork ?? [], "");

        var testCase = standaloneNetworkResult[0].Logic.ToString().Trim();
        //Xunit.Assert.True("STAT6 = RS_STAT6(IN0, STAT8)".Replace(" ", "")== testCase.Replace(" ", ""));
        Xunit.Assert.True("STAT6 = RS(IN0, STAT8)".Replace(" ", "") == testCase.Replace(" ", ""));
    }

    [Fact]
    public void Pattern_9_0()
    {
        var fileData = FileService.GetFileMock("""
            NETWORK
            TITLE =
                  AN    M    900.4; 
                  A     M    900.1; 
                  O     ; 
                  AN    M    900.1; 
                  AN    M    900.0; 
                  O     ; 
                  AN    M    900.5; 
                  A     M    900.0; 
                  O     M    900.2; 
                  O     M    900.3; 
                  O     DB103.DBX    0.2; 
                  O     DB104.DBX    0.2; 
                  O     DB106.DBX    0.2; 
                  =     M      8.0; 
            """);

        ProgramBuilder programBuilder = ProgramBuilder.GetProgramBuilder(fileData, "filePath");
        var standaloneNetworkResult = _networkService.GetFinalParser(programBuilder.StandaloneNetwork ?? [], "");

        var testCase = standaloneNetworkResult[0].Logic.ToString().Trim();
        Xunit.Assert.True("M8_0 = OR(AND (! M900_4 , M900_1), AND (! M900_1 , ! M900_0), AND (! M900_5 , M900_0), M900_2 , M900_3 , DB103_DBX0_2 , DB104_DBX0_2 , DB106_DBX0_2)".Replace(" ", "") == testCase.Replace(" ", ""));
    }

    [Fact]
    public void Pattern_9_1()
    {
        var fileData = FileService.GetFileMock("""
            NETWORK
            TITLE =
                  O     M     10.2; 
                  O     ; 
                  A     I      2.5; 
                  A     M     15.0; 
                  O     ; 
                  A     I      2.6; 
                  A     M     15.1; 
                  O     ; 
                  A     M     15.0; 
                  A     M     15.1; 
                  AN    I      2.5; 
                  AN    I      2.6; 
                  =     M     10.3; 
            """);

        ProgramBuilder programBuilder = ProgramBuilder.GetProgramBuilder(fileData, "filePath");
        var standaloneNetworkResult = _networkService.GetFinalParser(programBuilder.StandaloneNetwork ?? [], "");
        var testCase = standaloneNetworkResult[0].Logic.ToString().Trim();
        Xunit.Assert.True("M10_3 = OR(M10_2 , AND (I2_5 , M15_0) , AND (I2_6 , M15_1) , AND (M15_0 , M15_1 , ! I2_5 , ! I2_6))".Replace(" ", "") == testCase.Replace(" ", ""));
    }

    [Fact]
    public void Pattern_9_2()
    {
        var fileData = FileService.GetFileMock("""
            NETWORK
            TITLE =

                  A     I      2.4; 
                  A     M      4.0; 
                  O     ; 
                  A     I    133.1; 
                  A     M      4.1; 
                  A     M      5.0; 
                  O     M     10.7; 
                  O     M     47.0; 
                  O     M     19.1; 
                  O     M     19.2; 
                  O     M     15.3; 
                  O     T     3; 
                  =     M     10.2; 
            """);

        ProgramBuilder programBuilder = ProgramBuilder.GetProgramBuilder(fileData, "filePath");
        var standaloneNetworkResult = _networkService.GetFinalParser(programBuilder.StandaloneNetwork ?? [], "");

        var testCase = standaloneNetworkResult[0].Logic.ToString().Trim();
        Xunit.Assert.True("M10_2 = OR(AND (I2_4 , M4_0), AND (I133_1 , M4_1 , M5_0), M10_7 , M47_0 , M19_1 , M19_2 , M15_3 , T3)".Replace(" ", "") == testCase.Replace(" ", ""));
    }

    [Fact]
    public void Pattern_10()
    {
        var FileData = FileService.GetFileMock("""
            NETWORK
            TITLE =

                  A(    ; 
                  O     Q      1.2; 
                  O     Q      1.3; 
                  )     ; 
                  A     M      4.1; 
                  =     Q    132.0; 
            """);


        ProgramBuilder programBuilder = ProgramBuilder.GetProgramBuilder(FileData, "filePath");
        var StandaloneNetworkResult = _networkService.GetFinalParser(programBuilder.StandaloneNetwork ?? [], "");
        var testCase = StandaloneNetworkResult[0].Logic.ToString().Trim();
        Xunit.Assert.True("Q132_0 = AND (OR (Q1_2 , Q1_3), M4_1)".Replace(" ", "") == testCase.Replace(" ", ""));
    }

    [Fact]
    public void Pattern_11_0()
    {
        var FileData = FileService.GetFileMock("""
            NETWORK
            TITLE =

                  A     T      8; 
                  =     L      0.0; 
                  A     L      0.0; 
                  AN    M     12.3; 
                  =     M     12.4; 
                  A     L      0.0; 
                  BLD   102; 
                  =     M     12.3;  
            """);


        ProgramBuilder programBuilder = ProgramBuilder.GetProgramBuilder(FileData, "filePath");
        var StandaloneNetworkResult = _networkService.GetFinalParser(programBuilder.StandaloneNetwork ?? [], "");
        var testCase = StandaloneNetworkResult[0].Logic.ToString().Trim();
        var testCase_2 = StandaloneNetworkResult[1].Logic.ToString().Trim();
        Xunit.Assert.True("M12_4 = AND (T8 , ! M12_3)".Replace(" ", "") == testCase.Replace(" ", ""));
        Xunit.Assert.True("M12_3 = T8".Replace(" ", "") == testCase_2.Replace(" ", ""));
    }

    [Fact]
    public void Pattern_11_1()
    {
        var FileData = FileService.GetFileMock("""
            NETWORK
            TITLE =

                  A     T      8; 
                  =     L      0.0; 
                  A     L      0.0; 
                  AN    M     12.3; 
                  =     M     12.4; 
                  A     L      0.0; 
                  BLD   102; 
                  =     M     12.3;  
            """);


        ProgramBuilder programBuilder = ProgramBuilder.GetProgramBuilder(FileData, "filePath");
        var StandaloneNetworkResult = _networkService.GetFinalParser(programBuilder.StandaloneNetwork ?? [], "");
        var testCase = StandaloneNetworkResult[0].Logic.ToString().Trim();
        var testCase_2 = StandaloneNetworkResult[1].Logic.ToString().Trim();
        Xunit.Assert.True("M12_4 = AND (T8 , ! M12_3)".Replace(" ", "") == testCase.Replace(" ", ""));
        Xunit.Assert.True("M12_3 = T8".Replace(" ", "") == testCase_2.Replace(" ", ""));
    }

    [Fact]
    public void Pattern_12_0()
    {
        var FileData = FileService.GetFileMock("""
            NETWORK
            TITLE =

                  A(    ; 
                  A     I      2.3; 
                  A     M      4.0; 
                  O     ; 
                  A     I    133.0; 
                  A     M      4.1; 
                  A     M      5.0; 
                  )     ; 
                  S     M     11.1; 
                  A(    ; 
                  O     M     10.3; 
                  O     M     23.3; 
                  )     ; 
                  R     M     11.1; 
                  NOP   0; 
            """);


        ProgramBuilder programBuilder = ProgramBuilder.GetProgramBuilder(FileData, "filePath");
        var StandaloneNetworkResult = _networkService.GetFinalParser(programBuilder.StandaloneNetwork ?? [], "");
        var testCase = StandaloneNetworkResult[0].Logic.ToString().Trim();
        _output.WriteLine(testCase);
        //Xunit.Assert.True("M11_1 = RS_M11_1(OR(AND (I2_3 , M4_0), AND (I133_0 , M4_1 , M5_0)), OR (M10_3, M23_3))".Replace(" ", "")== testCase.Replace(" ", ""));
        Xunit.Assert.True("M11_1 = RS(OR(AND (I2_3 , M4_0), AND (I133_0 , M4_1 , M5_0)), OR (M10_3, M23_3))".Replace(" ", "") == testCase.Replace(" ", ""));
    }

    [Fact]
    public void Pattern_12_1()
    {
        var FileData = FileService.GetFileMock("""
            NETWORK
            TITLE =

                  A(    ; 
                  A     I      2.3; 
                  A     M      4.0; 
                  O     ; 
                  A     I    133.0; 
                  A     M      4.1; 
                  A     M      5.0; 
                  )     ; 
                  A(    ; 
                  O     M     10.3; 
                  O     M     23.3; 
                  )     ; 
                  =     M     11.1; 
            """);


        ProgramBuilder programBuilder = ProgramBuilder.GetProgramBuilder(FileData, "filePath");
        var StandaloneNetworkResult = _networkService.GetFinalParser(programBuilder.StandaloneNetwork ?? [], "");
        var testCase = StandaloneNetworkResult[0].Logic.ToString().Trim();
        _output.WriteLine(testCase);
        Xunit.Assert.True("M11_1 = OR(AND (I2_3 , M4_0), AND (I133_0 , M4_1 , M5_0)), AND( OR (M10_3 , M23_3))".Replace(" ", "") == testCase.Replace(" ", ""));
    }

    [Fact]
    public void Pattern_13_0()
    {
        var FileData = FileService.GetFileMock("""
            NETWORK
            TITLE =
                  A(    ; 
                  A     I      2.3; 
                  A     M      4.0; 
                  O     ; 
                  A     I    133.0; 
                  A     M      4.1; 
                  A     M      5.0; 
                  )     ; 
                  =     M     11.1; 
            """);


        ProgramBuilder programBuilder = ProgramBuilder.GetProgramBuilder(FileData, "filePath");
        var StandaloneNetworkResult = _networkService.GetFinalParser(programBuilder.StandaloneNetwork ?? [], "");
        var testCase = StandaloneNetworkResult[0].Logic.ToString().Trim();
        Xunit.Assert.True("M11_1=OR( AND (I2_3, M4_0), AND (I133_0 , M4_1 , M5_0))".Replace(" ", "") == testCase.Replace(" ", ""));
    }

    [Fact]
    public void Pattern_13_1()
    {
        var FileData = FileService.GetFileMock("""
            NETWORK
            TITLE =
                  A(    ; 
                  O     M     10.3; 
                  O     M     23.3; 
                  )     ; 
                  =     M     11.1; 
            """);


        ProgramBuilder programBuilder = ProgramBuilder.GetProgramBuilder(FileData, "filePath");
        var StandaloneNetworkResult = _networkService.GetFinalParser(programBuilder.StandaloneNetwork ?? [], "");
        var testCase = StandaloneNetworkResult[0].Logic.ToString().Trim();
        Xunit.Assert.True("M11_1 = AND (OR (M10_3 , M23_3))".Replace(" ", "") == testCase.Replace(" ", ""));
    }

    [Fact]
    public void Pattern_14_0()
    {
        var FileData = FileService.GetFileMock("""
            NETWORK
            TITLE =

                  O     M     10.2;
                  O     ;
                  A     I      2.5;
                  A     M     15.0;
                  O     ;
                  A     I      2.6;
                  A     M     15.1;
                  O     ;
                  A     M     15.0;
                  A     M     15.1;
                  AN    I      2.5;
                  AN    I      2.6;
                  =     M     10.3;
            """);


        ProgramBuilder programBuilder = ProgramBuilder.GetProgramBuilder(FileData, "filePath");
        var StandaloneNetworkResult = _networkService.GetFinalParser(programBuilder.StandaloneNetwork ?? [], "");
        var testCase = StandaloneNetworkResult[0].Logic.ToString().Trim();
        Xunit.Assert.True("M10_3 = OR(M10_2 , AND (I2_5 , M15_0) , AND (I2_6 , M15_1) , AND (M15_0 , M15_1 , ! I2_5 , ! I2_6))".Replace(" ", "") == testCase.Replace(" ", ""));
    }

    [Fact]
    public void Pattern_14_1()
    {
        var FileData = FileService.GetFileMock("""
            NETWORK
            TITLE =

                  A(    ; 
                  ON    I      0.1; 
                  ON    I      0.5; 
                  ON    I     10.0; 
                  ON    I     14.3; 
                  ON    I     18.6; 
                  ON    I     23.1; 
                  )     ; 
                  A(    ; 
                  ON    M     15.0; 
                  ON    M     15.1; 
                  )     ; 
                  =     M     12.2; 
            """);


        ProgramBuilder programBuilder = ProgramBuilder.GetProgramBuilder(FileData, "filePath");
        var StandaloneNetworkResult = _networkService.GetFinalParser(programBuilder.StandaloneNetwork ?? [], "");
        var testCase = StandaloneNetworkResult[0].Logic.ToString().Trim();
        _output.WriteLine(testCase);
        Xunit.Assert.True("M12_2 = AND(OR (! I0_1 , ! I0_5 , ! I10_0 , ! I14_3 , ! I18_6 , ! I23_1), OR (! M15_0 , ! M15_1))".Replace(" ", "") == testCase.Replace(" ", ""));
    }

    [Fact]
    public void Pattern_15_0()
    {
        var FileData = FileService.GetFileMock("""
            NETWORK
            TITLE =

                  A(    ; 
                  ON    I      0.1; 
                  ON    I      0.5; 
                  ON    I     10.0; 
                  ON    I     14.3; 
                  ON    I     18.6; 
                  ON    I     23.1; 
                  )     ; 
                  A     M     15.0; 
                  A     M     15.1; 
                  =     M     12.2; 
            """);


        ProgramBuilder programBuilder = ProgramBuilder.GetProgramBuilder(FileData, "filePath");
        var StandaloneNetworkResult = _networkService.GetFinalParser(programBuilder.StandaloneNetwork ?? [], "");
        var testCase = StandaloneNetworkResult[0].Logic.ToString().Trim();
        Xunit.Assert.True("M12_2 = AND( OR (! I0_1 , ! I0_5 , ! I10_0 , ! I14_3 , ! I18_6 , ! I23_1), M15_0, M15_1)".Replace(" ", "") == testCase.Replace(" ", ""));
    }

    [Fact]
    public void Pattern_15_1()
    {
        var FileData = FileService.GetFileMock("""
            NETWORK
            TITLE =

                  A(    ; 
                  A     I      0.1; 
                  A     I      0.5; 
                  A     I     10.0; 
                  A     I     14.3; 
                  A     I     18.6; 
                  A     I     23.1; 
                  )     ;
                  A(    ; 
                  ON    M     15.0; 
                  ON    M     15.1; 
                  )     ;
                  =     M     12.2; 
            """);


        ProgramBuilder programBuilder = ProgramBuilder.GetProgramBuilder(FileData, "filePath");
        var StandaloneNetworkResult = _networkService.GetFinalParser(programBuilder.StandaloneNetwork ?? [], "");
        var testCase = StandaloneNetworkResult[0].Logic.ToString().Trim();
        Xunit.Assert.True("M12_2 = AND (I0_1 , I0_5 , I10_0 , I14_3 , I18_6 , I23_1 , OR (! M15_0 , ! M15_1))".Replace(" ", "") == testCase.Trim().Replace(" ", ""));
    }

    [Fact]
    public void Pattern_15_2()
    {
        var FileData = FileService.GetFileMock("""
            NETWORK
            TITLE =

                  A     I      0.1; 
                  A     I      0.5; 
                  A     I     10.0; 
                  A     I     14.3; 
                  A     I     18.6; 
                  A     I     23.1; 
                  A(    ; 
                  ON    M     15.0; 
                  ON    M     15.1; 
                  )     ;
                  =     M     12.2; 
            """);


        ProgramBuilder programBuilder = ProgramBuilder.GetProgramBuilder(FileData, "filePath");
        var StandaloneNetworkResult = _networkService.GetFinalParser(programBuilder.StandaloneNetwork ?? [], "");
        var testCase = StandaloneNetworkResult[0].Logic.ToString().Trim();
        Xunit.Assert.True("M12_2 = AND (I0_1 , I0_5 , I10_0 , I14_3 , I18_6 , I23_1 , OR (! M15_0 , ! M15_1))".Replace(" ", "") == testCase.Trim().Replace(" ", ""));
    }

    [Fact]
    public void Pattern_15_3()
    {
        var FileData = FileService.GetFileMock("""
            NETWORK
            TITLE =

                A(    ; 
                O     M     70.0; 
                O     I    160.1; 
                )     ; 
                A     M      1.1; 
                O     ; 
                A     M     30.7; 
                A     M      1.3; 
                O     I    160.2; 
                O     M     70.1; 
                =     Q      2.1P1; 
            """);


        ProgramBuilder programBuilder = ProgramBuilder.GetProgramBuilder(FileData, "filePath");
        var StandaloneNetworkResult = _networkService.GetFinalParser(programBuilder.StandaloneNetwork ?? [], "");
        var testCase = StandaloneNetworkResult[0].Logic.ToString().Trim();
        _output.WriteLine(testCase);
        Xunit.Assert.True("Q2_1P1 = AND( OR(M70_0 , I160_1), M1_1),AND (M30_7 , M1_3),OR (I160_2 , M70_1))".Replace(" ", "") == testCase.Trim().Replace(" ", ""));
    }

    [Fact]
    public void Pattern_15_4()
    {
        var FileData = FileService.GetFileMock("""
            NETWORK
            TITLE =

                A(    ; 
                O     M      4.7; 
                O     M     47.1; 
                O     M     19.1; 
                O     M     19.2; 
                O     M     46.0; 
                O     M     15.4; 
                O     M     50.0; 
                O     M     25.0; 
                O     M     15.1; 
                O     ; 
                AN    I      5.0; 
                A     M     15.5; 
                O     M     47.0; 
                O     M     15.3; 
                O     ; 
                AN    I      5.0; 
                A     M     15.2; 
                O     M     15.0; 
                )     ; 
                A     M      1.1; 
                =     Q      2.1P2;  
            """);


        ProgramBuilder programBuilder = ProgramBuilder.GetProgramBuilder(FileData, "filePath");
        var StandaloneNetworkResult = _networkService.GetFinalParser(programBuilder.StandaloneNetwork ?? [], "");
        var testCase = StandaloneNetworkResult[0].Logic.ToString().Trim();
        _output.WriteLine(testCase);
        Xunit.Assert.True("Q2_1P2=AND(OR (M4_7 ,M47_1 ,M19_1 ,M19_2 ,M46_0 ,M15_4 ,M50_0 ,M25_0 ,M15_1, AND(!I5_0 , M15_5), M47_0, M15_3, AND(!I5_0,M15_2), M15_0) , M1_1)".Replace(" ", "") == testCase.Trim().Replace(" ", ""));
    }

    [Fact]
    public void Pattern_15_5()
    {
        var FileData = FileService.GetFileMock("""
            NETWORK
            TITLE =

                  A     M     15.0; 
                  A     M     14.0; 
                  =     L      0.0; 
                  A     L      0.0; 
                  AN    M     13.2; 
                  =     M     13.3; 
                  A     L      0.0; 
                  BLD   102; 
                  =     M     13.2; 
            """);


        ProgramBuilder programBuilder = ProgramBuilder.GetProgramBuilder(FileData, "filePath");
        var StandaloneNetworkResult = _networkService.GetFinalParser(programBuilder.StandaloneNetwork ?? [], "");
        var testCase = StandaloneNetworkResult[0].Logic.ToString().Trim();
        var testCase_2 = StandaloneNetworkResult[1].Logic.ToString().Trim();
        Xunit.Assert.True("M13_3 = AND (M15_0, M14_0 , ! M13_2)".Replace(" ", "") == testCase.Replace(" ", ""));
        Xunit.Assert.True("M13_2 = AND (M15_0, M14_0)".Replace(" ", "") == testCase_2.Replace(" ", ""));
    }

    [Fact]
    public void Pattern_15_6()
    {
        var FileData = FileService.GetFileMock("""
            NETWORK
            TITLE =

                  A     M     15.0; 
                  A     M     14.0; 
                  A     M     13.0; 
                  A     M     12.0; 
                  =     L      0.0; 
                  A     L      0.0; 
                  AN    M     13.2; 
                  AN    M     10.2; 
                  =     M     13.3; 
                  A     L      0.0; 
                  BLD   102; 
                  =     M     13.2; 
            """);


        ProgramBuilder programBuilder = ProgramBuilder.GetProgramBuilder(FileData, "filePath");
        var StandaloneNetworkResult = _networkService.GetFinalParser(programBuilder.StandaloneNetwork ?? [], "");
        var testCase = StandaloneNetworkResult[0].Logic.ToString().Trim();
        var testCase_2 = StandaloneNetworkResult[1].Logic.ToString().Trim();
        Xunit.Assert.True("M13_3 = AND (M15_0, M14_0 , M13_0, M12_0, ! M13_2, ! M10_2)".Replace(" ", "") == testCase.Replace(" ", ""));
        Xunit.Assert.True("M13_2 = AND (M15_0, M14_0 , M13_0, M12_0)".Replace(" ", "") == testCase_2.Replace(" ", ""));
    }

    [Fact]
    public void Pattern_16_0()
    {
        var FileData = FileService.GetFileMock("""
            NETWORK
            TITLE =

                  L     IB   131; 
                  T     QB   131; 
                  NOP   0; 
            """);


        ProgramBuilder programBuilder = ProgramBuilder.GetProgramBuilder(FileData, "filePath");
        var StandaloneNetworkResult = _networkService.GetFinalParser(programBuilder.StandaloneNetwork ?? [], "");
        var testCase = StandaloneNetworkResult[0].Logic.ToString().Trim();
        Xunit.Assert.True("QB131 = IB131".Replace(" ", "") == testCase.Replace(" ", ""));
    }

    [Fact]
    public void Pattern_16_1()
    {
        var FileData = FileService.GetFileMock("""
            NETWORK
            TITLE =

                  A(    ; 
                  A     M     13.7;
                  R     M     14.0;
                  A     M     13.6;
                  S     M     14.0;
                  A     M     14.0;
                  )     ;
                  AN    I      2.5;
                  AN    I      2.6;
                  O     I      2.5;
                  =     M     14.2;
            """);


        ProgramBuilder programBuilder = ProgramBuilder.GetProgramBuilder(FileData, "filePath");
        var StandaloneNetworkResult = _networkService.GetFinalParser(programBuilder.StandaloneNetwork ?? [], "");
        var testCase = StandaloneNetworkResult[0].Logic.ToString().Trim();
        var testCase_2 = StandaloneNetworkResult[1].Logic.ToString().Trim();
        _output.WriteLine(testCase);
        _output.WriteLine(testCase_2);
        Xunit.Assert.True("M14_0 = SR(M13_6, M13_7)".Replace(" ", "") == testCase.Replace(" ", ""));
        Xunit.Assert.True("M14_2 = OR(AND(M14_0, !I2_5, !I2.6), I2_5)".Replace(" ", "") == testCase_2.Replace(" ", ""));
    }

    [Fact]
    public void Pattern_16_2()
    {
        var FileData = FileService.GetFileMock("""
            NETWORK
            TITLE =

                  A(    ; 
                  O     T     17; 
                  O     T     20; 
                  )     ; 
                  AN    I      5.0; 
                  NOT   ; 
                  =     Q      2.4; 
            """);


        ProgramBuilder programBuilder = ProgramBuilder.GetProgramBuilder(FileData, "filePath");
        var StandaloneNetworkResult = _networkService.GetFinalParser(programBuilder.StandaloneNetwork ?? [], "");
        var testCase = StandaloneNetworkResult[0].Logic.ToString().Trim();
        //var testCase1 = StandaloneNetworkResult[1].Logic.ToString().Trim();
        _output.WriteLine(testCase);
        //_output.WriteLine(testCase1);
        Xunit.Assert.True("Q2_4 = !AND (OR (T17, T20), !I5_0)".Replace(" ", "") == testCase.Replace(" ", ""));
        //Xunit.Assert.True("Q2_4 = !Q2_4".Replace(" ", "") == testCase1.Replace(" ", ""));
    }

    //[Fact]
    //public void Pattern_X()
    //{
    //    var FileData = FileService.GetFileMock("""
    //        NETWORK
    //        TITLE =

    //              A(    ; 
    //              O     M     70.0; 
    //              O     I    160.1; 
    //              )     ; 
    //              A     M      1.1; 
    //              O     ; 
    //              A     M     30.7; 
    //              A     M      1.3; 
    //              O     I    160.2; 
    //              O     M     70.1; 
    //              O     ; 
    //              A(    ; 
    //              O     M      4.7; 
    //              O     M     47.1; 
    //              O     M     19.1; 
    //              O     M     19.2; 
    //              O     M     46.0; 
    //              O     M     15.4; 
    //              O     M     50.0; 
    //              O     M     25.0; 
    //              O     M     15.1; 
    //              O     ; 
    //              AN    I      5.0; 
    //              A     M     15.5; 
    //              O     M     47.0; 
    //              O     M     15.3; 
    //              O     ; 
    //              AN    I      5.0; 
    //              A     M     15.2; 
    //              O     M     15.0; 
    //              )     ; 
    //              A     M      1.1; 
    //              =     Q      2.1;  
    //        """
    //    );
    //    ProgramBuilder programBuilder = ProgramBuilder.GetProgramBuilder(FileData, "filePath");
    //    var StandaloneNetworkResult = _networkService.GetFinalParser(programBuilder.StandaloneNetwork ?? [], "");
    //    var testCase = StandaloneNetworkResult[0].Logic.ToString().Trim();
    //    Xunit.Assert.True("Q2_1 = AND(OR(M70_0, I160_1)AND(M1_1OR(AND(M30_7, M1_3)OR(I160_2, M70_1, AND(OR(M4_7, M47_1, M19_1, M19_2, M46_0, M15_4, M50_0, M25_0, M15_1, AND(! I5_0, M15_5)OR(M47_0, M15_3, AND(! I5_0, M15_2)OR(M15_0AND(M1_1)))))))))".Replace(" ", "") == testCase.Replace(" ", ""));
    //}
}
