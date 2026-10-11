# Unit tests

Run every test, with none skipped:

```
dotnet test Triconex_UnitTesting/TriconexMigration.UnitTesting.csproj
```

xUnit is the runner. `Assert` in new tests is written as `Xunit.Assert` because the project also imports MSTest, and the two `Assert` types clash.

## Projects

`ServiceFixture` builds one `NetworkService` with the real parsers. `TestFramework` feeds a network string through `FileService.GetFileMock` and `ProgramBuilder`, then compares equations with spaces removed.

## BitLogicUnitTest and TimerUnitTest

These classes hold the original hand-written patterns: flat AND/OR, `O ;` branches, parentheses, local `L` bits, `SR`/`RS`, and `TP`/`TON`/`TOF`.

Two expectations were corrected so they match the address rule and the rung:

- `Pattern_16_1` expects `!I2_6`. The source operand is `I 2.6`, and every address is stored with an underscore.
- `Pattern_15_3` expects `Q2_1P1=OR(AND(OR(M70_0,I160_1),M1_1),AND(M30_7,M1_3),I160_2,M70_1)`. The previous text had a spare `)` and did not describe the `O ;` chain.

## SampleAwlFileTests

`SampleAwlFileTests` reads files under `Triconex_Sample_Files`.

| Test | File | What must be true |
| --- | --- | --- |
| `Logic1_FunctionBlockNetworks_MatchKnownEquations` | `Logic-1.AWL` | FB 500 equations for `STAT5`, `STAT8`, `STAT7`, `STAT6`, and `OUT4`, and more than 20 rows. |
| `Logic1_LaterNetworks_MatchBitTimerAndLatchPatterns` | `Logic-1.AWL` | `M13_7`, the `M14_0` latch, `M14_2`, `M14_3`, `T11`/`Q3_0`, `Q3_1`, `T15`, `Q1_2`, `Q132_0`. |
| `Logic1_SpacedFileNameCopy_ParsesTheSameBlockEquations` | `Logic-1 1.AWL` | `OUT4` and `T11` match the main export. |
| `Logic1_DuplicateExport_ProducesTheSameEquations` | Siemens backup `Logic-1.AWL` | The equation list equals `Logic-1.AWL`. |
| `LogicTest_IsTheFunctionBlockSliceOfLogic1` | `Logic-test.AWL` | Six rows, including `OUT4`. |
| `WordAndDataBlockExports_DoNotThrow` | `FC2034.awl`, `FC2035.awl`, `FC2038.awl`, `Logic-2.AWL`, `Logic-3.AWL` | Parsing finishes. |
| `GermanMnemonicEquivalent_OfLogic1FirstOrNetwork` | Same `OUT4` rung with `U` / `UN` | Same `OUT4` equation. |
| `Pattern_FromLogic1_OrOfContactAndInput` | `O M1.0` / `O I3.3` / `= Q1.2` | `Q1_2=OR(M1_0,I3_3)`. |
| `Pattern_FromLogic1_AndOfMarkersWithNegation` | `M14.3` network | `M14_3=AND(M11_3,M14_2,!M15_0)`. |
| `Pattern_FromLogic1_PulseTimerInsideBracketThenNot` | `T12` / `Q3.1` network | `T12=TP(!M14_3,T#2S)` and `Q3_1=!T12`. |

No test in this project is marked skip.

## Saved Excel

`Saved Excel` holds the 46 `Triconex_Parser_*.xlsx` workbooks from `Triconex_Sample_Files\saved_file`. They are earlier parser outputs, not fixtures the tests open.
