using System;
using System.Linq;
using System.Threading.Tasks;
using EvolutionPlugins.GameStores.API;
using Microsoft.Extensions.Localization;
using OpenMod.API.Commands;
using OpenMod.Core.Commands;
using OpenMod.Unturned.Users;

namespace EvolutionPlugins.GameStores.Commands;

[Command("page")]
[CommandAlias("list")]
[CommandParent(typeof(CommandStore))]
[CommandActor(typeof(UnturnedUser))]
[CommandSyntax("<page>")]
public class CommandStorePage : Command
{
    private const int c_ItemsPerPage = 5;

    private readonly IGameStoresManager m_GameStoresManager;
    private readonly IStringLocalizer m_StringLocalizer;

    public CommandStorePage(IServiceProvider serviceProvider, IGameStoresManager gameStoresManager, IStringLocalizer stringLocalizer) : base(serviceProvider)
    {
        m_GameStoresManager = gameStoresManager;
        m_StringLocalizer = stringLocalizer;
    }

    protected override async Task OnExecuteAsync()
    {
        var user = (UnturnedUser)Context.Actor;
        if (!Context.Parameters.TryGet<int>(0, out var page))
        {
            page = 1;
        }

        if (page < 1)
        {
            throw new CommandWrongUsageException(Context);
        }

        var items = await m_GameStoresManager.GetItemsAsync(user.SteamId);
        if (items.Count == 0)
        {
            throw new UserFriendlyException(m_StringLocalizer["errors:empty"]);
        }

        var pageItems = items
            .Skip((page - 1) * c_ItemsPerPage)
            .Take(c_ItemsPerPage);

        if (!pageItems.Any())
        {
            throw new UserFriendlyException(m_StringLocalizer["errors:pageNotFound"]);
        }

        var pages = (int)Math.Ceiling(items.Count / (double)c_ItemsPerPage);
        await PrintAsync(m_StringLocalizer["page:header", new { Page = page, Pages = pages }]);

        var id = (page - 1) * c_ItemsPerPage;
        foreach (var item in pageItems)
        {
            id++;
            await PrintAsync(m_StringLocalizer["page:item", new { Index = id, item.Name, item.Amount }]);
        }

        await PrintAsync(m_StringLocalizer["page:takeCommand"]);

        if (pages > page)
        {
            await PrintAsync(m_StringLocalizer["page:nextPage", new { Page = page + 1 }]);
            return;
        }

        if (page - 1 > 0)
        {
            await PrintAsync(m_StringLocalizer["page:previousPage", new { Page = page - 1 }]);
        }
    }
}
