using System.Linq;
using Unity.Netcode;
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
    }
    #endregion
    private bool _iAmWhite;
    private MatchMovementState matchMovementState;
    [HideInInspector] public Circle SelectedCircle { get; set; }
    public bool IAmWhite => _iAmWhite;
    private short[] lastMove;

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
        GridManager.Instance.CreateCircles(IAmWhite);
    }

    public void TilePressed(Tile pressedTile)
    {
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
        PlayerNetwork pn = NetworkManager.Singleton.SpawnManager.GetPlayerNetworkObject(NetworkManager.Singleton.LocalClientId).GetComponent<PlayerNetwork>();
        pn.MoveTo(new short[] {
            (short)SelectedCircle.Position.x,
            (short)SelectedCircle.Position.y,
            (short)destination.Position.x,
            (short)destination.Position.y,
        });
        var obj = NetworkManager.Singleton.SpawnManager.GetPlayerNetworkObject(NetworkManager.Singleton.LocalClientId);
        GridManager.Instance.MoveCircle(SelectedCircle, destination);
    }

    public void OnOpponentMoves(short[] moveData)
    {
        if (lastMove != null && moveData.SequenceEqual(lastMove))
            return;
        lastMove = moveData;
        GridManager.Instance.MoveCircleByPos(new Vector2(moveData[0], moveData[1]), new Vector2(moveData[2], moveData[3]));
    }

    public void UpdateMatchMovementState(MatchMovementState state)
    {
        matchMovementState = state;
    }
}
