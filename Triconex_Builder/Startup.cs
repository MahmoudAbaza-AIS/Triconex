using Microsoft.Extensions.DependencyInjection;
using TriconexMigration.Application.Commands;
using TriconexMigration.Application.Factory;
using TriconexMigration.Application.Interfaces;
using TriconexMigration.Application.Parser;
using TriconexMigration.Application.Services;

namespace TriconexMigration.Application;

public class Startup
{
    public static void ConfigureServices(IServiceCollection services)
    {
        // Factories
        services.AddSingleton<IInstructionParserFactory, InstructionParserFactory>();
        services.AddSingleton<IInstructionCommandFactory, InstructionCommandFactory>();

        // Parsers
        services.AddSingleton<SimpleLogicParser>();
        services.AddSingleton<TimerParser>();
        services.AddSingleton<MultiLogicParser>();
        services.AddSingleton<SetResetParser>();

        // Services
        services.AddScoped<NetworkService>();
        services.AddScoped<FunctionService>();
        services.AddScoped<FunctionBlockService>();
        services.AddScoped<FileService>();

        // Commands
        services.AddTransient<SimpleLogicCommand>();
        services.AddTransient<GeneralInstructionCommand>();
    }
}