using Microsoft.Extensions.DependencyInjection;
using SabakaConvert;
using Spectre.Console.Cli;

var services = new ServiceCollection();

var registry = new ConverterRegistry();
registry.Register();
services.AddSingleton(registry);

var app = new CommandApp<ConvertCommand>(new DiTypeRegistrar(services));
return app.Run(args);