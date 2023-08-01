using UnityEngine;

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
}
