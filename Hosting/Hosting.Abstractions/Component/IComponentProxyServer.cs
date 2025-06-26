namespace Cronyx.Hosting.Abstractions.Component;

public interface IComponentProxyServer<TComponent>
   where TComponent : IComponent
{
   protected TComponent Component { get; }
}
