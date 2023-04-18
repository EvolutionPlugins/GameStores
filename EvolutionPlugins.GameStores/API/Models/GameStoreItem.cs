using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace EvolutionPlugins.GameStores.API.Models;

public sealed class GameStoreItem
{
    [JsonProperty("id")]
    public string Id { get; set; } = string.Empty;

    [JsonProperty("name")]
    public string? Name { get; set; }

    [JsonProperty("amount")]
    public string? Amount { get; set; }

    [JsonProperty("type")]
    [JsonConverter(typeof(StringEnumConverter))]
    public GameStoreItemType Type { get; set; }

    [JsonProperty("command")]
    public string? Command { get; set; }

    [JsonProperty("item_id")]
    public string? ItemId { get; set; }

    public override string ToString()
    {
        return $"[{Id}] {Type} {Name}";
    }
}