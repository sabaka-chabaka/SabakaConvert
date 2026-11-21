using System.ComponentModel;
using Spectre.Console.Cli;

namespace SabakaConvert;

public class ConvertSettings : CommandSettings
{
    [CommandArgument(0, "<input path>")]
    [Description("Path of input file")]
    public required string InputPath { get; init; }
    
    [CommandArgument(1, "<output path>")]
    [Description("Path of output file. Must be have required extension")]
    public required string OutputPath { get; init; }
}

public class ConvertCommand(ConverterRegistry registry) : Command<ConvertSettings>
{
    public override int Execute(CommandContext context, ConvertSettings settings, CancellationToken ct)
    {
        var converter = registry.FindConverter(Path.GetExtension(settings.InputPath), Path.GetExtension(settings.OutputPath));
        
        converter.Convert(settings.InputPath, settings.OutputPath);
        
        return 0;
    }
}