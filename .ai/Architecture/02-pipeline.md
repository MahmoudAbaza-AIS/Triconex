# Pipeline from file to Excel

```
AWL text
  FileService.GetFile
    tabs become spaces, blank lines drop, FC counter resets
  ProgramBuilder.GetProgramBuilder
    FUNCTION_BLOCK regions
    FUNCTION regions
    standalone networks only when both lists are empty
  For each network
    NetworkUtilities classifies each line as an instruction
    InstructionParserFactory chooses a parser
    InstructionCommandFactory chooses a command
    The command appends ResultParserDTO rows
  NetworkService post-process
    '.' becomes '_'
    ComplexEquationParser.Execute
  FileService.CreateFile
    one sheet per non-empty collection
    save and open the xlsx
```

`ProgramBuilder.Content` stores the file path, not the file text. The Functions sheet uses that path when it looks up `FCn`.

## How a file is split

| Marker | Effect |
| --- | --- |
| Line starts with `FUNCTION_BLOCK` | Starts a function block. The whole line is the name. |
| `END_FUNCTION_BLOCK` | Closes the block. |
| First token `FUNCTION` | Stops the block scan and starts a function. |
| `END_FUNCTION` | Closes the function. |
| `TITLE` | Starts a network. The title line is not kept. |
| `NETWORK` or `NETZWERK` | Closes the previous network when it has lines, then starts another network so a file without `TITLE` still parses. |
| `END_NETWORK` | Closes the current network. |

If the file does not start with `FUNCTION`, it is treated as standalone and a synthetic `END_NETWORK` is appended so the last network closes.

Standalone networks are extracted only when the file has neither function blocks nor functions. A file that contains either does not also emit a standalone sheet.

Function-block scanning stops at the first `FUNCTION` line. Blocks are expected before functions.

## Instruction lines

`IsBitLogicType` matches a line whose first token is a bit mnemonic followed by whitespace or end of token: `U`, `UN`, `U(`, `A`, `AN`, `A(`, `O`, `ON`, `O(`, `X`, `XN`, `X(`, `NOT`, `R`, `S`, `=`, `(`, `)`. Timer words such as `SP` do not match, because the check requires the mnemonic to end before the next character.

Anything else matches `IsLoadAndTransferType`. That check is an empty regular expression, so it matches every remaining line. `XInstruction` is not reached from this path.

Both live instruction classes split the line on spaces. Token 1 is the operator, token 2 is the memory area, token 3 is the value. `NormalizeAwlOperator` uppercases the operator and maps German mnemonics:

| AWL | Treated as |
| --- | --- |
| `U` | `A` |
| `UN` | `AN` |
| `U(` | `A(` |
| `SI` | `SP` (pulse, `TP`) |
| `SE` | `SD` (on-delay, `TON`) |
| `SA` | `SF` (off-delay, `TOF`) |

`BitLogicInstruction` removes `#` from symbolic operands (`#IN2` becomes `IN2`). Load/transfer keeps `#`, which is how `L S5T#2S` is recognized as a timer network.

## Parser selection

`InstructionParserFactory.Create` looks at the whole network, in order:

1. **Simple logic**, when the network has instructions and no operator contains `R`, no operator contains `(`, no memory area contains `#`, and no memory area contains `CALL`.
2. **Set/reset**, when some operator contains `R` and none contains `(`.
3. **Timer**, when some memory area contains `#`.
4. **Multi-logic**, when some operator contains `(`, including set/reset inside parentheses.
5. Otherwise `ApplicationException`, which `NetworkService` catches per network.

`InstructionCommandFactory` sends `SimpleLogicParser` to `SimpleLogicCommand` and every other parser to `GeneralInstructionCommand`.

`SimpleLogicCommand` splits the network at each `=` when there is more than one coil, then inlines local `L` bits. `GeneralInstructionCommand` parses the network once. A result that contains `;;` becomes two spreadsheet rows.

## Models

`Function` and `FunctionBlock` hold a name, a version line, and networks. Function blocks also parse `IN*`, `OUT*`, and `STAT*` declarations into `FunctionBlockAttribute` lists. Those attributes are not written to Excel.

`ResultParserDTO` is one equation row: `Logic`, `NetworkNumber`, and `FunctionBlock` (the parent name, or empty for standalone).

## Excel

`FileService.CreateFile` writes `Documents\TriconexMigration\ParsedOn_<DayOfWeek>-<yyyy-MM-dd_HH-mm-ss-fff>.xlsx` and opens it.

| Sheet | When | Columns |
| --- | --- | --- |
| Function Blocks | At least one block | Logic, Network Number, Function Block, Inputs, Status |
| Functions | At least one function | Logic, Network Number, Reference, Inputs, Status |
| Standalone Network | Standalone networks only | Logic, network number in column B with no header, Inputs |

Function-block status is `Passed` when the block name is not blank. Function status is `Passed` when the reference text is not `Unclear`. The FC lookup returns `FCn` and does not return `Unclear`, so that failure text is not produced by the current lookup. The reference scan is order-dependent because `_currentFC` is static for the file.

`GetInputs` keeps the right-hand side of `=` after removing gate names, timer names, and parentheses, and joins operands with `/`.
