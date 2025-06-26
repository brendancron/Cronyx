namespace CLI.Abstractions;

public interface ICliCommandRegistry
{
   public IEnumerable<string> AvailableCommands { get; }
   void RegisterCommand(string command, Delegate func);
   Task ExecuteCommand(string command, params object[] args);
}
