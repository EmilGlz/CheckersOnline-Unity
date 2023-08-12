using System.Linq;
using Unity.Netcode;
using Unity.Services.Lobbies.Models;
using UnityEngine;
public enum MatchMovementState
{
    None,
    ShowingAvailableMoves,
    Moving
}

public class GameController : MonoBehaviour
{
    #region Singleton
    private static GameController _instance;
    public static GameController Instance { get { return _instance; } }
    private void Awake()
    {
        _instance = this;
        _camera = Camera.main;
    }
    #endregion
    private Camera _camera;
    [HideInInspector] public Circle SelectedCircle { get; set; }
    private bool _iAmWhite;
    private MatchMovementState matchMovementState;
    public bool IAmWhite => _iAmWhite;
    private short[] lastMove;
    public Lobby CurrentLobby => LobbyManager.Instance.MyLobby;
    public Player Opponent => LobbyManager.GetOpponent(CurrentLobby);
    public bool IsOnline => LobbyManager.Instance.MyLobby != null;
    public bool MyTurn = false;
    private void Start()
    {
        InitUsername();
    }
    private void InitUsername()
    {
        if(string.IsNullOrEmpty(Settings.SavedUsername))
        {
            Settings.SavedUsername = "Player " + Random.Range(0, 100);
        }
        UIManager.Instance.UpdateUsernameEverywhereInUI();
    }
    public void InitGame(bool iamWhite)
    {
        _iAmWhite = iamWhite;
        GridManager.Instance.CreateGrid();
        GridManager.Instance.CreateCircles(true);
        _camera.transform.rotation = new Quaternion(0, 0, iamWhite ? 0 : 180, 0);
        MyTurn = iamWhite;
    }
    public void TilePressed(Tile pressedTile)
    {
        if (IsOnline)
            if (!MyTurn)
                return;
        var theCircle = GridManager.Instance.GetCircleAtPosition(pressedTile.Position);
        if (theCircle != null)
            if (IAmWhite != theCircle.IsWhite)
                return;
        switch (matchMovementState)
        {
            case MatchMovementState.None:
                GridManager.Instance.ShowAvailableMoves(pressedTile);
                break;
            case MatchMovementState.ShowingAvailableMoves:
                if (pressedTile.IsHighlighted)
                    MoveTo(pressedTile);
                else
                {
                    GridManager.Instance.ClearAvailableMoves();
                    matchMovementState = MatchMovementState.None;
                }
                break;
            case MatchMovementState.Moving:
                break;
            default:
                break;
        }
    }
    public void MoveTo(Tile destination)
    {
        if (SelectedCircle.Position == destination.Position)
            return;
        if (IsOnline)
        {
            PlayerNetwork pn = NetworkManager.Singleton.SpawnManager.GetPlayerNetworkObject(NetworkManager.Singleton.LocalClientId).GetComponent<PlayerNetwork>();
            pn.MoveTo(new short[] {
            (short)SelectedCircle.Position.x,
            (short)SelectedCircle.Position.y,
            (short)destination.Position.x,
            (short)destination.Position.y,
            });
        }
        else
        {
            _iAmWhite = !_iAmWhite;
        }
        GridManager.Instance.MoveCircle(SelectedCircle, destination);
        MyTurn = false;
    }
    public void OnOpponentMoves(short[] moveData)
    {
        if ((lastMove != null && moveData.SequenceEqual(lastMove)) || (moveData[0] == moveData[2] && moveData[1] == moveData[3]))
            return;
        lastMove = moveData;
        GridManager.Instance.MoveCircleByPos(new Vector2(moveData[0], moveData[1]), new Vector2(moveData[2], moveData[3]));
        MyTurn = true;
    }
    public void UpdateMatchMovementState(MatchMovementState state)
    {
        matchMovementState = state;
    }
    public void OnIJoinedRoom(Lobby lobby)
    {
        InitGame(Settings.hostIsWhiteLogicActivated && LobbyManager.IAmHost(lobby));
        UIManager.Instance.OnIJoinedRoom(lobby);
    }
    public void OnICreatedRoom(Lobby lobby)
    {
        LobbyManager.Instance.OnOppJoinedMe += OnOppJoinedMyRoom;
        UIManager.Instance.OnICreatedRoom(lobby);
    }
    public void OnOppJoinedMyRoom(Lobby lobby)
    {
        UIManager.Instance.OnOppJoinedMyRoom(lobby);
        LobbyManager.Instance.OnOppJoinedMe -= OnOppJoinedMyRoom;
        InitGame(Settings.hostIsWhiteLogicActivated && LobbyManager.IAmHost(lobby));
    }
}
