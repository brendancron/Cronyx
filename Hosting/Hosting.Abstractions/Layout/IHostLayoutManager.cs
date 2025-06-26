namespace Cronyx.Hosting.Abstractions.Layout;

public interface IHostLayoutManager
{

   List<HostDefinition> ParseConfig(string configPath);

}
