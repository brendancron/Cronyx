using Cronyx.Hosting.Abstractions.Layout;
using YamlDotNet.Serialization;

namespace LayoutManager.Yaml;

public class YamlHostLayoutManager : IHostLayoutManager
{
   public List<HostDefinition> ParseConfig(string configPath)
   {
      if (string.IsNullOrWhiteSpace(configPath) || !File.Exists(configPath))
      {
         return new();
      }
      var deserializer = new DeserializerBuilder()
         .IgnoreUnmatchedProperties()
         .Build();

      using var reader = new StreamReader(configPath);
      return deserializer.Deserialize<List<HostDefinition>>(reader);
   }
}
