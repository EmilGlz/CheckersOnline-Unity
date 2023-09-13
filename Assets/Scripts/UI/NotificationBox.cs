using Assets.Scripts.Utils;
using DG.Tweening;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

public class NotificationBox : Popup
{
    protected override string ItemTemplateName => "Prefabs/Popups/NotificationPopup";
    protected override bool FromTop => true;
    protected override float AnimationDuration => .3f;
    private readonly int _autoHideDelayTime = 2;
    private readonly string _title;
    protected override float OpeningDestinationY
    {
        get
        {
            var popupRect = ItemTemplate.GetComponent<RectTransform>();
            return FromTop ? 0 : popupRect.GetHeight();
        }
    }
    protected override float ClosingDestinationY
    {
        get
        {
            var popupRect = ItemTemplate.GetComponent<RectTransform>();
            var toolbarHeight = 30;
            return FromTop ? popupRect.GetHeight() + toolbarHeight : -toolbarHeight;
        }
    }
    public NotificationBox(string title)
    {
        _title = title;
    }
    public static void Create(string title)
    {
        var popup = new NotificationBox(title);
        popup.Show();
    }
    protected override void Show()
    {
        base.Show();
        ItemTemplate.name = typeof(NotificationBox).Name;
        var titleText = GuiUtils.FindGameObject("Text", ItemTemplate).GetComponent<TMP_Text>();
        titleText.text = _title;
        var vlg = ItemTemplate.GetComponent<VerticalLayoutGroup>();
        vlg.padding.top = FromTop ? 10 : 0;
        vlg.padding.bottom = FromTop ? 0 : 10;
        Hide(_autoHideDelayTime);
    }
    private void Hide(int delayTime = 0)
    {
        GameController.Instance.StartCoroutine(HideFunc(delayTime));
    }
    private IEnumerator HideFunc(int delayTime)
    {
        if (delayTime > 0)
            yield return new WaitForSeconds(delayTime);
        if (ItemTemplate == null)
            yield break;
        CloseAnimation(Dispose);

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
    protected override void OpenAnimation(Action callback = null)
    {
        GuiUtils.ForceUpdateLayout(ItemTemplate);
        var popupRect = ItemTemplate.GetComponent<RectTransform>();
        var value = popupRect.GetPosY();
        var destination = FromTop ? 0 : popupRect.GetHeight();
        void UpdateValue() => popupRect.SetPosY(value);
        DOTween.To(() => value, x => value = x, destination, AnimationDuration)
        .SetEase(Ease.Linear)
        .OnUpdate(UpdateValue);
    }
    protected override void CloseAnimation(Action callback = null)
    {
        var toolbarHeight = 30;
        var popupRect = ItemTemplate.GetComponent<RectTransform>();
        var value = popupRect.GetPosY();
        void UpdateValue() => popupRect.SetPosY(value);
        var destination = FromTop ? popupRect.GetHeight() + toolbarHeight : -toolbarHeight;
        DOTween.To(() => value, x => value = x, destination, AnimationDuration)
        .SetEase(Ease.Linear)
        .OnUpdate(UpdateValue)
        .OnComplete(() => callback?.Invoke());
    }
}
