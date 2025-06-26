using Microsoft.Extensions.Hosting;
using System.Net;

namespace Cronyx.Hosting.Abstractions.Modules;

public interface IModule
{

   IHostBuilder ConfigureModule(IHostBuilder builder);
   void InitializeModule(IHost host);

}
