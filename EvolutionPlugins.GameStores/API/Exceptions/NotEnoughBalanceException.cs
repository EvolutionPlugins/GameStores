using OpenMod.API.Commands;
using Steamworks;

namespace EvolutionPlugins.GameStores.API.Exceptions;
/// <summary>
/// The exception that is thrown when an player does not have enough balance
/// </summary>
public class NotEnoughBalanceException : UserFriendlyException
{
    /// <summary>
    /// The player id
    /// </summary>
    public CSteamID SteamID { get; }

    public NotEnoughBalanceException(string message, CSteamID steamID) : base(message)
    {
        SteamID = steamID;
    }

    public NotEnoughBalanceException(string message) : base(message)
    {
    }
}
