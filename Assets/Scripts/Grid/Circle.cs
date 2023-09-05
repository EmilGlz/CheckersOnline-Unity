using DG.Tweening;
using System;
using UnityEngine;
[Serializable]
public class Circle : MonoBehaviour
{
    [SerializeField] private SpriteRenderer _renderer;
    private bool _isKing;
    private bool _isWhite;
    private Vector2 _position;
    public Vector2 Position => _position;
    public bool IsWhite => _isWhite;
    public bool IsKing
    {
        get => _isKing;
        set
        {
            _isKing = value;
            var kingIcon = GuiUtils.FindGameObject("KingIcon", gameObject);
            if (kingIcon == null)  
                return;
            bool cameraIsUpsideDown = GameController.Instance.CameraRotation.z == 180;
            kingIcon.transform.localScale = cameraIsUpsideDown ? new Vector2(1, -1) : new Vector2(1, 1);
            kingIcon.SetActive(_isKing);
        }
    }
    public void Init(bool isWhite, Vector2 position, bool isKing = false)
    {
        IsKing = isKing;
        _position = position;
        _isWhite = isWhite;
        _renderer.color = isWhite ? Constants.WhiteCircleColor : Constants.BlackCircleColor;
    }
    public void Move(Tile destination, Action<Circle> onFinish = null)
    {
        void OnFinish() => onFinish?.Invoke(this);
        transform.DOMove(destination.transform.position + Vector3.back, Mock.movementDuration).OnComplete(OnFinish);
        _position = destination.Position;
    }

    public void Die()
    {
        gameObject.SetActive(false);
    }
}
