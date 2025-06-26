namespace Cronyx.Hosting.Abstractions.Layout;

public class HostDefinition
{

   public required string HostId { get; set; }
   public List<ComponentDefinition> Components { get; set; } = new();

}