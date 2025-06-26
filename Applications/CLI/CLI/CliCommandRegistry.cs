using CLI.Abstractions;

namespace CLI;

public class CliCommandRegistry : ICliCommandRegistry
{

   private Dictionary<string, Delegate> _commandRegistry = new();

   public IEnumerable<string> AvailableCommands => _commandRegistry.Keys;

   public void RegisterCommand(string command, Delegate func)
   {
      if (string.IsNullOrWhiteSpace(command) || command.Contains(' '))
         throw new ArgumentException("Command names must be non-empty and contain no spaces.", nameof(command));
      _commandRegistry[command] = func ?? throw new ArgumentNullException(nameof(func));
   }

   public async Task ExecuteCommand(string command, params object[] args)
   {
      if (!_commandRegistry.TryGetValue(command, out Delegate? func))
      {
         Console.WriteLine($"Command \"{command}\" not found.");
         return;
      }

      var method = func.Method;
      var parameters = method.GetParameters();

      if (args.Length != parameters.Length)
      {
         Console.WriteLine($"Command \"{command}\" expects {parameters.Length} arguments, but {args.Length} were provided.");
         return;
      }

      var convertedArgs = new object[parameters.Length];
      for (int i = 0; i < parameters.Length; i++)
      {
         try
         {
            convertedArgs[i] = Convert.ChangeType(args[i], parameters[i].ParameterType);
         }
         catch (Exception ex)
         {
            Console.WriteLine($"Failed to convert argument {i} ('{args[i]}') to {parameters[i].ParameterType.Name}: {ex.Message}");
            return;
         }
      }

      try
      {
         var result = func.DynamicInvoke(convertedArgs);
         if (result is Task task)
            await task;
      }
      catch (Exception ex)
      {
         Console.WriteLine($"Error invoking command \"{command}\": {ex.Message}");
      }
   }

}
