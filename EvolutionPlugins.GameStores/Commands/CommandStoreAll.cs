using System;
using System.Threading.Tasks;
using EvolutionPlugins.GameStores.API;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using OpenMod.API.Commands;
using OpenMod.Core.Commands;
using OpenMod.Core.Console;
using OpenMod.Unturned.Users;
using SmartFormat.ZString;
using Command = OpenMod.Core.Commands.Command;

namespace EvolutionPlugins.GameStores.Commands;

[Command("all")]
[CommandParent(typeof(CommandStore))]
[CommandActor(typeof(UnturnedUser))]
public class CommandStoreAll : CommandStore
{
    private readonly IGameStoresManager m_GameStoresManager;
    private readonly IStringLocalizer m_StringLocalizer;
    private readonly ILogger<CommandStoreAll> m_Logger;

    public CommandStoreAll(IServiceProvider serviceProvider, IGameStoresManager gameStoresManager, IStringLocalizer stringLocalizer, ICommandExecutor commandExecutor,
        IConsoleActorAccessor consoleActorAccessor, ILogger<CommandStoreAll> logger) : base(serviceProvider, gameStoresManager, stringLocalizer, commandExecutor, consoleActorAccessor)
    {
        m_GameStoresManager = gameStoresManager;
        m_StringLocalizer = stringLocalizer;
        m_Logger = logger;
    }

    protected override async Task OnExecuteAsync()
    {
        var user = (UnturnedUser)Context.Actor;

        var items = await m_GameStoresManager.GetItemsAsync(user.SteamId);
        if (items.Count == 0)
        {
            throw new UserFriendlyException(m_StringLocalizer["errors:empty"]);
        }

        using var sb = new ZStringBuilder(false);
        sb.Append("Giving to a ");
        sb.Append(user.FullActorName);
        sb.Append(' ');

        for (var i = 0; i < items.Count; i++)
        {
            if (i == 10)
            {
                await PrintAsync(m_StringLocalizer["errors:limit"]);
                return;
            }

            var item = items[i];
            await ProcessItemAsync(user, item);

            sb.Append(item.ToString());
            sb.Append(';');
        }

        m_Logger.LogInformation(sb.ToString());
    }
}
