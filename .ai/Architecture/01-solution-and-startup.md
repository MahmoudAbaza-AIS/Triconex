# Solution and startup

Triconex Migration is a .NET 8 Windows desktop application. It reads a Siemens STEP 7 statement list file (AWL / STL) and writes an Excel workbook of logic equations. It does not connect to a controller and it does not simulate the logic.

## Projects

| Folder | Project file | Assembly | Role |
| --- | --- | --- | --- |
| `Triconex_Migration` | `TriconexMigration.Presentaion.csproj` | WPF executable, `net8.0-windows` | Window, file dialog, host. The project name spells Presentation as `Presentaion`. |
| `Triconex_Builder` | `TriconexMigration.Application.csproj` | Class library, `net8.0` | Parsing, models, Excel. Namespace `TriconexMigration.Application`. |
| `Triconex_UnitTesting` | `TriconexMigration.UnitTesting.csproj` | xUnit, `net8.0` | Pattern tests and tests that read the sample AWL files. |

Packages that matter: `Microsoft.Extensions.Hosting` in the UI, `Microsoft.Extensions.DependencyInjection` in the library, and `ClosedXML` 0.104.1 for the workbook.

## Startup

`App` builds a generic host.

1. `Startup.ConfigureServices` registers factories, parsers, services, and commands.
2. `MainWindow` is registered as a singleton.
3. Logging is debug-only, minimum level Information. The window logs `Application is running` when it opens.
4. On exit the host stops with a five-second timeout and is disposed.

`MainWindow` receives one `FileService`. That service is registered as scoped, but the window resolves it once, so one instance lives for the process. `FileUtilities._currentFC` is static and is reset only when a file is loaded.

## Dependency lifetimes

| Lifetime | Types |
| --- | --- |
| Singleton | `IInstructionParserFactory`, `IInstructionCommandFactory`, `SimpleLogicParser`, `TimerParser`, `MultiLogicParser`, `SetResetParser`, `MainWindow` |
| Scoped | `NetworkService`, `FunctionService`, `FunctionBlockService`, `FileService` |
| Transient | `SimpleLogicCommand`, `GeneralInstructionCommand` |

The unit-test fixture does not use the host. It constructs the same parsers, commands, factories, and `NetworkService` directly.

## Window behavior

The window is 500 by 250. **Select File** accepts any file. **Execute** reads the path, builds a `ProgramBuilder`, writes the workbook under `Documents\TriconexMigration`, and opens it. An empty line list does nothing visible. Any exception is shown as `Error Parsing File: <message>`.

A network that no parser accepts does not fail the whole file. `NetworkService` catches that network, writes the raw instructions into the result list, and continues.

## Parser plan

`Software Development Siemens STL parser Plan.xlsx` is the copy of the workbook in `Triconex_Sample_Files`. It is the development plan for this parser, kept next to the architecture notes.
