using System;
using System.Linq;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using EvolutionPlugins.GameStores.API;
using EvolutionPlugins.GameStores.API.Models;
using EvolutionPlugins.GameStores.Helpers;
using Microsoft.Extensions.Localization;
using OpenMod.API.Commands;
using OpenMod.Core.Commands;
using OpenMod.Core.Console;
using OpenMod.Core.Helpers;
using OpenMod.Unturned.Users;
using SDG.Unturned;
using Command = OpenMod.Core.Commands.Command;

namespace EvolutionPlugins.GameStores.Commands;

[Command("store")]
[CommandSyntax("<all, page, take>")]
[CommandActor(typeof(UnturnedUser))]
public class CommandStore : Command
{
    private readonly IGameStoresManager m_GameStoresManager;
    private readonly IStringLocalizer m_StringLocalizer;
    private readonly ICommandExecutor m_CommandExecutor;
    private readonly IConsoleActorAccessor m_ConsoleActorAccessor;

    public CommandStore(IServiceProvider serviceProvider, IGameStoresManager gameStoresManager, IStringLocalizer stringLocalizer,
        ICommandExecutor commandExecutor, IConsoleActorAccessor consoleActorAccessor) : base(serviceProvider)
    {
        m_GameStoresManager = gameStoresManager;
        m_StringLocalizer = stringLocalizer;
        m_CommandExecutor = commandExecutor;
        m_ConsoleActorAccessor = consoleActorAccessor;
    }

    protected override Task OnExecuteAsync()
    {
        return Task.FromException(new CommandWrongUsageException(Context));
    }

    protected async Task ProcessItemAsync(UnturnedUser user, GameStoreItem item)
    {
        if (item.Type is GameStoreItemType.Item)
        {
            if (string.IsNullOrEmpty(item.Amount) || !int.TryParse(item.Amount, out var amount))
            {
                amount = 1;
            }

            if (!string.IsNullOrEmpty(item.ItemId) && ushort.TryParse(item.ItemId, out var itemId))
            {
                await m_GameStoresManager.SetItemStatusGivenAsync(item);

                await UniTask.SwitchToMainThread();
                for (var x = 0; x < amount; x++)
                {
                    var uItem = new Item(itemId, EItemOrigin.ADMIN);
                    user.Player.Player.inventory.forceAddItem(uItem, true);
                }

                await PrintAsync(m_StringLocalizer["take:itemWithAmount", new { item.Name, Amount = amount }]);
            }

            return;
        }

        if (!string.IsNullOrEmpty(item.Command) && item.Type is GameStoreItemType.Command)
        {
            await m_GameStoresManager.SetItemStatusGivenAsync(item);

            foreach (var command in item.Command!
                .Replace(Environment.NewLine, "\n")
                .Split('\n')
                .Select(x => ReplaceVariablesInCommand(user, x)))
            {
                var args = ArgumentsParser.ParseArguments(command);
                await m_CommandExecutor.ExecuteAsync(m_ConsoleActorAccessor.Actor, args, string.Empty);
            }

            await PrintAsync(m_StringLocalizer["take:item", new { item.Name }]);
        }
    }

    private static string ReplaceVariablesInCommand(UnturnedUser user, string command) => command
        .Replace("%steamid%", user.Id, StringComparison.OrdinalIgnoreCase)
        .Replace("%username%", user.DisplayName, StringComparison.OrdinalIgnoreCase);
}
