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
    public Circle SelectedCircle;
    public bool IAmWhite => _iAmWhite;

    private void Start()
    {
        InitGame(false);
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
        GridManager.Instance.MoveCircle(SelectedCircle, destination);
    }

    public void UpdateMatchMovementState(MatchMovementState state)
    {
        matchMovementState = state;
    }
}
