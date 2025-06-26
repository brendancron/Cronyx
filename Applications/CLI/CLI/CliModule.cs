using CLI.Abstractions;
using Cronyx.Hosting.Abstractions.Modules;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace CLI;

public class CliModule : IModule
{
   public IHostBuilder ConfigureModule(IHostBuilder builder)
   {
      return builder.ConfigureServices((context, services) =>
      {
         services.AddSingleton<CliRunner>();
         services.AddSingleton<ICliCommandRegistry, CliCommandRegistry>();
         services.AddHostedService<CliRunnerHostedService>();
      });
   }

   public void InitializeModule(IHost host)
   {
   }
}