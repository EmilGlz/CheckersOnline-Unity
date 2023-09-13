using Assets.Scripts.Utils;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;
public class MatchResultPopup : Popup, IDisposable
{
    protected override string ItemTemplateName => "Prefabs/Popups/MatchResultPopup";
    protected override bool FromTop => false;
    protected override float AnimationDuration => .3f;
    private readonly bool _isWin;
    private readonly string _message;
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
    public static void Create(bool isWin, string message = "")
    {
        var popup = new MatchResultPopup(isWin, message);
        popup.Show();
    }
    public MatchResultPopup(bool isWin, string message)
    {
        _isWin = isWin;
        _message = message;
    }
    protected override void Show()
    {
        base.Show();
        ItemTemplate.name = typeof(MatchResultPopup).Name;
        InitButtons();
        var titleText = GuiUtils.FindGameObject("TitleText", ItemTemplate).GetComponent<TMP_Text>();
        titleText.text = _message;
    }
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
