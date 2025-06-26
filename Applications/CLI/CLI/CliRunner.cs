using CLI.Abstractions;

namespace CLI;

public class CliRunner
{

   public const string EXIT_COMMAND = "exit";
   public const string COMMAND_START = ">";

   private readonly ICliCommandRegistry _cliCommandRegistry;

   public CliRunner(ICliCommandRegistry cliCommandRegistry)
   {
      _cliCommandRegistry = cliCommandRegistry;
   }

   public async Task RunCliAsync(CancellationToken stoppingToken)
   {
      Console.WriteLine("Running CLI");

      while (!stoppingToken.IsCancellationRequested)
      {
         Console.Write(COMMAND_START);

         string? userInput = await ReadLineWithCancellationAsync(stoppingToken);
         if (userInput == null || userInput.Trim() == EXIT_COMMAND)
            break;

         var args = userInput.Split(' ', StringSplitOptions.RemoveEmptyEntries);
         if (args.Length == 0)
            continue;

         var command = args[0];
         var commandArgs = args.Skip(1).ToArray();

         if (_cliCommandRegistry.AvailableCommands.Contains(command))
         {
            await _cliCommandRegistry.ExecuteCommand(command, commandArgs);
            Console.WriteLine("Command complete");
         }
         else
         {
            Console.WriteLine($"Command \"{command}\" unknown.");
         }
      }

      Console.WriteLine("CLI shutting down.");
   }

   // Here until Console.ReadLineAsync exists. Still blocks a thread unfortunately
   private static async Task<string?> ReadLineWithCancellationAsync(CancellationToken token)
   {
      return await Task.Run(() =>
      {
         try
         {
            return Console.ReadLine();
         }
         catch
         {
            return null;
         }
      }, token);
   }

}