using EvolutionPlugins.GameStores.API.Exceptions;
using EvolutionPlugins.GameStores.Services;
using Steamworks;

namespace EvolutionPlugins.GameStores.Tests;

public class GameStoresManagerTests
{
    private static readonly CSteamID s_CSteamID = new(76561198138512156);

    private GameStoresManager m_Manager;

    [SetUp]
    public async Task SetupAsync()
    {
        var secretKey = Environment.GetEnvironmentVariable("GS_SecretKey", EnvironmentVariableTarget.User) ?? throw new Exception("No key presented in environment");
        m_Manager = new(new HttpClient(), "39774", secretKey, "0");

        // set balance to zero
        var balance = await m_Manager.GetBalanceAsync(s_CSteamID);
        if (balance <= 0.01m)
        {
            return;
        }

        await m_Manager.UpdateBalanceAsync(s_CSteamID, -balance, "[TEST] Set to zero");
    }

    [Test]
    public async Task Balance_Tests()
    {
        var balance = await m_Manager.GetBalanceAsync(s_CSteamID);
        Assert.That(balance, Is.Zero);

        Assert.ThrowsAsync<NotEnoughBalanceException>(async () => await m_Manager.UpdateBalanceAsync(s_CSteamID, -1, null));

        var newBalance = await m_Manager.UpdateBalanceAsync(s_CSteamID, 100, Guid.NewGuid().ToString());
        Assert.That(newBalance, Is.EqualTo(100));

        newBalance = await m_Manager.UpdateBalanceAsync(s_CSteamID, -25.5m, Guid.NewGuid().ToString());
        Assert.That(newBalance, Is.EqualTo(74.5m));
    }

    [Test]
    public void UpdateBalance_ThrowsArgumentOutOfRangeException()
    {
        Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () => await m_Manager.UpdateBalanceAsync(s_CSteamID, -1000001, null));
        Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () => await m_Manager.UpdateBalanceAsync(s_CSteamID, 1000001, null));
    }

    [Test]
    public void UpdateBalance_ThrowsArgumentException()
    {
        Assert.ThrowsAsync<ArgumentException>(async () => await m_Manager.UpdateBalanceAsync(s_CSteamID, 0, null));
    }

    [Test]
    public void GetBalance_ThrowsUserNotFoundException()
    {
        Assert.ThrowsAsync<UserNotFoundException>(async () => await m_Manager.GetBalanceAsync((CSteamID)(s_CSteamID.m_SteamID + 1)));
    }

    [Test]
    public async Task GetItems_NotThrowsUserNotFoundException()
    {
        // gamestores returns 104 (player bucket is empty) even if player is not exists

        var items = await m_Manager.GetItemsAsync((CSteamID)(s_CSteamID.m_SteamID + 1));
        Assert.That(items.Any(), Is.False);
    }

    [Test]
    public void UpdateBalance_ThrowsUserNotFoundException()
    {
        Assert.ThrowsAsync<UserNotFoundException>(async () => await m_Manager.UpdateBalanceAsync((CSteamID)(s_CSteamID.m_SteamID + 1), -1, null));
    }

    [Test]
    public async Task GetItems_ShouldReturnsNothing()
    {
        var items = await m_Manager.GetItemsAsync(s_CSteamID);
        Assert.That(items, Is.Empty);
    }
}