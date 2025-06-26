using Cronyx.Hosting.Abstractions.Extensions;
using Cronyx.Hosting.Abstractions.Layout;
using Cronyx.Hosting.Abstractions.Modules;
using Microsoft.Extensions.Hosting;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Reflection;

namespace Cronyx.Hosting.Abstractions;

public class HostManager
{

   public async Task<IHost> LoadApplicationAsync(IHostBuilder builder, IHostLayoutManager layoutManager, string configPath, string hostId)
   {
      Stopwatch stopwatch = Stopwatch.StartNew();
      var layout = layoutManager.ParseConfig(configPath);
      Console.WriteLine($"Layout parse time: {stopwatch.Elapsed:hh\\:mm\\:ss\\.fff}");
      stopwatch.Restart();

      var assemblies = LoadAssemblies();
      var modules = LoadModules(assemblies);

      Console.WriteLine($"Module load time: {stopwatch.Elapsed:hh\\:mm\\:ss\\.fff}");
      stopwatch.Restart();

      ConfigureModules(builder, modules);

      ConfigureLayout(builder, layout, hostId);

      Console.WriteLine($"Time spent Configuring: {stopwatch.Elapsed:hh\\:mm\\:ss\\.fff}");
      stopwatch.Restart();

      IHost host = builder.Build();

      InitializeModules(host, modules);

      Console.WriteLine($"Time spent Initializing: {stopwatch.Elapsed:hh\\:mm\\:ss\\.fff}");
      stopwatch.Stop();

      await host.StartAsync();

      return host;
   }

   private List<Assembly> LoadAssemblies()
   {
      var assemblies = new ConcurrentBag<Assembly>();
      var assemblyFiles = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory, "*.dll");

      Parallel.ForEach(assemblyFiles, assemblyPath =>
      {
         try
         {
            var assembly = Assembly.LoadFrom(assemblyPath);
            assemblies.Add(assembly);
         }
         catch (Exception ex)
         {
            Console.WriteLine($"Could not load assembly {assemblyPath}: {ex.Message}");
         }
      });

      return assemblies.ToList();
   }

   private List<IModule> LoadModules(List<Assembly> assemblies)
   {
      var modules = new List<IModule>();
      foreach (var assembly in assemblies)
      {
         Type[] types;
         try
         {
            types = assembly.GetTypes();
         }
         catch (ReflectionTypeLoadException ex)
         {
            types = ex.Types.Where(t => t != null).ToArray()!;
         }

         foreach (var type in types)
         {
            if (type == null || type.IsAbstract || type.IsInterface || !typeof(IModule).IsAssignableFrom(type))
               continue;
            try
            {
               if (Activator.CreateInstance(type) is IModule module)
               {
                  modules.Add(module);
               }
            }
            catch (Exception ex)
            {
               Console.WriteLine($"Failed to create IModule instance from {type.FullName}: {ex.Message}");
            }
         }
      }
      return modules;
   }

   private void ConfigureModules(IHostBuilder builder, List<IModule> modules)
   {
      foreach (IModule module in modules)
      {
         module.ConfigureModule(builder);
      }
   }

   private void ConfigureLayout(IHostBuilder builder, List<HostDefinition> layout, string hostId)
   {
      builder.ConfigureServices((context, services) =>
      {
         foreach (HostDefinition hostDefinition in layout)
         {
            bool isLocal = hostId == hostDefinition.HostId;
            foreach (ComponentDefinition componentDefinition in hostDefinition.Components)
            {
               try
               {
                  services.AddComponent(componentDefinition, isLocal);
               }
               catch (KeyNotFoundException ex)
               {
                  Console.WriteLine(ex.Message);
               }
            }
         }
      });
   }

   private void InitializeModules(IHost host, List<IModule> modules)
   {
      foreach (IModule module in modules)
      {
         module.InitializeModule(host);
      }
   }

}
