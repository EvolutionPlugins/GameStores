using System;
using Steamworks;

namespace EvolutionPlugins.GameStores.API.Exceptions;
/// <summary>
/// The exception that is throw when requesting API data of not registered user
/// </summary>
public sealed class UserNotFoundException : Exception
{
    /// <summary>
    /// SteamId of not registered user
    /// </summary>
    public CSteamID SteamID { get; }

    public UserNotFoundException(string? message) : base(message)
    {
    }

    public UserNotFoundException(string? message, CSteamID steamID) : base(message)
    {
        SteamID = steamID;
    }
}
