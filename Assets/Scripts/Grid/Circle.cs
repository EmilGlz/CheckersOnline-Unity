using UnityEngine;

public class Circle : MonoBehaviour
{
    [SerializeField] private Color darkColor, lightColor;
    [SerializeField] private SpriteRenderer _renderer;
    private bool _isKing;
    private bool _isWhite;
    private Vector2 _position;
    public Vector2 Position => _position;
    public bool IsWhite => _isWhite;
    public bool IsKing => _isKing;
    public void Init(bool isWhite, Vector2 position, bool isKing = false)
    {
        _isKing = isKing;
        _position = position;
        _isWhite = isWhite;
        _renderer.color = isWhite ? lightColor : darkColor;
    }
}
