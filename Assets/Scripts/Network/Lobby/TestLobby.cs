using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;

public class TestLobby : MonoBehaviour
{
    #region Singleton
    private static TestLobby _instance;
    public static TestLobby Instance { get { return _instance; } }
    private void Awake()
    {
        _instance = this;
    }
    #endregion
    private Lobby hostLobby;
    private float heartBeatTimer;
    private float lobbyUpdateTimer;
    string username;
    async void Start()
    {
        username = "Emil " + Random.Range(0, 100);
        await UnityServices.InitializeAsync();
        AuthenticationService.Instance.SignedIn += OnSignIn;
        await AuthenticationService.Instance.SignInAnonymouslyAsync();
        Debug.Log("Username: " + username);
    }
    private void Update()
    {
        HandleHeartbeat();
        HandleLobbyPollUpdate();
    }
    private async void HandleHeartbeat()
    {
        if (hostLobby != null)
        {
            heartBeatTimer -= Time.deltaTime;
            if (heartBeatTimer < 0f)
            {
                float heartBeatTimerMax = 15f; // send every 15 seconds
                heartBeatTimer = heartBeatTimerMax;
                await LobbyService.Instance.SendHeartbeatPingAsync(hostLobby.Id);
            }
        }
    }
    private async void HandleLobbyPollUpdate()
    {
        if (hostLobby != null)
        {
            lobbyUpdateTimer -= Time.deltaTime;
            if (lobbyUpdateTimer < 0f)
            {
                float heartBeatTimerMax = 1.1f; // send every 15 seconds
                lobbyUpdateTimer = heartBeatTimerMax;
                var lobby = await LobbyService.Instance.GetLobbyAsync(hostLobby.Id);
                hostLobby = lobby;
            }
        }
    }
    private void OnSignIn()
    {
        Debug.Log("Signed in: " + AuthenticationService.Instance.PlayerId);
    }
    public async void CreateLobby()
    {
        try
        {
            var lobbyName = "MyLobby";
            var maxPlayers = 2;
            CreateLobbyOptions options = new CreateLobbyOptions()
            {
                IsPrivate = true,
                Player = GetPlayer(),
                Data = new Dictionary<string, DataObject>
                {
                    {"GameMode", new DataObject(DataObject.VisibilityOptions.Public, "CaptureTheFlag", DataObject.IndexOptions.S1) }
                }
            };
            Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, options);
            Debug.Log("Created lobby: " + lobby.Name + " " + lobby.MaxPlayers + " " + lobby.Id + " " + lobby.LobbyCode);
            hostLobby = lobby;
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
            throw;
        }
    }
    public async void ListLobbies()
    {
        try
        {
            // filering lobbies
            QueryLobbiesOptions options = new QueryLobbiesOptions()
            {
                Count = 5,
                Filters = new List<QueryFilter> {
                    new QueryFilter(QueryFilter.FieldOptions.AvailableSlots, "0", QueryFilter.OpOptions.GT), // gt = greater than
                    new QueryFilter(QueryFilter.FieldOptions.S1, "CaptureTheFlag", QueryFilter.OpOptions.EQ) // EQ = equal
                },
                Order = new List<QueryOrder>
                {
                    new QueryOrder(false, QueryOrder.FieldOptions.Created)
                }
            };
            QueryResponse queryResponse = await Lobbies.Instance.QueryLobbiesAsync(options);
            foreach (var item in queryResponse.Results)
            {
                Debug.Log("Lobby: " + item.Name + " " + item.MaxPlayers);
            }
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
            throw;
        }
    }
    public async void JoinLobby()
    {
        try
        {
            QueryResponse queryResponse = await Lobbies.Instance.QueryLobbiesAsync();
            await Lobbies.Instance.JoinLobbyByIdAsync(queryResponse.Results[0].Id);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
            throw;
        }
    }
    public async void JoinLobbyByCode(string lobbyCode)
    {
        try
        {
            JoinLobbyByCodeOptions options = new JoinLobbyByCodeOptions()
            {
                Player = GetPlayer()
            };
            await Lobbies.Instance.JoinLobbyByCodeAsync(lobbyCode, options);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
            throw;
        }
    }
    public async void QuickJoinLobby()
    {
        try
        {
            await LobbyService.Instance.QuickJoinLobbyAsync();
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
            throw;
        }
    }
    public async void PrintPlayers(Lobby lobby)
    {
        foreach (Player player in lobby.Players)
        {
            Debug.Log("Player: " + player.Data["Name"].Value);
        }
        try
        {
            await LobbyService.Instance.QuickJoinLobbyAsync();
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
            throw;
        }
    }
    private Player GetPlayer()
    {
        return new Player
        {
            Data = new Dictionary<string, PlayerDataObject>
                    {
                        {"Name",  new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, username)}
                    }
        };
    }
    private async void UpdateGameMode()
    {
        try
        {
            var options = new UpdateLobbyOptions()
            {
                Data = new Dictionary<string, DataObject>
                {
                    {"GameMode", new DataObject(DataObject.VisibilityOptions.Public, "NotCaptureTheFlag", DataObject.IndexOptions.S1) }
                }
            };
            hostLobby = await Lobbies.Instance.UpdateLobbyAsync(hostLobby.Id, options);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
            throw;
        }
    }
    private async void UpdatePlayerName()
    {
        try
        {
            var playername = "Emil" + Random.RandomRange(0, 100);
            var options = new UpdatePlayerOptions()
            {
                Data = new Dictionary<string, PlayerDataObject>
                {
                    {"PlayerName", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public, playername) }
                }
            };
            await LobbyService.Instance.UpdatePlayerAsync(hostLobby.Id, AuthenticationService.Instance.PlayerId, options);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
            throw;
        }
    }
    private async void LeaveLobby()
    {
        try
        {
            await LobbyService.Instance.RemovePlayerAsync(hostLobby.Id, AuthenticationService.Instance.PlayerId);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
            throw;
        }
    }
    private async void KickPlayer()
    {
        try
        {
            await LobbyService.Instance.RemovePlayerAsync(hostLobby.Id, hostLobby.Players[1].Id);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
            throw;
        }
    }
    private async void MigrateLobbyHost()
    {
        try
        {
            var options = new UpdateLobbyOptions()
            {
                HostId = hostLobby.Players[1].Id
            };
            hostLobby = await Lobbies.Instance.UpdateLobbyAsync(hostLobby.Id, options);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
            throw;
        }
    }

    private async void DeleteLobby()
    {
        try
        {
            await LobbyService.Instance.DeleteLobbyAsync(hostLobby.Id);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
            throw;
        }
    }


}
