using Cysharp.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OpenMod.API.Plugins;
using OpenMod.Unturned.Plugins;
using System;

[assembly: PluginMetadata("EvolutionPlugins.GameStores", DisplayName = "GameStores", Author = "EvolutionPlugins", Website = "https://discord.gg/6KymqGv")]

namespace EvolutionPlugins.GameStores;

public class GameStoresPlugin : OpenModUnturnedPlugin
{
    public GameStoresPlugin(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }

    protected override UniTask OnLoadAsync()
    {
        Logger.LogInformation("Made with <3 by Evolution Plugins");
        Logger.LogInformation("Owner of EvolutionPlugins: DiFFoZ");
        Logger.LogInformation("https://github.com/evolutionplugins \\ https://github.com/diffoz");
        Logger.LogInformation("Discord Support: https://discord.gg/6KymqGv");

        if (Configuration["secretKey"] is null or { Length: 0 } or "Key")
        {
            Logger.LogWarning("Secret key is not setted!");
        }

        return base.OnLoadAsync();
    }
}
