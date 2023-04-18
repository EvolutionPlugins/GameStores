using System;
using System.Threading.Tasks;
using EvolutionPlugins.GameStores.API;
using Microsoft.Extensions.Localization;
using OpenMod.API.Commands;
using OpenMod.Core.Commands;
using OpenMod.Core.Console;
using OpenMod.Unturned.Users;

namespace EvolutionPlugins.GameStores.Commands;

[Command("take")]
[CommandParent(typeof(CommandStore))]
[CommandSyntax("<index>")]
[CommandActor(typeof(UnturnedUser))]
public class CommandStoreTake : CommandStore
{
    private readonly IGameStoresManager m_GameStoresManager;
    private readonly IStringLocalizer m_StringLocalizer;

    public CommandStoreTake(IServiceProvider serviceProvider, IGameStoresManager gameStoresManager, IStringLocalizer stringLocalizer, ICommandExecutor commandExecutor,
        IConsoleActorAccessor consoleActorAccessor) : base(serviceProvider, gameStoresManager, stringLocalizer, commandExecutor, consoleActorAccessor)
    {
        m_GameStoresManager = gameStoresManager;
        m_StringLocalizer = stringLocalizer;
    }

    protected override async Task OnExecuteAsync()
    {
        if (Context.Parameters.Count != 1)
        {
            throw new CommandWrongUsageException(Context);
        }

        var count = await Context.Parameters.GetAsync<int>(0);
        if (count <= 0)
        {
            throw new CommandWrongUsageException(Context);
        }

        var user = (UnturnedUser)Context.Actor;

        var items = await m_GameStoresManager.GetItemsAsync(user.SteamId);
        if (items.Count == 0)
        {
            throw new UserFriendlyException(m_StringLocalizer["errors:empty"]);
        }

        if (count > items.Count)
        {
            throw new UserFriendlyException(m_StringLocalizer["errors:takeNumberError"]);
        }

        var item = items[count - 1];
        await ProcessItemAsync(user, item);
    }
}
