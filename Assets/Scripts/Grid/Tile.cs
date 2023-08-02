using UnityEngine;

public class Tile : MonoBehaviour
{
    [SerializeField] private Color darkColor, lightColor;
    [SerializeField] private SpriteRenderer _renderer;
    [SerializeField] private GameObject _highlight;
    private bool _isWhite;
    private Vector2 _position;
    public bool IsWhite => _isWhite;
    public Vector2 Position => _position;
    public bool IsHighlighted => _highlight.activeInHierarchy;
    public void Init(bool isOffset, Vector2 position)
    {
        _position = position;
        _isWhite = isOffset;
        _renderer.color = isOffset ? lightColor : darkColor;
    }

    public void SetHightlight(bool highlight = true)
    {
        _highlight.SetActive(highlight);
    }

    //void OnMouseEnter()
    //{
    //    SetHightlight();
    //}

    //void OnMouseExit()
    //{
    //    SetHightlight(false);
    //}

    public void OnMouseDown()
    {
        GameController.Instance.TilePressed(this);
    }
}