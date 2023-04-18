using EvolutionPlugins.GameStores.API.Models;
using OpenMod.API.Ioc;
using Steamworks;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using EvolutionPlugins.GameStores.API.Exceptions;
using System.Net.Http;

namespace EvolutionPlugins.GameStores.API;

[Service]
public interface IGameStoresManager
{
    /// <summary>
    /// Gets the player bucket of items
    /// </summary>
    /// <param name="steamID">Player steamId</param>
    /// <returns>List of items in the player bucket</returns>
    /// <exception cref="HttpRequestException">The request failed due to an underlying issue such as network connectivity, DNS failure, server certificate validation or timeout.</exception>
    /// <remarks>The <see cref="UserNotFoundException"/> is not thrown even if player is not exists. GameStores API returns instead 104 code (player bucket is empty)</remarks>
    Task<IReadOnlyList<GameStoreItem>> GetItemsAsync(CSteamID steamID);

    /// <summary>
    /// Sets the item status to 'Given'
    /// </summary>
    /// <param name="item">Item of player bucket</param>
    /// <exception cref="HttpRequestException">The request failed due to an underlying issue such as network connectivity, DNS failure, server certificate validation or timeout.</exception>
    Task SetItemStatusGivenAsync(GameStoreItem item);

    /// <summary>
    /// Gets balance of an player
    /// </summary>
    /// <param name="steamID">Player steamId</param>
    /// <returns>The balance of an player</returns>
    /// <exception cref="UserNotFoundException">Thrown when tried to get balance of unknown <paramref name="steamID"/></exception>
    /// <exception cref="HttpRequestException">The request failed due to an underlying issue such as network connectivity, DNS failure, server certificate validation or timeout.</exception>
    Task<decimal> GetBalanceAsync(CSteamID steamID);

    /// <summary>
    /// Updates balance of an player
    /// </summary>
    /// <param name="steamID">Player steamId</param>
    /// <param name="balance">The value to update the balance, <b>should be not zero and in range [-1000000;1000000]</b></param>
    /// <param name="reason">The reason of update</param>
    /// <returns>New balance after updating</returns>
    /// <exception cref="UserNotFoundException">Thrown when tried to update balance of unknown <paramref name="steamID"/></exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="balance"/> is out of range <b>[-1000000;1000000]</b></exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="balance"/> is zero</exception>
    /// <exception cref="HttpRequestException">The request failed due to an underlying issue such as network connectivity, DNS failure, server certificate validation or timeout.</exception>
    Task<decimal> UpdateBalanceAsync(CSteamID steamID, decimal balance, string? reason);
}
