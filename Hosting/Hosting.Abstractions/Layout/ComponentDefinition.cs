namespace Cronyx.Hosting.Abstractions.Layout;

public class ComponentDefinition
{

   public required string ComponentId { get; set; }
   public required string Type { get; set; }
   public bool UseProxy { get; set; } = true;

}
