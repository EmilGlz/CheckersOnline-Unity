using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Unity.Netcode.Transports.UTP;
using Unity.Netcode;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;
using static UnityEngine.RuleTile.TilingRuleOutput;

public class LobbyManager : MonoBehaviour
{
    #region Singleton
    private static LobbyManager _instance;
    public static LobbyManager Instance { get { return _instance; } }
    private void Awake()
    {
        _instance = this;
    }
    #endregion

    #region Counters
    private float heartBeatTimer;
    private float lobbyUpdateTimer;
    #endregion

    public Action<Lobby> OnOppJoinedMe;
    private Lobby hostLobby;
    private Lobby joinedLobby;
    private bool IsWaitingForOpponentToJoin => hostLobby != null && joinedLobby == null && hostLobby.Players.Count == 1;
    public Lobby MyLobby {

        get => hostLobby == null ? joinedLobby : hostLobby;
        set
        {
            if (hostLobby != null)
                hostLobby = value;
            else if(joinedLobby != null)
                joinedLobby = value;
        }
    }
    private static string _userId;
    private static string UserId => _userId;
    async void Start()
    {
        await UnityServices.InitializeAsync();
        AuthenticationService.Instance.SignedIn += OnSignIn;
        await AuthenticationService.Instance.SignInAnonymouslyAsync();
    }
    private void OnSignIn()
    {
        _userId = AuthenticationService.Instance.PlayerId;
        Debug.Log("Signed in: " + _userId);
    }
    private void Update()
    {
        HandleHeartbeat();
        HandleLobbyPollUpdate();
    }
    public async Task PlayRandom(Action<Lobby> onIJoinedRoom, Action<Lobby> onICreatedNewRoom, Action<string> OnError)
    {
        try
        {
            var lobbies = await GetLobbies();
            lobbies = lobbies.Where(i => i.HostId != AuthenticationService.Instance.PlayerId).ToList();
            if (lobbies.Count > 0)
            {
                var joiningLobby = lobbies[UnityEngine.Random.Range(0, lobbies.Count)];
                joinedLobby = await JoinLobby(joiningLobby);
                hostLobby = null;
                onIJoinedRoom.Invoke(joiningLobby);
            }
            else
            {
                await CreateLobby();
                onICreatedNewRoom.Invoke(hostLobby);
            }
        }
        catch (LobbyServiceException e)
        {
            OnError.Invoke(e.Message);
            throw;
        }
    }
    public async Task<List<Lobby>> GetLobbies()
    {
        try
        {
            // filering lobbies
            QueryLobbiesOptions options = new QueryLobbiesOptions()
            {
                Count = 5,
                Filters = new List<QueryFilter> {
                    //new QueryFilter(QueryFilter.FieldOptions.AvailableSlots, "0", QueryFilter.OpOptions.GT), // gt = greater than
                    //new QueryFilter(QueryFilter.FieldOptions.S1, "CaptureTheFlag", QueryFilter.OpOptions.EQ) // EQ = equal
                    new QueryFilter(QueryFilter.FieldOptions.AvailableSlots, "1", QueryFilter.OpOptions.EQ)
                },
                Order = new List<QueryOrder>
                {
                    new QueryOrder(false, QueryOrder.FieldOptions.Created)
                }
            };
            QueryResponse queryResponse = await Lobbies.Instance.QueryLobbiesAsync(options);
            return queryResponse.Results;
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
            throw;
        }
    }
    public async Task<Lobby> JoinLobby(Lobby lobby)
    {
        try
        {
            var options = new JoinLobbyByIdOptions { Player = GetPlayer() };
            var res = await Lobbies.Instance.JoinLobbyByIdAsync(lobby.Id, options);
            var joinCode = res.Data[Constants.JoinKey].Value;
            JoinAllocation allocation = await RelayService.Instance.JoinAllocationAsync(joinCode);
            RelayServerData relayServerData = new RelayServerData(allocation, "dtls");
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);
            NetworkManager.Singleton.StartClient();
            return res;
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
            throw;
        }
    }
    public async Task<Lobby> CreateLobby()
    {
        try
        {
            var lobbyName = Settings.SavedUsername + " lobby";
            var maxPlayers = 2;
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(3); // host + 3 players
            string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
            CreateLobbyOptions options = new CreateLobbyOptions()
            {
                IsPrivate = false,
                Player = GetPlayer(),
                Data = new Dictionary<string, DataObject> { 
                    { Constants.JoinKey, new DataObject(DataObject.VisibilityOptions.Member, joinCode) } }
            };
            Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, options);
            RelayServerData relayServerData = new RelayServerData(allocation, "dtls");
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);
            NetworkManager.Singleton.StartHost();
            hostLobby = lobby;
            joinedLobby = null;
            return lobby;
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
            throw;
        }
    }
    private static Player GetPlayer()
    {
        return new Player(UserId)
        {
            Data = new Dictionary<string, PlayerDataObject>
            {
                {"Name",  new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, Settings.SavedUsername)}
            }
        };
    }
    private async void HandleHeartbeat()
    {
        if (hostLobby != null)
        {
            heartBeatTimer -= Time.deltaTime;
            if (heartBeatTimer < 0f)
            {
                float heartBeatTimerMax = Settings.LobbyHeartBeat; // send every 15 seconds
                heartBeatTimer = heartBeatTimerMax;
                await LobbyService.Instance.SendHeartbeatPingAsync(hostLobby.Id);
            }
        }
    }
    private async void HandleLobbyPollUpdate()
    {
        if (MyLobby != null)
        {
            lobbyUpdateTimer -= Time.deltaTime;
            if (lobbyUpdateTimer < 0f)
            {
                float heartBeatTimerMax = Settings.LobbyPollTime; // send every 1.1 seconds
                lobbyUpdateTimer = heartBeatTimerMax;
                var lobby = await LobbyService.Instance.GetLobbyAsync(MyLobby.Id);
                if (IsWaitingForOpponentToJoin && lobby.Players.Count > 1)
                {
                    // Opponent joined my lobby
                    MyLobby = lobby;
                    OnOppJoinedMe?.Invoke(lobby);
                }
                MyLobby = lobby;
            }
        }
    }
    public static Player GetOpponent(Lobby lobby)
    {
        return lobby.Players.FirstOrDefault(p => p.Id != GetPlayer().Id);
    }
    public static bool IAmHost(Lobby lobby)
    {
        return lobby.HostId == GetPlayer().Id;
    }
    public static string GetPlayerName(Player player)
    {
        return player.Data["Name"].Value;
    }
    public async void CancelCurrentHostedLobby()
    {
        try
        {
            await LobbyService.Instance.DeleteLobbyAsync(hostLobby.Id);
            hostLobby = null;
            NetworkManager.Singleton.Shutdown();
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
            throw;
        }
    }





    //public async void QuickJoinLobby(Action successCallback = null, Action<string> errorCallback = null)
    //{
    //    try
    //    {
    //        await LobbyService.Instance.QuickJoinLobbyAsync();
    //        successCallback?.Invoke();
    //    }
    //    catch (LobbyServiceException e)
    //    {
    //        Debug.Log(e);
    //        errorCallback?.Invoke(e.Message);
    //        throw;
    //    }
    //}
    //public async Task<Lobby> CreatePublicLobby()
    //{
    //    try
    //    {
    //        var lobbyName = "MyLobby";
    //        var maxPlayers = 2;
    //        CreateLobbyOptions options = new CreateLobbyOptions()
    //        {
    //            IsPrivate = false,
    //            Player = GetPlayer(),
    //            Data = new Dictionary<string, DataObject>
    //            {
    //                //{"GameMode", new DataObject(DataObject.VisibilityOptions.Public, "CaptureTheFlag", DataObject.IndexOptions.S1) }
    //            }
    //        };
    //        Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, options);
    //        return lobby;
    //        //hostLobby = lobby;
    //    }
    //    catch (LobbyServiceException e)
    //    {
    //        Debug.Log(e);
    //        throw;
    //    }
    //}
    //public async void JoinLobbyByCode(string lobbyCode)
    //{
    //    try
    //    {
    //        JoinLobbyByCodeOptions options = new JoinLobbyByCodeOptions()
    //        {
    //            Player = GetPlayer()
    //        };
    //        await Lobbies.Instance.JoinLobbyByCodeAsync(lobbyCode, options);
    //    }
    //    catch (LobbyServiceException e)
    //    {
    //        Debug.Log(e);
    //        throw;
    //    }
    //}
    //public async void PrintPlayers(Lobby lobby)
    //{
    //    foreach (Player player in lobby.Players)
    //    {
    //        Debug.Log("Player: " + player.Data["Name"].Value);
    //    }
    //    try
    //    {
    //        await LobbyService.Instance.QuickJoinLobbyAsync();
    //    }
    //    catch (LobbyServiceException e)
    //    {
    //        Debug.Log(e);
    //        throw;
    //    }
    //}
    //private async void UpdateGameMode()
    //{
    //    try
    //    {
    //        var options = new UpdateLobbyOptions()
    //        {
    //            Data = new Dictionary<string, DataObject>
    //            {
    //                {"GameMode", new DataObject(DataObject.VisibilityOptions.Public, "NotCaptureTheFlag", DataObject.IndexOptions.S1) }
    //            }
    //        };
    //        hostLobby = await Lobbies.Instance.UpdateLobbyAsync(hostLobby.Id, options);
    //    }
    //    catch (LobbyServiceException e)
    //    {
    //        Debug.Log(e);
    //        throw;
    //    }
    //}
    //private async void UpdatePlayerName()
    //{
    //    try
    //    {
    //        var playername = "Emil" + UnityEngine.Random.Range(0, 100);
    //        var options = new UpdatePlayerOptions()
    //        {
    //            Data = new Dictionary<string, PlayerDataObject>
    //            {
    //                {"PlayerName", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public, playername) }
    //            }
    //        };
    //        await LobbyService.Instance.UpdatePlayerAsync(hostLobby.Id, AuthenticationService.Instance.PlayerId, options);
    //    }
    //    catch (LobbyServiceException e)
    //    {
    //        Debug.Log(e);
    //        throw;
    //    }
    //}
    //private async void LeaveLobby()
    //{
    //    try
    //    {
    //        await LobbyService.Instance.RemovePlayerAsync(hostLobby.Id, AuthenticationService.Instance.PlayerId);
    //    }
    //    catch (LobbyServiceException e)
    //    {
    //        Debug.Log(e);
    //        throw;
    //    }
    //}
    //private async void KickPlayer()
    //{
    //    try
    //    {
    //        await LobbyService.Instance.RemovePlayerAsync(hostLobby.Id, hostLobby.Players[1].Id);
    //    }
    //    catch (LobbyServiceException e)
    //    {
    //        Debug.Log(e);
    //        throw;
    //    }
    //}
    //private async void MigrateLobbyHost()
    //{
    //    try
    //    {
    //        var options = new UpdateLobbyOptions()
    //        {
    //            HostId = hostLobby.Players[1].Id
    //        };
    //        hostLobby = await Lobbies.Instance.UpdateLobbyAsync(hostLobby.Id, options);
    //    }
    //    catch (LobbyServiceException e)
    //    {
    //        Debug.Log(e);
    //        throw;
    //    }
    //}
    //private async void DeleteLobby()
    //{
    //    try
    //    {
    //        await LobbyService.Instance.DeleteLobbyAsync(hostLobby.Id);
    //    }
    //    catch (LobbyServiceException e)
    //    {
    //        Debug.Log(e);
    //        throw;
    //    }
    //}
}
