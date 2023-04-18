using Microsoft.Extensions.DependencyInjection;
using OpenMod.API.Ioc;
using System.Net.Http;

namespace EvolutionPlugins.GameStores;
public class ServiceConfigurator : IServiceConfigurator
{
    public void ConfigureServices(IOpenModServiceConfigurationContext openModStartupContext, IServiceCollection serviceCollection)
    {
        serviceCollection.AddSingleton<HttpClient>();
    }
}
