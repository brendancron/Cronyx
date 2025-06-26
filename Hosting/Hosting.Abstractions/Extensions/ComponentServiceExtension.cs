using Cronyx.Hosting.Abstractions.Component;
using Cronyx.Hosting.Abstractions.Layout;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Cronyx.Hosting.Abstractions.Extensions;

public static class ComponentServiceExtension
{

   private static readonly Dictionary<string, Type> _componentTypes = new();
   private static readonly Dictionary<Type, Type> _implTypes = new();
   private static readonly Dictionary<Type, Type> _proxyTypes = new();
   private static readonly Dictionary<Type, Type> _serverImpls = new();

   public static void AddComponentImpl<TComponent, TImpl>(this IServiceCollection services)
      where TComponent : IComponent
      where TImpl : TComponent
   {
      var componentType = typeof(TComponent);
      var implType = typeof(TImpl);
      _componentTypes[componentType.Name] = componentType;
      _implTypes[componentType] = implType;
   }

   public static void AddComponentProxy<TComponent, TProxy>(this IServiceCollection services)
      where TComponent : IComponent
      where TProxy : TComponent
   {
      var componentType = typeof(TComponent);
      var proxyType = typeof(TProxy);
      _componentTypes[componentType.Name] = componentType;
      _proxyTypes[componentType] = proxyType;
   }

   public static void AddComponentProxyServer<TComponent, TProxyServer>(this IServiceCollection services)
      where TComponent : IComponent
      where TProxyServer : IComponentProxyServer<TComponent>
   {
      var componentType = typeof(TComponent);
      var proxyServerType = typeof(TProxyServer);
      _componentTypes[componentType.Name] = componentType;
      _serverImpls[componentType] = proxyServerType;
   }

   public static void AddComponent(this IServiceCollection services, ComponentDefinition componentDefinition, bool isLocal)
   {
      string componentId = componentDefinition.ComponentId;
      if (!_componentTypes.TryGetValue(componentDefinition.Type, out Type? componentType))
      {
         throw new KeyNotFoundException($"Component with name {componentDefinition.Type} has not been registered");
      }
      if (isLocal)
      {
         if (_implTypes.TryGetValue(componentType, out Type? implType))
         {
            services.AddComponentWithData(componentType, implType, componentId);
            if (componentDefinition.UseProxy && _serverImpls.TryGetValue(componentType, out Type? serverType))
            {
               services.AddSingleton(typeof(IHostedService), provider =>
               {
                  return ActivatorUtilities.CreateInstance(provider, serverType, componentId);
               });
            }
         }
      }
      else
      {
         if (componentDefinition.UseProxy && _proxyTypes.TryGetValue(componentType, out Type? proxyType))
         {
            services.AddComponentWithData(componentType, proxyType, componentId);
            services.AddSingleton(typeof(IHostedService), provider => provider.GetRequiredKeyedService(proxyType, componentId));
         }
      }
   }

   private static IServiceCollection AddComponentWithData(this IServiceCollection services, Type definitionType, Type instanceType, string componentId)
   {
      services.AddKeyedSingleton(instanceType, componentId, (provider, key) =>
      {
         return ActivatorUtilities.CreateInstance(provider, instanceType, componentId);
      });

      services.AddKeyedSingleton(definitionType, componentId, (provider, key) =>
      {
         return provider.GetRequiredKeyedService(instanceType, componentId);
      });

      services.AddSingleton(instanceType, provider =>
      {
         return provider.GetRequiredKeyedService(instanceType, componentId);
      });

      services.AddSingleton(definitionType, provider =>
      {
         return provider.GetRequiredKeyedService(instanceType, componentId);
      });

      services.AddSingleton(typeof(IComponent), provider => provider.GetRequiredKeyedService(instanceType, componentId));

      return services;
   }

}
