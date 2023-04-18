using Cysharp.Text;
using EvolutionPlugins.GameStores.API;
using EvolutionPlugins.GameStores.API.Exceptions;
using EvolutionPlugins.GameStores.API.Models;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using OpenMod.API.Ioc;
using OpenMod.API.Plugins;
using OpenMod.API.Prioritization;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Threading.Tasks;

namespace EvolutionPlugins.GameStores.Services;

[ServiceImplementation(Lifetime = ServiceLifetime.Singleton, Priority = Priority.Lowest)]
public class GameStoresManager : IGameStoresManager
{
    private static readonly IReadOnlyList<GameStoreItem> s_EmptyList = new List<GameStoreItem>().AsReadOnly();

    private readonly HttpClient m_HttpClient;
    private readonly string m_BaseRequest;

    public GameStoresManager(IPluginAccessor<GameStoresPlugin> pluginActivator, HttpClient httpClient)
    {
        m_BaseRequest = GetBaseRequest(pluginActivator);
        m_HttpClient = httpClient;
    }

    internal GameStoresManager(HttpClient client, string shopId, string secretKey, string serverId)
    {
        m_HttpClient = client;
        m_BaseRequest = $"https://gamestores.app/api/?shop_id={shopId}&secret={secretKey}&server={serverId}";
    }

    private string GetBaseRequest(IPluginAccessor<GameStoresPlugin> pluginActivator)
    {
        var configuration = pluginActivator.Instance?.Configuration ?? throw new Exception("Plugin is not loaded");
        return $"https://gamestores.app/api/?shop_id={configuration["shopId"]}&secret={configuration["secretKey"]}&server={configuration["serverId"]}";
    }

    public async Task<decimal> GetBalanceAsync(CSteamID steamID)
    {
        using var stringBuilder = ZString.CreateStringBuilder();

        stringBuilder.Append(m_BaseRequest);
        stringBuilder.Append("&action=balance&steam_id=");
        stringBuilder.Append(steamID.m_SteamID);
        var url = stringBuilder.ToString();

        var json = await m_HttpClient.GetStringAsync(url);
        var response = JsonConvert.DeserializeObject<ResponseActionBalance?>(json)
            ?? throw new Exception("GameStores returned invalid JSON data");

        if (response.Value is null)
        {
            EnsureSuccessResponse(response, steamID);
            return 0;
        }

        return response.Value.Value;
    }

    public async Task<IReadOnlyList<GameStoreItem>> GetItemsAsync(CSteamID steamID)
    {
        using var stringBuilder = ZString.CreateStringBuilder();

        stringBuilder.Append(m_BaseRequest);
        stringBuilder.Append("&items=true&steam_id=");
        stringBuilder.Append(steamID.m_SteamID);
        var url = stringBuilder.ToString();

        var json = await m_HttpClient.GetStringAsync(url);
        var response = JsonConvert.DeserializeObject<ResponseActionItems?>(json)
            ?? throw new Exception("GameStores returned invalid JSON data");

        // player busket is empty or user not found
        if (response.CodeResult is 104)
            return s_EmptyList;

        EnsureSuccessResponse(response, steamID);
        return response.Items ?? s_EmptyList;
    }

    public async Task SetItemStatusGivenAsync(GameStoreItem item)
    {
        using var stringBuilder = ZString.CreateStringBuilder();

        stringBuilder.Append(m_BaseRequest);
        stringBuilder.Append("&gived=true&id=");
        stringBuilder.Append(item.Id);
        var url = stringBuilder.ToString();

        var json = await m_HttpClient.GetStringAsync(url);
        var response = JsonConvert.DeserializeObject<BaseResponse?>(json)
            ?? throw new Exception("GameStores returned invalid JSON data");

        EnsureSuccessResponse(response, CSteamID.Nil);
    }

    public async Task<decimal> UpdateBalanceAsync(CSteamID steamID, decimal changeAmount, string? reason)
    {
        if (Math.Abs(changeAmount) < 0.01m)
        {
            throw new ArgumentException("Change amount cannot be zero", nameof(changeAmount));
        }

        if (Math.Abs(changeAmount) > 1000000m)
        {
            throw new ArgumentOutOfRangeException(nameof(changeAmount));
        }

        using var stringBuilder = ZString.CreateStringBuilder();

        stringBuilder.Append(m_BaseRequest);
        stringBuilder.Append("&action=moneys&steam_id=");
        stringBuilder.Append(steamID.m_SteamID);
        stringBuilder.Append("&type=");
        stringBuilder.Append(changeAmount < 0 ? "minus" : "plus");
        stringBuilder.Append("&amount=");
        stringBuilder.Append(Math.Abs(changeAmount).ToString(CultureInfo.InvariantCulture));

        if (!string.IsNullOrEmpty(reason))
        {
            stringBuilder.Append("&mess=");
            stringBuilder.Append(Uri.EscapeDataString(reason));
        }
        var url = stringBuilder.ToString();

        var json = await m_HttpClient.GetStringAsync(url);
        var response = JsonConvert.DeserializeObject<ResponseActionUpdateBalance>(json)
            ?? throw new Exception("GameStores returned invalid JSON data");

        if (response.NewBalance is not null)
        {
            return response.NewBalance.Value;
        }

        EnsureSuccessResponse(response, steamID);
        return 0;
    }

    private void EnsureSuccessResponse(BaseResponse response, CSteamID steamID)
    {
        if (response.Status?.Equals("success", StringComparison.OrdinalIgnoreCase) == true)
        {
            return;
        }

        if (string.IsNullOrEmpty(response.MessageResult))
        {
            return;
        }

        switch (response.CodeResult)
        {
            // success
            case null or 100 or 107:
                return;

            // "Недопустимый steam ID"
            case 101:
                throw new ArgumentException(response.MessageResult, nameof(steamID));

            // "Пользователь не найден"
            case 113:
                throw new UserNotFoundException(response.MessageResult, steamID);

            // Недостаточно средств на счету
            case 115:
                throw new NotEnoughBalanceException(response.MessageResult!, steamID);

            // unhandled status error
            default:
                throw new Exception($"GameStore API result failed. Code result: {response.CodeResult}. Error message: {response.MessageResult}");
        }
    }

    private class BaseResponse
    {
        [JsonProperty("result")]
        public string Status { get; set; } = string.Empty;

        [JsonProperty("message")]
        public string? MessageResult { get; set; }

        [JsonProperty("code")]
        public int? CodeResult { get; set; }
    }

    private sealed class ResponseActionBalance : BaseResponse
    {
        [JsonProperty("value")]
        public decimal? Value { get; set; }
    }

    private sealed class ResponseActionUpdateBalance : BaseResponse
    {
        [JsonProperty("newBalance")]
        public decimal? NewBalance { get; set; }
    }

    private sealed class ResponseActionItems : BaseResponse
    {
        [JsonProperty("data")]
        public IReadOnlyList<GameStoreItem>? Items { get; set; }
    }
}
