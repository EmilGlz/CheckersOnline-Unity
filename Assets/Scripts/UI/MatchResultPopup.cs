using Assets.Scripts.Utils;
using System;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;
public class MatchResultPopup : Popup, IDisposable
{
    protected override string ItemTemplateName => "Prefabs/Popups/MatchResultPopup";
    protected override bool FromTop => false;
    protected override float AnimationDuration => .3f;
    protected override float OpeningDestinationY
    {
        get
        {
            return FromTop ? -Device.Height / 2f : Device.Height / 2f;
        }
    }
    protected override float ClosingDestinationY
    {
        get
        {
            var popupRect = ItemTemplate.GetComponent<RectTransform>();
            return FromTop ? popupRect.GetHeight() / 2f : -popupRect.GetHeight() / 2f;
        }
    }
    private readonly bool _isWin;
    private readonly string _message;
    private string[] _winEmojis = new string[] 
    {
        "Sprites/FaceEmojis/hugging-hands@3x",
        "Sprites/FaceEmojis/smile-with-smiling-eyes@3x",
        "Sprites/FaceEmojis/amazed@3x",
        "Sprites/FaceEmojis/jaw-dropped@3x",
    };

    private string[] _loseEmojis = new string[]
{
        "Sprites/FaceEmojis/confused@3x",
        "Sprites/FaceEmojis/defeated@3x",
        "Sprites/FaceEmojis/disappointed@3x",
        "Sprites/FaceEmojis/expresionless@3x",
        "Sprites/FaceEmojis/eye-rolling@3x",
        "Sprites/FaceEmojis/nauseated@3x",
};

    public static void Create(bool isWin, string message = "")
    {
        var popup = new MatchResultPopup(isWin, message);
        popup.Show();
    }
    public MatchResultPopup(bool isWin, string message)
    {
        _isWin = isWin;
        _message = string.IsNullOrEmpty(message) ?
            isWin ? "Congratulations, You Won!!!" : "Defeat! You Lost!!!" :
            message;
    }
    protected override void Show()
    {
        base.Show();
        ItemTemplate.name = typeof(MatchResultPopup).Name;
        InitButtons();
        var titleText = GuiUtils.FindGameObject("TitleText", ItemTemplate).GetComponent<TMP_Text>();
        titleText.text = _message;
        var img = GuiUtils.FindGameObject("EmojiImage", ItemTemplate).GetComponent<Image>();
        GuiUtils.SetIcon(img, _isWin ? GetRandomWinImage() : GetRandomLoseImage());

    }
    private string GetRandomWinImage() => _winEmojis[UnityEngine.Random.Range(0, _winEmojis.Length- 1)];
    private string GetRandomLoseImage() => _loseEmojis[UnityEngine.Random.Range(0, _loseEmojis.Length- 1)];
    private void InitButtons()
    {
        var goOnButton = GuiUtils.FindGameObject("ContinueButton", ItemTemplate).GetComponent<Button>();
        goOnButton.onClick.RemoveAllListeners();
        goOnButton.onClick.AddListener(() => {
            UIManager.Instance.CurrentMenu = Menu.MainMenu;
            GameController.Instance.DisposeTable();
            CloseAnimation(Dispose);
        });
    }
    public override void Dispose()
    {
        if (ItemTemplate != null)
        {
            Object.Destroy(ItemTemplate);
            ItemTemplate = null;
        }
        base.Dispose();
    }
    protected override void InitToolbar()
    {
    }
}
