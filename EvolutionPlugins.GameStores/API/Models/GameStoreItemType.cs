using System.Runtime.Serialization;

namespace EvolutionPlugins.GameStores.API.Models;

public enum GameStoreItemType
{
    [EnumMember(Value = "item")]
    Item,
    [EnumMember(Value = "command")]
    Command
}