using Xunit;
using Xunit.Abstractions;

namespace TriconexMigration.UnitTesting
{
    public class TimerUnitTest(ServiceFixture fixture, ITestOutputHelper output) : TestFramework(fixture, output)
    {
        [Fact]
        public void Pattern_1_0() => ValidateMultipleNetworkLogics(networkTitle:
            """
                NETWORK
                TITLE =

                      A     M     14.3; 
                      L     S5T#2S; 
                      SP    T     11; 
                      NOP   0; 
                      NOP   0; 
                      NOP   0; 
                      A     T     11; 
                      =     Q      3.0; 
             """, expectedOutputs: ["T11 = TP (M14_3, T#2S)", "Q3_0 = T11"]);

        [Fact]
        public void Pattern_2_0() => ValidateSingleNetworkLogic(networkTitle:
            """
                NETWORK
                TITLE =

                      A     M     15.4; 
                      L     S5T#5M; 
                      SD    T      3; 
                      NOP   0; 
                      NOP   0; 
                      NOP   0; 
                      NOP   0; 
             """, expectedOutput: "T3 = TON (M15_4, T#5M)");

        [Fact]
        public void Pattern_3_0() => ValidateSingleNetworkLogic(networkTitle:
            """
                NETWORK
                TITLE =

                      AN    I      3.3; 
                      A     M     14.3; 
                      L     S5T#3S; 
                      SD    T     15; 
                      NOP   0; 
                      NOP   0; 
                      NOP   0; 
                      NOP   0; 
             """, expectedOutput: "T15 = TON(AND (!I3_3, M14_3), T#3S)");

        [Fact]
        public void Pattern_3_1() => ValidateSingleNetworkLogic(networkTitle:
            """
                NETWORK
                TITLE =

                      ON    I      3.3; 
                      O     M     14.3; 
                      ON    M     14.0; 
                      O     M     15.0; 
                      L     S5T#3S; 
                      SF    T     15; 
                      NOP   0; 
                      NOP   0; 
                      NOP   0; 
                      NOP   0; 
             """, expectedOutput: "T15 = TOF(OR (!I3_3, M14_3, !M14_0, M15_0), T#3S)");

        [Fact]
        public void Pattern_4_0() => ValidateSingleNetworkLogic(networkTitle:
            """
                NETWORK
                TITLE =

                      AN    I      1.0; 
                      L     S5T#2S; 
                      SD    T     18; 
                      NOP   0; 
                      NOP   0; 
                      NOP   0; 
                      NOP   0;
             """, expectedOutput: "T18 = TON(!I1_0, T#2S)");

        [Fact]
        public void Pattern_5_0() => ValidateSingleNetworkLogic(networkTitle:
            """
                NETWORK
                TITLE =

                      A(    ; 
                      O     I      3.3; 
                      O     I      3.4; 
                      )     ; 
                      L     S5T#4M; 
                      SD    T     25; 
                      NOP   0; 
                      NOP   0;
             """, expectedOutput: "T25 = TON(OR(I3_3, I3_4), T#4M)");

        [Fact]
        public void Pattern_5_1() => ValidateSingleNetworkLogic(networkTitle:
            """
                NETWORK
                TITLE =

                       A(    ;  
                       O     T      0;
                       ON    T      0;
                       )     ;
                       L     S5T#3S;
                       SD    T      0;
                       NOP   0;
                       NOP   0;
             """, expectedOutput: "T0 = TON(OR (T0, !T0), T#3S)");

        [Fact]
        public void Pattern_6_0() => ValidateMultipleNetworkLogics(networkTitle:
            """
                NETWORK
                TITLE =

                       A(    ;
                       AN    M     14.5;
                       L     S5T#2S;
                       SP    T     14;
                       NOP   0;
                       NOP   0;
                       A     T     14;
                       )     ;
                       NOT   ;
                       =     Q      3.3;
             """, expectedOutputs: ["T14 = TP(!M14_5, T#2S)", "Q3_3 = !T14"]);
    }
}