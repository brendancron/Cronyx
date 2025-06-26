using Microsoft.Extensions.Hosting;

namespace CLI;

public class CliRunnerHostedService : BackgroundService
{
   private readonly CliRunner _cliRunner;

   public CliRunnerHostedService(CliRunner cliRunner)
   {
      _cliRunner = cliRunner;
   }

   protected override async Task ExecuteAsync(CancellationToken stoppingToken)
   {
      await _cliRunner.RunCliAsync(stoppingToken);
   }
}

